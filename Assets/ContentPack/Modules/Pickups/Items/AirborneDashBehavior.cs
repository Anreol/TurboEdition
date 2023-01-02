using RoR2;
using RoR2.Items;
using UnityEngine;

namespace TurboEdition.Items
{
    internal class AirborneDashBehavior : BaseItemBodyBehavior
    {
        [BaseItemBodyBehavior.ItemDefAssociationAttribute(useOnServer = false, useOnClient = true)]
        private static ItemDef GetItemDef()
        {
            return TEContent.Items.AirborneDash;
        }

        private const float curveDuration = 0.5f;
        private const float graceDuration = 0.75f;
        private const float coneAngle = 10f;
        private const float accumulationTime = 0.35f;

        private AnimationCurveAsset animationCurveAsset = Assets.mainAssetBundle.LoadAsset<AnimationCurveAsset>("AirborneDashEvaluationCurve");

        private Vector3 firstTapDirection;
        private bool firstTapReleased;

        private float stopwatch;
        private float graceStopwatch;
        private float accumulationAge;

        public void FixedUpdate()
        {
            if (!body.hasAuthority)
                return;
            //Gather inputs
            if (body.inputBank)
            {
                //Dont update the first tap if the first tap has finalized
                if (!firstTapReleased)
                {
                    if (body.inputBank.moveVector != Vector3.zero && graceStopwatch <= 0)
                    {
                        firstTapDirection = body.inputBank.moveVector;
                        graceStopwatch = graceDuration;
                    }
                    firstTapReleased = body.inputBank.moveVector == Vector3.zero;
                }
                firstTapReleased = firstTapReleased && graceStopwatch > 0 && stopwatch <= 0; //I'm smart...
            }

            //Handle Movements. Under stopwatch so you cannot stack effects.
            if (stopwatch <= 0)
            {
                ProcessEvasion();
            }

            //Perform or update Inputs
            if (graceStopwatch > 0)
            {
                graceStopwatch -= Time.fixedDeltaTime;
                if (body.inputBank.moveVector == Vector3.zero && !firstTapReleased) //Reset unless we are awaiting for a next input
                {
                    graceStopwatch = 0;
                }
            }
            if (accumulationAge > 0)
            {
                accumulationAge -= Time.fixedDeltaTime / 4;
            }
            //Evasion has been processed, update speed.
            if (stopwatch > 0)
            {
                stopwatch -= Time.fixedDeltaTime;
                if (body.characterDirection && body.characterMotor)
                {
                    Vector3 forwardDirectionBeforeChanges = body.characterDirection.forward;
                    float currentAirAcceleration = body.acceleration * body.characterMotor.airControl;
                    float speedFromItem = Mathf.Sqrt((10f + (10f * ((float)stack - 1))) / currentAirAcceleration); //10 + 5 per stack
                    float airSpeed = body.moveSpeed / currentAirAcceleration;

                    body.characterDirection.moveVector = body.inputBank.moveVector;
                    body.characterMotor.rootMotion += (((((speedFromItem + airSpeed) / airSpeed) * (animationCurveAsset.value.Evaluate(stopwatch / curveDuration))) * body.moveSpeed) * forwardDirectionBeforeChanges) * Time.fixedDeltaTime;
                }
            }
        }

        private void ProcessEvasion()
        {
            if (firstTapReleased && body.inputBank.moveVector != Vector3.zero && Vector3.Dot(firstTapDirection, body.inputBank.moveVector) >= Mathf.Cos(coneAngle * 0.017453292f))
            {
                //if (body.inputBank.moveVector != Vector3.zero && Vector3.Dot(body.inputBank.aimDirection, body.inputBank.moveVector) < PlayerCharacterMasterController.sprintMinAimMoveDot && (body.characterMotor.jumpCount < body.maxJumpCount || (body.characterMotor.jumpCount == 0 && body.maxJumpCount == 1))) //Always dash in the first jump..?
                {
                    EffectManager.SpawnEffect(Assets.mainAssetBundle.LoadAsset<GameObject>("Prefabs/Effects/BoostJumpEffect"), new EffectData
                    {
                        origin = body.footPosition,
                        rotation = Util.QuaternionSafeLookRotation(body.characterMotor.velocity)
                    }, true);

                    //Start the dash
                    body.characterDirection.forward = ((body.inputBank.moveVector == Vector3.zero) ? body.characterDirection.forward : body.inputBank.moveVector).normalized;
                    stopwatch = curveDuration;

                    //Add iframes
                    body.AddTimedBuffAuthority(RoR2Content.Buffs.HiddenInvincibility.buffIndex, Mathf.Clamp01(animationCurveAsset.value.Evaluate(accumulationAge)));

                    //Accumulate to stopwatch so spamming it results in less iframes
                    accumulationAge = Mathf.Clamp(0.25f, accumulationAge + accumulationTime, 10f);
                }
            }
        }
    }
}
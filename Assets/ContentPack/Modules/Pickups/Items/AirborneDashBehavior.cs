using RoR2;
using RoR2.Items;
using UnityEngine;

namespace TurboEdition.Items
{
    internal class AirborneDashBehavior : BaseItemBodyBehavior, IStatItemBehavior
    {
        [BaseItemBodyBehavior.ItemDefAssociationAttribute(useOnServer = false, useOnClient = true)]
        private static ItemDef GetItemDef()
        {
            return TEContent.Items.AirborneDash;
        }

        private const float curveDuration = 0.5f;

        private AnimationCurveAsset animationCurveAsset = Assets.mainAssetBundle.LoadAsset<AnimationCurveAsset>("AirborneDashEvaluationCurve");

        private bool jumpInputReceived;

        private float stopwatch = 5;
        private int timesDashed; //Since jumps are done at different timings...

        public void FixedUpdate()
        {
            //Gather inputs
            if (body.inputBank)
            {
                jumpInputReceived = body.inputBank.jump.justPressed;
            }
            //Handle Movements
            if (body.hasAuthority)
            {
                ProcessJump();
            }
            //Perform Inputs
            if (body.hasAuthority)
            {
                if (stopwatch <= curveDuration)
                {
                    stopwatch += Time.fixedDeltaTime;
                    if (body.characterDirection && body.characterMotor)
                    {
                        Vector3 forwardDirectionBeforeChanges = body.characterDirection.forward;
                        float currentAirAcceleration = body.acceleration * body.characterMotor.airControl;
                        float speedFromItem = Mathf.Sqrt((15f + (10f * ((float)stack - 1))) / currentAirAcceleration); //10 + 5 per stack
                        float airSpeed = body.moveSpeed / currentAirAcceleration;

                        body.characterDirection.moveVector = body.inputBank.moveVector;
                        body.characterMotor.rootMotion += (((((speedFromItem + airSpeed) / airSpeed) * (animationCurveAsset.value.Evaluate(stopwatch / curveDuration))) * body.moveSpeed) * forwardDirectionBeforeChanges) * Time.fixedDeltaTime;
                    }
                }
                if (body.characterMotor.isGrounded)
                    timesDashed = 0;

                jumpInputReceived = false;
            }
        }

        public void RecalculateStatsEnd()
        {
            //This check sometimes breaks
            //if (body.characterMotor.jumpCount > body.baseJumpCount + body.inventory.GetItemCount(RoR2Content.Items.Feather) - 1)
            {
                body.maxJumpCount += (1 + Mathf.FloorToInt((float)stack / 5f));
            }
        }

        public void RecalculateStatsStart()
        {
        }

        private void ProcessJump()
        {
            if (jumpInputReceived && body.characterMotor && !body.characterMotor.isGrounded)
            {
                if (body.inputBank.moveVector != Vector3.zero && Vector3.Dot(body.inputBank.aimDirection, body.inputBank.moveVector) < PlayerCharacterMasterController.sprintMinAimMoveDot && body.characterMotor.jumpCount > body.baseJumpCount && timesDashed < body.maxJumpCount - body.baseJumpCount /*+ body.inventory.GetItemCount(RoR2Content.Items.Feather*)*/)
                {
                    EffectManager.SpawnEffect(Assets.mainAssetBundle.LoadAsset<GameObject>("Prefabs/Effects/BoostJumpEffect"), new EffectData
                    {
                        origin = body.footPosition,
                        rotation = Util.QuaternionSafeLookRotation(body.characterMotor.velocity)
                    }, true);

                    //Start the dash
                    body.characterDirection.forward = ((body.inputBank.moveVector == Vector3.zero) ? body.characterDirection.forward : body.inputBank.moveVector).normalized;
                    stopwatch = 0;
                }
                timesDashed++;
            }
        }
    }
}
using EntityStates;
using RoR2;
using RoR2.Items;
using UnityEngine;

namespace TurboEdition.Items
{
    class AirborneDashBehavior : BaseItemBodyBehavior, IStatItemBehavior
    {
        [BaseItemBodyBehavior.ItemDefAssociationAttribute(useOnServer = false, useOnClient = true)]
        private static ItemDef GetItemDef()
        {
            return TEContent.Items.AirborneDash;
        }
        private bool jumpInputReceived;
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
                jumpInputReceived = false;
            }
        }
        private void ProcessJump()
        {
            if (body.characterMotor)
            {
                if (jumpInputReceived && body && body.characterMotor.jumpCount <= body.maxJumpCount && !body.characterMotor.isGrounded)
                {
                    float horizontalBonus = 1f;
                    float verticalBonus = body.characterMotor.velocity.y <= 0.75f * body.jumpPower ? 0.75f : body.jumpPower; //I dunno what i am doing
                    if (stack > 0 && body.characterMotor.jumpCount > body.baseJumpCount /*+ body.inventory.GetItemCount(RoR2Content.Items.Feather*)*/)
                    {
                        float currentAirAcceleration = body.acceleration * body.characterMotor.airControl;
                        if (body.moveSpeed > 0f && currentAirAcceleration > 0f)
                        {
                            EffectManager.SpawnEffect(Assets.mainAssetBundle.LoadAsset<GameObject>("Prefabs/Effects/BoostJumpEffect"), new EffectData
                            {
                                origin = body.footPosition,
                                rotation = Util.QuaternionSafeLookRotation(body.characterMotor.velocity)
                            }, true);

                            float speedFromItem = Mathf.Sqrt((10f + (5f * ((float)stack - 1))) / currentAirAcceleration); //4 + 2 per stack
                            float airSpeed = body.moveSpeed / currentAirAcceleration;
                            horizontalBonus = (speedFromItem + airSpeed) / airSpeed;
                        }
                        ///TODO: rework with curve.
                        GenericCharacterMain.ApplyJumpVelocity(body.characterMotor, body, horizontalBonus, verticalBonus, false);
                        if (body.characterMotor.jumpCount > body.baseJumpCount + body.inventory.GetItemCount(RoR2Content.Items.Feather))
                        {
                            body.characterMotor.jumpCount++;
                        }
                    }
                }
            }
        }

        public void RecalculateStatsEnd()
        {
            body.maxJumpCount += (1 + Mathf.FloorToInt(stack / 5f));
        }

        public void RecalculateStatsStart()
        {
        }
    }
}

using RoR2.Projectile;
using TurboEdition.ScriptableObjects;
using UnityEngine;

namespace TurboEdition.EntityStates.Grenadier.Weapon
{
    public class SpecialChargeSatchels : AimThrowableBaseChargable
    {
        public static float recoilAmplitudeY;
        public static float recoilAmplitudeX;

        public static AnimationCurve bloomCurve;

        [Tooltip("Used in a random calculation. Keep the same as maxFixedSpreadYaw to make it not random.")]
        public static float minFixedSpreadYaw;

        [Tooltip("Used in a random calculation. Keep the same as minFixedSpreadYaw to make it not random.")]
        public static float maxFixedSpreadYaw;

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            activatorSkillSlot.skillDef.canceledFromSprinting = !firedAtLeastOnce;
        }

        public override void Update()
        {
            characterBody?.SetSpreadBloom(bloomCurve.Evaluate(CalcCharge()), false);
            base.Update();
        }

        public override void FireProjectileOnce(Ray finalRay)
        {
            StartAimMode(finalRay, 3f, false);
            base.AddRecoil(-1f * recoilAmplitudeY, -1.5f * recoilAmplitudeY, -1f * recoilAmplitudeX, 1f * recoilAmplitudeX);
            if (isAuthority)
            {
                Vector3 rhs = Vector3.Cross(Vector3.up, finalRay.direction);
                Vector3 axis = Vector3.Cross(finalRay.direction, rhs);
                int startingStock = activatorSkillSlot.stock;
                float bloom = 0f;
                if (base.characterBody)
                {
                    bloom = base.characterBody.spreadBloomAngle;
                }
                float angle = 0f;
                float num2 = 0f;
                if (startingStock > 1)
                {
                    num2 = UnityEngine.Random.Range(minFixedSpreadYaw + bloom * 2f, maxFixedSpreadYaw + bloom * 2f) * 2f;
                    angle = num2 / (float)(startingStock - 1);
                }
                Vector3 direction = Quaternion.AngleAxis(-num2 * 0.5f, axis) * finalRay.direction;
                Quaternion rotation = Quaternion.AngleAxis(angle, axis);
                Ray aimRay2 = new Ray(finalRay.origin, direction);
                for (int i = 0; i < startingStock; i++)
                {
                    base.FireProjectileOnce(aimRay2);
                    
                    GrenadierSpecialSkillDef.InstanceData instanceData = (GrenadierSpecialSkillDef.InstanceData)activatorSkillSlot.skillInstanceData;
                    if (instanceData != null && instanceData.skillStocksExtra > 0)
                    {
                        instanceData.skillStocksExtra -= activatorSkillSlot.skillDef.stockToConsume;
                        activatorSkillSlot.RecalculateValues();
                    }
                    //if (firedAtLeastOnce)
                    //    activatorSkillSlot.DeductStock(activatorSkillSlot.skillDef.stockToConsume);
                    aimRay2.direction = rotation * aimRay2.direction;
                }
            }
        }
    }
}
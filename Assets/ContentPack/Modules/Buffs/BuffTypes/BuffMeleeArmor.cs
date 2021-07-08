using RoR2;
using UnityEngine;

namespace TurboEdition.Buffs
{
    public class MeleeArmor : Buff
    {
        public override BuffDef buffDef { get; set; } = Assets.mainAssetBundle.LoadAsset<BuffDef>("BuffMeleeArmor");
        public static BuffDef buff;
        public int oldCount;

        public override void Initialize()
        {
            buff = buffDef;
            Debug.LogWarning("Hello, Buff! " + buff);
        }

        public override void UpdateBuff(ref CharacterBody body, int stack)
        {
            TurboBuffManager buffManager = body.GetComponent<TurboBuffManager>();
            Debug.LogWarning("Updating " + buff + " "+ buffManager);
            if (stack <= 0 && stack != oldCount)
            {
                buffManager?.OnBuffFinalStackLost(this);
                OnBuffLastStackLost(ref body);
                return;
            }

            if (oldCount <= 0 && stack > oldCount) OnBuffFirstStackGained(ref body);
            oldCount = stack;

            BuffStep(ref body, stack);
            //HG.ArrayUtils.ArrayAppend(ref buffManager.detectedBuffs, this);
        }

        public override void BuffStep(ref CharacterBody body, int stack)
        {
            body.jumpPower += 500f;
            Debug.LogWarning("Step buff" + buff + " " + body.jumpPower);
        }

        public override void OnBuffFirstStackGained(ref CharacterBody body)
        {
            body.moveSpeed += 500f;
            Debug.LogWarning("On Buff first stack gained " + buff + " " + body.moveSpeed);
        }

        public override void OnBuffLastStackLost(ref CharacterBody body)
        {
            body.attackSpeed += 500f;
            Debug.LogWarning("On Buff last stack lost " + buff + " " + body.attackSpeed);
        }
        public override void RecalcStatsStart(ref CharacterBody body)
        {
            body.attackSpeed += 500f;
            Debug.LogWarning("Cock and balls " + buff);
        }
        public override void RecalcStatsEnd(ref CharacterBody body)
        {
            body.moveSpeed += 500f;
            Debug.LogWarning("Fuckhead " + buff);
        }
    }
}
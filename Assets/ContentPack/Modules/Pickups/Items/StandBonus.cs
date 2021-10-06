using RoR2;
using UnityEngine;

namespace TurboEdition.Items
{
    public class StandBonus : Item
    {
        public override ItemDef itemDef { get; set; } = Assets.mainAssetBundle.LoadAsset<ItemDef>("StandBonus");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<Sandbag>(stack);
        }

        internal class Sandbag : CharacterBody.ItemBehavior, IStatItemBehavior, IOnTakeDamageServerReceiver
        {
            private CharacterMotor motor; //brrrrum brrum
            private bool provideBuffs;
            private bool reset;
            private float accumulatedDamage;
            private float lerp = 0.0f;

            private void Start() //On Start since we need to subscribe to the body, ANYTHING THAT HAS TO DO WITH BODIES, CANNOT BE ON AWAKE() OR ONENABLE()
            {
                if (!body)
                {
                    TELog.LogE("Body not available or does not exist.");
                    return;
                }
                motor = base.GetComponent<CharacterMotor>();
            }

            private void FixedUpdate()
            {
                if (!body.GetNotMoving())
                {
                    accumulatedDamage = 0f;
                    lerp = 0f;
                    return;
                }
                lerp = Mathf.Lerp((stack / 4) * body.maxHealth, 0, accumulatedDamage);
                provideBuffs = body.GetNotMoving() && stack > 0;
            }

            public void RecalculateStatsEnd()
            {
                if (!provideBuffs) return;
                if (motor)
                {
                    motor.mass += (10 + ((stack - 1) * 5)); //[body] IS FAT
                }
                body.armor += 500 * lerp;
            }

            public void RecalculateStatsStart()
            {
            }

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                if (!damageReport.damageInfo.rejected)
                {
                    this.accumulatedDamage += damageReport.damageDealt;
                }
            }
        }
    }
}
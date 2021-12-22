using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurboEdition.UI;
using UnityEngine;

namespace TurboEdition.Items
{
    class GetDamageFromHurtVictim : Item
    {
        public override ItemDef itemDef { get; set; } = Assets.mainAssetBundle.LoadAsset<ItemDef>("SoulDevourer");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<GetDamageFromHurtVictimBehavior>(stack);
        }
        internal class GetDamageFromHurtVictimBehavior : CharacterBody.ItemBehavior, IStatItemBehavior, IStatBarProvider
        {
            private bool reset;
            private float accumulatedDamage;
            private float lerp = 0.0f;

            private void OnEnable()
            {
                GlobalEventManager.onServerDamageDealt += onServerDamageDealt;
            }
            private void OnDisable()
            {
                GlobalEventManager.onServerDamageDealt -= onServerDamageDealt;
            }
            private void onServerDamageDealt(DamageReport obj)
            {

            }

            private void FixedUpdate()
            {

            }

            public void RecalculateStatsEnd()
            {
                body.damage += accumulatedDamage;
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

            public float GetDataCurrent()
            {
                return lerp;
            }

            public float GetDataMax()
            {
                return 100 + ((stack - 1) * 50);
            }

            public Sprite GetSprite()
            {
                return Assets.mainAssetBundle.LoadAsset<ItemDef>("SoulDevourer").pickupIconSprite;
            }

            public Color GetColor()
            {
                return new Color(0.3f, 1f, 0.8f, 1f);
            }
        }
    }
}

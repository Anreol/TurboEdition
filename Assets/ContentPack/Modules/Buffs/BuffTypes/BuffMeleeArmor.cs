using RoR2;
using UnityEngine;

namespace TurboEdition.Buffs
{
    public class BuffMeleeArmor : Buff
    {
        public override BuffDef buffDef { get; set; } = Assets.mainAssetBundle.LoadAsset<BuffDef>("BuffMeleeArmor");
        public static BuffDef buff;

        public override void Initialize()
        {
            buff = buffDef;
        }

        public override void RecalcStatsEnd(ref CharacterBody body)
        {
            body.armor += 30 * body.GetBuffCount(buffDef);
        }
    }
}
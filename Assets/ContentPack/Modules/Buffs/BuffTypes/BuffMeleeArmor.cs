using RoR2;

namespace TurboEdition.Buffs
{
    public class BuffMeleeArmor : Buff
    {
        public override BuffDef buffDef { get; set; } = Assets.mainAssetBundle.LoadAsset<BuffDef>("BuffMeleeArmor");

        public override void Initialize()
        {
        }

        public override void RecalcStatsEnd(ref CharacterBody body)
        {
            body.armor += 30 * body.GetBuffCount(buffDef);
        }
    }
}
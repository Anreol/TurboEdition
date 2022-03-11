using RoR2;

namespace TurboEdition.Buffs
{
    public class BuffMeleeArmor : Buff
    {
        public override BuffDef buffDef { get; set; } = Assets.mainAssetBundle.LoadAsset<BuffDef>("bdMeleeArmor");

        public override void Initialize()
        {
        }

        public override void RecalcStatsEnd(ref CharacterBody body)
        {
            body.armor += 35 * body.GetBuffCount(buffDef);
            body.damage += body.baseDamage / 25f;
        }
    }
}
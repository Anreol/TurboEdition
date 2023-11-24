using RoR2;

namespace TurboEdition.Buffs
{
    public class BuffMeleeArmor : Buff
    {
        public override BuffDef buffDef { get; set; } = TEContent.Buffs.MeleeArmor;

        public override void Initialize()
        {
        }

        public override void RecalcStatsEnd(ref CharacterBody body)
        {
            body.armor += 40 * body.GetBuffCount(buffDef);
            body.damage += body.baseDamage / 25f;
        }
    }
}
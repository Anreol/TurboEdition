using RoR2;

namespace TurboEdition.Items
{
    internal class AirborneBonus : Item
    {
        public override ItemDef itemDef { get; set; } = TEContent.Items.AirborneBonus;

        public override void RecalcStatsEnd(ref CharacterBody body, int stack)
        {
            if (body.characterMotor && !body.characterMotor.isGrounded && stack > 0)
            {
                //body.characterMotor.airControl += 0.5f;
                body.damage += ((body.baseDamage * 0.10f) + ((body.baseDamage * 0.10f) * (stack - 1)));
            }
        }
    }
}
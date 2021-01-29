using BepInEx.Configuration;
using R2API;
using RoR2;

namespace TurboEdition.Equipment
{
    public class StageSkip : EquipmentBase
    {
        public override string EquipmentName => "Deprecate Me Equipment";

        public override string EquipmentLangTokenName => "DEPRECATE_ME_EQUIPMENT";

        public override string EquipmentPickupDesc => "";

        public override string EquipmentFullDescription => "";

        public override string EquipmentLore => "";

        public override string EquipmentModelPath => "";

        public override string EquipmentIconPath => "";


        protected override void CreateConfig(ConfigFile config)
        {

        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        protected override void Initialization()
        {

        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            return false;
        }


    }
}
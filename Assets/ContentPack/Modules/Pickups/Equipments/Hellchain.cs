using RoR2;

namespace TurboEdition.Equipments
{
    internal class Hellchain : Equipment
    {
        public override EquipmentDef equipmentDef { get; set; } = Assets.mainAssetBundle.LoadAsset<EquipmentDef>("HellChain");
        public override void Initialize()
        {
            base.Initialize();
        }

    }
}
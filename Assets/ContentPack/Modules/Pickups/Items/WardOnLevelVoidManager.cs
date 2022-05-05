using RoR2;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    public class WardOnLevelVoidManager
    {
        private static float activationWindow = 30f;

        [SystemInitializer(typeof(PickupCatalog))]
        public static void Initialize()
        {
            RoR2.CharacterBody.onBodyStartGlobal += onBodyStartGlobal;
        }

        private static void onBodyStartGlobal(CharacterBody obj)
        {
            if (!NetworkServer.active) return;
            if (!obj || !obj.inventory) return;
            int c = obj.inventory.GetItemCount(TEContent.Items.WardOnLevelVoid.itemIndex);
            if (Stage.instance.entryTime.timeSince <= activationWindow && c > 0)
                obj.AddTimedBuff(TEContent.Buffs.WardOnLevelVoid, 45 + ((c - 1 * 30)));
        }
    }
}
using RoR2;
using System;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    public class MoneyBankManager
    {
        /// <summary>
        /// Stored in the server ONLY. Gets fetched and synced by each interactor.
        /// </summary>
        public static uint serverCurrentMoneyAmount;

        /// <summary>
        /// Both client and server can access this information, as the calculation doesn't require networking.
        /// </summary>
        public static uint targetMoneyAmountToStore
        {
            get
            {
                int moneyFromFirstStack = 0;
                var teamComponents = TeamComponent.GetTeamMembers(TeamIndex.Player);
                foreach (var teamMember in teamComponents)
                {
                    if (teamMember.body.inventory && teamMember.body.inventory.GetItemCount(TEContent.Items.MoneyBank) > 0)
                    {
                        moneyFromFirstStack += 500; //Basically count that tracks bodies that have at least one item
                    }
                }
                return (uint)(moneyFromFirstStack + (Util.GetItemCountForTeam(TeamIndex.Player, TEContent.Items.MoneyBank.itemIndex, false, true) * 25) * (Run.instance.stageClearCount + 1));
            }
        }

        public static bool CanStoreMoney
        {
            get
            {
                if (!NetworkServer.active)
                {
                    return false;
                }
                return serverCurrentMoneyAmount < targetMoneyAmountToStore;
            }
        }

        [SystemInitializer(new Type[]
        {
            typeof(ItemCatalog),
        })]
        public static void Init()
        {
            CharacterBody.onBodyInventoryChangedGlobal += onBodyInventoryChangedGlobal;
        }

        private static void onBodyInventoryChangedGlobal(CharacterBody obj)
        {
            if (NetworkServer.active && obj.teamComponent.teamIndex == TeamIndex.Player)
            {
                if (serverCurrentMoneyAmount > targetMoneyAmountToStore)
                {
                    serverCurrentMoneyAmount = targetMoneyAmountToStore;
                }
            }
        }

        public static bool AddMoney(int amount)
        {
            if (serverCurrentMoneyAmount >= UInt32.MaxValue || amount < 0)
            {
                return false;
            }
            serverCurrentMoneyAmount += (uint)amount;
            return true;
        }

        public static uint SubstractMoney(uint amount)
        {
            if (serverCurrentMoneyAmount <= 0)
                return 0;
            serverCurrentMoneyAmount -= amount;
            return amount;
        }
    }
}
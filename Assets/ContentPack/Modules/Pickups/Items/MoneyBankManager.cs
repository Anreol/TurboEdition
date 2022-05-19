using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
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
                        moneyFromFirstStack += 200; //Basically count that tracks bodies that have at least one item
                    }
                }
                return (uint)(moneyFromFirstStack + (Run.instance.GetDifficultyScaledCost(Util.GetItemCountForTeam(TeamIndex.Player, TEContent.Items.MoneyBank.itemIndex, false, true) - (moneyFromFirstStack / 200))*100));
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

        public static bool AddMoney(int amount)
        {
            if (serverCurrentMoneyAmount >= UInt32.MaxValue)
            {
                return false;
            }
            if (amount < 0)
            {
                serverCurrentMoneyAmount -= (uint)Mathf.Abs(amount);
                return true;
            }
            serverCurrentMoneyAmount += (uint)amount;
            return true;
        }
        public static int SubstractMoney(int amount)
        {
            if (serverCurrentMoneyAmount <= 0)
                return 0;
            if (Mathf.Abs(amount) >= serverCurrentMoneyAmount)
            {
                amount = (int)serverCurrentMoneyAmount;
                serverCurrentMoneyAmount = 0;
                return amount;
            }
            serverCurrentMoneyAmount -= (uint)Mathf.Abs(amount);
            return amount;
        }
    }
}

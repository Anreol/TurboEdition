using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurboEdition.Components;
using UnityEngine;

namespace TurboEdition.UI
{
    [RequireComponent(typeof(RectTransform))]
    class MoneyBankPanel : MonoBehaviour
    {
        public MoneyBankInteractorController interactorController;
        public int moneyAmount;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static RoR2.UI.HealthBarStyle;

namespace TurboEdition.ScriptableObjects
{
    [CreateAssetMenu(menuName = "TurboEdition/UI/HealthBarStyle")]
    public class SerializableBarStyle : ScriptableObject
    {
        public BarStyle style;
    }
}

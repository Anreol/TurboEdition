using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TurboEdition.UI
{
    interface IStatBarProvider
    {
        StatBarData GetStatBarData();
    }
    public struct StatBarData
    {
        public float maxData;
        public float currentData;
        /// <summary>
        /// Appears if currentData is zero or less
        /// </summary>
        public string offData;
        public Color fillBarColor;
        public Sprite sprite;
        public TooltipContent tooltipContent;

    }
}
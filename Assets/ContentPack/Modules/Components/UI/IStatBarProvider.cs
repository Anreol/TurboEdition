using RoR2.UI;
using UnityEngine;

namespace TurboEdition.UI
{
    internal interface IStatBarProvider
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
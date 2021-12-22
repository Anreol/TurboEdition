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
        //So fucking stupid but i dont want to deal with unity's broken serialization of structs
        float GetDataCurrent();
        float GetDataMax();
        Sprite GetSprite();
        Color GetColor();
    }
}
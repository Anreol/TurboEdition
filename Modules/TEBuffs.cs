using RoR2;
using System.Collections.Generic;
using UnityEngine;

//This file's purpose is to add generic buffs not linked to any item in specific, and has or could have a wider use.
namespace TurboEdition.Modules
{
    public class TEBuffs
    {
        internal static List<BuffDef> buffDefs = new List<BuffDef>();

        public TEBuffs()
        {
            RegisterBuffs();
        }

        protected void RegisterBuffs()
        {
            //Apply flat % damage reduction and inmunity to knockback
            var fortifiedBuff = ScriptableObject.CreateInstance<BuffDef>();
            fortifiedBuff.buffColor = Color.white;
            fortifiedBuff.canStack = false;
            fortifiedBuff.iconSprite = TurboEdition.MainAssets.LoadAsset<Sprite>("Assets/Textures/Icons/Buffs/TODO");
            fortifiedBuff.isDebuff = false;
            fortifiedBuff.name = "Fortified";
            buffDefs.Add(fortifiedBuff);

            //Make enemies target you before anything else
            var tauntBuff = ScriptableObject.CreateInstance<BuffDef>();
            tauntBuff.buffColor = Color.white;
            tauntBuff.canStack = false;
            tauntBuff.iconSprite = TurboEdition.MainAssets.LoadAsset<Sprite>("Assets/Textures/Icons/Buffs/TODO");
            tauntBuff.isDebuff = false;
            tauntBuff.name = "Taunting";
            buffDefs.Add(tauntBuff);

            //Shake player's crosshair
            var staticShockBuff = ScriptableObject.CreateInstance<BuffDef>();
            staticShockBuff.buffColor = Color.white;
            staticShockBuff.canStack = false;
            staticShockBuff.iconSprite = TurboEdition.MainAssets.LoadAsset<Sprite>("Assets/Textures/Icons/Buffs/TODO");
            staticShockBuff.isDebuff = true;
            staticShockBuff.name = "Shocked";
            buffDefs.Add(staticShockBuff);

            //Inhability to sprint
            var buzzedBuff = ScriptableObject.CreateInstance<BuffDef>();
            buzzedBuff.buffColor = Color.white;
            buzzedBuff.canStack = false;
            buzzedBuff.iconSprite = TurboEdition.MainAssets.LoadAsset<Sprite>("Assets/Textures/Icons/Buffs/TODO");
            buzzedBuff.isDebuff = true;
            buzzedBuff.name = "Buzzed";
            buffDefs.Add(buzzedBuff);

            //Transform into beetle
            var polymorphBuff = ScriptableObject.CreateInstance<BuffDef>();
            polymorphBuff.buffColor = Color.white;
            polymorphBuff.canStack = false;
            polymorphBuff.iconSprite = TurboEdition.MainAssets.LoadAsset<Sprite>("Assets/Textures/Icons/Buffs/TODO");
            polymorphBuff.isDebuff = false;
            polymorphBuff.name = "Polymorph";
            buffDefs.Add(polymorphBuff);

            //Transform into any body
            var polymorphRandomBuff = ScriptableObject.CreateInstance<BuffDef>();
            polymorphRandomBuff.buffColor = Color.white;
            polymorphRandomBuff.canStack = false;
            polymorphRandomBuff.iconSprite = TurboEdition.MainAssets.LoadAsset<Sprite>("Assets/Textures/Icons/Buffs/TODO");
            polymorphRandomBuff.isDebuff = false;
            polymorphRandomBuff.name = "Chaotic Polymorph";
            buffDefs.Add(polymorphRandomBuff);

            //Disable lunar items except heresy (?)
            var disableLunarBuff = ScriptableObject.CreateInstance<BuffDef>();
            disableLunarBuff.buffColor = Color.white;
            disableLunarBuff.canStack = false;
            disableLunarBuff.iconSprite = TurboEdition.MainAssets.LoadAsset<Sprite>("Assets/Textures/Icons/Buffs/TODO");
            disableLunarBuff.isDebuff = false; //it's a debuff but lets make it so you cannot remove it
            disableLunarBuff.name = "Cosmic Distress";
            buffDefs.Add(disableLunarBuff);

            //Make fire deal more damage
            var oiledBuff = ScriptableObject.CreateInstance<BuffDef>();
            oiledBuff.buffColor = Color.white;
            oiledBuff.canStack = true;
            oiledBuff.iconSprite = TurboEdition.MainAssets.LoadAsset<Sprite>("Assets/Textures/Icons/Buffs/TODO");
            oiledBuff.isDebuff = true;
            oiledBuff.name = "Oiled";
            buffDefs.Add(oiledBuff);
        }
    }
}
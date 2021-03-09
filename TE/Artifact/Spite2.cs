using BepInEx.Configuration;
using MonoMod.Cil;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static TurboEdition.Utils.ItemHelpers;

namespace TurboEdition.Artifacts
{
    public class Spite2 : ArtifactBase<Spite2>
    {
        public override string ArtifactName => "Revenge";
        public override string ArtifactDesc => "Enemies have a chance to drop spite bombs on hit. Extra bombs if Spite is enabled.";
        public override string SpriteSelectedPath => "@TurboEdition:Assets/Textures/Icons/Artifacts/spite2_selected.png";
        public override string SpriteDeselectedPath => "@TurboEdition:Assets/Textures/Icons/Artifacts/spite2_deselected.png";
        public override string ArtifactModelPath => "@TurboEdition:Assets/Models/Prefabs/Default.prefab";

        protected override void Initialization() { }
        public override void Hooks()
        {

        }
    }
}

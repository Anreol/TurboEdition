using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace TurboEdition.Artifacts
{
    // The directly below is entirely from TILER2 API (by ThinkInvis) specifically the Item module. Utilized to keep instance checking functionality as I migrate off TILER2.
    // TILER2 API can be found at the following places:
    // https://github.com/ThinkInvis/RoR2-TILER2
    // https://thunderstore.io/package/ThinkInvis/TILER2/

    public abstract class ArtifactBase<T> : ArtifactBase where T : ArtifactBase<T>
    {
        public static T instance { get; private set; }
        public ArtifactBase()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBoilerplate/ArtifactBase was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class ArtifactBase
    {
        public ArtifactIndex ArtIndex;
        public abstract string ArtifactName { get; }
        public abstract string ArtifactDesc { get; }
        public abstract string SpriteSelectedPath { get; }
        public abstract string SpriteDeselectedPath { get; }
        public abstract string ArtifactModelPath { get; }

        protected abstract void Initialization();


        /// <summary>
        /// Only override when you know what you are doing, or call base.Init()!
        /// </summary>
        /// <param name="config"></param>
        internal virtual void Init(ConfigFile config)
        {
            CreateConfig(config);
            //CreateLang();
            CreateArtifact();
            Initialization();
            Hooks();
        }

        protected virtual void CreateConfig(ConfigFile config) { }


        //Do artifacts need lang tokens?
        //I do not know
        protected virtual void CreateLang()
        {
            LanguageAPI.Add("ARTIFACT_" + ArtifactName + "_NAME", ArtifactName);
            LanguageAPI.Add("ARTIFACT_" + ArtifactName + "_PICKUP", ArtifactDesc);
        }

        protected void CreateArtifact()
        {
            ArtifactDef ArtDef = ScriptableObject.CreateInstance<ArtifactDef>();
            ArtDef.pickupModelPrefab = Resources.Load<GameObject>(ArtifactModelPath);
            ArtDef.smallIconSelectedSprite = Resources.Load<Sprite>(SpriteSelectedPath);
            ArtDef.smallIconDeselectedSprite = Resources.Load<Sprite>(SpriteDeselectedPath);
            ArtDef.unlockableName = "ARTIFACT_" + ArtifactName;
            ArtDef.nameToken = "ARTIFACT_" + ArtifactName + "_NAME";
            ArtDef.descriptionToken = "ARTIFACT_" + ArtifactName + "_DESCRIPTION";
            ArtifactCatalog.getAdditionalEntries += (list) =>
            {
                list.Add(ArtDef);
            };
            On.RoR2.ArtifactCatalog.SetArtifactDefs += (orig, self) => {
                orig(self);
                ArtIndex = ArtDef.artifactIndex;
            };
        }

        public virtual void Hooks() { }

        //Based on ThinkInvis' methods
        public bool IsActive()
        {
            return RunArtifactManager.instance != null && RunArtifactManager.instance.IsArtifactEnabled(ArtIndex);
        }
    }
}


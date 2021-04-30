using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

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

        public abstract string ArtifactLangToken { get; }
        public abstract string ArtifactName { get; }
        public abstract string ArtifactDesc { get; }

        public abstract UnlockableDef ArtifactUnlockable { get; }
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
            CreateLang();
            CreateArtifact();
            Initialization();
            Hooks();
        }

        protected virtual void CreateConfig(ConfigFile config)
        {
        }

        //Do artifacts need lang tokens?
        //I do not know
        protected virtual void CreateLang()
        {
            LanguageAPI.Add("ARTIFACT_" + ArtifactLangToken + "_NAME", ArtifactName);
            LanguageAPI.Add("ARTIFACT_" + ArtifactLangToken + "_DESC", ArtifactDesc);
        }

        protected void CreateArtifact()
        {
#if DEBUG
            TurboEdition._logger.LogWarning("ArtifactBase, creating new artifact: " + ArtifactLangToken);
#endif
            ArtifactDef ArtDef = ScriptableObject.CreateInstance<ArtifactDef>();
            ArtDef.pickupModelPrefab = Resources.Load<GameObject>(ArtifactModelPath);
            ArtDef.smallIconSelectedSprite = Resources.Load<Sprite>(SpriteSelectedPath);
            ArtDef.smallIconDeselectedSprite = Resources.Load<Sprite>(SpriteDeselectedPath);
            ArtDef.unlockableDef = ArtifactUnlockable; //DO NOT SET THIS UP UNLESS THERES AN ACTUAL UNLOCKABLE
            ArtDef.nameToken = "ARTIFACT_" + ArtifactLangToken + "_NAME";
            ArtDef.descriptionToken = "ARTIFACT_" + ArtifactLangToken + "_DESC";
#if DEBUG
            TurboEdition._logger.LogWarning("ArtifactBase, done defining " + ArtifactLangToken + ", it has the following tokens: " + ArtDef.unlockableDef + " " + ArtDef.nameToken + " " + ArtDef.descriptionToken + " and assets: " + ArtDef.pickupModelPrefab + " " + ArtDef.smallIconDeselectedSprite + " " + ArtDef.smallIconSelectedSprite + ".");
#endif
            ArtifactCatalog.getAdditionalEntries += (list) =>
            {
                list.Add(ArtDef);
#if DEBUG
                TurboEdition._logger.LogWarning("ArtifactBase, added: " + ArtDef.nameToken + " with def " + ArtDef);
#endif
            };
            On.RoR2.ArtifactCatalog.SetArtifactDefs += (orig, self) =>
            {
                orig(self);
                ArtIndex = ArtDef.artifactIndex;
#if DEBUG
                TurboEdition._logger.LogWarning("ArtifactBase, got the ArtifactCatalog index " + ArtIndex + " (" + ArtDef.artifactIndex + ") of " + ArtDef + " (" + ArtDef.nameToken + ").");
                TurboEdition._logger.LogWarning("ArtifactBase, searching thru the ArtifactCatalog by index " + ArtIndex + " got " + ArtifactCatalog.GetArtifactDef(ArtDef.artifactIndex).unlockableDef + " finding artifact index by def name (" + ArtifactCatalog.FindArtifactIndex(ArtDef.nameToken) + " " + ArtifactCatalog.FindArtifactIndex(ArtDef.descriptionToken) + ")");
#endif
            };
        }

        //A hook that hooks hooks hooking hooks, is this stupid? Sounds stupid.
        //sometimes i look at english words and i go letter by letter and see how stupid they are
        //Hook: From Middle English hoke, from Old English hōc, from Proto-Germanic *hōkaz (cf. West Frisian/Dutch hoek 'hook, angle, corner', Low German Hook, Huuk 'id.'), variant of *hakōn 'hook'. More at hake.
        //Oi yong ladde, myntan tō laetan mē hōc tō bis hōc swā I cunnan modifien bis videō pleġan
        public virtual void Hooks()
        {
            RunArtifactManager.onArtifactEnabledGlobal += OnArtifactEnabled;
            RunArtifactManager.onArtifactDisabledGlobal += OnArtifactDisabled;
        }

        protected virtual void OnArtifactEnabled(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
        {
            if (!NetworkServer.active)
            {
                return;
            }
            if (artifactDef.artifactIndex != ArtIndex)
            {
                return;
            }
            HookEnabled();
        }

        protected virtual void OnArtifactDisabled(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
        {
            if (artifactDef.artifactIndex != ArtIndex)
            {
                return;
            }
            HookDisabled();
        }

        /// <summary>
        /// Gets called whenever the artifact gets activated.
        /// </summary>
        protected virtual void HookEnabled() { }

        /// <summary>
        /// Gets called whenever the artifact gets disabled.
        /// </summary>
        protected virtual void HookDisabled() { }

        //Latin is better anyways

        //Based on ThinkInvis' methods
        public bool ArtifactIsActive()
        {
            return (RunArtifactManager.instance != null && RunArtifactManager.instance.IsArtifactEnabled(ArtIndex));
        }
    }
}
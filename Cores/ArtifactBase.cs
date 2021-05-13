using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Artifacts
{
    public abstract class ArtifactBase<T> : ArtifactBase where T : ArtifactBase<T>
    {
        public static T instance { get; private set; }

        public ArtifactBase()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ArtifactBase was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class ArtifactBase
    {
        public ArtifactDef ArtDef;

        public abstract string ArtifactLangToken { get; }
        public abstract string ArtifactName { get; }
        public abstract string ArtifactDesc { get; }

        public abstract UnlockableDef ArtifactUnlockable { get; }
        public abstract Sprite ArtifactEnabledIcon { get; }
        public abstract Sprite ArtifactDisabledIcon { get; }
        public abstract GameObject ArtifactModel { get; }
        public bool ArtifactEnabled => RunArtifactManager.instance.IsArtifactEnabled(ArtDef);

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
            ArtDef.pickupModelPrefab = ArtifactModel;
            ArtDef.smallIconSelectedSprite = ArtifactEnabledIcon;
            ArtDef.smallIconDeselectedSprite = ArtifactDisabledIcon;
            ArtDef.unlockableDef = ArtifactUnlockable; //DO NOT SET THIS UP UNLESS THERES AN ACTUAL UNLOCKABLE
            ArtDef.nameToken = "ARTIFACT_" + ArtifactLangToken + "_NAME";
            ArtDef.descriptionToken = "ARTIFACT_" + ArtifactLangToken + "_DESC";

            ArtifactAPI.Add(ArtDef);
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
            if (!NetworkServer.active && artifactDef == ArtDef) { HookEnabled(); }
            return;
        }

        protected virtual void OnArtifactDisabled(RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
        {
            if (artifactDef == ArtDef) { HookDisabled(); }
            return;
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
    }
}
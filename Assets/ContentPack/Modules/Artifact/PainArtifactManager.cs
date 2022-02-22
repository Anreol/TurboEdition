using RoR2;
using RoR2.Artifacts;
using System;
using System.Collections.Generic;
using TurboEdition.Components;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Artifacts
{
    internal class PainArtifactManager
    {
        public static ArtifactDef artifact = Assets.mainAssetBundle.LoadAsset<ArtifactDef>("PainArtifact");

        [SystemInitializer(new Type[]
        {
            typeof(ArtifactCatalog)
        })]
        private static void Init()
        {
            RunArtifactManager.onArtifactEnabledGlobal += onArtifactEnabledGlobal;
            RunArtifactManager.onArtifactDisabledGlobal += onArtifactDisabledGlobal;
        }

        private static void onArtifactDisabledGlobal([JetBrains.Annotations.NotNull] RunArtifactManager runArtifactManager, [JetBrains.Annotations.NotNull] ArtifactDef artifactDef)
        {
            if (artifactDef != artifact || !NetworkServer.active)
                return;
            CharacterBody.onBodyStartGlobal -= onBodyStartServer;
            List<PainArtifactServerListener> painArtifactServerListeners = InstanceTracker.GetInstancesList<PainArtifactServerListener>();
            foreach (PainArtifactServerListener item in painArtifactServerListeners)
            {
                UnityEngine.Object.Destroy(item);
            }
        }

        private static void onArtifactEnabledGlobal([JetBrains.Annotations.NotNull] RunArtifactManager runArtifactManager, [JetBrains.Annotations.NotNull] ArtifactDef artifactDef)
        {    //uNet Weaver doesnt like [Server] Tags on something that isnt a network behavior
            if (artifactDef != artifact || !NetworkServer.active)
                return;
            CharacterBody.onBodyStartGlobal += onBodyStartServer;
        }

        private static void onBodyStartServer(CharacterBody obj)
        {
            if (obj.isPlayerControlled)
            {
                obj.gameObject.AddComponent<PainArtifactServerListener>().body = obj;
            }
        }
    }
}
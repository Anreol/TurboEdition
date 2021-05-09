using BepInEx;
using BepInEx.Logging;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TurboEdition.Artifacts;
using UnityEngine;

namespace TurboEdition.Modules
{
    class TEArtifacts
    {
        public static TEArtifacts instance;
        public List<ArtifactDef> ArtifactDefs = new List<ArtifactDef>();
        public List<ArtifactBase> ArtifactList = new List<ArtifactBase>();

        public TEArtifacts()
        {
            instance = this;
        }

        public void InitArtfs()
        {
            var ArtifactTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ArtifactBase)));
            foreach (var artifactType in ArtifactTypes)
            {
#if DEBUG
                TurboEdition._logger.LogWarning("Initializing artifacts, adding: " + artifactType + " in a ArtifactTypes of count " + ArtifactTypes.Count());
#endif
                ArtifactBase artifact = (ArtifactBase)System.Activator.CreateInstance(artifactType);
                if (ValidateArtifact(artifact, ArtifactList))
                {
#if DEBUG
                    TurboEdition._logger.LogWarning("Validated artifact, and intializing config of " + artifact);
#endif
                    artifact.Init(TurboEdition.instance.Config);
                }
            }
        }
        /// <summary>
        /// A helper to easily set up and initialize an artifact from your artifact classes if the user has it enabled in their configuration files.
        /// </summary>
        /// <param name="artifact">A new instance of an ArtifactBase class. E.g. "new ExampleArtifact()"</param>
        /// <param name="artifactList">The list you would like to add this to if it passes the config check.</param>
        public bool ValidateArtifact(ArtifactBase artifact, List<ArtifactBase> artifactList)
        {
            if (TurboEdition.instance.Config.Bind<bool>("Artifact: " + artifact.ArtifactName, "Enable Artifact?", true, "Should this artifact be enabled in game?").Value)
            {
#if DEBUG
                TurboEdition._logger.LogWarning("Validating artifact " + artifact + ", user has it set to enabled, so adding it to " + artifactList);
#endif
                artifactList.Add(artifact);
                return true;
            }
            return false;
        }
    }
}

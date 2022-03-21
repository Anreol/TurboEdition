using EntityStates;
using RoR2;
using RoR2.ContentManagement;
using RoR2.ExpansionManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UnityEngine;

namespace TurboEdition
{
    public class TEContent : IContentPackProvider
    {
        public delegate IEnumerator LoadStaticContentAsyncDelegate(LoadStaticContentAsyncArgs args);

        public delegate IEnumerator GenerateContentPackAsyncDelegate(GetContentPackAsyncArgs args);

        public delegate IEnumerator FinalizeAsyncDelegate(FinalizeAsyncArgs args);

        public static LoadStaticContentAsyncDelegate onLoadStaticContent { get; set; }
        public static GenerateContentPackAsyncDelegate onGenerateContentPack { get; set; }
        public static FinalizeAsyncDelegate onFinalizeAsync { get; set; }

        public string identifier => TurboUnityPlugin.ModIdentifier;
        public SerializableContentPack serializableContentPack; //Registration
        public ContentPack tempPackFromSerializablePack = new ContentPack(); //One step away from finalization

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            //Assetbundle fuckery, unity stuff.
            List<AssetBundle> loadedBundles = new List<AssetBundle>();
            var bundlePaths = Assets.GetAssetBundlePaths();
            int num;
            for (int i = 0; i < bundlePaths.Length; i = num)
            {
                var bundleLoadRequest = AssetBundle.LoadFromFileAsync(bundlePaths[i]);
                while (!bundleLoadRequest.isDone)
                {
                    args.ReportProgress(Util.Remap(bundleLoadRequest.progress + i, 0f, bundlePaths.Length, 0f, 0.8f));
                    yield return null;
                }
                num = i + 1;
                loadedBundles.Add(bundleLoadRequest.assetBundle);
            }

            //Content pack things, RoR2 systems.
            Assets.loadedAssetBundles = new ReadOnlyCollection<AssetBundle>(loadedBundles);
            serializableContentPack = Assets.mainAssetBundle.LoadAsset<SerializableContentPack>("ContentPackV2");

            tempPackFromSerializablePack = serializableContentPack.CreateContentPack();
            tempPackFromSerializablePack.identifier = identifier;

            InitBuffs.Init();
            InitVFX.Init();

            ContentLoadHelper.PopulateTypeFields<ArtifactDef>(typeof(TEContent.Artifacts), tempPackFromSerializablePack.artifactDefs);
            ContentLoadHelper.PopulateTypeFields<ItemDef>(typeof(TEContent.Items), tempPackFromSerializablePack.itemDefs);
            ContentLoadHelper.PopulateTypeFields<EquipmentDef>(typeof(TEContent.Equipment), tempPackFromSerializablePack.equipmentDefs);
            ContentLoadHelper.PopulateTypeFields<BuffDef>(typeof(TEContent.Buffs), tempPackFromSerializablePack.buffDefs, (string fieldName) => "bd" + fieldName);
            //ContentLoadHelper.PopulateTypeFields<EliteDef>(typeof(TEContent.Elites), contentPackFromSerializableContentPack.eliteDefs, (string fieldName) => "ed" + fieldName);
            ContentLoadHelper.PopulateTypeFields<SurvivorDef>(typeof(TEContent.Survivors), tempPackFromSerializablePack.survivorDefs);
            ContentLoadHelper.PopulateTypeFields<ExpansionDef>(typeof(TEContent.Expansions), tempPackFromSerializablePack.expansionDefs);

            if (onLoadStaticContent != null)
                yield return onLoadStaticContent;

            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            //NOTE: For some reason any instructions that are put inside of here are done twice. This will cause serious errors if you do not watch out.
            ContentPack.Copy(tempPackFromSerializablePack, args.output);

            if (onGenerateContentPack != null)
                yield return onGenerateContentPack;

            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            RoR2Application.onLoad += (delegate ()
            {
                if (Directory.Exists(Assets.languageRoot))
                {
                    //Misc.MiscLanguage.FixLanguageFolders(Assets.languageRoot);
                    Misc.MiscLanguage.AddDeathMessages();
                }
            });

            RoR2Application.isModded = true;
            args.ReportProgress(1f);
            yield break;
        }

        public static class Artifacts
        {
            public static ArtifactDef MeltdownArtifact;
            public static ArtifactDef PainArtifact;
            public static ArtifactDef Spite2Artifact;
            public static ArtifactDef WormsArtifact;
        }

        public static class Items
        {
            public static ItemDef AddTeleporterRadius;
            public static ItemDef BaneMask;
            public static ItemDef BloodEconomy;
            public static ItemDef DropletDupe;
            public static ItemDef GracePeriod;
            public static ItemDef Hitlag;
            public static ItemDef ItemDeployer;
            public static ItemDef KnifeFan;
            public static ItemDef MeleeArmor;
            public static ItemDef PackMagnet;
            public static ItemDef RadioSearch;
            public static ItemDef SoulDevourer;
            public static ItemDef StandBonus;
            public static ItemDef SuperStickies;
            public static ItemDef Typewriter;
            public static ItemDef WarbannerVoid;
        }

        public static class Equipment
        {
            public static EquipmentDef CursedScythe;
            public static EquipmentDef Hellchain;
            public static EquipmentDef LeaveStage;
        }

        public static class Buffs
        {
            public static BuffDef CannotSprint;
            public static BuffDef ElectroStatic;
            public static BuffDef HellLinked;
            public static BuffDef MeleeArmor;
            public static BuffDef Panic;
            public static BuffDef WarbannerVoid;
        }
        public static class Expansions
        {
            public static ExpansionDef TurboExpansion;
        }
        public static class Survivors
        {
            public static SurvivorDef Grenadier;
        }
    }
}
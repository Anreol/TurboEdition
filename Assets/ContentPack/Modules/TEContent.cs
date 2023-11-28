using RoR2;
using RoR2.ContentManagement;
using RoR2.ExpansionManagement;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using TurboEdition.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TurboEdition
{
    public class TEContent : IContentPackProvider
    {
        private static bool alreadyLoadedBaseGame = false;

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

            ContentLoadHelper.PopulateTypeFields<ArtifactDef>(typeof(TEContent.Artifacts), tempPackFromSerializablePack.artifactDefs);
            ContentLoadHelper.PopulateTypeFields<ItemTierDef>(typeof(TEContent.ItemTiers), tempPackFromSerializablePack.itemTierDefs);
            ContentLoadHelper.PopulateTypeFields<ItemDef>(typeof(TEContent.Items), tempPackFromSerializablePack.itemDefs);
            ContentLoadHelper.PopulateTypeFields<EquipmentDef>(typeof(TEContent.Equipment), tempPackFromSerializablePack.equipmentDefs);
            ContentLoadHelper.PopulateTypeFields<BuffDef>(typeof(TEContent.Buffs), tempPackFromSerializablePack.buffDefs, (string fieldName) => "bd" + fieldName);
            //ContentLoadHelper.PopulateTypeFields<EliteDef>(typeof(TEContent.Elites), contentPackFromSerializableContentPack.eliteDefs, (string fieldName) => "ed" + fieldName);
            ContentLoadHelper.PopulateTypeFields<SurvivorDef>(typeof(TEContent.Survivors), tempPackFromSerializablePack.survivorDefs);
            ContentLoadHelper.PopulateTypeFields<ExpansionDef>(typeof(TEContent.Expansions), tempPackFromSerializablePack.expansionDefs);
            ContentLoadHelper.PopulateTypeFields<SceneDef>(typeof(TEContent.Scenes), tempPackFromSerializablePack.sceneDefs);

            //This shouldn't go earlier than the type field population!
            InitBuffs.Init();
            InitVFX.Init();

            if (onLoadStaticContent != null)
                yield return onLoadStaticContent(args);

            args.ReportProgress(1f);
            yield break;
        }

        /// <summary>
        /// NOTE: Every instruction here will be done as many times as the current number of content packs the game has. This will cause serious errors if you do not watch out.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(tempPackFromSerializablePack, args.output);

            bool baseGameLoaded = false;
            bool dlcLoaded = false;
            foreach (ContentPackLoadInfo cpli in args.peerLoadInfos)
            {
                if (cpli.previousContentPack.identifier == "RoR2.BaseContent")
                    baseGameLoaded = true;
                if (cpli.previousContentPack.identifier == "RoR2.DLC1")
                    dlcLoaded = true;
            }

            if (baseGameLoaded && dlcLoaded && !alreadyLoadedBaseGame)
            {
                alreadyLoadedBaseGame = true;
                //Void Items.
                args.output.itemRelationshipProviders[0].relationshipType = DLC1Content.ItemRelationshipTypes.ContagiousItem;
                args.output.itemRelationshipProviders[0].relationships[0].itemDef1 = RoR2Content.Items.WardOnLevel;

                //Scene destinations.
                TEContent.Scenes.observatory.destinationsGroup = Addressables.LoadAssetAsync<SceneCollection>("RoR2/Base/SceneGroups/sgStage1.asset").WaitForCompletion();
            }

            if (onGenerateContentPack != null)
                yield return onGenerateContentPack(args);

            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            if (Directory.Exists(Assets.languageRoot))
            {
                Language.collectLanguageRootFolders += (List<string> stringList) => stringList.Add(Assets.languageRoot);
                Utils.MiscLanguage.AddDeathMessages();
            }
            CostAndStatExtras.Init();

            //Extra stuff for hopoo
            RoR2Application.isModded = true;
            //Gets resolved to a hash, adding mod guid makes it unique, and adding modVer changes the hash generated with different versions. It can actually be literally any string desired. This is what appears in the mod mismatch error message when connecting to a remote server.
            NetworkModCompatibilityHelper.networkModList = NetworkModCompatibilityHelper.networkModList.Append(TurboUnityPlugin.ModGuid + ";" + TurboUnityPlugin.ModVer);

            if (onFinalizeAsync != null)
                yield return onFinalizeAsync(args);

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

        public static class ItemTiers
        {
            public static ItemTierDef CurseItemTier;
            public static ItemTierDef DualItemTier;
        }

        public static class Items
        {
            public static ItemDef AddTeleporterRadius;
            public static ItemDef AirborneBonus;
            public static ItemDef AirborneDash;
            public static ItemDef BaneMask;
            public static ItemDef BloodEconomy;
            public static ItemDef DropletDupe;
            public static ItemDef GracePeriod;
            public static ItemDef Hitlag;
            public static ItemDef ItemDeployer;
            public static ItemDef KnifeFan;
            public static ItemDef MeleeArmor;
            public static ItemDef MoneyBank;
            public static ItemDef PackMagnet;
            public static ItemDef PackDuplicator;
            public static ItemDef RadioSearch;
            public static ItemDef RadioSearchVoid;
            public static ItemDef SoulDevourer;
            public static ItemDef StandBonus;
            public static ItemDef SuperStickies;
            public static ItemDef Typewriter;
            public static ItemDef WardOnLevelVoid;
        }

        public static class Equipment
        {
            public static EquipmentDef CursedScythe;
            public static EquipmentDef Hellchain;
            public static EquipmentDef LeaveStage;
            public static EquipmentDef VoidSquad;
        }

        public static class Buffs
        {
            public static BuffDef CannotSprint;
            public static BuffDef ElectroStatic;
            public static BuffDef HellLinked;
            public static BuffDef MeleeArmor;
            public static BuffDef Panic;
            public static BuffDef WardOnLevelVoid;
        }

        public static class Expansions
        {
            public static ExpansionDef TurboExpansion;
        }

        public static class Survivors
        {
            public static SurvivorDef Grenadier;
            public static SurvivorDef StaffMage;
        }

        public static class Scenes
        {
            public static SceneDef observatory;
        }
    }
}
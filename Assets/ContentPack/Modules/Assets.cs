using RoR2.ContentManagement;
using System.Collections;
using System.Reflection;
using UnityEngine;
using Path = System.IO.Path;

namespace TurboEdition
{
	public static class Assets
	{
		public static AssetBundle mainAssetBundle = null;
		internal static string assetBundleName = "assetTurbo";

		public static void PopulateAssets()
		{
			var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			mainAssetBundle = AssetBundle.LoadFromFile(Path.Combine(path, assetBundleName));
			ContentPackProvider.serializedContentPack = mainAssetBundle.LoadAsset<SerializableContentPack>(ContentPackProvider.contentPackName);
		}
	}

	public class ContentPackProvider : IContentPackProvider
	{
		public static SerializableContentPack serializedContentPack;
		public static ContentPack contentPack;
		//Should be the same names as your SerializableContentPack in the asset bundle
		public static string contentPackName = "ContentPack";

		public string identifier
		{
			get
			{
				//If I see this name while loading a mod I will make fun of you
				return TurboEdition.ModIdentifier;
			}
		}

		internal static void Initialize()
		{
			contentPack = serializedContentPack.CreateContentPack();
			ContentManager.collectContentPackProviders += AddCustomContent;
		}

		private static void AddCustomContent(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
		{
			addContentPackProvider(new ContentPackProvider());
		}

		public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
		{
			args.ReportProgress(1f);
			yield break;
		}

		public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
		{
			ContentPack.Copy(contentPack, args.output);
			args.ReportProgress(1f);
			yield break;
		}

		public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
		{
			args.ReportProgress(1f);
			yield break;
		}
		/*
		internal static void LoadSoundbank()
		{
			string soundBankPath = Path.Combine(assemblyDir, soundBankName);
			byte[] array = File.ReadAllBytes(soundBankPath);
			SoundAPI.SoundBanks.Add(array);
		}*/
	}
}
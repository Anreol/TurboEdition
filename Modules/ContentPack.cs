using RoR2.ContentManagement;

namespace TurboEdition.Modules
{
    internal class ContentPacks : IContentPackProvider
    {
        internal ContentPack contentPack = new ContentPack();
        public string identifier => TurboEdition.ModGuid;

        public void Initialize()
        {
            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
        }

        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(this);
        }

        public System.Collections.IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            this.contentPack.identifier = this.identifier;
            contentPack.buffDefs.Add(TEBuffs.buffDefs.ToArray());
            contentPack.equipmentDefs.Add(TEEquipments.instance.EquipmentDefs.ToArray());
            contentPack.itemDefs.Add(TEItems.instance.ItemDefs.ToArray());
            contentPack.artifactDefs.Add(TEArtifacts.instance.ArtifactDefs.ToArray());

            args.ReportProgress(1f);
            yield break;
        }

        public System.Collections.IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(this.contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public System.Collections.IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }
    }
}
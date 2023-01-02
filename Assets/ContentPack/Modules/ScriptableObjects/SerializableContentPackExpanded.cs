using EntityStates;
using RoR2;
using RoR2.ContentManagement;
using RoR2.EntitlementManagement;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TurboEdition
{
    [CreateAssetMenu(menuName = "TurboEdition/SerializableContentPackExpanded")]
    internal class SerializableContentPackExpanded : SerializableContentPack
    {
        public RoR2.ContentManagement.ContentPack CreateV1_2_1_0_ContentPack()
        {
            RoR2.ContentManagement.ContentPack contentPack = new RoR2.ContentManagement.ContentPack();
            contentPack.bodyPrefabs.Add(this.bodyPrefabs);
            contentPack.masterPrefabs.Add(this.masterPrefabs);
            contentPack.projectilePrefabs.Add(this.projectilePrefabs);
            contentPack.gameModePrefabs.Add(this.gameModePrefabs);
            contentPack.networkedObjectPrefabs.Add(this.networkedObjectPrefabs);
            contentPack.skillDefs.Add(this.skillDefs);
            contentPack.skillFamilies.Add(this.skillFamilies);
            contentPack.sceneDefs.Add(this.sceneDefs);
            contentPack.itemDefs.Add(this.itemDefs);
            contentPack.equipmentDefs.Add(this.equipmentDefs);
            contentPack.buffDefs.Add(this.buffDefs);
            contentPack.eliteDefs.Add(this.eliteDefs);
            contentPack.unlockableDefs.Add(this.unlockableDefs);
            contentPack.survivorDefs.Add(this.survivorDefs);
            contentPack.artifactDefs.Add(this.artifactDefs);
            contentPack.effectDefs.Add((from asset in this.effectDefs
                                        select new EffectDef(asset)).ToArray<EffectDef>());
            contentPack.surfaceDefs.Add(this.surfaceDefs);
            contentPack.networkSoundEventDefs.Add(this.networkSoundEventDefs);
            contentPack.musicTrackDefs.Add(this.musicTrackDefs);
            contentPack.gameEndingDefs.Add(this.gameEndingDefs);
            contentPack.entityStateConfigurations.Add(this.entityStateConfigurations);

            //Append with reflection
            GetType().Assembly.GetTypes()
                  .Where(type => typeof(EntityState).IsAssignableFrom(type))
                  .ToList()
                  .ForEach(state => HG.ArrayUtils.ArrayAppend(ref this.entityStateTypes, new SerializableEntityStateType(state)));

            List<Type> list = new List<Type>();
            for (int i = 0; i < this.entityStateTypes.Length; i++)
            {
                Type stateType = this.entityStateTypes[i].stateType;
                if (stateType != null)
                {
                    list.Add(stateType);
                }
                else
                {
                    Debug.LogWarning(string.Concat(new string[]
                    {
                        "SerializableContentPack \"",
                        base.name,
                        "\" could not resolve type with name \"",
                        this.entityStateTypes[i].typeName,
                        "\". The type will not be available in the content pack."
                    }));
                }
            }
            contentPack.entityStateTypes.Add(list.ToArray());

            //New Content
            contentPack.itemTierDefs.Add(this.itemTierDefs);
            contentPack.itemRelationshipProviders.Add(this.itemRelationshipProviders);
            contentPack.itemRelationshipTypes.Add(this.itemRelationshipTypes);
            contentPack.expansionDefs.Add(this.expansionDefs);
            contentPack.entitlementDefs.Add(this.entitlementDefs);
            contentPack.miscPickupDefs.Add(this.miscPickupDefs);
            return contentPack;
        }

        public override RoR2.ContentManagement.ContentPack CreateContentPack()
        {
            return CreateV1_2_1_0_ContentPack();
        }

        public ItemTierDef[] itemTierDefs = Array.Empty<ItemTierDef>();
        public ItemRelationshipProvider[] itemRelationshipProviders = Array.Empty<ItemRelationshipProvider>();
        public ItemRelationshipType[] itemRelationshipTypes = Array.Empty<ItemRelationshipType>();
        public ExpansionDef[] expansionDefs = Array.Empty<ExpansionDef>();
        public EntitlementDef[] entitlementDefs = Array.Empty<EntitlementDef>(); //I'll check if the user has RoR1 in its steam library, and that will be funny.
        public MiscPickupDef[] miscPickupDefs = Array.Empty<MiscPickupDef>();
    }
}
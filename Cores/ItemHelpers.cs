using RoR2;
using UnityEngine;
using RoR2.Networking;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

namespace TurboEdition.Utils
{
    internal class ItemHelpers
    {
        /// <summary>
        /// A helper that will set up the RendererInfos of a GameObject that you pass in.
        /// <para>This allows it to go invisible when your character is not visible, as well as letting overlays affect it.</para>
        /// </summary>
        /// <param name="obj">The GameObject/Prefab that you wish to set up RendererInfos for.</param>
        /// <returns>Returns an array full of RendererInfos for GameObject.</returns>
        public static CharacterModel.RendererInfo[] ItemDisplaySetup(GameObject obj)
        {
            MeshRenderer[] meshes = obj.GetComponentsInChildren<MeshRenderer>();
            CharacterModel.RendererInfo[] renderInfos = new CharacterModel.RendererInfo[meshes.Length];

            for (int i = 0; i < meshes.Length; i++)
            {
                renderInfos[i] = new CharacterModel.RendererInfo
                {
                    defaultMaterial = meshes[i].material,
                    renderer = meshes[i],
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false //We allow the mesh to be affected by overlays like OnFire or PredatoryInstinctsCritOverlay.
                };
            }

            return renderInfos;
        }

		//thing i copy pasted from the shop terminal beheavior because I cannot be fucking assed to figure out droptables
		
		//public List<PickupIndex> GetRunDropTables(ItemTier itemTier)
		//{
			/*Commenting the following since i cannot do a return on something that is not a void lol
			if (!NetworkServer.active)
			{
				Debug.LogWarning("[Server] function 'GenerateNewPickupServer FROM TURBO EDITION/CORES/ITEMHELPERS.cs' called on client");
				return;
			}
			//Also commenting this since i have no idea what the fuck this does
			/*if (this.dropTable)
			{
				newPickupIndex = this.dropTable.GenerateDrop(Run.instance.treasureRng);
			}*/
			//else
			/*{
				List<PickupIndex> list;
				switch (itemTier)
				{
					case ItemTier.Tier1:
						list = Run.instance.availableTier1DropList;
						break;
					case ItemTier.Tier2:
						list = Run.instance.availableTier2DropList;
						break;
					case ItemTier.Tier3:
						list = Run.instance.availableTier3DropList;
						break;
					case ItemTier.Lunar:
						list = Run.instance.availableLunarDropList;
						break;
					case ItemTier.Boss:
						list = Run.instance.availableBossDropList;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				return list;
			}*/ // just fuck me up
		//}
	}
}
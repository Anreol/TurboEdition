using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace TurboEdition.Misc
{
    internal class SceneChanges
    {
        [SystemInitializer]
        public static void Init()
        {
            //SceneCatalog.onMostRecentSceneDefChanged += onMostRecentSceneDefChanged;
        }

        const int sgEnviron = 10;
        const int sgSkybox = 0;
        private static void onMostRecentSceneDefChanged(SceneDef obj)
        {

            if (obj == SceneCatalog.GetSceneDefFromSceneName("shipgraveyard"))
            {
                GameObject[] roots = SceneInfo.instance.gameObject.scene.GetRootGameObjects();
                List<GameObject> disabledGO = new List<GameObject>();
                List<GameObject> GOtoDisable = new List<GameObject>();
                for (int i = 1; i < 4; i++)
                {
                    disabledGO.Add(roots[sgEnviron].transform.GetChild(1).GetChild(i).gameObject); //Add cave walls
                }
                disabledGO.Add(roots[sgEnviron].transform.GetChild(1).GetChild(12).gameObject); //Add dirt shelf secret
                disabledGO.Add(roots[sgSkybox].transform.GetChild(8).gameObject); //Add spikes
                disabledGO.Add(roots[sgSkybox].transform.GetChild(11).gameObject);
                disabledGO.Add(roots[sgSkybox].transform.GetChild(12).gameObject);
                disabledGO.Add(roots[sgSkybox].transform.GetChild(15).gameObject);
                disabledGO.Add(roots[sgSkybox].transform.GetChild(17).gameObject);
                disabledGO.Add(roots[sgSkybox].transform.GetChild(19).gameObject);
                disabledGO.Add(roots[sgSkybox].transform.GetChild(20).gameObject);
                disabledGO.Add(roots[sgSkybox].transform.GetChild(21).gameObject);
                disabledGO.Add(roots[sgSkybox].transform.GetChild(22).gameObject);

                GOtoDisable.Add(roots[sgEnviron].transform.GetChild(0).GetChild(55).gameObject); //Remove Spikes blocking shelf cave
                GOtoDisable.Add(roots[sgEnviron].transform.GetChild(0).GetChild(57).gameObject);
                GOtoDisable.Add(roots[sgEnviron].transform.GetChild(3).GetChild(37).gameObject); //Ship platform door
                GOtoDisable.Add(roots[sgEnviron].transform.GetChild(3).GetChild(38).gameObject);

                Transform enabled = roots[sgEnviron].transform.GetChild(6).transform; //Cave shelf is enabled
                Transform disabled = roots[sgEnviron].transform.GetChild(7); //Cave shelf disabled

                foreach (GameObject item in disabledGO)
                {
                    item.SetActive(true);
                    item.transform.SetParent(enabled);
                }
                foreach (var item in GOtoDisable)
                {
                    item.transform.SetParent(disabled);
                }
            }
        }
    }
}
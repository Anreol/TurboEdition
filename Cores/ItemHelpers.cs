using RoR2;
using System.Collections.Generic;
using UnityEngine;

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

        //What if we make the same shit we did with the hitlag manager but like, with anything
        //except we use an actual queue list since c# has that, thank derslayr and Microsoft
        //could i use this on Hitlag manager to replace that god damned sorted list? still has that issue where multiple hits in the same frame makes hits disappear because they have the same timestamp tKey
        //how do u coroutine

        //ok so we do the same shit we do on bases and add the <t> thing
        //If I keep being stupid read https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/generic-type-parameters again
        //Also https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/
        public abstract class DelayerManager<T> : MonoBehaviour
        {
            private readonly Queue<T> cuteList = new Queue<T>();
            public abstract float releaseAt { get; }
            public abstract int entriesToRelease { get; }
            public abstract float interval { get; }

            private float stopwatch;

            //Getters and setters
            //This is stupid this wont work on generics
            /*
            public float Interval { get => interval; set => interval = value; }
            public float ReleaseAt { get => releaseAt; set => releaseAt = value; }
            public int EntriesToRelease { get => entriesToRelease; set => entriesToRelease = value; }*/

            //protected since it doesnt let me make it private
            protected virtual void Awake()
            {
                stopwatch = interval;
                //releaseAt = 0f;
                //entriesToRelease = 1;
            }

            public void AddEntry(T newEntry)
            {
#if DEBUG
                TurboEdition._logger.LogDebug("ItemHelpers's DelayerManager: added new entry " + newEntry + " to " + cuteList);
#endif
                cuteList.Enqueue(newEntry);
            }

            public void ClearList()
            {
#if DEBUG
                TurboEdition._logger.LogDebug("ItemHelpers's DelayerManager: cleared " + cuteList);
#endif
                cuteList.Clear();
            }

            public virtual void ProcessQueue(T entry)
            {
                /* Override this so FixedUpdate can process whatever entries you are adding to the queue */
            }

            protected virtual void FixedUpdate()
            {
                stopwatch -= Time.fixedDeltaTime;
                if (stopwatch <= 0 && cuteList.Count > 0) //List should be frozen if theres no items, it will resume the countdown if theres any.
                {
#if DEBUG
                    TurboEdition._logger.LogDebug("ItemHelpers's DelayerManager: Fixed update, list has a count of " + cuteList.Count + " and we are about to release " + entriesToRelease);
#endif
                    stopwatch = interval;
                    var toRelease = Mathf.Min(entriesToRelease, cuteList.Count); //Get minimum, adding in case the list already has less count than entriesToRelease so we dont go off the list
                    for (int i = 0; i < toRelease; i++)
                    {
                        T entry = cuteList.Dequeue();
                        ProcessQueue(entry);
#if DEBUG
                        TurboEdition._logger.LogDebug("ItemHelpers's DelayerManager: Fixed update, dequeued " + entry + " and processing it.");
#endif
                    }
                }
                //Check if the lists obliterates itself once its out of elements or i have to do it myself
                //Yaaay dynamic memory
            }
        }
    }
}
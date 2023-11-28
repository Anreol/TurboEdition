using RoR2;
using System.Collections.Generic;
using System.Linq;
using TurboEdition.Utils;
using UnityEngine;
using UnityEngine.Networking;
using static Rewired.InputMapper;
using static RoR2.NetworkSession;

namespace TurboEdition.Items
{
    public static class BloodEconomyManager
    {
        public static GameObject LoadedZonePrefab
        {
            get
            {
                if (_cachedZonePrefab == null)
                {
                    _cachedZonePrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("Net_BloodEconomy_Zone");
                    return _cachedZonePrefab;
                }
                return _cachedZonePrefab;
            }
        }

        private static Dictionary<CharacterBody, UnsafeZoneDamageManager> _bodyAndUnsafeZoneManagerPairs = new Dictionary<CharacterBody, UnsafeZoneDamageManager>();
        private static Dictionary<IZone, PurchaseInteraction> _zonesAndPurchaseInteractorsPairs = new Dictionary<IZone, PurchaseInteraction>();
        private static List<PurchaseInteraction> _purchaseInteractionsToIgnore = new List<PurchaseInteraction>();

        private static GameObject discountEffectPrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("FX_BloodEconomy_Discount");
        private static GameObject _cachedZonePrefab;

        [SystemInitializer(typeof(PickupCatalog))]
        public static void Initialize()
        {
            On.RoR2.Interactor.PerformInteraction += OnInteractionPerformed;
            Stage.onServerStageBegin += FlushData;
            RoR2Application.onFixedUpdate += ProcessManagers;
        }

        private static void ProcessManagers()
        {
            if (NetworkServer.active && _bodyAndUnsafeZoneManagerPairs != null)
            {
                foreach (var item in _bodyAndUnsafeZoneManagerPairs)
                {
                    item.Value.ServerFixedUpdate(Time.fixedDeltaTime);
                }
            }
        }

        /// <summary>
        /// Flush data just in case, because I feel like this will break at some point.
        /// </summary>
        /// <param name="obj"></param>
        private static void FlushData(Stage obj)
        {
            _bodyAndUnsafeZoneManagerPairs.Clear();
            _zonesAndPurchaseInteractorsPairs.Clear();
            _purchaseInteractionsToIgnore.Clear();
        }

        private static void OnInteractionPerformed(On.RoR2.Interactor.orig_PerformInteraction orig, Interactor interactor, GameObject interactableObject)
        {
            orig(interactor, interactableObject);
            if (!NetworkServer.active || !interactableObject)
            {
                return;
            }
            //Get the first purchaseInteraction
            RoR2.PurchaseInteraction[] purchaseInteractions = interactableObject.GetComponents<RoR2.PurchaseInteraction>();

            //Make sure its money.
            //All of this shit runs on the first purchase interactor, could make a "get best fit" purchase interactor, but that might be troublesome...
            //Consider it if it gives problems.

            if (purchaseInteractions.Length > 0 && purchaseInteractions[0] && purchaseInteractions[0].GetInteractability(interactor) == Interactability.ConditionsNotMet && CostTypeCatalog.GetCostTypeDef(purchaseInteractions[0].costType) == CostTypeCatalog.GetCostTypeDef(CostTypeIndex.Money))
            {
                CharacterBody characterBody = interactor.GetComponent<CharacterBody>();
                int itemStacks = characterBody.inventory.GetItemCount(TEContent.Items.BloodEconomy);
                if (characterBody && characterBody.inventory && itemStacks > 0)
                {
                    //If the body has already added a single zone, its in here.
                    if (_bodyAndUnsafeZoneManagerPairs.ContainsKey(characterBody))
                    {
                        //For multishops and other shit.
                        bool containsAny = purchaseInteractions.Any(pi => _zonesAndPurchaseInteractorsPairs.ContainsValue(pi));
                        if (_bodyAndUnsafeZoneManagerPairs[characterBody].UnsafeZones.Count < 3 && !containsAny)
                        {
                            //Create the GameObject
                            GameObject newZone = CreateZoneForPurchaseInteractionAndZoneManager(purchaseInteractions[0], _bodyAndUnsafeZoneManagerPairs[characterBody]);
                            IZone zoneComponent = newZone.GetComponent<IZone>();

                            //Add the things
                            _zonesAndPurchaseInteractorsPairs.Add(zoneComponent, purchaseInteractions[0]);
                            _bodyAndUnsafeZoneManagerPairs[characterBody].AddUnsafeZone(zoneComponent);

                            //Update damage, because that's how things are
                            _bodyAndUnsafeZoneManagerPairs[characterBody].flatDamageEachSecond = (characterBody.damage * itemStacks) * 0.25f;

                            //Get other shops
                            CheckForOtherTerminals(zoneComponent, purchaseInteractions[0]);

                            //Spawn the thing
                            NetworkServer.Spawn(newZone);
                        }

                        //Cannot do anything, we have already deployed the thing in three interactables.
                        return;
                    }

                    //Else
                    //Declare new zone manager
                    UnsafeZoneDamageManager unsafeZoneDamageManager = new UnsafeZoneDamageManager()
                    {
                        attacker = characterBody.gameObject,
                        flatDamageEachSecond = (characterBody.damage * itemStacks) * 0.5f,
                        damageType = DamageType.BleedOnHit,
                        damageColorIndex = DamageColorIndex.Bleed,
                        tickPeriodSeconds = 0.5f,
                        teamMask = characterBody.teamComponent != null ? TeamMask.GetEnemyTeams(characterBody.teamComponent.teamIndex) : TeamMask.all,
                    };

                    //We need to know when damage happens
                    unsafeZoneDamageManager.OnDamageDealtAnywhere += OnDamageDealt;

                    //Create the gameobject
                    GameObject newZoneForNewManager = CreateZoneForPurchaseInteractionAndZoneManager(purchaseInteractions[0], unsafeZoneDamageManager);
                    IZone zoneComponentForNewManager = newZoneForNewManager.GetComponent<IZone>();

                    //Add the things
                    _zonesAndPurchaseInteractorsPairs.Add(zoneComponentForNewManager, purchaseInteractions[0]);
                    _bodyAndUnsafeZoneManagerPairs.Add(characterBody, unsafeZoneDamageManager);
                    unsafeZoneDamageManager.AddUnsafeZone(zoneComponentForNewManager);

                    //Get other shops
                    CheckForOtherTerminals(zoneComponentForNewManager, purchaseInteractions[0]);

                    //Finally spawn the thing
                    NetworkServer.Spawn(newZoneForNewManager);
                }
            }
        }

        private static void OnDamageDealt(DamageInfo obj, HealthComponent healthComponent)
        {
            if (obj == null || obj.rejected)
            {
                return;
            }

            DotController.InflictDot(healthComponent.gameObject, obj.attacker, DotController.DotIndex.Bleed, 3f, 0.5f);

            foreach (var zip in _zonesAndPurchaseInteractorsPairs)
            {
                if (zip.Key.IsInBounds(obj.position)) //XD XD XD XD XD XD XD XD XD XD XD XD XD XD XD XD XD XD XD XD XD XD XD XD
                {
                    //Renderer renderer = zip.Value.gameObject.GetComponentInChildren<Renderer>();

                    int futureCost = zip.Value.cost -= Mathf.Max(Mathf.FloorToInt(obj.damage * 0.10f), 1);
                    if (futureCost <= 0) //Code below based on captain's hacking beacon
                    {
                        EffectManager.SpawnEffect(discountEffectPrefab, new EffectData()
                        {
                            origin = zip.Value.transform.position,
                            scale = /*renderer ? renderer.bounds.size.sqrMagnitude * 3 :*/ 3,
                        }, true);

                        zip.Value.cost = 0;
                        if (zip.Value.available)
                        {
                            //Attempts interaction. If succeeds, gets rid of the zone through the onPurchase listener.
                            obj.attacker.GetComponent<Interactor>().AttemptInteraction(zip.Value.gameObject);
                            return;
                        }
                        NetworkServer.Destroy(((BaseZoneBehavior)zip.Key).gameObject);
                        _zonesAndPurchaseInteractorsPairs.Remove(zip.Key);
                    }

                    EffectManager.SpawnEffect(discountEffectPrefab, new EffectData()
                    {
                        origin = zip.Value.transform.position,
                        scale = /*renderer ? renderer.bounds.size.sqrMagnitude :*/ 1,
                    }, true);

                    zip.Value.cost = futureCost;
                }
            }
        }

        /// <summary>
        /// Code lifted from <see cref="RoR2.Items.MultiShopCardUtils"/>
        /// </summary>
        /// <param name="zoneBeingCreated"></param>
        /// <param name="purchaseInteraction"></param>
        private static void CheckForOtherTerminals(IZone zoneBeingCreated, PurchaseInteraction purchaseInteraction)
        {
            ShopTerminalBehavior shopTerminalBehavior = (purchaseInteraction != null) ? purchaseInteraction.GetComponent<ShopTerminalBehavior>() : null;
            if (shopTerminalBehavior && shopTerminalBehavior.serverMultiShopController)
            {
                foreach (var terminalGameObject in shopTerminalBehavior.serverMultiShopController._terminalGameObjects)
                {
                    PurchaseInteraction terminalPurchaseInteraction = terminalGameObject.GetComponent<PurchaseInteraction>();
                    if (terminalPurchaseInteraction)
                    {
                        _purchaseInteractionsToIgnore.Add(terminalPurchaseInteraction);
                    }
                }
            }
        }

        private static GameObject CreateZoneForPurchaseInteractionAndZoneManager(PurchaseInteraction purchaseInteraction, UnsafeZoneDamageManager unsafeZoneDamageManager)
        {
            //Npt sure if I should parent it to the purchase interaction...
            GameObject newZone = GameObject.Instantiate(LoadedZonePrefab, purchaseInteraction.gameObject.transform);
            IZone zone = newZone.GetComponent<IZone>();
            purchaseInteraction.onPurchase.AddListener((interactor) =>
            {
                //Just in case.
                if (NetworkServer.active && unsafeZoneDamageManager != null)
                {
                    if (zone != null && unsafeZoneDamageManager.UnsafeZones.Contains(zone))
                    {
                        unsafeZoneDamageManager.RemoveUnsafeZone(zone);
                        NetworkServer.Destroy(((BaseZoneBehavior)zone).gameObject);
                        _zonesAndPurchaseInteractorsPairs.Remove(zone);
                    }
                }
            });
            return newZone;
        }
    }
}
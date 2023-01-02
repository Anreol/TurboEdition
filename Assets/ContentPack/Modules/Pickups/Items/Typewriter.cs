using HG;
using RoR2;
using RoR2.Orbs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Items
{
    public class Typewriter : Item
    {
        public override ItemDef itemDef { get; set; } = Assets.mainAssetBundle.LoadAsset<ItemDef>("Typewriter");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<TypewriterBehavior>(stack);
        }

        internal class TypewriterBehavior : CharacterBody.ItemBehavior
        {
            private char[] arr = new char[40];
            private float sprintTimer;
            private static readonly float sprintingFireRate = 0.08571429f;

            private GameObject textPrefab = Assets.mainAssetBundle.LoadAsset<GameObject>("vfxTextEffect");
            private RoR2.UI.LanguageTextMeshController component;

            private void Start()
            {
                EquipmentSlot.onServerEquipmentActivated += onServerEquipmentActivated;
                base.body.onSkillActivatedServer += onSkillActivatedServer;

                if (component == null)
                {
                    component = textPrefab.GetComponentInChildren<RoR2.UI.LanguageTextMeshController>();
                }
            }

            private void OnDisable()
            {
                base.body.onSkillActivatedServer -= onSkillActivatedServer;
                EquipmentSlot.onServerEquipmentActivated -= onServerEquipmentActivated;
            }

            private void onSkillActivatedServer(GenericSkill obj)
            {
                if (!Util.HasEffectiveAuthority(body.networkIdentity))
                    return;
                ForceAttempt();
                for (int i = 0; i < stack; i++)
                {
                    AddLetterAndCheckEffects();
                }
            }

            private void onServerEquipmentActivated(EquipmentSlot arg1, EquipmentIndex arg2)
            {
                if (!arg1.characterBody == body || !Util.HasEffectiveAuthority(body.networkIdentity)) return;
                ForceAttempt();
                for (int i = 0; i < stack; i++)
                {
                    AddLetterAndCheckEffects();
                }
            }

            private void FixedUpdate()
            {
                SprintBehavior();
            }

            private void SprintBehavior()
            {
                if (!Util.HasEffectiveAuthority(body.networkIdentity))
                    return;
                if (body.isSprinting)
                {
                    this.sprintTimer -= Time.fixedDeltaTime;
                    if (this.sprintTimer <= 0f)
                    {
                        this.sprintTimer += 1f / (sprintingFireRate * this.body.moveSpeed);
                        ForceAttempt();
                        for (int i = 0; i < stack; i++)
                        {
                            AddLetterAndCheckEffects();
                        }
                    }
                }
            }

            private void AddLetterAndCheckEffects()
            {
                char pop = arr[0];
                for (int i = 0; i < arr.Length - 1; i++)
                {
                    arr[i] = arr[i + 1];
                }

                arr[arr.Length - 1] = (char)UnityEngine.Random.Range('a', 'z');
                //TELog.LogD("Adding a letter. Current array is " + arr.ToString());
                StartCoroutine(EffectSpawnBody());
                StartCoroutine(EffectSpawnItem());
                PopWord(pop.ToString());
            }

            private void PopWord(string word)
            {
                if (component != null && word != null)
                {
                    component.token = word;
                    if (word.Length == 1 && component.token == component.previousToken) //Don't dupe letters.
                    {
                        return;
                    }
                    EffectData effectData = new EffectData()
                    {
                        //networkSoundEventIndex = get a key press sound
                        origin = body.transform.position,
                        rotation = Util.QuaternionSafeLookRotation((body.characterDirection.moveVector != Vector3.zero) ? body.characterDirection.moveVector : UnityEngine.Random.onUnitSphere)
                    };
                    effectData.SetHurtBoxReference(body.gameObject);
                    EffectManager.SpawnEffect(textPrefab, effectData, true);
                }
            }

            private void ClearArray(string wordTriggered)
            {
                int len = arr.Length;
                HG.ArrayUtils.Clear(arr, ref len);
                PopWord(wordTriggered);
            }

            private void ForceAttempt()
            {
                if (Util.CheckRoll(0.001f * stack, body.master))
                {
                    int no = UnityEngine.Random.Range(0, Utils.CleanTokens.bodyIndices.Length - 1);
                    BodyIndex bi = Utils.CleanTokens.bodyIndices[no];
                    new MasterSummon
                    {
                        masterPrefab = MasterCatalog.GetMasterPrefab(MasterCatalog.FindAiMasterIndexForBody(bi)),
                        position = body.corePosition,
                        rotation = body.aimOriginTransform.rotation,
                        ignoreTeamMemberLimit = false,
                        summonerBodyObject = body.gameObject,
                        useAmbientLevel = true
                    }.Perform();
                    PopWord(BodyCatalog.GetBodyPrefabBodyComponent(bi).baseNameToken);
                }
                if (Util.CheckRoll(0.001f * stack, body.master))
                {
                    int no2 = UnityEngine.Random.Range(0, Utils.CleanTokens.itemIndices.Length - 1);
                    ItemIndex found = Utils.CleanTokens.itemIndices[no2];
                    ItemDef foundDef = ItemCatalog.GetItemDef(found);
                    if ((!body.isPlayerControlled && (foundDef.ContainsTag(ItemTag.AIBlacklist) || !foundDef.inDroppableTier || foundDef.ContainsTag(ItemTag.CannotCopy))) || (BodyCatalog.FindBodyIndex("BrotherBody") == body.bodyIndex && foundDef.ContainsTag(ItemTag.BrotherBlacklist)))
                    {
                        return;
                    }
                    List<RoR2.Orbs.ItemTransferOrb> inFlightOrbs = new List<RoR2.Orbs.ItemTransferOrb>();
                    for (int z = 0; z < 3; z++)
                    {
                        ItemTransferOrb itemOrb = ItemTransferOrb.DispatchItemTransferOrb(body.transform.position, body.inventory, found, 1, delegate (ItemTransferOrb orb)
                        {
                            body.inventory.GiveItem(orb.itemIndex, orb.stack);
                            inFlightOrbs.Remove(orb);
                        }, default(Either<NetworkIdentity, HurtBox>));
                        inFlightOrbs.Add(itemOrb);
                    }
                    PopWord(foundDef.nameToken);
                }
            }

            private IEnumerator EffectSpawnBody()
            {
                //TELog.LogD("Coroutine SpawnBody");
                for (int i = 0; i < Utils.CleanTokens.cleanBodyNames.Length; i++)
                {
                    if (arr.ToString().Contains(Utils.CleanTokens.cleanBodyNames[i]))
                    {
                        new MasterSummon
                        {
                            masterPrefab = MasterCatalog.GetMasterPrefab(MasterCatalog.FindAiMasterIndexForBody(Utils.CleanTokens.bodyIndices[i])),
                            position = body.corePosition,
                            rotation = body.aimOriginTransform.rotation,
                            ignoreTeamMemberLimit = true,
                            summonerBodyObject = body.gameObject,
                            useAmbientLevel = true
                        }.Perform();
                        ClearArray(BodyCatalog.GetBodyPrefabBodyComponent(Utils.CleanTokens.bodyIndices[i]).baseNameToken);
                    }
                }
                yield return new WaitForSeconds(.1f);
            }

            private IEnumerator EffectSpawnItem()
            {
                //TELog.LogD("Coroutine SpawnItem");
                for (int i = 0; i < Utils.CleanTokens.cleanItemNames.Length; i++)
                {
                    if (arr.ToString().Contains(Utils.CleanTokens.cleanItemNames[i]))
                    {
                        ItemIndex found = Utils.CleanTokens.itemIndices[i];
                        ItemDef foundDef = ItemCatalog.GetItemDef(found);
                        if ((!body.isPlayerControlled && (foundDef.ContainsTag(ItemTag.AIBlacklist) || foundDef.ContainsTag(ItemTag.CannotCopy))) || (BodyCatalog.FindBodyIndex("BrotherBody") == body.bodyIndex && foundDef.ContainsTag(ItemTag.BrotherBlacklist)))
                        {
                            continue;
                        }
                        List<RoR2.Orbs.ItemTransferOrb> inFlightOrbs = new List<RoR2.Orbs.ItemTransferOrb>();
                        for (int z = 0; z < 3; z++)
                        {
                            ItemTransferOrb itemOrb = ItemTransferOrb.DispatchItemTransferOrb(body.transform.position, body.inventory, found, 1, delegate (ItemTransferOrb orb)
                            {
                                body.inventory.GiveItem(orb.itemIndex, orb.stack);
                                inFlightOrbs.Remove(orb);
                            }, default(Either<NetworkIdentity, HurtBox>));
                            inFlightOrbs.Add(itemOrb);
                        }
                        ClearArray(foundDef.nameToken);
                    };
                }
                yield return new WaitForSeconds(.1f);
            }
        }
    }
}
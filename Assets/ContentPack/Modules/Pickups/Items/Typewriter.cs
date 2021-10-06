using HG;
using RoR2;
using RoR2.Orbs;
using System;
using System.Collections.Generic;
using System.Linq;
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
            private char[] arr = new char[25];
            private float sprintTimer;
            private static readonly float fireRate = 0.08571429f;

            private void Start()
            {
                EquipmentSlot.onServerEquipmentActivated += EquipmentSlot_onServerEquipmentActivated;
                base.body.onSkillActivatedServer += Body_onSkillActivatedServer;
            }

            private void OnDisable()
            {
                base.body.onSkillActivatedServer -= Body_onSkillActivatedServer;
                EquipmentSlot.onServerEquipmentActivated -= EquipmentSlot_onServerEquipmentActivated;
            }

            private void Body_onSkillActivatedServer(GenericSkill obj)
            {
                if (!Util.HasEffectiveAuthority(body.networkIdentity))
                {
                    TELog.LogW("Function 'System.Void TurboEdition.Items.Typewriter::Body_onSkillActivatedServer() called without authority.'");
                    return;
                }
                AddLetterAndCheckEffects();
            }

            private void EquipmentSlot_onServerEquipmentActivated(EquipmentSlot arg1, EquipmentIndex arg2)
            {
                if (!arg1.characterBody == body) return;
                AddLetterAndCheckEffects();
            }

            private void FixedUpdate()
            {
                SprintBehavior();
            }

            private void SprintBehavior()
            {
                if (body.isSprinting)
                {
                    this.sprintTimer -= Time.fixedDeltaTime;
                    if (this.sprintTimer <= 0f)
                    {
                        this.sprintTimer += 1f / (fireRate * this.body.moveSpeed);
                        AddLetterAndCheckEffects();
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
                if (EffectSpawnBody()) return;
                if (EffectSpawnItem()) return;
                PopWord(pop.ToString());
            }
            private void PopWord(String word)
            {

            }
            private void ClearArray(String wordTriggered)
            {
                int len = arr.Length;
                HGArrayUtilities.Clear(arr, ref len);
                PopWord(wordTriggered);
            }
            private bool EffectSpawnBody()
            {
                foreach (CharacterBody item in RoR2.BodyCatalog.allBodyPrefabBodyBodyComponents)
                {
                    String bodyString = String.Concat(Language.GetString(item.baseNameToken).Where(c => !Char.IsWhiteSpace(c)));
                    if (arr.ToString().Contains(bodyString))
                    {
                        new MasterSummon
                        {
                            masterPrefab = MasterCatalog.GetMasterPrefab(MasterCatalog.FindAiMasterIndexForBody(BodyCatalog.FindBodyIndex(item))),
                            position = body.corePosition,
                            rotation = body.aimOriginTransform.rotation,
                            ignoreTeamMemberLimit = true,
                            summonerBodyObject = body.gameObject,
                            useAmbientLevel = true
                        }.Perform();
                        ClearArray(bodyString);
                        return true;
                    }
                }
                return false;
            }
            private bool EffectSpawnItem()
            {
                foreach (ItemIndex item in RoR2.ItemCatalog.allItems)
                {
                    ItemDef itemDef = ItemCatalog.GetItemDef(item);
                    String itemString = String.Concat(Language.GetString(itemDef.nameToken).Where(c => !Char.IsWhiteSpace(c)));
                    if (arr.ToString().Contains(itemString))
                    {
                        if ((!body.isPlayerControlled && (itemDef.ContainsTag(ItemTag.AIBlacklist) || itemDef.ContainsTag(ItemTag.CannotCopy))) || (BodyCatalog.FindBodyIndex("BrotherBody") == body.bodyIndex && itemDef.ContainsTag(ItemTag.BrotherBlacklist)))
                        {
                            continue;
                        }
                        List<RoR2.Orbs.ItemTransferOrb> inFlightOrbs = new List<RoR2.Orbs.ItemTransferOrb>();
                        for (int i = 0; i < 3; i++)
                        {
                            ItemTransferOrb itemOrb = ItemTransferOrb.DispatchItemTransferOrb(body.transform.position, body.inventory, itemDef.itemIndex, 1, delegate (ItemTransferOrb orb)
                            {
                                body.inventory.GiveItem(orb.itemIndex, orb.stack);
                                inFlightOrbs.Remove(orb);
                            }, default(Either<NetworkIdentity, HurtBox>));
                            inFlightOrbs.Add(itemOrb);
                        }
                        ClearArray(itemString);
                        return true;
                    };
                }
                return false;
            }
        }
    }
}
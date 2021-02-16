using BepInEx.Configuration;
using MonoMod.Cil;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using static TurboEdition.Utils.ItemHelpers;

//TODO Get particle effect / model / whatever
//Think of Hades' Than's attack effect (shadowy "curtain" that comes from behind the enemy, then get a giant scythe slice the enemy)
//Also see Shadow Sneak from Gen 6 Pokemon, if we can get the shadow effect to be the same (ie travel to the enemy before striking) that would be sick
//Once thats done consider making the scythe summonable everytime and check for a target, if theres no target before a timeout the scythe disappears, but the equipment still gets consumed.
namespace TurboEdition.Equipment
{
    public class RandomKill : EquipmentBase<RandomKill>
    {
        public override string EquipmentName => "Cursed Scythe";

        public override string EquipmentLangTokenName => "RANDOMKILL";

        public override string EquipmentPickupDesc => "Summon a scythe that targets a random enemy and ignores armor. Deals more damage if there's more enemies.";

        public override string EquipmentFullDescription => "";

        public override string EquipmentLore => "";

        public override string EquipmentModelPath => "@TurboEdition:Assets/Models/Prefabs/Default.prefab";

        public override string EquipmentIconPath => "@TurboEdition:Assets/Textures/Icons/Equipment/Placeholder_Scythe.png";
        public override float Cooldown => equipmentRecharge;

        public float equipmentRecharge;
        public bool consumeEquipment;
        public float maxRange;
        public int attackCount;
        public float enemyMultiplier;
        public float baseMultiplier;

        protected override void CreateConfig(ConfigFile config)
        {
            equipmentRecharge = config.Bind<float>("Equipment : " + EquipmentName, "Recharge time", 75f, "Amount in seconds for this equipment to be available again.").Value;
            consumeEquipment = config.Bind<bool>("Equipment : " + EquipmentName, "Equipment use if failed", true, "Whenever to consume the equipment and put it on cooldown even if it failed to perform its task.").Value;
            maxRange = config.Bind<float>("Equipment : " + EquipmentName, "Maximum range", 80f, "Maximum range the item will search for a target when doing the raycast.").Value;
            attackCount = config.Bind<int>("Equipment : " + EquipmentName, "Number of attacks", 1, "Number of attacks per use that this equipment does.").Value;
            enemyMultiplier = config.Bind<float>("Equipment : " + EquipmentName, "Extra damage per enemy", 5f, "The total enemy team count will get multiplied by this value, i.e if theres one enemy, it will deal 5 damage more.").Value;
            baseMultiplier = config.Bind<float>("Equipment : " + EquipmentName, "Owners extra damage", 2.25f, "The owners current base damage will be multiplied by this value.").Value;
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        protected override void Initialization()
        {

        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            FindTarget(slot.characterBody);
            if (FindTarget(slot.characterBody))
            {
#if DEBUG
                TurboEdition._logger.LogWarning(EquipmentName + " Successfully attacked one or more enemies in the last equipment use.");
#endif
                return true;
            }
            if (consumeEquipment) { return true; }
            return false;
        }

        private bool FindTarget(CharacterBody cb)
        {
            TeamIndex ownerTeam = cb.teamComponent.teamIndex;
#if DEBUG
            TurboEdition._logger.LogWarning(EquipmentName + " Getting the team of the owner... " + ownerTeam);
#endif
            for (TeamIndex teamCounter = TeamIndex.Neutral; teamCounter < TeamIndex.Count; teamCounter++)
            {
#if DEBUG
                TurboEdition._logger.LogWarning(EquipmentName + " Trying to find the enemy team to owners... " + teamCounter);
#endif
                if (TeamManager.IsTeamEnemy(ownerTeam, teamCounter))
                {
                    var enemyMember = TeamComponent.GetTeamMembers(teamCounter);
#if DEBUG
                    TurboEdition._logger.LogWarning(EquipmentName + " Found the enemy team to owners: " + teamCounter);
#endif
                    for (int aC = 0; aC < attackCount; aC++)
                    {
#if DEBUG
                        TurboEdition._logger.LogWarning(EquipmentName + " Trying to attack somebody, " + aC + " out of " + attackCount);
#endif
                        for (int i = 0; i < enemyMember.Count; i++)
                        {
                            //Get the health component to see if its alive / its an enemy / thing we can MURDER
                            HealthComponent healthComponent = enemyMember[i].GetComponent<HealthComponent>();
                            GameObject enemyObject = enemyMember[i].gameObject;
                            if (healthComponent)
                            {
                                //Something something we are getting the cb direction, not the actual camera, and with aimDirection we are getting the aiming, not the actual camera
                                //It might break if something disconnects the aim movement from the camera
                                //Vector3 cbForward = cb.transform.TransformDirection(Vector3.forward);
                                Vector3 cbForward = cb.inputBank.aimDirection;
                                Vector3 enemyPos = enemyObject.transform.position - cb.transform.position;

                                if (Vector3.Dot(cbForward, enemyPos) > 0.2f) // Dot returns 1 if they point in exactly the same direction, -1 if they point in completely opposite directions and zero if the vectors are perpendicular.
                                {
#if DEBUG
                                    TurboEdition._logger.LogWarning(EquipmentName + " Found a suitable enemy inside the fov/range/whatever: " + enemyObject + " at " + enemyPos);
#endif
                                    if (HasLoS(cb.gameObject, enemyObject) && NetworkServer.active) //i dunno if packing both checks into one is a good idea
                                    {
                                        DamageInfo damageInfo = new DamageInfo
                                        {
                                            damage = CalcDamage(cb, teamCounter),
                                            crit = false,
                                            damageType = DamageType.BypassArmor,
                                            procCoefficient = 0, //Because fuck autoplay
                                            damageColorIndex = DamageColorIndex.Default,
                                            rejected = false, //I dunno if I can force bypassing bears this way
                                            attacker = cb.gameObject,
                                        };
                                        healthComponent.TakeDamage(damageInfo);
#if DEBUG
                                        TurboEdition._logger.LogWarning(EquipmentName + " Damaged " + healthComponent + " for " + damageInfo);
#endif
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        private bool HasLoS(GameObject origin, GameObject target)
        {
#if DEBUG
            TurboEdition._logger.LogWarning(EquipmentName + " Checking LOS between: " + origin + " and " + target);
#endif
            Ray ray = new Ray(origin.transform.position, target.transform.position - origin.transform.position);
            return !Physics.Raycast(ray, out RaycastHit raycastHit, this.maxRange, LayerIndex.defaultLayer.mask | LayerIndex.world.mask, QueryTriggerInteraction.Ignore) || raycastHit.collider.gameObject == target;
        }

        private float CalcDamage(CharacterBody cb, TeamIndex enemyTeam)
        {
            //I could get the calculations to get the enemy team to cb here too to save up a parameter but i wont lol
            float baseDamage = cb.damage;
#if DEBUG
            TurboEdition._logger.LogWarning(EquipmentName + " Calculating damage. Base damage: " + baseDamage + " and theres " + TeamComponent.GetTeamMembers(enemyTeam).Count + " enemies, dealing " + enemyMultiplier * TeamComponent.GetTeamMembers(enemyTeam).Count + " more damage.");
#endif
            return ((float)((baseDamage * baseMultiplier) + (enemyMultiplier * TeamComponent.GetTeamMembers(enemyTeam).Count)));
        }
    }
}
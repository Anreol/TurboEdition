using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

//TODO Get particle effect / model / whatever
//Think of Hades' Than's attack effect (shadowy "curtain" that comes from behind the enemy, then get a giant scythe slice the enemy)
//Also see Shadow Sneak from Gen 6 Pokemon, if we can get the shadow effect to be the same (ie travel to the enemy before striking) that would be sick
//Once thats done consider making the scythe summonable everytime and check for a target, if theres no target before a timeout the scythe disappears, but the equipment still gets consumed.
//Important!!: the item is not truly random since it does a simple loop through the bodies of each team, meaning that it will ALWAYS target the earliest available enemy in the list. It will keep damaging that enemy until there's no more LOS or it dies.
//The LOS check is also made within the character body, if theres no LOS between it and the enemy the item will fail, just because (YOU) see it doesn't mean you will hit it, so far it's kinda frustrating but you get used to it.
//04/03/21: Been thinking about the last few days about when finding a target, instead of damaging it make an area that might / might not follow the target at a slower pace than it, makes it fair for players and its not a press q to win against fast enemies / literally everything. also works better against groups of enemies.
//if this happens, thinking about removing the "only on screen" limitation since you would be able to see where it takes place
//also make it different than meteor. meteor is funny. this is not supposed to be funny. but useful.
namespace TurboEdition.Equipment
{
    public class RandomKill : EquipmentBase<RandomKill>
    {
        public override string EquipmentName => "Cursed Scythe";

        public override string EquipmentLangTokenName => "RANDOMKILL";

        public override string EquipmentPickupDesc => $"Summon a scythe that targets a random enemy and <style=cIsUtility>ignores armor</style> for <style=cIsDamage>{baseMultiplier * 100}% damage</style>. Deals <style=cIsDamage>{enemyMultiplier * 100}% or more damage</style> if <style=cIsUtility>there's more enemies</style>.";

        public override string EquipmentFullDescription => "";

        public override string EquipmentLore => "";

        public override GameObject EquipmentModel => TurboEdition.MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/Default.prefab");
        public override Sprite EquipmentIcon => TurboEdition.MainAssets.LoadAsset<Sprite>("Assets/Textures/Icons/Equipment/Placeholder_Scythe.png");
        public override float Cooldown => equipmentRecharge;

        public float equipmentRecharge;
        public bool consumeEquipment;
        public float maxRange;
        public int attackCount;
        public float enemyMultiplier;
        public float baseMultiplier;

        protected override void CreateConfig(ConfigFile config)
        {
            equipmentRecharge = config.Bind<float>("Equipment : " + EquipmentName, "Recharge time", 140f, "Amount in seconds for this equipment to be available again. For comparison, the highest cooldown is 140s (Preon). Royal Capacitor stands at 20s.").Value;
            consumeEquipment = config.Bind<bool>("Equipment : " + EquipmentName, "Equipment use if failed", true, "Whenever to consume the equipment and put it on cooldown even if it failed to perform its task.").Value;
            maxRange = config.Bind<float>("Equipment : " + EquipmentName, "Maximum range", 80f, "Maximum range the item will search for a target when doing the raycast.").Value;
            attackCount = config.Bind<int>("Equipment : " + EquipmentName, "Number of attacks", 1, "Number of attacks per use that this equipment performs.").Value;
            enemyMultiplier = config.Bind<float>("Equipment : " + EquipmentName, "Extra damage per enemy", 4.3f, "The total enemy team count will get multiplied by this value, i.e if theres one enemy, the base damage will be multiplied by 5.").Value;
            baseMultiplier = config.Bind<float>("Equipment : " + EquipmentName, "Owners extra damage", 2.00f, "The owners current base damage will be multiplied by this value.").Value;
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
#if DEBUG
            Chat.AddMessage(EquipmentName + " Activated.");
#endif
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
                    if (teamCounter != TeamIndex.Neutral && enemyMember.Count <= 0) { return false; } //Added extra check for neutral since it ALWAYS tries to cycle through neutral enemies first (ie pots or barrels), if the extra check wasn't there it would return and never check for monsters.
                    for (int aC = 0; aC < attackCount; aC++)
                    {
#if DEBUG
                        TurboEdition._logger.LogWarning(EquipmentName + " Trying to attack somebody, " + (aC + 1) + " out of " + attackCount);
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

                                if (HasLoS(cb.gameObject, enemyObject) && Vector3.Dot(cbForward, enemyPos) > 0.4f) // Dot returns 1 if they point in exactly the same direction, -1 if they point in completely opposite directions and zero if the vectors are perpendicular.
                                {
#if DEBUG
                                    TurboEdition._logger.LogWarning(EquipmentName + " Found a suitable enemy inside the fov/range/whatever: " + enemyObject + " at " + enemyPos);
#endif
                                    if (NetworkServer.active)
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
            TurboEdition._logger.LogWarning(EquipmentName + " Calculating damage. Base damage: " + baseDamage + " and theres " + TeamComponent.GetTeamMembers(enemyTeam).Count + " enemies, dealing " + enemyMultiplier * TeamComponent.GetTeamMembers(enemyTeam).Count + " (" + (baseDamage * (enemyMultiplier * TeamComponent.GetTeamMembers(enemyTeam).Count)) + ") more damage.");
#endif
            return ((float)((baseDamage * baseMultiplier) + (baseDamage * (enemyMultiplier * TeamComponent.GetTeamMembers(enemyTeam).Count))));
        }
    }
}
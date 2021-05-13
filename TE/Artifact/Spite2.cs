using BepInEx.Configuration;
using RoR2;
using UnityEngine.Networking;
using UnityEngine;

//TODO: figure out how can we move the spite bombs horizontally, so they dont stay in the same place constantly
//That would singlehandly fix what makes spite so shitty
namespace TurboEdition.Artifacts
{
    public class Spite2 : ArtifactBase<Spite2>
    {
        public override string ArtifactLangToken => "SPITE2";
        public override string ArtifactName => "Revenge";
        public override string ArtifactDesc => $"Enemies have a " + /*{spiteChance * 100}% + */  "chance to drop bombs on hit. Extra bombs if Spite is enabled.";

        public override UnlockableDef ArtifactUnlockable => null; //MUST BE SET TO NULL UNLESS THERES AN UNLOCKABLE
        public override Sprite ArtifactEnabledIcon => TurboEdition.MainAssets.LoadAsset<Sprite>("Assets/Textures/Icons/Artifacts/spite2_selected.png");
        public override Sprite ArtifactDisabledIcon => TurboEdition.MainAssets.LoadAsset<Sprite>("Assets/Textures/Icons/Artifacts/spite2_deselected.png");
        public override GameObject ArtifactModel => TurboEdition.MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/Default.prefab");

        //sasdjasdbasd
        private float spiteChance;

        private bool onlySpite;
        private int spiteBombs;
        private int playerBombs;
        private bool friendlyBombs;

        //FUCK FUCK FUCK FUCK
        protected override void CreateConfig(ConfigFile config)
        {
            spiteChance = config.Bind<float>("Artifact: " + ArtifactName, "Chance for bomb on hit", 0.05f, "Chance for one spite bomb to be created when an enemy is hit.").Value;
            onlySpite = config.Bind<bool>("Artifact: " + ArtifactName, "Extra functionality with Spite", true, "Should the extra functionality only work when Spite is on or not.").Value;
            spiteBombs = config.Bind<int>("Artifact: " + ArtifactName, "Bombs per each hit", 3, "Maximum number of bombs when anything but a Player gets hit (monsters or neutral). Set to ZERO to use the own game's calculations.").Value;
            playerBombs = config.Bind<int>("Artifact: " + ArtifactName, "Extras: Bombs on non-monster death", 20, "Maximum number of bombs when anything but a Monster dies (players or neutral). Set to ZERO to use the own game's calculations.").Value;
            friendlyBombs = config.Bind<bool>("Artifact: " + ArtifactName, "Extras: Friendly player bombs", false, "Like RoR1 behavior, should the bombs generated on player's team member death be on the player's team (hurts monsters and neutral, not players).").Value;
        }

        protected override void Initialization()
        {
#if DEBUG
            TurboEdition._logger.LogWarning(ArtifactName + " since we are running a DEBUG build, overwritting bomb chances.");
            spiteChance = 0.75f;
#endif
        }

        protected override void HookEnabled()
        {
            base.HookEnabled(); //vs added these by itself, if you ask me i have no idea why
            ManageManager(false);
            Run.onRunStartGlobal += Run_onRunStartGlobal;
            GlobalEventManager.onServerDamageDealt += BombOnHit;
            GlobalEventManager.onCharacterDeathGlobal += ExtraDeath;
        }

        //Works as initial setup
        private void Run_onRunStartGlobal(Run obj)
        {
            if (NetworkServer.active)
            {
                var managerGameobject = obj.gameObject.GetComponentInChildren<Bomb2ArtifactManager>()?.gameObject;
                if (!managerGameobject)
                {
                    obj.gameObject.AddComponent<Bomb2ArtifactManager>();
                }
            }
        }

        protected override void HookDisabled()
        {
            base.HookDisabled();
            ManageManager(true);
            Run.onRunStartGlobal -= Run_onRunStartGlobal;
            GlobalEventManager.onCharacterDeathGlobal -= ExtraDeath;
            GlobalEventManager.onServerDamageDealt -= BombOnHit;
        }

        private void ManageManager(bool disabling)
        {
            if (Run.instance)
            {
                var managerGameobject = Run.instance.gameObject.GetComponentInChildren<Bomb2ArtifactManager>()?.gameObject;
                if (managerGameobject && disabling)
                {
#if DEBUG
                    TurboEdition._logger.LogWarning(ArtifactName + " artifact got disabled and the Run had the manager, destroying it.");
#endif
                    UnityEngine.Object.Destroy(managerGameobject);
                }
                else if (!managerGameobject && !disabling)
                {
#if DEBUG
                    TurboEdition._logger.LogWarning(ArtifactName + " artifact got enabled and the Run didnt have the manager, creating it.");
#endif
                    Run.instance.gameObject.AddComponent<Bomb2ArtifactManager>();
                }
            }
        }

        private void BombOnHit(DamageReport damageReport)
        {
            if (!NetworkServer.active || !ArtifactEnabled) { return; }
            if (damageReport.victim.body.teamComponent.teamIndex == TeamIndex.Player) { return; } //Because pots and barrels making spite bombs is funny. also Birdsharks

#if DEBUG
            TurboEdition._logger.LogWarning(ArtifactName + " is enabled, and body hurt wasnt a player gonna check rolls.");
#endif

            if (Util.CheckRoll(spiteChance * 100) && damageReport.victim.body.healthComponent)
            {
#if DEBUG
                Chat.AddMessage(ArtifactName + " succeeded the roll.");
#endif
                Bomb2ArtifactManager manager = Run.instance.GetComponentInChildren<Bomb2ArtifactManager>();
                if (!manager) { return; }
#if DEBUG
                TurboEdition._logger.LogWarning(ArtifactName + " manager does exist, spawning bombs.");
#endif
                if (spiteBombs > 0)
                {
#if DEBUG
                    TurboEdition._logger.LogWarning(ArtifactName + " spiteBombs config is set to " + spiteBombs + " overwritting the game's calculation methods.");
#endif
                    int sbc = (int)UnityEngine.Random.Range(1, (float)spiteBombs);
#if DEBUG
                    TurboEdition._logger.LogWarning(ArtifactName + " spawning " + sbc + " spiteBombs.");
#endif
                    manager.SpawnBombFromBody(damageReport.victimBody, sbc);
                    return;
                }
                manager.SpawnBombFromBody(damageReport.victimBody);
            }
        }

        private void ExtraDeath(DamageReport damageReport)
        {
            if (!NetworkServer.active || !ArtifactEnabled) { return; }
            if (RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.bombArtifactDef) || !onlySpite)
            {
                if (damageReport.victim.body.teamComponent.teamIndex == TeamIndex.Monster) { return; } //funny incoming
#if DEBUG
                Chat.AddMessage(ArtifactName + " something that is not a monster died, spawning spite bombs.");
#endif
                Bomb2ArtifactManager manager = Run.instance.GetComponentInChildren<Bomb2ArtifactManager>();
                if (!manager) { return; }
#if DEBUG
                TurboEdition._logger.LogWarning(ArtifactName + " manager does exist, spawning MONSTER bombs.");
#endif
                if (playerBombs > 0)
                {
#if DEBUG
                    TurboEdition._logger.LogWarning(ArtifactName + " playerBombs config is set to " + playerBombs + " overwritting the game's calculation methods.");
#endif
                    int pbc = (int)UnityEngine.Random.Range(1, (float)playerBombs);
#if DEBUG
                    TurboEdition._logger.LogWarning(ArtifactName + " spawning " + pbc + " playerBombs.");
#endif
                    if (damageReport.victim.body.teamComponent.teamIndex == TeamIndex.Player && friendlyBombs)
                    {
                        manager.SpawnBombFromBody(damageReport.victimBody, pbc);
                        return;
                    }
                    manager.SpawnMonsterBombFromBody(damageReport.victimBody, pbc);
                    return;
                }
                manager.SpawnMonsterBombFromBody(damageReport.victimBody);
            }
        }
    }
}
using EntityStates;
using RoR2;
using UnityEngine;

namespace TurboEdition.EntityStates.ImpBomber.Weapon
{
    internal class BombGet : BaseState
    {
        public static float baseDuration;
        public static string enterSoundString;

        private ChildLocator childLocator;
        private GameObject bombInstance;
        public GameObject bombPrefabDefault;

        [Tooltip("The stages in which to use a specific model. Parallel to stageProjectilePrefabs.")]
        [SerializeField]
        public string[] stageNames;
        [Tooltip("The specific projectile prefabs to use per stage. Parallel to stageNames.")]
        [SerializeField]
        public GameObject[] stageBombPrefabs;

        private float duration
        {
            get
            {
                return baseDuration / this.attackSpeedStat;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            base.PlayCrossfade("Gesture, Additive", "EnterReload", "Reload.playbackRate", this.duration, 0.1f);
            Util.PlaySound(enterSoundString, base.gameObject);
            this.childLocator = base.GetModelChildLocator();
            if (this.childLocator)
            {
                Transform transform = this.childLocator.FindChild("MuzzleBetween") ?? base.characterBody.coreTransform;
                for (int i = 0; i < stageNames.Length; i++)
                {
                    if (SceneCatalog.mostRecentSceneDef == SceneCatalog.GetSceneDefFromSceneName(stageNames[i]) && stageBombPrefabs[i])
                    {
                        this.bombInstance = UnityEngine.Object.Instantiate<GameObject>(this.stageBombPrefabs[i], transform.position, transform.rotation);
                        this.bombInstance.transform.parent = transform;
                        return;
                    }
                }
                if (transform && this.bombPrefabDefault)
                {
                    this.bombInstance = UnityEngine.Object.Instantiate<GameObject>(this.bombPrefabDefault, transform.position, transform.rotation);
                    this.bombInstance.transform.parent = transform;
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && base.fixedAge > this.duration)
            {
                BombHolding jc = new BombHolding();
                jc.boneBombInstance = bombInstance;
                this.outer.SetNextState(jc);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
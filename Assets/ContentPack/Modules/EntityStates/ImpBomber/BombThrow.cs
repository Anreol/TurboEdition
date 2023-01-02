using EntityStates;
using UnityEngine;

namespace TurboEdition.EntityStates.ImpBomber.Weapon
{
    internal class BombThrow : GenericProjectileBaseState
    {
        public static string bombBoneChildName;

        [SerializeField]
        public GameObject bombPrefabDefault;

        private ChildLocator childLocator;
        private GameObject bombInstance;

        public override void OnEnter()
        {
            this.childLocator = base.GetModelChildLocator();
            if (this.childLocator)
            {
                Transform transform = this.childLocator.FindChild(bombBoneChildName) ?? base.characterBody.coreTransform;
                if (transform && this.bombPrefabDefault)
                {
                    this.bombInstance = UnityEngine.Object.Instantiate<GameObject>(this.bombPrefabDefault, transform.position, transform.rotation, transform);
                    bombInstance.GetComponent<ChildLocator>().FindChild("Light").gameObject.SetActive(true);
                }
            }
            base.GetModelAnimator().SetBool("BombHolding.active", false);
            base.OnEnter();
        }

        public override void FireProjectile()
        {
            if (bombInstance)
            {
                UnityEngine.Object.Destroy(bombInstance);
            }
            base.FireProjectile();
        }

        public override void PlayAnimation(float duration)
        {
            base.PlayAnimation(duration);
            base.PlayCrossfade("Gesture, Additive", "ThrowBomb", "BombThrow.playbackRate", duration, 0.1f);
        }
    }
}
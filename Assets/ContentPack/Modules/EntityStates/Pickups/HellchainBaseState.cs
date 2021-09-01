using EntityStates;
using RoR2;
using TurboEdition.Components;
using UnityEngine;

namespace TurboEdition.States.Pickups
{
    public class HellchainBaseState : BaseBodyAttachmentState
    {
        private protected HellchainController hellchainController { get; set; }
        private protected SimpleListTether tetherList { get; set; }
        private protected SphereSearch sphereSearch { get; set; }
        private protected BuffDef buffDef = Assets.mainAssetBundle.LoadAsset<BuffDef>("BuffHellLinked");
        private ParticleSystem chainParticle;

        protected virtual bool shouldVFXAppear
        {
            get
            {
                return false;
            }
        }

        protected bool HasBuff()
        {
            if (!base.attachedBody)
            {
                return false;
            }
            return base.attachedBody.GetBuffCount(buffDef) > 0;
        }

        protected float GetRemainingDuration()
        {
            if (HasBuff())
            {
                foreach (CharacterBody.TimedBuff timedBuff in base.attachedBody.timedBuffs)
                {
                    if (timedBuff.buffIndex == buffDef.buffIndex)
                    {
                        return timedBuff.timer;
                    }
                }
            }
            return 0;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            this.tetherList = base.GetComponent<SimpleListTether>();
            this.hellchainController = base.GetComponent<HellchainController>();

            ChildLocator component = base.GetComponent<ChildLocator>();
            if (component)
            {
                Transform transform = component.FindChild("Chains");
                this.chainParticle = ((transform != null) ? transform.GetComponent<ParticleSystem>() : null);
                if (this.chainParticle)
                {
                    ParticleSystem.ShapeModule shape = this.chainParticle.shape;
                    SkinnedMeshRenderer skinnedMeshRenderer = this.FindAttachedBodyMainRenderer();
                    if (skinnedMeshRenderer)
                    {
                        shape.skinnedMeshRenderer = this.FindAttachedBodyMainRenderer();
                        ParticleSystem.MainModule main = this.chainParticle.main;
                        float x = skinnedMeshRenderer.transform.lossyScale.x;
                        main.startSize = 0.5f / x;
                    }
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!HasBuff())
            {
                Destroy(this.gameObject);
            }
        }

        private SkinnedMeshRenderer FindAttachedBodyMainRenderer()
        {
            if (!base.attachedBody)
            {
                return null;
            }
            ModelLocator modelLocator = base.attachedBody.modelLocator;
            CharacterModel.RendererInfo[] array;
            if (modelLocator == null)
            {
                array = null;
            }
            else
            {
                CharacterModel component = modelLocator.modelTransform.GetComponent<CharacterModel>();
                array = ((component != null) ? component.baseRendererInfos : null);
            }
            CharacterModel.RendererInfo[] array2 = array;
            if (array2 == null)
            {
                return null;
            }
            for (int i = 0; i < array2.Length; i++)
            {
                SkinnedMeshRenderer result;
                if ((result = (array2[i].renderer as SkinnedMeshRenderer)) != null)
                {
                    return result;
                }
            }
            return null;
        }
    }
}
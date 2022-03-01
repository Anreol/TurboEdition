using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Components
{
    [RequireComponent(typeof(NetworkedBodyAttachment))]
    public class HellchainBodyAttachment : NetworkBehaviour, INetworkedBodyAttachmentListener, IOnTakeDamageServerReceiver
    {
        private NetworkedBodyAttachment nba;

        public GameObject tetherEffectPrefab;
        public GameObject tetherEffectInstance;
        private GameObject tetherEffectInstanceEnd;

        private GameObject targetTether;
        private void Awake()
        {
            this.nba = base.GetComponent<NetworkedBodyAttachment>();
            InstanceTracker.Add<HellchainBodyAttachment>(this);
        }
        public void Update() //Purely visual, so this goes on a update!
        {
            if (this.tetherEffectPrefab && !this.tetherEffectInstance)
            {
                var list = InstanceTracker.GetInstancesList<HellchainBodyAttachment>();
                int rand;
                int attempts = 3;
                do
                {
                    attempts--;
                    rand = UnityEngine.Random.Range(0, list.Count);
                } while (list[rand].gameObject == this || attempts > 0);
                targetTether = list[rand].gameObject;

                this.tetherEffectInstance = UnityEngine.Object.Instantiate<GameObject>(this.tetherEffectPrefab, base.transform.position, base.transform.rotation);
                this.tetherEffectInstance.transform.parent = base.transform;
                ChildLocator component = this.tetherEffectInstance.GetComponent<ChildLocator>();
                this.tetherEffectInstanceEnd = component.FindChild("TetherEnd").gameObject;
            }
            if (this.tetherEffectInstance)
            {
                Ray aimRay = default(Ray);
                aimRay.origin = base.transform.position;
                aimRay.direction = nba.attachedBody.transform.position - aimRay.origin;
                this.tetherEffectInstance.transform.rotation = Util.QuaternionSafeLookRotation(aimRay.direction);
                this.tetherEffectInstanceEnd.transform.position = aimRay.origin + aimRay.direction * this.GetRayDistance();
            }
        }
        private float GetRayDistance()
        {
            if (targetTether)
            {
                return (nba.attachedBody.transform.position - targetTether.transform.position).magnitude;
            }
            return 0f;
        }
        private void OnDestroy()
        {
            //This SHOULDNT cause any errors because nothing should be fucking with the order of things in this list... I hope.
            if (nba.attachedBody.healthComponent)
            {
                int i = System.Array.IndexOf(nba.attachedBody.healthComponent.onIncomingDamageReceivers, this);
                if (i > -1)
                    HG.ArrayUtils.ArrayRemoveAtAndResize(ref nba.attachedBody.healthComponent.onIncomingDamageReceivers, nba.attachedBody.healthComponent.onIncomingDamageReceivers.Length, i);
            }
            InstanceTracker.Remove<HellchainBodyAttachment>(this);
        }

        public void OnAttachedBodyDiscovered(NetworkedBodyAttachment networkedBodyAttachment, CharacterBody attachedBody)
        {
            if (attachedBody.healthComponent)
                HG.ArrayUtils.ArrayAppend(ref attachedBody.healthComponent.onTakeDamageReceivers, this);
        }

        public void OnTakeDamageServer(DamageReport damageReport)
        {
            if (damageReport.damageInfo.rejected || damageReport.damageInfo.procCoefficient <= 0)
                return;
            if (damageReport.dotType != DotController.DotIndex.None || damageReport.damageInfo.damageType == DamageType.VoidDeath || damageReport.isFallDamage || damageReport.damageInfo.damageType == DamageType.Nullify || damageReport.damageInfo.damageType == DamageType.Silent)
                return;
            damageReport.damageInfo.procCoefficient = 0; //Set to zero to fuck with procs and to avoid self proccing

            var list = InstanceTracker.GetInstancesList<HellchainBodyAttachment>();
            foreach (var item in list)
            {
                item.nba.attachedBody.healthComponent.TakeDamage(damageReport.damageInfo);
            }
        }
    }
}

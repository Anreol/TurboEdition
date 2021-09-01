using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Components
{
    [RequireComponent(typeof(NetworkIdentity))]
    [RequireComponent(typeof(NetworkStateMachine))]
    [RequireComponent(typeof(EntityStateMachine))]
    public class HellchainController : NetworkBehaviour
    {
        public List<CharacterBody> listOLinks;
        private protected CharacterBody attachedCB;

        private void Awake()
        {
            this.attachedCB = base.GetComponent<NetworkedBodyAttachment>().attachedBody;
        }

        private void OnEnable()
        {
            if (NetworkServer.active)
            {
                GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            }
        }

        private void OnDisable()
        {
            GlobalEventManager.onCharacterDeathGlobal -= GlobalEventManager_onCharacterDeathGlobal;
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport obj) //is this needed...? seems expensive...
        {
            if (listOLinks.Contains(obj.victimBody))
            {
                listOLinks.Remove(obj.victimBody);
            }
        }

        private void Start()
        {
            //Util.PlaySound(this.soundLoopString, base.gameObject); //do uh, whatever here
        }

        private void Update()
        {
            if (NetworkClient.active)
            {
                this.UpdateClient();
            }
        }

        public void FixedUpdate()
        {
        }

        [Client]
        private void UpdateClient()
        {
        }
    }
}
using System;
using TurboEdition.Items;
using TurboEdition.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2
{
    [RequireComponent(typeof(NetworkUIPromptController))]
    public class MoneyBankInteractionController : NetworkBehaviour, IInteractable
    {
        public bool available = true;

        private void Awake()
        {
            this.networkUIPromptController = base.GetComponent<NetworkUIPromptController>();
            if (NetworkClient.active)
            {
                this.networkUIPromptController.onDisplayBegin += this.OnDisplayBegin;
                this.networkUIPromptController.onDisplayEnd += this.OnDisplayEnd;
            }
            if (NetworkServer.active)
            {
                this.networkUIPromptController.messageFromClientHandler = new Action<NetworkReader>(this.HandleClientMessage);
            }
        }

        private void OnEnable()
        {
            InstanceTracker.Add<MoneyBankInteractionController>(this);
        }

        private void OnDisable()
        {
            InstanceTracker.Remove<MoneyBankInteractionController>(this);
        }

        private void HandleClientMessage(NetworkReader reader)
        {
            byte b = reader.ReadByte();
            if (b == 0)
            {
                int moneyAmount = reader.ReadInt32(); //Read the int from client's SubmitChoice
                this.HandleTransaction(this.networkUIPromptController.currentParticipantMaster, moneyAmount);
                return;
            }
            if (b != 1)
            {
                return;
            }
            this.networkUIPromptController.SetParticipantMaster(null);
        }

        private void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                this.FixedUpdateServer();
            }
            if (this.panelInstanceController?.moneyAmount != cachedMoneyAmount)
                this.panelInstanceController.moneyAmount = cachedMoneyAmount;
            if (this.panelInstanceController?.maxMoneyAmount != MoneyBankManager.maxMoneyAmountToStore)
                this.panelInstanceController.maxMoneyAmount = MoneyBankManager.maxMoneyAmountToStore;
        }

        [Server] //Originally didn't have a server tag
        private void FixedUpdateServer()
        {
            CharacterMaster currentParticipantMaster = this.networkUIPromptController.currentParticipantMaster;
            if (currentParticipantMaster)
            {
                CharacterBody body = currentParticipantMaster.GetBody();
                if (!body || (body.inputBank.aimOrigin - base.transform.position).sqrMagnitude > this.cutoffDistance * this.cutoffDistance)
                {
                    this.networkUIPromptController.SetParticipantMaster(null);
                    return;
                }
                if (cachedMoneyAmount != MoneyBankManager.serverCurrentMoneyAmount)
                    cachedMoneyAmount = MoneyBankManager.serverCurrentMoneyAmount;
            }
        }

        private void OnPanelDestroyed(OnDestroyCallback onDestroyCallback)
        {
            NetworkWriter networkWriter = this.networkUIPromptController.BeginMessageToServer();
            networkWriter.Write(1);
            this.networkUIPromptController.FinishMessageToServer(networkWriter);
        }

        private void OnDisplayBegin(NetworkUIPromptController networkUIPromptController, LocalUser localUser, CameraRigController cameraRigController)
        {
            this.panelInstance = UnityEngine.Object.Instantiate<GameObject>(this.panelPrefab, cameraRigController.hud.mainContainer.transform);
            this.panelInstanceController = this.panelInstance.GetComponent<MoneyBankPanel>();
            this.panelInstanceController.pickerController = this;
            this.panelInstanceController.moneyAmount = cachedMoneyAmount;
            OnDestroyCallback.AddCallback(this.panelInstance, new Action<OnDestroyCallback>(this.OnPanelDestroyed));
        }

        private void OnDisplayEnd(NetworkUIPromptController networkUIPromptController, LocalUser localUser, CameraRigController cameraRigController)
        {
            UnityEngine.Object.Destroy(this.panelInstance);
            this.panelInstance = null;
            this.panelInstanceController = null;
        }

        [Server]
        public void SetAvailable(bool newAvailable)
        {
            this.available = newAvailable;
        }

        //Gets called from the UI, each button. Sends the amount to the server, which will read the value and process the transaction.
        public void SubmitChoice(int moneyAmount)
        {
            //If its the client...
            if (!NetworkServer.active) 
            {
                NetworkWriter networkWriter = this.networkUIPromptController.BeginMessageToServer();
                networkWriter.Write(0);
                networkWriter.Write(moneyAmount); //Writes into HandleClientMessage
                this.networkUIPromptController.FinishMessageToServer(networkWriter);
                return;
            }
            //If its already server...
            this.HandleTransaction(this.networkUIPromptController.currentParticipantMaster, moneyAmount); 
        }

        [Server]
        private void HandleTransaction(CharacterMaster interactor, int moneyAmount)
        {
            MoneyBankInteractionController.MoneyInteractedUnityEvent moneyInteractedUnityEvent = this.onMoneyInteracted;
            if (moneyInteractedUnityEvent == null)
            {
                return;
            }
            moneyInteractedUnityEvent.Invoke(interactor, moneyAmount);
        }

        //Used in unity events
        [Server]
        private void AddMoneyToBank(CharacterMaster master, int moneyAmount)
        {
            if (MoneyBankManager.CanStoreMoney)
            {
                if (TurboEdition.Items.MoneyBankManager.AddMoney(moneyAmount))
                {
                    master.money -= (uint)moneyAmount;
                }
                
            }
        }

        //Used in unity events
        [Server]
        private void RemoveMoneyFromBank(CharacterMaster master, int moneyAmount)
        {
            int moneyFromBank = TurboEdition.Items.MoneyBankManager.SubstractMoney(moneyAmount);
            if (moneyFromBank > 0)
            {
                master.GiveMoney((uint)moneyFromBank);
            }
        }

        public string GetContextString(Interactor activator)
        {
            return Language.GetString(this.contextString);
        }

        public Interactability GetInteractability(Interactor activator)
        {
            if (this.networkUIPromptController.inUse)
            {
                return Interactability.ConditionsNotMet;
            }
            if (!this.available)
            {
                return Interactability.Disabled;
            }
            return Interactability.Available;
        }

        public void OnInteractionBegin(Interactor activator)
        {
            this.onServerInteractionBegin.Invoke(activator);
            this.networkUIPromptController.SetParticipantMasterFromInteractor(activator);
        }

        public bool ShouldIgnoreSpherecastForInteractibility(Interactor activator)
        {
            return false;
        }

        public bool ShouldShowOnScanner()
        {
            return true;
        }

        public GameObject panelPrefab;

        //Used on unity
        public MoneyBankInteractionController.MoneyInteractedUnityEvent onMoneyInteracted;

        public GenericInteraction.InteractorUnityEvent onServerInteractionBegin;

        public float cutoffDistance;
        public string contextString = "";
        private NetworkUIPromptController networkUIPromptController;

        private GameObject panelInstance;
        private MoneyBankPanel panelInstanceController;

        [SyncVar]
        private uint cachedMoneyAmount;

        private static readonly uint optionsDirtyBit = 1U;

        private static readonly uint allDirtyBits = MoneyBankInteractionController.optionsDirtyBit;

        [Serializable]
        public class MoneyInteractedUnityEvent : UnityEvent<CharacterMaster, int>
        {
            public MoneyInteractedUnityEvent()
            {
            }
        }
    }
}
using RoR2;
using System;
using TurboEdition.Items;
using TurboEdition.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace TurboEdition.Components
{
    [RequireComponent(typeof(NetworkUIPromptController))]
    public class MoneyBankInteractionController : NetworkBehaviour, IInteractable, IDisplayNameProvider
    {

        public GameObject panelPrefab;

        public bool available = true;
        public int baseContributeCost = 5;
        public int baseWithdrawCost = 5;
        public int contributeButtonsAmount = 3;
        public int withdrawButtonsAmount = 3;
        //Used on unity
        public MoneyBankInteractionController.MoneyInteractedUnityEvent onMoneyInteracted;
        public MoneyBankInteractionController.AffordableContributalUnityEvent onAffordableContributal;
        public MoneyBankInteractionController.AffordableWithdrawalUnityEvent onAffordableWithdrawal;

        public GenericInteraction.InteractorUnityEvent onServerInteractionBegin;

        public float cutoffDistance;
        public string interactableString = "";
        public string contextString = "";
        internal NetworkUIPromptController networkUIPromptController;

        private GameObject panelInstance;
        private MoneyBankPanel panelInstanceController;

        [SyncVar]
        internal uint cachedSyncedMoneyAmount;

        private static readonly uint optionsDirtyBit = 1U;

        private static readonly uint allDirtyBits = MoneyBankInteractionController.optionsDirtyBit;

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
            //The interaction can be done multiple times.
            //this.networkUIPromptController.SetParticipantMaster(null);
        }

        private void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                this.FixedUpdateServer();
            }
            if (panelInstanceController != null)
            {
                if (this.panelInstanceController.displayMoneyAmount != cachedSyncedMoneyAmount)
                {
                    this.panelInstanceController.displayMoneyAmount = cachedSyncedMoneyAmount;
                    panelInstanceController.dirtyUI = true;
                }
                if (this.panelInstanceController.displayTargetMoneyAmount != MoneyBankManager.targetMoneyAmountToStore)
                {
                    this.panelInstanceController.displayTargetMoneyAmount = MoneyBankManager.targetMoneyAmountToStore;
                    panelInstanceController.dirtyUI = true;
                }
            }
        }

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
                if (cachedSyncedMoneyAmount != MoneyBankManager.serverCurrentMoneyAmount)
                    cachedSyncedMoneyAmount = MoneyBankManager.serverCurrentMoneyAmount;
            }
        }

        private void OnPanelDestroyed(OnDestroyCallback onDestroyCallback)
        {
            NetworkWriter networkWriter = this.networkUIPromptController.BeginMessageToServer();
            networkWriter.Write(1);
            this.networkUIPromptController.FinishMessageToServer(networkWriter);
            this.networkUIPromptController.SetParticipantMaster(null); //Just in case.
        }

        private void OnDisplayBegin(NetworkUIPromptController networkUIPromptController, LocalUser localUser, CameraRigController cameraRigController)
        {
            this.panelInstance = UnityEngine.Object.Instantiate<GameObject>(this.panelPrefab, cameraRigController.hud.mainContainer.transform);
            this.panelInstanceController = this.panelInstance.GetComponent<MoneyBankPanel>();
            this.panelInstanceController.pickerController = this;
            this.panelInstanceController.displayMoneyAmount = cachedSyncedMoneyAmount;
            this.panelInstanceController.displayTargetMoneyAmount = MoneyBankManager.targetMoneyAmountToStore;
            this.panelInstanceController.dirtyUI = true;
            this.panelInstanceController.SetButtons(contributeButtonsAmount, withdrawButtonsAmount);

            OnDestroyCallback.AddCallback(this.panelInstance, new Action<OnDestroyCallback>(this.OnPanelDestroyed));
        }

        private void OnDisplayEnd(NetworkUIPromptController networkUIPromptController, LocalUser localUser, CameraRigController cameraRigController)
        {
            this.networkUIPromptController.SetParticipantMaster(null); //Uh, just in case?
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
                NetworkWriter networkWriter = this.networkUIPromptController.BeginMessageToServer(); //Start writing to server...
                networkWriter.Write(0);
                networkWriter.Write(moneyAmount); //Writes into HandleClientMessage
                this.networkUIPromptController.FinishMessageToServer(networkWriter); //Send to server...
                return; //And return.
            }
            //If its already server...
            this.HandleTransaction(this.networkUIPromptController.currentParticipantMaster, moneyAmount); 
        }

        [Server]
        private void HandleTransaction(CharacterMaster interactor, int moneyAmount)
        {
            if (moneyAmount > int.MaxValue || moneyAmount < int.MinValue)
            {
                return; //Just in case.
            }
            onMoneyInteracted?.Invoke(interactor, moneyAmount);
            if (moneyAmount > 0 && interactor.money >= moneyAmount && MoneyBankManager.CanStoreMoney)
            {
                onAffordableContributal?.Invoke(interactor, moneyAmount);
                return;
            }
            if (moneyAmount < 0 && moneyAmount <= MoneyBankManager.serverCurrentMoneyAmount)
            {
                onAffordableWithdrawal?.Invoke(interactor, moneyAmount);
                return;
            }
        }

        //Used in unity events
        [Server]
        public void AddMoneyToBank(CharacterMaster master, int moneyAmount)
        {
            if (MoneyBankManager.CanStoreMoney)
            {
                moneyAmount = (int)((moneyAmount + cachedSyncedMoneyAmount) > MoneyBankManager.targetMoneyAmountToStore ? MoneyBankManager.targetMoneyAmountToStore - (moneyAmount + cachedSyncedMoneyAmount) : moneyAmount);
                //uint moneyAmountuint = Mathf.Clamp(moneyAmount, 0, cachedSyncedMoneyAmount);
                if (TurboEdition.Items.MoneyBankManager.AddMoney(moneyAmount))
                {
                    master.money -= (uint)moneyAmount;
                }
            }
        }

        //Used in unity events
        [Server]
        public void RemoveMoneyFromBank(CharacterMaster master, int moneyAmount)
        {
            uint moneyFromBank = TurboEdition.Items.MoneyBankManager.SubstractMoney((uint)Math.Abs(moneyAmount));
            if (moneyFromBank > 0)
            {
                master.GiveMoney(moneyFromBank);
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

        public string GetDisplayName()
        {
            return Language.GetString(interactableString) + "(" + cachedSyncedMoneyAmount + "$ / " + MoneyBankManager.targetMoneyAmountToStore + "$ )";
        }


        [Serializable]
        public class MoneyInteractedUnityEvent : UnityEvent<CharacterMaster, int>
        {
        }

        [Serializable]
        public class AffordableContributalUnityEvent : UnityEvent<CharacterMaster, int>
        {
        }
        [Serializable]
        public class AffordableWithdrawalUnityEvent : UnityEvent<CharacterMaster, int>
        {
        }
    }
}
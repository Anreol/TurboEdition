using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurboEdition.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace TurboEdition.Components
{
    [RequireComponent(typeof(NetworkUIPromptController))]
    class MoneyBankInteractorController : NetworkBehaviour, IInteractable
    {
		public MoneyBankInteractorController.MoneySubmittedUnityEvent onMoneySubmitted;
		public GenericInteraction.InteractorUnityEvent onServerInteractionBegin;

        public NetworkUIPromptController networkUIPromptController;
        public GameObject panelPrefab;
		public bool available;
		public string contextString;
        public int cutoffDistance;
        
		private GameObject panelInstance;
        private MoneyBankPanel panelInstanceController;
        private int moneyMount;

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
			InstanceTracker.Add<MoneyBankInteractorController>(this);
		}
		private void OnDisable()
		{
			InstanceTracker.Remove<MoneyBankInteractorController>(this);
		}
		private void HandleClientMessage(NetworkReader reader)
		{
			byte b = reader.ReadByte();
			if (b == 0)
			{
				int moneyAmount = reader.ReadInt32();
				this.HandleMoneySubmitted(moneyAmount);
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
				}
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
			this.panelInstanceController.interactorController = this;
			this.panelInstanceController.moneyAmount = this.moneyMount;
			OnDestroyCallback.AddCallback(this.panelInstance, new Action<OnDestroyCallback>(this.OnPanelDestroyed));
		}
		private void OnDisplayEnd(NetworkUIPromptController networkUIPromptController, LocalUser localUser, CameraRigController cameraRigController)
		{
			UnityEngine.Object.Destroy(this.panelInstance);
			this.panelInstance = null;
			this.panelInstanceController = null;
		}

		[Server]
		public void SetMoneyServer(int newMoney)
		{
			this.SetMoneyInternal(newMoney);
		}
		private void SetMoneyInternal(int newMoney)
		{
			this.moneyMount = newMoney;
			if (this.panelInstanceController)
			{
				this.panelInstanceController.moneyAmount = (this.moneyMount);
			}
			if (NetworkServer.active)
			{
				base.SetDirtyBit(MoneyBankInteractorController.optionsDirtyBit);
			}
		}
		[Server]
		public void SetAvailable(bool newAvailable)
		{
			this.available = newAvailable;
		}
		public void SubmitMoney(int moneyAmount)
		{
			if (!NetworkServer.active)
			{
				NetworkWriter networkWriter = this.networkUIPromptController.BeginMessageToServer();
				networkWriter.Write(0);
				networkWriter.Write(moneyAmount);
				this.networkUIPromptController.FinishMessageToServer(networkWriter);
				return;
			}
			this.HandleMoneySubmitted(moneyAmount);
		}
		[Server]
		private void HandleMoneySubmitted(int moneyAmount)
		{
			MoneyBankInteractorController.MoneySubmittedUnityEvent moneySubmittedUnityEvent = this.onMoneySubmitted;
			ref int moneySubmitted = ref moneyAmount;
			moneySubmittedUnityEvent?.Invoke(moneySubmitted);
		}

		public override bool OnSerialize(NetworkWriter writer, bool initialState)
		{
			uint syncVarDirtyBits = base.syncVarDirtyBits;
			if (initialState)
			{
				syncVarDirtyBits = MoneyBankInteractorController.allDirtyBits;
			}
			bool flag = (syncVarDirtyBits & MoneyBankInteractorController.optionsDirtyBit) > 0U;
			writer.WritePackedUInt32(syncVarDirtyBits);
			if (flag)
			{
				writer.Write(moneyMount);
			}
			return syncVarDirtyBits > 0U;
		}

		public override void OnDeserialize(NetworkReader reader, bool initialState)
		{
			if ((reader.ReadPackedUInt32() & MoneyBankInteractorController.optionsDirtyBit) > 0U)
			{
				int readedMoney = reader.ReadPackedUInt32();
				this.SetMoneyInternal(readedMoney);
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

		[Serializable]
		public class MoneySubmittedUnityEvent : UnityEvent<int>
        {
        }
    }
}

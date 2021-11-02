using EntityStates;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
/*
namespace TurboEdition.Components
{
    class QuestTimer : NetworkBehaviour
	{
		public void StartTimer()
		{
			if (Util.HasEffectiveAuthority(base.gameObject))
			{
				this.mainStateMachine.SetNextState(new QuestTimer.QuestTimerMainState());
			}
		}

		public void CompleteTimer()
		{
			if (Util.HasEffectiveAuthority(base.gameObject))
			{
				this.mainStateMachine.SetNextState(new QuestTimer.QuestTimerSuccessState());
			}
		}

		private void UpdateScheduledEvents(float secondsRemaining)
		{
			for (int i = 0; i < this.scheduledEvents.Length; i++)
			{
				ref EscapeSequenceController.ScheduledEvent ptr = ref this.scheduledEvents[i];
				bool flag = ptr.minSecondsRemaining <= secondsRemaining && secondsRemaining <= ptr.maxSecondsRemaining;
				if (flag != ptr.inEvent)
				{
					if (flag)
					{
						UnityEvent onEnter = ptr.onEnter;
						if (onEnter != null)
						{
							onEnter.Invoke();
						}
					}
					else
					{
						UnityEvent onExit = ptr.onExit;
						if (onExit != null)
						{
							onExit.Invoke();
						}
					}
					ptr.inEvent = flag;
				}
			}
		}

		private void SetHudCountdownEnabled(HUD hud, bool shouldEnableCountdownPanel)
		{
			shouldEnableCountdownPanel &= base.enabled;
			GameObject gameObject;
			this.hudPanels.TryGetValue(hud, out gameObject);
			if (gameObject != shouldEnableCountdownPanel)
			{
				if (shouldEnableCountdownPanel)
				{
					RectTransform rectTransform = hud.GetComponent<ChildLocator>().FindChild("TopCenterCluster") as RectTransform;
					if (rectTransform)
					{
						GameObject value = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/UI/HudModules/HudCountdownPanel"), rectTransform);
						this.hudPanels[hud] = value;
						return;
					}
				}
				else
				{
					UnityEngine.Object.Destroy(gameObject);
					this.hudPanels.Remove(hud);
				}
			}
		}

		private void SetCountdownTime(double secondsRemaining)
		{
			foreach (KeyValuePair<HUD, GameObject> keyValuePair in this.hudPanels)
			{
				keyValuePair.Value.GetComponent<TimerText>().seconds = secondsRemaining;
			}
			AkSoundEngine.SetRTPCValue("EscapeTimer", Util.Remap((float)secondsRemaining, 0f, this.countdownDuration, 0f, 100f));
		}

		private void OnEnable()
		{
			base.GetComponent<QuestComponent>();
			this.hudPanels = new Dictionary<HUD, GameObject>();
		}

		private void OnDisable()
		{
			foreach (HUD hud in HUD.readOnlyInstanceList)
			{
				this.SetHudCountdownEnabled(hud, false);
			}
			this.hudPanels = null;
		}

		public EntityStateMachine mainStateMachine;
		public float countdownDuration;

		public UnityEvent onQuestTimerEnterMain;
		public UnityEvent onQuestTimerCompleteServer;
		public UnityEvent onQuestTimerFailureServer;

		public EscapeSequenceController.ScheduledEvent[] scheduledEvents; //Lets keep this from the escape sequence controller
		private Dictionary<HUD, GameObject> hudPanels;

		public class QuestTimerBaseState : BaseState
		{
			private protected QuestComponent questComponentController { get; private set; }
			private protected QuestTimer questTimer { get; private set; }
			public override void OnEnter()
			{
				base.OnEnter();
				this.questComponentController = base.GetComponent<QuestComponent>();
				this.questTimer = base.GetComponent<QuestTimer>();
			}
		}
		public class QuestTimerMainState : QuestTimer.QuestTimerBaseState
		{
			public override void OnEnter()
			{
				base.OnEnter();
				if (base.isAuthority)
				{
					this.startTime = Run.FixedTimeStamp.now;
					this.endTime = this.startTime + base.questTimer.countdownDuration;
				}
				UnityEvent onQuestTimerEnter = base.questTimer.onQuestTimerEnterMain;
				if (onQuestTimerEnter == null)
				{
					return;
				}
				onQuestTimerEnter.Invoke();
			}

			public override void OnExit()
			{
				foreach (HUD hud in HUD.readOnlyInstanceList)
				{
					base.questTimer.SetHudCountdownEnabled(hud, false);
				}
				base.OnExit();
			}

			public override void Update()
			{
				base.Update();
				foreach (HUD hud in HUD.readOnlyInstanceList)
				{
					base.questTimer.SetHudCountdownEnabled(hud, hud.targetBodyObject);
				}
				base.questTimer.SetCountdownTime((double)this.endTime.timeUntilClamped);
			}

			public override void FixedUpdate()
			{
				base.FixedUpdate();
				base.questTimer.UpdateScheduledEvents(this.endTime.timeUntil);
				if (base.isAuthority && this.endTime.hasPassed)
				{
					this.outer.SetNextState(new QuestTimer.QuestTimerFailureState());
				}
			}

			private Run.FixedTimeStamp startTime;
			private Run.FixedTimeStamp endTime;
		}

		public class QuestTimerFailureState : QuestTimer.QuestTimerBaseState
		{
			public override void OnEnter()
			{
				base.OnEnter();
				if (NetworkServer.active)
				{
					UnityEvent onQuestTimerFailureServer = base.questTimer.onQuestTimerFailureServer;
					if (onQuestTimerFailureServer == null)
					{
						return;
					}
					onQuestTimerFailureServer.Invoke();
				}
			}
		}

		public class QuestTimerSuccessState : QuestTimer.QuestTimerBaseState
		{
			public override void OnEnter()
			{
				base.OnEnter();
				if (NetworkServer.active)
				{
					UnityEvent onQuestTimerCompleteServer = base.questTimer.onQuestTimerCompleteServer;
					if (onQuestTimerCompleteServer == null)
					{
						return;
					}
					onQuestTimerCompleteServer.Invoke();
				}
			}
		}
	}
}*/
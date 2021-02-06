using BepInEx.Configuration;
using MonoMod.Cil;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static TurboEdition.Utils.ItemHelpers;

namespace TurboEdition.Items
{
    public class DebuffNearbyEnemies : ItemBase<DebuffNearbyEnemies>
    {
        public override string ItemName => "Voice Modulator";

        public override string ItemLangTokenName => "DEBUFFNEARBYENEMIES";

        public override string ItemPickupDesc => "FUCK.";

        public override string ItemFullDescription => $"When entering combat, shriek <style=cIsStack>(+{stackPulse} times per stack)</style> in <style=cIsUtility>{baseRadius} meters</style> <style=cIsStack>(+{stackRadius} per stack) and apply <style=cIsDamage>Shaken</style>.";

		public override string ItemLore => "UUUU";

        public override ItemTier Tier => ItemTier.Tier1;

        public override string ItemModelPath => "@TurboEdition:Assets/Models/Prefabs/Default.prefab";

        public override string ItemIconPath => "@TurboEdition:Assets/Textures/Icons/Items/Tier1.png";

		public static AnimationCurve novaRadiusCurve;
		public static float duration;

		//properties
		private float baseRadius;
		private float stackRadius;
		private int stackPulse;
		private static float debuffDuration;

		//fuck
		internal static GameObject debuffPulsePrefab;

		public override void CreateConfig(ConfigFile config)
        {
			baseRadius = config.Bind<float>("Item: " + ItemName, "Initial radius", 6f, "The radius that the first item will give you.").Value;
			stackRadius = config.Bind<float>("Item: " + ItemName, "Added radius per stack", 4f, "Extend the pulse radius by this each item you get.").Value;
			stackPulse = config.Bind<int>("Item: " + ItemName, "Number of pulses", 1, "Amount of pulses to generate everytime you enter combat per item stack.").Value;
			debuffDuration = config.Bind<float>("Item: " + ItemName, "Debuff duration", 18f, "Duration in seconds for each debuff.").Value;
		}

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        protected override void Initialization()
        {
#if DEBUG
			TurboEdition._logger.LogWarning("Initializing " + ItemName + " prefab.");
#endif
			var novaPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/TeleporterHealNovaPulse");

			var scPrefabPrefab = new GameObject("ScreamPulsePrefabPrefab");
			scPrefabPrefab.AddComponent<TeamFilter>().teamIndex = novaPrefab.GetComponent<TeamFilter>().teamIndex;
			//scPrefabPrefab.AddComponent<Transform>().position
			scPrefabPrefab.AddComponent<MeshFilter>().mesh = novaPrefab.GetComponentInChildren<MeshFilter>().mesh;
			scPrefabPrefab.AddComponent<MeshRenderer>().material = UnityEngine.Object.Instantiate(novaPrefab.GetComponentInChildren<MeshRenderer>().material);
			scPrefabPrefab.GetComponent<MeshRenderer>().material.SetVector("_TintColor", new Vector4(4f, 1f, 1f, 1f)); //should be black with a tint of red
			scPrefabPrefab.AddComponent<NetworkedBodyAttachment>().forceHostAuthority = true;

			var sc = scPrefabPrefab.AddComponent<DebuffPulse>();
			sc.pulseIndicator = scPrefabPrefab.GetComponent<MeshRenderer>().transform;
			//sc.interval = 1f;
			debuffPulsePrefab = scPrefabPrefab.InstantiateClone("DebuffAuraPrefab");
			UnityEngine.Object.Destroy(scPrefabPrefab);

		}

        public override void Hooks()
        {
			On.RoR2.CharacterBody.FixedUpdate += UpdateDebuffPulse;
        }


		private void UpdateDebuffPulse(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody body)
        {
			orig(body);

			var component = body.GetComponentInChildren<DebuffPulse>()?.gameObject;
			var InventoryCount = GetCount(body);
			if(InventoryCount <= 0 || body.outOfCombat)
            {
#if DEBUG
				TurboEdition._logger.LogWarning("Player has either zero items or out of combat, destroying component.");
#endif
				if (component) UnityEngine.Object.Destroy(component);
            }
            else
            {
                if (!component)
                {
#if DEBUG
					TurboEdition._logger.LogWarning("Player does not have a component, creating one.");
#endif
					component = UnityEngine.Object.Instantiate(debuffPulsePrefab);
					component.GetComponent<TeamFilter>().teamIndex = body.teamComponent.teamIndex;
					component.GetComponent<DebuffPulse>().owner = body.gameObject;
					component.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(body.gameObject);
                }
#if DEBUG
				TurboEdition._logger.LogWarning("Updating component properties.");
#endif
				component.GetComponent<DebuffPulse>().netRadius = baseRadius + (InventoryCount - 1) * stackRadius;
				component.GetComponent<DebuffPulse>().netPulse = InventoryCount * stackPulse; 
            }

        }
		//god check at how the lepton daisy does stuff with pulses because that might be just better yknow

		[RequireComponent(typeof(TeamFilter))]
		public class DebuffPulse : NetworkBehaviour
		{
			[SyncVar]
			float radius;
			public float netRadius
			{
				get { return radius; }
				set { base.SetSyncVar<float>(value, ref radius, 1u); }
			}

			[SyncVar]
			int pulses;
			public int netPulse
			{
				get { return pulses; }
				set { base.SetSyncVar<int>(value, ref pulses, 1u); }
			}

			public float interval;
			public Transform pulseIndicator;

			public GameObject owner;

			private TeamFilter teamFilter;
			private float rangeIndicatorScaleVelocity;

			private float stopwatch;

			private void Awake()
            {
#if DEBUG
				TurboEdition._logger.LogWarning("WAKEY WAKEY.");
#endif
				teamFilter = GetComponent<TeamFilter>();
            }

			public void Update()
			{
#if DEBUG
				TurboEdition._logger.LogWarning("Updating thing.");
#endif
				if (pulseIndicator)
				{
					float num = Mathf.SmoothDamp(pulseIndicator.localScale.x, radius * 2f, ref rangeIndicatorScaleVelocity, 0.2f);
					pulseIndicator.localScale = new Vector3(num, num, num);
				}
			}

			private void FixedUpdate()
			{
				stopwatch -= Time.fixedDeltaTime;
				if (stopwatch <= 0f)
				{
					if (NetworkServer.active)
					{
						stopwatch = interval;
						ServerPulse();
					}
				}
			}

			private void OnDestroy()
			{
#if DEBUG
				TurboEdition._logger.LogWarning("DESTROYING.");
#endif
				Destroy(pulseIndicator);
			}

			[Server]
			private void ServerPulse()
			{
#if DEBUG
				TurboEdition._logger.LogWarning("Server Pulse made.");
#endif
				List<TeamComponent> teamMembers = new List<TeamComponent>();
				bool isFF = FriendlyFireManager.friendlyFireMode != FriendlyFireManager.FriendlyFireMode.Off;
				if (isFF || teamFilter.teamIndex != TeamIndex.Monster) teamMembers.AddRange(TeamComponent.GetTeamMembers(TeamIndex.Monster));
				if (isFF || teamFilter.teamIndex != TeamIndex.Neutral) teamMembers.AddRange(TeamComponent.GetTeamMembers(TeamIndex.Neutral));
				if (isFF || teamFilter.teamIndex != TeamIndex.Player) teamMembers.AddRange(TeamComponent.GetTeamMembers(TeamIndex.Player));
				float sqrad = radius * radius;
				teamMembers.Remove(owner.GetComponent<TeamComponent>());
				foreach (TeamComponent tcpt in teamMembers)
				{
					if ((tcpt.transform.position - transform.position).sqrMagnitude <= sqrad)
					{
						if (tcpt.body && tcpt.body.mainHurtBox && tcpt.body.isActiveAndEnabled && radius > 0f)
						{
							#if DEBUG
							TurboEdition._logger.LogWarning("Debuffing somebody.");
							#endif
							DebuffTarget(tcpt.body);
						}
					}
				}
			}
			private void DebuffTarget(CharacterBody target)
			{
				#if DEBUG
				TurboEdition._logger.LogWarning("Debuffed: " + target);
				#endif
				target.AddTimedBuff(GenericBuffs.shakenBuff, debuffDuration);
				Util.PlaySound("Play_item_proc_TPhealingNova_hitPlayer", target.gameObject);
			}
		}
    }
}
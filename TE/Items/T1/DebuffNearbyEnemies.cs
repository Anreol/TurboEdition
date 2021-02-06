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
		public static Run.FixedTimeStamp fixedTime;

		//properties
		private float baseRadius;
		private float stackRadius;
		private int stackPulse;
		private static float debuffDuration;
		private float pulseDuration;

		//fuck
		internal static GameObject debuffPulsePrefab;

		public override void CreateConfig(ConfigFile config)
        {
			baseRadius = config.Bind<float>("Item: " + ItemName, "Initial radius", 6f, "The radius that the first item will give you.").Value;
			stackRadius = config.Bind<float>("Item: " + ItemName, "Added radius per stack", 4f, "Extend the pulse radius by this each item you get.").Value;
			stackPulse = config.Bind<int>("Item: " + ItemName, "Number of pulses", 1, "Amount of pulses to generate everytime you enter combat per item stack.").Value;
			debuffDuration = config.Bind<float>("Item: " + ItemName, "Debuff duration", 18f, "Duration in seconds for each debuff.").Value;
			pulseDuration = config.Bind<float>("Item: " + ItemName, "Pulse duration", 2f, "Duration in seconds for the pulse, slower makes it last longer.").Value;
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

			TurboEdition._logger.LogWarning("FUCK 1");
			var scPrefabPrefab = new GameObject("ScreamPulsePrefabPrefab");
			TurboEdition._logger.LogWarning("FUCK 2");
			scPrefabPrefab.AddComponent<TeamFilter>().teamIndex = novaPrefab.GetComponent<TeamFilter>().teamIndex;
			TurboEdition._logger.LogWarning("FUCK 3");
			//MAKES IT NOT WORK scPrefabPrefab.AddComponent<ParticleSystemRenderer>().material = novaPrefab.GetComponent<ParticleSystemRenderer>().material;
			TurboEdition._logger.LogWarning("FUCK 4");
			//MAKES IT NOT WORK EITHER scPrefabPrefab.GetComponent<ParticleSystemRenderer>().material.SetVector("_TintColor", new Vector4(2.2f, 2f, 2f, 1.5f));
			//IT SAYS THIS IS PRIVATE WHY IS IT PRIVATE HOW THE HELL scPrefabPrefab.AddComponent<ParticleSystem>().main = novaPrefab.GetComponent<ParticleSystem>().main;
			TurboEdition._logger.LogWarning("FUCK 5");
			scPrefabPrefab.AddComponent<NetworkedBodyAttachment>().forceHostAuthority = true;

			TurboEdition._logger.LogWarning("FUCK 6");
			var sc = scPrefabPrefab.AddComponent<DebuffPulse>();
			TurboEdition._logger.LogWarning("FUCK 7");
			// I DONT THINK THIS THING LIKES PARTICLESYSTEMREDERERS sc.pulseIndicator = scPrefabPrefab.GetComponent<ParticleSystemRenderer>().transform;

			TurboEdition._logger.LogWarning("FUCK 8");
			debuffPulsePrefab = scPrefabPrefab.InstantiateClone("DebuffAuraPrefab");
			TurboEdition._logger.LogWarning("FUCK 9");
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
			if (InventoryCount <= 0 || body.outOfCombat) //This is going to run constantly what the fuck is this ok?
			{
				if (component)
				{
#if DEBUG
					TurboEdition._logger.LogWarning("Player has either zero items or out of combat, destroying component.");
#endif
					UnityEngine.Object.Destroy(component);
				}
			}
			else
			{
				if (!component)
				{
#if DEBUG
					TurboEdition._logger.LogWarning("Player does not have a component, creating one.");
#endif
					fixedTime = Run.FixedTimeStamp.now;
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
				component.GetComponent<DebuffPulse>().netDuration = pulseDuration;
				component.GetComponent<DebuffPulse>().netStackPulse = stackPulse;
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

			[SyncVar]
			float duration;
			public float netDuration
			{
				get { return duration; }
				set { base.SetSyncVar<float>(value, ref duration, 1u); }
			}

			[SyncVar]
			float stackPulse;
			public float netStackPulse
			{
				get { return stackPulse; }
				set { base.SetSyncVar<float>(value, ref stackPulse, 1u); }
			}

			//pulse shit
			private readonly List<HurtBox> hurtBoxesList = new List<HurtBox>();
			public Transform pulseIndicator;
			private SphereSearch sphereSearch;
			private TeamMask enemyTeams;
			private float finalRadius;

			public GameObject owner;
			private float time;
			private float rate;

			//team shit
			private TeamFilter teamFilter;
			List<HealthComponent> debuffedTargets = new List<HealthComponent>();

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
			private void Awake()
            {
#if DEBUG
				TurboEdition._logger.LogWarning("WAKEY WAKEY.");
#endif
				teamFilter = GetComponent<TeamFilter>();
            }

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
			private void FixedUpdate()
			{
				if (NetworkServer.active)
				{
                    for (int i = 0; i < stackPulse; i++)
                    {
						ServerPulse(owner.GetComponent<TeamIndex>()); //is this how you do this?????
					}
					if (fixedTime.timeSince > duration)
					{
						UnityEngine.Object.Destroy(this);
					}
				}
			}

			//It really isnt tho? lol
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
			private void OnExit()
			{
				if (pulseIndicator)
				{
					pulseIndicator.gameObject.SetActive(false);
				}
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
			private void OnDestroy()
			{
#if DEBUG
				TurboEdition._logger.LogWarning("DESTROYING.");
#endif
				Destroy(pulseIndicator);
			}

			[Server]
			private void ServerPulse(TeamIndex teamIndex)
			{
#if DEBUG
				TurboEdition._logger.LogWarning("Server Pulse made.");
#endif
				sphereSearch = new SphereSearch
				{
					mask = LayerIndex.entityPrecise.mask,
					origin = owner.transform.position,
					queryTriggerInteraction = QueryTriggerInteraction.Collide,
					radius = 0f
				};
				finalRadius = radius;
				rate = 1f / duration;
				enemyTeams = TeamMask.GetEnemyTeams(teamIndex); //inb4 this will give issues with chaos and you will get affected by yourself
			}
			public void Update()
			{
				time += rate * Time.deltaTime;
				time = ((time > 1f) ? 1f : time);
				sphereSearch.radius = radius * novaRadiusCurve.Evaluate(time);
				sphereSearch.RefreshCandidates().FilterCandidatesByHurtBoxTeam(enemyTeams).FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes(hurtBoxesList);
				int i = 0;
				int count = hurtBoxesList.Count;
				while (i < count)
				{
					HealthComponent healthComponent = hurtBoxesList[i].healthComponent;
					if (!debuffedTargets.Contains(healthComponent))
					{
						debuffedTargets.Add(healthComponent);
						DebuffTarget(healthComponent.body);
					}
					i++;
				}
				hurtBoxesList.Clear();
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
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
			pulseDuration = config.Bind<float>("Item: " + ItemName, "Pulse duration", 4f, "Duration in seconds for the pulse, slower makes it last longer.").Value;
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
			var novaPulse = novaPrefab.transform.Find("PulseEffect");

			TurboEdition._logger.LogWarning("ST START");
			var scPrefabPrefab = new GameObject("ScreamPulsePrefabPrefab");
			TurboEdition._logger.LogWarning("ST 1");
			scPrefabPrefab.AddComponent<TeamFilter>().teamIndex = novaPrefab.GetComponent<TeamFilter>().teamIndex;
			
			TurboEdition._logger.LogWarning("Creating Particle System");

			//Lets remap texRampHealing.png because apparently Sphere and Donut uses them since they are meshRenderers
			//Thanks to TheMysticSword for this
			Color c1 = Color.red;
			Color c2 = Color.grey;

			Texture2D remapHealing = new Texture2D(256, 16, TextureFormat.ARGB32, false);
			remapHealing.wrapMode = TextureWrapMode.Clamp;
			remapHealing.filterMode = FilterMode.Bilinear;
			for (int x = 0; x < 110; x++) for (int y = 0; y < 16; y++) remapHealing.SetPixel(x, y, new Color(0f, 0f, 0f, 0f));
			for (int x = 0; x < 93; x++) for (int y = 0; y < 16; y++) remapHealing.SetPixel(110 + x, y, new Color(c1.r, c1.g, c1.b, c1.a * (71f / 255f)));
			for (int x = 0; x < 53; x++) for (int y = 0; y < 16; y++) remapHealing.SetPixel(203 + x, y, new Color(c2.r, c2.g, c2.b, c2.a * (151f / 255f)));
			remapHealing.Apply();

			//Particle system or something
			// BAD! var debuffMAIN = novaPulse.GetComponent<ParticleSystem>().main;

			TurboEdition._logger.LogWarning("Particle 1");
			var debuffPS = novaPulse.Find("Particle System").GetComponent<ParticleSystemRenderer>(); //This seems to work
			debuffPS.material.SetColor("_TintColor", new Color(1.5f, 1f, 1f, 1f));

			TurboEdition._logger.LogWarning("Particle 2");
			var debuffSPH = novaPulse.Find("Sphere").GetComponent<MeshRenderer>(); //This works too
			debuffSPH.material.SetTexture("_RemapTex", remapHealing);

			TurboEdition._logger.LogWarning("Particle 3");
			var debuffDONT = novaPulse.Find("Donut").GetComponent<MeshRenderer>(); //And this
			debuffDONT.material.SetTexture("_RemapTex", remapHealing);

			TurboEdition._logger.LogWarning("ST 3");
			//MAKES IT NOT WORK EITHER scPrefabPrefab.GetComponent<ParticleSystemRenderer>().material.SetVector("_TintColor", new Vector4(2.2f, 2f, 2f, 1.5f));
			//IT SAYS THIS IS PRIVATE WHY IS IT PRIVATE HOW THE HELL scPrefabPrefab.AddComponent<ParticleSystem>().main = novaPrefab.GetComponent<ParticleSystem>().main;
			TurboEdition._logger.LogWarning("ST 4");
			scPrefabPrefab.AddComponent<NetworkedBodyAttachment>().forceHostAuthority = true;

			TurboEdition._logger.LogWarning("ST 5");
			var sc = scPrefabPrefab.AddComponent<DebuffPulse>();
			TurboEdition._logger.LogWarning("ST 6");

			//sc.pulseIndicator = scPrefabPrefab.GetComponent<MeshRenderer>().transform;

			sc.interval = 1f;
			TurboEdition._logger.LogWarning("ST 7");
			debuffPulsePrefab = scPrefabPrefab.InstantiateClone("DebuffAuraPrefab");
			TurboEdition._logger.LogWarning("ST END");
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
					//fixedTime = Run.FixedTimeStamp.now; //I forgot why im using this

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

			//pulse shit
			private readonly List<HurtBox> hurtBoxesList = new List<HurtBox>();
			public Transform pulseIndicator;
			private SphereSearch sphereSearch;
			private TeamMask enemyTeams;
			private float finalRadius;

			public GameObject owner;
			private float lifeStopwatch;
			private float stopwatch;
			public float interval;

			private float time;
			private float rate;

			//team shit
			private TeamFilter teamFilter;
			List<HealthComponent> debuffedTargets = new List<HealthComponent>();

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
			private void Awake()
            {
#if DEBUG
				TurboEdition._logger.LogWarning("Waking up: " + this);
#endif
				stopwatch = 0f;
				lifeStopwatch = 0f;
				teamFilter = GetComponent<TeamFilter>();
				sphereSearch = GetComponent<SphereSearch>();
            }

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
			private void FixedUpdate()
			{
				stopwatch -= Time.fixedDeltaTime;
				lifeStopwatch += Time.fixedDeltaTime;

				if (lifeStopwatch > duration) Destroy(this.gameObject); //or is it just (this)?
				else if (stopwatch <= 0f)
				{
					stopwatch = interval;
					if (NetworkServer.active)
					{
						for (int i = 0; i < pulses; i++)
						{
							ServerPulse(teamFilter.teamIndex); //is this how you do this?????
						}
					}
				}
			}

			//It really isnt tho? lol
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
			private void OnExit()
			{
				if (pulseIndicator)
				{
					this.pulseIndicator.gameObject.SetActive(false);
				}
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
			private void OnDestroy()
			{
#if DEBUG
				TurboEdition._logger.LogWarning("OnDestroy: " + this);
#endif
				Destroy(this.pulseIndicator);
			}

			[Server]
			private void ServerPulse(TeamIndex teamIndex)
			{
#if DEBUG
				TurboEdition._logger.LogWarning("Server Pulse made.");
#endif
                this.sphereSearch = new RoR2.SphereSearch
				{
					mask = LayerIndex.entityPrecise.mask,						//This should be ok too
					origin = owner.transform.position,							//This should be ok
					queryTriggerInteraction = QueryTriggerInteraction.Collide,
					radius = 0f													//it starts with 0 radius, yea
				};
				this.finalRadius = radius;
				this.rate = 1f / duration;
				this.enemyTeams = TeamMask.GetEnemyTeams(teamIndex); //inb4 this will give issues with chaos and you will get affected by yourself
			}


			public void Update()
			{

				if (this.pulseIndicator)
				{
					float num = this.radius * novaRadiusCurve.Evaluate(Time.fixedDeltaTime / duration);
					this.pulseIndicator.localScale = new Vector3(num, num, num);
				}
				#if DEBUG
				TurboEdition._logger.LogWarning("Updating " + this);
				#endif

				this.time += this.rate * Time.deltaTime;
				this.time = ((this.time > 1f) ? 1f : this.time);

				#if DEBUG
				TurboEdition._logger.LogWarning("Updating sphereSearch radius, current time: " + time);
				#endif
				
				sphereSearch.radius = finalRadius * novaRadiusCurve.Evaluate(this.time); //NRE, NEEDS SOMETHING INSTANTIATED
				
				#if DEBUG
				TurboEdition._logger.LogWarning("Updating sphereSearch candidates, current radius: " + finalRadius);
				#endif
				sphereSearch.RefreshCandidates().FilterCandidatesByHurtBoxTeam(this.enemyTeams).OrderCandidatesByDistance().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes(this.hurtBoxesList);
				
				int i = 0;
				int count = this.hurtBoxesList.Count;
				while (i < count)
				{
					HealthComponent healthComponent = this.hurtBoxesList[i].healthComponent;
					if (!this.debuffedTargets.Contains(healthComponent))
					{
						#if DEBUG
						TurboEdition._logger.LogWarning("Debuffing somebody: " + healthComponent.body);
						#endif
						this.debuffedTargets.Add(healthComponent);
						this.DebuffTarget(healthComponent.body);
					}
					i++;
				}
				#if DEBUG
				TurboEdition._logger.LogWarning("Done debuffing, clearing hurtboxes list.");
				#endif
				this.hurtBoxesList.Clear();
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
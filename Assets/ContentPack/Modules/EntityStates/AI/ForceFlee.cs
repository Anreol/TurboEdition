using EntityStates.AI;
using EntityStates.AI.Walker;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Navigation;
using System.Collections.Generic;
using TurboEdition.Components;
using UnityEngine;

namespace TurboEdition.EntityStates.AI.Walker
{
    public class ForceFlee : BaseAIState
    {
        public float fleeDuration; //Fleeing duration, wont find a new enemy target and will move the opposite direction from the desired as long as this lasts.
        public bool numb = false;

        private Vector3? targetPosition;
        private Vector3 bodyFeetPos;

        private float aiUpdateTimer;
        private float lastPathUpdate;
        private float fallbackNodeStartAge;
        private readonly float fallbackNodeDuration = 4f;

        private BaseAI originalAI = null;
        private BaseAINumb numbAI = null;

        public override void OnEnter()
        {
            base.OnEnter();

            if (base.ai && base.body)
            {
                if ((base.ai.GetType() != typeof(BaseAINumb)) && numb)
                {
                    numbAI = outer.gameObject.AddComponent<BaseAINumb>();
                    originalAI = base.ai;
                    CloneData(originalAI, ref numbAI);
                    base.ai = numbAI;
                }
                BroadNavigationSystem.Agent broadNavigationAgent = base.ai.broadNavigationAgent;
                this.targetPosition = base.PickRandomNearbyReachablePosition(); //Let's use the default one at the start.
                if (this.targetPosition != null)
                {
                    broadNavigationAgent.goalPosition = new Vector3?(this.targetPosition.Value);
                    broadNavigationAgent.InvalidatePath();
                }
            }
            base.body.CallRpcBark();
            this.aiUpdateTimer = -1.0f; //Default is 0.5, but they seem to stand still when afflicted and take a bit til they react, so im making this zero
        }

        public override void OnExit()
        {
            base.OnExit();
            if (base.ai && numbAI != null)
            {
                base.ai = originalAI;
                Destroy(numbAI);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.ai && base.body)
            {
                //If AI is ready to enter combat again or be busy
                if (!body.HasBuff(BuffCatalog.GetBuffDef(BuffCatalog.FindBuffIndex("BuffPanicked"))))
                {
                    if (base.ai.skillDriverEvaluation.dominantSkillDriver)
                        this.outer.SetNextState(new Combat());
                    else
                        this.outer.SetNextState(new LookBusy());
                }
                this.fleeDuration -= Time.fixedDeltaTime;
                this.aiUpdateTimer -= Time.fixedDeltaTime;
                this.UpdateFootPosition();
                if (this.aiUpdateTimer <= 0f)
                {
                    this.UpdateAI(BaseAIState.cvAIUpdateInterval.value);
                    this.LoseAllFocus(fleeDuration, fleeDuration);
                    this.aiUpdateTimer = BaseAIState.cvAIUpdateInterval.value;
                }
            }
        }

        protected void UpdateAI(float deltaTime)
        {
            if (this.fallbackNodeStartAge + this.fallbackNodeDuration < base.fixedAge)
                base.ai.SetGoalPosition(this.targetPosition);

            BroadNavigationSystem.Agent broadNavigationAgent = base.ai.broadNavigationAgent;
            BroadNavigationSystem.AgentOutput output = broadNavigationAgent.output;

            Vector3 vector3 = base.bodyTransform.position;
            Vector3 a = output.nextPosition ?? this.bodyFeetPos;
            Vector3 vector4 = (a - this.bodyFeetPos).normalized * 10f;

            if (fleeDuration >= 0f && !body.isFlying) //Wisps tend to facepunch themselves into the ground, so yea
                vector3 -= vector4;

            base.ai.localNavigator.targetPosition = vector3;
            base.ai.localNavigator.allowWalkOffCliff = UnityEngine.Random.Range(-2, 1) >= 1; //1/4 chance to run off cliffs lol.
            base.ai.localNavigator.Update(deltaTime);

            if (base.bodyInputBank)
                this.bodyInputs.moveVector = base.ai.localNavigator.moveVector * 2f;

            if (output.lastPathUpdate > this.lastPathUpdate && !output.targetReachable && this.fallbackNodeStartAge + this.fallbackNodeDuration < base.fixedAge)
            {
                broadNavigationAgent.goalPosition = PickRandomNearbyReachablePositionInRange(10, 20);
                broadNavigationAgent.InvalidatePath();
            }
            this.lastPathUpdate = output.lastPathUpdate;
        }

        private void UpdateFootPosition()
        {
            this.bodyFeetPos = base.body.footPosition;
            BroadNavigationSystem.Agent broadNavigationAgent = base.ai.broadNavigationAgent;
            broadNavigationAgent.currentPosition = new Vector3?(this.bodyFeetPos);
        }

        public override BaseAI.BodyInputs GenerateBodyInputs(in BaseAI.BodyInputs previousBodyInputs)
        {
            this.bodyInputs.pressSprint = true; //Make it sprint, im pretty sure no AI normally sprints.
            base.AimInDirection(ref this.bodyInputs, this.bodyInputs.moveVector); //Make it aim in its direction no matter what.
            base.ModifyInputsForJumpIfNeccessary(ref this.bodyInputs); //A must if we want to make it jump
            return this.bodyInputs;
        }

        private void LoseAllFocus(float targetRefreshTimer, float busyTimer)
        {
            this.ai.currentEnemy.Reset(); //Reset Enemy Target
            this.ai.targetRefreshTimer = targetRefreshTimer; //Avoids searching for a new enemy until its done fleeing
            this.ai.enemyAttention = busyTimer; //Will distract the enemy with something else. Wont do much as we are resetting the current enemy anyways
                                                //logic goes as: on hurt -> if it doesnt have enemy or enemy has no attention -> enemy is now the one that dealt damage
                                                //AKA enemy attention is how long it will get fixated on the same target

            //enemies are set through: Doppleganger spawn (Enemy forcefully set to the originator), body damaged, teleporter boss (?).
            //Arena enemies will forcefully get the closest enemy available as target, ignoring range, view, and LoS

            //Enemies (Game Object) will ALWAYS be the health component, not the actual body.
            //To target something else, use CustomTarget, only used by the emergency drone. The target type of the ai skill driver will have to be changed.
            //Target can be probably forcefully set as a CustomTarget.
        }

        //dumb as fuck but who cares, playing safe.
        private void CloneData(BaseAI bai, ref BaseAINumb bain)
        {
            bain.aimVectorDampTime = bai.aimVectorDampTime;
            bain.aimVectorMaxSpeed = bai.aimVectorMaxSpeed;
            bain.aimVelocity = bai.aimVelocity;
            bain.body = bai.body;
            bain.bodyCharacterDirection = bai.bodyCharacterDirection;
            bain.bodyCharacterMotor = bai.bodyCharacterMotor;
            bain.bodyHealthComponent = bai.bodyHealthComponent;
            bain.bodyInputBank = bai.bodyInputBank;
            bain.bodyInputs = bai.bodyInputs;
            bain.bodySkillLocator = bai.bodySkillLocator;
            bain.bodyTransform = bai.bodyTransform;
            bain._broadNavigationAgent = bai.broadNavigationAgent;
            bain.broadNavigationSystem = bai.broadNavigationSystem;
            bain.buddy = bai.buddy;
            bain.buddySearch = bai.buddySearch;
            bain.currentEnemy = bai.currentEnemy;
            bain.customTarget = bai.customTarget;
            bain.debugEnemyHurtBox = bai.debugEnemyHurtBox;
            bain.desiredSpawnNodeGraphType = bai.desiredSpawnNodeGraphType;
            bain.enabled = bai.enabled;
            bain.enemyAttention = bai.enemyAttention;
            bain.enemyAttentionDuration = bai.enemyAttentionDuration;
            bain.enemySearch = bai.enemySearch;
            bain.fullVision = bai.fullVision;
            bain.hasAimConfirmation = bai.hasAimConfirmation;
            bain.hideFlags = bai.hideFlags;
            //bain.isActiveAndEnabled = bai.isActiveAndEnabled;
            bain.isHealer = bai.isHealer;
            bain.leader = bai.leader;
            bain.localNavigator = bai.localNavigator;
            bain.master = bai.master;
            //bain.networkIdentity = bai.networkIdentity; set on awake, should be fine
            bain.neverRetaliateFriendlies = bai.neverRetaliateFriendlies;
            bain.scanState = bai.scanState;
            bain.selectedSkilldriverName = bai.selectedSkilldriverName;
            bain.skillDriverEvaluation = bai.skillDriverEvaluation;
            bain.skillDrivers = bai.skillDrivers;
            bain.skillDriverUpdateTimer = bai.skillDriverUpdateTimer;
            bain.stateMachine = bai.stateMachine;
            bain.targetRefreshTimer = bai.targetRefreshTimer;
        }

        protected Vector3? PickRandomNearbyReachablePositionInRange(float minRange, float maxRange, int nodeCount = 6)
        {
            if (!this.ai || !this.body)
            {
                return null;
            }
            NodeGraph nodeGraph = SceneInfo.instance.GetNodeGraph(this.body.isFlying ? MapNodeGroup.GraphType.Air : MapNodeGroup.GraphType.Ground);
            NodeGraphSpider nodeGraphSpider = new NodeGraphSpider(nodeGraph, (HullMask)(1 << (int)this.body.hullClassification));
            List<NodeGraph.NodeIndex> nodeList = nodeGraph.FindNodesInRange(this.bodyTransform.position, minRange, maxRange, (HullMask)(1 << (int)this.body.hullClassification));
            int count = Mathf.Min(nodeList.Count, nodeCount);
            for (int i = 0; i < count; i++)
            {
                nodeGraphSpider.AddNodeForNextStep(nodeList[i]);
            }
            for (int i = 0; i < count; i++)
            {
                nodeGraphSpider.PerformStep(); //Transforms nodes into collected steps. Does a bunch of shit like hull checking and getting link end nodes
            }
            List<NodeGraphSpider.StepInfo> collectedSteps = nodeGraphSpider.collectedSteps;
            if (collectedSteps.Count == 0)
                return null;
            int index = UnityEngine.Random.Range(0, collectedSteps.Count);
            NodeGraph.NodeIndex node = collectedSteps[index].node;
            Vector3 value;
            if (nodeGraph.GetNodePosition(node, out value))
                return new Vector3?(value);
            return null;
        }
    }
}
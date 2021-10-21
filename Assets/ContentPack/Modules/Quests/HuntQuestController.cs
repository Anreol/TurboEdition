using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using RoR2;
using RoR2.UI;
using TurboEdition.Quests;
using QuestDef = TurboEdition.ScriptableObjects.QuestDef;
using System.Runtime.InteropServices;

namespace TurboEdition.Quests
{
    class HuntQuestController : NetworkBehaviour
    {
        public static event Action onInstanceChangedGlobal;
        public int numCurrentCount
        {
            get
            {
                return this._numCurrentCount;
            }
        }
        public int numRequiredCount
        {
            get
            {
                return this._numRequiredCount;
            }
        }
        public string objectiveToken
        {
            get
            {
                return this._objectiveToken;
            }
        }
        private Xoroshiro128Plus rng;
        private TeamIndex teamIndex = TeamIndex.None;
        private NetworkUser networkUserOrigin;
        private QuestDef questDefSpawner;
        private void OnEnable()
        {
            InstanceTracker.Add(this);
            Action action = HuntQuestController.onInstanceChangedGlobal;
            if (action != null)
            {
                action();
            }
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            ObjectivePanelController.collectObjectiveSources += ObjectivePanelController_collectObjectiveSources;
        }

        private void OnDisable()
        {
            InstanceTracker.Remove(this);
            Action action = HuntQuestController.onInstanceChangedGlobal;
            if (action != null)
            {
                action();
            }
            GlobalEventManager.onCharacterDeathGlobal -= GlobalEventManager_onCharacterDeathGlobal;
            
        }
        [Server]
        public override void OnStartServer()
        {
            if (!NetworkServer.active)
            {
                TELog.LogW("[Server] function 'System.Void TurboEdition.QuestController::OnStartServer()' called on client");
                return;
            }
            base.OnStartServer();
            this.rng = new Xoroshiro128Plus((ulong)Run.instance.stageRng.nextUint);
        }        
        private void ObjectivePanelController_collectObjectiveSources(CharacterMaster arg1, List<ObjectivePanelController.ObjectiveSourceDescriptor> arg2)
        {
            if (teamIndex != TeamIndex.None && arg1.teamIndex == teamIndex)
            {
                arg2.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
                {
                    master = arg1,
                    objectiveType = typeof(MoonBatteryMissionObjectiveTracker),
                    source = this
                });
            }
            else if (arg1 == networkUserOrigin.master)
            {
                arg2.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
                {
                    master = arg1,
                    objectiveType = typeof(MoonBatteryMissionObjectiveTracker),
                    source = this
                });
            }
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport obj)
        {
            throw new NotImplementedException();
        }

        public int Network_numCurrentCount
        {
            get
            {
                return this._numCurrentCount;
            }
            [param: In]
            set
            {
                base.SetSyncVar<int>(value, ref this._numCurrentCount, 1U);
            }
        }
        public int Network_numRequiredCount
        {
            get
            {
                return this._numRequiredCount;
            }
            [param: In]
            set
            {
                base.SetSyncVar<int>(value, ref this._numRequiredCount, 2U);
            }
        }
        public string Network_objectiveToken
        {
            get
            {
                return this._objectiveToken;
            }
            [param: In]
            set
            {
                base.SetSyncVar<string>(value, ref this._objectiveToken, 4U);
            }
        }
        public override bool OnSerialize(NetworkWriter writer, bool forceAll)
        {
            if (forceAll)
            {
                writer.WritePackedUInt32((uint)this._numCurrentCount);
                writer.WritePackedUInt32((uint)this._numRequiredCount);
                writer.Write((string)this._objectiveToken);
                return true;
            }
            bool flag = false;
            if ((base.syncVarDirtyBits & 1U) != 0U)
            {
                if (!flag)
                {
                    writer.WritePackedUInt32(base.syncVarDirtyBits);
                    flag = true;
                }
                writer.WritePackedUInt32((uint)this._numCurrentCount);
            }
            if ((base.syncVarDirtyBits & 2U) != 0U)
            {
                if (!flag)
                {
                    writer.WritePackedUInt32(base.syncVarDirtyBits);
                    flag = true;
                }
                writer.WritePackedUInt32((uint)this._numRequiredCount);
            }
            if ((base.syncVarDirtyBits & 4U) != 0U)
            {
                if (!flag)
                {
                    writer.WritePackedUInt32(base.syncVarDirtyBits);
                    flag = true;
                }
                writer.Write((string)this._objectiveToken);
            }
            if (!flag)
            {
                writer.WritePackedUInt32(base.syncVarDirtyBits);
            }
            return flag;
        }
        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (initialState)
            {
                this._numCurrentCount = (int)reader.ReadPackedUInt32();
                this._numRequiredCount = (int)reader.ReadPackedUInt32();
                this._objectiveToken = (string)reader.ReadString();
                return;
            }
            int num = (int)reader.ReadPackedUInt32();
            if ((num & 1) != 0)
            {
                this._numCurrentCount = (int)reader.ReadPackedUInt32();
            }
            if ((num & 2) != 0)
            {
                this._numRequiredCount = (int)reader.ReadPackedUInt32();
            }
            if ((num & 4) != 0)
            {
                this._objectiveToken = (string)reader.ReadString();
            }
        }

        [SyncVar]
        private int _numCurrentCount;
        [SyncVar]
        private int _numRequiredCount;
        [SyncVar]
        private string _objectiveToken;
    }
}

public class HuntQuestObjectiveTracker : ObjectivePanelController.ObjectiveTracker
{
    public override string GenerateString()
    {
        HuntQuestController huntQuestController = (HuntQuestController)this.sourceDescriptor.source;
        this.numCurrentCount = huntQuestController.numCurrentCount;
        return string.Format(Language.GetString(huntQuestController.objectiveToken), this.numCurrentCount, huntQuestController.numRequiredBatteries);
    }

    public override bool IsDirty()
    {
        return ((HuntQuestController)this.sourceDescriptor.source).numChargedBatteries != this.numCurrentCount;
    }
    private int numCurrentCount = -1;
}
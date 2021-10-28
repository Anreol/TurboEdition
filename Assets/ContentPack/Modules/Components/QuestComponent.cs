using RoR2;
using System;
using TurboEdition.Quests;
using UnityEngine.Networking;

namespace TurboEdition.Components
{
    public class QuestComponent : NetworkBehaviour
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

        public QuestCatalog.QuestIndex questIndexSpawner
        {
            get
            {
                return (QuestCatalog.QuestIndex)this._questIndexSpawner;
            }
            set
            {
                this._questIndexSpawner = (int)value;
            }
        }

        public NetworkInstanceId masterNetIdOrigin
        {
            get
            {
                return this._masterNetIdOrigin;
            }
            set
            {
                this._masterNetIdOrigin = value;
            }
        }

        public string titleToken
        {
            get
            {
                return QuestCatalog.GetQuestDef((QuestCatalog.QuestIndex)this._questIndexSpawner).nameToken;
            }
        }

        public string objectiveToken
        {
            get
            {
                return QuestCatalog.GetQuestDef((QuestCatalog.QuestIndex)this._questIndexSpawner).objectiveToken;
            }
        }

        public string loseToken
        {
            get
            {
                return QuestCatalog.GetQuestDef((QuestCatalog.QuestIndex)this._questIndexSpawner).loseToken;
            }
        }

        public virtual void OnEnable()
        {
            InstanceTracker.Add(this);
            Action action = QuestComponent.onInstanceChangedGlobal;
            if (action != null)
            {
                action();
            }
        }

        public virtual void OnDisable()
        {
            InstanceTracker.Remove(this);
            Action action = QuestComponent.onInstanceChangedGlobal;
            if (action != null)
            {
                action();
            }
        }

        [Server]
        public override void OnStartServer()
        {
            if (!NetworkServer.active)
            {
                TELog.LogW("[Server] function 'System.Void TurboEdition.QuestComponent::OnStartServer()' called on client");
                return;
            }
            base.OnStartServer();
            this.rng = new Xoroshiro128Plus((ulong)Run.instance.stageRng.nextUint);
        }

        [SyncVar]
        private int _numCurrentCount;

        [SyncVar]
        private int _numRequiredCount;

        [SyncVar]
        private int _questIndexSpawner;

        [SyncVar]
        private NetworkInstanceId _masterNetIdOrigin;

        public Xoroshiro128Plus rng;
        public TeamIndex teamIndex = TeamIndex.None;
    }
}
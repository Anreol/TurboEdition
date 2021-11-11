using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace TurboEdition.Components
{
    class QuestMissionController : NetworkBehaviour
    {
        private Xoroshiro128Plus rng;
        private EntityStateMachine[] questStateMachines;
        public static QuestMissionController instance { get; private set; }
        private void Awake()
        {
            this.questStateMachines = new EntityStateMachine[0];
        }
        private void OnEnable()
        {
            QuestMissionController.instance = SingletonHelper.Assign<QuestMissionController>(QuestMissionController.instance, this);

        }
        private void OnDisable()
        {
            QuestMissionController.instance = SingletonHelper.Unassign<QuestMissionController>(QuestMissionController.instance, this);
        }

        [Server]
        public override void OnStartServer()
        {
            base.OnStartServer();
            this.rng = new Xoroshiro128Plus((ulong)Run.instance.stageRng.nextUint);
        }
    }
}

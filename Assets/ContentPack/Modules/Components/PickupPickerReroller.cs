using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Components
{
    public class PickupPickerReroller : NetworkBehaviour
    {
        [Tooltip("Picker to be rerolled")]
        [SerializeField]
        protected PickupPickerController pickupPickerController;

        [Tooltip("The drop table to use when rerolling.")]
        [SerializeField]
        protected PickupDropTable rerollDropTable;

        public bool inheritOptionsCount;

        [Tooltip("Number of options to use after a reroll as base. Set to -1 or less to inherit.")]
        public int rerollBaseOptionsCount;

        [Tooltip("Should rerolling increase or decrease the amount of options and by what amount.")]
        public int rerollOptionsCountChange;

        [SyncVar(hook = "OnClientReRollRequested")]
        public bool clientRequestedReRoll;

        private Xoroshiro128Plus rng;
        private int timesRerolled;
        private PickupPickerController.Option[] currentServerOptions;
        public void Start()
        {
            rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
        }
        public void OnEnable()
        {
            if (rerollBaseOptionsCount <= -1)
            {
                rerollBaseOptionsCount = pickupPickerController.options.Length;
            }
        }

        private void GenerateServerOptions()
        {
            currentServerOptions = PickupPickerController.GenerateOptionsFromDropTable(rerollBaseOptionsCount + (rerollOptionsCountChange * timesRerolled), this.rerollDropTable, new Xoroshiro128Plus((ulong)timesRerolled * rng.nextUlong));
        }

        /// <summary>
        /// Automatically called when clientRequestedReRoll changes
        /// </summary>
        /// <param name="newValue">Sent by the SyncVar</param>
        public void OnClientReRollRequested(bool newValue)
        {
            if (newValue != clientRequestedReRoll)
            {
                RerollPickupPicker();
            }
        }

        /// <summary>
        /// Workaround so the client can call reroll on the server
        /// </summary>
        public void ReRollWithCurrentServerOptions()
        {
            if (NetworkServer.active)
            {
                RerollPickupPicker();
                return;
            }
            clientRequestedReRoll = !clientRequestedReRoll;
        }

        [Server]
        private void RerollPickupPicker()
        {
            timesRerolled++;
            pickupPickerController.SetOptionsServer(currentServerOptions);
        }

        /// <summary>
        /// For generating new options.
        /// </summary>
        public void GenerateNewOptions()
        {
            if (NetworkServer.active)
            {
                GenerateServerOptions();
                return;
            }
            CmdGenerateNewOptionsServer();
        }

        [Command]
        private void CmdGenerateNewOptionsServer()
        {
            GenerateServerOptions();
        }
    }
}
using RoR2;
using TurboEdition.Components.UI;
using TurboEdition.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Components
{
    public class PickupPickerRerollerController : NetworkBehaviour
    {
        [Tooltip("Picker to be rerolled")]
        [SerializeField] protected PickupPickerController pickupPickerController;

        [Tooltip("The drop table to use when rerolling.")]
        [SerializeField] protected PickupDropTable rerollDropTable;

        [Tooltip("Number of times to reroll.")]
        public int rerollMaxTimes;

        [Tooltip("Number of options to use after a reroll as base. Set to -1 or less to inherit.")]
        public int rerollBaseOptionsCount;

        [Tooltip("Should rerolling increase or decrease the amount of options and by the multiplied amount of timesRerolled.")]
        public int rerollOptionsCountMult;

        [SyncVar(hook = "OnTimesRerolledChanged")]
        internal int timesRerolled;

        private Xoroshiro128Plus rng;
        private PickupPickerController.Option[] currentServerOptions;
        private NetworkUIPromptController networkUIPromptController;
        private PickupPickerPanelReroller panelInstanceController;
        private int optionCount => rerollBaseOptionsCount + (rerollOptionsCountMult * timesRerolled);

        public void Start()
        {
            if (pickupPickerController == null)
            {
                TELog.LogE("PickupPickerRerollerController instance has no pickupPickerController, destroying self.", true);
                Destroy(this);
            }

            rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
            if (rerollBaseOptionsCount <= -1)
            {
                rerollBaseOptionsCount = pickupPickerController.options.Length;
            }

            networkUIPromptController = GetComponent<NetworkUIPromptController>();
            if (NetworkClient.active)
            {
                networkUIPromptController.onDisplayBegin += OnDisplayBegin;
                networkUIPromptController.onDisplayEnd += OnDisplayEnd;
            }
        }

        private void OnDisplayBegin(NetworkUIPromptController arg1, LocalUser arg2, CameraRigController arg3)
        {
            if (!pickupPickerController.panelInstance)
            {
                TELog.LogE("PickupPickerRerollerControllerCannot find the pickupPickerController panel instance, is this okay?!.", true);
            }
            panelInstanceController = pickupPickerController.panelInstance.GetComponent<PickupPickerPanelReroller>();
            panelInstanceController.pickupPickerRerollerController = this;
        }

        private void OnDisplayEnd(NetworkUIPromptController arg1, LocalUser arg2, CameraRigController arg3)
        {
            panelInstanceController = null;
        }

        private void OnEnable()
        {
            InstanceTracker.Add<PickupPickerRerollerController>(this);
        }

        private void OnDisable()
        {
            InstanceTracker.Remove<PickupPickerRerollerController>(this);
        }

        private void GenerateServerOptions()
        {
            if (rerollDropTable == null)
            {
                currentServerOptions = TurboUtils.GetOptionsFromPickupIndex(optionCount, pickupPickerController.options[UnityEngine.Random.Range(0, pickupPickerController.options.Length)].pickupIndex, new Xoroshiro128Plus((ulong)timesRerolled * rng.nextUlong));
            }
            currentServerOptions = PickupPickerController.GenerateOptionsFromDropTable(optionCount, this.rerollDropTable, new Xoroshiro128Plus((ulong)timesRerolled * rng.nextUlong));
        }

        /// <summary>
        /// Automatically called when clientRequestedReRoll changes
        /// </summary>
        /// <param name="newInt">Sent by the SyncVar</param>
        private void OnTimesRerolledChanged(int newInt)
        {
            if (NetworkServer.active)
            {
                SetNewOptionsServer();
            }
        }

        /// <summary>
        /// Workaround so the client can call reroll on the server, this is the method that gets called through unity events.
        /// </summary>
        public void SetWithCurrentServerOptions()
        {
            if (NetworkServer.active)
            {
                timesRerolled++;
                SetNewOptionsServer();
                return;
            }
            //I've wrote this code like a year ago by now... sure this is okay, in the client?
            timesRerolled++;
        }

        [Server]
        private void SetNewOptionsServer()
        {
            pickupPickerController.SetOptionsServer(currentServerOptions);
        }

        /// <summary>
        /// For generating new options. Can be called by client or server.
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

        private void OnValidate()
        {
            if (!pickupPickerController)
            {
                Debug.LogError("PickupPickerRerolledController MUST have a pickupPickerController assigned!", gameObject);
            }
        }
    }
}
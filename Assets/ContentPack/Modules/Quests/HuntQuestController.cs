using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using RoR2;

namespace TurboEdition.Quests
{
    class HuntQuestController : NetworkBehaviour
    {
        private void OnEnable()
        {
            InstanceTracker<HuntQuestController>()
        }
    }
}

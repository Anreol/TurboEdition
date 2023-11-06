using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TurboEdition.Components
{
    internal class ObservatoryEventController
    {
        public GameObject rainParticleGameObject;
        public GameObject thunderMeshGameObject;

        public string wwiseThunderPlayEvent;

        private uint wwiseThunderID;

        public void OnEnable()
        {
            //For Thunder, play the sound event.
           // wwiseThunderID = AkSoundEngine.PostEvent(wwiseThunderPlayEvent, )
        }
        public void Update()
        {
            //Check if the RCTP value has been changed in WWise

            //Do things if the RCTP value has changed
            RainStart();
        }

        public void OnDisable()
        {
            if (wwiseThunderID != 0)
            {
                AkSoundEngine.StopPlayingID(wwiseThunderID);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RainStart()
        {

        }

        /// <summary>
        /// Makes the <see cref="thunderMeshGameObject"/> appear and disappear. Meant to be called as a callback from a Wwise event.
        /// </summary>
        public void ThunderFlash()
        {
            //Get the mesh and change the alpha of the material rapidly
        }
    }
}

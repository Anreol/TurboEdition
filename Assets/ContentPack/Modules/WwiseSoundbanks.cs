using RoR2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TurboEdition
{
    public static class WwiseSoundbanks
    {
        public static string soundBankDirectory => System.IO.Path.Combine(Assets.assemblyDir, "soundbanks");

        [RoR2.SystemInitializer] //look at putting it in FinalizeAsync
        public static void Init()
        {
            uint akBankID;  // Not used. These banks can be unloaded with their file name.
            AkSoundEngine.AddBasePath(soundBankDirectory);
            AkSoundEngine.LoadBank("TurboInit.bnk", out akBankID);
            AkSoundEngine.LoadBank("TurboBank.bnk", out akBankID);        }
    }
}

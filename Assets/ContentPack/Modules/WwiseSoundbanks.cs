using RoR2;

namespace TurboEdition
{
    public static class WwiseSoundbanks
    {
        public static string soundBankDirectory => System.IO.Path.Combine(Assets.assemblyDir, "soundbanks");

        [SystemInitializer]
        private static void Init()
        {
            uint akBankID;  // Not used. These banks can be unloaded with their file name.
            AkSoundEngine.AddBasePath(soundBankDirectory);
            AkSoundEngine.LoadBank("TurboInit", out akBankID);
            AkSoundEngine.LoadBank("TurboBank", out akBankID);

            //Music
            AkBankManager.LoadBank("TurboMusicBank", false, false);
        }
    }
}
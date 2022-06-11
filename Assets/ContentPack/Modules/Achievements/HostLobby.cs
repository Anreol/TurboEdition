using RoR2;
using RoR2.Achievements;
using RoR2.Networking;
using UnityEngine.Networking;

namespace TurboEdition.Achievements
{
    //Achievements MUST be public classes
    [RegisterAchievement("HostLobby", "Items.Hitlag", "CompleteThreeStages", null)]
    public class HostLobby : BaseAchievement
    {
        public override void OnInstall()
        {
            base.OnInstall();
            //PlatformSystems.lobbyManager.onLobbyJoined = (Action<bool>)Delegate.Combine(PlatformSystems.lobbyManager.onLobbyJoined, new Action<bool>(OnLobbyJoined));
            NetworkManagerSystem.onStartHostGlobal += onStartHostGlobal;
        }

        public override void OnUninstall()
        {
            base.OnUninstall();
            //PlatformSystems.lobbyManager.onLobbyJoined = (Action<bool>)Delegate.Remove(PlatformSystems.lobbyManager.onLobbyJoined, new Action<bool>(OnLobbyJoined));
            NetworkManagerSystem.onStartHostGlobal -= onStartHostGlobal;
        }

        private void onStartHostGlobal()
        {
            if (!NetworkServer.dontListen)
            {
                base.Grant();
            }
        }
    }
}
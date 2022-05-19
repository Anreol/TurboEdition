using RoR2;
using RoR2.Achievements;
using RoR2.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace TurboEdition.Achievements
{
    [RegisterAchievement("HostLobby", "Items.Hitlag", "CompleteThreeStages", null)]
    class HostLobby : BaseAchievement
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

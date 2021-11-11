using EntityStates;
using RoR2.Skills;
using TurboEdition.Components;
using UnityEngine.Networking;

namespace TurboEdition.EntityStates.Quests
{
    class BaseQuestState : EntityState
    {
        private NetworkInstanceId _masterNetIdOrigin;

        protected QuestMissionController questMissionController
		{
			get
			{
				return QuestMissionController.instance;
			}
		}
        public NetworkInstanceId masterNetIdOrigin //Could also use CharacterMaster..?
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
    }
}

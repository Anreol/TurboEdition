using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using TurboEdition.Quests;
using QuestIndex = TurboEdition.Quests.QuestCatalog.QuestIndex;
using RoR2.Networking;
using RoR2;

namespace TurboEdition.Components
{
    class QuestTracker : NetworkBehaviour
    {
        public event Action onQuestsChanged;
		public event Action onQuestComplete;
        public event Action onQuestFailed;
        public event Action onQuestExpired;

		public static event Action<QuestTracker, QuestIndex, int> onServerQuestGiven;
		public event Action<QuestIndex> onQuestAddedClient;
		private bool spawnedOverNetwork
		{
			get
			{
				return base.isServer;
			}
		}

		[Server]
		public void GiveQuestString(string questString)
		{
			if (!NetworkServer.active)
			{
				TELog.LogW("[Server] function 'System.Void TurboEdition.QuestTracker::GiveQuestString(System.String)' called on client");
				return;
			}
			this.GiveQuest(QuestCatalog.FindQuestIndex(questString), 1);
		}

		[Server]
		public void GiveQuestString(string questString, int count)
		{
			if (!NetworkServer.active)
			{
				TELog.LogW("[Server] function 'System.Void TurboEdition.QuestTracker::GiveQuestString(System.String,System.Int32)' called on client");
				return;
			}
			this.GiveQuest(QuestCatalog.FindQuestIndex(questString), count);
		}

		[Server]
		public void GiveQuest(QuestCatalog.QuestIndex globalQuestIndex, int count = 1)
		{
			if (!NetworkServer.active)
			{
				TELog.LogW("[Server] function 'System.Void TurboEdition.QuestTracker::GiveQuest(TurboEdition.QuestIndex,System.Int32)' called on client");
				return;
			}
			if ((ulong)globalQuestIndex >= (ulong)((long)QuestCatalog.questCount))
			{
				return; //Quest isnt registered, bye bye! Yes I copied this from Inventory LOL
			}
			if (count <= 0)
			{
				if (count < 0)
				{
					this.RemoveItem(globalQuestIndex, -count);
				}
				return;
			}
			base.SetDirtyBit(1U);
			if ((this.questStacks[(int)globalQuestIndex] += count) == count)
			{
				this.questAcquisitionOrder.Add(globalQuestIndex);
				base.SetDirtyBit(8U);
			}
			Action action = this.onQuestsChanged;
			if (action != null)
			{
				action();
			}
			Action<QuestTracker, QuestIndex, int> action2 = QuestTracker.onServerQuestGiven;
			if (action2 != null)
			{
				action2(this, globalQuestIndex, count);
			}
			if (this.spawnedOverNetwork)
			{
				this.CallRpcQuestAdded(globalQuestIndex);
			}
		}
		[ClientRpc]
		private void RpcQuestAdded(QuestIndex questIndex)
		{
			Action<QuestIndex> action = this.onQuestAddedClient;
			if (action == null)
			{
				return;
			}
			action(questIndex);
		}
		[Server]
		public void RemoveItem(QuestIndex questIndex, int count = 1)
		{
			if (!NetworkServer.active)
			{
				TELog.LogW("[Server] function 'System.Void RoR2.Inventory::RemoveItem(RoR2.ItemIndex,System.Int32)' called on client");
				return;
			}
			if ((ulong)questIndex >= (ulong)((long)this.questStacks.Length))
			{
				return;
			}
			if (count <= 0)
			{
				if (count < 0)
				{
					this.GiveQuest(questIndex, -count);
				}
				return;
			}
			int num = this.questStacks[(int)questIndex];
			count = Math.Min(count, num);
			if (count == 0)
			{
				return;
			}
			if ((this.questStacks[(int)questIndex] = num - count) == 0)
			{
				this.questAcquisitionOrder.Remove(questIndex);
				base.SetDirtyBit(8U);
			}
			base.SetDirtyBit(1U);
			Action action = this.onQuestsChanged;
			if (action == null)
			{
				return;
			}
			action();
		}
		public void WriteItemStacks(int[] output)
		{
			Array.Copy(this.questStacks, output, output.Length);
		}
		public override int GetNetworkChannel()
		{
			return QosChannelIndex.defaultReliable.intVal;
		}
		public override void OnDeserialize(NetworkReader reader, bool initialState)
		{
			byte b = reader.ReadByte();
			bool flag = (b & 1) > 0;
			bool flag2 = (b & 4) > 0;
			if (flag)
			{
                reader.ReadItemStacks(this.questStacks);
			}
			if (flag2)
			{
				byte b2 = reader.ReadByte();
				this.questAcquisitionOrder.Clear();
				this.questAcquisitionOrder.Capacity = (int)b2;
				for (byte b3 = 0; b3 < b2; b3 += 1)
				{
					QuestIndex item = (QuestIndex)reader.ReadByte();
					this.questAcquisitionOrder.Add(item);
				}
			}
			if (flag || flag2)
			{
				Action action = this.onQuestsChanged;
				if (action == null)
				{
					return;
				}
				action();
			}
		}
		public override bool OnSerialize(NetworkWriter writer, bool initialState)
		{
			uint num = base.syncVarDirtyBits;
			if (initialState)
			{
				num = 29U;
			}
			bool flag = (num & 1U) > 0U;
			bool flag2 = (num & 4U) > 0U;
			writer.Write((byte)num);
			if (flag)
			{
				writer.WriteItemStacks(this.questStacks);
			}
			if (flag2)
			{
				byte b = (byte)this.questAcquisitionOrder.Count;
				writer.Write(b);
				for (byte b2 = 0; b2 < b; b2 += 1)
				{
					writer.Write((byte)this.questAcquisitionOrder[(int)b2]);
				}
			}
			return !initialState && num > 0U;
		}
		static QuestTracker()
		{
			NetworkBehaviour.RegisterRpcDelegate(typeof(QuestTracker), QuestTracker.kRpcRpcQuestAdded, new NetworkBehaviour.CmdDelegate(QuestTracker.InvokeRpcRpcQuestAdded));
			NetworkCRC.RegisterBehaviour("QuestTracker", 0);

		}
		protected static void InvokeRpcRpcQuestAdded(NetworkBehaviour obj, NetworkReader reader)
		{
			if (!NetworkClient.active)
			{
				TELog.LogE("RPC RpcQuestAdded called on server.");
				return;
			}
			((QuestTracker)obj).RpcQuestAdded((QuestIndex)reader.ReadInt32());
		}

		public void CallRpcQuestAdded(QuestIndex questIndex)
		{
			if (!NetworkServer.active)
			{
				TELog.LogE("RPC Function CallRpcQuestAdded called on client.");
				return;
			}
			NetworkWriter networkWriter = new NetworkWriter();
			networkWriter.Write(0);
			networkWriter.Write((short)((ushort)2));
			networkWriter.WritePackedUInt32((uint)QuestTracker.kRpcRpcQuestAdded);
			networkWriter.Write(base.GetComponent<NetworkIdentity>().netId);
			networkWriter.Write((int)questIndex);
			this.SendRPCInternal(networkWriter, 0, "RpcItemAdded");

		}
		private int[] questStacks = QuestCatalog.RequestQuestStackArray();
		public readonly List<QuestIndex> questAcquisitionOrder = new List<QuestIndex>();
		private static int kRpcRpcQuestAdded = 1978705788;  //I have no fucking idea what this is
		//Bits: 1U - questStacks
		//Bits: 4U - questOrder
	}
}

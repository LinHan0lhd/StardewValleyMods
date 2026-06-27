using System;
using System.Collections.Generic;
using System.IO;
using Netcode;
using StardewValley.Network;

namespace StardewValley
{
	// Token: 0x020000E6 RID: 230
	public abstract class NetSynchronizer
	{
		// Token: 0x06001161 RID: 4449 RVA: 0x000CB78F File Offset: 0x000C998F
		protected void reset()
		{
			this.variables.Clear();
			this.barriers.Clear();
		}

		// Token: 0x06001162 RID: 4450 RVA: 0x000CB7A8 File Offset: 0x000C99A8
		private HashSet<long> barrierPlayers(string name)
		{
			HashSet<long> barrier;
			if (!this.barriers.TryGetValue(name, out barrier))
			{
				barrier = (this.barriers[name] = new HashSet<long>());
			}
			return barrier;
		}

		// Token: 0x06001163 RID: 4451 RVA: 0x000CB7DC File Offset: 0x000C99DC
		private bool barrierReady(string name)
		{
			HashSet<long> playersReady = this.barrierPlayers(name);
			foreach (long id in Game1.otherFarmers.Keys)
			{
				if (!playersReady.Contains(id))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06001164 RID: 4452 RVA: 0x000CB840 File Offset: 0x000C9A40
		protected bool shouldAbort()
		{
			return Game1.client != null && Game1.client.timedOut;
		}

		// Token: 0x06001165 RID: 4453 RVA: 0x000CB858 File Offset: 0x000C9A58
		public void barrier(string name)
		{
			this.barrierPlayers(name).Add(Game1.player.UniqueMultiplayerID);
			Game1.multiplayer.UpdateLate(false);
			this.sendMessage(new object[]
			{
				1,
				name
			});
			while (!this.barrierReady(name))
			{
				this.processMessages();
				if (this.shouldAbort())
				{
					throw new AbortNetSynchronizerException();
				}
				if (LocalMultiplayer.IsLocalMultiplayer(false))
				{
					return;
				}
			}
			Game1.hooks.AfterNewDayBarrier(name);
		}

		// Token: 0x06001166 RID: 4454 RVA: 0x000CB8D3 File Offset: 0x000C9AD3
		public bool isBarrierReady(string name)
		{
			if (this.barrierReady(name))
			{
				return true;
			}
			this.processMessages();
			if (this.shouldAbort())
			{
				throw new AbortNetSynchronizerException();
			}
			return false;
		}

		// Token: 0x06001167 RID: 4455 RVA: 0x000CB8F5 File Offset: 0x000C9AF5
		public bool isVarReady(string varName)
		{
			if (this.variables.ContainsKey(varName))
			{
				return true;
			}
			this.processMessages();
			if (this.shouldAbort())
			{
				throw new AbortNetSynchronizerException();
			}
			LocalMultiplayer.IsLocalMultiplayer(false);
			return false;
		}

		// Token: 0x06001168 RID: 4456 RVA: 0x000CB928 File Offset: 0x000C9B28
		public T waitForVar<TField, T>(string varName) where TField : NetFieldBase<!!1, !!0>, new()
		{
			while (!this.variables.ContainsKey(varName))
			{
				this.processMessages();
				if (this.shouldAbort())
				{
					throw new AbortNetSynchronizerException();
				}
			}
			return (this.variables[varName] as TField).Value;
		}

		// Token: 0x06001169 RID: 4457 RVA: 0x000CB97C File Offset: 0x000C9B7C
		public void sendVar<TField, T>(string varName, T value) where TField : NetFieldBase<!!1, !!0>, new()
		{
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					NetRoot<TField> root = new NetRoot<TField>(Activator.CreateInstance<TField>());
					root.Value.Value = value;
					root.WriteFull(writer);
					this.variables[varName] = root.Value;
					stream.Seek(0L, SeekOrigin.Begin);
					this.sendMessage(new object[]
					{
						0,
						varName,
						stream.ToArray()
					});
				}
			}
		}

		// Token: 0x0600116A RID: 4458 RVA: 0x000CBA30 File Offset: 0x000C9C30
		public bool hasVar(string varName)
		{
			return this.variables.ContainsKey(varName);
		}

		// Token: 0x0600116B RID: 4459
		public abstract void processMessages();

		// Token: 0x0600116C RID: 4460
		protected abstract void sendMessage(params object[] data);

		// Token: 0x0600116D RID: 4461 RVA: 0x000CBA40 File Offset: 0x000C9C40
		public void receiveMessage(IncomingMessage msg)
		{
			byte messageType = msg.Reader.ReadByte();
			if (messageType == 0)
			{
				string varName = msg.Reader.ReadString();
				NetRoot<INetObject<INetSerializable>> root = new NetRoot<INetObject<INetSerializable>>();
				root.ReadFull(msg.Reader, default(NetVersion));
				this.variables[varName] = root.Value;
				return;
			}
			if (messageType != 1)
			{
				return;
			}
			string barrierName = msg.Reader.ReadString();
			this.barrierPlayers(barrierName).Add(msg.FarmerID);
		}

		// Token: 0x04000A5F RID: 2655
		private const byte MessageTypeVar = 0;

		// Token: 0x04000A60 RID: 2656
		private const byte MessageTypeBarrier = 1;

		// Token: 0x04000A61 RID: 2657
		private Dictionary<string, INetObject<INetSerializable>> variables = new Dictionary<string, INetObject<INetSerializable>>();

		// Token: 0x04000A62 RID: 2658
		private Dictionary<string, HashSet<long>> barriers = new Dictionary<string, HashSet<long>>();
	}
}

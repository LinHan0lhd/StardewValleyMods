using System;
using System.Collections.Generic;
using System.Threading;
using Netcode;
using StardewValley.Network;

namespace StardewValley
{
	// Token: 0x020000E7 RID: 231
	public class NewDaySynchronizer : NetSynchronizer
	{
		// Token: 0x0600116F RID: 4463 RVA: 0x000CBADA File Offset: 0x000C9CDA
		public NewDaySynchronizer()
		{
			this.ServerReady = false;
			this.Instantiated = false;
		}

		// Token: 0x06001170 RID: 4464 RVA: 0x000CBAF0 File Offset: 0x000C9CF0
		public bool hasInstance()
		{
			return this.Instantiated;
		}

		// Token: 0x06001171 RID: 4465 RVA: 0x000CBAF8 File Offset: 0x000C9CF8
		public void create()
		{
			this.Instantiated = true;
		}

		// Token: 0x06001172 RID: 4466 RVA: 0x000CBB01 File Offset: 0x000C9D01
		public void destroy()
		{
			this.Instantiated = false;
			this.ServerReady = false;
			base.reset();
		}

		// Token: 0x06001173 RID: 4467 RVA: 0x000CBB17 File Offset: 0x000C9D17
		public void flagServerReady()
		{
			if (Game1.IsMasterGame)
			{
				return;
			}
			this.ServerReady = true;
		}

		// Token: 0x06001174 RID: 4468 RVA: 0x000CBB28 File Offset: 0x000C9D28
		public void start()
		{
			Game1.multiplayer.UpdateEarly();
			if (Game1.IsMasterGame)
			{
				this.ServerReady = true;
				foreach (Farmer f in Game1.otherFarmers.Values)
				{
					Game1.server.sendMessage(f.UniqueMultiplayerID, new OutgoingMessage(30, Game1.player, Array.Empty<object>()));
				}
				return;
			}
			while (!this.ServerReady)
			{
				this.processMessages();
				if (base.shouldAbort())
				{
					this.ServerReady = false;
					throw new AbortNetSynchronizerException();
				}
				if (LocalMultiplayer.IsLocalMultiplayer(false))
				{
					return;
				}
			}
		}

		// Token: 0x06001175 RID: 4469 RVA: 0x000CBBD8 File Offset: 0x000C9DD8
		public bool hasStarted()
		{
			if (this.ServerReady)
			{
				return true;
			}
			this.processMessages();
			return false;
		}

		// Token: 0x06001176 RID: 4470 RVA: 0x000CBBEC File Offset: 0x000C9DEC
		public bool readyForFinish()
		{
			Game1.netReady.SetLocalReady("wakeup", true);
			Game1.player.team.Update();
			Game1.multiplayer.UpdateLate(false);
			Game1.multiplayer.UpdateEarly();
			return Game1.netReady.IsReady("wakeup");
		}

		// Token: 0x06001177 RID: 4471 RVA: 0x000CBC3C File Offset: 0x000C9E3C
		public bool readyForSave()
		{
			Game1.netReady.SetLocalReady("ready_for_save", true);
			Game1.player.team.Update();
			Game1.multiplayer.UpdateLate(false);
			Game1.multiplayer.UpdateEarly();
			return Game1.netReady.IsReady("ready_for_save");
		}

		// Token: 0x06001178 RID: 4472 RVA: 0x000CBC8C File Offset: 0x000C9E8C
		public int numReadyForSave()
		{
			return Game1.netReady.GetNumberReady("ready_for_save");
		}

		// Token: 0x06001179 RID: 4473 RVA: 0x000CBC9D File Offset: 0x000C9E9D
		public void finish()
		{
			if (Game1.IsServer)
			{
				base.sendVar<NetBool, bool>("finished", true);
			}
			Game1.multiplayer.UpdateLate(false);
		}

		// Token: 0x0600117A RID: 4474 RVA: 0x000CBCBD File Offset: 0x000C9EBD
		public bool hasFinished()
		{
			return base.hasVar("finished");
		}

		// Token: 0x0600117B RID: 4475 RVA: 0x000CBCCA File Offset: 0x000C9ECA
		public void flagSaved()
		{
			if (Game1.IsServer)
			{
				base.sendVar<NetBool, bool>("saved", true);
			}
			Game1.multiplayer.UpdateLate(false);
		}

		// Token: 0x0600117C RID: 4476 RVA: 0x000CBCEA File Offset: 0x000C9EEA
		public bool hasSaved()
		{
			return base.hasVar("saved");
		}

		// Token: 0x0600117D RID: 4477 RVA: 0x000CBCF7 File Offset: 0x000C9EF7
		public override void processMessages()
		{
			Game1.multiplayer.UpdateLate(false);
			Thread.Sleep(16);
			Program.sdk.Update();
			Game1.multiplayer.UpdateEarly();
		}

		// Token: 0x0600117E RID: 4478 RVA: 0x000CBD20 File Offset: 0x000C9F20
		protected override void sendMessage(params object[] data)
		{
			OutgoingMessage msg = new OutgoingMessage(14, Game1.player, data);
			if (Game1.IsServer)
			{
				using (IEnumerator<Farmer> enumerator = Game1.otherFarmers.Values.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Farmer f = enumerator.Current;
						Game1.server.sendMessage(f.UniqueMultiplayerID, msg);
					}
					return;
				}
			}
			if (Game1.IsClient)
			{
				Game1.client.sendMessage(msg);
			}
		}

		// Token: 0x04000A63 RID: 2659
		private bool ServerReady;

		// Token: 0x04000A64 RID: 2660
		private bool Instantiated;
	}
}

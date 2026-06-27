using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.Objects;

namespace StardewValley.Network.ChestHit
{
	// Token: 0x02000207 RID: 519
	public sealed class ChestHitSynchronizer
	{
		// Token: 0x060022F5 RID: 8949 RVA: 0x0017866A File Offset: 0x0017686A
		public void Reset()
		{
			this.EventQueue.Clear();
			this.SavedTimers.Clear();
		}

		// Token: 0x060022F6 RID: 8950 RVA: 0x00178684 File Offset: 0x00176884
		public void Update()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			while (this.EventQueue.Count > 0)
			{
				ChestHitArgs args = this.EventQueue.Dequeue();
				if (args == null)
				{
					break;
				}
				GameLocation location = args.Location;
				Chest chest = ((location != null) ? location.getObjectAtTile(args.ChestTile.X, args.ChestTile.Y, true) : null) as Chest;
				if (chest != null)
				{
					chest.HandleChestHit(args);
				}
			}
		}

		// Token: 0x060022F7 RID: 8951 RVA: 0x001786F4 File Offset: 0x001768F4
		public void Sync(ChestHitArgs args)
		{
			GameLocation location = args.Location;
			Chest chest = ((location != null) ? location.getObjectAtTile(args.ChestTile.X, args.ChestTile.Y, true) : null) as Chest;
			if (chest == null)
			{
				return;
			}
			if (Game1.IsMasterGame)
			{
				this.EventQueue.Enqueue(args);
				return;
			}
			if (chest.hitTimerInstance != null)
			{
				ChestHitTimer hitTimerInstance = chest.hitTimerInstance;
				GameTime currentGameTime = Game1.currentGameTime;
				hitTimerInstance.SavedTime = (int)((currentGameTime != null) ? currentGameTime.TotalGameTime.TotalMilliseconds : -999.0);
				Dictionary<ulong, ChestHitTimer> localTimers;
				if (!this.SavedTimers.TryGetValue(args.Location.NameOrUniqueName, out localTimers))
				{
					localTimers = new Dictionary<ulong, ChestHitTimer>();
					this.SavedTimers.Add(args.Location.NameOrUniqueName, localTimers);
				}
				localTimers[ChestHitSynchronizer.HashPosition(args.ChestTile.X, args.ChestTile.Y)] = chest.hitTimerInstance;
			}
			Client client = Game1.client;
			if (client == null)
			{
				return;
			}
			client.sendMessage(new OutgoingMessage(32, Game1.player, new object[]
			{
				0,
				args.Location.isStructure.Value,
				args.Location.NameOrUniqueName,
				args.ChestTile.X,
				args.ChestTile.Y,
				args.ToolPosition,
				args.StandingPixel.X,
				args.StandingPixel.Y,
				args.Direction,
				args.HoldDownClick,
				args.ToolCanHit,
				args.RecentlyHit
			}));
		}

		// Token: 0x060022F8 RID: 8952 RVA: 0x001788C4 File Offset: 0x00176AC4
		public void SignalMove(GameLocation location, int sourceTileX, int sourceTileY, int destTileX, int destTileY)
		{
			if (Game1.server == null || location == null)
			{
				return;
			}
			foreach (Farmer farmer in Game1.otherFarmers.Values)
			{
				Game1.server.sendMessage(farmer.UniqueMultiplayerID, new OutgoingMessage(32, Game1.player, new object[]
				{
					1,
					location.NameOrUniqueName,
					sourceTileX,
					sourceTileY,
					destTileX,
					destTileY
				}));
			}
		}

		// Token: 0x060022F9 RID: 8953 RVA: 0x00178974 File Offset: 0x00176B74
		public void SignalDelete(GameLocation location, int tileX, int tileY)
		{
			if (Game1.server == null || location == null)
			{
				return;
			}
			foreach (Farmer farmer in Game1.otherFarmers.Values)
			{
				Game1.server.sendMessage(farmer.UniqueMultiplayerID, new OutgoingMessage(32, Game1.player, new object[]
				{
					2,
					location.NameOrUniqueName,
					tileX,
					tileY
				}));
			}
		}

		// Token: 0x060022FA RID: 8954 RVA: 0x00178A10 File Offset: 0x00176C10
		public void ProcessMessage(IncomingMessage message)
		{
			switch (message.Reader.ReadByte())
			{
			case 0:
				this.ProcessSync(message);
				return;
			case 1:
				this.ProcessMove(message);
				return;
			case 2:
				this.ProcessDelete(message);
				return;
			default:
				return;
			}
		}

		// Token: 0x060022FB RID: 8955 RVA: 0x00178A53 File Offset: 0x00176C53
		internal unsafe static ulong HashPosition(int x, int y)
		{
			return (ulong)(*(&x)) << 32 | (ulong)(*(&y));
		}

		// Token: 0x060022FC RID: 8956 RVA: 0x00178A64 File Offset: 0x00176C64
		private static GameLocation ReadLocation(IncomingMessage message)
		{
			bool isStructure = message.Reader.ReadBoolean();
			GameLocation location = Game1.getLocationFromName(message.Reader.ReadString(), isStructure);
			if (location == null || Game1.multiplayer.locationRoot(location) == null)
			{
				return null;
			}
			return location;
		}

		// Token: 0x060022FD RID: 8957 RVA: 0x00178AA4 File Offset: 0x00176CA4
		private void ProcessSync(IncomingMessage message)
		{
			if (!Game1.IsMasterGame)
			{
				Game1.log.Warn("Unexpectedly received a chest hit sync message as a farmhand.");
				return;
			}
			ChestHitArgs args = new ChestHitArgs();
			bool isStructure = message.Reader.ReadBoolean();
			string locationName = message.Reader.ReadString();
			args.Location = Game1.getLocationFromName(locationName, isStructure);
			if (args.Location == null || Game1.multiplayer.locationRoot(args.Location) == null)
			{
				return;
			}
			args.ChestTile.X = message.Reader.ReadInt32();
			args.ChestTile.Y = message.Reader.ReadInt32();
			args.ToolPosition.X = message.Reader.ReadSingle();
			args.ToolPosition.Y = message.Reader.ReadSingle();
			args.StandingPixel.X = message.Reader.ReadInt32();
			args.StandingPixel.Y = message.Reader.ReadInt32();
			args.Direction = message.Reader.ReadInt32();
			args.HoldDownClick = message.Reader.ReadBoolean();
			args.ToolCanHit = message.Reader.ReadBoolean();
			args.RecentlyHit = message.Reader.ReadBoolean();
			this.EventQueue.Enqueue(args);
		}

		// Token: 0x060022FE RID: 8958 RVA: 0x00178BE4 File Offset: 0x00176DE4
		private void ProcessMove(IncomingMessage message)
		{
			if (Game1.IsMasterGame)
			{
				Game1.log.Warn("Unexpectedly received a chest move message as the host.");
				return;
			}
			string locationName = message.Reader.ReadString();
			if (locationName == null)
			{
				return;
			}
			int sourceTileX = message.Reader.ReadInt32();
			int sourceTileY = message.Reader.ReadInt32();
			int destTileX = message.Reader.ReadInt32();
			int destTileY = message.Reader.ReadInt32();
			Dictionary<ulong, ChestHitTimer> localTimers;
			if (!this.SavedTimers.TryGetValue(locationName, out localTimers))
			{
				return;
			}
			ulong sourceTile = ChestHitSynchronizer.HashPosition(sourceTileX, sourceTileY);
			ChestHitTimer timer;
			if (!localTimers.TryGetValue(sourceTile, out timer))
			{
				return;
			}
			localTimers.Remove(sourceTile);
			localTimers.TryAdd(ChestHitSynchronizer.HashPosition(destTileX, destTileY), timer);
		}

		// Token: 0x060022FF RID: 8959 RVA: 0x00178C90 File Offset: 0x00176E90
		private void ProcessDelete(IncomingMessage message)
		{
			if (Game1.IsMasterGame)
			{
				Game1.log.Warn("Unexpectedly received a chest delete message as the host.");
				return;
			}
			string locationName = message.Reader.ReadString();
			if (locationName == null)
			{
				return;
			}
			int deleteTileX = message.Reader.ReadInt32();
			int deleteTileY = message.Reader.ReadInt32();
			Dictionary<ulong, ChestHitTimer> localTimers;
			if (!this.SavedTimers.TryGetValue(locationName, out localTimers))
			{
				return;
			}
			localTimers.Remove(ChestHitSynchronizer.HashPosition(deleteTileX, deleteTileY));
		}

		// Token: 0x040014B4 RID: 5300
		private readonly Queue<ChestHitArgs> EventQueue = new Queue<ChestHitArgs>();

		// Token: 0x040014B5 RID: 5301
		internal readonly Dictionary<string, Dictionary<ulong, ChestHitTimer>> SavedTimers = new Dictionary<string, Dictionary<ulong, ChestHitTimer>>();
	}
}

using System;
using StardewValley.Characters;

namespace StardewValley
{
	// Token: 0x020000CC RID: 204
	public class LocationRequest
	{
		// Token: 0x14000016 RID: 22
		// (add) Token: 0x06000DF4 RID: 3572 RVA: 0x00095040 File Offset: 0x00093240
		// (remove) Token: 0x06000DF5 RID: 3573 RVA: 0x00095078 File Offset: 0x00093278
		public event LocationRequest.Callback OnLoad;

		// Token: 0x14000017 RID: 23
		// (add) Token: 0x06000DF6 RID: 3574 RVA: 0x000950B0 File Offset: 0x000932B0
		// (remove) Token: 0x06000DF7 RID: 3575 RVA: 0x000950E8 File Offset: 0x000932E8
		public event LocationRequest.Callback OnWarp;

		// Token: 0x06000DF8 RID: 3576 RVA: 0x0009511D File Offset: 0x0009331D
		public LocationRequest(string name, bool isStructure, GameLocation location)
		{
			this.Name = name;
			this.IsStructure = isStructure;
			this.Location = location;
		}

		// Token: 0x06000DF9 RID: 3577 RVA: 0x0009513A File Offset: 0x0009333A
		public void Loaded(GameLocation location)
		{
			LocationRequest.Callback onLoad = this.OnLoad;
			if (onLoad == null)
			{
				return;
			}
			onLoad();
		}

		// Token: 0x06000DFA RID: 3578 RVA: 0x0009514C File Offset: 0x0009334C
		public void Warped(GameLocation location)
		{
			LocationRequest.Callback onWarp = this.OnWarp;
			if (onWarp != null)
			{
				onWarp();
			}
			Game1.player.ridingMineElevator = false;
			Horse mount = Game1.player.mount;
			if (mount != null)
			{
				mount.SyncPositionToRider();
			}
			Game1.player.ClearCachedPosition();
			Game1.forceSnapOnNextViewportUpdate = true;
		}

		// Token: 0x06000DFB RID: 3579 RVA: 0x0009519A File Offset: 0x0009339A
		public bool IsRequestFor(GameLocation location)
		{
			return (!this.IsStructure && location.Name == this.Name) || (location.NameOrUniqueName == this.Name && location.isStructure.Value);
		}

		// Token: 0x06000DFC RID: 3580 RVA: 0x000951D9 File Offset: 0x000933D9
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"LocationRequest(",
				this.Name,
				", ",
				this.IsStructure.ToString(),
				")"
			});
		}

		// Token: 0x04000940 RID: 2368
		public string Name;

		// Token: 0x04000941 RID: 2369
		public bool IsStructure;

		// Token: 0x04000942 RID: 2370
		public GameLocation Location;

		// Token: 0x02000474 RID: 1140
		// (Invoke) Token: 0x06003E3E RID: 15934
		public delegate void Callback();
	}
}

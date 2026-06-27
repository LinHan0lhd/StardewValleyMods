using System;
using System.Collections.Generic;
using System.Linq;
using Netcode;
using StardewValley.Network;

namespace StardewValley.Locations
{
	// Token: 0x020002CA RID: 714
	public class DecorationFacade : SerializationCollectionFacade<int>
	{
		// Token: 0x14000023 RID: 35
		// (add) Token: 0x06002E6D RID: 11885 RVA: 0x002441F4 File Offset: 0x002423F4
		// (remove) Token: 0x06002E6E RID: 11886 RVA: 0x0024422C File Offset: 0x0024242C
		public event DecorationFacade.ChangeEvent OnChange;

		// Token: 0x17000410 RID: 1040
		public int this[int whichRoom]
		{
			get
			{
				this.WarnDeprecation();
				int value;
				if (!this.Field.TryGetValue(whichRoom, out value))
				{
					return 0;
				}
				return value;
			}
			set
			{
				this.WarnDeprecation();
				this.Field[whichRoom] = value;
			}
		}

		// Token: 0x17000411 RID: 1041
		// (get) Token: 0x06002E71 RID: 11889 RVA: 0x0024429F File Offset: 0x0024249F
		public int Count
		{
			get
			{
				if (this.Field.Length == 0)
				{
					return 0;
				}
				return this.Field.Keys.Max() + 1;
			}
		}

		// Token: 0x06002E72 RID: 11890 RVA: 0x002442C7 File Offset: 0x002424C7
		public DecorationFacade()
		{
			this.Field.OnValueAdded += delegate(int whichRoom, int which)
			{
				this.Field.InterpolationWait = false;
				this.Field.FieldDict[whichRoom].fieldChangeEvent += delegate(NetInt field, int oldValue, int newValue)
				{
					this.changed(whichRoom, newValue);
				};
				this.changed(whichRoom, which);
			};
		}

		// Token: 0x06002E73 RID: 11891 RVA: 0x00244304 File Offset: 0x00242504
		private void changed(int whichRoom, int which)
		{
			this.pendingChanges.Add(delegate
			{
				DecorationFacade.ChangeEvent onChange = this.OnChange;
				if (onChange == null)
				{
					return;
				}
				onChange(whichRoom, which);
			});
		}

		// Token: 0x06002E74 RID: 11892 RVA: 0x00244344 File Offset: 0x00242544
		protected override List<int> Serialize()
		{
			List<int> result = new List<int>();
			while (result.Count < this.Count)
			{
				result.Add(0);
			}
			foreach (KeyValuePair<int, int> pair in this.Field.Pairs)
			{
				result[pair.Key] = pair.Value;
			}
			return result;
		}

		// Token: 0x06002E75 RID: 11893 RVA: 0x002443CC File Offset: 0x002425CC
		protected override void DeserializeAdd(int serialValue)
		{
			this.Field[this.Count] = serialValue;
		}

		// Token: 0x06002E76 RID: 11894 RVA: 0x002443E0 File Offset: 0x002425E0
		public void Set(DecorationFacade other)
		{
			this.Field.Set(other.Field.Pairs);
		}

		// Token: 0x06002E77 RID: 11895 RVA: 0x002443FD File Offset: 0x002425FD
		public void SetCountAtLeast(int targetCount)
		{
			while (this.Count < targetCount)
			{
				this[this.Count] = 0;
			}
		}

		// Token: 0x06002E78 RID: 11896 RVA: 0x00244418 File Offset: 0x00242618
		public void Update()
		{
			foreach (Action action in this.pendingChanges)
			{
				action();
			}
			this.pendingChanges.Clear();
		}

		// Token: 0x06002E79 RID: 11897 RVA: 0x00244474 File Offset: 0x00242674
		public virtual void WarnDeprecation()
		{
			if (Game1.gameMode != 6 && !DecorationFacade.warnedDeprecated)
			{
				DecorationFacade.warnedDeprecated = true;
				Game1.log.Warn("WARNING: DecorationFacade/DecoratableLocation.wallPaper and floor are deprecated. Use wallpaperIDs, appliedWallpaper, wallPaperTiles/floorIDs, appliedFloor, and floorTiles instead.");
			}
		}

		// Token: 0x04001FB0 RID: 8112
		public readonly NetIntDictionary<int, NetInt> Field = new NetIntDictionary<int, NetInt>
		{
			InterpolationWait = false
		};

		// Token: 0x04001FB2 RID: 8114
		private List<Action> pendingChanges = new List<Action>();

		// Token: 0x04001FB3 RID: 8115
		[NonInstancedStatic]
		public static bool warnedDeprecated;

		// Token: 0x0200064C RID: 1612
		// (Invoke) Token: 0x060044EE RID: 17646
		public delegate void ChangeEvent(int whichRoom, int which);
	}
}

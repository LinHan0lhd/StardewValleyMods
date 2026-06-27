using System;
using Microsoft.Xna.Framework;
using StardewValley.Characters;
using StardewValley.Util;

namespace StardewValley.Buildings
{
	// Token: 0x0200038A RID: 906
	public class Stable : Building
	{
		// Token: 0x1700047D RID: 1149
		// (get) Token: 0x060037E6 RID: 14310 RVA: 0x002C49EB File Offset: 0x002C2BEB
		// (set) Token: 0x060037E7 RID: 14311 RVA: 0x002C49F8 File Offset: 0x002C2BF8
		public Guid HorseId
		{
			get
			{
				return this.id.Value;
			}
			set
			{
				this.id.Value = value;
			}
		}

		// Token: 0x060037E8 RID: 14312 RVA: 0x002C4A06 File Offset: 0x002C2C06
		public Stable() : this(Vector2.Zero)
		{
		}

		// Token: 0x060037E9 RID: 14313 RVA: 0x002C4A13 File Offset: 0x002C2C13
		public Stable(Vector2 tileLocation) : this(tileLocation, GuidHelper.NewGuid())
		{
		}

		// Token: 0x060037EA RID: 14314 RVA: 0x002C4A21 File Offset: 0x002C2C21
		public Stable(Vector2 tileLocation, Guid horseId) : base("Stable", tileLocation)
		{
			this.HorseId = horseId;
		}

		// Token: 0x060037EB RID: 14315 RVA: 0x002C4A36 File Offset: 0x002C2C36
		public override Rectangle? getSourceRectForMenu()
		{
			return new Rectangle?(new Rectangle(0, 0, this.texture.Value.Bounds.Width, this.texture.Value.Bounds.Height));
		}

		// Token: 0x060037EC RID: 14316 RVA: 0x002C4A6E File Offset: 0x002C2C6E
		public Horse getStableHorse()
		{
			return Utility.findHorse(this.HorseId);
		}

		// Token: 0x060037ED RID: 14317 RVA: 0x002C4A7B File Offset: 0x002C2C7B
		public Point GetDefaultHorseTile()
		{
			return new Point(this.tileX.Value + 1, this.tileY.Value + 1);
		}

		// Token: 0x060037EE RID: 14318 RVA: 0x002C4A9C File Offset: 0x002C2C9C
		public virtual void grabHorse()
		{
			if (this.daysOfConstructionLeft.Value > 0)
			{
				return;
			}
			Horse horse = Utility.findHorse(this.HorseId);
			Point defaultTile = this.GetDefaultHorseTile();
			if (horse == null)
			{
				horse = new Horse(this.HorseId, defaultTile.X, defaultTile.Y);
				base.GetParentLocation().characters.Add(horse);
			}
			else
			{
				Game1.warpCharacter(horse, this.parentLocationName.Value, defaultTile);
			}
			horse.ownerId.Value = this.owner.Value;
		}

		// Token: 0x060037EF RID: 14319 RVA: 0x002C4B24 File Offset: 0x002C2D24
		public virtual void updateHorseOwnership()
		{
			if (this.daysOfConstructionLeft.Value > 0)
			{
				return;
			}
			Horse horse = Utility.findHorse(this.HorseId);
			if (horse != null)
			{
				horse.ownerId.Value = this.owner.Value;
				if (horse.getOwner() != null)
				{
					if (horse.getOwner().horseName.Value != null)
					{
						horse.name.Value = horse.getOwner().horseName.Value;
						horse.displayName = horse.getOwner().horseName.Value;
						return;
					}
					horse.name.Value = "";
					horse.displayName = "";
				}
			}
		}

		// Token: 0x060037F0 RID: 14320 RVA: 0x002C4BCC File Offset: 0x002C2DCC
		public override void dayUpdate(int dayOfMonth)
		{
			base.dayUpdate(dayOfMonth);
			this.grabHorse();
		}

		// Token: 0x060037F1 RID: 14321 RVA: 0x002C4BDC File Offset: 0x002C2DDC
		public override void performActionOnDemolition(GameLocation location)
		{
			base.performActionOnDemolition(location);
			Horse horse = this.getStableHorse();
			if (horse != null)
			{
				GameLocation currentLocation = horse.currentLocation;
				if (currentLocation == null)
				{
					return;
				}
				currentLocation.characters.Remove(horse);
			}
		}
	}
}

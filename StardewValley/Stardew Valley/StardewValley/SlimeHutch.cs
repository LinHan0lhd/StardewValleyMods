using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Tools;

namespace StardewValley
{
	// Token: 0x020000D9 RID: 217
	public class SlimeHutch : DecoratableLocation
	{
		// Token: 0x06001089 RID: 4233 RVA: 0x000C6D78 File Offset: 0x000C4F78
		public SlimeHutch()
		{
		}

		// Token: 0x0600108A RID: 4234 RVA: 0x000C6D9E File Offset: 0x000C4F9E
		public SlimeHutch(string m, string name) : base(m, name)
		{
		}

		// Token: 0x0600108B RID: 4235 RVA: 0x000C6DC6 File Offset: 0x000C4FC6
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.slimeMatingsLeft, "slimeMatingsLeft").AddField(this.waterSpots, "waterSpots");
		}

		// Token: 0x0600108C RID: 4236 RVA: 0x000C6DF5 File Offset: 0x000C4FF5
		public override void OnParentBuildingUpgraded(Building building)
		{
			base.OnParentBuildingUpgraded(building);
			this._slimeCapacity = -1;
		}

		// Token: 0x0600108D RID: 4237 RVA: 0x000C6E08 File Offset: 0x000C5008
		public bool isFull()
		{
			if (this._slimeCapacity < 0)
			{
				Building parentBuilding = this.ParentBuilding;
				int? num;
				if (parentBuilding == null)
				{
					num = null;
				}
				else
				{
					BuildingData data = parentBuilding.GetData();
					num = ((data != null) ? new int?(data.MaxOccupants) : null);
				}
				int? num2 = num;
				this._slimeCapacity = num2.GetValueOrDefault(20);
			}
			return this.characters.Count >= this._slimeCapacity;
		}

		// Token: 0x0600108E RID: 4238 RVA: 0x000C6E78 File Offset: 0x000C5078
		public override bool canSlimeMateHere()
		{
			int matesLeft = this.slimeMatingsLeft.Value;
			NetInt netInt = this.slimeMatingsLeft;
			int value = netInt.Value;
			netInt.Value = value - 1;
			return !this.isFull() && matesLeft > 0;
		}

		// Token: 0x0600108F RID: 4239 RVA: 0x000C6EB4 File Offset: 0x000C50B4
		public override bool canSlimeHatchHere()
		{
			return !this.isFull();
		}

		// Token: 0x06001090 RID: 4240 RVA: 0x000C6EC0 File Offset: 0x000C50C0
		public override void DayUpdate(int dayOfMonth)
		{
			int waters = 0;
			int startIndex = Game1.random.Next(this.waterSpots.Length);
			for (int i = 0; i < this.waterSpots.Length; i++)
			{
				if (this.waterSpots[(i + startIndex) % this.waterSpots.Length] && waters * 5 < this.characters.Count)
				{
					waters++;
					this.waterSpots[(i + startIndex) % this.waterSpots.Length] = false;
				}
			}
			foreach (Object sprinkler in this.objects.Values)
			{
				if (sprinkler.IsSprinkler())
				{
					foreach (Vector2 v in sprinkler.GetSprinklerTiles())
					{
						if (v.X == 16f && v.Y >= 6f && v.Y <= 9f)
						{
							this.waterSpots[(int)v.Y - 6] = true;
						}
					}
				}
			}
			for (int numSlimeBalls = Math.Min(this.characters.Count / 5, waters); numSlimeBalls > 0; numSlimeBalls--)
			{
				int tries = 50;
				Vector2 tile = base.getRandomTile(null);
				while ((!this.CanItemBePlacedHere(tile, false, CollisionMask.All, CollisionMask.None, false, false) || this.doesTileHaveProperty((int)tile.X, (int)tile.Y, "NPCBarrier", "Back", false) != null || tile.Y >= 12f) && tries > 0)
				{
					tile = base.getRandomTile(null);
					tries--;
				}
				if (tries > 0)
				{
					Object slimeBall = ItemRegistry.Create<Object>("(BC)56", 1, 0, false);
					slimeBall.fragility.Value = 2;
					this.objects.Add(tile, slimeBall);
				}
			}
			while (this.slimeMatingsLeft.Value > 0)
			{
				if (this.characters.Count > 1 && !this.isFull())
				{
					GreenSlime mate = this.characters[Game1.random.Next(this.characters.Count)] as GreenSlime;
					if (mate != null && mate.ageUntilFullGrown.Value <= 0)
					{
						for (int distance = 1; distance < 10; distance++)
						{
							GreenSlime otherMate = (GreenSlime)Utility.checkForCharacterWithinArea(mate.GetType(), mate.Position, this, new Rectangle((int)mate.Position.X - 64 * distance, (int)mate.Position.Y - 64 * distance, 64 * (distance * 2 + 1), 64 * (distance * 2 + 1)));
							if (otherMate != null && otherMate.cute.Value != mate.cute.Value && otherMate.ageUntilFullGrown.Value <= 0)
							{
								mate.mateWith(otherMate, this);
								break;
							}
						}
					}
				}
				NetInt netInt = this.slimeMatingsLeft;
				int value = netInt.Value;
				netInt.Value = value - 1;
			}
			this.slimeMatingsLeft.Value = this.characters.Count / 5 + 1;
			base.DayUpdate(dayOfMonth);
		}

		// Token: 0x06001091 RID: 4241 RVA: 0x000C7228 File Offset: 0x000C5428
		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			SlimeHutch slimeHutch = l as SlimeHutch;
			if (slimeHutch != null)
			{
				for (int i = 0; i < this.waterSpots.Length; i++)
				{
					if (i < slimeHutch.waterSpots.Count)
					{
						this.waterSpots[i] = slimeHutch.waterSpots[i];
					}
				}
			}
			base.TransferDataFromSavedLocation(l);
		}

		// Token: 0x06001092 RID: 4242 RVA: 0x000C7284 File Offset: 0x000C5484
		protected override void resetLocalState()
		{
			base.resetLocalState();
			Object tileObj;
			if (this.objects.TryGetValue(new Vector2(1f, 4f), out tileObj))
			{
				tileObj.Fragility = 0;
			}
		}

		// Token: 0x06001093 RID: 4243 RVA: 0x000C72BC File Offset: 0x000C54BC
		public override bool performToolAction(Tool t, int tileX, int tileY)
		{
			if (t is WateringCan && tileX == 16 && tileY >= 6 && tileY <= 9)
			{
				this.waterSpots[tileY - 6] = true;
			}
			return false;
		}

		// Token: 0x06001094 RID: 4244 RVA: 0x000C72E4 File Offset: 0x000C54E4
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			for (int i = 0; i < this.waterSpots.Length; i++)
			{
				int tileIndex = this.waterSpots[i] ? 2135 : 2134;
				base.setMapTile(16, 6 + i, tileIndex, "Buildings", "untitled tile sheet", null, true);
			}
		}

		// Token: 0x04000A0A RID: 2570
		[XmlElement("slimeMatingsLeft")]
		public readonly NetInt slimeMatingsLeft = new NetInt();

		// Token: 0x04000A0B RID: 2571
		public readonly NetArray<bool, NetBool> waterSpots = new NetArray<bool, NetBool>(4);

		// Token: 0x04000A0C RID: 2572
		protected int _slimeCapacity = -1;
	}
}

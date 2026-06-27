using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Network;
using StardewValley.Objects;

namespace StardewValley.Locations
{
	// Token: 0x020002DD RID: 733
	public class IslandShrine : IslandForestLocation
	{
		// Token: 0x0600305D RID: 12381 RVA: 0x00263515 File Offset: 0x00261715
		public IslandShrine()
		{
		}

		// Token: 0x0600305E RID: 12382 RVA: 0x00263534 File Offset: 0x00261734
		public IslandShrine(string map, string name) : base(map, name)
		{
			this.AddMissingPedestals();
		}

		// Token: 0x0600305F RID: 12383 RVA: 0x0026355B File Offset: 0x0026175B
		public override List<Vector2> GetAdditionalWalnutBushes()
		{
			return new List<Vector2>
			{
				new Vector2(23f, 34f)
			};
		}

		// Token: 0x06003060 RID: 12384 RVA: 0x00263578 File Offset: 0x00261778
		public ItemPedestal AddOrUpdatePedestal(Vector2 position, string birdLocation)
		{
			ItemPedestal pedestal = base.getObjectAtTile((int)position.X, (int)position.Y, false) as ItemPedestal;
			string itemId = IslandGemBird.GetItemIndex(IslandGemBird.GetBirdTypeForLocation(birdLocation));
			if (pedestal == null || !pedestal.isIslandShrinePedestal.Value)
			{
				OverlaidDictionary objects = this.objects;
				ItemPedestal itemPedestal = new ItemPedestal(position, null, false, Color.White, "221");
				itemPedestal.Fragility = 2;
				itemPedestal.isIslandShrinePedestal.Value = true;
				pedestal = itemPedestal;
				objects[position] = itemPedestal;
			}
			pedestal.successColor.Value = Color.Transparent;
			Object value = pedestal.requiredItem.Value;
			if (((value != null) ? value.ItemId : null) != itemId)
			{
				pedestal.requiredItem.Value = new Object(itemId, 1, false, -1, 0);
				Object value2 = pedestal.heldObject.Value;
				if (((value2 != null) ? value2.ItemId : null) != itemId)
				{
					pedestal.heldObject.Value = null;
				}
			}
			return pedestal;
		}

		// Token: 0x06003061 RID: 12385 RVA: 0x00263660 File Offset: 0x00261860
		public virtual void AddMissingPedestals()
		{
			this.westPedestal = this.AddOrUpdatePedestal(new Vector2(21f, 27f), "IslandWest");
			this.eastPedestal = this.AddOrUpdatePedestal(new Vector2(27f, 27f), "IslandEast");
			this.southPedestal = this.AddOrUpdatePedestal(new Vector2(24f, 28f), "IslandSouth");
			this.northPedestal = this.AddOrUpdatePedestal(new Vector2(24f, 25f), "IslandNorth");
		}

		// Token: 0x06003062 RID: 12386 RVA: 0x002636F0 File Offset: 0x002618F0
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.puzzleFinished, "puzzleFinished").AddField(this.puzzleFinishedEvent, "puzzleFinishedEvent");
			this.puzzleFinishedEvent.onEvent += this.OnPuzzleFinish;
		}

		// Token: 0x06003063 RID: 12387 RVA: 0x00263741 File Offset: 0x00261941
		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (Game1.IsMasterGame)
			{
				this.AddMissingPedestals();
			}
		}

		// Token: 0x06003064 RID: 12388 RVA: 0x00263756 File Offset: 0x00261956
		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			if (this.puzzleFinished.Value)
			{
				this.ApplyFinishedTiles();
			}
		}

		// Token: 0x06003065 RID: 12389 RVA: 0x00263774 File Offset: 0x00261974
		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			base.TransferDataFromSavedLocation(l);
			IslandShrine shrine = l as IslandShrine;
			if (shrine != null)
			{
				this.northPedestal = (shrine.getObjectAtTile((int)this.northPedestal.TileLocation.X, (int)this.northPedestal.TileLocation.Y, false) as ItemPedestal);
				this.southPedestal = (shrine.getObjectAtTile((int)this.southPedestal.TileLocation.X, (int)this.southPedestal.TileLocation.Y, false) as ItemPedestal);
				this.eastPedestal = (shrine.getObjectAtTile((int)this.eastPedestal.TileLocation.X, (int)this.eastPedestal.TileLocation.Y, false) as ItemPedestal);
				this.westPedestal = (shrine.getObjectAtTile((int)this.westPedestal.TileLocation.X, (int)this.westPedestal.TileLocation.Y, false) as ItemPedestal);
				this.puzzleFinished.Value = shrine.puzzleFinished.Value;
			}
		}

		// Token: 0x06003066 RID: 12390 RVA: 0x0026387C File Offset: 0x00261A7C
		public void OnPuzzleFinish()
		{
			if (Game1.IsMasterGame)
			{
				for (int i = 0; i < 5; i++)
				{
					Game1.createItemDebris(ItemRegistry.Create("(O)73", 1, 0, false), new Vector2(24f, 19f) * 64f, -1, this, -1, false);
				}
			}
			if (Game1.currentLocation == this)
			{
				Game1.playSound("boulderBreak", null);
				Game1.playSound("secret1", null);
				Game1.flashAlpha = 1f;
				this.ApplyFinishedTiles();
			}
		}

		// Token: 0x06003067 RID: 12391 RVA: 0x0026390C File Offset: 0x00261B0C
		public virtual void ApplyFinishedTiles()
		{
			base.setMapTile(23, 19, 142, "AlwaysFront", "untitled tile sheet3", null, true);
			base.setMapTile(24, 19, 143, "AlwaysFront", "untitled tile sheet3", null, true);
			base.setMapTile(25, 19, 144, "AlwaysFront", "untitled tile sheet3", null, true);
		}

		// Token: 0x06003068 RID: 12392 RVA: 0x00263970 File Offset: 0x00261B70
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			if (Game1.IsMasterGame && !this.puzzleFinished.Value && this.northPedestal.match.Value && this.southPedestal.match.Value && this.eastPedestal.match.Value && this.westPedestal.match.Value)
			{
				Game1.player.team.MarkCollectedNut("IslandShrinePuzzle");
				this.puzzleFinishedEvent.Fire();
				this.puzzleFinished.Value = true;
				this.northPedestal.locked.Value = true;
				this.northPedestal.heldObject.Value = null;
				this.southPedestal.locked.Value = true;
				this.southPedestal.heldObject.Value = null;
				this.eastPedestal.locked.Value = true;
				this.eastPedestal.heldObject.Value = null;
				this.westPedestal.locked.Value = true;
				this.westPedestal.heldObject.Value = null;
			}
		}

		// Token: 0x04002099 RID: 8345
		[XmlIgnore]
		public ItemPedestal northPedestal;

		// Token: 0x0400209A RID: 8346
		[XmlIgnore]
		public ItemPedestal southPedestal;

		// Token: 0x0400209B RID: 8347
		[XmlIgnore]
		public ItemPedestal eastPedestal;

		// Token: 0x0400209C RID: 8348
		[XmlIgnore]
		public ItemPedestal westPedestal;

		// Token: 0x0400209D RID: 8349
		[XmlIgnore]
		public NetEvent0 puzzleFinishedEvent = new NetEvent0(false);

		// Token: 0x0400209E RID: 8350
		[XmlElement("puzzleFinished")]
		public NetBool puzzleFinished = new NetBool();
	}
}

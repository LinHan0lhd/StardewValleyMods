using System;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Extensions;
using StardewValley.TerrainFeatures;

namespace StardewValley.Tools
{
	// Token: 0x02000127 RID: 295
	public class Axe : Tool
	{
		// Token: 0x060017F1 RID: 6129 RVA: 0x001131F9 File Offset: 0x001113F9
		public Axe() : base("Axe", 0, 189, 215, false, 0)
		{
		}

		// Token: 0x060017F2 RID: 6130 RVA: 0x0011321F File Offset: 0x0011141F
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.additionalPower, "additionalPower");
		}

		// Token: 0x060017F3 RID: 6131 RVA: 0x00113240 File Offset: 0x00111440
		protected override void MigrateLegacyItemId()
		{
			switch (base.UpgradeLevel)
			{
			case 0:
				base.ItemId = "Axe";
				return;
			case 1:
				base.ItemId = "CopperAxe";
				return;
			case 2:
				base.ItemId = "SteelAxe";
				return;
			case 3:
				base.ItemId = "GoldAxe";
				return;
			case 4:
				base.ItemId = "IridiumAxe";
				return;
			default:
				base.ItemId = "Axe";
				return;
			}
		}

		// Token: 0x060017F4 RID: 6132 RVA: 0x001132B7 File Offset: 0x001114B7
		protected override Item GetOneNew()
		{
			return new Axe();
		}

		// Token: 0x060017F5 RID: 6133 RVA: 0x001132BE File Offset: 0x001114BE
		public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
		{
			this.Update(who.FacingDirection, 0, who);
			who.EndUsingTool();
			return true;
		}

		// Token: 0x060017F6 RID: 6134 RVA: 0x001132D8 File Offset: 0x001114D8
		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			base.DoFunction(location, x, y, power, who);
			if (!this.isEfficient.Value)
			{
				who.Stamina -= (float)(2 * power) - (float)who.ForagingLevel * 0.1f;
			}
			int tileX = x / 64;
			int tileY = y / 64;
			Rectangle tileRect = new Rectangle(tileX * 64, tileY * 64, 64, 64);
			Vector2 tile = new Vector2((float)tileX, (float)tileY);
			if (location.Map.RequireLayer("Buildings").Tiles[tileX, tileY] != null && location.Map.RequireLayer("Buildings").Tiles[tileX, tileY].TileIndexProperties.ContainsKey("TreeStump"))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Axe.cs.14023"));
				return;
			}
			this.upgradeLevel.Value += this.additionalPower.Value;
			location.performToolAction(this, tileX, tileY);
			TerrainFeature terrainFeature;
			if (location.terrainFeatures.TryGetValue(tile, out terrainFeature) && terrainFeature.performToolAction(this, 0, tile))
			{
				location.terrainFeatures.Remove(tile);
			}
			NetCollection<LargeTerrainFeature> largeTerrainFeatures = location.largeTerrainFeatures;
			if (largeTerrainFeatures != null)
			{
				largeTerrainFeatures.RemoveWhere((LargeTerrainFeature largeFeature) => largeFeature.getBoundingBox().Intersects(tileRect) && largeFeature.performToolAction(this, 0, tile));
			}
			Vector2 toolTilePosition = new Vector2((float)tileX, (float)tileY);
			Object obj;
			if (location.Objects.TryGetValue(toolTilePosition, out obj) && obj.Type != null && obj.performToolAction(this))
			{
				if (obj.Type == "Crafting" && obj.fragility.Value != 2)
				{
					location.debris.Add(new Debris(obj.QualifiedItemId, who.GetToolLocation(false), Utility.PointToVector2(who.StandingPixel)));
				}
				obj.performRemoveAction();
				location.Objects.Remove(toolTilePosition);
			}
			this.upgradeLevel.Value -= this.additionalPower.Value;
		}

		// Token: 0x04000E68 RID: 3688
		public NetInt additionalPower = new NetInt(0);
	}
}

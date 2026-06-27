using System;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.GameData;
using StardewValley.Locations;

namespace StardewValley.Objects
{
	// Token: 0x020001BA RID: 442
	public class Wallpaper : Object
	{
		// Token: 0x17000336 RID: 822
		// (get) Token: 0x06001F99 RID: 8089 RVA: 0x0016B106 File Offset: 0x00169306
		public override string TypeDefinitionId
		{
			get
			{
				if (!this.isFloor.Value)
				{
					return "(WP)";
				}
				return "(FL)";
			}
		}

		// Token: 0x06001F9A RID: 8090 RVA: 0x0016B120 File Offset: 0x00169320
		public Wallpaper()
		{
		}

		// Token: 0x06001F9B RID: 8091 RVA: 0x0016B14C File Offset: 0x0016934C
		public Wallpaper(int which, bool isFloor = false) : this()
		{
			base.ItemId = which.ToString();
			this.isFloor.Value = isFloor;
			base.ParentSheetIndex = which;
			base.name = (isFloor ? "Flooring" : "Wallpaper");
			this.sourceRect.Value = (isFloor ? new Rectangle(which % 8 * 32, 336 + which / 8 * 32, 28, 26) : new Rectangle(which % 16 * 16, which / 16 * 48 + 8, 16, 28));
			this.price.Value = 100;
		}

		// Token: 0x06001F9C RID: 8092 RVA: 0x0016B1E8 File Offset: 0x001693E8
		public Wallpaper(string setId, int which) : this()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
			defaultInterpolatedStringHandler.AppendFormatted(setId);
			defaultInterpolatedStringHandler.AppendLiteral(":");
			defaultInterpolatedStringHandler.AppendFormatted<int>(which);
			base.ItemId = defaultInterpolatedStringHandler.ToStringAndClear();
			this.setId.Value = setId;
			base.ParentSheetIndex = which;
			ModWallpaperOrFlooring setData = this.GetSetData();
			if (setData == null)
			{
				this.setId.Value = null;
			}
			this.isFloor.Value = (setData != null && setData.IsFlooring);
			this.sourceRect.Value = (this.isFloor.Value ? new Rectangle(which % 8 * 32, 336 + which / 8 * 32, 28, 26) : new Rectangle(which % 16 * 16, which / 16 * 48 + 8, 16, 28));
			if (setData != null && this.isFloor.Value)
			{
				this.sourceRect.Y = which / 8 * 32;
			}
			base.name = (this.isFloor.Value ? "Flooring" : "Wallpaper");
			this.price.Value = 100;
		}

		// Token: 0x06001F9D RID: 8093 RVA: 0x0016B308 File Offset: 0x00169508
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.sourceRect, "sourceRect").AddField(this.isFloor, "isFloor").AddField(this.setId, "setId");
		}

		// Token: 0x06001F9E RID: 8094 RVA: 0x0016B348 File Offset: 0x00169548
		public virtual ModWallpaperOrFlooring GetSetData()
		{
			if (this.setId.Value == null)
			{
				return null;
			}
			if (this.setData != null)
			{
				return this.setData;
			}
			foreach (ModWallpaperOrFlooring entry in DataLoader.AdditionalWallpaperFlooring(Game1.content))
			{
				if (entry.Id == this.setId.Value)
				{
					this.setData = entry;
					return entry;
				}
			}
			return null;
		}

		// Token: 0x06001F9F RID: 8095 RVA: 0x0016B3DC File Offset: 0x001695DC
		protected override string loadDisplayName()
		{
			if (!this.isFloor.Value)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13204");
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13203");
		}

		// Token: 0x06001FA0 RID: 8096 RVA: 0x0016B40A File Offset: 0x0016960A
		public override string getDescription()
		{
			if (!this.isFloor.Value)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13206");
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13205");
		}

		// Token: 0x06001FA1 RID: 8097 RVA: 0x0016B438 File Offset: 0x00169638
		public override bool performDropDownAction(Farmer who)
		{
			return true;
		}

		// Token: 0x06001FA2 RID: 8098 RVA: 0x0016B43B File Offset: 0x0016963B
		public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed = false)
		{
			return false;
		}

		// Token: 0x06001FA3 RID: 8099 RVA: 0x0016B440 File Offset: 0x00169640
		public override bool canBePlacedHere(GameLocation l, Vector2 tile, CollisionMask collisionMask = CollisionMask.All, bool showError = false)
		{
			Vector2 nonTile = tile * 64f;
			nonTile.X += 32f;
			nonTile.Y += 32f;
			foreach (Furniture f in l.furniture)
			{
				if (f.furniture_type.Value != 12 && f.GetBoundingBox().Contains((int)nonTile.X, (int)nonTile.Y))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06001FA4 RID: 8100 RVA: 0x0016B4F0 File Offset: 0x001696F0
		public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
		{
			if (who == null)
			{
				who = Game1.player;
			}
			DecoratableLocation decoratableLocation = location as DecoratableLocation;
			if (decoratableLocation != null)
			{
				Point tile = new Point(x / 64, y / 64);
				if (this.isFloor.Value)
				{
					string floor_id = decoratableLocation.GetFloorID(tile.X, tile.Y);
					if (floor_id != null)
					{
						if (this.GetSetData() != null)
						{
							decoratableLocation.SetFloor(this.GetSetData().Id + ":" + this.parentSheetIndex.Value.ToString(), floor_id);
						}
						else
						{
							decoratableLocation.SetFloor(this.parentSheetIndex.Value.ToString(), floor_id);
						}
						location.playSound("coin", null, null, SoundContext.Default);
						return true;
					}
				}
				else
				{
					string wall_id = decoratableLocation.GetWallpaperID(tile.X, tile.Y);
					if (wall_id != null)
					{
						if (this.GetSetData() != null)
						{
							decoratableLocation.SetWallpaper(this.GetSetData().Id + ":" + this.parentSheetIndex.Value.ToString(), wall_id);
						}
						else
						{
							decoratableLocation.SetWallpaper(this.parentSheetIndex.Value.ToString(), wall_id);
						}
						location.playSound("coin", null, null, SoundContext.Default);
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06001FA5 RID: 8101 RVA: 0x0016B652 File Offset: 0x00169852
		public override bool isPlaceable()
		{
			return true;
		}

		// Token: 0x06001FA6 RID: 8102 RVA: 0x0016B655 File Offset: 0x00169855
		public override int salePrice(bool ignoreProfitMargins = false)
		{
			return this.price.Value;
		}

		// Token: 0x06001FA7 RID: 8103 RVA: 0x0016B662 File Offset: 0x00169862
		public override int maximumStackSize()
		{
			return 1;
		}

		// Token: 0x17000337 RID: 823
		// (get) Token: 0x06001FA8 RID: 8104 RVA: 0x0016B665 File Offset: 0x00169865
		[XmlIgnore]
		public override string Name
		{
			get
			{
				return base.name;
			}
		}

		// Token: 0x06001FA9 RID: 8105 RVA: 0x0016B66D File Offset: 0x0016986D
		public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
		{
			base.drawInMenu(spriteBatch, objectPosition, 1f);
		}

		// Token: 0x06001FAA RID: 8106 RVA: 0x0016B67C File Offset: 0x0016987C
		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			base.AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
			Texture2D wallpaperTexture;
			if (this.GetSetData() != null)
			{
				try
				{
					wallpaperTexture = Game1.content.Load<Texture2D>(this.GetSetData().Texture);
					goto IL_4D;
				}
				catch (Exception)
				{
					wallpaperTexture = Game1.content.Load<Texture2D>("Maps\\walls_and_floors");
					goto IL_4D;
				}
			}
			wallpaperTexture = Game1.content.Load<Texture2D>("Maps\\walls_and_floors");
			IL_4D:
			if (this.isFloor.Value)
			{
				spriteBatch.Draw(Game1.mouseCursors2, location + new Vector2(32f, 32f), new Rectangle?(Wallpaper.floorContainerRect), color * transparency, 0f, new Vector2(8f, 8f), 4f * scaleSize, SpriteEffects.None, layerDepth);
				spriteBatch.Draw(wallpaperTexture, location + new Vector2(32f, 30f), new Rectangle?(this.sourceRect.Value), color * transparency, 0f, new Vector2(14f, 13f), 2f * scaleSize, SpriteEffects.None, layerDepth + 0.001f);
			}
			else
			{
				spriteBatch.Draw(Game1.mouseCursors2, location + new Vector2(32f, 32f), new Rectangle?(Wallpaper.wallpaperContainerRect), color * transparency, 0f, new Vector2(8f, 8f), 4f * scaleSize, SpriteEffects.None, layerDepth);
				spriteBatch.Draw(wallpaperTexture, location + new Vector2(32f, 32f), new Rectangle?(this.sourceRect.Value), color * transparency, 0f, new Vector2(8f, 14f), 2f * scaleSize, SpriteEffects.None, layerDepth + 0.001f);
			}
			this.DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
		}

		// Token: 0x06001FAB RID: 8107 RVA: 0x0016B860 File Offset: 0x00169A60
		protected override Item GetOneNew()
		{
			ModWallpaperOrFlooring data = this.GetSetData();
			if (data == null)
			{
				return new Wallpaper(this.parentSheetIndex.Value, this.isFloor.Value);
			}
			return new Wallpaper(data.Id, this.parentSheetIndex.Value);
		}

		// Token: 0x04001364 RID: 4964
		[XmlElement("sourceRect")]
		public readonly NetRectangle sourceRect = new NetRectangle();

		// Token: 0x04001365 RID: 4965
		[XmlElement("isFloor")]
		public readonly NetBool isFloor = new NetBool(false);

		// Token: 0x04001366 RID: 4966
		[XmlElement("sourceTexture")]
		public readonly NetString setId = new NetString(null);

		// Token: 0x04001367 RID: 4967
		protected ModWallpaperOrFlooring setData;

		// Token: 0x04001368 RID: 4968
		private static readonly Rectangle wallpaperContainerRect = new Rectangle(39, 31, 16, 16);

		// Token: 0x04001369 RID: 4969
		private static readonly Rectangle floorContainerRect = new Rectangle(55, 31, 16, 16);
	}
}

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.GameData.FloorsAndPaths;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Tools;

namespace StardewValley.TerrainFeatures
{
	// Token: 0x02000140 RID: 320
	public class Flooring : TerrainFeature
	{
		// Token: 0x0600194E RID: 6478 RVA: 0x0012931C File Offset: 0x0012751C
		public Flooring() : base(false)
		{
			this.loadSprite();
			if (Flooring.drawGuide == null)
			{
				Flooring.populateDrawGuide();
			}
		}

		// Token: 0x0600194F RID: 6479 RVA: 0x00129358 File Offset: 0x00127558
		public Flooring(string which) : this()
		{
			this.whichFloor.Value = which;
			this.ApplyFlooringFlags();
		}

		// Token: 0x06001950 RID: 6480 RVA: 0x00129372 File Offset: 0x00127572
		public override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.whichFloor, "whichFloor").AddField(this.whichView, "whichView");
		}

		// Token: 0x06001951 RID: 6481 RVA: 0x001293A1 File Offset: 0x001275A1
		public virtual void ApplyFlooringFlags()
		{
			FloorPathData data = this.GetData();
			if (data != null && data.ConnectType == FloorPathConnectType.Random)
			{
				this.whichView.Value = Game1.random.Next(16);
			}
		}

		// Token: 0x06001952 RID: 6482 RVA: 0x001293D1 File Offset: 0x001275D1
		public static Dictionary<string, string> GetFloorPathItemLookup()
		{
			if (Flooring._FloorPathItemLookup == null)
			{
				Flooring.LoadFloorPathItemLookup();
			}
			return Flooring._FloorPathItemLookup;
		}

		// Token: 0x06001953 RID: 6483 RVA: 0x001293E4 File Offset: 0x001275E4
		public FloorPathData GetData()
		{
			FloorPathData data;
			if (!Flooring.TryGetData(this.whichFloor.Value, out data))
			{
				return null;
			}
			return data;
		}

		// Token: 0x06001954 RID: 6484 RVA: 0x00129408 File Offset: 0x00127608
		public static bool TryGetData(string id, out FloorPathData data)
		{
			if (id == null)
			{
				data = null;
				return false;
			}
			return Game1.floorPathData.TryGetValue(id, out data);
		}

		// Token: 0x06001955 RID: 6485 RVA: 0x00129420 File Offset: 0x00127620
		protected static void LoadFloorPathItemLookup()
		{
			Flooring._FloorPathItemLookup = new Dictionary<string, string>();
			foreach (KeyValuePair<string, FloorPathData> pair in Game1.floorPathData)
			{
				string floorId = pair.Key;
				string itemId = pair.Value.ItemId;
				if (!string.IsNullOrEmpty(itemId))
				{
					Flooring._FloorPathItemLookup[itemId] = floorId;
				}
			}
		}

		// Token: 0x06001956 RID: 6486 RVA: 0x00129498 File Offset: 0x00127698
		public override Rectangle getBoundingBox()
		{
			Vector2 tileLocation = this.Tile;
			return new Rectangle((int)(tileLocation.X * 64f), (int)(tileLocation.Y * 64f), 64, 64);
		}

		// Token: 0x06001957 RID: 6487 RVA: 0x001294D0 File Offset: 0x001276D0
		public static void populateDrawGuide()
		{
			Dictionary<byte, int> dictionary = new Dictionary<byte, int>();
			dictionary[0] = 0;
			dictionary[6] = 1;
			dictionary[14] = 2;
			dictionary[12] = 3;
			dictionary[4] = 16;
			dictionary[7] = 17;
			dictionary[15] = 18;
			dictionary[13] = 19;
			dictionary[5] = 32;
			dictionary[3] = 33;
			dictionary[11] = 34;
			dictionary[9] = 35;
			dictionary[1] = 48;
			dictionary[2] = 49;
			dictionary[10] = 50;
			dictionary[8] = 51;
			Flooring.drawGuide = dictionary;
			Flooring.drawGuideList = new List<int>(Flooring.drawGuide.Count);
			foreach (KeyValuePair<byte, int> pair in Flooring.drawGuide)
			{
				Flooring.drawGuideList.Add(pair.Value);
			}
		}

		// Token: 0x06001958 RID: 6488 RVA: 0x001295E0 File Offset: 0x001277E0
		public override void loadSprite()
		{
		}

		// Token: 0x06001959 RID: 6489 RVA: 0x001295E4 File Offset: 0x001277E4
		public override void doCollisionAction(Rectangle positionOfCollider, int speedOfCollision, Vector2 tileLocation, Character who)
		{
			base.doCollisionAction(positionOfCollider, speedOfCollision, tileLocation, who);
			FloorPathData data = this.GetData();
			GameLocation location = this.Location;
			Farmer player = who as Farmer;
			if (player != null && (location is Farm || location is IslandWest))
			{
				float speedBuff = 0.1f;
				if (data != null && data.FarmSpeedBuff >= 0f)
				{
					speedBuff = data.FarmSpeedBuff;
				}
				player.temporarySpeedBuff = speedBuff;
			}
		}

		// Token: 0x0600195A RID: 6490 RVA: 0x00129649 File Offset: 0x00127849
		public override bool isPassable(Character c = null)
		{
			return true;
		}

		// Token: 0x0600195B RID: 6491 RVA: 0x0012964C File Offset: 0x0012784C
		public string getFootstepSound()
		{
			FloorPathData data = this.GetData();
			return ((data != null) ? data.FootstepSound : null) ?? "stoneStep";
		}

		// Token: 0x0600195C RID: 6492 RVA: 0x00129669 File Offset: 0x00127869
		public Point GetTextureCorner(bool useSeasonalVariants = true)
		{
			if (!useSeasonalVariants || !this.ShouldDrawWinterVersion())
			{
				return this.GetData().Corner;
			}
			return this.GetData().WinterCorner;
		}

		// Token: 0x0600195D RID: 6493 RVA: 0x00129690 File Offset: 0x00127890
		public Texture2D GetTexture(bool useSeasonalVariants = true)
		{
			if (useSeasonalVariants && this.ShouldDrawWinterVersion())
			{
				if (this.floorTextureWinter == null)
				{
					this.floorTextureWinter = Game1.content.Load<Texture2D>(this.GetData().WinterTexture);
				}
				return this.floorTextureWinter;
			}
			if (this.floorTexture == null)
			{
				this.floorTexture = Game1.content.Load<Texture2D>(this.GetData().Texture);
			}
			return this.floorTexture;
		}

		// Token: 0x0600195E RID: 6494 RVA: 0x001296FB File Offset: 0x001278FB
		public bool ShouldDrawWinterVersion()
		{
			return this.Location != null && !this.Location.isGreenhouse.Value && this.GetData().WinterTexture != null && this.Location.IsWinterHere();
		}

		// Token: 0x0600195F RID: 6495 RVA: 0x00129734 File Offset: 0x00127934
		public override bool performToolAction(Tool t, int damage, Vector2 tileLocation)
		{
			GameLocation location = this.Location ?? Game1.currentLocation;
			if ((t != null || damage > 0) && (damage > 0 || t is Pickaxe || t is Axe))
			{
				FloorPathData data = this.GetData();
				if (data != null)
				{
					location.playSound(data.RemovalSound ?? data.PlacementSound, new Vector2?(tileLocation), null, SoundContext.Default);
					Game1.createRadialDebris(location, data.RemovalDebrisType, (int)tileLocation.X, (int)tileLocation.Y, 4, false, -1, false, null);
					if (data.ItemId != null)
					{
						Item floorItem = ItemRegistry.Create(data.ItemId, 1, 0, false);
						if (floorItem != null)
						{
							location.debris.Add(new Debris(floorItem, tileLocation * 64f + new Vector2(32f, 32f)));
						}
					}
				}
				return true;
			}
			return false;
		}

		// Token: 0x06001960 RID: 6496 RVA: 0x0012981C File Offset: 0x00127A1C
		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 positionOnScreen, Vector2 tileLocation, float scale, float layerDepth)
		{
		}

		// Token: 0x06001961 RID: 6497 RVA: 0x00129820 File Offset: 0x00127A20
		public override void draw(SpriteBatch spriteBatch)
		{
			Vector2 tileLocation = this.Tile;
			FloorPathData data = this.GetData();
			if (data == null)
			{
				IItemDataDefinition itemType = ItemRegistry.RequireTypeDefinition("(O)");
				spriteBatch.Draw(itemType.GetErrorTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle?(itemType.GetErrorSourceRect()), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-09f);
				return;
			}
			Texture2D texture = this.GetTexture(true);
			Point corner = this.GetTextureCorner(true);
			float cornerSortOffset = 1f;
			switch (data.ConnectType)
			{
			case FloorPathConnectType.Default:
			{
				int borderSize = data.CornerSize;
				if ((this.neighborMask & 9) == 9 && (this.neighborMask & 32) == 0)
				{
					spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle?(new Rectangle(64 - borderSize + corner.X, 48 - borderSize + corner.Y, borderSize, borderSize)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y * 64f + 2f + tileLocation.X / 10000f) / 20000f);
				}
				if ((this.neighborMask & 3) == 3 && (this.neighborMask & 16) == 0)
				{
					spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 64f - (float)(borderSize * 4), tileLocation.Y * 64f)), new Rectangle?(new Rectangle(16 + corner.X, 48 - borderSize + corner.Y, borderSize, borderSize)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y * 64f + 2f + tileLocation.X / 10000f + cornerSortOffset) / 20000f);
				}
				if ((this.neighborMask & 6) == 6 && (this.neighborMask & 64) == 0)
				{
					spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 64f - (float)(borderSize * 4), tileLocation.Y * 64f + 48f)), new Rectangle?(new Rectangle(16 + corner.X, corner.Y, borderSize, borderSize)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y * 64f + 2f + tileLocation.X / 10000f) / 20000f);
				}
				if ((this.neighborMask & 12) == 12 && (this.neighborMask & 128) == 0)
				{
					spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f + 64f - (float)(borderSize * 4))), new Rectangle?(new Rectangle(64 - borderSize + corner.X, corner.Y, borderSize, borderSize)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y * 64f + 2f + tileLocation.X / 10000f) / 20000f);
				}
				break;
			}
			case FloorPathConnectType.CornerDecorated:
			{
				int border_size = data.CornerSize;
				if ((this.neighborMask & 9) == 9 && (this.neighborMask & 32) == 0)
				{
					spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle?(new Rectangle(64 - border_size + corner.X, 48 - border_size + corner.Y, border_size, border_size)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y * 64f + 2f + tileLocation.X / 10000f) / 20000f);
				}
				if ((this.neighborMask & 3) == 3 && (this.neighborMask & 16) == 0)
				{
					spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 64f - (float)(border_size * 4), tileLocation.Y * 64f)), new Rectangle?(new Rectangle(16 + corner.X, 48 - border_size + corner.Y, border_size, border_size)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y * 64f + 2f + tileLocation.X / 10000f + cornerSortOffset) / 20000f);
				}
				if ((this.neighborMask & 6) == 6 && (this.neighborMask & 64) == 0)
				{
					spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 64f - (float)(border_size * 4), tileLocation.Y * 64f + 64f - (float)(border_size * 4))), new Rectangle?(new Rectangle(16 + corner.X, corner.Y, border_size, border_size)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y * 64f + 2f + tileLocation.X / 10000f) / 20000f);
				}
				if ((this.neighborMask & 12) == 12 && (this.neighborMask & 128) == 0)
				{
					spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f + 64f - (float)(border_size * 4))), new Rectangle?(new Rectangle(64 - border_size + corner.X, corner.Y, border_size, border_size)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y * 64f + 2f + tileLocation.X / 10000f) / 20000f);
				}
				break;
			}
			}
			byte drawSum = this.neighborMask & 15;
			int sourceRectPosition = Flooring.drawGuide[drawSum];
			if (data.ConnectType == FloorPathConnectType.Random)
			{
				sourceRectPosition = Flooring.drawGuideList[this.whichView.Value];
			}
			FloorPathShadowType shadowType = data.ShadowType;
			if (shadowType != FloorPathShadowType.Square)
			{
				if (shadowType == FloorPathShadowType.Contoured)
				{
					Color shadowColor = Color.Black;
					shadowColor.A = (byte)((float)shadowColor.A * 0.33f);
					spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)) + new Vector2(-4f, 4f), new Rectangle?(new Rectangle(corner.X + sourceRectPosition * 16 % 256, sourceRectPosition / 16 * 16 + corner.Y, 16, 16)), shadowColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-10f);
				}
			}
			else
			{
				spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)(tileLocation.X * 64f) - 4 - Game1.viewport.X, (int)(tileLocation.Y * 64f) + 4 - Game1.viewport.Y, 64, 64), Color.Black * 0.33f);
			}
			spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle?(new Rectangle(corner.X + sourceRectPosition * 16 % 256, sourceRectPosition / 16 * 16 + corner.Y, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-09f);
		}

		// Token: 0x06001962 RID: 6498 RVA: 0x0012A054 File Offset: 0x00128254
		public override bool tickUpdate(GameTime time)
		{
			base.NeedsUpdate = false;
			return false;
		}

		// Token: 0x06001963 RID: 6499 RVA: 0x0012A060 File Offset: 0x00128260
		private List<Flooring.Neighbor> gatherNeighbors()
		{
			List<Flooring.Neighbor> results = this._neighbors;
			results.Clear();
			GameLocation loc = this.Location;
			Vector2 tilePos = this.Tile;
			NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>> terrainFeatures = loc.terrainFeatures;
			foreach (Flooring.NeighborLoc item in Flooring._offsets)
			{
				Vector2 tile = tilePos + item.Offset;
				TerrainFeature feature;
				if (loc.map != null && !loc.isTileOnMap(tile))
				{
					Flooring.Neighbor i = new Flooring.Neighbor(null, item.Direction, item.InvDirection);
					results.Add(i);
				}
				else if (terrainFeatures.TryGetValue(tile, out feature))
				{
					Flooring flooring = feature as Flooring;
					if (flooring != null && flooring.whichFloor.Value == this.whichFloor.Value)
					{
						Flooring.Neighbor j = new Flooring.Neighbor(flooring, item.Direction, item.InvDirection);
						results.Add(j);
					}
				}
			}
			return results;
		}

		// Token: 0x06001964 RID: 6500 RVA: 0x0012A154 File Offset: 0x00128354
		public void OnAdded(GameLocation loc, Vector2 tilePos)
		{
			this.Location = loc;
			this.Tile = tilePos;
			List<Flooring.Neighbor> list = this.gatherNeighbors();
			this.neighborMask = 0;
			foreach (Flooring.Neighbor i in list)
			{
				this.neighborMask |= i.direction;
				Flooring feature = i.feature;
				if (feature != null)
				{
					feature.OnNeighborAdded(i.invDirection);
				}
			}
		}

		// Token: 0x06001965 RID: 6501 RVA: 0x0012A1E0 File Offset: 0x001283E0
		public void OnRemoved()
		{
			List<Flooring.Neighbor> list = this.gatherNeighbors();
			this.neighborMask = 0;
			foreach (Flooring.Neighbor i in list)
			{
				Flooring feature = i.feature;
				if (feature != null)
				{
					feature.OnNeighborRemoved(i.invDirection);
				}
			}
		}

		// Token: 0x06001966 RID: 6502 RVA: 0x0012A24C File Offset: 0x0012844C
		public void OnNeighborAdded(byte direction)
		{
			this.neighborMask |= direction;
		}

		// Token: 0x06001967 RID: 6503 RVA: 0x0012A25D File Offset: 0x0012845D
		public void OnNeighborRemoved(byte direction)
		{
			this.neighborMask &= ~direction;
		}

		// Token: 0x04000F32 RID: 3890
		public const byte N = 1;

		// Token: 0x04000F33 RID: 3891
		public const byte E = 2;

		// Token: 0x04000F34 RID: 3892
		public const byte S = 4;

		// Token: 0x04000F35 RID: 3893
		public const byte W = 8;

		// Token: 0x04000F36 RID: 3894
		public const byte NE = 16;

		// Token: 0x04000F37 RID: 3895
		public const byte NW = 32;

		// Token: 0x04000F38 RID: 3896
		public const byte SE = 64;

		// Token: 0x04000F39 RID: 3897
		public const byte SW = 128;

		// Token: 0x04000F3A RID: 3898
		public const byte Cardinals = 15;

		// Token: 0x04000F3B RID: 3899
		public static readonly Vector2 N_Offset = new Vector2(0f, -1f);

		// Token: 0x04000F3C RID: 3900
		public static readonly Vector2 E_Offset = new Vector2(1f, 0f);

		// Token: 0x04000F3D RID: 3901
		public static readonly Vector2 S_Offset = new Vector2(0f, 1f);

		// Token: 0x04000F3E RID: 3902
		public static readonly Vector2 W_Offset = new Vector2(-1f, 0f);

		// Token: 0x04000F3F RID: 3903
		public static readonly Vector2 NE_Offset = new Vector2(1f, -1f);

		// Token: 0x04000F40 RID: 3904
		public static readonly Vector2 NW_Offset = new Vector2(-1f, -1f);

		// Token: 0x04000F41 RID: 3905
		public static readonly Vector2 SE_Offset = new Vector2(1f, 1f);

		// Token: 0x04000F42 RID: 3906
		public static readonly Vector2 SW_Offset = new Vector2(-1f, 1f);

		// Token: 0x04000F43 RID: 3907
		public const string wood = "0";

		// Token: 0x04000F44 RID: 3908
		public const string stone = "1";

		// Token: 0x04000F45 RID: 3909
		public const string ghost = "2";

		// Token: 0x04000F46 RID: 3910
		public const string iceTile = "3";

		// Token: 0x04000F47 RID: 3911
		public const string straw = "4";

		// Token: 0x04000F48 RID: 3912
		public const string gravel = "5";

		// Token: 0x04000F49 RID: 3913
		public const string boardwalk = "6";

		// Token: 0x04000F4A RID: 3914
		public const string colored_cobblestone = "7";

		// Token: 0x04000F4B RID: 3915
		public const string cobblestone = "8";

		// Token: 0x04000F4C RID: 3916
		public const string steppingStone = "9";

		// Token: 0x04000F4D RID: 3917
		public const string brick = "10";

		// Token: 0x04000F4E RID: 3918
		public const string plankFlooring = "11";

		// Token: 0x04000F4F RID: 3919
		public const string townFlooring = "12";

		// Token: 0x04000F50 RID: 3920
		[XmlIgnore]
		public Texture2D floorTexture;

		// Token: 0x04000F51 RID: 3921
		[XmlIgnore]
		public Texture2D floorTextureWinter;

		// Token: 0x04000F52 RID: 3922
		[InstancedStatic]
		public static Dictionary<byte, int> drawGuide;

		// Token: 0x04000F53 RID: 3923
		[InstancedStatic]
		public static List<int> drawGuideList;

		// Token: 0x04000F54 RID: 3924
		[XmlElement("whichFloor")]
		public readonly NetString whichFloor = new NetString();

		// Token: 0x04000F55 RID: 3925
		[XmlElement("whichView")]
		public readonly NetInt whichView = new NetInt();

		// Token: 0x04000F56 RID: 3926
		private byte neighborMask;

		// Token: 0x04000F57 RID: 3927
		protected static Dictionary<string, string> _FloorPathItemLookup;

		// Token: 0x04000F58 RID: 3928
		private static readonly Flooring.NeighborLoc[] _offsets = new Flooring.NeighborLoc[]
		{
			new Flooring.NeighborLoc(Flooring.N_Offset, 1, 4),
			new Flooring.NeighborLoc(Flooring.S_Offset, 4, 1),
			new Flooring.NeighborLoc(Flooring.E_Offset, 2, 8),
			new Flooring.NeighborLoc(Flooring.W_Offset, 8, 2),
			new Flooring.NeighborLoc(Flooring.NE_Offset, 16, 128),
			new Flooring.NeighborLoc(Flooring.NW_Offset, 32, 64),
			new Flooring.NeighborLoc(Flooring.SE_Offset, 64, 32),
			new Flooring.NeighborLoc(Flooring.SW_Offset, 128, 16)
		};

		// Token: 0x04000F59 RID: 3929
		private List<Flooring.Neighbor> _neighbors = new List<Flooring.Neighbor>();

		// Token: 0x02000520 RID: 1312
		private struct NeighborLoc
		{
			// Token: 0x060040AC RID: 16556 RVA: 0x003039F8 File Offset: 0x00301BF8
			public NeighborLoc(Vector2 a, byte b, byte c)
			{
				this.Offset = a;
				this.Direction = b;
				this.InvDirection = c;
			}

			// Token: 0x04002A9D RID: 10909
			public readonly Vector2 Offset;

			// Token: 0x04002A9E RID: 10910
			public readonly byte Direction;

			// Token: 0x04002A9F RID: 10911
			public readonly byte InvDirection;
		}

		// Token: 0x02000521 RID: 1313
		private struct Neighbor
		{
			// Token: 0x060040AD RID: 16557 RVA: 0x00303A0F File Offset: 0x00301C0F
			public Neighbor(Flooring a, byte b, byte c)
			{
				this.feature = a;
				this.direction = b;
				this.invDirection = c;
			}

			// Token: 0x04002AA0 RID: 10912
			public readonly Flooring feature;

			// Token: 0x04002AA1 RID: 10913
			public readonly byte direction;

			// Token: 0x04002AA2 RID: 10914
			public readonly byte invDirection;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.GiantCrops;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Logging;
using StardewValley.Tools;

namespace StardewValley.TerrainFeatures
{
	// Token: 0x02000142 RID: 322
	public class GiantCrop : ResourceClump
	{
		// Token: 0x170002C6 RID: 710
		// (get) Token: 0x0600198D RID: 6541 RVA: 0x0012D178 File Offset: 0x0012B378
		// (set) Token: 0x0600198E RID: 6542 RVA: 0x0012D1AE File Offset: 0x0012B3AE
		[XmlIgnore]
		public string Id
		{
			get
			{
				if (this.netId.Value == null)
				{
					this.netId.Value = this.GetIdFromLegacySpriteIndex(this.parentSheetIndex.Value);
				}
				return this.netId.Value;
			}
			set
			{
				this.netId.Value = value;
			}
		}

		// Token: 0x0600198F RID: 6543 RVA: 0x0012D1BC File Offset: 0x0012B3BC
		public GiantCrop()
		{
		}

		// Token: 0x06001990 RID: 6544 RVA: 0x0012D1D0 File Offset: 0x0012B3D0
		public GiantCrop(string id, Vector2 tile) : this()
		{
			this.Tile = tile;
			this.Id = id;
			GiantCropData data = this.GetData();
			this.width.Value = ((data != null) ? data.TileSize.X : 3);
			this.height.Value = ((data != null) ? data.TileSize.Y : 3);
			this.health.Value = (float)((data != null) ? data.Health : 3);
		}

		// Token: 0x06001991 RID: 6545 RVA: 0x0012D248 File Offset: 0x0012B448
		public override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.netId, "netId");
		}

		// Token: 0x06001992 RID: 6546 RVA: 0x0012D268 File Offset: 0x0012B468
		public override void draw(SpriteBatch spriteBatch)
		{
			Vector2 tileLocation = this.Tile;
			GiantCropData data = this.GetData();
			if (data != null)
			{
				Texture2D texture = Game1.content.Load<Texture2D>(data.Texture);
				spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, tileLocation * 64f - new Vector2((this.shakeTimer > 0f) ? ((float)Math.Sin(6.283185307179586 / (double)this.shakeTimer) * 2f) : 0f, 64f)), new Rectangle?(new Rectangle(data.TexturePosition.X, data.TexturePosition.Y, 16 * data.TileSize.X, 16 * (data.TileSize.Y + 1))), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y + (float)data.TileSize.Y) * 64f / 10000f);
				return;
			}
			IItemDataDefinition itemType = ItemRegistry.RequireTypeDefinition("(O)");
			spriteBatch.Draw(itemType.GetErrorTexture(), Game1.GlobalToLocal(Game1.viewport, tileLocation * 64f - new Vector2((this.shakeTimer > 0f) ? ((float)Math.Sin(6.283185307179586 / (double)this.shakeTimer) * 2f) : 0f, 64f)), new Rectangle?(itemType.GetErrorSourceRect()), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y + 2f) * 64f / 10000f);
		}

		// Token: 0x06001993 RID: 6547 RVA: 0x0012D410 File Offset: 0x0012B610
		public override bool performToolAction(Tool t, int damage, Vector2 tileLocation)
		{
			if (!(t is Axe))
			{
				return false;
			}
			GameLocation location = this.Location;
			Farmer player = t.getLastFarmerToUse() ?? Game1.player;
			int power = t.upgradeLevel.Value / 2 + 1;
			float healthDeducted = Math.Min(this.health.Value, (float)power);
			GiantCropData data = this.GetData();
			Random r = Game1.IsMultiplayer ? (Game1.recentMultiplayerRandom = Utility.CreateRandom((double)tileLocation.X * 1000.0, (double)tileLocation.Y, 0.0, 0.0, 0.0)) : Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, (double)tileLocation.X * 7.0, (double)tileLocation.Y * 11.0, 0.0);
			location.playSound("axchop", new Vector2?(tileLocation), null, SoundContext.Default);
			Game1.createRadialDebris(Game1.currentLocation, 12, (int)tileLocation.X + this.width.Value / 2, (int)tileLocation.Y + this.height.Value / 2, r.Next(4, 9), false, -1, false, null);
			if (this.shakeTimer <= 0f)
			{
				this.shakeTimer = 100f;
				base.NeedsUpdate = true;
			}
			if (t.hasEnchantmentOfType<ShavingEnchantment>() && r.NextBool((float)power / 5f) && ((data != null) ? data.HarvestItems : null) != null)
			{
				foreach (GiantCropHarvestItemData drop in data.HarvestItems)
				{
					Item item = this.TryGetDrop(drop, r, player, true, healthDeducted);
					if (item != null)
					{
						if (this.Id.Equals("QiFruit") && !Game1.player.team.SpecialOrderActive("QiChallenge2"))
						{
							break;
						}
						Debris d = new Debris(item, new Vector2((tileLocation.X + (float)(this.width.Value / 2)) * 64f, (tileLocation.Y + (float)(this.height.Value / 2)) * 64f), Game1.player.getStandingPosition());
						d.Chunks[0].xVelocity.Value += (float)r.Next(-10, 11) / 10f;
						d.chunkFinalYLevel = (int)(tileLocation.Y * 64f + 128f);
						location.debris.Add(d);
					}
				}
			}
			this.health.Value -= (float)power;
			if (this.health.Value <= 0f)
			{
				t.getLastFarmerToUse().gainExperience(5, 50 * ((t.getLastFarmerToUse().luckLevel.Value + 1) / 2));
				if (location.HasUnlockedAreaSecretNotes(t.getLastFarmerToUse()))
				{
					Object o = location.tryToCreateUnseenSecretNote(t.getLastFarmerToUse());
					if (o != null)
					{
						Game1.createItemDebris(o, tileLocation * 64f, -1, location, -1, false);
					}
				}
				if (((data != null) ? data.HarvestItems : null) != null)
				{
					foreach (GiantCropHarvestItemData drop2 in data.HarvestItems)
					{
						Item item2 = this.TryGetDrop(drop2, r, player, false, healthDeducted);
						if (item2 != null)
						{
							if (this.Id.Equals("QiFruit") && !Game1.player.team.SpecialOrderActive("QiChallenge2"))
							{
								if (!Game1.player.mailReceived.Contains("GiantQiFruitMessage"))
								{
									Game1.player.mailReceived.Add("GiantQiFruitMessage");
									Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\1_6_Strings:GiantQiFruitMessage"), new Color(100, 50, 255));
								}
								Game1.createMultipleItemDebris(ItemRegistry.Create("(O)MysteryBox", 1, 0, false), new Vector2((float)((int)tileLocation.X + this.width.Value / 2), (float)((int)tileLocation.Y + this.width.Value / 2)) * 64f, -2, location, -1, false);
							}
							else
							{
								Game1.createMultipleItemDebris(item2, new Vector2((float)((int)tileLocation.X + this.width.Value / 2), (float)((int)tileLocation.Y + this.width.Value / 2)) * 64f, -2, location, -1, false);
								Game1.setRichPresence("giantcrop", item2.Name);
							}
						}
					}
				}
				Game1.createRadialDebris(Game1.currentLocation, 12, (int)tileLocation.X + this.width.Value / 2, (int)tileLocation.Y + this.width.Value / 2, r.Next(4, 9), false, -1, false, null);
				location.playSound("stumpCrack", new Vector2?(tileLocation), null, SoundContext.Default);
				for (int x = 0; x < this.width.Value; x++)
				{
					for (int y = 0; y < this.height.Value; y++)
					{
						float animationInterval = Utility.RandomFloat(80f, 110f, null);
						if (this.width.Value >= 2 && this.height.Value >= 2 && (x == 0 || x == this.width.Value - 2) && (y == 0 || y == this.height.Value - 2))
						{
							Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
							{
								new TemporaryAnimatedSprite(5, (tileLocation + new Vector2((float)x + 0.5f, (float)y + 0.5f)) * 64f, Color.White, 8, false, 70f, 0, -1, -1f, -1, 0)
							});
						}
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite(5, (tileLocation + new Vector2((float)x, (float)y)) * 64f, Color.White, 8, false, animationInterval, 0, -1, -1f, -1, 0)
						});
					}
				}
				return true;
			}
			return false;
		}

		// Token: 0x06001994 RID: 6548 RVA: 0x0012DABC File Offset: 0x0012BCBC
		public GiantCropData GetData()
		{
			GiantCropData data;
			if (!GiantCrop.TryGetData(this.Id, out data))
			{
				return null;
			}
			return data;
		}

		// Token: 0x06001995 RID: 6549 RVA: 0x0012DADB File Offset: 0x0012BCDB
		public static bool TryGetData(string id, out GiantCropData data)
		{
			if (id == null)
			{
				data = null;
				return false;
			}
			return DataLoader.GiantCrops(Game1.content).TryGetValue(id, out data);
		}

		// Token: 0x06001996 RID: 6550 RVA: 0x0012DAF8 File Offset: 0x0012BCF8
		public static IReadOnlyList<KeyValuePair<string, GiantCropData>> GetGiantCropsFor(string cropId)
		{
			cropId = ItemRegistry.QualifyItemId(cropId);
			if (cropId != null)
			{
				GiantCrop.RebuildCropIdCacheIfNeeded(false);
				List<KeyValuePair<string, GiantCropData>> giantCrops;
				if (GiantCrop.CacheByCropId.TryGetValue(cropId, out giantCrops))
				{
					return giantCrops;
				}
			}
			return LegacyShims.EmptyArray<KeyValuePair<string, GiantCropData>>();
		}

		// Token: 0x06001997 RID: 6551 RVA: 0x0012DB30 File Offset: 0x0012BD30
		public static bool RebuildCropIdCacheIfNeeded(bool forceRebuild = false)
		{
			if (!forceRebuild && GiantCrop.CacheTick == Game1.ticks)
			{
				return false;
			}
			GiantCrop.CacheTick = Game1.ticks;
			GiantCrop.CacheByCropId.Clear();
			foreach (KeyValuePair<string, GiantCropData> pair in DataLoader.GiantCrops(Game1.content))
			{
				string fromItemId = ItemRegistry.QualifyItemId(pair.Value.FromItemId);
				if (fromItemId != null)
				{
					List<KeyValuePair<string, GiantCropData>> list;
					if (!GiantCrop.CacheByCropId.TryGetValue(fromItemId, out list))
					{
						list = (GiantCrop.CacheByCropId[fromItemId] = new List<KeyValuePair<string, GiantCropData>>());
					}
					list.Add(pair);
				}
			}
			return true;
		}

		// Token: 0x06001998 RID: 6552 RVA: 0x0012DBE4 File Offset: 0x0012BDE4
		public Item TryGetDrop(GiantCropHarvestItemData drop, Random r, Farmer targetFarmer, bool isShaving, float healthDeducted)
		{
			if (!r.NextBool(drop.Chance))
			{
				return null;
			}
			if (drop.Condition != null && !GameStateQuery.CheckConditions(drop.Condition, this.Location, targetFarmer, null, null, r, null))
			{
				return null;
			}
			if (drop.ForShavingEnchantment != null)
			{
				bool? forShavingEnchantment = drop.ForShavingEnchantment;
				if (!(forShavingEnchantment.GetValueOrDefault() == isShaving & forShavingEnchantment != null))
				{
					return null;
				}
			}
			ISpawnItemData drop2 = drop;
			GameLocation location = this.Location;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(29, 2);
			defaultInterpolatedStringHandler.AppendLiteral("giant crop ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Id);
			defaultInterpolatedStringHandler.AppendLiteral(" > harvest item '");
			defaultInterpolatedStringHandler.AppendFormatted(drop.Id);
			defaultInterpolatedStringHandler.AppendLiteral("'");
			Item item = ItemQueryResolver.TryResolveRandomItem(drop2, new ItemQueryContext(location, targetFarmer, r, defaultInterpolatedStringHandler.ToStringAndClear()), false, null, null, null, delegate(string query, string error)
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(64, 4);
				defaultInterpolatedStringHandler2.AppendLiteral("Giant crop '");
				defaultInterpolatedStringHandler2.AppendFormatted(this.Id);
				defaultInterpolatedStringHandler2.AppendLiteral("' failed parsing item query '");
				defaultInterpolatedStringHandler2.AppendFormatted(query);
				defaultInterpolatedStringHandler2.AppendLiteral("' for harvest item '");
				defaultInterpolatedStringHandler2.AppendFormatted(drop.Id);
				defaultInterpolatedStringHandler2.AppendLiteral("': ");
				defaultInterpolatedStringHandler2.AppendFormatted(error);
				log.Error(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
			});
			if (isShaving)
			{
				this.AdjustStackSizeWhenShaving(item, drop.ScaledMinStackWhenShaving, drop.ScaledMaxStackWhenShaving, healthDeducted, r);
			}
			return item;
		}

		// Token: 0x06001999 RID: 6553 RVA: 0x0012DD24 File Offset: 0x0012BF24
		private void AdjustStackSizeWhenShaving(Item item, int? min, int? max, float healthDeducted, Random random)
		{
			if (item == null || (min == null && max == null))
			{
				return;
			}
			int? num;
			if (min != null)
			{
				num = min;
				min = new int?((int)(((num != null) ? new float?((float)num.GetValueOrDefault()) : null) * healthDeducted).Value);
			}
			if (max != null)
			{
				num = max;
				max = new int?((int)(((num != null) ? new float?((float)num.GetValueOrDefault()) : null) * healthDeducted).Value);
			}
			if (min != null && max != null)
			{
				item.Stack = random.Next(min.Value, max.Value + 1);
				return;
			}
			int stack = item.Stack;
			num = min;
			if (stack < num.GetValueOrDefault() & num != null)
			{
				item.Stack = min.Value;
				return;
			}
			int stack2 = item.Stack;
			num = max;
			if (stack2 > num.GetValueOrDefault() & num != null)
			{
				item.Stack = max.Value;
			}
		}

		// Token: 0x0600199A RID: 6554 RVA: 0x0012DE8C File Offset: 0x0012C08C
		private string GetIdFromLegacySpriteIndex(int spriteIndex)
		{
			if (spriteIndex == 190)
			{
				return "Cauliflower";
			}
			if (spriteIndex == 254)
			{
				return "Melon";
			}
			if (spriteIndex != 276)
			{
				return spriteIndex.ToString();
			}
			return "Pumpkin";
		}

		// Token: 0x04000F7F RID: 3967
		private static readonly Dictionary<string, List<KeyValuePair<string, GiantCropData>>> CacheByCropId = new Dictionary<string, List<KeyValuePair<string, GiantCropData>>>();

		// Token: 0x04000F80 RID: 3968
		private static int CacheTick;

		// Token: 0x04000F81 RID: 3969
		[XmlElement("id")]
		public readonly NetString netId = new NetString();
	}
}

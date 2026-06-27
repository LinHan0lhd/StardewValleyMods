using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.Monsters;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	// Token: 0x020002DC RID: 732
	public class IslandSecret : IslandLocation
	{
		// Token: 0x06003049 RID: 12361 RVA: 0x00262797 File Offset: 0x00260997
		public IslandSecret()
		{
		}

		// Token: 0x0600304A RID: 12362 RVA: 0x002627B5 File Offset: 0x002609B5
		public IslandSecret(string map, string name) : base(map, name)
		{
		}

		// Token: 0x0600304B RID: 12363 RVA: 0x002627D5 File Offset: 0x002609D5
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.addedSlimesToday, "addedSlimesToday");
		}

		// Token: 0x0600304C RID: 12364 RVA: 0x002627F4 File Offset: 0x002609F4
		protected override void resetSharedState()
		{
			base.resetSharedState();
			if (!this.addedSlimesToday.Value)
			{
				this.addedSlimesToday.Value = true;
				Random rand = Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame, 12.0, 0.0, 0.0);
				Microsoft.Xna.Framework.Rectangle spawnArea = new Microsoft.Xna.Framework.Rectangle(13, 15, 7, 6);
				for (int tries = 5; tries > 0; tries--)
				{
					Vector2 tile = Utility.getRandomPositionInThisRectangle(spawnArea, rand);
					if (this.CanItemBePlacedHere(tile, false, CollisionMask.All, ~CollisionMask.Objects, false, false))
					{
						GreenSlime i = new GreenSlime(tile * 64f, 9999899);
						this.characters.Add(i);
					}
				}
				if (rand.NextBool() && this.CanItemBePlacedHere(new Vector2(17f, 18f), false, CollisionMask.All, ~CollisionMask.Objects, false, false))
				{
					this.objects.Add(new Vector2(17f, 18f), ItemRegistry.Create<Object>("(BC)56", 1, 0, false));
				}
				GreenSlime slime = new GreenSlime(new Vector2(42f, 34f) * 64f);
				slime.makeTigerSlime(false);
				this.characters.Add(slime);
				slime = new GreenSlime(new Vector2(38f, 33f) * 64f);
				slime.makeTigerSlime(false);
				this.characters.Add(slime);
			}
		}

		// Token: 0x0600304D RID: 12365 RVA: 0x0026296C File Offset: 0x00260B6C
		public override string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
		{
			if (xLocation == 82 && yLocation == 83 && who.secretNotesSeen.Contains(1002))
			{
				if (!Game1.MasterPlayer.hasOrWillReceiveMail("Island_Secret_BuriedTreasureNut"))
				{
					Game1.createItemDebris(ItemRegistry.Create("(O)73", 1, 0, false), new Vector2((float)xLocation, (float)yLocation) * 64f, 1, null, -1, false);
					Game1.addMailForTomorrow("Island_Secret_BuriedTreasureNut", true, true);
				}
				if (!Game1.player.hasOrWillReceiveMail("Island_Secret_BuriedTreasure"))
				{
					Game1.createItemDebris(ItemRegistry.Create("(O)166", 1, 0, false), new Vector2((float)xLocation, (float)yLocation) * 64f, 1, null, -1, false);
					Game1.addMailForTomorrow("Island_Secret_BuriedTreasure", true, false);
				}
			}
			return base.checkForBuriedItem(xLocation, yLocation, explosion, detectOnly, who);
		}

		// Token: 0x0600304E RID: 12366 RVA: 0x00262A3C File Offset: 0x00260C3C
		protected override void resetLocalState()
		{
			base.resetLocalState();
			this.suspensionBridges.Clear();
			this.suspensionBridges.Add(new SuspensionBridge(46, 44));
			this.suspensionBridges.Add(new SuspensionBridge(47, 34));
			NPC i = base.getCharacterFromName("Birdie");
			if (i != null)
			{
				if (i.Sprite.SourceRect.Width < 32)
				{
					i.extendSourceRect(16, 0, true);
				}
				i.Sprite.SpriteWidth = 32;
				i.Sprite.ignoreSourceRectUpdates = false;
				i.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(8, 1000, 0, false, false, null, false, 0),
					new FarmerSprite.AnimationFrame(9, 1000, 0, false, false, null, false, 0)
				});
				i.Sprite.loop = true;
				i.HideShadow = true;
				i.IsInvisible = base.IsRainingHere();
			}
		}

		// Token: 0x0600304F RID: 12367 RVA: 0x00262B2C File Offset: 0x00260D2C
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			NPC birdie = base.getCharacterFromName("Birdie");
			if (birdie != null && !birdie.IsInvisible && birdie.Tile == new Vector2((float)tileLocation.X, (float)tileLocation.Y))
			{
				if (who.mailReceived.Add("birdieQuestBegun"))
				{
					Game1.globalFadeToBlack(delegate
					{
						this.startEvent(new Event(Game1.content.LoadString("Strings\\Locations:IslandSecret_Event_BirdieIntro"), null));
					}, 0.02f);
				}
				else if (!who.mailReceived.Contains("birdieQuestFinished"))
				{
					Object activeObject = who.ActiveObject;
					if (((activeObject != null) ? activeObject.QualifiedItemId : null) == "(O)870")
					{
						Game1.globalFadeToBlack(delegate
						{
							this.startEvent(new Event(Game1.content.LoadString("Strings\\Locations:IslandSecret_Event_BirdieFinished"), null));
							who.ActiveObject = null;
						}, 0.02f);
						who.mailReceived.Add("birdieQuestFinished");
					}
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		// Token: 0x06003050 RID: 12368 RVA: 0x00262C30 File Offset: 0x00260E30
		public override void DayUpdate(int dayOfMonth)
		{
			this.characters.RemoveWhere((NPC npc) => npc is Monster);
			this.addedSlimesToday.Value = false;
			base.DayUpdate(dayOfMonth);
		}

		// Token: 0x06003051 RID: 12369 RVA: 0x00262C70 File Offset: 0x00260E70
		public override bool performAction(string[] action, Farmer who, Location tileLocation)
		{
			if (ArgUtility.Get(action, 0, null, true) == "BananaShrine")
			{
				Item currentItem = who.CurrentItem;
				if (((currentItem != null) ? currentItem.QualifiedItemId : null) == "(O)91" && base.getTemporarySpriteByID(777) == null)
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", new Microsoft.Xna.Framework.Rectangle(304, 48, 16, 16), new Vector2((float)tileLocation.X, (float)(tileLocation.Y - 1)) * 64f, false, 0f, Color.White)
					{
						id = 888,
						scale = 4f,
						layerDepth = ((float)tileLocation.Y + 1.2f) * 64f / 10000f
					});
					this.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(32, 352, 32, 32), 400f, 2, 999, new Vector2(15.5f, 20f) * 64f, false, false, 0.128f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
					{
						id = 777,
						yStopCoordinate = 1561,
						motion = new Vector2(0f, 2f),
						reachedStopCoordinate = new TemporaryAnimatedSprite.endBehavior(this.gorillaReachedShrine),
						delayBeforeAnimationStart = 1000
					});
					base.playSound("coin", null, null, SoundContext.Default);
					DelayedAction.playSoundAfterDelay("grassyStep", 1400, null, null, -1, false);
					DelayedAction.playSoundAfterDelay("grassyStep", 1800, null, null, -1, false);
					DelayedAction.playSoundAfterDelay("grassyStep", 2200, null, null, -1, false);
					DelayedAction.playSoundAfterDelay("grassyStep", 2600, null, null, -1, false);
					DelayedAction.playSoundAfterDelay("grassyStep", 3000, null, null, -1, false);
					who.reduceActiveItemByOne();
					Game1.changeMusicTrack("none", false, MusicContext.Default);
					DelayedAction.playSoundAfterDelay("gorilla_intro", 2000, null, null, -1, false);
				}
				return true;
			}
			return base.performAction(action, who, tileLocation);
		}

		// Token: 0x06003052 RID: 12370 RVA: 0x00262EE0 File Offset: 0x002610E0
		private void gorillaReachedShrine(int extra)
		{
			TemporaryAnimatedSprite temporarySpriteByID = base.getTemporarySpriteByID(777);
			temporarySpriteByID.sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 352, 32, 32);
			temporarySpriteByID.sourceRectStartingPos = Utility.PointToVector2(temporarySpriteByID.sourceRect.Location);
			temporarySpriteByID.currentNumberOfLoops = 0;
			temporarySpriteByID.totalNumberOfLoops = 1;
			temporarySpriteByID.interval = 1000f;
			temporarySpriteByID.timer = 0f;
			temporarySpriteByID.motion = Vector2.Zero;
			temporarySpriteByID.animationLength = 1;
			temporarySpriteByID.endFunction = new TemporaryAnimatedSprite.endBehavior(this.gorillaGrabBanana);
		}

		// Token: 0x06003053 RID: 12371 RVA: 0x00262F6C File Offset: 0x0026116C
		private void gorillaGrabBanana(int extra)
		{
			TemporaryAnimatedSprite gorilla = base.getTemporarySpriteByID(777);
			base.removeTemporarySpritesWithID(888);
			base.playSound("slimeHit", null, null, SoundContext.Default);
			gorilla.sourceRect = new Microsoft.Xna.Framework.Rectangle(96, 352, 32, 32);
			gorilla.sourceRectStartingPos = Utility.PointToVector2(gorilla.sourceRect.Location);
			gorilla.currentNumberOfLoops = 0;
			gorilla.totalNumberOfLoops = 1;
			gorilla.interval = 1000f;
			gorilla.timer = 0f;
			gorilla.animationLength = 1;
			gorilla.endFunction = new TemporaryAnimatedSprite.endBehavior(this.gorillaEatBanana);
			this.temporarySprites.Add(gorilla);
		}

		// Token: 0x06003054 RID: 12372 RVA: 0x00263024 File Offset: 0x00261224
		private void gorillaEatBanana(int extra)
		{
			TemporaryAnimatedSprite gorilla = base.getTemporarySpriteByID(777);
			gorilla.sourceRect = new Microsoft.Xna.Framework.Rectangle(128, 352, 32, 32);
			gorilla.sourceRectStartingPos = Utility.PointToVector2(gorilla.sourceRect.Location);
			gorilla.currentNumberOfLoops = 0;
			gorilla.totalNumberOfLoops = 5;
			gorilla.interval = 300f;
			gorilla.timer = 0f;
			gorilla.animationLength = 2;
			gorilla.endFunction = new TemporaryAnimatedSprite.endBehavior(this.gorillaAfterEat);
			base.playSound("eat", null, null, SoundContext.Default);
			DelayedAction.playSoundAfterDelay("eat", 600, null, null, -1, false);
			DelayedAction.playSoundAfterDelay("eat", 1200, null, null, -1, false);
			DelayedAction.playSoundAfterDelay("eat", 1800, null, null, -1, false);
			DelayedAction.playSoundAfterDelay("eat", 2400, null, null, -1, false);
			this.temporarySprites.Add(gorilla);
		}

		// Token: 0x06003055 RID: 12373 RVA: 0x00263140 File Offset: 0x00261340
		private void gorillaAfterEat(int extra)
		{
			TemporaryAnimatedSprite gorilla = base.getTemporarySpriteByID(777);
			gorilla.sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 352, 32, 32);
			gorilla.sourceRectStartingPos = Utility.PointToVector2(gorilla.sourceRect.Location);
			gorilla.currentNumberOfLoops = 0;
			gorilla.totalNumberOfLoops = 1;
			gorilla.interval = 1000f;
			gorilla.timer = 0f;
			gorilla.motion = Vector2.Zero;
			gorilla.animationLength = 1;
			gorilla.endFunction = new TemporaryAnimatedSprite.endBehavior(this.gorillaSpawnNut);
			gorilla.shakeIntensity = 1f;
			gorilla.shakeIntensityChange = -0.01f;
			this.temporarySprites.Add(gorilla);
		}

		// Token: 0x06003056 RID: 12374 RVA: 0x002631F0 File Offset: 0x002613F0
		private void gorillaSpawnNut(int extra)
		{
			TemporaryAnimatedSprite gorilla = base.getTemporarySpriteByID(777);
			gorilla.sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 352, 32, 32);
			gorilla.sourceRectStartingPos = Utility.PointToVector2(gorilla.sourceRect.Location);
			gorilla.currentNumberOfLoops = 0;
			gorilla.totalNumberOfLoops = 1;
			gorilla.interval = 1000f;
			gorilla.shakeIntensity = 2f;
			gorilla.shakeIntensityChange = -0.01f;
			base.playSound("grunt", null, null, SoundContext.Default);
			Game1.createItemDebris(ItemRegistry.Create("(O)73", 1, 0, false), new Vector2(16.5f, 25f) * 64f, 0, this, 1280, false);
			gorilla.timer = 0f;
			gorilla.motion = Vector2.Zero;
			gorilla.animationLength = 1;
			gorilla.endFunction = new TemporaryAnimatedSprite.endBehavior(this.gorillaReturn);
			this.temporarySprites.Add(gorilla);
		}

		// Token: 0x06003057 RID: 12375 RVA: 0x002632F0 File Offset: 0x002614F0
		private void gorillaReturn(int extra)
		{
			TemporaryAnimatedSprite gorilla = base.getTemporarySpriteByID(777);
			gorilla.sourceRect = new Microsoft.Xna.Framework.Rectangle(32, 352, 32, 32);
			gorilla.sourceRectStartingPos = Utility.PointToVector2(gorilla.sourceRect.Location);
			gorilla.currentNumberOfLoops = 0;
			gorilla.totalNumberOfLoops = 6;
			gorilla.interval = 200f;
			gorilla.timer = 0f;
			gorilla.motion = new Vector2(0f, -3f);
			gorilla.animationLength = 2;
			gorilla.yStopCoordinate = 1280;
			gorilla.reachedStopCoordinate = delegate(int x)
			{
				base.removeTemporarySpritesWithID(777);
			};
			this.temporarySprites.Add(gorilla);
			DelayedAction.functionAfterDelay(delegate
			{
				Game1.playMorningSong(false);
			}, 3000);
		}

		// Token: 0x06003058 RID: 12376 RVA: 0x002633C8 File Offset: 0x002615C8
		public override void SetBuriedNutLocations()
		{
			this.buriedNutPoints.Add(new Point(23, 47));
			this.buriedNutPoints.Add(new Point(61, 21));
			base.SetBuriedNutLocations();
		}

		// Token: 0x06003059 RID: 12377 RVA: 0x002633F8 File Offset: 0x002615F8
		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			foreach (SuspensionBridge suspensionBridge in this.suspensionBridges)
			{
				suspensionBridge.Update(time);
			}
		}

		// Token: 0x0600305A RID: 12378 RVA: 0x00263450 File Offset: 0x00261650
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			foreach (SuspensionBridge suspensionBridge in this.suspensionBridges)
			{
				suspensionBridge.Draw(b);
			}
		}

		// Token: 0x0600305B RID: 12379 RVA: 0x002634A8 File Offset: 0x002616A8
		public override bool IsLocationSpecificPlacementRestriction(Vector2 tileLocation)
		{
			using (List<SuspensionBridge>.Enumerator enumerator = this.suspensionBridges.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.CheckPlacementPrevention(tileLocation))
					{
						return true;
					}
				}
			}
			return base.IsLocationSpecificPlacementRestriction(tileLocation);
		}

		// Token: 0x04002097 RID: 8343
		[XmlIgnore]
		public List<SuspensionBridge> suspensionBridges = new List<SuspensionBridge>();

		// Token: 0x04002098 RID: 8344
		[XmlElement("addedSlimesToday")]
		private readonly NetBool addedSlimesToday = new NetBool();
	}
}

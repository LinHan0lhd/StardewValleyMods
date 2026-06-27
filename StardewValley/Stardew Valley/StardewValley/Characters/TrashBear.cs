using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Network;

namespace StardewValley.Characters
{
	// Token: 0x0200037D RID: 893
	public class TrashBear : NPC
	{
		// Token: 0x17000476 RID: 1142
		// (get) Token: 0x060036EE RID: 14062 RVA: 0x002B64D3 File Offset: 0x002B46D3
		[XmlIgnore]
		public override bool IsVillager
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060036EF RID: 14063 RVA: 0x002B64D8 File Offset: 0x002B46D8
		public TrashBear() : base(new AnimatedSprite("Characters\\TrashBear", 0, 32, 32), new Vector2(102f, 95f) * 64f, 0, "TrashBear", null)
		{
			this.CurrentDialogue.Clear();
			base.HideShadow = true;
		}

		// Token: 0x060036F0 RID: 14064 RVA: 0x002B6544 File Offset: 0x002B4744
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.cutsceneEvent, "cutsceneEvent").AddField(this.eatEvent, "eatEvent");
			this.cutsceneEvent.onEvent += this.doCutscene;
			this.eatEvent.onEvent += this.doEatEvent;
		}

		// Token: 0x060036F1 RID: 14065 RVA: 0x002B65AC File Offset: 0x002B47AC
		public override void ChooseAppearance(LocalizedContentManager content = null)
		{
		}

		// Token: 0x060036F2 RID: 14066 RVA: 0x002B65B0 File Offset: 0x002B47B0
		public override bool checkAction(Farmer who, GameLocation l)
		{
			if (this.sprite.Value.CurrentAnimation != null)
			{
				return false;
			}
			if (this.tryToReceiveActiveObject(who, false))
			{
				return true;
			}
			base.faceTowardFarmerForPeriod(4000, 3, false, who);
			base.shake(500);
			Game1.playSound("trashbear", null);
			this.showWantBubbleTimer = 3000;
			this.updateItemWanted();
			return false;
		}

		// Token: 0x060036F3 RID: 14067 RVA: 0x002B661C File Offset: 0x002B481C
		public void updateItemWanted()
		{
			int which = 0;
			if (NetWorldState.checkAnywhereForWorldStateID("trashBear1"))
			{
				which = 1;
			}
			if (NetWorldState.checkAnywhereForWorldStateID("trashBear2"))
			{
				which = 2;
			}
			if (NetWorldState.checkAnywhereForWorldStateID("trashBear3"))
			{
				which = 3;
			}
			int randomSeed = Utility.CreateRandomSeed(777111.0, (double)which, 0.0, 0.0, 0.0);
			this.itemWantedIndex = Utility.getRandomPureSeasonalItem(Game1.season, randomSeed);
			if (which > 1)
			{
				int position = Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)randomSeed, 0.0, 0.0, 0.0).Next(CraftingRecipe.cookingRecipes.Count);
				int counter = 0;
				foreach (string v in CraftingRecipe.cookingRecipes.Values)
				{
					if (counter == position)
					{
						string craft_result = ArgUtility.Get(v.Split('/', StringSplitOptions.None), 2, null, true);
						craft_result = ArgUtility.SplitBySpaceAndGet(craft_result, 0, null);
						this.itemWantedIndex = craft_result;
						break;
					}
					counter++;
				}
			}
		}

		// Token: 0x060036F4 RID: 14068 RVA: 0x002B674C File Offset: 0x002B494C
		public override void update(GameTime time, GameLocation location)
		{
			base.update(time, location);
			this.cutsceneEvent.Poll();
			this.eatEvent.Poll();
			if (this.showWantBubbleTimer > 0)
			{
				this.showWantBubbleTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			}
		}

		// Token: 0x060036F5 RID: 14069 RVA: 0x002B679C File Offset: 0x002B499C
		public override bool tryToReceiveActiveObject(Farmer who, bool probe = false)
		{
			this.updateItemWanted();
			Object activeObject = who.ActiveObject;
			if (((activeObject != null) ? activeObject.QualifiedItemId : null) == "(O)" + this.itemWantedIndex)
			{
				if (!probe)
				{
					Game1.currentLocation.playSound("coin", null, null, SoundContext.Default);
					if (NetWorldState.checkAnywhereForWorldStateID("trashBear3"))
					{
						NetWorldState.addWorldStateIDEverywhere("trashBearDone");
					}
					else if (NetWorldState.checkAnywhereForWorldStateID("trashBear2"))
					{
						NetWorldState.addWorldStateIDEverywhere("trashBear3");
					}
					else if (NetWorldState.checkAnywhereForWorldStateID("trashBear1"))
					{
						NetWorldState.addWorldStateIDEverywhere("trashBear2");
					}
					else
					{
						NetWorldState.addWorldStateIDEverywhere("trashBear1");
					}
					this.eatEvent.Fire(this.itemWantedIndex);
					who.reduceActiveItemByOne();
				}
				return true;
			}
			return false;
		}

		// Token: 0x060036F6 RID: 14070 RVA: 0x002B6870 File Offset: 0x002B4A70
		public void doEatEvent(string item_index)
		{
			if (!(Game1.currentLocation is Forest))
			{
				return;
			}
			this.showWantBubbleTimer = 0;
			this.itemBeingEaten = item_index;
			this.sprite.Value.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(9, 1500, false, false, new AnimatedSprite.endOfAnimationBehavior(this.throwUpItem), true),
				new FarmerSprite.AnimationFrame(5, 1000, false, false, null, false),
				new FarmerSprite.AnimationFrame(6, 250, false, false, new AnimatedSprite.endOfAnimationBehavior(this.chew), false),
				new FarmerSprite.AnimationFrame(7, 250, false, false, null, false),
				new FarmerSprite.AnimationFrame(6, 250, false, false, null, false),
				new FarmerSprite.AnimationFrame(7, 250, false, false, null, false),
				new FarmerSprite.AnimationFrame(6, 250, false, false, null, false),
				new FarmerSprite.AnimationFrame(7, 500, false, false, new AnimatedSprite.endOfAnimationBehavior(this.doneAnimating), true)
			});
			this.sprite.Value.loop = false;
			ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem("(O)" + this.itemBeingEaten);
			string textureName = dataOrErrorItem.GetTextureName();
			Rectangle sourceRect = dataOrErrorItem.GetSourceRect(0, null);
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(textureName, sourceRect, 1500f, 1, 0, base.Position + new Vector2(96f, -92f), false, false, (float)(base.StandingPixel.Y + 1) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f, false));
		}

		// Token: 0x060036F7 RID: 14071 RVA: 0x002B6A20 File Offset: 0x002B4C20
		private void throwUpItem(Farmer who)
		{
			ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem("(O)" + this.itemBeingEaten);
			string textureName = dataOrErrorItem.GetTextureName();
			Rectangle sourceRect = dataOrErrorItem.GetSourceRect(0, null);
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(textureName, sourceRect, 1000f, 1, 0, base.Position + new Vector2(96f, -108f), false, false, (float)(base.StandingPixel.Y + 1) / 10000f, 0f, Color.White, 4f, -0.01f, 0f, 0f, false)
			{
				motion = new Vector2(-0.8f, -15f),
				acceleration = new Vector2(0f, 0.5f)
			});
			Game1.playSound("dwop", null);
		}

		// Token: 0x060036F8 RID: 14072 RVA: 0x002B6B04 File Offset: 0x002B4D04
		private void chew(Farmer who)
		{
			Game1.playSound("eat", null);
			DelayedAction.playSoundAfterDelay("dirtyHit", 500, null, null, -1, false);
			DelayedAction.playSoundAfterDelay("dirtyHit", 1000, null, null, -1, false);
			DelayedAction.playSoundAfterDelay("gulp", 1400, null, null, -1, false);
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem("(O)" + this.itemBeingEaten);
			string textureName = itemData.GetTextureName();
			for (int i = 0; i < 8; i++)
			{
				Rectangle sourceRect = itemData.GetSourceRect(0, null).Clone();
				sourceRect.X += 8;
				sourceRect.Y += 8;
				sourceRect.Width = 4;
				sourceRect.Height = 4;
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(textureName, sourceRect, 400f, 1, 0, base.Position + new Vector2(64f, -48f), false, false, (float)base.StandingPixel.Y / 10000f + 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
				{
					motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, (float)Game1.random.Next(-6, -3)),
					acceleration = new Vector2(0f, 0.5f)
				});
			}
		}

		// Token: 0x060036F9 RID: 14073 RVA: 0x002B6C9D File Offset: 0x002B4E9D
		private void doneAnimating(Farmer who)
		{
			this.sprite.Value.CurrentFrame = 8;
			if (NetWorldState.checkAnywhereForWorldStateID("trashBearDone") && Game1.currentLocation is Forest)
			{
				this.doCutsceneEvent();
			}
		}

		// Token: 0x060036FA RID: 14074 RVA: 0x002B6CCE File Offset: 0x002B4ECE
		private void doCutsceneEvent()
		{
			this.cutsceneEvent.Fire();
		}

		// Token: 0x060036FB RID: 14075 RVA: 0x002B6CDC File Offset: 0x002B4EDC
		private void doCutscene()
		{
			if (Game1.currentLocation is Forest)
			{
				if (Game1.activeClickableMenu != null && Game1.activeClickableMenu.readyToClose())
				{
					Game1.activeClickableMenu.exitThisMenuNoSound();
				}
				if (Game1.activeClickableMenu == null)
				{
					Game1.player.freezePause = 2000;
					Game1.globalFadeToBlack(delegate
					{
						Game1.currentLocation.startEvent(new Event("spring_day_ambient/-1000 -1000/farmer 104 95 3/skippable/addTemporaryActor TrashBear 32 32 102 95 0 false/animate TrashBear false true 250 0 1/viewport 102 97 clamp true/pause 3000/stopAnimation TrashBear/move TrashBear 0 2 2/faceDirection farmer 2/pause 1000/animate TrashBear false true 275 13 14 15 14/playSound trashbear_flute/specificTemporarySprite trashBearPrelude/viewport move -1 1 4000/pause 9000/stopAnimation TrashBear/playSound yoba/specificTemporarySprite trashBearMagic/pause 500/animate farmer false true 100 94/jump farmer/pause 2000/viewport move 1 -1 4000/stopAnimation farmer/move farmer 0 2 2/pause 4000/playSound trashbear/specificTemporarySprite trashBearUmbrella1/warp TrashBear -100 -100/pause 2000/faceDirection farmer 1/pause 2000/fade/viewport -5000 -5000/changeLocation Town/viewport 54 68 true/specificTemporarySprite trashBearTown/pause 10000/end", null, "777111", null));
					}, 0.02f);
				}
			}
		}

		// Token: 0x060036FC RID: 14076 RVA: 0x002B6D54 File Offset: 0x002B4F54
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (this.showWantBubbleTimer > 0)
			{
				float yOffset = 2f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				Point tile = base.TilePoint;
				float drawLayer = (float)((tile.Y + 1) * 64) / 10000f;
				yOffset -= 40f;
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(tile.X * 64 + 32), (float)(tile.Y * 64 - 96 - 48) + yOffset)), new Rectangle?(new Rectangle(141, 465, 20, 24)), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, drawLayer + 1E-06f);
				ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem("(O)" + this.itemWantedIndex);
				Texture2D texture = dataOrErrorItem.GetTexture();
				Rectangle sourceRect = dataOrErrorItem.GetSourceRect(0, null);
				b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(tile.X * 64 + 64 + 8), (float)(tile.Y * 64 - 64 - 32 - 8) + yOffset)), new Rectangle?(sourceRect), Color.White, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, drawLayer + 1E-05f);
			}
		}

		// Token: 0x040023CC RID: 9164
		private int showWantBubbleTimer;

		// Token: 0x040023CD RID: 9165
		[XmlIgnore]
		public string itemWantedIndex;

		// Token: 0x040023CE RID: 9166
		[XmlIgnore]
		private readonly NetEvent0 cutsceneEvent = new NetEvent0(false);

		// Token: 0x040023CF RID: 9167
		[XmlIgnore]
		private readonly NetEvent1Field<string, NetString> eatEvent = new NetEvent1Field<string, NetString>();

		// Token: 0x040023D0 RID: 9168
		[XmlIgnore]
		private string itemBeingEaten;
	}
}

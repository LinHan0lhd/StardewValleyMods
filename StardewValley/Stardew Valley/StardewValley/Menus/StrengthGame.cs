using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	// Token: 0x020002AE RID: 686
	public class StrengthGame : IClickableMenu
	{
		// Token: 0x06002CC2 RID: 11458 RVA: 0x0022A918 File Offset: 0x00228B18
		public StrengthGame() : base(2008, 3624, 20, 136, false)
		{
			this.power = 0f;
			this.changeSpeed = (float)(3 + Game1.random.Next(2));
			this.barColor = Color.Red;
			Game1.playSound("cowboy_monsterhit", null);
		}

		// Token: 0x06002CC3 RID: 11459 RVA: 0x0022A988 File Offset: 0x00228B88
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!this.clicked)
			{
				Game1.player.faceDirection(1);
				Game1.player.CurrentToolIndex = 107;
				Game1.player.FarmerSprite.animateOnce(168, 80f, 8);
				Game1.player.toolOverrideFunction = new AnimatedSprite.endOfAnimationBehavior(this.afterSwingAnimation);
				this.clicked = true;
			}
			if (this.showedResult && Game1.dialogueTyping)
			{
				Game1.currentDialogueCharacterIndex = Game1.currentObjectDialogue.Peek().Length - 1;
			}
			if (this.showedResult && !Game1.dialogueTyping)
			{
				Game1.player.toolOverrideFunction = null;
				Game1.exitActiveMenu();
				Game1.afterDialogues = null;
				Game1.pressActionButton(Game1.oldKBState, Game1.oldMouseState, Game1.oldPadState);
			}
		}

		// Token: 0x06002CC4 RID: 11460 RVA: 0x0022AA4C File Offset: 0x00228C4C
		public void afterSwingAnimation(Farmer who)
		{
			if (!Game1.isFestival())
			{
				who.toolOverrideFunction = null;
				return;
			}
			this.changeSpeed = 0f;
			Game1.playSound("hammer", null);
			Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite[]
			{
				new TemporaryAnimatedSprite(46, new Vector2(30f, 56f) * 64f, Color.White, 8, false, 100f, 0, -1, -1f, -1, 0)
			});
			if (this.power >= 99f)
			{
				this.endTimer = 2000f;
				return;
			}
			this.endTimer = 1000f;
		}

		// Token: 0x06002CC5 RID: 11461 RVA: 0x0022AAF8 File Offset: 0x00228CF8
		public override void update(GameTime time)
		{
			base.update(time);
			if (this.changeSpeed == 0f)
			{
				this.endTimer -= (float)time.ElapsedGameTime.Milliseconds;
				if (this.power >= 99f)
				{
					if (this.endTimer < 1500f)
					{
						if (!this.victorySound)
						{
							this.victorySound = true;
							Game1.playSound("getNewSpecialItem", null);
							this.barColor = Color.Orange;
						}
						if (!this.showedResult && Game1.random.NextDouble() < 0.08)
						{
							Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite[]
							{
								new TemporaryAnimatedSprite(10 + Game1.random.Next(2), new Vector2(31f, 55f) * 64f + new Vector2((float)Game1.random.Next(-64, 64), (float)Game1.random.Next(-64, 64)), Color.Yellow, 8, false, 100f, 0, -1, -1f, -1, 0)
								{
									layerDepth = 1f
								}
							});
						}
					}
				}
				else
				{
					this.transparency = Math.Max(0f, this.transparency - 0.02f);
				}
				if (this.endTimer <= 0f && !this.showedResult)
				{
					this.showedResult = true;
					if (this.power >= 99f)
					{
						Game1.player.festivalScore++;
						Game1.playSound("purchase", null);
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11660"));
					}
					else if (this.power >= 2f)
					{
						string strengthLevel = "";
						switch ((int)this.power)
						{
						case 2:
						case 3:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11701");
							break;
						case 4:
						case 5:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11700");
							break;
						case 6:
						case 7:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11699");
							break;
						case 8:
						case 9:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11698");
							break;
						case 10:
						case 11:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11697");
							break;
						case 12:
						case 13:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11696");
							break;
						case 14:
						case 15:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11695");
							break;
						case 16:
						case 17:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11694");
							break;
						case 18:
						case 19:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11693");
							break;
						case 20:
						case 21:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11692");
							break;
						case 22:
						case 23:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11691");
							break;
						case 24:
						case 25:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11690");
							break;
						case 26:
						case 27:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11689");
							break;
						case 28:
						case 29:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11688");
							break;
						case 30:
						case 31:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11687");
							break;
						case 32:
						case 33:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11686");
							break;
						case 34:
						case 35:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11685");
							break;
						case 36:
						case 37:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11684");
							break;
						case 38:
						case 39:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11683");
							break;
						case 40:
						case 41:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11682");
							break;
						case 42:
						case 43:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11681");
							break;
						case 44:
						case 45:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11680");
							break;
						case 46:
						case 47:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11679");
							break;
						case 48:
						case 49:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11678");
							break;
						case 50:
						case 51:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11677");
							break;
						case 52:
						case 53:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11676");
							break;
						case 54:
						case 55:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11675");
							break;
						case 56:
						case 57:
						case 58:
						case 59:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11674");
							break;
						case 60:
						case 61:
						case 62:
						case 63:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11673");
							break;
						case 64:
						case 65:
						case 66:
						case 67:
						case 68:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11672");
							break;
						case 69:
						case 70:
						case 71:
						case 72:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11671");
							break;
						case 73:
						case 74:
						case 75:
						case 76:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11670");
							break;
						case 77:
						case 78:
						case 79:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11669");
							break;
						case 80:
						case 81:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11668");
							break;
						case 82:
						case 83:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11667");
							break;
						case 84:
						case 85:
						case 86:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11666");
							break;
						case 87:
						case 89:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11665");
							break;
						case 88:
						case 90:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11664");
							break;
						case 91:
						case 92:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11663");
							break;
						case 93:
						case 94:
						case 95:
						case 96:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11662");
							break;
						case 97:
						case 98:
							strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11661");
							break;
						}
						Game1.playSound("dwop", null);
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11703", strengthLevel));
					}
					else
					{
						Game1.player.festivalScore++;
						Game1.playSound("purchase", null);
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11705")));
					}
					Game1.afterDialogues = new Game1.afterFadeFunction(base.exitThisMenuNoSound);
					return;
				}
			}
			else
			{
				this.power += this.changeSpeed;
				if (this.power > 100f)
				{
					this.power = 100f;
					this.changeSpeed = -this.changeSpeed;
					return;
				}
				if (this.power < 0f)
				{
					this.power = 0f;
					this.changeSpeed = -this.changeSpeed;
				}
			}
		}

		// Token: 0x06002CC6 RID: 11462 RVA: 0x0022B28D File Offset: 0x0022948D
		public override void performHoverAction(int x, int y)
		{
		}

		// Token: 0x06002CC7 RID: 11463 RVA: 0x0022B28F File Offset: 0x0022948F
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
		}

		// Token: 0x06002CC8 RID: 11464 RVA: 0x0022B294 File Offset: 0x00229494
		public override void draw(SpriteBatch b)
		{
			if (Game1.IsRenderingNonNativeUIScale())
			{
				b.End();
				Game1.PopUIMode();
				b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			}
			if (!Game1.dialogueUp)
			{
				b.Draw(Game1.staminaRect, Game1.GlobalToLocal(Game1.viewport, new Rectangle(this.xPositionOnScreen, (int)((float)this.yPositionOnScreen - this.power / 100f * (float)this.height), this.width, (int)(this.power / 100f * (float)this.height))), new Rectangle?(Game1.staminaRect.Bounds), this.barColor * this.transparency, 0f, Vector2.Zero, SpriteEffects.None, 1E-05f);
			}
			if (Game1.player.FarmerSprite.isOnToolAnimation())
			{
				Game1.drawTool(Game1.player, Game1.player.CurrentToolIndex);
			}
			if (Game1.IsRenderingNonNativeUIScale())
			{
				b.End();
				Game1.PushUIMode();
				b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			}
		}

		// Token: 0x04001E77 RID: 7799
		private float power;

		// Token: 0x04001E78 RID: 7800
		private float changeSpeed;

		// Token: 0x04001E79 RID: 7801
		private float endTimer;

		// Token: 0x04001E7A RID: 7802
		private float transparency = 1f;

		// Token: 0x04001E7B RID: 7803
		private Color barColor;

		// Token: 0x04001E7C RID: 7804
		private bool victorySound;

		// Token: 0x04001E7D RID: 7805
		private bool clicked;

		// Token: 0x04001E7E RID: 7806
		private bool showedResult;
	}
}

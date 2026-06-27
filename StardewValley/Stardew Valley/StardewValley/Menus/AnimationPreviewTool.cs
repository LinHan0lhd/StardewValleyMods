using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	// Token: 0x0200024E RID: 590
	public class AnimationPreviewTool : IClickableMenu
	{
		// Token: 0x0600273D RID: 10045 RVA: 0x001BCFFC File Offset: 0x001BB1FC
		public AnimationPreviewTool() : base(Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64, 632 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2 + 64, false)
		{
			Game1.player.faceDirection(2);
			Game1.player.FarmerSprite.StopAnimation();
			IEnumerable<FieldInfo> fields = typeof(FarmerSprite).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
			this.animationButtons = new List<ClickableTextureComponent>();
			foreach (FieldInfo field in from fi in fields
			where fi.IsLiteral && !fi.IsInitOnly
			select fi)
			{
				ClickableTextureComponent component = new ClickableTextureComponent(new Rectangle(0, 0, 200, 48), null, default(Rectangle), 1f, false);
				component.myID = (int)field.GetValue(null);
				component.name = field.Name;
				this.animationButtons.Add(component);
			}
			this.okButton = new ClickableTextureComponent("OK", new Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, this.yPositionOnScreen + this.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 16, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false)
			{
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			this.components = new List<List<ClickableTextureComponent>>();
			this.components.Add(new List<ClickableTextureComponent>(new ClickableTextureComponent[]
			{
				new ClickableTextureComponent("Hair Heading", new Rectangle(0, 0, 64, 16), "Hair", "", null, default(Rectangle), 1f, false)
			}));
			this.hairLabel = new ClickableTextureComponent("Hair Label", new Rectangle(0, 0, 64, 64), "0", "", null, default(Rectangle), 1f, false);
			this.components.Add(new List<ClickableTextureComponent>(new ClickableTextureComponent[]
			{
				new ClickableTextureComponent("Hair Style", new Rectangle(0, 0, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false)
				{
					myID = -1
				},
				this.hairLabel,
				new ClickableTextureComponent("Hair Style", new Rectangle(0, 0, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false)
				{
					myID = 1
				}
			}));
			this.components.Add(new List<ClickableTextureComponent>(new ClickableTextureComponent[]
			{
				new ClickableTextureComponent("Shirt Heading", new Rectangle(0, 0, 64, 16), "Shirt", "", null, default(Rectangle), 1f, false)
			}));
			this.shirtLabel = new ClickableTextureComponent("Shirt Label", new Rectangle(0, 0, 64, 64), "0", "", null, default(Rectangle), 1f, false);
			this.components.Add(new List<ClickableTextureComponent>(new ClickableTextureComponent[]
			{
				new ClickableTextureComponent("Shirt Style", new Rectangle(0, 0, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false)
				{
					myID = -1
				},
				this.shirtLabel,
				new ClickableTextureComponent("Shirt Style", new Rectangle(0, 0, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false)
				{
					myID = 1
				}
			}));
			this.components.Add(new List<ClickableTextureComponent>(new ClickableTextureComponent[]
			{
				new ClickableTextureComponent("Pants Heading", new Rectangle(0, 0, 64, 16), "Pants", "", null, default(Rectangle), 1f, false)
			}));
			this.pantsLabel = new ClickableTextureComponent("Pants Label", new Rectangle(0, 0, 64, 64), "0", "", null, default(Rectangle), 1f, false);
			this.components.Add(new List<ClickableTextureComponent>(new ClickableTextureComponent[]
			{
				new ClickableTextureComponent("Pants Style", new Rectangle(0, 0, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false)
				{
					myID = -1
				},
				this.pantsLabel,
				new ClickableTextureComponent("Pants Style", new Rectangle(0, 0, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false)
				{
					myID = 1
				}
			}));
			this.components.Add(new List<ClickableTextureComponent>(new ClickableTextureComponent[]
			{
				new ClickableTextureComponent("Toggle Gender", new Rectangle(0, 0, 64, 64), "Toggle Gender", "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 25, -1, -1), 1f, false)
			}));
			this.RepositionElements();
		}

		// Token: 0x0600273E RID: 10046 RVA: 0x001BD588 File Offset: 0x001BB788
		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			this.xPositionOnScreen = Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2;
			this.yPositionOnScreen = Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64;
			this.RepositionElements();
		}

		// Token: 0x0600273F RID: 10047 RVA: 0x001BD5EA File Offset: 0x001BB7EA
		public void SwitchShirt(int direction)
		{
			Game1.player.rotateShirt(direction, null);
			this.UpdateLabels();
		}

		// Token: 0x06002740 RID: 10048 RVA: 0x001BD5FE File Offset: 0x001BB7FE
		public void SwitchHair(int direction)
		{
			Game1.player.changeHairStyle(Game1.player.hair.Value + direction);
			this.UpdateLabels();
		}

		// Token: 0x06002741 RID: 10049 RVA: 0x001BD621 File Offset: 0x001BB821
		public void SwitchPants(int direction)
		{
			Game1.player.rotatePantStyle(direction, null);
			this.UpdateLabels();
		}

		// Token: 0x06002742 RID: 10050 RVA: 0x001BD638 File Offset: 0x001BB838
		private void RepositionElements()
		{
			this.scrollView = new Rectangle(this.xPositionOnScreen + 320, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder, 250, 500);
			if (this.scrollView.Left < Game1.graphics.GraphicsDevice.ScissorRectangle.Left)
			{
				int size_difference = Game1.graphics.GraphicsDevice.ScissorRectangle.Left - this.scrollView.Left;
				this.scrollView.X = this.scrollView.X + size_difference;
				this.scrollView.Width = this.scrollView.Width - size_difference;
			}
			if (this.scrollView.Right > Game1.graphics.GraphicsDevice.ScissorRectangle.Right)
			{
				int size_difference2 = this.scrollView.Right - Game1.graphics.GraphicsDevice.ScissorRectangle.Right;
				this.scrollView.X = this.scrollView.X - size_difference2;
				this.scrollView.Width = this.scrollView.Width - size_difference2;
			}
			if (this.scrollView.Top < Game1.graphics.GraphicsDevice.ScissorRectangle.Top)
			{
				int size_difference3 = Game1.graphics.GraphicsDevice.ScissorRectangle.Top - this.scrollView.Top;
				this.scrollView.Y = this.scrollView.Y + size_difference3;
				this.scrollView.Width = this.scrollView.Width - size_difference3;
			}
			if (this.scrollView.Bottom > Game1.graphics.GraphicsDevice.ScissorRectangle.Bottom)
			{
				int size_difference4 = this.scrollView.Bottom - Game1.graphics.GraphicsDevice.ScissorRectangle.Bottom;
				this.scrollView.Y = this.scrollView.Y - size_difference4;
				this.scrollView.Width = this.scrollView.Width - size_difference4;
			}
			int component_y = this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 200;
			foreach (List<ClickableTextureComponent> list in this.components)
			{
				int component_x = this.xPositionOnScreen + 70;
				int max_height = 0;
				foreach (ClickableTextureComponent component in list)
				{
					component.bounds.X = component_x;
					component.bounds.Y = component_y;
					component_x += component.bounds.Width + 8;
					max_height = Math.Max(component.bounds.Height, max_height);
				}
				component_y += max_height + 8;
			}
			this.RepositionScrollElements();
			this.UpdateLabels();
		}

		// Token: 0x06002743 RID: 10051 RVA: 0x001BD918 File Offset: 0x001BBB18
		public void UpdateLabels()
		{
			this.pantsLabel.label = (Game1.player.GetPantsIndex().ToString() ?? "");
			this.shirtLabel.label = (Game1.player.GetShirtIndex().ToString() ?? "");
			this.hairLabel.label = (Game1.player.getHair(false).ToString() ?? "");
		}

		// Token: 0x06002744 RID: 10052 RVA: 0x001BD998 File Offset: 0x001BBB98
		public void RepositionScrollElements()
		{
			int y_offset = (int)this.scrollY;
			if (this.scrollY > 0f)
			{
				this.scrollY = 0f;
			}
			foreach (ClickableTextureComponent component in this.animationButtons)
			{
				component.bounds.X = this.scrollView.X;
				component.bounds.Y = this.scrollView.Y + y_offset;
				component.bounds.Width = this.scrollView.Width;
				y_offset += component.bounds.Height;
				if (this.scrollView.Intersects(component.bounds))
				{
					component.visible = true;
				}
				else
				{
					component.visible = false;
				}
			}
		}

		// Token: 0x06002745 RID: 10053 RVA: 0x001BDA7C File Offset: 0x001BBC7C
		public override void snapToDefaultClickableComponent()
		{
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002746 RID: 10054 RVA: 0x001BDA84 File Offset: 0x001BBC84
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			foreach (ClickableTextureComponent component in this.animationButtons)
			{
				if (component.bounds.Contains(x, y) && this.scrollView.Contains(x, y))
				{
					if (component.name.Contains("Left"))
					{
						Game1.player.faceDirection(3);
					}
					else if (component.name.Contains("Right"))
					{
						Game1.player.faceDirection(1);
					}
					else if (component.name.Contains("Up"))
					{
						Game1.player.faceDirection(0);
					}
					else
					{
						Game1.player.faceDirection(2);
					}
					Game1.player.completelyStopAnimatingOrDoingAction();
					Game1.player.animateOnce(component.myID);
				}
			}
			foreach (List<ClickableTextureComponent> list in this.components)
			{
				foreach (ClickableTextureComponent component2 in list)
				{
					if (component2.containsPoint(x, y))
					{
						string name = component2.name;
						if (!(name == "Shirt Style"))
						{
							if (!(name == "Pants Style"))
							{
								if (!(name == "Hair Style"))
								{
									if (name == "Toggle Gender")
									{
										Game1.player.changeGender(!Game1.player.IsMale);
									}
								}
								else
								{
									this.SwitchHair(component2.myID);
								}
							}
							else
							{
								this.SwitchPants(component2.myID);
							}
						}
						else
						{
							this.SwitchShirt(component2.myID);
						}
					}
				}
			}
			if (this.okButton.containsPoint(x, y))
			{
				base.exitThisMenu(true);
			}
		}

		// Token: 0x06002747 RID: 10055 RVA: 0x001BDCA0 File Offset: 0x001BBEA0
		public override void receiveKeyPress(Keys key)
		{
		}

		// Token: 0x06002748 RID: 10056 RVA: 0x001BDCA2 File Offset: 0x001BBEA2
		public override void receiveScrollWheelAction(int direction)
		{
			this.scrollY += (float)direction;
			this.RepositionScrollElements();
			base.receiveScrollWheelAction(direction);
		}

		// Token: 0x06002749 RID: 10057 RVA: 0x001BDCC0 File Offset: 0x001BBEC0
		public override void performHoverAction(int x, int y)
		{
		}

		// Token: 0x0600274A RID: 10058 RVA: 0x001BDCC2 File Offset: 0x001BBEC2
		public bool canLeaveMenu()
		{
			return true;
		}

		// Token: 0x0600274B RID: 10059 RVA: 0x001BDCC8 File Offset: 0x001BBEC8
		public override void draw(SpriteBatch b)
		{
			Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true, null, false, true, -1, -1, -1);
			b.Draw(Game1.daybg, new Vector2((float)(this.xPositionOnScreen + 64 + 42 - 2), (float)(this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16)), Color.White);
			Game1.player.FarmerRenderer.draw(b, Game1.player.FarmerSprite.CurrentAnimationFrame, Game1.player.FarmerSprite.CurrentFrame, Game1.player.FarmerSprite.SourceRect, new Vector2((float)(this.xPositionOnScreen - 2 + 42 + 128 - 32), (float)(this.yPositionOnScreen + IClickableMenu.borderWidth - 16 + IClickableMenu.spaceToClearTopBorder + 32)), Vector2.Zero, 0.8f, Color.White, 0f, 1f, Game1.player);
			b.End();
			Rectangle cached_scissor_rect = b.GraphicsDevice.ScissorRectangle;
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled, null, null);
			b.GraphicsDevice.ScissorRectangle = this.scrollView;
			foreach (ClickableTextureComponent component in this.animationButtons)
			{
				if (component.visible)
				{
					Game1.DrawBox(component.bounds.X, component.bounds.Y, component.bounds.Width, component.bounds.Height, null);
					Utility.drawTextWithShadow(b, component.name, Game1.smallFont, new Vector2((float)component.bounds.X, (float)component.bounds.Y), Color.Black, 1f, -1f, -1, -1, 1f, 3);
				}
			}
			b.End();
			b.GraphicsDevice.ScissorRectangle = cached_scissor_rect;
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			foreach (List<ClickableTextureComponent> list in this.components)
			{
				foreach (ClickableTextureComponent clickableTextureComponent in list)
				{
					clickableTextureComponent.draw(b);
				}
			}
			this.okButton.draw(b);
			base.drawMouse(b, false, -1);
		}

		// Token: 0x04001859 RID: 6233
		public List<List<ClickableTextureComponent>> components;

		// Token: 0x0400185A RID: 6234
		public Rectangle scrollView;

		// Token: 0x0400185B RID: 6235
		public List<ClickableTextureComponent> animationButtons;

		// Token: 0x0400185C RID: 6236
		public ClickableTextureComponent okButton;

		// Token: 0x0400185D RID: 6237
		public ClickableTextureComponent hairLabel;

		// Token: 0x0400185E RID: 6238
		public ClickableTextureComponent shirtLabel;

		// Token: 0x0400185F RID: 6239
		public ClickableTextureComponent pantsLabel;

		// Token: 0x04001860 RID: 6240
		public float scrollY;
	}
}

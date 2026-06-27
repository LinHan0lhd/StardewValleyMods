using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	// Token: 0x0200026F RID: 623
	internal class FarmersBox : IClickableMenu
	{
		// Token: 0x06002926 RID: 10534 RVA: 0x001E3D01 File Offset: 0x001E1F01
		public FarmersBox() : base(0, 200, 528, 400, false)
		{
		}

		// Token: 0x06002927 RID: 10535 RVA: 0x001E3D28 File Offset: 0x001E1F28
		private void UpdateFarmers(List<ClickableComponent> parentComponents)
		{
			if (this._updateTimer > 0f)
			{
				return;
			}
			this._farmers.Clear();
			foreach (Farmer farmer in Game1.getOnlineFarmers())
			{
				this._farmers.Add(farmer);
			}
			this._updateTimer = 1f;
		}

		// Token: 0x06002928 RID: 10536 RVA: 0x001E3DA4 File Offset: 0x001E1FA4
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x06002929 RID: 10537 RVA: 0x001E3DA8 File Offset: 0x001E1FA8
		public override void update(GameTime time)
		{
			this._updateTimer -= (float)time.ElapsedGameTime.TotalSeconds;
		}

		// Token: 0x0600292A RID: 10538 RVA: 0x001E3DD4 File Offset: 0x001E1FD4
		public void draw(SpriteBatch b, int left, int bottom, ClickableComponent current, List<ClickableComponent> parentComponents)
		{
			this.UpdateFarmers(parentComponents);
			if (this._farmers.Count == 0)
			{
				return;
			}
			int sizeY = 100;
			this.height = sizeY * this._farmers.Count;
			this.xPositionOnScreen = left;
			this.yPositionOnScreen = bottom - this.height;
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(301, 288, 15, 15), this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, Color.White, 4f, false, -1f);
			b.End();
			b.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled, null, null);
			Rectangle origClip = b.GraphicsDevice.ScissorRectangle;
			int x = this.xPositionOnScreen + 16;
			int y = this.yPositionOnScreen;
			for (int i = 0; i < this._farmers.Count; i++)
			{
				Farmer farmer = this._farmers[i];
				Rectangle newClip = origClip;
				newClip.X = x;
				newClip.Y = y;
				newClip.Height = sizeY - 8;
				newClip.Width = 200;
				b.GraphicsDevice.ScissorRectangle = newClip;
				FarmerRenderer.isDrawingForUI = true;
				farmer.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(farmer.bathingClothes.Value ? 108 : 0, 0, false, false, null, false), farmer.bathingClothes.Value ? 108 : 0, new Rectangle(0, farmer.bathingClothes.Value ? 576 : 0, 16, 32), new Vector2((float)x, (float)y), Vector2.Zero, 0.8f, 2, Color.White, 0f, 1f, farmer);
				FarmerRenderer.isDrawingForUI = false;
				b.GraphicsDevice.ScissorRectangle = origClip;
				int textX = x + 80;
				int textY = y + 12;
				string farmerName = ChatBox.formattedUserName(farmer);
				b.DrawString(Game1.dialogueFont, farmerName, new Vector2((float)textX, (float)textY), Color.White);
				string platformUserName = Game1.multiplayer.getUserName(farmer.UniqueMultiplayerID);
				if (!string.IsNullOrEmpty(platformUserName))
				{
					textY += Game1.dialogueFont.LineSpacing + 4;
					string userName = "(" + platformUserName + ")";
					b.DrawString(Game1.smallFont, userName, new Vector2((float)textX, (float)textY), Color.White);
				}
				y += sizeY;
			}
			b.GraphicsDevice.ScissorRectangle = origClip;
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
		}

		// Token: 0x04001AE8 RID: 6888
		private readonly List<Farmer> _farmers = new List<Farmer>();

		// Token: 0x04001AE9 RID: 6889
		public float _updateTimer;
	}
}

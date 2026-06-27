using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using xTile;
using xTile.Dimensions;
using xTile.Layers;

namespace StardewValley.Minigames
{
	// Token: 0x02000244 RID: 580
	public class TelescopeScene : IMinigame
	{
		// Token: 0x060026B5 RID: 9909 RVA: 0x001B73C4 File Offset: 0x001B55C4
		public TelescopeScene(NPC Maru)
		{
			this.temporaryContent = Game1.content.CreateTemporary();
			this.background = this.temporaryContent.Load<Texture2D>("LooseSprites\\nightSceneMaru");
			this.trees = this.temporaryContent.Load<Texture2D>("LooseSprites\\nightSceneMaruTrees");
			this.walkSpace = new GameLocation(null, "walkSpace");
			this.walkSpace.map = new Map();
			this.walkSpace.map.AddLayer(new Layer("Back", this.walkSpace.map, new Size(30, 1), new Size(64)));
			Game1.currentLocation = this.walkSpace;
		}

		// Token: 0x060026B6 RID: 9910 RVA: 0x001B7473 File Offset: 0x001B5673
		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		// Token: 0x060026B7 RID: 9911 RVA: 0x001B747F File Offset: 0x001B567F
		public bool tick(GameTime time)
		{
			return false;
		}

		// Token: 0x060026B8 RID: 9912 RVA: 0x001B7482 File Offset: 0x001B5682
		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x060026B9 RID: 9913 RVA: 0x001B7484 File Offset: 0x001B5684
		public void leftClickHeld(int x, int y)
		{
		}

		// Token: 0x060026BA RID: 9914 RVA: 0x001B7486 File Offset: 0x001B5686
		public void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x060026BB RID: 9915 RVA: 0x001B7488 File Offset: 0x001B5688
		public void releaseLeftClick(int x, int y)
		{
		}

		// Token: 0x060026BC RID: 9916 RVA: 0x001B748A File Offset: 0x001B568A
		public void releaseRightClick(int x, int y)
		{
		}

		// Token: 0x060026BD RID: 9917 RVA: 0x001B748C File Offset: 0x001B568C
		public void receiveKeyPress(Keys k)
		{
		}

		// Token: 0x060026BE RID: 9918 RVA: 0x001B748E File Offset: 0x001B568E
		public void receiveKeyRelease(Keys k)
		{
		}

		// Token: 0x060026BF RID: 9919 RVA: 0x001B7490 File Offset: 0x001B5690
		public void draw(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			b.Draw(this.background, new Vector2((float)(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - this.background.Bounds.Width / 2 * 4), (float)(-(float)(this.background.Bounds.Height * 4) + Game1.graphics.GraphicsDevice.Viewport.Height)), new Microsoft.Xna.Framework.Rectangle?(this.background.Bounds), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			b.Draw(this.trees, new Vector2((float)(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - this.trees.Bounds.Width / 2 * 4), (float)(-(float)(this.trees.Bounds.Height * 4) + Game1.graphics.GraphicsDevice.Viewport.Height)), new Microsoft.Xna.Framework.Rectangle?(this.trees.Bounds), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.End();
		}

		// Token: 0x060026C0 RID: 9920 RVA: 0x001B75EC File Offset: 0x001B57EC
		public void changeScreenSize()
		{
		}

		// Token: 0x060026C1 RID: 9921 RVA: 0x001B75EE File Offset: 0x001B57EE
		public void unload()
		{
			this.temporaryContent.Unload();
		}

		// Token: 0x060026C2 RID: 9922 RVA: 0x001B75FB File Offset: 0x001B57FB
		public void receiveEventPoke(int data)
		{
			throw new NotImplementedException();
		}

		// Token: 0x060026C3 RID: 9923 RVA: 0x001B7602 File Offset: 0x001B5802
		public string minigameId()
		{
			return null;
		}

		// Token: 0x060026C4 RID: 9924 RVA: 0x001B7605 File Offset: 0x001B5805
		public bool doMainGameUpdates()
		{
			return false;
		}

		// Token: 0x060026C5 RID: 9925 RVA: 0x001B7608 File Offset: 0x001B5808
		public bool forceQuit()
		{
			return false;
		}

		// Token: 0x04001808 RID: 6152
		public LocalizedContentManager temporaryContent;

		// Token: 0x04001809 RID: 6153
		public Texture2D background;

		// Token: 0x0400180A RID: 6154
		public Texture2D trees;

		// Token: 0x0400180B RID: 6155
		public float yOffset;

		// Token: 0x0400180C RID: 6156
		public GameLocation walkSpace;
	}
}

using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

// Token: 0x02000008 RID: 8
public class FrameRateCounter : DrawableGameComponent
{
	// Token: 0x0600001A RID: 26 RVA: 0x00002518 File Offset: 0x00000718
	public FrameRateCounter(Game game) : base(game)
	{
		this.content = new LocalizedContentManager(game.Services, base.Game.Content.RootDirectory);
	}

	// Token: 0x0600001B RID: 27 RVA: 0x0000254D File Offset: 0x0000074D
	protected override void LoadContent()
	{
		this.spriteBatch = new SpriteBatch(base.GraphicsDevice);
	}

	// Token: 0x0600001C RID: 28 RVA: 0x00002560 File Offset: 0x00000760
	protected override void UnloadContent()
	{
		this.content.Unload();
	}

	// Token: 0x0600001D RID: 29 RVA: 0x00002570 File Offset: 0x00000770
	public override void Update(GameTime gameTime)
	{
		this.elapsedTime += gameTime.ElapsedGameTime;
		if (this.elapsedTime > TimeSpan.FromSeconds(1.0))
		{
			this.elapsedTime -= TimeSpan.FromSeconds(1.0);
			this.frameRate = this.frameCounter;
			this.frameCounter = 0;
		}
	}

	// Token: 0x0600001E RID: 30 RVA: 0x000025E4 File Offset: 0x000007E4
	public override void Draw(GameTime gameTime)
	{
		this.frameCounter++;
		DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(5, 1);
		defaultInterpolatedStringHandler.AppendLiteral("fps: ");
		defaultInterpolatedStringHandler.AppendFormatted<int>(this.frameRate);
		string fps = defaultInterpolatedStringHandler.ToStringAndClear();
		this.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
		this.spriteBatch.DrawString(Game1.dialogueFont, fps, new Vector2(33f, 33f), Color.Black);
		this.spriteBatch.DrawString(Game1.dialogueFont, fps, new Vector2(32f, 32f), Color.White);
		this.spriteBatch.End();
	}

	// Token: 0x0400000C RID: 12
	private LocalizedContentManager content;

	// Token: 0x0400000D RID: 13
	private SpriteBatch spriteBatch;

	// Token: 0x0400000E RID: 14
	private int frameRate;

	// Token: 0x0400000F RID: 15
	private int frameCounter;

	// Token: 0x04000010 RID: 16
	private TimeSpan elapsedTime = TimeSpan.Zero;
}

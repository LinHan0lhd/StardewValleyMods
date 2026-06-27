using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley
{
	// Token: 0x020000CA RID: 202
	public class InstanceGame
	{
		// Token: 0x170001C3 RID: 451
		// (get) Token: 0x06000DC5 RID: 3525 RVA: 0x0009443B File Offset: 0x0009263B
		public bool IsMainInstance
		{
			get
			{
				return GameRunner.instance.gameInstances.Count == 0 || GameRunner.instance.gameInstances[0] == this;
			}
		}

		// Token: 0x06000DC6 RID: 3526 RVA: 0x00094463 File Offset: 0x00092663
		protected virtual void Initialize()
		{
		}

		// Token: 0x06000DC7 RID: 3527 RVA: 0x00094465 File Offset: 0x00092665
		protected virtual void LoadContent()
		{
		}

		// Token: 0x06000DC8 RID: 3528 RVA: 0x00094467 File Offset: 0x00092667
		protected virtual void UnloadContent()
		{
		}

		// Token: 0x06000DC9 RID: 3529 RVA: 0x00094469 File Offset: 0x00092669
		protected virtual void Update(GameTime game_time)
		{
		}

		// Token: 0x06000DCA RID: 3530 RVA: 0x0009446B File Offset: 0x0009266B
		protected virtual void OnActivated(object sender, EventArgs args)
		{
		}

		// Token: 0x06000DCB RID: 3531 RVA: 0x0009446D File Offset: 0x0009266D
		protected virtual void Draw(GameTime game_time)
		{
		}

		// Token: 0x170001C4 RID: 452
		// (get) Token: 0x06000DCC RID: 3532 RVA: 0x0009446F File Offset: 0x0009266F
		public GraphicsDevice GraphicsDevice
		{
			get
			{
				return GameRunner.instance.GraphicsDevice;
			}
		}

		// Token: 0x170001C5 RID: 453
		// (get) Token: 0x06000DCD RID: 3533 RVA: 0x0009447B File Offset: 0x0009267B
		public ContentManager Content
		{
			get
			{
				return GameRunner.instance.Content;
			}
		}

		// Token: 0x170001C6 RID: 454
		// (get) Token: 0x06000DCE RID: 3534 RVA: 0x00094487 File Offset: 0x00092687
		public GameComponentCollection Components
		{
			get
			{
				return GameRunner.instance.Components;
			}
		}

		// Token: 0x170001C7 RID: 455
		// (get) Token: 0x06000DCF RID: 3535 RVA: 0x00094493 File Offset: 0x00092693
		public GameWindow Window
		{
			get
			{
				return GameRunner.instance.Window;
			}
		}

		// Token: 0x170001C8 RID: 456
		// (get) Token: 0x06000DD0 RID: 3536 RVA: 0x0009449F File Offset: 0x0009269F
		// (set) Token: 0x06000DD1 RID: 3537 RVA: 0x000944AB File Offset: 0x000926AB
		public bool IsFixedTimeStep
		{
			get
			{
				return GameRunner.instance.IsFixedTimeStep;
			}
			set
			{
				GameRunner.instance.IsFixedTimeStep = value;
			}
		}

		// Token: 0x170001C9 RID: 457
		// (get) Token: 0x06000DD2 RID: 3538 RVA: 0x000944B8 File Offset: 0x000926B8
		public bool IsActive
		{
			get
			{
				return GameRunner.instance.IsActive;
			}
		}

		// Token: 0x170001CA RID: 458
		// (get) Token: 0x06000DD3 RID: 3539 RVA: 0x000944C4 File Offset: 0x000926C4
		// (set) Token: 0x06000DD4 RID: 3540 RVA: 0x000944D0 File Offset: 0x000926D0
		public bool IsMouseVisible
		{
			get
			{
				return GameRunner.instance.IsMouseVisible;
			}
			set
			{
				GameRunner.instance.IsMouseVisible = value;
			}
		}

		// Token: 0x06000DD5 RID: 3541 RVA: 0x000944DD File Offset: 0x000926DD
		protected virtual void BeginDraw()
		{
		}

		// Token: 0x06000DD6 RID: 3542 RVA: 0x000944DF File Offset: 0x000926DF
		protected virtual void EndDraw()
		{
		}

		// Token: 0x06000DD7 RID: 3543 RVA: 0x000944E1 File Offset: 0x000926E1
		public void Exit()
		{
			GameRunner.instance.Exit();
		}

		// Token: 0x170001CB RID: 459
		// (get) Token: 0x06000DD8 RID: 3544 RVA: 0x000944ED File Offset: 0x000926ED
		// (set) Token: 0x06000DD9 RID: 3545 RVA: 0x000944F9 File Offset: 0x000926F9
		public TimeSpan TargetElapsedTime
		{
			get
			{
				return GameRunner.instance.TargetElapsedTime;
			}
			set
			{
				GameRunner.instance.TargetElapsedTime = value;
			}
		}

		// Token: 0x04000934 RID: 2356
		public object staticVarHolder;
	}
}

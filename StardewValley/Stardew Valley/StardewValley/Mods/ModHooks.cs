using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Events;
using StardewValley.Menus;
using xTile.Dimensions;

namespace StardewValley.Mods
{
	// Token: 0x0200022F RID: 559
	public class ModHooks
	{
		// Token: 0x060024D3 RID: 9427 RVA: 0x00192CFB File Offset: 0x00190EFB
		public virtual void OnGame1_PerformTenMinuteClockUpdate(Action action)
		{
			action();
		}

		// Token: 0x060024D4 RID: 9428 RVA: 0x00192D03 File Offset: 0x00190F03
		public virtual void OnGame1_NewDayAfterFade(Action action)
		{
			action();
		}

		// Token: 0x060024D5 RID: 9429 RVA: 0x00192D0B File Offset: 0x00190F0B
		public virtual void OnGame1_ShowEndOfNightStuff(Action action)
		{
			action();
		}

		// Token: 0x060024D6 RID: 9430 RVA: 0x00192D13 File Offset: 0x00190F13
		public virtual void OnGame1_UpdateControlInput(ref KeyboardState keyboardState, ref MouseState mouseState, ref GamePadState gamePadState, Action action)
		{
			action();
		}

		// Token: 0x060024D7 RID: 9431 RVA: 0x00192D1C File Offset: 0x00190F1C
		public virtual void OnGameLocation_ResetForPlayerEntry(GameLocation location, Action action)
		{
			action();
		}

		// Token: 0x060024D8 RID: 9432 RVA: 0x00192D24 File Offset: 0x00190F24
		public virtual bool OnGameLocation_CheckAction(GameLocation location, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, Func<bool> action)
		{
			return action();
		}

		// Token: 0x060024D9 RID: 9433 RVA: 0x00192D2D File Offset: 0x00190F2D
		public virtual FarmEvent OnUtility_PickFarmEvent(Func<FarmEvent> action)
		{
			return action();
		}

		// Token: 0x060024DA RID: 9434 RVA: 0x00192D35 File Offset: 0x00190F35
		public virtual void AfterNewDayBarrier(string barrier_id)
		{
		}

		// Token: 0x060024DB RID: 9435 RVA: 0x00192D37 File Offset: 0x00190F37
		public virtual void CreatedInitialLocations()
		{
		}

		// Token: 0x060024DC RID: 9436 RVA: 0x00192D39 File Offset: 0x00190F39
		public virtual void SaveAddedLocations()
		{
		}

		// Token: 0x060024DD RID: 9437 RVA: 0x00192D3B File Offset: 0x00190F3B
		public virtual bool OnRendering(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D target_screen)
		{
			return true;
		}

		// Token: 0x060024DE RID: 9438 RVA: 0x00192D3E File Offset: 0x00190F3E
		public virtual void OnRendered(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D target_screen)
		{
		}

		// Token: 0x060024DF RID: 9439 RVA: 0x00192D40 File Offset: 0x00190F40
		public virtual bool TryDrawMenu(IClickableMenu menu, Action draw_menu_action)
		{
			if (draw_menu_action != null)
			{
				draw_menu_action();
			}
			return true;
		}

		// Token: 0x060024E0 RID: 9440 RVA: 0x00192D4C File Offset: 0x00190F4C
		public virtual Task StartTask(Task task, string id)
		{
			task.Start();
			return task;
		}

		// Token: 0x060024E1 RID: 9441 RVA: 0x00192D55 File Offset: 0x00190F55
		public virtual Task<T> StartTask<T>(Task<T> task, string id)
		{
			task.Start();
			return task;
		}
	}
}

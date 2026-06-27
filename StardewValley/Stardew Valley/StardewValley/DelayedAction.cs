using System;
using Microsoft.Xna.Framework;
using StardewValley.Audio;
using StardewValley.GameData;

namespace StardewValley
{
	// Token: 0x02000097 RID: 151
	public class DelayedAction
	{
		// Token: 0x06000698 RID: 1688 RVA: 0x00025688 File Offset: 0x00023888
		public DelayedAction(int delay)
		{
			this.timeUntilAction = delay;
		}

		// Token: 0x06000699 RID: 1689 RVA: 0x00025697 File Offset: 0x00023897
		public DelayedAction(int delay, Action behavior)
		{
			this.timeUntilAction = delay;
			this.behavior = behavior;
		}

		// Token: 0x0600069A RID: 1690 RVA: 0x000256B0 File Offset: 0x000238B0
		public bool update(GameTime time)
		{
			if (!this.waitUntilMenusGone || Game1.activeClickableMenu == null)
			{
				this.timeUntilAction -= time.ElapsedGameTime.Milliseconds;
				if (this.timeUntilAction <= 0)
				{
					this.behavior();
				}
			}
			return this.timeUntilAction <= 0;
		}

		// Token: 0x0600069B RID: 1691 RVA: 0x00025708 File Offset: 0x00023908
		public static void warpAfterDelay(string targetLocation, Point targetTile, int delay)
		{
			DelayedAction action = new DelayedAction(delay);
			action.behavior = new Action(action.ApplyWarp);
			action.stringData = targetLocation;
			action.pointData = targetTile;
			Game1.delayedActions.Add(action);
		}

		// Token: 0x0600069C RID: 1692 RVA: 0x00025748 File Offset: 0x00023948
		public static void addTemporarySpriteAfterDelay(TemporaryAnimatedSprite sprite, GameLocation location, int delay, bool waitUntilMenusGone = false)
		{
			DelayedAction action = new DelayedAction(delay);
			action.behavior = new Action(action.ApplyTempSprite);
			action.temporarySpriteData = sprite;
			action.location = location;
			action.waitUntilMenusGone = waitUntilMenusGone;
			Game1.delayedActions.Add(action);
		}

		// Token: 0x0600069D RID: 1693 RVA: 0x00025790 File Offset: 0x00023990
		public static void playSoundAfterDelay(string soundName, int delay, GameLocation location = null, Vector2? position = null, int pitch = -1, bool local = false)
		{
			DelayedAction action = new DelayedAction(delay);
			if (local)
			{
				action.behavior = new Action(action.ApplySoundLocal);
			}
			else
			{
				action.behavior = new Action(action.ApplySound);
			}
			action.stringData = soundName;
			action.location = location;
			action.intData = pitch;
			if (position != null)
			{
				action.pointData = Utility.Vector2ToPoint(position.Value);
			}
			Game1.delayedActions.Add(action);
		}

		// Token: 0x0600069E RID: 1694 RVA: 0x0002580C File Offset: 0x00023A0C
		public static void removeTemporarySpriteAfterDelay(GameLocation location, int idOfTempSprite, int delay)
		{
			DelayedAction action = new DelayedAction(delay);
			action.behavior = new Action(action.ApplyRemoveTemporarySprite);
			action.location = location;
			action.intData = idOfTempSprite;
			Game1.delayedActions.Add(action);
		}

		// Token: 0x0600069F RID: 1695 RVA: 0x0002584C File Offset: 0x00023A4C
		public static DelayedAction playMusicAfterDelay(string musicName, int delay, bool interruptable = true)
		{
			DelayedAction action = new DelayedAction(delay);
			action.behavior = new Action(action.ApplyMusicTrack);
			action.stringData = musicName;
			action.intData = ((interruptable > false) ? 1 : 0);
			Game1.delayedActions.Add(action);
			return action;
		}

		// Token: 0x060006A0 RID: 1696 RVA: 0x00025890 File Offset: 0x00023A90
		public static void textAboveHeadAfterDelay(string text, NPC who, int delay)
		{
			DelayedAction action = new DelayedAction(delay);
			action.behavior = new Action(action.ApplyTextAboveHead);
			action.stringData = text;
			action.character = who;
			Game1.delayedActions.Add(action);
		}

		// Token: 0x060006A1 RID: 1697 RVA: 0x000258D0 File Offset: 0x00023AD0
		public static void stopFarmerGlowing(int delay)
		{
			DelayedAction action = new DelayedAction(delay);
			action.behavior = new Action(action.ApplyStopGlowing);
			Game1.delayedActions.Add(action);
		}

		// Token: 0x060006A2 RID: 1698 RVA: 0x00025904 File Offset: 0x00023B04
		public static void showDialogueAfterDelay(string dialogue, int delay)
		{
			DelayedAction action = new DelayedAction(delay);
			action.behavior = new Action(action.ApplyDialogue);
			action.stringData = dialogue;
			Game1.delayedActions.Add(action);
		}

		// Token: 0x060006A3 RID: 1699 RVA: 0x0002593C File Offset: 0x00023B3C
		public static void screenFlashAfterDelay(float intensity, int delay, string sound = null)
		{
			DelayedAction action = new DelayedAction(delay);
			action.behavior = new Action(action.ApplyScreenFlash);
			action.stringData = sound;
			action.floatData = intensity;
			Game1.delayedActions.Add(action);
		}

		// Token: 0x060006A4 RID: 1700 RVA: 0x0002597C File Offset: 0x00023B7C
		public static void removeTileAfterDelay(int x, int y, int delay, GameLocation location, string whichLayer)
		{
			DelayedAction action = new DelayedAction(delay);
			action.behavior = new Action(action.ApplyRemoveMapTile);
			action.pointData = new Point(x, y);
			action.location = location;
			action.stringData = whichLayer;
			Game1.delayedActions.Add(action);
		}

		// Token: 0x060006A5 RID: 1701 RVA: 0x000259CC File Offset: 0x00023BCC
		public static void fadeAfterDelay(Game1.afterFadeFunction behaviorAfterFade, int delay)
		{
			DelayedAction action = new DelayedAction(delay);
			action.behavior = new Action(action.ApplyFade);
			action.afterFadeBehavior = behaviorAfterFade;
			Game1.delayedActions.Add(action);
		}

		// Token: 0x060006A6 RID: 1702 RVA: 0x00025A04 File Offset: 0x00023C04
		public static DelayedAction functionAfterDelay(Action func, int delay)
		{
			DelayedAction action = new DelayedAction(delay);
			action.behavior = func;
			Game1.delayedActions.Add(action);
			return action;
		}

		// Token: 0x060006A7 RID: 1703 RVA: 0x00025A2B File Offset: 0x00023C2B
		private void ApplyFade()
		{
			Game1.globalFadeToBlack(this.afterFadeBehavior, 0.02f);
		}

		// Token: 0x060006A8 RID: 1704 RVA: 0x00025A40 File Offset: 0x00023C40
		private void ApplyTextAboveHead()
		{
			string text = this.stringData;
			if (text != null)
			{
				NPC npc = this.character;
				if (npc == null)
				{
					return;
				}
				npc.showTextAboveHead(text, null, 2, 3000, 0);
			}
		}

		// Token: 0x060006A9 RID: 1705 RVA: 0x00025A78 File Offset: 0x00023C78
		private void ApplyTempSprite()
		{
			if (this.temporarySpriteData != null)
			{
				GameLocation gameLocation = this.location;
				if (gameLocation == null)
				{
					return;
				}
				gameLocation.TemporarySprites.Add(this.temporarySpriteData);
			}
		}

		// Token: 0x060006AA RID: 1706 RVA: 0x00025A9D File Offset: 0x00023C9D
		private void ApplyStopGlowing()
		{
			Game1.player.stopGlowing();
			Game1.player.stopJittering();
			Game1.screenGlowHold = false;
			if (Game1.isFestival() && Game1.IsFall)
			{
				Game1.changeMusicTrack("fallFest", false, MusicContext.Default);
			}
		}

		// Token: 0x060006AB RID: 1707 RVA: 0x00025AD3 File Offset: 0x00023CD3
		private void ApplyDialogue()
		{
			Game1.drawObjectDialogue(this.stringData);
		}

		// Token: 0x060006AC RID: 1708 RVA: 0x00025AE0 File Offset: 0x00023CE0
		private void ApplyWarp()
		{
			string targetLocation = this.stringData;
			Point targetTile = this.pointData;
			if (targetLocation != null)
			{
				Game1.warpFarmer(targetLocation, targetTile.X, targetTile.Y, false);
			}
		}

		// Token: 0x060006AD RID: 1709 RVA: 0x00025B14 File Offset: 0x00023D14
		private void ApplyRemoveMapTile()
		{
			string layerId = this.stringData;
			Point tile = this.pointData;
			if (layerId != null)
			{
				GameLocation gameLocation = this.location;
				if (gameLocation == null)
				{
					return;
				}
				gameLocation.removeTile(tile.X, tile.Y, layerId);
			}
		}

		// Token: 0x060006AE RID: 1710 RVA: 0x00025B50 File Offset: 0x00023D50
		private void ApplyRemoveTemporarySprite()
		{
			int spriteId = this.intData;
			GameLocation gameLocation = this.location;
			if (gameLocation == null)
			{
				return;
			}
			gameLocation.removeTemporarySpritesWithID(spriteId);
		}

		// Token: 0x060006AF RID: 1711 RVA: 0x00025B78 File Offset: 0x00023D78
		private void ApplySoundHelper(bool local)
		{
			string soundId = this.stringData;
			int? pitch = (this.intData > -1) ? new int?(this.intData) : null;
			Vector2? position = (this.pointData != Point.Zero) ? new Vector2?(Utility.PointToVector2(this.pointData)) : null;
			if (soundId != null)
			{
				if (this.location == null)
				{
					Game1.playSound(soundId, pitch);
					return;
				}
				if (local)
				{
					this.location.localSound(soundId, position, pitch, SoundContext.Default);
					return;
				}
				this.location.playSound(soundId, position, pitch, SoundContext.Default);
			}
		}

		// Token: 0x060006B0 RID: 1712 RVA: 0x00025C11 File Offset: 0x00023E11
		private void ApplySound()
		{
			this.ApplySoundHelper(false);
		}

		// Token: 0x060006B1 RID: 1713 RVA: 0x00025C1A File Offset: 0x00023E1A
		private void ApplySoundLocal()
		{
			this.ApplySoundHelper(true);
		}

		// Token: 0x060006B2 RID: 1714 RVA: 0x00025C24 File Offset: 0x00023E24
		private void ApplyMusicTrack()
		{
			string cueId = this.stringData;
			bool interruptable = this.intData > 0;
			if (cueId != null)
			{
				Game1.changeMusicTrack(cueId, interruptable, MusicContext.Default);
			}
		}

		// Token: 0x060006B3 RID: 1715 RVA: 0x00025C50 File Offset: 0x00023E50
		private void ApplyScreenFlash()
		{
			float flashAlpha = this.floatData;
			string soundId = this.stringData;
			if (!string.IsNullOrEmpty(soundId))
			{
				Game1.playSound(soundId, null);
			}
			Game1.flashAlpha = flashAlpha;
		}

		// Token: 0x04000344 RID: 836
		public int timeUntilAction;

		// Token: 0x04000345 RID: 837
		public int intData;

		// Token: 0x04000346 RID: 838
		public float floatData;

		// Token: 0x04000347 RID: 839
		public string stringData;

		// Token: 0x04000348 RID: 840
		public Point pointData;

		// Token: 0x04000349 RID: 841
		public NPC character;

		// Token: 0x0400034A RID: 842
		public GameLocation location;

		// Token: 0x0400034B RID: 843
		public Action behavior;

		// Token: 0x0400034C RID: 844
		public Game1.afterFadeFunction afterFadeBehavior;

		// Token: 0x0400034D RID: 845
		public bool waitUntilMenusGone;

		// Token: 0x0400034E RID: 846
		public TemporaryAnimatedSprite temporarySpriteData;
	}
}

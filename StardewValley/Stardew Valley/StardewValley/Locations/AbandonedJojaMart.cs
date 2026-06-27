using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Menus;
using StardewValley.Network;

namespace StardewValley.Locations
{
	// Token: 0x020002BD RID: 701
	public class AbandonedJojaMart : GameLocation
	{
		// Token: 0x06002D8C RID: 11660 RVA: 0x0023960C File Offset: 0x0023780C
		public AbandonedJojaMart()
		{
		}

		// Token: 0x06002D8D RID: 11661 RVA: 0x0023962B File Offset: 0x0023782B
		public AbandonedJojaMart(string mapPath, string name) : base(mapPath, name)
		{
		}

		// Token: 0x06002D8E RID: 11662 RVA: 0x0023964C File Offset: 0x0023784C
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.restoreAreaCutsceneEvent, "restoreAreaCutsceneEvent").AddField(this.bundleMutex.NetFields, "bundleMutex.NetFields");
			this.restoreAreaCutsceneEvent.onEvent += this.doRestoreAreaCutscene;
		}

		// Token: 0x06002D8F RID: 11663 RVA: 0x002396A2 File Offset: 0x002378A2
		public void checkBundle()
		{
			this.bundleMutex.RequestLock(delegate
			{
				Dictionary<int, bool[]> bundlesDict = Game1.RequireLocation<CommunityCenter>("CommunityCenter", false).bundlesDict();
				Game1.activeClickableMenu = new JunimoNoteMenu(6, bundlesDict);
			}, null);
		}

		// Token: 0x06002D90 RID: 11664 RVA: 0x002396CF File Offset: 0x002378CF
		public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
		{
			base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
			this.bundleMutex.Update(this);
			if (this.bundleMutex.IsLockHeld() && Game1.activeClickableMenu == null)
			{
				this.bundleMutex.ReleaseLock();
			}
			this.restoreAreaCutsceneEvent.Poll();
		}

		// Token: 0x06002D91 RID: 11665 RVA: 0x0023970F File Offset: 0x0023790F
		public void restoreAreaCutscene()
		{
			this.restoreAreaCutsceneEvent.Fire();
		}

		// Token: 0x06002D92 RID: 11666 RVA: 0x0023971C File Offset: 0x0023791C
		private void doRestoreAreaCutscene()
		{
			if (Game1.currentLocation == this)
			{
				Game1.player.freezePause = 1000;
				DelayedAction.removeTileAfterDelay(8, 8, 100, Game1.currentLocation, "Buildings");
				Game1.RequireLocation("AbandonedJojaMart", false).startEvent(new Event(Game1.content.Load<Dictionary<string, string>>("Data\\Events\\AbandonedJojaMart")["missingBundleComplete"], "Data\\Events\\AbandonedJojaMart", "192393", null));
			}
			Game1.addMailForTomorrow("ccMovieTheater", true, true);
			if (Game1.player.team.theaterBuildDate.Value < 0L)
			{
				Game1.player.team.theaterBuildDate.Set((long)(Game1.Date.TotalDays + 1));
			}
		}

		// Token: 0x06002D93 RID: 11667 RVA: 0x002397D4 File Offset: 0x002379D4
		protected override void resetSharedState()
		{
			bool[] bundles;
			if (Game1.netWorldState.Value.Bundles.TryGetValue(36, out bundles) && !this.bundleMutex.IsLocked() && !Game1.eventUp && !Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
			{
				bool[] array = bundles;
				for (int i = 0; i < array.Length; i++)
				{
					if (!array[i])
					{
						return;
					}
				}
				this.restoreAreaCutscene();
			}
			base.resetSharedState();
		}

		// Token: 0x04001F48 RID: 8008
		[XmlIgnore]
		private readonly NetEvent0 restoreAreaCutsceneEvent = new NetEvent0(false);

		// Token: 0x04001F49 RID: 8009
		[XmlIgnore]
		public NetMutex bundleMutex = new NetMutex();
	}
}

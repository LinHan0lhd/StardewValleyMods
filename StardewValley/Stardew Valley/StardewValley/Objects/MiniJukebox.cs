using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewValley.Objects
{
	// Token: 0x020001B1 RID: 433
	public class MiniJukebox : Object
	{
		// Token: 0x1700032D RID: 813
		// (get) Token: 0x06001F0A RID: 7946 RVA: 0x001657B3 File Offset: 0x001639B3
		public override string TypeDefinitionId
		{
			get
			{
				return "(BC)";
			}
		}

		// Token: 0x06001F0B RID: 7947 RVA: 0x001657BA File Offset: 0x001639BA
		public MiniJukebox()
		{
		}

		// Token: 0x06001F0C RID: 7948 RVA: 0x001657C4 File Offset: 0x001639C4
		public MiniJukebox(Vector2 position) : base(position, "209", false)
		{
			this.Name = "Mini-Jukebox";
			this.type.Value = "Crafting";
			this.bigCraftable.Value = true;
			this.canBeSetDown.Value = true;
		}

		// Token: 0x06001F0D RID: 7949 RVA: 0x00165814 File Offset: 0x00163A14
		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			GameLocation location = this.Location;
			if (!location.IsFarm && !location.IsGreenhouse && !(location is Cellar) && !(location is IslandWest))
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Mini_JukeBox_NotFarmPlay"), true);
			}
			else if (location.IsOutdoors && location.IsRainingHere())
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Mini_JukeBox_OutdoorRainy"), true);
			}
			else
			{
				List<string> jukeboxTracks = Utility.GetJukeboxTracks(Game1.player, Game1.player.currentLocation);
				jukeboxTracks.Insert(0, "turn_off");
				jukeboxTracks.Add("random");
				Game1.activeClickableMenu = new ChooseFromListMenu(jukeboxTracks, new ChooseFromListMenu.actionOnChoosingListOption(this.OnSongChosen), true, location.miniJukeboxTrack.Value);
			}
			return true;
		}

		// Token: 0x06001F0E RID: 7950 RVA: 0x001658D9 File Offset: 0x00163AD9
		public void RegisterToLocation()
		{
			GameLocation location = this.Location;
			if (location == null)
			{
				return;
			}
			location.OnMiniJukeboxAdded();
		}

		// Token: 0x06001F0F RID: 7951 RVA: 0x001658EB File Offset: 0x00163AEB
		public override void performRemoveAction()
		{
			GameLocation location = this.Location;
			if (location != null)
			{
				location.OnMiniJukeboxRemoved();
			}
			base.performRemoveAction();
		}

		// Token: 0x06001F10 RID: 7952 RVA: 0x00165904 File Offset: 0x00163B04
		public override void updateWhenCurrentLocation(GameTime time)
		{
			GameLocation environment = this.Location;
			if (environment != null && environment.IsMiniJukeboxPlaying())
			{
				this.showNextIndex.Value = true;
				if (this.showNote)
				{
					this.showNote = false;
					for (int i = 0; i < 4; i++)
					{
						environment.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(516, 1916, 7, 10), 9999f, 1, 1, this.tileLocation.Value * 64f + new Vector2((float)Game1.random.Next(48), -80f), false, false, (this.tileLocation.Value.Y + 1f) * 64f / 10000f, 0.01f, Color.White, 4f, 0f, 0f, 0f, false)
						{
							xPeriodic = true,
							xPeriodicLoopTime = 1200f,
							xPeriodicRange = 8f,
							motion = new Vector2((float)Game1.random.Next(-10, 10) / 100f, -1f),
							delayBeforeAnimationStart = 1200 + 300 * i
						});
					}
				}
			}
			else
			{
				this.showNextIndex.Value = false;
			}
			base.updateWhenCurrentLocation(time);
		}

		// Token: 0x06001F11 RID: 7953 RVA: 0x00165A68 File Offset: 0x00163C68
		public void OnSongChosen(string selection)
		{
			GameLocation location = this.Location;
			if (location != null)
			{
				if (selection == "turn_off")
				{
					location.miniJukeboxTrack.Value = "";
					return;
				}
				if (selection != location.miniJukeboxTrack.Value)
				{
					this.showNote = true;
					this.shakeTimer = 1000;
				}
				location.miniJukeboxTrack.Value = selection;
				if (selection == "random")
				{
					location.SelectRandomMiniJukeboxTrack();
				}
			}
		}

		// Token: 0x04001314 RID: 4884
		private bool showNote;
	}
}

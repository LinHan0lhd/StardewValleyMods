using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Network;

namespace StardewValley.Menus
{
	// Token: 0x02000270 RID: 624
	public class FarmhandMenu : LoadGameMenu
	{
		// Token: 0x0600292B RID: 10539 RVA: 0x001E406F File Offset: 0x001E226F
		public FarmhandMenu() : this(null)
		{
		}

		// Token: 0x0600292C RID: 10540 RVA: 0x001E4078 File Offset: 0x001E2278
		public FarmhandMenu(Client client) : base(null)
		{
			if (client == null && Program.sdk.Networking != null)
			{
				client = Program.sdk.Networking.GetRequestedClient();
			}
			this.client = client;
			if (client != null)
			{
				this.gettingFarmhands = true;
			}
		}

		// Token: 0x0600292D RID: 10541 RVA: 0x001E40B2 File Offset: 0x001E22B2
		public override bool readyToClose()
		{
			return !this.loading;
		}

		// Token: 0x0600292E RID: 10542 RVA: 0x001E40BD File Offset: 0x001E22BD
		protected override bool hasDeleteButtons()
		{
			return false;
		}

		// Token: 0x0600292F RID: 10543 RVA: 0x001E40C0 File Offset: 0x001E22C0
		protected override void startListPopulation(string filter)
		{
		}

		// Token: 0x06002930 RID: 10544 RVA: 0x001E40C2 File Offset: 0x001E22C2
		public override void UpdateButtons()
		{
			base.UpdateButtons();
			if (LocalMultiplayer.IsLocalMultiplayer(false) && !Game1.game1.IsMainInstance && this.backButton != null)
			{
				this.backButton.visible = false;
			}
		}

		// Token: 0x06002931 RID: 10545 RVA: 0x001E40F4 File Offset: 0x001E22F4
		protected override bool checkListPopulation()
		{
			if (this.client != null && (this.gettingFarmhands || this.approvingFarmhand) && (this.client.availableFarmhands != null || this.client.connectionMessage != null))
			{
				this.timerToLoad = 0;
				this.selected = -1;
				this.loading = false;
				this.gettingFarmhands = false;
				if (this.menuSlots == null)
				{
					this.menuSlots = new List<LoadGameMenu.MenuSlot>();
				}
				else
				{
					this.menuSlots.Clear();
				}
				if (this.client.availableFarmhands == null)
				{
					this.approvingFarmhand = true;
				}
				else
				{
					this.approvingFarmhand = false;
					this.menuSlots.AddRange(from farmer in this.client.availableFarmhands
					select new FarmhandMenu.FarmhandSlot(this, farmer));
				}
				if (Game1.activeClickableMenu is TitleMenu)
				{
					Game1.gameMode = 0;
				}
				else if (!Game1.game1.IsMainInstance)
				{
					Game1.gameMode = 0;
				}
				this.UpdateButtons();
				if (Game1.options.SnappyMenus)
				{
					this.populateClickableComponentList();
					this.snapToDefaultClickableComponent();
				}
			}
			return false;
		}

		// Token: 0x06002932 RID: 10546 RVA: 0x001E4201 File Offset: 0x001E2401
		public override void receiveGamePadButton(Buttons button)
		{
			if (button == Buttons.B && this.readyToClose())
			{
				base.exitThisMenu(true);
			}
			base.receiveGamePadButton(button);
		}

		// Token: 0x06002933 RID: 10547 RVA: 0x001E4224 File Offset: 0x001E2424
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			for (int i = 0; i < this.slotButtons.Count; i++)
			{
				if (this.slotButtons[i].containsPoint(x, y) && i < this.MenuSlots.Count)
				{
					FarmhandMenu.FarmhandSlot slot = this.MenuSlots[this.currentItemIndex + i] as FarmhandMenu.FarmhandSlot;
					if (slot != null && slot.BelongsToAnotherPlayer())
					{
						Game1.playSound("cancel", null);
						return;
					}
				}
			}
			base.receiveLeftClick(x, y, playSound);
		}

		// Token: 0x06002934 RID: 10548 RVA: 0x001E42AC File Offset: 0x001E24AC
		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			if (this.hoverText == "")
			{
				for (int i = 0; i < this.slotButtons.Count; i++)
				{
					if (this.currentItemIndex + i < this.MenuSlots.Count && this.slotButtons[i].containsPoint(x, y))
					{
						FarmhandMenu.FarmhandSlot farmhandSlot = this.MenuSlots[this.currentItemIndex + i] as FarmhandMenu.FarmhandSlot;
						if (farmhandSlot != null && farmhandSlot.BelongsToAnotherPlayer())
						{
							this.hoverText = Game1.content.LoadString("Strings\\UI:Farmhand_Locked");
						}
					}
				}
			}
		}

		// Token: 0x06002935 RID: 10549 RVA: 0x001E434C File Offset: 0x001E254C
		public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
		{
			return (b == null || (b.myID != 800 && b.myID != 801) || this.menuSlots.Count > 4) && base.IsAutomaticSnapValid(direction, a, b);
		}

		// Token: 0x06002936 RID: 10550 RVA: 0x001E4384 File Offset: 0x001E2584
		public override void update(GameTime time)
		{
			if (this.client != null)
			{
				if (!this.client.connectionStarted && this.drawn)
				{
					this.client.connect();
				}
				if (this.client.connectionStarted)
				{
					this.client.receiveMessages();
				}
				if (this.client.readyToPlay)
				{
					Game1.gameMode = 3;
					this.loadClientOptions();
					if (Game1.activeClickableMenu is FarmhandMenu || (Game1.activeClickableMenu is TitleMenu && TitleMenu.subMenu is FarmhandMenu))
					{
						Game1.exitActiveMenu();
					}
				}
				else if (this.client.timedOut)
				{
					if (this.approvingFarmhand)
					{
						Game1.multiplayer.clientRemotelyDisconnected(Multiplayer.IsTimeout(this.client.pendingDisconnect) ? Multiplayer.DisconnectType.Timeout_FarmhandSelection : this.client.pendingDisconnect);
					}
					else
					{
						this.menuSlots.RemoveAll((LoadGameMenu.MenuSlot slot) => slot is FarmhandMenu.FarmhandSlot);
					}
				}
			}
			base.update(time);
		}

		// Token: 0x06002937 RID: 10551 RVA: 0x001E4490 File Offset: 0x001E2690
		private void loadClientOptions()
		{
			if (LocalMultiplayer.IsLocalMultiplayer(false))
			{
				Game1.currentSong = Game1.soundBank.GetCue("spring_day_ambient");
				FarmhandMenu.<loadClientOptions>g__LoadOptions|16_0();
				return;
			}
			Task task = new Task(new Action(FarmhandMenu.<loadClientOptions>g__LoadOptions|16_0));
			Game1.hooks.StartTask(task, "ClientOptions_Load");
		}

		// Token: 0x06002938 RID: 10552 RVA: 0x001E44E4 File Offset: 0x001E26E4
		protected override string getStatusText()
		{
			if (this.client == null)
			{
				return Game1.content.LoadString("Strings\\UI:CoopMenu_NoInvites");
			}
			if (this.client.timedOut)
			{
				return Game1.content.LoadString("Strings\\UI:CoopMenu_Failed");
			}
			if (this.client.connectionMessage != null)
			{
				return this.client.connectionMessage;
			}
			if (this.gettingFarmhands || this.approvingFarmhand)
			{
				return Game1.content.LoadString("Strings\\UI:CoopMenu_Connecting");
			}
			if (this.menuSlots.Count == 0)
			{
				return Game1.content.LoadString("Strings\\UI:CoopMenu_NoSlots");
			}
			return null;
		}

		// Token: 0x06002939 RID: 10553 RVA: 0x001E4580 File Offset: 0x001E2780
		protected override void Dispose(bool disposing)
		{
			if (this.client != null && disposing && Game1.client != this.client)
			{
				Multiplayer.LogDisconnect(Multiplayer.IsTimeout(this.client.pendingDisconnect) ? Multiplayer.DisconnectType.Timeout_FarmhandSelection : Multiplayer.DisconnectType.ExitedToMainMenu_FromFarmhandSelect);
				this.client.disconnect(true);
				if (!Game1.game1.IsMainInstance)
				{
					GameRunner.instance.RemoveGameInstance(Game1.game1);
				}
			}
			base.Dispose(disposing);
		}

		// Token: 0x0600293B RID: 10555 RVA: 0x001E45FC File Offset: 0x001E27FC
		[CompilerGenerated]
		internal static void <loadClientOptions>g__LoadOptions|16_0()
		{
			StartupPreferences preferences = new StartupPreferences();
			preferences.loadPreferences(false, false);
			if (Game1.game1.IsMainInstance)
			{
				Game1.options = preferences.clientOptions;
			}
			else
			{
				Game1.options = new Options();
			}
			Game1.initializeVolumeLevels();
		}

		// Token: 0x04001AEA RID: 6890
		public bool gettingFarmhands;

		// Token: 0x04001AEB RID: 6891
		public bool approvingFarmhand;

		// Token: 0x04001AEC RID: 6892
		public Client client;

		// Token: 0x02000607 RID: 1543
		public class FarmhandSlot : LoadGameMenu.SaveFileSlot
		{
			// Token: 0x060043F6 RID: 17398 RVA: 0x0031B5B4 File Offset: 0x003197B4
			public bool BelongsToAnotherPlayer()
			{
				return (Game1.game1 == null || Game1.game1.IsMainInstance) && this._belongsToAnotherPlayer;
			}

			// Token: 0x060043F7 RID: 17399 RVA: 0x0031B5D4 File Offset: 0x003197D4
			public FarmhandSlot(FarmhandMenu menu, Farmer farmer) : base(menu, farmer, null)
			{
				this.menu = menu;
				if (Program.sdk.Networking != null)
				{
					string local_user_id = Program.sdk.Networking.GetUserID();
					if (local_user_id != "" && farmer != null && farmer.userID.Value != "" && local_user_id != farmer.userID.Value)
					{
						this._belongsToAnotherPlayer = true;
					}
				}
			}

			// Token: 0x060043F8 RID: 17400 RVA: 0x0031B658 File Offset: 0x00319858
			public override void Activate()
			{
				if (this.menu.client != null)
				{
					Game1.game1.loadForNewGame(false);
					Game1.player = this.Farmer;
					this.menu.client.availableFarmhands = null;
					this.menu.client.sendPlayerIntroduction();
					this.menu.approvingFarmhand = true;
					this.menu.menuSlots.Clear();
					Game1.gameMode = 6;
				}
			}

			// Token: 0x060043F9 RID: 17401 RVA: 0x0031B6CB File Offset: 0x003198CB
			public override float getSlotAlpha()
			{
				if (this.BelongsToAnotherPlayer())
				{
					return 0.5f;
				}
				return base.getSlotAlpha();
			}

			// Token: 0x060043FA RID: 17402 RVA: 0x0031B6E4 File Offset: 0x003198E4
			protected override void drawSlotName(SpriteBatch b, int i)
			{
				if (this.Farmer.isCustomized.Value)
				{
					base.drawSlotName(b, i);
					return;
				}
				string slotName = Game1.content.LoadString("Strings\\UI:CoopMenu_NewFarmhand");
				SpriteText.drawString(b, slotName, this.menu.slotButtons[i].bounds.X + 128 + 36, this.menu.slotButtons[i].bounds.Y + 36, 999999, -1, 999999, 1f, 0.88f, false, -1, "", null, SpriteText.ScrollTextAlignment.Left);
			}

			// Token: 0x060043FB RID: 17403 RVA: 0x0031B78B File Offset: 0x0031998B
			protected override void drawSlotShadow(SpriteBatch b, int i)
			{
				if (this.Farmer.isCustomized.Value)
				{
					base.drawSlotShadow(b, i);
				}
			}

			// Token: 0x060043FC RID: 17404 RVA: 0x0031B7A7 File Offset: 0x003199A7
			protected override void drawSlotFarmer(SpriteBatch b, int i)
			{
				if (this.Farmer.isCustomized.Value)
				{
					base.drawSlotFarmer(b, i);
				}
			}

			// Token: 0x060043FD RID: 17405 RVA: 0x0031B7C3 File Offset: 0x003199C3
			protected override void drawSlotTimer(SpriteBatch b, int i)
			{
				if (this.Farmer.isCustomized.Value)
				{
					base.drawSlotTimer(b, i);
				}
			}

			// Token: 0x060043FE RID: 17406 RVA: 0x0031B7DF File Offset: 0x003199DF
			protected override void drawSlotMoney(SpriteBatch b, int i)
			{
			}

			// Token: 0x04002E52 RID: 11858
			protected new FarmhandMenu menu;

			// Token: 0x04002E53 RID: 11859
			protected bool _belongsToAnotherPlayer;
		}
	}
}

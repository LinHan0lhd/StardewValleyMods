using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewValley.Audio;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Network;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	// Token: 0x020002C5 RID: 709
	public class Cabin : FarmHouse
	{
		// Token: 0x1700040D RID: 1037
		// (get) Token: 0x06002E02 RID: 11778 RVA: 0x00240039 File Offset: 0x0023E239
		[XmlIgnore]
		public override Farmer owner
		{
			get
			{
				return this.farmhandReference.Value;
			}
		}

		// Token: 0x06002E03 RID: 11779 RVA: 0x00240046 File Offset: 0x0023E246
		public Cabin()
		{
		}

		// Token: 0x06002E04 RID: 11780 RVA: 0x00240064 File Offset: 0x0023E264
		public Cabin(string map) : base(map, "Cabin")
		{
		}

		// Token: 0x06002E05 RID: 11781 RVA: 0x00240088 File Offset: 0x0023E288
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.farmhandReference.NetFields, "farmhandReference.NetFields").AddField(this.inventoryMutex.NetFields, "inventoryMutex.NetFields");
		}

		// Token: 0x06002E06 RID: 11782 RVA: 0x002400C4 File Offset: 0x0023E2C4
		public void CreateFarmhand()
		{
			if (this.HasOwner)
			{
				return;
			}
			long newId;
			do
			{
				newId = Utility.RandomLong(null);
			}
			while (Game1.GetPlayer(newId, false) != null);
			Farmer newFarmer = new Farmer(new FarmerSprite(null), new Vector2(0f, 0f), 1, "", Farmer.initialTools(), true)
			{
				UniqueMultiplayerID = newId
			};
			newFarmer.addQuest("9");
			newFarmer.homeLocation.Value = base.NameOrUniqueName;
			Game1.netWorldState.Value.farmhandData[newFarmer.UniqueMultiplayerID] = newFarmer;
			this.AssignFarmhand(newFarmer);
			Game1.netWorldState.Value.ResetFarmhandState(newFarmer);
		}

		// Token: 0x06002E07 RID: 11783 RVA: 0x00240166 File Offset: 0x0023E366
		public void DeleteFarmhand()
		{
			if (!this.HasOwner)
			{
				return;
			}
			Game1.player.team.DeleteFarmhand(this.owner);
			this.farmhandReference.Value = null;
		}

		// Token: 0x06002E08 RID: 11784 RVA: 0x00240192 File Offset: 0x0023E392
		public bool CanAssignTo(Farmer farmhand)
		{
			return !this.HasOwner || this.OwnerId == farmhand.UniqueMultiplayerID || this.owner.isUnclaimedFarmhand;
		}

		// Token: 0x06002E09 RID: 11785 RVA: 0x002401B8 File Offset: 0x0023E3B8
		public void AssignFarmhand(Farmer farmhand)
		{
			if (this.HasOwner && this.OwnerId != farmhand.UniqueMultiplayerID)
			{
				if (!this.owner.isUnclaimedFarmhand)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(63, 4);
					defaultInterpolatedStringHandler.AppendLiteral("Can't assign cabin to ");
					defaultInterpolatedStringHandler.AppendFormatted(farmhand.Name);
					defaultInterpolatedStringHandler.AppendLiteral(" (");
					defaultInterpolatedStringHandler.AppendFormatted<long>(farmhand.UniqueMultiplayerID);
					defaultInterpolatedStringHandler.AppendLiteral(") because it's already assigned to ");
					defaultInterpolatedStringHandler.AppendFormatted(this.owner.Name);
					defaultInterpolatedStringHandler.AppendLiteral(" (");
					defaultInterpolatedStringHandler.AppendFormatted<long>(this.owner.UniqueMultiplayerID);
					defaultInterpolatedStringHandler.AppendLiteral(").");
					throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				this.DeleteFarmhand();
			}
			this.farmhandReference.Value = farmhand;
			farmhand.homeLocation.Value = base.NameOrUniqueName;
		}

		// Token: 0x06002E0A RID: 11786 RVA: 0x002402A8 File Offset: 0x0023E4A8
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			int tileIndexAt = base.getTileIndexAt(tileLocation, "Buildings", "indoor");
			if (tileIndexAt - 647 <= 1 && !base.IsOwnerActivated)
			{
				this.inventoryMutex.RequestLock(delegate
				{
					base.playSound("Ship", null, null, SoundContext.Default);
					this.openFarmhandInventory();
				}, null);
				return true;
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		// Token: 0x06002E0B RID: 11787 RVA: 0x00240301 File Offset: 0x0023E501
		public override void updateEvenIfFarmerIsntHere(GameTime time, bool skipWasUpdatedFlush = false)
		{
			base.updateEvenIfFarmerIsntHere(time, skipWasUpdatedFlush);
			this.inventoryMutex.Update(Game1.getOnlineFarmers());
			if (this.inventoryMutex.IsLockHeld() && !(Game1.activeClickableMenu is ItemGrabMenu))
			{
				this.inventoryMutex.ReleaseLock();
			}
		}

		// Token: 0x06002E0C RID: 11788 RVA: 0x0024033F File Offset: 0x0023E53F
		public IInventory getInventory()
		{
			Farmer owner = this.owner;
			if (owner == null)
			{
				return null;
			}
			return owner.Items;
		}

		// Token: 0x06002E0D RID: 11789 RVA: 0x00240354 File Offset: 0x0023E554
		public void openFarmhandInventory()
		{
			Game1.activeClickableMenu = new ItemGrabMenu(this.getInventory(), false, true, new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems), new ItemGrabMenu.behaviorOnItemSelect(this.grabItemFromPlayerInventory), null, new ItemGrabMenu.behaviorOnItemSelect(this.grabItemFromFarmhandInventory), false, true, true, true, true, 1, null, -1, this, ItemExitBehavior.ReturnToPlayer, false);
		}

		// Token: 0x06002E0E RID: 11790 RVA: 0x002403A3 File Offset: 0x0023E5A3
		public bool isInventoryOpen()
		{
			return this.inventoryMutex.IsLocked();
		}

		// Token: 0x06002E0F RID: 11791 RVA: 0x002403B0 File Offset: 0x0023E5B0
		private void grabItemFromPlayerInventory(Item item, Farmer who)
		{
			if (!this.HasOwner)
			{
				return;
			}
			item.FixStackSize();
			Item tmp = this.owner.addItemToInventory(item);
			if (tmp == null)
			{
				who.removeItemFromInventory(item);
			}
			else
			{
				who.addItemToInventory(tmp);
			}
			int oldID = (Game1.activeClickableMenu.currentlySnappedComponent != null) ? Game1.activeClickableMenu.currentlySnappedComponent.myID : -1;
			this.openFarmhandInventory();
			if (oldID != -1)
			{
				Game1.activeClickableMenu.currentlySnappedComponent = Game1.activeClickableMenu.getComponentWithID(oldID);
				Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x06002E10 RID: 11792 RVA: 0x00240435 File Offset: 0x0023E635
		private void grabItemFromFarmhandInventory(Item item, Farmer who)
		{
			if (who.couldInventoryAcceptThisItem(item))
			{
				this.getInventory().Remove(item);
				this.openFarmhandInventory();
			}
		}

		// Token: 0x06002E11 RID: 11793 RVA: 0x00240453 File Offset: 0x0023E653
		public override void updateWarps()
		{
			if (Game1.IsClient)
			{
				return;
			}
			base.updateWarps();
		}

		// Token: 0x06002E12 RID: 11794 RVA: 0x00240464 File Offset: 0x0023E664
		public List<Item> demolish()
		{
			List<Item> items = (from item in new List<Item>(this.getInventory())
			where item != null
			select item).ToList<Item>();
			this.getInventory().Clear();
			Farmer.removeInitialTools(items);
			foreach (NPC npc in new List<NPC>(this.characters))
			{
				if (npc.IsVillager && Game1.characterData.ContainsKey(npc.Name))
				{
					npc.reloadDefaultLocation();
					npc.ClearSchedule();
					Game1.warpCharacter(npc, npc.DefaultMap, npc.DefaultPosition / 64f);
				}
				Pet pet = npc as Pet;
				if (pet != null)
				{
					pet.warpToFarmHouse(Game1.MasterPlayer);
				}
			}
			Cellar cellar = base.GetCellar();
			if (cellar != null)
			{
				cellar.objects.Clear();
				cellar.setUpAgingBoards();
			}
			if (this.HasOwner)
			{
				Game1.player.team.DeleteFarmhand(this.owner);
			}
			Game1.updateCellarAssignments();
			return items;
		}

		// Token: 0x06002E13 RID: 11795 RVA: 0x00240594 File Offset: 0x0023E794
		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			if (this.HasOwner)
			{
				this.owner.stamina = (float)this.owner.MaxStamina;
			}
		}

		// Token: 0x06002E14 RID: 11796 RVA: 0x002405BC File Offset: 0x0023E7BC
		public override Point getPorchStandingSpot()
		{
			Building parentBuilding = this.ParentBuilding;
			if (parentBuilding == null)
			{
				return base.getPorchStandingSpot();
			}
			return parentBuilding.getPorchStandingSpot();
		}

		// Token: 0x04001F85 RID: 8069
		[XmlElement("farmhand")]
		public Farmer obsolete_farmhand;

		// Token: 0x04001F86 RID: 8070
		[XmlElement("farmhandReference")]
		public readonly NetFarmerRef farmhandReference = new NetFarmerRef();

		// Token: 0x04001F87 RID: 8071
		[XmlIgnore]
		public readonly NetMutex inventoryMutex = new NetMutex();
	}
}

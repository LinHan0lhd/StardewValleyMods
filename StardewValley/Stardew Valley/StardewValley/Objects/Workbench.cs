using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Network;

namespace StardewValley.Objects
{
	// Token: 0x020001BC RID: 444
	public class Workbench : Object
	{
		// Token: 0x17000339 RID: 825
		// (get) Token: 0x06001FB8 RID: 8120 RVA: 0x0016C474 File Offset: 0x0016A674
		public override string TypeDefinitionId
		{
			get
			{
				return "(BC)";
			}
		}

		// Token: 0x06001FB9 RID: 8121 RVA: 0x0016C47B File Offset: 0x0016A67B
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.mutex.NetFields, "mutex.NetFields");
		}

		// Token: 0x06001FBA RID: 8122 RVA: 0x0016C49F File Offset: 0x0016A69F
		public Workbench()
		{
		}

		// Token: 0x06001FBB RID: 8123 RVA: 0x0016C4B4 File Offset: 0x0016A6B4
		public Workbench(Vector2 position) : base(position, "208", false)
		{
			this.Name = "Workbench";
			this.type.Value = "Crafting";
			this.bigCraftable.Value = true;
			this.canBeSetDown.Value = true;
		}

		// Token: 0x06001FBC RID: 8124 RVA: 0x0016C50C File Offset: 0x0016A70C
		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			GameLocation location = this.Location;
			if (location == null)
			{
				return false;
			}
			if (justCheckingForActivity)
			{
				return true;
			}
			List<Chest> nearby_chests = new List<Chest>();
			Point? fridgePosition = location.GetFridgePosition();
			Vector2[] neighbor_tiles = new Vector2[]
			{
				new Vector2(-1f, 1f),
				new Vector2(0f, 1f),
				new Vector2(1f, 1f),
				new Vector2(-1f, 0f),
				new Vector2(1f, 0f),
				new Vector2(-1f, -1f),
				new Vector2(0f, -1f),
				new Vector2(1f, -1f)
			};
			for (int i = 0; i < neighbor_tiles.Length; i++)
			{
				Vector2 tile_location = new Vector2((float)((int)(this.tileLocation.X + neighbor_tiles[i].X)), (float)((int)(this.tileLocation.Y + neighbor_tiles[i].Y)));
				int num = (int)this.tileLocation.X;
				int? num2 = (fridgePosition != null) ? new int?(fridgePosition.GetValueOrDefault().X) : null;
				if ((num == num2.GetValueOrDefault() & num2 != null) && (int)this.tileLocation.Y == fridgePosition.Value.Y)
				{
					Chest fridge = location.GetFridge(true);
					if (fridge != null)
					{
						nearby_chests.Add(fridge);
					}
				}
				Object neighbor_object;
				if (location.objects.TryGetValue(tile_location, out neighbor_object))
				{
					Chest chest = neighbor_object as Chest;
					if (chest != null && (chest.SpecialChestType == Chest.SpecialChestTypes.None || chest.SpecialChestType == Chest.SpecialChestTypes.BigChest))
					{
						nearby_chests.Add(chest);
					}
				}
			}
			List<NetMutex> muticies = new List<NetMutex>();
			List<IInventory> inventories = new List<IInventory>();
			foreach (Chest chest2 in nearby_chests)
			{
				muticies.Add(chest2.mutex);
				inventories.Add(chest2.Items);
			}
			if (!this.mutex.IsLocked())
			{
				new MultipleMutexRequest(muticies, delegate(MultipleMutexRequest request)
				{
					IClickableMenu.onExit <>9__3;
					this.mutex.RequestLock(delegate
					{
						Vector2 center = Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, 0, 0);
						Game1.activeClickableMenu = new CraftingPage((int)center.X, (int)center.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, false, true, inventories);
						IClickableMenu activeClickableMenu = Game1.activeClickableMenu;
						IClickableMenu.onExit exitFunction;
						if ((exitFunction = <>9__3) == null)
						{
							exitFunction = (<>9__3 = delegate()
							{
								this.mutex.ReleaseLock();
								request.ReleaseLocks();
							});
						}
						activeClickableMenu.exitFunction = exitFunction;
					}, new Action(request.ReleaseLocks));
				}, delegate(MultipleMutexRequest request)
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Workbench_Chest_Warning"), true);
				});
			}
			return true;
		}

		// Token: 0x06001FBD RID: 8125 RVA: 0x0016C7B4 File Offset: 0x0016A9B4
		public override void updateWhenCurrentLocation(GameTime time)
		{
			GameLocation environment = this.Location;
			if (environment != null)
			{
				this.mutex.Update(environment);
			}
			base.updateWhenCurrentLocation(time);
		}

		// Token: 0x0400136F RID: 4975
		[XmlIgnore]
		public readonly NetMutex mutex = new NetMutex();
	}
}

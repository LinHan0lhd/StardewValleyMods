using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.GameData.Buildings;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using xTile.Dimensions;
using xTile.Layers;

namespace StardewValley.Menus
{
	// Token: 0x02000255 RID: 597
	public class CarpenterMenu : IClickableMenu
	{
		// Token: 0x170003E9 RID: 1001
		// (get) Token: 0x06002793 RID: 10131 RVA: 0x001C3CF9 File Offset: 0x001C1EF9
		// (set) Token: 0x06002794 RID: 10132 RVA: 0x001C3D01 File Offset: 0x001C1F01
		public bool readOnly
		{
			get
			{
				return this._readOnly;
			}
			set
			{
				if (value != this._readOnly)
				{
					this._readOnly = value;
					this.resetBounds();
				}
			}
		}

		// Token: 0x06002795 RID: 10133 RVA: 0x001C3D1C File Offset: 0x001C1F1C
		public CarpenterMenu(string builder, GameLocation targetLocation = null)
		{
			this.Builder = builder;
			this.BuilderLocationName = Game1.currentLocation.NameOrUniqueName;
			this.BuilderViewport = Game1.viewport.Location;
			this.TargetLocation = (targetLocation ?? Game1.getFarm());
			Game1.player.forceCanMove();
			this.resetBounds();
			int index = 0;
			foreach (KeyValuePair<string, BuildingData> data in Game1.buildingData)
			{
				if (!(data.Value.Builder != builder) && GameStateQuery.CheckConditions(data.Value.BuildCondition, targetLocation, null, null, null, null, null) && (data.Value.BuildingToUpgrade == null || this.TargetLocation.getNumberBuildingsConstructed(data.Value.BuildingToUpgrade, false) != 0) && this.IsValidBuildingForLocation(data.Key, data.Value, this.TargetLocation))
				{
					this.Blueprints.Add(new CarpenterMenu.BlueprintEntry(index++, data.Key, data.Value, null));
					if (data.Value.Skins != null)
					{
						foreach (BuildingSkin skin in data.Value.Skins)
						{
							if (skin.ShowAsSeparateConstructionEntry && GameStateQuery.CheckConditions(skin.Condition, this.TargetLocation, null, null, null, null, null))
							{
								this.Blueprints.Add(new CarpenterMenu.BlueprintEntry(index++, data.Key, data.Value, skin.Id));
							}
						}
					}
				}
			}
			this.SetNewActiveBlueprint(0);
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x06002796 RID: 10134 RVA: 0x001C3F78 File Offset: 0x001C2178
		public override bool shouldClampGamePadCursor()
		{
			return this.onFarm;
		}

		// Token: 0x06002797 RID: 10135 RVA: 0x001C3F80 File Offset: 0x001C2180
		public override void snapToDefaultClickableComponent()
		{
			this.currentlySnappedComponent = base.getComponentWithID(107);
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002798 RID: 10136 RVA: 0x001C3F98 File Offset: 0x001C2198
		private void resetBounds()
		{
			bool hasOwnedBuildings = false;
			bool hasPaintableBuildings = false;
			foreach (Building building in this.TargetLocation.buildings)
			{
				if (building.hasCarpenterPermissions())
				{
					hasOwnedBuildings = true;
				}
				if ((building.CanBePainted() || building.CanBeReskinned(true)) && this.HasPermissionsToPaint(building))
				{
					hasPaintableBuildings = true;
				}
			}
			this.xPositionOnScreen = Game1.uiViewport.Width / 2 - this.maxWidthOfBuildingViewer - IClickableMenu.spaceToClearSideBorder;
			this.yPositionOnScreen = Game1.uiViewport.Height / 2 - this.maxHeightOfBuildingViewer / 2 - IClickableMenu.spaceToClearTopBorder + 32;
			this.width = this.maxWidthOfBuildingViewer + this.maxWidthOfDescription + IClickableMenu.spaceToClearSideBorder * 2 + 64;
			this.height = this.maxHeightOfBuildingViewer + IClickableMenu.spaceToClearTopBorder;
			bool isReadOnly = this.readOnly;
			base.initialize(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, true);
			this.okButton = new ClickableTextureComponent("OK", new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 192 - 12, this.yPositionOnScreen + this.maxHeightOfBuildingViewer + 64, 64, 64), null, null, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(366, 373, 16, 16), 4f, false)
			{
				myID = 106,
				rightNeighborID = 104,
				leftNeighborID = 105,
				upNeighborID = 109,
				visible = !isReadOnly
			};
			this.cancelButton = new ClickableTextureComponent("OK", new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, this.yPositionOnScreen + this.maxHeightOfBuildingViewer + 64, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47, -1, -1), 1f, false)
			{
				myID = 107,
				leftNeighborID = (isReadOnly ? 102 : 104),
				upNeighborID = 109
			};
			this.backButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + 64, this.yPositionOnScreen + this.maxHeightOfBuildingViewer + 64, 48, 44), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(352, 495, 12, 11), 4f, false)
			{
				myID = 101,
				rightNeighborID = 102,
				upNeighborID = 109
			};
			this.forwardButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + this.maxWidthOfBuildingViewer - 256 + 16, this.yPositionOnScreen + this.maxHeightOfBuildingViewer + 64, 48, 44), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(365, 495, 12, 11), 4f, false)
			{
				myID = 102,
				leftNeighborID = 101,
				rightNeighborID = -99998,
				upNeighborID = 109
			};
			this.demolishButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\UI:Carpenter_Demolish"), new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 128 - 8, this.yPositionOnScreen + this.maxHeightOfBuildingViewer + 64 - 4, 64, 64), null, null, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(348, 372, 17, 17), 4f, false)
			{
				myID = 104,
				rightNeighborID = 107,
				leftNeighborID = 106,
				upNeighborID = 109,
				visible = (!isReadOnly && Game1.IsMasterGame)
			};
			this.upgradeIcon = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + this.maxWidthOfBuildingViewer - 128 + 32, this.yPositionOnScreen + 8, 36, 52), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(402, 328, 9, 13), 4f, false)
			{
				myID = 103,
				rightNeighborID = 104,
				leftNeighborID = 105,
				upNeighborID = 109,
				visible = !isReadOnly
			};
			this.moveButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\UI:Carpenter_MoveBuildings"), new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 256 - 20, this.yPositionOnScreen + this.maxHeightOfBuildingViewer + 64, 64, 64), null, null, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(257, 284, 16, 16), 4f, false)
			{
				myID = 105,
				rightNeighborID = 106,
				leftNeighborID = -99998,
				upNeighborID = 109,
				visible = (!isReadOnly && (Game1.IsMasterGame || Game1.player.team.farmhandsCanMoveBuildings.Value == FarmerTeam.RemoteBuildingPermissions.On || (Game1.player.team.farmhandsCanMoveBuildings.Value == FarmerTeam.RemoteBuildingPermissions.OwnedBuildings && hasOwnedBuildings)))
			};
			this.paintButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\UI:Carpenter_PaintBuildings"), new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + this.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 320 - 20, this.yPositionOnScreen + this.maxHeightOfBuildingViewer + 64, 64, 64), null, null, Game1.mouseCursors2, new Microsoft.Xna.Framework.Rectangle(80, 208, 16, 16), 4f, false)
			{
				myID = 105,
				rightNeighborID = -99998,
				leftNeighborID = -99998,
				upNeighborID = 109,
				visible = (!isReadOnly && hasPaintableBuildings)
			};
			this.appearanceButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\UI:Carpenter_ChangeAppearance"), new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen + this.maxWidthOfBuildingViewer - 128 + 16, this.yPositionOnScreen + this.maxHeightOfBuildingViewer - 64 + 32, 64, 64), null, null, Game1.mouseCursors2, new Microsoft.Xna.Framework.Rectangle(96, 208, 16, 16), 4f, false)
			{
				myID = 109,
				downNeighborID = -99998
			};
			if (!this.demolishButton.visible)
			{
				this.upgradeIcon.rightNeighborID = this.demolishButton.rightNeighborID;
				this.okButton.rightNeighborID = this.demolishButton.rightNeighborID;
				this.cancelButton.leftNeighborID = this.demolishButton.leftNeighborID;
			}
			if (!this.moveButton.visible)
			{
				this.upgradeIcon.leftNeighborID = this.moveButton.leftNeighborID;
				this.forwardButton.rightNeighborID = -99998;
				this.okButton.leftNeighborID = this.moveButton.leftNeighborID;
			}
			this.UpdateAppearanceButtonVisibility();
		}

		// Token: 0x06002799 RID: 10137 RVA: 0x001C4660 File Offset: 0x001C2860
		public void SetNewActiveBlueprint(int index)
		{
			index %= this.Blueprints.Count;
			if (index < 0)
			{
				index = this.Blueprints.Count + index;
			}
			this.SetNewActiveBlueprint(this.Blueprints[index]);
		}

		// Token: 0x0600279A RID: 10138 RVA: 0x001C4698 File Offset: 0x001C2898
		public void SetNewActiveBlueprint(CarpenterMenu.BlueprintEntry blueprint)
		{
			this.Blueprint = blueprint;
			this.currentBuilding = Building.CreateInstanceFromId(blueprint.Id, Vector2.Zero);
			NetFieldBase<string, NetString> skinId = this.currentBuilding.skinId;
			BuildingSkin skin = blueprint.Skin;
			skinId.Value = ((skin != null) ? skin.Id : null);
			this.ingredients.Clear();
			if (blueprint.BuildMaterials != null)
			{
				foreach (BuildingMaterial material in blueprint.BuildMaterials)
				{
					this.ingredients.Add(ItemRegistry.Create(material.ItemId, material.Amount, 0, false));
				}
			}
			this.UpdateAppearanceButtonVisibility();
			if (Game1.options.SnappyMenus && this.currentlySnappedComponent != null && this.currentlySnappedComponent == this.appearanceButton && !this.appearanceButton.visible)
			{
				this.setCurrentlySnappedComponentTo(102);
				this.snapToDefaultClickableComponent();
			}
		}

		// Token: 0x0600279B RID: 10139 RVA: 0x001C4798 File Offset: 0x001C2998
		public virtual void UpdateAppearanceButtonVisibility()
		{
			if (this.appearanceButton != null && this.currentBuilding != null)
			{
				this.appearanceButton.visible = this.currentBuilding.CanBeReskinned(true);
			}
		}

		// Token: 0x0600279C RID: 10140 RVA: 0x001C47C4 File Offset: 0x001C29C4
		public override void performHoverAction(int x, int y)
		{
			this.cancelButton.tryHover(x, y, 0.1f);
			base.performHoverAction(x, y);
			if (this.onFarm)
			{
				if (this.Action != CarpenterMenu.CarpentryAction.None && !this.freeze)
				{
					foreach (Building building in this.TargetLocation.buildings)
					{
						building.color = Color.White;
					}
					Vector2 tile = new Vector2((float)((Game1.viewport.X + Game1.getOldMouseX(false)) / 64), (float)((Game1.viewport.Y + Game1.getOldMouseY(false)) / 64));
					Building building2;
					if ((building2 = this.TargetLocation.getBuildingAt(tile)) == null && (building2 = this.TargetLocation.getBuildingAt(new Vector2(tile.X, tile.Y + 1f))) == null)
					{
						building2 = (this.TargetLocation.getBuildingAt(new Vector2(tile.X, tile.Y + 2f)) ?? this.TargetLocation.getBuildingAt(new Vector2(tile.X, tile.Y + 3f)));
					}
					Building b = building2;
					BuildingData data = (b != null) ? b.GetData() : null;
					if (data != null)
					{
						int stickOutTilesHigh = (data.SourceRect.IsEmpty ? b.texture.Value.Height : b.GetData().SourceRect.Height) * 4 / 64 - b.tilesHigh.Value;
						if ((float)(b.tileY.Value - stickOutTilesHigh) > tile.Y)
						{
							b = null;
						}
					}
					switch (this.Action)
					{
					case CarpenterMenu.CarpentryAction.Demolish:
						if (b != null && this.hasPermissionsToDemolish(b) && this.CanDemolishThis(b))
						{
							b.color = Color.Red * 0.8f;
							return;
						}
						break;
					case CarpenterMenu.CarpentryAction.Move:
						if (b != null && this.hasPermissionsToMove(b))
						{
							b.color = Color.Lime * 0.8f;
							return;
						}
						break;
					case CarpenterMenu.CarpentryAction.Paint:
						if (b != null && (b.CanBePainted() || b.CanBeReskinned(true)) && this.HasPermissionsToPaint(b))
						{
							b.color = Color.Lime * 0.8f;
						}
						break;
					case CarpenterMenu.CarpentryAction.Upgrade:
						if (b != null)
						{
							b.color = ((b.buildingType.Value == this.Blueprint.UpgradeFrom) ? (Color.Lime * 0.8f) : (Color.Red * 0.8f));
							return;
						}
						break;
					default:
						return;
					}
				}
				return;
			}
			this.backButton.tryHover(x, y, 1f);
			this.forwardButton.tryHover(x, y, 1f);
			this.okButton.tryHover(x, y, 0.1f);
			this.demolishButton.tryHover(x, y, 0.1f);
			this.moveButton.tryHover(x, y, 0.1f);
			this.paintButton.tryHover(x, y, 0.1f);
			this.appearanceButton.tryHover(x, y, 0.1f);
			if (this.Blueprint.IsUpgrade && this.upgradeIcon.containsPoint(x, y))
			{
				this.hoverText = Game1.content.LoadString("Strings\\UI:Carpenter_Upgrade", this.Blueprint.GetDisplayNameForBuildingToUpgrade());
				return;
			}
			if (this.demolishButton.containsPoint(x, y) && this.CanDemolishThis())
			{
				this.hoverText = Game1.content.LoadString("Strings\\UI:Carpenter_Demolish");
				return;
			}
			if (this.moveButton.containsPoint(x, y))
			{
				this.hoverText = Game1.content.LoadString("Strings\\UI:Carpenter_MoveBuildings");
				return;
			}
			if (this.okButton.containsPoint(x, y) && this.CanBuildCurrentBlueprint())
			{
				this.hoverText = Game1.content.LoadString("Strings\\UI:Carpenter_Build");
				return;
			}
			if (this.paintButton.containsPoint(x, y))
			{
				this.hoverText = this.paintButton.name;
				return;
			}
			if (this.appearanceButton.containsPoint(x, y))
			{
				this.hoverText = this.appearanceButton.name;
				return;
			}
			this.hoverText = "";
		}

		// Token: 0x0600279D RID: 10141 RVA: 0x001C4BE8 File Offset: 0x001C2DE8
		public bool hasPermissionsToDemolish(Building b)
		{
			return Game1.IsMasterGame && this.CanDemolishThis(b);
		}

		// Token: 0x0600279E RID: 10142 RVA: 0x001C4BFC File Offset: 0x001C2DFC
		public bool HasPermissionsToPaint(Building b)
		{
			if (b.isCabin || b.HasIndoorsName("Farmhouse"))
			{
				FarmHouse house = b.GetIndoors() as FarmHouse;
				if (house != null)
				{
					return house.IsOwnedByCurrentPlayer || house.OwnerId.ToString() == Game1.player.spouse;
				}
			}
			return true;
		}

		// Token: 0x0600279F RID: 10143 RVA: 0x001C4C5C File Offset: 0x001C2E5C
		public bool hasPermissionsToMove(Building b)
		{
			if (!Game1.getFarm().greenhouseUnlocked.Value && b is GreenhouseBuilding)
			{
				return false;
			}
			if (Game1.IsMasterGame)
			{
				return true;
			}
			FarmerTeam.RemoteBuildingPermissions value = Game1.player.team.farmhandsCanMoveBuildings.Value;
			if (value != FarmerTeam.RemoteBuildingPermissions.OwnedBuildings)
			{
				if (value == FarmerTeam.RemoteBuildingPermissions.On)
				{
					return true;
				}
			}
			else if (b.hasCarpenterPermissions())
			{
				return true;
			}
			return false;
		}

		// Token: 0x060027A0 RID: 10144 RVA: 0x001C4CB8 File Offset: 0x001C2EB8
		public override void receiveGamePadButton(Buttons button)
		{
			base.receiveGamePadButton(button);
			if (!this.onFarm)
			{
				if (button != Buttons.RightTrigger)
				{
					if (button == Buttons.LeftTrigger)
					{
						this.SetNewActiveBlueprint(this.Blueprint.Index - 1);
						Game1.playSound("shwip", null);
						return;
					}
				}
				else
				{
					this.SetNewActiveBlueprint(this.Blueprint.Index + 1);
					Game1.playSound("shwip", null);
				}
			}
		}

		// Token: 0x060027A1 RID: 10145 RVA: 0x001C4D34 File Offset: 0x001C2F34
		public override void gamePadButtonHeld(Buttons b)
		{
			base.gamePadButtonHeld(b);
			if (this.onFarm && (b == Buttons.DPadDown || b == Buttons.DPadRight || b == Buttons.DPadLeft || b == Buttons.DPadUp))
			{
				GamePadState gamepadstate = Game1.input.GetGamePadState();
				MouseState mouseState = Game1.input.GetMouseState();
				int speed = 12 + ((gamepadstate.IsButtonDown(Buttons.RightTrigger) || gamepadstate.IsButtonDown(Buttons.RightShoulder)) ? 8 : 0);
				int xOff = (b == Buttons.DPadRight) ? speed : ((b == Buttons.DPadLeft) ? (-speed) : 0);
				int yOff = (b == Buttons.DPadDown) ? speed : ((b == Buttons.DPadUp) ? (-speed) : 0);
				Game1.setMousePositionRaw(mouseState.X + xOff, mouseState.Y + yOff);
			}
		}

		// Token: 0x060027A2 RID: 10146 RVA: 0x001C4DD8 File Offset: 0x001C2FD8
		public override void receiveKeyPress(Keys key)
		{
			if (this.freeze)
			{
				return;
			}
			if (!this.onFarm)
			{
				base.receiveKeyPress(key);
			}
			if (!Game1.IsFading() && this.onFarm)
			{
				if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.readyToClose() && Game1.locationRequest == null)
				{
					this.returnToCarpentryMenu();
					return;
				}
				if (!Game1.options.SnappyMenus)
				{
					if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
					{
						Game1.panScreen(0, 4);
						return;
					}
					if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
					{
						Game1.panScreen(4, 0);
						return;
					}
					if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
					{
						Game1.panScreen(0, -4);
						return;
					}
					if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
					{
						Game1.panScreen(-4, 0);
					}
				}
			}
		}

		// Token: 0x060027A3 RID: 10147 RVA: 0x001C4EC8 File Offset: 0x001C30C8
		public override void update(GameTime time)
		{
			base.update(time);
			if (this.onFarm && !Game1.IsFading())
			{
				int mouseX = Game1.getOldMouseX(false) + Game1.viewport.X;
				int mouseY = Game1.getOldMouseY(false) + Game1.viewport.Y;
				if (mouseX - Game1.viewport.X < 64)
				{
					Game1.panScreen(-8, 0);
				}
				else if (mouseX - (Game1.viewport.X + Game1.viewport.Width) >= -128)
				{
					Game1.panScreen(8, 0);
				}
				if (mouseY - Game1.viewport.Y < 64)
				{
					Game1.panScreen(0, -8);
				}
				else if (mouseY - (Game1.viewport.Y + Game1.viewport.Height) >= -64)
				{
					Game1.panScreen(0, 8);
				}
				foreach (Keys key in Game1.oldKBState.GetPressedKeys())
				{
					this.receiveKeyPress(key);
				}
				if (!Game1.IsMultiplayer)
				{
					GameLocation target = this.TargetLocation;
					foreach (FarmAnimal farmAnimal in target.animals.Values)
					{
						farmAnimal.MovePosition(Game1.currentGameTime, Game1.viewport, target);
					}
				}
			}
		}

		// Token: 0x060027A4 RID: 10148 RVA: 0x001C501C File Offset: 0x001C321C
		protected bool VerifyTileAccessibility(int tileX, int tileY, Vector2 buildingPosition)
		{
			if (!this.TargetLocation.isTilePassable(new Location(tileX, tileY), Game1.viewport))
			{
				return false;
			}
			int relativeX = tileX - (int)buildingPosition.X;
			int relativeY = tileY - (int)buildingPosition.Y;
			if (!this.buildingToMove.isTilePassable(new Vector2((float)(this.buildingToMove.tileX.Value + relativeX), (float)(this.buildingToMove.tileY.Value + relativeY))))
			{
				return false;
			}
			Building tileBuilding = this.TargetLocation.getBuildingAt(new Vector2((float)tileX, (float)tileY));
			if (tileBuilding != null && !tileBuilding.isMoving && !tileBuilding.isTilePassable(new Vector2((float)tileX, (float)tileY)))
			{
				return false;
			}
			Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle(tileX * 64, tileY * 64, 64, 64);
			tileRect.Inflate(-1, -1);
			using (List<ResourceClump>.Enumerator enumerator = this.TargetLocation.resourceClumps.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.getBoundingBox().Intersects(tileRect))
					{
						return false;
					}
				}
			}
			using (List<LargeTerrainFeature>.Enumerator enumerator2 = this.TargetLocation.largeTerrainFeatures.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					if (enumerator2.Current.getBoundingBox().Intersects(tileRect))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x060027A5 RID: 10149 RVA: 0x001C5198 File Offset: 0x001C3398
		public virtual bool ConfirmBuildingAccessibility(Vector2 buildingPosition)
		{
			if (this.buildingToMove == null)
			{
				return false;
			}
			if (this.buildingToMove.buildingType.Value != "Farmhouse")
			{
				return true;
			}
			Point startPoint = this.buildingToMove.humanDoor.Value;
			startPoint.X += (int)buildingPosition.X;
			startPoint.Y += (int)buildingPosition.Y;
			startPoint.Y++;
			HashSet<Point> closedTiles = new HashSet<Point>();
			Stack<Point> openTiles = new Stack<Point>();
			openTiles.Push(startPoint);
			closedTiles.Add(startPoint);
			HashSet<Point> validWarpTiles = new HashSet<Point>();
			foreach (Warp w in this.TargetLocation.warps)
			{
				if (!(w.TargetName == "FarmCave"))
				{
					validWarpTiles.Add(new Point(w.X, w.Y));
				}
			}
			bool success = false;
			while (openTiles.Count > 0)
			{
				Point tile = openTiles.Pop();
				if (validWarpTiles.Contains(tile))
				{
					success = true;
					break;
				}
				if (this.TargetLocation.isTileOnMap(tile.X, tile.Y) && this.VerifyTileAccessibility(tile.X, tile.Y, buildingPosition))
				{
					Point newPoint = tile;
					newPoint.X++;
					if (closedTiles.Add(newPoint))
					{
						openTiles.Push(newPoint);
					}
					newPoint = tile;
					newPoint.X--;
					if (closedTiles.Add(newPoint))
					{
						openTiles.Push(newPoint);
					}
					newPoint = tile;
					newPoint.Y--;
					if (closedTiles.Add(newPoint))
					{
						openTiles.Push(newPoint);
					}
					newPoint = tile;
					newPoint.Y++;
					if (closedTiles.Add(newPoint))
					{
						openTiles.Push(newPoint);
					}
				}
			}
			return success;
		}

		// Token: 0x060027A6 RID: 10150 RVA: 0x001C5394 File Offset: 0x001C3594
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (this.freeze)
			{
				return;
			}
			if (!this.onFarm)
			{
				base.receiveLeftClick(x, y, playSound);
			}
			if (this.cancelButton.containsPoint(x, y))
			{
				if (!this.onFarm)
				{
					base.exitThisMenu(true);
					Game1.player.forceCanMove();
					Game1.playSound("bigDeSelect", null);
				}
				else
				{
					if (this.Action == CarpenterMenu.CarpentryAction.Move && this.buildingToMove != null)
					{
						Game1.playSound("cancel", null);
						return;
					}
					this.returnToCarpentryMenu();
					Game1.playSound("smallSelect", null);
					return;
				}
			}
			if (!this.onFarm && this.backButton.containsPoint(x, y))
			{
				this.SetNewActiveBlueprint(this.Blueprint.Index - 1);
				Game1.playSound("shwip", null);
				this.backButton.scale = this.backButton.baseScale;
			}
			if (!this.onFarm && this.forwardButton.containsPoint(x, y))
			{
				this.SetNewActiveBlueprint(this.Blueprint.Index + 1);
				this.forwardButton.scale = this.forwardButton.baseScale;
				Game1.playSound("shwip", null);
			}
			if (!this.onFarm)
			{
				if (this.demolishButton.containsPoint(x, y) && this.demolishButton.visible && this.CanDemolishThis())
				{
					Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.setUpForBuildingPlacement), 0.02f);
					Game1.playSound("smallSelect", null);
					this.onFarm = true;
					this.Action = CarpenterMenu.CarpentryAction.Demolish;
				}
				else if (this.moveButton.containsPoint(x, y) && this.moveButton.visible)
				{
					Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.setUpForBuildingPlacement), 0.02f);
					Game1.playSound("smallSelect", null);
					this.onFarm = true;
					this.Action = CarpenterMenu.CarpentryAction.Move;
				}
				else if (this.paintButton.containsPoint(x, y) && this.paintButton.visible)
				{
					Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.setUpForBuildingPlacement), 0.02f);
					Game1.playSound("smallSelect", null);
					this.onFarm = true;
					this.Action = CarpenterMenu.CarpentryAction.Paint;
				}
				else if (this.appearanceButton.containsPoint(x, y) && this.appearanceButton.visible)
				{
					if (this.currentBuilding.CanBeReskinned(true))
					{
						BuildingSkinMenu skinMenu = new BuildingSkinMenu(this.currentBuilding, true);
						Game1.playSound("smallSelect", null);
						BuildingSkinMenu skinMenu2 = skinMenu;
						skinMenu2.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(skinMenu2.behaviorBeforeCleanup, new Action<IClickableMenu>(delegate(IClickableMenu menu)
						{
							if (Game1.options.SnappyMenus)
							{
								this.setCurrentlySnappedComponentTo(109);
								this.snapCursorToCurrentSnappedComponent();
							}
							CarpenterMenu.BlueprintEntry blueprint = this.Blueprint;
							BuildingSkinMenu.SkinEntry skin = skinMenu.Skin;
							blueprint.SetSkin((skin != null) ? skin.Id : null);
						}));
						base.SetChildMenu(skinMenu);
					}
				}
				else if (this.okButton.containsPoint(x, y) && !this.onFarm && this.CanBuildCurrentBlueprint())
				{
					Game1.globalFadeToBlack(new Game1.afterFadeFunction(this.setUpForBuildingPlacement), 0.02f);
					Game1.playSound("smallSelect", null);
					this.onFarm = true;
				}
			}
			if (this.onFarm && !this.freeze && !Game1.IsFading())
			{
				switch (this.Action)
				{
				case CarpenterMenu.CarpentryAction.Demolish:
				{
					CarpenterMenu.<>c__DisplayClass58_1 CS$<>8__locals2 = new CarpenterMenu.<>c__DisplayClass58_1();
					CS$<>8__locals2.<>4__this = this;
					CS$<>8__locals2.farm = this.TargetLocation;
					CS$<>8__locals2.destroyed = CS$<>8__locals2.farm.getBuildingAt(new Vector2((float)((Game1.viewport.X + Game1.getOldMouseX(false)) / 64), (float)((Game1.viewport.Y + Game1.getOldMouseY(false)) / 64)));
					if (CS$<>8__locals2.destroyed == null)
					{
						return;
					}
					CS$<>8__locals2.interior = CS$<>8__locals2.destroyed.GetIndoors();
					CS$<>8__locals2.cabin = (CS$<>8__locals2.interior as Cabin);
					if (CS$<>8__locals2.destroyed != null)
					{
						if (CS$<>8__locals2.cabin != null && !Game1.IsMasterGame)
						{
							Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_LockFailed"), 3));
							CS$<>8__locals2.destroyed = null;
							return;
						}
						if (!this.CanDemolishThis(CS$<>8__locals2.destroyed))
						{
							CS$<>8__locals2.destroyed = null;
							return;
						}
						if (!Game1.IsMasterGame && !this.hasPermissionsToDemolish(CS$<>8__locals2.destroyed))
						{
							CS$<>8__locals2.destroyed = null;
							return;
						}
					}
					Cabin cabin = CS$<>8__locals2.cabin;
					if (cabin != null && cabin.HasOwner && CS$<>8__locals2.cabin.owner.isCustomized.Value)
					{
						Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\UI:Carpenter_DemolishCabinConfirm", CS$<>8__locals2.cabin.owner.Name), Game1.currentLocation.createYesNoResponses(), delegate(Farmer f, string answer)
						{
							if (answer == "Yes")
							{
								Game1.activeClickableMenu = CS$<>8__locals2.<>4__this;
								Game1.player.team.demolishLock.RequestLock(new Action(base.<receiveLeftClick>g__ContinueDemolish|3), new Action(CS$<>8__locals2.<>4__this.<receiveLeftClick>g__BuildingLockFailed|58_2));
								return;
							}
							DelayedAction.functionAfterDelay(new Action(CS$<>8__locals2.<>4__this.returnToCarpentryMenu), 500);
						}, null);
						return;
					}
					if (CS$<>8__locals2.destroyed != null)
					{
						Game1.player.team.demolishLock.RequestLock(new Action(CS$<>8__locals2.<receiveLeftClick>g__ContinueDemolish|3), new Action(this.<receiveLeftClick>g__BuildingLockFailed|58_2));
						return;
					}
					break;
				}
				case CarpenterMenu.CarpentryAction.Move:
					if (this.buildingToMove == null)
					{
						this.buildingToMove = this.TargetLocation.getBuildingAt(new Vector2((float)((Game1.viewport.X + Game1.getMouseX(false)) / 64), (float)((Game1.viewport.Y + Game1.getMouseY(false)) / 64)));
						if (this.buildingToMove != null)
						{
							if (this.buildingToMove.daysOfConstructionLeft.Value > 0)
							{
								this.buildingToMove = null;
								return;
							}
							if (!this.hasPermissionsToMove(this.buildingToMove))
							{
								this.buildingToMove = null;
								return;
							}
							this.buildingToMove.isMoving = true;
							Game1.playSound("axchop", null);
							return;
						}
					}
					else
					{
						Vector2 buildingPosition = new Vector2((float)((Game1.viewport.X + Game1.getMouseX(false)) / 64), (float)((Game1.viewport.Y + Game1.getMouseY(false)) / 64));
						if (!this.ConfirmBuildingAccessibility(buildingPosition))
						{
							Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantBuild"), 3));
							Game1.playSound("cancel", null);
							return;
						}
						if (this.TargetLocation.buildStructure(this.buildingToMove, buildingPosition, Game1.player, false))
						{
							this.buildingToMove.isMoving = false;
							this.buildingToMove = null;
							Game1.playSound("axchop", null);
							DelayedAction.playSoundAfterDelay("dirtyHit", 50, null, null, -1, false);
							DelayedAction.playSoundAfterDelay("dirtyHit", 150, null, null, -1, false);
							return;
						}
						Game1.playSound("cancel", null);
						return;
					}
					break;
				case CarpenterMenu.CarpentryAction.Paint:
				{
					Vector2 tile_position = new Vector2((float)((Game1.viewport.X + Game1.getMouseX(false)) / 64), (float)((Game1.viewport.Y + Game1.getMouseY(false)) / 64));
					Building paint_building = this.TargetLocation.getBuildingAt(tile_position);
					if (paint_building != null)
					{
						if (!paint_building.CanBePainted() && !paint_building.CanBeReskinned(true))
						{
							Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CannotPaint"), 3));
							return;
						}
						if (!this.HasPermissionsToPaint(paint_building))
						{
							Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CannotPaint_Permission"), 3));
							return;
						}
						paint_building.color = Color.White;
						base.SetChildMenu(paint_building.CanBePainted() ? new BuildingPaintMenu(paint_building) : new BuildingSkinMenu(paint_building, true));
						return;
					}
					break;
				}
				case CarpenterMenu.CarpentryAction.Upgrade:
				{
					Building toUpgrade = this.TargetLocation.getBuildingAt(new Vector2((float)((Game1.viewport.X + Game1.getOldMouseX(false)) / 64), (float)((Game1.viewport.Y + Game1.getOldMouseY(false)) / 64)));
					if (toUpgrade != null && toUpgrade.buildingType.Value == this.Blueprint.UpgradeFrom)
					{
						this.ConsumeResources();
						toUpgrade.upgradeName.Value = this.Blueprint.Id;
						toUpgrade.daysUntilUpgrade.Value = Math.Max(this.Blueprint.BuildDays, 1);
						toUpgrade.showUpgradeAnimation(this.TargetLocation);
						Game1.playSound("axe", null);
						DelayedAction.functionAfterDelay(new Action(this.returnToCarpentryMenuAfterSuccessfulBuild), 1500);
						this.freeze = true;
						Game1.multiplayer.globalChatInfoMessage("BuildingBuild", new string[]
						{
							Game1.player.Name,
							"aOrAn:" + this.Blueprint.TokenizedDisplayName,
							this.Blueprint.TokenizedDisplayName,
							Game1.player.farmName.Value
						});
						if (this.Blueprint.BuildDays < 1)
						{
							toUpgrade.FinishConstruction(false);
							return;
						}
						Game1.netWorldState.Value.MarkUnderConstruction(this.Builder, toUpgrade);
						return;
					}
					else if (toUpgrade != null)
					{
						Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantUpgrade_BuildingType"), 3));
						return;
					}
					break;
				}
				default:
					Game1.player.team.buildLock.RequestLock(delegate
					{
						if (this.onFarm && Game1.locationRequest == null)
						{
							if (this.tryToBuild())
							{
								this.ConsumeResources();
								DelayedAction.functionAfterDelay(new Action(this.returnToCarpentryMenuAfterSuccessfulBuild), 2000);
								this.freeze = true;
							}
							else
							{
								Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantBuild"), 3));
							}
						}
						Game1.player.team.buildLock.ReleaseLock();
					}, null);
					break;
				}
			}
		}

		// Token: 0x060027A7 RID: 10151 RVA: 0x001C5CD4 File Offset: 0x001C3ED4
		public bool tryToBuild()
		{
			NetString skinId = this.currentBuilding.skinId;
			Vector2 tileLocation = new Vector2((float)((Game1.viewport.X + Game1.getOldMouseX(false)) / 64), (float)((Game1.viewport.Y + Game1.getOldMouseY(false)) / 64));
			Building building;
			if (this.TargetLocation.buildStructure(this.currentBuilding.buildingType.Value, tileLocation, Game1.player, out building, this.Blueprint.MagicalConstruction, false))
			{
				building.skinId.Value = skinId.Value;
				if (building.isUnderConstruction(true))
				{
					Game1.netWorldState.Value.MarkUnderConstruction(this.Builder, building);
				}
				return true;
			}
			return false;
		}

		// Token: 0x060027A8 RID: 10152 RVA: 0x001C5D84 File Offset: 0x001C3F84
		public virtual void returnToCarpentryMenu()
		{
			LocationRequest locationRequest = Game1.getLocationRequest(this.BuilderLocationName, false);
			locationRequest.OnWarp += delegate()
			{
				this.onFarm = false;
				Game1.player.viewingLocation.Value = null;
				this.resetBounds();
				this.Action = CarpenterMenu.CarpentryAction.None;
				this.buildingToMove = null;
				this.freeze = false;
				Game1.displayHUD = true;
				Game1.viewportFreeze = false;
				Game1.viewport.Location = this.BuilderViewport;
				this.drawBG = true;
				Game1.displayFarmer = true;
				if (Game1.options.SnappyMenus)
				{
					this.populateClickableComponentList();
					this.snapToDefaultClickableComponent();
				}
			};
			Game1.warpFarmer(locationRequest, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, Game1.player.FacingDirection);
		}

		// Token: 0x060027A9 RID: 10153 RVA: 0x001C5DDC File Offset: 0x001C3FDC
		public void returnToCarpentryMenuAfterSuccessfulBuild()
		{
			LocationRequest locationRequest = Game1.getLocationRequest(this.BuilderLocationName, false);
			locationRequest.OnWarp += delegate()
			{
				Game1.displayHUD = true;
				Game1.player.viewingLocation.Value = null;
				Game1.viewportFreeze = false;
				Game1.viewport.Location = this.BuilderViewport;
				this.freeze = true;
				Game1.displayFarmer = true;
				this.robinConstructionMessage();
			};
			Game1.warpFarmer(locationRequest, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, Game1.player.FacingDirection);
		}

		// Token: 0x060027AA RID: 10154 RVA: 0x001C5E34 File Offset: 0x001C4034
		public void robinConstructionMessage()
		{
			base.exitThisMenu(true);
			Game1.player.forceCanMove();
			if (!this.Blueprint.MagicalConstruction)
			{
				string dialoguePath = "Data\\ExtraDialogue:Robin_" + ((this.Action == CarpenterMenu.CarpentryAction.Upgrade) ? "Upgrade" : "New") + "Construction";
				if (Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.season))
				{
					dialoguePath += "_Festival";
				}
				string displayName = this.Blueprint.DisplayName;
				string generalName = this.Blueprint.DisplayNameForGeneralType;
				if (this.Blueprint.BuildDays <= 0)
				{
					Game1.DrawDialogue(Game1.getCharacterFromName("Robin", true, false), "Data\\ExtraDialogue:Robin_Instant", new object[]
					{
						displayName.ToLower(),
						displayName
					});
					return;
				}
				Game1.DrawDialogue(Game1.getCharacterFromName("Robin", true, false), dialoguePath, new object[]
				{
					displayName.ToLower(),
					generalName.ToLower(),
					displayName,
					generalName
				});
			}
		}

		// Token: 0x060027AB RID: 10155 RVA: 0x001C5F28 File Offset: 0x001C4128
		public override bool overrideSnappyMenuCursorMovementBan()
		{
			return this.onFarm;
		}

		// Token: 0x060027AC RID: 10156 RVA: 0x001C5F30 File Offset: 0x001C4130
		public void setUpForBuildingPlacement()
		{
			Game1.currentLocation.cleanupBeforePlayerExit();
			this.hoverText = "";
			Game1.currentLocation = this.TargetLocation;
			Game1.player.viewingLocation.Value = this.TargetLocation.NameOrUniqueName;
			Game1.currentLocation.resetForPlayerEntry();
			Game1.globalFadeToClear(null, 0.02f);
			this.onFarm = true;
			this.cancelButton.bounds.X = Game1.uiViewport.Width - 128;
			this.cancelButton.bounds.Y = Game1.uiViewport.Height - 128;
			Game1.displayHUD = false;
			Game1.viewportFreeze = true;
			Game1.viewport.Location = this.GetInitialBuildingPlacementViewport(this.TargetLocation);
			Game1.clampViewportToGameMap();
			Game1.panScreen(0, 0);
			this.drawBG = false;
			this.freeze = false;
			Game1.displayFarmer = false;
			if (this.Blueprint.IsUpgrade && this.Action == CarpenterMenu.CarpentryAction.None)
			{
				this.Action = CarpenterMenu.CarpentryAction.Upgrade;
			}
		}

		// Token: 0x060027AD RID: 10157 RVA: 0x001C6034 File Offset: 0x001C4234
		public Location GetInitialBuildingPlacementViewport(GameLocation location)
		{
			if (this.TargetViewportCenterOnTile != null)
			{
				Vector2 tile = this.TargetViewportCenterOnTile.Value;
				return CarpenterMenu.<GetInitialBuildingPlacementViewport>g__CenterOnTile|65_0((int)tile.X, (int)tile.Y);
			}
			Building building = location.getBuildingByName("FarmHouse") ?? location.buildings.FirstOrDefault<Building>();
			if (building != null)
			{
				return CarpenterMenu.<GetInitialBuildingPlacementViewport>g__CenterOnTile|65_0(building.tileX.Value + building.tilesWide.Value / 2, building.tileY.Value + building.tilesHigh.Value / 2);
			}
			Layer layer = location.Map.Layers[0];
			return CarpenterMenu.<GetInitialBuildingPlacementViewport>g__CenterOnTile|65_0(layer.LayerWidth / 2, layer.LayerHeight / 2);
		}

		// Token: 0x060027AE RID: 10158 RVA: 0x001C60EB File Offset: 0x001C42EB
		public override void gameWindowSizeChanged(Microsoft.Xna.Framework.Rectangle oldBounds, Microsoft.Xna.Framework.Rectangle newBounds)
		{
			this.resetBounds();
		}

		// Token: 0x060027AF RID: 10159 RVA: 0x001C60F3 File Offset: 0x001C42F3
		public virtual bool IsValidBuildingForLocation(string typeId, BuildingData data, GameLocation targetLocation)
		{
			return !(typeId == "Cabin") || !(this.TargetLocation.Name != "Farm");
		}

		// Token: 0x060027B0 RID: 10160 RVA: 0x001C611C File Offset: 0x001C431C
		public virtual bool CanBuildCurrentBlueprint()
		{
			CarpenterMenu.BlueprintEntry blueprint = this.Blueprint;
			return this.IsValidBuildingForLocation(blueprint.Id, blueprint.Data, this.TargetLocation) && this.DoesFarmerHaveEnoughResourcesToBuild() && (blueprint.BuildCost <= 0 || Game1.player.Money >= blueprint.BuildCost);
		}

		// Token: 0x060027B1 RID: 10161 RVA: 0x001C6174 File Offset: 0x001C4374
		public bool CanDemolishThis()
		{
			return this.CanDemolishThis(this.currentBuilding);
		}

		// Token: 0x060027B2 RID: 10162 RVA: 0x001C6184 File Offset: 0x001C4384
		public virtual bool CanDemolishThis(Building building)
		{
			string type = (building != null) ? building.buildingType.Value : null;
			if (!(type == "Farmhouse"))
			{
				if (!(type == "Greenhouse"))
				{
					if (type == "Pet Bowl" || type == "Shipping Bin")
					{
						if (this.TargetLocation == Game1.getFarm() && !this.TargetLocation.HasMinBuildings(type, 2))
						{
							return false;
						}
					}
				}
				else if (building.HasIndoorsName("Greenhouse"))
				{
					return false;
				}
			}
			else if (building.HasIndoorsName("FarmHouse"))
			{
				return false;
			}
			return building != null;
		}

		// Token: 0x060027B3 RID: 10163 RVA: 0x001C621C File Offset: 0x001C441C
		public override void draw(SpriteBatch b)
		{
			CarpenterMenu.BlueprintEntry blueprint = this.Blueprint;
			if (this.drawBG && !Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f);
			}
			if (Game1.IsFading() || this.freeze)
			{
				return;
			}
			if (!this.onFarm)
			{
				base.draw(b);
				Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen - 96, this.yPositionOnScreen - 16, this.maxWidthOfBuildingViewer + 64, this.maxHeightOfBuildingViewer + 64);
				IClickableMenu.drawTextureBox(b, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, blueprint.MagicalConstruction ? Color.RoyalBlue : Color.White);
				rectangle.Inflate(-12, -12);
				b.End();
				b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled, null, null);
				b.GraphicsDevice.ScissorRectangle = rectangle;
				Microsoft.Xna.Framework.Rectangle sourceRect = this.currentBuilding.getSourceRectForMenu() ?? this.currentBuilding.getSourceRect();
				Point offset = blueprint.Data.BuildMenuDrawOffset;
				this.currentBuilding.drawInMenu(b, this.xPositionOnScreen + this.maxWidthOfBuildingViewer / 2 - this.currentBuilding.tilesWide.Value * 64 / 2 - 64 + offset.X, this.yPositionOnScreen + this.maxHeightOfBuildingViewer / 2 - sourceRect.Height * 4 / 2 + offset.Y);
				b.End();
				b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
				if (blueprint.IsUpgrade)
				{
					this.upgradeIcon.draw(b);
				}
				string placeholder = " Deluxe  Barn   ";
				if (SpriteText.getWidthOfString(blueprint.DisplayName, 999999) >= SpriteText.getWidthOfString(placeholder, 999999))
				{
					placeholder = blueprint.DisplayName + " ";
				}
				SpriteText.drawStringWithScrollCenteredAt(b, blueprint.DisplayName, this.xPositionOnScreen + this.maxWidthOfBuildingViewer - IClickableMenu.spaceToClearSideBorder - 16 + 64 + (this.width - (this.maxWidthOfBuildingViewer + 128)) / 2, this.yPositionOnScreen, SpriteText.getWidthOfString(placeholder, 999999), 1f, null, 0, 0.88f, false);
				int descriptionWidth;
				switch (LocalizedContentManager.CurrentLanguageCode)
				{
				case LocalizedContentManager.LanguageCode.es:
					descriptionWidth = this.maxWidthOfDescription + 64 + ((blueprint.Id == "Deluxe Barn") ? 96 : 0);
					goto IL_392;
				case LocalizedContentManager.LanguageCode.fr:
					descriptionWidth = this.maxWidthOfDescription + 96 + ((blueprint.Id == "Slime Hutch" || blueprint.Id == "Deluxe Coop" || blueprint.Id == "Deluxe Barn") ? 72 : 0);
					goto IL_392;
				case LocalizedContentManager.LanguageCode.ko:
					descriptionWidth = this.maxWidthOfDescription + 96 + ((blueprint.Id == "Slime Hutch") ? 64 : ((blueprint.Id == "Deluxe Coop") ? 96 : ((blueprint.Id == "Deluxe Barn") ? 112 : ((blueprint.Id == "Big Barn") ? 64 : 0))));
					goto IL_392;
				case LocalizedContentManager.LanguageCode.it:
					descriptionWidth = this.maxWidthOfDescription + 96;
					goto IL_392;
				}
				descriptionWidth = this.maxWidthOfDescription + 64;
				IL_392:
				IClickableMenu.drawTextureBox(b, this.xPositionOnScreen + this.maxWidthOfBuildingViewer - 16, this.yPositionOnScreen + 80, descriptionWidth, this.maxHeightOfBuildingViewer - 32, blueprint.MagicalConstruction ? Color.RoyalBlue : Color.White);
				if (blueprint.MagicalConstruction)
				{
					Utility.drawTextWithShadow(b, Game1.parseText(blueprint.Description, Game1.dialogueFont, descriptionWidth - 32), Game1.dialogueFont, new Vector2((float)(this.xPositionOnScreen + this.maxWidthOfBuildingViewer - 4), (float)(this.yPositionOnScreen + 80 + 16 + 4)), Game1.textColor * 0.25f, 1f, -1f, -1, -1, 0f, 3);
					Utility.drawTextWithShadow(b, Game1.parseText(blueprint.Description, Game1.dialogueFont, descriptionWidth - 32), Game1.dialogueFont, new Vector2((float)(this.xPositionOnScreen + this.maxWidthOfBuildingViewer - 1), (float)(this.yPositionOnScreen + 80 + 16 + 4)), Game1.textColor * 0.25f, 1f, -1f, -1, -1, 0f, 3);
				}
				Utility.drawTextWithShadow(b, Game1.parseText(blueprint.Description, Game1.dialogueFont, descriptionWidth - 32), Game1.dialogueFont, new Vector2((float)(this.xPositionOnScreen + this.maxWidthOfBuildingViewer), (float)(this.yPositionOnScreen + 80 + 16)), blueprint.MagicalConstruction ? Color.PaleGoldenrod : Game1.textColor, 1f, -1f, -1, -1, blueprint.MagicalConstruction ? 0f : 0.75f, 3);
				Vector2 ingredientsPosition = new Vector2((float)(this.xPositionOnScreen + this.maxWidthOfBuildingViewer + 16), (float)(this.yPositionOnScreen + 256 + 32));
				if (this.ingredients.Count < 3 && (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt))
				{
					ingredientsPosition.Y += 64f;
				}
				if (blueprint.BuildCost >= 0)
				{
					b.Draw(Game1.mouseCursors_1_6, ingredientsPosition + new Vector2(-8f, -4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(241, 303, 14, 13)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.89f);
					string price_string = Utility.getNumberWithCommas(blueprint.BuildCost);
					if (blueprint.MagicalConstruction)
					{
						Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", price_string), Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f, ingredientsPosition.Y + 8f), Game1.textColor * 0.5f, 1f, -1f, -1, -1, 0f, 3);
						Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", price_string), Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f + 4f - 1f, ingredientsPosition.Y + 8f), Game1.textColor * 0.25f, 1f, -1f, -1, -1, 0f, 3);
					}
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", price_string), Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f + 4f, ingredientsPosition.Y + 4f), (Game1.player.Money >= blueprint.BuildCost) ? (blueprint.MagicalConstruction ? Color.PaleGoldenrod : Game1.textColor) : Color.Red, 1f, -1f, -1, -1, blueprint.MagicalConstruction ? 0f : 0.25f, 3);
				}
				if (!blueprint.MagicalConstruction)
				{
					int daysToBuild = blueprint.BuildDays;
					string timeString = (daysToBuild > 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11374", daysToBuild) : ((daysToBuild == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11375", daysToBuild) : Game1.content.LoadString("Strings\\1_6_Strings:Instant"));
					rectangle = new Microsoft.Xna.Framework.Rectangle(this.xPositionOnScreen - 96 + this.width + 64, this.yPositionOnScreen + 80, 72 + (int)Game1.smallFont.MeasureString(timeString).X, 68);
					IClickableMenu.drawTextureBox(b, rectangle.X - 8, rectangle.Y, rectangle.Width + 16, rectangle.Height, Color.White);
					b.Draw(Game1.mouseCursors, new Vector2((float)(rectangle.X + 8), (float)(rectangle.Y + 16)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(410, 501, 9, 9)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.89f);
					Utility.drawTextWithShadow(b, timeString, Game1.smallFont, new Vector2((float)(rectangle.X + 8 + 44), (float)(rectangle.Y + 20)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
				}
				ingredientsPosition.X -= 16f;
				ingredientsPosition.Y -= 21f;
				foreach (Item i in this.ingredients)
				{
					ingredientsPosition.Y += 68f;
					i.drawInMenu(b, ingredientsPosition, 1f);
					bool hasItem = Game1.player.Items.ContainsId(i.QualifiedItemId, i.Stack);
					if (blueprint.MagicalConstruction)
					{
						Utility.drawTextWithShadow(b, i.DisplayName, Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f + 12f, ingredientsPosition.Y + 24f), Game1.textColor * 0.25f, 1f, -1f, -1, -1, 0f, 3);
						Utility.drawTextWithShadow(b, i.DisplayName, Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f + 16f - 1f, ingredientsPosition.Y + 24f), Game1.textColor * 0.25f, 1f, -1f, -1, -1, 0f, 3);
					}
					Utility.drawTextWithShadow(b, i.DisplayName, Game1.dialogueFont, new Vector2(ingredientsPosition.X + 64f + 16f, ingredientsPosition.Y + 20f), hasItem ? (blueprint.MagicalConstruction ? Color.PaleGoldenrod : Game1.textColor) : Color.Red, 1f, -1f, -1, -1, blueprint.MagicalConstruction ? 0f : 0.25f, 3);
				}
				this.backButton.draw(b);
				this.forwardButton.draw(b);
				this.okButton.draw(b, this.CanBuildCurrentBlueprint() ? Color.White : (Color.Gray * 0.8f), 0.88f, 0, 0, 0);
				this.demolishButton.draw(b, this.CanDemolishThis() ? Color.White : (Color.Gray * 0.8f), 0.88f, 0, 0, 0);
				this.moveButton.draw(b);
				this.paintButton.draw(b);
				this.appearanceButton.draw(b);
			}
			else
			{
				string message;
				switch (this.Action)
				{
				case CarpenterMenu.CarpentryAction.Demolish:
					message = Game1.content.LoadString("Strings\\UI:Carpenter_SelectBuilding_Demolish");
					break;
				case CarpenterMenu.CarpentryAction.Move:
					message = Game1.content.LoadString("Strings\\UI:Carpenter_SelectBuilding_Move");
					break;
				case CarpenterMenu.CarpentryAction.Paint:
					message = Game1.content.LoadString("Strings\\UI:Carpenter_SelectBuilding_Paint");
					break;
				case CarpenterMenu.CarpentryAction.Upgrade:
					message = Game1.content.LoadString("Strings\\UI:Carpenter_SelectBuilding_Upgrade", blueprint.GetDisplayNameForBuildingToUpgrade());
					break;
				default:
					message = Game1.content.LoadString("Strings\\UI:Carpenter_ChooseLocation");
					break;
				}
				SpriteText.drawStringWithScrollBackground(b, message, Game1.uiViewport.Width / 2 - SpriteText.getWidthOfString(message, 999999) / 2, 16, "", 1f, null, SpriteText.ScrollTextAlignment.Left);
				Game1.StartWorldDrawInUI(b);
				CarpenterMenu.CarpentryAction action = this.Action;
				if (action != CarpenterMenu.CarpentryAction.None)
				{
					if (action != CarpenterMenu.CarpentryAction.Move)
					{
						goto IL_106A;
					}
				}
				else
				{
					Vector2 mousePositionTile = new Vector2((float)((Game1.viewport.X + Game1.getOldMouseX(false)) / 64), (float)((Game1.viewport.Y + Game1.getOldMouseY(false)) / 64));
					for (int y = 0; y < this.currentBuilding.tilesHigh.Value; y++)
					{
						for (int x = 0; x < this.currentBuilding.tilesWide.Value; x++)
						{
							int sheetIndex = this.currentBuilding.getTileSheetIndexForStructurePlacementTile(x, y);
							Vector2 currentGlobalTilePosition = new Vector2(mousePositionTile.X + (float)x, mousePositionTile.Y + (float)y);
							if (!Game1.currentLocation.isBuildable(currentGlobalTilePosition, false))
							{
								sheetIndex++;
							}
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, currentGlobalTilePosition * 64f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(194 + sheetIndex * 16, 388, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
						}
					}
					using (IEnumerator<BuildingPlacementTile> enumerator2 = this.currentBuilding.GetAdditionalPlacementTiles().GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							BuildingPlacementTile buildingPlacementTile = enumerator2.Current;
							bool onlyNeedsToBePassable = buildingPlacementTile.OnlyNeedsToBePassable;
							foreach (Point point in buildingPlacementTile.TileArea.GetPoints())
							{
								int x2 = point.X;
								int y2 = point.Y;
								int sheetIndex2 = this.currentBuilding.getTileSheetIndexForStructurePlacementTile(x2, y2);
								Vector2 currentGlobalTilePosition2 = new Vector2(mousePositionTile.X + (float)x2, mousePositionTile.Y + (float)y2);
								if (!Game1.currentLocation.isBuildable(currentGlobalTilePosition2, onlyNeedsToBePassable))
								{
									sheetIndex2++;
								}
								b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, currentGlobalTilePosition2 * 64f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(194 + sheetIndex2 * 16, 388, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
							}
						}
						goto IL_106A;
					}
				}
				if (this.buildingToMove != null)
				{
					Vector2 mousePositionTile2 = new Vector2((float)((Game1.viewport.X + Game1.getOldMouseX(false)) / 64), (float)((Game1.viewport.Y + Game1.getOldMouseY(false)) / 64));
					for (int y3 = 0; y3 < this.buildingToMove.tilesHigh.Value; y3++)
					{
						for (int x3 = 0; x3 < this.buildingToMove.tilesWide.Value; x3++)
						{
							int sheetIndex3 = this.buildingToMove.getTileSheetIndexForStructurePlacementTile(x3, y3);
							Vector2 currentGlobalTilePosition3 = new Vector2(mousePositionTile2.X + (float)x3, mousePositionTile2.Y + (float)y3);
							if (!Game1.currentLocation.isBuildable(currentGlobalTilePosition3, false))
							{
								sheetIndex3++;
							}
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, currentGlobalTilePosition3 * 64f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(194 + sheetIndex3 * 16, 388, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
						}
					}
					foreach (BuildingPlacementTile buildingPlacementTile2 in this.buildingToMove.GetAdditionalPlacementTiles())
					{
						bool onlyNeedsToBePassable2 = buildingPlacementTile2.OnlyNeedsToBePassable;
						foreach (Point point2 in buildingPlacementTile2.TileArea.GetPoints())
						{
							int x4 = point2.X;
							int y4 = point2.Y;
							int sheetIndex4 = this.buildingToMove.getTileSheetIndexForStructurePlacementTile(x4, y4);
							Vector2 currentGlobalTilePosition4 = new Vector2(mousePositionTile2.X + (float)x4, mousePositionTile2.Y + (float)y4);
							if (!Game1.currentLocation.isBuildable(currentGlobalTilePosition4, onlyNeedsToBePassable2))
							{
								sheetIndex4++;
							}
							b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, currentGlobalTilePosition4 * 64f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(194 + sheetIndex4 * 16, 388, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
						}
					}
				}
				IL_106A:
				Game1.EndWorldDrawInUI(b);
			}
			this.cancelButton.draw(b);
			if (base.GetChildMenu() == null)
			{
				base.drawMouse(b, false, -1);
				if (this.hoverText.Length > 0)
				{
					IClickableMenu.drawHoverText(b, this.hoverText, Game1.dialogueFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
				}
			}
		}

		// Token: 0x060027B4 RID: 10164 RVA: 0x001C738C File Offset: 0x001C558C
		public void ConsumeResources()
		{
			CarpenterMenu.BlueprintEntry blueprint = this.Blueprint;
			foreach (Item ingredient in this.ingredients)
			{
				Game1.player.Items.ReduceId(ingredient.QualifiedItemId, ingredient.Stack);
			}
			Game1.player.Money -= blueprint.BuildCost;
		}

		// Token: 0x060027B5 RID: 10165 RVA: 0x001C7414 File Offset: 0x001C5614
		public bool DoesFarmerHaveEnoughResourcesToBuild()
		{
			CarpenterMenu.BlueprintEntry blueprint = this.Blueprint;
			if (blueprint.BuildCost < 0)
			{
				return false;
			}
			foreach (Item item in this.ingredients)
			{
				if (!Game1.player.Items.ContainsId(item.QualifiedItemId, item.Stack))
				{
					return false;
				}
			}
			return Game1.player.Money >= blueprint.BuildCost;
		}

		// Token: 0x060027B6 RID: 10166 RVA: 0x001C74AC File Offset: 0x001C56AC
		[CompilerGenerated]
		private void <receiveLeftClick>g__BuildingLockFailed|58_2()
		{
			if (this.Action != CarpenterMenu.CarpentryAction.Demolish)
			{
				return;
			}
			Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_LockFailed"), 3));
		}

		// Token: 0x060027BA RID: 10170 RVA: 0x001C7618 File Offset: 0x001C5818
		[CompilerGenerated]
		internal static Location <GetInitialBuildingPlacementViewport>g__CenterOnTile|65_0(int x, int y)
		{
			x = (int)((float)(x * 64) - (float)Game1.viewport.Width / 2f);
			y = (int)((float)(y * 64) - (float)Game1.viewport.Height / 2f);
			return new Location(x, y);
		}

		// Token: 0x040018E4 RID: 6372
		public const int region_backButton = 101;

		// Token: 0x040018E5 RID: 6373
		public const int region_forwardButton = 102;

		// Token: 0x040018E6 RID: 6374
		public const int region_upgradeIcon = 103;

		// Token: 0x040018E7 RID: 6375
		public const int region_demolishButton = 104;

		// Token: 0x040018E8 RID: 6376
		public const int region_moveBuitton = 105;

		// Token: 0x040018E9 RID: 6377
		public const int region_okButton = 106;

		// Token: 0x040018EA RID: 6378
		public const int region_cancelButton = 107;

		// Token: 0x040018EB RID: 6379
		public const int region_paintButton = 108;

		// Token: 0x040018EC RID: 6380
		public const int region_appearanceButton = 109;

		// Token: 0x040018ED RID: 6381
		private bool _readOnly;

		// Token: 0x040018EE RID: 6382
		public int maxWidthOfBuildingViewer = 448;

		// Token: 0x040018EF RID: 6383
		public int maxHeightOfBuildingViewer = 512;

		// Token: 0x040018F0 RID: 6384
		public int maxWidthOfDescription = 416;

		// Token: 0x040018F1 RID: 6385
		public readonly string Builder;

		// Token: 0x040018F2 RID: 6386
		public readonly string BuilderLocationName;

		// Token: 0x040018F3 RID: 6387
		public readonly Location BuilderViewport;

		// Token: 0x040018F4 RID: 6388
		public GameLocation TargetLocation;

		// Token: 0x040018F5 RID: 6389
		public Vector2? TargetViewportCenterOnTile;

		// Token: 0x040018F6 RID: 6390
		public readonly List<CarpenterMenu.BlueprintEntry> Blueprints = new List<CarpenterMenu.BlueprintEntry>();

		// Token: 0x040018F7 RID: 6391
		public CarpenterMenu.BlueprintEntry Blueprint;

		// Token: 0x040018F8 RID: 6392
		public ClickableTextureComponent okButton;

		// Token: 0x040018F9 RID: 6393
		public ClickableTextureComponent cancelButton;

		// Token: 0x040018FA RID: 6394
		public ClickableTextureComponent backButton;

		// Token: 0x040018FB RID: 6395
		public ClickableTextureComponent forwardButton;

		// Token: 0x040018FC RID: 6396
		public ClickableTextureComponent upgradeIcon;

		// Token: 0x040018FD RID: 6397
		public ClickableTextureComponent demolishButton;

		// Token: 0x040018FE RID: 6398
		public ClickableTextureComponent moveButton;

		// Token: 0x040018FF RID: 6399
		public ClickableTextureComponent paintButton;

		// Token: 0x04001900 RID: 6400
		public ClickableTextureComponent appearanceButton;

		// Token: 0x04001901 RID: 6401
		public Building currentBuilding;

		// Token: 0x04001902 RID: 6402
		public Building buildingToMove;

		// Token: 0x04001903 RID: 6403
		public readonly List<Item> ingredients = new List<Item>();

		// Token: 0x04001904 RID: 6404
		public bool onFarm;

		// Token: 0x04001905 RID: 6405
		public CarpenterMenu.CarpentryAction Action;

		// Token: 0x04001906 RID: 6406
		public bool drawBG = true;

		// Token: 0x04001907 RID: 6407
		public bool freeze;

		// Token: 0x04001908 RID: 6408
		private string hoverText = "";

		// Token: 0x020005EF RID: 1519
		public enum CarpentryAction
		{
			// Token: 0x04002E12 RID: 11794
			None,
			// Token: 0x04002E13 RID: 11795
			Demolish,
			// Token: 0x04002E14 RID: 11796
			Move,
			// Token: 0x04002E15 RID: 11797
			Paint,
			// Token: 0x04002E16 RID: 11798
			Upgrade
		}

		// Token: 0x020005F0 RID: 1520
		public class BlueprintEntry
		{
			// Token: 0x170004FA RID: 1274
			// (get) Token: 0x0600439C RID: 17308 RVA: 0x0031A73F File Offset: 0x0031893F
			public int Index { get; }

			// Token: 0x170004FB RID: 1275
			// (get) Token: 0x0600439D RID: 17309 RVA: 0x0031A747 File Offset: 0x00318947
			public string Id { get; }

			// Token: 0x170004FC RID: 1276
			// (get) Token: 0x0600439E RID: 17310 RVA: 0x0031A74F File Offset: 0x0031894F
			public BuildingData Data { get; }

			// Token: 0x170004FD RID: 1277
			// (get) Token: 0x0600439F RID: 17311 RVA: 0x0031A757 File Offset: 0x00318957
			// (set) Token: 0x060043A0 RID: 17312 RVA: 0x0031A75F File Offset: 0x0031895F
			public BuildingSkin Skin { get; private set; }

			// Token: 0x170004FE RID: 1278
			// (get) Token: 0x060043A1 RID: 17313 RVA: 0x0031A768 File Offset: 0x00318968
			// (set) Token: 0x060043A2 RID: 17314 RVA: 0x0031A770 File Offset: 0x00318970
			public string DisplayName { get; private set; }

			// Token: 0x170004FF RID: 1279
			// (get) Token: 0x060043A3 RID: 17315 RVA: 0x0031A779 File Offset: 0x00318979
			// (set) Token: 0x060043A4 RID: 17316 RVA: 0x0031A781 File Offset: 0x00318981
			public string DisplayNameForGeneralType { get; private set; }

			// Token: 0x17000500 RID: 1280
			// (get) Token: 0x060043A5 RID: 17317 RVA: 0x0031A78A File Offset: 0x0031898A
			// (set) Token: 0x060043A6 RID: 17318 RVA: 0x0031A792 File Offset: 0x00318992
			public string TokenizedDisplayName { get; private set; }

			// Token: 0x17000501 RID: 1281
			// (get) Token: 0x060043A7 RID: 17319 RVA: 0x0031A79B File Offset: 0x0031899B
			// (set) Token: 0x060043A8 RID: 17320 RVA: 0x0031A7A3 File Offset: 0x003189A3
			public string TokenizedDisplayNameForGeneralType { get; private set; }

			// Token: 0x17000502 RID: 1282
			// (get) Token: 0x060043A9 RID: 17321 RVA: 0x0031A7AC File Offset: 0x003189AC
			// (set) Token: 0x060043AA RID: 17322 RVA: 0x0031A7B4 File Offset: 0x003189B4
			public string Description { get; private set; }

			// Token: 0x17000503 RID: 1283
			// (get) Token: 0x060043AB RID: 17323 RVA: 0x0031A7BD File Offset: 0x003189BD
			public int TilesWide { get; }

			// Token: 0x17000504 RID: 1284
			// (get) Token: 0x060043AC RID: 17324 RVA: 0x0031A7C5 File Offset: 0x003189C5
			public int TilesHigh { get; }

			// Token: 0x17000505 RID: 1285
			// (get) Token: 0x060043AD RID: 17325 RVA: 0x0031A7CD File Offset: 0x003189CD
			public bool IsUpgrade
			{
				get
				{
					string buildingToUpgrade = this.Data.BuildingToUpgrade;
					return buildingToUpgrade != null && buildingToUpgrade.Length > 0;
				}
			}

			// Token: 0x17000506 RID: 1286
			// (get) Token: 0x060043AE RID: 17326 RVA: 0x0031A7E8 File Offset: 0x003189E8
			public int BuildDays
			{
				get
				{
					BuildingSkin skin = this.Skin;
					int? num = (skin != null) ? skin.BuildDays : null;
					if (num == null)
					{
						return this.Data.BuildDays;
					}
					return num.GetValueOrDefault();
				}
			}

			// Token: 0x17000507 RID: 1287
			// (get) Token: 0x060043AF RID: 17327 RVA: 0x0031A82C File Offset: 0x00318A2C
			public int BuildCost
			{
				get
				{
					BuildingSkin skin = this.Skin;
					int? num = (skin != null) ? skin.BuildCost : null;
					if (num == null)
					{
						return this.Data.BuildCost;
					}
					return num.GetValueOrDefault();
				}
			}

			// Token: 0x17000508 RID: 1288
			// (get) Token: 0x060043B0 RID: 17328 RVA: 0x0031A870 File Offset: 0x00318A70
			public List<BuildingMaterial> BuildMaterials
			{
				get
				{
					BuildingSkin skin = this.Skin;
					return ((skin != null) ? skin.BuildMaterials : null) ?? this.Data.BuildMaterials;
				}
			}

			// Token: 0x17000509 RID: 1289
			// (get) Token: 0x060043B1 RID: 17329 RVA: 0x0031A893 File Offset: 0x00318A93
			public string UpgradeFrom
			{
				get
				{
					return this.Data.BuildingToUpgrade;
				}
			}

			// Token: 0x1700050A RID: 1290
			// (get) Token: 0x060043B2 RID: 17330 RVA: 0x0031A8A0 File Offset: 0x00318AA0
			public bool MagicalConstruction
			{
				get
				{
					return this.Data.MagicalConstruction;
				}
			}

			// Token: 0x060043B3 RID: 17331 RVA: 0x0031A8B0 File Offset: 0x00318AB0
			public BlueprintEntry(int index, string id, BuildingData data, string skinId)
			{
				this.Index = index;
				this.Id = id;
				this.Data = data;
				this.TilesWide = data.Size.X;
				this.TilesHigh = data.Size.Y;
				this.SetSkin(skinId);
			}

			// Token: 0x060043B4 RID: 17332 RVA: 0x0031A904 File Offset: 0x00318B04
			public void SetSkin(string id)
			{
				if (this.Data.Skins != null)
				{
					foreach (BuildingSkin skin in this.Data.Skins)
					{
						if (skin.Id == id)
						{
							this.Skin = skin;
							this.TokenizedDisplayName = (skin.Name ?? this.Data.Name);
							this.TokenizedDisplayNameForGeneralType = skin.NameForGeneralType;
							this.DisplayName = TokenParser.ParseText(this.TokenizedDisplayName, null, null, null);
							this.DisplayNameForGeneralType = (TokenParser.ParseText(this.TokenizedDisplayNameForGeneralType, null, null, null) ?? this.DisplayName);
							this.Description = (TokenParser.ParseText(skin.Description, null, null, null) ?? TokenParser.ParseText(this.Data.Description, null, null, null));
							return;
						}
					}
				}
				this.Skin = null;
				this.TokenizedDisplayName = this.Data.Name;
				this.TokenizedDisplayNameForGeneralType = this.Data.NameForGeneralType;
				this.DisplayName = TokenParser.ParseText(this.TokenizedDisplayName, null, null, null);
				this.DisplayNameForGeneralType = (TokenParser.ParseText(this.TokenizedDisplayNameForGeneralType, null, null, null) ?? this.DisplayName);
				this.Description = TokenParser.ParseText(this.Data.Description, null, null, null);
			}

			// Token: 0x060043B5 RID: 17333 RVA: 0x0031AA80 File Offset: 0x00318C80
			public string GetDisplayNameForBuildingToUpgrade()
			{
				BuildingData otherData;
				if (!this.IsUpgrade || !Game1.buildingData.TryGetValue(this.Data.BuildingToUpgrade, out otherData))
				{
					return null;
				}
				return TokenParser.ParseText(otherData.Name, null, null, null);
			}
		}
	}
}

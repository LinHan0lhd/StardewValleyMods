using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Buffs;
using StardewValley.Enchantments;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using StardewValley.Tools;

namespace StardewValley.Menus
{
	// Token: 0x02000275 RID: 629
	[InstanceStatics]
	public abstract class IClickableMenu
	{
		// Token: 0x170003F0 RID: 1008
		// (get) Token: 0x06002995 RID: 10645 RVA: 0x001EB8FD File Offset: 0x001E9AFD
		public Vector2 Position
		{
			get
			{
				return new Vector2((float)this.xPositionOnScreen, (float)this.yPositionOnScreen);
			}
		}

		// Token: 0x06002996 RID: 10646 RVA: 0x001EB912 File Offset: 0x001E9B12
		public IClickableMenu()
		{
		}

		// Token: 0x06002997 RID: 10647 RVA: 0x001EB928 File Offset: 0x001E9B28
		public IClickableMenu(int x, int y, int width, int height, bool showUpperRightCloseButton = false)
		{
			Game1.mouseCursorTransparency = 1f;
			this.initialize(x, y, width, height, showUpperRightCloseButton);
			if (Game1.gameMode == 3 && Game1.player != null && !Game1.eventUp)
			{
				Game1.player.Halt();
			}
		}

		// Token: 0x06002998 RID: 10648 RVA: 0x001EB980 File Offset: 0x001E9B80
		public void initialize(int x, int y, int width, int height, bool showUpperRightCloseButton = false)
		{
			if (Game1.player != null && !Game1.player.UsingTool && !Game1.eventUp)
			{
				Game1.player.forceCanMove();
			}
			this.xPositionOnScreen = x;
			this.yPositionOnScreen = y;
			this.width = width;
			this.height = height;
			if (showUpperRightCloseButton)
			{
				this.upperRightCloseButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width - 36, this.yPositionOnScreen - 8, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f, false)
				{
					myID = 9175502
				};
			}
			for (int i = 0; i < 4; i++)
			{
				Game1.directionKeyPolling[i] = 250;
			}
		}

		// Token: 0x06002999 RID: 10649 RVA: 0x001EBA3A File Offset: 0x001E9C3A
		public IClickableMenu GetChildMenu()
		{
			return this._childMenu;
		}

		// Token: 0x0600299A RID: 10650 RVA: 0x001EBA42 File Offset: 0x001E9C42
		public IClickableMenu GetParentMenu()
		{
			return this._parentMenu;
		}

		// Token: 0x0600299B RID: 10651 RVA: 0x001EBA4A File Offset: 0x001E9C4A
		public void SetChildMenu(IClickableMenu menu)
		{
			this._childMenu = menu;
			if (this._childMenu != null)
			{
				this._childMenu._parentMenu = this;
			}
		}

		// Token: 0x0600299C RID: 10652 RVA: 0x001EBA67 File Offset: 0x001E9C67
		public void AddDependency()
		{
			this._dependencies++;
		}

		// Token: 0x0600299D RID: 10653 RVA: 0x001EBA77 File Offset: 0x001E9C77
		public void RemoveDependency()
		{
			this._dependencies--;
			if (this._dependencies <= 0 && Game1.activeClickableMenu != this && TitleMenu.subMenu != this)
			{
				IDisposable disposable = this as IDisposable;
				if (disposable == null)
				{
					return;
				}
				disposable.Dispose();
			}
		}

		// Token: 0x0600299E RID: 10654 RVA: 0x001EBAB0 File Offset: 0x001E9CB0
		public bool HasDependencies()
		{
			return this._dependencies > 0;
		}

		// Token: 0x0600299F RID: 10655 RVA: 0x001EBABB File Offset: 0x001E9CBB
		public virtual bool areGamePadControlsImplemented()
		{
			return false;
		}

		// Token: 0x060029A0 RID: 10656 RVA: 0x001EBABE File Offset: 0x001E9CBE
		public virtual void receiveGamePadButton(Buttons button)
		{
		}

		// Token: 0x060029A1 RID: 10657 RVA: 0x001EBAC0 File Offset: 0x001E9CC0
		public void drawMouse(SpriteBatch b, bool ignore_transparency = false, int cursor = -1)
		{
			if (!Game1.options.hardwareCursor)
			{
				float transparency = Game1.mouseCursorTransparency;
				if (ignore_transparency)
				{
					transparency = 1f;
				}
				if (cursor < 0)
				{
					if (Game1.options.snappyMenus && Game1.options.gamepadControls)
					{
						cursor = 44;
					}
					else
					{
						cursor = 0;
					}
				}
				b.Draw(Game1.mouseCursors, new Vector2((float)Game1.getMouseX(), (float)Game1.getMouseY()), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, cursor, 16, 16)), Color.White * transparency, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
			}
		}

		// Token: 0x060029A2 RID: 10658 RVA: 0x001EBB6C File Offset: 0x001E9D6C
		public virtual void populateClickableComponentList()
		{
			this.allClickableComponents = new List<ClickableComponent>();
			foreach (FieldInfo f in base.GetType().GetFields())
			{
				Type fieldType = f.FieldType;
				if (!fieldType.IsPrimitive && !(fieldType == typeof(string)) && f.GetCustomAttribute<SkipForClickableAggregation>() == null && !(f.DeclaringType == typeof(IClickableMenu)))
				{
					object value = f.GetValue(this);
					ClickableComponent component = value as ClickableComponent;
					if (component == null)
					{
						List<List<ClickableTextureComponent>> listOfLists = value as List<List<ClickableTextureComponent>>;
						InventoryMenu inventoryMenu;
						if (listOfLists == null)
						{
							inventoryMenu = (value as InventoryMenu);
							if (inventoryMenu == null)
							{
								List<Dictionary<ClickableTextureComponent, CraftingRecipe>> list = value as List<Dictionary<ClickableTextureComponent, CraftingRecipe>>;
								Dictionary<int, List<List<ClickableTextureComponent>>> dict;
								IDictionary dictionary;
								if (list == null)
								{
									dict = (value as Dictionary<int, List<List<ClickableTextureComponent>>>);
									if (dict == null)
									{
										dictionary = (value as IDictionary);
										if (dictionary != null)
										{
											goto IL_23B;
										}
										IEnumerable list2 = value as IEnumerable;
										if (list2 == null)
										{
											goto IL_396;
										}
										goto IL_311;
									}
								}
								else
								{
									using (List<Dictionary<ClickableTextureComponent, CraftingRecipe>>.Enumerator enumerator = list.GetEnumerator())
									{
										while (enumerator.MoveNext())
										{
											Dictionary<ClickableTextureComponent, CraftingRecipe> dict2 = enumerator.Current;
											this.allClickableComponents.AddRange(dict2.Keys);
										}
										goto IL_396;
									}
								}
								using (Dictionary<int, List<List<ClickableTextureComponent>>>.ValueCollection.Enumerator enumerator2 = dict.Values.GetEnumerator())
								{
									while (enumerator2.MoveNext())
									{
										List<List<ClickableTextureComponent>> list4 = enumerator2.Current;
										foreach (List<ClickableTextureComponent> list3 in list4)
										{
											this.allClickableComponents.AddRange(list3);
										}
									}
									goto IL_396;
								}
								IL_23B:
								if (!fieldType.IsGenericType || !(fieldType.GetGenericTypeDefinition() == typeof(Dictionary<, >)))
								{
									goto IL_396;
								}
								Type componentType = typeof(ClickableComponent);
								Type[] genericArguments = fieldType.GetGenericArguments();
								Type dictKeyType = genericArguments[0];
								Type dictValueType = genericArguments[1];
								if (!componentType.IsAssignableFrom(dictKeyType) && !componentType.IsAssignableFrom(dictValueType))
								{
									goto IL_396;
								}
								using (IDictionaryEnumerator enumerator4 = dictionary.GetEnumerator())
								{
									while (enumerator4.MoveNext())
									{
										object obj = enumerator4.Current;
										DictionaryEntry entry = (DictionaryEntry)obj;
										ClickableComponent keyComponent = entry.Key as ClickableComponent;
										if (keyComponent != null)
										{
											this.allClickableComponents.Add(keyComponent);
										}
										ClickableComponent valueComponent = entry.Value as ClickableComponent;
										if (valueComponent != null)
										{
											this.allClickableComponents.Add(valueComponent);
										}
									}
									goto IL_396;
								}
								IL_311:
								if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>) && typeof(ClickableComponent).IsAssignableFrom(fieldType.GetGenericArguments()[0]))
								{
									IEnumerable list2;
									foreach (object obj2 in list2)
									{
										ClickableComponent component2 = obj2 as ClickableComponent;
										if (component2 != null)
										{
											this.allClickableComponents.Add(component2);
										}
									}
									goto IL_396;
								}
								goto IL_396;
							}
						}
						else
						{
							using (List<List<ClickableTextureComponent>>.Enumerator enumerator3 = listOfLists.GetEnumerator())
							{
								while (enumerator3.MoveNext())
								{
									List<ClickableTextureComponent> list5 = enumerator3.Current;
									foreach (ClickableTextureComponent component3 in list5)
									{
										if (component3 != null)
										{
											this.allClickableComponents.Add(component3);
										}
									}
								}
								goto IL_396;
							}
						}
						this.allClickableComponents.AddRange(inventoryMenu.inventory);
						this.allClickableComponents.Add(inventoryMenu.dropItemInvisibleButton);
					}
					else
					{
						this.allClickableComponents.Add(component);
					}
				}
				IL_396:;
			}
			GameMenu gameMenu = Game1.activeClickableMenu as GameMenu;
			if (gameMenu != null && this == gameMenu.GetCurrentPage())
			{
				gameMenu.AddTabsToClickableComponents(this);
			}
			if (this.upperRightCloseButton != null)
			{
				this.allClickableComponents.Add(this.upperRightCloseButton);
			}
		}

		// Token: 0x060029A3 RID: 10659 RVA: 0x001EBFAC File Offset: 0x001EA1AC
		public virtual void applyMovementKey(int direction)
		{
			if (this.allClickableComponents == null)
			{
				this.populateClickableComponentList();
			}
			this.moveCursorInDirection(direction);
		}

		// Token: 0x060029A4 RID: 10660 RVA: 0x001EBFC3 File Offset: 0x001EA1C3
		public virtual void snapToDefaultClickableComponent()
		{
		}

		// Token: 0x060029A5 RID: 10661 RVA: 0x001EBFC8 File Offset: 0x001EA1C8
		public void applyMovementKey(Keys key)
		{
			if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
			{
				this.applyMovementKey(0);
				return;
			}
			if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
			{
				this.applyMovementKey(1);
				return;
			}
			if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
			{
				this.applyMovementKey(2);
				return;
			}
			if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
			{
				this.applyMovementKey(3);
			}
		}

		// Token: 0x060029A6 RID: 10662 RVA: 0x001EC050 File Offset: 0x001EA250
		public virtual void setCurrentlySnappedComponentTo(int id)
		{
			this.currentlySnappedComponent = this.getComponentWithID(id);
		}

		// Token: 0x060029A7 RID: 10663 RVA: 0x001EC060 File Offset: 0x001EA260
		public void moveCursorInDirection(int direction)
		{
			if (this.currentlySnappedComponent == null)
			{
				List<ClickableComponent> list = this.allClickableComponents;
				if (list != null && list.Count > 0)
				{
					this.snapToDefaultClickableComponent();
					if (this.currentlySnappedComponent == null)
					{
						this.currentlySnappedComponent = this.allClickableComponents[0];
					}
				}
			}
			if (this.currentlySnappedComponent != null)
			{
				ClickableComponent old = this.currentlySnappedComponent;
				switch (direction)
				{
				case 0:
				{
					int num = this.currentlySnappedComponent.upNeighborID;
					if (num != -99999)
					{
						if (num != -99998)
						{
							if (num != -7777)
							{
								this.currentlySnappedComponent = this.getComponentWithID(this.currentlySnappedComponent.upNeighborID);
							}
							else
							{
								this.customSnapBehavior(0, this.currentlySnappedComponent.region, this.currentlySnappedComponent.myID);
							}
						}
						else
						{
							this.automaticSnapBehavior(0, this.currentlySnappedComponent.region, this.currentlySnappedComponent.myID);
						}
					}
					else
					{
						this.snapToDefaultClickableComponent();
					}
					if (this.currentlySnappedComponent != null && (old == null || (old.upNeighborID != -7777 && old.upNeighborID != -99998)) && !this.currentlySnappedComponent.downNeighborImmutable && !this.currentlySnappedComponent.fullyImmutable)
					{
						this.currentlySnappedComponent.downNeighborID = old.myID;
					}
					if (this.currentlySnappedComponent == null)
					{
						this.noSnappedComponentFound(0, old.region, old.myID);
					}
					break;
				}
				case 1:
				{
					int num = this.currentlySnappedComponent.rightNeighborID;
					if (num != -99999)
					{
						if (num != -99998)
						{
							if (num != -7777)
							{
								this.currentlySnappedComponent = this.getComponentWithID(this.currentlySnappedComponent.rightNeighborID);
							}
							else
							{
								this.customSnapBehavior(1, this.currentlySnappedComponent.region, this.currentlySnappedComponent.myID);
							}
						}
						else
						{
							this.automaticSnapBehavior(1, this.currentlySnappedComponent.region, this.currentlySnappedComponent.myID);
						}
					}
					else
					{
						this.snapToDefaultClickableComponent();
					}
					if (this.currentlySnappedComponent != null && (old == null || (old.rightNeighborID != -7777 && old.rightNeighborID != -99998)) && !this.currentlySnappedComponent.leftNeighborImmutable && !this.currentlySnappedComponent.fullyImmutable)
					{
						this.currentlySnappedComponent.leftNeighborID = old.myID;
					}
					if (this.currentlySnappedComponent == null && old.tryDefaultIfNoRightNeighborExists)
					{
						this.snapToDefaultClickableComponent();
					}
					else if (this.currentlySnappedComponent == null)
					{
						this.noSnappedComponentFound(1, old.region, old.myID);
					}
					break;
				}
				case 2:
				{
					int num = this.currentlySnappedComponent.downNeighborID;
					if (num != -99999)
					{
						if (num != -99998)
						{
							if (num != -7777)
							{
								this.currentlySnappedComponent = this.getComponentWithID(this.currentlySnappedComponent.downNeighborID);
							}
							else
							{
								this.customSnapBehavior(2, this.currentlySnappedComponent.region, this.currentlySnappedComponent.myID);
							}
						}
						else
						{
							this.automaticSnapBehavior(2, this.currentlySnappedComponent.region, this.currentlySnappedComponent.myID);
						}
					}
					else
					{
						this.snapToDefaultClickableComponent();
					}
					if (this.currentlySnappedComponent != null && (old == null || (old.downNeighborID != -7777 && old.downNeighborID != -99998)) && !this.currentlySnappedComponent.upNeighborImmutable && !this.currentlySnappedComponent.fullyImmutable)
					{
						this.currentlySnappedComponent.upNeighborID = old.myID;
					}
					if (this.currentlySnappedComponent == null && old.tryDefaultIfNoDownNeighborExists)
					{
						this.snapToDefaultClickableComponent();
					}
					else if (this.currentlySnappedComponent == null)
					{
						this.noSnappedComponentFound(2, old.region, old.myID);
					}
					break;
				}
				case 3:
				{
					int num = this.currentlySnappedComponent.leftNeighborID;
					if (num != -99999)
					{
						if (num != -99998)
						{
							if (num != -7777)
							{
								this.currentlySnappedComponent = this.getComponentWithID(this.currentlySnappedComponent.leftNeighborID);
							}
							else
							{
								this.customSnapBehavior(3, this.currentlySnappedComponent.region, this.currentlySnappedComponent.myID);
							}
						}
						else
						{
							this.automaticSnapBehavior(3, this.currentlySnappedComponent.region, this.currentlySnappedComponent.myID);
						}
					}
					else
					{
						this.snapToDefaultClickableComponent();
					}
					if (this.currentlySnappedComponent != null && (old == null || (old.leftNeighborID != -7777 && old.leftNeighborID != -99998)) && !this.currentlySnappedComponent.rightNeighborImmutable && !this.currentlySnappedComponent.fullyImmutable)
					{
						this.currentlySnappedComponent.rightNeighborID = old.myID;
					}
					if (this.currentlySnappedComponent == null)
					{
						this.noSnappedComponentFound(3, old.region, old.myID);
					}
					break;
				}
				}
				if (this.currentlySnappedComponent != null && old != null && this.currentlySnappedComponent.region != old.region)
				{
					this.actionOnRegionChange(old.region, this.currentlySnappedComponent.region);
				}
				if (this.currentlySnappedComponent == null)
				{
					this.currentlySnappedComponent = old;
				}
				this.snapCursorToCurrentSnappedComponent();
				if (this.currentlySnappedComponent != old)
				{
					Game1.playSound("shiny4", null);
				}
			}
		}

		// Token: 0x060029A8 RID: 10664 RVA: 0x001EC548 File Offset: 0x001EA748
		public virtual void snapCursorToCurrentSnappedComponent()
		{
			if (this.currentlySnappedComponent != null)
			{
				Game1.setMousePosition(this.currentlySnappedComponent.bounds.Right - this.currentlySnappedComponent.bounds.Width / 4, this.currentlySnappedComponent.bounds.Bottom - this.currentlySnappedComponent.bounds.Height / 4, true);
			}
		}

		// Token: 0x060029A9 RID: 10665 RVA: 0x001EC5A9 File Offset: 0x001EA7A9
		protected virtual void noSnappedComponentFound(int direction, int oldRegion, int oldID)
		{
		}

		// Token: 0x060029AA RID: 10666 RVA: 0x001EC5AB File Offset: 0x001EA7AB
		protected virtual void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
		}

		// Token: 0x060029AB RID: 10667 RVA: 0x001EC5B0 File Offset: 0x001EA7B0
		public virtual bool IsActive()
		{
			if (this._parentMenu == null)
			{
				return this == Game1.activeClickableMenu;
			}
			IClickableMenu root = this._parentMenu;
			while (((root != null) ? root._parentMenu : null) != null)
			{
				root = root._parentMenu;
			}
			return root == Game1.activeClickableMenu;
		}

		// Token: 0x060029AC RID: 10668 RVA: 0x001EC5F4 File Offset: 0x001EA7F4
		public virtual void automaticSnapBehavior(int direction, int oldRegion, int oldID)
		{
			if (this.currentlySnappedComponent == null)
			{
				this.snapToDefaultClickableComponent();
				return;
			}
			Vector2 snap_direction = Vector2.Zero;
			switch (direction)
			{
			case 0:
				snap_direction.X = 0f;
				snap_direction.Y = -1f;
				break;
			case 1:
				snap_direction.X = 1f;
				snap_direction.Y = 0f;
				break;
			case 2:
				snap_direction.X = 0f;
				snap_direction.Y = 1f;
				break;
			case 3:
				snap_direction.X = -1f;
				snap_direction.Y = 0f;
				break;
			}
			float closest_distance = -1f;
			ClickableComponent closest_component_in_direction = null;
			for (int i = 0; i < this.allClickableComponents.Count; i++)
			{
				ClickableComponent other_component = this.allClickableComponents[i];
				if ((other_component.leftNeighborID != -1 || other_component.rightNeighborID != -1 || other_component.upNeighborID != -1 || other_component.downNeighborID != -1) && other_component.myID != -500 && this.IsAutomaticSnapValid(direction, this.currentlySnappedComponent, other_component) && other_component.visible && other_component != this.upperRightCloseButton && other_component != this.currentlySnappedComponent)
				{
					Vector2 offset = new Vector2((float)(other_component.bounds.Center.X - this.currentlySnappedComponent.bounds.Center.X), (float)(other_component.bounds.Center.Y - this.currentlySnappedComponent.bounds.Center.Y));
					Vector2 normalized_offset = new Vector2(offset.X, offset.Y);
					normalized_offset.Normalize();
					float dot = Vector2.Dot(snap_direction, normalized_offset);
					if (dot > 0.01f)
					{
						float score = Vector2.DistanceSquared(Vector2.Zero, offset);
						bool close_enough = false;
						switch (direction)
						{
						case 0:
						case 2:
							if (Math.Abs(offset.X) < 32f)
							{
								close_enough = true;
							}
							break;
						case 1:
						case 3:
							if (Math.Abs(offset.Y) < 32f)
							{
								close_enough = true;
							}
							break;
						}
						if (this._ShouldAutoSnapPrioritizeAlignedElements() && (dot > 0.99999f || close_enough))
						{
							score *= 0.01f;
						}
						if (closest_distance == -1f || score < closest_distance)
						{
							closest_distance = score;
							closest_component_in_direction = other_component;
						}
					}
				}
			}
			if (closest_component_in_direction != null)
			{
				this.currentlySnappedComponent = closest_component_in_direction;
			}
		}

		// Token: 0x060029AD RID: 10669 RVA: 0x001EC859 File Offset: 0x001EAA59
		protected virtual bool _ShouldAutoSnapPrioritizeAlignedElements()
		{
			return true;
		}

		// Token: 0x060029AE RID: 10670 RVA: 0x001EC85C File Offset: 0x001EAA5C
		public virtual bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
		{
			return true;
		}

		// Token: 0x060029AF RID: 10671 RVA: 0x001EC85F File Offset: 0x001EAA5F
		protected virtual void actionOnRegionChange(int oldRegion, int newRegion)
		{
		}

		// Token: 0x060029B0 RID: 10672 RVA: 0x001EC864 File Offset: 0x001EAA64
		public ClickableComponent getComponentWithID(int id)
		{
			if (id == -500)
			{
				return null;
			}
			if (this.allClickableComponents != null)
			{
				for (int i = 0; i < this.allClickableComponents.Count; i++)
				{
					if (this.allClickableComponents[i] != null && this.allClickableComponents[i].myID == id && this.allClickableComponents[i].visible)
					{
						return this.allClickableComponents[i];
					}
				}
				for (int j = 0; j < this.allClickableComponents.Count; j++)
				{
					if (this.allClickableComponents[j] != null && this.allClickableComponents[j].myAlternateID == id && this.allClickableComponents[j].visible)
					{
						return this.allClickableComponents[j];
					}
				}
			}
			return null;
		}

		// Token: 0x060029B1 RID: 10673 RVA: 0x001EC938 File Offset: 0x001EAB38
		public void initializeUpperRightCloseButton()
		{
			this.upperRightCloseButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - 36, this.yPositionOnScreen - 8, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f, false);
		}

		// Token: 0x060029B2 RID: 10674 RVA: 0x001EC990 File Offset: 0x001EAB90
		public virtual void drawBackground(SpriteBatch b)
		{
			if (this is ShopMenu)
			{
				for (int x = 0; x < Game1.uiViewport.Width; x += 400)
				{
					for (int y = 0; y < Game1.uiViewport.Height; y += 384)
					{
						b.Draw(Game1.mouseCursors, new Vector2((float)x, (float)y), new Rectangle?(new Rectangle(527, 0, 100, 96)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.08f);
					}
				}
				return;
			}
			if (Game1.isDarkOut(Game1.currentLocation))
			{
				b.Draw(Game1.mouseCursors, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Rectangle?(new Rectangle(639, 858, 1, 144)), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.9f);
			}
			else if (Game1.IsRainingHere(null))
			{
				b.Draw(Game1.mouseCursors, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Rectangle?(new Rectangle(640, 858, 1, 184)), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.9f);
			}
			else
			{
				b.Draw(Game1.mouseCursors, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Rectangle?(new Rectangle(639 + Game1.seasonIndex, 1051, 1, 400)), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.9f);
			}
			b.Draw(Game1.mouseCursors, new Vector2(-120f, (float)(Game1.uiViewport.Height - 592)), new Rectangle?(new Rectangle(0, (Game1.season == Season.Winter) ? 1035 : ((Game1.isRaining || Game1.isDarkOut(Game1.currentLocation)) ? 886 : 737), 639, 148)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.08f);
			b.Draw(Game1.mouseCursors, new Vector2(2436f, (float)(Game1.uiViewport.Height - 592)), new Rectangle?(new Rectangle(0, (Game1.season == Season.Winter) ? 1035 : ((Game1.isRaining || Game1.isDarkOut(Game1.currentLocation)) ? 886 : 737), 639, 148)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.08f);
			if (Game1.isRaining)
			{
				b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Blue * 0.2f);
			}
		}

		// Token: 0x060029B3 RID: 10675 RVA: 0x001ECC7C File Offset: 0x001EAE7C
		public virtual bool showWithoutTransparencyIfOptionIsSet()
		{
			return this is GameMenu || this is ShopMenu || this is WheelSpinGame || this is ItemGrabMenu;
		}

		// Token: 0x060029B4 RID: 10676 RVA: 0x001ECCA1 File Offset: 0x001EAEA1
		public virtual void clickAway()
		{
		}

		// Token: 0x060029B5 RID: 10677 RVA: 0x001ECCA4 File Offset: 0x001EAEA4
		public virtual void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			this.xPositionOnScreen = (int)((float)(newBounds.Width - this.width) * ((float)this.xPositionOnScreen / (float)(oldBounds.Width - this.width)));
			this.yPositionOnScreen = (int)((float)(newBounds.Height - this.height) * ((float)this.yPositionOnScreen / (float)(oldBounds.Height - this.height)));
		}

		// Token: 0x060029B6 RID: 10678 RVA: 0x001ECD09 File Offset: 0x001EAF09
		public virtual void setUpForGamePadMode()
		{
		}

		// Token: 0x060029B7 RID: 10679 RVA: 0x001ECD0B File Offset: 0x001EAF0B
		public virtual bool shouldClampGamePadCursor()
		{
			return false;
		}

		// Token: 0x060029B8 RID: 10680 RVA: 0x001ECD0E File Offset: 0x001EAF0E
		public virtual void releaseLeftClick(int x, int y)
		{
		}

		// Token: 0x060029B9 RID: 10681 RVA: 0x001ECD10 File Offset: 0x001EAF10
		public virtual void leftClickHeld(int x, int y)
		{
		}

		// Token: 0x060029BA RID: 10682 RVA: 0x001ECD14 File Offset: 0x001EAF14
		public virtual void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (this.upperRightCloseButton != null && this.readyToClose() && this.upperRightCloseButton.containsPoint(x, y))
			{
				if (playSound)
				{
					Game1.playSound(this.closeSound, null);
				}
				this.exitThisMenu(true);
			}
		}

		// Token: 0x060029BB RID: 10683 RVA: 0x001ECD5F File Offset: 0x001EAF5F
		public virtual bool overrideSnappyMenuCursorMovementBan()
		{
			return false;
		}

		// Token: 0x060029BC RID: 10684 RVA: 0x001ECD62 File Offset: 0x001EAF62
		public virtual void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		// Token: 0x060029BD RID: 10685 RVA: 0x001ECD64 File Offset: 0x001EAF64
		public virtual void receiveKeyPress(Keys key)
		{
			if (key == Keys.None)
			{
				return;
			}
			if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.readyToClose())
			{
				this.exitThisMenu(true);
				return;
			}
			if (Game1.options.snappyMenus && Game1.options.gamepadControls && !this.overrideSnappyMenuCursorMovementBan())
			{
				this.applyMovementKey(key);
			}
		}

		// Token: 0x060029BE RID: 10686 RVA: 0x001ECDC3 File Offset: 0x001EAFC3
		public virtual void gamePadButtonHeld(Buttons b)
		{
		}

		// Token: 0x060029BF RID: 10687 RVA: 0x001ECDC5 File Offset: 0x001EAFC5
		public virtual ClickableComponent getCurrentlySnappedComponent()
		{
			return this.currentlySnappedComponent;
		}

		// Token: 0x060029C0 RID: 10688 RVA: 0x001ECDCD File Offset: 0x001EAFCD
		public virtual void receiveScrollWheelAction(int direction)
		{
		}

		// Token: 0x060029C1 RID: 10689 RVA: 0x001ECDCF File Offset: 0x001EAFCF
		public virtual void performHoverAction(int x, int y)
		{
			ClickableTextureComponent clickableTextureComponent = this.upperRightCloseButton;
			if (clickableTextureComponent == null)
			{
				return;
			}
			clickableTextureComponent.tryHover(x, y, 0.5f);
		}

		// Token: 0x060029C2 RID: 10690 RVA: 0x001ECDE8 File Offset: 0x001EAFE8
		public virtual void draw(SpriteBatch b, int red, int green, int blue)
		{
			if (this.upperRightCloseButton != null && this.shouldDrawCloseButton())
			{
				this.upperRightCloseButton.draw(b);
			}
		}

		// Token: 0x060029C3 RID: 10691 RVA: 0x001ECE06 File Offset: 0x001EB006
		public virtual void draw(SpriteBatch b)
		{
			if (this.upperRightCloseButton != null && this.shouldDrawCloseButton())
			{
				this.upperRightCloseButton.draw(b);
			}
		}

		// Token: 0x060029C4 RID: 10692 RVA: 0x001ECE24 File Offset: 0x001EB024
		public virtual bool isWithinBounds(int x, int y)
		{
			return x - this.xPositionOnScreen < this.width && x - this.xPositionOnScreen >= 0 && y - this.yPositionOnScreen < this.height && y - this.yPositionOnScreen >= 0;
		}

		// Token: 0x060029C5 RID: 10693 RVA: 0x001ECE61 File Offset: 0x001EB061
		public virtual void update(GameTime time)
		{
		}

		// Token: 0x060029C6 RID: 10694 RVA: 0x001ECE63 File Offset: 0x001EB063
		protected virtual void cleanupBeforeExit()
		{
		}

		// Token: 0x060029C7 RID: 10695 RVA: 0x001ECE65 File Offset: 0x001EB065
		public virtual bool shouldDrawCloseButton()
		{
			return true;
		}

		// Token: 0x060029C8 RID: 10696 RVA: 0x001ECE68 File Offset: 0x001EB068
		public void exitThisMenuNoSound()
		{
			this.exitThisMenu(false);
		}

		// Token: 0x060029C9 RID: 10697 RVA: 0x001ECE74 File Offset: 0x001EB074
		public void exitThisMenu(bool playSound = true)
		{
			Action<IClickableMenu> action = this.behaviorBeforeCleanup;
			if (action != null)
			{
				action(this);
			}
			this.cleanupBeforeExit();
			if (playSound)
			{
				Game1.playSound(this.closeSound, null);
			}
			if (this == Game1.activeClickableMenu)
			{
				Game1.exitActiveMenu();
			}
			else
			{
				GameMenu gameMenu = Game1.activeClickableMenu as GameMenu;
				if (gameMenu != null && gameMenu.GetCurrentPage() == this)
				{
					Game1.exitActiveMenu();
				}
			}
			if (this._parentMenu != null)
			{
				IClickableMenu parentMenu = this._parentMenu;
				this._parentMenu = null;
				parentMenu.SetChildMenu(null);
			}
			if (this.exitFunction != null)
			{
				IClickableMenu.onExit onExit = this.exitFunction;
				this.exitFunction = null;
				onExit();
			}
		}

		// Token: 0x060029CA RID: 10698 RVA: 0x001ECF11 File Offset: 0x001EB111
		public virtual void emergencyShutDown()
		{
		}

		// Token: 0x060029CB RID: 10699 RVA: 0x001ECF13 File Offset: 0x001EB113
		public virtual bool readyToClose()
		{
			return true;
		}

		// Token: 0x060029CC RID: 10700 RVA: 0x001ECF18 File Offset: 0x001EB118
		protected void drawHorizontalPartition(SpriteBatch b, int yPosition, bool small = false, int red = -1, int green = -1, int blue = -1)
		{
			Color tint = (red == -1) ? Color.White : new Color(red, green, blue);
			Texture2D texture = (red == -1) ? Game1.menuTexture : Game1.uncoloredMenuTexture;
			if (small)
			{
				b.Draw(texture, new Rectangle(this.xPositionOnScreen + 32, yPosition, this.width - 64, 64), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 25, -1, -1)), tint);
				return;
			}
			b.Draw(texture, new Vector2((float)this.xPositionOnScreen, (float)yPosition), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 4, -1, -1)), tint);
			b.Draw(texture, new Rectangle(this.xPositionOnScreen + 64, yPosition, this.width - 128, 64), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 6, -1, -1)), tint);
			b.Draw(texture, new Vector2((float)(this.xPositionOnScreen + this.width - 64), (float)yPosition), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 7, -1, -1)), tint);
		}

		// Token: 0x060029CD RID: 10701 RVA: 0x001ED01C File Offset: 0x001EB21C
		protected void drawVerticalPartition(SpriteBatch b, int xPosition, bool small = false, int red = -1, int green = -1, int blue = -1, int heightOverride = -1)
		{
			Color tint = (red == -1) ? Color.White : new Color(red, green, blue);
			Texture2D texture = (red == -1) ? Game1.menuTexture : Game1.uncoloredMenuTexture;
			if (small)
			{
				b.Draw(texture, new Rectangle(xPosition, this.yPositionOnScreen + 64 + 32, 64, (heightOverride != -1) ? heightOverride : (this.height - 128)), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 26, -1, -1)), tint);
				return;
			}
			b.Draw(texture, new Vector2((float)xPosition, (float)(this.yPositionOnScreen + 64)), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 1, -1, -1)), tint);
			b.Draw(texture, new Rectangle(xPosition, this.yPositionOnScreen + 128, 64, (heightOverride != -1) ? heightOverride : (this.height - 192)), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 5, -1, -1)), tint);
			b.Draw(texture, new Vector2((float)xPosition, (float)(this.yPositionOnScreen + ((heightOverride != -1) ? heightOverride : (this.height - 64)))), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 13, -1, -1)), tint);
		}

		// Token: 0x060029CE RID: 10702 RVA: 0x001ED148 File Offset: 0x001EB348
		protected void drawVerticalIntersectingPartition(SpriteBatch b, int xPosition, int yPosition, int red = -1, int green = -1, int blue = -1)
		{
			Color tint = (red == -1) ? Color.White : new Color(red, green, blue);
			Texture2D texture = (red == -1) ? Game1.menuTexture : Game1.uncoloredMenuTexture;
			b.Draw(texture, new Vector2((float)xPosition, (float)yPosition), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 59, -1, -1)), tint);
			b.Draw(texture, new Rectangle(xPosition, yPosition + 64, 64, this.yPositionOnScreen + this.height - 64 - yPosition - 64), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 63, -1, -1)), tint);
			b.Draw(texture, new Vector2((float)xPosition, (float)(this.yPositionOnScreen + this.height - 64)), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 62, -1, -1)), tint);
		}

		// Token: 0x060029CF RID: 10703 RVA: 0x001ED214 File Offset: 0x001EB414
		protected void drawVerticalUpperIntersectingPartition(SpriteBatch b, int xPosition, int partitionHeight, int red = -1, int green = -1, int blue = -1)
		{
			Color tint = (red == -1) ? Color.White : new Color(red, green, blue);
			Texture2D texture = (red == -1) ? Game1.menuTexture : Game1.uncoloredMenuTexture;
			b.Draw(texture, new Vector2((float)xPosition, (float)(this.yPositionOnScreen + 64)), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 44, -1, -1)), tint);
			b.Draw(texture, new Rectangle(xPosition, this.yPositionOnScreen + 128, 64, partitionHeight - 32), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 63, -1, -1)), tint);
			b.Draw(texture, new Vector2((float)xPosition, (float)(this.yPositionOnScreen + partitionHeight + 64)), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 39, -1, -1)), tint);
		}

		// Token: 0x060029D0 RID: 10704 RVA: 0x001ED2D8 File Offset: 0x001EB4D8
		public static void drawTextureBox(SpriteBatch b, int x, int y, int width, int height, Color color)
		{
			IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y, width, height, color, 1f, true, -1f);
		}

		// Token: 0x060029D1 RID: 10705 RVA: 0x001ED314 File Offset: 0x001EB514
		public static void drawTextureBox(SpriteBatch b, Texture2D texture, Rectangle sourceRect, int x, int y, int width, int height, Color color, float scale = 1f, bool drawShadow = true, float draw_layer = -1f)
		{
			int cornerSize = sourceRect.Width / 3;
			float shadow_layer = draw_layer - 0.03f;
			if (draw_layer < 0f)
			{
				draw_layer = 0.8f - (float)y * 1E-06f;
				shadow_layer = 0.77f;
			}
			if (drawShadow)
			{
				b.Draw(texture, new Vector2((float)(x + width - (int)((float)cornerSize * scale) - 8), (float)(y + 8)), new Rectangle?(new Rectangle(sourceRect.X + cornerSize * 2, sourceRect.Y, cornerSize, cornerSize)), Color.Black * 0.4f, 0f, Vector2.Zero, scale, SpriteEffects.None, shadow_layer);
				b.Draw(texture, new Vector2((float)(x - 8), (float)(y + height - (int)((float)cornerSize * scale) + 8)), new Rectangle?(new Rectangle(sourceRect.X, cornerSize * 2 + sourceRect.Y, cornerSize, cornerSize)), Color.Black * 0.4f, 0f, Vector2.Zero, scale, SpriteEffects.None, shadow_layer);
				b.Draw(texture, new Vector2((float)(x + width - (int)((float)cornerSize * scale) - 8), (float)(y + height - (int)((float)cornerSize * scale) + 8)), new Rectangle?(new Rectangle(sourceRect.X + cornerSize * 2, cornerSize * 2 + sourceRect.Y, cornerSize, cornerSize)), Color.Black * 0.4f, 0f, Vector2.Zero, scale, SpriteEffects.None, shadow_layer);
				b.Draw(texture, new Rectangle(x + (int)((float)cornerSize * scale) - 8, y + 8, width - (int)((float)cornerSize * scale) * 2, (int)((float)cornerSize * scale)), new Rectangle?(new Rectangle(sourceRect.X + cornerSize, sourceRect.Y, cornerSize, cornerSize)), Color.Black * 0.4f, 0f, Vector2.Zero, SpriteEffects.None, shadow_layer);
				b.Draw(texture, new Rectangle(x + (int)((float)cornerSize * scale) - 8, y + height - (int)((float)cornerSize * scale) + 8, width - (int)((float)cornerSize * scale) * 2, (int)((float)cornerSize * scale)), new Rectangle?(new Rectangle(sourceRect.X + cornerSize, cornerSize * 2 + sourceRect.Y, cornerSize, cornerSize)), Color.Black * 0.4f, 0f, Vector2.Zero, SpriteEffects.None, shadow_layer);
				b.Draw(texture, new Rectangle(x - 8, y + (int)((float)cornerSize * scale) + 8, (int)((float)cornerSize * scale), height - (int)((float)cornerSize * scale) * 2), new Rectangle?(new Rectangle(sourceRect.X, cornerSize + sourceRect.Y, cornerSize, cornerSize)), Color.Black * 0.4f, 0f, Vector2.Zero, SpriteEffects.None, shadow_layer);
				b.Draw(texture, new Rectangle(x + width - (int)((float)cornerSize * scale) - 8, y + (int)((float)cornerSize * scale) + 8, (int)((float)cornerSize * scale), height - (int)((float)cornerSize * scale) * 2), new Rectangle?(new Rectangle(sourceRect.X + cornerSize * 2, cornerSize + sourceRect.Y, cornerSize, cornerSize)), Color.Black * 0.4f, 0f, Vector2.Zero, SpriteEffects.None, shadow_layer);
				b.Draw(texture, new Rectangle((int)((float)cornerSize * scale / 2f) + x - 8, (int)((float)cornerSize * scale / 2f) + y + 8, width - (int)((float)cornerSize * scale), height - (int)((float)cornerSize * scale)), new Rectangle?(new Rectangle(cornerSize + sourceRect.X, cornerSize + sourceRect.Y, cornerSize, cornerSize)), Color.Black * 0.4f, 0f, Vector2.Zero, SpriteEffects.None, shadow_layer);
			}
			b.Draw(texture, new Rectangle((int)((float)cornerSize * scale) + x, (int)((float)cornerSize * scale) + y, width - (int)((float)cornerSize * scale * 2f), height - (int)((float)cornerSize * scale * 2f)), new Rectangle?(new Rectangle(cornerSize + sourceRect.X, cornerSize + sourceRect.Y, cornerSize, cornerSize)), color, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
			b.Draw(texture, new Vector2((float)x, (float)y), new Rectangle?(new Rectangle(sourceRect.X, sourceRect.Y, cornerSize, cornerSize)), color, 0f, Vector2.Zero, scale, SpriteEffects.None, draw_layer);
			b.Draw(texture, new Vector2((float)(x + width - (int)((float)cornerSize * scale)), (float)y), new Rectangle?(new Rectangle(sourceRect.X + cornerSize * 2, sourceRect.Y, cornerSize, cornerSize)), color, 0f, Vector2.Zero, scale, SpriteEffects.None, draw_layer);
			b.Draw(texture, new Vector2((float)x, (float)(y + height - (int)((float)cornerSize * scale))), new Rectangle?(new Rectangle(sourceRect.X, cornerSize * 2 + sourceRect.Y, cornerSize, cornerSize)), color, 0f, Vector2.Zero, scale, SpriteEffects.None, draw_layer);
			b.Draw(texture, new Vector2((float)(x + width - (int)((float)cornerSize * scale)), (float)(y + height - (int)((float)cornerSize * scale))), new Rectangle?(new Rectangle(sourceRect.X + cornerSize * 2, cornerSize * 2 + sourceRect.Y, cornerSize, cornerSize)), color, 0f, Vector2.Zero, scale, SpriteEffects.None, draw_layer);
			b.Draw(texture, new Rectangle(x + (int)((float)cornerSize * scale), y, width - (int)((float)cornerSize * scale) * 2, (int)((float)cornerSize * scale)), new Rectangle?(new Rectangle(sourceRect.X + cornerSize, sourceRect.Y, cornerSize, cornerSize)), color, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
			b.Draw(texture, new Rectangle(x + (int)((float)cornerSize * scale), y + height - (int)((float)cornerSize * scale), width - (int)((float)cornerSize * scale) * 2, (int)((float)cornerSize * scale)), new Rectangle?(new Rectangle(sourceRect.X + cornerSize, cornerSize * 2 + sourceRect.Y, cornerSize, cornerSize)), color, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
			b.Draw(texture, new Rectangle(x, y + (int)((float)cornerSize * scale), (int)((float)cornerSize * scale), height - (int)((float)cornerSize * scale) * 2), new Rectangle?(new Rectangle(sourceRect.X, cornerSize + sourceRect.Y, cornerSize, cornerSize)), color, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
			b.Draw(texture, new Rectangle(x + width - (int)((float)cornerSize * scale), y + (int)((float)cornerSize * scale), (int)((float)cornerSize * scale), height - (int)((float)cornerSize * scale) * 2), new Rectangle?(new Rectangle(sourceRect.X + cornerSize * 2, cornerSize + sourceRect.Y, cornerSize, cornerSize)), color, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
		}

		// Token: 0x060029D2 RID: 10706 RVA: 0x001ED964 File Offset: 0x001EBB64
		public void drawBorderLabel(SpriteBatch b, string text, SpriteFont font, int x, int y)
		{
			int width = (int)font.MeasureString(text).X;
			y += 52;
			b.Draw(Game1.mouseCursors, new Vector2((float)x, (float)y), new Rectangle?(new Rectangle(256, 267, 6, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			b.Draw(Game1.mouseCursors, new Vector2((float)(x + 24), (float)y), new Rectangle?(new Rectangle(262, 267, 1, 16)), Color.White, 0f, Vector2.Zero, new Vector2((float)width, 4f), SpriteEffects.None, 0.87f);
			b.Draw(Game1.mouseCursors, new Vector2((float)(x + 24 + width), (float)y), new Rectangle?(new Rectangle(263, 267, 6, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
			Utility.drawTextWithShadow(b, text, font, new Vector2((float)(x + 24), (float)(y + 20)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
		}

		// Token: 0x060029D3 RID: 10707 RVA: 0x001EDA9C File Offset: 0x001EBC9C
		public static void drawToolTip(SpriteBatch b, string hoverText, string hoverTitle, Item hoveredItem, bool heldItem = false, int healAmountToDisplay = -1, int currencySymbol = 0, string extraItemToShowIndex = null, int extraItemToShowAmount = -1, CraftingRecipe craftingIngredients = null, int moneyAmountToShowAtBottom = -1, IList<Item> additionalCraftMaterials = null)
		{
			Object hoveredObj = hoveredItem as Object;
			bool edibleItem = hoveredObj != null && hoveredObj.edibility.Value != -300;
			string[] buffIcons = null;
			ObjectData rawData;
			if (edibleItem && Game1.objectData.TryGetValue(hoveredItem.ItemId, out rawData))
			{
				BuffEffects effects = new BuffEffects();
				int millisecondsDuration = int.MinValue;
				foreach (Buff buff in Object.TryCreateBuffsFromData(rawData, hoveredItem.Name, hoveredItem.DisplayName, 1f, new Action<BuffEffects>(hoveredItem.ModifyItemBuffs)))
				{
					effects.Add(buff.effects);
					if (buff.millisecondsDuration == -2 || (buff.millisecondsDuration > millisecondsDuration && millisecondsDuration != -2))
					{
						millisecondsDuration = buff.millisecondsDuration;
					}
				}
				if (effects.HasAnyValue())
				{
					buffIcons = effects.ToLegacyAttributeFormat();
					if (millisecondsDuration != -2)
					{
						buffIcons[12] = " " + Utility.getMinutesSecondsStringFromMilliseconds(millisecondsDuration);
					}
				}
			}
			IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont, heldItem ? 40 : 0, heldItem ? 40 : 0, moneyAmountToShowAtBottom, hoverTitle, edibleItem ? (hoveredItem as Object).edibility.Value : -1, buffIcons, hoveredItem, currencySymbol, extraItemToShowIndex, extraItemToShowAmount, -1, -1, 1f, craftingIngredients, additionalCraftMaterials, null, null, null, null, 1f, -1, -1);
		}

		// Token: 0x060029D4 RID: 10708 RVA: 0x001EDC28 File Offset: 0x001EBE28
		public static void drawHoverText(SpriteBatch b, string text, SpriteFont font, int xOffset = 0, int yOffset = 0, int moneyAmountToDisplayAtBottom = -1, string boldTitleText = null, int healAmountToDisplay = -1, string[] buffIconsToDisplay = null, Item hoveredItem = null, int currencySymbol = 0, string extraItemToShowIndex = null, int extraItemToShowAmount = -1, int overrideX = -1, int overrideY = -1, float alpha = 1f, CraftingRecipe craftingIngredients = null, IList<Item> additional_craft_materials = null, Texture2D boxTexture = null, Rectangle? boxSourceRect = null, Color? textColor = null, Color? textShadowColor = null, float boxScale = 1f, int boxWidthOverride = -1, int boxHeightOverride = -1)
		{
			IClickableMenu.HoverTextStringBuilder.Clear();
			IClickableMenu.HoverTextStringBuilder.Append(text);
			IClickableMenu.drawHoverText(b, IClickableMenu.HoverTextStringBuilder, font, xOffset, yOffset, moneyAmountToDisplayAtBottom, boldTitleText, healAmountToDisplay, buffIconsToDisplay, hoveredItem, currencySymbol, extraItemToShowIndex, extraItemToShowAmount, overrideX, overrideY, alpha, craftingIngredients, additional_craft_materials, boxTexture, boxSourceRect, textColor, textShadowColor, boxScale, boxWidthOverride, boxHeightOverride);
		}

		// Token: 0x060029D5 RID: 10709 RVA: 0x001EDC84 File Offset: 0x001EBE84
		public static void drawHoverText(SpriteBatch b, StringBuilder text, SpriteFont font, int xOffset = 0, int yOffset = 0, int moneyAmountToDisplayAtBottom = -1, string boldTitleText = null, int healAmountToDisplay = -1, string[] buffIconsToDisplay = null, Item hoveredItem = null, int currencySymbol = 0, string extraItemToShowIndex = null, int extraItemToShowAmount = -1, int overrideX = -1, int overrideY = -1, float alpha = 1f, CraftingRecipe craftingIngredients = null, IList<Item> additional_craft_materials = null, Texture2D boxTexture = null, Rectangle? boxSourceRect = null, Color? textColor = null, Color? textShadowColor = null, float boxScale = 1f, int boxWidthOverride = -1, int boxHeightOverride = -1)
		{
			boxTexture = (boxTexture ?? Game1.menuTexture);
			boxSourceRect = new Rectangle?(boxSourceRect ?? new Rectangle(0, 256, 60, 60));
			textColor = new Color?(textColor ?? Game1.textColor);
			textShadowColor = new Color?(textShadowColor ?? Game1.textShadowColor);
			if (text == null || text.Length == 0)
			{
				return;
			}
			if (hoveredItem != null && craftingIngredients != null && hoveredItem.getDescription().Equals(text.ToString()))
			{
				text = new StringBuilder(" ");
			}
			if (moneyAmountToDisplayAtBottom <= -1 && currencySymbol == 0 && hoveredItem != null && Game1.player.stats.Get("Book_PriceCatalogue") > 0U && !(hoveredItem is Furniture) && hoveredItem.CanBeLostOnDeath() && !(hoveredItem is Clothing) && !(hoveredItem is Wallpaper) && (!(hoveredItem is Object) || !(hoveredItem as Object).bigCraftable.Value) && hoveredItem.sellToStorePrice(-1L) > 0)
			{
				moneyAmountToDisplayAtBottom = hoveredItem.sellToStorePrice(-1L) * hoveredItem.Stack;
			}
			string bold_title_subtext = null;
			if (boldTitleText != null && boldTitleText.Length == 0)
			{
				boldTitleText = null;
			}
			int num = 20;
			int width = Math.Max((healAmountToDisplay != -1) ? ((int)font.MeasureString(healAmountToDisplay.ToString() + "+ Energy" + 32.ToString()).X) : 0, Math.Max((int)font.MeasureString(text).X, (boldTitleText != null) ? ((int)Game1.dialogueFont.MeasureString(boldTitleText).X) : 0)) + 32;
			int height = Math.Max(num * 3, (int)font.MeasureString(text).Y + 32 + (int)((moneyAmountToDisplayAtBottom > -1) ? Math.Max(font.MeasureString(moneyAmountToDisplayAtBottom.ToString() ?? "").Y + 4f, 44f) : 0f) + (int)((boldTitleText != null) ? (Game1.dialogueFont.MeasureString(boldTitleText).Y + 16f) : 0f));
			if (extraItemToShowIndex != null)
			{
				ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem("(O)" + extraItemToShowIndex);
				string objName = dataOrErrorItem.DisplayName;
				ref Rectangle sourceRect2 = dataOrErrorItem.GetSourceRect(0, null);
				string requirement = Game1.content.LoadString("Strings\\UI:ItemHover_Requirements", extraItemToShowAmount, (extraItemToShowAmount > 1) ? Lexicon.makePlural(objName, false) : objName);
				int spriteWidth = sourceRect2.Width * 2 * 4;
				width = Math.Max(width, spriteWidth + (int)font.MeasureString(requirement).X);
			}
			if (buffIconsToDisplay != null)
			{
				foreach (string s in buffIconsToDisplay)
				{
					if (!s.Equals("0") && s != "")
					{
						height += 39;
					}
				}
				height += 4;
			}
			if (craftingIngredients != null && Game1.options.showAdvancedCraftingInformation && craftingIngredients.getCraftCountText() != null)
			{
				height += (int)font.MeasureString("T").Y + 2;
			}
			string categoryName = null;
			if (hoveredItem != null)
			{
				if (hoveredItem is FishingRod)
				{
					if (hoveredItem.attachmentSlots() == 1)
					{
						height += 68;
					}
					else if (hoveredItem.attachmentSlots() > 1)
					{
						height += 144;
					}
				}
				else
				{
					height += 68 * hoveredItem.attachmentSlots();
				}
				categoryName = hoveredItem.getCategoryName();
				if (categoryName.Length > 0)
				{
					width = Math.Max(width, (int)font.MeasureString(categoryName).X + 32);
					height += (int)font.MeasureString("T").Y;
				}
				int maxStat = 9999;
				int buffer = 92;
				Point p = hoveredItem.getExtraSpaceNeededForTooltipSpecialIcons(font, width, buffer, height, text, boldTitleText, moneyAmountToDisplayAtBottom);
				width = ((p.X != 0) ? p.X : width);
				height = ((p.Y != 0) ? p.Y : height);
				MeleeWeapon weapon = hoveredItem as MeleeWeapon;
				if (weapon == null)
				{
					Object obj = hoveredItem as Object;
					if (obj != null)
					{
						if (obj.edibility.Value != -300 && obj.edibility.Value != 0)
						{
							healAmountToDisplay = obj.staminaRecoveredOnConsumption();
							if (healAmountToDisplay != -1)
							{
								height += 40 * ((healAmountToDisplay > 0 && obj.healthRecoveredOnConsumption() > 0) ? 2 : 1);
							}
							else
							{
								height += 40;
							}
							if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.zh && Game1.options.useChineseSmoothFont)
							{
								height += 16;
							}
							width = (int)Math.Max((float)width, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Energy", maxStat)).X + (float)buffer, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Health", maxStat)).X + (float)buffer));
						}
					}
				}
				else
				{
					if (weapon.GetTotalForgeLevels(false) > 0)
					{
						height += (int)font.MeasureString("T").Y;
					}
					if (weapon.GetEnchantmentLevel<GalaxySoulEnchantment>() > 0)
					{
						height += (int)font.MeasureString("T").Y;
					}
				}
				if (buffIconsToDisplay != null)
				{
					for (int i = 0; i < buffIconsToDisplay.Length; i++)
					{
						if (!buffIconsToDisplay[i].Equals("0") && i <= 12)
						{
							width = (int)Math.Max((float)width, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Buff" + i.ToString(), maxStat)).X + (float)buffer);
						}
					}
				}
			}
			Vector2 small_text_size = Vector2.Zero;
			if (craftingIngredients != null)
			{
				if (Game1.options.showAdvancedCraftingInformation)
				{
					int craftable_count = craftingIngredients.getCraftableCount(additional_craft_materials);
					if (craftable_count > 1)
					{
						bold_title_subtext = " (" + craftable_count.ToString() + ")";
						small_text_size = Game1.smallFont.MeasureString(bold_title_subtext);
					}
				}
				width = (int)Math.Max(Game1.dialogueFont.MeasureString(boldTitleText).X + small_text_size.X + 12f, 384f);
				height += craftingIngredients.getDescriptionHeight(width + 4 - 8) - 32;
				if (craftingIngredients != null && hoveredItem != null && hoveredItem.getDescription().Equals(text.ToString()))
				{
					height -= (int)font.MeasureString(text.ToString()).Y;
				}
				if (craftingIngredients != null && Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.zh)
				{
					height += 8;
				}
			}
			else if (bold_title_subtext != null && boldTitleText != null)
			{
				small_text_size = Game1.smallFont.MeasureString(bold_title_subtext);
				width = (int)Math.Max((float)width, Game1.dialogueFont.MeasureString(boldTitleText).X + small_text_size.X + 12f);
			}
			int x = Game1.getOldMouseX() + 32 + xOffset;
			int y = Game1.getOldMouseY() + 32 + yOffset;
			if (overrideX != -1)
			{
				x = overrideX;
			}
			if (overrideY != -1)
			{
				y = overrideY;
			}
			if (x + width > Utility.getSafeArea().Right)
			{
				x = Utility.getSafeArea().Right - width;
				y += 16;
			}
			if (y + height > Utility.getSafeArea().Bottom)
			{
				x += 16;
				if (x + width > Utility.getSafeArea().Right)
				{
					x = Utility.getSafeArea().Right - width;
				}
				y = Utility.getSafeArea().Bottom - height;
			}
			width += 4;
			int boxWidth = (boxWidthOverride != -1) ? boxWidthOverride : (width + ((craftingIngredients != null) ? 21 : 0));
			int boxHeight = (boxHeightOverride != -1) ? boxHeightOverride : height;
			IClickableMenu.drawTextureBox(b, boxTexture, boxSourceRect.Value, x, y, boxWidth, boxHeight, Color.White * alpha, boxScale, true, -1f);
			if (boldTitleText != null)
			{
				Vector2 bold_text_size = Game1.dialogueFont.MeasureString(boldTitleText);
				IClickableMenu.drawTextureBox(b, boxTexture, boxSourceRect.Value, x, y, width + ((craftingIngredients != null) ? 21 : 0), (int)Game1.dialogueFont.MeasureString(boldTitleText).Y + 32 + (int)((hoveredItem != null && categoryName.Length > 0) ? font.MeasureString("asd").Y : 0f) - 4, Color.White * alpha, 1f, false, -1f);
				b.Draw(Game1.menuTexture, new Rectangle(x + 12, y + (int)Game1.dialogueFont.MeasureString(boldTitleText).Y + 32 + (int)((hoveredItem != null && categoryName.Length > 0) ? font.MeasureString("asd").Y : 0f) - 4, width - 4 * ((craftingIngredients == null) ? 6 : 1), 4), new Rectangle?(new Rectangle(44, 300, 4, 4)), Color.White);
				b.DrawString(Game1.dialogueFont, boldTitleText, new Vector2((float)(x + 16), (float)(y + 16 + 4)) + new Vector2(2f, 2f), textShadowColor.Value);
				b.DrawString(Game1.dialogueFont, boldTitleText, new Vector2((float)(x + 16), (float)(y + 16 + 4)) + new Vector2(0f, 2f), textShadowColor.Value);
				b.DrawString(Game1.dialogueFont, boldTitleText, new Vector2((float)(x + 16), (float)(y + 16 + 4)), textColor.Value);
				if (bold_title_subtext != null)
				{
					Utility.drawTextWithShadow(b, bold_title_subtext, Game1.smallFont, new Vector2((float)(x + 16) + bold_text_size.X, (float)((int)((float)(y + 16 + 4) + bold_text_size.Y / 2f - small_text_size.Y / 2f))), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
				}
				y += (int)Game1.dialogueFont.MeasureString(boldTitleText).Y;
			}
			if (hoveredItem != null && categoryName.Length > 0)
			{
				y -= 4;
				Utility.drawTextWithShadow(b, categoryName, font, new Vector2((float)(x + 16), (float)(y + 16 + 4)), hoveredItem.getCategoryColor(), 1f, -1f, 2, 2, 1f, 3);
				y += (int)font.MeasureString("T").Y + ((boldTitleText != null) ? 16 : 0) + 4;
				Tool tool = hoveredItem as Tool;
				if (tool != null && tool.GetTotalForgeLevels(false) > 0)
				{
					string forged_string = Game1.content.LoadString("Strings\\UI:Item_Tooltip_Forged");
					Utility.drawTextWithShadow(b, forged_string, font, new Vector2((float)(x + 16), (float)(y + 16 + 4)), Color.DarkRed, 1f, -1f, 2, 2, 1f, 3);
					int forges = tool.GetTotalForgeLevels(false);
					if (forges < tool.GetMaxForges() && !tool.hasEnchantmentOfType<DiamondEnchantment>())
					{
						Utility.drawTextWithShadow(b, string.Concat(new string[]
						{
							" (",
							forges.ToString(),
							"/",
							tool.GetMaxForges().ToString(),
							")"
						}), font, new Vector2((float)(x + 16) + font.MeasureString(forged_string).X, (float)(y + 16 + 4)), Color.DimGray, 1f, -1f, 2, 2, 1f, 3);
					}
					y += (int)font.MeasureString("T").Y;
				}
				MeleeWeapon weapon2 = hoveredItem as MeleeWeapon;
				if (weapon2 != null && weapon2.GetEnchantmentLevel<GalaxySoulEnchantment>() > 0)
				{
					GalaxySoulEnchantment enchantment = weapon2.GetEnchantmentOfType<GalaxySoulEnchantment>();
					string forged_string2 = Game1.content.LoadString("Strings\\UI:Item_Tooltip_GalaxyForged");
					Utility.drawTextWithShadow(b, forged_string2, font, new Vector2((float)(x + 16), (float)(y + 16 + 4)), Color.DarkRed, 1f, -1f, 2, 2, 1f, 3);
					int level = enchantment.GetLevel();
					if (level < enchantment.GetMaximumLevel())
					{
						Utility.drawTextWithShadow(b, string.Concat(new string[]
						{
							" (",
							level.ToString(),
							"/",
							enchantment.GetMaximumLevel().ToString(),
							")"
						}), font, new Vector2((float)(x + 16) + font.MeasureString(forged_string2).X, (float)(y + 16 + 4)), Color.DimGray, 1f, -1f, 2, 2, 1f, 3);
					}
					y += (int)font.MeasureString("T").Y;
				}
			}
			else
			{
				y += ((boldTitleText != null) ? 16 : 0);
			}
			if (hoveredItem != null && craftingIngredients == null)
			{
				hoveredItem.drawTooltip(b, ref x, ref y, font, alpha, text);
			}
			else if (text != null && text.Length != 0 && (text.Length != 1 || text[0] != ' ') && (craftingIngredients == null || hoveredItem == null || !hoveredItem.getDescription().Equals(text.ToString())))
			{
				if (text.ToString().Contains("[line]"))
				{
					string[] textSplit = text.ToString().Split("[line]", StringSplitOptions.None);
					b.DrawString(font, textSplit[0], new Vector2((float)(x + 16), (float)(y + 16 + 4)) + new Vector2(2f, 2f), textShadowColor.Value * alpha);
					b.DrawString(font, textSplit[0], new Vector2((float)(x + 16), (float)(y + 16 + 4)) + new Vector2(0f, 2f), textShadowColor.Value * alpha);
					b.DrawString(font, textSplit[0], new Vector2((float)(x + 16), (float)(y + 16 + 4)) + new Vector2(2f, 0f), textShadowColor.Value * alpha);
					b.DrawString(font, textSplit[0], new Vector2((float)(x + 16), (float)(y + 16 + 4)), textColor.Value * 0.9f * alpha);
					y += (int)font.MeasureString(textSplit[0]).Y - 16;
					Utility.drawLineWithScreenCoordinates(x + 16 - 4, y + 16 + 4, x + 16 + width - 28, y + 16 + 4, b, textShadowColor.Value, 1f, 1);
					Utility.drawLineWithScreenCoordinates(x + 16 - 4, y + 16 + 5, x + 16 + width - 28, y + 16 + 5, b, textShadowColor.Value, 1f, 1);
					if (textSplit.Length > 1)
					{
						y -= 16;
						b.DrawString(font, textSplit[1], new Vector2((float)(x + 16), (float)(y + 16 + 4)) + new Vector2(2f, 2f), textShadowColor.Value * alpha);
						b.DrawString(font, textSplit[1], new Vector2((float)(x + 16), (float)(y + 16 + 4)) + new Vector2(0f, 2f), textShadowColor.Value * alpha);
						b.DrawString(font, textSplit[1], new Vector2((float)(x + 16), (float)(y + 16 + 4)) + new Vector2(2f, 0f), textShadowColor.Value * alpha);
						b.DrawString(font, textSplit[1], new Vector2((float)(x + 16), (float)(y + 16 + 4)), textColor.Value * 0.9f * alpha);
						y += (int)font.MeasureString(textSplit[1]).Y;
					}
					y += 4;
				}
				else
				{
					b.DrawString(font, text, new Vector2((float)(x + 16), (float)(y + 16 + 4)) + new Vector2(2f, 2f), textShadowColor.Value * alpha);
					b.DrawString(font, text, new Vector2((float)(x + 16), (float)(y + 16 + 4)) + new Vector2(0f, 2f), textShadowColor.Value * alpha);
					b.DrawString(font, text, new Vector2((float)(x + 16), (float)(y + 16 + 4)) + new Vector2(2f, 0f), textShadowColor.Value * alpha);
					b.DrawString(font, text, new Vector2((float)(x + 16), (float)(y + 16 + 4)), textColor.Value * 0.9f * alpha);
					y += (int)font.MeasureString(text).Y + 4;
				}
			}
			if (craftingIngredients != null)
			{
				craftingIngredients.drawRecipeDescription(b, new Vector2((float)(x + 16), (float)(y - 8)), width, additional_craft_materials);
				y += craftingIngredients.getDescriptionHeight(width - 8);
			}
			if (healAmountToDisplay != -1)
			{
				int stamina_recovery = (hoveredItem as Object).staminaRecoveredOnConsumption();
				if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.zh)
				{
					y += 8;
				}
				if (stamina_recovery >= 0)
				{
					int health_recovery = (hoveredItem as Object).healthRecoveredOnConsumption();
					if (stamina_recovery > 0)
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(x + 16 + 4), (float)(y + 16)), new Rectangle(0, 428, 10, 10), Color.White, 0f, Vector2.Zero, 3f, false, 0.95f, -1, -1, 0.35f);
						Utility.drawTextWithShadow(b, (stamina_recovery >= 999) ? " 100%" : Game1.content.LoadString("Strings\\UI:ItemHover_Energy", "+" + stamina_recovery.ToString()), font, new Vector2((float)(x + 16 + 34 + 4), (float)(y + 16)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
						y += 34;
					}
					if (health_recovery > 0)
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(x + 16 + 4), (float)(y + 16)), new Rectangle(0, 438, 10, 10), Color.White, 0f, Vector2.Zero, 3f, false, 0.95f, -1, -1, 0.35f);
						Utility.drawTextWithShadow(b, (health_recovery >= 999) ? " 100%" : Game1.content.LoadString("Strings\\UI:ItemHover_Health", "+" + health_recovery.ToString()), font, new Vector2((float)(x + 16 + 34 + 4), (float)(y + 16)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
						y += 34;
					}
				}
				else if (stamina_recovery != -300)
				{
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(x + 16 + 4), (float)(y + 16)), new Rectangle(140, 428, 10, 10), Color.White, 0f, Vector2.Zero, 3f, false, 0.95f, -1, -1, 0.35f);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:ItemHover_Energy", stamina_recovery.ToString() ?? ""), font, new Vector2((float)(x + 16 + 34 + 4), (float)(y + 16)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
					y += 34;
				}
			}
			if (buffIconsToDisplay != null)
			{
				y += 16;
				b.Draw(Game1.staminaRect, new Rectangle(x + 12, y + 6, width - ((craftingIngredients != null) ? 4 : 24), 2), new Color(207, 147, 103) * 0.8f);
				for (int j = 0; j < buffIconsToDisplay.Length; j++)
				{
					if (!buffIconsToDisplay[j].Equals("0") && buffIconsToDisplay[j] != "")
					{
						if (j == 12)
						{
							Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(x + 16 + 4), (float)(y + 16)), new Rectangle(410, 501, 9, 9), Color.White, 0f, Vector2.Zero, 3f, false, 0.95f, -1, -1, 0.35f);
							Utility.drawTextWithShadow(b, buffIconsToDisplay[j], font, new Vector2((float)(x + 16 + 34 + 4), (float)(y + 16)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
						}
						else
						{
							Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(x + 16 + 4), (float)(y + 16)), new Rectangle(10 + j * 10, 428, 10, 10), Color.White, 0f, Vector2.Zero, 3f, false, 0.95f, -1, -1, 0.35f);
							string buffName = ((Convert.ToDouble(buffIconsToDisplay[j]) > 0.0) ? "+" : "") + buffIconsToDisplay[j];
							if (j <= 11)
							{
								buffName = Game1.content.LoadString("Strings\\UI:ItemHover_Buff" + j.ToString(), buffName);
							}
							Utility.drawTextWithShadow(b, buffName, font, new Vector2((float)(x + 16 + 34 + 4), (float)(y + 16)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
						}
						y += 39;
					}
				}
				y -= 8;
			}
			if (hoveredItem != null && hoveredItem.attachmentSlots() > 0)
			{
				hoveredItem.drawAttachments(b, x + 16, y + 16);
				if (moneyAmountToDisplayAtBottom > -1)
				{
					y += 68 * hoveredItem.attachmentSlots();
				}
			}
			if (moneyAmountToDisplayAtBottom > -1)
			{
				b.Draw(Game1.staminaRect, new Rectangle(x + 12, y + 22 - ((healAmountToDisplay <= 0) ? 6 : 0), width - ((craftingIngredients != null) ? 4 : 24), 2), new Color(207, 147, 103) * 0.5f);
				string moneyStr = moneyAmountToDisplayAtBottom.ToString();
				int extraY = 0;
				if ((buffIconsToDisplay != null && buffIconsToDisplay.Length > 1) || healAmountToDisplay > 0 || craftingIngredients != null)
				{
					extraY = 8;
				}
				b.DrawString(font, moneyStr, new Vector2((float)(x + 16), (float)(y + 16 + 4 + extraY)) + new Vector2(2f, 2f), textShadowColor.Value);
				b.DrawString(font, moneyStr, new Vector2((float)(x + 16), (float)(y + 16 + 4 + extraY)) + new Vector2(0f, 2f), textShadowColor.Value);
				b.DrawString(font, moneyStr, new Vector2((float)(x + 16), (float)(y + 16 + 4 + extraY)) + new Vector2(2f, 0f), textShadowColor.Value);
				b.DrawString(font, moneyStr, new Vector2((float)(x + 16), (float)(y + 16 + 4 + extraY)), textColor.Value);
				switch (currencySymbol)
				{
				case 0:
					b.Draw(Game1.debrisSpriteSheet, new Vector2((float)(x + 16) + font.MeasureString(moneyStr).X + 20f, (float)(y + 16 + 20 + extraY)), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, 8, 16, 16)), Color.White, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, 0.95f);
					break;
				case 1:
					b.Draw(Game1.mouseCursors, new Vector2((float)(x + 8) + font.MeasureString(moneyStr).X + 20f, (float)(y + 16 - 5 + extraY)), new Rectangle?(new Rectangle(338, 400, 8, 8)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					break;
				case 2:
					b.Draw(Game1.mouseCursors, new Vector2((float)(x + 8) + font.MeasureString(moneyStr).X + 20f, (float)(y + 16 - 7 + extraY)), new Rectangle?(new Rectangle(211, 373, 9, 10)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					break;
				case 4:
					b.Draw(Game1.objectSpriteSheet, new Vector2((float)(x + 8) + font.MeasureString(moneyStr).X + 20f, (float)(y + 16 - 7 + extraY)), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 858, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					break;
				}
				y += 48;
				if (extraItemToShowIndex != null)
				{
					y += extraY;
				}
			}
			if (extraItemToShowIndex != null)
			{
				if (moneyAmountToDisplayAtBottom == -1)
				{
					y += 8;
				}
				ParsedItemData dataOrErrorItem2 = ItemRegistry.GetDataOrErrorItem(extraItemToShowIndex);
				string displayName = dataOrErrorItem2.DisplayName;
				Texture2D texture = dataOrErrorItem2.GetTexture();
				Rectangle sourceRect = dataOrErrorItem2.GetSourceRect(0, null);
				string requirement2 = Game1.content.LoadString("Strings\\UI:ItemHover_Requirements", extraItemToShowAmount, displayName);
				float minimum_box_height = Math.Max(font.MeasureString(requirement2).Y + 21f, 96f);
				IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y + 4, width + ((craftingIngredients != null) ? 21 : 0), (int)minimum_box_height, Color.White, 1f, true, -1f);
				y += 20;
				b.DrawString(font, requirement2, new Vector2((float)(x + 16), (float)(y + 4)) + new Vector2(2f, 2f), textShadowColor.Value);
				b.DrawString(font, requirement2, new Vector2((float)(x + 16), (float)(y + 4)) + new Vector2(0f, 2f), textShadowColor.Value);
				b.DrawString(font, requirement2, new Vector2((float)(x + 16), (float)(y + 4)) + new Vector2(2f, 0f), textShadowColor.Value);
				b.DrawString(Game1.smallFont, requirement2, new Vector2((float)(x + 16), (float)(y + 4)), textColor.Value);
				b.Draw(texture, new Vector2((float)(x + 16 + (int)font.MeasureString(requirement2).X + 21), (float)y), new Rectangle?(sourceRect), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			}
			if (craftingIngredients != null && Game1.options.showAdvancedCraftingInformation)
			{
				Utility.drawTextWithShadow(b, craftingIngredients.getCraftCountText(), font, new Vector2((float)(x + 16), (float)(y + 16 + 4)), Game1.textColor, 1f, -1f, 2, 2, 1f, 3);
				y += (int)font.MeasureString("T").Y + 4;
			}
		}

		// Token: 0x04001B3A RID: 6970
		protected IClickableMenu _childMenu;

		// Token: 0x04001B3B RID: 6971
		protected IClickableMenu _parentMenu;

		// Token: 0x04001B3C RID: 6972
		public const int upperRightCloseButton_ID = 9175502;

		// Token: 0x04001B3D RID: 6973
		public const int currency_g = 0;

		// Token: 0x04001B3E RID: 6974
		public const int currency_starTokens = 1;

		// Token: 0x04001B3F RID: 6975
		public const int currency_qiCoins = 2;

		// Token: 0x04001B40 RID: 6976
		public const int currency_qiGems = 4;

		// Token: 0x04001B41 RID: 6977
		public const int greyedOutSpotIndex = 57;

		// Token: 0x04001B42 RID: 6978
		public const int presentIconIndex = 58;

		// Token: 0x04001B43 RID: 6979
		public const int itemSpotIndex = 10;

		// Token: 0x04001B44 RID: 6980
		protected string closeSound = "bigDeSelect";

		// Token: 0x04001B45 RID: 6981
		public static int borderWidth = 40;

		// Token: 0x04001B46 RID: 6982
		public static int tabYPositionRelativeToMenuY = -48;

		// Token: 0x04001B47 RID: 6983
		public static int spaceToClearTopBorder = 96;

		// Token: 0x04001B48 RID: 6984
		public static int spaceToClearSideBorder = 16;

		// Token: 0x04001B49 RID: 6985
		public const int spaceBetweenTabs = 4;

		// Token: 0x04001B4A RID: 6986
		public int xPositionOnScreen;

		// Token: 0x04001B4B RID: 6987
		public int yPositionOnScreen;

		// Token: 0x04001B4C RID: 6988
		public int width;

		// Token: 0x04001B4D RID: 6989
		public int height;

		// Token: 0x04001B4E RID: 6990
		public Action<IClickableMenu> behaviorBeforeCleanup;

		// Token: 0x04001B4F RID: 6991
		public IClickableMenu.onExit exitFunction;

		// Token: 0x04001B50 RID: 6992
		public ClickableTextureComponent upperRightCloseButton;

		// Token: 0x04001B51 RID: 6993
		public bool destroy;

		// Token: 0x04001B52 RID: 6994
		protected int _dependencies;

		// Token: 0x04001B53 RID: 6995
		public List<ClickableComponent> allClickableComponents;

		// Token: 0x04001B54 RID: 6996
		public ClickableComponent currentlySnappedComponent;

		// Token: 0x04001B55 RID: 6997
		public static StringBuilder HoverTextStringBuilder = new StringBuilder();

		// Token: 0x0200060D RID: 1549
		// (Invoke) Token: 0x06004409 RID: 17417
		public delegate void onExit();
	}
}

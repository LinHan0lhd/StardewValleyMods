using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Characters;

namespace StardewValley.Menus
{
	// Token: 0x0200024C RID: 588
	public class AnimalPage : IClickableMenu
	{
		// Token: 0x06002712 RID: 10002 RVA: 0x001BA0D8 File Offset: 0x001B82D8
		public AnimalPage(int x, int y, int width, int height) : base(x, y, width, height, false)
		{
		}

		// Token: 0x06002713 RID: 10003 RVA: 0x001BA107 File Offset: 0x001B8307
		public void init()
		{
			this.AnimalEntries = this.FindAnimals();
			this.CreateComponents();
			this.slotPosition = 0;
			this.setScrollBarToCurrentIndex();
			this.updateSlots();
		}

		// Token: 0x06002714 RID: 10004 RVA: 0x001BA12E File Offset: 0x001B832E
		public override void populateClickableComponentList()
		{
			this.init();
			base.populateClickableComponentList();
		}

		// Token: 0x06002715 RID: 10005 RVA: 0x001BA13C File Offset: 0x001B833C
		public List<AnimalPage.AnimalEntry> FindAnimals()
		{
			List<AnimalPage.AnimalEntry> pets = new List<AnimalPage.AnimalEntry>();
			List<AnimalPage.AnimalEntry> farmAnimals = new List<AnimalPage.AnimalEntry>();
			List<AnimalPage.AnimalEntry> horses = new List<AnimalPage.AnimalEntry>();
			foreach (Character animal in this.GetAllAnimals())
			{
				if (!(animal is Pet))
				{
					if (!(animal is Horse))
					{
						farmAnimals.Add(new AnimalPage.AnimalEntry(animal));
					}
					else
					{
						horses.Add(new AnimalPage.AnimalEntry(animal));
					}
				}
				else
				{
					pets.Add(new AnimalPage.AnimalEntry(animal));
				}
			}
			foreach (Farmer f in Game1.getAllFarmers())
			{
				if (f.mount != null)
				{
					horses.Add(new AnimalPage.AnimalEntry(f.mount));
				}
			}
			List<AnimalPage.AnimalEntry> list = new List<AnimalPage.AnimalEntry>();
			list.AddRange(pets);
			list.AddRange(horses);
			list.AddRange(from entry in farmAnimals
			orderby entry.AnimalBaseType, entry.AnimalType, entry.FriendshipLevel descending
			select entry);
			return list;
		}

		// Token: 0x06002716 RID: 10006 RVA: 0x001BA2B0 File Offset: 0x001B84B0
		public IEnumerable<Character> GetAllAnimals()
		{
			List<Character> animals = new List<Character>();
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				foreach (NPC i in location.characters)
				{
					if ((i is Pet || i is Horse) && !i.hideFromAnimalSocialMenu.Value)
					{
						animals.Add(i);
					}
				}
				foreach (FarmAnimal animal in location.animals.Values)
				{
					if (!animal.hideFromAnimalSocialMenu.Value)
					{
						animals.Add(animal);
					}
				}
				return true;
			}, true, false);
			return animals;
		}

		// Token: 0x06002717 RID: 10007 RVA: 0x001BA2DC File Offset: 0x001B84DC
		public void CreateComponents()
		{
			this.sprites.Clear();
			this.characterSlots.Clear();
			for (int i = 0; i < this.AnimalEntries.Count; i++)
			{
				this.sprites.Add(this.CreateSpriteComponent(this.AnimalEntries[i], i));
				ClickableTextureComponent slot = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + IClickableMenu.borderWidth, 0, this.width - IClickableMenu.borderWidth * 2, this.rowPosition(1) - this.rowPosition(0)), null, new Rectangle(0, 0, 0, 0), 4f, false)
				{
					myID = i,
					downNeighborID = i + 1,
					upNeighborID = i - 1
				};
				if (slot.upNeighborID < 0)
				{
					slot.upNeighborID = 12342;
				}
				this.characterSlots.Add(slot);
			}
			this.upButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 16, this.yPositionOnScreen + 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f, false);
			this.downButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 16, this.yPositionOnScreen + this.height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f, false);
			this.scrollBar = new ClickableTextureComponent(new Rectangle(this.upButton.bounds.X + 12, this.upButton.bounds.Y + this.upButton.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f, false);
			this.scrollBarRunner = new Rectangle(this.scrollBar.bounds.X, this.upButton.bounds.Y + this.upButton.bounds.Height + 4, this.scrollBar.bounds.Width, this.height - 128 - this.upButton.bounds.Height - 8);
		}

		// Token: 0x06002718 RID: 10008 RVA: 0x001BA528 File Offset: 0x001B8728
		public ClickableTextureComponent CreateSpriteComponent(AnimalPage.AnimalEntry entry, int index)
		{
			Rectangle bounds = new Rectangle(this.xPositionOnScreen + IClickableMenu.borderWidth + 4, 0, this.width, 64);
			Rectangle sourceRect = entry.TextureSourceRect;
			if (sourceRect.Height <= 16)
			{
				bounds.Height--;
				bounds.X += 24;
			}
			return new ClickableTextureComponent(index.ToString(), bounds, null, "", entry.Texture, sourceRect, 4f, false);
		}

		// Token: 0x06002719 RID: 10009 RVA: 0x001BA59D File Offset: 0x001B879D
		public AnimalPage.AnimalEntry GetSocialEntry(int index)
		{
			if (index < 0 || index >= this.AnimalEntries.Count)
			{
				index = 0;
			}
			if (this.AnimalEntries.Count == 0)
			{
				return null;
			}
			return this.AnimalEntries[index];
		}

		// Token: 0x0600271A RID: 10010 RVA: 0x001BA5CF File Offset: 0x001B87CF
		public override void snapToDefaultClickableComponent()
		{
			if (this.slotPosition < this.characterSlots.Count)
			{
				this.currentlySnappedComponent = this.characterSlots[this.slotPosition];
			}
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x0600271B RID: 10011 RVA: 0x001BA604 File Offset: 0x001B8804
		public void updateSlots()
		{
			for (int i = 0; i < this.characterSlots.Count; i++)
			{
				this.characterSlots[i].bounds.Y = this.rowPosition(i - 1);
			}
			int index = 0;
			for (int j = this.slotPosition; j < this.slotPosition + 5; j++)
			{
				if (this.slotPosition >= 0 && this.sprites.Count > j)
				{
					int y = this.yPositionOnScreen + IClickableMenu.borderWidth + 32 + 112 * index + 16;
					if (this.sprites[j].bounds.Height < 64)
					{
						y += 48;
					}
					this.sprites[j].bounds.Y = y;
				}
				index++;
			}
			base.populateClickableComponentList();
			this.addTabsToClickableComponents();
		}

		// Token: 0x0600271C RID: 10012 RVA: 0x001BA6D8 File Offset: 0x001B88D8
		public void addTabsToClickableComponents()
		{
			GameMenu gameMenu = Game1.activeClickableMenu as GameMenu;
			if (gameMenu != null && !this.allClickableComponents.Contains(gameMenu.tabs[0]))
			{
				this.allClickableComponents.AddRange(gameMenu.tabs);
			}
		}

		// Token: 0x0600271D RID: 10013 RVA: 0x001BA720 File Offset: 0x001B8920
		protected void _SelectSlot(AnimalPage.AnimalEntry entry)
		{
			bool found = false;
			for (int i = 0; i < this.AnimalEntries.Count; i++)
			{
				if (this.AnimalEntries[i].InternalName == entry.InternalName)
				{
					this._SelectSlot(this.characterSlots[i]);
					found = true;
					break;
				}
			}
			if (!found)
			{
				this._SelectSlot(this.characterSlots[0]);
			}
		}

		// Token: 0x0600271E RID: 10014 RVA: 0x001BA790 File Offset: 0x001B8990
		protected void _SelectSlot(ClickableComponent slot_component)
		{
			if (slot_component != null && this.characterSlots.Contains(slot_component))
			{
				int index = this.characterSlots.IndexOf(slot_component as ClickableTextureComponent);
				this.currentlySnappedComponent = slot_component;
				if (index < this.slotPosition)
				{
					this.slotPosition = index;
				}
				else if (index >= this.slotPosition + 5)
				{
					this.slotPosition = index - 5 + 1;
				}
				this.setScrollBarToCurrentIndex();
				this.updateSlots();
				if (Game1.options.snappyMenus && Game1.options.gamepadControls)
				{
					this.snapCursorToCurrentSnappedComponent();
				}
			}
		}

		// Token: 0x0600271F RID: 10015 RVA: 0x001BA81C File Offset: 0x001B8A1C
		public void ConstrainSelectionToVisibleSlots()
		{
			if (this.characterSlots.Contains(this.currentlySnappedComponent))
			{
				int index = this.characterSlots.IndexOf(this.currentlySnappedComponent as ClickableTextureComponent);
				if (index < this.slotPosition)
				{
					index = this.slotPosition;
				}
				else if (index >= this.slotPosition + 5)
				{
					index = this.slotPosition + 5 - 1;
				}
				this.currentlySnappedComponent = this.characterSlots[index];
				if (Game1.options.snappyMenus && Game1.options.gamepadControls)
				{
					this.snapCursorToCurrentSnappedComponent();
				}
			}
		}

		// Token: 0x06002720 RID: 10016 RVA: 0x001BA8AC File Offset: 0x001B8AAC
		public override void snapCursorToCurrentSnappedComponent()
		{
			if (this.currentlySnappedComponent != null && this.characterSlots.Contains(this.currentlySnappedComponent))
			{
				Game1.setMousePosition(this.currentlySnappedComponent.bounds.Left + 64, this.currentlySnappedComponent.bounds.Center.Y);
				return;
			}
			base.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002721 RID: 10017 RVA: 0x001BA908 File Offset: 0x001B8B08
		public override void applyMovementKey(int direction)
		{
			base.applyMovementKey(direction);
			if (this.characterSlots.Contains(this.currentlySnappedComponent))
			{
				this._SelectSlot(this.currentlySnappedComponent);
			}
		}

		// Token: 0x06002722 RID: 10018 RVA: 0x001BA930 File Offset: 0x001B8B30
		public override void leftClickHeld(int x, int y)
		{
			base.leftClickHeld(x, y);
			if (this.scrolling)
			{
				int y2 = this.scrollBar.bounds.Y;
				this.scrollBar.bounds.Y = Math.Min(this.yPositionOnScreen + this.height - 64 - 12 - this.scrollBar.bounds.Height, Math.Max(y, this.yPositionOnScreen + this.upButton.bounds.Height + 20));
				float percentage = (float)(y - this.scrollBarRunner.Y) / (float)this.scrollBarRunner.Height;
				this.slotPosition = Math.Min(this.sprites.Count - 5, Math.Max(0, (int)((float)this.sprites.Count * percentage)));
				this.setScrollBarToCurrentIndex();
				if (y2 != this.scrollBar.bounds.Y)
				{
					Game1.playSound("shiny4", null);
				}
			}
		}

		// Token: 0x06002723 RID: 10019 RVA: 0x001BAA2E File Offset: 0x001B8C2E
		public override void releaseLeftClick(int x, int y)
		{
			base.releaseLeftClick(x, y);
			this.scrolling = false;
		}

		// Token: 0x06002724 RID: 10020 RVA: 0x001BAA40 File Offset: 0x001B8C40
		private void setScrollBarToCurrentIndex()
		{
			if (this.sprites.Count > 0)
			{
				this.scrollBar.bounds.Y = this.scrollBarRunner.Height / Math.Max(1, this.sprites.Count - 5 + 1) * this.slotPosition + this.upButton.bounds.Bottom + 4;
				if (this.slotPosition == this.sprites.Count - 5)
				{
					this.scrollBar.bounds.Y = this.downButton.bounds.Y - this.scrollBar.bounds.Height - 4;
				}
			}
			this.updateSlots();
		}

		// Token: 0x06002725 RID: 10021 RVA: 0x001BAAF8 File Offset: 0x001B8CF8
		public override void receiveScrollWheelAction(int direction)
		{
			base.receiveScrollWheelAction(direction);
			if (direction > 0 && this.slotPosition > 0)
			{
				this.upArrowPressed();
				this.ConstrainSelectionToVisibleSlots();
				Game1.playSound("shiny4", null);
				return;
			}
			if (direction < 0 && this.slotPosition < Math.Max(0, this.sprites.Count - 5))
			{
				this.downArrowPressed();
				this.ConstrainSelectionToVisibleSlots();
				Game1.playSound("shiny4", null);
			}
		}

		// Token: 0x06002726 RID: 10022 RVA: 0x001BAB79 File Offset: 0x001B8D79
		public void upArrowPressed()
		{
			this.slotPosition--;
			this.updateSlots();
			this.upButton.scale = 3.5f;
			this.setScrollBarToCurrentIndex();
		}

		// Token: 0x06002727 RID: 10023 RVA: 0x001BABA5 File Offset: 0x001B8DA5
		public void downArrowPressed()
		{
			this.slotPosition++;
			this.updateSlots();
			this.downButton.scale = 3.5f;
			this.setScrollBarToCurrentIndex();
		}

		// Token: 0x06002728 RID: 10024 RVA: 0x001BABD4 File Offset: 0x001B8DD4
		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (this.upButton.containsPoint(x, y) && this.slotPosition > 0)
			{
				this.upArrowPressed();
				Game1.playSound("shwip", null);
				return;
			}
			if (this.downButton.containsPoint(x, y) && this.slotPosition < this.sprites.Count - 5)
			{
				this.downArrowPressed();
				Game1.playSound("shwip", null);
				return;
			}
			if (this.scrollBar.containsPoint(x, y))
			{
				this.scrolling = true;
				return;
			}
			if (!this.downButton.containsPoint(x, y) && x > this.xPositionOnScreen + this.width && x < this.xPositionOnScreen + this.width + 128 && y > this.yPositionOnScreen && y < this.yPositionOnScreen + this.height)
			{
				this.scrolling = true;
				this.leftClickHeld(x, y);
				this.releaseLeftClick(x, y);
				return;
			}
			for (int i = 0; i < this.characterSlots.Count; i++)
			{
				if (i >= this.slotPosition)
				{
					int num = this.slotPosition + 5;
				}
			}
			this.slotPosition = Math.Max(0, Math.Min(this.sprites.Count - 5, this.slotPosition));
		}

		// Token: 0x06002729 RID: 10025 RVA: 0x001BAD1E File Offset: 0x001B8F1E
		public override void performHoverAction(int x, int y)
		{
			this.hoverText = "";
			this.upButton.tryHover(x, y, 0.1f);
			this.downButton.tryHover(x, y, 0.1f);
		}

		// Token: 0x0600272A RID: 10026 RVA: 0x001BAD4F File Offset: 0x001B8F4F
		private bool isCharacterSlotClickable(int i)
		{
			this.GetSocialEntry(i);
			return false;
		}

		// Token: 0x0600272B RID: 10027 RVA: 0x001BAD5C File Offset: 0x001B8F5C
		private void drawNPCSlot(SpriteBatch b, int i)
		{
			AnimalPage.AnimalEntry entry = this.GetSocialEntry(i);
			if (entry == null)
			{
				return;
			}
			if (i < 0)
			{
				return;
			}
			if (this.isCharacterSlotClickable(i) && this.characterSlots[i].bounds.Contains(Game1.getMouseX(), Game1.getMouseY()))
			{
				b.Draw(Game1.staminaRect, new Rectangle(this.xPositionOnScreen + IClickableMenu.borderWidth - 4, this.sprites[i].bounds.Y - 4, this.characterSlots[i].bounds.Width, this.characterSlots[i].bounds.Height - 12), Color.White * 0.25f);
			}
			this.sprites[i].draw(b);
			string internalName = entry.InternalName;
			int friendshipLevel = entry.FriendshipLevel;
			float lineHeight = Game1.smallFont.MeasureString("W").Y;
			float russianOffsetY = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? (-lineHeight / 2f) : 0f;
			int yOffset = (entry.TextureSourceRect.Height <= 16) ? -40 : 8;
			b.DrawString(Game1.dialogueFont, entry.DisplayName, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.borderWidth * 3 / 2 + 192 - 20 + 96 - (int)(Game1.dialogueFont.MeasureString(entry.DisplayName).X / 2f)), (float)(this.sprites[i].bounds.Y + 48 + yOffset) + russianOffsetY - 20f), Game1.textColor);
			if (entry.FriendshipLevel != -1)
			{
				double loveLevel = (double)((float)entry.FriendshipLevel / 1000f);
				int halfHeart = (int)((loveLevel * 1000.0 % 200.0 >= 100.0) ? (loveLevel * 1000.0 / 200.0) : -100.0);
				int heartYOffset = entry.ReceivedAnimalCracker ? -24 : 0;
				for (int hearts = 0; hearts < 5; hearts++)
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + 512 - 4 + hearts * 32), (float)(this.sprites[i].bounds.Y + heartYOffset + yOffset + 64 - 24)), new Rectangle?(new Rectangle(211 + ((loveLevel * 1000.0 <= (double)((hearts + 1) * 195)) ? 7 : 0), 428, 7, 6)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.89f);
					if (halfHeart == hearts)
					{
						b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + 512 - 4 + hearts * 32), (float)(this.sprites[i].bounds.Y + heartYOffset + yOffset + 64 - 24)), new Rectangle?(new Rectangle(211, 428, 4, 6)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.891f);
					}
				}
			}
			if (entry.WasPetYet != -1)
			{
				b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + 704 - 4), (float)(this.sprites[i].bounds.Y + yOffset + 64 - 52)), new Rectangle?(new Rectangle(32, 0, 10, 10)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
				b.Draw(Game1.mouseCursors_1_6, new Vector2((float)(this.xPositionOnScreen + 704 - 4), (float)(this.sprites[i].bounds.Y + yOffset + 64 - 8)), new Rectangle?(new Rectangle(273 + entry.WasPetYet * 9, 253, 9, 9)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
			}
			if (entry.special == 1)
			{
				Utility.drawWithShadow(b, Game1.objectSpriteSheet_2, new Vector2((float)(this.xPositionOnScreen + 704 - 16), (float)(this.sprites[i].bounds.Y + yOffset + 64 - 52)), new Rectangle(0, 160, 16, 16), Color.White, 0f, Vector2.Zero, 4f, false, 0.8f, 0, 8, 0.35f);
			}
			if (entry.ReceivedAnimalCracker)
			{
				Utility.drawWithShadow(b, Game1.objectSpriteSheet_2, new Vector2((float)(this.xPositionOnScreen + 576 - 20), (float)(this.sprites[i].bounds.Y + yOffset + 64 - 16)), new Rectangle(16, 242, 15, 11), Color.White, 0f, Vector2.Zero, 4f, false, 0.8f, -1, -1, 0.35f);
			}
		}

		// Token: 0x0600272C RID: 10028 RVA: 0x001BB278 File Offset: 0x001B9478
		private int rowPosition(int i)
		{
			int j = i - this.slotPosition;
			int rowHeight = 112;
			return this.yPositionOnScreen + IClickableMenu.borderWidth + 160 + 4 + j * rowHeight;
		}

		// Token: 0x0600272D RID: 10029 RVA: 0x001BB2AC File Offset: 0x001B94AC
		public override void draw(SpriteBatch b)
		{
			b.End();
			b.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled, null, null);
			if (this.sprites.Count > 0)
			{
				base.drawHorizontalPartition(b, this.yPositionOnScreen + IClickableMenu.borderWidth + 128 + 4, true, -1, -1, -1);
			}
			if (this.sprites.Count > 1)
			{
				base.drawHorizontalPartition(b, this.yPositionOnScreen + IClickableMenu.borderWidth + 192 + 32 + 20, true, -1, -1, -1);
			}
			if (this.sprites.Count > 2)
			{
				base.drawHorizontalPartition(b, this.yPositionOnScreen + IClickableMenu.borderWidth + 320 + 36, true, -1, -1, -1);
			}
			if (this.sprites.Count > 3)
			{
				base.drawHorizontalPartition(b, this.yPositionOnScreen + IClickableMenu.borderWidth + 384 + 32 + 52, true, -1, -1, -1);
			}
			int i = this.slotPosition;
			while (i < this.slotPosition + 5 && i < this.sprites.Count)
			{
				if (this.GetSocialEntry(i) != null)
				{
					this.drawNPCSlot(b, i);
				}
				i++;
			}
			Rectangle newClip = b.GraphicsDevice.ScissorRectangle;
			newClip.Y = Math.Max(0, this.rowPosition(4 - this.sprites.Count));
			newClip.Height -= newClip.Y;
			if (newClip.Height > 0)
			{
				int heightOverride = (this.sprites.Count >= 5) ? -1 : ((108 + this.sprites.Count) * this.sprites.Count);
				base.drawVerticalPartition(b, this.xPositionOnScreen + 448 + 12, true, -1, -1, -1, heightOverride);
				base.drawVerticalPartition(b, this.xPositionOnScreen + 256 + 12 + 376, true, -1, -1, -1, heightOverride);
			}
			this.upButton.draw(b);
			this.downButton.draw(b);
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.scrollBarRunner.X, this.scrollBarRunner.Y, this.scrollBarRunner.Width, this.scrollBarRunner.Height, Color.White, 4f, true, -1f);
			this.scrollBar.draw(b);
			if (!this.hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
			}
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
		}

		// Token: 0x04001834 RID: 6196
		public const int slotsOnPage = 5;

		// Token: 0x04001835 RID: 6197
		public string hoverText = "";

		// Token: 0x04001836 RID: 6198
		public ClickableTextureComponent upButton;

		// Token: 0x04001837 RID: 6199
		public ClickableTextureComponent downButton;

		// Token: 0x04001838 RID: 6200
		public ClickableTextureComponent scrollBar;

		// Token: 0x04001839 RID: 6201
		public Rectangle scrollBarRunner;

		// Token: 0x0400183A RID: 6202
		public List<AnimalPage.AnimalEntry> AnimalEntries;

		// Token: 0x0400183B RID: 6203
		public readonly List<ClickableTextureComponent> sprites = new List<ClickableTextureComponent>();

		// Token: 0x0400183C RID: 6204
		public int slotPosition;

		// Token: 0x0400183D RID: 6205
		public readonly List<ClickableTextureComponent> characterSlots = new List<ClickableTextureComponent>();

		// Token: 0x0400183E RID: 6206
		public bool scrolling;

		// Token: 0x020005E1 RID: 1505
		public class AnimalEntry
		{
			// Token: 0x0600434C RID: 17228 RVA: 0x00318E94 File Offset: 0x00317094
			public AnimalEntry(Character animal)
			{
				this.Animal = animal;
				this.DisplayName = animal.displayName;
				FarmAnimal farmAnimal = animal as FarmAnimal;
				if (farmAnimal != null)
				{
					NetLong myID = farmAnimal.myID;
					this.InternalName = (((myID != null) ? myID.ToString() : null) ?? "");
					this.FriendshipLevel = farmAnimal.friendshipTowardFarmer.Value;
					this.Texture = farmAnimal.Sprite.Texture;
					if (farmAnimal.Sprite.SourceRect.Height > 16)
					{
						if (farmAnimal.type.Equals("Ostrich"))
						{
							this.TextureSourceRect = new Rectangle(0, farmAnimal.Sprite.SourceRect.Height * 2 - 32, farmAnimal.Sprite.SourceRect.Width, 28);
						}
						else
						{
							this.TextureSourceRect = new Rectangle(0, farmAnimal.Sprite.SourceRect.Height * 2 - 28, farmAnimal.Sprite.SourceRect.Width, 28);
						}
					}
					else
					{
						this.TextureSourceRect = new Rectangle(0, 16, 16, 16);
					}
					this.AnimalType = farmAnimal.type.Value;
					if (this.AnimalType.Contains(' '))
					{
						this.AnimalBaseType = this.AnimalType.Split(' ', StringSplitOptions.None)[1];
					}
					else
					{
						this.AnimalBaseType = this.AnimalType;
					}
					this.WasPetYet = (farmAnimal.wasPet.Value ? 2 : ((farmAnimal.wasAutoPet.Value > false) ? 1 : 0));
					this.ReceivedAnimalCracker = farmAnimal.hasEatenAnimalCracker.Value;
					return;
				}
				Pet pet = animal as Pet;
				if (pet != null)
				{
					NetGuid petId = pet.petId;
					this.InternalName = (((petId != null) ? petId.ToString() : null) ?? "");
					this.FriendshipLevel = pet.friendshipTowardFarmer.Value;
					this.Texture = pet.Sprite.Texture;
					this.TextureSourceRect = new Rectangle(0, pet.Sprite.SourceRect.Height * 2 - 24, pet.Sprite.SourceRect.Width, 24);
					this.AnimalType = pet.petType.Value;
					this.WasPetYet = (pet.grantedFriendshipForPet.Value ? 2 : 0);
					return;
				}
				Horse horse = animal as Horse;
				if (horse == null)
				{
					return;
				}
				this.InternalName = horse.HorseId.ToString();
				this.Texture = horse.Sprite.Texture;
				this.TextureSourceRect = new Rectangle(0, horse.Sprite.SourceRect.Height * 2 - 26, horse.Sprite.SourceRect.Width, 24);
				this.AnimalType = "Horse";
				this.WasPetYet = -1;
				this.special = ((horse.ateCarrotToday > false) ? 1 : 0);
			}

			// Token: 0x04002DC2 RID: 11714
			public Character Animal;

			// Token: 0x04002DC3 RID: 11715
			public readonly string InternalName;

			// Token: 0x04002DC4 RID: 11716
			public readonly string DisplayName;

			// Token: 0x04002DC5 RID: 11717
			public readonly string AnimalType;

			// Token: 0x04002DC6 RID: 11718
			public readonly string AnimalBaseType;

			// Token: 0x04002DC7 RID: 11719
			public readonly int FriendshipLevel = -1;

			// Token: 0x04002DC8 RID: 11720
			public readonly bool ReceivedAnimalCracker;

			// Token: 0x04002DC9 RID: 11721
			public readonly int WasPetYet;

			// Token: 0x04002DCA RID: 11722
			public readonly int special;

			// Token: 0x04002DCB RID: 11723
			public Texture2D Texture;

			// Token: 0x04002DCC RID: 11724
			public Rectangle TextureSourceRect;
		}
	}
}

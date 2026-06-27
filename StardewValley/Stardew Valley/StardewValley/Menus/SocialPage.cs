using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Characters;
using StardewValley.GameData.Characters;
using StardewValley.Logging;

namespace StardewValley.Menus
{
	// Token: 0x020002AA RID: 682
	public class SocialPage : IClickableMenu
	{
		// Token: 0x06002C85 RID: 11397 RVA: 0x00225DC0 File Offset: 0x00223FC0
		public SocialPage(int x, int y, int width, int height) : base(x, y, width, height, false)
		{
			this.SocialEntries = this.FindSocialCharacters();
			this.numFarmers = this.SocialEntries.Count((SocialPage.SocialEntry p) => p.IsPlayer);
			this.CreateComponents();
			this.slotPosition = 0;
			for (int i = 0; i < this.SocialEntries.Count; i++)
			{
				if (!this.SocialEntries[i].IsPlayer)
				{
					this.slotPosition = i;
					break;
				}
			}
			this.setScrollBarToCurrentIndex();
			this.updateSlots();
		}

		// Token: 0x06002C86 RID: 11398 RVA: 0x00225E84 File Offset: 0x00224084
		public void postWindowSizeChange(IClickableMenu oldPage)
		{
			SocialPage oldCollectionsPage = oldPage as SocialPage;
			if (oldCollectionsPage != null)
			{
				this.slotPosition = oldCollectionsPage.slotPosition;
				this.setScrollBarToCurrentIndex();
			}
		}

		// Token: 0x06002C87 RID: 11399 RVA: 0x00225EB0 File Offset: 0x002240B0
		public List<SocialPage.SocialEntry> FindSocialCharacters()
		{
			List<SocialPage.SocialEntry> players = new List<SocialPage.SocialEntry>();
			Dictionary<string, SocialPage.SocialEntry> villagers = new Dictionary<string, SocialPage.SocialEntry>();
			List<SocialPage.SocialEntry> children = new List<SocialPage.SocialEntry>();
			foreach (NPC npc in this.GetAllNpcs())
			{
				Friendship friendship;
				if (!Game1.player.friendshipData.TryGetValue(npc.Name, out friendship))
				{
					friendship = null;
				}
				if (npc is Child)
				{
					children.Add(new SocialPage.SocialEntry(npc, friendship, null, npc.displayName));
				}
				else if (npc.CanSocialize)
				{
					CharacterData data = npc.GetData();
					string displayName = npc.displayName;
					SocialTabBehavior? socialTabBehavior = (data != null) ? new SocialTabBehavior?(data.SocialTab) : null;
					if (socialTabBehavior != null)
					{
						switch (socialTabBehavior.GetValueOrDefault())
						{
						case SocialTabBehavior.UnknownUntilMet:
							if (friendship == null)
							{
								displayName = "???";
							}
							break;
						case SocialTabBehavior.AlwaysShown:
							if (friendship == null)
							{
								Game1.player.friendshipData.Add(npc.Name, friendship = new Friendship());
							}
							break;
						case SocialTabBehavior.HiddenUntilMet:
							if (friendship == null)
							{
								continue;
							}
							break;
						case SocialTabBehavior.HiddenAlways:
							continue;
						}
					}
					villagers[npc.Name] = new SocialPage.SocialEntry(npc, friendship, data, displayName);
				}
			}
			int orderMet = 0;
			foreach (KeyValuePair<string, Friendship> pair in Game1.player.friendshipData.Pairs)
			{
				SocialPage.SocialEntry entry2;
				if (villagers.TryGetValue(pair.Key, out entry2))
				{
					entry2.OrderMet = new int?(orderMet++);
				}
			}
			foreach (Farmer player in Game1.getAllFarmers())
			{
				if (!player.IsLocalPlayer && (player.IsMainPlayer || player.isCustomized.Value) && !player.IsDedicatedPlayer)
				{
					Friendship friendship2 = Game1.player.team.GetFriendship(Game1.player.UniqueMultiplayerID, player.UniqueMultiplayerID);
					players.Add(new SocialPage.SocialEntry(player, friendship2));
				}
			}
			List<SocialPage.SocialEntry> list = new List<SocialPage.SocialEntry>();
			list.AddRange(players);
			list.AddRange(villagers.Values.OrderByDescending(delegate(SocialPage.SocialEntry entry)
			{
				Friendship friendship3 = entry.Friendship;
				if (friendship3 == null)
				{
					return null;
				}
				return new int?(friendship3.Points);
			}).ThenBy((SocialPage.SocialEntry entry) => entry.OrderMet).ThenBy((SocialPage.SocialEntry entry) => entry.DisplayName));
			list.AddRange(from p in children
			orderby p.DisplayName
			select p);
			return list;
		}

		// Token: 0x06002C88 RID: 11400 RVA: 0x002261E0 File Offset: 0x002243E0
		public IEnumerable<NPC> GetAllNpcs()
		{
			HashSet<string> nonSocial = new HashSet<string>();
			Dictionary<string, NPC> found = new Dictionary<string, NPC>();
			Utility.ForEachCharacter(delegate(NPC npc)
			{
				if (npc is Child)
				{
					found[npc.Name + "$$child"] = npc;
				}
				else if (npc.IsVillager)
				{
					NPC duplicate;
					if (!npc.CanSocialize)
					{
						nonSocial.Add(npc.Name);
					}
					else if (found.TryGetValue(npc.Name, out duplicate) && npc != duplicate)
					{
						bool showError = true;
						if (Game1.IsClient)
						{
							bool flag = duplicate.currentLocation.IsActiveLocation();
							bool newSynced = npc.currentLocation.IsActiveLocation();
							if (flag != newSynced)
							{
								if (newSynced)
								{
									found[npc.Name] = npc;
								}
								showError = false;
							}
						}
						if (showError)
						{
							IGameLogger log = Game1.log;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(107, 5);
							defaultInterpolatedStringHandler.AppendLiteral("The social page found conflicting NPCs with name ");
							defaultInterpolatedStringHandler.AppendFormatted(npc.Name);
							defaultInterpolatedStringHandler.AppendLiteral(" (one at ");
							GameLocation currentLocation2 = duplicate.currentLocation;
							defaultInterpolatedStringHandler.AppendFormatted((currentLocation2 != null) ? currentLocation2.NameOrUniqueName : null);
							defaultInterpolatedStringHandler.AppendLiteral(" ");
							defaultInterpolatedStringHandler.AppendFormatted<Point>(duplicate.TilePoint);
							defaultInterpolatedStringHandler.AppendLiteral(", the other at ");
							GameLocation currentLocation3 = npc.currentLocation;
							defaultInterpolatedStringHandler.AppendFormatted((currentLocation3 != null) ? currentLocation3.NameOrUniqueName : null);
							defaultInterpolatedStringHandler.AppendLiteral(" ");
							defaultInterpolatedStringHandler.AppendFormatted<Point>(npc.TilePoint);
							defaultInterpolatedStringHandler.AppendLiteral("); only the first will be shown.");
							log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
						}
					}
					else
					{
						found[npc.Name] = npc;
					}
				}
				return true;
			}, false);
			GameLocation currentLocation = Game1.currentLocation;
			Event @event = (currentLocation != null) ? currentLocation.currentEvent : null;
			if (@event != null)
			{
				foreach (NPC actor in @event.actors)
				{
					if (actor.IsVillager && actor.CanSocialize)
					{
						found[actor.Name] = actor;
					}
				}
			}
			foreach (string name in Game1.player.friendshipData.Keys)
			{
				CharacterData characterData;
				if (!nonSocial.Contains(name) && !found.ContainsKey(name) && NPC.TryGetData(name, out characterData))
				{
					string textureName = NPC.getTextureNameForCharacter(name);
					string spriteAssetName = "Characters\\" + textureName;
					string portraitAssetName = "Portraits\\" + textureName;
					if (Game1.content.DoesAssetExist<Texture2D>(spriteAssetName) && Game1.content.DoesAssetExist<Texture2D>(portraitAssetName))
					{
						try
						{
							AnimatedSprite sprite = new AnimatedSprite(spriteAssetName, 0, 16, 32);
							Texture2D portraits = Game1.content.Load<Texture2D>(portraitAssetName);
							found[name] = new NPC(sprite, Vector2.Zero, "Town", 0, name, portraits, false);
						}
						catch
						{
						}
					}
				}
			}
			return found.Values;
		}

		// Token: 0x06002C89 RID: 11401 RVA: 0x002263B4 File Offset: 0x002245B4
		public void CreateComponents()
		{
			this.sprites.Clear();
			this.characterSlots.Clear();
			for (int i = 0; i < this.SocialEntries.Count; i++)
			{
				this.sprites.Add(this.CreateSpriteComponent(this.SocialEntries[i], i));
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

		// Token: 0x06002C8A RID: 11402 RVA: 0x00226600 File Offset: 0x00224800
		public ClickableTextureComponent CreateSpriteComponent(SocialPage.SocialEntry entry, int index)
		{
			Rectangle bounds = new Rectangle(this.xPositionOnScreen + IClickableMenu.borderWidth + 4, 0, this.width, 64);
			Rectangle rectangle;
			if (!entry.IsPlayer)
			{
				NPC npc = entry.Character as NPC;
				if (npc != null)
				{
					rectangle = npc.getMugShotSourceRect();
					goto IL_42;
				}
			}
			rectangle = Rectangle.Empty;
			IL_42:
			Rectangle sourceRect = rectangle;
			return new ClickableTextureComponent(index.ToString(), bounds, null, "", entry.Character.Sprite.Texture, sourceRect, 4f, false);
		}

		// Token: 0x06002C8B RID: 11403 RVA: 0x0022667A File Offset: 0x0022487A
		public SocialPage.SocialEntry GetSocialEntry(int index)
		{
			if (index < 0 || index >= this.SocialEntries.Count)
			{
				index = 0;
			}
			return this.SocialEntries[index];
		}

		// Token: 0x06002C8C RID: 11404 RVA: 0x0022669D File Offset: 0x0022489D
		public override void snapToDefaultClickableComponent()
		{
			if (this.slotPosition < this.characterSlots.Count)
			{
				this.currentlySnappedComponent = this.characterSlots[this.slotPosition];
			}
			this.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002C8D RID: 11405 RVA: 0x002266D0 File Offset: 0x002248D0
		public void updateSlots()
		{
			for (int i = 0; i < this.characterSlots.Count; i++)
			{
				this.characterSlots[i].bounds.Y = this.rowPosition(i - 1);
			}
			int index = 0;
			for (int j = this.slotPosition; j < this.slotPosition + 5; j++)
			{
				if (this.sprites.Count > j)
				{
					int y = this.yPositionOnScreen + IClickableMenu.borderWidth + 32 + 112 * index + 32;
					this.sprites[j].bounds.Y = y;
				}
				index++;
			}
			this.populateClickableComponentList();
			this.addTabsToClickableComponents();
		}

		// Token: 0x06002C8E RID: 11406 RVA: 0x0022677C File Offset: 0x0022497C
		public void addTabsToClickableComponents()
		{
			GameMenu gameMenu = Game1.activeClickableMenu as GameMenu;
			if (gameMenu != null && !this.allClickableComponents.Contains(gameMenu.tabs[0]))
			{
				this.allClickableComponents.AddRange(gameMenu.tabs);
			}
		}

		// Token: 0x06002C8F RID: 11407 RVA: 0x002267C4 File Offset: 0x002249C4
		protected void _SelectSlot(SocialPage.SocialEntry entry)
		{
			bool found = false;
			for (int i = 0; i < this.SocialEntries.Count; i++)
			{
				SocialPage.SocialEntry cur = this.SocialEntries[i];
				if (cur.InternalName == entry.InternalName && cur.IsPlayer == entry.IsPlayer && cur.IsChild == entry.IsChild)
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

		// Token: 0x06002C90 RID: 11408 RVA: 0x00226850 File Offset: 0x00224A50
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

		// Token: 0x06002C91 RID: 11409 RVA: 0x002268DC File Offset: 0x00224ADC
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

		// Token: 0x06002C92 RID: 11410 RVA: 0x0022696C File Offset: 0x00224B6C
		public override void snapCursorToCurrentSnappedComponent()
		{
			if (this.currentlySnappedComponent != null && this.characterSlots.Contains(this.currentlySnappedComponent))
			{
				Game1.setMousePosition(this.currentlySnappedComponent.bounds.Left + 64, this.currentlySnappedComponent.bounds.Center.Y);
				return;
			}
			base.snapCursorToCurrentSnappedComponent();
		}

		// Token: 0x06002C93 RID: 11411 RVA: 0x002269C8 File Offset: 0x00224BC8
		public override void applyMovementKey(int direction)
		{
			base.applyMovementKey(direction);
			if (this.characterSlots.Contains(this.currentlySnappedComponent))
			{
				this._SelectSlot(this.currentlySnappedComponent);
			}
		}

		// Token: 0x06002C94 RID: 11412 RVA: 0x002269F0 File Offset: 0x00224BF0
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

		// Token: 0x06002C95 RID: 11413 RVA: 0x00226AEE File Offset: 0x00224CEE
		public override void releaseLeftClick(int x, int y)
		{
			base.releaseLeftClick(x, y);
			this.scrolling = false;
		}

		// Token: 0x06002C96 RID: 11414 RVA: 0x00226B00 File Offset: 0x00224D00
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

		// Token: 0x06002C97 RID: 11415 RVA: 0x00226BB8 File Offset: 0x00224DB8
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

		// Token: 0x06002C98 RID: 11416 RVA: 0x00226C39 File Offset: 0x00224E39
		public void upArrowPressed()
		{
			this.slotPosition--;
			this.updateSlots();
			this.upButton.scale = 3.5f;
			this.setScrollBarToCurrentIndex();
		}

		// Token: 0x06002C99 RID: 11417 RVA: 0x00226C65 File Offset: 0x00224E65
		public void downArrowPressed()
		{
			this.slotPosition++;
			this.updateSlots();
			this.downButton.scale = 3.5f;
			this.setScrollBarToCurrentIndex();
		}

		// Token: 0x06002C9A RID: 11418 RVA: 0x00226C94 File Offset: 0x00224E94
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
				if (i >= this.slotPosition && i < this.slotPosition + 5 && this.characterSlots[i].bounds.Contains(x, y))
				{
					SocialPage.SocialEntry entry = this.GetSocialEntry(i);
					if (!entry.IsPlayer && !entry.IsChild)
					{
						Character character = entry.Character;
						if (Game1.player.friendshipData.ContainsKey(character.name.Value))
						{
							Game1.playSound("bigSelect", null);
							int cached_slot_position = this.slotPosition;
							ProfileMenu profileMenu = new ProfileMenu(entry, this.SocialEntries);
							profileMenu.exitFunction = delegate()
							{
								SocialPage socialPage = (Game1.activeClickableMenu = new GameMenu(GameMenu.socialTab, -1, false)).GetCurrentPage() as SocialPage;
								if (socialPage != null)
								{
									socialPage.slotPosition = cached_slot_position;
									socialPage._SelectSlot(profileMenu.Current);
								}
							};
							Game1.activeClickableMenu = profileMenu;
							if (Game1.options.SnappyMenus)
							{
								profileMenu.snapToDefaultClickableComponent();
							}
							return;
						}
					}
					Game1.playSound("shiny4", null);
					break;
				}
			}
			this.slotPosition = Math.Max(0, Math.Min(this.sprites.Count - 5, this.slotPosition));
		}

		// Token: 0x06002C9B RID: 11419 RVA: 0x00226ED7 File Offset: 0x002250D7
		public override void performHoverAction(int x, int y)
		{
			this.hoverText = "";
			this.upButton.tryHover(x, y, 0.1f);
			this.downButton.tryHover(x, y, 0.1f);
		}

		// Token: 0x06002C9C RID: 11420 RVA: 0x00226F08 File Offset: 0x00225108
		public bool isCharacterSlotClickable(int i)
		{
			SocialPage.SocialEntry entry = this.GetSocialEntry(i);
			return entry != null && !entry.IsPlayer && !entry.IsChild && entry.IsMet;
		}

		// Token: 0x06002C9D RID: 11421 RVA: 0x00226F38 File Offset: 0x00225138
		public void drawNPCSlot(SpriteBatch b, int i)
		{
			SocialPage.SocialEntry entry = this.GetSocialEntry(i);
			if (entry == null)
			{
				return;
			}
			if (this.isCharacterSlotClickable(i) && this.characterSlots[i].bounds.Contains(Game1.getMouseX(), Game1.getMouseY()))
			{
				b.Draw(Game1.staminaRect, new Rectangle(this.xPositionOnScreen + IClickableMenu.borderWidth - 4, this.sprites[i].bounds.Y - 4, this.characterSlots[i].bounds.Width, this.characterSlots[i].bounds.Height - 12), Color.White * 0.25f);
			}
			this.sprites[i].draw(b);
			string name = entry.InternalName;
			Gender gender = entry.Gender;
			bool datable = entry.IsDatable;
			bool isDating = entry.IsDatingCurrentPlayer();
			bool isCurrentSpouse = entry.IsMarriedToCurrentPlayer();
			bool housemate = entry.IsRoommateForCurrentPlayer();
			float lineHeight = Game1.smallFont.MeasureString("W").Y;
			float russianOffsetY = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? (-lineHeight / 2f) : 0f;
			b.DrawString(Game1.dialogueFont, entry.DisplayName, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.borderWidth * 3 / 2 + 64 - 20 + 96) - Game1.dialogueFont.MeasureString(entry.DisplayName).X / 2f, (float)(this.sprites[i].bounds.Y + 48) + russianOffsetY - (float)(datable ? 24 : 20)), Game1.textColor);
			for (int hearts = 0; hearts < Math.Max(Utility.GetMaximumHeartsForCharacter(Game1.getCharacterFromName(name, true, false)), 10); hearts++)
			{
				this.drawNPCSlotHeart(b, i, entry, hearts, isDating, isCurrentSpouse);
			}
			if (datable || housemate)
			{
				string text;
				if (housemate)
				{
					text = ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Housemate_Female") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Housemate_Male"));
				}
				else if (isCurrentSpouse)
				{
					text = ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Wife") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Husband"));
				}
				else if (entry.IsMarriedToAnyone())
				{
					text = ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\UI:SocialPage_Relationship_MarriedToOtherPlayer_FemaleNpc") : Game1.content.LoadString("Strings\\UI:SocialPage_Relationship_MarriedToOtherPlayer_MaleNpc"));
				}
				else if (!Game1.player.isMarriedOrRoommates() && isDating)
				{
					text = ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Girlfriend") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Boyfriend"));
				}
				else if (entry.IsDivorcedFromCurrentPlayer())
				{
					text = ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_ExWife") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_ExHusband"));
				}
				else
				{
					text = ((gender == Gender.Female) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Single_Female") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Single_Male"));
				}
				int width = (IClickableMenu.borderWidth * 3 + 128 - 40 + 192) / 2;
				text = Game1.parseText(text, Game1.smallFont, width);
				Vector2 textSize = Game1.smallFont.MeasureString(text);
				b.DrawString(Game1.smallFont, text, new Vector2((float)(this.xPositionOnScreen + 192 + 8) - textSize.X / 2f, (float)this.sprites[i].bounds.Bottom - (textSize.Y - lineHeight)), Game1.textColor);
			}
			if (!isCurrentSpouse && !entry.IsChild)
			{
				Utility.drawWithShadow(b, Game1.mouseCursors2, new Vector2((float)(this.xPositionOnScreen + 384 + 304), (float)(this.sprites[i].bounds.Y - 4)), new Rectangle(166, 174, 14, 12), Color.White, 0f, Vector2.Zero, 4f, false, 0.88f, 0, -1, 0.2f);
				Texture2D mouseCursors = Game1.mouseCursors;
				Vector2 position = new Vector2((float)(this.xPositionOnScreen + 384 + 296), (float)(this.sprites[i].bounds.Y + 32 + 20));
				int num = 227;
				Friendship friendship = entry.Friendship;
				b.Draw(mouseCursors, position, new Rectangle?(new Rectangle(num + ((friendship != null && friendship.GiftsThisWeek >= 2) ? 9 : 0), 425, 9, 9)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				Texture2D mouseCursors2 = Game1.mouseCursors;
				Vector2 position2 = new Vector2((float)(this.xPositionOnScreen + 384 + 336), (float)(this.sprites[i].bounds.Y + 32 + 20));
				int num2 = 227;
				Friendship friendship2 = entry.Friendship;
				b.Draw(mouseCursors2, position2, new Rectangle?(new Rectangle(num2 + ((friendship2 != null && friendship2.GiftsThisWeek >= 1) ? 9 : 0), 425, 9, 9)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				Utility.drawWithShadow(b, Game1.mouseCursors2, new Vector2((float)(this.xPositionOnScreen + 384 + 424), (float)this.sprites[i].bounds.Y), new Rectangle(180, 175, 13, 11), Color.White, 0f, Vector2.Zero, 4f, false, 0.88f, 0, -1, 0.2f);
				Texture2D mouseCursors3 = Game1.mouseCursors;
				Vector2 position3 = new Vector2((float)(this.xPositionOnScreen + 384 + 432), (float)(this.sprites[i].bounds.Y + 32 + 20));
				int num3 = 227;
				Friendship friendship3 = entry.Friendship;
				b.Draw(mouseCursors3, position3, new Rectangle?(new Rectangle(num3 + ((friendship3 != null && friendship3.TalkedToToday) ? 9 : 0), 425, 9, 9)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
			}
			if (isCurrentSpouse)
			{
				if (!housemate || name == "Krobus")
				{
					b.Draw(Game1.objectSpriteSheet, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.borderWidth * 7 / 4 + 192), (float)this.sprites[i].bounds.Y), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, housemate ? 808 : 460, 16, 16)), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
					return;
				}
			}
			else if (isDating)
			{
				b.Draw(Game1.objectSpriteSheet, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.borderWidth * 7 / 4 + 192), (float)this.sprites[i].bounds.Y), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, housemate ? 808 : 458, 16, 16)), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
			}
		}

		// Token: 0x06002C9E RID: 11422 RVA: 0x00227684 File Offset: 0x00225884
		public void drawNPCSlotHeart(SpriteBatch b, int npcIndex, SocialPage.SocialEntry entry, int hearts, bool isDating, bool isCurrentSpouse)
		{
			bool isLockedHeart = entry.IsDatable && !isDating && !isCurrentSpouse && hearts >= 8;
			int heartX = (hearts < entry.HeartLevel || isLockedHeart) ? 211 : 218;
			Color heartTint = (hearts < 10 && isLockedHeart) ? (Color.Black * 0.35f) : Color.White;
			if (hearts < 10)
			{
				b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + 320 - 4 + hearts * 32), (float)(this.sprites[npcIndex].bounds.Y + 64 - 28)), new Rectangle?(new Rectangle(heartX, 428, 7, 6)), heartTint, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				return;
			}
			b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + 320 - 4 + (hearts - 10) * 32), (float)(this.sprites[npcIndex].bounds.Y + 64)), new Rectangle?(new Rectangle(heartX, 428, 7, 6)), heartTint, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
		}

		// Token: 0x06002C9F RID: 11423 RVA: 0x002277C4 File Offset: 0x002259C4
		public int rowPosition(int i)
		{
			int j = i - this.slotPosition;
			int rowHeight = 112;
			return this.yPositionOnScreen + IClickableMenu.borderWidth + 160 + 4 + j * rowHeight;
		}

		// Token: 0x06002CA0 RID: 11424 RVA: 0x002277F8 File Offset: 0x002259F8
		public void drawFarmerSlot(SpriteBatch b, int i)
		{
			SocialPage.SocialEntry entry = this.GetSocialEntry(i);
			if (entry == null)
			{
				return;
			}
			if (!entry.IsPlayer)
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(76, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Social page can't draw farmer slot for index ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(i);
				defaultInterpolatedStringHandler.AppendLiteral(": this is NPC '");
				defaultInterpolatedStringHandler.AppendFormatted(entry.InternalName);
				defaultInterpolatedStringHandler.AppendLiteral("', not a farmer.");
				log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
				return;
			}
			Farmer farmer = (Farmer)entry.Character;
			Gender gender = entry.Gender;
			ClickableTextureComponent clickableTextureComponent = this.sprites[i];
			int x = clickableTextureComponent.bounds.X;
			int y = clickableTextureComponent.bounds.Y;
			Rectangle origClip = b.GraphicsDevice.ScissorRectangle;
			Rectangle newClip = origClip;
			newClip.Height = Math.Min(newClip.Bottom, this.rowPosition(i)) - newClip.Y - 4;
			b.GraphicsDevice.ScissorRectangle = newClip;
			FarmerRenderer.isDrawingForUI = true;
			try
			{
				farmer.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(farmer.bathingClothes.Value ? 108 : 0, 0, false, false, null, false), farmer.bathingClothes.Value ? 108 : 0, new Rectangle(0, farmer.bathingClothes.Value ? 576 : 0, 16, 32), new Vector2((float)x, (float)y), Vector2.Zero, 0.8f, 2, Color.White, 0f, 1f, farmer);
			}
			finally
			{
				b.GraphicsDevice.ScissorRectangle = origClip;
			}
			FarmerRenderer.isDrawingForUI = false;
			bool flag = entry.IsMarriedToCurrentPlayer();
			float lineHeight = Game1.smallFont.MeasureString("W").Y;
			float russianOffsetY = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru) ? (-lineHeight / 2f) : 0f;
			b.DrawString(Game1.dialogueFont, farmer.Name, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.borderWidth * 3 / 2 + 96 - 20), (float)(this.sprites[i].bounds.Y + 48) + russianOffsetY - 24f), Game1.textColor);
			string text = (!Game1.content.ShouldUseGenderedCharacterTranslations()) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Single_Female") : ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Single_Female").Split('/', StringSplitOptions.None)[0] : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Single_Female").Split('/', StringSplitOptions.None).Last<string>());
			if (flag)
			{
				text = ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Husband") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Wife"));
			}
			else if (farmer.isMarriedOrRoommates() && !farmer.hasRoommate())
			{
				text = ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\UI:SocialPage_Relationship_MarriedToOtherPlayer_MaleNpc") : Game1.content.LoadString("Strings\\UI:SocialPage_Relationship_MarriedToOtherPlayer_FemaleNpc"));
			}
			else if (!Game1.player.isMarriedOrRoommates() && entry.IsDatingCurrentPlayer())
			{
				text = ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Boyfriend") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_Girlfriend"));
			}
			else if (entry.IsDivorcedFromCurrentPlayer())
			{
				text = ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_ExHusband") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage_Relationship_ExWife"));
			}
			int width = (IClickableMenu.borderWidth * 3 + 128 - 40 + 192) / 2;
			text = Game1.parseText(text, Game1.smallFont, width);
			Vector2 textSize = Game1.smallFont.MeasureString(text);
			b.DrawString(Game1.smallFont, text, new Vector2((float)(this.xPositionOnScreen + 192 + 8) - textSize.X / 2f, (float)this.sprites[i].bounds.Bottom - (textSize.Y - lineHeight)), Game1.textColor);
			if (flag)
			{
				b.Draw(Game1.objectSpriteSheet, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.borderWidth * 7 / 4 + 192), (float)this.sprites[i].bounds.Y), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 801, 16, 16)), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
				return;
			}
			if (entry.IsDatingCurrentPlayer())
			{
				b.Draw(Game1.objectSpriteSheet, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.borderWidth * 7 / 4 + 192), (float)this.sprites[i].bounds.Y), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 458, 16, 16)), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
			}
		}

		// Token: 0x06002CA1 RID: 11425 RVA: 0x00227CBC File Offset: 0x00225EBC
		public override void draw(SpriteBatch b)
		{
			b.End();
			b.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled, null, null);
			base.drawHorizontalPartition(b, this.yPositionOnScreen + IClickableMenu.borderWidth + 128 + 4, true, -1, -1, -1);
			base.drawHorizontalPartition(b, this.yPositionOnScreen + IClickableMenu.borderWidth + 192 + 32 + 20, true, -1, -1, -1);
			base.drawHorizontalPartition(b, this.yPositionOnScreen + IClickableMenu.borderWidth + 320 + 36, true, -1, -1, -1);
			base.drawHorizontalPartition(b, this.yPositionOnScreen + IClickableMenu.borderWidth + 384 + 32 + 52, true, -1, -1, -1);
			int i = this.slotPosition;
			while (i < this.slotPosition + 5 && i < this.sprites.Count)
			{
				SocialPage.SocialEntry entry = this.GetSocialEntry(i);
				if (entry != null)
				{
					if (entry.IsPlayer)
					{
						this.drawFarmerSlot(b, i);
					}
					else
					{
						this.drawNPCSlot(b, i);
					}
				}
				i++;
			}
			Rectangle origClip = b.GraphicsDevice.ScissorRectangle;
			Rectangle newClip = origClip;
			newClip.Y = Math.Max(0, this.rowPosition(this.numFarmers - 1));
			newClip.Height -= newClip.Y;
			if (newClip.Height > 0)
			{
				b.GraphicsDevice.ScissorRectangle = newClip;
				try
				{
					base.drawVerticalPartition(b, this.xPositionOnScreen + 256 + 12, true, -1, -1, -1, -1);
					base.drawVerticalPartition(b, this.xPositionOnScreen + 384 + 368, true, -1, -1, -1, -1);
					base.drawVerticalPartition(b, this.xPositionOnScreen + 256 + 12 + 352, true, -1, -1, -1, -1);
				}
				finally
				{
					b.GraphicsDevice.ScissorRectangle = origClip;
				}
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

		// Token: 0x04001E5A RID: 7770
		public const int slotsOnPage = 5;

		// Token: 0x04001E5B RID: 7771
		public string hoverText = "";

		// Token: 0x04001E5C RID: 7772
		public ClickableTextureComponent upButton;

		// Token: 0x04001E5D RID: 7773
		public ClickableTextureComponent downButton;

		// Token: 0x04001E5E RID: 7774
		public ClickableTextureComponent scrollBar;

		// Token: 0x04001E5F RID: 7775
		public Rectangle scrollBarRunner;

		// Token: 0x04001E60 RID: 7776
		public readonly List<SocialPage.SocialEntry> SocialEntries;

		// Token: 0x04001E61 RID: 7777
		public readonly List<ClickableTextureComponent> sprites = new List<ClickableTextureComponent>();

		// Token: 0x04001E62 RID: 7778
		public int slotPosition;

		// Token: 0x04001E63 RID: 7779
		public int numFarmers;

		// Token: 0x04001E64 RID: 7780
		public readonly List<ClickableTextureComponent> characterSlots = new List<ClickableTextureComponent>();

		// Token: 0x04001E65 RID: 7781
		public bool scrolling;

		// Token: 0x02000635 RID: 1589
		public class SocialEntry
		{
			// Token: 0x060044A9 RID: 17577 RVA: 0x0031D100 File Offset: 0x0031B300
			public SocialEntry(Farmer player, Friendship friendship)
			{
				this.Character = player;
				this.InternalName = player.UniqueMultiplayerID.ToString();
				this.DisplayName = player.Name;
				this.IsMet = true;
				this.IsPlayer = true;
				this.Gender = player.Gender;
				this.Friendship = friendship;
			}

			// Token: 0x060044AA RID: 17578 RVA: 0x0031D15C File Offset: 0x0031B35C
			public SocialEntry(NPC npc, Friendship friendship, CharacterData data, string overrideDisplayName = null)
			{
				this.Character = npc;
				this.InternalName = npc.Name;
				this.DisplayName = (overrideDisplayName ?? npc.displayName);
				this.IsMet = (friendship != null || npc is Child);
				this.IsDatable = (data != null && data.CanBeRomanced);
				this.SocialTabBehavior = ((data != null) ? data.SocialTab : SocialTabBehavior.AlwaysShown);
				this.IsChild = (npc is Child);
				this.Gender = npc.Gender;
				this.HeartLevel = ((friendship != null) ? friendship.Points : 0) / 250;
				this.Friendship = friendship;
				this.Data = data;
			}

			// Token: 0x060044AB RID: 17579 RVA: 0x0031D20E File Offset: 0x0031B40E
			public bool IsDatingCurrentPlayer()
			{
				Friendship friendship = this.Friendship;
				return friendship != null && friendship.IsDating();
			}

			// Token: 0x060044AC RID: 17580 RVA: 0x0031D221 File Offset: 0x0031B421
			public bool IsMarriedToCurrentPlayer()
			{
				Friendship friendship = this.Friendship;
				return friendship != null && friendship.IsMarried();
			}

			// Token: 0x060044AD RID: 17581 RVA: 0x0031D234 File Offset: 0x0031B434
			public bool IsRoommateForCurrentPlayer()
			{
				Friendship friendship = this.Friendship;
				return friendship != null && friendship.IsRoommate();
			}

			// Token: 0x060044AE RID: 17582 RVA: 0x0031D247 File Offset: 0x0031B447
			public bool IsDivorcedFromCurrentPlayer()
			{
				Friendship friendship = this.Friendship;
				return friendship != null && friendship.IsDivorced();
			}

			// Token: 0x060044AF RID: 17583 RVA: 0x0031D25C File Offset: 0x0031B45C
			public bool IsMarriedToAnyone()
			{
				if (this.CachedIsMarriedToAnyone == null)
				{
					if (this.IsMarriedToCurrentPlayer())
					{
						this.CachedIsMarriedToAnyone = new bool?(true);
					}
					else
					{
						foreach (Farmer farmer in Game1.getAllFarmers())
						{
							if (farmer.spouse == this.InternalName && farmer.isMarriedOrRoommates())
							{
								this.CachedIsMarriedToAnyone = new bool?(true);
								break;
							}
						}
						if (this.CachedIsMarriedToAnyone == null)
						{
							this.CachedIsMarriedToAnyone = new bool?(false);
						}
					}
				}
				return this.CachedIsMarriedToAnyone.Value;
			}

			// Token: 0x04002EDA RID: 11994
			private bool? CachedIsMarriedToAnyone;

			// Token: 0x04002EDB RID: 11995
			public Character Character;

			// Token: 0x04002EDC RID: 11996
			public readonly string InternalName;

			// Token: 0x04002EDD RID: 11997
			public readonly string DisplayName;

			// Token: 0x04002EDE RID: 11998
			public readonly bool IsMet;

			// Token: 0x04002EDF RID: 11999
			public readonly bool IsDatable;

			// Token: 0x04002EE0 RID: 12000
			public readonly SocialTabBehavior SocialTabBehavior;

			// Token: 0x04002EE1 RID: 12001
			public readonly bool IsChild;

			// Token: 0x04002EE2 RID: 12002
			public readonly bool IsPlayer;

			// Token: 0x04002EE3 RID: 12003
			public readonly Gender Gender;

			// Token: 0x04002EE4 RID: 12004
			public readonly int HeartLevel;

			// Token: 0x04002EE5 RID: 12005
			public readonly Friendship Friendship;

			// Token: 0x04002EE6 RID: 12006
			public readonly CharacterData Data;

			// Token: 0x04002EE7 RID: 12007
			public int? OrderMet;
		}
	}
}

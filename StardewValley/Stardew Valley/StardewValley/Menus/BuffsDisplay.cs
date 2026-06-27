using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Buffs;
using StardewValley.Extensions;

namespace StardewValley.Menus
{
	// Token: 0x02000251 RID: 593
	public class BuffsDisplay : IClickableMenu
	{
		// Token: 0x06002761 RID: 10081 RVA: 0x001C1529 File Offset: 0x001BF729
		public BuffsDisplay()
		{
			this.updatePosition();
		}

		// Token: 0x06002762 RID: 10082 RVA: 0x001C1558 File Offset: 0x001BF758
		private void updatePosition()
		{
			Rectangle tsarea = Game1.game1.GraphicsDevice.Viewport.GetTitleSafeArea();
			int w = 288;
			int h = 64;
			int x = tsarea.Right - 300 - this.width;
			int y = tsarea.Top + 8;
			if (x != this.xPositionOnScreen || y != this.yPositionOnScreen || w != this.width || h != this.height)
			{
				this.xPositionOnScreen = x;
				this.yPositionOnScreen = y;
				this.width = w;
				this.height = h;
				this.resetIcons();
			}
		}

		// Token: 0x06002763 RID: 10083 RVA: 0x001C15EC File Offset: 0x001BF7EC
		public override bool isWithinBounds(int x, int y)
		{
			foreach (KeyValuePair<ClickableTextureComponent, Buff> c in this.buffs)
			{
				if (c.Key.containsPoint(x, y))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002764 RID: 10084 RVA: 0x001C1650 File Offset: 0x001BF850
		public int getNumBuffs()
		{
			if (this.buffs == null)
			{
				return 0;
			}
			return this.buffs.Count;
		}

		// Token: 0x06002765 RID: 10085 RVA: 0x001C1668 File Offset: 0x001BF868
		public override void performHoverAction(int x, int y)
		{
			this.hoverText = "";
			foreach (KeyValuePair<ClickableTextureComponent, Buff> c in this.buffs)
			{
				if (c.Key.containsPoint(x, y))
				{
					this.hoverText = c.Key.hoverText + ((c.Value.millisecondsDuration != -2) ? (Environment.NewLine + c.Value.getTimeLeft()) : "");
					string format = this.hoverText;
					object[] buffDescriptionTextReplacement = this.getBuffDescriptionTextReplacement(c.Value.id);
					this.hoverText = string.Format(format, buffDescriptionTextReplacement);
					c.Key.scale = Math.Min(c.Key.baseScale + 0.1f, c.Key.scale + 0.02f);
					break;
				}
			}
		}

		// Token: 0x06002766 RID: 10086 RVA: 0x001C1778 File Offset: 0x001BF978
		public string[] getBuffDescriptionTextReplacement(string buffName)
		{
			if (buffName == "statue_of_blessings_3")
			{
				return new string[]
				{
					Game1.player.stats.Get("blessingOfWaters").ToString()
				};
			}
			return LegacyShims.EmptyArray<string>();
		}

		// Token: 0x06002767 RID: 10087 RVA: 0x001C17C0 File Offset: 0x001BF9C0
		public void arrangeTheseComponentsInThisRectangle(int rectangleX, int rectangleY, int rectangleWidthInComponentWidthUnits, int componentWidth, int componentHeight, int buffer, bool rightToLeft)
		{
			int x = 0;
			int y = 0;
			foreach (KeyValuePair<ClickableTextureComponent, Buff> pair in this.buffs)
			{
				ClickableTextureComponent c = pair.Key;
				if (rightToLeft)
				{
					c.bounds = new Rectangle(rectangleX + rectangleWidthInComponentWidthUnits * componentWidth - (x + 1) * (componentWidth + buffer), rectangleY + y * (componentHeight + buffer), componentWidth, componentHeight);
				}
				else
				{
					c.bounds = new Rectangle(rectangleX + x * (componentWidth + buffer), rectangleY + y * (componentHeight + buffer), componentWidth, componentHeight);
				}
				x++;
				if (x > rectangleWidthInComponentWidthUnits)
				{
					y++;
					x = 0;
				}
			}
		}

		// Token: 0x06002768 RID: 10088 RVA: 0x001C1878 File Offset: 0x001BFA78
		protected virtual void resetIcons()
		{
			this.buffs.Clear();
			if (Game1.player == null)
			{
				return;
			}
			IDictionary<string, float> prevIconScales = new Dictionary<string, float>();
			foreach (KeyValuePair<ClickableTextureComponent, Buff> entry in this.buffs)
			{
				prevIconScales[entry.Value.id] = entry.Key.scale;
			}
			foreach (Buff buff in this.GetSortedBuffs())
			{
				if (buff.visible)
				{
					bool isUpdated = this.updatedIDs.Contains(buff.id);
					foreach (ClickableTextureComponent icon in this.getClickableComponents(buff))
					{
						float scale;
						if (isUpdated)
						{
							icon.scale = icon.baseScale + 0.2f;
						}
						else if (prevIconScales.TryGetValue(buff.id, out scale))
						{
							icon.scale = Math.Max(icon.baseScale, scale);
						}
						this.buffs.Add(icon, buff);
					}
				}
			}
			this.updatedIDs.Clear();
			this.arrangeTheseComponentsInThisRectangle(this.xPositionOnScreen, this.yPositionOnScreen, this.width / 64, 64, 64, 8, true);
		}

		// Token: 0x06002769 RID: 10089 RVA: 0x001C1A10 File Offset: 0x001BFC10
		public new void update(GameTime time)
		{
			if (this.dirty)
			{
				this.resetIcons();
				this.dirty = false;
			}
			if (!Game1.wasMouseVisibleThisFrame)
			{
				this.hoverText = "";
			}
			foreach (KeyValuePair<ClickableTextureComponent, Buff> pair in this.buffs)
			{
				ClickableTextureComponent icon = pair.Key;
				Buff buff = pair.Value;
				icon.scale = Math.Max(icon.baseScale, icon.scale - 0.01f);
				if (!buff.alreadyUpdatedIconAlpha && (float)buff.millisecondsDuration < Math.Min(10000f, (float)buff.totalMillisecondsDuration / 10f) && buff.millisecondsDuration != -2)
				{
					buff.displayAlphaTimer += (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds / (((float)buff.millisecondsDuration < Math.Min(2000f, (float)buff.totalMillisecondsDuration / 20f)) ? 1f : 2f);
					buff.alreadyUpdatedIconAlpha = true;
				}
			}
		}

		// Token: 0x0600276A RID: 10090 RVA: 0x001C1B40 File Offset: 0x001BFD40
		public override void draw(SpriteBatch b)
		{
			this.updatePosition();
			foreach (KeyValuePair<ClickableTextureComponent, Buff> pair in this.buffs)
			{
				pair.Key.draw(b, Color.White * ((pair.Value.displayAlphaTimer > 0f) ? ((float)(Math.Cos((double)(pair.Value.displayAlphaTimer / 100f)) + 3.0) / 4f) : 1f), 0.8f, 0, 0, 0);
				pair.Value.alreadyUpdatedIconAlpha = false;
			}
			if (this.hoverText.Length != 0 && this.isWithinBounds(Game1.getOldMouseX(), Game1.getOldMouseY()))
			{
				this.performHoverAction(Game1.getOldMouseX(), Game1.getOldMouseY());
				IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, null, -1, -1, -1, 1f, null, null, null, null, null, null, 1f, -1, -1);
			}
		}

		// Token: 0x0600276B RID: 10091 RVA: 0x001C1C78 File Offset: 0x001BFE78
		public IEnumerable<Buff> GetSortedBuffs()
		{
			return from p in Game1.player.buffs.AppliedBuffs.Values
			orderby p.id == "food" descending, p.id == "drink" descending
			select p;
		}

		// Token: 0x0600276C RID: 10092 RVA: 0x001C1CE4 File Offset: 0x001BFEE4
		protected virtual string getDescription(Buff buff)
		{
			StringBuilder s = new StringBuilder();
			string displayName = buff.displayName;
			if (displayName != null && displayName.Length > 1)
			{
				s.AppendLine(buff.displayName);
				s.AppendLine("[line]");
			}
			string description2 = buff.description;
			if (description2 != null && description2.Length > 1)
			{
				s.AppendLine(buff.description);
			}
			foreach (BuffAttributeDisplay attribute in BuffsDisplay.displayAttributes)
			{
				string description = this.getDescription(buff, attribute, false);
				if (description != null)
				{
					s.AppendLine(description);
				}
			}
			string source = this.getSourceLine(buff);
			if (source != null)
			{
				s.AppendLine(source);
			}
			return s.ToString().TrimEnd();
		}

		// Token: 0x0600276D RID: 10093 RVA: 0x001C1DC0 File Offset: 0x001BFFC0
		protected virtual string getDescription(Buff buff, BuffAttributeDisplay attribute, bool withSource)
		{
			float value = attribute.Value(buff);
			if (value == 0f)
			{
				return null;
			}
			string description = attribute.Description(value);
			if (withSource)
			{
				string source = this.getSourceLine(buff);
				if (source != null)
				{
					description = description + "\n" + source;
				}
			}
			return description;
		}

		// Token: 0x0600276E RID: 10094 RVA: 0x001C1E10 File Offset: 0x001C0010
		protected virtual string getSourceLine(Buff buff)
		{
			string source = buff.displaySource ?? buff.source;
			if (string.IsNullOrWhiteSpace(source))
			{
				return null;
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.508") + source;
		}

		// Token: 0x0600276F RID: 10095 RVA: 0x001C1E4D File Offset: 0x001C004D
		public virtual IEnumerable<ClickableTextureComponent> getClickableComponents(Buff buff)
		{
			if (!buff.visible)
			{
				yield break;
			}
			if (buff.iconTexture != null)
			{
				Rectangle sourceRect = Game1.getSourceRectForStandardTileSheet(buff.iconTexture, buff.iconSheetIndex, 16, 16);
				yield return new ClickableTextureComponent("", Rectangle.Empty, null, this.getDescription(buff), buff.iconTexture, sourceRect, 4f, false);
			}
			else
			{
				foreach (BuffAttributeDisplay attribute in BuffsDisplay.displayAttributes)
				{
					string description = this.getDescription(buff, attribute, true);
					if (description != null)
					{
						Rectangle sourceRect2 = Game1.getSourceRectForStandardTileSheet(attribute.Texture(), attribute.SpriteIndex, 16, 16);
						yield return new ClickableTextureComponent("", Rectangle.Empty, null, description, attribute.Texture(), sourceRect2, 4f, false);
					}
				}
				List<BuffAttributeDisplay>.Enumerator enumerator = default(List<BuffAttributeDisplay>.Enumerator);
			}
			yield break;
			yield break;
		}

		// Token: 0x040018A7 RID: 6311
		public static readonly List<BuffAttributeDisplay> displayAttributes = new List<BuffAttributeDisplay>
		{
			new BuffAttributeDisplay(0, (BuffEffects buff) => buff.FarmingLevel, "Strings\\StringsFromCSFiles:Buff.cs.480"),
			new BuffAttributeDisplay(1, (BuffEffects buff) => buff.FishingLevel, "Strings\\StringsFromCSFiles:Buff.cs.483"),
			new BuffAttributeDisplay(2, (BuffEffects buff) => buff.MiningLevel, "Strings\\StringsFromCSFiles:Buff.cs.486"),
			new BuffAttributeDisplay(4, (BuffEffects buff) => buff.LuckLevel, "Strings\\StringsFromCSFiles:Buff.cs.489"),
			new BuffAttributeDisplay(5, (BuffEffects buff) => buff.ForagingLevel, "Strings\\StringsFromCSFiles:Buff.cs.492"),
			new BuffAttributeDisplay(16, (BuffEffects buff) => buff.MaxStamina, "Strings\\StringsFromCSFiles:Buff.cs.495"),
			new BuffAttributeDisplay(11, (BuffEffects buff) => buff.Attack, "Strings\\StringsFromCSFiles:Buff.cs.504"),
			new BuffAttributeDisplay(8, (BuffEffects buff) => buff.MagneticRadius, "Strings\\StringsFromCSFiles:Buff.cs.498"),
			new BuffAttributeDisplay(10, (BuffEffects buff) => buff.Defense, "Strings\\StringsFromCSFiles:Buff.cs.501"),
			new BuffAttributeDisplay(9, (BuffEffects buff) => buff.Speed, "Strings\\StringsFromCSFiles:Buff.cs.507")
		};

		// Token: 0x040018A8 RID: 6312
		private readonly Dictionary<ClickableTextureComponent, Buff> buffs = new Dictionary<ClickableTextureComponent, Buff>();

		// Token: 0x040018A9 RID: 6313
		public readonly HashSet<string> updatedIDs = new HashSet<string>();

		// Token: 0x040018AA RID: 6314
		public bool dirty;

		// Token: 0x040018AB RID: 6315
		public string hoverText = "";
	}
}

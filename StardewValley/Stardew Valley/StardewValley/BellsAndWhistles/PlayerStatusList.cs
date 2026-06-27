using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x020003A2 RID: 930
	public class PlayerStatusList : INetObject<NetFields>
	{
		// Token: 0x17000493 RID: 1171
		// (get) Token: 0x060038B6 RID: 14518 RVA: 0x002CF260 File Offset: 0x002CD460
		public NetFields NetFields { get; } = new NetFields("PlayerStatusList");

		// Token: 0x060038B7 RID: 14519 RVA: 0x002CF268 File Offset: 0x002CD468
		public PlayerStatusList()
		{
			this.InitNetFields();
		}

		// Token: 0x060038B8 RID: 14520 RVA: 0x002CF2D8 File Offset: 0x002CD4D8
		public void InitNetFields()
		{
			this.NetFields.SetOwner(this).AddField(this._statusList, "_statusList");
			this._statusList.OnValueRemoved += delegate(long <p0>, string <p1>)
			{
				this._OnValueChanged();
			};
			this._statusList.OnValueAdded += delegate(long <p0>, string <p1>)
			{
				this._OnValueChanged();
			};
			this._statusList.OnConflictResolve += delegate(long <p0>, NetString <p1>, NetString <p2>)
			{
				this._OnValueChanged();
			};
			this._statusList.OnValueTargetUpdated += delegate(long key, string value, string targetValue)
			{
				NetString netString;
				if (this._statusList.FieldDict.TryGetValue(key, out netString))
				{
					netString.CancelInterpolation();
				}
				this._OnValueChanged();
			};
		}

		// Token: 0x060038B9 RID: 14521 RVA: 0x002CF360 File Offset: 0x002CD560
		public void AddSpriteDefinition(string key, string file, int x, int y, int width, int height)
		{
			Texture2D iconSprite;
			if (!this._iconSprites.TryGetValue(file, out iconSprite) || iconSprite.IsDisposed)
			{
				this._iconSprites[file] = Game1.content.Load<Texture2D>(file);
			}
			this._iconDefinitions[key] = new KeyValuePair<string, Rectangle>(file, new Rectangle(x, y, width, height));
			if (width > this.largestSpriteWidth)
			{
				this.largestSpriteWidth = width;
			}
			if (height > this.largestSpriteHeight)
			{
				this.largestSpriteHeight = height;
			}
		}

		// Token: 0x060038BA RID: 14522 RVA: 0x002CF3E0 File Offset: 0x002CD5E0
		public void UpdateState(string newState)
		{
			string oldState;
			if (!this._statusList.TryGetValue(Game1.player.UniqueMultiplayerID, out oldState) || oldState != newState)
			{
				this._statusList[Game1.player.UniqueMultiplayerID] = newState;
			}
		}

		// Token: 0x060038BB RID: 14523 RVA: 0x002CF425 File Offset: 0x002CD625
		public void WithdrawState()
		{
			this._statusList.Remove(Game1.player.UniqueMultiplayerID);
		}

		// Token: 0x060038BC RID: 14524 RVA: 0x002CF440 File Offset: 0x002CD640
		protected void _OnValueChanged()
		{
			foreach (long id in this._statusList.Keys)
			{
				this._formattedStatusList[id] = this.GetStatusText(id, "");
			}
			this._ResortList();
		}

		// Token: 0x060038BD RID: 14525 RVA: 0x002CF4B4 File Offset: 0x002CD6B4
		protected void _ResortList()
		{
			this._sortedFarmers.Clear();
			foreach (Farmer farmer in Game1.getOnlineFarmers())
			{
				this._sortedFarmers.Add(farmer);
			}
			foreach (Farmer farmer2 in Game1.getAllFarmers())
			{
				if (Game1.IsMasterGame && !this._sortedFarmers.Contains(farmer2))
				{
					this._statusList.Remove(farmer2.UniqueMultiplayerID);
				}
				if (!this._statusList.ContainsKey(farmer2.UniqueMultiplayerID))
				{
					this._sortedFarmers.Remove(farmer2);
				}
			}
			PlayerStatusList.SortMode sortMode = this.sortMode;
			if (sortMode - PlayerStatusList.SortMode.NumberSort > 1)
			{
				if (sortMode - PlayerStatusList.SortMode.AlphaSort <= 1)
				{
					this._sortedFarmers.Sort((Farmer a, Farmer b) => this.GetStatusText(a.UniqueMultiplayerID, "").CompareTo(this.GetStatusText(b.UniqueMultiplayerID, "")));
					if (this.sortMode == PlayerStatusList.SortMode.AlphaSortDescending)
					{
						this._sortedFarmers.Reverse();
						return;
					}
				}
			}
			else
			{
				this._sortedFarmers.Sort((Farmer a, Farmer b) => this.GetStatusInt(a.UniqueMultiplayerID, 0).CompareTo(this.GetStatusInt(b.UniqueMultiplayerID, 0)));
				if (this.sortMode == PlayerStatusList.SortMode.NumberSortDescending)
				{
					this._sortedFarmers.Reverse();
				}
			}
		}

		// Token: 0x060038BE RID: 14526 RVA: 0x002CF600 File Offset: 0x002CD800
		public bool TryGetStatusText(long id, out string statusText)
		{
			if (this._statusList.TryGetValue(id, out statusText))
			{
				if (this.displayMode == PlayerStatusList.DisplayMode.LocalizedText)
				{
					statusText = Game1.content.LoadString(statusText);
				}
				return true;
			}
			statusText = null;
			return false;
		}

		// Token: 0x060038BF RID: 14527 RVA: 0x002CF630 File Offset: 0x002CD830
		public string GetStatusText(long id, string fallback = "")
		{
			string statusText;
			if (!this.TryGetStatusText(id, out statusText))
			{
				return fallback;
			}
			return statusText;
		}

		// Token: 0x060038C0 RID: 14528 RVA: 0x002CF64C File Offset: 0x002CD84C
		public int GetStatusInt(long id, int fallback = 0)
		{
			string statusText;
			int status;
			if (!this.TryGetStatusText(id, out statusText) || !int.TryParse(statusText, out status))
			{
				return fallback;
			}
			return status;
		}

		// Token: 0x060038C1 RID: 14529 RVA: 0x002CF674 File Offset: 0x002CD874
		public void Draw(SpriteBatch b, Vector2 draw_position, float draw_scale = 4f, float draw_layer = 0.45f, PlayerStatusList.HorizontalAlignment horizontal_origin = PlayerStatusList.HorizontalAlignment.Left, PlayerStatusList.VerticalAlignment vertical_origin = PlayerStatusList.VerticalAlignment.Top)
		{
			float y_offset_per_entry = 12f;
			if (this.displayMode == PlayerStatusList.DisplayMode.Icons && (float)this.largestSpriteHeight > y_offset_per_entry)
			{
				y_offset_per_entry = (float)this.largestSpriteHeight;
			}
			if (horizontal_origin == PlayerStatusList.HorizontalAlignment.Right)
			{
				float longest_string = 0f;
				if (this.displayMode == PlayerStatusList.DisplayMode.Icons)
				{
					draw_position.X -= (float)this.largestSpriteWidth * draw_scale;
				}
				else
				{
					foreach (Farmer farmer in this._sortedFarmers)
					{
						string state;
						if (!farmer.IsDedicatedPlayer && this._formattedStatusList.TryGetValue(farmer.UniqueMultiplayerID, out state))
						{
							float string_length = Game1.dialogueFont.MeasureString(state).X;
							if (longest_string < string_length)
							{
								longest_string = string_length;
							}
						}
					}
					draw_position.X -= (longest_string + 16f) * draw_scale;
				}
			}
			if (vertical_origin == PlayerStatusList.VerticalAlignment.Bottom)
			{
				draw_position.Y -= y_offset_per_entry * (float)this._statusList.Length * draw_scale;
			}
			foreach (Farmer farmer2 in this._sortedFarmers)
			{
				if (!farmer2.IsDedicatedPlayer)
				{
					float sort_direction = (float)(Game1.isUsingBackToFrontSorting ? -1 : 1);
					string state2;
					if (this._formattedStatusList.TryGetValue(farmer2.UniqueMultiplayerID, out state2))
					{
						Vector2 draw_offset = Vector2.Zero;
						farmer2.FarmerRenderer.drawMiniPortrat(b, draw_position, draw_layer, draw_scale * 0.75f, 2, farmer2, 1f);
						KeyValuePair<string, Rectangle> spriteDefinition;
						if (this.displayMode == PlayerStatusList.DisplayMode.Icons && this._iconDefinitions.TryGetValue(state2, out spriteDefinition))
						{
							draw_offset.X += 12f * draw_scale;
							Rectangle currentSrcRect = spriteDefinition.Value;
							currentSrcRect.Y = (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % (double)(this.iconAnimationFrames * 100) / 100.0) * 16;
							b.Draw(this._iconSprites[spriteDefinition.Key], draw_position + draw_offset, new Rectangle?(currentSrcRect), Color.White, 0f, Vector2.Zero, draw_scale, SpriteEffects.None, draw_layer - 0.0001f * sort_direction);
						}
						else
						{
							draw_offset.X += 16f * draw_scale;
							draw_offset.Y += 2f * draw_scale;
							string drawn_string = state2;
							b.DrawString(Game1.dialogueFont, drawn_string, draw_position + draw_offset + Vector2.One * draw_scale, Color.Black, 0f, Vector2.Zero, draw_scale / 4f, SpriteEffects.None, draw_layer - 0.0001f * sort_direction);
							b.DrawString(Game1.dialogueFont, drawn_string, draw_position + draw_offset, Color.White, 0f, Vector2.Zero, draw_scale / 4f, SpriteEffects.None, draw_layer);
						}
						draw_position.Y += y_offset_per_entry * draw_scale;
					}
				}
			}
		}

		// Token: 0x0400253E RID: 9534
		protected readonly NetLongDictionary<string, NetString> _statusList = new NetLongDictionary<string, NetString>
		{
			InterpolationWait = false
		};

		// Token: 0x0400253F RID: 9535
		protected readonly Dictionary<long, string> _formattedStatusList = new Dictionary<long, string>();

		// Token: 0x04002540 RID: 9536
		protected readonly Dictionary<string, Texture2D> _iconSprites = new Dictionary<string, Texture2D>();

		// Token: 0x04002541 RID: 9537
		protected readonly List<Farmer> _sortedFarmers = new List<Farmer>();

		// Token: 0x04002542 RID: 9538
		public int iconAnimationFrames = 1;

		// Token: 0x04002543 RID: 9539
		public int largestSpriteWidth;

		// Token: 0x04002544 RID: 9540
		public int largestSpriteHeight;

		// Token: 0x04002545 RID: 9541
		public PlayerStatusList.SortMode sortMode;

		// Token: 0x04002546 RID: 9542
		public PlayerStatusList.DisplayMode displayMode;

		// Token: 0x04002547 RID: 9543
		protected Dictionary<string, KeyValuePair<string, Rectangle>> _iconDefinitions = new Dictionary<string, KeyValuePair<string, Rectangle>>();

		// Token: 0x020006BB RID: 1723
		public enum SortMode
		{
			// Token: 0x04003099 RID: 12441
			None,
			// Token: 0x0400309A RID: 12442
			NumberSort,
			// Token: 0x0400309B RID: 12443
			NumberSortDescending,
			// Token: 0x0400309C RID: 12444
			AlphaSort,
			// Token: 0x0400309D RID: 12445
			AlphaSortDescending
		}

		// Token: 0x020006BC RID: 1724
		public enum DisplayMode
		{
			// Token: 0x0400309F RID: 12447
			Text,
			// Token: 0x040030A0 RID: 12448
			LocalizedText,
			// Token: 0x040030A1 RID: 12449
			Icons
		}

		// Token: 0x020006BD RID: 1725
		public enum VerticalAlignment
		{
			// Token: 0x040030A3 RID: 12451
			Top,
			// Token: 0x040030A4 RID: 12452
			Bottom
		}

		// Token: 0x020006BE RID: 1726
		public enum HorizontalAlignment
		{
			// Token: 0x040030A6 RID: 12454
			Left,
			// Token: 0x040030A7 RID: 12455
			Right
		}
	}
}

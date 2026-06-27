using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus
{
	// Token: 0x020002AB RID: 683
	public class SpecialCurrencyDisplay
	{
		// Token: 0x06002CA2 RID: 11426 RVA: 0x00227F88 File Offset: 0x00226188
		public virtual void Register(string key, NetIntDelta field, Action<int> playSound = null, Action<SpriteBatch, Vector2> drawIcon = null)
		{
			if (this.registeredCurrencyDisplays.ContainsKey(key))
			{
				this.Unregister(key);
			}
			playSound = (playSound ?? delegate(int delta)
			{
				this.PlaySound(key, delta);
			});
			drawIcon = (drawIcon ?? delegate(SpriteBatch b, Vector2 position)
			{
				this.DrawIcon(key, b, position);
			});
			this.registeredCurrencyDisplays[key] = new SpecialCurrencyDisplay.CurrencyDisplayType
			{
				key = key,
				field = field,
				playSound = playSound,
				drawIcon = drawIcon
			};
			field.fieldChangeVisibleEvent += this.OnCurrencyChange;
		}

		// Token: 0x06002CA3 RID: 11427 RVA: 0x0022803C File Offset: 0x0022623C
		public virtual void ShowCurrency(string currency, Func<bool> keepOpen = null, float timeToLive = 5f)
		{
			if (currency == null)
			{
				return;
			}
			foreach (SpecialCurrencyDisplay.CurrencyRenderInfo rendered in this.displayedCurrencies)
			{
				if (rendered.currency.key == currency)
				{
					rendered.keepOpen = (keepOpen ?? rendered.keepOpen);
					rendered.timeToLive = Math.Max(rendered.timeToLive, timeToLive);
					return;
				}
			}
			SpecialCurrencyDisplay.CurrencyDisplayType data;
			if (this.registeredCurrencyDisplays.TryGetValue(currency, out data))
			{
				this.displayedCurrencies.Add(new SpecialCurrencyDisplay.CurrencyRenderInfo(data, keepOpen, timeToLive));
				return;
			}
			Game1.log.Warn("Can't show unknown currency type '" + currency + "'.");
		}

		// Token: 0x06002CA4 RID: 11428 RVA: 0x00228104 File Offset: 0x00226304
		public virtual void HideCurrency(string currency, bool immediate = true)
		{
			if (immediate)
			{
				this.displayedCurrencies.RemoveAll((SpecialCurrencyDisplay.CurrencyRenderInfo p) => p.currency.key == currency);
				return;
			}
			foreach (SpecialCurrencyDisplay.CurrencyRenderInfo rendered in this.displayedCurrencies)
			{
				if (rendered.currency.key == currency)
				{
					rendered.keepOpen = null;
					rendered.timeToLive = 0f;
				}
			}
		}

		// Token: 0x06002CA5 RID: 11429 RVA: 0x002281A4 File Offset: 0x002263A4
		public virtual void OnCurrencyChange(NetIntDelta field, int oldValue, int newValue)
		{
			if (Game1.gameMode != 3 || oldValue == newValue)
			{
				return;
			}
			foreach (SpecialCurrencyDisplay.CurrencyRenderInfo render in this.displayedCurrencies)
			{
				if (render.currency.field == field)
				{
					render.OnCurrencyChanged(oldValue, newValue);
					return;
				}
			}
			foreach (SpecialCurrencyDisplay.CurrencyDisplayType currency in this.registeredCurrencyDisplays.Values)
			{
				if (currency.field == field)
				{
					SpecialCurrencyDisplay.CurrencyRenderInfo render2 = new SpecialCurrencyDisplay.CurrencyRenderInfo(currency, null, 5f);
					render2.OnCurrencyChanged(oldValue, newValue);
					this.displayedCurrencies.Add(render2);
					return;
				}
			}
			Game1.log.Warn("Can't show currency change for unknown field '" + field.Name + "'.");
		}

		// Token: 0x06002CA6 RID: 11430 RVA: 0x002282A8 File Offset: 0x002264A8
		public virtual void Unregister(string key)
		{
			this.HideCurrency(key, true);
			SpecialCurrencyDisplay.CurrencyDisplayType newCurrencyType;
			if (this.registeredCurrencyDisplays.TryGetValue(key, out newCurrencyType))
			{
				newCurrencyType.field.fieldChangeVisibleEvent -= this.OnCurrencyChange;
				this.registeredCurrencyDisplays.Remove(key);
			}
		}

		// Token: 0x06002CA7 RID: 11431 RVA: 0x002282F4 File Offset: 0x002264F4
		public virtual void Cleanup()
		{
			foreach (string key in new List<string>(this.registeredCurrencyDisplays.Keys))
			{
				this.Unregister(key);
			}
		}

		// Token: 0x06002CA8 RID: 11432 RVA: 0x00228354 File Offset: 0x00226554
		public virtual void DrawIcon(string currency, SpriteBatch b, Vector2 position)
		{
			if (currency == "walnuts")
			{
				b.Draw(Game1.objectSpriteSheet, position, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 73, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
				return;
			}
			if (!(currency == "qiGems"))
			{
				return;
			}
			b.Draw(Game1.objectSpriteSheet, position, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 858, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
		}

		// Token: 0x06002CA9 RID: 11433 RVA: 0x002283F8 File Offset: 0x002265F8
		public virtual void PlaySound(string currency, int direction)
		{
			if (currency == "walnuts")
			{
				Game1.playSound("goldenWalnut", null);
			}
		}

		// Token: 0x06002CAA RID: 11434 RVA: 0x00228428 File Offset: 0x00226628
		public virtual void Update(GameTime time)
		{
			for (int i = 0; i < this.displayedCurrencies.Count; i++)
			{
				SpecialCurrencyDisplay.CurrencyRenderInfo render = this.displayedCurrencies[i];
				Func<bool> keepOpen2 = render.keepOpen;
				bool keepOpen = keepOpen2 != null && keepOpen2();
				if (!keepOpen)
				{
					render.keepOpen = null;
					render.timeToLive -= (float)time.ElapsedGameTime.TotalSeconds;
					if (render.timeToLive < 0f)
					{
						render.timeToLive = 0f;
					}
				}
				float positionDelta = (float)time.ElapsedGameTime.TotalSeconds / 0.5f;
				render.slidePosition += ((keepOpen || render.timeToLive > 0f) ? positionDelta : (-positionDelta));
				render.slidePosition = Utility.Clamp(render.slidePosition, 0f, 1f);
				if (!keepOpen && render.timeToLive <= 0f && render.slidePosition <= 0f)
				{
					this.displayedCurrencies.RemoveAt(i);
					i--;
				}
			}
		}

		// Token: 0x06002CAB RID: 11435 RVA: 0x00228531 File Offset: 0x00226731
		public Vector2 GetUpperLeft(float slidePosition)
		{
			return new Vector2(16f, (float)((int)Utility.Lerp(-26f, 0f, slidePosition) * 4));
		}

		// Token: 0x06002CAC RID: 11436 RVA: 0x00228554 File Offset: 0x00226754
		public virtual void Draw(SpriteBatch b)
		{
			if (this.displayedCurrencies.Count == 0)
			{
				return;
			}
			int leftOffset = 0;
			foreach (SpecialCurrencyDisplay.CurrencyRenderInfo render in this.displayedCurrencies)
			{
				MoneyDial moneyDial = render.moneyDial;
				Vector2 drawPosition = this.GetUpperLeft(render.slidePosition);
				if (leftOffset > 0)
				{
					drawPosition.X += (float)leftOffset;
				}
				Rectangle backgroundSourceRect = new Rectangle(48, 176, 52, 26);
				b.Draw(Game1.mouseCursors2, drawPosition, new Rectangle?(backgroundSourceRect), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
				leftOffset += backgroundSourceRect.Width * 4;
				int displayedValue = render.currency.field.Value;
				if (render.slidePosition < 0.5f)
				{
					displayedValue = moneyDial.previousTargetValue;
				}
				moneyDial.draw(b, drawPosition + new Vector2(108f, 40f), displayedValue);
				Action<SpriteBatch, Vector2> drawIcon = render.currency.drawIcon;
				if (drawIcon != null)
				{
					drawIcon(b, drawPosition + new Vector2(4f, 6f) * 4f);
				}
			}
		}

		// Token: 0x06002CAD RID: 11437 RVA: 0x002286B4 File Offset: 0x002268B4
		public static void Draw(SpriteBatch b, Vector2 drawPosition, MoneyDial moneyDial, int displayedValue, Texture2D drawSpriteTexture, Rectangle drawSpriteSourceRect)
		{
			if (moneyDial != null && moneyDial.numDigits > 3)
			{
				b.Draw(Game1.mouseCursors_1_6, drawPosition, new Rectangle?(new Rectangle(42, 0, 57, 26)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
			}
			else
			{
				b.Draw(Game1.mouseCursors2, drawPosition, new Rectangle?(new Rectangle(48, 176, 52, 26)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
			}
			if (moneyDial != null)
			{
				moneyDial.draw(b, drawPosition + new Vector2(108f, 40f), displayedValue);
			}
			b.Draw(drawSpriteTexture, drawPosition + new Vector2(4f, 6f) * 4f, new Rectangle?(drawSpriteSourceRect), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
		}

		// Token: 0x06002CAE RID: 11438 RVA: 0x002287AC File Offset: 0x002269AC
		public static void Draw(SpriteBatch b, Vector2 drawPosition, int displayedValue, Texture2D drawSpriteTexture, Rectangle drawSpriteSourceRect)
		{
			b.Draw(Game1.mouseCursors2, drawPosition, new Rectangle?(new Rectangle(48, 176, 52, 26)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
			int numDigits = 3;
			int xPosition = 0;
			int digitStrip = (int)Math.Pow(10.0, (double)(numDigits - 1));
			bool significant = false;
			for (int i = 0; i < numDigits; i++)
			{
				int currentDigit = displayedValue / digitStrip % 10;
				if (currentDigit > 0 || i == numDigits - 1)
				{
					significant = true;
				}
				if (significant)
				{
					b.Draw(Game1.mouseCursors, drawPosition + new Vector2(108f, 40f) + new Vector2((float)xPosition, 0f), new Rectangle?(new Rectangle(286, 502 - currentDigit * 8, 5, 8)), Color.Maroon, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				}
				xPosition += 24;
				digitStrip /= 10;
			}
			b.Draw(drawSpriteTexture, drawPosition + new Vector2(4f, 6f) * 4f, new Rectangle?(drawSpriteSourceRect), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
		}

		// Token: 0x04001E66 RID: 7782
		public const string currency_walnuts = "walnuts";

		// Token: 0x04001E67 RID: 7783
		public const string currency_qiGems = "qiGems";

		// Token: 0x04001E68 RID: 7784
		public const int defaultSeconds = 5;

		// Token: 0x04001E69 RID: 7785
		public Dictionary<string, SpecialCurrencyDisplay.CurrencyDisplayType> registeredCurrencyDisplays = new Dictionary<string, SpecialCurrencyDisplay.CurrencyDisplayType>();

		// Token: 0x04001E6A RID: 7786
		public readonly List<SpecialCurrencyDisplay.CurrencyRenderInfo> displayedCurrencies = new List<SpecialCurrencyDisplay.CurrencyRenderInfo>();

		// Token: 0x02000639 RID: 1593
		public class CurrencyDisplayType
		{
			// Token: 0x04002EF2 RID: 12018
			public string key;

			// Token: 0x04002EF3 RID: 12019
			public NetIntDelta field;

			// Token: 0x04002EF4 RID: 12020
			public Action<int> playSound;

			// Token: 0x04002EF5 RID: 12021
			public Action<SpriteBatch, Vector2> drawIcon;
		}

		// Token: 0x0200063A RID: 1594
		public class CurrencyRenderInfo
		{
			// Token: 0x060044BC RID: 17596 RVA: 0x0031D568 File Offset: 0x0031B768
			public CurrencyRenderInfo(SpecialCurrencyDisplay.CurrencyDisplayType currency, Func<bool> keepOpen = null, float timeToLive = 5f)
			{
				this.currency = currency;
				this.keepOpen = keepOpen;
				this.timeToLive = timeToLive;
				this.moneyDial.currentValue = currency.field.TargetValue;
				this.moneyDial.previousTargetValue = currency.field.Value;
				this.moneyDial.onPlaySound = currency.playSound;
			}

			// Token: 0x060044BD RID: 17597 RVA: 0x0031D5E1 File Offset: 0x0031B7E1
			public void OnCurrencyChanged(int oldValue, int newValue)
			{
				this.timeToLive = Math.Max(this.timeToLive, 5f);
				this.moneyDial.currentValue = oldValue;
				Action<int> onPlaySound = this.moneyDial.onPlaySound;
				if (onPlaySound == null)
				{
					return;
				}
				onPlaySound(newValue - oldValue);
			}

			// Token: 0x04002EF6 RID: 12022
			public SpecialCurrencyDisplay.CurrencyDisplayType currency;

			// Token: 0x04002EF7 RID: 12023
			public MoneyDial moneyDial = new MoneyDial(3, true)
			{
				onPlaySound = null
			};

			// Token: 0x04002EF8 RID: 12024
			public float slidePosition;

			// Token: 0x04002EF9 RID: 12025
			public Func<bool> keepOpen;

			// Token: 0x04002EFA RID: 12026
			public float timeToLive;
		}
	}
}

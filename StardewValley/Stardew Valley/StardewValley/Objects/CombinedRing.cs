using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buffs;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Monsters;

namespace StardewValley.Objects
{
	// Token: 0x020001A8 RID: 424
	public class CombinedRing : Ring
	{
		// Token: 0x06001E16 RID: 7702 RVA: 0x00159913 File Offset: 0x00157B13
		public CombinedRing() : base("880")
		{
		}

		// Token: 0x06001E17 RID: 7703 RVA: 0x0015992C File Offset: 0x00157B2C
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.combinedRings, "combinedRings");
			this.combinedRings.OnElementChanged += delegate(NetList<Ring, NetRef<Ring>> <p0>, int <p1>, Ring <p2>, Ring <p3>)
			{
				this.OnCombinedRingsChanged();
			};
			this.combinedRings.OnArrayReplaced += delegate(NetList<Ring, NetRef<Ring>> <p0>, IList<Ring> <p1>, IList<Ring> <p2>)
			{
				this.OnCombinedRingsChanged();
			};
		}

		// Token: 0x06001E18 RID: 7704 RVA: 0x00159984 File Offset: 0x00157B84
		protected override bool loadDisplayFields()
		{
			base.loadDisplayFields();
			this.description = "";
			foreach (Ring ring in this.combinedRings)
			{
				ring.getDescription();
				this.description = this.description + ring.description + "\n\n";
			}
			this.description = this.description.Trim();
			return true;
		}

		// Token: 0x06001E19 RID: 7705 RVA: 0x00159A18 File Offset: 0x00157C18
		public override bool GetsEffectOfRing(string ringId)
		{
			using (NetList<Ring, NetRef<Ring>>.Enumerator enumerator = this.combinedRings.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.GetsEffectOfRing(ringId))
					{
						return true;
					}
				}
			}
			return base.GetsEffectOfRing(ringId);
		}

		// Token: 0x06001E1A RID: 7706 RVA: 0x00159A78 File Offset: 0x00157C78
		protected override Item GetOneNew()
		{
			return new CombinedRing();
		}

		// Token: 0x06001E1B RID: 7707 RVA: 0x00159A80 File Offset: 0x00157C80
		protected override void GetOneCopyFrom(Item source)
		{
			base.GetOneCopyFrom(source);
			CombinedRing fromRing = source as CombinedRing;
			if (fromRing != null)
			{
				this.combinedRings.Clear();
				foreach (Ring ring2 in fromRing.combinedRings)
				{
					Ring ring = (Ring)ring2.getOne();
					this.combinedRings.Add(ring);
				}
			}
		}

		// Token: 0x06001E1C RID: 7708 RVA: 0x00159B00 File Offset: 0x00157D00
		public override int GetEffectsOfRingMultiplier(string ringId)
		{
			int count = 0;
			foreach (Ring ring in this.combinedRings)
			{
				count += ring.GetEffectsOfRingMultiplier(ringId);
			}
			return count;
		}

		// Token: 0x06001E1D RID: 7709 RVA: 0x00159B5C File Offset: 0x00157D5C
		public override void onEquip(Farmer who)
		{
			foreach (Ring ring in this.combinedRings)
			{
				ring.onEquip(who);
			}
			base.onEquip(who);
		}

		// Token: 0x06001E1E RID: 7710 RVA: 0x00159BB4 File Offset: 0x00157DB4
		public override void onUnequip(Farmer who)
		{
			foreach (Ring ring in this.combinedRings)
			{
				ring.onUnequip(who);
			}
			base.onUnequip(who);
		}

		// Token: 0x06001E1F RID: 7711 RVA: 0x00159C0C File Offset: 0x00157E0C
		public override void AddEquipmentEffects(BuffEffects effects)
		{
			base.AddEquipmentEffects(effects);
			foreach (Ring ring in this.combinedRings)
			{
				ring.AddEquipmentEffects(effects);
			}
		}

		// Token: 0x06001E20 RID: 7712 RVA: 0x00159C64 File Offset: 0x00157E64
		public override void onLeaveLocation(Farmer who, GameLocation environment)
		{
			foreach (Ring ring in this.combinedRings)
			{
				ring.onLeaveLocation(who, environment);
			}
			base.onLeaveLocation(who, environment);
		}

		// Token: 0x06001E21 RID: 7713 RVA: 0x00159CC0 File Offset: 0x00157EC0
		public override void onMonsterSlay(Monster m, GameLocation location, Farmer who)
		{
			foreach (Ring ring in this.combinedRings)
			{
				ring.onMonsterSlay(m, location, who);
			}
			base.onMonsterSlay(m, location, who);
		}

		// Token: 0x06001E22 RID: 7714 RVA: 0x00159D1C File Offset: 0x00157F1C
		public override void onNewLocation(Farmer who, GameLocation environment)
		{
			foreach (Ring ring in this.combinedRings)
			{
				ring.onNewLocation(who, environment);
			}
			base.onNewLocation(who, environment);
		}

		// Token: 0x06001E23 RID: 7715 RVA: 0x00159D78 File Offset: 0x00157F78
		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			if (this.combinedRings.Count >= 2)
			{
				base.AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
				float oldScaleSize = scaleSize;
				scaleSize = 1f;
				location.Y -= (oldScaleSize - 1f) * 32f;
				ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(this.combinedRings[0].QualifiedItemId);
				Texture2D texture = dataOrErrorItem.GetTexture();
				Rectangle src = dataOrErrorItem.GetSourceRect(0, null).Clone();
				src.X += 5;
				src.Y += 7;
				src.Width = 4;
				src.Height = 6;
				spriteBatch.Draw(texture, location + new Vector2(51f, 51f) * scaleSize + new Vector2(-12f, 8f) * scaleSize, new Rectangle?(src), color * transparency, 0f, new Vector2(1.5f, 2f) * 4f * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
				src.X++;
				src.Y += 4;
				src.Width = 3;
				src.Height = 1;
				spriteBatch.Draw(texture, location + new Vector2(51f, 51f) * scaleSize + new Vector2(-8f, 4f) * scaleSize, new Rectangle?(src), color * transparency, 0f, new Vector2(1.5f, 2f) * 4f * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
				ParsedItemData dataOrErrorItem2 = ItemRegistry.GetDataOrErrorItem(this.combinedRings[1].QualifiedItemId);
				texture = dataOrErrorItem2.GetTexture();
				src = dataOrErrorItem2.GetSourceRect(0, null).Clone();
				src.X += 9;
				src.Y += 7;
				src.Width = 4;
				src.Height = 6;
				spriteBatch.Draw(texture, location + new Vector2(51f, 51f) * scaleSize + new Vector2(4f, 8f) * scaleSize, new Rectangle?(src), color * transparency, 0f, new Vector2(1.5f, 2f) * 4f * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
				src.Y += 4;
				src.Width = 3;
				src.Height = 1;
				spriteBatch.Draw(texture, location + new Vector2(51f, 51f) * scaleSize + new Vector2(4f, 4f) * scaleSize, new Rectangle?(src), color * transparency, 0f, new Vector2(1.5f, 2f) * 4f * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
				Color? color2 = TailoringMenu.GetDyeColor(this.combinedRings[0]);
				Color? color3 = TailoringMenu.GetDyeColor(this.combinedRings[1]);
				Color color1noNull = Color.Red;
				Color color2noNull = Color.Blue;
				if (color2 != null)
				{
					color1noNull = color2.Value;
				}
				if (color3 != null)
				{
					color2noNull = color3.Value;
				}
				base.drawInMenu(spriteBatch, location + new Vector2(-5f, -1f), scaleSize, transparency, layerDepth, drawStackNumber, Utility.Get2PhaseColor(color1noNull, color2noNull, 0, 1f, 0f), drawShadow);
				spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(13f, 35f) * scaleSize, new Rectangle?(new Rectangle(263, 579, 4, 2)), Utility.Get2PhaseColor(color1noNull, color2noNull, 0, 1f, 1125f) * transparency, -1.5707964f, new Vector2(2f, 1.5f) * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
				spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(49f, 35f) * scaleSize, new Rectangle?(new Rectangle(263, 579, 4, 2)), Utility.Get2PhaseColor(color1noNull, color2noNull, 0, 1f, 375f) * transparency, 1.5707964f, new Vector2(2f, 1.5f) * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
				spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(31f, 53f) * scaleSize, new Rectangle?(new Rectangle(263, 579, 4, 2)), Utility.Get2PhaseColor(color1noNull, color2noNull, 0, 1f, 750f) * transparency, 3.1415927f, new Vector2(2f, 1.5f) * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
				this.DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
				return;
			}
			base.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
		}

		// Token: 0x06001E24 RID: 7716 RVA: 0x0015A2C8 File Offset: 0x001584C8
		public override void update(GameTime time, GameLocation environment, Farmer who)
		{
			foreach (Ring ring in this.combinedRings)
			{
				ring.update(time, environment, who);
			}
			base.update(time, environment, who);
		}

		// Token: 0x06001E25 RID: 7717 RVA: 0x0015A324 File Offset: 0x00158524
		protected virtual void OnCombinedRingsChanged()
		{
			this.description = null;
		}

		// Token: 0x04001288 RID: 4744
		public NetList<Ring, NetRef<Ring>> combinedRings = new NetList<Ring, NetRef<Ring>>();
	}
}

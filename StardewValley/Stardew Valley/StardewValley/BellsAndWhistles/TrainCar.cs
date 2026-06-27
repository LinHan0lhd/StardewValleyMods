using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x020003AE RID: 942
	public class TrainCar : INetObject<NetFields>
	{
		// Token: 0x170004A4 RID: 1188
		// (get) Token: 0x06003928 RID: 14632 RVA: 0x002D56C1 File Offset: 0x002D38C1
		public NetFields NetFields { get; } = new NetFields("TrainCar");

		// Token: 0x06003929 RID: 14633 RVA: 0x002D56CC File Offset: 0x002D38CC
		[Obsolete("This constructor is for deserialization and shouldn't be called directly.")]
		public TrainCar()
		{
			this.initNetFields();
		}

		// Token: 0x0600392A RID: 14634 RVA: 0x002D5744 File Offset: 0x002D3944
		public TrainCar(Random random, int carType, int frontDecal, Color color, int resourceType = 0, int loaded = 0) : this()
		{
			this.carType.Value = carType;
			this.frontDecal.Value = frontDecal;
			this.color.Value = color;
			this.resourceType.Value = resourceType;
			this.loaded.Value = loaded;
			if (carType != 0 && carType != 1)
			{
				this.color.Value = Color.White;
			}
			if (carType != 0)
			{
				if (carType != 2)
				{
					return;
				}
				if (random.NextBool())
				{
					this.alternateCar.Value = true;
				}
			}
			else if (!color.Equals(Color.DimGray))
			{
				for (int i = 0; i < this.topFeatures.Count; i++)
				{
					if (random.NextDouble() < 0.2)
					{
						this.topFeatures[i] = random.Next(2);
					}
					else
					{
						this.topFeatures[i] = -1;
					}
				}
				return;
			}
		}

		// Token: 0x0600392B RID: 14635 RVA: 0x002D5824 File Offset: 0x002D3A24
		private void initNetFields()
		{
			this.NetFields.SetOwner(this).AddField(this.frontDecal, "frontDecal").AddField(this.carType, "carType").AddField(this.resourceType, "resourceType").AddField(this.loaded, "loaded").AddField(this.topFeatures, "topFeatures").AddField(this.alternateCar, "alternateCar").AddField(this.color, "color");
		}

		// Token: 0x0600392C RID: 14636 RVA: 0x002D58B0 File Offset: 0x002D3AB0
		public void draw(SpriteBatch b, Vector2 globalPosition, float wheelRotation, GameLocation location)
		{
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, globalPosition), new Rectangle?(new Rectangle(192 + this.carType.Value * 128, 512 - (this.alternateCar.Value ? 64 : 0), 128, 57)), this.color.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, (globalPosition.Y + 256f) / 10000f);
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, globalPosition + new Vector2(0f, 228f)), new Rectangle?(new Rectangle(192 + this.carType.Value * 128, 569, 128, 7)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (globalPosition.Y + 256f) / 10000f);
			switch (this.carType.Value)
			{
			case 0:
				for (int i = 0; i < this.topFeatures.Count; i += 64)
				{
					if (this.topFeatures[i] != -1)
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, globalPosition + new Vector2((float)(64 + i), 20f)), new Rectangle?(new Rectangle(192, 608 + this.topFeatures[i] * 16, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (globalPosition.Y + 260f) / 10000f);
					}
				}
				this.DrawFrontDecal(b, globalPosition);
				return;
			case 1:
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, globalPosition), new Rectangle?(new Rectangle(448 + this.resourceType.Value * 128 % 256, 576 + this.resourceType.Value / 2 * 32, 128, 32)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (globalPosition.Y + 260f) / 10000f);
				if (this.loaded.Value > 0 && Game1.random.NextDouble() < 0.02 && globalPosition.X > 320f && globalPosition.X < (float)(location.map.DisplayWidth - 256))
				{
					NetInt netInt = this.loaded;
					int value = netInt.Value;
					netInt.Value = value - 1;
					string debrisId = null;
					switch (this.resourceType.Value)
					{
					case 0:
						debrisId = "(O)382";
						break;
					case 1:
						debrisId = ((this.color.R > this.color.G) ? "(O)378" : ((this.color.G > this.color.B) ? "(O)380" : ((this.color.B > this.color.R) ? "(O)384" : "(O)378")));
						break;
					case 2:
						debrisId = ((Game1.random.NextDouble() < 0.05) ? "(O)709" : "(O)388");
						break;
					case 6:
						debrisId = "(O)390";
						break;
					case 7:
						debrisId = (location.IsWinterHere() ? "(O)536" : ((Game1.stats.DaysPlayed > 120U && this.color.R > this.color.G) ? "(O)537" : "(O)535"));
						break;
					case 9:
						if (Utility.tryRollMysteryBox(0.02, null))
						{
							debrisId = "(O)MysteryBox";
						}
						break;
					}
					if (debrisId != null)
					{
						Game1.createObjectDebris(debrisId, (int)globalPosition.X / 64 + 2, (int)globalPosition.Y / 64, (int)(globalPosition.Y + 320f), 0, 1f, null);
					}
					if (Game1.random.NextDouble() < 0.01)
					{
						Game1.createItemDebris(ItemRegistry.Create("(B)806", 1, 0, false), new Vector2((float)((int)globalPosition.X + 128), (float)((int)globalPosition.Y)), (int)(globalPosition.Y + 320f), null, -1, false);
					}
				}
				this.DrawFrontDecal(b, globalPosition);
				return;
			case 2:
				break;
			case 3:
			{
				Vector2 backWheel = Game1.GlobalToLocal(Game1.viewport, globalPosition + new Vector2(72f, 208f));
				Vector2 frontWheel = Game1.GlobalToLocal(Game1.viewport, globalPosition + new Vector2(316f, 208f));
				b.Draw(Game1.mouseCursors, backWheel, new Rectangle?(new Rectangle(192, 576, 20, 20)), Color.White, wheelRotation, new Vector2(10f, 10f), 4f, SpriteEffects.None, (globalPosition.Y + 260f) / 10000f);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, globalPosition + new Vector2(228f, 208f)), new Rectangle?(new Rectangle(192, 576, 20, 20)), Color.White, wheelRotation, new Vector2(10f, 10f), 4f, SpriteEffects.None, (globalPosition.Y + 260f) / 10000f);
				b.Draw(Game1.mouseCursors, frontWheel, new Rectangle?(new Rectangle(192, 576, 20, 20)), Color.White, wheelRotation, new Vector2(10f, 10f), 4f, SpriteEffects.None, (globalPosition.Y + 260f) / 10000f);
				int startX = (int)((double)(backWheel.X + 4f) + 24.0 * Math.Cos((double)wheelRotation));
				int startY = (int)((double)(backWheel.Y + 4f) + 24.0 * Math.Sin((double)wheelRotation));
				int endX = (int)((double)(frontWheel.X + 4f) + 24.0 * Math.Cos((double)wheelRotation));
				int endY = (int)((double)(frontWheel.Y + 4f) + 24.0 * Math.Sin((double)wheelRotation));
				Utility.drawLineWithScreenCoordinates(startX, startY, endX, endY, b, new Color(112, 98, 92), (globalPosition.Y + 264f) / 10000f, 1);
				Utility.drawLineWithScreenCoordinates(startX, startY + 2, endX, endY + 2, b, new Color(112, 98, 92), (globalPosition.Y + 264f) / 10000f, 1);
				Utility.drawLineWithScreenCoordinates(startX, startY + 4, endX, endY + 4, b, new Color(53, 46, 43), (globalPosition.Y + 264f) / 10000f, 1);
				Utility.drawLineWithScreenCoordinates(startX, startY + 6, endX, endY + 6, b, new Color(53, 46, 43), (globalPosition.Y + 264f) / 10000f, 1);
				b.Draw(Game1.mouseCursors, new Vector2((float)(startX - 8), (float)(startY - 8)), new Rectangle?(new Rectangle(192, 640, 24, 24)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (globalPosition.Y + 268f) / 10000f);
				b.Draw(Game1.mouseCursors, new Vector2((float)(endX - 8), (float)(endY - 8)), new Rectangle?(new Rectangle(192, 640, 24, 24)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (globalPosition.Y + 268f) / 10000f);
				break;
			}
			default:
				return;
			}
		}

		// Token: 0x0600392D RID: 14637 RVA: 0x002D6098 File Offset: 0x002D4298
		private void DrawFrontDecal(SpriteBatch b, Vector2 globalPosition)
		{
			if (this.frontDecal.Value == 35)
			{
				b.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(Game1.viewport, globalPosition + new Vector2(192f, 92f)), new Rectangle?(new Rectangle(480, 480, 32, 32)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (globalPosition.Y + 260f) / 10000f);
				return;
			}
			if (this.frontDecal.Value != -1 && this.frontDecal.Value < 35)
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, globalPosition + new Vector2(192f, 92f)), new Rectangle?(new Rectangle(224 + this.frontDecal.Value * 32 % 224, 576 + this.frontDecal.Value * 32 / 224 * 32, 32, 32)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (globalPosition.Y + 260f) / 10000f);
			}
		}

		// Token: 0x040025D6 RID: 9686
		public const int spotsForTopFeatures = 6;

		// Token: 0x040025D7 RID: 9687
		public const double chanceForTopFeature = 0.2;

		// Token: 0x040025D8 RID: 9688
		public const int engine = 3;

		// Token: 0x040025D9 RID: 9689
		public const int passengerCar = 2;

		// Token: 0x040025DA RID: 9690
		public const int coalCar = 1;

		// Token: 0x040025DB RID: 9691
		public const int plainCar = 0;

		// Token: 0x040025DC RID: 9692
		public const int coal = 0;

		// Token: 0x040025DD RID: 9693
		public const int metal = 1;

		// Token: 0x040025DE RID: 9694
		public const int wood = 2;

		// Token: 0x040025DF RID: 9695
		public const int compartments = 3;

		// Token: 0x040025E0 RID: 9696
		public const int grass = 4;

		// Token: 0x040025E1 RID: 9697
		public const int hay = 5;

		// Token: 0x040025E2 RID: 9698
		public const int bricks = 6;

		// Token: 0x040025E3 RID: 9699
		public const int rocks = 7;

		// Token: 0x040025E4 RID: 9700
		public const int packages = 8;

		// Token: 0x040025E5 RID: 9701
		public const int presents = 9;

		// Token: 0x040025E6 RID: 9702
		public readonly NetInt frontDecal = new NetInt();

		// Token: 0x040025E7 RID: 9703
		public readonly NetInt carType = new NetInt();

		// Token: 0x040025E8 RID: 9704
		public readonly NetInt resourceType = new NetInt();

		// Token: 0x040025E9 RID: 9705
		public readonly NetInt loaded = new NetInt();

		// Token: 0x040025EA RID: 9706
		public readonly NetArray<int, NetInt> topFeatures = new NetArray<int, NetInt>(6);

		// Token: 0x040025EB RID: 9707
		public readonly NetBool alternateCar = new NetBool();

		// Token: 0x040025EC RID: 9708
		public readonly NetColor color = new NetColor();
	}
}

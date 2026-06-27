using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x020003AD RID: 941
	public class Train : INetObject<NetFields>
	{
		// Token: 0x170004A3 RID: 1187
		// (get) Token: 0x06003922 RID: 14626 RVA: 0x002D4F97 File Offset: 0x002D3197
		public NetFields NetFields { get; } = new NetFields("Train");

		// Token: 0x06003923 RID: 14627 RVA: 0x002D4FA0 File Offset: 0x002D31A0
		public Train()
		{
			this.initNetFields();
			Random r = Game1.random;
			if (r.NextDouble() < 0.1)
			{
				this.type.Value = 3;
			}
			else if (r.NextDouble() < 0.1)
			{
				this.type.Value = 1;
			}
			else if (r.NextDouble() < 0.1)
			{
				this.type.Value = 2;
			}
			else if (r.NextDouble() < 0.05)
			{
				this.type.Value = 5;
			}
			else if (Game1.IsWinter && r.NextDouble() < 0.2)
			{
				this.type.Value = 6;
			}
			else
			{
				this.type.Value = 0;
			}
			int numCars = r.Next(8, 25);
			if (r.NextDouble() < 0.1)
			{
				numCars *= 2;
			}
			this.speed = 0.2f;
			this.smokeTimer = this.speed * 2000f;
			Color color = Color.White;
			double chanceForPassengerCar = 1.0;
			double chanceForCoalCar = 1.0;
			switch (this.type.Value)
			{
			case 0:
				chanceForPassengerCar = 0.2;
				chanceForCoalCar = 0.2;
				break;
			case 1:
				chanceForPassengerCar = 0.0;
				chanceForCoalCar = 0.0;
				color = Color.DimGray;
				break;
			case 2:
				chanceForPassengerCar = 0.0;
				chanceForCoalCar = 0.7;
				break;
			case 3:
				chanceForPassengerCar = 1.0;
				chanceForCoalCar = 0.0;
				this.speed = 0.4f;
				break;
			case 5:
				chanceForCoalCar = 0.0;
				chanceForPassengerCar = 0.0;
				color = Color.MediumBlue;
				this.speed = 0.4f;
				break;
			case 6:
				chanceForPassengerCar = 0.0;
				chanceForCoalCar = 1.0;
				color = Color.Red;
				break;
			}
			this.cars.Add(new TrainCar(r, 3, -1, Color.White, 0, 0));
			for (int i = 1; i < numCars; i++)
			{
				int whichCar = 0;
				if (r.NextDouble() < chanceForPassengerCar)
				{
					whichCar = 2;
				}
				else if (r.NextDouble() < chanceForCoalCar)
				{
					whichCar = 1;
				}
				Color carColor = color;
				if (color.Equals(Color.White))
				{
					bool redTint = false;
					bool greenTint = false;
					bool blueTint = false;
					switch (r.Next(3))
					{
					case 0:
						redTint = true;
						break;
					case 1:
						greenTint = true;
						break;
					case 2:
						blueTint = true;
						break;
					}
					carColor = new Color(r.Next(redTint ? 0 : 100, 250), r.Next(greenTint ? 0 : 100, 250), r.Next(blueTint ? 0 : 100, 250));
				}
				int value = this.type.Value;
				int frontDecal;
				if (value != 1)
				{
					if (value != 5)
					{
						if (value != 6)
						{
							frontDecal = ((r.NextDouble() < 0.3) ? r.Next(36) : -1);
						}
						else
						{
							frontDecal = -1;
						}
					}
					else
					{
						frontDecal = 1;
					}
				}
				else
				{
					frontDecal = 2;
				}
				int resourceType = 0;
				if (whichCar == 1)
				{
					resourceType = r.Next(9);
					if (this.type.Value == 6)
					{
						resourceType = 9;
					}
				}
				this.cars.Add(new TrainCar(r, whichCar, frontDecal, carColor, resourceType, r.Next(4, 10)));
			}
		}

		// Token: 0x06003924 RID: 14628 RVA: 0x002D5348 File Offset: 0x002D3548
		private void initNetFields()
		{
			this.NetFields.SetOwner(this).AddField(this.cars, "cars").AddField(this.type, "type").AddField(this.position.NetFields, "position.NetFields");
		}

		// Token: 0x06003925 RID: 14629 RVA: 0x002D5398 File Offset: 0x002D3598
		public Rectangle getBoundingBox()
		{
			return new Rectangle(-this.cars.Count * 128 * 4 + (int)this.position.X, 2720, this.cars.Count * 128 * 4, 128);
		}

		// Token: 0x06003926 RID: 14630 RVA: 0x002D53E8 File Offset: 0x002D35E8
		public bool Update(GameTime time, GameLocation location)
		{
			if (Game1.IsMasterGame)
			{
				this.position.X += (float)time.ElapsedGameTime.Milliseconds * this.speed;
			}
			this.wheelRotation += (float)time.ElapsedGameTime.Milliseconds * 0.012271847f;
			this.wheelRotation %= 6.2831855f;
			if (!Game1.eventUp && location.Equals(Game1.currentLocation))
			{
				Farmer player = Game1.player;
				Rectangle playerBounds = player.GetBoundingBox();
				Rectangle trainBounds = this.getBoundingBox();
				if (playerBounds.Intersects(trainBounds))
				{
					player.xVelocity = 8f;
					player.yVelocity = (float)(trainBounds.Center.Y - playerBounds.Center.Y) / 4f;
					player.takeDamage(20, true, null);
					if (player.UsingTool)
					{
						Game1.playSound("clank", null);
					}
				}
			}
			if (Game1.random.NextDouble() < 0.001 && location.Equals(Game1.currentLocation))
			{
				Game1.playSound("trainWhistle", null);
				this.whistleSteam = new TemporaryAnimatedSprite(27, new Vector2(this.position.X - 250f, 2624f), Color.White, 8, false, 100f, 0, 64, 1f, 64, 0);
			}
			if (this.whistleSteam != null)
			{
				this.whistleSteam.Position = new Vector2(this.position.X - 258f, 2592f);
				if (this.whistleSteam.update(time))
				{
					this.whistleSteam = null;
				}
			}
			this.smokeTimer -= (float)time.ElapsedGameTime.Milliseconds;
			if (this.smokeTimer <= 0f)
			{
				location.temporarySprites.Add(new TemporaryAnimatedSprite(25, new Vector2(this.position.X - 170f, 2496f), Color.White, 8, false, 100f, 0, 64, 1f, 128, 0));
				this.smokeTimer = this.speed * 2000f;
			}
			return this.position.X > (float)(this.cars.Count * 128 * 4 + 4480);
		}

		// Token: 0x06003927 RID: 14631 RVA: 0x002D564C File Offset: 0x002D384C
		public void draw(SpriteBatch b, GameLocation location)
		{
			for (int i = 0; i < this.cars.Count; i++)
			{
				this.cars[i].draw(b, new Vector2(this.position.X - (float)((i + 1) * 512), 2592f), this.wheelRotation, location);
			}
			TemporaryAnimatedSprite temporaryAnimatedSprite = this.whistleSteam;
			if (temporaryAnimatedSprite == null)
			{
				return;
			}
			temporaryAnimatedSprite.draw(b, false, 0, 0, 1f);
		}

		// Token: 0x040025C4 RID: 9668
		public const int minCars = 8;

		// Token: 0x040025C5 RID: 9669
		public const int maxCars = 24;

		// Token: 0x040025C6 RID: 9670
		public const double chanceForLongTrain = 0.1;

		// Token: 0x040025C7 RID: 9671
		public const int randomTrain = 0;

		// Token: 0x040025C8 RID: 9672
		public const int jojaTrain = 1;

		// Token: 0x040025C9 RID: 9673
		public const int coalTrain = 2;

		// Token: 0x040025CA RID: 9674
		public const int passengerTrain = 3;

		// Token: 0x040025CB RID: 9675
		public const int uniformColorPlainTrain = 4;

		// Token: 0x040025CC RID: 9676
		public const int prisonTrain = 5;

		// Token: 0x040025CD RID: 9677
		public const int christmasTrain = 6;

		// Token: 0x040025CE RID: 9678
		public readonly NetObjectList<TrainCar> cars = new NetObjectList<TrainCar>();

		// Token: 0x040025CF RID: 9679
		public readonly NetInt type = new NetInt();

		// Token: 0x040025D0 RID: 9680
		public readonly NetPosition position = new NetPosition();

		// Token: 0x040025D1 RID: 9681
		public float speed;

		// Token: 0x040025D2 RID: 9682
		public float wheelRotation;

		// Token: 0x040025D3 RID: 9683
		public float smokeTimer;

		// Token: 0x040025D4 RID: 9684
		private TemporaryAnimatedSprite whistleSteam;
	}
}

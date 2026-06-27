using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.Extensions;

namespace StardewValley.Companions
{
	// Token: 0x02000372 RID: 882
	public class FlyingCompanion : Companion
	{
		// Token: 0x060035F7 RID: 13815 RVA: 0x002A8134 File Offset: 0x002A6334
		public FlyingCompanion()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
			defaultInterpolatedStringHandler.AppendFormatted("FlyingCompanion");
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<int>(Game1.random.Next());
			this.lightId = defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x060035F8 RID: 13816 RVA: 0x002A81AC File Offset: 0x002A63AC
		public FlyingCompanion(int whichVariant, int whichSubVariant = -1) : this()
		{
			this.whichVariant.Value = whichVariant;
			this.whichSubVariant.Value = whichSubVariant;
			if (whichVariant == 1)
			{
				this.startingYForVariant.Value = 160;
				this.hasLight = false;
			}
		}

		// Token: 0x060035F9 RID: 13817 RVA: 0x002A81E7 File Offset: 0x002A63E7
		public override void InitNetFields()
		{
			base.InitNetFields();
			base.NetFields.AddField(this.whichSubVariant, "whichSubVariant").AddField(this.startingYForVariant, "startingYForVariant");
		}

		// Token: 0x060035FA RID: 13818 RVA: 0x002A8218 File Offset: 0x002A6418
		public override void Draw(SpriteBatch b)
		{
			Farmer owner = base.Owner;
			if (((owner != null) ? owner.currentLocation : null) == null || (base.Owner.currentLocation.DisplayName == "Temp" && !Game1.isFestival()))
			{
				return;
			}
			Texture2D texture = Game1.content.Load<Texture2D>("TileSheets\\companions");
			SpriteEffects effect = SpriteEffects.None;
			if (this.direction.Value == 1)
			{
				effect = SpriteEffects.FlipHorizontally;
			}
			if (!this.perching)
			{
				b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, -this.height * 4f) + this.extraPosition), new Rectangle?(new Rectangle(this.whichSubVariant.Value * 64 + (int)(this.flitTimer / (float)(500 / this.flapAnimationLength)) * 16, this.startingYForVariant.Value, 16, 16)), Color.White, 0f, new Vector2(8f, 8f), 4f, effect, this._position.Y / 10000f);
				b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(this.extraPosition.X, 0f)), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f * Utility.Lerp(1f, 0.8f, Math.Min(this.height, 1f)), SpriteEffects.None, (this._position.Y - 8f) / 10000f - 2E-06f);
				return;
			}
			if (this.parrot_squatTimer > 0f)
			{
				b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, -this.height * 4f) + this.extraPosition), new Rectangle?(new Rectangle((int)(this.parrot_squatTimer % 1000f) / 500 * 16 + 128, this.startingYForVariant.Value, 16, 16)), Color.White, 0f, new Vector2(8f, 8f), 4f, effect, this._position.Y / 10000f);
				return;
			}
			if (this.parrot_squawkTimer > 0f)
			{
				b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, -this.height * 4f) + this.extraPosition), new Rectangle?(new Rectangle(160, this.startingYForVariant.Value, 16, 16)), Color.White, 0f, new Vector2(8f, 8f), 4f, effect, this._position.Y / 10000f);
				return;
			}
			b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, -this.height * 4f) + this.extraPosition), new Rectangle?(new Rectangle(128, this.startingYForVariant.Value, 16, 16)), Color.White, 0f, new Vector2(8f, 8f), 4f, effect, this._position.Y / 10000f);
		}

		// Token: 0x060035FB RID: 13819 RVA: 0x002A8614 File Offset: 0x002A6814
		public override void Update(GameTime time, GameLocation location)
		{
			base.Update(time, location);
			this.height = 32f;
			this.flitTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
			if (this.flitTimer > (float)(this.flapAnimationLength * 125))
			{
				this.flitTimer = 0f;
				this.extraPositionMotion = new Vector2((Game1.random.NextDouble() < 0.5) ? 0.1f : -0.1f, -2f);
				if (this.extraPositionMotion.X < 0f)
				{
					this.currentSidewaysFlap--;
				}
				else
				{
					this.currentSidewaysFlap++;
				}
				if (this.currentSidewaysFlap < -4 || this.currentSidewaysFlap > 4)
				{
					this.extraPositionMotion.X = this.extraPositionMotion.X * -1f;
				}
				this.extraPositionAcceleration = new Vector2(0f, this.floatup ? 0.13f : 0.14f);
				if (this.extraPosition.Y > 8f)
				{
					this.floatup = true;
				}
				else if (this.extraPosition.Y < -8f)
				{
					this.floatup = false;
				}
			}
			if (!this.perching)
			{
				this.extraPosition += this.extraPositionMotion;
				this.extraPositionMotion += this.extraPositionAcceleration;
			}
			if (this.hasLight && location.Equals(Game1.currentLocation))
			{
				Utility.repositionLightSource(this.lightId, base.Position - new Vector2(0f, this.height * 4f) + this.extraPosition);
			}
			if (this.whichVariant.Value == 1)
			{
				if (this.lerp <= 0f)
				{
					this.timeSinceLastZeroLerp += (float)time.ElapsedGameTime.TotalMilliseconds;
				}
				else
				{
					this.timeSinceLastZeroLerp = 0f;
				}
				this.whichSubVariant.Value = ((this.timeSinceLastZeroLerp >= 100f) ? 1 : 0);
				if (this.timeSinceLastZeroLerp > 2000f)
				{
					if (this.perching || (Math.Abs(base.OwnerPosition.X - (base.Position.X + this.extraPosition.X)) < 8f && Math.Abs(base.OwnerPosition.Y - (base.Position.Y + this.extraPosition.Y)) < 8f))
					{
						if (this.perching && !(base.Owner.Position + new Vector2(32f, 20f)).Equals(base.Position))
						{
							this.perching = false;
							this.timeSinceLastZeroLerp = 0f;
							this.parrot_squatTimer = 0f;
							this.parrot_squawkTimer = 0f;
							return;
						}
						if (this.parrot_squawkTimer > 0f)
						{
							this.parrot_squawkTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
						}
						if (this.parrot_squatTimer > 0f)
						{
							this.parrot_squatTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
						}
						this.perching = true;
						base.Position = base.Owner.Position + new Vector2(32f, 20f);
						this.extraPosition = Vector2.Zero;
						this.endPosition = base.Position;
						if (Game1.random.NextDouble() < 0.0005 && this.parrot_squawkTimer <= 0f)
						{
							this.parrot_squawkTimer = 500f;
							location.localSound("parrot_squawk", null, null, SoundContext.Default);
							return;
						}
						if (Game1.random.NextDouble() < 0.0015 && this.parrot_squatTimer <= 0f)
						{
							this.parrot_squatTimer = (float)(Game1.random.Next(2, 6) * 1000);
							return;
						}
					}
				}
				else
				{
					this.perching = false;
				}
			}
		}

		// Token: 0x060035FC RID: 13820 RVA: 0x002A8A34 File Offset: 0x002A6C34
		public override void InitializeCompanion(Farmer farmer)
		{
			base.InitializeCompanion(farmer);
			if (this.hasLight)
			{
				Game1.currentLightSources.Add(new LightSource(this.lightId, 1, base.Position, 2f, Color.Black, LightSource.LightContext.None, 0L, null));
			}
			if (this.whichSubVariant.Value == -1)
			{
				Random r = Utility.CreateRandom((double)farmer.uniqueMultiplayerID.Value, 0.0, 0.0, 0.0, 0.0);
				this.whichSubVariant.Value = r.Next(4);
				if (this.whichVariant.Value == 0 && r.NextDouble() < 0.5)
				{
					this.startingYForVariant.Value += 176;
				}
			}
		}

		// Token: 0x060035FD RID: 13821 RVA: 0x002A8B05 File Offset: 0x002A6D05
		public override void CleanupCompanion()
		{
			base.CleanupCompanion();
			if (this.hasLight)
			{
				Utility.removeLightSource(this.lightId);
			}
		}

		// Token: 0x060035FE RID: 13822 RVA: 0x002A8B24 File Offset: 0x002A6D24
		public override void OnOwnerWarp()
		{
			base.OnOwnerWarp();
			this.extraPosition = Vector2.Zero;
			this.extraPositionMotion = Vector2.Zero;
			this.extraPositionAcceleration = Vector2.Zero;
			if (this.hasLight)
			{
				Game1.currentLightSources.Add(new LightSource(this.lightId, 1, base.Position, 2f, Color.Black, LightSource.LightContext.None, 0L, null));
			}
		}

		// Token: 0x060035FF RID: 13823 RVA: 0x002A8B8A File Offset: 0x002A6D8A
		public override void Hop(float amount)
		{
		}

		// Token: 0x04002357 RID: 9047
		public const int VARIANT_FAIRY = 0;

		// Token: 0x04002358 RID: 9048
		public const int VARIANT_PARROT = 1;

		// Token: 0x04002359 RID: 9049
		private float flitTimer;

		// Token: 0x0400235A RID: 9050
		private Vector2 extraPosition;

		// Token: 0x0400235B RID: 9051
		private Vector2 extraPositionMotion;

		// Token: 0x0400235C RID: 9052
		private Vector2 extraPositionAcceleration;

		// Token: 0x0400235D RID: 9053
		private bool floatup;

		// Token: 0x0400235E RID: 9054
		private int flapAnimationLength = 4;

		// Token: 0x0400235F RID: 9055
		private int currentSidewaysFlap;

		// Token: 0x04002360 RID: 9056
		private bool hasLight = true;

		// Token: 0x04002361 RID: 9057
		private string lightId;

		// Token: 0x04002362 RID: 9058
		private NetInt whichSubVariant = new NetInt(-1);

		// Token: 0x04002363 RID: 9059
		private NetInt startingYForVariant = new NetInt(0);

		// Token: 0x04002364 RID: 9060
		private bool perching;

		// Token: 0x04002365 RID: 9061
		private float timeSinceLastZeroLerp;

		// Token: 0x04002366 RID: 9062
		private float parrot_squawkTimer;

		// Token: 0x04002367 RID: 9063
		private float parrot_squatTimer;
	}
}

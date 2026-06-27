using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;

namespace StardewValley.Companions
{
	// Token: 0x02000371 RID: 881
	public class Companion : INetObject<NetFields>
	{
		// Token: 0x17000466 RID: 1126
		// (get) Token: 0x060035E8 RID: 13800 RVA: 0x002A7C7C File Offset: 0x002A5E7C
		public NetFields NetFields { get; } = new NetFields("Companion");

		// Token: 0x17000467 RID: 1127
		// (get) Token: 0x060035E9 RID: 13801 RVA: 0x002A7C84 File Offset: 0x002A5E84
		// (set) Token: 0x060035EA RID: 13802 RVA: 0x002A7C91 File Offset: 0x002A5E91
		public Farmer Owner
		{
			get
			{
				return this._owner.Value;
			}
			set
			{
				this._owner.Value = value;
			}
		}

		// Token: 0x17000468 RID: 1128
		// (get) Token: 0x060035EB RID: 13803 RVA: 0x002A7C9F File Offset: 0x002A5E9F
		// (set) Token: 0x060035EC RID: 13804 RVA: 0x002A7CAC File Offset: 0x002A5EAC
		public Vector2 Position
		{
			get
			{
				return this._position.Value;
			}
			set
			{
				this._position.Value = value;
			}
		}

		// Token: 0x17000469 RID: 1129
		// (get) Token: 0x060035ED RID: 13805 RVA: 0x002A7CBC File Offset: 0x002A5EBC
		public Vector2 OwnerPosition
		{
			get
			{
				return Utility.PointToVector2(this.Owner.GetBoundingBox().Center);
			}
		}

		// Token: 0x1700046A RID: 1130
		// (get) Token: 0x060035EE RID: 13806 RVA: 0x002A7CE1 File Offset: 0x002A5EE1
		public bool IsLocal
		{
			get
			{
				return this.Owner.IsLocalPlayer;
			}
		}

		// Token: 0x060035EF RID: 13807 RVA: 0x002A7CF0 File Offset: 0x002A5EF0
		public Companion()
		{
			this.InitNetFields();
			this.direction.Value = 1;
		}

		// Token: 0x060035F0 RID: 13808 RVA: 0x002A7D67 File Offset: 0x002A5F67
		public virtual void InitializeCompanion(Farmer farmer)
		{
			this._owner.Value = farmer;
			this._position.Value = farmer.Position;
		}

		// Token: 0x060035F1 RID: 13809 RVA: 0x002A7D86 File Offset: 0x002A5F86
		public virtual void CleanupCompanion()
		{
			this._owner.Value = null;
		}

		// Token: 0x060035F2 RID: 13810 RVA: 0x002A7D94 File Offset: 0x002A5F94
		public virtual void InitNetFields()
		{
			this.NetFields.SetOwner(this).AddField(this._owner.NetFields, "_owner.NetFields").AddField(this._position.NetFields, "_position.NetFields").AddField(this.hopEvent, "hopEvent").AddField(this.direction, "direction").AddField(this.whichVariant, "whichVariant");
			this.hopEvent.onEvent += this.Hop;
		}

		// Token: 0x060035F3 RID: 13811 RVA: 0x002A7E20 File Offset: 0x002A6020
		public virtual void Hop(float amount)
		{
			this.height = 0f;
			this.gravity = amount;
		}

		// Token: 0x060035F4 RID: 13812 RVA: 0x002A7E34 File Offset: 0x002A6034
		public virtual void Update(GameTime time, GameLocation location)
		{
			if (this.IsLocal)
			{
				if (this.lerp < 0f)
				{
					if ((this.OwnerPosition - this.Position).Length() > 768f)
					{
						Utility.addRainbowStarExplosion(location, this.Position + new Vector2(0f, -this.height), 1);
						this.Position = this.Owner.Position;
						this.lerp = -1f;
					}
					if ((this.OwnerPosition - this.Position).Length() > 80f)
					{
						this.startPosition = this.Position;
						float radius = 0.33f;
						this.endPosition = this.OwnerPosition + new Vector2(Utility.RandomFloat(-64f, 64f, null) * radius, Utility.RandomFloat(-64f, 64f, null) * radius);
						if (location.isCollidingPosition(new Rectangle((int)this.endPosition.X - 8, (int)this.endPosition.Y - 8, 16, 16), Game1.viewport, false, 0, false, null, true, false, true, false))
						{
							this.endPosition = this.OwnerPosition;
						}
						this.lerp = 0f;
						this.hopEvent.Fire(1f);
						if (Math.Abs(this.OwnerPosition.X - this.Position.X) > 8f)
						{
							if (this.OwnerPosition.X > this.Position.X)
							{
								this.direction.Value = 1;
							}
							else
							{
								this.direction.Value = 3;
							}
						}
					}
				}
				if (this.lerp >= 0f)
				{
					this.lerp += (float)time.ElapsedGameTime.TotalSeconds / 0.4f;
					if (this.lerp > 1f)
					{
						this.lerp = 1f;
					}
					float x = Utility.Lerp(this.startPosition.X, this.endPosition.X, this.lerp);
					float y = Utility.Lerp(this.startPosition.Y, this.endPosition.Y, this.lerp);
					this.Position = new Vector2(x, y);
					if (this.lerp == 1f)
					{
						this.lerp = -1f;
					}
				}
			}
			this.hopEvent.Poll();
			if (this.gravity != 0f || this.height != 0f)
			{
				this.height += this.gravity;
				this.gravity -= (float)time.ElapsedGameTime.TotalSeconds * 6f;
				if (this.height <= 0f)
				{
					this.height = 0f;
					this.gravity = 0f;
				}
			}
		}

		// Token: 0x060035F5 RID: 13813 RVA: 0x002A8112 File Offset: 0x002A6312
		public virtual void Draw(SpriteBatch b)
		{
		}

		// Token: 0x060035F6 RID: 13814 RVA: 0x002A8114 File Offset: 0x002A6314
		public virtual void OnOwnerWarp()
		{
			this._position.Value = this._owner.Value.Position;
		}

		// Token: 0x0400234D RID: 9037
		public readonly NetInt direction = new NetInt();

		// Token: 0x0400234E RID: 9038
		protected readonly NetPosition _position = new NetPosition();

		// Token: 0x0400234F RID: 9039
		protected readonly NetFarmerRef _owner = new NetFarmerRef();

		// Token: 0x04002350 RID: 9040
		public readonly NetInt whichVariant = new NetInt();

		// Token: 0x04002351 RID: 9041
		public float lerp = -1f;

		// Token: 0x04002352 RID: 9042
		public Vector2 startPosition;

		// Token: 0x04002353 RID: 9043
		public Vector2 endPosition;

		// Token: 0x04002354 RID: 9044
		public float height;

		// Token: 0x04002355 RID: 9045
		public float gravity;

		// Token: 0x04002356 RID: 9046
		public NetEvent1Field<float, NetFloat> hopEvent = new NetEvent1Field<float, NetFloat>();
	}
}

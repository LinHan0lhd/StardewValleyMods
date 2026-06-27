using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.GameData.Buildings;
using StardewValley.Tools;

namespace StardewValley.Buildings
{
	// Token: 0x02000388 RID: 904
	public class PetBowl : Building
	{
		// Token: 0x060037CA RID: 14282 RVA: 0x002C3AB5 File Offset: 0x002C1CB5
		public PetBowl(Vector2 tileLocation) : base("Pet Bowl", tileLocation)
		{
		}

		// Token: 0x060037CB RID: 14283 RVA: 0x002C3AD9 File Offset: 0x002C1CD9
		public PetBowl() : this(Vector2.Zero)
		{
		}

		// Token: 0x060037CC RID: 14284 RVA: 0x002C3AE6 File Offset: 0x002C1CE6
		public virtual void AssignPet(Pet pet)
		{
			this.petId.Value = pet.petId.Value;
			pet.homeLocationName.Value = this.parentLocationName.Value;
		}

		// Token: 0x060037CD RID: 14285 RVA: 0x002C3B14 File Offset: 0x002C1D14
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.watered, "watered").AddField(this.petId, "petId");
		}

		// Token: 0x060037CE RID: 14286 RVA: 0x002C3B43 File Offset: 0x002C1D43
		public virtual Point GetPetSpot()
		{
			return new Point(this.tileX.Value, this.tileY.Value + 1);
		}

		// Token: 0x060037CF RID: 14287 RVA: 0x002C3B64 File Offset: 0x002C1D64
		public override bool doAction(Vector2 tileLocation, Farmer who)
		{
			if (!this.isTilePassable(tileLocation))
			{
				Guid value = this.petId.Value;
				Pet p = Utility.findPet(this.petId.Value);
				if (p != null)
				{
					this.nameTimer = 3500;
					this.nameTimerMessage = Game1.content.LoadString("Strings\\1_6_Strings:PetBowlName", p.displayName);
				}
			}
			return base.doAction(tileLocation, who);
		}

		// Token: 0x060037D0 RID: 14288 RVA: 0x002C3BC8 File Offset: 0x002C1DC8
		public override void Update(GameTime time)
		{
			if (this.nameTimer > 0)
			{
				this.nameTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			}
			base.Update(time);
		}

		// Token: 0x060037D1 RID: 14289 RVA: 0x002C3C04 File Offset: 0x002C1E04
		public override void performToolAction(Tool t, int tileX, int tileY)
		{
			if (t is WateringCan)
			{
				string value = null;
				if (this.doesTileHaveProperty(tileX, tileY, "PetBowl", "Buildings", ref value))
				{
					this.watered.Value = true;
				}
			}
			base.performToolAction(t, tileX, tileY);
		}

		// Token: 0x060037D2 RID: 14290 RVA: 0x002C3C46 File Offset: 0x002C1E46
		public bool HasPet()
		{
			return this.petId.Value != Guid.Empty;
		}

		// Token: 0x060037D3 RID: 14291 RVA: 0x002C3C60 File Offset: 0x002C1E60
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (base.isMoving || base.isUnderConstruction(true))
			{
				return;
			}
			if (this.watered.Value)
			{
				BuildingData data = this.GetData();
				float sortY = (float)((this.tileY.Value + this.tilesHigh.Value) * 64);
				if (data != null)
				{
					sortY -= data.SortTileOffset * 64f;
				}
				sortY += 1.5f;
				sortY /= 10000f;
				Vector2 drawPosition = new Vector2((float)(this.tileX.Value * 64), (float)(this.tileY.Value * 64 + this.tilesHigh.Value * 64));
				Vector2 drawOffset = Vector2.Zero;
				if (data != null)
				{
					drawOffset = data.DrawOffset * 4f;
				}
				Rectangle sourceRect = this.getSourceRect();
				sourceRect.X += sourceRect.Width;
				Vector2 drawOrigin = new Vector2(0f, (float)sourceRect.Height);
				b.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, drawPosition + drawOffset), new Rectangle?(sourceRect), this.color * this.alpha, 0f, drawOrigin, 4f, SpriteEffects.None, sortY);
			}
			if (this.nameTimer > 0)
			{
				BuildingData data2 = this.GetData();
				float sortY2 = (float)((this.tileY.Value + this.tilesHigh.Value) * 64);
				if (data2 != null)
				{
					sortY2 -= data2.SortTileOffset * 64f;
				}
				sortY2 += 1.5f;
				sortY2 /= 10000f;
				SpriteText.drawSmallTextBubble(b, this.nameTimerMessage, Game1.GlobalToLocal(new Vector2(((float)this.tileX.Value + 1.5f) * 64f, (float)(this.tileY.Value * 64 - 32))), -1, sortY2 + 1E-06f, false);
			}
		}

		// Token: 0x04002443 RID: 9283
		[XmlElement("watered")]
		public readonly NetBool watered = new NetBool();

		// Token: 0x04002444 RID: 9284
		private int nameTimer;

		// Token: 0x04002445 RID: 9285
		private string nameTimerMessage;

		// Token: 0x04002446 RID: 9286
		[XmlElement("petGuid")]
		public readonly NetGuid petId = new NetGuid();
	}
}

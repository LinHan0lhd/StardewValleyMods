using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.GameData.Machines;
using StardewValley.Locations;
using StardewValley.Tools;

namespace StardewValley.Objects
{
	// Token: 0x020001A4 RID: 420
	public class Cask : Object
	{
		// Token: 0x17000317 RID: 791
		// (get) Token: 0x06001DB9 RID: 7609 RVA: 0x001542E2 File Offset: 0x001524E2
		public override string TypeDefinitionId
		{
			get
			{
				return "(BC)";
			}
		}

		// Token: 0x06001DBA RID: 7610 RVA: 0x001542E9 File Offset: 0x001524E9
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.agingRate, "agingRate").AddField(this.daysToMature, "daysToMature");
		}

		// Token: 0x06001DBB RID: 7611 RVA: 0x00154318 File Offset: 0x00152518
		public Cask()
		{
		}

		// Token: 0x06001DBC RID: 7612 RVA: 0x00154336 File Offset: 0x00152536
		public Cask(Vector2 v) : base(v, "163", false)
		{
		}

		// Token: 0x06001DBD RID: 7613 RVA: 0x0015435C File Offset: 0x0015255C
		public override bool performToolAction(Tool t)
		{
			if (t == null || !t.isHeavyHitter() || t is MeleeWeapon)
			{
				return base.performToolAction(t);
			}
			if (this.heldObject.Value != null)
			{
				Game1.createItemDebris(this.heldObject.Value, this.tileLocation.Value * 64f, -1, null, -1, false);
			}
			base.playNearbySoundAll("woodWhack", null, SoundContext.Default);
			if (this.heldObject.Value == null)
			{
				return true;
			}
			this.heldObject.Value = null;
			this.readyForHarvest.Value = false;
			this.minutesUntilReady.Value = -1;
			return false;
		}

		// Token: 0x06001DBE RID: 7614 RVA: 0x00154410 File Offset: 0x00152610
		public virtual bool IsValidCaskLocation()
		{
			GameLocation location = this.Location;
			return location != null && (location is Cellar || location.HasMapPropertyWithValue("CanCaskHere"));
		}

		// Token: 0x06001DBF RID: 7615 RVA: 0x00154440 File Offset: 0x00152640
		public static Item OutputCask(Object machine, Item inputItem, bool probe, MachineItemOutput outputData, Farmer player, out int? overrideMinutesUntilReady)
		{
			overrideMinutesUntilReady = null;
			Cask cask = machine as Cask;
			if (cask == null)
			{
				return null;
			}
			if (!cask.IsValidCaskLocation())
			{
				if (Object.autoLoadFrom == null && !probe)
				{
					Game1.showRedMessageUsingLoadString("Strings\\Objects:CaskNoCellar", true);
				}
				return null;
			}
			if (cask.quality.Value >= 4)
			{
				return null;
			}
			if (inputItem.Quality >= 4)
			{
				return null;
			}
			float multiplier = 1f;
			string rawMultiplier;
			if (((outputData != null) ? outputData.CustomData : null) != null && outputData.CustomData.TryGetValue("AgingMultiplier", out rawMultiplier) && (!float.TryParse(rawMultiplier, out multiplier) || multiplier <= 0f))
			{
				Game1.log.Error("Failed to parse cask aging multiplier '" + rawMultiplier + "' for trigger rule. This must be a positive float value.", null);
				return null;
			}
			if (multiplier <= 0f)
			{
				return null;
			}
			Object output = (Object)inputItem.getOne();
			if (!probe)
			{
				cask.agingRate.Value = multiplier;
				cask.daysToMature.Value = cask.GetDaysForQuality(output.Quality);
				overrideMinutesUntilReady = new int?((output.Quality >= 4) ? 1 : 999999);
				return output;
			}
			return output;
		}

		// Token: 0x06001DC0 RID: 7616 RVA: 0x00154550 File Offset: 0x00152750
		public override bool TryApplyFairyDust(bool probe = false)
		{
			if (this.heldObject.Value == null)
			{
				return false;
			}
			if (this.heldObject.Value.Quality == 4)
			{
				return false;
			}
			if (!probe)
			{
				Utility.addSprinklesToLocation(this.Location, (int)this.tileLocation.X, (int)this.tileLocation.Y, 1, 2, 400, 40, Color.White, null, false);
				Game1.playSound("yoba", null);
				this.daysToMature.Value = this.GetDaysForQuality(this.GetNextQuality(this.heldObject.Value.Quality));
				this.checkForMaturity();
			}
			return true;
		}

		// Token: 0x06001DC1 RID: 7617 RVA: 0x001545FC File Offset: 0x001527FC
		public override void DayUpdate()
		{
			base.DayUpdate();
			if (this.heldObject.Value != null)
			{
				this.minutesUntilReady.Value = 999999;
				this.daysToMature.Value -= this.agingRate.Value;
				this.checkForMaturity();
			}
		}

		// Token: 0x06001DC2 RID: 7618 RVA: 0x0015464F File Offset: 0x0015284F
		public float GetDaysForQuality(int quality)
		{
			switch (quality)
			{
			case 1:
				return 42f;
			case 2:
				return 28f;
			case 4:
				return 0f;
			}
			return 56f;
		}

		// Token: 0x06001DC3 RID: 7619 RVA: 0x00154682 File Offset: 0x00152882
		public int GetNextQuality(int quality)
		{
			switch (quality)
			{
			case 1:
				return 2;
			case 2:
			case 4:
				return 4;
			}
			return 1;
		}

		// Token: 0x06001DC4 RID: 7620 RVA: 0x001546A4 File Offset: 0x001528A4
		public void checkForMaturity()
		{
			if (this.daysToMature.Value <= this.GetDaysForQuality(this.GetNextQuality(this.heldObject.Value.quality.Value)))
			{
				this.heldObject.Value.quality.Value = this.GetNextQuality(this.heldObject.Value.quality.Value);
				if (this.heldObject.Value.Quality == 4)
				{
					this.minutesUntilReady.Value = 1;
				}
			}
		}

		// Token: 0x06001DC5 RID: 7621 RVA: 0x00154730 File Offset: 0x00152930
		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			base.draw(spriteBatch, x, y, alpha);
			Object value = this.heldObject.Value;
			if (value != null && value.quality.Value > 0)
			{
				Vector2 scaleFactor = (base.MinutesUntilReady > 0) ? new Vector2(Math.Abs(this.scale.X - 5f), Math.Abs(this.scale.Y - 5f)) : Vector2.Zero;
				scaleFactor *= 4f;
				Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - 64)));
				Rectangle destination = new Rectangle((int)(position.X + 32f - 8f - scaleFactor.X / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y + 64f + 8f - scaleFactor.Y / 2f) + ((this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(16f + scaleFactor.X), (int)(16f + scaleFactor.Y / 2f));
				spriteBatch.Draw(Game1.mouseCursors, destination, new Rectangle?((this.heldObject.Value.quality.Value < 4) ? new Rectangle(338 + (this.heldObject.Value.quality.Value - 1) * 8, 400, 8, 8) : new Rectangle(346, 392, 8, 8)), Color.White * 0.95f, 0f, Vector2.Zero, SpriteEffects.None, (float)((y + 1) * 64) / 10000f);
			}
		}

		// Token: 0x04001259 RID: 4697
		public const int defaultDaysToMature = 56;

		// Token: 0x0400125A RID: 4698
		[XmlElement("agingRate")]
		public readonly NetFloat agingRate = new NetFloat();

		// Token: 0x0400125B RID: 4699
		[XmlElement("daysToMature")]
		public readonly NetFloat daysToMature = new NetFloat();
	}
}

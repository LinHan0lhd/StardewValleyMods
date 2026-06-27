using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Audio;

namespace StardewValley.Tools
{
	// Token: 0x02000132 RID: 306
	public class Shears : Tool
	{
		// Token: 0x0600189D RID: 6301 RVA: 0x001226C4 File Offset: 0x001208C4
		public Shears() : base("Shears", -1, 7, 7, false, 0)
		{
		}

		// Token: 0x0600189E RID: 6302 RVA: 0x001226E2 File Offset: 0x001208E2
		protected override Item GetOneNew()
		{
			return new Shears();
		}

		// Token: 0x0600189F RID: 6303 RVA: 0x001226E9 File Offset: 0x001208E9
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.finishEvent, "finishEvent");
			this.finishEvent.onEvent += this.doFinish;
		}

		// Token: 0x060018A0 RID: 6304 RVA: 0x00122720 File Offset: 0x00120920
		public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
		{
			x = (int)who.GetToolLocation(false).X;
			y = (int)who.GetToolLocation(false).Y;
			Rectangle r = new Rectangle(x - 32, y - 32, 64, 64);
			this.animal = Utility.GetBestHarvestableFarmAnimal(location.animals.Values, this, r);
			who.Halt();
			int g = who.FarmerSprite.CurrentFrame;
			who.FarmerSprite.animateOnce(283 + who.FacingDirection, 50f, 4);
			who.FarmerSprite.oldFrame = g;
			who.UsingTool = true;
			who.CanMove = false;
			return true;
		}

		// Token: 0x060018A1 RID: 6305 RVA: 0x001227D0 File Offset: 0x001209D0
		public static void playSnip(Farmer who)
		{
			who.playNearbySoundAll("scissors", null, SoundContext.Default);
		}

		// Token: 0x060018A2 RID: 6306 RVA: 0x001227F2 File Offset: 0x001209F2
		public override void tickUpdate(GameTime time, Farmer who)
		{
			this.lastUser = who;
			base.tickUpdate(time, who);
			this.finishEvent.Poll();
		}

		// Token: 0x060018A3 RID: 6307 RVA: 0x00122810 File Offset: 0x00120A10
		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			base.DoFunction(location, x, y, power, who);
			who.Stamina -= 4f;
			if (this.PlayUseSounds)
			{
				Shears.playSnip(who);
			}
			base.CurrentParentTileIndex = 7;
			base.IndexOfMenuItemView = 7;
			FarmAnimal farmAnimal = this.animal;
			if (((farmAnimal != null) ? farmAnimal.currentProduce.Value : null) != null && this.animal.isAdult() && this.animal.CanGetProduceWithTool(this))
			{
				Object produce = ItemRegistry.Create<Object>("(O)" + this.animal.currentProduce.Value, 1, 0, false);
				produce.CanBeSetDown = false;
				produce.Quality = this.animal.produceQuality.Value;
				if (this.animal.hasEatenAnimalCracker.Value)
				{
					produce.Stack = 2;
				}
				if (who.addItemToInventoryBool(produce, false))
				{
					this.animal.HandleStatsOnProduceCollected(produce, (uint)produce.Stack);
					this.animal.currentProduce.Value = null;
					if (this.PlayUseSounds)
					{
						Game1.playSound("coin", null);
					}
					this.animal.friendshipTowardFarmer.Value = Math.Min(1000, this.animal.friendshipTowardFarmer.Value + 5);
					this.animal.ReloadTextureIfNeeded(false);
					who.gainExperience(0, 5);
				}
			}
			else
			{
				string toSay = null;
				if (this.animal != null)
				{
					if (!this.animal.CanGetProduceWithTool(this))
					{
						toSay = Game1.content.LoadString("Strings\\StringsFromCSFiles:Shears.cs.14245", this.animal.displayName);
					}
					else
					{
						toSay = (this.animal.isBaby() ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Shears.cs.14246", this.animal.displayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Shears.cs.14247", this.animal.displayName));
					}
				}
				if (toSay != null)
				{
					Game1.drawObjectDialogue(toSay);
				}
			}
			this.finish();
		}

		// Token: 0x060018A4 RID: 6308 RVA: 0x00122A0B File Offset: 0x00120C0B
		private void finish()
		{
			this.finishEvent.Fire();
		}

		// Token: 0x060018A5 RID: 6309 RVA: 0x00122A18 File Offset: 0x00120C18
		private void doFinish()
		{
			this.animal = null;
			this.lastUser.CanMove = true;
			this.lastUser.completelyStopAnimatingOrDoingAction();
			this.lastUser.UsingTool = false;
			this.lastUser.canReleaseTool = true;
		}

		// Token: 0x04000EEC RID: 3820
		[XmlIgnore]
		private readonly NetEvent0 finishEvent = new NetEvent0(false);

		// Token: 0x04000EED RID: 3821
		[XmlIgnore]
		public FarmAnimal animal;
	}
}

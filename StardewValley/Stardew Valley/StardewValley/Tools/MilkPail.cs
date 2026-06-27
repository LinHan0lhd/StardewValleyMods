using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Audio;
using StardewValley.GameData.FarmAnimals;

namespace StardewValley.Tools
{
	// Token: 0x0200012E RID: 302
	public class MilkPail : Tool
	{
		// Token: 0x0600187D RID: 6269 RVA: 0x00120CBD File Offset: 0x0011EEBD
		public MilkPail() : base("Milk Pail", -1, 6, 6, false, 0)
		{
		}

		// Token: 0x0600187E RID: 6270 RVA: 0x00120CDB File Offset: 0x0011EEDB
		protected override Item GetOneNew()
		{
			return new MilkPail();
		}

		// Token: 0x0600187F RID: 6271 RVA: 0x00120CE2 File Offset: 0x0011EEE2
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.finishEvent, "finishEvent");
			this.finishEvent.onEvent += this.doFinish;
		}

		// Token: 0x06001880 RID: 6272 RVA: 0x00120D18 File Offset: 0x0011EF18
		public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
		{
			x = (int)who.GetToolLocation(false).X;
			y = (int)who.GetToolLocation(false).Y;
			Rectangle r = new Rectangle(x - 32, y - 32, 64, 64);
			this.animal = Utility.GetBestHarvestableFarmAnimal(location.animals.Values, this, r);
			FarmAnimal farmAnimal = this.animal;
			if (((farmAnimal != null) ? farmAnimal.currentProduce.Value : null) != null && this.animal.isAdult() && this.animal.CanGetProduceWithTool(this) && who.couldInventoryAcceptThisItem(this.animal.currentProduce.Value, 1, 0))
			{
				this.animal.pauseTimer = 1500;
				this.animal.doEmote(20, true);
				if (this.PlayUseSounds)
				{
					who.playNearbySoundLocal("Milking", null, SoundContext.Default);
				}
			}
			else
			{
				FarmAnimal farmAnimal2 = this.animal;
				if (((farmAnimal2 != null) ? farmAnimal2.currentProduce.Value : null) != null && this.animal.isAdult())
				{
					if (who == Game1.player)
					{
						if (!this.animal.CanGetProduceWithTool(this))
						{
							FarmAnimalData animalData = this.animal.GetAnimalData();
							string harvestTool = (animalData != null) ? animalData.HarvestTool : null;
							if (harvestTool != null)
							{
								Game1.showRedMessage(Game1.content.LoadString("Strings\\Tools:MilkPail_Name", harvestTool), true);
							}
						}
						else if (!who.couldInventoryAcceptThisItem(this.animal.currentProduce.Value, this.animal.hasEatenAnimalCracker.Value ? 2 : 1, 0))
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"), true);
						}
					}
				}
				else if (who == Game1.player)
				{
					if (this.PlayUseSounds)
					{
						DelayedAction.playSoundAfterDelay("fishingRodBend", 300, null, null, -1, false);
						DelayedAction.playSoundAfterDelay("fishingRodBend", 1200, null, null, -1, false);
					}
					string toSay = null;
					if (this.animal != null)
					{
						if (!this.animal.CanGetProduceWithTool(this))
						{
							toSay = Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14175", this.animal.displayName);
						}
						else
						{
							toSay = (this.animal.isBaby() ? Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14176", this.animal.displayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:MilkPail.cs.14177", this.animal.displayName));
						}
					}
					if (toSay != null)
					{
						DelayedAction.showDialogueAfterDelay(toSay, 1000);
					}
				}
			}
			who.Halt();
			int g = who.FarmerSprite.CurrentFrame;
			who.FarmerSprite.animateOnce(287 + who.FacingDirection, 50f, 4);
			who.FarmerSprite.oldFrame = g;
			who.UsingTool = true;
			who.CanMove = false;
			return true;
		}

		// Token: 0x06001881 RID: 6273 RVA: 0x00120FFD File Offset: 0x0011F1FD
		public override void tickUpdate(GameTime time, Farmer who)
		{
			this.lastUser = who;
			base.tickUpdate(time, who);
			this.finishEvent.Poll();
		}

		// Token: 0x06001882 RID: 6274 RVA: 0x0012101C File Offset: 0x0011F21C
		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			base.DoFunction(location, x, y, power, who);
			who.Stamina -= 4f;
			base.CurrentParentTileIndex = 6;
			base.IndexOfMenuItemView = 6;
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
					if (this.PlayUseSounds)
					{
						Game1.playSound("coin", null);
					}
					this.animal.currentProduce.Value = null;
					this.animal.friendshipTowardFarmer.Value = Math.Min(1000, this.animal.friendshipTowardFarmer.Value + 5);
					this.animal.ReloadTextureIfNeeded(false);
					who.gainExperience(0, 5);
				}
			}
			this.finish();
		}

		// Token: 0x06001883 RID: 6275 RVA: 0x00121181 File Offset: 0x0011F381
		private void finish()
		{
			this.finishEvent.Fire();
		}

		// Token: 0x06001884 RID: 6276 RVA: 0x0012118E File Offset: 0x0011F38E
		private void doFinish()
		{
			this.animal = null;
			this.lastUser.CanMove = true;
			this.lastUser.completelyStopAnimatingOrDoingAction();
			this.lastUser.UsingTool = false;
			this.lastUser.canReleaseTool = true;
		}

		// Token: 0x04000EE3 RID: 3811
		[XmlIgnore]
		private readonly NetEvent0 finishEvent = new NetEvent0(false);

		// Token: 0x04000EE4 RID: 3812
		[XmlIgnore]
		public FarmAnimal animal;
	}
}

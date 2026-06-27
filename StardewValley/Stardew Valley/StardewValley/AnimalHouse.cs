using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Audio;
using StardewValley.Buildings;
using StardewValley.Events;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.FarmAnimals;
using StardewValley.GameData.Machines;
using StardewValley.TokenizableStrings;
using xTile.Dimensions;

namespace StardewValley
{
	// Token: 0x020000CE RID: 206
	public class AnimalHouse : GameLocation
	{
		// Token: 0x06000E02 RID: 3586 RVA: 0x00095215 File Offset: 0x00093415
		public AnimalHouse()
		{
		}

		// Token: 0x06000E03 RID: 3587 RVA: 0x00095234 File Offset: 0x00093434
		public AnimalHouse(string mapPath, string name) : base(mapPath, name)
		{
		}

		// Token: 0x06000E04 RID: 3588 RVA: 0x00095255 File Offset: 0x00093455
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.animalLimit, "animalLimit").AddField(this.animalsThatLiveHere, "animalsThatLiveHere");
		}

		// Token: 0x06000E05 RID: 3589 RVA: 0x00095284 File Offset: 0x00093484
		public override void OnParentBuildingUpgraded(Building building)
		{
			base.OnParentBuildingUpgraded(building);
			BuildingData buildingData = building.GetData();
			if (buildingData != null)
			{
				this.animalLimit.Value = buildingData.MaxOccupants;
			}
			this.resetPositionsOfAllAnimals();
			base.loadLights();
		}

		// Token: 0x06000E06 RID: 3590 RVA: 0x000952BF File Offset: 0x000934BF
		public bool isFull()
		{
			return this.animalsThatLiveHere.Count >= this.animalLimit.Value;
		}

		// Token: 0x06000E07 RID: 3591 RVA: 0x000952DC File Offset: 0x000934DC
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			Object activeObject = who.ActiveObject;
			if (((activeObject != null) ? activeObject.QualifiedItemId : null) == "(O)178" && this.doesTileHaveProperty(tileLocation.X, tileLocation.Y, "Trough", "Back", false) != null && !this.objects.ContainsKey(new Vector2((float)tileLocation.X, (float)tileLocation.Y)))
			{
				this.objects.Add(new Vector2((float)tileLocation.X, (float)tileLocation.Y), (Object)who.ActiveObject.getOne());
				who.reduceActiveItemByOne();
				who.currentLocation.playSound("coin", null, null, SoundContext.Default);
				Game1.haltAfterCheck = false;
				return true;
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		// Token: 0x06000E08 RID: 3592 RVA: 0x000953B4 File Offset: 0x000935B4
		protected override void resetSharedState()
		{
			this.resetPositionsOfAllAnimals();
			foreach (Object o in this.objects.Values)
			{
				if (o.bigCraftable.Value)
				{
					MachineData machineData = o.GetMachineData();
					if (machineData != null && machineData.IsIncubator && o.heldObject.Value != null && o.MinutesUntilReady <= 0)
					{
						if (!this.isFull())
						{
							string whatHatched = "??";
							FarmAnimalData hatchedAnimal = FarmAnimal.GetAnimalDataFromEgg(o.heldObject.Value, this);
							if (hatchedAnimal != null && hatchedAnimal.BirthText != null)
							{
								whatHatched = TokenParser.ParseText(hatchedAnimal.BirthText, null, null, null);
							}
							this.currentEvent = new Event("none/-1000 -1000/farmer 2 9 0/pause 250/message \"" + whatHatched + "\"/pause 500/animalNaming/pause 500/end", null);
							break;
						}
						if (!this.hasShownIncubatorBuildingFullMessage)
						{
							this.hasShownIncubatorBuildingFullMessage = true;
							Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Locations:AnimalHouse_Incubator_HouseFull"));
						}
					}
				}
			}
			base.resetSharedState();
		}

		// Token: 0x06000E09 RID: 3593 RVA: 0x000954D4 File Offset: 0x000936D4
		public void addNewHatchedAnimal(string name)
		{
			bool foundIncubator = false;
			foreach (Object o in this.objects.Values)
			{
				if (o.bigCraftable.Value)
				{
					MachineData machineData = o.GetMachineData();
					if (machineData != null && machineData.IsIncubator && o.heldObject.Value != null && o.MinutesUntilReady <= 0 && !this.isFull())
					{
						foundIncubator = true;
						string hatchedAnimalId;
						FarmAnimalData farmAnimalData;
						FarmAnimal a = new FarmAnimal(FarmAnimal.TryGetAnimalDataFromEgg(o.heldObject.Value, this, out hatchedAnimalId, out farmAnimalData) ? hatchedAnimalId : "White Chicken", Game1.multiplayer.getNewID(), Game1.player.UniqueMultiplayerID);
						a.Name = name;
						a.displayName = name;
						o.heldObject.Value = null;
						this.adoptAnimal(a);
						break;
					}
				}
			}
			if (!foundIncubator)
			{
				QuestionEvent questionEvent = Game1.farmEvent as QuestionEvent;
				if (questionEvent != null)
				{
					this.adoptAnimal(new FarmAnimal(questionEvent.animal.type.Value, Game1.multiplayer.getNewID(), Game1.player.UniqueMultiplayerID)
					{
						Name = name,
						displayName = name,
						parentId = 
						{
							Value = questionEvent.animal.myID.Value
						}
					});
					questionEvent.forceProceed = true;
				}
			}
			Game1.exitActiveMenu();
		}

		// Token: 0x06000E0A RID: 3594 RVA: 0x00095654 File Offset: 0x00093854
		public void adoptAnimal(FarmAnimal animal)
		{
			this.animals.Add(animal.myID.Value, animal);
			animal.currentLocation = this;
			this.animalsThatLiveHere.Add(animal.myID.Value);
			animal.homeInterior = this;
			animal.setRandomPosition(this);
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				string displayType = animal.displayType;
				if (displayType == "White Chicken" || displayType == "Brown Chicken")
				{
					displayType = "Chicken";
				}
				farmer.autoGenerateActiveDialogueEvent("purchasedAnimal_" + displayType, 4);
			}
		}

		// Token: 0x06000E0B RID: 3595 RVA: 0x00095714 File Offset: 0x00093914
		public void resetPositionsOfAllAnimals()
		{
			foreach (KeyValuePair<long, FarmAnimal> kvp in this.animals.Pairs)
			{
				kvp.Value.setRandomPosition(this);
			}
		}

		// Token: 0x06000E0C RID: 3596 RVA: 0x00095778 File Offset: 0x00093978
		public override bool dropObject(Object obj, Vector2 location, xTile.Dimensions.Rectangle viewport, bool initialPlacement, Farmer who = null)
		{
			Vector2 tileLocation = new Vector2((float)((int)(location.X / 64f)), (float)((int)(location.Y / 64f)));
			if (obj.QualifiedItemId == "(O)178" && this.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Trough", "Back", false) != null)
			{
				return this.objects.TryAdd(tileLocation, obj);
			}
			return base.dropObject(obj, location, viewport, initialPlacement, null);
		}

		// Token: 0x06000E0D RID: 3597 RVA: 0x000957F5 File Offset: 0x000939F5
		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			this.animalLimit.Value = ((AnimalHouse)l).animalLimit.Value;
			base.TransferDataFromSavedLocation(l);
		}

		// Token: 0x06000E0E RID: 3598 RVA: 0x00095819 File Offset: 0x00093A19
		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			if (base.HasMapPropertyWithValue("AutoFeed"))
			{
				this.feedAllAnimals();
			}
		}

		// Token: 0x06000E0F RID: 3599 RVA: 0x00095838 File Offset: 0x00093A38
		public void feedAllAnimals()
		{
			GameLocation rootLocation = base.GetRootLocation();
			int fed = 0;
			for (int x = 0; x < this.map.Layers[0].LayerWidth; x++)
			{
				for (int y = 0; y < this.map.Layers[0].LayerHeight; y++)
				{
					if (this.doesTileHaveProperty(x, y, "Trough", "Back", false) != null)
					{
						Vector2 tileLocation = new Vector2((float)x, (float)y);
						if (!this.objects.ContainsKey(tileLocation))
						{
							Object hay = GameLocation.GetHayFromAnySilo(rootLocation);
							if (hay == null)
							{
								return;
							}
							this.objects.Add(tileLocation, hay);
							fed++;
						}
						if (fed >= this.animalLimit.Value)
						{
							return;
						}
					}
				}
			}
		}

		// Token: 0x04000945 RID: 2373
		[XmlElement("animalLimit")]
		public readonly NetInt animalLimit = new NetInt(4);

		// Token: 0x04000946 RID: 2374
		public readonly NetLongList animalsThatLiveHere = new NetLongList();

		// Token: 0x04000947 RID: 2375
		[XmlIgnore]
		public bool hasShownIncubatorBuildingFullMessage;
	}
}

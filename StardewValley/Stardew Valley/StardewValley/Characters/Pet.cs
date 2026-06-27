using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.GameData.Pets;
using StardewValley.Internal;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Objects;

namespace StardewValley.Characters
{
	// Token: 0x0200037B RID: 891
	public class Pet : NPC
	{
		// Token: 0x17000474 RID: 1140
		// (get) Token: 0x06003699 RID: 13977 RVA: 0x002B23E3 File Offset: 0x002B05E3
		[XmlIgnore]
		public override bool IsVillager
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600369A RID: 13978 RVA: 0x002B23E6 File Offset: 0x002B05E6
		public override void reloadData()
		{
		}

		// Token: 0x0600369B RID: 13979 RVA: 0x002B23E8 File Offset: 0x002B05E8
		protected override string translateName()
		{
			return this.name.Value.Trim();
		}

		// Token: 0x0600369C RID: 13980 RVA: 0x002B23FC File Offset: 0x002B05FC
		public Pet(int xTile, int yTile, string petBreed, string petType)
		{
			base.Name = petType;
			this.displayName = this.name.Value;
			this.petType.Value = petType;
			this.whichBreed.Value = petBreed;
			this.Sprite = new AnimatedSprite(this.getPetTextureName(), 0, 32, 32);
			base.Position = new Vector2((float)xTile, (float)yTile) * 64f;
			base.Breather = false;
			base.willDestroyObjectsUnderfoot = false;
			base.currentLocation = Game1.currentLocation;
			base.HideShadow = true;
		}

		// Token: 0x0600369D RID: 13981 RVA: 0x002B2539 File Offset: 0x002B0739
		public Pet() : this(0, 0, "0", "Dog")
		{
		}

		// Token: 0x17000475 RID: 1141
		// (get) Token: 0x0600369E RID: 13982 RVA: 0x002B254D File Offset: 0x002B074D
		// (set) Token: 0x0600369F RID: 13983 RVA: 0x002B255A File Offset: 0x002B075A
		public string CurrentBehavior
		{
			get
			{
				return this.netCurrentBehavior.Value;
			}
			set
			{
				if (this.netCurrentBehavior.Value != value)
				{
					this.netCurrentBehavior.Value = value;
				}
			}
		}

		// Token: 0x060036A0 RID: 13984 RVA: 0x002B257C File Offset: 0x002B077C
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.petId, "petId").AddField(this.petType, "petType").AddField(this.whichBreed, "whichBreed").AddField(this.netCurrentBehavior, "netCurrentBehavior").AddField(this.homeLocationName, "homeLocationName").AddField(this.petPushEvent, "petPushEvent").AddField(this.lastPetDay, "lastPetDay").AddField(this.grantedFriendshipForPet, "grantedFriendshipForPet").AddField(this.friendshipTowardFarmer, "friendshipTowardFarmer").AddField(this.isSleepingOnFarmerBed, "isSleepingOnFarmerBed").AddField(this.mutex.NetFields, "mutex.NetFields").AddField(this.hat, "hat").AddField(this.timesPet, "timesPet");
			this.name.FilterStringEvent += Utility.FilterDirtyWords;
			this.name.fieldChangeVisibleEvent += delegate(NetString <p0>, string <p1>, string <p2>)
			{
				base.resetCachedDisplayName();
			};
			this.petPushEvent.onEvent += this.OnPetPush;
			this.friendshipTowardFarmer.fieldChangeVisibleEvent += delegate(NetInt field, int old_value, int new_value)
			{
				this.GrantLoveMailIfNecessary();
			};
			this.isSleepingOnFarmerBed.fieldChangeVisibleEvent += delegate(NetBool a, bool b, bool c)
			{
				this.UpdateSleepingOnBed();
			};
			this.petType.fieldChangeVisibleEvent += delegate(NetString a, string b, string c)
			{
				this.reloadBreedSprite();
			};
			this.whichBreed.fieldChangeVisibleEvent += delegate(NetString a, string b, string c)
			{
				this.reloadBreedSprite();
			};
			this.netCurrentBehavior.fieldChangeVisibleEvent += delegate(NetString a, string b, string c)
			{
				if (this._currentBehavior != this.CurrentBehavior)
				{
					this._OnNewBehavior();
				}
			};
		}

		// Token: 0x060036A1 RID: 13985 RVA: 0x002B2724 File Offset: 0x002B0924
		public virtual void OnPetPush(long farmerId)
		{
			this.pushingTimer = 0;
			if (Game1.IsMasterGame)
			{
				Farmer farmer = Game1.GetPlayer(farmerId, false) ?? Game1.player;
				Vector2 trajectory = Utility.getAwayFromPlayerTrajectory(this.GetBoundingBox(), farmer);
				base.setTrajectory((int)trajectory.X / 2, (int)trajectory.Y / 2);
				this._walkFromPushTimer = 250;
				this.CurrentBehavior = "Walk";
				this.OnNewBehavior();
				this.Halt();
				this.faceDirection(farmer.FacingDirection);
				base.setMovingInFacingDirection();
			}
		}

		// Token: 0x060036A2 RID: 13986 RVA: 0x002B27A9 File Offset: 0x002B09A9
		public override int getTimeFarmerMustPushBeforeStartShaking()
		{
			return 300;
		}

		// Token: 0x060036A3 RID: 13987 RVA: 0x002B27B0 File Offset: 0x002B09B0
		public override int getTimeFarmerMustPushBeforePassingThrough()
		{
			return 750;
		}

		// Token: 0x060036A4 RID: 13988 RVA: 0x002B27B8 File Offset: 0x002B09B8
		public override void behaviorOnFarmerLocationEntry(GameLocation location, Farmer who)
		{
			base.behaviorOnFarmerLocationEntry(location, who);
			if (location is Farm && Game1.timeOfDay >= 2000 && !location.farmers.Any())
			{
				if (this.CurrentBehavior != "Sleep" || base.currentLocation is Farm)
				{
					Game1.player.team.requestPetWarpHomeEvent.Fire(Game1.player.UniqueMultiplayerID);
				}
			}
			else if (Game1.timeOfDay < 2000 && Game1.random.NextBool() && this._currentBehavior != "Sleep")
			{
				this.CurrentBehavior = "Sleep";
				this._OnNewBehavior();
				this.Sprite.UpdateSourceRect();
			}
			this.UpdateSleepingOnBed();
		}

		// Token: 0x060036A5 RID: 13989 RVA: 0x002B287C File Offset: 0x002B0A7C
		public override void behaviorOnLocalFarmerLocationEntry(GameLocation location)
		{
			base.behaviorOnLocalFarmerLocationEntry(location);
			this.netCurrentBehavior.CancelInterpolation();
			if (this.netCurrentBehavior.Value == "Sleep")
			{
				this.position.NetFields.CancelInterpolation();
				if (this._currentBehavior != "Sleep")
				{
					this._OnNewBehavior();
					this.Sprite.UpdateSourceRect();
				}
			}
			this.UpdateSleepingOnBed();
		}

		// Token: 0x060036A6 RID: 13990 RVA: 0x002B28EB File Offset: 0x002B0AEB
		public override bool canTalk()
		{
			return false;
		}

		// Token: 0x060036A7 RID: 13991 RVA: 0x002B28F0 File Offset: 0x002B0AF0
		public PetData GetPetData()
		{
			PetData petData;
			if (!Pet.TryGetData(this.petType.Value, out petData))
			{
				return null;
			}
			return petData;
		}

		// Token: 0x060036A8 RID: 13992 RVA: 0x002B2914 File Offset: 0x002B0B14
		public static bool TryGetData(string petType, out PetData data)
		{
			if (petType != null && Game1.petData.TryGetValue(petType, out data))
			{
				return true;
			}
			data = null;
			return false;
		}

		// Token: 0x060036A9 RID: 13993 RVA: 0x002B2930 File Offset: 0x002B0B30
		public void GetPetIcon(out string assetName, out Rectangle sourceRect)
		{
			PetData petData = this.GetPetData();
			PetBreed petBreed;
			if ((petBreed = ((petData != null) ? petData.GetBreedById(this.whichBreed.Value, false) : null)) == null)
			{
				PetBreed petBreed2;
				if (petData == null)
				{
					petBreed2 = null;
				}
				else
				{
					List<PetBreed> breeds = petData.Breeds;
					petBreed2 = ((breeds != null) ? breeds.FirstOrDefault<PetBreed>() : null);
				}
				if ((petBreed = petBreed2) == null)
				{
					PetData dogData;
					if (!Pet.TryGetData("Dog", out dogData))
					{
						petBreed = null;
					}
					else
					{
						List<PetBreed> breeds2 = dogData.Breeds;
						petBreed = ((breeds2 != null) ? breeds2.FirstOrDefault<PetBreed>() : null);
					}
				}
			}
			PetBreed breed = petBreed;
			if (breed != null)
			{
				assetName = breed.IconTexture;
				sourceRect = breed.IconSourceRect;
				return;
			}
			assetName = "Animals\\dog";
			sourceRect = new Rectangle(208, 208, 16, 16);
		}

		// Token: 0x060036AA RID: 13994 RVA: 0x002B29D8 File Offset: 0x002B0BD8
		public virtual string getPetTextureName()
		{
			try
			{
				PetData petType = this.GetPetData();
				if (petType != null)
				{
					return petType.GetBreedById(this.whichBreed.Value, false).Texture;
				}
			}
			catch (Exception)
			{
			}
			return "Animals\\dog";
		}

		// Token: 0x060036AB RID: 13995 RVA: 0x002B2A28 File Offset: 0x002B0C28
		public void reloadBreedSprite()
		{
			AnimatedSprite sprite = this.Sprite;
			if (sprite == null)
			{
				return;
			}
			sprite.LoadTexture(this.getPetTextureName(), true);
		}

		// Token: 0x060036AC RID: 13996 RVA: 0x002B2A44 File Offset: 0x002B0C44
		public override void reloadSprite(bool onlyAppearance = false)
		{
			this.reloadBreedSprite();
			base.HideShadow = true;
			base.Breather = false;
			if (!onlyAppearance)
			{
				base.DefaultPosition = new Vector2(54f, 8f) * 64f;
				this.setAtFarmPosition();
				if (this.GetPetBowl() == null)
				{
					this.warpToFarmHouse(Game1.MasterPlayer);
				}
				this.GrantLoveMailIfNecessary();
			}
		}

		// Token: 0x060036AD RID: 13997 RVA: 0x002B2AA6 File Offset: 0x002B0CA6
		public override void ChooseAppearance(LocalizedContentManager content = null)
		{
			AnimatedSprite sprite = this.Sprite;
			if (((sprite != null) ? sprite.Texture : null) == null)
			{
				this.reloadSprite(true);
			}
		}

		// Token: 0x060036AE RID: 13998 RVA: 0x002B2AC4 File Offset: 0x002B0CC4
		public void warpToFarmHouse(Farmer who)
		{
			PetData petData = this.GetPetData();
			this.isSleepingOnFarmerBed.Value = false;
			FarmHouse farmHouse = Utility.getHomeOfFarmer(who);
			int tries = 0;
			Vector2 sleepTile = new Vector2((float)Game1.random.Next(2, farmHouse.map.Layers[0].LayerWidth - 3), (float)Game1.random.Next(3, farmHouse.map.Layers[0].LayerHeight - 5));
			List<Furniture> rugs = new List<Furniture>();
			foreach (Furniture house_furniture in farmHouse.furniture)
			{
				if (house_furniture.furniture_type.Value == 12)
				{
					rugs.Add(house_furniture);
				}
			}
			BedFurniture player_bed = farmHouse.GetPlayerBed();
			float sleepOnBedChance = 0f;
			float sleepAtBedFootChance = 0.3f;
			float sleepOnRugChance = 0.5f;
			if (petData != null)
			{
				sleepOnBedChance = petData.SleepOnBedChance;
				sleepAtBedFootChance = petData.SleepNearBedChance;
				sleepOnRugChance = petData.SleepOnRugChance;
			}
			if (player_bed != null && !Game1.newDay && Game1.timeOfDay >= 2000 && Game1.random.NextDouble() <= (double)sleepOnBedChance)
			{
				sleepTile = Utility.PointToVector2(player_bed.GetBedSpot()) + new Vector2(-1f, 0f);
				if (farmHouse.isCharacterAtTile(sleepTile) == null)
				{
					Game1.warpCharacter(this, farmHouse, sleepTile);
					base.NetFields.CancelInterpolation();
					this.CurrentBehavior = "Sleep";
					this.isSleepingOnFarmerBed.Value = true;
					Rectangle petBounds = this.GetBoundingBox();
					foreach (Furniture furniture in farmHouse.furniture)
					{
						BedFurniture bed = furniture as BedFurniture;
						if (bed != null && bed.GetBoundingBox().Intersects(petBounds))
						{
							bed.ReserveForNPC();
							break;
						}
					}
					this.UpdateSleepingOnBed();
					this._OnNewBehavior();
					this.Sprite.UpdateSourceRect();
					return;
				}
			}
			else if (Game1.random.NextDouble() <= (double)sleepAtBedFootChance)
			{
				sleepTile = Utility.PointToVector2(farmHouse.getBedSpot(BedFurniture.BedType.Any)) + new Vector2(0f, 2f);
			}
			else if (Game1.random.NextDouble() <= (double)sleepOnRugChance)
			{
				Furniture rug = Game1.random.ChooseFrom(rugs);
				if (rug != null)
				{
					sleepTile = Utility.getRandomPositionInThisRectangle(rug.boundingBox.Value, Game1.random) / 64f;
				}
			}
			while (tries < 50 && (!farmHouse.canPetWarpHere(sleepTile) || !farmHouse.CanItemBePlacedHere(sleepTile, false, ~CollisionMask.Farmers, ~CollisionMask.Objects, false, false) || !farmHouse.CanItemBePlacedHere(sleepTile + new Vector2(1f, 0f), false, ~CollisionMask.Farmers, ~CollisionMask.Objects, false, false) || farmHouse.isTileOnWall((int)sleepTile.X, (int)sleepTile.Y)))
			{
				sleepTile = new Vector2((float)Game1.random.Next(2, farmHouse.map.Layers[0].LayerWidth - 3), (float)Game1.random.Next(3, farmHouse.map.Layers[0].LayerHeight - 4));
				tries++;
			}
			if (tries < 50)
			{
				Game1.warpCharacter(this, farmHouse, sleepTile);
				this.CurrentBehavior = "Sleep";
			}
			else
			{
				this.WarpToPetBowl();
			}
			this.UpdateSleepingOnBed();
			this._OnNewBehavior();
			this.Sprite.UpdateSourceRect();
		}

		// Token: 0x060036AF RID: 13999 RVA: 0x002B2E54 File Offset: 0x002B1054
		public virtual void UpdateSleepingOnBed()
		{
			this.drawOnTop = false;
			this.collidesWithOtherCharacters.Value = !this.isSleepingOnFarmerBed.Value;
			this.farmerPassesThrough = this.isSleepingOnFarmerBed.Value;
		}

		// Token: 0x060036B0 RID: 14000 RVA: 0x002B2E88 File Offset: 0x002B1088
		public override void dayUpdate(int dayOfMonth)
		{
			this.isSleepingOnFarmerBed.Value = false;
			this.UpdateSleepingOnBed();
			base.DefaultPosition = new Vector2(54f, 8f) * 64f;
			this.Sprite.loop = false;
			base.Breather = false;
			if (Game1.IsMasterGame && this.GetPetBowl() == null)
			{
				foreach (Building building in Game1.getFarm().buildings)
				{
					PetBowl bowl = building as PetBowl;
					if (bowl != null && !bowl.HasPet())
					{
						bowl.AssignPet(this);
						break;
					}
				}
			}
			PetBowl petBowl = this.GetPetBowl();
			if (Game1.isRaining)
			{
				this.CurrentBehavior = "SitDown";
				this.warpToFarmHouse(Game1.player);
			}
			else if (petBowl != null && base.currentLocation is FarmHouse)
			{
				this.setAtFarmPosition();
			}
			else if (petBowl == null)
			{
				this.warpToFarmHouse(Game1.player);
			}
			if (Game1.IsMasterGame)
			{
				if (petBowl != null && petBowl.watered.Value)
				{
					this.friendshipTowardFarmer.Set(Math.Min(1000, this.friendshipTowardFarmer.Value + 6));
					petBowl.watered.Set(false);
				}
				if (petBowl == null)
				{
					this.friendshipTowardFarmer.Value -= 10;
				}
			}
			if (petBowl == null)
			{
				Game1.addMorningFluffFunction(delegate
				{
					base.doEmote(28, true);
				});
			}
			this.Halt();
			this.CurrentBehavior = "Sleep";
			this.grantedFriendshipForPet.Set(false);
			this._OnNewBehavior();
			this.Sprite.UpdateSourceRect();
		}

		// Token: 0x060036B1 RID: 14001 RVA: 0x002B3030 File Offset: 0x002B1230
		public void GrantLoveMailIfNecessary()
		{
			if (this.friendshipTowardFarmer.Value >= 1000)
			{
				foreach (Farmer farmer in Game1.getAllFarmers())
				{
					if (farmer != null && farmer.mailReceived.Add("petLoveMessage") && farmer == Game1.player)
					{
						if (Game1.newDay)
						{
							Game1.addMorningFluffFunction(delegate
							{
								Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Characters:PetLovesYou", this.displayName));
							});
						}
						else
						{
							Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Characters:PetLovesYou", this.displayName));
						}
					}
					if (!farmer.hasOrWillReceiveMail("MarniePetAdoption"))
					{
						Game1.addMailForTomorrow("MarniePetAdoption", false, false);
					}
				}
			}
		}

		// Token: 0x060036B2 RID: 14002 RVA: 0x002B30F4 File Offset: 0x002B12F4
		public PetBowl GetPetBowl()
		{
			foreach (Building building in (Game1.getLocationFromName(this.homeLocationName.Value) ?? Game1.getFarm()).buildings)
			{
				PetBowl bowl = building as PetBowl;
				if (bowl != null && bowl.petId.Value == this.petId.Value)
				{
					return bowl;
				}
			}
			return null;
		}

		// Token: 0x060036B3 RID: 14003 RVA: 0x002B3184 File Offset: 0x002B1384
		public virtual void WarpToPetBowl()
		{
			PetBowl bowl = this.GetPetBowl();
			if (bowl != null)
			{
				this.faceDirection(2);
				Game1.warpCharacter(this, bowl.parentLocationName.Value, bowl.GetPetSpot());
			}
		}

		// Token: 0x060036B4 RID: 14004 RVA: 0x002B31B9 File Offset: 0x002B13B9
		public void setAtFarmPosition()
		{
			if (Game1.IsMasterGame)
			{
				if (!Game1.isRaining)
				{
					this.WarpToPetBowl();
					return;
				}
				this.warpToFarmHouse(Game1.MasterPlayer);
			}
		}

		// Token: 0x060036B5 RID: 14005 RVA: 0x002B31DB File Offset: 0x002B13DB
		public override bool shouldCollideWithBuildingLayer(GameLocation location)
		{
			return true;
		}

		// Token: 0x060036B6 RID: 14006 RVA: 0x002B31DE File Offset: 0x002B13DE
		public override bool canPassThroughActionTiles()
		{
			return false;
		}

		// Token: 0x060036B7 RID: 14007 RVA: 0x002B31E4 File Offset: 0x002B13E4
		public void unassignPetBowl()
		{
			foreach (Building building in (Game1.getLocationFromName(this.homeLocationName.Value) ?? Game1.getFarm()).buildings)
			{
				PetBowl bowl = building as PetBowl;
				if (bowl != null && bowl.petId.Value == this.petId.Value)
				{
					bowl.petId.Value = Guid.Empty;
				}
			}
		}

		// Token: 0x060036B8 RID: 14008 RVA: 0x002B3280 File Offset: 0x002B1480
		public void applyButterflyPowder(Farmer who, string responseKey)
		{
			if (responseKey.Contains("Yes"))
			{
				GameLocation i = base.currentLocation;
				this.unassignPetBowl();
				i.characters.Remove(this);
				this.playContentSound();
				Game1.playSound("fireball", null);
				Rectangle r = this.GetBoundingBox();
				r.Inflate(32, 32);
				r.X -= 32;
				r.Y -= 32;
				i.temporarySprites.AddRange(Utility.sparkleWithinArea(r, 6, Color.White, 50, 0, ""));
				i.temporarySprites.Add(new TemporaryAnimatedSprite(5, Utility.PointToVector2(this.GetBoundingBox().Center) - new Vector2(32f), Color.White, 8, false, 50f, 0, -1, -1f, -1, 0));
				for (int j = 0; j < 8; j++)
				{
					i.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), base.Position + new Vector2(32f) + new Vector2((float)Game1.random.Next(-16, 16), (float)Game1.random.Next(-32, 16)), false, 0.002f, Color.White)
					{
						alphaFade = 0.0043333336f,
						alpha = 0.75f,
						motion = new Vector2((float)Game1.random.Next(-10, 11) / 20f, -1f),
						acceleration = new Vector2(0f, 0f),
						interval = 99999f,
						layerDepth = 1f,
						scale = 3f,
						scaleChange = 0.01f,
						rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f
					});
				}
				i.instantiateCrittersList();
				i.addCritter(new Butterfly(i, base.Tile + new Vector2(0f, 1f), false, false, -1, false));
				who.reduceActiveItemByOne();
				if (this.hat.Value != null)
				{
					Game1.createItemDebris(this.hat.Value, base.Position, -1, i, -1, false);
				}
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:ButterflyPowder_Goodbye", base.Name));
			}
		}

		// Token: 0x060036B9 RID: 14009 RVA: 0x002B3500 File Offset: 0x002B1700
		public override bool checkAction(Farmer who, GameLocation l)
		{
			if (who.Items.Count > who.CurrentToolIndex && who.Items[who.CurrentToolIndex] != null && who.Items[who.CurrentToolIndex] is Hat && (this.petType.Value == "Cat" || this.petType.Value == "Dog"))
			{
				if (this.hat.Value != null)
				{
					Game1.createItemDebris(this.hat.Value, base.Position, this.FacingDirection, null, -1, false);
					this.hat.Value = null;
				}
				else
				{
					Hat hatItem = who.Items[who.CurrentToolIndex] as Hat;
					who.Items[who.CurrentToolIndex] = null;
					this.hat.Value = hatItem;
					Game1.playSound("dirtyHit", null);
				}
				this.mutex.ReleaseLock();
			}
			if (who.CurrentItem != null && who.CurrentItem.QualifiedItemId.Equals("(O)ButterflyPowder"))
			{
				l.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:ButterflyPowder_Question", base.Name), l.createYesNoResponses(), new GameLocation.afterQuestionBehavior(this.applyButterflyPowder), null);
			}
			int curLastPetDay;
			if (!this.lastPetDay.TryGetValue(who.UniqueMultiplayerID, out curLastPetDay) || curLastPetDay != Game1.Date.TotalDays)
			{
				this.lastPetDay[who.UniqueMultiplayerID] = Game1.Date.TotalDays;
				this.mutex.RequestLock(delegate
				{
					if (!this.grantedFriendshipForPet.Value)
					{
						this.grantedFriendshipForPet.Set(true);
						this.friendshipTowardFarmer.Set(Math.Min(1000, this.friendshipTowardFarmer.Value + 12));
						if (Utility.CreateDaySaveRandom((double)this.timesPet.Value, 71928.0, (double)this.petId.Value.GetHashCode()).NextDouble() < (double)this.GetPetData().GiftChance)
						{
							Item i = this.TryGetGiftItem(this.GetPetData().Gifts);
							if (i != null)
							{
								Game1.createMultipleItemDebris(i, this.Position, -1, l, -1, true);
							}
						}
						NetInt netInt = this.timesPet;
						int value = netInt.Value;
						netInt.Value = value + 1;
					}
					this.mutex.ReleaseLock();
				}, null);
				base.doEmote(20, true);
				this.playContentSound();
				return true;
			}
			return false;
		}

		// Token: 0x060036BA RID: 14010 RVA: 0x002B36E8 File Offset: 0x002B18E8
		public virtual void playContentSound()
		{
			if (Utility.isOnScreen(base.TilePoint, 128, base.currentLocation) && !Game1.options.muteAnimalSounds)
			{
				PetData petData = this.GetPetData();
				if (petData != null && petData.ContentSound != null)
				{
					string contentSound = petData.ContentSound;
					this.PlaySound(contentSound, true, -1, -1);
					if (petData.RepeatContentSoundAfter >= 0)
					{
						DelayedAction.functionAfterDelay(delegate
						{
							this.PlaySound(contentSound, true, -1, -1);
						}, petData.RepeatContentSoundAfter);
					}
				}
			}
		}

		// Token: 0x060036BB RID: 14011 RVA: 0x002B3774 File Offset: 0x002B1974
		public void hold(Farmer who)
		{
			FarmerSprite.AnimationFrame lastFrame = this.Sprite.CurrentAnimation.Last<FarmerSprite.AnimationFrame>();
			this.flip = lastFrame.flip;
			this.Sprite.CurrentFrame = lastFrame.frame;
			this.Sprite.CurrentAnimation = null;
			this.Sprite.loop = false;
		}

		// Token: 0x060036BC RID: 14012 RVA: 0x002B37C8 File Offset: 0x002B19C8
		public override void behaviorOnFarmerPushing()
		{
			if (this.CurrentBehavior == "Sprint")
			{
				return;
			}
			this.pushingTimer += 2;
			if (this.pushingTimer > 100)
			{
				this.petPushEvent.Fire(Game1.player.UniqueMultiplayerID);
			}
		}

		// Token: 0x060036BD RID: 14013 RVA: 0x002B3815 File Offset: 0x002B1A15
		public override void update(GameTime time, GameLocation location, long id, bool move)
		{
			base.update(time, location, id, move);
			this.pushingTimer = Math.Max(0, this.pushingTimer - 1);
		}

		// Token: 0x060036BE RID: 14014 RVA: 0x002B3838 File Offset: 0x002B1A38
		public override void update(GameTime time, GameLocation location)
		{
			base.update(time, location);
			this.petPushEvent.Poll();
			if (this.isSleepingOnFarmerBed.Value && this.CurrentBehavior != "Sleep" && Game1.IsMasterGame)
			{
				this.isSleepingOnFarmerBed.Value = false;
				this.UpdateSleepingOnBed();
			}
			if (base.currentLocation == null)
			{
				base.currentLocation = location;
			}
			this.mutex.Update(location);
			if (Game1.eventUp)
			{
				return;
			}
			if (this._currentBehavior != this.CurrentBehavior)
			{
				this._OnNewBehavior();
			}
			this.RunState(time);
			if (Game1.IsMasterGame)
			{
				PetBehavior currentBehavior = this.GetCurrentPetBehavior();
				if (currentBehavior != null && currentBehavior.WalkInDirection)
				{
					if (currentBehavior.Animation == null)
					{
						this.MovePosition(time, Game1.viewport, location);
					}
					else
					{
						base.tryToMoveInDirection(this.FacingDirection, false, -1, false);
					}
				}
			}
			this.flip = false;
			if (this.FacingDirection == 3 && this.Sprite.CurrentFrame >= 16)
			{
				this.flip = true;
			}
		}

		// Token: 0x060036BF RID: 14015 RVA: 0x002B3938 File Offset: 0x002B1B38
		public Item TryGetGiftItem(List<PetGift> gifts)
		{
			float totalWeight = 0f;
			gifts = new List<PetGift>(gifts);
			gifts.RemoveAll(delegate(PetGift gift)
			{
				if (this.friendshipTowardFarmer.Value >= gift.MinimumFriendshipThreshold && GameStateQuery.CheckConditions(gift.Condition, null, null, null, null, null, null))
				{
					totalWeight += gift.Weight;
					return false;
				}
				return true;
			});
			if (gifts.Count > 0)
			{
				totalWeight = Utility.RandomFloat(0f, totalWeight, null);
				foreach (PetGift gift2 in gifts)
				{
					totalWeight -= gift2.Weight;
					if (totalWeight <= 0f)
					{
						Item item = ItemQueryResolver.TryResolveRandomItem(gift2, null, false, null, null, null, null);
						if (item != null && !item.Name.Contains("Error Item"))
						{
							return item;
						}
					}
				}
			}
			return null;
		}

		// Token: 0x060036C0 RID: 14016 RVA: 0x002B3A20 File Offset: 0x002B1C20
		public bool TryBehaviorChange(List<PetBehaviorChanges> changes)
		{
			float totalWeight = 0f;
			foreach (PetBehaviorChanges change in changes)
			{
				if (!change.OutsideOnly || base.currentLocation.IsOutdoors)
				{
					totalWeight += change.Weight;
				}
			}
			totalWeight = Utility.RandomFloat(0f, totalWeight, null);
			foreach (PetBehaviorChanges change2 in changes)
			{
				if (!change2.OutsideOnly || base.currentLocation.IsOutdoors)
				{
					totalWeight -= change2.Weight;
					if (totalWeight <= 0f)
					{
						string nextBehavior = null;
						switch (this.FacingDirection)
						{
						case 0:
							nextBehavior = change2.UpBehavior;
							break;
						case 1:
							nextBehavior = change2.RightBehavior;
							break;
						case 2:
							nextBehavior = change2.DownBehavior;
							break;
						case 3:
							nextBehavior = change2.LeftBehavior;
							break;
						}
						if (nextBehavior == null)
						{
							nextBehavior = change2.Behavior;
						}
						if (nextBehavior != null)
						{
							this.CurrentBehavior = nextBehavior;
						}
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060036C1 RID: 14017 RVA: 0x002B3B68 File Offset: 0x002B1D68
		public PetBehavior GetCurrentPetBehavior()
		{
			PetData petData = this.GetPetData();
			if (((petData != null) ? petData.Behaviors : null) != null)
			{
				foreach (PetBehavior behavior in petData.Behaviors)
				{
					if (behavior.Id == this.CurrentBehavior)
					{
						return behavior;
					}
				}
			}
			return null;
		}

		// Token: 0x060036C2 RID: 14018 RVA: 0x002B3BE4 File Offset: 0x002B1DE4
		public virtual void RunState(GameTime time)
		{
			if (this._currentBehavior == "Walk" && Game1.IsMasterGame && this._walkFromPushTimer <= 0 && base.currentLocation.isCollidingPosition(this.nextPosition(this.FacingDirection), Game1.viewport, this))
			{
				int new_direction = Game1.random.Next(0, 4);
				if (!base.currentLocation.isCollidingPosition(this.nextPosition(this.FacingDirection), Game1.viewport, this))
				{
					this.faceDirection(new_direction);
				}
			}
			if (Game1.IsMasterGame && Game1.timeOfDay >= 2000 && this.Sprite.CurrentAnimation == null && this.xVelocity == 0f && this.yVelocity == 0f)
			{
				this.CurrentBehavior = "Sleep";
			}
			if (this.CurrentBehavior == "Sleep")
			{
				if (Game1.IsMasterGame && Game1.timeOfDay < 2000 && Game1.random.NextDouble() < 0.001)
				{
					this.CurrentBehavior = "Walk";
				}
				if (Game1.random.NextDouble() < 0.002)
				{
					base.doEmote(24, true);
				}
			}
			if (this._walkFromPushTimer > 0)
			{
				this._walkFromPushTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
				if (this._walkFromPushTimer <= 0)
				{
					this._walkFromPushTimer = 0;
				}
			}
			PetBehavior behavior = this.GetCurrentPetBehavior();
			if (behavior != null && Game1.IsMasterGame)
			{
				if (this.behaviorTimer >= 0)
				{
					this.behaviorTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
					if (this.behaviorTimer <= 0)
					{
						this.behaviorTimer = -1;
						this.TryBehaviorChange(behavior.TimeoutBehaviorChanges);
						return;
					}
				}
				if (this._walkFromPushTimer <= 0)
				{
					if (behavior.RandomBehaviorChanges != null && behavior.RandomBehaviorChangeChance > 0f && Game1.random.NextDouble() < (double)behavior.RandomBehaviorChangeChance)
					{
						this.TryBehaviorChange(behavior.RandomBehaviorChanges);
						return;
					}
					if (behavior.PlayerNearbyBehaviorChanges != null && this.withinPlayerThreshold(2))
					{
						this.TryBehaviorChange(behavior.PlayerNearbyBehaviorChanges);
						return;
					}
				}
				if (behavior.JumpLandBehaviorChanges != null && this.yJumpOffset == 0 && this.yJumpVelocity == 0f)
				{
					this.TryBehaviorChange(behavior.JumpLandBehaviorChanges);
					return;
				}
			}
		}

		// Token: 0x060036C3 RID: 14019 RVA: 0x002B3E28 File Offset: 0x002B2028
		protected override void updateSlaveAnimation(GameTime time)
		{
			if (this.Sprite.CurrentAnimation != null)
			{
				this.Sprite.animateOnce(time);
				return;
			}
			if (this.CurrentBehavior == "Walk")
			{
				this.Sprite.faceDirection(this.FacingDirection);
				if (this.isMoving())
				{
					this.animateInFacingDirection(time);
					int target = -1;
					switch (this.FacingDirection)
					{
					case 0:
						target = 12;
						break;
					case 1:
						target = 8;
						break;
					case 2:
						target = 4;
						break;
					case 3:
						target = 16;
						break;
					}
					if (this.Sprite.CurrentFrame == target)
					{
						this.Sprite.CurrentFrame -= 4;
						return;
					}
				}
				else
				{
					this.Sprite.StopAnimation();
				}
				return;
			}
		}

		// Token: 0x060036C4 RID: 14020 RVA: 0x002B3EE1 File Offset: 0x002B20E1
		protected void _OnNewBehavior()
		{
			this._currentBehavior = this.CurrentBehavior;
			this.Halt();
			this.Sprite.CurrentAnimation = null;
			this.OnNewBehavior();
		}

		// Token: 0x060036C5 RID: 14021 RVA: 0x002B3F08 File Offset: 0x002B2108
		public virtual void OnNewBehavior()
		{
			this.Sprite.loop = false;
			this.Sprite.CurrentAnimation = null;
			this.behaviorTimer = -1;
			this.animationLoopsLeft = -1;
			if (this.CurrentBehavior == "Sleep")
			{
				this.Sprite.loop = true;
				bool local_sleep_flip = Game1.random.NextBool();
				this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(28, 1000, false, local_sleep_flip, null, false),
					new FarmerSprite.AnimationFrame(29, 1000, false, local_sleep_flip, null, false)
				});
			}
			PetBehavior behavior = this.GetCurrentPetBehavior();
			if (behavior != null)
			{
				if (Game1.IsMasterGame)
				{
					if (this._walkFromPushTimer <= 0)
					{
						int direction;
						if (Utility.TryParseDirection(behavior.Direction, out direction))
						{
							this.FacingDirection = direction;
						}
						if (behavior.RandomizeDirection)
						{
							this.FacingDirection = (behavior.IsSideBehavior ? Game1.random.Choose(3, 1) : Game1.random.Next(4));
						}
					}
					if ((this.FacingDirection == 0 || this.FacingDirection == 2) && behavior.IsSideBehavior)
					{
						this.FacingDirection = (Game1.random.NextBool() ? 3 : 1);
					}
					if (behavior.WalkInDirection)
					{
						if (behavior.MoveSpeed >= 0)
						{
							base.speed = behavior.MoveSpeed;
						}
						base.setMovingInFacingDirection();
					}
					if (behavior.Duration >= 0)
					{
						this.behaviorTimer = behavior.Duration;
					}
					else if (behavior.MinimumDuration >= 0 && behavior.MaximumDuration >= 0)
					{
						this.behaviorTimer = Game1.random.Next(behavior.MinimumDuration, behavior.MaximumDuration + 1);
					}
				}
				if (behavior.SoundOnStart != null)
				{
					this.PlaySound(behavior.SoundOnStart, behavior.SoundIsVoice, behavior.SoundRangeFromBorder, behavior.SoundRange);
				}
				if (behavior.Shake > 0)
				{
					base.shake(behavior.Shake);
				}
				if (behavior.Animation != null)
				{
					this.Sprite.ClearAnimation();
					for (int i = 0; i < behavior.Animation.Count; i++)
					{
						FarmerSprite.AnimationFrame frame = new FarmerSprite.AnimationFrame(behavior.Animation[i].Frame, behavior.Animation[i].Duration, false, false, null, false);
						if (behavior.Animation[i].HitGround)
						{
							frame.AddFrameAction(new AnimatedSprite.endOfAnimationBehavior(this.hitGround));
						}
						if (behavior.Animation[i].Jump)
						{
							this.jump();
						}
						if (behavior.AnimationMinimumLoops >= 0 && behavior.AnimationMaximumLoops >= 0)
						{
							this.animationLoopsLeft = Game1.random.Next(behavior.AnimationMinimumLoops, behavior.AnimationMaximumLoops + 1);
						}
						if (behavior.Animation[i].Sound != null)
						{
							frame.AddFrameAction(new AnimatedSprite.endOfAnimationBehavior(this._PerformAnimationSound));
						}
						if (i == behavior.Animation.Count - 1)
						{
							if (this.animationLoopsLeft > 0 || behavior.AnimationEndBehaviorChanges != null)
							{
								frame.AddFrameEndAction(new AnimatedSprite.endOfAnimationBehavior(this._TryAnimationEndBehaviorChange));
							}
							if (behavior.LoopMode == PetAnimationLoopMode.Hold)
							{
								if (behavior.AnimationEndBehaviorChanges != null)
								{
									frame.AddFrameEndAction(new AnimatedSprite.endOfAnimationBehavior(this.hold));
								}
								else
								{
									frame.AddFrameAction(new AnimatedSprite.endOfAnimationBehavior(this.hold));
								}
							}
						}
						this.Sprite.AddFrame(frame);
						if (behavior.Animation.Count == 1 && behavior.LoopMode == PetAnimationLoopMode.Hold)
						{
							this.Sprite.AddFrame(frame);
						}
						this.Sprite.UpdateSourceRect();
					}
					this.Sprite.loop = (behavior.LoopMode == PetAnimationLoopMode.Loop || this.animationLoopsLeft > 0);
				}
			}
		}

		// Token: 0x060036C6 RID: 14022 RVA: 0x002B429C File Offset: 0x002B249C
		public void _PerformAnimationSound(Farmer who)
		{
			PetBehavior behavior = this.GetCurrentPetBehavior();
			if (((behavior != null) ? behavior.Animation : null) != null && this.Sprite.currentAnimationIndex >= 0 && this.Sprite.currentAnimationIndex < behavior.Animation.Count)
			{
				PetAnimationFrame frame = behavior.Animation[this.Sprite.currentAnimationIndex];
				if (frame.Sound != null)
				{
					this.PlaySound(frame.Sound, frame.SoundIsVoice, frame.SoundRangeFromBorder, frame.SoundRange);
				}
			}
		}

		// Token: 0x060036C7 RID: 14023 RVA: 0x002B4324 File Offset: 0x002B2524
		public void PlaySound(string sound, bool is_voice, int range_from_border, int range)
		{
			if (Game1.options.muteAnimalSounds && is_voice)
			{
				return;
			}
			if (!this.IsSoundInRange(range_from_border, range))
			{
				return;
			}
			float pitch = 1f;
			PetBreed breed = this.GetPetData().GetBreedById(this.whichBreed.Value, false);
			if (sound == "BARK")
			{
				sound = this.GetPetData().BarkSound;
				if (breed.BarkOverride != null)
				{
					sound = breed.BarkOverride;
				}
			}
			if (is_voice)
			{
				pitch = breed.VoicePitch;
			}
			if (pitch != 1f)
			{
				base.playNearbySoundAll(sound, new int?((int)(1200f * pitch)), SoundContext.Default);
				return;
			}
			Game1.playSound(sound, null);
		}

		// Token: 0x060036C8 RID: 14024 RVA: 0x002B43CC File Offset: 0x002B25CC
		public bool IsSoundInRange(int range_from_border, int sound_range)
		{
			if (sound_range > 0)
			{
				return this.withinLocalPlayerThreshold(sound_range);
			}
			return range_from_border <= 0 || Utility.isOnScreen(base.TilePoint, range_from_border * 64, base.currentLocation);
		}

		// Token: 0x060036C9 RID: 14025 RVA: 0x002B43F8 File Offset: 0x002B25F8
		public virtual void _TryAnimationEndBehaviorChange(Farmer who)
		{
			if (this.animationLoopsLeft <= 0)
			{
				if (this.animationLoopsLeft == 0)
				{
					this.animationLoopsLeft = -1;
					this.hold(who);
				}
				PetBehavior behavior = this.GetCurrentPetBehavior();
				if (behavior != null && Game1.IsMasterGame)
				{
					this.TryBehaviorChange(behavior.AnimationEndBehaviorChanges);
				}
				return;
			}
			this.animationLoopsLeft--;
		}

		// Token: 0x060036CA RID: 14026 RVA: 0x002B4454 File Offset: 0x002B2654
		public override Rectangle GetBoundingBox()
		{
			Vector2 position = base.Position;
			return new Rectangle((int)position.X + 16, (int)position.Y + 16, this.Sprite.SpriteWidth * 4 * 3 / 4, 32);
		}

		// Token: 0x060036CB RID: 14027 RVA: 0x002B4494 File Offset: 0x002B2694
		public virtual void drawHat(SpriteBatch b, Vector2 shake)
		{
			if (this.hat.Value != null)
			{
				Vector2 hatOffset = Vector2.Zero;
				hatOffset *= 4f;
				if (hatOffset.X <= -100f)
				{
					return;
				}
				float horse_draw_layer = Math.Max(0f, this.isSleepingOnFarmerBed.Value ? (((float)base.StandingPixel.Y + 112f) / 10000f) : ((float)base.StandingPixel.Y / 10000f));
				hatOffset.X = -2f;
				hatOffset.Y = -24f;
				horse_draw_layer += 1E-07f;
				int direction = 2;
				bool flipped = this.flip || (this.sprite.Value.CurrentAnimation != null && this.sprite.Value.CurrentAnimation[this.sprite.Value.currentAnimationIndex].flip);
				float scale = 1.3333334f;
				string value = this.petType.Value;
				if (!(value == "Cat"))
				{
					if (value == "Dog")
					{
						hatOffset.Y -= 20f;
						switch (this.Sprite.CurrentFrame)
						{
						case 0:
						case 2:
							hatOffset.Y += 28f;
							direction = 2;
							break;
						case 1:
						case 3:
							hatOffset.Y += 32f;
							direction = 2;
							break;
						case 4:
						case 6:
							direction = 1;
							hatOffset.X += 26f;
							hatOffset.Y += 24f;
							break;
						case 5:
						case 7:
							direction = 1;
							hatOffset.X += 26f;
							hatOffset.Y += 28f;
							break;
						case 8:
						case 10:
							direction = 0;
							hatOffset.Y += 4f;
							break;
						case 9:
						case 11:
							direction = 0;
							hatOffset.Y += 8f;
							break;
						case 12:
						case 14:
							direction = 3;
							hatOffset.X -= 26f;
							hatOffset.Y += 24f;
							break;
						case 13:
						case 15:
							hatOffset.Y += 24f;
							hatOffset.Y += 4f;
							direction = 3;
							hatOffset.X -= 26f;
							break;
						case 16:
							hatOffset.Y += 20f;
							direction = 2;
							break;
						case 17:
							hatOffset.Y += 12f;
							break;
						case 18:
						case 19:
							hatOffset.Y += 8f;
							break;
						case 20:
							direction = (flipped ? 3 : 1);
							hatOffset.X += 26f;
							hatOffset.Y += (float)((this.whichBreed.Value == "2") ? 16 : ((this.whichBreed.Value == "1") ? 24 : 20));
							break;
						case 21:
							direction = (flipped ? 3 : 1);
							hatOffset.X += 22f;
							hatOffset.Y += (float)((this.whichBreed.Value == "2") ? 12 : ((this.whichBreed.Value == "1") ? 20 : 16));
							break;
						case 22:
							direction = (flipped ? 3 : 1);
							hatOffset.X += 18f;
							hatOffset.Y += (float)((this.whichBreed.Value == "2") ? 8 : ((this.whichBreed.Value == "1") ? 8 : 12));
							break;
						case 23:
							direction = (flipped ? 3 : 1);
							hatOffset.X += 18f;
							hatOffset.Y += 8f;
							break;
						case 24:
						case 25:
							direction = (flipped ? 3 : 1);
							hatOffset.X += (float)(21 - (flipped ? 4 : 4) + 1);
							hatOffset.Y += 8f;
							break;
						case 26:
							direction = (flipped ? 3 : 1);
							hatOffset.X += 18f;
							hatOffset.Y -= 8f;
							break;
						case 27:
							direction = 2;
							hatOffset.Y += (float)(12 + ((this.whichBreed.Value == "2") ? -4 : 0));
							break;
						case 28:
						case 29:
							scale = 1.3333334f;
							hatOffset.Y += 48f;
							hatOffset.X += (float)((flipped ? 6 : 5) * 4);
							hatOffset.X += 2f;
							direction = 2;
							break;
						case 30:
						case 31:
							direction = (flipped ? 3 : 1);
							hatOffset.X += 18f;
							hatOffset.Y += 8f;
							break;
						case 32:
							direction = (flipped ? 3 : 1);
							hatOffset.X += 26f;
							hatOffset.Y += (float)((this.whichBreed.Value == "2") ? 12 : 16);
							break;
						case 33:
							direction = (flipped ? 3 : 1);
							hatOffset.X += 26f;
							hatOffset.Y += (float)((this.whichBreed.Value == "2") ? 16 : 20);
							break;
						case 34:
							direction = (flipped ? 3 : 1);
							hatOffset.X += 26f;
							hatOffset.Y += (float)((this.whichBreed.Value == "2") ? 20 : 24);
							break;
						}
						string value2 = this.whichBreed.Value;
						if (!(value2 == "2"))
						{
							if (value2 == "3")
							{
								if (direction == 3 && this.Sprite.CurrentFrame > 16)
								{
									hatOffset.X += 4f;
								}
							}
						}
						else
						{
							if (direction == 1)
							{
								hatOffset.X -= 4f;
							}
							hatOffset.Y += 8f;
						}
						if (flipped)
						{
							hatOffset.X *= -1f;
						}
					}
				}
				else
				{
					switch (this.Sprite.CurrentFrame)
					{
					case 0:
					case 2:
						hatOffset.Y += 28f;
						direction = 2;
						break;
					case 1:
					case 3:
						hatOffset.Y += 32f;
						direction = 2;
						break;
					case 4:
					case 6:
						direction = 1;
						hatOffset.X += 23f;
						hatOffset.Y += 20f;
						break;
					case 5:
					case 7:
						hatOffset.Y += 4f;
						direction = 1;
						hatOffset.X += 23f;
						hatOffset.Y += 20f;
						break;
					case 8:
					case 10:
						direction = 0;
						hatOffset.Y -= 4f;
						break;
					case 9:
					case 11:
						direction = 0;
						break;
					case 12:
					case 14:
						direction = 3;
						hatOffset.X -= 22f;
						hatOffset.Y += 20f;
						break;
					case 13:
					case 15:
						hatOffset.Y += 20f;
						hatOffset.Y += 4f;
						direction = 3;
						hatOffset.X -= 22f;
						break;
					case 16:
						hatOffset.Y += 20f;
						direction = 2;
						break;
					case 17:
					case 20:
					case 22:
						hatOffset.Y += 12f;
						break;
					case 18:
					case 19:
						hatOffset.Y += 8f;
						break;
					case 21:
					case 23:
						hatOffset.Y += 16f;
						break;
					case 24:
						direction = (flipped ? 3 : 1);
						hatOffset.X += (float)((flipped ? -1 : 1) * 29);
						hatOffset.Y += 28f;
						break;
					case 25:
						direction = (flipped ? 3 : 1);
						hatOffset.X += (float)((flipped ? -1 : 1) * 29);
						hatOffset.Y += 36f;
						break;
					case 26:
						direction = (flipped ? 3 : 1);
						hatOffset.X += (float)((flipped ? -1 : 1) * 29);
						hatOffset.Y += 40f;
						break;
					case 27:
						direction = (flipped ? 3 : 1);
						hatOffset.X += (float)((flipped ? -1 : 1) * 29);
						hatOffset.Y += 44f;
						break;
					case 28:
					case 29:
						scale = 1.2f;
						hatOffset.Y += 46f;
						hatOffset.X -= (float)((flipped ? 0 : -1) * 4);
						hatOffset.X += (float)((flipped ? -1 : 1) * 2);
						direction = (flipped ? 1 : 3);
						break;
					case 30:
					case 31:
						direction = (flipped ? 3 : 1);
						hatOffset.X += (float)((flipped ? -1 : 1) * 25);
						hatOffset.Y += 32f;
						break;
					}
					if ((this.whichBreed.Value == "3" || this.whichBreed.Value == "4") && direction == 3)
					{
						hatOffset.X -= 4f;
					}
				}
				hatOffset += shake;
				if (flipped)
				{
					hatOffset.X -= 4f;
				}
				this.hat.Value.draw(b, base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(30f, -42f), scale, 1f, horse_draw_layer, direction, true);
			}
		}

		// Token: 0x060036CC RID: 14028 RVA: 0x002B4F3C File Offset: 0x002B313C
		public override void draw(SpriteBatch b)
		{
			int standingY = base.StandingPixel.Y;
			Vector2 shake = (this.shakeTimer > 0 && !this.isSleepingOnFarmerBed.Value) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero;
			b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2((float)(this.Sprite.SpriteWidth * 4 / 2), (float)(this.GetBoundingBox().Height / 2)) + shake, new Rectangle?(this.Sprite.SourceRect), Color.White, this.rotation, new Vector2((float)(this.Sprite.SpriteWidth / 2), (float)this.Sprite.SpriteHeight * 3f / 4f), Math.Max(0.2f, this.scale.Value) * 4f, (this.flip || (this.Sprite.CurrentAnimation != null && this.Sprite.CurrentAnimation[this.Sprite.currentAnimationIndex].flip)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.isSleepingOnFarmerBed.Value ? (((float)standingY + 112f) / 10000f) : ((float)standingY / 10000f)));
			this.drawHat(b, shake);
			if (base.IsEmoting)
			{
				Vector2 localPosition = base.getLocalPosition(Game1.viewport);
				PetData petData = this.GetPetData();
				Point emoteOffset = (petData != null) ? petData.EmoteOffset : Point.Zero;
				Vector2 emotePosition = new Vector2(localPosition.X + 32f + (float)emoteOffset.X, localPosition.Y - 96f + (float)emoteOffset.Y);
				b.Draw(Game1.emoteSpriteSheet, emotePosition, new Rectangle?(new Rectangle(base.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, base.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)standingY / 10000f + 0.0001f);
			}
		}

		// Token: 0x060036CD RID: 14029 RVA: 0x002B5178 File Offset: 0x002B3378
		public virtual bool withinLocalPlayerThreshold(int threshold)
		{
			if (base.currentLocation != Game1.currentLocation)
			{
				return false;
			}
			Vector2 tileLocationOfMonster = base.Tile;
			Vector2 tileLocationOfPlayer = Game1.player.Tile;
			return Math.Abs(tileLocationOfMonster.X - tileLocationOfPlayer.X) <= (float)threshold && Math.Abs(tileLocationOfMonster.Y - tileLocationOfPlayer.Y) <= (float)threshold;
		}

		// Token: 0x060036CE RID: 14030 RVA: 0x002B51D8 File Offset: 0x002B33D8
		public override bool withinPlayerThreshold(int threshold)
		{
			if (base.currentLocation != null && !base.currentLocation.farmers.Any())
			{
				return false;
			}
			Vector2 tileLocationOfMonster = base.Tile;
			foreach (Farmer farmer in base.currentLocation.farmers)
			{
				Vector2 tileLocationOfPlayer = farmer.Tile;
				if (Math.Abs(tileLocationOfMonster.X - tileLocationOfPlayer.X) <= (float)threshold && Math.Abs(tileLocationOfMonster.Y - tileLocationOfPlayer.Y) <= (float)threshold)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060036CF RID: 14031 RVA: 0x002B5284 File Offset: 0x002B3484
		public void hitGround(Farmer who)
		{
			if (Utility.isOnScreen(base.TilePoint, 128, base.currentLocation))
			{
				base.currentLocation.playTerrainSound(base.Tile, this, false);
			}
		}

		// Token: 0x040023AE RID: 9134
		public const string type_cat = "Cat";

		// Token: 0x040023AF RID: 9135
		public const string type_dog = "Dog";

		// Token: 0x040023B0 RID: 9136
		[XmlElement("guid")]
		public NetGuid petId = new NetGuid(Guid.NewGuid());

		// Token: 0x040023B1 RID: 9137
		public const int bedTime = 2000;

		// Token: 0x040023B2 RID: 9138
		public const int maxFriendship = 1000;

		// Token: 0x040023B3 RID: 9139
		public const string behavior_Walk = "Walk";

		// Token: 0x040023B4 RID: 9140
		public const string behavior_Sleep = "Sleep";

		// Token: 0x040023B5 RID: 9141
		public const string behavior_SitDown = "SitDown";

		// Token: 0x040023B6 RID: 9142
		public const string behavior_Sprint = "Sprint";

		// Token: 0x040023B7 RID: 9143
		protected int behaviorTimer = -1;

		// Token: 0x040023B8 RID: 9144
		protected int animationLoopsLeft;

		// Token: 0x040023B9 RID: 9145
		[XmlElement("petType")]
		public readonly NetString petType = new NetString("Dog");

		// Token: 0x040023BA RID: 9146
		[XmlElement("whichBreed")]
		public readonly NetString whichBreed = new NetString("0");

		// Token: 0x040023BB RID: 9147
		private readonly NetString netCurrentBehavior = new NetString();

		// Token: 0x040023BC RID: 9148
		[XmlElement("homeLocationName")]
		public readonly NetString homeLocationName = new NetString();

		// Token: 0x040023BD RID: 9149
		[XmlIgnore]
		public readonly NetEvent1Field<long, NetLong> petPushEvent = new NetEvent1Field<long, NetLong>();

		// Token: 0x040023BE RID: 9150
		[XmlIgnore]
		protected string _currentBehavior;

		// Token: 0x040023BF RID: 9151
		[XmlElement("lastPetDay")]
		public NetLongDictionary<int, NetInt> lastPetDay = new NetLongDictionary<int, NetInt>();

		// Token: 0x040023C0 RID: 9152
		[XmlElement("grantedFriendshipForPet")]
		public NetBool grantedFriendshipForPet = new NetBool(false);

		// Token: 0x040023C1 RID: 9153
		[XmlElement("friendshipTowardFarmer")]
		public NetInt friendshipTowardFarmer = new NetInt(0);

		// Token: 0x040023C2 RID: 9154
		[XmlElement("timesPet")]
		public NetInt timesPet = new NetInt(0);

		// Token: 0x040023C3 RID: 9155
		[XmlElement("hat")]
		public readonly NetRef<Hat> hat = new NetRef<Hat>();

		// Token: 0x040023C4 RID: 9156
		protected int _walkFromPushTimer;

		// Token: 0x040023C5 RID: 9157
		public NetBool isSleepingOnFarmerBed = new NetBool(false);

		// Token: 0x040023C6 RID: 9158
		[XmlIgnore]
		public readonly NetMutex mutex = new NetMutex();

		// Token: 0x040023C7 RID: 9159
		private int pushingTimer;
	}
}

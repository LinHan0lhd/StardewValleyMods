using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Characters;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Logging;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Pathfinding;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using xTile.Dimensions;

namespace StardewValley
{
	// Token: 0x020000EA RID: 234
	[XmlInclude(typeof(Cat))]
	[XmlInclude(typeof(Child))]
	[XmlInclude(typeof(Dog))]
	[XmlInclude(typeof(Horse))]
	[XmlInclude(typeof(Junimo))]
	[XmlInclude(typeof(JunimoHarvester))]
	[XmlInclude(typeof(Pet))]
	[XmlInclude(typeof(TrashBear))]
	[XmlInclude(typeof(Raccoon))]
	[XmlInclude(typeof(Monster))]
	public class NPC : Character, IComparable
	{
		// Token: 0x170001F9 RID: 505
		// (get) Token: 0x06001197 RID: 4503 RVA: 0x000CC32F File Offset: 0x000CA52F
		// (set) Token: 0x06001198 RID: 4504 RVA: 0x000CC337 File Offset: 0x000CA537
		[XmlIgnore]
		public SchedulePathDescription DirectionsToNewLocation
		{
			get
			{
				return this.directionsToNewLocation;
			}
			set
			{
				this.directionsToNewLocation = value;
			}
		}

		// Token: 0x170001FA RID: 506
		// (get) Token: 0x06001199 RID: 4505 RVA: 0x000CC340 File Offset: 0x000CA540
		// (set) Token: 0x0600119A RID: 4506 RVA: 0x000CC348 File Offset: 0x000CA548
		public int DefaultFacingDirection
		{
			get
			{
				return this.defaultFacingDirection;
			}
			set
			{
				this.defaultFacingDirection = value;
			}
		}

		// Token: 0x170001FB RID: 507
		// (get) Token: 0x0600119B RID: 4507 RVA: 0x000CC354 File Offset: 0x000CA554
		[XmlIgnore]
		public Dictionary<string, string> Dialogue
		{
			get
			{
				if (this is Monster || this is Pet || this is Horse || this is Child)
				{
					this.LoadedDialogueKey = null;
					return null;
				}
				if (this.dialogue == null)
				{
					string dialogue_file = "Characters\\Dialogue\\" + this.GetDialogueSheetName();
					if (NPC.invalidDialogueFiles.Contains(dialogue_file))
					{
						this.LoadedDialogueKey = null;
						this.dialogue = new Dictionary<string, string>();
					}
					try
					{
						this.dialogue = Game1.content.Load<Dictionary<string, string>>(dialogue_file).Select(delegate(KeyValuePair<string, string> pair)
						{
							string key = pair.Key;
							string text = pair.Value;
							text = StardewValley.Dialogue.applyGenderSwitch(Game1.player.Gender, text, true);
							return new KeyValuePair<string, string>(key, text);
						}).ToDictionary((KeyValuePair<string, string> p) => p.Key, (KeyValuePair<string, string> p) => p.Value);
						this.LoadedDialogueKey = dialogue_file;
					}
					catch (ContentLoadException)
					{
						NPC.invalidDialogueFiles.Add(dialogue_file);
						this.dialogue = new Dictionary<string, string>();
						this.LoadedDialogueKey = null;
					}
				}
				return this.dialogue;
			}
		}

		// Token: 0x170001FC RID: 508
		// (get) Token: 0x0600119C RID: 4508 RVA: 0x000CC480 File Offset: 0x000CA680
		// (set) Token: 0x0600119D RID: 4509 RVA: 0x000CC488 File Offset: 0x000CA688
		[XmlIgnore]
		public string LoadedDialogueKey { get; private set; }

		// Token: 0x170001FD RID: 509
		// (get) Token: 0x0600119E RID: 4510 RVA: 0x000CC491 File Offset: 0x000CA691
		// (set) Token: 0x0600119F RID: 4511 RVA: 0x000CC49E File Offset: 0x000CA69E
		[XmlIgnore]
		public string DefaultMap
		{
			get
			{
				return this.defaultMap.Value;
			}
			set
			{
				this.defaultMap.Value = value;
			}
		}

		// Token: 0x170001FE RID: 510
		// (get) Token: 0x060011A0 RID: 4512 RVA: 0x000CC4AC File Offset: 0x000CA6AC
		// (set) Token: 0x060011A1 RID: 4513 RVA: 0x000CC4B9 File Offset: 0x000CA6B9
		public Vector2 DefaultPosition
		{
			get
			{
				return this.defaultPosition.Value;
			}
			set
			{
				this.defaultPosition.Value = value;
			}
		}

		// Token: 0x170001FF RID: 511
		// (get) Token: 0x060011A2 RID: 4514 RVA: 0x000CC4C7 File Offset: 0x000CA6C7
		// (set) Token: 0x060011A3 RID: 4515 RVA: 0x000CC4E6 File Offset: 0x000CA6E6
		[XmlIgnore]
		public Texture2D Portrait
		{
			get
			{
				if (this.portrait == null && this.IsVillager)
				{
					this.ChooseAppearance(null);
				}
				return this.portrait;
			}
			set
			{
				this.portrait = value;
			}
		}

		// Token: 0x17000200 RID: 512
		// (get) Token: 0x060011A4 RID: 4516 RVA: 0x000CC4EF File Offset: 0x000CA6EF
		// (set) Token: 0x060011A5 RID: 4517 RVA: 0x000CC4F7 File Offset: 0x000CA6F7
		[XmlIgnore]
		public bool AllowDynamicAppearance { get; set; } = true;

		// Token: 0x17000201 RID: 513
		// (get) Token: 0x060011A6 RID: 4518 RVA: 0x000CC500 File Offset: 0x000CA700
		[XmlIgnore]
		public override bool IsVillager
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000202 RID: 514
		// (get) Token: 0x060011A7 RID: 4519 RVA: 0x000CC503 File Offset: 0x000CA703
		// (set) Token: 0x060011A8 RID: 4520 RVA: 0x000CC50B File Offset: 0x000CA70B
		[XmlIgnore]
		public Dictionary<int, SchedulePathDescription> Schedule { get; private set; }

		// Token: 0x17000203 RID: 515
		// (get) Token: 0x060011A9 RID: 4521 RVA: 0x000CC514 File Offset: 0x000CA714
		[XmlIgnore]
		public string ScheduleKey
		{
			get
			{
				return this.dayScheduleName.Value;
			}
		}

		// Token: 0x17000204 RID: 516
		// (get) Token: 0x060011AA RID: 4522 RVA: 0x000CC521 File Offset: 0x000CA721
		// (set) Token: 0x060011AB RID: 4523 RVA: 0x000CC529 File Offset: 0x000CA729
		public bool IsWalkingInSquare
		{
			get
			{
				return this.isWalkingInSquare;
			}
			set
			{
				this.isWalkingInSquare = value;
			}
		}

		// Token: 0x17000205 RID: 517
		// (get) Token: 0x060011AC RID: 4524 RVA: 0x000CC532 File Offset: 0x000CA732
		// (set) Token: 0x060011AD RID: 4525 RVA: 0x000CC53F File Offset: 0x000CA73F
		public bool IsWalkingTowardPlayer
		{
			get
			{
				return this.isWalkingTowardPlayer.Value;
			}
			set
			{
				this.isWalkingTowardPlayer.Value = value;
			}
		}

		// Token: 0x17000206 RID: 518
		// (get) Token: 0x060011AE RID: 4526 RVA: 0x000CC550 File Offset: 0x000CA750
		// (set) Token: 0x060011AF RID: 4527 RVA: 0x000CC5BC File Offset: 0x000CA7BC
		[XmlIgnore]
		public virtual Stack<Dialogue> CurrentDialogue
		{
			get
			{
				if (this.TemporaryDialogue != null)
				{
					return this.TemporaryDialogue;
				}
				if (Game1.npcDialogues == null)
				{
					Game1.npcDialogues = new Dictionary<string, Stack<Dialogue>>();
				}
				if (!this.IsVillager)
				{
					return NPC._EmptyDialogue;
				}
				Stack<Dialogue> currentDialogue;
				Game1.npcDialogues.TryGetValue(base.Name, out currentDialogue);
				if (currentDialogue == null)
				{
					currentDialogue = (Game1.npcDialogues[base.Name] = this.loadCurrentDialogue());
				}
				return currentDialogue;
			}
			set
			{
				if (this.TemporaryDialogue != null)
				{
					this.TemporaryDialogue = value;
					return;
				}
				if (Game1.npcDialogues != null)
				{
					Game1.npcDialogues[base.Name] = value;
				}
			}
		}

		// Token: 0x17000207 RID: 519
		// (get) Token: 0x060011B0 RID: 4528 RVA: 0x000CC5E6 File Offset: 0x000CA7E6
		// (set) Token: 0x060011B1 RID: 4529 RVA: 0x000CC5F3 File Offset: 0x000CA7F3
		[XmlIgnore]
		public string Birthday_Season
		{
			get
			{
				return this.birthday_Season.Value;
			}
			set
			{
				this.birthday_Season.Value = value;
			}
		}

		// Token: 0x17000208 RID: 520
		// (get) Token: 0x060011B2 RID: 4530 RVA: 0x000CC601 File Offset: 0x000CA801
		// (set) Token: 0x060011B3 RID: 4531 RVA: 0x000CC60E File Offset: 0x000CA80E
		[XmlIgnore]
		public int Birthday_Day
		{
			get
			{
				return this.birthday_Day.Value;
			}
			set
			{
				this.birthday_Day.Value = value;
			}
		}

		// Token: 0x17000209 RID: 521
		// (get) Token: 0x060011B4 RID: 4532 RVA: 0x000CC61C File Offset: 0x000CA81C
		// (set) Token: 0x060011B5 RID: 4533 RVA: 0x000CC629 File Offset: 0x000CA829
		[XmlIgnore]
		public int Age
		{
			get
			{
				return this.age.Value;
			}
			set
			{
				this.age.Value = value;
			}
		}

		// Token: 0x1700020A RID: 522
		// (get) Token: 0x060011B6 RID: 4534 RVA: 0x000CC637 File Offset: 0x000CA837
		// (set) Token: 0x060011B7 RID: 4535 RVA: 0x000CC644 File Offset: 0x000CA844
		[XmlIgnore]
		public int Manners
		{
			get
			{
				return this.manners.Value;
			}
			set
			{
				this.manners.Value = value;
			}
		}

		// Token: 0x1700020B RID: 523
		// (get) Token: 0x060011B8 RID: 4536 RVA: 0x000CC652 File Offset: 0x000CA852
		// (set) Token: 0x060011B9 RID: 4537 RVA: 0x000CC65F File Offset: 0x000CA85F
		[XmlIgnore]
		public int SocialAnxiety
		{
			get
			{
				return this.socialAnxiety.Value;
			}
			set
			{
				this.socialAnxiety.Value = value;
			}
		}

		// Token: 0x1700020C RID: 524
		// (get) Token: 0x060011BA RID: 4538 RVA: 0x000CC66D File Offset: 0x000CA86D
		// (set) Token: 0x060011BB RID: 4539 RVA: 0x000CC67A File Offset: 0x000CA87A
		[XmlIgnore]
		public int Optimism
		{
			get
			{
				return this.optimism.Value;
			}
			set
			{
				this.optimism.Value = value;
			}
		}

		// Token: 0x1700020D RID: 525
		// (get) Token: 0x060011BC RID: 4540 RVA: 0x000CC688 File Offset: 0x000CA888
		// (set) Token: 0x060011BD RID: 4541 RVA: 0x000CC695 File Offset: 0x000CA895
		[XmlIgnore]
		public override Gender Gender
		{
			get
			{
				return this.gender.Value;
			}
			set
			{
				this.gender.Value = value;
			}
		}

		// Token: 0x1700020E RID: 526
		// (get) Token: 0x060011BE RID: 4542 RVA: 0x000CC6A3 File Offset: 0x000CA8A3
		// (set) Token: 0x060011BF RID: 4543 RVA: 0x000CC6B0 File Offset: 0x000CA8B0
		[XmlIgnore]
		public bool Breather
		{
			get
			{
				return this.breather.Value;
			}
			set
			{
				this.breather.Value = value;
			}
		}

		// Token: 0x1700020F RID: 527
		// (get) Token: 0x060011C0 RID: 4544 RVA: 0x000CC6BE File Offset: 0x000CA8BE
		// (set) Token: 0x060011C1 RID: 4545 RVA: 0x000CC6CB File Offset: 0x000CA8CB
		[XmlIgnore]
		public bool HideShadow
		{
			get
			{
				return this.hideShadow.Value;
			}
			set
			{
				this.hideShadow.Value = value;
			}
		}

		// Token: 0x17000210 RID: 528
		// (get) Token: 0x060011C2 RID: 4546 RVA: 0x000CC6DC File Offset: 0x000CA8DC
		[XmlIgnore]
		public bool HasPartnerForDance
		{
			get
			{
				using (FarmerCollection.Enumerator enumerator = Game1.getOnlineFarmers().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.dancePartner.TryGetVillager() == this)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		// Token: 0x17000211 RID: 529
		// (get) Token: 0x060011C3 RID: 4547 RVA: 0x000CC73C File Offset: 0x000CA93C
		// (set) Token: 0x060011C4 RID: 4548 RVA: 0x000CC749 File Offset: 0x000CA949
		[XmlIgnore]
		public bool IsInvisible
		{
			get
			{
				return this.isInvisible.Value;
			}
			set
			{
				this.isInvisible.Value = value;
			}
		}

		// Token: 0x17000212 RID: 530
		// (get) Token: 0x060011C5 RID: 4549 RVA: 0x000CC757 File Offset: 0x000CA957
		public virtual bool CanSocialize
		{
			get
			{
				return this.IsVillager && NPC.CanSocializePerData(base.Name, base.currentLocation);
			}
		}

		// Token: 0x060011C6 RID: 4550 RVA: 0x000CC774 File Offset: 0x000CA974
		public NPC()
		{
		}

		// Token: 0x060011C7 RID: 4551 RVA: 0x000CC940 File Offset: 0x000CAB40
		public NPC(AnimatedSprite sprite, Vector2 position, int facingDir, string name, LocalizedContentManager content = null) : base(sprite, position, 2, name)
		{
			this.faceDirection(facingDir);
			this.defaultPosition.Value = position;
			this.defaultFacingDirection = facingDir;
			this.lastCrossroad = new Microsoft.Xna.Framework.Rectangle((int)position.X, (int)position.Y + 64, 64, 64);
			if (content != null)
			{
				try
				{
					this.portrait = content.Load<Texture2D>("Portraits\\" + name);
				}
				catch (Exception)
				{
				}
			}
		}

		// Token: 0x060011C8 RID: 4552 RVA: 0x000CCB80 File Offset: 0x000CAD80
		public NPC(AnimatedSprite sprite, Vector2 position, string defaultMap, int facingDirection, string name, bool datable, Texture2D portrait) : this(sprite, position, defaultMap, facingDirection, name, portrait, false)
		{
			this.datable.Value = datable;
		}

		// Token: 0x060011C9 RID: 4553 RVA: 0x000CCBA0 File Offset: 0x000CADA0
		public NPC(AnimatedSprite sprite, Vector2 position, string defaultMap, int facingDir, string name, Texture2D portrait, bool eventActor) : base(sprite, position, 2, name)
		{
			this.portrait = portrait;
			this.faceDirection(facingDir);
			if (!eventActor)
			{
				this.lastCrossroad = new Microsoft.Xna.Framework.Rectangle((int)position.X, (int)position.Y + 64, 64, 64);
			}
			this.reloadData();
			this.defaultPosition.Value = position;
			this.defaultMap.Value = defaultMap;
			base.currentLocation = Game1.getLocationFromName(defaultMap);
			this.defaultFacingDirection = facingDir;
		}

		// Token: 0x060011CA RID: 4554 RVA: 0x000CCDD8 File Offset: 0x000CAFD8
		public virtual void reloadData()
		{
			if (this is Child)
			{
				return;
			}
			CharacterData data = this.GetData();
			if (data == null)
			{
				return;
			}
			this.Age = (int)Utility.GetEnumOrDefault<NpcAge>(data.Age, NpcAge.Adult);
			this.Manners = (int)Utility.GetEnumOrDefault<NpcManner>(data.Manner, NpcManner.Neutral);
			this.SocialAnxiety = (int)Utility.GetEnumOrDefault<NpcSocialAnxiety>(data.SocialAnxiety, NpcSocialAnxiety.Outgoing);
			this.Optimism = (int)Utility.GetEnumOrDefault<NpcOptimism>(data.Optimism, NpcOptimism.Positive);
			this.Gender = Utility.GetEnumOrDefault<Gender>(data.Gender, Gender.Male);
			this.datable.Value = data.CanBeRomanced;
			this.loveInterest = data.LoveInterest;
			this.Birthday_Season = ((data.BirthSeason != null) ? Utility.getSeasonKey(data.BirthSeason.Value) : null);
			this.Birthday_Day = data.BirthDay;
			this.id = ((data.FestivalVanillaActorIndex > -1) ? data.FestivalVanillaActorIndex : Game1.hash.GetDeterministicHashCode(this.name.Value));
			this.breather.Value = data.Breather;
			if (!this.isMarried())
			{
				this.reloadDefaultLocation();
			}
			this.displayName = this.translateName();
		}

		// Token: 0x060011CB RID: 4555 RVA: 0x000CCEFC File Offset: 0x000CB0FC
		public virtual void reloadDefaultLocation()
		{
			CharacterData data = this.GetData();
			string locationName;
			Point tile;
			int direction;
			if (data != null && NPC.ReadNpcHomeData(data, base.currentLocation, out locationName, out tile, out direction))
			{
				this.DefaultMap = locationName;
				this.DefaultPosition = new Vector2((float)(tile.X * 64), (float)(tile.Y * 64));
				this.DefaultFacingDirection = direction;
			}
		}

		// Token: 0x060011CC RID: 4556 RVA: 0x000CCF54 File Offset: 0x000CB154
		public static bool ReadNpcHomeData(CharacterData data, GameLocation currentLocation, out string locationName, out Point tile, out int direction)
		{
			if (((data != null) ? data.Home : null) != null)
			{
				foreach (CharacterHomeData home in data.Home)
				{
					if (home.Condition == null || GameStateQuery.CheckConditions(home.Condition, currentLocation, null, null, null, null, null))
					{
						locationName = home.Location;
						tile = home.Tile;
						int parsedDirection;
						direction = (Utility.TryParseDirection(home.Direction, out parsedDirection) ? parsedDirection : 0);
						return true;
					}
				}
			}
			locationName = "Town";
			tile = new Point(29, 67);
			direction = 2;
			return false;
		}

		// Token: 0x060011CD RID: 4557 RVA: 0x000CD014 File Offset: 0x000CB214
		public virtual bool canTalk()
		{
			return true;
		}

		// Token: 0x060011CE RID: 4558 RVA: 0x000CD018 File Offset: 0x000CB218
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.birthday_Season, "birthday_Season").AddField(this.birthday_Day, "birthday_Day").AddField(this.datable, "datable").AddField(this.shouldPlayRobinHammerAnimation, "shouldPlayRobinHammerAnimation").AddField(this.shouldPlaySpousePatioAnimation, "shouldPlaySpousePatioAnimation").AddField(this.isWalkingTowardPlayer, "isWalkingTowardPlayer").AddField(this.moveTowardPlayerThreshold, "moveTowardPlayerThreshold").AddField(this.age, "age").AddField(this.manners, "manners").AddField(this.socialAnxiety, "socialAnxiety").AddField(this.optimism, "optimism").AddField(this.gender, "gender").AddField(this.breather, "breather").AddField(this.isSleeping, "isSleeping").AddField(this.hideShadow, "hideShadow").AddField(this.isInvisible, "isInvisible").AddField(this.defaultMap, "defaultMap").AddField(this.defaultPosition, "defaultPosition").AddField(this.removeHenchmanEvent, "removeHenchmanEvent").AddField(this.doingEndOfRouteAnimation, "doingEndOfRouteAnimation").AddField(this.goingToDoEndOfRouteAnimation, "goingToDoEndOfRouteAnimation").AddField(this.endOfRouteMessage, "endOfRouteMessage").AddField(this.endOfRouteBehaviorName, "endOfRouteBehaviorName").AddField(this.lastSeenMovieWeek, "lastSeenMovieWeek").AddField(this.currentMarriageDialogue, "currentMarriageDialogue").AddField(this.marriageDefaultDialogue, "marriageDefaultDialogue").AddField(this.shouldSayMarriageDialogue, "shouldSayMarriageDialogue").AddField(this.hasBeenKissedToday, "hasBeenKissedToday").AddField(this.hasSaidAfternoonDialogue, "hasSaidAfternoonDialogue").AddField(this.dayScheduleName, "dayScheduleName").AddField(this.islandScheduleName, "islandScheduleName").AddField(this.sleptInBed, "sleptInBed").AddField(this.shouldWearIslandAttire, "shouldWearIslandAttire").AddField(this.isMovingOnPathFindPath, "isMovingOnPathFindPath");
			this.position.Field.AxisAlignedMovement = true;
			this.removeHenchmanEvent.onEvent += this.performRemoveHenchman;
		}

		// Token: 0x060011CF RID: 4559 RVA: 0x000CD27C File Offset: 0x000CB47C
		public virtual void ChooseAppearance(LocalizedContentManager content = null)
		{
			this.LastAppearanceId = null;
			if (base.SimpleNonVillagerNPC)
			{
				return;
			}
			content = (content ?? Game1.content);
			GameLocation location = base.currentLocation;
			if (location == null)
			{
				return;
			}
			this.LastLocationNameForAppearance = location.NameOrUniqueName;
			bool appliedLegacyUniquePortraits = false;
			string uniquePortraitsProperty;
			if (location.TryGetMapProperty("UniquePortrait", out uniquePortraitsProperty) && ArgUtility.SplitBySpace(uniquePortraitsProperty).Contains(base.Name))
			{
				string assetName = "Portraits\\" + this.getTextureName() + "_" + location.Name;
				string errorPhrase;
				appliedLegacyUniquePortraits = this.TryLoadPortraits(assetName, out errorPhrase, content);
				if (!appliedLegacyUniquePortraits)
				{
					IGameLogger log = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(101, 5);
					defaultInterpolatedStringHandler.AppendLiteral("NPC ");
					defaultInterpolatedStringHandler.AppendFormatted(base.Name);
					defaultInterpolatedStringHandler.AppendLiteral(" can't load portraits from '");
					defaultInterpolatedStringHandler.AppendFormatted(assetName);
					defaultInterpolatedStringHandler.AppendLiteral("' (per the ");
					defaultInterpolatedStringHandler.AppendFormatted("UniquePortrait");
					defaultInterpolatedStringHandler.AppendLiteral(" map property in '");
					defaultInterpolatedStringHandler.AppendFormatted(location.NameOrUniqueName);
					defaultInterpolatedStringHandler.AppendLiteral("'): ");
					defaultInterpolatedStringHandler.AppendFormatted(errorPhrase);
					defaultInterpolatedStringHandler.AppendLiteral(". Falling back to default portraits.");
					log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
				}
			}
			bool appliedLegacyUniqueSprites = false;
			string uniqueSpritesProperty;
			if (location.TryGetMapProperty("UniqueSprite", out uniqueSpritesProperty) && ArgUtility.SplitBySpace(uniqueSpritesProperty).Contains(base.Name))
			{
				string assetName2 = "Characters\\" + this.getTextureName() + "_" + location.Name;
				string errorPhrase2;
				appliedLegacyUniqueSprites = this.TryLoadSprites(assetName2, out errorPhrase2, content);
				if (!appliedLegacyUniqueSprites)
				{
					IGameLogger log2 = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(97, 5);
					defaultInterpolatedStringHandler.AppendLiteral("NPC ");
					defaultInterpolatedStringHandler.AppendFormatted(base.Name);
					defaultInterpolatedStringHandler.AppendLiteral(" can't load sprites from '");
					defaultInterpolatedStringHandler.AppendFormatted(assetName2);
					defaultInterpolatedStringHandler.AppendLiteral("' (per the ");
					defaultInterpolatedStringHandler.AppendFormatted("UniqueSprite");
					defaultInterpolatedStringHandler.AppendLiteral(" map property in '");
					defaultInterpolatedStringHandler.AppendFormatted(location.NameOrUniqueName);
					defaultInterpolatedStringHandler.AppendLiteral("'): ");
					defaultInterpolatedStringHandler.AppendFormatted(errorPhrase2);
					defaultInterpolatedStringHandler.AppendLiteral(". Falling back to default sprites.");
					log2.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
				}
			}
			if (appliedLegacyUniquePortraits && appliedLegacyUniqueSprites)
			{
				return;
			}
			CharacterData data = null;
			CharacterAppearanceData appearance = null;
			if (!this.IsMonster)
			{
				data = this.GetData();
				if (data != null)
				{
					List<CharacterAppearanceData> appearance2 = data.Appearance;
					int? num = (appearance2 != null) ? new int?(appearance2.Count) : null;
					int num2 = 0;
					if (num.GetValueOrDefault() > num2 & num != null)
					{
						List<CharacterAppearanceData> possibleOptions = new List<CharacterAppearanceData>();
						int totalWeight = 0;
						Random random = Utility.CreateDaySaveRandom((double)Game1.hash.GetDeterministicHashCode(base.Name), 0.0, 0.0);
						Season season = location.GetSeason();
						bool isOutdoors = location.IsOutdoors;
						int precedence = int.MaxValue;
						foreach (CharacterAppearanceData option in data.Appearance)
						{
							if (option.Precedence <= precedence)
							{
								if (option.IsIslandAttire != this.isWearingIslandAttire)
								{
									goto IL_34D;
								}
								Season? season2 = option.Season;
								if ((season2 != null && option.Season.Value != season) || !(isOutdoors ? option.Outdoors : option.Indoors))
								{
									goto IL_34D;
								}
								bool flag = GameStateQuery.CheckConditions(option.Condition, location, null, null, null, random, null);
								IL_34E:
								if (flag)
								{
									if (option.Precedence < precedence)
									{
										precedence = option.Precedence;
										possibleOptions.Clear();
										totalWeight = 0;
									}
									possibleOptions.Add(option);
									totalWeight += option.Weight;
									continue;
								}
								continue;
								IL_34D:
								flag = false;
								goto IL_34E;
							}
						}
						num2 = possibleOptions.Count;
						if (num2 != 0)
						{
							if (num2 == 1)
							{
								appearance = possibleOptions[0];
							}
							else
							{
								appearance = possibleOptions[possibleOptions.Count - 1];
								int cursor = Utility.CreateDaySaveRandom((double)Game1.hash.GetDeterministicHashCode(base.Name), 0.0, 0.0).Next(totalWeight + 1);
								foreach (CharacterAppearanceData option2 in possibleOptions)
								{
									cursor -= option2.Weight;
									if (cursor <= 0)
									{
										appearance = option2;
										break;
									}
								}
							}
						}
					}
				}
			}
			if (!appliedLegacyUniquePortraits)
			{
				string defaultAsset = "Portraits/" + this.getTextureName();
				bool loaded = false;
				if (appearance != null && appearance.Portrait != null && appearance.Portrait != defaultAsset)
				{
					string errorPhrase3;
					loaded = this.TryLoadPortraits(appearance.Portrait, out errorPhrase3, content);
					if (!loaded)
					{
						IGameLogger log3 = Game1.log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(116, 4);
						defaultInterpolatedStringHandler.AppendLiteral("NPC ");
						defaultInterpolatedStringHandler.AppendFormatted(base.Name);
						defaultInterpolatedStringHandler.AppendLiteral(" can't load portraits from '");
						defaultInterpolatedStringHandler.AppendFormatted(appearance.Portrait);
						defaultInterpolatedStringHandler.AppendLiteral("' (per appearance entry '");
						defaultInterpolatedStringHandler.AppendFormatted(appearance.Id);
						defaultInterpolatedStringHandler.AppendLiteral("' in Data/Characters): ");
						defaultInterpolatedStringHandler.AppendFormatted(errorPhrase3);
						defaultInterpolatedStringHandler.AppendLiteral(". Falling back to default portraits.");
						log3.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
					}
				}
				if (!loaded && this.isWearingIslandAttire)
				{
					string beachAsset = defaultAsset + "_Beach";
					if (content.DoesAssetExist<Texture2D>(beachAsset))
					{
						string errorPhrase4;
						loaded = this.TryLoadPortraits(beachAsset, out errorPhrase4, content);
						if (!loaded)
						{
							IGameLogger log4 = Game1.log;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(89, 3);
							defaultInterpolatedStringHandler.AppendLiteral("NPC ");
							defaultInterpolatedStringHandler.AppendFormatted(base.Name);
							defaultInterpolatedStringHandler.AppendLiteral(" can't load portraits from '");
							defaultInterpolatedStringHandler.AppendFormatted(beachAsset);
							defaultInterpolatedStringHandler.AppendLiteral("' for island attire: ");
							defaultInterpolatedStringHandler.AppendFormatted(errorPhrase4);
							defaultInterpolatedStringHandler.AppendLiteral(". Falling back to default portraits.");
							log4.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
						}
					}
				}
				string errorPhrase5;
				if (!loaded && !this.TryLoadPortraits(defaultAsset, out errorPhrase5, content))
				{
					IGameLogger log5 = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(36, 3);
					defaultInterpolatedStringHandler.AppendLiteral("NPC ");
					defaultInterpolatedStringHandler.AppendFormatted(base.Name);
					defaultInterpolatedStringHandler.AppendLiteral(" can't load portraits from '");
					defaultInterpolatedStringHandler.AppendFormatted(defaultAsset);
					defaultInterpolatedStringHandler.AppendLiteral("': ");
					defaultInterpolatedStringHandler.AppendFormatted(errorPhrase5);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					log5.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				if (loaded)
				{
					this.LastAppearanceId = ((appearance != null) ? appearance.Id : null);
				}
			}
			if (!appliedLegacyUniqueSprites)
			{
				string defaultAsset2 = "Characters/" + this.getTextureName();
				bool loaded2 = false;
				if (appearance != null && appearance.Sprite != null && appearance.Sprite != defaultAsset2)
				{
					string errorPhrase6;
					loaded2 = this.TryLoadSprites(appearance.Sprite, out errorPhrase6, content);
					if (!loaded2)
					{
						IGameLogger log6 = Game1.log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(112, 4);
						defaultInterpolatedStringHandler.AppendLiteral("NPC ");
						defaultInterpolatedStringHandler.AppendFormatted(base.Name);
						defaultInterpolatedStringHandler.AppendLiteral(" can't load sprites from '");
						defaultInterpolatedStringHandler.AppendFormatted(appearance.Sprite);
						defaultInterpolatedStringHandler.AppendLiteral("' (per appearance entry '");
						defaultInterpolatedStringHandler.AppendFormatted(appearance.Id);
						defaultInterpolatedStringHandler.AppendLiteral("' in Data/Characters): ");
						defaultInterpolatedStringHandler.AppendFormatted(errorPhrase6);
						defaultInterpolatedStringHandler.AppendLiteral(". Falling back to default sprites.");
						log6.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
					}
				}
				if (!loaded2 && this.isWearingIslandAttire)
				{
					string beachAsset2 = defaultAsset2 + "_Beach";
					if (content.DoesAssetExist<Texture2D>(beachAsset2))
					{
						string errorPhrase7;
						loaded2 = this.TryLoadSprites(beachAsset2, out errorPhrase7, content);
						if (!loaded2)
						{
							IGameLogger log7 = Game1.log;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(85, 3);
							defaultInterpolatedStringHandler.AppendLiteral("NPC ");
							defaultInterpolatedStringHandler.AppendFormatted(base.Name);
							defaultInterpolatedStringHandler.AppendLiteral(" can't load sprites from '");
							defaultInterpolatedStringHandler.AppendFormatted(beachAsset2);
							defaultInterpolatedStringHandler.AppendLiteral("' for island attire: ");
							defaultInterpolatedStringHandler.AppendFormatted(errorPhrase7);
							defaultInterpolatedStringHandler.AppendLiteral(". Falling back to default sprites.");
							log7.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
						}
					}
				}
				string errorPhrase8;
				if (!loaded2 && !this.TryLoadSprites(defaultAsset2, out errorPhrase8, content))
				{
					IGameLogger log8 = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 3);
					defaultInterpolatedStringHandler.AppendLiteral("NPC ");
					defaultInterpolatedStringHandler.AppendFormatted(base.Name);
					defaultInterpolatedStringHandler.AppendLiteral(" can't load sprites from '");
					defaultInterpolatedStringHandler.AppendFormatted(defaultAsset2);
					defaultInterpolatedStringHandler.AppendLiteral("': ");
					defaultInterpolatedStringHandler.AppendFormatted(errorPhrase8);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					log8.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				if (loaded2)
				{
					this.LastAppearanceId = ((appearance != null) ? appearance.Id : null);
				}
			}
			if (data != null && this.Sprite != null)
			{
				this.Sprite.SpriteWidth = data.Size.X;
				this.Sprite.SpriteHeight = data.Size.Y;
				this.Sprite.ignoreSourceRectUpdates = false;
			}
		}

		// Token: 0x060011D0 RID: 4560 RVA: 0x000CDB6C File Offset: 0x000CBD6C
		protected override string translateName()
		{
			return NPC.GetDisplayName(this.name.Value);
		}

		// Token: 0x060011D1 RID: 4561 RVA: 0x000CDB7E File Offset: 0x000CBD7E
		public string getName()
		{
			if (this.displayName != null && this.displayName.Length > 0)
			{
				return this.displayName;
			}
			return base.Name;
		}

		// Token: 0x060011D2 RID: 4562 RVA: 0x000CDBA3 File Offset: 0x000CBDA3
		public virtual string getTextureName()
		{
			return NPC.getTextureNameForCharacter(base.Name);
		}

		// Token: 0x060011D3 RID: 4563 RVA: 0x000CDBB0 File Offset: 0x000CBDB0
		public static string getTextureNameForCharacter(string character_name)
		{
			CharacterData data;
			NPC.TryGetData(character_name, out data);
			string textureName = (data != null) ? data.TextureName : null;
			if (string.IsNullOrEmpty(textureName))
			{
				return character_name;
			}
			return textureName;
		}

		// Token: 0x060011D4 RID: 4564 RVA: 0x000CDBDE File Offset: 0x000CBDDE
		public void resetSeasonalDialogue()
		{
			this.dialogue = null;
		}

		// Token: 0x060011D5 RID: 4565 RVA: 0x000CDBE8 File Offset: 0x000CBDE8
		public void performSpecialScheduleChanges()
		{
			if (this.Schedule == null)
			{
				return;
			}
			if (base.Name.Equals("Pam") && Game1.MasterPlayer.mailReceived.Contains("ccVault"))
			{
				bool foundBus = false;
				foreach (KeyValuePair<int, SchedulePathDescription> v in this.Schedule)
				{
					bool pamGone = false;
					string targetLocationName = v.Value.targetLocationName;
					if (!(targetLocationName == "BusStop"))
					{
						if (targetLocationName == "DesertFestival" || targetLocationName == "Desert" || targetLocationName == "IslandSouth")
						{
							GameLocation gameLocation = Game1.RequireLocation<BusStop>("BusStop", false);
							Game1.netWorldState.Value.canDriveYourselfToday.Value = true;
							Object sign = ItemRegistry.Create<Object>("(BC)TextSign", 1, 0, false);
							sign.signText.Value = TokenStringBuilder.LocalizedText((v.Value.targetLocationName == "IslandSouth") ? "Strings\\1_6_Strings:Pam_busSign_resort" : "Strings\\1_6_Strings:Pam_busSign");
							sign.SpecialVariable = 987659;
							gameLocation.tryPlaceObject(new Vector2(25f, 10f), sign);
							foundBus = true;
							pamGone = true;
						}
					}
					else
					{
						foundBus = true;
					}
					if (pamGone)
					{
						break;
					}
				}
				if (!foundBus && !Game1.isGreenRain)
				{
					GameLocation gameLocation2 = Game1.getLocationFromName("BusStop") as BusStop;
					Game1.netWorldState.Value.canDriveYourselfToday.Value = true;
					Object sign2 = (Object)ItemRegistry.Create("(BC)TextSign", 1, 0, false);
					sign2.signText.Value = TokenStringBuilder.LocalizedText("Strings\\1_6_Strings:Pam_busSign_generic");
					sign2.SpecialVariable = 987659;
					gameLocation2.tryPlaceObject(new Vector2(25f, 10f), sign2);
				}
			}
		}

		// Token: 0x060011D6 RID: 4566 RVA: 0x000CDDD4 File Offset: 0x000CBFD4
		public virtual void reloadSprite(bool onlyAppearance = false)
		{
			if (base.SimpleNonVillagerNPC)
			{
				return;
			}
			this.ChooseAppearance(null);
			if (onlyAppearance || (!Game1.newDay && Game1.gameMode != 6))
			{
				return;
			}
			this.faceDirection(this.DefaultFacingDirection);
			this.previousEndPoint = new Point((int)this.defaultPosition.X / 64, (int)this.defaultPosition.Y / 64);
			this.TryLoadSchedule();
			this.performSpecialScheduleChanges();
			this.resetSeasonalDialogue();
			this.resetCurrentDialogue();
			this.updateConstructionAnimation();
			try
			{
				this.displayName = this.translateName();
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x060011D7 RID: 4567 RVA: 0x000CDE7C File Offset: 0x000CC07C
		public bool TryLoadPortraits(string assetName, out string error, LocalizedContentManager content = null)
		{
			if (base.Name == "Raccoon" || base.Name == "MrsRaccoon")
			{
				error = null;
				return true;
			}
			if (this.portraitOverridden)
			{
				error = null;
				return true;
			}
			if (string.IsNullOrWhiteSpace(assetName))
			{
				error = "the asset name is empty";
				return false;
			}
			Texture2D texture2D = this.portrait;
			if (((texture2D != null) ? texture2D.Name : null) == assetName && !this.portrait.IsDisposed)
			{
				error = null;
				return true;
			}
			if (content == null)
			{
				content = Game1.content;
			}
			bool result;
			try
			{
				this.portrait = content.Load<Texture2D>(assetName);
				this.portrait.Name = assetName;
				error = null;
				result = true;
			}
			catch (Exception ex)
			{
				error = ex.ToString();
				result = false;
			}
			return result;
		}

		// Token: 0x060011D8 RID: 4568 RVA: 0x000CDF48 File Offset: 0x000CC148
		public bool TryLoadSprites(string assetName, out string error, LocalizedContentManager content = null)
		{
			if (this.spriteOverridden)
			{
				error = null;
				return true;
			}
			if (string.IsNullOrWhiteSpace(assetName))
			{
				error = "the asset name is empty";
				return false;
			}
			AnimatedSprite sprite = this.Sprite;
			if (((sprite != null) ? sprite.spriteTexture : null) != null && ((this.Sprite.overrideTextureName ?? this.Sprite.textureName.Value) == assetName || this.Sprite.spriteTexture.Name == assetName) && !this.Sprite.spriteTexture.IsDisposed)
			{
				error = null;
				return true;
			}
			if (content == null)
			{
				content = Game1.content;
			}
			bool result;
			try
			{
				if (this.Sprite == null)
				{
					this.Sprite = new AnimatedSprite(content, assetName);
				}
				else
				{
					this.Sprite.LoadTexture(assetName, Game1.IsMasterGame);
				}
				error = null;
				result = true;
			}
			catch (Exception ex)
			{
				error = ex.ToString();
				result = false;
			}
			return result;
		}

		// Token: 0x060011D9 RID: 4569 RVA: 0x000CE038 File Offset: 0x000CC238
		private void updateConstructionAnimation()
		{
			bool isFestivalDay = Utility.isFestivalDay();
			if (Game1.IsMasterGame && base.Name == "Robin" && !isFestivalDay && (!Game1.isGreenRain || Game1.year > 1))
			{
				if (Game1.player.daysUntilHouseUpgrade.Value > 0)
				{
					Farm farm = Game1.getFarm();
					Game1.warpCharacter(this, farm.NameOrUniqueName, new Vector2((float)(farm.GetMainFarmHouseEntry().X + 4), (float)(farm.GetMainFarmHouseEntry().Y - 1)));
					this.isPlayingRobinHammerAnimation = false;
					this.shouldPlayRobinHammerAnimation.Value = true;
					return;
				}
				if (Game1.IsThereABuildingUnderConstruction("Robin"))
				{
					Building b = Game1.GetBuildingUnderConstruction("Robin");
					if (b == null)
					{
						return;
					}
					GameLocation indoors = b.GetIndoors();
					if (b.daysUntilUpgrade.Value > 0 && indoors != null)
					{
						GameLocation currentLocation = base.currentLocation;
						if (currentLocation != null)
						{
							currentLocation.characters.Remove(this);
						}
						base.currentLocation = indoors;
						if (base.currentLocation != null && !base.currentLocation.characters.Contains(this))
						{
							base.currentLocation.addCharacter(this);
						}
						string indoorsName = b.GetIndoorsName();
						if (indoorsName != null && indoorsName.StartsWith("Shed"))
						{
							this.setTilePosition(2, 2);
							this.position.X -= 28f;
						}
						else
						{
							this.setTilePosition(1, 5);
						}
					}
					else
					{
						Game1.warpCharacter(this, b.parentLocationName.Value, new Vector2((float)(b.tileX.Value + b.tilesWide.Value / 2), (float)(b.tileY.Value + b.tilesHigh.Value / 2)));
						this.position.X += 16f;
						this.position.Y -= 32f;
					}
					this.isPlayingRobinHammerAnimation = false;
					this.shouldPlayRobinHammerAnimation.Value = true;
					return;
				}
				else if (Game1.RequireLocation<Town>("Town", false).daysUntilCommunityUpgrade.Value > 0)
				{
					if (Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
					{
						Game1.warpCharacter(this, "Backwoods", new Vector2(41f, 23f));
						this.isPlayingRobinHammerAnimation = false;
						this.shouldPlayRobinHammerAnimation.Value = true;
						return;
					}
					if (Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
					{
						Game1.warpCharacter(this, "Town", new Vector2(77f, 68f));
						this.isPlayingRobinHammerAnimation = false;
						this.shouldPlayRobinHammerAnimation.Value = true;
					}
					return;
				}
			}
			this.shouldPlayRobinHammerAnimation.Value = false;
		}

		// Token: 0x060011DA RID: 4570 RVA: 0x000CE2E0 File Offset: 0x000CC4E0
		private void doPlayRobinHammerAnimation()
		{
			this.Sprite.ClearAnimation();
			this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(24, 75));
			this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(25, 75));
			this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(26, 300, false, false, new AnimatedSprite.endOfAnimationBehavior(this.robinHammerSound), false));
			this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(27, 1000, false, false, new AnimatedSprite.endOfAnimationBehavior(this.robinVariablePause), false));
			this.ignoreScheduleToday = true;
			bool oneDayLeft = Game1.player.daysUntilHouseUpgrade.Value == 1 || Game1.RequireLocation<Town>("Town", false).daysUntilCommunityUpgrade.Value == 1;
			this.CurrentDialogue.Clear();
			this.CurrentDialogue.Push(new Dialogue(this, oneDayLeft ? "Strings\\StringsFromCSFiles:NPC.cs.3927" : "Strings\\StringsFromCSFiles:NPC.cs.3926", false));
		}

		// Token: 0x060011DB RID: 4571 RVA: 0x000CE3D0 File Offset: 0x000CC5D0
		public void showTextAboveHead(string text, Color? spriteTextColor = null, int style = 2, int duration = 3000, int preTimer = 0)
		{
			if (this.IsInvisible)
			{
				return;
			}
			this.textAboveHeadAlpha = 0f;
			this.textAboveHead = StardewValley.Dialogue.applyGenderSwitchBlocks(Game1.player.Gender, text);
			this.textAboveHeadPreTimer = preTimer;
			this.textAboveHeadTimer = duration;
			this.textAboveHeadStyle = style;
			this.textAboveHeadColor = spriteTextColor;
		}

		// Token: 0x060011DC RID: 4572 RVA: 0x000CE425 File Offset: 0x000CC625
		public virtual bool hitWithTool(Tool t)
		{
			return false;
		}

		// Token: 0x060011DD RID: 4573 RVA: 0x000CE428 File Offset: 0x000CC628
		public bool CanReceiveGifts()
		{
			if (this.CanSocialize && !base.SimpleNonVillagerNPC && Game1.NPCGiftTastes.ContainsKey(base.Name))
			{
				CharacterData data = this.GetData();
				return data == null || data.CanReceiveGifts;
			}
			return false;
		}

		// Token: 0x060011DE RID: 4574 RVA: 0x000CE460 File Offset: 0x000CC660
		public int getGiftTasteForThisItem(Item item)
		{
			if (item.QualifiedItemId == "(O)StardropTea")
			{
				return 7;
			}
			int tasteForItem = 8;
			Object obj = item as Object;
			if (obj != null)
			{
				int categoryNumber = obj.Category;
				string categoryNumberString = categoryNumber.ToString() ?? "";
				string[] universalLoves = ArgUtility.SplitBySpace(Game1.NPCGiftTastes["Universal_Love"]);
				string[] universalHates = ArgUtility.SplitBySpace(Game1.NPCGiftTastes["Universal_Hate"]);
				string[] universalLikes = ArgUtility.SplitBySpace(Game1.NPCGiftTastes["Universal_Like"]);
				string[] universalDislikes = ArgUtility.SplitBySpace(Game1.NPCGiftTastes["Universal_Dislike"]);
				string[] universalNeutrals = ArgUtility.SplitBySpace(Game1.NPCGiftTastes["Universal_Neutral"]);
				if (universalLoves.Contains(categoryNumberString))
				{
					tasteForItem = 0;
				}
				else if (universalHates.Contains(categoryNumberString))
				{
					tasteForItem = 6;
				}
				else if (universalLikes.Contains(categoryNumberString))
				{
					tasteForItem = 2;
				}
				else if (universalDislikes.Contains(categoryNumberString))
				{
					tasteForItem = 4;
				}
				if (this.CheckTasteContextTags(obj, universalLoves))
				{
					tasteForItem = 0;
				}
				else if (this.CheckTasteContextTags(obj, universalHates))
				{
					tasteForItem = 6;
				}
				else if (this.CheckTasteContextTags(obj, universalLikes))
				{
					tasteForItem = 2;
				}
				else if (this.CheckTasteContextTags(obj, universalDislikes))
				{
					tasteForItem = 4;
				}
				bool wasIndividualUniversal = false;
				bool skipDefaultValueRules = false;
				if (this.CheckTaste(universalLoves, obj))
				{
					tasteForItem = 0;
					wasIndividualUniversal = true;
				}
				else if (this.CheckTaste(universalHates, obj))
				{
					tasteForItem = 6;
					wasIndividualUniversal = true;
				}
				else if (this.CheckTaste(universalLikes, obj))
				{
					tasteForItem = 2;
					wasIndividualUniversal = true;
				}
				else if (this.CheckTaste(universalDislikes, obj))
				{
					tasteForItem = 4;
					wasIndividualUniversal = true;
				}
				else if (this.CheckTaste(universalNeutrals, obj))
				{
					tasteForItem = 8;
					wasIndividualUniversal = true;
					skipDefaultValueRules = true;
				}
				if (obj.Type == "Arch")
				{
					tasteForItem = 4;
					if (base.Name.Equals("Penny") || this.name.Equals("Dwarf"))
					{
						tasteForItem = 2;
					}
				}
				if (tasteForItem == 8 && !skipDefaultValueRules)
				{
					if (obj.edibility.Value != -300 && obj.edibility.Value < 0)
					{
						tasteForItem = 6;
					}
					else if (obj.price.Value < 20)
					{
						tasteForItem = 4;
					}
				}
				string dispositionData;
				if (Game1.NPCGiftTastes.TryGetValue(base.Name, out dispositionData))
				{
					string[] split = dispositionData.Split('/', StringSplitOptions.None);
					List<string[]> items = new List<string[]>();
					for (int i = 0; i < 10; i += 2)
					{
						string[] splitItems = ArgUtility.SplitBySpace(split[i + 1]);
						string[] thisItems = new string[splitItems.Length];
						for (int j = 0; j < splitItems.Length; j++)
						{
							if (splitItems[j].Length > 0)
							{
								thisItems[j] = splitItems[j];
							}
						}
						items.Add(thisItems);
					}
					if (this.CheckTaste(items[0], obj))
					{
						return 0;
					}
					if (this.CheckTaste(items[3], obj))
					{
						return 6;
					}
					if (this.CheckTaste(items[1], obj))
					{
						return 2;
					}
					if (this.CheckTaste(items[2], obj))
					{
						return 4;
					}
					if (this.CheckTaste(items[4], obj))
					{
						return 8;
					}
					if (this.CheckTasteContextTags(obj, items[0]))
					{
						return 0;
					}
					if (this.CheckTasteContextTags(obj, items[3]))
					{
						return 6;
					}
					if (this.CheckTasteContextTags(obj, items[1]))
					{
						return 2;
					}
					if (this.CheckTasteContextTags(obj, items[2]))
					{
						return 4;
					}
					if (this.CheckTasteContextTags(obj, items[4]))
					{
						return 8;
					}
					if (!wasIndividualUniversal)
					{
						if (categoryNumber != 0 && items[0].Contains(categoryNumberString))
						{
							return 0;
						}
						if (categoryNumber != 0 && items[3].Contains(categoryNumberString))
						{
							return 6;
						}
						if (categoryNumber != 0 && items[1].Contains(categoryNumberString))
						{
							return 2;
						}
						if (categoryNumber != 0 && items[2].Contains(categoryNumberString))
						{
							return 4;
						}
						if (categoryNumber != 0 && items[4].Contains(categoryNumberString))
						{
							return 8;
						}
					}
				}
			}
			return tasteForItem;
		}

		// Token: 0x060011DF RID: 4575 RVA: 0x000CE814 File Offset: 0x000CCA14
		public bool CheckTaste(IEnumerable<string> list, Item item)
		{
			foreach (string item_entry in list)
			{
				if (item_entry != null && !item_entry.StartsWith('-'))
				{
					ParsedItemData data = ItemRegistry.GetData(item_entry);
					if (((data != null) ? data.ItemType : null) != null && item.QualifiedItemId == data.QualifiedItemId)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060011E0 RID: 4576 RVA: 0x000CE894 File Offset: 0x000CCA94
		public virtual bool CheckTasteContextTags(Item item, string[] list)
		{
			foreach (string entry in list)
			{
				if (entry != null && entry.Length > 0 && !char.IsNumber(entry[0]) && entry[0] != '-' && item.HasContextTag(entry))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060011E1 RID: 4577 RVA: 0x000CE8E8 File Offset: 0x000CCAE8
		private void goblinDoorEndBehavior(Character c, GameLocation l)
		{
			l.characters.Remove(this);
			l.playSound("doorClose", null, null, SoundContext.Default);
		}

		// Token: 0x060011E2 RID: 4578 RVA: 0x000CE920 File Offset: 0x000CCB20
		private void performRemoveHenchman()
		{
			this.Sprite.CurrentFrame = 4;
			Game1.netWorldState.Value.IsGoblinRemoved = true;
			Game1.player.removeQuest("27");
			Stack<Point> p = new Stack<Point>();
			p.Push(new Point(20, 21));
			p.Push(new Point(20, 22));
			p.Push(new Point(20, 23));
			p.Push(new Point(20, 24));
			p.Push(new Point(20, 25));
			p.Push(new Point(20, 26));
			p.Push(new Point(20, 27));
			p.Push(new Point(20, 28));
			this.addedSpeed = 2f;
			this.controller = new PathFindController(p, this, base.currentLocation);
			this.controller.endBehaviorFunction = new PathFindController.endBehavior(this.goblinDoorEndBehavior);
			this.showTextAboveHead(Game1.content.LoadString("Strings\\Characters:Henchman6"), null, 2, 3000, 0);
			Game1.player.mailReceived.Add("henchmanGone");
			base.currentLocation.removeTile(20, 29, "Buildings");
		}

		// Token: 0x060011E3 RID: 4579 RVA: 0x000CEA5C File Offset: 0x000CCC5C
		private void engagementResponse(Farmer who, bool asRoommate = false)
		{
			Game1.changeMusicTrack("silence", false, MusicContext.Default);
			who.spouse = base.Name;
			if (!asRoommate)
			{
				Game1.multiplayer.globalChatInfoMessage("Engaged", new string[]
				{
					Game1.player.Name,
					this.GetTokenizedDisplayName()
				});
			}
			Friendship friendship = who.friendshipData[base.Name];
			friendship.Status = FriendshipStatus.Engaged;
			friendship.RoommateMarriage = asRoommate;
			WorldDate weddingDate = new WorldDate(Game1.Date);
			weddingDate.TotalDays += 3;
			who.removeDatingActiveDialogueEvents(Game1.player.spouse);
			while (!Game1.canHaveWeddingOnDay(weddingDate.DayOfMonth, weddingDate.Season))
			{
				weddingDate.TotalDays++;
			}
			friendship.WeddingDate = weddingDate;
			this.CurrentDialogue.Clear();
			if (asRoommate && DataLoader.EngagementDialogue(Game1.content).ContainsKey(base.Name + "Roommate0"))
			{
				this.CurrentDialogue.Push(new Dialogue(this, "Data\\EngagementDialogue:" + base.Name + "Roommate0", false));
				Dialogue attemptDialogue = StardewValley.Dialogue.TryGetDialogue(this, "Strings\\StringsFromCSFiles:" + base.Name + "_EngagedRoommate");
				if (attemptDialogue != null)
				{
					this.CurrentDialogue.Push(attemptDialogue);
				}
				else
				{
					attemptDialogue = StardewValley.Dialogue.TryGetDialogue(this, "Strings\\StringsFromCSFiles:" + base.Name + "_Engaged");
					if (attemptDialogue != null)
					{
						this.CurrentDialogue.Push(attemptDialogue);
					}
					else
					{
						this.CurrentDialogue.Push(new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3980", false));
					}
				}
			}
			else
			{
				Dialogue attemptDialogue2 = StardewValley.Dialogue.TryGetDialogue(this, "Data\\EngagementDialogue:" + base.Name + "0");
				if (attemptDialogue2 != null)
				{
					this.CurrentDialogue.Push(attemptDialogue2);
				}
				attemptDialogue2 = StardewValley.Dialogue.TryGetDialogue(this, "Strings\\StringsFromCSFiles:" + base.Name + "_Engaged");
				if (attemptDialogue2 != null)
				{
					this.CurrentDialogue.Push(attemptDialogue2);
				}
				else
				{
					this.CurrentDialogue.Push(new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3980", false));
				}
			}
			Dialogue dialogue = this.CurrentDialogue.Peek();
			dialogue.onFinish = (Action)Delegate.Combine(dialogue.onFinish, new Action(delegate()
			{
				Game1.changeMusicTrack("none", true, MusicContext.Default);
				GameLocation.HandleMusicChange(null, who.currentLocation);
			}));
			who.changeFriendship(1, this);
			who.reduceActiveItemByOne();
			who.completelyStopAnimatingOrDoingAction();
			Game1.drawDialogue(this);
		}

		// Token: 0x060011E4 RID: 4580 RVA: 0x000CECE0 File Offset: 0x000CCEE0
		public virtual bool tryToReceiveActiveObject(Farmer who, bool probe = false)
		{
			if (base.SimpleNonVillagerNPC)
			{
				return false;
			}
			Object activeObj = who.ActiveObject;
			if (activeObj == null)
			{
				return false;
			}
			if (!probe)
			{
				who.Halt();
				who.faceGeneralDirection(base.getStandingPosition(), 0, false, false);
			}
			if (base.Name == "Henchman" && Game1.currentLocation.NameOrUniqueName == "WitchSwamp")
			{
				if (activeObj.QualifiedItemId == "(O)308")
				{
					if (this.controller != null)
					{
						return false;
					}
					if (!probe)
					{
						who.currentLocation.localSound("coin", null, null, SoundContext.Default);
						who.reduceActiveItemByOne();
						this.CurrentDialogue.Push(new Dialogue(this, "Strings\\Characters:Henchman5", false));
						Game1.drawDialogue(this);
						who.freezePause = 2000;
						this.removeHenchmanEvent.Fire();
					}
				}
				else if (!probe)
				{
					this.CurrentDialogue.Push(new Dialogue(this, (activeObj.QualifiedItemId == "(O)684") ? "Strings\\Characters:Henchman4" : "Strings\\Characters:Henchman3", false));
					Game1.drawDialogue(this);
				}
				return true;
			}
			if (Game1.player.team.specialOrders != null)
			{
				foreach (SpecialOrder order in Game1.player.team.specialOrders)
				{
					if (order.onItemDelivered != null)
					{
						Delegate[] invocationList = order.onItemDelivered.GetInvocationList();
						for (int i = 0; i < invocationList.Length; i++)
						{
							if (((Func<Farmer, NPC, Item, bool, int>)invocationList[i])(Game1.player, this, activeObj, probe) > 0)
							{
								if (!probe && activeObj.Stack <= 0)
								{
									who.ActiveObject = null;
									who.showNotCarrying();
								}
								return true;
							}
						}
					}
				}
			}
			if (who.NotifyQuests((Quest quest) => quest.OnItemOfferedToNpc(this, activeObj, probe), true))
			{
				if (!probe)
				{
					who.completelyStopAnimatingOrDoingAction();
					if (Game1.random.NextDouble() < 0.3 && base.Name != "Wizard")
					{
						base.doEmote(32, true);
					}
				}
				return true;
			}
			Object activeObject = who.ActiveObject;
			string text = (activeObject != null) ? activeObject.QualifiedItemId : null;
			if (text != null)
			{
				int i = text.Length;
				if (i != 5)
				{
					if (i == 6)
					{
						switch (text[5])
						{
						case '0':
							if (!(text == "(O)870"))
							{
								goto IL_76A;
							}
							break;
						case '1':
						case '2':
							goto IL_76A;
						case '3':
							if (!(text == "(O)233"))
							{
								goto IL_76A;
							}
							if (this.name.Value == "Jas" && Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && base.currentLocation is Desert && !who.mailReceived.Contains("Jas_IceCream_DF_" + Game1.year.ToString()))
							{
								if (!probe)
								{
									who.reduceActiveItemByOne();
									this.jump();
									base.doEmote(16, true);
									this.CurrentDialogue.Clear();
									this.setNewDialogue("Strings\\1_6_Strings:Jas_IceCream", true, false);
									Game1.drawDialogue(this);
									who.mailReceived.Add("Jas_IceCream_DF_" + Game1.year.ToString());
									who.changeFriendship(200, this);
								}
								return true;
							}
							goto IL_76A;
						case '4':
							if (!(text == "(O)864"))
							{
								goto IL_76A;
							}
							break;
						case '5':
							if (!(text == "(O)865"))
							{
								goto IL_76A;
							}
							break;
						case '6':
							if (!(text == "(O)866"))
							{
								goto IL_76A;
							}
							break;
						case '7':
							if (text == "(O)897")
							{
								if (!probe)
								{
									if (base.Name == "Pierre" && !Game1.player.hasOrWillReceiveMail("PierreStocklist"))
									{
										Game1.addMail("PierreStocklist", true, true);
										who.reduceActiveItemByOne();
										who.completelyStopAnimatingOrDoingAction();
										who.currentLocation.localSound("give_gift", null, null, SoundContext.Default);
										Game1.player.team.itemsToRemoveOvernight.Add("897");
										this.setNewDialogue("Strings\\Characters:PierreStockListDialogue", true, false);
										Game1.drawDialogue(this);
										Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(delegate()
										{
											Game1.multiplayer.globalChatInfoMessage("StockList", Array.Empty<string>());
										}));
									}
									else
									{
										Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", this.displayName)));
									}
								}
								return true;
							}
							if (!(text == "(O)867"))
							{
								goto IL_76A;
							}
							break;
						case '8':
							if (!(text == "(O)868"))
							{
								goto IL_76A;
							}
							break;
						case '9':
							if (!(text == "(O)869"))
							{
								goto IL_76A;
							}
							break;
						default:
							goto IL_76A;
						}
						if (who.hasQuest("130"))
						{
							Dialogue dialogue = this.TryGetDialogue("accept_" + activeObj.ItemId);
							if (dialogue != null)
							{
								if (!probe)
								{
									this.setNewDialogue(dialogue, false, false);
									Game1.drawDialogue(this);
									this.CurrentDialogue.Peek().onFinish = delegate()
									{
										Object o = ItemRegistry.Create<Object>("(O)" + (activeObj.ParentSheetIndex + 1).ToString(), 1, 0, false);
										o.specialItem = true;
										o.questItem.Value = true;
										who.reduceActiveItemByOne();
										DelayedAction.playSoundAfterDelay("coin", 200, null, null, -1, false);
										DelayedAction.functionAfterDelay(delegate
										{
											who.addItemByMenuIfNecessary(o, null, false);
										}, 200);
										Game1.player.freezePause = 550;
										DelayedAction.functionAfterDelay(delegate
										{
											Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1919", o.DisplayName, Lexicon.getProperArticleForWord(o.DisplayName)));
										}, 550);
									};
									base.faceTowardFarmerForPeriod(6000, 4, false, who);
								}
								return true;
							}
							dialogue = this.TryGetDialogue("reject_" + activeObj.ItemId);
							if (dialogue != null)
							{
								if (!probe)
								{
									this.setNewDialogue(dialogue, false, false);
									Game1.drawDialogue(this);
								}
								return true;
							}
						}
						return false;
					}
				}
				else if (text == "(O)71")
				{
					if (base.Name == "Lewis" && who.hasQuest("102"))
					{
						if (!probe)
						{
							GameLocation currentLocation = who.currentLocation;
							if (((currentLocation != null) ? currentLocation.NameOrUniqueName : null) == "IslandSouth")
							{
								Game1.player.activeDialogueEvents["lucky_pants_lewis"] = 28;
							}
							who.completeQuest("102");
							string[] questFields = Quest.GetRawQuestFields("102");
							Dialogue thankYou = new Dialogue(this, null, ArgUtility.Get(questFields, 9, "Data\\ExtraDialogue:LostItemQuest_DefaultThankYou", false));
							this.setNewDialogue(thankYou, false, false);
							Game1.drawDialogue(this);
							Game1.player.changeFriendship(250, this);
							who.ActiveObject = null;
						}
						return true;
					}
					return false;
				}
			}
			IL_76A:
			if (activeObj.questItem.Value)
			{
				return false;
			}
			Dialogue dialogue6;
			if ((dialogue6 = this.TryGetDialogue("RejectItem_" + activeObj.QualifiedItemId)) == null)
			{
				dialogue6 = ((from tag in activeObj.GetContextTags()
				select this.TryGetDialogue("RejectItem_" + tag)).FirstOrDefault((Dialogue p) => p != null) ?? (activeObj.HasTypeObject() ? this.TryGetDialogue("reject_" + activeObj.ItemId) : null));
			}
			Dialogue dialogue2 = dialogue6;
			if (dialogue2 != null)
			{
				if (!probe)
				{
					this.setNewDialogue(dialogue2, false, false);
					Game1.drawDialogue(this);
				}
				return true;
			}
			Friendship friendship;
			who.friendshipData.TryGetValue(base.Name, out friendship);
			bool canReceiveGifts = this.CanReceiveGifts();
			text = activeObj.QualifiedItemId;
			if (!(text == "(O)809"))
			{
				if (!(text == "(O)458"))
				{
					if (!(text == "(O)277"))
					{
						if (!(text == "(O)460"))
						{
							if (canReceiveGifts && activeObj.HasContextTag(ItemContextTagManager.SanitizeContextTag("propose_roommate_" + base.Name)))
							{
								string failedKey = null;
								object[] substitutions = null;
								bool defaultToPuzzledMessage = base.Name != "Krobus";
								if (who.spouse == base.Name)
								{
									failedKey = "RejectRoommateProposal_AlreadyAccepted";
									defaultToPuzzledMessage = false;
								}
								else if (this.isMarriedOrEngaged())
								{
									failedKey = "RejectRoommateProposal_NpcWithSomeoneElse";
								}
								else if (who.isMarriedOrRoommates() || who.isEngaged())
								{
									failedKey = "RejectRoommateProposal_PlayerWithSomeoneElse";
									object[] array = new object[1];
									int num = 0;
									NPC spouse2 = who.getSpouse();
									array[num] = (((spouse2 != null) ? spouse2.displayName : null) ?? who.spouse);
									substitutions = array;
								}
								else if (who.getFriendshipHeartLevelForNPC(base.Name) < 10)
								{
									failedKey = "RejectRoommateProposal_LowFriendship";
								}
								else if (who.houseUpgradeLevel.Value < 1)
								{
									failedKey = "RejectRoommateProposal_SmallHouse";
								}
								if (failedKey != null)
								{
									Dialogue dialogue3 = ((substitutions != null) ? this.TryGetDialogue(failedKey, substitutions) : this.TryGetDialogue(failedKey)) ?? this.TryGetDialogue("RejectRoommateProposal");
									if (!probe)
									{
										if (dialogue3 != null)
										{
											this.CurrentDialogue.Push(dialogue3);
											Game1.drawDialogue(this);
										}
										else if (defaultToPuzzledMessage)
										{
											Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", this.displayName)));
										}
									}
									return dialogue3 != null || defaultToPuzzledMessage;
								}
								if (!probe)
								{
									this.engagementResponse(who, true);
								}
								return true;
							}
							else
							{
								bool obsoleteNotGiftable = ItemContextTagManager.HasBaseTag(activeObj.QualifiedItemId, "not_giftable");
								if (!canReceiveGifts || !activeObj.canBeGivenAsGift() || obsoleteNotGiftable)
								{
									return false;
								}
								foreach (string activeKey in who.activeDialogueEvents.Keys)
								{
									if (activeKey.Contains("dumped") && this.Dialogue.ContainsKey(activeKey))
									{
										if (!probe)
										{
											base.doEmote(12, true);
										}
										return true;
									}
								}
								if (!probe)
								{
									who.completeQuest("25");
								}
								if (Game1.IsGreenRainingHere(null) && Game1.year == 1 && !this.isMarried())
								{
									if (!probe)
									{
										Game1.showRedMessage(".........", true);
									}
									return false;
								}
								if ((friendship != null && friendship.GiftsThisWeek < 2) || who.spouse == base.Name || this is Child || this.isBirthday() || who.ActiveObject.QualifiedItemId == "(O)StardropTea")
								{
									if (!probe)
									{
										if (friendship == null)
										{
											friendship = (who.friendshipData[base.Name] = new Friendship());
										}
										if (friendship.IsDivorced())
										{
											this.CurrentDialogue.Push(this.TryGetDialogue("RejectGift_Divorced") ?? new Dialogue(this, "Strings\\Characters:Divorced_gift", false));
											Game1.drawDialogue(this);
											return true;
										}
										if (friendship.GiftsToday == 1 && who.ActiveObject.QualifiedItemId != "(O)StardropTea")
										{
											Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3981", this.displayName)));
											return true;
										}
										this.receiveGift(who.ActiveObject, who, who.ActiveObject.QualifiedItemId != "(O)StardropTea", 1f, true);
										who.reduceActiveItemByOne();
										who.completelyStopAnimatingOrDoingAction();
										base.faceTowardFarmerForPeriod(4000, 3, false, who);
										if (this.datable.Value && who.spouse != null && who.spouse != base.Name && !who.hasCurrentOrPendingRoommate() && Utility.isMale(who.spouse) == Utility.isMale(base.Name) && Game1.random.NextDouble() < 0.3 - (double)((float)who.LuckLevel / 100f) - who.DailyLuck && !this.isBirthday() && friendship.IsDating())
										{
											NPC spouse = Game1.getCharacterFromName(who.spouse, true, false);
											CharacterData spouseData = (spouse != null) ? spouse.GetData() : null;
											if (spouse != null && GameStateQuery.CheckConditions((spouseData != null) ? spouseData.SpouseGiftJealousy : null, null, who, activeObj, null, null, null))
											{
												who.changeFriendship((spouseData != null) ? spouseData.SpouseGiftJealousyFriendshipChange : -30, spouse);
												spouse.CurrentDialogue.Clear();
												spouse.CurrentDialogue.Push(spouse.TryGetDialogue("SpouseGiftJealous", new object[]
												{
													this.displayName,
													activeObj.DisplayName
												}) ?? StardewValley.Dialogue.FromTranslation(spouse, "Strings\\StringsFromCSFiles:NPC.cs.3985", this.displayName));
											}
										}
									}
									return true;
								}
								if (!probe)
								{
									Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3987", this.displayName, 2)));
								}
								return true;
							}
						}
						else
						{
							if (!canReceiveGifts)
							{
								return false;
							}
							bool isDivorced = friendship != null && friendship.IsDivorced();
							if (who.spouse == base.Name)
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(37, 1);
								defaultInterpolatedStringHandler.AppendLiteral("RejectMermaidPendant_AlreadyAccepted_");
								defaultInterpolatedStringHandler.AppendFormatted<FriendshipStatus?>((friendship != null) ? new FriendshipStatus?(friendship.Status) : null);
								Dialogue dialogue4 = this.TryGetDialogue(defaultInterpolatedStringHandler.ToStringAndClear()) ?? this.TryGetDialogue("RejectMermaidPendant_AlreadyAccepted");
								if (!probe && dialogue4 != null)
								{
									this.CurrentDialogue.Push(dialogue4);
									Game1.drawDialogue(this);
								}
								return dialogue4 != null;
							}
							if (who.isMarriedOrRoommates() || who.isEngaged())
							{
								if (!probe)
								{
									if (who.hasCurrentOrPendingRoommate())
									{
										Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:TriedToMarryButKrobus"));
									}
									else if (who.isEngaged())
									{
										Stack<Dialogue> currentDialogue = this.CurrentDialogue;
										string key = "RejectMermaidPendant_PlayerWithSomeoneElse";
										object[] array2 = new object[1];
										int num2 = 0;
										NPC spouse3 = who.getSpouse();
										array2[num2] = (((spouse3 != null) ? spouse3.displayName : null) ?? who.spouse);
										Dialogue item;
										if ((item = this.TryGetDialogue(key, array2)) == null)
										{
											item = (this.TryGetDialogue("RejectMermaidPendant") ?? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3965", "3966"), true));
										}
										currentDialogue.Push(item);
										Game1.drawDialogue(this);
									}
									else
									{
										Stack<Dialogue> currentDialogue2 = this.CurrentDialogue;
										Dialogue item2;
										if ((item2 = this.TryGetDialogue("RejectMermaidPendant_PlayerWithSomeoneElse")) == null)
										{
											item2 = (this.TryGetDialogue("RejectMermaidPendant") ?? (Game1.random.NextBool() ? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3967", false) : new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3968", true)));
										}
										currentDialogue2.Push(item2);
										Game1.drawDialogue(this);
									}
								}
								return true;
							}
							if (!this.datable.Value || this.isMarriedOrEngaged() || isDivorced || (friendship != null && friendship.Points < 1500))
							{
								if (!probe)
								{
									if (Game1.random.NextBool())
									{
										Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3969", this.displayName));
									}
									else
									{
										Stack<Dialogue> currentDialogue3 = this.CurrentDialogue;
										Dialogue item3;
										if ((item3 = ((!this.datable.Value) ? this.TryGetDialogue("RejectMermaidPendant_NotDatable") : null)) == null && (item3 = (isDivorced ? this.TryGetDialogue("RejectMermaidPendant_Divorced") : null)) == null)
										{
											Dialogue dialogue7;
											if (!this.isMarriedOrEngaged())
											{
												dialogue7 = null;
											}
											else
											{
												string key2 = "RejectMermaidPendant_NpcWithSomeoneElse";
												object[] array3 = new object[1];
												int num3 = 0;
												Farmer spouse4 = this.getSpouse();
												array3[num3] = ((spouse4 != null) ? spouse4.Name : null);
												dialogue7 = this.TryGetDialogue(key2, array3);
											}
											if ((item3 = dialogue7) == null && (item3 = ((this.datable.Value && friendship != null && friendship.Points < 1500) ? this.TryGetDialogue("RejectMermaidPendant_Under8Hearts") : null)) == null && (item3 = this.TryGetDialogue("RejectMermaidPendant")) == null)
											{
												item3 = new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs." + ((this.Gender == Gender.Female) ? "3970" : "3971"), false);
											}
										}
										currentDialogue3.Push(item3);
										Game1.drawDialogue(this);
									}
								}
								return true;
							}
							if (this.datable.Value && friendship != null && friendship.Points < 2500)
							{
								if (!probe)
								{
									if (!friendship.ProposalRejected)
									{
										this.CurrentDialogue.Push(this.TryGetDialogue("RejectMermaidPendant_Under10Hearts") ?? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3972", "3973"), false));
										Game1.drawDialogue(this);
										who.changeFriendship(-20, this);
										friendship.ProposalRejected = true;
									}
									else
									{
										Stack<Dialogue> currentDialogue4 = this.CurrentDialogue;
										Dialogue item4;
										if ((item4 = this.TryGetDialogue("RejectMermaidPendant_Under10Hearts_AskedAgain")) == null && (item4 = this.TryGetDialogue("RejectMermaidPendant_Under10Hearts")) == null)
										{
											item4 = (this.TryGetDialogue("RejectMermaidPendant") ?? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3974", "3975"), true));
										}
										currentDialogue4.Push(item4);
										Game1.drawDialogue(this);
										who.changeFriendship(-50, this);
									}
								}
								return true;
							}
							if (this.datable.Value && who.houseUpgradeLevel.Value < 1)
							{
								if (!probe)
								{
									if (Game1.random.NextBool())
									{
										Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3969", this.displayName));
									}
									else
									{
										Stack<Dialogue> currentDialogue5 = this.CurrentDialogue;
										Dialogue item5;
										if ((item5 = this.TryGetDialogue("RejectMermaidPendant_NeedHouseUpgrade")) == null)
										{
											item5 = (this.TryGetDialogue("RejectMermaidPendant") ?? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3972", false));
										}
										currentDialogue5.Push(item5);
										Game1.drawDialogue(this);
									}
								}
								return true;
							}
							if (!probe)
							{
								this.engagementResponse(who, false);
							}
							return true;
						}
					}
					else
					{
						if (canReceiveGifts)
						{
							if (!probe)
							{
								if (!this.datable.Value || (friendship == null || !friendship.IsDating()) || (friendship != null && friendship.IsMarried()))
								{
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Wilted_Bouquet_Meaningless", this.displayName));
								}
								else
								{
									Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Wilted_Bouquet_Effect", this.displayName));
									Game1.multiplayer.globalChatInfoMessage("BreakUp", new string[]
									{
										Game1.player.Name,
										this.GetTokenizedDisplayName()
									});
									who.removeDatingActiveDialogueEvents(base.Name);
									who.reduceActiveItemByOne();
									friendship.Status = FriendshipStatus.Friendly;
									if (who.spouse == base.Name)
									{
										who.spouse = null;
									}
									friendship.WeddingDate = null;
									who.completelyStopAnimatingOrDoingAction();
									friendship.Points = Math.Min(friendship.Points, 1250);
									string a = this.name.Value;
									if (!(a == "Maru") && !(a == "Haley"))
									{
										if (!(a == "Shane") && !(a == "Alex"))
										{
											base.doEmote(28, true);
										}
									}
									else
									{
										base.doEmote(12, true);
									}
									this.CurrentDialogue.Clear();
									this.CurrentDialogue.Push(new Dialogue(this, "Characters\\Dialogue\\" + this.GetDialogueSheetName() + ":breakUp", false));
									Game1.drawDialogue(this);
								}
							}
							return true;
						}
						return false;
					}
				}
				else
				{
					if (!canReceiveGifts)
					{
						return false;
					}
					bool npcMarriedToSomeoneElse = who.spouse != base.Name && this.isMarriedOrEngaged();
					if (!this.datable.Value || npcMarriedToSomeoneElse)
					{
						if (!probe)
						{
							if (Game1.random.NextBool())
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3955", this.displayName));
							}
							else
							{
								Stack<Dialogue> currentDialogue6 = this.CurrentDialogue;
								Dialogue item6;
								if ((item6 = ((!this.datable.Value) ? this.TryGetDialogue("RejectBouquet_NotDatable") : null)) == null)
								{
									Dialogue dialogue8;
									if (!npcMarriedToSomeoneElse)
									{
										dialogue8 = null;
									}
									else
									{
										string key3 = "RejectBouquet_NpcAlreadyMarried";
										object[] array4 = new object[1];
										int num4 = 0;
										Farmer spouse5 = this.getSpouse();
										array4[num4] = ((spouse5 != null) ? spouse5.Name : null);
										dialogue8 = this.TryGetDialogue(key3, array4);
									}
									if ((item6 = dialogue8) == null)
									{
										item6 = (this.TryGetDialogue("RejectBouquet") ?? (Game1.random.NextBool() ? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3956", false) : new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3957", true)));
									}
								}
								currentDialogue6.Push(item6);
								Game1.drawDialogue(this);
							}
						}
						return true;
					}
					if (friendship == null)
					{
						friendship = (who.friendshipData[base.Name] = new Friendship());
					}
					if (friendship.IsDating())
					{
						if (!probe)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 1);
							defaultInterpolatedStringHandler.AppendLiteral("RejectBouquet_AlreadyAccepted_");
							defaultInterpolatedStringHandler.AppendFormatted<FriendshipStatus>(friendship.Status);
							Dialogue dialogue5 = this.TryGetDialogue(defaultInterpolatedStringHandler.ToStringAndClear()) ?? this.TryGetDialogue("RejectBouquet_AlreadyAccepted");
							if (dialogue5 != null)
							{
								this.CurrentDialogue.Push(dialogue5);
								Game1.drawDialogue(this);
							}
							else
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:AlreadyDatingBouquet", this.displayName));
							}
						}
						return true;
					}
					if (friendship.IsDivorced())
					{
						if (!probe)
						{
							Stack<Dialogue> currentDialogue7 = this.CurrentDialogue;
							Dialogue item7;
							if ((item7 = this.TryGetDialogue("RejectBouquet_Divorced")) == null)
							{
								item7 = (this.TryGetDialogue("RejectBouquet") ?? new Dialogue(this, "Strings\\Characters:Divorced_bouquet", false));
							}
							currentDialogue7.Push(item7);
							Game1.drawDialogue(this);
						}
						return true;
					}
					if (friendship.Points < 1000)
					{
						if (!probe)
						{
							Stack<Dialogue> currentDialogue8 = this.CurrentDialogue;
							Dialogue item8;
							if ((item8 = this.TryGetDialogue("RejectBouquet_VeryLowHearts")) == null)
							{
								item8 = (this.TryGetDialogue("RejectBouquet") ?? (Game1.random.NextBool() ? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3958", false) : new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3959", true)));
							}
							currentDialogue8.Push(item8);
							Game1.drawDialogue(this);
						}
						return true;
					}
					if (friendship.Points < 2000)
					{
						if (!probe)
						{
							Stack<Dialogue> currentDialogue9 = this.CurrentDialogue;
							Dialogue item9;
							if ((item9 = this.TryGetDialogue("RejectBouquet_LowHearts")) == null)
							{
								item9 = (this.TryGetDialogue("RejectBouquet") ?? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3960", "3961"), false));
							}
							currentDialogue9.Push(item9);
							Game1.drawDialogue(this);
						}
						return true;
					}
					if (!probe)
					{
						friendship.Status = FriendshipStatus.Dating;
						Game1.multiplayer.globalChatInfoMessage("Dating", new string[]
						{
							Game1.player.Name,
							this.GetTokenizedDisplayName()
						});
						this.CurrentDialogue.Push(this.TryGetDialogue("AcceptBouquet") ?? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3962", "3963"), true));
						who.autoGenerateActiveDialogueEvent("dating_" + base.Name, 4);
						who.autoGenerateActiveDialogueEvent("dating", 4);
						who.changeFriendship(25, this);
						who.reduceActiveItemByOne();
						who.completelyStopAnimatingOrDoingAction();
						base.doEmote(20, true);
						Game1.drawDialogue(this);
					}
					return true;
				}
			}
			else
			{
				if (!Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater"))
				{
					if (!probe)
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", this.displayName)));
					}
					return true;
				}
				if (this.SpeaksDwarvish() && !who.canUnderstandDwarves)
				{
					if (!probe)
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", this.displayName)));
					}
					return true;
				}
				string a = base.Name;
				if (!(a == "Krobus"))
				{
					if (a == "Leo")
					{
						if (!Game1.MasterPlayer.mailReceived.Contains("leoMoved"))
						{
							if (!probe)
							{
								Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", this.displayName)));
							}
							return true;
						}
					}
				}
				else if (Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == "Fri")
				{
					if (!probe)
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", this.displayName)));
					}
					return true;
				}
				if (!this.IsVillager || !this.CanSocialize)
				{
					if (!probe)
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_CantInvite", this.displayName)));
					}
					return true;
				}
				if (friendship == null)
				{
					if (!probe)
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_NoTheater", this.displayName)));
					}
					return true;
				}
				if (friendship.IsDivorced())
				{
					if (!probe)
					{
						if (who == Game1.player)
						{
							Game1.multiplayer.globalChatInfoMessage("MovieInviteReject", new string[]
							{
								Game1.player.displayName,
								this.GetTokenizedDisplayName()
							});
						}
						Stack<Dialogue> currentDialogue10 = this.CurrentDialogue;
						Dialogue item10;
						if ((item10 = this.TryGetDialogue("RejectMovieTicket_Divorced")) == null)
						{
							item10 = (this.TryGetDialogue("RejectMovieTicket") ?? new Dialogue(this, "Strings\\Characters:Divorced_gift", false));
						}
						currentDialogue10.Push(item10);
						Game1.drawDialogue(this);
					}
					return true;
				}
				if (who.lastSeenMovieWeek.Value >= Game1.Date.TotalWeeks)
				{
					if (!probe)
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_FarmerAlreadySeen")));
					}
					return true;
				}
				if (Utility.isFestivalDay())
				{
					if (!probe)
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_Festival")));
					}
					return true;
				}
				if (Game1.timeOfDay > 2100)
				{
					if (!probe)
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_Closed")));
					}
					return true;
				}
				foreach (MovieInvitation invitation in who.team.movieInvitations)
				{
					if (invitation.farmer == who)
					{
						if (!probe)
						{
							Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieInvite_AlreadyInvitedSomeone", invitation.invitedNPC.displayName)));
						}
						return true;
					}
				}
				if (!probe)
				{
					base.faceTowardFarmerForPeriod(4000, 3, false, who);
				}
				foreach (MovieInvitation invitation2 in who.team.movieInvitations)
				{
					if (invitation2.invitedNPC == this)
					{
						if (!probe)
						{
							if (who == Game1.player)
							{
								Game1.multiplayer.globalChatInfoMessage("MovieInviteReject", new string[]
								{
									Game1.player.displayName,
									this.GetTokenizedDisplayName()
								});
							}
							Stack<Dialogue> currentDialogue11 = this.CurrentDialogue;
							Dialogue item11;
							if ((item11 = this.TryGetDialogue("RejectMovieTicket_AlreadyInvitedBySomeoneElse", new object[]
							{
								invitation2.farmer.displayName
							})) == null && (item11 = this.TryGetDialogue("RejectMovieTicket")) == null)
							{
								item11 = new Dialogue(this, "Strings\\Characters:MovieInvite_InvitedBySomeoneElse", this.GetDispositionModifiedString("Strings\\Characters:MovieInvite_InvitedBySomeoneElse", new object[]
								{
									invitation2.farmer.displayName
								}));
							}
							currentDialogue11.Push(item11);
							Game1.drawDialogue(this);
						}
						return true;
					}
				}
				if (this.lastSeenMovieWeek.Value >= Game1.Date.TotalWeeks)
				{
					if (!probe)
					{
						if (who == Game1.player)
						{
							Game1.multiplayer.globalChatInfoMessage("MovieInviteReject", new string[]
							{
								Game1.player.displayName,
								this.GetTokenizedDisplayName()
							});
						}
						Stack<Dialogue> currentDialogue12 = this.CurrentDialogue;
						Dialogue item12;
						if ((item12 = this.TryGetDialogue("RejectMovieTicket_AlreadyWatchedThisWeek")) == null)
						{
							item12 = (this.TryGetDialogue("RejectMovieTicket") ?? new Dialogue(this, "Strings\\Characters:MovieInvite_AlreadySeen", this.GetDispositionModifiedString("Strings\\Characters:MovieInvite_AlreadySeen", Array.Empty<object>())));
						}
						currentDialogue12.Push(item12);
						Game1.drawDialogue(this);
					}
					return true;
				}
				if (MovieTheater.GetResponseForMovie(this) == "reject")
				{
					if (!probe)
					{
						if (who == Game1.player)
						{
							Game1.multiplayer.globalChatInfoMessage("MovieInviteReject", new string[]
							{
								Game1.player.displayName,
								this.GetTokenizedDisplayName()
							});
						}
						Stack<Dialogue> currentDialogue13 = this.CurrentDialogue;
						Dialogue item13;
						if ((item13 = this.TryGetDialogue("RejectMovieTicket_DontWantToSeeThatMovie")) == null)
						{
							item13 = (this.TryGetDialogue("RejectMovieTicket") ?? new Dialogue(this, "Strings\\Characters:MovieInvite_Reject", this.GetDispositionModifiedString("Strings\\Characters:MovieInvite_Reject", Array.Empty<object>())));
						}
						currentDialogue13.Push(item13);
						Game1.drawDialogue(this);
					}
					return true;
				}
				if (!probe)
				{
					Stack<Dialogue> currentDialogue14 = this.CurrentDialogue;
					Dialogue item14;
					if ((item14 = ((this.getSpouse() == who) ? StardewValley.Dialogue.TryGetDialogue(this, "Strings\\Characters:MovieInvite_Spouse_" + this.name.Value) : null)) == null)
					{
						item14 = (this.TryGetDialogue("MovieInvitation") ?? new Dialogue(this, "Strings\\Characters:MovieInvite_Invited", this.GetDispositionModifiedString("Strings\\Characters:MovieInvite_Invited", Array.Empty<object>())));
					}
					currentDialogue14.Push(item14);
					Game1.drawDialogue(this);
					who.reduceActiveItemByOne();
					who.completelyStopAnimatingOrDoingAction();
					who.currentLocation.localSound("give_gift", null, null, SoundContext.Default);
					MovieTheater.Invite(who, this);
					if (who == Game1.player)
					{
						Game1.multiplayer.globalChatInfoMessage("MovieInviteAccept", new string[]
						{
							Game1.player.displayName,
							this.GetTokenizedDisplayName()
						});
					}
				}
				return true;
			}
			bool result;
			return result;
		}

		// Token: 0x060011E5 RID: 4581 RVA: 0x000D0C14 File Offset: 0x000CEE14
		public string GetDispositionModifiedString(string path, params object[] substitutions)
		{
			List<string> disposition_tags = new List<string>();
			disposition_tags.Add(this.name.Value);
			if (Game1.player.isMarriedOrRoommates() && Game1.player.getSpouse() == this)
			{
				disposition_tags.Add("spouse");
			}
			CharacterData npcData = this.GetData();
			if (npcData != null)
			{
				disposition_tags.Add(npcData.Manner.ToString().ToLower());
				disposition_tags.Add(npcData.SocialAnxiety.ToString().ToLower());
				disposition_tags.Add(npcData.Optimism.ToString().ToLower());
				disposition_tags.Add(npcData.Age.ToString().ToLower());
			}
			foreach (string tag in disposition_tags)
			{
				string current_path = path + "_" + Utility.capitalizeFirstLetter(tag);
				string found_string = Game1.content.LoadString(current_path, substitutions);
				if (!(found_string == current_path))
				{
					return found_string;
				}
			}
			return Game1.content.LoadString(path, substitutions);
		}

		// Token: 0x060011E6 RID: 4582 RVA: 0x000D0D54 File Offset: 0x000CEF54
		public void haltMe(Farmer who)
		{
			this.Halt();
		}

		// Token: 0x060011E7 RID: 4583 RVA: 0x000D0D5C File Offset: 0x000CEF5C
		public virtual bool checkAction(Farmer who, GameLocation l)
		{
			if (this.IsInvisible)
			{
				return false;
			}
			if (this.isSleeping.Value)
			{
				if (!this.isEmoting)
				{
					base.doEmote(24, true);
				}
				this.shake(250);
				return false;
			}
			if (!who.CanMove)
			{
				return false;
			}
			Friendship friendship;
			Game1.player.friendshipData.TryGetValue(base.Name, out friendship);
			if (base.Name.Equals("Henchman") && l.Name.Equals("WitchSwamp"))
			{
				if (Game1.player.mailReceived.Add("Henchman1"))
				{
					this.CurrentDialogue.Push(new Dialogue(this, "Strings\\Characters:Henchman1", false));
					Game1.drawDialogue(this);
					Game1.player.addQuest("27");
					if (!Game1.player.friendshipData.ContainsKey("Henchman"))
					{
						Game1.player.friendshipData.Add("Henchman", friendship = new Friendship());
					}
				}
				else
				{
					if (who.ActiveObject != null && !who.isRidingHorse() && this.tryToReceiveActiveObject(who, false))
					{
						return true;
					}
					if (this.controller == null)
					{
						this.CurrentDialogue.Push(new Dialogue(this, "Strings\\Characters:Henchman2", false));
						Game1.drawDialogue(this);
					}
				}
				return true;
			}
			Clothing value = who.pantsItem.Value;
			bool reactingToShorts = ((value != null) ? value.QualifiedItemId : null) == "(P)15" && (base.Name == "Lewis" || base.Name == "Marnie");
			if (this.CanReceiveGifts() && friendship == null)
			{
				Game1.player.friendshipData.Add(base.Name, friendship = new Friendship(0));
				if (base.Name.Equals("Krobus"))
				{
					this.CurrentDialogue.Push(new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.3990", false));
					Game1.drawDialogue(this);
					return true;
				}
			}
			if (who.NotifyQuests((Quest quest) => quest.OnNpcSocialized(this, false), false) && Game1.dialogueUp)
			{
				base.faceTowardFarmerForPeriod(6000, 3, false, who);
				return true;
			}
			if (base.Name.Equals("Krobus") && who.hasQuest("28"))
			{
				this.CurrentDialogue.Push(new Dialogue(this, (l is Sewer) ? "Strings\\Characters:KrobusDarkTalisman" : "Strings\\Characters:KrobusDarkTalisman_elsewhere", false));
				Game1.drawDialogue(this);
				who.removeQuest("28");
				who.mailReceived.Add("krobusUnseal");
				if (l is Sewer)
				{
					DelayedAction.addTemporarySpriteAfterDelay(new TemporaryAnimatedSprite("TileSheets\\Projectiles", new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 16), 3000f, 1, 0, new Vector2(31f, 17f) * 64f, false, false)
					{
						scale = 4f,
						delayBeforeAnimationStart = 1,
						startSound = "debuffSpell",
						motion = new Vector2(-9f, 1f),
						rotationChange = 0.049087387f,
						lightId = "Krobus_Unseal_1",
						lightRadius = 1f,
						lightcolor = new Color(150, 0, 50),
						layerDepth = 1f,
						alphaFade = 0.003f
					}, l, 200, true);
					DelayedAction.addTemporarySpriteAfterDelay(new TemporaryAnimatedSprite("TileSheets\\Projectiles", new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 16), 3000f, 1, 0, new Vector2(31f, 17f) * 64f, false, false)
					{
						startSound = "debuffSpell",
						delayBeforeAnimationStart = 1,
						scale = 4f,
						motion = new Vector2(-9f, 1f),
						rotationChange = 0.049087387f,
						lightId = "Krobus_Unseal_2",
						lightRadius = 1f,
						lightcolor = new Color(150, 0, 50),
						layerDepth = 1f,
						alphaFade = 0.003f
					}, l, 700, true);
				}
				return true;
			}
			if (this.name.Value == "Jas" && base.currentLocation is Desert && who.mailReceived.Contains("Jas_IceCream_DF_" + Game1.year.ToString()))
			{
				base.doEmote(32, true);
				return true;
			}
			if (base.Name == who.spouse && who.IsLocalPlayer && this.Sprite.CurrentAnimation == null)
			{
				this.faceDirection(-3);
				if (friendship != null && friendship.Points >= 3125 && who.mailReceived.Add("CF_Spouse"))
				{
					this.CurrentDialogue.Push(this.TryGetDialogue("SpouseStardrop") ?? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4001", false));
					Object stardrop = ItemRegistry.Create<Object>("(O)434", 1, 0, false);
					stardrop.CanBeSetDown = false;
					stardrop.CanBeGrabbed = false;
					Game1.player.addItemByMenuIfNecessary(stardrop, null, false);
					this.shouldSayMarriageDialogue.Value = false;
					this.currentMarriageDialogue.Clear();
					return true;
				}
				if (!this.hasTemporaryMessageAvailable() && this.currentMarriageDialogue.Count == 0 && this.CurrentDialogue.Count == 0 && Game1.timeOfDay < 2200 && !this.isMoving() && who.ActiveObject == null)
				{
					if (this.faceTowardFarmerTimer <= 0)
					{
						this.facingDirectionBeforeSpeakingToPlayer.Value = this.FacingDirection;
					}
					base.faceGeneralDirection(who.getStandingPosition(), 0, false, false);
					who.faceGeneralDirection(base.getStandingPosition(), 0, false, false);
					if (this.FacingDirection == 3 || this.FacingDirection == 1)
					{
						CharacterData data = this.GetData();
						int spouseFrame = (data != null) ? data.KissSpriteIndex : 28;
						bool facingRight = data == null || data.KissSpriteFacingRight;
						bool flip = facingRight != (this.FacingDirection == 1);
						if (who.getFriendshipHeartLevelForNPC(base.Name) > 9 && this.sleptInBed.Value)
						{
							int delay = Game1.IsMultiplayer ? 1000 : 10;
							this.movementPause = delay;
							base.faceTowardFarmerForPeriod(3000, 3, false, who);
							this.Sprite.ClearAnimation();
							this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(spouseFrame, delay, false, flip, new AnimatedSprite.endOfAnimationBehavior(this.haltMe), true));
							if (!this.hasBeenKissedToday.Value)
							{
								who.changeFriendship(10, this);
								if (who.hasCurrentOrPendingRoommate())
								{
									Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite[]
									{
										new TemporaryAnimatedSprite("LooseSprites\\emojis", new Microsoft.Xna.Framework.Rectangle(0, 0, 9, 9), 2000f, 1, 0, base.Tile * 64f + new Vector2(16f, -64f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
										{
											motion = new Vector2(0f, -0.5f),
											alphaFade = 0.01f
										}
									});
								}
								else
								{
									Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite[]
									{
										new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(211, 428, 7, 6), 2000f, 1, 0, base.Tile * 64f + new Vector2(16f, -64f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
										{
											motion = new Vector2(0f, -0.5f),
											alphaFade = 0.01f
										}
									});
								}
								l.playSound("dwop", null, null, SoundContext.NPC);
								who.exhausted.Value = false;
							}
							else if (Game1.random.NextDouble() < 0.1)
							{
								base.doEmote(20, true);
							}
							this.hasBeenKissedToday.Value = true;
							this.Sprite.UpdateSourceRect();
						}
						else
						{
							this.faceDirection(Game1.random.Choose(2, 0));
							base.doEmote(12, true);
						}
						int playerFaceDirection = 1;
						if ((facingRight && !flip) || (!facingRight && flip))
						{
							playerFaceDirection = 3;
						}
						who.PerformKiss(playerFaceDirection);
						return true;
					}
					if (this.faceTowardFarmerTimer <= 0 && Game1.random.NextDouble() < 0.1)
					{
						Game1.playSound("dwop", null);
						if (who.getFriendshipHeartLevelForNPC(base.Name) > 9)
						{
							base.doEmote((Game1.random.NextDouble() < 0.5) ? 32 : 20, true);
						}
						else if (who.getFriendshipHeartLevelForNPC(base.Name) > 7)
						{
							base.doEmote((Game1.random.NextDouble() < 0.5) ? 40 : 8, true);
						}
						else
						{
							base.doEmote((Game1.random.NextDouble() < 0.5) ? 28 : 12, true);
						}
					}
					else if (this.facingDirectionBeforeSpeakingToPlayer.Value >= 0 && Math.Abs(this.facingDirectionBeforeSpeakingToPlayer.Value - this.FacingDirection) == 2 && Game1.random.NextDouble() < 0.1)
					{
						this.jump();
						base.doEmote(16, true);
					}
					base.faceTowardFarmerForPeriod(3000, 4, false, who);
				}
			}
			if (base.SimpleNonVillagerNPC)
			{
				if (this.name.Value == "Fizz")
				{
					int waivers = Game1.netWorldState.Value.PerfectionWaivers;
					if (Utility.percentGameComplete() + (float)waivers * 0.01f >= 1f)
					{
						base.doEmote(56, true);
						this.shakeTimer = 250;
					}
					else
					{
						this.CurrentDialogue.Clear();
						if (!Game1.player.mailReceived.Contains("FizzFirstDialogue"))
						{
							Game1.player.mailReceived.Add("FizzFirstDialogue");
							this.CurrentDialogue.Push(new Dialogue(this, "Strings\\1_6_Strings:Fizz_Intro_1", false));
							Game1.drawDialogue(this);
						}
						else
						{
							this.CurrentDialogue.Push(new Dialogue(this, "Strings\\1_6_Strings:Fizz_Intro_2", false));
							Game1.drawDialogue(this);
							Game1.afterDialogues = delegate()
							{
								Game1.currentLocation.createQuestionDialogue("", new Response[]
								{
									new Response("Yes", Game1.content.LoadString("Strings\\1_6_Strings:Fizz_Yes")).SetHotKey(Keys.Y),
									new Response("No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")).SetHotKey(Keys.Escape)
								}, "Fizz");
							};
						}
					}
				}
				else
				{
					string path = "Strings\\SimpleNonVillagerDialogues:" + base.Name;
					string s = Game1.content.LoadString(path);
					if (s != path)
					{
						string[] split = s.Split("||", StringSplitOptions.None);
						if (this.nonVillagerNPCTimesTalked != -1 && this.nonVillagerNPCTimesTalked < split.Length)
						{
							Game1.drawObjectDialogue(split[this.nonVillagerNPCTimesTalked]);
							this.nonVillagerNPCTimesTalked++;
							if (this.nonVillagerNPCTimesTalked >= split.Length)
							{
								this.nonVillagerNPCTimesTalked = -1;
							}
						}
					}
				}
				return true;
			}
			bool newCurrentDialogue = false;
			if (friendship != null)
			{
				if (this.getSpouse() == Game1.player && this.shouldSayMarriageDialogue.Value && this.currentMarriageDialogue.Count > 0 && this.currentMarriageDialogue.Count > 0)
				{
					while (this.currentMarriageDialogue.Count > 0)
					{
						MarriageDialogueReference dialogue_reference = this.currentMarriageDialogue[this.currentMarriageDialogue.Count - 1];
						if (dialogue_reference == this.marriageDefaultDialogue.Value)
						{
							this.marriageDefaultDialogue.Value = null;
						}
						this.currentMarriageDialogue.RemoveAt(this.currentMarriageDialogue.Count - 1);
						this.CurrentDialogue.Push(dialogue_reference.GetDialogue(this));
					}
					newCurrentDialogue = true;
				}
				if (!newCurrentDialogue)
				{
					newCurrentDialogue = this.checkForNewCurrentDialogue(friendship.Points / 250, false);
					if (!newCurrentDialogue)
					{
						newCurrentDialogue = this.checkForNewCurrentDialogue(friendship.Points / 250, true);
					}
				}
			}
			if (who.IsLocalPlayer && friendship != null && (this.endOfRouteMessage.Value != null || newCurrentDialogue || (base.currentLocation != null && base.currentLocation.HasLocationOverrideDialogue(this))))
			{
				if (!newCurrentDialogue && this.setTemporaryMessages(who))
				{
					who.NotifyQuests((Quest quest) => quest.OnNpcSocialized(this, false), false);
					return false;
				}
				Texture2D texture = this.Sprite.Texture;
				if (texture != null && texture.Bounds.Height > 32 && (this.CurrentDialogue.Count <= 0 || !this.CurrentDialogue.Peek().dontFaceFarmer))
				{
					base.faceTowardFarmerForPeriod(5000, 4, false, who);
				}
				if (who.ActiveObject != null && !who.isRidingHorse() && this.tryToReceiveActiveObject(who, false))
				{
					who.NotifyQuests((Quest quest) => quest.OnNpcSocialized(this, false), false);
					base.faceTowardFarmerForPeriod(3000, 4, false, who);
					return true;
				}
				this.grantConversationFriendship(who, 20);
				Game1.drawDialogue(this);
				return true;
			}
			else
			{
				if (this.canTalk() && who.hasClubCard && base.Name.Equals("Bouncer") && who.IsLocalPlayer)
				{
					Response[] responses = new Response[]
					{
						new Response("Yes.", Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4018")),
						new Response("That's", Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4020"))
					};
					l.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4021"), responses, "ClubCard");
				}
				else if (this.canTalk() && this.CurrentDialogue.Count > 0)
				{
					if (who.ActiveObject != null && !who.isRidingHorse() && this.tryToReceiveActiveObject(who, true))
					{
						if (who.IsLocalPlayer)
						{
							this.tryToReceiveActiveObject(who, false);
						}
						else
						{
							base.faceTowardFarmerForPeriod(3000, 4, false, who);
						}
						return true;
					}
					if (this.CurrentDialogue.Count >= 1 || this.endOfRouteMessage.Value != null || (base.currentLocation != null && base.currentLocation.HasLocationOverrideDialogue(this)))
					{
						if (this.setTemporaryMessages(who))
						{
							who.NotifyQuests((Quest quest) => quest.OnNpcSocialized(this, false), false);
							return false;
						}
						Texture2D texture2 = this.Sprite.Texture;
						if (texture2 != null && texture2.Bounds.Height > 32 && !this.CurrentDialogue.Peek().dontFaceFarmer)
						{
							base.faceTowardFarmerForPeriod(5000, 4, false, who);
						}
						if (who.IsLocalPlayer)
						{
							this.grantConversationFriendship(who, 20);
							if (!reactingToShorts)
							{
								Game1.drawDialogue(this);
								return true;
							}
						}
					}
					else if (!this.doingEndOfRouteAnimation.Value)
					{
						try
						{
							if (friendship != null)
							{
								base.faceTowardFarmerForPeriod(friendship.Points / 125 * 1000 + 1000, 4, false, who);
							}
						}
						catch (Exception)
						{
						}
						if (Game1.random.NextDouble() < 0.1)
						{
							base.doEmote(8, true);
						}
					}
				}
				else if (this.canTalk() && !Game1.game1.wasAskedLeoMemory && Game1.CurrentEvent == null && this.name.Value == "Leo" && base.currentLocation != null && (base.currentLocation.NameOrUniqueName == "LeoTreeHouse" || base.currentLocation.NameOrUniqueName == "Mountain") && Game1.MasterPlayer.hasOrWillReceiveMail("leoMoved") && this.GetUnseenLeoEvent() != null && this.CanRevisitLeoMemory(this.GetUnseenLeoEvent()))
				{
					Game1.DrawDialogue(this, "Strings\\Characters:Leo_Memory");
					Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(this.AskLeoMemoryPrompt));
				}
				else
				{
					if (who.ActiveObject != null && !who.isRidingHorse() && this.tryToReceiveActiveObject(who, false))
					{
						base.faceTowardFarmerForPeriod(3000, 4, false, who);
						return true;
					}
					string name = base.Name;
					if (!(name == "Krobus"))
					{
						if (name == "Dwarf")
						{
							if (who.canUnderstandDwarves && l is Mine)
							{
								Utility.TryOpenShopMenu("Dwarf", base.Name, true);
								return true;
							}
						}
					}
					else if (l is Sewer)
					{
						Utility.TryOpenShopMenu("ShadowShop", "Krobus", true);
						return true;
					}
				}
				if (reactingToShorts)
				{
					if (this.yJumpVelocity != 0f || this.Sprite.CurrentAnimation != null)
					{
						return true;
					}
					string name = base.Name;
					if (!(name == "Lewis"))
					{
						if (name == "Marnie")
						{
							base.faceTowardFarmerForPeriod(1000, 3, false, who);
							this.Sprite.ClearAnimation();
							this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(33, 150, false, false, delegate(Farmer x)
							{
								l.playSound("dustMeep", null, null, SoundContext.Default);
							}, false));
							this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(34, 180));
							this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(33, 180, false, false, delegate(Farmer x)
							{
								l.playSound("dustMeep", null, null, SoundContext.Default);
							}, false));
							this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(34, 180));
							this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(33, 180, false, false, delegate(Farmer x)
							{
								l.playSound("dustMeep", null, null, SoundContext.Default);
							}, false));
							this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(34, 180));
							this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(33, 180, false, false, delegate(Farmer x)
							{
								l.playSound("dustMeep", null, null, SoundContext.Default);
							}, false));
							this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(34, 180));
							this.Sprite.loop = false;
						}
					}
					else
					{
						base.faceTowardFarmerForPeriod(1000, 3, false, who);
						this.jump();
						this.Sprite.ClearAnimation();
						this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(26, 1000, false, false, delegate(Farmer x)
						{
							this.doEmote(12, true);
						}, true));
						this.Sprite.loop = false;
						this.shakeTimer = 1000;
						l.playSound("batScreech", null, null, SoundContext.Default);
					}
					return true;
				}
				else
				{
					if (this.setTemporaryMessages(who))
					{
						return false;
					}
					if ((this.doingEndOfRouteAnimation.Value || !this.goingToDoEndOfRouteAnimation.Value) && this.endOfRouteMessage.Value != null)
					{
						Game1.drawDialogue(this);
						return true;
					}
					return false;
				}
			}
		}

		// Token: 0x060011E8 RID: 4584 RVA: 0x000D204C File Offset: 0x000D024C
		public void grantConversationFriendship(Farmer who, int amount = 20)
		{
			Friendship friendship;
			if (!who.hasPlayerTalkedToNPC(base.Name) && who.friendshipData.TryGetValue(base.Name, out friendship))
			{
				friendship.TalkedToToday = true;
				who.NotifyQuests((Quest quest) => quest.OnNpcSocialized(this, false), false);
				if (this.isDivorcedFrom(who))
				{
					return;
				}
				if (who.hasBuff("statue_of_blessings_4"))
				{
					amount = 60;
				}
				who.changeFriendship(amount, this);
			}
		}

		// Token: 0x060011E9 RID: 4585 RVA: 0x000D20BC File Offset: 0x000D02BC
		public virtual void AskLeoMemoryPrompt()
		{
			GameLocation i = base.currentLocation;
			Response[] responses = new Response[]
			{
				new Response("Yes", Game1.content.LoadString("Strings\\Characters:Leo_Memory_Answer_Yes")),
				new Response("No", Game1.content.LoadString("Strings\\Characters:Leo_Memory_Answer_No"))
			};
			string question = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Characters:Leo_Memory_" + this.GetUnseenLeoEvent().Value.Value, true);
			if (question == null)
			{
				question = "";
			}
			i.createQuestionDialogue(question, responses, new GameLocation.afterQuestionBehavior(this.OnLeoMemoryResponse), this);
		}

		// Token: 0x060011EA RID: 4586 RVA: 0x000D215C File Offset: 0x000D035C
		public bool CanRevisitLeoMemory(KeyValuePair<string, string>? event_data)
		{
			if (event_data == null)
			{
				return false;
			}
			string location_name = event_data.Value.Key;
			string event_id = event_data.Value.Value;
			Dictionary<string, string> location_events;
			try
			{
				location_events = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + location_name);
			}
			catch
			{
				return false;
			}
			if (location_events == null)
			{
				return false;
			}
			foreach (string key in location_events.Keys)
			{
				if (Event.SplitPreconditions(key)[0] == event_id)
				{
					GameLocation locationFromName = Game1.getLocationFromName(location_name);
					string event_key = key;
					event_key = event_key.Replace("/e 1039573", "");
					event_key = event_key.Replace("/Hl leoMoved", "");
					string condition = (locationFromName != null) ? locationFromName.checkEventPrecondition(event_key) : null;
					if (locationFromName != null && string.IsNullOrEmpty(condition) && condition != "-1")
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060011EB RID: 4587 RVA: 0x000D2280 File Offset: 0x000D0480
		public KeyValuePair<string, string>? GetUnseenLeoEvent()
		{
			if (!Game1.player.eventsSeen.Contains("6497423"))
			{
				return new KeyValuePair<string, string>?(new KeyValuePair<string, string>("IslandWest", "6497423"));
			}
			if (!Game1.player.eventsSeen.Contains("6497421"))
			{
				return new KeyValuePair<string, string>?(new KeyValuePair<string, string>("IslandNorth", "6497421"));
			}
			if (!Game1.player.eventsSeen.Contains("6497428"))
			{
				return new KeyValuePair<string, string>?(new KeyValuePair<string, string>("IslandSouth", "6497428"));
			}
			return null;
		}

		// Token: 0x060011EC RID: 4588 RVA: 0x000D2318 File Offset: 0x000D0518
		public void OnLeoMemoryResponse(Farmer who, string whichAnswer)
		{
			if (whichAnswer.EqualsIgnoreCase("yes"))
			{
				KeyValuePair<string, string>? event_data = this.GetUnseenLeoEvent();
				if (event_data == null)
				{
					return;
				}
				string location_name = event_data.Value.Key;
				string event_id = event_data.Value.Value;
				string eventAssetName = "Data\\Events\\" + location_name;
				Dictionary<string, string> location_events;
				try
				{
					location_events = Game1.content.Load<Dictionary<string, string>>(eventAssetName);
				}
				catch
				{
					return;
				}
				if (location_events == null)
				{
					return;
				}
				Point oldTile = Game1.player.TilePoint;
				string oldLocation = Game1.player.currentLocation.NameOrUniqueName;
				int oldDirection = Game1.player.FacingDirection;
				using (Dictionary<string, string>.KeyCollection.Enumerator enumerator = location_events.Keys.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						string key = enumerator.Current;
						if (Event.SplitPreconditions(key)[0] == event_id)
						{
							LocationRequest location_request = Game1.getLocationRequest(location_name, false);
							Game1.warpingForForcedRemoteEvent = true;
							location_request.OnWarp += delegate()
							{
								Event memory_event = new Event(location_events[key], eventAssetName, "event_id", null);
								memory_event.isMemory = true;
								memory_event.setExitLocation(oldLocation, oldTile.X, oldTile.Y);
								Game1.player.orientationBeforeEvent = oldDirection;
								location_request.Location.currentEvent = memory_event;
								location_request.Location.startEvent(memory_event);
								Game1.warpingForForcedRemoteEvent = false;
							};
							int x = 8;
							int y = 8;
							Utility.getDefaultWarpLocation(location_request.Name, ref x, ref y);
							Game1.warpFarmer(location_request, x, y, Game1.player.FacingDirection);
						}
					}
					return;
				}
			}
			Game1.game1.wasAskedLeoMemory = true;
		}

		// Token: 0x060011ED RID: 4589 RVA: 0x000D24CC File Offset: 0x000D06CC
		public bool isDivorcedFrom(Farmer who)
		{
			return NPC.IsDivorcedFrom(who, base.Name);
		}

		// Token: 0x060011EE RID: 4590 RVA: 0x000D24DC File Offset: 0x000D06DC
		public static bool IsDivorcedFrom(Farmer player, string npcName)
		{
			Friendship friendship;
			return player != null && player.friendshipData.TryGetValue(npcName, out friendship) && friendship.IsDivorced();
		}

		// Token: 0x060011EF RID: 4591 RVA: 0x000D2504 File Offset: 0x000D0704
		public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
		{
			if (this.movementPause <= 0)
			{
				this.faceTowardFarmerTimer = 0;
				base.MovePosition(time, viewport, currentLocation);
			}
		}

		// Token: 0x060011F0 RID: 4592 RVA: 0x000D251F File Offset: 0x000D071F
		public GameLocation getHome()
		{
			if (this.isMarried() && this.getSpouse() != null)
			{
				return Utility.getHomeOfFarmer(this.getSpouse());
			}
			return Game1.RequireLocation(this.defaultMap.Value, false);
		}

		// Token: 0x060011F1 RID: 4593 RVA: 0x000D254E File Offset: 0x000D074E
		public override bool canPassThroughActionTiles()
		{
			return true;
		}

		// Token: 0x060011F2 RID: 4594 RVA: 0x000D2551 File Offset: 0x000D0751
		public virtual void behaviorOnFarmerPushing()
		{
		}

		// Token: 0x060011F3 RID: 4595 RVA: 0x000D2554 File Offset: 0x000D0754
		public virtual void behaviorOnFarmerLocationEntry(GameLocation location, Farmer who)
		{
			if (this.Sprite != null && this.Sprite.CurrentAnimation == null && this.Sprite.SourceRect.Height > 32 && !base.SimpleNonVillagerNPC)
			{
				this.Sprite.SpriteWidth = 16;
				this.Sprite.SpriteHeight = 16;
				this.Sprite.currentFrame = 0;
			}
		}

		// Token: 0x060011F4 RID: 4596 RVA: 0x000D25B8 File Offset: 0x000D07B8
		public virtual void behaviorOnLocalFarmerLocationEntry(GameLocation location)
		{
			this.shouldPlayRobinHammerAnimation.CancelInterpolation();
			this.shouldPlaySpousePatioAnimation.CancelInterpolation();
			this.shouldWearIslandAttire.CancelInterpolation();
			this.isSleeping.CancelInterpolation();
			this.doingEndOfRouteAnimation.CancelInterpolation();
			if (this.doingEndOfRouteAnimation.Value)
			{
				this._skipRouteEndIntro = true;
			}
			else
			{
				this._skipRouteEndIntro = false;
			}
			this.endOfRouteBehaviorName.CancelInterpolation();
			if (this.isSleeping.Value)
			{
				this.position.Field.CancelInterpolation();
			}
		}

		// Token: 0x060011F5 RID: 4597 RVA: 0x000D2644 File Offset: 0x000D0844
		public override void updateMovement(GameLocation location, GameTime time)
		{
			this.lastPosition = base.Position;
			if (this.DirectionsToNewLocation != null && !Game1.newDay)
			{
				Point standingPixel = base.StandingPixel;
				if (standingPixel.X < -64 || standingPixel.X > location.map.DisplayWidth + 64 || standingPixel.Y < -64 || standingPixel.Y > location.map.DisplayHeight + 64)
				{
					this.IsWalkingInSquare = false;
					Game1.warpCharacter(this, this.DefaultMap, this.DefaultPosition);
					location.characters.Remove(this);
					return;
				}
				if (this.IsWalkingInSquare)
				{
					this.returnToEndPoint();
					this.MovePosition(time, Game1.viewport, location);
					return;
				}
			}
			else if (this.IsWalkingInSquare)
			{
				this.randomSquareMovement(time);
				this.MovePosition(time, Game1.viewport, location);
			}
		}

		// Token: 0x060011F6 RID: 4598 RVA: 0x000D2718 File Offset: 0x000D0918
		public void facePlayer(Farmer who)
		{
			if (this.facingDirectionBeforeSpeakingToPlayer.Value == -1)
			{
				this.facingDirectionBeforeSpeakingToPlayer.Value = base.getFacingDirection();
			}
			this.faceDirection((who.FacingDirection + 2) % 4);
		}

		// Token: 0x060011F7 RID: 4599 RVA: 0x000D2749 File Offset: 0x000D0949
		public void doneFacingPlayer(Farmer who)
		{
		}

		// Token: 0x060011F8 RID: 4600 RVA: 0x000D274C File Offset: 0x000D094C
		public override void update(GameTime time, GameLocation location)
		{
			if (this.AllowDynamicAppearance && base.currentLocation != null && base.currentLocation.NameOrUniqueName != this.LastLocationNameForAppearance)
			{
				this.ChooseAppearance(null);
			}
			if (Game1.IsMasterGame && this.currentScheduleDelay > 0f)
			{
				this.currentScheduleDelay -= (float)time.ElapsedGameTime.TotalSeconds;
				if (this.currentScheduleDelay <= 0f)
				{
					this.currentScheduleDelay = -1f;
					this.checkSchedule(Game1.timeOfDay);
					this.currentScheduleDelay = 0f;
				}
			}
			this.removeHenchmanEvent.Poll();
			if (Game1.IsMasterGame && this.shouldWearIslandAttire.Value && (base.currentLocation == null || base.currentLocation.InValleyContext()))
			{
				this.shouldWearIslandAttire.Value = false;
			}
			if (this._startedEndOfRouteBehavior == null && this._finishingEndOfRouteBehavior == null && this.loadedEndOfRouteBehavior != this.endOfRouteBehaviorName.Value)
			{
				this.loadEndOfRouteBehavior(this.endOfRouteBehaviorName.Value);
			}
			if (this.doingEndOfRouteAnimation.Value != this.currentlyDoingEndOfRouteAnimation)
			{
				if (!this.currentlyDoingEndOfRouteAnimation)
				{
					if (string.Equals(this.loadedEndOfRouteBehavior, this.endOfRouteBehaviorName.Value, StringComparison.Ordinal))
					{
						this.reallyDoAnimationAtEndOfScheduleRoute();
					}
				}
				else
				{
					this.finishEndOfRouteAnimation();
				}
				this.currentlyDoingEndOfRouteAnimation = this.doingEndOfRouteAnimation.Value;
			}
			if (this.shouldWearIslandAttire.Value != this.isWearingIslandAttire)
			{
				if (!this.isWearingIslandAttire)
				{
					this.wearIslandAttire();
				}
				else
				{
					this.wearNormalClothes();
				}
			}
			if (this.isSleeping.Value != this.isPlayingSleepingAnimation)
			{
				if (!this.isPlayingSleepingAnimation)
				{
					this.playSleepingAnimation();
				}
				else
				{
					this.Sprite.StopAnimation();
					this.isPlayingSleepingAnimation = false;
				}
			}
			if (this.shouldPlayRobinHammerAnimation.Value != this.isPlayingRobinHammerAnimation)
			{
				if (!this.isPlayingRobinHammerAnimation)
				{
					this.doPlayRobinHammerAnimation();
					this.isPlayingRobinHammerAnimation = true;
				}
				else
				{
					this.Sprite.StopAnimation();
					this.isPlayingRobinHammerAnimation = false;
				}
			}
			if (this.shouldPlaySpousePatioAnimation.Value != this.isPlayingSpousePatioAnimation)
			{
				if (!this.isPlayingSpousePatioAnimation)
				{
					this.doPlaySpousePatioAnimation();
					this.isPlayingSpousePatioAnimation = true;
				}
				else
				{
					this.Sprite.StopAnimation();
					this.isPlayingSpousePatioAnimation = false;
				}
			}
			if (this.returningToEndPoint)
			{
				this.returnToEndPoint();
				this.MovePosition(time, Game1.viewport, location);
			}
			else if (this.temporaryController != null)
			{
				if (this.temporaryController.update(time))
				{
					bool npcschedule = this.temporaryController.NPCSchedule;
					this.temporaryController = null;
					if (npcschedule)
					{
						this.currentScheduleDelay = -1f;
						this.checkSchedule(Game1.timeOfDay);
						this.currentScheduleDelay = 0f;
					}
				}
				base.updateEmote(time);
			}
			else
			{
				base.update(time, location);
			}
			if (this.textAboveHeadTimer > 0)
			{
				if (this.textAboveHeadPreTimer > 0)
				{
					this.textAboveHeadPreTimer -= time.ElapsedGameTime.Milliseconds;
				}
				else
				{
					this.textAboveHeadTimer -= time.ElapsedGameTime.Milliseconds;
					if (this.textAboveHeadTimer > 500)
					{
						this.textAboveHeadAlpha = Math.Min(1f, this.textAboveHeadAlpha + 0.1f);
					}
					else
					{
						this.textAboveHeadAlpha = Math.Max(0f, this.textAboveHeadAlpha - 0.04f);
					}
				}
			}
			if (this.isWalkingInSquare && !this.returningToEndPoint)
			{
				this.randomSquareMovement(time);
			}
			AnimatedSprite sprite = this.Sprite;
			if (((sprite != null) ? sprite.CurrentAnimation : null) != null && !Game1.eventUp && Game1.IsMasterGame && this.Sprite.animateOnce(time))
			{
				this.Sprite.CurrentAnimation = null;
			}
			if (this.movementPause > 0 && (!Game1.dialogueUp || this.controller != null))
			{
				this.freezeMotion = true;
				this.movementPause -= time.ElapsedGameTime.Milliseconds;
				if (this.movementPause <= 0)
				{
					this.freezeMotion = false;
				}
			}
			if (this.shakeTimer > 0)
			{
				this.shakeTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (this.lastPosition.Equals(base.Position))
			{
				this.timerSinceLastMovement += (float)time.ElapsedGameTime.Milliseconds;
			}
			else
			{
				this.timerSinceLastMovement = 0f;
			}
			if (this.swimming.Value)
			{
				this.yOffset = (float)(Math.Cos(time.TotalGameTime.TotalMilliseconds / 2000.0) * 4.0);
				float oldSwimTimer = this.swimTimer;
				this.swimTimer -= (float)time.ElapsedGameTime.Milliseconds;
				if (this.timerSinceLastMovement == 0f)
				{
					if (oldSwimTimer > 400f && this.swimTimer <= 400f && location.Equals(Game1.currentLocation))
					{
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
						{
							new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(this.xVelocity) + Math.Abs(this.yVelocity)) * 3f, 8, 0, new Vector2(base.Position.X, (float)(base.StandingPixel.Y - 32)), false, Game1.random.NextBool(), 0.01f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f, false)
						});
						location.playSound("slosh", null, null, SoundContext.NPC);
					}
					if (this.swimTimer < 0f)
					{
						this.swimTimer = 800f;
						if (location.Equals(Game1.currentLocation))
						{
							location.playSound("slosh", null, null, SoundContext.NPC);
							Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
							{
								new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(this.xVelocity) + Math.Abs(this.yVelocity)) * 3f, 8, 0, new Vector2(base.Position.X, (float)(base.StandingPixel.Y - 32)), false, Game1.random.NextBool(), 0.01f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f, false)
							});
						}
					}
				}
				else if (this.swimTimer < 0f)
				{
					this.swimTimer = 100f;
				}
			}
			if (Game1.IsMasterGame)
			{
				this.isMovingOnPathFindPath.Value = (this.controller != null && this.temporaryController != null);
			}
		}

		// Token: 0x060011F9 RID: 4601 RVA: 0x000D2E0D File Offset: 0x000D100D
		public virtual void wearIslandAttire()
		{
			this.isWearingIslandAttire = true;
			this.ChooseAppearance(null);
		}

		// Token: 0x060011FA RID: 4602 RVA: 0x000D2E1D File Offset: 0x000D101D
		public virtual void wearNormalClothes()
		{
			this.isWearingIslandAttire = false;
			this.ChooseAppearance(null);
		}

		// Token: 0x060011FB RID: 4603 RVA: 0x000D2E30 File Offset: 0x000D1030
		public virtual void performTenMinuteUpdate(int timeOfDay, GameLocation location)
		{
			if (!Game1.eventUp && location != null)
			{
				string rawText;
				if (Game1.random.NextDouble() < 0.1 && this.Dialogue != null && this.Dialogue.TryGetValue(location.Name + "_Ambient", out rawText))
				{
					CharacterData data2 = this.GetData();
					if (data2 == null || data2.CanGreetNearbyCharacters)
					{
						string[] split = rawText.Split('/', StringSplitOptions.None);
						int extraTime = Game1.random.Next(4) * 1000;
						this.showTextAboveHead(Game1.random.Choose(split), null, 2, 3000, extraTime);
						return;
					}
				}
				if (this.isMoving() && location.IsOutdoors && timeOfDay < 1800 && Game1.random.NextDouble() < 0.3 + ((this.SocialAnxiety == 0) ? 0.25 : ((this.SocialAnxiety == 1) ? ((this.Manners == 2) ? -1.0 : -0.2) : 0.0)) && (this.Age != 1 || (this.Manners == 1 && this.SocialAnxiety == 0)) && !this.isMarried())
				{
					CharacterData data = this.GetData();
					if (data != null && data.CanGreetNearbyCharacters)
					{
						Character c = Utility.isThereAFarmerOrCharacterWithinDistance(base.Tile, 4, location);
						if (c != null && !(c.Name == base.Name) && !(c is Horse))
						{
							NPC npc = c as NPC;
							bool flag;
							if (npc == null)
							{
								flag = false;
							}
							else
							{
								CharacterData data3 = npc.GetData();
								bool? flag2 = (data3 != null) ? new bool?(data3.CanGreetNearbyCharacters) : null;
								bool flag3 = false;
								flag = (flag2.GetValueOrDefault() == flag3 & flag2 != null);
							}
							if (!flag)
							{
								NPC npc2 = c as NPC;
								if (npc2 == null || !npc2.SimpleNonVillagerNPC)
								{
									Dictionary<string, string> friendsAndFamily = data.FriendsAndFamily;
									if ((friendsAndFamily == null || !friendsAndFamily.ContainsKey(c.Name)) && this.isFacingToward(c.Tile))
									{
										this.sayHiTo(c);
										return;
									}
									return;
								}
							}
						}
						return;
					}
				}
			}
		}

		// Token: 0x060011FC RID: 4604 RVA: 0x000D3064 File Offset: 0x000D1264
		public void sayHiTo(Character c)
		{
			if (this.getHi(c.displayName) == null)
			{
				return;
			}
			this.showTextAboveHead(this.getHi(c.displayName), null, 2, 3000, 0);
			NPC npc = c as NPC;
			if (npc != null && Game1.random.NextDouble() < 0.66)
			{
				if (npc.getHi(this.displayName) == null)
				{
					return;
				}
				npc.showTextAboveHead(npc.getHi(this.displayName), null, 2, 3000, 1000 + Game1.random.Next(500));
			}
		}

		// Token: 0x060011FD RID: 4605 RVA: 0x000D3108 File Offset: 0x000D1308
		public string getHi(string nameToGreet)
		{
			if (this.Age == 2)
			{
				if (this.SocialAnxiety != 1)
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4059");
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4058");
			}
			else
			{
				int num = this.SocialAnxiety;
				if (num != 0)
				{
					if (num == 1)
					{
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("4060", "4061"));
					}
					if (Game1.random.NextDouble() < 0.33)
					{
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4060");
					}
					if (!Game1.random.NextBool())
					{
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4072");
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4071", nameToGreet);
				}
				else
				{
					if (Game1.random.NextDouble() < 0.33)
					{
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4062");
					}
					if (!Game1.random.NextBool())
					{
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4068", nameToGreet);
					}
					return ((Game1.timeOfDay < 1200) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4063") : ((Game1.timeOfDay < 1700) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4064") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4065"))) + ", " + Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4066", nameToGreet);
				}
			}
		}

		// Token: 0x060011FE RID: 4606 RVA: 0x000D327C File Offset: 0x000D147C
		public bool isFacingToward(Vector2 tileLocation)
		{
			switch (this.FacingDirection)
			{
			case 0:
				return (float)base.TilePoint.Y > tileLocation.Y;
			case 1:
				return (float)base.TilePoint.X < tileLocation.X;
			case 2:
				return (float)base.TilePoint.Y < tileLocation.Y;
			case 3:
				return (float)base.TilePoint.X > tileLocation.X;
			default:
				return false;
			}
		}

		// Token: 0x060011FF RID: 4607 RVA: 0x000D3300 File Offset: 0x000D1500
		public virtual void arriveAt(GameLocation l)
		{
			string rawText;
			if (!Game1.eventUp && Game1.random.NextBool() && this.Dialogue != null && this.Dialogue.TryGetValue(l.name.Value + "_Entry", out rawText))
			{
				this.showTextAboveHead(Game1.random.Choose(rawText.Split('/', StringSplitOptions.None)), null, 2, 3000, 0);
			}
		}

		// Token: 0x06001200 RID: 4608 RVA: 0x000D3378 File Offset: 0x000D1578
		public override void Halt()
		{
			base.Halt();
			this.shouldPlaySpousePatioAnimation.Value = false;
			this.isPlayingSleepingAnimation = false;
			this.isCharging = false;
			base.speed = 2;
			this.addedSpeed = 0f;
			if (this.isSleeping.Value)
			{
				this.playSleepingAnimation();
				this.Sprite.UpdateSourceRect();
			}
		}

		// Token: 0x06001201 RID: 4609 RVA: 0x000D33D5 File Offset: 0x000D15D5
		public void addExtraDialogue(Dialogue dialogue)
		{
			if (this.updatedDialogueYet)
			{
				if (dialogue != null)
				{
					this.CurrentDialogue.Push(dialogue);
					return;
				}
			}
			else
			{
				this.extraDialogueMessageToAddThisMorning = dialogue;
			}
		}

		// Token: 0x06001202 RID: 4610 RVA: 0x000D33F6 File Offset: 0x000D15F6
		public void PerformDivorce()
		{
			this.reloadDefaultLocation();
			Game1.warpCharacter(this, this.defaultMap.Value, this.DefaultPosition / 64f);
		}

		// Token: 0x06001203 RID: 4611 RVA: 0x000D3420 File Offset: 0x000D1620
		public Dialogue tryToGetMarriageSpecificDialogue(string dialogueKey)
		{
			Dictionary<string, string> marriageDialogues = null;
			string assetName = null;
			bool skip_married_dialogue = false;
			if (this.isRoommate())
			{
				try
				{
					assetName = "Characters\\Dialogue\\MarriageDialogue" + this.GetDialogueSheetName() + "Roommate";
					Dictionary<string, string> rawData = Game1.content.Load<Dictionary<string, string>>(assetName);
					if (rawData != null)
					{
						skip_married_dialogue = true;
						marriageDialogues = rawData;
						string rawText;
						if (marriageDialogues != null && marriageDialogues.TryGetValue(dialogueKey, out rawText))
						{
							return new Dialogue(this, assetName + ":" + dialogueKey, rawText);
						}
					}
				}
				catch (Exception)
				{
					assetName = null;
				}
			}
			if (!skip_married_dialogue)
			{
				try
				{
					assetName = "Characters\\Dialogue\\MarriageDialogue" + this.GetDialogueSheetName();
					marriageDialogues = Game1.content.Load<Dictionary<string, string>>(assetName);
				}
				catch (Exception)
				{
					assetName = null;
				}
			}
			string rawText2;
			if (marriageDialogues != null && marriageDialogues.TryGetValue(dialogueKey, out rawText2))
			{
				return new Dialogue(this, assetName + ":" + dialogueKey, rawText2);
			}
			assetName = "Characters\\Dialogue\\MarriageDialogue";
			marriageDialogues = Game1.content.Load<Dictionary<string, string>>(assetName);
			if (this.isRoommate())
			{
				string key = dialogueKey + "Roommate";
				string rawText3;
				if (marriageDialogues != null && marriageDialogues.TryGetValue(key, out rawText3))
				{
					return new Dialogue(this, assetName + ":" + dialogueKey, rawText3);
				}
			}
			string rawText4;
			if (marriageDialogues != null && marriageDialogues.TryGetValue(dialogueKey, out rawText4))
			{
				return new Dialogue(this, assetName + ":" + dialogueKey, rawText4);
			}
			return null;
		}

		// Token: 0x06001204 RID: 4612 RVA: 0x000D356C File Offset: 0x000D176C
		public void resetCurrentDialogue()
		{
			this.CurrentDialogue = null;
			this.shouldSayMarriageDialogue.Value = false;
			this.currentMarriageDialogue.Clear();
		}

		// Token: 0x06001205 RID: 4613 RVA: 0x000D358C File Offset: 0x000D178C
		private Stack<Dialogue> loadCurrentDialogue()
		{
			this.updatedDialogueYet = true;
			Stack<Dialogue> currentDialogue = new Stack<Dialogue>();
			try
			{
				Friendship friends;
				int heartLevel = Game1.player.friendshipData.TryGetValue(base.Name, out friends) ? (friends.Points / 250) : 0;
				Random r = Utility.CreateDaySaveRandom(Game1.stats.DaysPlayed * 77U, (double)(2f + this.defaultPosition.X * 77f), (double)(this.defaultPosition.Y * 777f));
				if (base.currentLocation != null && base.currentLocation.IsGreenRainingHere())
				{
					Dialogue dialogue = null;
					if (Game1.year >= 2)
					{
						dialogue = this.TryGetDialogue("GreenRain_2");
					}
					if (dialogue == null)
					{
						dialogue = this.TryGetDialogue("GreenRain");
					}
					if (dialogue != null)
					{
						currentDialogue.Clear();
						currentDialogue.Push(dialogue);
						return currentDialogue;
					}
				}
				if (r.NextDouble() < 0.025 && heartLevel >= 1)
				{
					CharacterData npcData = this.GetData();
					string relativeName;
					string relativeTitle;
					if (((npcData != null) ? npcData.FriendsAndFamily : null) != null && Utility.TryGetRandom<string, string>(npcData.FriendsAndFamily, out relativeName, out relativeTitle, null))
					{
						NPC relative = Game1.getCharacterFromName(relativeName, true, false);
						string relativeDisplayName = ((relative != null) ? relative.displayName : null) ?? NPC.GetDisplayName(relativeName);
						CharacterData relativeData;
						bool relativeIsMale = (relative != null) ? (relative.gender.Value == Gender.Male) : (NPC.TryGetData(relativeName, out relativeData) && relativeData.Gender == Gender.Male);
						relativeTitle = TokenParser.ParseText(relativeTitle, null, null, null);
						if (string.IsNullOrWhiteSpace(relativeTitle))
						{
							relativeTitle = null;
						}
						IDictionary<string, string> npcGiftTastes = Game1.NPCGiftTastes;
						string rawGiftTasteData;
						if (npcGiftTastes.TryGetValue(relativeName, out rawGiftTasteData))
						{
							string[] rawGiftTasteFields = rawGiftTasteData.Split('/', StringSplitOptions.None);
							string item = null;
							string itemName = null;
							string nameAndTitle = (relativeTitle != null && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.ja) ? (relativeIsMale ? Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4079", new object[]
							{
								relativeTitle
							}) : Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4080", new object[]
							{
								relativeTitle
							})) : relativeDisplayName;
							string message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4083", nameAndTitle);
							if (r.NextBool())
							{
								int tries = 0;
								string[] lovedItems = ArgUtility.SplitBySpace(ArgUtility.Get(rawGiftTasteFields, 1, null, true));
								while ((item == null || item.StartsWith("-")) && tries < 30)
								{
									item = r.Choose(lovedItems);
									tries++;
								}
								if (base.Name == "Penny" && relativeName == "Pam")
								{
									while (item == "303" || item == "346" || item == "348" || item == "459")
									{
										item = r.Choose(lovedItems);
									}
								}
								if (item != null)
								{
									ParsedItemData itemData = ItemRegistry.GetData(item);
									if (itemData != null)
									{
										itemName = itemData.DisplayName;
										message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4084", itemName);
										if (this.Age == 2)
										{
											message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4086", nameAndTitle, itemName) + (relativeIsMale ? Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4088") : Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4089"));
										}
										else
										{
											switch (r.Next(5))
											{
											case 0:
												message = Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4091", new object[]
												{
													nameAndTitle,
													itemName
												});
												break;
											case 1:
												message = (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4094", nameAndTitle, itemName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4097", nameAndTitle, itemName));
												break;
											case 2:
												message = (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4100", nameAndTitle, itemName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4103", nameAndTitle, itemName));
												break;
											case 3:
												message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4106", nameAndTitle, itemName);
												break;
											}
											if (r.NextDouble() < 0.65)
											{
												switch (r.Next(5))
												{
												case 0:
													message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4109") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4111"));
													break;
												case 1:
													message += (relativeIsMale ? (r.NextBool() ? Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4113") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4114")) : (r.NextBool() ? Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4115") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4116")));
													break;
												case 2:
													message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4118") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4120"));
													break;
												case 3:
													message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4125");
													break;
												case 4:
													message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4126") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4128"));
													break;
												}
												if (relativeName.Equals("Abigail") && r.NextBool())
												{
													message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4128", relativeDisplayName, itemName);
												}
											}
										}
									}
								}
							}
							else
							{
								string[] hatedItems = ArgUtility.SplitBySpace(ArgUtility.Get(rawGiftTasteFields, 7, null, true));
								if (hatedItems.Count<string>() > 0)
								{
									int tries2 = 0;
									while ((item == null || item.StartsWith("-")) && tries2 < 30)
									{
										item = r.Choose(hatedItems);
										tries2++;
									}
								}
								if (item == null)
								{
									int tries3 = 0;
									while ((item == null || item.StartsWith("-")) && tries3 < 30)
									{
										item = r.Choose(ArgUtility.SplitBySpace(npcGiftTastes["Universal_Hate"]));
										tries3++;
									}
								}
								if (item != null)
								{
									ParsedItemData itemData2 = ItemRegistry.GetData(item);
									if (itemData2 != null)
									{
										itemName = itemData2.DisplayName;
										message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4135", itemName, Lexicon.getRandomNegativeFoodAdjective(null)) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4138", itemName, Lexicon.getRandomNegativeFoodAdjective(null)));
										if (this.Age == 2)
										{
											message = (relativeIsMale ? Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4141", new object[]
											{
												relativeDisplayName,
												itemName
											}) : Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4144", new object[]
											{
												relativeDisplayName,
												itemName
											}));
										}
										else
										{
											switch (r.Next(4))
											{
											case 0:
												message = (r.NextBool() ? Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4146") : "") + Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4147", new object[]
												{
													nameAndTitle,
													itemName
												});
												break;
											case 1:
												message = (relativeIsMale ? (r.NextBool() ? Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4149", new object[]
												{
													nameAndTitle,
													itemName
												}) : Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4152", new object[]
												{
													nameAndTitle,
													itemName
												})) : (r.NextBool() ? Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4153", new object[]
												{
													nameAndTitle,
													itemName
												}) : Game1.LoadStringByGender(this.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4154", new object[]
												{
													nameAndTitle,
													itemName
												})));
												break;
											case 2:
												message = (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4161", nameAndTitle, itemName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4164", nameAndTitle, itemName));
												break;
											}
											if (r.NextDouble() < 0.65)
											{
												switch (r.Next(5))
												{
												case 0:
													message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4170");
													break;
												case 1:
													message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4171");
													break;
												case 2:
													message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4172") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4174"));
													break;
												case 3:
													message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4176") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4178"));
													break;
												case 4:
													message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4180");
													break;
												}
												if (base.Name.Equals("Lewis") && r.NextBool())
												{
													message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4182", relativeDisplayName, itemName);
												}
											}
										}
									}
								}
							}
							if (itemName != null)
							{
								if (Game1.getCharacterFromName(relativeName, true, false) != null)
								{
									message = string.Concat(new string[]
									{
										message,
										"%revealtaste:",
										relativeName,
										":",
										item
									});
								}
								currentDialogue.Clear();
								if (message.Length > 0)
								{
									try
									{
										message = message.Substring(0, 1).ToUpper() + message.Substring(1, message.Length - 1);
									}
									catch (Exception)
									{
									}
								}
								currentDialogue.Push(new Dialogue(this, null, message));
								return currentDialogue;
							}
						}
					}
				}
				if (this.Dialogue != null && this.Dialogue.Count != 0)
				{
					currentDialogue.Clear();
					if (Game1.player.spouse != null && Game1.player.spouse == base.Name)
					{
						if (Game1.player.isEngaged())
						{
							Dictionary<string, string> engagementDialogue = Game1.content.Load<Dictionary<string, string>>("Data\\EngagementDialogue");
							if (Game1.player.hasCurrentOrPendingRoommate() && engagementDialogue.ContainsKey(base.Name + "Roommate0"))
							{
								currentDialogue.Push(new Dialogue(this, "Data\\EngagementDialogue:" + base.Name + "Roommate" + r.Next(2).ToString(), false));
							}
							else if (engagementDialogue.ContainsKey(base.Name + "0"))
							{
								currentDialogue.Push(new Dialogue(this, "Data\\EngagementDialogue:" + base.Name + r.Next(2).ToString(), false));
							}
						}
						else if (!Game1.newDay && this.marriageDefaultDialogue.Value != null && !this.shouldSayMarriageDialogue.Value)
						{
							currentDialogue.Push(this.marriageDefaultDialogue.Value.GetDialogue(this));
							this.marriageDefaultDialogue.Value = null;
						}
					}
					else
					{
						Friendship friendship;
						if (Game1.player.friendshipData.TryGetValue(base.Name, out friendship) && friendship.IsDivorced())
						{
							Dialogue dialogue2 = StardewValley.Dialogue.TryGetDialogue(this, "Characters\\Dialogue\\" + this.GetDialogueSheetName() + ":divorced");
							if (dialogue2 != null)
							{
								currentDialogue.Push(dialogue2);
								return currentDialogue;
							}
						}
						if (Game1.isRaining && r.NextBool() && (base.currentLocation == null || base.currentLocation.InValleyContext()) && (!base.Name.Equals("Krobus") || !(Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == "Fri")) && (!base.Name.Equals("Penny") || !Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade")) && (!base.Name.Equals("Emily") || !Game1.IsFall || Game1.dayOfMonth != 15))
						{
							Dialogue dialogue3 = StardewValley.Dialogue.TryGetDialogue(this, "Characters\\Dialogue\\rainy:" + this.GetDialogueSheetName());
							if (dialogue3 != null)
							{
								currentDialogue.Push(dialogue3);
								return currentDialogue;
							}
						}
						Dialogue d = this.tryToRetrieveDialogue(Game1.currentSeason + "_", heartLevel, "");
						if (d == null)
						{
							d = this.tryToRetrieveDialogue("", heartLevel, "");
						}
						if (d != null)
						{
							currentDialogue.Push(d);
						}
					}
				}
				else if (base.Name.Equals("Bouncer"))
				{
					currentDialogue.Push(new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4192", false));
				}
				if (this.extraDialogueMessageToAddThisMorning != null)
				{
					currentDialogue.Push(this.extraDialogueMessageToAddThisMorning);
				}
			}
			catch (Exception ex)
			{
				Game1.log.Error("NPC '" + base.Name + "' failed loading their current dialogue.", ex);
			}
			return currentDialogue;
		}

		// Token: 0x06001206 RID: 4614 RVA: 0x000D4294 File Offset: 0x000D2494
		public bool checkForNewCurrentDialogue(int heartLevel, bool noPreface = false)
		{
			if (Game1.IsGreenRainingHere(null))
			{
				return false;
			}
			foreach (string eventMessageKey in Game1.player.activeDialogueEvents.Keys)
			{
				if (!(eventMessageKey == ""))
				{
					Dialogue dialogue = this.TryGetDialogue(eventMessageKey);
					if (dialogue != null)
					{
						string mailKey = base.Name + "_" + eventMessageKey;
						if (dialogue != null && !Game1.player.mailReceived.Contains(mailKey))
						{
							this.CurrentDialogue.Clear();
							this.CurrentDialogue.Push(dialogue);
							if (!eventMessageKey.Contains("dumped"))
							{
								Game1.player.mailReceived.Add(mailKey);
							}
							return true;
						}
					}
				}
			}
			string preface = (Game1.season != Season.Spring && !noPreface) ? Game1.currentSeason : "";
			string[] array = new string[6];
			array[0] = preface;
			array[1] = Game1.currentLocation.name.Value;
			array[2] = "_";
			int num = 3;
			Point tilePoint = base.TilePoint;
			array[num] = tilePoint.X.ToString();
			array[4] = "_";
			int num2 = 5;
			tilePoint = base.TilePoint;
			array[num2] = tilePoint.Y.ToString();
			Dialogue dialogue2 = this.TryGetDialogue(string.Concat(array)) ?? this.TryGetDialogue(preface + Game1.currentLocation.name.Value + "_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth));
			int hearts = 10;
			while (dialogue2 == null && hearts >= 2)
			{
				if (heartLevel >= hearts)
				{
					dialogue2 = this.TryGetDialogue(preface + Game1.currentLocation.name.Value + hearts.ToString());
				}
				hearts -= 2;
			}
			dialogue2 = (dialogue2 ?? this.TryGetDialogue(preface + Game1.currentLocation.Name));
			if (dialogue2 != null)
			{
				dialogue2.removeOnNextMove = true;
				this.CurrentDialogue.Push(dialogue2);
				return true;
			}
			return false;
		}

		// Token: 0x06001207 RID: 4615 RVA: 0x000D44A8 File Offset: 0x000D26A8
		public Dialogue TryGetDialogue(string key)
		{
			Dictionary<string, string> dialogue = this.Dialogue;
			string text;
			if (dialogue != null && dialogue.TryGetValue(key, out text))
			{
				return new Dialogue(this, this.LoadedDialogueKey + ":" + key, text);
			}
			return null;
		}

		// Token: 0x06001208 RID: 4616 RVA: 0x000D44E4 File Offset: 0x000D26E4
		public Dialogue TryGetDialogueByGiftTaste(int giftTaste, Func<string, string> getKey)
		{
			switch (giftTaste)
			{
			case 0:
			case 7:
				return this.TryGetDialogue(getKey("Loved")) ?? this.TryGetDialogue(getKey("Positive"));
			case 2:
				return this.TryGetDialogue(getKey("Liked")) ?? this.TryGetDialogue(getKey("Positive"));
			case 4:
				return this.TryGetDialogue(getKey("Disliked")) ?? this.TryGetDialogue(getKey("Negative"));
			case 6:
				return this.TryGetDialogue(getKey("Hated")) ?? this.TryGetDialogue(getKey("Negative"));
			}
			return this.TryGetDialogue(getKey("Neutral")) ?? this.TryGetDialogue(getKey("Positive"));
		}

		// Token: 0x06001209 RID: 4617 RVA: 0x000D45E0 File Offset: 0x000D27E0
		public Dialogue TryGetDialogue(string key, params object[] substitutions)
		{
			Dictionary<string, string> dialogue = this.Dialogue;
			string text;
			if (dialogue != null && dialogue.TryGetValue(key, out text))
			{
				return new Dialogue(this, this.LoadedDialogueKey + ":" + key, string.Format(text, substitutions));
			}
			return null;
		}

		// Token: 0x0600120A RID: 4618 RVA: 0x000D4624 File Offset: 0x000D2824
		public Dialogue tryToRetrieveDialogue(string preface, int heartLevel, string appendToEnd = "")
		{
			int year = Game1.year;
			if (Game1.year > 2)
			{
				year = 2;
			}
			if (!string.IsNullOrEmpty(Game1.player.spouse) && appendToEnd.Equals(""))
			{
				if (Game1.player.hasCurrentOrPendingRoommate())
				{
					Dialogue s = this.tryToRetrieveDialogue(preface, heartLevel, "_roommate_" + Game1.player.spouse);
					if (s != null)
					{
						return s;
					}
				}
				else
				{
					Dialogue s2 = this.tryToRetrieveDialogue(preface, heartLevel, "_inlaw_" + Game1.player.spouse);
					if (s2 != null)
					{
						return s2;
					}
				}
			}
			string day_name = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
			if (year == 1)
			{
				Dialogue dialogue = this.TryGetDialogue(preface + Game1.dayOfMonth.ToString() + appendToEnd);
				if (dialogue != null)
				{
					return dialogue;
				}
			}
			Dialogue dialogue2 = this.TryGetDialogue(string.Concat(new string[]
			{
				preface,
				Game1.dayOfMonth.ToString(),
				"_",
				year.ToString(),
				appendToEnd
			}));
			if (dialogue2 != null)
			{
				return dialogue2;
			}
			Dialogue dialogue3 = this.TryGetDialogue(preface + Game1.dayOfMonth.ToString() + "_*" + appendToEnd);
			if (dialogue3 != null)
			{
				return dialogue3;
			}
			for (int hearts = 10; hearts >= 2; hearts -= 2)
			{
				if (heartLevel >= hearts)
				{
					Dialogue dialogue4 = this.TryGetDialogue(string.Concat(new string[]
					{
						preface,
						day_name,
						hearts.ToString(),
						"_",
						year.ToString(),
						appendToEnd
					})) ?? this.TryGetDialogue(preface + day_name + hearts.ToString() + appendToEnd);
					if (dialogue4 != null)
					{
						if (hearts == 4 && preface == "fall_" && day_name == "Mon" && base.Name.Equals("Penny") && Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
						{
							return this.TryGetDialogue(string.Concat(new string[]
							{
								preface,
								day_name,
								"_",
								year.ToString(),
								appendToEnd
							})) ?? this.TryGetDialogue("fall_Mon");
						}
						return dialogue4;
					}
				}
			}
			Dialogue dialogue5 = this.TryGetDialogue(preface + day_name + appendToEnd);
			if (dialogue5 != null)
			{
				Dialogue specificDialogue = this.TryGetDialogue(string.Concat(new string[]
				{
					preface,
					day_name,
					"_",
					year.ToString(),
					appendToEnd
				}));
				if (specificDialogue != null)
				{
					dialogue5 = specificDialogue;
				}
			}
			if (dialogue5 != null && base.Name.Equals("Caroline") && Game1.isLocationAccessible("CommunityCenter") && preface == "summer_" && day_name == "Mon")
			{
				dialogue5 = this.TryGetDialogue("summer_Wed");
			}
			if (dialogue5 != null)
			{
				return dialogue5;
			}
			return null;
		}

		// Token: 0x0600120B RID: 4619 RVA: 0x000D48F0 File Offset: 0x000D2AF0
		public virtual void checkSchedule(int timeOfDay)
		{
			if (this.currentScheduleDelay == 0f && this.scheduleDelaySeconds > 0f)
			{
				this.currentScheduleDelay = this.scheduleDelaySeconds;
				return;
			}
			if (this.returningToEndPoint)
			{
				return;
			}
			this.updatedDialogueYet = false;
			this.extraDialogueMessageToAddThisMorning = null;
			if (this.ignoreScheduleToday)
			{
				return;
			}
			if (this.Schedule != null)
			{
				SchedulePathDescription possibleNewDirections = null;
				if (this.lastAttemptedSchedule < timeOfDay)
				{
					this.lastAttemptedSchedule = timeOfDay;
					this.Schedule.TryGetValue(timeOfDay, out possibleNewDirections);
					if (possibleNewDirections != null)
					{
						this.queuedSchedulePaths.Add(possibleNewDirections);
					}
					possibleNewDirections = null;
				}
				PathFindController controller = this.controller;
				bool flag;
				if (controller == null)
				{
					flag = false;
				}
				else
				{
					Stack<Point> pathToEndPoint = controller.pathToEndPoint;
					int? num = (pathToEndPoint != null) ? new int?(pathToEndPoint.Count) : null;
					int num2 = 0;
					flag = (num.GetValueOrDefault() > num2 & num != null);
				}
				if (flag)
				{
					return;
				}
				if (this.queuedSchedulePaths.Count > 0 && timeOfDay >= this.queuedSchedulePaths[0].time)
				{
					possibleNewDirections = this.queuedSchedulePaths[0];
				}
				if (possibleNewDirections != null)
				{
					this.prepareToDisembarkOnNewSchedulePath();
					if (this.returningToEndPoint)
					{
						return;
					}
					if (this.temporaryController != null)
					{
						return;
					}
					this.directionsToNewLocation = possibleNewDirections;
					if (this.queuedSchedulePaths.Count > 0)
					{
						this.queuedSchedulePaths.RemoveAt(0);
					}
					this.controller = new PathFindController(this.directionsToNewLocation.route, this, Utility.getGameLocationOfCharacter(this))
					{
						finalFacingDirection = this.directionsToNewLocation.facingDirection,
						endBehaviorFunction = this.getRouteEndBehaviorFunction(this.directionsToNewLocation.endOfRouteBehavior, this.directionsToNewLocation.endOfRouteMessage)
					};
					if (this.controller.pathToEndPoint == null || this.controller.pathToEndPoint.Count == 0)
					{
						PathFindController.endBehavior endBehaviorFunction = this.controller.endBehaviorFunction;
						if (endBehaviorFunction != null)
						{
							endBehaviorFunction(this, base.currentLocation);
						}
						this.controller = null;
					}
					SchedulePathDescription schedulePathDescription = this.directionsToNewLocation;
					if (((schedulePathDescription != null) ? schedulePathDescription.route : null) != null)
					{
						this.previousEndPoint = this.directionsToNewLocation.route.LastOrDefault<Point>();
					}
				}
			}
		}

		// Token: 0x0600120C RID: 4620 RVA: 0x000D4AF0 File Offset: 0x000D2CF0
		private void finishEndOfRouteAnimation()
		{
			this._finishingEndOfRouteBehavior = this._startedEndOfRouteBehavior;
			this._startedEndOfRouteBehavior = null;
			string finishingEndOfRouteBehavior = this._finishingEndOfRouteBehavior;
			if (!(finishingEndOfRouteBehavior == "change_beach"))
			{
				if (finishingEndOfRouteBehavior == "change_normal")
				{
					this.shouldWearIslandAttire.Value = false;
					this.currentlyDoingEndOfRouteAnimation = false;
				}
			}
			else
			{
				this.shouldWearIslandAttire.Value = true;
				this.currentlyDoingEndOfRouteAnimation = false;
			}
			while (this.CurrentDialogue.Count > 0 && this.CurrentDialogue.Peek().removeOnNextMove)
			{
				this.CurrentDialogue.Pop();
			}
			this.shouldSayMarriageDialogue.Value = false;
			this.currentMarriageDialogue.Clear();
			this.nextEndOfRouteMessage = null;
			this.endOfRouteMessage.Value = null;
			if (this.currentlyDoingEndOfRouteAnimation && this.routeEndOutro != null)
			{
				bool addedFrame = false;
				for (int i = 0; i < this.routeEndOutro.Length; i++)
				{
					if (!addedFrame)
					{
						this.Sprite.ClearAnimation();
						addedFrame = true;
					}
					if (i == this.routeEndOutro.Length - 1)
					{
						this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(this.routeEndOutro[i], 100, 0, false, false, new AnimatedSprite.endOfAnimationBehavior(this.routeEndAnimationFinished), true, 0));
					}
					else
					{
						this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(this.routeEndOutro[i], 100, 0, false, false, null, false, 0));
					}
				}
				if (!addedFrame)
				{
					this.routeEndAnimationFinished(null);
				}
				if (this._finishingEndOfRouteBehavior != null)
				{
					this.finishRouteBehavior(this._finishingEndOfRouteBehavior);
					return;
				}
			}
			else
			{
				this.routeEndAnimationFinished(null);
			}
		}

		// Token: 0x0600120D RID: 4621 RVA: 0x000D4C70 File Offset: 0x000D2E70
		protected virtual void prepareToDisembarkOnNewSchedulePath()
		{
			this.finishEndOfRouteAnimation();
			this.doingEndOfRouteAnimation.Value = false;
			this.currentlyDoingEndOfRouteAnimation = false;
			if (this.isMarried())
			{
				if (this.temporaryController == null && Utility.getGameLocationOfCharacter(this) is FarmHouse)
				{
					this.temporaryController = new PathFindController(this, this.getHome(), new Point(this.getHome().warps[0].X, this.getHome().warps[0].Y), 2, true)
					{
						NPCSchedule = true
					};
					if (this.temporaryController.pathToEndPoint == null || this.temporaryController.pathToEndPoint.Count <= 0)
					{
						this.temporaryController = null;
						this.ClearSchedule();
						return;
					}
					this.followSchedule = true;
					return;
				}
				else if (Utility.getGameLocationOfCharacter(this) is Farm)
				{
					this.temporaryController = null;
					this.ClearSchedule();
				}
			}
		}

		// Token: 0x0600120E RID: 4622 RVA: 0x000D4D58 File Offset: 0x000D2F58
		public void checkForMarriageDialogue(int timeOfDay, GameLocation location)
		{
			if (base.Name == "Krobus" && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == "Fri")
			{
				return;
			}
			if (timeOfDay == 1100)
			{
				this.setRandomAfternoonMarriageDialogue(1100, location, false);
				return;
			}
			if (timeOfDay != 1800)
			{
				return;
			}
			if (location is FarmHouse)
			{
				int which = Utility.CreateDaySaveRandom((double)timeOfDay, (double)this.getSpouse().UniqueMultiplayerID, 0.0).Next(Game1.isRaining ? 7 : 6) - 1;
				string suffix = (which >= 0) ? (which.ToString() ?? "") : base.Name;
				this.currentMarriageDialogue.Clear();
				this.addMarriageDialogue("MarriageDialogue", (Game1.isRaining ? "Rainy" : "Indoor") + "_Night_" + suffix, false, Array.Empty<string>());
			}
		}

		// Token: 0x0600120F RID: 4623 RVA: 0x000D4E40 File Offset: 0x000D3040
		private void routeEndAnimationFinished(Farmer who)
		{
			this.doingEndOfRouteAnimation.Value = false;
			this.freezeMotion = false;
			CharacterData data = this.GetData();
			this.Sprite.SpriteWidth = ((data != null) ? data.Size.X : 16);
			this.Sprite.SpriteHeight = ((data != null) ? data.Size.Y : 32);
			this.Sprite.UpdateSourceRect();
			this.Sprite.oldFrame = this._beforeEndOfRouteAnimationFrame;
			this.Sprite.StopAnimation();
			this.endOfRouteMessage.Value = null;
			this.isCharging = false;
			base.speed = 2;
			this.addedSpeed = 0f;
			this.goingToDoEndOfRouteAnimation.Value = false;
			if (this.isWalkingInSquare)
			{
				this.returningToEndPoint = true;
			}
			if (this._finishingEndOfRouteBehavior == "penny_dishes")
			{
				this.drawOffset = Vector2.Zero;
			}
			if (this.appliedRouteAnimationOffset != Vector2.Zero)
			{
				this.drawOffset = Vector2.Zero;
				this.appliedRouteAnimationOffset = Vector2.Zero;
			}
			this._finishingEndOfRouteBehavior = null;
		}

		// Token: 0x06001210 RID: 4624 RVA: 0x000D4F54 File Offset: 0x000D3154
		public bool isOnSilentTemporaryMessage()
		{
			return (this.doingEndOfRouteAnimation.Value || !this.goingToDoEndOfRouteAnimation.Value) && this.endOfRouteMessage.Value != null && this.endOfRouteMessage.Value.EqualsIgnoreCase("silent");
		}

		// Token: 0x06001211 RID: 4625 RVA: 0x000D4FA4 File Offset: 0x000D31A4
		public bool hasTemporaryMessageAvailable()
		{
			return !this.isDivorcedFrom(Game1.player) && ((base.currentLocation != null && base.currentLocation.HasLocationOverrideDialogue(this)) || (this.endOfRouteMessage.Value != null && (this.doingEndOfRouteAnimation.Value || !this.goingToDoEndOfRouteAnimation.Value)));
		}

		// Token: 0x06001212 RID: 4626 RVA: 0x000D5004 File Offset: 0x000D3204
		public bool setTemporaryMessages(Farmer who)
		{
			if (this.isOnSilentTemporaryMessage())
			{
				return true;
			}
			if (this.endOfRouteMessage.Value != null && (this.doingEndOfRouteAnimation.Value || !this.goingToDoEndOfRouteAnimation.Value))
			{
				if (!this.isDivorcedFrom(Game1.player) && (!this.endOfRouteMessage.Value.Contains("marriage") || this.getSpouse() == Game1.player))
				{
					this._PushTemporaryDialogue(this.endOfRouteMessage.Value);
					return false;
				}
			}
			else if (base.currentLocation != null && base.currentLocation.HasLocationOverrideDialogue(this))
			{
				this._PushTemporaryDialogue(base.currentLocation.GetLocationOverrideDialogue(this));
				return false;
			}
			return false;
		}

		// Token: 0x06001213 RID: 4627 RVA: 0x000D50B4 File Offset: 0x000D32B4
		protected void _PushTemporaryDialogue(string translationKey)
		{
			string oldTranslationKey = translationKey;
			try
			{
				Friendship friendship;
				if (Game1.player.friendshipData.TryGetValue(base.Name, out friendship))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
					defaultInterpolatedStringHandler.AppendFormatted(translationKey);
					defaultInterpolatedStringHandler.AppendLiteral("_");
					defaultInterpolatedStringHandler.AppendFormatted<FriendshipStatus>(friendship.Status);
					string relationshipKey = defaultInterpolatedStringHandler.ToStringAndClear();
					if (Game1.content.LoadStringReturnNullIfNotFound(relationshipKey, true) != null)
					{
						translationKey = relationshipKey;
					}
				}
				if (this.CurrentDialogue.Count == 0 || this.CurrentDialogue.Peek().temporaryDialogueKey != translationKey)
				{
					Dialogue temporaryDialogue = new Dialogue(this, translationKey, false)
					{
						removeOnNextMove = true,
						temporaryDialogueKey = translationKey
					};
					this.CurrentDialogue.Push(temporaryDialogue);
				}
			}
			catch (Exception ex)
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(47, 3);
				defaultInterpolatedStringHandler.AppendLiteral("NPC '");
				defaultInterpolatedStringHandler.AppendFormatted(base.Name);
				defaultInterpolatedStringHandler.AppendLiteral("' failed setting temporary dialogue key '");
				defaultInterpolatedStringHandler.AppendFormatted(translationKey);
				defaultInterpolatedStringHandler.AppendLiteral("'");
				defaultInterpolatedStringHandler.AppendFormatted((translationKey != oldTranslationKey) ? (" (from dialogue key '" + oldTranslationKey + "')") : "");
				log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), ex);
			}
		}

		// Token: 0x06001214 RID: 4628 RVA: 0x000D5200 File Offset: 0x000D3400
		private void walkInSquareAtEndOfRoute(Character c, GameLocation l)
		{
			this.startRouteBehavior(this.endOfRouteBehaviorName.Value);
		}

		// Token: 0x06001215 RID: 4629 RVA: 0x000D5213 File Offset: 0x000D3413
		private void doAnimationAtEndOfScheduleRoute(Character c, GameLocation l)
		{
			this.doingEndOfRouteAnimation.Value = true;
			this.reallyDoAnimationAtEndOfScheduleRoute();
			this.currentlyDoingEndOfRouteAnimation = true;
		}

		// Token: 0x06001216 RID: 4630 RVA: 0x000D5230 File Offset: 0x000D3430
		private void reallyDoAnimationAtEndOfScheduleRoute()
		{
			this._startedEndOfRouteBehavior = this.loadedEndOfRouteBehavior;
			bool is_special_route_behavior = false;
			string a = this._startedEndOfRouteBehavior;
			if (a == "change_beach" || a == "change_normal")
			{
				is_special_route_behavior = true;
			}
			if (!is_special_route_behavior)
			{
				if (this._startedEndOfRouteBehavior == "penny_dishes")
				{
					this.drawOffset = new Vector2(0f, 16f);
				}
				if (this._startedEndOfRouteBehavior.EndsWith("_sleep"))
				{
					this.layingDown = true;
					this.HideShadow = true;
				}
				if (this.routeAnimationMetadata != null)
				{
					for (int i = 0; i < this.routeAnimationMetadata.Length; i++)
					{
						string[] metadata = ArgUtility.SplitBySpace(this.routeAnimationMetadata[i]);
						a = metadata[0];
						if (!(a == "laying_down"))
						{
							if (a == "offset")
							{
								this.appliedRouteAnimationOffset = new Vector2((float)int.Parse(metadata[1]), (float)int.Parse(metadata[2]));
							}
						}
						else
						{
							this.layingDown = true;
							this.HideShadow = true;
						}
					}
				}
				if (this.appliedRouteAnimationOffset != Vector2.Zero)
				{
					this.drawOffset = this.appliedRouteAnimationOffset;
				}
				if (this._skipRouteEndIntro)
				{
					this.doMiddleAnimation(null);
				}
				else
				{
					this.Sprite.ClearAnimation();
					for (int j = 0; j < this.routeEndIntro.Length; j++)
					{
						if (j == this.routeEndIntro.Length - 1)
						{
							this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(this.routeEndIntro[j], 100, 0, false, false, new AnimatedSprite.endOfAnimationBehavior(this.doMiddleAnimation), true, 0));
						}
						else
						{
							this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(this.routeEndIntro[j], 100, 0, false, false, null, false, 0));
						}
					}
				}
			}
			this._skipRouteEndIntro = false;
			this.doingEndOfRouteAnimation.Value = true;
			this.freezeMotion = true;
			this._beforeEndOfRouteAnimationFrame = this.Sprite.oldFrame;
		}

		// Token: 0x06001217 RID: 4631 RVA: 0x000D540C File Offset: 0x000D360C
		private void doMiddleAnimation(Farmer who)
		{
			this.Sprite.ClearAnimation();
			for (int i = 0; i < this.routeEndAnimation.Length; i++)
			{
				this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(this.routeEndAnimation[i], 100, 0, false, false, null, false, 0));
			}
			this.Sprite.loop = true;
			if (this._startedEndOfRouteBehavior != null)
			{
				this.startRouteBehavior(this._startedEndOfRouteBehavior);
			}
		}

		// Token: 0x06001218 RID: 4632 RVA: 0x000D5478 File Offset: 0x000D3678
		private void startRouteBehavior(string behaviorName)
		{
			if (behaviorName.Length > 0 && behaviorName[0] == '"')
			{
				if (Game1.IsMasterGame)
				{
					this.endOfRouteMessage.Value = behaviorName.Replace("\"", "");
					return;
				}
			}
			else
			{
				if (behaviorName.Contains("square_") && Game1.IsMasterGame)
				{
					this.lastCrossroad = new Microsoft.Xna.Framework.Rectangle(base.TilePoint.X * 64, base.TilePoint.Y * 64, 64, 64);
					string[] squareSplit = behaviorName.Split('_', StringSplitOptions.None);
					this.walkInSquare(Convert.ToInt32(squareSplit[1]), Convert.ToInt32(squareSplit[2]), 6000);
					if (squareSplit.Length > 3)
					{
						this.squareMovementFacingPreference = Convert.ToInt32(squareSplit[3]);
					}
					else
					{
						this.squareMovementFacingPreference = -1;
					}
				}
				if (behaviorName.Contains("sleep"))
				{
					this.isPlayingSleepingAnimation = true;
					this.playSleepingAnimation();
				}
				if (!(behaviorName == "abigail_videogames"))
				{
					if (!(behaviorName == "dick_fish"))
					{
						if (behaviorName == "clint_hammer")
						{
							base.extendSourceRect(16, 0, true);
							this.Sprite.SpriteWidth = 32;
							this.Sprite.ignoreSourceRectUpdates = false;
							this.Sprite.currentFrame = 8;
							this.Sprite.CurrentAnimation[14] = new FarmerSprite.AnimationFrame(9, 100, 0, false, false, new AnimatedSprite.endOfAnimationBehavior(this.clintHammerSound), false, 0);
							return;
						}
						if (!(behaviorName == "birdie_fish"))
						{
							return;
						}
						base.extendSourceRect(16, 0, true);
						this.Sprite.SpriteWidth = 32;
						this.Sprite.ignoreSourceRectUpdates = false;
						this.Sprite.currentFrame = 8;
					}
					else
					{
						base.extendSourceRect(0, 32, true);
						this.Sprite.tempSpriteHeight = 64;
						this.drawOffset = new Vector2(0f, 96f);
						this.Sprite.ignoreSourceRectUpdates = false;
						if (Utility.isOnScreen(Utility.Vector2ToPoint(base.Position), 64, base.currentLocation))
						{
							base.currentLocation.playSound("slosh", new Vector2?(base.Tile), null, SoundContext.Default);
							return;
						}
					}
				}
				else if (Game1.IsMasterGame)
				{
					Game1.multiplayer.broadcastSprites(Utility.getGameLocationOfCharacter(this), new TemporaryAnimatedSprite[]
					{
						new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(167, 1714, 19, 14), 100f, 3, 999999, new Vector2(2f, 3f) * 64f + new Vector2(7f, 12f) * 4f, false, false, 0.0002f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
						{
							id = 688
						}
					});
					base.doEmote(52, true);
					return;
				}
			}
		}

		// Token: 0x06001219 RID: 4633 RVA: 0x000D575C File Offset: 0x000D395C
		public void playSleepingAnimation()
		{
			this.isSleeping.Value = true;
			Vector2 draw_offset = new Vector2(0f, (float)(this.name.Equals("Sebastian") ? 12 : -4));
			if (this.isMarried())
			{
				draw_offset.X = -12f;
			}
			this.drawOffset = draw_offset;
			if (!this.isPlayingSleepingAnimation)
			{
				string animationData;
				if (DataLoader.AnimationDescriptions(Game1.content).TryGetValue(this.name.Value.ToLower() + "_sleep", out animationData))
				{
					int sleep_frame = Convert.ToInt32(animationData.Split('/', StringSplitOptions.None)[0]);
					this.Sprite.ClearAnimation();
					this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(sleep_frame, 100, false, false, null, false));
					this.Sprite.loop = true;
				}
				this.isPlayingSleepingAnimation = true;
			}
		}

		// Token: 0x0600121A RID: 4634 RVA: 0x000D5830 File Offset: 0x000D3A30
		private void finishRouteBehavior(string behaviorName)
		{
			if (!(behaviorName == "abigail_videogames"))
			{
				if (behaviorName == "birdie_fish" || behaviorName == "clint_hammer" || behaviorName == "dick_fish")
				{
					this.reloadSprite(false);
					CharacterData data = this.GetData();
					this.Sprite.SpriteWidth = ((data != null) ? data.Size.X : 16);
					this.Sprite.SpriteHeight = ((data != null) ? data.Size.Y : 32);
					this.Sprite.UpdateSourceRect();
					this.drawOffset = Vector2.Zero;
					this.Halt();
					this.movementPause = 1;
				}
			}
			else
			{
				Utility.getGameLocationOfCharacter(this).removeTemporarySpritesWithID(688);
			}
			if (this.layingDown)
			{
				this.layingDown = false;
				this.HideShadow = false;
			}
		}

		// Token: 0x0600121B RID: 4635 RVA: 0x000D5906 File Offset: 0x000D3B06
		public bool IsReturningToEndPoint()
		{
			return this.returningToEndPoint;
		}

		// Token: 0x0600121C RID: 4636 RVA: 0x000D5910 File Offset: 0x000D3B10
		public void StartActivityWalkInSquare(int square_width, int square_height, int pause_offset)
		{
			Point tile = base.TilePoint;
			this.lastCrossroad = new Microsoft.Xna.Framework.Rectangle(tile.X * 64, tile.Y * 64, 64, 64);
			this.walkInSquare(square_height, square_height, pause_offset);
		}

		// Token: 0x0600121D RID: 4637 RVA: 0x000D594E File Offset: 0x000D3B4E
		public void EndActivityRouteEndBehavior()
		{
			this.finishEndOfRouteAnimation();
		}

		// Token: 0x0600121E RID: 4638 RVA: 0x000D5956 File Offset: 0x000D3B56
		public void StartActivityRouteEndBehavior(string behavior_name, string end_message)
		{
			PathFindController.endBehavior routeEndBehaviorFunction = this.getRouteEndBehaviorFunction(behavior_name, end_message);
			if (routeEndBehaviorFunction == null)
			{
				return;
			}
			routeEndBehaviorFunction(this, base.currentLocation);
		}

		// Token: 0x0600121F RID: 4639 RVA: 0x000D5974 File Offset: 0x000D3B74
		protected PathFindController.endBehavior getRouteEndBehaviorFunction(string behaviorName, string endMessage)
		{
			if (endMessage != null || (behaviorName != null && behaviorName.Length > 0 && behaviorName[0] == '"'))
			{
				this.nextEndOfRouteMessage = endMessage.Replace("\"", "");
			}
			if (behaviorName == null)
			{
				return null;
			}
			if (behaviorName.Length > 0 && behaviorName.Contains("square_"))
			{
				this.endOfRouteBehaviorName.Value = behaviorName;
				return new PathFindController.endBehavior(this.walkInSquareAtEndOfRoute);
			}
			Dictionary<string, string> animationDescriptions = DataLoader.AnimationDescriptions(Game1.content);
			if (behaviorName == "change_beach" || behaviorName == "change_normal")
			{
				this.endOfRouteBehaviorName.Value = behaviorName;
				this.goingToDoEndOfRouteAnimation.Value = true;
			}
			else
			{
				if (!animationDescriptions.ContainsKey(behaviorName))
				{
					return null;
				}
				this.endOfRouteBehaviorName.Value = behaviorName;
				this.loadEndOfRouteBehavior(this.endOfRouteBehaviorName.Value);
				this.goingToDoEndOfRouteAnimation.Value = true;
			}
			return new PathFindController.endBehavior(this.doAnimationAtEndOfScheduleRoute);
		}

		// Token: 0x06001220 RID: 4640 RVA: 0x000D5A6C File Offset: 0x000D3C6C
		private void loadEndOfRouteBehavior(string name)
		{
			this.loadedEndOfRouteBehavior = name;
			if (name.Length > 0 && name.Contains("square_"))
			{
				return;
			}
			string rawData = null;
			try
			{
				if (DataLoader.AnimationDescriptions(Game1.content).TryGetValue(name, out rawData))
				{
					string[] fields = rawData.Split('/', StringSplitOptions.None);
					this.routeEndIntro = Utility.parseStringToIntArray(fields[0], ' ');
					this.routeEndAnimation = Utility.parseStringToIntArray(fields[1], ' ');
					this.routeEndOutro = Utility.parseStringToIntArray(fields[2], ' ');
					if (fields.Length > 3 && fields[3] != "")
					{
						this.nextEndOfRouteMessage = fields[3];
					}
					if (fields.Length > 4)
					{
						this.routeAnimationMetadata = fields.Skip(4).ToArray<string>();
					}
					else
					{
						this.routeAnimationMetadata = null;
					}
				}
			}
			catch (Exception ex)
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(46, 3);
				defaultInterpolatedStringHandler.AppendLiteral("NPC ");
				defaultInterpolatedStringHandler.AppendFormatted(base.Name);
				defaultInterpolatedStringHandler.AppendLiteral(" failed to apply end-of-route behavior '");
				defaultInterpolatedStringHandler.AppendFormatted(name);
				defaultInterpolatedStringHandler.AppendLiteral("'");
				defaultInterpolatedStringHandler.AppendFormatted((rawData != null) ? (" with raw data '" + rawData + "'") : "");
				defaultInterpolatedStringHandler.AppendLiteral(".");
				log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), ex);
			}
		}

		// Token: 0x06001221 RID: 4641 RVA: 0x000D5BC0 File Offset: 0x000D3DC0
		public void shake(int duration)
		{
			this.shakeTimer = duration;
		}

		// Token: 0x06001222 RID: 4642 RVA: 0x000D5BC9 File Offset: 0x000D3DC9
		public void setNewDialogue(string translationKey, bool add = false, bool clearOnMovement = false)
		{
			this.setNewDialogue(new Dialogue(this, translationKey, false), add, clearOnMovement);
		}

		// Token: 0x06001223 RID: 4643 RVA: 0x000D5BDB File Offset: 0x000D3DDB
		public void setNewDialogue(Dialogue dialogue, bool add = false, bool clearOnMovement = false)
		{
			if (!add)
			{
				this.CurrentDialogue.Clear();
			}
			dialogue.removeOnNextMove = clearOnMovement;
			this.CurrentDialogue.Push(dialogue);
		}

		// Token: 0x06001224 RID: 4644 RVA: 0x000D5C00 File Offset: 0x000D3E00
		private void setNewDialogue(string dialogueSheetName, string dialogueSheetKey, bool clearOnMovement = false)
		{
			this.CurrentDialogue.Clear();
			string translationKey = dialogueSheetKey + base.Name;
			if (!dialogueSheetName.Contains("Marriage"))
			{
				string translationPath = "Characters\\Dialogue\\" + dialogueSheetName + ":" + translationKey;
				Dialogue dialogue = StardewValley.Dialogue.TryGetDialogue(this, translationPath);
				if (dialogue == null)
				{
					IGameLogger log = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(47, 2);
					defaultInterpolatedStringHandler.AppendLiteral("NPC '");
					defaultInterpolatedStringHandler.AppendFormatted(base.Name);
					defaultInterpolatedStringHandler.AppendLiteral("' couldn't set dialogue key '");
					defaultInterpolatedStringHandler.AppendFormatted(translationPath);
					defaultInterpolatedStringHandler.AppendLiteral("': not found.");
					log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
					dialogue = StardewValley.Dialogue.GetFallbackForError(this);
				}
				if (dialogue != null)
				{
					dialogue.removeOnNextMove = clearOnMovement;
					this.CurrentDialogue.Push(dialogue);
				}
				return;
			}
			if (this.getSpouse() != Game1.player)
			{
				return;
			}
			Dialogue dialogue2 = this.tryToGetMarriageSpecificDialogue(translationKey);
			if (dialogue2 == null)
			{
				IGameLogger log2 = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(56, 2);
				defaultInterpolatedStringHandler.AppendLiteral("NPC '");
				defaultInterpolatedStringHandler.AppendFormatted(base.Name);
				defaultInterpolatedStringHandler.AppendLiteral("' couldn't set marriage dialogue key '");
				defaultInterpolatedStringHandler.AppendFormatted(translationKey);
				defaultInterpolatedStringHandler.AppendLiteral("': not found.");
				log2.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
				dialogue2 = StardewValley.Dialogue.GetFallbackForError(this);
			}
			dialogue2.removeOnNextMove = clearOnMovement;
			this.CurrentDialogue.Push(dialogue2);
		}

		// Token: 0x06001225 RID: 4645 RVA: 0x000D5D51 File Offset: 0x000D3F51
		public string GetDialogueSheetName()
		{
			if (base.Name == "Leo" && this.DefaultMap != "IslandHut")
			{
				return base.Name + "Mainland";
			}
			return base.Name;
		}

		// Token: 0x06001226 RID: 4646 RVA: 0x000D5D8E File Offset: 0x000D3F8E
		public void setSpouseRoomMarriageDialogue()
		{
			this.currentMarriageDialogue.Clear();
			this.addMarriageDialogue("MarriageDialogue", "spouseRoom_" + base.Name, false, Array.Empty<string>());
		}

		// Token: 0x06001227 RID: 4647 RVA: 0x000D5DBC File Offset: 0x000D3FBC
		public void setRandomAfternoonMarriageDialogue(int time, GameLocation location, bool countAsDailyAfternoon = false)
		{
			if (base.Name == "Krobus" && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == "Fri")
			{
				return;
			}
			if (this.hasSaidAfternoonDialogue.Value)
			{
				return;
			}
			if (countAsDailyAfternoon)
			{
				this.hasSaidAfternoonDialogue.Value = true;
			}
			Random r = Utility.CreateDaySaveRandom((double)time, 0.0, 0.0);
			int hearts = this.getSpouse().getFriendshipHeartLevelForNPC(base.Name);
			if (!(location is FarmHouse))
			{
				if (!(location is Farm))
				{
					return;
				}
				this.currentMarriageDialogue.Clear();
				if (r.NextDouble() < 0.2)
				{
					this.addMarriageDialogue("MarriageDialogue", "Outdoor_" + base.Name, false, Array.Empty<string>());
					return;
				}
				this.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5).ToString(), false, Array.Empty<string>());
			}
			else if (r.NextBool())
			{
				if (hearts < 9)
				{
					this.currentMarriageDialogue.Clear();
					this.addMarriageDialogue("MarriageDialogue", ((r.NextDouble() < (double)((float)hearts / 11f)) ? "Neutral_" : "Bad_") + r.Next(10).ToString(), false, Array.Empty<string>());
					return;
				}
				if (r.NextDouble() < 0.05)
				{
					this.currentMarriageDialogue.Clear();
					this.addMarriageDialogue("MarriageDialogue", Game1.currentSeason + "_" + base.Name, false, Array.Empty<string>());
					return;
				}
				if ((hearts >= 10 && r.NextBool()) || (hearts >= 11 && r.NextDouble() < 0.75) || (hearts >= 12 && r.NextDouble() < 0.95))
				{
					this.currentMarriageDialogue.Clear();
					this.addMarriageDialogue("MarriageDialogue", "Good_" + r.Next(10).ToString(), false, Array.Empty<string>());
					return;
				}
				this.currentMarriageDialogue.Clear();
				this.addMarriageDialogue("MarriageDialogue", "Neutral_" + r.Next(10).ToString(), false, Array.Empty<string>());
				return;
			}
		}

		// Token: 0x06001228 RID: 4648 RVA: 0x000D6000 File Offset: 0x000D4200
		public bool isBirthday()
		{
			return this.Birthday_Season == Game1.currentSeason && this.Birthday_Day == Game1.dayOfMonth;
		}

		// Token: 0x06001229 RID: 4649 RVA: 0x000D6024 File Offset: 0x000D4224
		public Item getFavoriteItem()
		{
			string rawData;
			if (Game1.NPCGiftTastes.TryGetValue(base.Name, out rawData))
			{
				Item item = ArgUtility.SplitBySpace(rawData.Split('/', StringSplitOptions.None)[1]).Select(delegate(string id)
				{
					ItemMetadata itemMetadata = ItemRegistry.ResolveMetadata(id);
					if (itemMetadata == null)
					{
						return null;
					}
					return itemMetadata.CreateItem(1, 0);
				}).FirstOrDefault((Item p) => p != null);
				if (item != null)
				{
					return item;
				}
			}
			return null;
		}

		// Token: 0x0600122A RID: 4650 RVA: 0x000D60A4 File Offset: 0x000D42A4
		public CharacterData GetData()
		{
			CharacterData data;
			if (!this.IsVillager || !NPC.TryGetData(this.name.Value, out data))
			{
				return null;
			}
			return data;
		}

		// Token: 0x0600122B RID: 4651 RVA: 0x000D60D0 File Offset: 0x000D42D0
		public static bool TryGetData(string name, out CharacterData data)
		{
			if (name == null)
			{
				data = null;
				return false;
			}
			return Game1.characterData.TryGetValue(name, out data);
		}

		// Token: 0x0600122C RID: 4652 RVA: 0x000D60E8 File Offset: 0x000D42E8
		public static string GetDisplayName(string name)
		{
			CharacterData data;
			NPC.TryGetData(name, out data);
			return TokenParser.ParseText((data != null) ? data.DisplayName : null, null, null, null) ?? name;
		}

		// Token: 0x0600122D RID: 4653 RVA: 0x000D6118 File Offset: 0x000D4318
		public static bool CanSocializePerData(string name, GameLocation location)
		{
			CharacterData data;
			return NPC.TryGetData(name, out data) && GameStateQuery.CheckConditions(data.CanSocialize, location, null, null, null, null, null);
		}

		// Token: 0x0600122E RID: 4654 RVA: 0x000D6142 File Offset: 0x000D4342
		public string GetTokenizedDisplayName()
		{
			CharacterData data = this.GetData();
			return ((data != null) ? data.DisplayName : null) ?? this.displayName;
		}

		// Token: 0x0600122F RID: 4655 RVA: 0x000D6160 File Offset: 0x000D4360
		public bool SpeaksDwarvish()
		{
			CharacterData data = this.GetData();
			return data != null && data.Language == NpcLanguage.Dwarvish;
		}

		// Token: 0x06001230 RID: 4656 RVA: 0x000D6178 File Offset: 0x000D4378
		public virtual void receiveGift(Object o, Farmer giver, bool updateGiftLimitInfo = true, float friendshipChangeMultiplier = 1f, bool showResponse = true)
		{
			if (!this.CanReceiveGifts())
			{
				return;
			}
			float qualityChangeMultipler = 1f;
			switch (o.Quality)
			{
			case 1:
				qualityChangeMultipler = 1.1f;
				break;
			case 2:
				qualityChangeMultipler = 1.25f;
				break;
			case 4:
				qualityChangeMultipler = 1.5f;
				break;
			}
			if (this.isBirthday())
			{
				friendshipChangeMultiplier = 8f;
			}
			if (this.getSpouse() != null && this.getSpouse().Equals(giver))
			{
				friendshipChangeMultiplier /= 2f;
			}
			giver.onGiftGiven(this, o);
			Stats stats = Game1.stats;
			uint giftsGiven = stats.GiftsGiven;
			stats.GiftsGiven = giftsGiven + 1U;
			giver.currentLocation.localSound("give_gift", null, null, SoundContext.Default);
			if (updateGiftLimitInfo)
			{
				Friendship friendship = giver.friendshipData[base.Name];
				int num = friendship.GiftsToday;
				friendship.GiftsToday = num + 1;
				Friendship friendship2 = giver.friendshipData[base.Name];
				num = friendship2.GiftsThisWeek;
				friendship2.GiftsThisWeek = num + 1;
				giver.friendshipData[base.Name].LastGiftDate = new WorldDate(Game1.Date);
			}
			switch (giver.FacingDirection)
			{
			case 0:
				((FarmerSprite)giver.Sprite).animateBackwardsOnce(80, 50f);
				break;
			case 1:
				((FarmerSprite)giver.Sprite).animateBackwardsOnce(72, 50f);
				break;
			case 2:
				((FarmerSprite)giver.Sprite).animateBackwardsOnce(64, 50f);
				break;
			case 3:
				((FarmerSprite)giver.Sprite).animateBackwardsOnce(88, 50f);
				break;
			}
			int tasteForItem = this.getGiftTasteForThisItem(o);
			switch (tasteForItem)
			{
			case 0:
				giver.changeFriendship((int)(80f * friendshipChangeMultiplier * qualityChangeMultipler), this);
				base.doEmote(20, true);
				base.faceTowardFarmerForPeriod(15000, 4, false, giver);
				goto IL_29B;
			case 2:
				giver.changeFriendship((int)(45f * friendshipChangeMultiplier * qualityChangeMultipler), this);
				base.faceTowardFarmerForPeriod(7000, 3, true, giver);
				goto IL_29B;
			case 4:
				giver.changeFriendship((int)(-20f * friendshipChangeMultiplier), this);
				goto IL_29B;
			case 6:
				giver.changeFriendship((int)(-40f * friendshipChangeMultiplier), this);
				base.doEmote(12, true);
				base.faceTowardFarmerForPeriod(15000, 4, true, giver);
				goto IL_29B;
			case 7:
				giver.changeFriendship(Math.Min(750, (int)(250f * friendshipChangeMultiplier)), this);
				base.doEmote(56, true);
				base.faceTowardFarmerForPeriod(15000, 4, false, giver);
				goto IL_29B;
			}
			giver.changeFriendship((int)(20f * friendshipChangeMultiplier), this);
			IL_29B:
			if (showResponse)
			{
				Game1.DrawDialogue(this.GetGiftReaction(giver, o, tasteForItem));
			}
		}

		// Token: 0x06001231 RID: 4657 RVA: 0x000D6434 File Offset: 0x000D4634
		public virtual Dialogue GetGiftReaction(Farmer giver, Object gift, int taste)
		{
			string rawData;
			if (!this.CanReceiveGifts() || !Game1.NPCGiftTastes.TryGetValue(base.Name, out rawData))
			{
				return null;
			}
			string portrait = null;
			Dialogue dialogue;
			if (base.Name == "Krobus" && Game1.Date.DayOfWeek == DayOfWeek.Friday)
			{
				dialogue = (this.TryGetDialogue("Fri") ?? StardewValley.Dialogue.GetFallbackForError(this));
			}
			else if (this.isBirthday())
			{
				Dialogue dialogue2;
				if ((dialogue2 = this.TryGetDialogue("AcceptBirthdayGift_" + gift.QualifiedItemId)) == null)
				{
					if ((dialogue2 = (from tag in gift.GetContextTags()
					select this.TryGetDialogueByGiftTaste(taste, (string tasteTag) => "AcceptBirthdayGift_" + tasteTag + "_" + tag)).FirstOrDefault((Dialogue p) => p != null)) == null)
					{
						if ((dialogue2 = (from tag in gift.GetContextTags()
						select this.TryGetDialogue("AcceptBirthdayGift_" + tag)).FirstOrDefault((Dialogue p) => p != null)) == null)
						{
							dialogue2 = (this.TryGetDialogueByGiftTaste(taste, (string tasteTag) => "AcceptBirthdayGift_" + tasteTag) ?? this.TryGetDialogue("AcceptBirthdayGift"));
						}
					}
				}
				dialogue = dialogue2;
				switch (taste)
				{
				case 0:
				case 2:
				case 7:
					portrait = "$h";
					dialogue = (dialogue ?? (Game1.random.NextBool() ? ((this.Manners == 2) ? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4276", true) : new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4277", true)) : ((this.Manners == 2) ? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4274", true) : new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4275", false))));
					goto IL_3DF;
				case 4:
				case 6:
					portrait = "$s";
					dialogue = (dialogue ?? ((this.Manners == 2) ? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4278", true) : new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4279", true)));
					goto IL_3DF;
				}
				dialogue = (dialogue ?? ((this.Manners == 2) ? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4280", false) : new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4281", true)));
			}
			else
			{
				Dialogue dialogue3;
				if ((dialogue3 = this.TryGetDialogue("AcceptGift_" + gift.QualifiedItemId)) == null)
				{
					if ((dialogue3 = (from tag in gift.GetContextTags()
					select this.TryGetDialogueByGiftTaste(taste, (string tasteTag) => "AcceptGift_" + tasteTag + "_" + tag)).FirstOrDefault((Dialogue p) => p != null)) == null)
					{
						if ((dialogue3 = (from tag in gift.GetContextTags()
						select this.TryGetDialogue("AcceptGift_" + tag)).FirstOrDefault((Dialogue p) => p != null)) == null)
						{
							dialogue3 = (this.TryGetDialogueByGiftTaste(taste, (string tasteTag) => "AcceptGift_" + tasteTag) ?? this.TryGetDialogue("AcceptGift"));
						}
					}
				}
				dialogue = dialogue3;
				string[] rawFields = rawData.Split('/', StringSplitOptions.None);
				switch (taste)
				{
				case 0:
				case 2:
					if (dialogue == null)
					{
						portrait = "$h";
					}
					dialogue = (dialogue ?? new Dialogue(this, null, ArgUtility.Get(rawFields, taste, null, true)));
					goto IL_3DF;
				case 4:
				case 6:
					portrait = "$s";
					dialogue = (dialogue ?? new Dialogue(this, null, ArgUtility.Get(rawFields, taste, null, true)));
					goto IL_3DF;
				case 7:
					portrait = "$h";
					dialogue = (dialogue ?? new Dialogue(this, null, ArgUtility.Get(rawFields, 0, null, true)));
					goto IL_3DF;
				}
				dialogue = (dialogue ?? new Dialogue(this, null, ArgUtility.Get(rawFields, 8, null, true)));
			}
			IL_3DF:
			if (!giver.canUnderstandDwarves && this.SpeaksDwarvish())
			{
				dialogue.convertToDwarvish();
			}
			else if (portrait != null && !dialogue.CurrentEmotionSetExplicitly)
			{
				dialogue.CurrentEmotion = portrait;
			}
			return dialogue;
		}

		// Token: 0x06001232 RID: 4658 RVA: 0x000D684C File Offset: 0x000D4A4C
		public override void draw(SpriteBatch b, float alpha = 1f)
		{
			int standingY = base.StandingPixel.Y;
			float mainLayerDepth = Math.Max(0f, this.drawOnTop ? 0.991f : ((float)standingY / 10000f));
			if (this.Sprite.Texture == null)
			{
				Vector2 position = Game1.GlobalToLocal(Game1.viewport, base.Position);
				Microsoft.Xna.Framework.Rectangle spriteArea = new Microsoft.Xna.Framework.Rectangle((int)position.X, (int)position.Y - this.Sprite.SpriteWidth * 4, this.Sprite.SpriteWidth * 4, this.Sprite.SpriteHeight * 4);
				Utility.DrawErrorTexture(b, spriteArea, mainLayerDepth);
				return;
			}
			if (!this.IsInvisible && (Utility.isOnScreen(base.Position, 128) || (this.EventActor && base.currentLocation is Summit)))
			{
				if (this.swimming.Value)
				{
					b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, (float)(80 + this.yJumpOffset * 2)) + ((this.shakeTimer > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero) - new Vector2(0f, this.yOffset), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(this.Sprite.SourceRect.X, this.Sprite.SourceRect.Y, this.Sprite.SourceRect.Width, this.Sprite.SourceRect.Height / 2 - (int)(this.yOffset / 4f))), Color.White, this.rotation, new Vector2(32f, 96f) / 4f, Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, mainLayerDepth);
					Vector2 localPosition = base.getLocalPosition(Game1.viewport);
					b.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle((int)localPosition.X + (int)this.yOffset + 8, (int)localPosition.Y - 128 + this.Sprite.SourceRect.Height * 4 + 48 + this.yJumpOffset * 2 - (int)this.yOffset, this.Sprite.SourceRect.Width * 4 - (int)this.yOffset * 2 - 16, 4), new Microsoft.Xna.Framework.Rectangle?(Game1.staminaRect.Bounds), Color.White * 0.75f, 0f, Vector2.Zero, SpriteEffects.None, (float)standingY / 10000f + 0.001f);
				}
				else
				{
					b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2((float)(base.GetSpriteWidthForPositioning() * 4 / 2), (float)(this.GetBoundingBox().Height / 2)) + ((this.shakeTimer > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle?(this.Sprite.SourceRect), Color.White * alpha, this.rotation, new Vector2((float)(this.Sprite.SpriteWidth / 2), (float)this.Sprite.SpriteHeight * 3f / 4f), Math.Max(0.2f, this.scale.Value) * 4f, (this.flip || (this.Sprite.CurrentAnimation != null && this.Sprite.CurrentAnimation[this.Sprite.currentAnimationIndex].flip)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, mainLayerDepth);
				}
				this.DrawBreathing(b, alpha);
				this.DrawGlow(b);
				if (!Game1.eventUp)
				{
					this.DrawEmote(b);
				}
			}
		}

		// Token: 0x06001233 RID: 4659 RVA: 0x000D6C4C File Offset: 0x000D4E4C
		public virtual void DrawBreathing(SpriteBatch b, float alpha = 1f)
		{
			if (this.Breather && this.shakeTimer <= 0 && !this.swimming.Value && !this.farmerPassesThrough)
			{
				AnimatedSprite sprite2 = this.Sprite;
				if (sprite2 == null || sprite2.SpriteHeight <= 32)
				{
					AnimatedSprite sprite3 = this.Sprite;
					if (sprite3 == null || sprite3.SpriteWidth <= 16)
					{
						AnimatedSprite sprite = this.Sprite;
						if (sprite.currentFrame >= 16)
						{
							return;
						}
						CharacterData data = this.GetData();
						Microsoft.Xna.Framework.Rectangle spriteRect = sprite.SourceRect;
						Microsoft.Xna.Framework.Rectangle chestBox;
						if (data != null && data.BreathChestRect != null)
						{
							Microsoft.Xna.Framework.Rectangle dataRect = data.BreathChestRect.Value;
							chestBox = new Microsoft.Xna.Framework.Rectangle(spriteRect.X + dataRect.X, spriteRect.Y + dataRect.Y, dataRect.Width, dataRect.Height);
						}
						else
						{
							chestBox = new Microsoft.Xna.Framework.Rectangle(spriteRect.X + sprite.SpriteWidth / 4, spriteRect.Y + sprite.SpriteHeight / 2 + sprite.SpriteHeight / 32, sprite.SpriteHeight / 4, sprite.SpriteWidth / 2);
							if (this.Age == 2)
							{
								chestBox.Y += sprite.SpriteHeight / 6 + 1;
								chestBox.Height /= 2;
							}
							else if (this.Gender == Gender.Female)
							{
								chestBox.Y++;
								chestBox.Height /= 2;
							}
						}
						Vector2 chestPosition;
						if (data != null && data.BreathChestPosition != null)
						{
							chestPosition = Utility.PointToVector2(data.BreathChestPosition.Value);
						}
						else
						{
							chestPosition = new Vector2((float)(sprite.SpriteWidth * 4 / 2), 8f);
							if (this.Age == 2)
							{
								chestPosition.Y += (float)(sprite.SpriteHeight / 8 * 4);
								Child child = this as Child;
								if (child != null)
								{
									int num = child.Age;
									if (num != 0)
									{
										if (num == 1)
										{
											chestPosition.X -= 4f;
										}
									}
									else
									{
										chestPosition.X -= 12f;
									}
								}
							}
							else if (this.Gender == Gender.Female)
							{
								chestPosition.Y -= 4f;
							}
						}
						float breathScale = Math.Max(0f, (float)Math.Ceiling(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 600.0 + (double)(this.defaultPosition.X * 20f))) / 4f);
						int standingY = base.StandingPixel.Y;
						b.Draw(sprite.Texture, base.getLocalPosition(Game1.viewport) + chestPosition + ((this.shakeTimer > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle?(chestBox), Color.White * alpha, this.rotation, new Vector2((float)(chestBox.Width / 2), (float)(chestBox.Height / 2 + 1)), Math.Max(0.2f, this.scale.Value) * 4f + breathScale, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.992f : (((float)standingY + 0.01f) / 10000f)));
						return;
					}
				}
			}
		}

		// Token: 0x06001234 RID: 4660 RVA: 0x000D6F98 File Offset: 0x000D5198
		public virtual void DrawGlow(SpriteBatch b)
		{
			int standingY = base.StandingPixel.Y;
			if (this.isGlowing)
			{
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2((float)(base.GetSpriteWidthForPositioning() * 4 / 2), (float)(this.GetBoundingBox().Height / 2)) + ((this.shakeTimer > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle?(this.Sprite.SourceRect), this.glowingColor * this.glowingTransparency, this.rotation, new Vector2((float)(this.Sprite.SpriteWidth / 2), (float)this.Sprite.SpriteHeight * 3f / 4f), Math.Max(0.2f, this.scale.Value) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.drawOnTop ? 0.99f : ((float)standingY / 10000f + 0.001f)));
			}
		}

		// Token: 0x06001235 RID: 4661 RVA: 0x000D70D0 File Offset: 0x000D52D0
		public virtual void DrawEmote(SpriteBatch b)
		{
			if (!base.IsEmoting || this is Child || this is Pet)
			{
				return;
			}
			CharacterData data = this.GetData();
			Point dataOffset = (data != null) ? data.EmoteOffset : Point.Zero;
			Vector2 emotePosition = base.getLocalPosition(Game1.viewport);
			emotePosition = new Vector2(emotePosition.X + (float)dataOffset.X + ((float)this.Sprite.SourceRect.Width / 2f - 8f) * 4f, emotePosition.Y + (float)dataOffset.Y + (float)this.emoteYOffset - (float)((this.Sprite.SpriteHeight + 3) * 4));
			if (this.NeedsBirdieEmoteHack())
			{
				emotePosition.X += 64f;
			}
			if (this.Age == 2)
			{
				emotePosition.Y += 32f;
			}
			else if (this.Gender == Gender.Female)
			{
				emotePosition.Y += 10f;
			}
			b.Draw(Game1.emoteSpriteSheet, emotePosition, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(base.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, base.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)base.StandingPixel.Y / 10000f);
		}

		// Token: 0x06001236 RID: 4662 RVA: 0x000D7234 File Offset: 0x000D5434
		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			if (this.textAboveHeadTimer > 0 && this.textAboveHead != null)
			{
				Point standingPixel = base.StandingPixel;
				Vector2 local = Game1.GlobalToLocal(new Vector2((float)standingPixel.X, (float)(standingPixel.Y - this.Sprite.SpriteHeight * 4 - 64 + this.yJumpOffset)));
				if (this.textAboveHeadStyle == 0)
				{
					local += new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2));
				}
				if (this.NeedsBirdieEmoteHack())
				{
					local.X += (float)(-(float)this.GetBoundingBox().Width / 4 + 64);
				}
				if (this.shouldShadowBeOffset)
				{
					local += this.drawOffset;
				}
				Point tile = base.TilePoint;
				SpriteText.drawStringWithScrollCenteredAt(b, this.textAboveHead, (int)local.X, (int)local.Y, "", this.textAboveHeadAlpha, this.textAboveHeadColor, 1, (float)(tile.Y * 64) / 10000f + 0.001f + (float)tile.X / 10000f, false);
			}
		}

		// Token: 0x06001237 RID: 4663 RVA: 0x000D7350 File Offset: 0x000D5550
		public bool NeedsBirdieEmoteHack()
		{
			return Game1.eventUp && this.Sprite.SpriteWidth == 32 && base.Name == "Birdie";
		}

		// Token: 0x06001238 RID: 4664 RVA: 0x000D7380 File Offset: 0x000D5580
		public void warpToPathControllerDestination()
		{
			if (this.controller != null)
			{
				while (this.controller.pathToEndPoint.Count > 2)
				{
					this.controller.pathToEndPoint.Pop();
					this.controller.handleWarps(new Microsoft.Xna.Framework.Rectangle(this.controller.pathToEndPoint.Peek().X * 64, this.controller.pathToEndPoint.Peek().Y * 64, 64, 64));
					base.Position = new Vector2((float)(this.controller.pathToEndPoint.Peek().X * 64), (float)(this.controller.pathToEndPoint.Peek().Y * 64 + 16));
					this.Halt();
				}
			}
		}

		// Token: 0x06001239 RID: 4665 RVA: 0x000D7450 File Offset: 0x000D5650
		public virtual Microsoft.Xna.Framework.Rectangle getMugShotSourceRect()
		{
			CharacterData data = this.GetData();
			Microsoft.Xna.Framework.Rectangle? rectangle = (data != null) ? data.MugShotSourceRect : null;
			if (rectangle == null)
			{
				return new Microsoft.Xna.Framework.Rectangle(0, (this.Age == 2) ? 4 : 0, 16, 24);
			}
			return rectangle.GetValueOrDefault();
		}

		// Token: 0x0600123A RID: 4666 RVA: 0x000D74A0 File Offset: 0x000D56A0
		public void getHitByPlayer(Farmer who, GameLocation location)
		{
			base.doEmote(12, true);
			if (who == null)
			{
				if (Game1.IsMultiplayer)
				{
					return;
				}
				who = Game1.player;
			}
			if (who.friendshipData.ContainsKey(base.Name))
			{
				who.changeFriendship(-30, this);
				if (who.IsLocalPlayer)
				{
					this.CurrentDialogue.Clear();
					this.CurrentDialogue.Push(this.TryGetDialogue("HitBySlingshot") ?? (Game1.random.NextBool() ? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4293", true) : new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4294", false)));
				}
				if (this.Sprite.Texture != null)
				{
					location.debris.Add(new Debris(this.Sprite.textureName.Value, Game1.random.Next(3, 8), Utility.PointToVector2(base.StandingPixel)));
				}
			}
			if (base.Name.Equals("Bouncer"))
			{
				location.localSound("crafting", null, null, SoundContext.Default);
				return;
			}
			location.localSound("hitEnemy", null, null, SoundContext.Default);
		}

		// Token: 0x0600123B RID: 4667 RVA: 0x000D75CF File Offset: 0x000D57CF
		public void walkInSquare(int squareWidth, int squareHeight, int squarePauseOffset)
		{
			this.isWalkingInSquare = true;
			this.lengthOfWalkingSquareX = squareWidth;
			this.lengthOfWalkingSquareY = squareHeight;
			this.squarePauseOffset = squarePauseOffset;
		}

		// Token: 0x0600123C RID: 4668 RVA: 0x000D75ED File Offset: 0x000D57ED
		public void moveTowardPlayer(int threshold)
		{
			this.isWalkingTowardPlayer.Value = true;
			this.moveTowardPlayerThreshold.Value = threshold;
		}

		// Token: 0x0600123D RID: 4669 RVA: 0x000D7607 File Offset: 0x000D5807
		protected virtual Farmer findPlayer()
		{
			return Game1.MasterPlayer;
		}

		// Token: 0x0600123E RID: 4670 RVA: 0x000D760E File Offset: 0x000D580E
		public virtual bool withinPlayerThreshold()
		{
			return this.withinPlayerThreshold(this.moveTowardPlayerThreshold.Value);
		}

		// Token: 0x0600123F RID: 4671 RVA: 0x000D7624 File Offset: 0x000D5824
		public virtual bool withinPlayerThreshold(int threshold)
		{
			if (base.currentLocation != null && !base.currentLocation.farmers.Any())
			{
				return false;
			}
			Vector2 tileLocationOfPlayer = this.findPlayer().Tile;
			Vector2 tileLocationOfMonster = base.Tile;
			return Math.Abs(tileLocationOfMonster.X - tileLocationOfPlayer.X) <= (float)threshold && Math.Abs(tileLocationOfMonster.Y - tileLocationOfPlayer.Y) <= (float)threshold;
		}

		// Token: 0x06001240 RID: 4672 RVA: 0x000D768F File Offset: 0x000D588F
		private Stack<Point> addToStackForSchedule(Stack<Point> original, Stack<Point> toAdd)
		{
			if (toAdd == null)
			{
				return original;
			}
			original = new Stack<Point>(original);
			while (original.Count > 0)
			{
				toAdd.Push(original.Pop());
			}
			return toAdd;
		}

		// Token: 0x06001241 RID: 4673 RVA: 0x000D76B8 File Offset: 0x000D58B8
		public virtual SchedulePathDescription pathfindToNextScheduleLocation(string scheduleKey, string startingLocation, int startingX, int startingY, string endingLocation, int endingX, int endingY, int finalFacingDirection, string endBehavior, string endMessage)
		{
			Stack<Point> path = new Stack<Point>();
			Point locationStartPoint = new Point(startingX, startingY);
			if (locationStartPoint == Point.Zero)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(98, 3);
				defaultInterpolatedStringHandler.AppendLiteral("NPC ");
				defaultInterpolatedStringHandler.AppendFormatted(base.Name);
				defaultInterpolatedStringHandler.AppendLiteral(" has an invalid schedule with key '");
				defaultInterpolatedStringHandler.AppendFormatted(scheduleKey);
				defaultInterpolatedStringHandler.AppendLiteral("': start position in ");
				defaultInterpolatedStringHandler.AppendFormatted(startingLocation);
				defaultInterpolatedStringHandler.AppendLiteral(" is at tile (0, 0), which isn't valid.");
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			string[] locationsRoute = (!startingLocation.Equals(endingLocation, StringComparison.Ordinal)) ? this.getLocationRoute(startingLocation, endingLocation) : null;
			if (locationsRoute != null)
			{
				for (int i = 0; i < locationsRoute.Length; i++)
				{
					string targetLocationName = locationsRoute[i];
					using (IEnumerator<string> enumerator = Game1.netWorldState.Value.ActivePassiveFestivals.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							PassiveFestivalData data;
							string newName;
							if (Utility.TryGetPassiveFestivalData(enumerator.Current, out data) && data.MapReplacements != null && data.MapReplacements.TryGetValue(targetLocationName, out newName))
							{
								targetLocationName = newName;
								break;
							}
						}
					}
					GameLocation currentLocation = Game1.RequireLocation(targetLocationName, false);
					if (currentLocation.Name.Equals("Trailer") && Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
					{
						currentLocation = Game1.RequireLocation("Trailer_Big", false);
					}
					if (i < locationsRoute.Length - 1)
					{
						Point target = currentLocation.getWarpPointTo(locationsRoute[i + 1], null);
						if (target == Point.Zero)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(91, 4);
							defaultInterpolatedStringHandler.AppendLiteral("NPC ");
							defaultInterpolatedStringHandler.AppendFormatted(base.Name);
							defaultInterpolatedStringHandler.AppendLiteral(" has an invalid schedule with key '");
							defaultInterpolatedStringHandler.AppendFormatted(scheduleKey);
							defaultInterpolatedStringHandler.AppendLiteral("': it requires a warp from ");
							defaultInterpolatedStringHandler.AppendFormatted(currentLocation.NameOrUniqueName);
							defaultInterpolatedStringHandler.AppendLiteral(" to ");
							defaultInterpolatedStringHandler.AppendFormatted(locationsRoute[i + 1]);
							defaultInterpolatedStringHandler.AppendLiteral(", but none was found.");
							throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
						}
						path = this.addToStackForSchedule(path, PathFindController.findPathForNPCSchedules(locationStartPoint, target, currentLocation, 30000, this));
						locationStartPoint = currentLocation.getWarpPointTarget(target, this);
					}
					else
					{
						path = this.addToStackForSchedule(path, PathFindController.findPathForNPCSchedules(locationStartPoint, new Point(endingX, endingY), currentLocation, 30000, this));
					}
				}
			}
			else if (startingLocation.Equals(endingLocation, StringComparison.Ordinal))
			{
				string targetLocationName2 = startingLocation;
				using (IEnumerator<string> enumerator = Game1.netWorldState.Value.ActivePassiveFestivals.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						PassiveFestivalData data2;
						string newName2;
						if (Utility.TryGetPassiveFestivalData(enumerator.Current, out data2) && data2.MapReplacements != null && data2.MapReplacements.TryGetValue(targetLocationName2, out newName2))
						{
							targetLocationName2 = newName2;
							break;
						}
					}
				}
				GameLocation location = Game1.RequireLocation(targetLocationName2, false);
				if (location.Name.Equals("Trailer") && Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
				{
					location = Game1.RequireLocation("Trailer_Big", false);
				}
				path = PathFindController.findPathForNPCSchedules(locationStartPoint, new Point(endingX, endingY), location, 30000, this);
			}
			return new SchedulePathDescription(path, finalFacingDirection, endBehavior, endMessage, endingLocation, new Point(endingX, endingY));
		}

		// Token: 0x06001242 RID: 4674 RVA: 0x000D7A18 File Offset: 0x000D5C18
		private string[] getLocationRoute(string startingLocation, string endingLocation)
		{
			return WarpPathfindingCache.GetLocationRoute(startingLocation, endingLocation, this.Gender);
		}

		// Token: 0x06001243 RID: 4675 RVA: 0x000D7A28 File Offset: 0x000D5C28
		private bool changeScheduleForLocationAccessibility(ref string locationName, ref int tileX, ref int tileY, ref int facingDirection)
		{
			string a = locationName;
			if (!(a == "JojaMart") && !(a == "Railroad"))
			{
				if (a == "CommunityCenter")
				{
					return !Game1.isLocationAccessible(locationName);
				}
			}
			else if (!Game1.isLocationAccessible(locationName))
			{
				if (!this.hasMasterScheduleEntry(locationName + "_Replacement"))
				{
					return true;
				}
				string[] split = ArgUtility.SplitBySpace(this.getMasterScheduleEntry(locationName + "_Replacement"));
				locationName = split[0];
				tileX = Convert.ToInt32(split[1]);
				tileY = Convert.ToInt32(split[2]);
				facingDirection = Convert.ToInt32(split[3]);
			}
			return false;
		}

		// Token: 0x06001244 RID: 4676 RVA: 0x000D7ACA File Offset: 0x000D5CCA
		public virtual Dictionary<int, SchedulePathDescription> parseMasterSchedule(string scheduleKey, string rawData)
		{
			return this.parseMasterScheduleImpl(scheduleKey, rawData, new List<string>());
		}

		// Token: 0x06001245 RID: 4677 RVA: 0x000D7ADC File Offset: 0x000D5CDC
		protected virtual Dictionary<int, SchedulePathDescription> parseMasterScheduleImpl(string scheduleKey, string rawData, List<string> visited)
		{
			if (visited.Contains(scheduleKey, StringComparer.OrdinalIgnoreCase))
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(70, 3);
				defaultInterpolatedStringHandler.AppendLiteral("NPC ");
				defaultInterpolatedStringHandler.AppendFormatted(base.Name);
				defaultInterpolatedStringHandler.AppendLiteral(" can't load schedules because they led to an infinite loop (");
				defaultInterpolatedStringHandler.AppendFormatted(string.Join(" -> ", visited));
				defaultInterpolatedStringHandler.AppendLiteral(" -> ");
				defaultInterpolatedStringHandler.AppendFormatted(scheduleKey);
				defaultInterpolatedStringHandler.AppendLiteral(").");
				log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
				return new Dictionary<int, SchedulePathDescription>();
			}
			visited.Add(scheduleKey);
			Dictionary<int, SchedulePathDescription> result;
			try
			{
				string[] split = NPC.SplitScheduleCommands(rawData);
				Dictionary<int, SchedulePathDescription> oneDaySchedule = new Dictionary<int, SchedulePathDescription>();
				int routesToSkip = 0;
				if (split[0].Contains("GOTO"))
				{
					string newKey = ArgUtility.SplitBySpaceAndGet(split[0], 1, null);
					Dictionary<string, string> allSchedules = this.getMasterScheduleRawData();
					if (newKey.EqualsIgnoreCase("season"))
					{
						newKey = Game1.currentSeason;
						if (!allSchedules.ContainsKey(newKey))
						{
							newKey = "spring";
						}
					}
					try
					{
						string newScript;
						if (allSchedules.TryGetValue(newKey, out newScript))
						{
							return this.parseMasterScheduleImpl(newKey, newScript, visited);
						}
						IGameLogger log2 = Game1.log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(113, 3);
						defaultInterpolatedStringHandler.AppendLiteral("Failed to load schedule '");
						defaultInterpolatedStringHandler.AppendFormatted(scheduleKey);
						defaultInterpolatedStringHandler.AppendLiteral("' for NPC '");
						defaultInterpolatedStringHandler.AppendFormatted(base.Name);
						defaultInterpolatedStringHandler.AppendLiteral("': GOTO references schedule '");
						defaultInterpolatedStringHandler.AppendFormatted(newKey);
						defaultInterpolatedStringHandler.AppendLiteral("' which doesn't exist. Falling back to 'spring'.");
						log2.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
					}
					catch (Exception e)
					{
						IGameLogger log3 = Game1.log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(118, 3);
						defaultInterpolatedStringHandler.AppendLiteral("Failed to load schedule '");
						defaultInterpolatedStringHandler.AppendFormatted(scheduleKey);
						defaultInterpolatedStringHandler.AppendLiteral("' for NPC '");
						defaultInterpolatedStringHandler.AppendFormatted(base.Name);
						defaultInterpolatedStringHandler.AppendLiteral("': GOTO references schedule '");
						defaultInterpolatedStringHandler.AppendFormatted(newKey);
						defaultInterpolatedStringHandler.AppendLiteral("' which couldn't be parsed. Falling back to 'spring'.");
						log3.Error(defaultInterpolatedStringHandler.ToStringAndClear(), e);
					}
					result = this.parseMasterScheduleImpl("spring", this.getMasterScheduleEntry("spring"), visited);
				}
				else
				{
					if (split[0].Contains("NOT"))
					{
						string[] commandSplit = ArgUtility.SplitBySpace(split[0]);
						if (commandSplit[1].ToLower() == "friendship")
						{
							int index = 2;
							bool conditionMet = false;
							while (index < commandSplit.Length)
							{
								string who = commandSplit[index];
								int level;
								if (int.TryParse(commandSplit[index + 1], out level))
								{
									using (IEnumerator<Farmer> enumerator = Game1.getAllFarmers().GetEnumerator())
									{
										while (enumerator.MoveNext())
										{
											if (enumerator.Current.getFriendshipHeartLevelForNPC(who) >= level)
											{
												conditionMet = true;
												break;
											}
										}
									}
								}
								if (conditionMet)
								{
									break;
								}
								index += 2;
							}
							if (conditionMet)
							{
								return this.parseMasterScheduleImpl("spring", this.getMasterScheduleEntry("spring"), visited);
							}
							routesToSkip++;
						}
					}
					else if (split[0].Contains("MAIL"))
					{
						string mailID = ArgUtility.SplitBySpace(split[0])[1];
						if (Game1.MasterPlayer.mailReceived.Contains(mailID) || NetWorldState.checkAnywhereForWorldStateID(mailID))
						{
							routesToSkip += 2;
						}
						else
						{
							routesToSkip++;
						}
					}
					if (split[routesToSkip].Contains("GOTO"))
					{
						string newKey2 = ArgUtility.SplitBySpaceAndGet(split[routesToSkip], 1, null);
						string a = newKey2.ToLower();
						if (!(a == "season"))
						{
							if (a == "no_schedule")
							{
								this.followSchedule = false;
								return null;
							}
						}
						else
						{
							newKey2 = Game1.currentSeason;
						}
						result = this.parseMasterScheduleImpl(newKey2, this.getMasterScheduleEntry(newKey2), visited);
					}
					else
					{
						Point previousPosition = this.isMarried() ? new Point(10, 23) : new Point((int)this.defaultPosition.X / 64, (int)this.defaultPosition.Y / 64);
						string previousGameLocation = this.isMarried() ? "BusStop" : this.defaultMap.Value;
						int previousTime = 610;
						string default_map = this.DefaultMap;
						int default_x = (int)(this.defaultPosition.X / 64f);
						int default_y = (int)(this.defaultPosition.Y / 64f);
						bool default_map_dirty = false;
						for (int i = routesToSkip; i < split.Length; i++)
						{
							int index2 = 0;
							string[] newDestinationDescription = ArgUtility.SplitBySpace(split[i]);
							bool time_is_arrival_time = false;
							string time_string = newDestinationDescription[index2];
							if (time_string.Length > 0 && newDestinationDescription[index2][0] == 'a')
							{
								time_is_arrival_time = true;
								time_string = time_string.Substring(1);
							}
							int time = Convert.ToInt32(time_string);
							index2++;
							string location = newDestinationDescription[index2];
							string endOfRouteAnimation = null;
							string endOfRouteMessage = null;
							int xLocation = 0;
							int yLocation = 0;
							int localFacingDirection = 2;
							if (location == "bed")
							{
								if (this.isMarried())
								{
									location = "BusStop";
									xLocation = 9;
									yLocation = 23;
									localFacingDirection = 3;
								}
								else
								{
									string default_schedule = null;
									if (this.hasMasterScheduleEntry("default"))
									{
										default_schedule = this.getMasterScheduleEntry("default");
									}
									else if (this.hasMasterScheduleEntry("spring"))
									{
										default_schedule = this.getMasterScheduleEntry("spring");
									}
									if (default_schedule != null)
									{
										try
										{
											string[] array = NPC.SplitScheduleCommands(default_schedule);
											string[] last_schedule_split = ArgUtility.SplitBySpace(array[array.Length - 1]);
											location = last_schedule_split[1];
											if (last_schedule_split.Length > 3)
											{
												if (!int.TryParse(last_schedule_split[2], out xLocation) || !int.TryParse(last_schedule_split[3], out yLocation))
												{
													default_schedule = null;
												}
											}
											else
											{
												default_schedule = null;
											}
										}
										catch (Exception)
										{
											default_schedule = null;
										}
									}
									if (default_schedule == null)
									{
										location = default_map;
										xLocation = default_x;
										yLocation = default_y;
									}
								}
								index2++;
								Dictionary<string, string> dictionary = DataLoader.AnimationDescriptions(Game1.content);
								string sleep_behavior = this.name.Value.ToLower() + "_sleep";
								if (dictionary.ContainsKey(sleep_behavior))
								{
									endOfRouteAnimation = sleep_behavior;
								}
							}
							else
							{
								int num;
								if (int.TryParse(location, out num))
								{
									location = previousGameLocation;
									index2--;
								}
								index2++;
								xLocation = Convert.ToInt32(newDestinationDescription[index2]);
								index2++;
								yLocation = Convert.ToInt32(newDestinationDescription[index2]);
								index2++;
								try
								{
									if (newDestinationDescription.Length > index2)
									{
										if (int.TryParse(newDestinationDescription[index2], out localFacingDirection))
										{
											index2++;
										}
										else
										{
											localFacingDirection = 2;
										}
									}
								}
								catch (Exception)
								{
									localFacingDirection = 2;
								}
							}
							if (this.changeScheduleForLocationAccessibility(ref location, ref xLocation, ref yLocation, ref localFacingDirection))
							{
								string newKey3 = this.getMasterScheduleRawData().ContainsKey("default") ? "default" : "spring";
								return this.parseMasterScheduleImpl(newKey3, this.getMasterScheduleEntry(newKey3), visited);
							}
							if (index2 < newDestinationDescription.Length)
							{
								if (newDestinationDescription[index2].Length > 0 && newDestinationDescription[index2][0] == '"')
								{
									endOfRouteMessage = split[i].Substring(split[i].IndexOf('"'));
								}
								else
								{
									endOfRouteAnimation = newDestinationDescription[index2];
									index2++;
									if (index2 < newDestinationDescription.Length && newDestinationDescription[index2].Length > 0 && newDestinationDescription[index2][0] == '"')
									{
										endOfRouteMessage = split[i].Substring(split[i].IndexOf('"')).Replace("\"", "");
									}
								}
							}
							if (time == 0)
							{
								default_map_dirty = true;
								default_map = location;
								default_x = xLocation;
								default_y = yLocation;
								previousGameLocation = location;
								previousPosition.X = xLocation;
								previousPosition.Y = yLocation;
								this.faceDirection(localFacingDirection);
								this.previousEndPoint = new Point(xLocation, yLocation);
							}
							else
							{
								SchedulePathDescription path_description = this.pathfindToNextScheduleLocation(scheduleKey, previousGameLocation, previousPosition.X, previousPosition.Y, location, xLocation, yLocation, localFacingDirection, endOfRouteAnimation, endOfRouteMessage);
								if (time_is_arrival_time)
								{
									int distance_traveled = 0;
									Point? last_point = null;
									foreach (Point point in path_description.route)
									{
										if (last_point == null)
										{
											last_point = new Point?(point);
										}
										else
										{
											if (Math.Abs(last_point.Value.X - point.X) + Math.Abs(last_point.Value.Y - point.Y) == 1)
											{
												distance_traveled += 64;
											}
											last_point = new Point?(point);
										}
									}
									float num2 = (float)(distance_traveled / 2);
									int ticks_per_ten_minutes = Game1.realMilliSecondsPerGameTenMinutes / 1000 * 60;
									int travel_time = (int)Math.Round((double)(num2 / (float)ticks_per_ten_minutes)) * 10;
									time = Math.Max(Utility.ConvertMinutesToTime(Utility.ConvertTimeToMinutes(time) - travel_time), previousTime);
								}
								path_description.time = time;
								oneDaySchedule.Add(time, path_description);
								previousPosition.X = xLocation;
								previousPosition.Y = yLocation;
								previousGameLocation = location;
								previousTime = time;
							}
						}
						if (Game1.IsMasterGame && default_map_dirty)
						{
							Game1.warpCharacter(this, default_map, new Point(default_x, default_y));
						}
						result = oneDaySchedule;
					}
				}
			}
			catch (Exception ex)
			{
				IGameLogger log4 = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(59, 3);
				defaultInterpolatedStringHandler.AppendLiteral("NPC '");
				defaultInterpolatedStringHandler.AppendFormatted(base.Name);
				defaultInterpolatedStringHandler.AppendLiteral("' failed to parse master schedule '");
				defaultInterpolatedStringHandler.AppendFormatted(scheduleKey);
				defaultInterpolatedStringHandler.AppendLiteral("' with raw data '");
				defaultInterpolatedStringHandler.AppendFormatted(rawData);
				defaultInterpolatedStringHandler.AppendLiteral("'.");
				log4.Error(defaultInterpolatedStringHandler.ToStringAndClear(), ex);
				result = new Dictionary<int, SchedulePathDescription>();
			}
			return result;
		}

		// Token: 0x06001246 RID: 4678 RVA: 0x000D8454 File Offset: 0x000D6654
		public static string[] SplitScheduleCommands(string rawScript)
		{
			return LegacyShims.SplitAndTrim(rawScript, '/', StringSplitOptions.RemoveEmptyEntries);
		}

		// Token: 0x06001247 RID: 4679 RVA: 0x000D8460 File Offset: 0x000D6660
		public bool TryLoadSchedule()
		{
			string season = Game1.currentSeason;
			int day = Game1.dayOfMonth;
			string dayName = Game1.shortDayNameFromDayOfSeason(day);
			int heartLevel = Math.Max(0, Utility.GetAllPlayerFriendshipLevel(this)) / 250;
			if (this.getMasterScheduleRawData() == null)
			{
				this.ClearSchedule();
				return false;
			}
			if (Game1.isGreenRain && Game1.year == 1 && this.TryLoadSchedule("GreenRain"))
			{
				return true;
			}
			if (!string.IsNullOrWhiteSpace(this.islandScheduleName.Value))
			{
				this.TryLoadSchedule(this.islandScheduleName.Value, this.Schedule);
				return true;
			}
			foreach (string festivalId in Game1.netWorldState.Value.ActivePassiveFestivals)
			{
				int dayOfPassiveFestival = Utility.GetDayOfPassiveFestival(festivalId);
				if (this.isMarried())
				{
					if (this.TryLoadSchedule("marriage_" + festivalId + "_" + dayOfPassiveFestival.ToString()))
					{
						return true;
					}
					if (this.TryLoadSchedule("marriage_" + festivalId))
					{
						return true;
					}
				}
				else
				{
					if (this.TryLoadSchedule(festivalId + "_" + dayOfPassiveFestival.ToString()))
					{
						return true;
					}
					if (this.TryLoadSchedule(festivalId))
					{
						return true;
					}
				}
			}
			if (this.isMarried())
			{
				if (this.TryLoadSchedule("marriage_" + season + "_" + day.ToString()))
				{
					return true;
				}
				if (((base.Name == "Penny" && (dayName == "Tue" || dayName == "Wed" || dayName == "Fri")) || (base.Name == "Maru" && (dayName == "Tue" || dayName == "Thu")) || (base.Name == "Harvey" && (dayName == "Tue" || dayName == "Thu"))) && this.TryLoadSchedule("marriageJob"))
				{
					return true;
				}
				if (!Game1.isRaining && this.TryLoadSchedule("marriage_" + dayName))
				{
					return true;
				}
				this.ClearSchedule();
				return false;
			}
			else
			{
				if (this.TryLoadSchedule(season + "_" + day.ToString()))
				{
					return true;
				}
				for (int tryHearts = heartLevel; tryHearts > 0; tryHearts--)
				{
					if (this.TryLoadSchedule(day.ToString() + "_" + tryHearts.ToString()))
					{
						return true;
					}
				}
				if (this.TryLoadSchedule(day.ToString()))
				{
					return true;
				}
				if (base.Name == "Pam" && Game1.player.mailReceived.Contains("ccVault") && this.TryLoadSchedule("bus"))
				{
					return true;
				}
				GameLocation currentLocation = base.currentLocation;
				bool? flag = (currentLocation != null) ? new bool?(currentLocation.IsRainingHere()) : null;
				if (flag != null && flag.GetValueOrDefault())
				{
					if (Game1.random.NextBool() && this.TryLoadSchedule("rain2"))
					{
						return true;
					}
					if (this.TryLoadSchedule("rain"))
					{
						return true;
					}
				}
				for (int tryHearts2 = heartLevel; tryHearts2 > 0; tryHearts2--)
				{
					if (this.TryLoadSchedule(string.Concat(new string[]
					{
						season,
						"_",
						dayName,
						"_",
						tryHearts2.ToString()
					})))
					{
						return true;
					}
					tryHearts2--;
				}
				if (this.TryLoadSchedule(season + "_" + dayName))
				{
					return true;
				}
				for (int tryHearts3 = heartLevel; tryHearts3 > 0; tryHearts3--)
				{
					if (this.TryLoadSchedule(dayName + "_" + tryHearts3.ToString()))
					{
						return true;
					}
					tryHearts3--;
				}
				if (this.TryLoadSchedule(dayName))
				{
					return true;
				}
				if (this.TryLoadSchedule(season))
				{
					return true;
				}
				if (this.TryLoadSchedule("spring_" + dayName))
				{
					return true;
				}
				if (this.TryLoadSchedule("spring"))
				{
					return true;
				}
				this.ClearSchedule();
				return false;
			}
			bool result;
			return result;
		}

		// Token: 0x06001248 RID: 4680 RVA: 0x000D8884 File Offset: 0x000D6A84
		public bool TryLoadSchedule(string key)
		{
			try
			{
				if (this.hasMasterScheduleEntry(key))
				{
					this.TryLoadSchedule(key, this.parseMasterSchedule(key, this.getMasterScheduleEntry(key)));
					return true;
				}
			}
			catch (Exception ex)
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to load schedule key '");
				defaultInterpolatedStringHandler.AppendFormatted(key);
				defaultInterpolatedStringHandler.AppendLiteral("' for NPC '");
				defaultInterpolatedStringHandler.AppendFormatted(base.Name);
				defaultInterpolatedStringHandler.AppendLiteral("'.");
				log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), ex);
			}
			this.ClearSchedule();
			return false;
		}

		// Token: 0x06001249 RID: 4681 RVA: 0x000D8928 File Offset: 0x000D6B28
		public bool TryLoadSchedule(string key, string rawSchedule)
		{
			Dictionary<int, SchedulePathDescription> schedule;
			try
			{
				schedule = this.parseMasterSchedule(key, rawSchedule);
			}
			catch (Exception ex)
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(58, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to load schedule key '");
				defaultInterpolatedStringHandler.AppendFormatted(key);
				defaultInterpolatedStringHandler.AppendLiteral("' from raw string for NPC '");
				defaultInterpolatedStringHandler.AppendFormatted(base.Name);
				defaultInterpolatedStringHandler.AppendLiteral("'.");
				log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), ex);
				this.ClearSchedule();
				return false;
			}
			return this.TryLoadSchedule(key, schedule);
		}

		// Token: 0x0600124A RID: 4682 RVA: 0x000D89BC File Offset: 0x000D6BBC
		public bool TryLoadSchedule(string key, Dictionary<int, SchedulePathDescription> schedule)
		{
			if (schedule == null)
			{
				this.ClearSchedule();
				return false;
			}
			this.Schedule = schedule;
			if (Game1.IsMasterGame)
			{
				this.dayScheduleName.Value = key;
			}
			this.followSchedule = true;
			return true;
		}

		// Token: 0x0600124B RID: 4683 RVA: 0x000D89EB File Offset: 0x000D6BEB
		public void ClearSchedule()
		{
			this.Schedule = null;
			if (Game1.IsMasterGame)
			{
				this.dayScheduleName.Value = null;
			}
			this.followSchedule = false;
		}

		// Token: 0x0600124C RID: 4684 RVA: 0x000D8A0E File Offset: 0x000D6C0E
		public virtual void handleMasterScheduleFileLoadError(Exception e)
		{
			Game1.log.Error("NPC '" + base.Name + "' failed loading schedule file.", e);
		}

		// Token: 0x0600124D RID: 4685 RVA: 0x000D8A30 File Offset: 0x000D6C30
		public virtual void InvalidateMasterSchedule()
		{
			this._hasLoadedMasterScheduleData = false;
		}

		// Token: 0x0600124E RID: 4686 RVA: 0x000D8A3C File Offset: 0x000D6C3C
		public Dictionary<string, string> getMasterScheduleRawData()
		{
			if (!this._hasLoadedMasterScheduleData)
			{
				this._hasLoadedMasterScheduleData = true;
				string assetName = "Characters\\schedules\\" + base.Name;
				if (base.Name == "Leo" && this.DefaultMap != "IslandHut")
				{
					assetName += "Mainland";
				}
				try
				{
					if (Game1.content.DoesAssetExist<Dictionary<string, string>>(assetName))
					{
						this._masterScheduleData = Game1.content.Load<Dictionary<string, string>>(assetName);
						this._masterScheduleData = new Dictionary<string, string>(this._masterScheduleData, StringComparer.OrdinalIgnoreCase);
					}
				}
				catch (Exception e)
				{
					this.handleMasterScheduleFileLoadError(e);
				}
			}
			return this._masterScheduleData;
		}

		// Token: 0x0600124F RID: 4687 RVA: 0x000D8AF4 File Offset: 0x000D6CF4
		public string getMasterScheduleEntry(string schedule_key)
		{
			if (this.getMasterScheduleRawData() == null)
			{
				throw new KeyNotFoundException("The schedule file for NPC '" + base.Name + "' could not be loaded...");
			}
			string data;
			if (this._masterScheduleData.TryGetValue(schedule_key, out data))
			{
				return data;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(54, 2);
			defaultInterpolatedStringHandler.AppendLiteral("The schedule file for NPC '");
			defaultInterpolatedStringHandler.AppendFormatted(base.Name);
			defaultInterpolatedStringHandler.AppendLiteral("' has no schedule named '");
			defaultInterpolatedStringHandler.AppendFormatted(schedule_key);
			defaultInterpolatedStringHandler.AppendLiteral("'.");
			throw new KeyNotFoundException(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x06001250 RID: 4688 RVA: 0x000D8B85 File Offset: 0x000D6D85
		public bool hasMasterScheduleEntry(string key)
		{
			return this.getMasterScheduleRawData() != null && this.getMasterScheduleRawData().ContainsKey(key);
		}

		// Token: 0x06001251 RID: 4689 RVA: 0x000D8BA0 File Offset: 0x000D6DA0
		public virtual bool isRoommate()
		{
			if (!this.IsVillager)
			{
				return false;
			}
			foreach (Farmer f in Game1.getAllFarmers())
			{
				if (f.spouse != null && f.spouse == base.Name && !f.isEngaged() && f.isRoommate(base.Name))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001252 RID: 4690 RVA: 0x000D8C28 File Offset: 0x000D6E28
		public bool isMarried()
		{
			if (!this.IsVillager)
			{
				return false;
			}
			foreach (Farmer f in Game1.getAllFarmers())
			{
				if (f.spouse != null && f.spouse == base.Name && !f.isEngaged())
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001253 RID: 4691 RVA: 0x000D8CA4 File Offset: 0x000D6EA4
		public bool isMarriedOrEngaged()
		{
			if (!this.IsVillager)
			{
				return false;
			}
			foreach (Farmer f in Game1.getAllFarmers())
			{
				if (f.spouse != null && f.spouse == base.Name)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001254 RID: 4692 RVA: 0x000D8D18 File Offset: 0x000D6F18
		public virtual void dayUpdate(int dayOfMonth)
		{
			bool villager = this.IsVillager;
			this.isMovingOnPathFindPath.Value = false;
			this.queuedSchedulePaths.Clear();
			this.lastAttemptedSchedule = -1;
			this.drawOffset = Vector2.Zero;
			this.appliedRouteAnimationOffset = Vector2.Zero;
			this.shouldWearIslandAttire.Value = false;
			if (this.layingDown)
			{
				this.layingDown = false;
				this.HideShadow = false;
			}
			if (this.isWearingIslandAttire)
			{
				this.wearNormalClothes();
			}
			if (base.currentLocation != null && this.defaultMap.Value != null)
			{
				try
				{
					Game1.warpCharacter(this, this.defaultMap.Value, this.defaultPosition.Value / 64f);
				}
				catch (Exception ex)
				{
					IGameLogger log = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 2);
					defaultInterpolatedStringHandler.AppendLiteral("NPC '");
					defaultInterpolatedStringHandler.AppendFormatted(base.Name);
					defaultInterpolatedStringHandler.AppendLiteral("' failed to warp home to '");
					defaultInterpolatedStringHandler.AppendFormatted<NetString>(this.defaultMap);
					defaultInterpolatedStringHandler.AppendLiteral("' overnight.");
					log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), ex);
				}
			}
			if (villager)
			{
				string name = base.Name;
				if (!(name == "Willy"))
				{
					if (name == "Elliott")
					{
						if (Game1.IsMasterGame && Game1.netWorldState.Value.hasWorldStateID("elliottGone"))
						{
							this.daysUntilNotInvisible = 7;
							Game1.netWorldState.Value.removeWorldStateID("elliottGone");
							Game1.worldStateIDs.Remove("elliottGone");
						}
					}
				}
				else
				{
					this.IsInvisible = false;
				}
			}
			this.UpdateInvisibilityOnNewDay();
			this.resetForNewDay(dayOfMonth);
			this.ChooseAppearance(null);
			if (villager)
			{
				this.updateConstructionAnimation();
			}
			this.clearTextAboveHead();
		}

		// Token: 0x06001255 RID: 4693 RVA: 0x000D8EDC File Offset: 0x000D70DC
		public void OnDayStarted()
		{
			if (Game1.IsMasterGame && this.isMarried() && !this.getSpouse().divorceTonight.Value && !this.IsInvisible)
			{
				this.marriageDuties();
			}
		}

		// Token: 0x06001256 RID: 4694 RVA: 0x000D8F10 File Offset: 0x000D7110
		protected void UpdateInvisibilityOnNewDay()
		{
			if (Game1.IsMasterGame && (this.IsInvisible || this.daysUntilNotInvisible > 0))
			{
				this.daysUntilNotInvisible--;
				this.IsInvisible = (this.daysUntilNotInvisible > 0);
				if (!this.IsInvisible)
				{
					this.daysUntilNotInvisible = 0;
				}
			}
		}

		// Token: 0x06001257 RID: 4695 RVA: 0x000D8F64 File Offset: 0x000D7164
		public virtual void resetForNewDay(int dayOfMonth)
		{
			this.sleptInBed.Value = true;
			if (this.isMarried() && !this.isRoommate())
			{
				FarmHouse house = Utility.getHomeOfFarmer(this.getSpouse());
				if (house != null && house.GetSpouseBed() == null)
				{
					this.sleptInBed.Value = false;
				}
			}
			if (this.doingEndOfRouteAnimation.Value)
			{
				this.routeEndAnimationFinished(null);
			}
			this.Halt();
			this.wasKissedYesterday = this.hasBeenKissedToday.Value;
			this.hasBeenKissedToday.Value = false;
			this.currentMarriageDialogue.Clear();
			this.marriageDefaultDialogue.Value = null;
			this.shouldSayMarriageDialogue.Value = false;
			this.isSleeping.Value = false;
			this.drawOffset = Vector2.Zero;
			this.faceTowardFarmer = false;
			this.faceTowardFarmerTimer = 0;
			this.drawOffset = Vector2.Zero;
			this.hasSaidAfternoonDialogue.Value = false;
			this.isPlayingSleepingAnimation = false;
			this.ignoreScheduleToday = false;
			this.Halt();
			this.controller = null;
			this.temporaryController = null;
			this.directionsToNewLocation = null;
			this.faceDirection(this.DefaultFacingDirection);
			this.Sprite.oldFrame = this.Sprite.CurrentFrame;
			this.previousEndPoint = new Point((int)this.defaultPosition.X / 64, (int)this.defaultPosition.Y / 64);
			this.isWalkingInSquare = false;
			this.returningToEndPoint = false;
			this.lastCrossroad = Microsoft.Xna.Framework.Rectangle.Empty;
			this._startedEndOfRouteBehavior = null;
			this._finishingEndOfRouteBehavior = null;
			this.loadedEndOfRouteBehavior = null;
			this._beforeEndOfRouteAnimationFrame = this.Sprite.CurrentFrame;
			if (this.IsVillager)
			{
				if (base.Name == "Willy" && Game1.stats.DaysPlayed < 2U)
				{
					this.IsInvisible = true;
					this.daysUntilNotInvisible = 1;
				}
				this.TryLoadSchedule();
				this.performSpecialScheduleChanges();
			}
			this.endOfRouteMessage.Value = null;
		}

		// Token: 0x06001258 RID: 4696 RVA: 0x000D9148 File Offset: 0x000D7348
		public void returnHomeFromFarmPosition(Farm farm)
		{
			Farmer farmer = this.getSpouse();
			if (farmer == null)
			{
				return;
			}
			FarmHouse farm_house = Utility.getHomeOfFarmer(farmer);
			Point porchPoint = farm_house.getPorchStandingSpot();
			if (base.TilePoint == porchPoint)
			{
				this.drawOffset = Vector2.Zero;
				string nameOfHome = this.getHome().NameOrUniqueName;
				base.willDestroyObjectsUnderfoot = true;
				Point destination = farm.getWarpPointTo(nameOfHome, this);
				this.controller = new PathFindController(this, farm, destination, 0)
				{
					NPCSchedule = true
				};
				return;
			}
			if (!this.shouldPlaySpousePatioAnimation.Value || !farm.farmers.Any())
			{
				this.drawOffset = Vector2.Zero;
				this.Halt();
				this.controller = null;
				this.temporaryController = null;
				this.ignoreScheduleToday = true;
				Game1.warpCharacter(this, farm_house, Utility.PointToVector2(farm_house.getKitchenStandingSpot()));
			}
		}

		// Token: 0x06001259 RID: 4697 RVA: 0x000D920E File Offset: 0x000D740E
		public virtual Vector2 GetSpousePatioPosition()
		{
			return Utility.PointToVector2(Game1.getFarm().spousePatioSpot);
		}

		// Token: 0x0600125A RID: 4698 RVA: 0x000D9220 File Offset: 0x000D7420
		public void setUpForOutdoorPatioActivity()
		{
			Vector2 patio_location = this.GetSpousePatioPosition();
			if (NPC.checkTileOccupancyForSpouse(Game1.getFarm(), patio_location, ""))
			{
				return;
			}
			Game1.warpCharacter(this, "Farm", patio_location);
			this.popOffAnyNonEssentialItems();
			this.currentMarriageDialogue.Clear();
			this.addMarriageDialogue("MarriageDialogue", "patio_" + base.Name, false, Array.Empty<string>());
			this.setTilePosition((int)patio_location.X, (int)patio_location.Y);
			this.shouldPlaySpousePatioAnimation.Value = true;
		}

		// Token: 0x0600125B RID: 4699 RVA: 0x000D92A8 File Offset: 0x000D74A8
		private void doPlaySpousePatioAnimation()
		{
			CharacterData data = this.GetData();
			CharacterSpousePatioData patioData = (data != null) ? data.SpousePatio : null;
			if (patioData == null)
			{
				return;
			}
			List<int[]> frames = patioData.SpriteAnimationFrames;
			if (frames != null && frames.Count > 0)
			{
				this.drawOffset = Utility.PointToVector2(patioData.SpriteAnimationPixelOffset);
				this.Sprite.ClearAnimation();
				for (int i = 0; i < frames.Count; i++)
				{
					int[] frame = frames[i];
					if (frame != null && frame.Length != 0)
					{
						int index = frame[0];
						int duration = ArgUtility.HasIndex<int>(frame, 1) ? frame[1] : 100;
						this.Sprite.AddFrame(new FarmerSprite.AnimationFrame(index, duration, 0, false, false, null, false, 0));
					}
				}
			}
		}

		// Token: 0x0600125C RID: 4700 RVA: 0x000D934C File Offset: 0x000D754C
		public virtual bool hasDarkSkin()
		{
			if (this.IsVillager)
			{
				CharacterData data = this.GetData();
				return data != null && data.IsDarkSkinned;
			}
			return false;
		}

		// Token: 0x0600125D RID: 4701 RVA: 0x000D936C File Offset: 0x000D756C
		public bool isAdoptionSpouse()
		{
			Farmer spouse = this.getSpouse();
			if (spouse == null)
			{
				return false;
			}
			CharacterData data = this.GetData();
			string isAdoptionSpouse = (data != null) ? data.SpouseAdopts : null;
			if (isAdoptionSpouse != null)
			{
				return GameStateQuery.CheckConditions(isAdoptionSpouse, base.currentLocation, spouse, null, null, null, null);
			}
			return this.Gender == spouse.Gender;
		}

		// Token: 0x0600125E RID: 4702 RVA: 0x000D93BC File Offset: 0x000D75BC
		public bool canGetPregnant()
		{
			if (this is Horse || base.Name.Equals("Krobus") || this.isRoommate() || this.IsInvisible)
			{
				return false;
			}
			Farmer spouse = this.getSpouse();
			if (spouse == null || spouse.divorceTonight.Value)
			{
				return false;
			}
			int heartsWithSpouse = spouse.getFriendshipHeartLevelForNPC(base.Name);
			Friendship friendship = spouse.GetSpouseFriendship();
			List<Child> kids = spouse.getChildren();
			this.defaultMap.Value = spouse.homeLocation.Value;
			FarmHouse farmHouse = Utility.getHomeOfFarmer(spouse);
			return farmHouse.cribStyle.Value > 0 && (farmHouse.upgradeLevel >= 2 && friendship.DaysUntilBirthing < 0 && heartsWithSpouse >= 10 && spouse.GetDaysMarried() >= 7) && (kids.Count == 0 || (kids.Count < 2 && kids[0].Age > 2));
		}

		// Token: 0x0600125F RID: 4703 RVA: 0x000D94A0 File Offset: 0x000D76A0
		public void marriageDuties()
		{
			Farmer spouse = this.getSpouse();
			if (spouse != null)
			{
				this.shouldSayMarriageDialogue.Value = true;
				this.DefaultMap = spouse.homeLocation.Value;
				FarmHouse farmHouse = Game1.RequireLocation<FarmHouse>(spouse.homeLocation.Value, false);
				Random r = Utility.CreateDaySaveRandom((double)spouse.UniqueMultiplayerID, 0.0, 0.0);
				int heartsWithSpouse = spouse.getFriendshipHeartLevelForNPC(base.Name);
				if (Game1.IsMasterGame && (base.currentLocation == null || !base.currentLocation.Equals(farmHouse)))
				{
					Game1.warpCharacter(this, spouse.homeLocation.Value, farmHouse.getSpouseBedSpot(base.Name));
				}
				if (Game1.isRaining)
				{
					this.marriageDefaultDialogue.Value = new MarriageDialogueReference("MarriageDialogue", "Rainy_Day_" + r.Next(5).ToString(), false, Array.Empty<string>());
				}
				else
				{
					this.marriageDefaultDialogue.Value = new MarriageDialogueReference("MarriageDialogue", "Indoor_Day_" + r.Next(5).ToString(), false, Array.Empty<string>());
				}
				this.currentMarriageDialogue.Add(new MarriageDialogueReference(this.marriageDefaultDialogue.Value.DialogueFile, this.marriageDefaultDialogue.Value.DialogueKey, this.marriageDefaultDialogue.Value.IsGendered, this.marriageDefaultDialogue.Value.Substitutions));
				if (spouse.GetSpouseFriendship().DaysUntilBirthing == 0)
				{
					this.setTilePosition(farmHouse.getKitchenStandingSpot());
					this.currentMarriageDialogue.Clear();
					return;
				}
				if (this.daysAfterLastBirth >= 0)
				{
					this.daysAfterLastBirth--;
					int num = this.getSpouse().getChildrenCount();
					if (num == 1)
					{
						this.setTilePosition(farmHouse.getKitchenStandingSpot());
						if (!this.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4406", false, Array.Empty<string>()), farmHouse, false))
						{
							this.currentMarriageDialogue.Clear();
							this.addMarriageDialogue("MarriageDialogue", "OneKid_" + r.Next(4).ToString(), false, Array.Empty<string>());
						}
						return;
					}
					if (num == 2)
					{
						this.setTilePosition(farmHouse.getKitchenStandingSpot());
						if (!this.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4406", false, Array.Empty<string>()), farmHouse, false))
						{
							this.currentMarriageDialogue.Clear();
							this.addMarriageDialogue("MarriageDialogue", "TwoKids_" + r.Next(4).ToString(), false, Array.Empty<string>());
						}
						return;
					}
				}
				this.setTilePosition(farmHouse.getKitchenStandingSpot());
				if (!this.sleptInBed.Value)
				{
					this.currentMarriageDialogue.Clear();
					this.addMarriageDialogue("MarriageDialogue", "NoBed_" + r.Next(4).ToString(), false, Array.Empty<string>());
					return;
				}
				if (this.tryToGetMarriageSpecificDialogue(Game1.currentSeason + "_" + Game1.dayOfMonth.ToString()) != null)
				{
					if (spouse != null)
					{
						this.currentMarriageDialogue.Clear();
						this.addMarriageDialogue("MarriageDialogue", Game1.currentSeason + "_" + Game1.dayOfMonth.ToString(), false, Array.Empty<string>());
					}
					return;
				}
				if (this.Schedule != null)
				{
					if (this.ScheduleKey == "marriage_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth))
					{
						this.currentMarriageDialogue.Clear();
						this.addMarriageDialogue("MarriageDialogue", "funLeave_" + base.Name, false, Array.Empty<string>());
						return;
					}
					if (this.ScheduleKey == "marriageJob")
					{
						this.currentMarriageDialogue.Clear();
						this.addMarriageDialogue("MarriageDialogue", "jobLeave_" + base.Name, false, Array.Empty<string>());
					}
					return;
				}
				else
				{
					if (!Game1.isRaining && !Game1.IsWinter && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Sat") && spouse == Game1.MasterPlayer && !base.Name.Equals("Krobus"))
					{
						this.setUpForOutdoorPatioActivity();
						return;
					}
					int minHeartLevelForNegativeDialogue = 12;
					int totalDays = Game1.Date.TotalDays;
					WorldDate lastGiftDate = spouse.GetSpouseFriendship().LastGiftDate;
					int? num2 = totalDays - ((lastGiftDate != null) ? new int?(lastGiftDate.TotalDays) : null);
					int num = 1;
					if (num2.GetValueOrDefault() <= num & num2 != null)
					{
						minHeartLevelForNegativeDialogue--;
					}
					if (this.wasKissedYesterday)
					{
						minHeartLevelForNegativeDialogue--;
					}
					if (spouse.GetDaysMarried() > 7 && r.NextDouble() < (double)(1f - (float)Math.Max(1, heartsWithSpouse) / (float)minHeartLevelForNegativeDialogue))
					{
						Furniture f = farmHouse.getRandomFurniture(r);
						if (f != null && f.isGroundFurniture() && f.furniture_type.Value != 15 && f.furniture_type.Value != 12)
						{
							Point p = new Point((int)f.tileLocation.X - 1, (int)f.tileLocation.Y);
							if (farmHouse.CanItemBePlacedHere(new Vector2((float)p.X, (float)p.Y), false, CollisionMask.All, ~CollisionMask.Objects, false, false))
							{
								this.setTilePosition(p);
								this.faceDirection(1);
								switch (r.Next(10))
								{
								case 0:
									this.currentMarriageDialogue.Clear();
									this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4420", false, Array.Empty<string>());
									return;
								case 1:
									this.currentMarriageDialogue.Clear();
									this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4421", false, Array.Empty<string>());
									return;
								case 2:
									this.currentMarriageDialogue.Clear();
									this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4422", true, Array.Empty<string>());
									return;
								case 3:
									this.currentMarriageDialogue.Clear();
									this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4423", false, Array.Empty<string>());
									return;
								case 4:
									this.currentMarriageDialogue.Clear();
									this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4424", false, Array.Empty<string>());
									return;
								case 5:
									this.currentMarriageDialogue.Clear();
									this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4425", false, Array.Empty<string>());
									return;
								case 6:
									this.currentMarriageDialogue.Clear();
									this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4426", false, Array.Empty<string>());
									return;
								case 7:
									if (this.Gender == Gender.Female)
									{
										this.currentMarriageDialogue.Clear();
										this.addMarriageDialogue("Strings\\StringsFromCSFiles", r.Choose("NPC.cs.4427", "NPC.cs.4429"), false, Array.Empty<string>());
										return;
									}
									this.currentMarriageDialogue.Clear();
									this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4431", false, Array.Empty<string>());
									return;
								case 8:
									this.currentMarriageDialogue.Clear();
									this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4432", false, Array.Empty<string>());
									return;
								case 9:
									this.currentMarriageDialogue.Clear();
									this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4433", false, Array.Empty<string>());
									return;
								default:
									return;
								}
							}
						}
						this.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4406", false, Array.Empty<string>()), farmHouse, true);
						return;
					}
					Friendship friendship = spouse.GetSpouseFriendship();
					if (friendship.DaysUntilBirthing != -1 && friendship.DaysUntilBirthing <= 7 && r.NextBool())
					{
						if (this.isAdoptionSpouse())
						{
							this.setTilePosition(farmHouse.getKitchenStandingSpot());
							if (!this.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4439", false, Array.Empty<string>()), farmHouse, false))
							{
								if (r.NextBool())
								{
									this.currentMarriageDialogue.Clear();
								}
								if (r.NextBool())
								{
									this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4440", false, new string[]
									{
										this.getSpouse().displayName
									});
									return;
								}
								this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4441", false, new string[]
								{
									"%endearment"
								});
								return;
							}
						}
						else if (this.Gender == Gender.Female)
						{
							this.setTilePosition(farmHouse.getKitchenStandingSpot());
							if (!this.spouseObstacleCheck(r.NextBool() ? new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4442", false, Array.Empty<string>()) : new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4443", false, Array.Empty<string>()), farmHouse, false))
							{
								if (r.NextBool())
								{
									this.currentMarriageDialogue.Clear();
								}
								this.currentMarriageDialogue.Add(r.NextBool() ? new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4444", false, new string[]
								{
									this.getSpouse().displayName
								}) : new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4445", false, new string[]
								{
									"%endearment"
								}));
								return;
							}
						}
						else
						{
							this.setTilePosition(farmHouse.getKitchenStandingSpot());
							if (!this.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4446", true, Array.Empty<string>()), farmHouse, false))
							{
								if (r.NextBool())
								{
									this.currentMarriageDialogue.Clear();
								}
								this.currentMarriageDialogue.Add(r.NextBool() ? new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4447", true, new string[]
								{
									this.getSpouse().displayName
								}) : new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4448", false, new string[]
								{
									"%endearment"
								}));
							}
						}
						return;
					}
					if (r.NextDouble() < 0.07)
					{
						num = this.getSpouse().getChildrenCount();
						if (num == 1)
						{
							this.setTilePosition(farmHouse.getKitchenStandingSpot());
							if (!this.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4449", true, Array.Empty<string>()), farmHouse, false))
							{
								this.currentMarriageDialogue.Clear();
								this.addMarriageDialogue("MarriageDialogue", "OneKid_" + r.Next(4).ToString(), false, Array.Empty<string>());
							}
							return;
						}
						if (num == 2)
						{
							this.setTilePosition(farmHouse.getKitchenStandingSpot());
							if (!this.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4452", true, Array.Empty<string>()), farmHouse, false))
							{
								this.currentMarriageDialogue.Clear();
								this.addMarriageDialogue("MarriageDialogue", "TwoKids_" + r.Next(4).ToString(), false, Array.Empty<string>());
							}
							return;
						}
					}
					Farm farm = Game1.getFarm();
					if (this.currentMarriageDialogue.Count > 0 && this.currentMarriageDialogue[0].IsItemGrabDialogue(this))
					{
						this.setTilePosition(farmHouse.getKitchenStandingSpot());
						this.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4455", true, Array.Empty<string>()), farmHouse, false);
						return;
					}
					if (!Game1.isRaining && r.NextDouble() < 0.4 && !NPC.checkTileOccupancyForSpouse(farm, Utility.PointToVector2(farmHouse.getPorchStandingSpot()), "") && !base.Name.Equals("Krobus"))
					{
						bool filledBowl = false;
						if (!NPC.hasSomeoneFedThePet)
						{
							foreach (Building building in farm.buildings)
							{
								PetBowl bowl = building as PetBowl;
								if (bowl != null && !bowl.watered.Value)
								{
									filledBowl = true;
									bowl.watered.Value = true;
									NPC.hasSomeoneFedThePet = true;
								}
							}
						}
						if (r.NextDouble() < 0.6 && Game1.season != Season.Winter && !NPC.hasSomeoneWateredCrops)
						{
							Vector2 origin = Vector2.Zero;
							int tries = 0;
							bool foundWatered = false;
							while (tries < Math.Min(50, farm.terrainFeatures.Length) && origin.Equals(Vector2.Zero))
							{
								Vector2 tile;
								TerrainFeature feature;
								if (Utility.TryGetRandom<Vector2, TerrainFeature, NetRef<TerrainFeature>, SerializableDictionary<Vector2, TerrainFeature>, NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>>(farm.terrainFeatures, out tile, out feature, null))
								{
									HoeDirt dirt = feature as HoeDirt;
									if (dirt != null && dirt.needsWatering())
									{
										if (!dirt.isWatered())
										{
											origin = tile;
										}
										else
										{
											foundWatered = true;
										}
									}
								}
								tries++;
							}
							if (!origin.Equals(Vector2.Zero))
							{
								foreach (Vector2 currentPosition in new Microsoft.Xna.Framework.Rectangle((int)origin.X - 30, (int)origin.Y - 30, 60, 60).GetVectors())
								{
									TerrainFeature terrainFeature;
									if (farm.isTileOnMap(currentPosition) && farm.terrainFeatures.TryGetValue(currentPosition, out terrainFeature))
									{
										HoeDirt dirt2 = terrainFeature as HoeDirt;
										if (dirt2 != null && Game1.IsMasterGame && dirt2.needsWatering())
										{
											dirt2.state.Value = 1;
										}
									}
								}
								this.faceDirection(2);
								this.currentMarriageDialogue.Clear();
								this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4462", true, Array.Empty<string>());
								if (filledBowl)
								{
									if (Utility.getAllPets().Count > 1 && Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en)
									{
										this.addMarriageDialogue("Strings\\StringsFromCSFiles", "MultiplePetBowls_watered", false, new string[]
										{
											Game1.player.getPetDisplayName()
										});
									}
									else
									{
										this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, new string[]
										{
											Game1.player.getPetDisplayName()
										});
									}
								}
								this.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5).ToString(), false, Array.Empty<string>());
								NPC.hasSomeoneWateredCrops = true;
							}
							else
							{
								this.faceDirection(2);
								if (foundWatered)
								{
									this.currentMarriageDialogue.Clear();
									if (Game1.gameMode == 6)
									{
										if (r.NextBool())
										{
											this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4465", false, new string[]
											{
												"%endearment"
											});
										}
										else
										{
											this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4466", false, new string[]
											{
												"%endearment"
											});
											this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4462", true, Array.Empty<string>());
											if (filledBowl)
											{
												if (Utility.getAllPets().Count > 1 && Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en)
												{
													this.addMarriageDialogue("Strings\\StringsFromCSFiles", "MultiplePetBowls_watered", false, new string[]
													{
														Game1.player.getPetDisplayName()
													});
												}
												else
												{
													this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, new string[]
													{
														Game1.player.getPetDisplayName()
													});
												}
											}
										}
									}
									else
									{
										this.currentMarriageDialogue.Clear();
										this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4470", true, Array.Empty<string>());
									}
								}
								else
								{
									this.currentMarriageDialogue.Clear();
									this.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5).ToString(), false, Array.Empty<string>());
								}
							}
						}
						else
						{
							if (r.NextDouble() < 0.6 && !NPC.hasSomeoneFedTheAnimals)
							{
								bool fedAnything = false;
								foreach (Building b in farm.buildings)
								{
									AnimalHouse animalHouse = b.GetIndoors() as AnimalHouse;
									if (animalHouse != null && b.daysOfConstructionLeft.Value <= 0 && Game1.IsMasterGame)
									{
										animalHouse.feedAllAnimals();
										fedAnything = true;
									}
								}
								this.faceDirection(2);
								if (fedAnything)
								{
									NPC.hasSomeoneFedTheAnimals = true;
									this.currentMarriageDialogue.Clear();
									this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4474", true, Array.Empty<string>());
									if (filledBowl)
									{
										if (Utility.getAllPets().Count > 1 && Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en)
										{
											this.addMarriageDialogue("Strings\\StringsFromCSFiles", "MultiplePetBowls_watered", false, new string[]
											{
												Game1.player.getPetDisplayName()
											});
										}
										else
										{
											this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, new string[]
											{
												Game1.player.getPetDisplayName()
											});
										}
									}
									this.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5).ToString(), false, Array.Empty<string>());
								}
								else
								{
									this.currentMarriageDialogue.Clear();
									this.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5).ToString(), false, Array.Empty<string>());
								}
								if (!Game1.IsMasterGame)
								{
									goto IL_12A5;
								}
								using (List<Building>.Enumerator enumerator = farm.buildings.GetEnumerator())
								{
									while (enumerator.MoveNext())
									{
										Building building2 = enumerator.Current;
										PetBowl bowl2 = building2 as PetBowl;
										if (bowl2 != null && !bowl2.watered.Value)
										{
											filledBowl = true;
											bowl2.watered.Value = true;
											NPC.hasSomeoneFedThePet = true;
										}
									}
									goto IL_12A5;
								}
							}
							if (!NPC.hasSomeoneRepairedTheFences)
							{
								int tries2 = 0;
								this.faceDirection(2);
								Vector2 origin2 = Vector2.Zero;
								while (tries2 < Math.Min(50, farm.objects.Length) && origin2.Equals(Vector2.Zero))
								{
									Vector2 tile2;
									Object obj;
									if (Utility.TryGetRandom(farm.objects, out tile2, out obj, null) && obj is Fence)
									{
										origin2 = tile2;
									}
									tries2++;
								}
								if (!origin2.Equals(Vector2.Zero))
								{
									foreach (Vector2 currentPosition2 in new Microsoft.Xna.Framework.Rectangle((int)origin2.X - 10, (int)origin2.Y - 10, 20, 20).GetVectors())
									{
										Object obj2;
										if (farm.isTileOnMap(currentPosition2) && farm.objects.TryGetValue(currentPosition2, out obj2))
										{
											Fence fence = obj2 as Fence;
											if (fence != null && Game1.IsMasterGame)
											{
												fence.repair();
											}
										}
									}
									this.currentMarriageDialogue.Clear();
									this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4481", true, Array.Empty<string>());
									if (filledBowl)
									{
										if (Utility.getAllPets().Count > 1 && Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en)
										{
											this.addMarriageDialogue("Strings\\StringsFromCSFiles", "MultiplePetBowls_watered", false, new string[]
											{
												Game1.player.getPetDisplayName()
											});
										}
										else
										{
											this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4463", false, new string[]
											{
												Game1.player.getPetDisplayName()
											});
										}
									}
									this.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5).ToString(), false, Array.Empty<string>());
									NPC.hasSomeoneRepairedTheFences = true;
								}
								else
								{
									this.currentMarriageDialogue.Clear();
									this.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5).ToString(), false, Array.Empty<string>());
								}
							}
						}
						IL_12A5:
						Game1.warpCharacter(this, "Farm", farmHouse.getPorchStandingSpot());
						this.popOffAnyNonEssentialItems();
						this.faceDirection(2);
						return;
					}
					if (base.Name.Equals("Krobus") && Game1.isRaining && r.NextDouble() < 0.4 && !NPC.checkTileOccupancyForSpouse(farm, Utility.PointToVector2(farmHouse.getPorchStandingSpot()), ""))
					{
						this.addMarriageDialogue("MarriageDialogue", "Outdoor_" + r.Next(5).ToString(), false, Array.Empty<string>());
						Game1.warpCharacter(this, "Farm", farmHouse.getPorchStandingSpot());
						this.popOffAnyNonEssentialItems();
						this.faceDirection(2);
						return;
					}
					if (spouse.GetDaysMarried() >= 1 && r.NextDouble() < 0.045)
					{
						if (r.NextDouble() < 0.75)
						{
							Point spot = farmHouse.getRandomOpenPointInHouse(r, 1, 30);
							Furniture new_furniture;
							try
							{
								new_furniture = ItemRegistry.Create<Furniture>(Utility.getRandomSingleTileFurniture(r), 1, 0, false).SetPlacement(spot, 0);
							}
							catch
							{
								new_furniture = null;
							}
							if (new_furniture == null || spot.X <= 0 || !farmHouse.CanItemBePlacedHere(new Vector2((float)(spot.X - 1), (float)spot.Y), false, CollisionMask.All, ~CollisionMask.Objects, false, false))
							{
								this.setTilePosition(farmHouse.getKitchenStandingSpot());
								this.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4490", false, Array.Empty<string>()), farmHouse, false);
								return;
							}
							farmHouse.furniture.Add(new_furniture);
							this.setTilePosition(spot.X - 1, spot.Y);
							this.faceDirection(1);
							this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4486", false, new string[]
							{
								"%endearmentlower"
							});
							if (Game1.random.NextBool())
							{
								this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4488", true, Array.Empty<string>());
								return;
							}
							this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4489", false, Array.Empty<string>());
							return;
						}
						else
						{
							Point p2 = farmHouse.getRandomOpenPointInHouse(r, 0, 30);
							if (p2.X > 0)
							{
								this.setTilePosition(p2.X, p2.Y);
								this.faceDirection(0);
								if (r.NextBool())
								{
									string wall = farmHouse.GetWallpaperID(p2.X, p2.Y);
									if (wall != null)
									{
										Random random = r;
										CharacterData data = this.GetData();
										string wallpaperId = random.ChooseFrom((data != null) ? data.SpouseWallpapers : null) ?? r.Next(112).ToString();
										farmHouse.SetWallpaper(wallpaperId, wall);
										this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4496", false, Array.Empty<string>());
										return;
									}
								}
								else
								{
									string floor = farmHouse.getFloorRoomIdAt(p2);
									if (floor != null)
									{
										Random random2 = r;
										CharacterData data2 = this.GetData();
										string floorId = random2.ChooseFrom((data2 != null) ? data2.SpouseFloors : null) ?? r.Next(40).ToString();
										farmHouse.SetFloor(floorId, floor);
										this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4497", false, Array.Empty<string>());
										return;
									}
								}
							}
						}
					}
					else
					{
						if (Game1.isRaining && r.NextDouble() < 0.08 && heartsWithSpouse < 11 && spouse.GetDaysMarried() > 7 && base.Name != "Krobus")
						{
							foreach (Furniture f2 in farmHouse.furniture)
							{
								if (f2.furniture_type.Value == 13 && farmHouse.CanItemBePlacedHere(new Vector2((float)((int)f2.tileLocation.X), (float)((int)f2.tileLocation.Y + 1)), false, CollisionMask.All, ~CollisionMask.Objects, false, false))
								{
									this.setTilePosition((int)f2.tileLocation.X, (int)f2.tileLocation.Y + 1);
									this.faceDirection(0);
									this.currentMarriageDialogue.Clear();
									this.addMarriageDialogue("Strings\\StringsFromCSFiles", "NPC.cs.4498", true, Array.Empty<string>());
									return;
								}
							}
							this.spouseObstacleCheck(new MarriageDialogueReference("Strings\\StringsFromCSFiles", "NPC.cs.4499", false, Array.Empty<string>()), farmHouse, true);
							return;
						}
						if (r.NextDouble() < 0.45)
						{
							Vector2 spot2 = Utility.PointToVector2(farmHouse.GetSpouseRoomSpot());
							this.setTilePosition((int)spot2.X, (int)spot2.Y);
							this.faceDirection(0);
							this.setSpouseRoomMarriageDialogue();
							if (this.name.Value == "Sebastian" && Game1.netWorldState.Value.hasWorldStateID("sebastianFrog"))
							{
								Point frog_spot = farmHouse.GetSpouseRoomCorner();
								frog_spot.X += 2;
								frog_spot.Y += 5;
								this.setTilePosition(frog_spot);
								this.faceDirection(2);
								return;
							}
						}
						else
						{
							this.setTilePosition(farmHouse.getKitchenStandingSpot());
							this.faceDirection(0);
							if (r.NextDouble() < 0.2)
							{
								this.setRandomAfternoonMarriageDialogue(Game1.timeOfDay, farmHouse, false);
							}
						}
					}
				}
			}
		}

		// Token: 0x06001260 RID: 4704 RVA: 0x000DACBC File Offset: 0x000D8EBC
		public virtual void popOffAnyNonEssentialItems()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			if (base.currentLocation != null)
			{
				Point tile = base.TilePoint;
				Object tile_object = base.currentLocation.getObjectAtTile(tile.X, tile.Y, false);
				if (tile_object != null && (tile_object.QualifiedItemId == "(O)93" || tile_object is Torch))
				{
					Vector2 tile_position = tile_object.TileLocation;
					tile_object.performRemoveAction();
					base.currentLocation.objects.Remove(tile_position);
					tile_object.dropItem(base.currentLocation, tile_position * 64f, tile_position * 64f);
				}
			}
		}

		// Token: 0x06001261 RID: 4705 RVA: 0x000DAD57 File Offset: 0x000D8F57
		public static bool checkTileOccupancyForSpouse(GameLocation location, Vector2 point, string characterToIgnore = "")
		{
			return location == null || location.IsTileOccupiedBy(point, ~(CollisionMask.Characters | CollisionMask.Farmers), CollisionMask.All, false);
		}

		// Token: 0x06001262 RID: 4706 RVA: 0x000DAD70 File Offset: 0x000D8F70
		public void addMarriageDialogue(string dialogue_file, string dialogue_key, bool gendered = false, params string[] substitutions)
		{
			this.shouldSayMarriageDialogue.Value = true;
			this.currentMarriageDialogue.Add(new MarriageDialogueReference(dialogue_file, dialogue_key, gendered, substitutions));
		}

		// Token: 0x06001263 RID: 4707 RVA: 0x000DAD93 File Offset: 0x000D8F93
		public void clearTextAboveHead()
		{
			this.textAboveHead = null;
			this.textAboveHeadPreTimer = -1;
			this.textAboveHeadTimer = -1;
		}

		// Token: 0x06001264 RID: 4708 RVA: 0x000DADAA File Offset: 0x000D8FAA
		[Obsolete("Use IsVillager instead.")]
		public bool isVillager()
		{
			return this.IsVillager;
		}

		// Token: 0x06001265 RID: 4709 RVA: 0x000DADB2 File Offset: 0x000D8FB2
		public override bool shouldCollideWithBuildingLayer(GameLocation location)
		{
			return (this.isMarried() && (this.Schedule == null || location is FarmHouse)) || base.shouldCollideWithBuildingLayer(location);
		}

		// Token: 0x06001266 RID: 4710 RVA: 0x000DADD8 File Offset: 0x000D8FD8
		public virtual void arriveAtFarmHouse(FarmHouse farmHouse)
		{
			if (!Game1.newDay && this.isMarried() && Game1.timeOfDay > 630 && base.TilePoint != farmHouse.getSpouseBedSpot(this.name.Value))
			{
				this.setTilePosition(farmHouse.getEntryLocation());
				this.ignoreScheduleToday = true;
				this.temporaryController = null;
				this.controller = null;
				if (Game1.timeOfDay >= 2130)
				{
					Point bed_spot = farmHouse.getSpouseBedSpot(this.name.Value);
					bool found_bed = farmHouse.GetSpouseBed() != null;
					PathFindController.endBehavior end_behavior = null;
					if (found_bed)
					{
						end_behavior = new PathFindController.endBehavior(FarmHouse.spouseSleepEndFunction);
					}
					this.controller = new PathFindController(this, farmHouse, bed_spot, 0, end_behavior);
					if (this.controller.pathToEndPoint == null || !found_bed)
					{
						goto IL_150;
					}
					using (List<Furniture>.Enumerator enumerator = farmHouse.furniture.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Furniture furniture = enumerator.Current;
							BedFurniture bed = furniture as BedFurniture;
							if (bed != null && furniture.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(bed_spot.X * 64, bed_spot.Y * 64, 64, 64)))
							{
								bed.ReserveForNPC();
								break;
							}
						}
						goto IL_150;
					}
				}
				this.controller = new PathFindController(this, farmHouse, farmHouse.getKitchenStandingSpot(), 0);
				IL_150:
				if (this.controller.pathToEndPoint == null)
				{
					base.willDestroyObjectsUnderfoot = true;
					this.controller = new PathFindController(this, farmHouse, farmHouse.getKitchenStandingSpot(), 0);
					this.setNewDialogue(this.TryGetDialogue("SpouseFarmhouseClutter") ?? new Dialogue(this, "Strings\\StringsFromCSFiles:NPC.cs.4500", true), false, false);
				}
				else if (Game1.timeOfDay > 1300)
				{
					if (this.ScheduleKey == "marriage_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth))
					{
						this.setNewDialogue("MarriageDialogue", "funReturn_", true);
					}
					else if (this.ScheduleKey == "marriageJob")
					{
						this.setNewDialogue("MarriageDialogue", "jobReturn_", false);
					}
					else if (Game1.timeOfDay < 1800)
					{
						this.setRandomAfternoonMarriageDialogue(Game1.timeOfDay, base.currentLocation, true);
					}
				}
				if (Game1.currentLocation == farmHouse)
				{
					Game1.currentLocation.playSound("doorClose", null, null, SoundContext.NPC);
				}
			}
		}

		// Token: 0x06001267 RID: 4711 RVA: 0x000DB044 File Offset: 0x000D9244
		public Farmer getSpouse()
		{
			foreach (Farmer f in Game1.getAllFarmers())
			{
				if (f.spouse != null && f.spouse == base.Name)
				{
					return f;
				}
			}
			return null;
		}

		// Token: 0x06001268 RID: 4712 RVA: 0x000DB0AC File Offset: 0x000D92AC
		public string getTermOfSpousalEndearment(bool happy = true)
		{
			Farmer spouse = this.getSpouse();
			if (spouse == null)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4517");
			}
			if (this.isRoommate())
			{
				return spouse.displayName;
			}
			if (spouse.getFriendshipHeartLevelForNPC(base.Name) < 9)
			{
				return spouse.displayName;
			}
			if (!happy)
			{
				int num = Game1.random.Next(2);
				if (num == 0)
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4517");
				}
				if (num != 1)
				{
					return spouse.displayName;
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4518");
			}
			else if (Game1.random.NextDouble() < 0.08)
			{
				switch (Game1.random.Next(8))
				{
				case 0:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4507");
				case 1:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4508");
				case 2:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4509");
				case 3:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4510");
				case 4:
					if (!spouse.IsMale)
					{
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4512");
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4511");
				case 5:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4513");
				case 6:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4514");
				default:
					if (!spouse.IsMale)
					{
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4516");
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4515");
				}
			}
			else
			{
				switch (Game1.random.Next(5))
				{
				case 0:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4519");
				case 1:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4518");
				case 2:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4517");
				case 3:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4522");
				default:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4523");
				}
			}
		}

		// Token: 0x06001269 RID: 4713 RVA: 0x000DB2AC File Offset: 0x000D94AC
		public bool spouseObstacleCheck(MarriageDialogueReference backToBedMessage, GameLocation currentLocation, bool force = false)
		{
			if (force || NPC.checkTileOccupancyForSpouse(currentLocation, base.Tile, base.Name))
			{
				Game1.warpCharacter(this, this.defaultMap.Value, Game1.RequireLocation<FarmHouse>(this.defaultMap.Value, false).getSpouseBedSpot(this.name.Value));
				this.faceDirection(1);
				this.currentMarriageDialogue.Clear();
				this.currentMarriageDialogue.Add(backToBedMessage);
				this.shouldSayMarriageDialogue.Value = true;
				return true;
			}
			return false;
		}

		// Token: 0x0600126A RID: 4714 RVA: 0x000DB32F File Offset: 0x000D952F
		public void setTilePosition(Point p)
		{
			this.setTilePosition(p.X, p.Y);
		}

		// Token: 0x0600126B RID: 4715 RVA: 0x000DB343 File Offset: 0x000D9543
		public void setTilePosition(int x, int y)
		{
			base.Position = new Vector2((float)(x * 64), (float)(y * 64));
		}

		// Token: 0x0600126C RID: 4716 RVA: 0x000DB35C File Offset: 0x000D955C
		private void clintHammerSound(Farmer who)
		{
			base.currentLocation.playSound("hammer", new Vector2?(base.Tile), null, SoundContext.Default);
		}

		// Token: 0x0600126D RID: 4717 RVA: 0x000DB390 File Offset: 0x000D9590
		private void robinHammerSound(Farmer who)
		{
			if (Game1.currentLocation.Equals(base.currentLocation) && Utility.isOnScreen(base.Position, 256))
			{
				Game1.playSound((Game1.random.NextDouble() < 0.1) ? "clank" : "axchop", null);
				this.shakeTimer = 250;
			}
		}

		// Token: 0x0600126E RID: 4718 RVA: 0x000DB3FC File Offset: 0x000D95FC
		private void robinVariablePause(Farmer who)
		{
			if (Game1.random.NextDouble() < 0.4)
			{
				this.Sprite.CurrentAnimation[this.Sprite.currentAnimationIndex] = new FarmerSprite.AnimationFrame(27, 300, false, false, new AnimatedSprite.endOfAnimationBehavior(this.robinVariablePause), false);
				return;
			}
			if (Game1.random.NextDouble() < 0.25)
			{
				this.Sprite.CurrentAnimation[this.Sprite.currentAnimationIndex] = new FarmerSprite.AnimationFrame(23, Game1.random.Next(500, 4000), false, false, new AnimatedSprite.endOfAnimationBehavior(this.robinVariablePause), false);
				return;
			}
			this.Sprite.CurrentAnimation[this.Sprite.currentAnimationIndex] = new FarmerSprite.AnimationFrame(27, Game1.random.Next(1000, 4000), false, false, new AnimatedSprite.endOfAnimationBehavior(this.robinVariablePause), false);
		}

		// Token: 0x0600126F RID: 4719 RVA: 0x000DB4F8 File Offset: 0x000D96F8
		public void randomSquareMovement(GameTime time)
		{
			Microsoft.Xna.Framework.Rectangle boundingBox = this.GetBoundingBox();
			boundingBox.Inflate(2, 2);
			Microsoft.Xna.Framework.Rectangle endRect = new Microsoft.Xna.Framework.Rectangle((int)this.nextSquarePosition.X * 64, (int)this.nextSquarePosition.Y * 64, 64, 64);
			if (this.nextSquarePosition.Equals(Vector2.Zero))
			{
				this.squarePauseAccumulation = 0;
				this.squarePauseTotal = Game1.random.Next(6000 + this.squarePauseOffset, 12000 + this.squarePauseOffset);
				this.nextSquarePosition = new Vector2((float)(this.lastCrossroad.X / 64 - this.lengthOfWalkingSquareX / 2 + Game1.random.Next(this.lengthOfWalkingSquareX)), (float)(this.lastCrossroad.Y / 64 - this.lengthOfWalkingSquareY / 2 + Game1.random.Next(this.lengthOfWalkingSquareY)));
			}
			else if (endRect.Contains(boundingBox))
			{
				this.Halt();
				if (this.squareMovementFacingPreference != -1)
				{
					this.faceDirection(this.squareMovementFacingPreference);
				}
				this.isCharging = false;
				base.speed = 2;
			}
			else if (boundingBox.Left <= endRect.Left)
			{
				this.SetMovingOnlyRight();
			}
			else if (boundingBox.Right >= endRect.Right)
			{
				this.SetMovingOnlyLeft();
			}
			else if (boundingBox.Top <= endRect.Top)
			{
				this.SetMovingOnlyDown();
			}
			else if (boundingBox.Bottom >= endRect.Bottom)
			{
				this.SetMovingOnlyUp();
			}
			this.squarePauseAccumulation += time.ElapsedGameTime.Milliseconds;
			if (this.squarePauseAccumulation >= this.squarePauseTotal && endRect.Contains(boundingBox))
			{
				this.nextSquarePosition = Vector2.Zero;
				this.isCharging = false;
				base.speed = 2;
			}
		}

		// Token: 0x06001270 RID: 4720 RVA: 0x000DB6C4 File Offset: 0x000D98C4
		public void returnToEndPoint()
		{
			Microsoft.Xna.Framework.Rectangle boundingBox = this.GetBoundingBox();
			boundingBox.Inflate(2, 2);
			if (boundingBox.Left <= this.lastCrossroad.Left)
			{
				this.SetMovingOnlyRight();
			}
			else if (boundingBox.Right >= this.lastCrossroad.Right)
			{
				this.SetMovingOnlyLeft();
			}
			else if (boundingBox.Top <= this.lastCrossroad.Top)
			{
				this.SetMovingOnlyDown();
			}
			else if (boundingBox.Bottom >= this.lastCrossroad.Bottom)
			{
				this.SetMovingOnlyUp();
			}
			boundingBox.Inflate(-2, -2);
			if (this.lastCrossroad.Contains(boundingBox))
			{
				this.isWalkingInSquare = false;
				this.nextSquarePosition = Vector2.Zero;
				this.returningToEndPoint = false;
				this.Halt();
			}
		}

		// Token: 0x06001271 RID: 4721 RVA: 0x000DB787 File Offset: 0x000D9987
		public void SetMovingOnlyUp()
		{
			this.moveUp = true;
			this.moveDown = false;
			this.moveLeft = false;
			this.moveRight = false;
		}

		// Token: 0x06001272 RID: 4722 RVA: 0x000DB7A5 File Offset: 0x000D99A5
		public void SetMovingOnlyRight()
		{
			this.moveUp = false;
			this.moveDown = false;
			this.moveLeft = false;
			this.moveRight = true;
		}

		// Token: 0x06001273 RID: 4723 RVA: 0x000DB7C3 File Offset: 0x000D99C3
		public void SetMovingOnlyDown()
		{
			this.moveUp = false;
			this.moveDown = true;
			this.moveLeft = false;
			this.moveRight = false;
		}

		// Token: 0x06001274 RID: 4724 RVA: 0x000DB7E1 File Offset: 0x000D99E1
		public void SetMovingOnlyLeft()
		{
			this.moveUp = false;
			this.moveDown = false;
			this.moveLeft = true;
			this.moveRight = false;
		}

		// Token: 0x06001275 RID: 4725 RVA: 0x000DB7FF File Offset: 0x000D99FF
		public virtual int getTimeFarmerMustPushBeforePassingThrough()
		{
			return 1500;
		}

		// Token: 0x06001276 RID: 4726 RVA: 0x000DB806 File Offset: 0x000D9A06
		public virtual int getTimeFarmerMustPushBeforeStartShaking()
		{
			return 400;
		}

		// Token: 0x06001277 RID: 4727 RVA: 0x000DB810 File Offset: 0x000D9A10
		public int CompareTo(object obj)
		{
			NPC npc = obj as NPC;
			if (npc != null)
			{
				return npc.id - this.id;
			}
			return 0;
		}

		// Token: 0x06001278 RID: 4728 RVA: 0x000DB836 File Offset: 0x000D9A36
		public virtual void Removed()
		{
		}

		// Token: 0x04000A6B RID: 2667
		public const int minimum_square_pause = 6000;

		// Token: 0x04000A6C RID: 2668
		public const int maximum_square_pause = 12000;

		// Token: 0x04000A6D RID: 2669
		public const int portrait_width = 64;

		// Token: 0x04000A6E RID: 2670
		public const int portrait_height = 64;

		// Token: 0x04000A6F RID: 2671
		public const int portrait_neutral_index = 0;

		// Token: 0x04000A70 RID: 2672
		public const int portrait_happy_index = 1;

		// Token: 0x04000A71 RID: 2673
		public const int portrait_sad_index = 2;

		// Token: 0x04000A72 RID: 2674
		public const int portrait_custom_index = 3;

		// Token: 0x04000A73 RID: 2675
		public const int portrait_blush_index = 4;

		// Token: 0x04000A74 RID: 2676
		public const int portrait_angry_index = 5;

		// Token: 0x04000A75 RID: 2677
		public const int startingFriendship = 0;

		// Token: 0x04000A76 RID: 2678
		public const int defaultSpeed = 2;

		// Token: 0x04000A77 RID: 2679
		public const int maxGiftsPerWeek = 2;

		// Token: 0x04000A78 RID: 2680
		public const int friendshipPointsPerHeartLevel = 250;

		// Token: 0x04000A79 RID: 2681
		public const int maxFriendshipPoints = 2500;

		// Token: 0x04000A7A RID: 2682
		public const int gift_taste_love = 0;

		// Token: 0x04000A7B RID: 2683
		public const int gift_taste_like = 2;

		// Token: 0x04000A7C RID: 2684
		public const int gift_taste_neutral = 8;

		// Token: 0x04000A7D RID: 2685
		public const int gift_taste_dislike = 4;

		// Token: 0x04000A7E RID: 2686
		public const int gift_taste_hate = 6;

		// Token: 0x04000A7F RID: 2687
		public const int gift_taste_stardroptea = 7;

		// Token: 0x04000A80 RID: 2688
		public const int textStyle_shake = 0;

		// Token: 0x04000A81 RID: 2689
		public const int textStyle_none = 2;

		// Token: 0x04000A82 RID: 2690
		public const int adult = 0;

		// Token: 0x04000A83 RID: 2691
		public const int teen = 1;

		// Token: 0x04000A84 RID: 2692
		public const int child = 2;

		// Token: 0x04000A85 RID: 2693
		public const int neutral = 0;

		// Token: 0x04000A86 RID: 2694
		public const int polite = 1;

		// Token: 0x04000A87 RID: 2695
		public const int rude = 2;

		// Token: 0x04000A88 RID: 2696
		public const int outgoing = 0;

		// Token: 0x04000A89 RID: 2697
		public const int shy = 1;

		// Token: 0x04000A8A RID: 2698
		public const int positive = 0;

		// Token: 0x04000A8B RID: 2699
		public const int negative = 1;

		// Token: 0x04000A8C RID: 2700
		public const string region_desert = "Desert";

		// Token: 0x04000A8D RID: 2701
		public const string region_town = "Town";

		// Token: 0x04000A8E RID: 2702
		public const string region_other = "Other";

		// Token: 0x04000A8F RID: 2703
		public const int defaultSpriteWidth = 16;

		// Token: 0x04000A90 RID: 2704
		public const int defaultSpriteHeight = 32;

		// Token: 0x04000A91 RID: 2705
		private Dictionary<string, string> dialogue;

		// Token: 0x04000A92 RID: 2706
		private SchedulePathDescription directionsToNewLocation;

		// Token: 0x04000A93 RID: 2707
		private int lengthOfWalkingSquareX;

		// Token: 0x04000A94 RID: 2708
		private int lengthOfWalkingSquareY;

		// Token: 0x04000A95 RID: 2709
		private int squarePauseAccumulation;

		// Token: 0x04000A96 RID: 2710
		private int squarePauseTotal;

		// Token: 0x04000A97 RID: 2711
		private int squarePauseOffset;

		// Token: 0x04000A98 RID: 2712
		public Microsoft.Xna.Framework.Rectangle lastCrossroad;

		// Token: 0x04000A99 RID: 2713
		private Texture2D portrait;

		// Token: 0x04000A9A RID: 2714
		private string LastLocationNameForAppearance;

		// Token: 0x04000A9B RID: 2715
		[XmlIgnore]
		public string LastAppearanceId;

		// Token: 0x04000A9C RID: 2716
		private Vector2 nextSquarePosition;

		// Token: 0x04000A9D RID: 2717
		[XmlIgnore]
		public int shakeTimer;

		// Token: 0x04000A9E RID: 2718
		private bool isWalkingInSquare;

		// Token: 0x04000A9F RID: 2719
		private readonly NetBool isWalkingTowardPlayer = new NetBool();

		// Token: 0x04000AA0 RID: 2720
		protected string textAboveHead;

		// Token: 0x04000AA1 RID: 2721
		protected int textAboveHeadPreTimer;

		// Token: 0x04000AA2 RID: 2722
		protected int textAboveHeadTimer;

		// Token: 0x04000AA3 RID: 2723
		protected int textAboveHeadStyle;

		// Token: 0x04000AA4 RID: 2724
		protected Color? textAboveHeadColor;

		// Token: 0x04000AA5 RID: 2725
		protected float textAboveHeadAlpha;

		// Token: 0x04000AA6 RID: 2726
		public int daysAfterLastBirth = -1;

		// Token: 0x04000AA7 RID: 2727
		protected Dialogue extraDialogueMessageToAddThisMorning;

		// Token: 0x04000AA8 RID: 2728
		[XmlElement("birthday_Season")]
		public readonly NetString birthday_Season = new NetString();

		// Token: 0x04000AA9 RID: 2729
		[XmlElement("birthday_Day")]
		public readonly NetInt birthday_Day = new NetInt();

		// Token: 0x04000AAA RID: 2730
		[XmlElement("age")]
		public readonly NetInt age = new NetInt();

		// Token: 0x04000AAB RID: 2731
		[XmlElement("manners")]
		public readonly NetInt manners = new NetInt();

		// Token: 0x04000AAC RID: 2732
		[XmlElement("socialAnxiety")]
		public readonly NetInt socialAnxiety = new NetInt();

		// Token: 0x04000AAD RID: 2733
		[XmlElement("optimism")]
		public readonly NetInt optimism = new NetInt();

		// Token: 0x04000AAE RID: 2734
		[XmlElement("gender")]
		public readonly NetEnum<Gender> gender = new NetEnum<Gender>();

		// Token: 0x04000AAF RID: 2735
		[XmlIgnore]
		public readonly NetBool breather = new NetBool(true);

		// Token: 0x04000AB0 RID: 2736
		[XmlIgnore]
		public readonly NetBool isSleeping = new NetBool(false);

		// Token: 0x04000AB1 RID: 2737
		[XmlElement("sleptInBed")]
		public readonly NetBool sleptInBed = new NetBool(true);

		// Token: 0x04000AB2 RID: 2738
		[XmlIgnore]
		public readonly NetBool hideShadow = new NetBool();

		// Token: 0x04000AB3 RID: 2739
		[XmlElement("isInvisible")]
		public readonly NetBool isInvisible = new NetBool(false);

		// Token: 0x04000AB4 RID: 2740
		[XmlElement("lastSeenMovieWeek")]
		public readonly NetInt lastSeenMovieWeek = new NetInt(-1);

		// Token: 0x04000AB5 RID: 2741
		public bool? datingFarmer;

		// Token: 0x04000AB6 RID: 2742
		public bool? divorcedFromFarmer;

		// Token: 0x04000AB7 RID: 2743
		[XmlElement("datable")]
		public readonly NetBool datable = new NetBool();

		// Token: 0x04000AB8 RID: 2744
		[XmlIgnore]
		public bool updatedDialogueYet;

		// Token: 0x04000AB9 RID: 2745
		[XmlIgnore]
		public bool immediateSpeak;

		// Token: 0x04000ABA RID: 2746
		[XmlIgnore]
		public bool ignoreScheduleToday;

		// Token: 0x04000ABB RID: 2747
		protected int defaultFacingDirection;

		// Token: 0x04000ABC RID: 2748
		private readonly NetVector2 defaultPosition = new NetVector2();

		// Token: 0x04000ABD RID: 2749
		[XmlElement("defaultMap")]
		public readonly NetString defaultMap = new NetString();

		// Token: 0x04000ABE RID: 2750
		public string loveInterest;

		// Token: 0x04000ABF RID: 2751
		public int id = -1;

		// Token: 0x04000AC0 RID: 2752
		public int daysUntilNotInvisible;

		// Token: 0x04000AC1 RID: 2753
		public bool followSchedule = true;

		// Token: 0x04000AC2 RID: 2754
		[XmlIgnore]
		public PathFindController temporaryController;

		// Token: 0x04000AC3 RID: 2755
		[XmlElement("moveTowardPlayerThreshold")]
		public readonly NetInt moveTowardPlayerThreshold = new NetInt();

		// Token: 0x04000AC4 RID: 2756
		[XmlIgnore]
		public float rotation;

		// Token: 0x04000AC5 RID: 2757
		[XmlIgnore]
		public float yOffset;

		// Token: 0x04000AC6 RID: 2758
		[XmlIgnore]
		public float swimTimer;

		// Token: 0x04000AC7 RID: 2759
		[XmlIgnore]
		public float timerSinceLastMovement;

		// Token: 0x04000AC8 RID: 2760
		[XmlIgnore]
		public string mapBeforeEvent;

		// Token: 0x04000AC9 RID: 2761
		[XmlIgnore]
		public Vector2 positionBeforeEvent;

		// Token: 0x04000ACA RID: 2762
		[XmlIgnore]
		public Vector2 lastPosition;

		// Token: 0x04000ACB RID: 2763
		[XmlIgnore]
		public float currentScheduleDelay;

		// Token: 0x04000ACC RID: 2764
		[XmlIgnore]
		public float scheduleDelaySeconds;

		// Token: 0x04000ACD RID: 2765
		[XmlIgnore]
		public bool layingDown;

		// Token: 0x04000ACE RID: 2766
		[XmlIgnore]
		public Vector2 appliedRouteAnimationOffset = Vector2.Zero;

		// Token: 0x04000ACF RID: 2767
		[XmlIgnore]
		public string[] routeAnimationMetadata;

		// Token: 0x04000AD0 RID: 2768
		[XmlElement("hasSaidAfternoonDialogue")]
		private NetBool hasSaidAfternoonDialogue = new NetBool(false);

		// Token: 0x04000AD1 RID: 2769
		[XmlIgnore]
		public static bool hasSomeoneWateredCrops;

		// Token: 0x04000AD2 RID: 2770
		[XmlIgnore]
		public static bool hasSomeoneFedThePet;

		// Token: 0x04000AD3 RID: 2771
		[XmlIgnore]
		public static bool hasSomeoneFedTheAnimals;

		// Token: 0x04000AD4 RID: 2772
		[XmlIgnore]
		public static bool hasSomeoneRepairedTheFences = false;

		// Token: 0x04000AD5 RID: 2773
		[XmlIgnore]
		protected bool _skipRouteEndIntro;

		// Token: 0x04000AD6 RID: 2774
		[NonInstancedStatic]
		public static HashSet<string> invalidDialogueFiles = new HashSet<string>();

		// Token: 0x04000AD9 RID: 2777
		[XmlIgnore]
		protected bool _hasLoadedMasterScheduleData;

		// Token: 0x04000ADA RID: 2778
		[XmlIgnore]
		protected Dictionary<string, string> _masterScheduleData;

		// Token: 0x04000ADC RID: 2780
		protected static Stack<Dialogue> _EmptyDialogue = new Stack<Dialogue>();

		// Token: 0x04000ADD RID: 2781
		[XmlIgnore]
		public Stack<Dialogue> TemporaryDialogue;

		// Token: 0x04000ADE RID: 2782
		[XmlIgnore]
		public readonly NetList<MarriageDialogueReference, NetRef<MarriageDialogueReference>> currentMarriageDialogue = new NetList<MarriageDialogueReference, NetRef<MarriageDialogueReference>>();

		// Token: 0x04000ADF RID: 2783
		public readonly NetBool hasBeenKissedToday = new NetBool(false);

		// Token: 0x04000AE0 RID: 2784
		[XmlIgnore]
		public readonly NetRef<MarriageDialogueReference> marriageDefaultDialogue = new NetRef<MarriageDialogueReference>(null);

		// Token: 0x04000AE1 RID: 2785
		[XmlIgnore]
		public readonly NetBool shouldSayMarriageDialogue = new NetBool(false);

		// Token: 0x04000AE2 RID: 2786
		public readonly NetEvent0 removeHenchmanEvent = new NetEvent0(false);

		// Token: 0x04000AE3 RID: 2787
		private bool isPlayingSleepingAnimation;

		// Token: 0x04000AE4 RID: 2788
		public readonly NetBool shouldPlayRobinHammerAnimation = new NetBool();

		// Token: 0x04000AE5 RID: 2789
		private bool isPlayingRobinHammerAnimation;

		// Token: 0x04000AE6 RID: 2790
		public readonly NetBool shouldPlaySpousePatioAnimation = new NetBool();

		// Token: 0x04000AE7 RID: 2791
		private bool isPlayingSpousePatioAnimation;

		// Token: 0x04000AE8 RID: 2792
		public readonly NetBool shouldWearIslandAttire = new NetBool();

		// Token: 0x04000AE9 RID: 2793
		private bool isWearingIslandAttire;

		// Token: 0x04000AEA RID: 2794
		public readonly NetBool isMovingOnPathFindPath = new NetBool();

		// Token: 0x04000AEB RID: 2795
		[XmlIgnore]
		public bool portraitOverridden;

		// Token: 0x04000AEC RID: 2796
		[XmlIgnore]
		public bool spriteOverridden;

		// Token: 0x04000AED RID: 2797
		[XmlIgnore]
		public List<SchedulePathDescription> queuedSchedulePaths = new List<SchedulePathDescription>();

		// Token: 0x04000AEE RID: 2798
		[XmlIgnore]
		public int lastAttemptedSchedule = -1;

		// Token: 0x04000AEF RID: 2799
		[XmlIgnore]
		public readonly NetBool doingEndOfRouteAnimation = new NetBool();

		// Token: 0x04000AF0 RID: 2800
		private bool currentlyDoingEndOfRouteAnimation;

		// Token: 0x04000AF1 RID: 2801
		[XmlIgnore]
		public readonly NetBool goingToDoEndOfRouteAnimation = new NetBool();

		// Token: 0x04000AF2 RID: 2802
		[XmlIgnore]
		public readonly NetString endOfRouteMessage = new NetString();

		// Token: 0x04000AF3 RID: 2803
		[XmlElement("dayScheduleName")]
		public readonly NetString dayScheduleName = new NetString();

		// Token: 0x04000AF4 RID: 2804
		[XmlElement("islandScheduleName")]
		public readonly NetString islandScheduleName = new NetString();

		// Token: 0x04000AF5 RID: 2805
		private int[] routeEndIntro;

		// Token: 0x04000AF6 RID: 2806
		private int[] routeEndAnimation;

		// Token: 0x04000AF7 RID: 2807
		private int[] routeEndOutro;

		// Token: 0x04000AF8 RID: 2808
		[XmlIgnore]
		public string nextEndOfRouteMessage;

		// Token: 0x04000AF9 RID: 2809
		private string loadedEndOfRouteBehavior;

		// Token: 0x04000AFA RID: 2810
		[XmlIgnore]
		protected string _startedEndOfRouteBehavior;

		// Token: 0x04000AFB RID: 2811
		[XmlIgnore]
		protected string _finishingEndOfRouteBehavior;

		// Token: 0x04000AFC RID: 2812
		[XmlIgnore]
		protected int _beforeEndOfRouteAnimationFrame;

		// Token: 0x04000AFD RID: 2813
		public readonly NetString endOfRouteBehaviorName = new NetString();

		// Token: 0x04000AFE RID: 2814
		public Point previousEndPoint;

		// Token: 0x04000AFF RID: 2815
		public int squareMovementFacingPreference;

		// Token: 0x04000B00 RID: 2816
		protected bool returningToEndPoint;

		// Token: 0x04000B01 RID: 2817
		private bool wasKissedYesterday;
	}
}

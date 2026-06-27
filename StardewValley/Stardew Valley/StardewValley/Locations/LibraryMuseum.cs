using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.GameData.Museum;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Logging;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Triggers;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	// Token: 0x020002E4 RID: 740
	public class LibraryMuseum : GameLocation
	{
		// Token: 0x1700041A RID: 1050
		// (get) Token: 0x060030EE RID: 12526 RVA: 0x0026A684 File Offset: 0x00268884
		public static int totalArtifacts
		{
			get
			{
				if (LibraryMuseum._totalArtifacts < 0)
				{
					LibraryMuseum._totalArtifacts = 0;
					foreach (string itemId in ItemRegistry.RequireTypeDefinition("(O)").GetAllIds())
					{
						if (LibraryMuseum.IsItemSuitableForDonation("(O)" + itemId, false))
						{
							LibraryMuseum._totalArtifacts++;
						}
					}
				}
				return LibraryMuseum._totalArtifacts;
			}
		}

		// Token: 0x1700041B RID: 1051
		// (get) Token: 0x060030EF RID: 12527 RVA: 0x0026A708 File Offset: 0x00268908
		[XmlElement("museumPieces")]
		public NetVector2Dictionary<string, NetString> museumPieces
		{
			get
			{
				return Game1.netWorldState.Value.MuseumPieces;
			}
		}

		// Token: 0x060030F0 RID: 12528 RVA: 0x0026A719 File Offset: 0x00268919
		public LibraryMuseum()
		{
		}

		// Token: 0x060030F1 RID: 12529 RVA: 0x0026A737 File Offset: 0x00268937
		public LibraryMuseum(string mapPath, string name) : base(mapPath, name)
		{
		}

		// Token: 0x060030F2 RID: 12530 RVA: 0x0026A757 File Offset: 0x00268957
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.mutex.NetFields, "mutex.NetFields");
		}

		// Token: 0x060030F3 RID: 12531 RVA: 0x0026A77B File Offset: 0x0026897B
		public override void updateEvenIfFarmerIsntHere(GameTime time, bool skipWasUpdatedFlush = false)
		{
			this.mutex.Update(this);
			base.updateEvenIfFarmerIsntHere(time, skipWasUpdatedFlush);
		}

		// Token: 0x060030F4 RID: 12532 RVA: 0x0026A791 File Offset: 0x00268991
		public static bool HasDonatedArtifacts()
		{
			return Game1.netWorldState.Value.MuseumPieces.Length > 0;
		}

		// Token: 0x060030F5 RID: 12533 RVA: 0x0026A7AA File Offset: 0x002689AA
		public static bool HasDonatedArtifactAt(Vector2 tile)
		{
			return Game1.netWorldState.Value.MuseumPieces.ContainsKey(tile);
		}

		// Token: 0x060030F6 RID: 12534 RVA: 0x0026A7C4 File Offset: 0x002689C4
		public static bool HasDonatedArtifact(string itemId)
		{
			if (itemId == null)
			{
				return false;
			}
			itemId = ItemRegistry.ManuallyQualifyItemId(itemId, "(O)", false);
			foreach (KeyValuePair<Vector2, string> pair in Game1.netWorldState.Value.MuseumPieces.Pairs)
			{
				if (itemId == "(O)" + pair.Value)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060030F7 RID: 12535 RVA: 0x0026A854 File Offset: 0x00268A54
		public bool isItemSuitableForDonation(Item i)
		{
			return LibraryMuseum.IsItemSuitableForDonation((i != null) ? i.QualifiedItemId : null, true);
		}

		// Token: 0x060030F8 RID: 12536 RVA: 0x0026A868 File Offset: 0x00268A68
		public static bool IsItemSuitableForDonation(string itemId, bool checkDonatedItems = true)
		{
			if (itemId == null)
			{
				return false;
			}
			itemId = ItemRegistry.ManuallyQualifyItemId(itemId, "(O)", false);
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(itemId);
			HashSet<string> tags = ItemContextTagManager.GetBaseContextTags(itemId);
			return itemData.HasTypeObject() && !tags.Contains("not_museum_donatable") && (!checkDonatedItems || !LibraryMuseum.HasDonatedArtifact(itemData.QualifiedItemId)) && (tags.Contains("museum_donatable") || tags.Contains("item_type_arch") || tags.Contains("item_type_minerals"));
		}

		// Token: 0x060030F9 RID: 12537 RVA: 0x0026A8E8 File Offset: 0x00268AE8
		public bool doesFarmerHaveAnythingToDonate(Farmer who)
		{
			for (int i = 0; i < who.maxItems.Value; i++)
			{
				if (i < who.Items.Count)
				{
					Object obj = who.Items[i] as Object;
					if (obj != null && this.isItemSuitableForDonation(obj))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060030FA RID: 12538 RVA: 0x0026A93C File Offset: 0x00268B3C
		private Dictionary<int, Vector2> getLostBooksLocations()
		{
			Dictionary<int, Vector2> lostBooksLocations = new Dictionary<int, Vector2>();
			for (int x = 0; x < this.map.Layers[0].LayerWidth; x++)
			{
				for (int y = 0; y < this.map.Layers[0].LayerHeight; y++)
				{
					string[] action = base.GetTilePropertySplitBySpaces("Action", "Buildings", x, y);
					if (ArgUtility.Get(action, 0, null, true) == "Notes")
					{
						int noteId;
						string error;
						if (ArgUtility.TryGetInt(action, 1, out noteId, out error, "int noteId"))
						{
							lostBooksLocations.Add(noteId, new Vector2((float)x, (float)y));
						}
						else
						{
							base.LogTileActionError(action, x, y, error);
						}
					}
				}
			}
			return lostBooksLocations;
		}

		// Token: 0x060030FB RID: 12539 RVA: 0x0026A9F0 File Offset: 0x00268BF0
		protected override void resetLocalState()
		{
			if (!Game1.player.eventsSeen.Contains("0") && this.doesFarmerHaveAnythingToDonate(Game1.player))
			{
				Game1.player.mailReceived.Add("somethingToDonate");
			}
			if (LibraryMuseum.HasDonatedArtifacts())
			{
				Game1.player.mailReceived.Add("somethingWasDonated");
			}
			base.resetLocalState();
			int booksFound = Game1.netWorldState.Value.LostBooksFound;
			foreach (KeyValuePair<int, Vector2> pair in this.getLostBooksLocations())
			{
				int id = pair.Key;
				Vector2 tile = pair.Value;
				if (id <= booksFound && !Game1.player.mailReceived.Contains("lb_" + id.ToString()))
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(144, 447, 15, 15), new Vector2(tile.X * 64f, tile.Y * 64f - 96f - 16f), false, 0f, Color.White)
					{
						interval = 99999f,
						animationLength = 1,
						totalNumberOfLoops = 9999,
						yPeriodic = true,
						yPeriodicLoopTime = 4000f,
						yPeriodicRange = 16f,
						layerDepth = 1f,
						scale = 4f,
						id = id
					});
				}
			}
		}

		// Token: 0x060030FC RID: 12540 RVA: 0x0026ABAC File Offset: 0x00268DAC
		public override void cleanupBeforePlayerExit()
		{
			Dictionary<Item, string> itemToRewardsLookup = this._itemToRewardsLookup;
			if (itemToRewardsLookup != null)
			{
				itemToRewardsLookup.Clear();
			}
			base.cleanupBeforePlayerExit();
		}

		// Token: 0x060030FD RID: 12541 RVA: 0x0026ABC8 File Offset: 0x00268DC8
		public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
		{
			if (questionAndAnswer == null)
			{
				return false;
			}
			if (!(questionAndAnswer == "Museum_Collect"))
			{
				if (!(questionAndAnswer == "Museum_Donate"))
				{
					if (questionAndAnswer == "Museum_Rearrange_Yes")
					{
						this.OpenRearrangeMenu();
					}
				}
				else
				{
					this.OpenDonationMenu();
				}
			}
			else
			{
				this.OpenRewardMenu();
			}
			return base.answerDialogueAction(questionAndAnswer, questionParams);
		}

		// Token: 0x060030FE RID: 12542 RVA: 0x0026AC21 File Offset: 0x00268E21
		public string getRewardItemKey(Item item)
		{
			return "museumCollectedReward" + Utility.getStandardDescriptionFromItem(item, 1, '_');
		}

		// Token: 0x060030FF RID: 12543 RVA: 0x0026AC38 File Offset: 0x00268E38
		public override bool performAction(string[] action, Farmer who, Location tileLocation)
		{
			if (who.IsLocalPlayer)
			{
				string a = ArgUtility.Get(action, 0, null, true);
				if (a == "Gunther")
				{
					this.OpenGuntherDialogueMenu();
					return true;
				}
				if (a == "Rearrange")
				{
					if (!this.doesFarmerHaveAnythingToDonate(Game1.player))
					{
						if (LibraryMuseum.HasDonatedArtifacts())
						{
							base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Rearrange"), base.createYesNoResponses(), "Museum_Rearrange");
						}
						return true;
					}
				}
			}
			return base.performAction(action, who, tileLocation);
		}

		// Token: 0x06003100 RID: 12544 RVA: 0x0026ACBC File Offset: 0x00268EBC
		public List<Item> getRewardsForPlayer(Farmer player)
		{
			this._itemToRewardsLookup.Clear();
			Dictionary<string, MuseumRewards> museumRewardData = DataLoader.MuseumRewards(Game1.content);
			Dictionary<string, int> countsByTag = this.GetDonatedByContextTag(museumRewardData);
			List<Item> rewards = new List<Item>();
			foreach (KeyValuePair<string, MuseumRewards> pair in museumRewardData)
			{
				string id = pair.Key;
				MuseumRewards value = pair.Value;
				if (this.CanCollectReward(value, id, player, countsByTag))
				{
					bool rewardAdded = false;
					if (value.RewardItemId != null)
					{
						Item item = ItemRegistry.Create(value.RewardItemId, value.RewardItemCount, 0, false);
						item.IsRecipe = value.RewardItemIsRecipe;
						item.specialItem = value.RewardItemIsSpecial;
						if (this.AddRewardItemIfUncollected(player, rewards, item))
						{
							this._itemToRewardsLookup[item] = id;
							rewardAdded = true;
						}
					}
					if (!rewardAdded)
					{
						this.AddNonItemRewards(value, id, player);
					}
				}
			}
			return rewards;
		}

		// Token: 0x06003101 RID: 12545 RVA: 0x0026ADBC File Offset: 0x00268FBC
		public void AddNonItemRewards(MuseumRewards data, string rewardId, Farmer player)
		{
			if (data.FlagOnCompletion)
			{
				player.mailReceived.Add(rewardId);
			}
			if (data.RewardActions != null)
			{
				foreach (string action in data.RewardActions)
				{
					string error;
					Exception ex;
					if (!TriggerActionManager.TryRunAction(action, out error, out ex))
					{
						IGameLogger log = Game1.log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(48, 3);
						defaultInterpolatedStringHandler.AppendLiteral("Museum reward ");
						defaultInterpolatedStringHandler.AppendFormatted(rewardId);
						defaultInterpolatedStringHandler.AppendLiteral(" ignored invalid event action '");
						defaultInterpolatedStringHandler.AppendFormatted(action);
						defaultInterpolatedStringHandler.AppendLiteral("': ");
						defaultInterpolatedStringHandler.AppendFormatted(error);
						log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), ex);
					}
				}
			}
		}

		// Token: 0x06003102 RID: 12546 RVA: 0x0026AE8C File Offset: 0x0026908C
		public bool AddRewardItemIfUncollected(Farmer player, List<Item> rewards, Item rewardItem)
		{
			if (!player.mailReceived.Contains(this.getRewardItemKey(rewardItem)))
			{
				rewards.Add(rewardItem);
				return true;
			}
			return false;
		}

		// Token: 0x06003103 RID: 12547 RVA: 0x0026AEAC File Offset: 0x002690AC
		public bool HighlightCollectableRewards(Item item)
		{
			return Game1.player.couldInventoryAcceptThisItem(item);
		}

		// Token: 0x06003104 RID: 12548 RVA: 0x0026AEB9 File Offset: 0x002690B9
		public void OpenRearrangeMenu()
		{
			if (!this.mutex.IsLocked())
			{
				this.mutex.RequestLock(delegate
				{
					Game1.activeClickableMenu = new MuseumMenu(new InventoryMenu.highlightThisItem(InventoryMenu.highlightNoItems))
					{
						exitFunction = new IClickableMenu.onExit(this.mutex.ReleaseLock)
					};
				}, null);
			}
		}

		// Token: 0x06003105 RID: 12549 RVA: 0x0026AEE0 File Offset: 0x002690E0
		public void OpenRewardMenu()
		{
			Game1.activeClickableMenu = new ItemGrabMenu(this.getRewardsForPlayer(Game1.player), false, true, new InventoryMenu.highlightThisItem(this.HighlightCollectableRewards), null, "Rewards", new ItemGrabMenu.behaviorOnItemSelect(this.OnRewardCollected), false, true, false, false, false, 0, null, -1, this, ItemExitBehavior.ReturnToPlayer, true);
		}

		// Token: 0x06003106 RID: 12550 RVA: 0x0026AF2D File Offset: 0x0026912D
		public void OpenDonationMenu()
		{
			this.mutex.RequestLock(delegate
			{
				Game1.activeClickableMenu = new MuseumMenu(new InventoryMenu.highlightThisItem(this.isItemSuitableForDonation))
				{
					exitFunction = new IClickableMenu.onExit(this.OnDonationMenuClosed)
				};
			}, null);
		}

		// Token: 0x06003107 RID: 12551 RVA: 0x0026AF47 File Offset: 0x00269147
		public void OnDonationMenuClosed()
		{
			this.mutex.ReleaseLock();
			this.getRewardsForPlayer(Game1.player);
		}

		// Token: 0x06003108 RID: 12552 RVA: 0x0026AF60 File Offset: 0x00269160
		public void OnRewardCollected(Item item, Farmer who)
		{
			if (item != null)
			{
				string rewardKey;
				if (item is Object && this._itemToRewardsLookup.TryGetValue(item, out rewardKey))
				{
					MuseumRewards rewardData;
					if (DataLoader.MuseumRewards(Game1.content).TryGetValue(rewardKey, out rewardData))
					{
						this.AddNonItemRewards(rewardData, rewardKey, who);
					}
					this._itemToRewardsLookup.Remove(item);
				}
				if (!who.hasOrWillReceiveMail(this.getRewardItemKey(item)))
				{
					who.mailReceived.Add(this.getRewardItemKey(item));
					if (item.QualifiedItemId.Equals("(O)499"))
					{
						who.craftingRecipes.TryAdd("Ancient Seeds", 0);
					}
				}
			}
		}

		// Token: 0x06003109 RID: 12553 RVA: 0x0026AFFC File Offset: 0x002691FC
		private void OpenGuntherDialogueMenu()
		{
			if (this.doesFarmerHaveAnythingToDonate(Game1.player) && !this.mutex.IsLocked())
			{
				Response[] choice;
				if (this.getRewardsForPlayer(Game1.player).Count > 0)
				{
					choice = new Response[]
					{
						new Response("Donate", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Donate")),
						new Response("Collect", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Collect")),
						new Response("Leave", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Leave"))
					};
				}
				else
				{
					choice = new Response[]
					{
						new Response("Donate", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Donate")),
						new Response("Leave", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Leave"))
					};
				}
				base.createQuestionDialogue("", choice, "Museum");
				return;
			}
			if (this.getRewardsForPlayer(Game1.player).Count > 0)
			{
				base.createQuestionDialogue("", new Response[]
				{
					new Response("Collect", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Collect")),
					new Response("Leave", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Leave"))
				}, "Museum");
				return;
			}
			if (this.doesFarmerHaveAnythingToDonate(Game1.player) && this.mutex.IsLocked())
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NPC_Busy", Game1.RequireCharacter("Gunther", true).displayName));
				return;
			}
			NPC gunther = Game1.getCharacterFromName("Gunther", true, false);
			if (Game1.player.achievements.Contains(5))
			{
				Game1.DrawDialogue(new Dialogue(gunther, "Data\\ExtraDialogue:Gunther_MuseumComplete", Game1.parseText(Game1.content.LoadString("Data\\ExtraDialogue:Gunther_MuseumComplete"))));
				return;
			}
			if (Game1.player.mailReceived.Contains("artifactFound"))
			{
				Game1.DrawDialogue(new Dialogue(gunther, "Data\\ExtraDialogue:Gunther_NothingToDonate", Game1.parseText(Game1.content.LoadString("Data\\ExtraDialogue:Gunther_NothingToDonate"))));
				return;
			}
			Game1.DrawDialogue(gunther, "Data\\ExtraDialogue:Gunther_NoArtifactsFound");
		}

		// Token: 0x0600310A RID: 12554 RVA: 0x0026B218 File Offset: 0x00269418
		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			string itemId;
			if (this.museumPieces.TryGetValue(new Vector2((float)tileLocation.X, (float)tileLocation.Y), out itemId) || this.museumPieces.TryGetValue(new Vector2((float)tileLocation.X, (float)(tileLocation.Y - 1)), out itemId))
			{
				ParsedItemData data = ItemRegistry.GetDataOrErrorItem("(O)" + itemId);
				Game1.drawObjectDialogue(Game1.parseText(string.Concat(new string[]
				{
					" - ",
					data.DisplayName,
					" - ",
					"^",
					data.Description
				})));
				return true;
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		// Token: 0x0600310B RID: 12555 RVA: 0x0026B2C8 File Offset: 0x002694C8
		public bool isTileSuitableForMuseumPiece(int x, int y)
		{
			if (!LibraryMuseum.HasDonatedArtifactAt(new Vector2((float)x, (float)y)))
			{
				int tileIndexAt = base.getTileIndexAt(x, y, "Buildings", "untitled tile sheet");
				if (tileIndexAt - 1072 <= 2 || tileIndexAt - 1237 <= 1)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600310C RID: 12556 RVA: 0x0026B310 File Offset: 0x00269510
		public Dictionary<string, int> GetDonatedByContextTag(Dictionary<string, MuseumRewards> museumRewardData)
		{
			Dictionary<string, int> counts = new Dictionary<string, int>();
			foreach (MuseumRewards museumRewards in museumRewardData.Values)
			{
				foreach (MuseumDonationRequirement targetTags in museumRewards.TargetContextTags)
				{
					counts[targetTags.Tag] = 0;
				}
			}
			string[] contextTags = counts.Keys.ToArray<string>();
			foreach (string itemId in this.museumPieces.Values)
			{
				foreach (string tag in contextTags)
				{
					if (tag == "" || ItemContextTagManager.HasBaseTag(itemId, tag))
					{
						Dictionary<string, int> dictionary = counts;
						string key = tag;
						dictionary[key]++;
					}
				}
			}
			return counts;
		}

		// Token: 0x0600310D RID: 12557 RVA: 0x0026B450 File Offset: 0x00269650
		public bool CanCollectReward(MuseumRewards reward, string rewardId, Farmer player, Dictionary<string, int> countsByTag)
		{
			if (reward.FlagOnCompletion && player.mailReceived.Contains(rewardId))
			{
				return false;
			}
			foreach (MuseumDonationRequirement targetTags in reward.TargetContextTags)
			{
				if (targetTags.Tag == "" && targetTags.Count == -1)
				{
					if (countsByTag[targetTags.Tag] < LibraryMuseum.totalArtifacts)
					{
						return false;
					}
				}
				else if (countsByTag[targetTags.Tag] < targetTags.Count)
				{
					return false;
				}
			}
			if (reward.RewardItemId != null)
			{
				if (player.canUnderstandDwarves && ItemRegistry.QualifyItemId(reward.RewardItemId) == "(O)326")
				{
					return false;
				}
				if (reward.RewardItemIsSpecial)
				{
					ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(reward.RewardItemId);
					if (((itemData.HasTypeId("(F)") || itemData.HasTypeBigCraftable()) ? player.specialBigCraftables : player.specialItems).Contains(itemData.ItemId))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x0600310E RID: 12558 RVA: 0x0026B578 File Offset: 0x00269778
		public Microsoft.Xna.Framework.Rectangle getMuseumDonationBounds()
		{
			return new Microsoft.Xna.Framework.Rectangle(26, 5, 22, 13);
		}

		// Token: 0x0600310F RID: 12559 RVA: 0x0026B588 File Offset: 0x00269788
		public Vector2 getFreeDonationSpot()
		{
			Microsoft.Xna.Framework.Rectangle bounds = this.getMuseumDonationBounds();
			for (int x = bounds.X; x <= bounds.Right; x++)
			{
				for (int y = bounds.Y; y <= bounds.Bottom; y++)
				{
					if (this.isTileSuitableForMuseumPiece(x, y))
					{
						return new Vector2((float)x, (float)y);
					}
				}
			}
			return new Vector2(26f, 5f);
		}

		// Token: 0x06003110 RID: 12560 RVA: 0x0026B5F0 File Offset: 0x002697F0
		public Vector2 findMuseumPieceLocationInDirection(Vector2 startingPoint, int direction, int distanceToCheck = 8, bool ignoreExistingItems = true)
		{
			Vector2 checkTile = startingPoint;
			Vector2 offset = Vector2.Zero;
			switch (direction)
			{
			case 0:
				offset = new Vector2(0f, -1f);
				break;
			case 1:
				offset = new Vector2(1f, 0f);
				break;
			case 2:
				offset = new Vector2(0f, 1f);
				break;
			case 3:
				offset = new Vector2(-1f, 0f);
				break;
			}
			for (int i = 0; i < distanceToCheck; i++)
			{
				for (int j = 0; j < distanceToCheck; j++)
				{
					checkTile += offset;
					if (this.isTileSuitableForMuseumPiece((int)checkTile.X, (int)checkTile.Y) || (!ignoreExistingItems && LibraryMuseum.HasDonatedArtifactAt(checkTile)))
					{
						return checkTile;
					}
				}
				checkTile = startingPoint;
				int sign = (i % 2 == 0) ? -1 : 1;
				switch (direction)
				{
				case 0:
				case 2:
					checkTile.X += (float)(sign * (i / 2 + 1));
					break;
				case 1:
				case 3:
					checkTile.Y += (float)(sign * (i / 2 + 1));
					break;
				}
			}
			return startingPoint;
		}

		// Token: 0x06003111 RID: 12561 RVA: 0x0026B704 File Offset: 0x00269904
		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			foreach (TemporaryAnimatedSprite t in this.temporarySprites)
			{
				if (t.layerDepth >= 1f)
				{
					t.draw(b, false, 0, 0, 1f);
				}
			}
		}

		// Token: 0x06003112 RID: 12562 RVA: 0x0026B768 File Offset: 0x00269968
		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			foreach (KeyValuePair<Vector2, string> v in this.museumPieces.Pairs)
			{
				b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, v.Key * 64f + new Vector2(32f, 52f)), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (v.Key.Y * 64f - 2f) / 10000f);
				ParsedItemData data = ItemRegistry.GetDataOrErrorItem("(O)" + v.Value);
				b.Draw(data.GetTexture(), Game1.GlobalToLocal(Game1.viewport, v.Key * 64f), new Microsoft.Xna.Framework.Rectangle?(data.GetSourceRect(0, null)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, v.Key.Y * 64f / 10000f);
			}
		}

		// Token: 0x040020E8 RID: 8424
		public const int dwarvenGuide = 0;

		// Token: 0x040020E9 RID: 8425
		protected static int _totalArtifacts = -1;

		// Token: 0x040020EA RID: 8426
		public const int totalNotes = 21;

		// Token: 0x040020EB RID: 8427
		private readonly NetMutex mutex = new NetMutex();

		// Token: 0x040020EC RID: 8428
		[XmlIgnore]
		protected Dictionary<Item, string> _itemToRewardsLookup = new Dictionary<Item, string>();
	}
}

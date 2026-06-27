using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using SkiaSharp;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Delegates;
using StardewValley.Enchantments;
using StardewValley.Events;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Characters;
using StardewValley.GameData.Crops;
using StardewValley.GameData.FarmAnimals;
using StardewValley.GameData.FloorsAndPaths;
using StardewValley.GameData.FruitTrees;
using StardewValley.GameData.LocationContexts;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Pants;
using StardewValley.GameData.Pets;
using StardewValley.GameData.Shirts;
using StardewValley.GameData.Tools;
using StardewValley.GameData.Weapons;
using StardewValley.Hashing;
using StardewValley.Internal;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Logging;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Mods;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Network.Dedicated;
using StardewValley.Network.NetReady;
using StardewValley.Objects;
using StardewValley.Pathfinding;
using StardewValley.Projectiles;
using StardewValley.Quests;
using StardewValley.SaveMigrations;
using StardewValley.SaveSerialization;
using StardewValley.SDKs.Steam;
using StardewValley.SpecialOrders;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;
using StardewValley.Triggers;
using StardewValley.Util;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley
{
	// Token: 0x020000B1 RID: 177
	[InstanceStatics]
	public class Game1 : InstanceGame
	{
		// Token: 0x1700014B RID: 331
		// (get) Token: 0x06000A5E RID: 2654 RVA: 0x0006F3DD File Offset: 0x0006D5DD
		public bool IsActiveNoOverlay
		{
			get
			{
				return base.IsActive && !Program.sdk.HasOverlay;
			}
		}

		// Token: 0x06000A5F RID: 2655 RVA: 0x0006F3F8 File Offset: 0x0006D5F8
		public static void GetHasRoomAnotherFarmAsync(ReportHasRoomAnotherFarmDelegate callback)
		{
			if (LocalMultiplayer.IsLocalMultiplayer(false))
			{
				bool yes = Game1.GetHasRoomAnotherFarm();
				callback(yes);
				return;
			}
			Task task = new Task(delegate()
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
				bool yes2 = Game1.GetHasRoomAnotherFarm();
				callback(yes2);
			});
			Game1.hooks.StartTask(task, "Farm_SpaceCheck");
		}

		// Token: 0x06000A60 RID: 2656 RVA: 0x0006F450 File Offset: 0x0006D650
		private static string GameModeToString(byte mode)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler;
			switch (mode)
			{
			case 0:
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 1);
				defaultInterpolatedStringHandler.AppendLiteral("titleScreenGameMode (");
				defaultInterpolatedStringHandler.AppendFormatted<byte>(mode);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			case 1:
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(21, 1);
				defaultInterpolatedStringHandler.AppendLiteral("loadScreenGameMode (");
				defaultInterpolatedStringHandler.AppendFormatted<byte>(mode);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			case 2:
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 1);
				defaultInterpolatedStringHandler.AppendLiteral("newGameMode (");
				defaultInterpolatedStringHandler.AppendFormatted<byte>(mode);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			case 3:
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(18, 1);
				defaultInterpolatedStringHandler.AppendLiteral("playingGameMode (");
				defaultInterpolatedStringHandler.AppendFormatted<byte>(mode);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			case 4:
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(21, 1);
				defaultInterpolatedStringHandler.AppendLiteral("logoScreenGameMode (");
				defaultInterpolatedStringHandler.AppendFormatted<byte>(mode);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			case 6:
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 1);
				defaultInterpolatedStringHandler.AppendLiteral("loadingMode (");
				defaultInterpolatedStringHandler.AppendFormatted<byte>(mode);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			case 7:
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
				defaultInterpolatedStringHandler.AppendLiteral("saveMode (");
				defaultInterpolatedStringHandler.AppendFormatted<byte>(mode);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			case 8:
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 1);
				defaultInterpolatedStringHandler.AppendLiteral("saveCompleteMode (");
				defaultInterpolatedStringHandler.AppendFormatted<byte>(mode);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			case 9:
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 1);
				defaultInterpolatedStringHandler.AppendLiteral("selectGameScreen (");
				defaultInterpolatedStringHandler.AppendFormatted<byte>(mode);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			case 10:
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 1);
				defaultInterpolatedStringHandler.AppendLiteral("creditsMode (");
				defaultInterpolatedStringHandler.AppendFormatted<byte>(mode);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			case 11:
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 1);
				defaultInterpolatedStringHandler.AppendLiteral("errorLogMode (");
				defaultInterpolatedStringHandler.AppendFormatted<byte>(mode);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
			defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
			defaultInterpolatedStringHandler.AppendLiteral("unknown (");
			defaultInterpolatedStringHandler.AppendFormatted<byte>(mode);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06000A61 RID: 2657 RVA: 0x0006F6F0 File Offset: 0x0006D8F0
		public static string GetVersionString()
		{
			string label = Game1.version;
			if (!string.IsNullOrEmpty(Game1.versionLabel))
			{
				label = label + " '" + Game1.versionLabel + "'";
			}
			if (Game1.versionBuildNumber > 0)
			{
				label = label + " build " + Game1.versionBuildNumber.ToString();
			}
			return label;
		}

		// Token: 0x1700014C RID: 332
		// (get) Token: 0x06000A62 RID: 2658 RVA: 0x0006F744 File Offset: 0x0006D944
		public static LocalizedContentManager temporaryContent
		{
			get
			{
				if (Game1._temporaryContent == null)
				{
					Game1._temporaryContent = Game1.content.CreateTemporary();
				}
				return Game1._temporaryContent;
			}
		}

		// Token: 0x1700014D RID: 333
		// (get) Token: 0x06000A63 RID: 2659 RVA: 0x0006F761 File Offset: 0x0006D961
		private bool ShouldLoadIncrementally
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700014E RID: 334
		// (get) Token: 0x06000A64 RID: 2660 RVA: 0x0006F764 File Offset: 0x0006D964
		// (set) Token: 0x06000A65 RID: 2661 RVA: 0x0006F76B File Offset: 0x0006D96B
		public static Farmer player
		{
			get
			{
				return Game1._player;
			}
			internal set
			{
				Farmer player = Game1._player;
				if (player != null)
				{
					player.unload();
				}
				Game1._player = value;
				Game1._player.Items.IsLocalPlayerInventory = true;
			}
		}

		// Token: 0x1700014F RID: 335
		// (get) Token: 0x06000A66 RID: 2662 RVA: 0x0006F793 File Offset: 0x0006D993
		// (set) Token: 0x06000A67 RID: 2663 RVA: 0x0006F79F File Offset: 0x0006D99F
		public static bool IsPlayingBackgroundMusic
		{
			get
			{
				return Game1.game1._instanceIsPlayingBackgroundMusic;
			}
			set
			{
				Game1.game1._instanceIsPlayingBackgroundMusic = value;
			}
		}

		// Token: 0x17000150 RID: 336
		// (get) Token: 0x06000A68 RID: 2664 RVA: 0x0006F7AC File Offset: 0x0006D9AC
		// (set) Token: 0x06000A69 RID: 2665 RVA: 0x0006F7B8 File Offset: 0x0006D9B8
		public static bool IsPlayingOutdoorsAmbience
		{
			get
			{
				return Game1.game1._instanceIsPlayingOutdoorsAmbience;
			}
			set
			{
				Game1.game1._instanceIsPlayingOutdoorsAmbience = value;
			}
		}

		// Token: 0x17000151 RID: 337
		// (get) Token: 0x06000A6A RID: 2666 RVA: 0x0006F7C5 File Offset: 0x0006D9C5
		// (set) Token: 0x06000A6B RID: 2667 RVA: 0x0006F7D1 File Offset: 0x0006D9D1
		public static bool IsPlayingNightAmbience
		{
			get
			{
				return Game1.game1._instanceIsPlayingNightAmbience;
			}
			set
			{
				Game1.game1._instanceIsPlayingNightAmbience = value;
			}
		}

		// Token: 0x17000152 RID: 338
		// (get) Token: 0x06000A6C RID: 2668 RVA: 0x0006F7DE File Offset: 0x0006D9DE
		// (set) Token: 0x06000A6D RID: 2669 RVA: 0x0006F7EA File Offset: 0x0006D9EA
		public static bool IsPlayingTownMusic
		{
			get
			{
				return Game1.game1._instanceIsPlayingTownMusic;
			}
			set
			{
				Game1.game1._instanceIsPlayingTownMusic = value;
			}
		}

		// Token: 0x17000153 RID: 339
		// (get) Token: 0x06000A6E RID: 2670 RVA: 0x0006F7F7 File Offset: 0x0006D9F7
		// (set) Token: 0x06000A6F RID: 2671 RVA: 0x0006F803 File Offset: 0x0006DA03
		public static bool IsPlayingMorningSong
		{
			get
			{
				return Game1.game1._instanceIsPlayingMorningSong;
			}
			set
			{
				Game1.game1._instanceIsPlayingMorningSong = value;
			}
		}

		// Token: 0x17000154 RID: 340
		// (get) Token: 0x06000A70 RID: 2672 RVA: 0x0006F810 File Offset: 0x0006DA10
		public static bool isWarping
		{
			get
			{
				return Game1._isWarping;
			}
		}

		// Token: 0x17000155 RID: 341
		// (get) Token: 0x06000A71 RID: 2673 RVA: 0x0006F817 File Offset: 0x0006DA17
		public static IList<GameLocation> locations
		{
			get
			{
				return Game1.game1._locations;
			}
		}

		// Token: 0x17000156 RID: 342
		// (get) Token: 0x06000A72 RID: 2674 RVA: 0x0006F823 File Offset: 0x0006DA23
		// (set) Token: 0x06000A73 RID: 2675 RVA: 0x0006F830 File Offset: 0x0006DA30
		public static GameLocation currentLocation
		{
			get
			{
				return Game1.game1.instanceGameLocation;
			}
			set
			{
				if (Game1.game1.instanceGameLocation != value)
				{
					if (Game1._PreviousNonNullLocation == null)
					{
						Game1._PreviousNonNullLocation = Game1.game1.instanceGameLocation;
					}
					Game1.game1.instanceGameLocation = value;
					if (Game1.game1.instanceGameLocation != null)
					{
						GameLocation previousNonNullLocation = Game1._PreviousNonNullLocation;
						Game1._PreviousNonNullLocation = null;
						Game1.OnLocationChanged(previousNonNullLocation, Game1.game1.instanceGameLocation);
					}
				}
			}
		}

		// Token: 0x17000157 RID: 343
		// (get) Token: 0x06000A74 RID: 2676 RVA: 0x0006F891 File Offset: 0x0006DA91
		public static Texture2D toolSpriteSheet
		{
			get
			{
				if (Game1._toolSpriteSheet == null)
				{
					Game1.ResetToolSpriteSheet();
				}
				return Game1._toolSpriteSheet;
			}
		}

		// Token: 0x06000A75 RID: 2677 RVA: 0x0006F8A4 File Offset: 0x0006DAA4
		public static void ResetToolSpriteSheet()
		{
			if (Game1._toolSpriteSheet != null)
			{
				Game1._toolSpriteSheet.Dispose();
				Game1._toolSpriteSheet = null;
			}
			Texture2D texture = Game1.content.Load<Texture2D>("TileSheets\\tools");
			int w = texture.Width;
			int h = texture.Height;
			Texture2D texture2D = new Texture2D(Game1.game1.GraphicsDevice, w, h, false, SurfaceFormat.Color);
			texture2D.Name = texture.Name;
			Color[] data = new Color[w * h];
			texture.GetData<Color>(data);
			texture2D.SetData<Color>(data);
			Game1._toolSpriteSheet = texture2D;
		}

		// Token: 0x17000158 RID: 344
		// (get) Token: 0x06000A76 RID: 2678 RVA: 0x0006F921 File Offset: 0x0006DB21
		public static RenderTarget2D lightmap
		{
			get
			{
				return Game1._lightmap;
			}
		}

		// Token: 0x06000A77 RID: 2679 RVA: 0x0006F928 File Offset: 0x0006DB28
		public static void SetSaveName(string new_save_name)
		{
			if (new_save_name == null)
			{
				new_save_name = "";
			}
			Game1._currentSaveName = new_save_name;
			Game1._setSaveName = true;
		}

		// Token: 0x06000A78 RID: 2680 RVA: 0x0006F940 File Offset: 0x0006DB40
		public static string GetSaveGameName(bool set_value = true)
		{
			if (!Game1._setSaveName && set_value)
			{
				string base_name = Game1.MasterPlayer.farmName.Value;
				string save_name = base_name;
				int collision_index = 2;
				while (SaveGame.IsNewGameSaveNameCollision(save_name))
				{
					save_name = base_name + collision_index.ToString();
					collision_index++;
				}
				Game1.SetSaveName(save_name);
			}
			return Game1._currentSaveName;
		}

		// Token: 0x06000A79 RID: 2681 RVA: 0x0006F994 File Offset: 0x0006DB94
		private static void allocateLightmap(int width, int height)
		{
			int quality = 8;
			float zoom = 1f;
			if (Game1.options != null)
			{
				quality = Game1.options.lightingQuality;
				if (Game1.game1.useUnscaledLighting)
				{
					zoom = 1f;
				}
				else
				{
					zoom = Game1.options.zoomLevel;
				}
			}
			int w = (int)((float)width * (1f / zoom) + 64f) / (quality / 2);
			int h = (int)((float)height * (1f / zoom) + 64f) / (quality / 2);
			RenderTarget2D lightmap = Game1.lightmap;
			if (lightmap == null || lightmap.Width != w || Game1.lightmap.Height != h)
			{
				RenderTarget2D lightmap2 = Game1._lightmap;
				if (lightmap2 != null)
				{
					lightmap2.Dispose();
				}
				Game1._lightmap = new RenderTarget2D(Game1.graphics.GraphicsDevice, w, h, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			}
		}

		// Token: 0x17000159 RID: 345
		// (get) Token: 0x06000A7A RID: 2682 RVA: 0x0006FA57 File Offset: 0x0006DC57
		public static bool IsHudDrawn
		{
			get
			{
				return (Game1.displayHUD || Game1.eventUp) && Game1.gameMode == 3 && !Game1.freezeControls && !Game1.panMode && !Game1.HostPaused && !Game1.game1.takingMapScreenshot;
			}
		}

		// Token: 0x1700015A RID: 346
		// (get) Token: 0x06000A7B RID: 2683 RVA: 0x0006FA93 File Offset: 0x0006DC93
		// (set) Token: 0x06000A7C RID: 2684 RVA: 0x0006FA9A File Offset: 0x0006DC9A
		public static bool isGreenRain
		{
			get
			{
				return Game1._isGreenRain;
			}
			set
			{
				Game1._isGreenRain = value;
				Game1.wasGreenRain = (Game1.wasGreenRain || value);
			}
		}

		// Token: 0x1700015B RID: 347
		// (get) Token: 0x06000A7D RID: 2685 RVA: 0x0006FAAE File Offset: 0x0006DCAE
		// (set) Token: 0x06000A7E RID: 2686 RVA: 0x0006FAC4 File Offset: 0x0006DCC4
		public static bool spawnMonstersAtNight
		{
			get
			{
				return Game1.player.team.spawnMonstersAtNight.Value;
			}
			set
			{
				Game1.player.team.spawnMonstersAtNight.Value = value;
			}
		}

		// Token: 0x1700015C RID: 348
		// (get) Token: 0x06000A7F RID: 2687 RVA: 0x0006FADB File Offset: 0x0006DCDB
		// (set) Token: 0x06000A80 RID: 2688 RVA: 0x0006FAF1 File Offset: 0x0006DCF1
		public static bool UseLegacyRandom
		{
			get
			{
				return Game1.player.team.useLegacyRandom.Value;
			}
			set
			{
				Game1.player.team.useLegacyRandom.Value = value;
			}
		}

		// Token: 0x1700015D RID: 349
		// (get) Token: 0x06000A81 RID: 2689 RVA: 0x0006FB08 File Offset: 0x0006DD08
		// (set) Token: 0x06000A82 RID: 2690 RVA: 0x0006FB14 File Offset: 0x0006DD14
		public static bool fadeToBlack
		{
			get
			{
				return Game1.screenFade.fadeToBlack;
			}
			set
			{
				Game1.screenFade.fadeToBlack = value;
			}
		}

		// Token: 0x1700015E RID: 350
		// (get) Token: 0x06000A83 RID: 2691 RVA: 0x0006FB21 File Offset: 0x0006DD21
		// (set) Token: 0x06000A84 RID: 2692 RVA: 0x0006FB2D File Offset: 0x0006DD2D
		public static bool fadeIn
		{
			get
			{
				return Game1.screenFade.fadeIn;
			}
			set
			{
				Game1.screenFade.fadeIn = value;
			}
		}

		// Token: 0x1700015F RID: 351
		// (get) Token: 0x06000A85 RID: 2693 RVA: 0x0006FB3A File Offset: 0x0006DD3A
		// (set) Token: 0x06000A86 RID: 2694 RVA: 0x0006FB46 File Offset: 0x0006DD46
		public static bool globalFade
		{
			get
			{
				return Game1.screenFade.globalFade;
			}
			set
			{
				Game1.screenFade.globalFade = value;
			}
		}

		// Token: 0x17000160 RID: 352
		// (get) Token: 0x06000A87 RID: 2695 RVA: 0x0006FB53 File Offset: 0x0006DD53
		// (set) Token: 0x06000A88 RID: 2696 RVA: 0x0006FB5F File Offset: 0x0006DD5F
		public static bool nonWarpFade
		{
			get
			{
				return Game1.screenFade.nonWarpFade;
			}
			set
			{
				Game1.screenFade.nonWarpFade = value;
			}
		}

		// Token: 0x17000161 RID: 353
		// (get) Token: 0x06000A89 RID: 2697 RVA: 0x0006FB6C File Offset: 0x0006DD6C
		// (set) Token: 0x06000A8A RID: 2698 RVA: 0x0006FB78 File Offset: 0x0006DD78
		public static float fadeToBlackAlpha
		{
			get
			{
				return Game1.screenFade.fadeToBlackAlpha;
			}
			set
			{
				Game1.screenFade.fadeToBlackAlpha = value;
			}
		}

		// Token: 0x17000162 RID: 354
		// (get) Token: 0x06000A8B RID: 2699 RVA: 0x0006FB85 File Offset: 0x0006DD85
		// (set) Token: 0x06000A8C RID: 2700 RVA: 0x0006FB91 File Offset: 0x0006DD91
		public static float globalFadeSpeed
		{
			get
			{
				return Game1.screenFade.globalFadeSpeed;
			}
			set
			{
				Game1.screenFade.globalFadeSpeed = value;
			}
		}

		// Token: 0x17000163 RID: 355
		// (get) Token: 0x06000A8D RID: 2701 RVA: 0x0006FB9E File Offset: 0x0006DD9E
		public static string CurrentSeasonDisplayName
		{
			get
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:" + Game1.currentSeason);
			}
		}

		// Token: 0x17000164 RID: 356
		// (get) Token: 0x06000A8E RID: 2702 RVA: 0x0006FBB9 File Offset: 0x0006DDB9
		// (set) Token: 0x06000A8F RID: 2703 RVA: 0x0006FBC8 File Offset: 0x0006DDC8
		public static string currentSeason
		{
			get
			{
				return Utility.getSeasonKey(Game1.season);
			}
			set
			{
				Season seasonValue;
				if (Utility.TryParseEnum<Season>(value, out seasonValue))
				{
					Game1.season = seasonValue;
					return;
				}
				throw new ArgumentException("Can't parse value '" + value + "' as a season name.");
			}
		}

		// Token: 0x17000165 RID: 357
		// (get) Token: 0x06000A90 RID: 2704 RVA: 0x0006FBFB File Offset: 0x0006DDFB
		public static int seasonIndex
		{
			get
			{
				return (int)Game1.season;
			}
		}

		// Token: 0x17000166 RID: 358
		// (get) Token: 0x06000A91 RID: 2705 RVA: 0x0006FC02 File Offset: 0x0006DE02
		// (set) Token: 0x06000A92 RID: 2706 RVA: 0x0006FC0C File Offset: 0x0006DE0C
		public static string debugOutput
		{
			get
			{
				return Game1._debugOutput;
			}
			set
			{
				object debugOutputLock = Game1._debugOutputLock;
				lock (debugOutputLock)
				{
					if (Game1._debugOutput != value)
					{
						Game1._debugOutput = value;
						if (!string.IsNullOrEmpty(Game1._debugOutput))
						{
							Game1.log.Debug("DebugOutput: " + Game1._debugOutput);
						}
					}
				}
			}
		}

		// Token: 0x17000167 RID: 359
		// (get) Token: 0x06000A93 RID: 2707 RVA: 0x0006FC80 File Offset: 0x0006DE80
		// (set) Token: 0x06000A94 RID: 2708 RVA: 0x0006FCF6 File Offset: 0x0006DEF6
		public static string elliottBookName
		{
			get
			{
				if (Game1.player != null && Game1.player.DialogueQuestionsAnswered.Contains("958699"))
				{
					return Game1.content.LoadString("Strings\\Events:ElliottBook_mystery");
				}
				if (Game1.player != null && Game1.player.DialogueQuestionsAnswered.Contains("958700"))
				{
					return Game1.content.LoadString("Strings\\Events:ElliottBook_romance");
				}
				return Game1.content.LoadString("Strings\\Events:ElliottBook_default");
			}
			set
			{
			}
		}

		// Token: 0x17000168 RID: 360
		// (get) Token: 0x06000A95 RID: 2709 RVA: 0x0006FCF8 File Offset: 0x0006DEF8
		// (set) Token: 0x06000A96 RID: 2710 RVA: 0x0006FD04 File Offset: 0x0006DF04
		protected static Dictionary<MusicContext, KeyValuePair<string, bool>> _requestedMusicTracks
		{
			get
			{
				return Game1.game1._instanceRequestedMusicTracks;
			}
			set
			{
				Game1.game1._instanceRequestedMusicTracks = value;
			}
		}

		// Token: 0x17000169 RID: 361
		// (get) Token: 0x06000A97 RID: 2711 RVA: 0x0006FD11 File Offset: 0x0006DF11
		// (set) Token: 0x06000A98 RID: 2712 RVA: 0x0006FD1D File Offset: 0x0006DF1D
		protected static MusicContext _activeMusicContext
		{
			get
			{
				return Game1.game1._instanceActiveMusicContext;
			}
			set
			{
				Game1.game1._instanceActiveMusicContext = value;
			}
		}

		// Token: 0x1700016A RID: 362
		// (get) Token: 0x06000A99 RID: 2713 RVA: 0x0006FD2A File Offset: 0x0006DF2A
		// (set) Token: 0x06000A9A RID: 2714 RVA: 0x0006FD36 File Offset: 0x0006DF36
		public static bool isOverridingTrack
		{
			get
			{
				return Game1.game1.instanceIsOverridingTrack;
			}
			set
			{
				Game1.game1.instanceIsOverridingTrack = value;
			}
		}

		// Token: 0x1700016B RID: 363
		// (get) Token: 0x06000A9B RID: 2715 RVA: 0x0006FD43 File Offset: 0x0006DF43
		// (set) Token: 0x06000A9C RID: 2716 RVA: 0x0006FD4B File Offset: 0x0006DF4B
		public bool useUnscaledLighting
		{
			get
			{
				return this._useUnscaledLighting;
			}
			set
			{
				if (this._useUnscaledLighting != value)
				{
					this._useUnscaledLighting = value;
					Game1.allocateLightmap(this.localMultiplayerWindow.Width, this.localMultiplayerWindow.Height);
				}
			}
		}

		// Token: 0x1700016C RID: 364
		// (get) Token: 0x06000A9D RID: 2717 RVA: 0x0006FD78 File Offset: 0x0006DF78
		public static IList<string> mailbox
		{
			get
			{
				return Game1.player.mailbox;
			}
		}

		// Token: 0x1700016D RID: 365
		// (get) Token: 0x06000A9E RID: 2718 RVA: 0x0006FD84 File Offset: 0x0006DF84
		// (set) Token: 0x06000A9F RID: 2719 RVA: 0x0006FD90 File Offset: 0x0006DF90
		public static ICue currentSong
		{
			get
			{
				return Game1.game1.instanceCurrentSong;
			}
			set
			{
				Game1.game1.instanceCurrentSong = value;
			}
		}

		// Token: 0x1700016E RID: 366
		// (get) Token: 0x06000AA0 RID: 2720 RVA: 0x0006FD9D File Offset: 0x0006DF9D
		// (set) Token: 0x06000AA1 RID: 2721 RVA: 0x0006FDA9 File Offset: 0x0006DFA9
		public static PlayerIndex playerOneIndex
		{
			get
			{
				return Game1.game1.instancePlayerOneIndex;
			}
			set
			{
				Game1.game1.instancePlayerOneIndex = value;
			}
		}

		// Token: 0x1700016F RID: 367
		// (get) Token: 0x06000AA2 RID: 2722 RVA: 0x0006FDB6 File Offset: 0x0006DFB6
		// (set) Token: 0x06000AA3 RID: 2723 RVA: 0x0006FDBD File Offset: 0x0006DFBD
		public static int gameModeTicks { get; private set; }

		// Token: 0x17000170 RID: 368
		// (get) Token: 0x06000AA4 RID: 2724 RVA: 0x0006FDC5 File Offset: 0x0006DFC5
		// (set) Token: 0x06000AA5 RID: 2725 RVA: 0x0006FDCC File Offset: 0x0006DFCC
		public static byte gameMode
		{
			get
			{
				return Game1._gameMode;
			}
			set
			{
				if (Game1._gameMode == value)
				{
					return;
				}
				Game1.log.Verbose(string.Concat(new string[]
				{
					"gameMode was '",
					Game1.GameModeToString(Game1._gameMode),
					"', set to '",
					Game1.GameModeToString(value),
					"'."
				}));
				Game1._gameMode = value;
				Game1.gameModeTicks = 0;
			}
		}

		// Token: 0x17000171 RID: 369
		// (get) Token: 0x06000AA6 RID: 2726 RVA: 0x0006FE31 File Offset: 0x0006E031
		// (set) Token: 0x06000AA7 RID: 2727 RVA: 0x0006FE39 File Offset: 0x0006E039
		public bool IsSaving
		{
			get
			{
				return this._isSaving;
			}
			set
			{
				this._isSaving = value;
			}
		}

		// Token: 0x17000172 RID: 370
		// (get) Token: 0x06000AA8 RID: 2728 RVA: 0x0006FE42 File Offset: 0x0006E042
		public static Multiplayer Multiplayer
		{
			get
			{
				return Game1.multiplayer;
			}
		}

		// Token: 0x17000173 RID: 371
		// (get) Token: 0x06000AA9 RID: 2729 RVA: 0x0006FE49 File Offset: 0x0006E049
		public static Stats stats
		{
			get
			{
				return Game1.player.stats;
			}
		}

		// Token: 0x17000174 RID: 372
		// (get) Token: 0x06000AAA RID: 2730 RVA: 0x0006FE55 File Offset: 0x0006E055
		public static Quest questOfTheDay
		{
			get
			{
				return Game1.netWorldState.Value.QuestOfTheDay;
			}
		}

		// Token: 0x17000175 RID: 373
		// (get) Token: 0x06000AAB RID: 2731 RVA: 0x0006FE66 File Offset: 0x0006E066
		// (set) Token: 0x06000AAC RID: 2732 RVA: 0x0006FE70 File Offset: 0x0006E070
		public static IClickableMenu activeClickableMenu
		{
			get
			{
				return Game1._activeClickableMenu;
			}
			set
			{
				bool flag = (Game1.activeClickableMenu is SaveGameMenu || Game1.activeClickableMenu is ShippingMenu) && !(value is SaveGameMenu) && !(value is ShippingMenu);
				IDisposable disposable = Game1._activeClickableMenu as IDisposable;
				if (disposable != null && !Game1._activeClickableMenu.HasDependencies())
				{
					disposable.Dispose();
				}
				if (Game1.textEntry != null && Game1._activeClickableMenu != value)
				{
					Game1.closeTextEntry();
				}
				if (Game1._activeClickableMenu != null && value == null)
				{
					Game1.timerUntilMouseFade = 0;
				}
				Game1._activeClickableMenu = value;
				if (flag)
				{
					Game1.OnDayStarted();
				}
				if (Game1._activeClickableMenu != null)
				{
					if (!Game1.eventUp || (Game1.CurrentEvent != null && Game1.CurrentEvent.playerControlSequence && !Game1.player.UsingTool))
					{
						Game1.player.Halt();
					}
					return;
				}
				if (Game1.nextClickableMenu.Count > 0)
				{
					Game1.activeClickableMenu = Game1.nextClickableMenu[0];
					Game1.nextClickableMenu.RemoveAt(0);
				}
			}
		}

		// Token: 0x17000176 RID: 374
		// (get) Token: 0x06000AAD RID: 2733 RVA: 0x0006FF60 File Offset: 0x0006E160
		// (set) Token: 0x06000AAE RID: 2734 RVA: 0x0006FF68 File Offset: 0x0006E168
		public static IMinigame currentMinigame
		{
			get
			{
				return Game1._currentMinigame;
			}
			set
			{
				Game1._currentMinigame = value;
				if (value == null)
				{
					if (Game1.currentLocation != null)
					{
						Game1.setRichPresence("location", Game1.currentLocation.Name);
					}
					Game1.randomizeDebrisWeatherPositions(Game1.debrisWeather);
					Game1.randomizeRainPositions();
					return;
				}
				if (value.minigameId() != null)
				{
					Game1.setRichPresence("minigame", value.minigameId());
				}
			}
		}

		// Token: 0x06000AAF RID: 2735 RVA: 0x0006FFC1 File Offset: 0x0006E1C1
		public static bool canHaveWeddingOnDay(int day, Season season)
		{
			return !Utility.isFestivalDay(day, season) && !Utility.isGreenRainDay(day, season);
		}

		// Token: 0x06000AB0 RID: 2736 RVA: 0x0006FFD8 File Offset: 0x0006E1D8
		public static void RefreshQuestOfTheDay()
		{
			Quest quest = (!Utility.isFestivalDay() && !Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.season)) ? Utility.getQuestOfTheDay() : null;
			if (quest != null)
			{
				quest.dailyQuest.Set(true);
			}
			if (quest != null)
			{
				quest.reloadObjective();
			}
			if (quest != null)
			{
				quest.reloadDescription();
			}
			Game1.netWorldState.Value.SetQuestOfTheDay(quest);
		}

		// Token: 0x06000AB1 RID: 2737 RVA: 0x00070039 File Offset: 0x0006E239
		public static void ExitToTitle(Action postExitCallback = null)
		{
			IMinigame currentMinigame = Game1.currentMinigame;
			if (currentMinigame != null)
			{
				currentMinigame.unload();
			}
			Game1._requestedMusicTracks.Clear();
			Game1.UpdateRequestedMusicTrack();
			Game1.changeMusicTrack("none", false, MusicContext.Default);
			Game1.setGameMode(0);
			Game1.exitToTitle = true;
			Game1.postExitToTitleCallback = postExitCallback;
		}

		// Token: 0x17000177 RID: 375
		// (get) Token: 0x06000AB2 RID: 2738 RVA: 0x00070078 File Offset: 0x0006E278
		// (set) Token: 0x06000AB3 RID: 2739 RVA: 0x00070089 File Offset: 0x0006E289
		public static Object dishOfTheDay
		{
			get
			{
				return Game1.netWorldState.Value.DishOfTheDay;
			}
			set
			{
				Game1.netWorldState.Value.DishOfTheDay = value;
			}
		}

		// Token: 0x17000178 RID: 376
		// (get) Token: 0x06000AB4 RID: 2740 RVA: 0x0007009B File Offset: 0x0006E29B
		// (set) Token: 0x06000AB5 RID: 2741 RVA: 0x000700A7 File Offset: 0x0006E2A7
		public static KeyboardDispatcher keyboardDispatcher
		{
			get
			{
				return Game1.game1.instanceKeyboardDispatcher;
			}
			set
			{
				Game1.game1.instanceKeyboardDispatcher = value;
			}
		}

		// Token: 0x17000179 RID: 377
		// (get) Token: 0x06000AB6 RID: 2742 RVA: 0x000700B4 File Offset: 0x0006E2B4
		// (set) Token: 0x06000AB7 RID: 2743 RVA: 0x000700C0 File Offset: 0x0006E2C0
		public static Options options
		{
			get
			{
				return Game1.game1.instanceOptions;
			}
			set
			{
				Game1.game1.instanceOptions = value;
			}
		}

		// Token: 0x1700017A RID: 378
		// (get) Token: 0x06000AB8 RID: 2744 RVA: 0x000700CD File Offset: 0x0006E2CD
		// (set) Token: 0x06000AB9 RID: 2745 RVA: 0x000700D9 File Offset: 0x0006E2D9
		public static TextEntryMenu textEntry
		{
			get
			{
				return Game1.game1.instanceTextEntry;
			}
			set
			{
				Game1.game1.instanceTextEntry = value;
			}
		}

		// Token: 0x1700017B RID: 379
		// (get) Token: 0x06000ABA RID: 2746 RVA: 0x000700E6 File Offset: 0x0006E2E6
		public static WorldDate Date
		{
			get
			{
				return Game1.netWorldState.Value.Date;
			}
		}

		// Token: 0x1700017C RID: 380
		// (get) Token: 0x06000ABB RID: 2747 RVA: 0x000700F7 File Offset: 0x0006E2F7
		public static bool NetTimePaused
		{
			get
			{
				return Game1.netWorldState.Get().IsTimePaused;
			}
		}

		// Token: 0x1700017D RID: 381
		// (get) Token: 0x06000ABC RID: 2748 RVA: 0x00070108 File Offset: 0x0006E308
		public static bool HostPaused
		{
			get
			{
				return Game1.netWorldState.Get().IsPaused;
			}
		}

		// Token: 0x1700017E RID: 382
		// (get) Token: 0x06000ABD RID: 2749 RVA: 0x00070119 File Offset: 0x0006E319
		public static bool IsMultiplayer
		{
			get
			{
				return Game1.otherFarmers.Count > 0;
			}
		}

		// Token: 0x1700017F RID: 383
		// (get) Token: 0x06000ABE RID: 2750 RVA: 0x00070128 File Offset: 0x0006E328
		public static bool IsClient
		{
			get
			{
				return Game1.multiplayerMode == 1;
			}
		}

		// Token: 0x17000180 RID: 384
		// (get) Token: 0x06000ABF RID: 2751 RVA: 0x00070132 File Offset: 0x0006E332
		public static bool IsServer
		{
			get
			{
				return Game1.multiplayerMode == 2;
			}
		}

		// Token: 0x17000181 RID: 385
		// (get) Token: 0x06000AC0 RID: 2752 RVA: 0x0007013C File Offset: 0x0006E33C
		public static bool IsMasterGame
		{
			get
			{
				return Game1.multiplayerMode == 0 || Game1.multiplayerMode == 2;
			}
		}

		// Token: 0x17000182 RID: 386
		// (get) Token: 0x06000AC1 RID: 2753 RVA: 0x00070150 File Offset: 0x0006E350
		public static bool HasDedicatedHost
		{
			get
			{
				if (Game1.multiplayerMode != 0)
				{
					Farmer player = Game1.player;
					bool? flag;
					if (player == null)
					{
						flag = null;
					}
					else
					{
						FarmerTeam team = player.team;
						flag = ((team != null) ? new bool?(team.hasDedicatedHost.Value) : null);
					}
					bool? flag2 = flag;
					return flag2.GetValueOrDefault();
				}
				return false;
			}
		}

		// Token: 0x17000183 RID: 387
		// (get) Token: 0x06000AC2 RID: 2754 RVA: 0x000701A5 File Offset: 0x0006E3A5
		public static bool IsDedicatedHost
		{
			get
			{
				return Game1.IsServer && Game1.HasDedicatedHost;
			}
		}

		// Token: 0x17000184 RID: 388
		// (get) Token: 0x06000AC3 RID: 2755 RVA: 0x000701B5 File Offset: 0x0006E3B5
		public static Farmer MasterPlayer
		{
			get
			{
				if (!Game1.IsMasterGame)
				{
					return Game1.serverHost.Value;
				}
				return Game1.player;
			}
		}

		// Token: 0x17000185 RID: 389
		// (get) Token: 0x06000AC4 RID: 2756 RVA: 0x000701CE File Offset: 0x0006E3CE
		// (set) Token: 0x06000AC5 RID: 2757 RVA: 0x000701E3 File Offset: 0x0006E3E3
		public static bool IsChatting
		{
			get
			{
				return Game1.chatBox != null && Game1.chatBox.isActive();
			}
			set
			{
				if (value == Game1.chatBox.isActive())
				{
					return;
				}
				if (value)
				{
					Game1.chatBox.activate();
					return;
				}
				Game1.chatBox.clickAway();
			}
		}

		// Token: 0x17000186 RID: 390
		// (get) Token: 0x06000AC6 RID: 2758 RVA: 0x0007020B File Offset: 0x0006E40B
		public static Event CurrentEvent
		{
			get
			{
				if (Game1.currentLocation == null)
				{
					return null;
				}
				return Game1.currentLocation.currentEvent;
			}
		}

		// Token: 0x17000187 RID: 391
		// (get) Token: 0x06000AC7 RID: 2759 RVA: 0x00070220 File Offset: 0x0006E420
		public static MineShaft mine
		{
			get
			{
				LocationRequest locationRequest = Game1.locationRequest;
				return (((locationRequest != null) ? locationRequest.Location : null) as MineShaft) ?? (Game1.currentLocation as MineShaft);
			}
		}

		// Token: 0x17000188 RID: 392
		// (get) Token: 0x06000AC8 RID: 2760 RVA: 0x00070246 File Offset: 0x0006E446
		public static int CurrentMineLevel
		{
			get
			{
				MineShaft mineShaft = Game1.currentLocation as MineShaft;
				if (mineShaft == null)
				{
					return 0;
				}
				return mineShaft.mineLevel;
			}
		}

		// Token: 0x06000AC9 RID: 2761 RVA: 0x00070260 File Offset: 0x0006E460
		static Game1()
		{
			Game1.GameAssemblyName = typeof(Game1).Assembly.GetName().Name;
			AssemblyInformationalVersionAttribute attribute = typeof(Game1).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
			if (!string.IsNullOrWhiteSpace((attribute != null) ? attribute.InformationalVersion : null))
			{
				string[] parts = attribute.InformationalVersion.Split(',', StringSplitOptions.None);
				if (parts.Length == 4)
				{
					Game1.version = parts[0].Trim();
					if (!string.IsNullOrWhiteSpace(parts[1]))
					{
						Game1.versionLabel = parts[1].Trim();
					}
					if (!string.IsNullOrWhiteSpace(parts[2]))
					{
						int buildNumber;
						if (!int.TryParse(parts[2], out buildNumber))
						{
							throw new InvalidOperationException("Can't parse game build number value '" + parts[2] + "' as a number.");
						}
						Game1.versionBuildNumber = buildNumber;
					}
					if (!string.IsNullOrWhiteSpace(parts[3]))
					{
						Multiplayer.protocolVersionOverride = parts[3].Trim();
					}
				}
			}
			if (string.IsNullOrWhiteSpace(Game1.version))
			{
				throw new InvalidOperationException("No game version found in assembly info.");
			}
		}

		// Token: 0x06000ACA RID: 2762 RVA: 0x000708F8 File Offset: 0x0006EAF8
		public Game1(PlayerIndex player_index, int index) : this()
		{
			this.instancePlayerOneIndex = player_index;
			this.instanceIndex = index;
		}

		// Token: 0x06000ACB RID: 2763 RVA: 0x00070910 File Offset: 0x0006EB10
		public Game1()
		{
			this.instanceId = GameRunner.instance.GetNewInstanceID();
			if (Program.gamePtr == null)
			{
				Program.gamePtr = this;
			}
			Game1._temporaryContent = this.CreateContentManager(base.Content.ServiceProvider, base.Content.RootDirectory);
		}

		// Token: 0x06000ACC RID: 2764 RVA: 0x000709C0 File Offset: 0x0006EBC0
		public void TranslateFields()
		{
			LocalizedContentManager.localizedAssetNames.Clear();
			BaseEnchantment.ResetEnchantments();
			Game1.samBandName = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2156");
			Game1.elliottBookName = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2157");
			Game1.objectSpriteSheet = Game1.content.Load<Texture2D>("Maps\\springobjects");
			Game1.objectSpriteSheet_2 = Game1.content.Load<Texture2D>("TileSheets\\Objects_2");
			Game1.bobbersTexture = Game1.content.Load<Texture2D>("TileSheets\\bobbers");
			Game1.dialogueFont = Game1.content.Load<SpriteFont>("Fonts\\SpriteFont1");
			Game1.smallFont = Game1.content.Load<SpriteFont>("Fonts\\SmallFont");
			Game1.smallFont.LineSpacing = 28;
			switch (LocalizedContentManager.CurrentLanguageCode)
			{
			case LocalizedContentManager.LanguageCode.ko:
				Game1.smallFont.LineSpacing += 16;
				break;
			case LocalizedContentManager.LanguageCode.tr:
				Game1.smallFont.LineSpacing += 4;
				break;
			case LocalizedContentManager.LanguageCode.mod:
				Game1.smallFont.LineSpacing = LocalizedContentManager.CurrentModLanguage.SmallFontLineSpacing;
				break;
			}
			Game1.tinyFont = Game1.content.Load<SpriteFont>("Fonts\\tinyFont");
			Game1.objectData = DataLoader.Objects(Game1.content);
			Game1.bigCraftableData = DataLoader.BigCraftables(Game1.content);
			Game1.achievements = DataLoader.Achievements(Game1.content);
			CraftingRecipe.craftingRecipes = DataLoader.CraftingRecipes(Game1.content);
			CraftingRecipe.cookingRecipes = DataLoader.CookingRecipes(Game1.content);
			ItemRegistry.ResetCache();
			MovieTheater.ClearCachedLocalizedData();
			Game1.mouseCursors = Game1.content.Load<Texture2D>("LooseSprites\\Cursors");
			Game1.mouseCursors2 = Game1.content.Load<Texture2D>("LooseSprites\\Cursors2");
			Game1.mouseCursors_1_6 = Game1.content.Load<Texture2D>("LooseSprites\\Cursors_1_6");
			Game1.giftboxTexture = Game1.content.Load<Texture2D>("LooseSprites\\Giftbox");
			Game1.controllerMaps = Game1.content.Load<Texture2D>("LooseSprites\\ControllerMaps");
			Game1.NPCGiftTastes = DataLoader.NpcGiftTastes(Game1.content);
			Game1._shortDayDisplayName[0] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3042");
			Game1._shortDayDisplayName[1] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3043");
			Game1._shortDayDisplayName[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3044");
			Game1._shortDayDisplayName[3] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3045");
			Game1._shortDayDisplayName[4] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3046");
			Game1._shortDayDisplayName[5] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3047");
			Game1._shortDayDisplayName[6] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3048");
		}

		// Token: 0x06000ACD RID: 2765 RVA: 0x00070C4C File Offset: 0x0006EE4C
		public void exitEvent(object sender, EventArgs e)
		{
			Game1.multiplayer.Disconnect(Multiplayer.DisconnectType.ClosedGame);
			Game1.keyboardDispatcher.Cleanup();
		}

		// Token: 0x06000ACE RID: 2766 RVA: 0x00070C63 File Offset: 0x0006EE63
		public void refreshWindowSettings()
		{
			GameRunner.instance.OnWindowSizeChange(null, null);
		}

		// Token: 0x06000ACF RID: 2767 RVA: 0x00070C74 File Offset: 0x0006EE74
		public void Window_ClientSizeChanged(object sender, EventArgs e)
		{
			if (this._windowResizing)
			{
				return;
			}
			Game1.log.Verbose("Window_ClientSizeChanged(); Window.ClientBounds=" + base.Window.ClientBounds.ToString());
			if (Game1.options == null)
			{
				Game1.log.Verbose("Window_ClientSizeChanged(); options is null, returning.");
				return;
			}
			this._windowResizing = true;
			int w = Game1.graphics.IsFullScreen ? Game1.graphics.PreferredBackBufferWidth : base.Window.ClientBounds.Width;
			int h = Game1.graphics.IsFullScreen ? Game1.graphics.PreferredBackBufferHeight : base.Window.ClientBounds.Height;
			GameRunner.instance.ExecuteForInstances(delegate(Game1 instance)
			{
				instance.SetWindowSize(w, h);
			});
			this._windowResizing = false;
		}

		// Token: 0x06000AD0 RID: 2768 RVA: 0x00070D54 File Offset: 0x0006EF54
		public virtual void SetWindowSize(int w, int h)
		{
			Microsoft.Xna.Framework.Rectangle oldWindow = new Microsoft.Xna.Framework.Rectangle(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height);
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				if (w < 1280 && !Game1.graphics.IsFullScreen)
				{
					w = 1280;
				}
				if (h < 720 && !Game1.graphics.IsFullScreen)
				{
					h = 720;
				}
			}
			if (!Game1.graphics.IsFullScreen && base.Window.AllowUserResizing)
			{
				Game1.graphics.PreferredBackBufferWidth = w;
				Game1.graphics.PreferredBackBufferHeight = h;
			}
			if (base.IsMainInstance && Game1.graphics.SynchronizeWithVerticalRetrace != Game1.options.vsyncEnabled)
			{
				Game1.graphics.SynchronizeWithVerticalRetrace = Game1.options.vsyncEnabled;
				Game1.log.Verbose("Vsync toggled: " + Game1.graphics.SynchronizeWithVerticalRetrace.ToString());
			}
			Game1.graphics.ApplyChanges();
			try
			{
				if (Game1.graphics.IsFullScreen)
				{
					this.localMultiplayerWindow = new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.graphics.PreferredBackBufferWidth, Game1.graphics.PreferredBackBufferHeight);
				}
				else
				{
					this.localMultiplayerWindow = new Microsoft.Xna.Framework.Rectangle(0, 0, w, h);
				}
			}
			catch (Exception)
			{
			}
			Game1.defaultDeviceViewport = new Viewport(this.localMultiplayerWindow);
			List<Vector4> screen_splits = new List<Vector4>();
			if (GameRunner.instance.gameInstances.Count <= 1)
			{
				screen_splits.Add(new Vector4(0f, 0f, 1f, 1f));
			}
			else
			{
				switch (GameRunner.instance.gameInstances.Count)
				{
				case 2:
					screen_splits.Add(new Vector4(0f, 0f, 0.5f, 1f));
					screen_splits.Add(new Vector4(0.5f, 0f, 0.5f, 1f));
					break;
				case 3:
					screen_splits.Add(new Vector4(0f, 0f, 1f, 0.5f));
					screen_splits.Add(new Vector4(0f, 0.5f, 0.5f, 0.5f));
					screen_splits.Add(new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
					break;
				case 4:
					screen_splits.Add(new Vector4(0f, 0f, 0.5f, 0.5f));
					screen_splits.Add(new Vector4(0.5f, 0f, 0.5f, 0.5f));
					screen_splits.Add(new Vector4(0f, 0.5f, 0.5f, 0.5f));
					screen_splits.Add(new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
					break;
				}
			}
			if (GameRunner.instance.gameInstances.Count <= 1)
			{
				this.zoomModifier = 1f;
			}
			else
			{
				this.zoomModifier = 0.5f;
			}
			Vector4 current_screen_split = screen_splits[Game1.game1.instanceIndex];
			Vector2? old_ui_dimensions = null;
			if (this.uiScreen != null)
			{
				old_ui_dimensions = new Vector2?(new Vector2((float)this.uiScreen.Width, (float)this.uiScreen.Height));
			}
			this.localMultiplayerWindow.X = (int)((float)w * current_screen_split.X);
			this.localMultiplayerWindow.Y = (int)((float)h * current_screen_split.Y);
			this.localMultiplayerWindow.Width = (int)Math.Ceiling((double)((float)w * current_screen_split.Z));
			this.localMultiplayerWindow.Height = (int)Math.Ceiling((double)((float)h * current_screen_split.W));
			try
			{
				int sw = (int)Math.Ceiling((double)((float)this.localMultiplayerWindow.Width * (1f / Game1.options.zoomLevel)));
				int sh = (int)Math.Ceiling((double)((float)this.localMultiplayerWindow.Height * (1f / Game1.options.zoomLevel)));
				this.screen = new RenderTarget2D(Game1.graphics.GraphicsDevice, sw, sh, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents)
				{
					Name = "@Game1.screen"
				};
				int uw = (int)Math.Ceiling((double)((float)this.localMultiplayerWindow.Width / Game1.options.uiScale));
				int uh = (int)Math.Ceiling((double)((float)this.localMultiplayerWindow.Height / Game1.options.uiScale));
				this.uiScreen = new RenderTarget2D(Game1.graphics.GraphicsDevice, uw, uh, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents)
				{
					Name = "@Game1.uiScreen"
				};
			}
			catch (Exception)
			{
			}
			Game1.updateViewportForScreenSizeChange(false, this.localMultiplayerWindow.Width, this.localMultiplayerWindow.Height);
			if (old_ui_dimensions == null || old_ui_dimensions.Value.X != (float)this.uiScreen.Width || old_ui_dimensions.Value.Y != (float)this.uiScreen.Height)
			{
				Game1.PushUIMode();
				TextEntryMenu textEntry = Game1.textEntry;
				if (textEntry != null)
				{
					textEntry.gameWindowSizeChanged(oldWindow, new Microsoft.Xna.Framework.Rectangle(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height));
				}
				foreach (IClickableMenu clickableMenu in Game1.onScreenMenus)
				{
					clickableMenu.gameWindowSizeChanged(oldWindow, new Microsoft.Xna.Framework.Rectangle(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height));
				}
				IMinigame currentMinigame = Game1.currentMinigame;
				if (currentMinigame != null)
				{
					currentMinigame.changeScreenSize();
				}
				IClickableMenu activeClickableMenu = Game1.activeClickableMenu;
				if (activeClickableMenu != null)
				{
					activeClickableMenu.gameWindowSizeChanged(oldWindow, new Microsoft.Xna.Framework.Rectangle(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height));
				}
				IClickableMenu activeClickableMenu2 = Game1.activeClickableMenu;
				if (((activeClickableMenu2 != null) ? activeClickableMenu2.GetType() : null) == typeof(GameMenu))
				{
					GameMenu gameMenu = Game1.activeClickableMenu as GameMenu;
					IClickableMenu oldPage = gameMenu.GetCurrentPage();
					IClickableMenu currentPage = (Game1.activeClickableMenu = new GameMenu(gameMenu.currentTab, -1, true)).GetCurrentPage();
					CollectionsPage collectionsPage = currentPage as CollectionsPage;
					if (collectionsPage == null)
					{
						OptionsPage optionsPage = currentPage as OptionsPage;
						if (optionsPage == null)
						{
							SocialPage socialPage = currentPage as SocialPage;
							if (socialPage != null)
							{
								socialPage.postWindowSizeChange(oldPage);
							}
						}
						else
						{
							optionsPage.postWindowSizeChange(oldPage);
						}
					}
					else
					{
						collectionsPage.postWindowSizeChange(oldPage);
					}
				}
				Game1.PopUIMode();
			}
		}

		// Token: 0x06000AD1 RID: 2769 RVA: 0x00071400 File Offset: 0x0006F600
		private void Game1_Exiting(object sender, EventArgs e)
		{
			Program.sdk.Shutdown();
		}

		// Token: 0x06000AD2 RID: 2770 RVA: 0x0007140C File Offset: 0x0006F60C
		public static void setGameMode(byte mode)
		{
			Game1.log.Verbose("setGameMode( '" + Game1.GameModeToString(mode) + "' )");
			Game1._gameMode = mode;
			LocalizedContentManager temporaryContent = Game1.temporaryContent;
			if (temporaryContent != null)
			{
				temporaryContent.Unload();
			}
			if (mode == 0)
			{
				bool skip = false;
				if (Game1.activeClickableMenu != null)
				{
					GameTime gameTime = Game1.currentGameTime;
					if (gameTime != null && gameTime.TotalGameTime.TotalSeconds > (double)10)
					{
						skip = true;
					}
				}
				if (Game1.game1.instanceIndex > 0)
				{
					return;
				}
				TitleMenu titleMenu = new TitleMenu();
				Game1.activeClickableMenu = titleMenu;
				if (skip)
				{
					titleMenu.skipToTitleButtons();
				}
			}
		}

		// Token: 0x06000AD3 RID: 2771 RVA: 0x000714A0 File Offset: 0x0006F6A0
		public static void updateViewportForScreenSizeChange(bool fullscreenChange, int width, int height)
		{
			Game1.forceSnapOnNextViewportUpdate = true;
			if (Game1.graphics.GraphicsDevice != null)
			{
				Game1.allocateLightmap(width, height);
			}
			width = (int)Math.Ceiling((double)((float)width / Game1.options.zoomLevel));
			height = (int)Math.Ceiling((double)((float)height / Game1.options.zoomLevel));
			Point center = new Point(Game1.viewport.X + Game1.viewport.Width / 2, Game1.viewport.Y + Game1.viewport.Height / 2);
			bool sizeDirty = Game1.viewport.Width != width || Game1.viewport.Height != height;
			Game1.viewport = new xTile.Dimensions.Rectangle(center.X - width / 2, center.Y - height / 2, width, height);
			if (Game1.currentLocation == null)
			{
				return;
			}
			if (!Game1.eventUp)
			{
				if (Game1.viewport.X >= 0 || !Game1.currentLocation.IsOutdoors || fullscreenChange)
				{
					center = new Point(Game1.viewport.X + Game1.viewport.Width / 2, Game1.viewport.Y + Game1.viewport.Height / 2);
					Game1.viewport = new xTile.Dimensions.Rectangle(center.X - width / 2, center.Y - height / 2, width, height);
					Game1.UpdateViewPort(true, center);
				}
				if (sizeDirty)
				{
					Game1.forceSnapOnNextViewportUpdate = true;
					Game1.randomizeRainPositions();
					Game1.randomizeDebrisWeatherPositions(Game1.debrisWeather);
				}
				return;
			}
			if (Game1.IsFakedBlackScreen())
			{
				return;
			}
			if (!Game1.currentLocation.IsOutdoors)
			{
				return;
			}
			Game1.clampViewportToGameMap();
		}

		// Token: 0x06000AD4 RID: 2772 RVA: 0x00071628 File Offset: 0x0006F828
		public void Instance_Initialize()
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			this.Initialize();
			stopwatch.Stop();
			IGameLogger gameLogger = Game1.log;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(44, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Instance_Initialize() finished, elapsed = '");
			defaultInterpolatedStringHandler.AppendFormatted<TimeSpan>(stopwatch.Elapsed);
			defaultInterpolatedStringHandler.AppendLiteral("'");
			gameLogger.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x06000AD5 RID: 2773 RVA: 0x00071687 File Offset: 0x0006F887
		public static bool IsFading()
		{
			return Game1.globalFade || (Game1.fadeIn && Game1.fadeToBlackAlpha > 0f) || (Game1.fadeToBlack && Game1.fadeToBlackAlpha < 1f);
		}

		// Token: 0x06000AD6 RID: 2774 RVA: 0x000716BC File Offset: 0x0006F8BC
		public static bool IsFakedBlackScreen()
		{
			return Game1.currentMinigame == null && (Game1.CurrentEvent == null || Game1.CurrentEvent.currentCustomEventScript == null) && Game1.eventUp && (float)((int)Math.Floor((double)((float)new Point(Game1.viewport.X + Game1.viewport.Width / 2, Game1.viewport.Y + Game1.viewport.Height / 2).X / 64f))) <= -200f;
		}

		// Token: 0x06000AD7 RID: 2775 RVA: 0x00071741 File Offset: 0x0006F941
		private void DoThreadedInitTask(ThreadStart initTask)
		{
			if (this.ShouldLoadIncrementally)
			{
				new Thread(initTask)
				{
					CurrentCulture = CultureInfo.InvariantCulture,
					Priority = ThreadPriority.Highest
				}.Start();
				return;
			}
			initTask();
		}

		// Token: 0x06000AD8 RID: 2776 RVA: 0x00071770 File Offset: 0x0006F970
		protected override void Initialize()
		{
			Game1.keyboardDispatcher = new KeyboardDispatcher(base.Window);
			Game1.screenFade = new ScreenFade(new Func<bool>(this.onFadeToBlackComplete), new Action(Game1.onFadedBackInComplete));
			Game1.options = new Options();
			Game1.options.musicVolumeLevel = 1f;
			Game1.options.soundVolumeLevel = 1f;
			Game1.otherFarmers = new NetRootDictionary<long, Farmer>();
			this.DoThreadedInitTask(new ThreadStart(this.InitializeSerializers));
			Game1.viewport = new xTile.Dimensions.Rectangle(new Size(Game1.graphics.PreferredBackBufferWidth, Game1.graphics.PreferredBackBufferHeight));
			Game1.currentSong = null;
			this.DoThreadedInitTask(new ThreadStart(this.InitializeSounds));
			int width = Game1.graphics.GraphicsDevice.Viewport.Width;
			int height = Game1.graphics.GraphicsDevice.Viewport.Height;
			this.screen = new RenderTarget2D(Game1.graphics.GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			Game1.allocateLightmap(width, height);
			Game1.previousViewportPosition = Vector2.Zero;
			Game1.PushUIMode();
			Game1.PopUIMode();
			Game1.setRichPresence("menus", null);
		}

		// Token: 0x06000AD9 RID: 2777 RVA: 0x000718A0 File Offset: 0x0006FAA0
		private void InitializeSounds()
		{
			if (base.IsMainInstance)
			{
				try
				{
					string rootpath = base.Content.RootDirectory;
					AudioEngine audioEngine = new AudioEngine(Path.Combine(rootpath, "XACT", "FarmerSounds.xgs"));
					audioEngine.GetReverbSettings()[18] = 4f;
					audioEngine.GetReverbSettings()[17] = -12f;
					Game1.audioEngine = new AudioEngineWrapper(audioEngine);
					Game1.waveBank = new WaveBank(Game1.audioEngine.Engine, Path.Combine(rootpath, "XACT", "Wave Bank.xwb"));
					Game1.waveBank1_4 = new WaveBank(Game1.audioEngine.Engine, Path.Combine(rootpath, "XACT", "Wave Bank(1.4).xwb"));
					Game1.soundBank = new SoundBankWrapper(new SoundBank(Game1.audioEngine.Engine, Path.Combine(rootpath, "XACT", "Sound Bank.xsb")));
				}
				catch (Exception e)
				{
					Game1.log.Error("Game.Initialize() caught exception initializing XACT.", e);
					Game1.audioEngine = new DummyAudioEngine();
					Game1.soundBank = new DummySoundBank();
				}
			}
			Game1.audioEngine.Update();
			Game1.musicCategory = Game1.audioEngine.GetCategory("Music");
			Game1.soundCategory = Game1.audioEngine.GetCategory("Sound");
			Game1.ambientCategory = Game1.audioEngine.GetCategory("Ambient");
			Game1.footstepCategory = Game1.audioEngine.GetCategory("Footsteps");
			Game1.wind = Game1.soundBank.GetCue("wind");
			Game1.chargeUpSound = Game1.soundBank.GetCue("toolCharge");
			AmbientLocationSounds.InitShared();
			Game1.FinishedFirstInitSounds = true;
		}

		// Token: 0x06000ADA RID: 2778 RVA: 0x00071A40 File Offset: 0x0006FC40
		private void InitializeSerializers()
		{
			Game1.otherFarmers.Serializer = SaveSerializer.GetSerializer(typeof(Farmer));
			if (StartupPreferences.serializer == null)
			{
				StartupPreferences.serializer = SaveSerializer.GetSerializer(typeof(StartupPreferences));
			}
			Game1.FinishedFirstInitSerializers = true;
		}

		// Token: 0x06000ADB RID: 2779 RVA: 0x00071A7E File Offset: 0x0006FC7E
		public static void pauseThenDoFunction(int pauseTime, Game1.afterFadeFunction function)
		{
			Game1.afterPause = function;
			Game1.pauseThenDoFunctionTimer = pauseTime;
		}

		// Token: 0x06000ADC RID: 2780 RVA: 0x00071A8C File Offset: 0x0006FC8C
		protected internal virtual LocalizedContentManager CreateContentManager(IServiceProvider serviceProvider, string rootDirectory)
		{
			return new LocalizedContentManager(serviceProvider, rootDirectory);
		}

		// Token: 0x06000ADD RID: 2781 RVA: 0x00071A95 File Offset: 0x0006FC95
		protected internal virtual IDisplayDevice CreateDisplayDevice(ContentManager content, GraphicsDevice graphicsDevice)
		{
			return new XnaDisplayDevice(content, graphicsDevice);
		}

		// Token: 0x06000ADE RID: 2782 RVA: 0x00071AA0 File Offset: 0x0006FCA0
		public void Instance_LoadContent()
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			this.LoadContent();
			stopwatch.Stop();
			IGameLogger gameLogger = Game1.log;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(45, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Instance_LoadContent() finished, elapsed = '");
			defaultInterpolatedStringHandler.AppendFormatted<TimeSpan>(stopwatch.Elapsed);
			defaultInterpolatedStringHandler.AppendLiteral("'");
			gameLogger.Verbose(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x06000ADF RID: 2783 RVA: 0x00071B00 File Offset: 0x0006FD00
		protected override void LoadContent()
		{
			Game1.content = this.CreateContentManager(base.Content.ServiceProvider, base.Content.RootDirectory);
			this.xTileContent = this.CreateContentManager(Game1.content.ServiceProvider, Game1.content.RootDirectory);
			Game1.mapDisplayDevice = this.CreateDisplayDevice(Game1.content, base.GraphicsDevice);
			Game1.spriteBatch = new SpriteBatch(base.GraphicsDevice);
			Game1.netWorldState = new NetRoot<NetWorldState>(new NetWorldState());
			Game1.LoadContentEnumerator = this.GetLoadContentEnumerator();
			if (!this.ShouldLoadIncrementally)
			{
				while (Game1.LoadContentEnumerator.MoveNext())
				{
				}
				Game1.LoadContentEnumerator = null;
				this.AfterLoadContent();
			}
		}

		// Token: 0x06000AE0 RID: 2784 RVA: 0x00071BB0 File Offset: 0x0006FDB0
		private void AfterLoadContent()
		{
			Game1.saveOnNewDay = true;
			if (Game1.gameMode == 4)
			{
				Game1.fadeToBlackAlpha = -0.5f;
				Game1.fadeIn = true;
			}
			if (Game1.random.NextDouble() < 0.7)
			{
				Game1.isDebrisWeather = true;
				Game1.populateDebrisWeatherArray();
			}
			Game1.resetPlayer();
			Game1.CueModification.OnStartup();
			Game1.setGameMode(0);
		}

		// Token: 0x06000AE1 RID: 2785 RVA: 0x00071C10 File Offset: 0x0006FE10
		private IEnumerator<int> GetLoadContentEnumerator()
		{
			int step = 0;
			Game1.bigCraftableData = DataLoader.BigCraftables(Game1.content);
			int num = step + 1;
			step = num;
			yield return num;
			Game1.objectData = DataLoader.Objects(Game1.content);
			num = step + 1;
			step = num;
			yield return num;
			Game1.cropData = DataLoader.Crops(Game1.content);
			num = step + 1;
			step = num;
			yield return num;
			Game1.characterData = DataLoader.Characters(Game1.content);
			num = step + 1;
			step = num;
			yield return num;
			Game1.pantsData = DataLoader.Pants(Game1.content);
			num = step + 1;
			step = num;
			yield return num;
			Game1.shirtData = DataLoader.Shirts(Game1.content);
			num = step + 1;
			step = num;
			yield return num;
			Game1.toolData = DataLoader.Tools(Game1.content);
			num = step + 1;
			step = num;
			yield return num;
			Game1.weaponData = DataLoader.Weapons(Game1.content);
			num = step + 1;
			step = num;
			yield return num;
			Game1.achievements = DataLoader.Achievements(Game1.content);
			num = step + 1;
			step = num;
			yield return num;
			Game1.buildingData = DataLoader.Buildings(Game1.content);
			num = step + 1;
			step = num;
			yield return num;
			Game1.farmAnimalData = DataLoader.FarmAnimals(Game1.content);
			num = step + 1;
			step = num;
			yield return num;
			Game1.floorPathData = DataLoader.FloorsAndPaths(Game1.content);
			num = step + 1;
			step = num;
			yield return num;
			Game1.fruitTreeData = DataLoader.FruitTrees(Game1.content);
			num = step + 1;
			step = num;
			yield return num;
			Game1.locationData = DataLoader.Locations(Game1.content);
			num = step + 1;
			step = num;
			yield return num;
			Game1.locationContextData = DataLoader.LocationContexts(Game1.content);
			num = step + 1;
			step = num;
			yield return num;
			Game1.petData = DataLoader.Pets(Game1.content);
			num = step + 1;
			step = num;
			yield return num;
			Game1.NPCGiftTastes = DataLoader.NpcGiftTastes(Game1.content);
			num = step + 1;
			step = num;
			yield return num;
			CraftingRecipe.InitShared();
			num = step + 1;
			step = num;
			yield return num;
			ItemRegistry.ResetCache();
			num = step + 1;
			step = num;
			yield return num;
			Game1.jukeboxTrackData = new Dictionary<string, JukeboxTrackData>(StringComparer.OrdinalIgnoreCase);
			foreach (KeyValuePair<string, JukeboxTrackData> pair in DataLoader.JukeboxTracks(Game1.content))
			{
				if (!Game1.jukeboxTrackData.TryAdd(pair.Key, pair.Value))
				{
					Game1.log.Warn("Ignored duplicate ID '" + pair.Key + "' in Data/JukeboxTracks.");
				}
			}
			num = step + 1;
			step = num;
			yield return num;
			Game1.concessionsSpriteSheet = Game1.content.Load<Texture2D>("LooseSprites\\Concessions");
			num = step + 1;
			step = num;
			yield return num;
			Game1.birdsSpriteSheet = Game1.content.Load<Texture2D>("LooseSprites\\birds");
			num = step + 1;
			step = num;
			yield return num;
			Game1.daybg = Game1.content.Load<Texture2D>("LooseSprites\\daybg");
			num = step + 1;
			step = num;
			yield return num;
			Game1.nightbg = Game1.content.Load<Texture2D>("LooseSprites\\nightbg");
			num = step + 1;
			step = num;
			yield return num;
			Game1.menuTexture = Game1.content.Load<Texture2D>("Maps\\MenuTiles");
			num = step + 1;
			step = num;
			yield return num;
			Game1.uncoloredMenuTexture = Game1.content.Load<Texture2D>("Maps\\MenuTilesUncolored");
			num = step + 1;
			step = num;
			yield return num;
			Game1.lantern = Game1.content.Load<Texture2D>("LooseSprites\\Lighting\\lantern");
			num = step + 1;
			step = num;
			yield return num;
			Game1.windowLight = Game1.content.Load<Texture2D>("LooseSprites\\Lighting\\windowLight");
			num = step + 1;
			step = num;
			yield return num;
			Game1.sconceLight = Game1.content.Load<Texture2D>("LooseSprites\\Lighting\\sconceLight");
			num = step + 1;
			step = num;
			yield return num;
			Game1.cauldronLight = Game1.content.Load<Texture2D>("LooseSprites\\Lighting\\greenLight");
			num = step + 1;
			step = num;
			yield return num;
			Game1.indoorWindowLight = Game1.content.Load<Texture2D>("LooseSprites\\Lighting\\indoorWindowLight");
			num = step + 1;
			step = num;
			yield return num;
			Game1.shadowTexture = Game1.content.Load<Texture2D>("LooseSprites\\shadow");
			num = step + 1;
			step = num;
			yield return num;
			Game1.mouseCursors = Game1.content.Load<Texture2D>("LooseSprites\\Cursors");
			num = step + 1;
			step = num;
			yield return num;
			Game1.mouseCursors2 = Game1.content.Load<Texture2D>("LooseSprites\\Cursors2");
			num = step + 1;
			step = num;
			yield return num;
			Game1.mouseCursors_1_6 = Game1.content.Load<Texture2D>("LooseSprites\\Cursors_1_6");
			num = step + 1;
			step = num;
			yield return num;
			Game1.giftboxTexture = Game1.content.Load<Texture2D>("LooseSprites\\Giftbox");
			num = step + 1;
			step = num;
			yield return num;
			Game1.controllerMaps = Game1.content.Load<Texture2D>("LooseSprites\\ControllerMaps");
			num = step + 1;
			step = num;
			yield return num;
			Game1.animations = Game1.content.Load<Texture2D>("TileSheets\\animations");
			num = step + 1;
			step = num;
			yield return num;
			Game1.objectSpriteSheet = Game1.content.Load<Texture2D>("Maps\\springobjects");
			num = step + 1;
			step = num;
			yield return num;
			Game1.objectSpriteSheet_2 = Game1.content.Load<Texture2D>("TileSheets\\Objects_2");
			num = step + 1;
			step = num;
			yield return num;
			Game1.bobbersTexture = Game1.content.Load<Texture2D>("TileSheets\\bobbers");
			num = step + 1;
			step = num;
			yield return num;
			Game1.cropSpriteSheet = Game1.content.Load<Texture2D>("TileSheets\\crops");
			num = step + 1;
			step = num;
			yield return num;
			Game1.emoteSpriteSheet = Game1.content.Load<Texture2D>("TileSheets\\emotes");
			num = step + 1;
			step = num;
			yield return num;
			Game1.debrisSpriteSheet = Game1.content.Load<Texture2D>("TileSheets\\debris");
			num = step + 1;
			step = num;
			yield return num;
			Game1.bigCraftableSpriteSheet = Game1.content.Load<Texture2D>("TileSheets\\Craftables");
			num = step + 1;
			step = num;
			yield return num;
			Game1.rainTexture = Game1.content.Load<Texture2D>("TileSheets\\rain");
			num = step + 1;
			step = num;
			yield return num;
			Game1.buffsIcons = Game1.content.Load<Texture2D>("TileSheets\\BuffsIcons");
			num = step + 1;
			step = num;
			yield return num;
			Tool.weaponsTexture = Game1.content.Load<Texture2D>("TileSheets\\weapons");
			num = step + 1;
			step = num;
			yield return num;
			FarmerRenderer.hairStylesTexture = Game1.content.Load<Texture2D>("Characters\\Farmer\\hairstyles");
			num = step + 1;
			step = num;
			yield return num;
			FarmerRenderer.shirtsTexture = Game1.content.Load<Texture2D>("Characters\\Farmer\\shirts");
			num = step + 1;
			step = num;
			yield return num;
			FarmerRenderer.pantsTexture = Game1.content.Load<Texture2D>("Characters\\Farmer\\pants");
			num = step + 1;
			step = num;
			yield return num;
			FarmerRenderer.hatsTexture = Game1.content.Load<Texture2D>("Characters\\Farmer\\hats");
			num = step + 1;
			step = num;
			yield return num;
			FarmerRenderer.accessoriesTexture = Game1.content.Load<Texture2D>("Characters\\Farmer\\accessories");
			num = step + 1;
			step = num;
			yield return num;
			MapSeat.mapChairTexture = Game1.content.Load<Texture2D>("TileSheets\\ChairTiles");
			num = step + 1;
			step = num;
			yield return num;
			SpriteText.spriteTexture = Game1.content.Load<Texture2D>("LooseSprites\\font_bold");
			num = step + 1;
			step = num;
			yield return num;
			SpriteText.coloredTexture = Game1.content.Load<Texture2D>("LooseSprites\\font_colored");
			num = step + 1;
			step = num;
			yield return num;
			Projectile.projectileSheet = Game1.content.Load<Texture2D>("TileSheets\\Projectiles");
			num = step + 1;
			step = num;
			yield return num;
			Color[] white = new Color[]
			{
				Color.White
			};
			for (int i = 0; i < Game1.dynamicPixelRects.Length; i++)
			{
				Texture2D[] array = Game1.dynamicPixelRects;
				int num2 = i;
				Texture2D texture2D = new Texture2D(base.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(4, 3);
				defaultInterpolatedStringHandler.AppendLiteral("@");
				defaultInterpolatedStringHandler.AppendFormatted("Game1");
				defaultInterpolatedStringHandler.AppendLiteral(".");
				defaultInterpolatedStringHandler.AppendFormatted("dynamicPixelRects");
				defaultInterpolatedStringHandler.AppendLiteral("[");
				defaultInterpolatedStringHandler.AppendFormatted<int>(i);
				defaultInterpolatedStringHandler.AppendLiteral("]");
				texture2D.Name = defaultInterpolatedStringHandler.ToStringAndClear();
				array[num2] = texture2D;
				Game1.dynamicPixelRects[i].SetData<Color>(white);
			}
			Game1.fadeToBlackRect = Game1.dynamicPixelRects[0];
			Game1.staminaRect = Game1.dynamicPixelRects[1];
			Game1.lightingRect = Game1.dynamicPixelRects[2];
			num = step + 1;
			step = num;
			yield return num;
			Game1.onScreenMenus.Clear();
			Game1.onScreenMenus.Add(Game1.dayTimeMoneyBox = new DayTimeMoneyBox());
			Game1.onScreenMenus.Add(new Toolbar());
			Game1.onScreenMenus.Add(Game1.buffsDisplay = new BuffsDisplay());
			num = step + 1;
			step = num;
			yield return num;
			for (int j = 0; j < 70; j++)
			{
				Game1.rainDrops[j] = new RainDrop(Game1.random.Next(Game1.viewport.Width), Game1.random.Next(Game1.viewport.Height), Game1.random.Next(4), Game1.random.Next(70));
			}
			num = step + 1;
			step = num;
			yield return num;
			Game1.dialogueWidth = Math.Min(1024, Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Width - 256);
			Game1.dialogueFont = Game1.content.Load<SpriteFont>("Fonts\\SpriteFont1");
			Game1.dialogueFont.LineSpacing = 42;
			num = step + 1;
			step = num;
			yield return num;
			Game1.smallFont = Game1.content.Load<SpriteFont>("Fonts\\SmallFont");
			Game1.smallFont.LineSpacing = 28;
			num = step + 1;
			step = num;
			yield return num;
			Game1.tinyFont = Game1.content.Load<SpriteFont>("Fonts\\tinyFont");
			num = step + 1;
			step = num;
			yield return num;
			Game1._shortDayDisplayName[0] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3042");
			num = step + 1;
			step = num;
			yield return num;
			Game1._shortDayDisplayName[1] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3043");
			num = step + 1;
			step = num;
			yield return num;
			Game1._shortDayDisplayName[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3044");
			num = step + 1;
			step = num;
			yield return num;
			Game1._shortDayDisplayName[3] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3045");
			num = step + 1;
			step = num;
			yield return num;
			Game1._shortDayDisplayName[4] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3046");
			num = step + 1;
			step = num;
			yield return num;
			Game1._shortDayDisplayName[5] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3047");
			num = step + 1;
			step = num;
			yield return num;
			Game1._shortDayDisplayName[6] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3048");
			num = step + 1;
			step = num;
			yield return num;
			yield break;
		}

		// Token: 0x06000AE2 RID: 2786 RVA: 0x00071C20 File Offset: 0x0006FE20
		public static void resetPlayer()
		{
			List<Item> farmersInitialTools = Farmer.initialTools();
			Game1.player = new Farmer(new FarmerSprite(null), new Vector2(192f, 192f), 1, "", farmersInitialTools, true);
		}

		// Token: 0x06000AE3 RID: 2787 RVA: 0x00071C5C File Offset: 0x0006FE5C
		public static void resetVariables()
		{
			Game1.xLocationAfterWarp = 0;
			Game1.yLocationAfterWarp = 0;
			Game1.gameTimeInterval = 0;
			Game1.currentQuestionChoice = 0;
			Game1.currentDialogueCharacterIndex = 0;
			Game1.dialogueTypingInterval = 0;
			Game1.dayOfMonth = 0;
			Game1.year = 1;
			Game1.timeOfDay = 600;
			Game1.timeOfDayAfterFade = -1;
			Game1.facingDirectionAfterWarp = 0;
			Game1.dialogueWidth = 0;
			Game1.facingDirectionAfterWarp = 0;
			Game1.mouseClickPolling = 0;
			Game1.weatherIcon = 0;
			Game1.hitShakeTimer = 0;
			Game1.staminaShakeTimer = 0;
			Game1.pauseThenDoFunctionTimer = 0;
			Game1.weatherForTomorrow = "Sun";
		}

		// Token: 0x06000AE4 RID: 2788 RVA: 0x00071CE4 File Offset: 0x0006FEE4
		public static bool playSound(string cueName, int? pitch = null)
		{
			ICue cue;
			return Game1.sounds.PlayLocal(cueName, null, null, pitch, SoundContext.Default, out cue);
		}

		// Token: 0x06000AE5 RID: 2789 RVA: 0x00071D0C File Offset: 0x0006FF0C
		public static bool playSound(string cueName, out ICue cue)
		{
			return Game1.sounds.PlayLocal(cueName, null, null, null, SoundContext.Default, out cue);
		}

		// Token: 0x06000AE6 RID: 2790 RVA: 0x00071D3C File Offset: 0x0006FF3C
		public static bool playSound(string cueName, int pitch, out ICue cue)
		{
			return Game1.sounds.PlayLocal(cueName, null, null, new int?(pitch), SoundContext.Default, out cue);
		}

		// Token: 0x06000AE7 RID: 2791 RVA: 0x00071D68 File Offset: 0x0006FF68
		public static void setRichPresence(string friendlyName, object argument = null)
		{
			if (friendlyName != null)
			{
				switch (friendlyName.Length)
				{
				case 5:
					if (!(friendlyName == "menus"))
					{
						return;
					}
					Game1.debugPresenceString = "In menus";
					return;
				case 6:
					break;
				case 7:
				{
					char c = friendlyName[0];
					if (c != 'f')
					{
						if (c != 'w')
						{
							return;
						}
						if (!(friendlyName == "wedding"))
						{
							return;
						}
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Getting married to ");
						defaultInterpolatedStringHandler.AppendFormatted<object>(argument);
						Game1.debugPresenceString = defaultInterpolatedStringHandler.ToStringAndClear();
						return;
					}
					else
					{
						if (!(friendlyName == "fishing"))
						{
							return;
						}
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Fishing at ");
						defaultInterpolatedStringHandler.AppendFormatted<object>(argument);
						Game1.debugPresenceString = defaultInterpolatedStringHandler.ToStringAndClear();
						return;
					}
					break;
				}
				case 8:
				{
					char c = friendlyName[0];
					if (c <= 'f')
					{
						if (c != 'e')
						{
							if (c != 'f')
							{
								return;
							}
							if (!(friendlyName == "festival"))
							{
								return;
							}
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 1);
							defaultInterpolatedStringHandler.AppendLiteral("At ");
							defaultInterpolatedStringHandler.AppendFormatted<object>(argument);
							Game1.debugPresenceString = defaultInterpolatedStringHandler.ToStringAndClear();
							return;
						}
						else
						{
							if (!(friendlyName == "earnings"))
							{
								return;
							}
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
							defaultInterpolatedStringHandler.AppendLiteral("Made ");
							defaultInterpolatedStringHandler.AppendFormatted<object>(argument);
							defaultInterpolatedStringHandler.AppendLiteral("g last night");
							Game1.debugPresenceString = defaultInterpolatedStringHandler.ToStringAndClear();
							return;
						}
					}
					else if (c != 'l')
					{
						if (c != 'm')
						{
							return;
						}
						if (!(friendlyName == "minigame"))
						{
							return;
						}
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(8, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Playing ");
						defaultInterpolatedStringHandler.AppendFormatted<object>(argument);
						Game1.debugPresenceString = defaultInterpolatedStringHandler.ToStringAndClear();
						return;
					}
					else
					{
						if (!(friendlyName == "location"))
						{
							return;
						}
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 1);
						defaultInterpolatedStringHandler.AppendLiteral("At ");
						defaultInterpolatedStringHandler.AppendFormatted<object>(argument);
						Game1.debugPresenceString = defaultInterpolatedStringHandler.ToStringAndClear();
						return;
					}
					break;
				}
				case 9:
				{
					if (!(friendlyName == "giantcrop"))
					{
						return;
					}
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Just harvested a Giant ");
					defaultInterpolatedStringHandler.AppendFormatted<object>(argument);
					Game1.debugPresenceString = defaultInterpolatedStringHandler.ToStringAndClear();
					break;
				}
				default:
					return;
				}
			}
		}

		// Token: 0x06000AE8 RID: 2792 RVA: 0x00071F98 File Offset: 0x00070198
		public static void GenerateBundles(Game1.BundleType bundle_type, bool use_seed = true)
		{
			if (bundle_type == Game1.BundleType.Remixed)
			{
				Random r = use_seed ? Utility.CreateRandom(Game1.uniqueIDForThisGame * 9.0, 0.0, 0.0, 0.0, 0.0) : new Random();
				Dictionary<string, string> bundle_data = new BundleGenerator().Generate(DataLoader.RandomBundles(Game1.content), r);
				Game1.netWorldState.Value.SetBundleData(bundle_data);
				return;
			}
			Game1.netWorldState.Value.SetBundleData(DataLoader.Bundles(Game1.content));
		}

		// Token: 0x06000AE9 RID: 2793 RVA: 0x0007202E File Offset: 0x0007022E
		public void SetNewGameOption<T>(string key, T val)
		{
			this.newGameSetupOptions[key] = val;
		}

		// Token: 0x06000AEA RID: 2794 RVA: 0x00072044 File Offset: 0x00070244
		public T GetNewGameOption<T>(string key)
		{
			object value;
			if (!this.newGameSetupOptions.TryGetValue(key, out value))
			{
				return default(T);
			}
			return (T)((object)value);
		}

		// Token: 0x06000AEB RID: 2795 RVA: 0x00072074 File Offset: 0x00070274
		public virtual void loadForNewGame(bool loadedGame = false)
		{
			if (Game1.startingGameSeed != null)
			{
				Game1.uniqueIDForThisGame = Game1.startingGameSeed.Value;
			}
			Game1.specialCurrencyDisplay = new SpecialCurrencyDisplay();
			Game1.flushLocationLookup();
			Game1.locations.Clear();
			Game1.mailbox.Clear();
			Game1.currentLightSources.Clear();
			Game1.questionChoices.Clear();
			Game1.hudMessages.Clear();
			Game1.weddingToday = false;
			Game1.timeOfDay = 600;
			Game1.season = Season.Spring;
			if (!loadedGame)
			{
				Game1.year = 1;
			}
			Game1.dayOfMonth = 0;
			Game1.isQuestion = false;
			Game1.nonWarpFade = false;
			Game1.newDay = false;
			Game1.eventUp = false;
			Game1.viewportFreeze = false;
			Game1.eventOver = false;
			Game1.screenGlow = false;
			Game1.screenGlowHold = false;
			Game1.screenGlowUp = false;
			Game1.isRaining = false;
			Game1.wasGreenRain = false;
			Game1.killScreen = false;
			Game1.messagePause = false;
			Game1.isDebrisWeather = false;
			Game1.weddingToday = false;
			Game1.exitToTitle = false;
			Game1.dialogueUp = false;
			Game1.postExitToTitleCallback = null;
			Game1.displayHUD = true;
			Game1.messageAfterPause = "";
			Game1.samBandName = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2156");
			Game1.background = null;
			Game1.currentCursorTile = Vector2.Zero;
			if (!loadedGame)
			{
				Game1.lastAppliedSaveFix = SaveMigrator.LatestSaveFix;
			}
			Game1.resetVariables();
			Game1.player.team.sharedDailyLuck.Value = 0.001;
			if (!loadedGame)
			{
				Game1.options = new Options();
				Game1.options.LoadDefaultOptions();
				Game1.initializeVolumeLevels();
			}
			Game1.game1.CheckGamepadMode();
			Game1.onScreenMenus.Add(Game1.chatBox = new ChatBox());
			Game1.outdoorLight = Color.White;
			Game1.ambientLight = Color.White;
			Game1.UpdateDishOfTheDay();
			Game1.locations.Clear();
			Farm farm = new Farm("Maps\\" + Farm.getMapNameFromTypeInt(Game1.whichFarm), "Farm");
			Game1.locations.Add(farm);
			Game1.AddLocations();
			foreach (GameLocation gameLocation in Game1.locations)
			{
				gameLocation.AddDefaultBuildings(true);
			}
			Game1.forceSnapOnNextViewportUpdate = true;
			farm.onNewGame();
			if (!loadedGame)
			{
				foreach (GameLocation gameLocation2 in Game1.locations)
				{
					IslandLocation islandLocation = gameLocation2 as IslandLocation;
					if (islandLocation != null)
					{
						islandLocation.AddAdditionalWalnutBushes();
					}
				}
			}
			if (!loadedGame)
			{
				Game1.hooks.CreatedInitialLocations();
			}
			else
			{
				Game1.hooks.SaveAddedLocations();
			}
			if (!loadedGame)
			{
				Game1.AddNPCs();
			}
			WarpPathfindingCache.PopulateCache();
			if (!loadedGame)
			{
				Game1.GenerateBundles(Game1.bundleType, true);
				foreach (string text in Game1.netWorldState.Value.BundleData.Values)
				{
					string[] item_split = ArgUtility.SplitBySpace(text.Split('/', StringSplitOptions.None)[2]);
					if (Game1.game1.GetNewGameOption<bool>("YearOneCompletable"))
					{
						for (int i = 0; i < item_split.Length; i += 3)
						{
							if (item_split[i] == "266")
							{
								int visits = (16 - 2) * 2;
								visits += 3;
								Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame * 12.0, 0.0, 0.0, 0.0, 0.0);
								Game1.netWorldState.Value.VisitsUntilY1Guarantee = r.Next(2, visits);
							}
						}
					}
				}
				Game1.netWorldState.Value.ShuffleMineChests = Game1.game1.GetNewGameOption<Game1.MineChestType>("MineChests");
				if (Game1.game1.newGameSetupOptions.ContainsKey("SpawnMonstersAtNight"))
				{
					Game1.spawnMonstersAtNight = Game1.game1.GetNewGameOption<bool>("SpawnMonstersAtNight");
				}
			}
			Game1.player.ConvertClothingOverrideToClothesItems();
			Game1.player.addQuest("9");
			Game1.RefreshQuestOfTheDay();
			Game1.player.currentLocation = Game1.RequireLocation("FarmHouse", false);
			Game1.player.gameVersion = Game1.version;
			Game1.hudMessages.Clear();
			Game1.hasLoadedGame = true;
			Game1.setGraphicsForSeason(true);
			if (!loadedGame)
			{
				Game1._setSaveName = false;
			}
			Game1.game1.newGameSetupOptions.Clear();
			Game1.updateCellarAssignments();
			if (!loadedGame && Game1.netWorldState != null && Game1.netWorldState.Value != null)
			{
				Game1.netWorldState.Value.RegisterSpecialCurrencies();
			}
		}

		// Token: 0x06000AEC RID: 2796 RVA: 0x00072508 File Offset: 0x00070708
		public bool IsLocalCoopJoinable()
		{
			return GameRunner.instance.gameInstances.Count < GameRunner.instance.GetMaxSimultaneousPlayers() && !Game1.IsClient;
		}

		// Token: 0x06000AED RID: 2797 RVA: 0x00072531 File Offset: 0x00070731
		public static void StartLocalMultiplayerIfNecessary()
		{
			if (Game1.multiplayerMode == 0)
			{
				Game1.log.Verbose("Starting multiplayer server for local multiplayer...");
				Game1.multiplayerMode = 2;
				if (Game1.server == null)
				{
					Game1.multiplayer.StartLocalMultiplayerServer();
				}
			}
		}

		// Token: 0x06000AEE RID: 2798 RVA: 0x00072560 File Offset: 0x00070760
		public static void EndLocalMultiplayer()
		{
		}

		// Token: 0x06000AEF RID: 2799 RVA: 0x00072564 File Offset: 0x00070764
		public static void UpdatePassiveFestivalStates()
		{
			Game1.netWorldState.Value.ActivePassiveFestivals.Clear();
			foreach (KeyValuePair<string, PassiveFestivalData> pair in DataLoader.PassiveFestivals(Game1.content))
			{
				string id = pair.Key;
				PassiveFestivalData festival = pair.Value;
				if (Game1.dayOfMonth >= festival.StartDay && Game1.dayOfMonth <= festival.EndDay && Game1.season == festival.Season && GameStateQuery.CheckConditions(festival.Condition, null, null, null, null, null, null))
				{
					Game1.netWorldState.Value.ActivePassiveFestivals.Add(id);
				}
			}
		}

		// Token: 0x06000AF0 RID: 2800 RVA: 0x00072628 File Offset: 0x00070828
		public void Instance_UnloadContent()
		{
			this.UnloadContent();
		}

		// Token: 0x06000AF1 RID: 2801 RVA: 0x00072630 File Offset: 0x00070830
		protected override void UnloadContent()
		{
			base.UnloadContent();
			Game1.spriteBatch.Dispose();
			Game1.content.Unload();
			this.xTileContent.Unload();
			IGameServer gameServer = Game1.server;
			if (gameServer == null)
			{
				return;
			}
			gameServer.stopServer();
		}

		// Token: 0x06000AF2 RID: 2802 RVA: 0x00072668 File Offset: 0x00070868
		public static void showRedMessage(string message, bool playSound = true)
		{
			Game1.addHUDMessage(new HUDMessage(message, 3));
			if (!message.Contains("Inventory") && playSound)
			{
				Game1.playSound("cancel", null);
				return;
			}
			if (Game1.player.mailReceived.Add("BackpackTip"))
			{
				Game1.addMailForTomorrow("pierreBackpack", false, false);
			}
		}

		// Token: 0x06000AF3 RID: 2803 RVA: 0x000726CA File Offset: 0x000708CA
		public static void showRedMessageUsingLoadString(string loadString, bool playSound = true)
		{
			Game1.showRedMessage(Game1.content.LoadString(loadString), playSound);
		}

		// Token: 0x06000AF4 RID: 2804 RVA: 0x000726E0 File Offset: 0x000708E0
		public static bool didPlayerJustLeftClick(bool ignoreNonMouseHeldInput = false)
		{
			return (Game1.input.GetMouseState().LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton != ButtonState.Pressed) || (Game1.input.GetGamePadState().Buttons.X == ButtonState.Pressed && (!ignoreNonMouseHeldInput || !Game1.oldPadState.IsButtonDown(Buttons.X))) || (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.useToolButton) && (!ignoreNonMouseHeldInput || Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.useToolButton)));
		}

		// Token: 0x06000AF5 RID: 2805 RVA: 0x0007277C File Offset: 0x0007097C
		public static bool didPlayerJustRightClick(bool ignoreNonMouseHeldInput = false)
		{
			return (Game1.input.GetMouseState().RightButton == ButtonState.Pressed && Game1.oldMouseState.RightButton != ButtonState.Pressed) || (Game1.input.GetGamePadState().Buttons.A == ButtonState.Pressed && (!ignoreNonMouseHeldInput || !Game1.oldPadState.IsButtonDown(Buttons.A))) || (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.actionButton) && (!ignoreNonMouseHeldInput || !Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.actionButton)));
		}

		// Token: 0x06000AF6 RID: 2806 RVA: 0x00072817 File Offset: 0x00070A17
		public static bool didPlayerJustClickAtAll(bool ignoreNonMouseHeldInput = false)
		{
			return Game1.didPlayerJustLeftClick(ignoreNonMouseHeldInput) || Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput);
		}

		// Token: 0x06000AF7 RID: 2807 RVA: 0x00072829 File Offset: 0x00070A29
		public static void showGlobalMessage(string message)
		{
			Game1.addHUDMessage(HUDMessage.ForCornerTextbox(message));
		}

		// Token: 0x06000AF8 RID: 2808 RVA: 0x00072836 File Offset: 0x00070A36
		public static void globalFadeToBlack(Game1.afterFadeFunction afterFade = null, float fadeSpeed = 0.02f)
		{
			Game1.screenFade.GlobalFadeToBlack(afterFade, fadeSpeed);
		}

		// Token: 0x06000AF9 RID: 2809 RVA: 0x00072844 File Offset: 0x00070A44
		public static void globalFadeToClear(Game1.afterFadeFunction afterFade = null, float fadeSpeed = 0.02f)
		{
			Game1.screenFade.GlobalFadeToClear(afterFade, fadeSpeed);
		}

		// Token: 0x06000AFA RID: 2810 RVA: 0x00072854 File Offset: 0x00070A54
		public void CheckGamepadMode()
		{
			bool old_gamepad_active_state = Game1.options.gamepadControls;
			Options.GamepadModes gamepadMode = Game1.options.gamepadMode;
			if (gamepadMode == Options.GamepadModes.ForceOn)
			{
				Game1.options.gamepadControls = true;
				return;
			}
			if (gamepadMode != Options.GamepadModes.ForceOff)
			{
				MouseState mouseState = Game1.input.GetMouseState();
				KeyboardState keyState = Game1.GetKeyboardState();
				GamePadState padState = Game1.input.GetGamePadState();
				bool non_gamepad_control_was_used = false;
				if ((mouseState.LeftButton == ButtonState.Pressed || mouseState.MiddleButton == ButtonState.Pressed || mouseState.RightButton == ButtonState.Pressed || mouseState.ScrollWheelValue != this._oldScrollWheelValue || ((mouseState.X != this._oldMousePosition.X || mouseState.Y != this._oldMousePosition.Y) && Game1.lastCursorMotionWasMouse) || keyState.GetPressedKeys().Length != 0) && (keyState.GetPressedKeys().Length != 1 || keyState.GetPressedKeys()[0] != Keys.Pause))
				{
					non_gamepad_control_was_used = true;
					SteamHelper steamHelper = Program.sdk as SteamHelper;
					if (steamHelper != null && steamHelper.IsRunningOnSteamDeck())
					{
						non_gamepad_control_was_used = false;
					}
				}
				this._oldScrollWheelValue = mouseState.ScrollWheelValue;
				this._oldMousePosition.X = mouseState.X;
				this._oldMousePosition.Y = mouseState.Y;
				bool gamepad_control_was_used = Game1.isAnyGamePadButtonBeingPressed() || Game1.isDPadPressed() || Game1.isGamePadThumbstickInMotion(0.2) || padState.Triggers.Left != 0f || padState.Triggers.Right != 0f;
				if (this._oldGamepadConnectedState != padState.IsConnected)
				{
					this._oldGamepadConnectedState = padState.IsConnected;
					if (this._oldGamepadConnectedState)
					{
						Game1.options.gamepadControls = true;
						Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2574"));
					}
					else
					{
						Game1.options.gamepadControls = false;
						if (this.instancePlayerOneIndex != (PlayerIndex)(-1))
						{
							Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2575"));
							if (Game1.CanShowPauseMenu() && Game1.activeClickableMenu == null)
							{
								Game1.activeClickableMenu = new GameMenu(true);
							}
						}
					}
				}
				if (non_gamepad_control_was_used && Game1.options.gamepadControls)
				{
					Game1.options.gamepadControls = false;
				}
				if (!Game1.options.gamepadControls && gamepad_control_was_used)
				{
					Game1.options.gamepadControls = true;
				}
				if (old_gamepad_active_state != Game1.options.gamepadControls && Game1.options.gamepadControls)
				{
					Game1.lastMousePositionBeforeFade = new Point(this.localMultiplayerWindow.Width / 2, this.localMultiplayerWindow.Height / 2);
					if (Game1.activeClickableMenu != null)
					{
						Game1.activeClickableMenu.setUpForGamePadMode();
						if (Game1.options.SnappyMenus)
						{
							Game1.activeClickableMenu.populateClickableComponentList();
							Game1.activeClickableMenu.snapToDefaultClickableComponent();
						}
					}
					Game1.timerUntilMouseFade = 0;
				}
				return;
			}
			Game1.options.gamepadControls = false;
		}

		// Token: 0x06000AFB RID: 2811 RVA: 0x00072B0E File Offset: 0x00070D0E
		public void Instance_Update(GameTime gameTime)
		{
			this.Update(gameTime);
		}

		// Token: 0x06000AFC RID: 2812 RVA: 0x00072B18 File Offset: 0x00070D18
		protected override void Update(GameTime gameTime)
		{
			GameTime time = gameTime;
			DebugTools.BeforeGameUpdate(this, ref time);
			Game1.input.UpdateStates();
			if (Game1.input.GetGamePadState().IsButtonDown(Buttons.RightStick))
			{
				Game1.rightStickHoldTime += gameTime.ElapsedGameTime.Milliseconds;
			}
			GameMenu.bundleItemHovered = false;
			this._update(time);
			if (Game1.IsMultiplayer && Game1.player != null)
			{
				Game1.player.requestingTimePause.Value = !Game1.shouldTimePass(LocalMultiplayer.IsLocalMultiplayer(true));
				if (Game1.IsMasterGame)
				{
					bool should_time_pause = false;
					if (LocalMultiplayer.IsLocalMultiplayer(true))
					{
						should_time_pause = true;
						using (FarmerCollection.Enumerator enumerator = Game1.getOnlineFarmers().GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								if (!enumerator.Current.requestingTimePause.Value)
								{
									should_time_pause = false;
									break;
								}
							}
						}
					}
					Game1.netWorldState.Value.IsTimePaused = should_time_pause;
				}
			}
			Rumble.update((float)gameTime.ElapsedGameTime.Milliseconds);
			if (Game1.options.gamepadControls && Game1.thumbstickMotionMargin > 0)
			{
				Game1.thumbstickMotionMargin -= gameTime.ElapsedGameTime.Milliseconds;
			}
			if (!Game1.input.GetGamePadState().IsButtonDown(Buttons.RightStick))
			{
				Game1.rightStickHoldTime = 0;
			}
			base.Update(gameTime);
		}

		// Token: 0x06000AFD RID: 2813 RVA: 0x00072C80 File Offset: 0x00070E80
		public void Instance_OnActivated(object sender, EventArgs args)
		{
			this.OnActivated(sender, args);
		}

		// Token: 0x06000AFE RID: 2814 RVA: 0x00072C8C File Offset: 0x00070E8C
		protected override void OnActivated(object sender, EventArgs args)
		{
			base.OnActivated(sender, args);
			Game1._activatedTick = Game1.ticks + 1;
			Game1.input.IgnoreKeys(Game1.GetKeyboardState().GetPressedKeys());
		}

		// Token: 0x06000AFF RID: 2815 RVA: 0x00072CC4 File Offset: 0x00070EC4
		public bool HasKeyboardFocus()
		{
			if (Game1.keyboardFocusInstance == null)
			{
				return base.IsMainInstance;
			}
			return Game1.keyboardFocusInstance == this;
		}

		// Token: 0x06000B00 RID: 2816 RVA: 0x00072CDC File Offset: 0x00070EDC
		private void _update(GameTime gameTime)
		{
			if (Game1.graphics.GraphicsDevice == null)
			{
				return;
			}
			bool zoom_dirty = false;
			Game1.gameModeTicks++;
			if (Game1.options != null && !this.takingMapScreenshot)
			{
				if (Game1.options.baseUIScale != Game1.options.desiredUIScale)
				{
					if (Game1.options.desiredUIScale < 0f)
					{
						Game1.options.desiredUIScale = Game1.options.desiredBaseZoomLevel;
					}
					Game1.options.baseUIScale = Game1.options.desiredUIScale;
					zoom_dirty = true;
				}
				if (Game1.options.desiredBaseZoomLevel != Game1.options.baseZoomLevel)
				{
					Game1.options.baseZoomLevel = Game1.options.desiredBaseZoomLevel;
					Game1.forceSnapOnNextViewportUpdate = true;
					zoom_dirty = true;
				}
			}
			if (zoom_dirty)
			{
				this.refreshWindowSettings();
			}
			if (!this.ShouldLoadIncrementally)
			{
				this.CheckGamepadMode();
			}
			FarmAnimal.NumPathfindingThisTick = 0;
			Game1.options.reApplySetOptions();
			if (Game1.toggleFullScreen)
			{
				Game1.toggleFullscreen();
				Game1.toggleFullScreen = false;
			}
			Game1.input.Update();
			if (Game1.frameByFrame)
			{
				if (Game1.GetKeyboardState().IsKeyDown(Keys.Escape) && Game1.oldKBState.IsKeyUp(Keys.Escape))
				{
					Game1.frameByFrame = false;
				}
				if (!Game1.GetKeyboardState().IsKeyDown(Keys.G) || !Game1.oldKBState.IsKeyUp(Keys.G))
				{
					Game1.oldKBState = Game1.GetKeyboardState();
					return;
				}
			}
			if (Game1.client != null && Game1.client.timedOut)
			{
				Game1.multiplayer.clientRemotelyDisconnected(Game1.client.pendingDisconnect);
			}
			if (Game1._newDayTask != null)
			{
				if (Game1._newDayTask.Status == TaskStatus.Created)
				{
					Game1.hooks.StartTask(Game1._newDayTask, "NewDay");
				}
				if (Game1._newDayTask.Status >= TaskStatus.RanToCompletion)
				{
					if (Game1._newDayTask.IsFaulted)
					{
						Exception e = Game1._newDayTask.Exception.GetBaseException();
						if (!Game1.IsMasterGame)
						{
							if (e is AbortNetSynchronizerException)
							{
								Game1.log.Verbose("_newDayTask failed: client lost connection to the server");
							}
							else
							{
								Game1.log.Error("Client _newDayTask failed with an exception:", e);
							}
							Game1.multiplayer.clientRemotelyDisconnected(Multiplayer.DisconnectType.ClientTimeout);
							Game1._newDayTask = null;
							Utility.CollectGarbage("", 0);
							return;
						}
						Game1.log.Error("_newDayTask failed with an exception:", e);
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Error on new day: \n---------------\n");
						defaultInterpolatedStringHandler.AppendFormatted<Exception>(e);
						defaultInterpolatedStringHandler.AppendLiteral("\n---------------\n");
						throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					else
					{
						Game1._newDayTask = null;
						Utility.CollectGarbage("", 0);
					}
				}
				Game1.UpdateChatBox();
				return;
			}
			if (this.isLocalMultiplayerNewDayActive)
			{
				Game1.UpdateChatBox();
				return;
			}
			if (this.IsSaving)
			{
				Game1.PushUIMode();
				IClickableMenu activeClickableMenu = Game1.activeClickableMenu;
				if (activeClickableMenu != null)
				{
					activeClickableMenu.update(gameTime);
				}
				if (Game1.overlayMenu != null)
				{
					Game1.overlayMenu.update(gameTime);
					if (Game1.overlayMenu == null)
					{
						Game1.PopUIMode();
						return;
					}
				}
				Game1.PopUIMode();
				Game1.UpdateChatBox();
				return;
			}
			if (Game1.exitToTitle)
			{
				Game1.exitToTitle = false;
				this.CleanupReturningToTitle();
				Utility.CollectGarbage("", 0);
				Action action = Game1.postExitToTitleCallback;
				if (action != null)
				{
					action();
				}
			}
			Game1.SetFreeCursorElapsed((float)gameTime.ElapsedGameTime.TotalSeconds);
			Program.sdk.Update();
			if (Game1.game1.IsMainInstance)
			{
				Game1.keyboardFocusInstance = Game1.game1;
				foreach (Game1 instance in GameRunner.instance.gameInstances)
				{
					if (instance.instanceKeyboardDispatcher.Subscriber != null && instance.instanceTextEntry != null)
					{
						Game1.keyboardFocusInstance = instance;
						break;
					}
				}
			}
			if (base.IsMainInstance)
			{
				int current_display_index = base.Window.GetDisplayIndex();
				if (this._lastUsedDisplay != -1 && this._lastUsedDisplay != current_display_index)
				{
					StartupPreferences startupPreferences = new StartupPreferences();
					startupPreferences.loadPreferences(false, false);
					startupPreferences.displayIndex = current_display_index;
					startupPreferences.savePreferences(false, false);
				}
				this._lastUsedDisplay = current_display_index;
			}
			if (this.HasKeyboardFocus())
			{
				Game1.keyboardDispatcher.Poll();
			}
			else
			{
				Game1.keyboardDispatcher.Discard();
			}
			if (Game1.gameMode == 6)
			{
				Game1.multiplayer.UpdateLoading();
			}
			if (Game1.gameMode == 3)
			{
				Game1.multiplayer.UpdateEarly();
				Game1.dedicatedServer.Tick();
				Farmer player = Game1.player;
				if (((player != null) ? player.team : null) != null)
				{
					Game1.player.team.Update();
				}
			}
			if ((Game1.paused || (!this.IsActiveNoOverlay && Program.releaseBuild)) && (Game1.options == null || Game1.options.pauseWhenOutOfFocus || Game1.paused) && Game1.multiplayerMode == 0)
			{
				Game1.UpdateChatBox();
				return;
			}
			if (Game1.quit)
			{
				base.Exit();
			}
			Game1.currentGameTime = gameTime;
			if (Game1.gameMode != 11 && !this.ShouldLoadIncrementally)
			{
				Game1.ticks++;
				if (this.IsActiveNoOverlay)
				{
					this.checkForEscapeKeys();
				}
				Game1.updateMusic();
				Game1.updateRaindropPosition();
				if (Game1.globalFade)
				{
					Game1.screenFade.UpdateGlobalFade();
				}
				else if (Game1.pauseThenDoFunctionTimer > 0)
				{
					Game1.freezeControls = true;
					Game1.pauseThenDoFunctionTimer -= gameTime.ElapsedGameTime.Milliseconds;
					if (Game1.pauseThenDoFunctionTimer <= 0)
					{
						Game1.freezeControls = false;
						Game1.afterFadeFunction afterFadeFunction = Game1.afterPause;
						if (afterFadeFunction != null)
						{
							afterFadeFunction();
						}
					}
				}
				bool flag;
				if (Game1.options.gamepadControls)
				{
					IClickableMenu activeClickableMenu2 = Game1.activeClickableMenu;
					flag = (((activeClickableMenu2 != null) ? new bool?(activeClickableMenu2.shouldClampGamePadCursor()) : null) ?? false);
				}
				else
				{
					flag = false;
				}
				if (flag)
				{
					Point pos = Game1.getMousePositionRaw();
					Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(0, 0, this.localMultiplayerWindow.Width, this.localMultiplayerWindow.Height);
					if (pos.X < rect.X)
					{
						pos.X = rect.X;
					}
					else if (pos.X > rect.Right)
					{
						pos.X = rect.Right;
					}
					if (pos.Y < rect.Y)
					{
						pos.Y = rect.Y;
					}
					else if (pos.Y > rect.Bottom)
					{
						pos.Y = rect.Bottom;
					}
					Game1.setMousePositionRaw(pos.X, pos.Y);
				}
				if (Game1.gameMode == 3 || Game1.gameMode == 2)
				{
					if (!Game1.warpingForForcedRemoteEvent && !Game1.eventUp && !Game1.dialogueUp && Game1.remoteEventQueue.Count > 0 && Game1.player != null && Game1.player.isCustomized.Value && (!Game1.fadeIn || Game1.fadeToBlackAlpha <= 0f))
					{
						if (Game1.activeClickableMenu != null)
						{
							Game1.activeClickableMenu.emergencyShutDown();
							Game1.exitActiveMenu();
						}
						else if (Game1.currentMinigame != null && Game1.currentMinigame.forceQuit())
						{
							Game1.currentMinigame = null;
						}
						if (Game1.activeClickableMenu == null && Game1.currentMinigame == null && Game1.player.freezePause <= 0)
						{
							Action action2 = Game1.remoteEventQueue[0];
							Game1.remoteEventQueue.RemoveAt(0);
							action2();
						}
					}
					Game1.player.millisecondsPlayed += (ulong)gameTime.ElapsedGameTime.Milliseconds;
					bool doMainGameUpdates = true;
					if (Game1.currentMinigame != null && !Game1.HostPaused)
					{
						if (Game1.pauseTime > 0f)
						{
							Game1.updatePause(gameTime);
						}
						if (Game1.fadeToBlack)
						{
							Game1.screenFade.UpdateFadeAlpha(gameTime);
							if (Game1.fadeToBlackAlpha >= 1f)
							{
								Game1.fadeToBlack = false;
							}
						}
						else
						{
							if (Game1.thumbstickMotionMargin > 0)
							{
								Game1.thumbstickMotionMargin -= gameTime.ElapsedGameTime.Milliseconds;
							}
							KeyboardState currentKBState = default(KeyboardState);
							MouseState currentMouseState = default(MouseState);
							GamePadState currentPadState = default(GamePadState);
							if (base.IsActive)
							{
								currentKBState = Game1.GetKeyboardState();
								currentMouseState = Game1.input.GetMouseState();
								currentPadState = Game1.input.GetGamePadState();
								ChatBox chatBox = Game1.chatBox;
								if ((((chatBox != null) ? new bool?(chatBox.isActive()) : null) ?? false) || Game1.textEntry != null)
								{
									currentKBState = default(KeyboardState);
									currentPadState = default(GamePadState);
								}
								else
								{
									foreach (Keys i in currentKBState.GetPressedKeys())
									{
										if (!Game1.oldKBState.IsKeyDown(i) && Game1.currentMinigame != null)
										{
											Game1.currentMinigame.receiveKeyPress(i);
										}
									}
									if (Game1.options.gamepadControls)
									{
										if (Game1.currentMinigame == null)
										{
											Game1.oldMouseState = currentMouseState;
											Game1.oldKBState = currentKBState;
											Game1.oldPadState = currentPadState;
											Game1.UpdateChatBox();
											return;
										}
										foreach (Buttons b in Utility.getPressedButtons(currentPadState, Game1.oldPadState))
										{
											IMinigame currentMinigame = Game1.currentMinigame;
											if (currentMinigame != null)
											{
												currentMinigame.receiveKeyPress(Utility.mapGamePadButtonToKey(b));
											}
										}
										if (Game1.currentMinigame == null)
										{
											Game1.oldMouseState = currentMouseState;
											Game1.oldKBState = currentKBState;
											Game1.oldPadState = currentPadState;
											Game1.UpdateChatBox();
											return;
										}
										if (currentPadState.ThumbSticks.Right.Y < -0.2f && Game1.oldPadState.ThumbSticks.Right.Y >= -0.2f)
										{
											Game1.currentMinigame.receiveKeyPress(Keys.Down);
										}
										if (currentPadState.ThumbSticks.Right.Y > 0.2f && Game1.oldPadState.ThumbSticks.Right.Y <= 0.2f)
										{
											Game1.currentMinigame.receiveKeyPress(Keys.Up);
										}
										if (currentPadState.ThumbSticks.Right.X < -0.2f && Game1.oldPadState.ThumbSticks.Right.X >= -0.2f)
										{
											Game1.currentMinigame.receiveKeyPress(Keys.Left);
										}
										if (currentPadState.ThumbSticks.Right.X > 0.2f && Game1.oldPadState.ThumbSticks.Right.X <= 0.2f)
										{
											Game1.currentMinigame.receiveKeyPress(Keys.Right);
										}
										if (Game1.oldPadState.ThumbSticks.Right.Y < -0.2f && currentPadState.ThumbSticks.Right.Y >= -0.2f)
										{
											Game1.currentMinigame.receiveKeyRelease(Keys.Down);
										}
										if (Game1.oldPadState.ThumbSticks.Right.Y > 0.2f && currentPadState.ThumbSticks.Right.Y <= 0.2f)
										{
											Game1.currentMinigame.receiveKeyRelease(Keys.Up);
										}
										if (Game1.oldPadState.ThumbSticks.Right.X < -0.2f && currentPadState.ThumbSticks.Right.X >= -0.2f)
										{
											Game1.currentMinigame.receiveKeyRelease(Keys.Left);
										}
										if (Game1.oldPadState.ThumbSticks.Right.X > 0.2f && currentPadState.ThumbSticks.Right.X <= 0.2f)
										{
											Game1.currentMinigame.receiveKeyRelease(Keys.Right);
										}
										if (Game1.isGamePadThumbstickInMotion(0.2) && Game1.currentMinigame != null && !Game1.currentMinigame.overrideFreeMouseMovement())
										{
											Game1.setMousePosition(Game1.getMouseX() + (int)(currentPadState.ThumbSticks.Left.X * Game1.thumbstickToMouseModifier), Game1.getMouseY() - (int)(currentPadState.ThumbSticks.Left.Y * Game1.thumbstickToMouseModifier));
										}
										else if (Game1.getMouseX() != Game1.getOldMouseX() || Game1.getMouseY() != Game1.getOldMouseY())
										{
											Game1.lastCursorMotionWasMouse = true;
										}
									}
									foreach (Keys j in Game1.oldKBState.GetPressedKeys())
									{
										if (!currentKBState.IsKeyDown(j) && Game1.currentMinigame != null)
										{
											Game1.currentMinigame.receiveKeyRelease(j);
										}
									}
									if (Game1.options.gamepadControls)
									{
										if (Game1.currentMinigame == null)
										{
											Game1.oldMouseState = currentMouseState;
											Game1.oldKBState = currentKBState;
											Game1.oldPadState = currentPadState;
											Game1.UpdateChatBox();
											return;
										}
										if (currentPadState.IsConnected)
										{
											if (currentPadState.IsButtonDown(Buttons.X) && !Game1.oldPadState.IsButtonDown(Buttons.X))
											{
												Game1.currentMinigame.receiveRightClick(Game1.getMouseX(), Game1.getMouseY(), true);
											}
											else if (currentPadState.IsButtonDown(Buttons.A) && !Game1.oldPadState.IsButtonDown(Buttons.A))
											{
												Game1.currentMinigame.receiveLeftClick(Game1.getMouseX(), Game1.getMouseY(), true);
											}
											else if (!currentPadState.IsButtonDown(Buttons.X) && Game1.oldPadState.IsButtonDown(Buttons.X))
											{
												Game1.currentMinigame.releaseRightClick(Game1.getMouseX(), Game1.getMouseY());
											}
											else if (!currentPadState.IsButtonDown(Buttons.A) && Game1.oldPadState.IsButtonDown(Buttons.A))
											{
												Game1.currentMinigame.releaseLeftClick(Game1.getMouseX(), Game1.getMouseY());
											}
										}
										foreach (Buttons b2 in Utility.getPressedButtons(Game1.oldPadState, currentPadState))
										{
											IMinigame currentMinigame2 = Game1.currentMinigame;
											if (currentMinigame2 != null)
											{
												currentMinigame2.receiveKeyRelease(Utility.mapGamePadButtonToKey(b2));
											}
										}
										if (currentPadState.IsConnected && currentPadState.IsButtonDown(Buttons.A) && Game1.currentMinigame != null)
										{
											Game1.currentMinigame.leftClickHeld(0, 0);
										}
									}
									if (Game1.currentMinigame == null)
									{
										Game1.oldMouseState = currentMouseState;
										Game1.oldKBState = currentKBState;
										Game1.oldPadState = currentPadState;
										Game1.UpdateChatBox();
										return;
									}
									if (Game1.currentMinigame != null && currentMouseState.LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton != ButtonState.Pressed)
									{
										Game1.currentMinigame.receiveLeftClick(Game1.getMouseX(), Game1.getMouseY(), true);
									}
									if (Game1.currentMinigame != null && currentMouseState.RightButton == ButtonState.Pressed && Game1.oldMouseState.RightButton != ButtonState.Pressed)
									{
										Game1.currentMinigame.receiveRightClick(Game1.getMouseX(), Game1.getMouseY(), true);
									}
									if (Game1.currentMinigame != null && currentMouseState.LeftButton == ButtonState.Released && Game1.oldMouseState.LeftButton == ButtonState.Pressed)
									{
										Game1.currentMinigame.releaseLeftClick(Game1.getMouseX(), Game1.getMouseY());
									}
									if (Game1.currentMinigame != null && currentMouseState.RightButton == ButtonState.Released && Game1.oldMouseState.RightButton == ButtonState.Pressed)
									{
										Game1.currentMinigame.releaseLeftClick(Game1.getMouseX(), Game1.getMouseY());
									}
									if (Game1.currentMinigame != null && currentMouseState.LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton == ButtonState.Pressed)
									{
										Game1.currentMinigame.leftClickHeld(Game1.getMouseX(), Game1.getMouseY());
									}
								}
							}
							if (Game1.currentMinigame != null && Game1.currentMinigame.tick(gameTime))
							{
								Game1.oldMouseState = currentMouseState;
								Game1.oldKBState = currentKBState;
								Game1.oldPadState = currentPadState;
								IMinigame currentMinigame3 = Game1.currentMinigame;
								if (currentMinigame3 != null)
								{
									currentMinigame3.unload();
								}
								Game1.currentMinigame = null;
								Game1.fadeIn = true;
								Game1.fadeToBlackAlpha = 1f;
								Game1.UpdateChatBox();
								return;
							}
							if (Game1.currentMinigame == null && Game1.IsMusicContextActive(MusicContext.MiniGame))
							{
								Game1.stopMusicTrack(MusicContext.MiniGame);
							}
							Game1.oldMouseState = currentMouseState;
							Game1.oldKBState = currentKBState;
							Game1.oldPadState = currentPadState;
						}
						doMainGameUpdates = (Game1.IsMultiplayer || Game1.currentMinigame == null || Game1.currentMinigame.doMainGameUpdates());
					}
					else if (Game1.farmEvent != null && !Game1.HostPaused && Game1.farmEvent.tickUpdate(gameTime))
					{
						Game1.farmEvent.makeChangesToLocation();
						Game1.timeOfDay = 600;
						Game1.outdoorLight = Color.White;
						Game1.displayHUD = true;
						Game1.farmEvent = null;
						Game1.netWorldState.Value.WriteToGame1(false);
						Game1.currentLocation = Game1.player.currentLocation;
						LocationRequest locationRequest = Game1.getLocationRequest(Game1.currentLocation.Name, false);
						locationRequest.OnWarp += delegate()
						{
							FarmHouse farmHouse = Game1.currentLocation as FarmHouse;
							if (farmHouse != null)
							{
								Game1.player.Position = Utility.PointToVector2(farmHouse.GetPlayerBedSpot()) * 64f;
								BedFurniture.ShiftPositionForBed(Game1.player);
							}
							else
							{
								BedFurniture.ApplyWakeUpPosition(Game1.player);
							}
							if (Game1.player.IsSitting())
							{
								Game1.player.StopSitting(false);
							}
							Game1.changeMusicTrack("none", true, MusicContext.Default);
							Game1.player.forceCanMove();
							Game1.freezeControls = false;
							Game1.displayFarmer = true;
							Game1.viewportFreeze = false;
							Game1.fadeToBlackAlpha = 0f;
							Game1.fadeToBlack = false;
							Game1.globalFadeToClear(null, 0.02f);
							Game1.RemoveDeliveredMailForTomorrow();
							Game1.handlePostFarmEventActions();
							Game1.showEndOfNightStuff();
						};
						Game1.warpFarmer(locationRequest, 5, 9, Game1.player.FacingDirection);
						Game1.fadeToBlackAlpha = 1.1f;
						Game1.fadeToBlack = true;
						Game1.nonWarpFade = false;
						Game1.UpdateOther(gameTime);
					}
					if (doMainGameUpdates)
					{
						if (Game1.endOfNightMenus.Count > 0 && Game1.activeClickableMenu == null)
						{
							Game1.activeClickableMenu = Game1.endOfNightMenus.Pop();
							if (Game1.activeClickableMenu != null && Game1.options.SnappyMenus)
							{
								Game1.activeClickableMenu.snapToDefaultClickableComponent();
							}
						}
						SpecialCurrencyDisplay specialCurrencyDisplay = Game1.specialCurrencyDisplay;
						if (specialCurrencyDisplay != null)
						{
							specialCurrencyDisplay.Update(gameTime);
						}
						if (Game1.currentLocation != null && Game1.currentMinigame == null)
						{
							if (Game1.emoteMenu != null)
							{
								Game1.emoteMenu.update(gameTime);
								if (Game1.emoteMenu != null)
								{
									Game1.PushUIMode();
									Game1.emoteMenu.performHoverAction(Game1.getMouseX(), Game1.getMouseY());
									KeyboardState currentState = Game1.GetKeyboardState();
									if (Game1.input.GetMouseState().LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton == ButtonState.Released)
									{
										Game1.emoteMenu.receiveLeftClick(Game1.getMouseX(), Game1.getMouseY(), true);
									}
									else if (Game1.input.GetMouseState().RightButton == ButtonState.Pressed && Game1.oldMouseState.RightButton == ButtonState.Released)
									{
										Game1.emoteMenu.receiveRightClick(Game1.getMouseX(), Game1.getMouseY(), true);
									}
									else if (Game1.isOneOfTheseKeysDown(currentState, Game1.options.menuButton) || (Game1.isOneOfTheseKeysDown(currentState, Game1.options.emoteButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.emoteButton)))
									{
										Game1.emoteMenu.exitThisMenu(false);
									}
									Game1.PopUIMode();
									Game1.oldKBState = currentState;
									Game1.oldMouseState = Game1.input.GetMouseState();
								}
							}
							else if (Game1.textEntry != null)
							{
								Game1.PushUIMode();
								Game1.updateTextEntry(gameTime);
								Game1.PopUIMode();
							}
							else if (Game1.activeClickableMenu != null)
							{
								Game1.PushUIMode();
								Game1.updateActiveMenu(gameTime);
								Game1.PopUIMode();
							}
							else
							{
								if (Game1.pauseTime > 0f)
								{
									Game1.updatePause(gameTime);
								}
								if (!Game1.globalFade && !Game1.freezeControls && Game1.activeClickableMenu == null && (this.IsActiveNoOverlay || Game1.inputSimulator != null))
								{
									this.UpdateControlInput(gameTime);
								}
							}
						}
						if (Game1.showingEndOfNightStuff && Game1.endOfNightMenus.Count == 0 && Game1.activeClickableMenu == null)
						{
							Game1.newDaySync.destroy();
							Game1.player.team.endOfNightStatus.WithdrawState();
							Game1.showingEndOfNightStuff = false;
							Action afterAction = Game1._afterNewDayAction;
							if (afterAction != null)
							{
								Game1._afterNewDayAction = null;
								afterAction();
							}
							Game1.player.ReequipEnchantments();
							Game1.globalFadeToClear(new Game1.afterFadeFunction(Game1.doMorningStuff), 0.02f);
						}
						if (Game1.currentLocation != null)
						{
							if (!Game1.HostPaused && !Game1.showingEndOfNightStuff)
							{
								if (Game1.IsMultiplayer || (Game1.activeClickableMenu == null && Game1.currentMinigame == null) || Game1.player.viewingLocation.Value != null)
								{
									Game1.UpdateGameClock(gameTime);
								}
								this.UpdateCharacters(gameTime);
								this.UpdateLocations(gameTime);
								if (Game1.currentMinigame == null)
								{
									Game1.UpdateViewPort(false, this.getViewportCenter());
								}
								else
								{
									Game1.previousViewportPosition.X = (float)Game1.viewport.X;
									Game1.previousViewportPosition.Y = (float)Game1.viewport.Y;
								}
								Game1.UpdateOther(gameTime);
							}
							if (Game1.messagePause)
							{
								KeyboardState tmp = Game1.GetKeyboardState();
								MouseState tmp2 = Game1.input.GetMouseState();
								GamePadState tmp3 = Game1.input.GetGamePadState();
								if (Game1.isOneOfTheseKeysDown(tmp, Game1.options.actionButton) && !Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.actionButton))
								{
									Game1.pressActionButton(tmp, tmp2, tmp3);
								}
								Game1.oldKBState = tmp;
								Game1.oldPadState = tmp3;
							}
						}
					}
					else if (Game1.textEntry != null)
					{
						Game1.PushUIMode();
						Game1.updateTextEntry(gameTime);
						Game1.PopUIMode();
					}
				}
				else
				{
					this.UpdateTitleScreen(gameTime);
					if (Game1.textEntry != null)
					{
						Game1.PushUIMode();
						Game1.updateTextEntry(gameTime);
						Game1.PopUIMode();
					}
					else if (Game1.activeClickableMenu != null)
					{
						Game1.PushUIMode();
						Game1.updateActiveMenu(gameTime);
						Game1.PopUIMode();
					}
					if (Game1.gameMode == 10)
					{
						Game1.UpdateOther(gameTime);
					}
				}
				IAudioEngine audioEngine = Game1.audioEngine;
				if (audioEngine != null)
				{
					audioEngine.Update();
				}
				Game1.UpdateChatBox();
				if (Game1.gameMode != 6)
				{
					Game1.multiplayer.UpdateLate(false);
				}
			}
			else if (this.ShouldLoadIncrementally)
			{
				Stopwatch loadTimer = Stopwatch.StartNew();
				while (Game1.LoadContentEnumerator.MoveNext())
				{
					if (loadTimer.Elapsed.TotalMilliseconds >= 25.0)
					{
						IL_1450:
						if (Game1.FinishedFirstLoadContent && Game1.FinishedFirstInitSounds && Game1.FinishedFirstInitSerializers)
						{
							Game1.FinishedIncrementalLoad = true;
							this.AfterLoadContent();
							goto IL_1475;
						}
						goto IL_1475;
					}
				}
				Game1.FinishedFirstLoadContent = true;
				goto IL_1450;
			}
			IL_1475:
			if (Game1.gameMode == 3 && Game1.gameModeTicks == 1)
			{
				Game1.OnDayStarted();
			}
		}

		// Token: 0x06000B01 RID: 2817 RVA: 0x00074184 File Offset: 0x00072384
		public static void OnDayStarted()
		{
			TriggerActionManager.Raise("DayStarted", null, null, null, null, null);
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				location.OnDayStarted();
				return true;
			}, true, false);
			Utility.fixAllAnimals();
			foreach (NPC npc in Utility.getAllCharacters())
			{
				npc.OnDayStarted();
			}
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				foreach (FarmAnimal farmAnimal in location.animals.Values)
				{
					farmAnimal.OnDayStarted();
				}
				return true;
			}, true, false);
			Game1.player.currentLocation.resetForPlayerEntry();
			if (!Game1.hasStartedDay)
			{
				using (IEnumerator<string> enumerator2 = Game1.player.team.constructedBuildings.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						string buildingType = enumerator2.Current;
						Game1.player.NotifyQuests((Quest quest) => quest.OnBuildingExists(buildingType, false), false);
					}
				}
				if (Stats.AllowRetroactiveAchievements)
				{
					foreach (int which in Game1.player.achievements)
					{
						Game1.getPlatformAchievement(which.ToString());
					}
				}
				Game1.hasStartedDay = true;
			}
			if (Game1.IsMasterGame)
			{
				Woods.ResetLostItemsShop();
			}
		}

		// Token: 0x06000B02 RID: 2818 RVA: 0x0007430C File Offset: 0x0007250C
		public static void PerformPassiveFestivalSetup()
		{
			foreach (string festival_id in Game1.netWorldState.Value.ActivePassiveFestivals)
			{
				PassiveFestivalData data;
				if (Utility.TryGetPassiveFestivalData(festival_id, out data) && data.DailySetupMethod != null)
				{
					FestivalDailySetupDelegate method;
					string error;
					if (StaticDelegateBuilder.TryCreateDelegate<FestivalDailySetupDelegate>(data.DailySetupMethod, out method, out error))
					{
						method();
					}
					else
					{
						IGameLogger gameLogger = Game1.log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(55, 3);
						defaultInterpolatedStringHandler.AppendLiteral("Passive festival '");
						defaultInterpolatedStringHandler.AppendFormatted(festival_id);
						defaultInterpolatedStringHandler.AppendLiteral("' has invalid daily setup method '");
						defaultInterpolatedStringHandler.AppendFormatted(data.DailySetupMethod);
						defaultInterpolatedStringHandler.AppendLiteral("': ");
						defaultInterpolatedStringHandler.AppendFormatted(error);
						gameLogger.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
					}
				}
			}
		}

		// Token: 0x17000189 RID: 393
		// (get) Token: 0x06000B03 RID: 2819 RVA: 0x000743EC File Offset: 0x000725EC
		public static int CurrentPlayerLimit
		{
			get
			{
				NetRoot<NetWorldState> netRoot = Game1.netWorldState;
				if (((netRoot != null) ? netRoot.Value : null) != null)
				{
					int currentPlayerLimit = Game1.netWorldState.Value.CurrentPlayerLimit;
					return Game1.netWorldState.Value.CurrentPlayerLimit;
				}
				return Game1.multiplayer.playerLimit;
			}
		}

		// Token: 0x06000B04 RID: 2820 RVA: 0x00074438 File Offset: 0x00072638
		public static void showTextEntry(TextBox text_box)
		{
			Game1.timerUntilMouseFade = 0;
			Game1.PushUIMode();
			Game1.textEntry = new TextEntryMenu(text_box);
			Game1.PopUIMode();
		}

		// Token: 0x06000B05 RID: 2821 RVA: 0x00074458 File Offset: 0x00072658
		public static void closeTextEntry()
		{
			if (Game1.textEntry != null)
			{
				Game1.textEntry = null;
			}
			if (Game1.activeClickableMenu != null && Game1.options.SnappyMenus)
			{
				if (Game1.activeClickableMenu is TitleMenu && TitleMenu.subMenu != null)
				{
					TitleMenu.subMenu.snapCursorToCurrentSnappedComponent();
					return;
				}
				Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
			}
		}

		// Token: 0x06000B06 RID: 2822 RVA: 0x000744AD File Offset: 0x000726AD
		public static bool isDarkOut(GameLocation location)
		{
			return Game1.timeOfDay >= Game1.getTrulyDarkTime(location);
		}

		// Token: 0x06000B07 RID: 2823 RVA: 0x000744BF File Offset: 0x000726BF
		public static bool isTimeToTurnOffLighting(GameLocation location)
		{
			return Game1.timeOfDay >= Game1.getTrulyDarkTime(location) - 100;
		}

		// Token: 0x06000B08 RID: 2824 RVA: 0x000744D4 File Offset: 0x000726D4
		public static bool isStartingToGetDarkOut(GameLocation location)
		{
			return Game1.timeOfDay >= Game1.getStartingToGetDarkTime(location);
		}

		// Token: 0x06000B09 RID: 2825 RVA: 0x000744E8 File Offset: 0x000726E8
		public static int getStartingToGetDarkTime(GameLocation location)
		{
			if (location != null && location.InIslandContext())
			{
				return 1800;
			}
			Season season = Game1.season;
			if (season == Season.Fall)
			{
				return 1700;
			}
			if (season != Season.Winter)
			{
				return 1800;
			}
			return 1500;
		}

		// Token: 0x06000B0A RID: 2826 RVA: 0x00074528 File Offset: 0x00072728
		public static void updateCellarAssignments()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			Game1.player.team.cellarAssignments[1] = Game1.MasterPlayer.UniqueMultiplayerID;
			for (int i = 2; i <= Game1.netWorldState.Value.HighestPlayerLimit; i++)
			{
				string cellar_name = "Cellar" + i.ToString();
				if (i != 1 && Game1.getLocationFromName(cellar_name) != null)
				{
					long assignedFarmerId;
					if (Game1.player.team.cellarAssignments.TryGetValue(i, out assignedFarmerId))
					{
						if (Game1.GetPlayer(assignedFarmerId, false) != null)
						{
							goto IL_FB;
						}
						Game1.player.team.cellarAssignments.Remove(i);
					}
					foreach (Farmer farmer in Game1.getAllFarmers())
					{
						if (!Game1.player.team.cellarAssignments.Values.Contains(farmer.UniqueMultiplayerID))
						{
							Game1.player.team.cellarAssignments[i] = farmer.UniqueMultiplayerID;
							break;
						}
					}
				}
				IL_FB:;
			}
		}

		// Token: 0x06000B0B RID: 2827 RVA: 0x0007465C File Offset: 0x0007285C
		public static int getModeratelyDarkTime(GameLocation location)
		{
			return (Game1.getTrulyDarkTime(location) + Game1.getStartingToGetDarkTime(location)) / 2;
		}

		// Token: 0x06000B0C RID: 2828 RVA: 0x0007466D File Offset: 0x0007286D
		public static int getTrulyDarkTime(GameLocation location)
		{
			return Game1.getStartingToGetDarkTime(location) + 200;
		}

		// Token: 0x06000B0D RID: 2829 RVA: 0x0007467C File Offset: 0x0007287C
		public static void playMorningSong(bool ignoreDelay = false)
		{
			if (Game1.eventUp || Game1.dayOfMonth <= 0)
			{
				if (Game1.getMusicTrackName(MusicContext.Default) == "silence")
				{
					Game1.changeMusicTrack("none", true, MusicContext.Default);
				}
				return;
			}
			Game1.<>c__DisplayClass678_0 CS$<>8__locals1 = new Game1.<>c__DisplayClass678_0();
			LocationData data = Game1.currentLocation.GetData();
			if (Game1.currentLocation.GetLocationSpecificMusic() != null && (data == null || !data.MusicIsTownTheme))
			{
				Game1.changeMusicTrack("none", true, MusicContext.Default);
				GameLocation.HandleMusicChange(null, Game1.currentLocation);
				return;
			}
			if (Game1.IsRainingHere(null))
			{
				if (ignoreDelay)
				{
					Game1.<playMorningSong>g__PlayRain|678_0();
					return;
				}
				Game1.morningSongPlayAction = DelayedAction.functionAfterDelay(new Action(Game1.<playMorningSong>g__PlayRain|678_0), 500);
				return;
			}
			else
			{
				Game1.<>c__DisplayClass678_0 CS$<>8__locals2 = CS$<>8__locals1;
				GameLocation currentLocation = Game1.currentLocation;
				CS$<>8__locals2.context = ((currentLocation != null) ? currentLocation.GetLocationContext() : null);
				LocationContextData context = CS$<>8__locals1.context;
				if (((context != null) ? context.DefaultMusic : null) != null)
				{
					if (CS$<>8__locals1.context.DefaultMusicCondition == null || GameStateQuery.CheckConditions(CS$<>8__locals1.context.DefaultMusicCondition, null, null, null, null, null, null))
					{
						if (ignoreDelay)
						{
							CS$<>8__locals1.<playMorningSong>g__PlayLocationSong|1();
							return;
						}
						Game1.morningSongPlayAction = DelayedAction.functionAfterDelay(new Action(CS$<>8__locals1.<playMorningSong>g__PlayLocationSong|1), 500);
					}
					return;
				}
				if (ignoreDelay)
				{
					Game1.<playMorningSong>g__PlayDefault|678_2();
					return;
				}
				Game1.morningSongPlayAction = DelayedAction.functionAfterDelay(new Action(Game1.<playMorningSong>g__PlayDefault|678_2), 500);
				return;
			}
		}

		// Token: 0x06000B0E RID: 2830 RVA: 0x000747CC File Offset: 0x000729CC
		public static void doMorningStuff()
		{
			Game1.playMorningSong(false);
			DelayedAction.functionAfterDelay(delegate
			{
				while (Game1.morningQueue.Count > 0)
				{
					Game1.morningQueue.Dequeue()();
				}
			}, 1000);
			if (Game1.player.hasPendingCompletedQuests)
			{
				Game1.dayTimeMoneyBox.PingQuestLog();
			}
		}

		// Token: 0x06000B0F RID: 2831 RVA: 0x0007481F File Offset: 0x00072A1F
		public static void addMorningFluffFunction(Action action)
		{
			Game1.morningQueue.Enqueue(action);
		}

		// Token: 0x06000B10 RID: 2832 RVA: 0x0007482C File Offset: 0x00072A2C
		private Point getViewportCenter()
		{
			if (Game1.viewportTarget.X != -2.1474836E+09f)
			{
				if (Math.Abs((float)Game1.viewportCenter.X - Game1.viewportTarget.X) > Game1.viewportSpeed || Math.Abs((float)Game1.viewportCenter.Y - Game1.viewportTarget.Y) > Game1.viewportSpeed)
				{
					Vector2 velocity = Utility.getVelocityTowardPoint(Game1.viewportCenter, Game1.viewportTarget, Game1.viewportSpeed);
					Game1.viewportCenter.X = Game1.viewportCenter.X + (int)Math.Round((double)velocity.X);
					Game1.viewportCenter.Y = Game1.viewportCenter.Y + (int)Math.Round((double)velocity.Y);
				}
				else
				{
					if (Game1.viewportReachedTarget != null)
					{
						Game1.viewportReachedTarget();
						Game1.viewportReachedTarget = null;
					}
					Game1.viewportHold -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
					if (Game1.viewportHold <= 0)
					{
						Game1.viewportTarget = new Vector2(-2.1474836E+09f, -2.1474836E+09f);
						Game1.afterFadeFunction afterFadeFunction = Game1.afterViewport;
						if (afterFadeFunction != null)
						{
							afterFadeFunction();
						}
					}
				}
			}
			else
			{
				Game1.viewportCenter = Game1.getPlayerOrEventFarmer().StandingPixel;
			}
			return Game1.viewportCenter;
		}

		// Token: 0x06000B11 RID: 2833 RVA: 0x00074954 File Offset: 0x00072B54
		public static void afterFadeReturnViewportToPlayer()
		{
			Game1.viewportTarget = new Vector2(-2.1474836E+09f, -2.1474836E+09f);
			Game1.viewportHold = 0;
			Game1.viewportFreeze = false;
			Game1.viewportCenter = Game1.player.StandingPixel;
			Game1.globalFadeToClear(null, 0.02f);
		}

		// Token: 0x06000B12 RID: 2834 RVA: 0x00074990 File Offset: 0x00072B90
		public static bool isViewportOnCustomPath()
		{
			return Game1.viewportTarget.X != -2.1474836E+09f;
		}

		// Token: 0x06000B13 RID: 2835 RVA: 0x000749A6 File Offset: 0x00072BA6
		public static void moveViewportTo(Vector2 target, float speed, int holdTimer = 0, Game1.afterFadeFunction reachedTarget = null, Game1.afterFadeFunction endFunction = null)
		{
			Game1.viewportTarget = target;
			Game1.viewportSpeed = speed;
			Game1.viewportHold = holdTimer;
			Game1.afterViewport = endFunction;
			Game1.viewportReachedTarget = reachedTarget;
		}

		// Token: 0x06000B14 RID: 2836 RVA: 0x000749C7 File Offset: 0x00072BC7
		public static Farm getFarm()
		{
			return Game1.RequireLocation<Farm>("Farm", false);
		}

		// Token: 0x06000B15 RID: 2837 RVA: 0x000749D4 File Offset: 0x00072BD4
		public static void setMousePosition(int x, int y, bool ui_scale)
		{
			if (ui_scale)
			{
				Game1.setMousePositionRaw((int)((float)x * Game1.options.uiScale), (int)((float)y * Game1.options.uiScale));
				return;
			}
			Game1.setMousePositionRaw((int)((float)x * Game1.options.zoomLevel), (int)((float)y * Game1.options.zoomLevel));
		}

		// Token: 0x06000B16 RID: 2838 RVA: 0x00074A27 File Offset: 0x00072C27
		public static void setMousePosition(int x, int y)
		{
			Game1.setMousePosition(x, y, Game1.uiMode);
		}

		// Token: 0x06000B17 RID: 2839 RVA: 0x00074A35 File Offset: 0x00072C35
		public static void setMousePosition(Point position, bool ui_scale)
		{
			Game1.setMousePosition(position.X, position.Y, ui_scale);
		}

		// Token: 0x06000B18 RID: 2840 RVA: 0x00074A49 File Offset: 0x00072C49
		public static void setMousePosition(Point position)
		{
			Game1.setMousePosition(position, Game1.uiMode);
		}

		// Token: 0x06000B19 RID: 2841 RVA: 0x00074A56 File Offset: 0x00072C56
		public static void setMousePositionRaw(int x, int y)
		{
			Game1.input.SetMousePosition(x, y);
			Game1.InvalidateOldMouseMovement();
			Game1.lastCursorMotionWasMouse = false;
		}

		// Token: 0x06000B1A RID: 2842 RVA: 0x00074A6F File Offset: 0x00072C6F
		public static Point getMousePositionRaw()
		{
			return new Point(Game1.getMouseXRaw(), Game1.getMouseYRaw());
		}

		// Token: 0x06000B1B RID: 2843 RVA: 0x00074A80 File Offset: 0x00072C80
		public static Point getMousePosition(bool ui_scale)
		{
			return new Point(Game1.getMouseX(ui_scale), Game1.getMouseY(ui_scale));
		}

		// Token: 0x06000B1C RID: 2844 RVA: 0x00074A93 File Offset: 0x00072C93
		public static Point getMousePosition()
		{
			return Game1.getMousePosition(Game1.uiMode);
		}

		// Token: 0x1700018A RID: 394
		// (get) Token: 0x06000B1D RID: 2845 RVA: 0x00074AA0 File Offset: 0x00072CA0
		private static float thumbstickToMouseModifier
		{
			get
			{
				if (Game1._cursorSpeedDirty)
				{
					Game1.ComputeCursorSpeed();
				}
				return Game1._cursorSpeed / 720f * (float)Game1.viewport.Height * (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
			}
		}

		// Token: 0x06000B1E RID: 2846 RVA: 0x00074AE4 File Offset: 0x00072CE4
		private static void ComputeCursorSpeed()
		{
			Game1._cursorSpeedDirty = false;
			GamePadState p = Game1.input.GetGamePadState();
			float accellTol = 0.9f;
			bool isAccell = false;
			float num = p.ThumbSticks.Left.Length();
			float rlen = p.ThumbSticks.Right.Length();
			if (num > accellTol || rlen > accellTol)
			{
				isAccell = true;
			}
			float min = 0.7f;
			float max = 2f;
			float rate = 1f;
			if (Game1._cursorDragEnabled)
			{
				min = 0.5f;
				max = 2f;
				rate = 1f;
			}
			if (!isAccell)
			{
				rate = -5f;
			}
			if (Game1._cursorDragPrevEnabled != Game1._cursorDragEnabled)
			{
				Game1._cursorSpeedScale *= 0.5f;
			}
			Game1._cursorDragPrevEnabled = Game1._cursorDragEnabled;
			Game1._cursorSpeedScale += Game1._cursorUpdateElapsedSec * rate;
			Game1._cursorSpeedScale = MathHelper.Clamp(Game1._cursorSpeedScale, min, max);
			float num2 = 16f / (float)Game1.game1.TargetElapsedTime.TotalSeconds * Game1._cursorSpeedScale;
			float deltaSpeed = num2 - Game1._cursorSpeed;
			Game1._cursorSpeed = num2;
			Game1._cursorUpdateElapsedSec = 0f;
			if (Game1.debugMode)
			{
				Game1.log.Verbose(string.Concat(new string[]
				{
					"_cursorSpeed=",
					Game1._cursorSpeed.ToString("0.0"),
					", _cursorSpeedScale=",
					Game1._cursorSpeedScale.ToString("0.0"),
					", deltaSpeed=",
					deltaSpeed.ToString("0.0")
				}));
			}
		}

		// Token: 0x06000B1F RID: 2847 RVA: 0x00074C6C File Offset: 0x00072E6C
		private static void SetFreeCursorElapsed(float elapsedSec)
		{
			if (elapsedSec != Game1._cursorUpdateElapsedSec)
			{
				Game1._cursorUpdateElapsedSec = elapsedSec;
				Game1._cursorSpeedDirty = true;
			}
		}

		// Token: 0x06000B20 RID: 2848 RVA: 0x00074C82 File Offset: 0x00072E82
		public static void ResetFreeCursorDrag()
		{
			if (Game1._cursorDragEnabled)
			{
				Game1._cursorSpeedDirty = true;
			}
			Game1._cursorDragEnabled = false;
		}

		// Token: 0x06000B21 RID: 2849 RVA: 0x00074C97 File Offset: 0x00072E97
		public static void SetFreeCursorDrag()
		{
			if (!Game1._cursorDragEnabled)
			{
				Game1._cursorSpeedDirty = true;
			}
			Game1._cursorDragEnabled = true;
		}

		// Token: 0x06000B22 RID: 2850 RVA: 0x00074CAC File Offset: 0x00072EAC
		public static void updateActiveMenu(GameTime gameTime)
		{
			IClickableMenu active_menu = Game1.activeClickableMenu;
			while (active_menu.GetChildMenu() != null)
			{
				active_menu = active_menu.GetChildMenu();
			}
			if (!Program.gamePtr.IsActiveNoOverlay && Program.releaseBuild)
			{
				if (active_menu != null && active_menu.IsActive())
				{
					active_menu.update(gameTime);
				}
				return;
			}
			MouseState mouseState = Game1.input.GetMouseState();
			KeyboardState keyState = Game1.GetKeyboardState();
			GamePadState padState = Game1.input.GetGamePadState();
			if (Game1.CurrentEvent != null)
			{
				if ((mouseState.LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton == ButtonState.Released) || (Game1.options.gamepadControls && padState.IsButtonDown(Buttons.A) && Game1.oldPadState.IsButtonUp(Buttons.A)))
				{
					Game1.CurrentEvent.receiveMouseClick(Game1.getMouseX(), Game1.getMouseY());
				}
				else if (Game1.options.gamepadControls && padState.IsButtonDown(Buttons.Back) && Game1.oldPadState.IsButtonUp(Buttons.Back) && !Game1.CurrentEvent.skipped && Game1.CurrentEvent.skippable)
				{
					Game1.CurrentEvent.skipped = true;
					Game1.CurrentEvent.skipEvent();
					Game1.freezeControls = false;
				}
				if (Game1.CurrentEvent != null && Game1.CurrentEvent.skipped)
				{
					Game1.oldMouseState = Game1.input.GetMouseState();
					Game1.oldKBState = keyState;
					Game1.oldPadState = padState;
					return;
				}
			}
			if (Game1.options.gamepadControls && active_menu != null && active_menu.IsActive())
			{
				if (Game1.isGamePadThumbstickInMotion(0.2) && (!Game1.options.snappyMenus || active_menu.overrideSnappyMenuCursorMovementBan()))
				{
					Game1.setMousePositionRaw((int)((float)mouseState.X + padState.ThumbSticks.Left.X * Game1.thumbstickToMouseModifier), (int)((float)mouseState.Y - padState.ThumbSticks.Left.Y * Game1.thumbstickToMouseModifier));
				}
				if (active_menu != null && active_menu.IsActive() && (Game1.chatBox == null || !Game1.chatBox.isActive()))
				{
					foreach (Buttons b in Utility.getPressedButtons(padState, Game1.oldPadState))
					{
						active_menu.receiveGamePadButton(b);
						if (active_menu == null || !active_menu.IsActive())
						{
							break;
						}
					}
					foreach (Buttons b2 in Utility.getHeldButtons(padState))
					{
						if (active_menu != null && active_menu.IsActive())
						{
							active_menu.gamePadButtonHeld(b2);
						}
						if (active_menu == null || !active_menu.IsActive())
						{
							break;
						}
					}
				}
			}
			if ((Game1.getMouseX() != Game1.getOldMouseX() || Game1.getMouseY() != Game1.getOldMouseY()) && !Game1.isGamePadThumbstickInMotion(0.2) && !Game1.isDPadPressed())
			{
				Game1.lastCursorMotionWasMouse = true;
			}
			Game1.ResetFreeCursorDrag();
			if (active_menu != null && active_menu.IsActive())
			{
				active_menu.performHoverAction(Game1.getMouseX(), Game1.getMouseY());
			}
			if (active_menu != null && active_menu.IsActive())
			{
				active_menu.update(gameTime);
			}
			if (active_menu != null && active_menu.IsActive() && mouseState.LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton == ButtonState.Released)
			{
				if (Game1.chatBox != null && Game1.chatBox.isActive() && Game1.chatBox.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()))
				{
					Game1.chatBox.receiveLeftClick(Game1.getMouseX(), Game1.getMouseY(), true);
				}
				else
				{
					active_menu.receiveLeftClick(Game1.getMouseX(), Game1.getMouseY(), true);
				}
			}
			else if (active_menu != null && active_menu.IsActive() && mouseState.RightButton == ButtonState.Pressed && (Game1.oldMouseState.RightButton == ButtonState.Released || ((float)Game1.mouseClickPolling > 650f && !(active_menu is DialogueBox))))
			{
				active_menu.receiveRightClick(Game1.getMouseX(), Game1.getMouseY(), true);
				if ((float)Game1.mouseClickPolling > 650f)
				{
					Game1.mouseClickPolling = 600;
				}
				if ((active_menu == null || !active_menu.IsActive()) && Game1.activeClickableMenu == null)
				{
					Game1.rightClickPolling = 500;
					Game1.mouseClickPolling = 0;
				}
			}
			if (mouseState.ScrollWheelValue != Game1.oldMouseState.ScrollWheelValue && active_menu != null && active_menu.IsActive())
			{
				if (Game1.chatBox != null && Game1.chatBox.choosingEmoji && Game1.chatBox.emojiMenu.isWithinBounds(Game1.getOldMouseX(), Game1.getOldMouseY()))
				{
					Game1.chatBox.receiveScrollWheelAction(mouseState.ScrollWheelValue - Game1.oldMouseState.ScrollWheelValue);
				}
				else
				{
					active_menu.receiveScrollWheelAction(mouseState.ScrollWheelValue - Game1.oldMouseState.ScrollWheelValue);
				}
			}
			if (Game1.options.gamepadControls && active_menu != null && active_menu.IsActive())
			{
				Game1.thumbstickPollingTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
				if (Game1.thumbstickPollingTimer <= 0)
				{
					if (padState.ThumbSticks.Right.Y > 0.2f)
					{
						active_menu.receiveScrollWheelAction(1);
					}
					else if (padState.ThumbSticks.Right.Y < -0.2f)
					{
						active_menu.receiveScrollWheelAction(-1);
					}
				}
				if (Game1.thumbstickPollingTimer <= 0)
				{
					Game1.thumbstickPollingTimer = 220 - (int)(Math.Abs(padState.ThumbSticks.Right.Y) * 170f);
				}
				if (Math.Abs(padState.ThumbSticks.Right.Y) < 0.2f)
				{
					Game1.thumbstickPollingTimer = 0;
				}
			}
			if (active_menu != null && active_menu.IsActive() && mouseState.LeftButton == ButtonState.Released && Game1.oldMouseState.LeftButton == ButtonState.Pressed)
			{
				active_menu.releaseLeftClick(Game1.getMouseX(), Game1.getMouseY());
			}
			else if (active_menu != null && active_menu.IsActive() && mouseState.LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton == ButtonState.Pressed)
			{
				active_menu.leftClickHeld(Game1.getMouseX(), Game1.getMouseY());
			}
			foreach (Keys i in keyState.GetPressedKeys())
			{
				if (active_menu != null && active_menu.IsActive() && !Game1.oldKBState.GetPressedKeys().Contains(i))
				{
					active_menu.receiveKeyPress(i);
				}
			}
			if (Game1.chatBox == null || !Game1.chatBox.isActive())
			{
				if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveUpButton) || (Game1.options.snappyMenus && Game1.options.gamepadControls && (Math.Abs(padState.ThumbSticks.Left.X) < padState.ThumbSticks.Left.Y || padState.IsButtonDown(Buttons.DPadUp))))
				{
					Game1.directionKeyPolling[0] -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
				}
				else if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveRightButton) || (Game1.options.snappyMenus && Game1.options.gamepadControls && (padState.ThumbSticks.Left.X > Math.Abs(padState.ThumbSticks.Left.Y) || padState.IsButtonDown(Buttons.DPadRight))))
				{
					Game1.directionKeyPolling[1] -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
				}
				else if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveDownButton) || (Game1.options.snappyMenus && Game1.options.gamepadControls && (Math.Abs(padState.ThumbSticks.Left.X) < Math.Abs(padState.ThumbSticks.Left.Y) || padState.IsButtonDown(Buttons.DPadDown))))
				{
					Game1.directionKeyPolling[2] -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
				}
				else if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveLeftButton) || (Game1.options.snappyMenus && Game1.options.gamepadControls && (Math.Abs(padState.ThumbSticks.Left.X) > Math.Abs(padState.ThumbSticks.Left.Y) || padState.IsButtonDown(Buttons.DPadLeft))))
				{
					Game1.directionKeyPolling[3] -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
				}
				if (Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveUpButton) && (!Game1.options.snappyMenus || !Game1.options.gamepadControls || ((double)padState.ThumbSticks.Left.Y < 0.1 && padState.IsButtonUp(Buttons.DPadUp))))
				{
					Game1.directionKeyPolling[0] = 250;
				}
				if (Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveRightButton) && (!Game1.options.snappyMenus || !Game1.options.gamepadControls || ((double)padState.ThumbSticks.Left.X < 0.1 && padState.IsButtonUp(Buttons.DPadRight))))
				{
					Game1.directionKeyPolling[1] = 250;
				}
				if (Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveDownButton) && (!Game1.options.snappyMenus || !Game1.options.gamepadControls || ((double)padState.ThumbSticks.Left.Y > -0.1 && padState.IsButtonUp(Buttons.DPadDown))))
				{
					Game1.directionKeyPolling[2] = 250;
				}
				if (Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveLeftButton) && (!Game1.options.snappyMenus || !Game1.options.gamepadControls || ((double)padState.ThumbSticks.Left.X > -0.1 && padState.IsButtonUp(Buttons.DPadLeft))))
				{
					Game1.directionKeyPolling[3] = 250;
				}
				if (Game1.directionKeyPolling[0] <= 0 && active_menu != null && active_menu.IsActive())
				{
					active_menu.receiveKeyPress(Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveUpButton));
					Game1.directionKeyPolling[0] = 70;
				}
				if (Game1.directionKeyPolling[1] <= 0 && active_menu != null && active_menu.IsActive())
				{
					active_menu.receiveKeyPress(Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveRightButton));
					Game1.directionKeyPolling[1] = 70;
				}
				if (Game1.directionKeyPolling[2] <= 0 && active_menu != null && active_menu.IsActive())
				{
					active_menu.receiveKeyPress(Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveDownButton));
					Game1.directionKeyPolling[2] = 70;
				}
				if (Game1.directionKeyPolling[3] <= 0 && active_menu != null && active_menu.IsActive())
				{
					active_menu.receiveKeyPress(Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveLeftButton));
					Game1.directionKeyPolling[3] = 70;
				}
				if (Game1.options.gamepadControls && active_menu != null && active_menu.IsActive())
				{
					if (!active_menu.areGamePadControlsImplemented() && padState.IsButtonDown(Buttons.A) && (!Game1.oldPadState.IsButtonDown(Buttons.A) || ((float)Game1.gamePadAButtonPolling > 650f && !(active_menu is DialogueBox))))
					{
						active_menu.receiveLeftClick(Game1.getMousePosition().X, Game1.getMousePosition().Y, true);
						if ((float)Game1.gamePadAButtonPolling > 650f)
						{
							Game1.gamePadAButtonPolling = 600;
						}
					}
					else if (!active_menu.areGamePadControlsImplemented() && !padState.IsButtonDown(Buttons.A) && Game1.oldPadState.IsButtonDown(Buttons.A))
					{
						active_menu.releaseLeftClick(Game1.getMousePosition().X, Game1.getMousePosition().Y);
					}
					else if (!active_menu.areGamePadControlsImplemented() && padState.IsButtonDown(Buttons.X) && (!Game1.oldPadState.IsButtonDown(Buttons.X) || ((float)Game1.gamePadXButtonPolling > 650f && !(active_menu is DialogueBox))))
					{
						active_menu.receiveRightClick(Game1.getMousePosition().X, Game1.getMousePosition().Y, true);
						if ((float)Game1.gamePadXButtonPolling > 650f)
						{
							Game1.gamePadXButtonPolling = 600;
						}
					}
					foreach (Buttons b3 in Utility.getPressedButtons(padState, Game1.oldPadState))
					{
						if (active_menu == null || !active_menu.IsActive())
						{
							break;
						}
						Keys key = Utility.mapGamePadButtonToKey(b3);
						if (!(active_menu is FarmhandMenu) || Game1.game1.IsMainInstance || !Game1.options.doesInputListContain(Game1.options.menuButton, key))
						{
							active_menu.receiveKeyPress(key);
						}
					}
					if (active_menu != null && active_menu.IsActive() && !active_menu.areGamePadControlsImplemented() && padState.IsButtonDown(Buttons.A) && Game1.oldPadState.IsButtonDown(Buttons.A))
					{
						active_menu.leftClickHeld(Game1.getMousePosition().X, Game1.getMousePosition().Y);
					}
					if (padState.IsButtonDown(Buttons.X))
					{
						Game1.gamePadXButtonPolling += gameTime.ElapsedGameTime.Milliseconds;
					}
					else
					{
						Game1.gamePadXButtonPolling = 0;
					}
					if (padState.IsButtonDown(Buttons.A))
					{
						Game1.gamePadAButtonPolling += gameTime.ElapsedGameTime.Milliseconds;
					}
					else
					{
						Game1.gamePadAButtonPolling = 0;
					}
					if (!active_menu.IsActive() && Game1.activeClickableMenu == null)
					{
						Game1.rightClickPolling = 500;
						Game1.gamePadXButtonPolling = 0;
						Game1.gamePadAButtonPolling = 0;
					}
				}
			}
			if (mouseState.RightButton == ButtonState.Pressed)
			{
				Game1.mouseClickPolling += gameTime.ElapsedGameTime.Milliseconds;
			}
			else
			{
				Game1.mouseClickPolling = 0;
			}
			Game1.oldMouseState = Game1.input.GetMouseState();
			Game1.oldKBState = keyState;
			Game1.oldPadState = padState;
		}

		// Token: 0x06000B23 RID: 2851 RVA: 0x00075A50 File Offset: 0x00073C50
		public bool ShowLocalCoopJoinMenu()
		{
			if (!base.IsMainInstance)
			{
				return false;
			}
			if (Game1.gameMode != 3)
			{
				return false;
			}
			int free_farmhands = 0;
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				Cabin cabin = location as Cabin;
				if (cabin != null && (!cabin.HasOwner || !cabin.IsOwnerActivated))
				{
					int free_farmhands = free_farmhands;
					free_farmhands++;
				}
				return true;
			}, true, false);
			if (free_farmhands == 0)
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:CoopMenu_NoSlots"), true);
				return false;
			}
			if (Game1.currentMinigame != null)
			{
				return false;
			}
			if (Game1.activeClickableMenu != null)
			{
				return false;
			}
			if (!this.IsLocalCoopJoinable())
			{
				return false;
			}
			Game1.playSound("bigSelect", null);
			Game1.activeClickableMenu = new LocalCoopJoinMenu();
			return true;
		}

		// Token: 0x06000B24 RID: 2852 RVA: 0x00075AEC File Offset: 0x00073CEC
		public static void updateTextEntry(GameTime gameTime)
		{
			MouseState mouseState = Game1.input.GetMouseState();
			KeyboardState keyState = Game1.GetKeyboardState();
			GamePadState padState = Game1.input.GetGamePadState();
			if (Game1.options.gamepadControls && Game1.textEntry != null && Game1.textEntry != null)
			{
				foreach (Buttons b in Utility.getPressedButtons(padState, Game1.oldPadState))
				{
					Game1.textEntry.receiveGamePadButton(b);
					if (Game1.textEntry == null)
					{
						break;
					}
				}
				foreach (Buttons b2 in Utility.getHeldButtons(padState))
				{
					TextEntryMenu textEntry = Game1.textEntry;
					if (textEntry != null)
					{
						textEntry.gamePadButtonHeld(b2);
					}
					if (Game1.textEntry == null)
					{
						break;
					}
				}
			}
			TextEntryMenu textEntry2 = Game1.textEntry;
			if (textEntry2 != null)
			{
				textEntry2.performHoverAction(Game1.getMouseX(), Game1.getMouseY());
			}
			TextEntryMenu textEntry3 = Game1.textEntry;
			if (textEntry3 != null)
			{
				textEntry3.update(gameTime);
			}
			if (Game1.textEntry != null && mouseState.LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton == ButtonState.Released)
			{
				Game1.textEntry.receiveLeftClick(Game1.getMouseX(), Game1.getMouseY(), true);
			}
			else if (Game1.textEntry != null && mouseState.RightButton == ButtonState.Pressed && (Game1.oldMouseState.RightButton == ButtonState.Released || (float)Game1.mouseClickPolling > 650f))
			{
				Game1.textEntry.receiveRightClick(Game1.getMouseX(), Game1.getMouseY(), true);
				if ((float)Game1.mouseClickPolling > 650f)
				{
					Game1.mouseClickPolling = 600;
				}
				if (Game1.textEntry == null)
				{
					Game1.rightClickPolling = 500;
					Game1.mouseClickPolling = 0;
				}
			}
			if (mouseState.ScrollWheelValue != Game1.oldMouseState.ScrollWheelValue && Game1.textEntry != null)
			{
				if (Game1.chatBox != null && Game1.chatBox.choosingEmoji && Game1.chatBox.emojiMenu.isWithinBounds(Game1.getOldMouseX(), Game1.getOldMouseY()))
				{
					Game1.chatBox.receiveScrollWheelAction(mouseState.ScrollWheelValue - Game1.oldMouseState.ScrollWheelValue);
				}
				else
				{
					Game1.textEntry.receiveScrollWheelAction(mouseState.ScrollWheelValue - Game1.oldMouseState.ScrollWheelValue);
				}
			}
			if (Game1.options.gamepadControls && Game1.textEntry != null)
			{
				Game1.thumbstickPollingTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
				if (Game1.thumbstickPollingTimer <= 0)
				{
					if (padState.ThumbSticks.Right.Y > 0.2f)
					{
						Game1.textEntry.receiveScrollWheelAction(1);
					}
					else if (padState.ThumbSticks.Right.Y < -0.2f)
					{
						Game1.textEntry.receiveScrollWheelAction(-1);
					}
				}
				if (Game1.thumbstickPollingTimer <= 0)
				{
					Game1.thumbstickPollingTimer = 220 - (int)(Math.Abs(padState.ThumbSticks.Right.Y) * 170f);
				}
				if (Math.Abs(padState.ThumbSticks.Right.Y) < 0.2f)
				{
					Game1.thumbstickPollingTimer = 0;
				}
			}
			if (Game1.textEntry != null && mouseState.LeftButton == ButtonState.Released && Game1.oldMouseState.LeftButton == ButtonState.Pressed)
			{
				Game1.textEntry.releaseLeftClick(Game1.getMouseX(), Game1.getMouseY());
			}
			else if (Game1.textEntry != null && mouseState.LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton == ButtonState.Pressed)
			{
				Game1.textEntry.leftClickHeld(Game1.getMouseX(), Game1.getMouseY());
			}
			foreach (Keys i in keyState.GetPressedKeys())
			{
				if (Game1.textEntry != null && !Game1.oldKBState.GetPressedKeys().Contains(i))
				{
					Game1.textEntry.receiveKeyPress(i);
				}
			}
			if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveUpButton) || (Game1.options.snappyMenus && Game1.options.gamepadControls && (Math.Abs(padState.ThumbSticks.Left.X) < padState.ThumbSticks.Left.Y || padState.IsButtonDown(Buttons.DPadUp))))
			{
				Game1.directionKeyPolling[0] -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			}
			else if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveRightButton) || (Game1.options.snappyMenus && Game1.options.gamepadControls && (padState.ThumbSticks.Left.X > Math.Abs(padState.ThumbSticks.Left.Y) || padState.IsButtonDown(Buttons.DPadRight))))
			{
				Game1.directionKeyPolling[1] -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			}
			else if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveDownButton) || (Game1.options.snappyMenus && Game1.options.gamepadControls && (Math.Abs(padState.ThumbSticks.Left.X) < Math.Abs(padState.ThumbSticks.Left.Y) || padState.IsButtonDown(Buttons.DPadDown))))
			{
				Game1.directionKeyPolling[2] -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			}
			else if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveLeftButton) || (Game1.options.snappyMenus && Game1.options.gamepadControls && (Math.Abs(padState.ThumbSticks.Left.X) > Math.Abs(padState.ThumbSticks.Left.Y) || padState.IsButtonDown(Buttons.DPadLeft))))
			{
				Game1.directionKeyPolling[3] -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			}
			if (Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveUpButton) && (!Game1.options.snappyMenus || !Game1.options.gamepadControls || ((double)padState.ThumbSticks.Left.Y < 0.1 && padState.IsButtonUp(Buttons.DPadUp))))
			{
				Game1.directionKeyPolling[0] = 250;
			}
			if (Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveRightButton) && (!Game1.options.snappyMenus || !Game1.options.gamepadControls || ((double)padState.ThumbSticks.Left.X < 0.1 && padState.IsButtonUp(Buttons.DPadRight))))
			{
				Game1.directionKeyPolling[1] = 250;
			}
			if (Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveDownButton) && (!Game1.options.snappyMenus || !Game1.options.gamepadControls || ((double)padState.ThumbSticks.Left.Y > -0.1 && padState.IsButtonUp(Buttons.DPadDown))))
			{
				Game1.directionKeyPolling[2] = 250;
			}
			if (Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveLeftButton) && (!Game1.options.snappyMenus || !Game1.options.gamepadControls || ((double)padState.ThumbSticks.Left.X > -0.1 && padState.IsButtonUp(Buttons.DPadLeft))))
			{
				Game1.directionKeyPolling[3] = 250;
			}
			if (Game1.directionKeyPolling[0] <= 0 && Game1.textEntry != null)
			{
				Game1.textEntry.receiveKeyPress(Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveUpButton));
				Game1.directionKeyPolling[0] = 70;
			}
			if (Game1.directionKeyPolling[1] <= 0 && Game1.textEntry != null)
			{
				Game1.textEntry.receiveKeyPress(Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveRightButton));
				Game1.directionKeyPolling[1] = 70;
			}
			if (Game1.directionKeyPolling[2] <= 0 && Game1.textEntry != null)
			{
				Game1.textEntry.receiveKeyPress(Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveDownButton));
				Game1.directionKeyPolling[2] = 70;
			}
			if (Game1.directionKeyPolling[3] <= 0 && Game1.textEntry != null)
			{
				Game1.textEntry.receiveKeyPress(Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveLeftButton));
				Game1.directionKeyPolling[3] = 70;
			}
			if (Game1.options.gamepadControls && Game1.textEntry != null)
			{
				if (!Game1.textEntry.areGamePadControlsImplemented() && padState.IsButtonDown(Buttons.A) && (!Game1.oldPadState.IsButtonDown(Buttons.A) || (float)Game1.gamePadAButtonPolling > 650f))
				{
					Game1.textEntry.receiveLeftClick(Game1.getMousePosition().X, Game1.getMousePosition().Y, true);
					if ((float)Game1.gamePadAButtonPolling > 650f)
					{
						Game1.gamePadAButtonPolling = 600;
					}
				}
				else if (!Game1.textEntry.areGamePadControlsImplemented() && !padState.IsButtonDown(Buttons.A) && Game1.oldPadState.IsButtonDown(Buttons.A))
				{
					Game1.textEntry.releaseLeftClick(Game1.getMousePosition().X, Game1.getMousePosition().Y);
				}
				else if (!Game1.textEntry.areGamePadControlsImplemented() && padState.IsButtonDown(Buttons.X) && (!Game1.oldPadState.IsButtonDown(Buttons.X) || (float)Game1.gamePadXButtonPolling > 650f))
				{
					Game1.textEntry.receiveRightClick(Game1.getMousePosition().X, Game1.getMousePosition().Y, true);
					if ((float)Game1.gamePadXButtonPolling > 650f)
					{
						Game1.gamePadXButtonPolling = 600;
					}
				}
				foreach (Buttons b3 in Utility.getPressedButtons(padState, Game1.oldPadState))
				{
					if (Game1.textEntry == null)
					{
						break;
					}
					Game1.textEntry.receiveKeyPress(Utility.mapGamePadButtonToKey(b3));
				}
				if (Game1.textEntry != null && !Game1.textEntry.areGamePadControlsImplemented() && padState.IsButtonDown(Buttons.A) && Game1.oldPadState.IsButtonDown(Buttons.A))
				{
					Game1.textEntry.leftClickHeld(Game1.getMousePosition().X, Game1.getMousePosition().Y);
				}
				if (padState.IsButtonDown(Buttons.X))
				{
					Game1.gamePadXButtonPolling += gameTime.ElapsedGameTime.Milliseconds;
				}
				else
				{
					Game1.gamePadXButtonPolling = 0;
				}
				if (padState.IsButtonDown(Buttons.A))
				{
					Game1.gamePadAButtonPolling += gameTime.ElapsedGameTime.Milliseconds;
				}
				else
				{
					Game1.gamePadAButtonPolling = 0;
				}
				if (Game1.textEntry == null)
				{
					Game1.rightClickPolling = 500;
					Game1.gamePadAButtonPolling = 0;
					Game1.gamePadXButtonPolling = 0;
				}
			}
			if (mouseState.RightButton == ButtonState.Pressed)
			{
				Game1.mouseClickPolling += gameTime.ElapsedGameTime.Milliseconds;
			}
			else
			{
				Game1.mouseClickPolling = 0;
			}
			Game1.oldMouseState = Game1.input.GetMouseState();
			Game1.oldKBState = keyState;
			Game1.oldPadState = padState;
		}

		// Token: 0x06000B25 RID: 2853 RVA: 0x000765EC File Offset: 0x000747EC
		public static string DateCompiled()
		{
			Version version = Assembly.GetExecutingAssembly().GetName().Version;
			return string.Concat(new string[]
			{
				version.Major.ToString(),
				".",
				version.Minor.ToString(),
				".",
				version.Build.ToString(),
				".",
				version.Revision.ToString()
			});
		}

		// Token: 0x06000B26 RID: 2854 RVA: 0x00076670 File Offset: 0x00074870
		public static void updatePause(GameTime gameTime)
		{
			if (Game1.IsDedicatedHost)
			{
				Game1.pauseTime = 0f;
			}
			Game1.pauseTime -= (float)gameTime.ElapsedGameTime.Milliseconds;
			if (Game1.player.isCrafting && Game1.random.NextDouble() < 0.007)
			{
				Game1.playSound("crafting", null);
			}
			if (Game1.pauseTime <= 0f)
			{
				if (Game1.currentObjectDialogue.Count == 0)
				{
					Game1.messagePause = false;
				}
				Game1.pauseTime = 0f;
				if (!string.IsNullOrEmpty(Game1.messageAfterPause))
				{
					Game1.player.isCrafting = false;
					Game1.drawObjectDialogue(Game1.messageAfterPause);
					Game1.messageAfterPause = "";
					if (Game1.killScreen)
					{
						Game1.killScreen = false;
						Game1.player.health = 10;
					}
				}
				else if (Game1.killScreen)
				{
					Game1.multiplayer.globalChatInfoMessage("PlayerDeath", new string[]
					{
						Game1.player.Name
					});
					Game1.screenGlow = false;
					bool handledRevive = false;
					if (Game1.currentLocation.GetLocationContext().ReviveLocations != null)
					{
						using (List<ReviveLocation>.Enumerator enumerator = Game1.currentLocation.GetLocationContext().ReviveLocations.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								ReviveLocation revive_location = enumerator.Current;
								if (GameStateQuery.CheckConditions(revive_location.Condition, null, Game1.player, null, null, null, null))
								{
									Game1.warpFarmer(revive_location.Location, revive_location.Position.X, revive_location.Position.Y, false);
									handledRevive = true;
									break;
								}
							}
							goto IL_214;
						}
					}
					foreach (ReviveLocation revive_location2 in LocationContexts.Default.ReviveLocations)
					{
						if (GameStateQuery.CheckConditions(revive_location2.Condition, null, Game1.player, null, null, null, null))
						{
							Game1.warpFarmer(revive_location2.Location, revive_location2.Position.X, revive_location2.Position.Y, false);
							handledRevive = true;
							break;
						}
					}
					IL_214:
					if (!handledRevive)
					{
						Game1.warpFarmer("Hospital", 20, 12, false);
					}
				}
				if (Game1.currentLocation.currentEvent != null)
				{
					Event currentEvent = Game1.currentLocation.currentEvent;
					int currentCommand = currentEvent.CurrentCommand;
					currentEvent.CurrentCommand = currentCommand + 1;
				}
			}
		}

		// Token: 0x06000B27 RID: 2855 RVA: 0x000768E8 File Offset: 0x00074AE8
		public static void CheckValidFullscreenResolution(ref int width, ref int height)
		{
			int preferredW = width;
			int preferredH = height;
			foreach (DisplayMode v in Game1.graphics.GraphicsDevice.Adapter.SupportedDisplayModes)
			{
				if (v.Width >= 1280 && v.Width == preferredW && v.Height == preferredH)
				{
					width = preferredW;
					height = preferredH;
					return;
				}
			}
			foreach (DisplayMode v2 in Game1.graphics.GraphicsDevice.Adapter.SupportedDisplayModes)
			{
				if (v2.Width >= 1280 && v2.Width == Game1.graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width && v2.Height == Game1.graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height)
				{
					width = Game1.graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width;
					height = Game1.graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height;
					return;
				}
			}
			bool found_resolution = false;
			foreach (DisplayMode v3 in Game1.graphics.GraphicsDevice.Adapter.SupportedDisplayModes)
			{
				if (v3.Width >= 1280 && preferredW > v3.Width)
				{
					width = v3.Width;
					height = v3.Height;
					found_resolution = true;
				}
			}
			if (found_resolution)
			{
				return;
			}
			Game1.log.Warn("Requested fullscreen resolution not valid, switching to windowed.");
			width = 1280;
			height = 720;
			Game1.options.fullscreen = false;
		}

		// Token: 0x06000B28 RID: 2856 RVA: 0x00076AE8 File Offset: 0x00074CE8
		public static void toggleNonBorderlessWindowedFullscreen()
		{
			int width = Game1.options.preferredResolutionX;
			int height = Game1.options.preferredResolutionY;
			Game1.graphics.HardwareModeSwitch = (Game1.options.fullscreen && !Game1.options.windowedBorderlessFullscreen);
			if (Game1.options.fullscreen && !Game1.options.windowedBorderlessFullscreen)
			{
				Game1.CheckValidFullscreenResolution(ref width, ref height);
			}
			if (!Game1.options.fullscreen && !Game1.options.windowedBorderlessFullscreen)
			{
				width = 1280;
				height = 720;
			}
			Game1.graphics.PreferredBackBufferWidth = width;
			Game1.graphics.PreferredBackBufferHeight = height;
			if (Game1.options.fullscreen != Game1.graphics.IsFullScreen)
			{
				Game1.graphics.ToggleFullScreen();
			}
			Game1.graphics.ApplyChanges();
			Game1.updateViewportForScreenSizeChange(true, Game1.graphics.PreferredBackBufferWidth, Game1.graphics.PreferredBackBufferHeight);
			GameRunner.instance.OnWindowSizeChange(null, null);
		}

		// Token: 0x06000B29 RID: 2857 RVA: 0x00076BDC File Offset: 0x00074DDC
		public static void toggleFullscreen()
		{
			if (Game1.options.windowedBorderlessFullscreen)
			{
				Game1.graphics.HardwareModeSwitch = false;
				Game1.graphics.IsFullScreen = true;
				Game1.graphics.ApplyChanges();
				Game1.graphics.PreferredBackBufferWidth = Program.gamePtr.Window.ClientBounds.Width;
				Game1.graphics.PreferredBackBufferHeight = Program.gamePtr.Window.ClientBounds.Height;
			}
			else
			{
				Game1.toggleNonBorderlessWindowedFullscreen();
			}
			GameRunner.instance.OnWindowSizeChange(null, null);
		}

		// Token: 0x1700018B RID: 395
		// (get) Token: 0x06000B2A RID: 2858 RVA: 0x00076C64 File Offset: 0x00074E64
		public static bool isFullscreen
		{
			get
			{
				return Game1.graphics.IsFullScreen;
			}
		}

		// Token: 0x06000B2B RID: 2859 RVA: 0x00076C70 File Offset: 0x00074E70
		private void checkForEscapeKeys()
		{
			KeyboardState kbState = Game1.input.GetKeyboardState();
			if (!base.IsMainInstance)
			{
				return;
			}
			if (kbState.IsKeyDown(Keys.LeftAlt) && kbState.IsKeyDown(Keys.Enter) && (Game1.oldKBState.IsKeyUp(Keys.LeftAlt) || Game1.oldKBState.IsKeyUp(Keys.Enter)))
			{
				if (Game1.options.isCurrentlyFullscreen() || Game1.options.isCurrentlyWindowedBorderless())
				{
					Game1.options.setWindowedOption(1);
				}
				else
				{
					Game1.options.setWindowedOption(0);
				}
			}
			if ((Game1.player.UsingTool || Game1.freezeControls) && kbState.IsKeyDown(Keys.RightShift) && kbState.IsKeyDown(Keys.R) && kbState.IsKeyDown(Keys.Delete))
			{
				Game1.freezeControls = false;
				Game1.player.forceCanMove();
				Game1.player.completelyStopAnimatingOrDoingAction();
				Game1.player.UsingTool = false;
			}
		}

		// Token: 0x06000B2C RID: 2860 RVA: 0x00076D55 File Offset: 0x00074F55
		public static bool IsPressEvent(ref KeyboardState state, Keys key)
		{
			if (state.IsKeyDown(key) && !Game1.oldKBState.IsKeyDown(key))
			{
				Game1.oldKBState = state;
				return true;
			}
			return false;
		}

		// Token: 0x06000B2D RID: 2861 RVA: 0x00076D7B File Offset: 0x00074F7B
		public static bool IsPressEvent(ref GamePadState state, Buttons btn)
		{
			if (state.IsConnected && state.IsButtonDown(btn) && !Game1.oldPadState.IsButtonDown(btn))
			{
				Game1.oldPadState = state;
				return true;
			}
			return false;
		}

		// Token: 0x06000B2E RID: 2862 RVA: 0x00076DAC File Offset: 0x00074FAC
		public static bool isOneOfTheseKeysDown(KeyboardState state, InputButton[] keys)
		{
			foreach (InputButton i in keys)
			{
				if (i.key != Keys.None && state.IsKeyDown(i.key))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000B2F RID: 2863 RVA: 0x00076DEC File Offset: 0x00074FEC
		public static bool areAllOfTheseKeysUp(KeyboardState state, InputButton[] keys)
		{
			foreach (InputButton i in keys)
			{
				if (i.key != Keys.None && !state.IsKeyUp(i.key))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000B30 RID: 2864 RVA: 0x00076E2C File Offset: 0x0007502C
		internal void UpdateTitleScreen(GameTime time)
		{
			if (Game1.quit)
			{
				base.Exit();
				Game1.changeMusicTrack("none", false, MusicContext.Default);
			}
			switch (Game1.gameMode)
			{
			case 6:
				this.UpdateTitleScreenDuringLoadingMode();
				return;
			case 7:
				Game1.currentLoader.MoveNext();
				return;
			case 8:
				Game1.pauseAccumulator -= (float)time.ElapsedGameTime.Milliseconds;
				if (Game1.pauseAccumulator <= 0f)
				{
					Game1.pauseAccumulator = 0f;
					Game1.setGameMode(3);
					if (Game1.currentObjectDialogue.Count > 0)
					{
						Game1.messagePause = true;
						Game1.pauseTime = 1E+10f;
						Game1.fadeToBlackAlpha = 1f;
						Game1.player.CanMove = false;
					}
				}
				return;
			default:
				if (Game1.game1.instanceIndex > 0)
				{
					if (Game1.activeClickableMenu == null && Game1.ticks > 1)
					{
						Game1.activeClickableMenu = new FarmhandMenu(Game1.multiplayer.InitClient(new LidgrenClient("localhost")));
						Game1.activeClickableMenu.populateClickableComponentList();
						if (Game1.options.SnappyMenus)
						{
							Game1.activeClickableMenu.snapToDefaultClickableComponent();
						}
					}
					return;
				}
				if (Game1.fadeToBlackAlpha < 1f && Game1.fadeIn)
				{
					Game1.fadeToBlackAlpha += 0.02f;
				}
				else if (Game1.fadeToBlackAlpha > 0f && Game1.fadeToBlack)
				{
					Game1.fadeToBlackAlpha -= 0.02f;
				}
				if (Game1.pauseTime > 0f)
				{
					Game1.pauseTime = Math.Max(0f, Game1.pauseTime - (float)time.ElapsedGameTime.Milliseconds);
				}
				if (Game1.fadeToBlackAlpha >= 1f)
				{
					byte gameMode = Game1.gameMode;
					if (gameMode != 0)
					{
						if (gameMode == 4 && !Game1.fadeToBlack)
						{
							Game1.fadeIn = false;
							Game1.fadeToBlack = true;
							Game1.fadeToBlackAlpha = 2.5f;
							return;
						}
					}
					else
					{
						if (Game1.currentSong == null && Game1.pauseTime <= 0f && base.IsMainInstance)
						{
							ICue cue;
							Game1.playSound("spring_day_ambient", out cue);
							Game1.currentSong = cue;
						}
						if (Game1.activeClickableMenu == null && !Game1.quit)
						{
							Game1.activeClickableMenu = new TitleMenu();
							return;
						}
					}
				}
				else if (Game1.fadeToBlackAlpha <= 0f)
				{
					byte gameMode = Game1.gameMode;
					if (gameMode != 0)
					{
						if (gameMode == 4 && Game1.fadeToBlack)
						{
							Game1.fadeIn = true;
							Game1.fadeToBlack = false;
							Game1.setGameMode(0);
							Game1.pauseTime = 2000f;
							return;
						}
					}
					else if (Game1.fadeToBlack)
					{
						Game1.currentLoader = Utility.generateNewFarm(Game1.IsClient);
						Game1.setGameMode(6);
						Game1.loadingMessage = (Game1.IsClient ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2574", Game1.client.serverName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2575"));
						Game1.exitActiveMenu();
					}
				}
				return;
			}
		}

		// Token: 0x06000B31 RID: 2865 RVA: 0x000770E0 File Offset: 0x000752E0
		internal void UpdateTitleScreenDuringLoadingMode()
		{
			if (Game1._requestedMusicTracks.Count > 0)
			{
				Game1._requestedMusicTracks = new Dictionary<MusicContext, KeyValuePair<string, bool>>();
			}
			Game1.requestedMusicTrack = "none";
			Game1.requestedMusicTrackOverrideable = false;
			Game1.requestedMusicDirty = true;
			if (Game1.currentLoader != null && !Game1.currentLoader.MoveNext())
			{
				Game1.currentLoader = null;
				if (Game1.gameMode == 3)
				{
					Game1.setGameMode(3);
					Game1.fadeIn = true;
					Game1.fadeToBlackAlpha = 0.99f;
					return;
				}
				Game1.ExitToTitle(null);
			}
		}

		// Token: 0x06000B32 RID: 2866 RVA: 0x00077158 File Offset: 0x00075358
		public static bool IsThereABuildingUnderConstruction(string builder = "Robin")
		{
			return Game1.netWorldState.Value.GetBuilderData(builder) != null;
		}

		// Token: 0x06000B33 RID: 2867 RVA: 0x00077170 File Offset: 0x00075370
		public static Building GetBuildingUnderConstruction(string builder = "Robin")
		{
			BuilderData builder_data = Game1.netWorldState.Value.GetBuilderData(builder);
			if (builder_data == null)
			{
				return null;
			}
			GameLocation location = Game1.getLocationFromName(builder_data.buildingLocation.Value);
			if (location == null)
			{
				return null;
			}
			if (Game1.client != null && !Game1.multiplayer.isActiveLocation(location))
			{
				return null;
			}
			return location.getBuildingAt(Utility.PointToVector2(builder_data.buildingTile.Value));
		}

		// Token: 0x06000B34 RID: 2868 RVA: 0x000771D5 File Offset: 0x000753D5
		public static bool IsBuildingConstructed(string name)
		{
			return Game1.GetNumberBuildingsConstructed(name, false) > 0;
		}

		// Token: 0x06000B35 RID: 2869 RVA: 0x000771E4 File Offset: 0x000753E4
		public static int GetNumberBuildingsConstructed(bool includeUnderConstruction = false)
		{
			int count = 0;
			foreach (string locationName in Game1.netWorldState.Value.LocationsWithBuildings)
			{
				int num = count;
				GameLocation locationFromName = Game1.getLocationFromName(locationName);
				count = num + ((locationFromName != null) ? locationFromName.getNumberBuildingsConstructed(includeUnderConstruction) : 0);
			}
			return count;
		}

		// Token: 0x06000B36 RID: 2870 RVA: 0x0007724C File Offset: 0x0007544C
		public static int GetNumberBuildingsConstructed(string name, bool includeUnderConstruction = false)
		{
			int count = 0;
			foreach (string locationName in Game1.netWorldState.Value.LocationsWithBuildings)
			{
				int num = count;
				GameLocation locationFromName = Game1.getLocationFromName(locationName);
				count = num + ((locationFromName != null) ? locationFromName.getNumberBuildingsConstructed(name, includeUnderConstruction) : 0);
			}
			return count;
		}

		// Token: 0x06000B37 RID: 2871 RVA: 0x000772B8 File Offset: 0x000754B8
		private void UpdateLocations(GameTime time)
		{
			Game1.loopingLocationCues.Update(Game1.currentLocation);
			if (Game1.IsClient)
			{
				Game1.currentLocation.UpdateWhenCurrentLocation(time);
				using (IEnumerator<GameLocation> enumerator = Game1.multiplayer.activeLocations().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						GameLocation gameLocation = enumerator.Current;
						gameLocation.updateEvenIfFarmerIsntHere(time, false);
					}
					return;
				}
			}
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				this._UpdateLocation(location, time);
				return true;
			}, true, false);
			if (Game1.currentLocation.IsTemporary)
			{
				this._UpdateLocation(Game1.currentLocation, time);
			}
			MineShaft.UpdateMines(time);
			VolcanoDungeon.UpdateLevels(time);
		}

		// Token: 0x06000B38 RID: 2872 RVA: 0x00077390 File Offset: 0x00075590
		protected void _UpdateLocation(GameLocation location, GameTime time)
		{
			bool shouldUpdate = location.farmers.Any();
			if (!shouldUpdate && location.CanBeRemotedlyViewed())
			{
				if (Game1.player.currentLocation == location)
				{
					shouldUpdate = true;
				}
				else
				{
					foreach (Farmer who in Game1.otherFarmers.Values)
					{
						if (who.viewingLocation.Value != null && who.viewingLocation.Value.Equals(location.NameOrUniqueName))
						{
							shouldUpdate = true;
							break;
						}
					}
				}
			}
			if (shouldUpdate)
			{
				location.UpdateWhenCurrentLocation(time);
			}
			location.updateEvenIfFarmerIsntHere(time, false);
			if (location.wasInhabited != shouldUpdate)
			{
				location.wasInhabited = shouldUpdate;
				if (Game1.IsMasterGame)
				{
					location.cleanupForVacancy();
				}
			}
		}

		// Token: 0x06000B39 RID: 2873 RVA: 0x0007745C File Offset: 0x0007565C
		public static void performTenMinuteClockUpdate()
		{
			Game1.hooks.OnGame1_PerformTenMinuteClockUpdate(delegate
			{
				int startToGetReallyDark = Game1.getTrulyDarkTime(Game1.currentLocation) - 100;
				Game1.gameTimeInterval = 0;
				if (Game1.IsMasterGame)
				{
					Game1.timeOfDay += 10;
				}
				if (Game1.timeOfDay % 100 >= 60)
				{
					Game1.timeOfDay = Game1.timeOfDay - Game1.timeOfDay % 100 + 100;
				}
				Game1.timeOfDay = Math.Min(Game1.timeOfDay, 2600);
				if (Game1.isLightning && Game1.timeOfDay < 2400 && Game1.IsMasterGame)
				{
					Utility.performLightningUpdate(Game1.timeOfDay);
				}
				if (Game1.timeOfDay == startToGetReallyDark)
				{
					Game1.currentLocation.switchOutNightTiles();
				}
				else if (Game1.timeOfDay == Game1.getModeratelyDarkTime(Game1.currentLocation) && Game1.currentLocation.IsOutdoors && !Game1.currentLocation.IsRainingHere())
				{
					Game1.ambientLight = Color.White;
				}
				if (!Game1.eventUp && Game1.isDarkOut(Game1.currentLocation) && Game1.IsPlayingBackgroundMusic)
				{
					Game1.changeMusicTrack("none", true, MusicContext.Default);
				}
				if (Game1.weatherIcon == 1)
				{
					Dictionary<string, string> festival_data = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + Game1.currentSeason + Game1.dayOfMonth.ToString());
					string[] split = festival_data["conditions"].Split('/', StringSplitOptions.None);
					int startTime = Convert.ToInt32(ArgUtility.SplitBySpaceAndGet(split[1], 0, null));
					if (Game1.whereIsTodaysFest == null)
					{
						Game1.whereIsTodaysFest = split[0];
					}
					if (Game1.timeOfDay == startTime)
					{
						string message;
						if (festival_data.TryGetValue("startedMessage", out message))
						{
							Game1.showGlobalMessage(TokenParser.ParseText(message, null, null, null));
						}
						else
						{
							string where;
							if (!festival_data.TryGetValue("locationDisplayName", out where))
							{
								where = split[0];
								if (!(where == "Forest"))
								{
									if (!(where == "Town"))
									{
										if (!(where == "Beach"))
										{
											LocationData data = GameLocation.GetData(where);
											where = (TokenParser.ParseText((data != null) ? data.DisplayName : null, null, null, null) ?? where);
										}
										else
										{
											where = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2639");
										}
									}
									else
									{
										where = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2637");
									}
								}
								else
								{
									where = (Game1.IsWinter ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2634") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2635"));
								}
							}
							Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2640", festival_data["name"]) + where);
						}
					}
				}
				Game1.player.performTenMinuteUpdate();
				int num = Game1.timeOfDay;
				if (num <= 2400)
				{
					if (num != 1200)
					{
						if (num != 2000)
						{
							if (num == 2400)
							{
								Game1.dayTimeMoneyBox.timeShakeTimer = 2000;
								Game1.player.doEmote(24);
								Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2652"));
							}
						}
						else if (Game1.IsPlayingTownMusic)
						{
							Game1.changeMusicTrack("none", true, MusicContext.Default);
						}
					}
					else if (Game1.currentLocation.isOutdoors.Value && !Game1.currentLocation.IsRainingHere() && (Game1.IsPlayingOutdoorsAmbience || Game1.currentSong == null || Game1.isMusicContextActiveButNotPlaying(MusicContext.Default)))
					{
						Game1.playMorningSong(false);
					}
				}
				else if (num != 2500)
				{
					if (num != 2600)
					{
						if (num == 2800)
						{
							if (Game1.activeClickableMenu != null)
							{
								Game1.activeClickableMenu.emergencyShutDown();
								Game1.exitActiveMenu();
							}
							Game1.player.startToPassOut();
							Horse mount = Game1.player.mount;
							if (mount != null)
							{
								mount.dismount(false);
							}
						}
					}
					else
					{
						Game1.dayTimeMoneyBox.timeShakeTimer = 2000;
						Horse mount2 = Game1.player.mount;
						if (mount2 != null)
						{
							mount2.dismount(false);
						}
						if (Game1.player.IsSitting())
						{
							Game1.player.StopSitting(false);
						}
						if (Game1.player.UsingTool)
						{
							FishingRod fishing_rod = Game1.player.CurrentTool as FishingRod;
							if (fishing_rod == null || (!fishing_rod.isReeling && !fishing_rod.pullingOutOfWater))
							{
								if (Game1.player.UsingTool && Game1.player.CurrentTool != null)
								{
									FishingRod rod = Game1.player.CurrentTool as FishingRod;
									if (rod != null && rod.fishCaught)
									{
										rod.doneHoldingFish(Game1.player, true);
										goto IL_47A;
									}
								}
								Game1.player.completelyStopAnimatingOrDoingAction();
							}
						}
					}
				}
				else
				{
					Game1.dayTimeMoneyBox.timeShakeTimer = 2000;
					Game1.player.doEmote(24);
				}
				IL_47A:
				foreach (string festival_id in Game1.netWorldState.Value.ActivePassiveFestivals)
				{
					PassiveFestivalData festival;
					if (Utility.TryGetPassiveFestivalData(festival_id, out festival) && Game1.timeOfDay == festival.StartTime && (!festival.OnlyShowMessageOnFirstDay || Utility.GetDayOfPassiveFestival(festival_id) == 1))
					{
						Game1.showGlobalMessage(TokenParser.ParseText(festival.StartMessage, null, null, null));
					}
				}
				foreach (GameLocation location in Game1.locations)
				{
					if (location.NameOrUniqueName == Game1.currentLocation.NameOrUniqueName)
					{
						location = Game1.currentLocation;
					}
					location.performTenMinuteUpdate(Game1.timeOfDay);
					location.timeUpdate(10);
				}
				MineShaft.UpdateMines10Minutes(Game1.timeOfDay);
				VolcanoDungeon.UpdateLevels10Minutes(Game1.timeOfDay);
				if (Game1.IsMasterGame && Game1.farmEvent == null)
				{
					Game1.netWorldState.Value.UpdateFromGame1();
				}
				Game1.currentLightSources.RemoveWhere((KeyValuePair<string, LightSource> p) => p.Value.color.A <= 0);
			});
		}

		// Token: 0x06000B3A RID: 2874 RVA: 0x00077488 File Offset: 0x00075688
		public static bool shouldPlayMorningSong(bool loading_game = false)
		{
			return !Game1.eventUp && (double)Game1.options.musicVolumeLevel > 0.025 && Game1.timeOfDay < 1200 && (loading_game || (Game1.currentSong != null && Game1.IsPlayingOutdoorsAmbience));
		}

		// Token: 0x06000B3B RID: 2875 RVA: 0x000774D8 File Offset: 0x000756D8
		public static void UpdateGameClock(GameTime time)
		{
			if (Game1.shouldTimePass(false) && !Game1.IsClient)
			{
				Game1.gameTimeInterval += time.ElapsedGameTime.Milliseconds;
			}
			if (Game1.timeOfDay >= Game1.getTrulyDarkTime(Game1.currentLocation))
			{
				int adjustedTime = (int)((float)(Game1.timeOfDay - Game1.timeOfDay % 100) + (float)(Game1.timeOfDay % 100 / 10) * 16.66f);
				float transparency = Math.Min(0.93f, 0.75f + ((float)(adjustedTime - Game1.getTrulyDarkTime(Game1.currentLocation)) + (float)Game1.gameTimeInterval / (float)Game1.realMilliSecondsPerGameTenMinutes * 16.6f) * 0.000625f);
				Game1.outdoorLight = (Game1.IsRainingHere(null) ? Game1.ambientLight : Game1.eveningColor) * transparency;
			}
			else if (Game1.timeOfDay >= Game1.getStartingToGetDarkTime(Game1.currentLocation))
			{
				int adjustedTime2 = (int)((float)(Game1.timeOfDay - Game1.timeOfDay % 100) + (float)(Game1.timeOfDay % 100 / 10) * 16.66f);
				float transparency2 = Math.Min(0.93f, 0.3f + ((float)(adjustedTime2 - Game1.getStartingToGetDarkTime(Game1.currentLocation)) + (float)Game1.gameTimeInterval / (float)Game1.realMilliSecondsPerGameTenMinutes * 16.6f) * 0.00225f);
				Game1.outdoorLight = (Game1.IsRainingHere(null) ? Game1.ambientLight : Game1.eveningColor) * transparency2;
			}
			else if (Game1.IsRainingHere(null))
			{
				Game1.outdoorLight = Game1.ambientLight * 0.3f;
			}
			else
			{
				Game1.outdoorLight = Game1.ambientLight;
			}
			int num = Game1.gameTimeInterval;
			int num2 = Game1.realMilliSecondsPerGameTenMinutes;
			GameLocation currentLocation = Game1.currentLocation;
			int? num3 = num2 + ((currentLocation != null) ? new int?(currentLocation.ExtraMillisecondsPerInGameMinute * 10) : null);
			if (num > num3.GetValueOrDefault() & num3 != null)
			{
				if (Game1.panMode)
				{
					Game1.gameTimeInterval = 0;
					return;
				}
				Game1.performTenMinuteClockUpdate();
			}
		}

		// Token: 0x06000B3C RID: 2876 RVA: 0x000776D8 File Offset: 0x000758D8
		public static Event getAvailableWeddingEvent()
		{
			if (Game1.weddingsToday.Count <= 0)
			{
				return null;
			}
			long id = Game1.weddingsToday[0];
			Game1.weddingsToday.RemoveAt(0);
			Farmer farmer = Game1.GetPlayer(id, false);
			if (farmer == null)
			{
				return null;
			}
			if (farmer.hasRoommate())
			{
				return null;
			}
			Event wedding_event;
			if (farmer.spouse != null)
			{
				wedding_event = Utility.getWeddingEvent(farmer);
			}
			else
			{
				long? spouseID = farmer.team.GetSpouse(farmer.UniqueMultiplayerID);
				Farmer spouse = Game1.GetPlayer(spouseID.Value, false);
				if (spouse == null)
				{
					return null;
				}
				if (!Game1.getOnlineFarmers().Contains(farmer) || !Game1.getOnlineFarmers().Contains(spouse))
				{
					return null;
				}
				Game1.player.team.GetFriendship(farmer.UniqueMultiplayerID, spouseID.Value).Status = FriendshipStatus.Married;
				Game1.player.team.GetFriendship(farmer.UniqueMultiplayerID, spouseID.Value).WeddingDate = new WorldDate(Game1.Date);
				wedding_event = Utility.getWeddingEvent(farmer);
			}
			return wedding_event;
		}

		// Token: 0x06000B3D RID: 2877 RVA: 0x000777CD File Offset: 0x000759CD
		public static void exitActiveMenu()
		{
			Game1.activeClickableMenu = null;
		}

		// Token: 0x06000B3E RID: 2878 RVA: 0x000777D5 File Offset: 0x000759D5
		public static void PerformActionWhenPlayerFree(Action action)
		{
			if (Game1.player.IsBusyDoingSomething())
			{
				Game1.actionsWhenPlayerFree.Add(action);
				return;
			}
			action();
		}

		// Token: 0x06000B3F RID: 2879 RVA: 0x000777F5 File Offset: 0x000759F5
		public static void fadeScreenToBlack()
		{
			Game1.screenFade.FadeScreenToBlack(0f, true);
		}

		// Token: 0x06000B40 RID: 2880 RVA: 0x00077807 File Offset: 0x00075A07
		public static void fadeClear()
		{
			Game1.screenFade.FadeClear(1f);
		}

		// Token: 0x06000B41 RID: 2881 RVA: 0x00077818 File Offset: 0x00075A18
		private bool onFadeToBlackComplete()
		{
			bool should_halt = false;
			if (Game1.killScreen)
			{
				Game1.viewportFreeze = true;
				Game1.viewport.X = -10000;
			}
			if (Game1.exitToTitle)
			{
				Game1.setGameMode(4);
				Game1.fadeIn = false;
				Game1.fadeToBlack = true;
				Game1.fadeToBlackAlpha = 0.01f;
				Game1.exitToTitle = false;
				Game1.changeMusicTrack("none", false, MusicContext.Default);
				Game1.debrisWeather.Clear();
				return true;
			}
			if (Game1.timeOfDayAfterFade != -1)
			{
				Game1.timeOfDay = Game1.timeOfDayAfterFade;
				Game1.timeOfDayAfterFade = -1;
			}
			if (!Game1.nonWarpFade && Game1.locationRequest != null)
			{
				if (Game1.IsMasterGame && Game1.locationRequest.Location == null)
				{
					Game1.log.Error("Warp to " + Game1.locationRequest.Name + " failed: location wasn't found or couldn't be loaded.", null);
					Game1.locationRequest = null;
				}
				if (Game1.locationRequest != null)
				{
					GameLocation previousLocation = Game1.currentLocation;
					EmoteMenu emoteMenu = Game1.emoteMenu;
					if (emoteMenu != null)
					{
						emoteMenu.exitThisMenuNoSound();
					}
					if (Game1.client != null)
					{
						GameLocation currentLocation = Game1.currentLocation;
						if (currentLocation != null)
						{
							currentLocation.StoreCachedMultiplayerMap(Game1.multiplayer.cachedMultiplayerMaps);
						}
					}
					Game1.currentLocation.cleanupBeforePlayerExit();
					Game1.multiplayer.broadcastLocationDelta(Game1.currentLocation);
					bool hasResetLocation = false;
					Game1.displayFarmer = true;
					if (Game1.eventOver)
					{
						Game1.eventFinished();
						if (Game1.dayOfMonth == 0)
						{
							Game1.newDayAfterFade(delegate
							{
								Game1.player.Position = new Vector2(320f, 320f);
							});
						}
						return true;
					}
					if (Game1.locationRequest.IsRequestFor(Game1.currentLocation) && Game1.player.previousLocationName != "" && !Game1.eventUp && !MineShaft.IsGeneratedLevel(Game1.currentLocation))
					{
						Game1.player.Position = new Vector2((float)(Game1.xLocationAfterWarp * 64), (float)(Game1.yLocationAfterWarp * 64 - (Game1.player.Sprite.getHeight() - 32) + 16));
						Game1.viewportFreeze = false;
						Game1.currentLocation.resetForPlayerEntry();
						hasResetLocation = true;
					}
					else
					{
						if (MineShaft.IsGeneratedLevel(Game1.locationRequest.Name))
						{
							MineShaft mine = Game1.locationRequest.Location as MineShaft;
							if (Game1.player.IsSitting())
							{
								Game1.player.StopSitting(false);
							}
							Game1.player.Halt();
							Game1.player.forceCanMove();
							if (Game1.IsClient)
							{
								GameLocation location = Game1.locationRequest.Location;
								if (!(((location != null) ? location.Root : null) != null))
								{
									goto IL_26D;
								}
							}
							Game1.currentLocation = mine;
							mine.resetForPlayerEntry();
							hasResetLocation = true;
							IL_26D:
							Game1.currentLocation.Map.LoadTileSheets(Game1.mapDisplayDevice);
							Game1.checkForRunButton(Game1.GetKeyboardState(), false);
						}
						if (!Game1.eventUp)
						{
							Game1.player.Position = new Vector2((float)(Game1.xLocationAfterWarp * 64), (float)(Game1.yLocationAfterWarp * 64 - (Game1.player.Sprite.getHeight() - 32) + 16));
						}
						if (!MineShaft.IsGeneratedLevel(Game1.locationRequest.Name) && Game1.locationRequest.Location != null)
						{
							Game1.currentLocation = Game1.locationRequest.Location;
							if (!Game1.IsClient)
							{
								Game1.locationRequest.Loaded(Game1.locationRequest.Location);
								Game1.currentLocation.resetForPlayerEntry();
								hasResetLocation = true;
							}
							Game1.currentLocation.Map.LoadTileSheets(Game1.mapDisplayDevice);
							if (!Game1.viewportFreeze && Game1.currentLocation.Map.DisplayWidth <= Game1.viewport.Width)
							{
								Game1.viewport.X = (Game1.currentLocation.Map.DisplayWidth - Game1.viewport.Width) / 2;
							}
							if (!Game1.viewportFreeze && Game1.currentLocation.Map.DisplayHeight <= Game1.viewport.Height)
							{
								Game1.viewport.Y = (Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height) / 2;
							}
							Game1.checkForRunButton(Game1.GetKeyboardState(), true);
						}
						if (!Game1.eventUp)
						{
							Game1.viewportFreeze = false;
						}
					}
					Game1.forceSnapOnNextViewportUpdate = true;
					Game1.player.FarmerSprite.PauseForSingleAnimation = false;
					Game1.player.faceDirection(Game1.facingDirectionAfterWarp);
					Game1._isWarping = false;
					if (Game1.player.ActiveObject != null)
					{
						Game1.player.showCarrying();
					}
					else
					{
						Game1.player.showNotCarrying();
					}
					if (Game1.IsClient)
					{
						if (Game1.locationRequest.Location != null && Game1.locationRequest.Location.Root != null && Game1.multiplayer.isActiveLocation(Game1.locationRequest.Location))
						{
							if (Game1.HasDedicatedHost)
							{
								Game1.notifyServerOfWarp(false);
							}
							Game1.currentLocation = Game1.locationRequest.Location;
							Game1.locationRequest.Loaded(Game1.locationRequest.Location);
							if (!hasResetLocation)
							{
								Game1.currentLocation.resetForPlayerEntry();
							}
							Game1.player.currentLocation = Game1.currentLocation;
							Game1.locationRequest.Warped(Game1.currentLocation);
							Game1.currentLocation.updateSeasonalTileSheets(null);
							if (Game1.IsDebrisWeatherHere(null))
							{
								Game1.populateDebrisWeatherArray();
							}
							Game1.warpingForForcedRemoteEvent = false;
							Game1.locationRequest = null;
						}
						else
						{
							Game1.requestLocationInfoFromServer();
							if (Game1.currentLocation == null)
							{
								return true;
							}
						}
					}
					else
					{
						Game1.player.currentLocation = Game1.locationRequest.Location;
						Game1.locationRequest.Warped(Game1.locationRequest.Location);
						Game1.locationRequest = null;
					}
					if (Game1.locationRequest == null && Game1.currentLocation.Name == "Farm" && !Game1.eventUp)
					{
						if (Game1.player.position.X / 64f >= (float)(Game1.currentLocation.map.Layers[0].LayerWidth - 1))
						{
							Game1.player.position.X -= 64f;
						}
						else if (Game1.player.position.Y / 64f >= (float)(Game1.currentLocation.map.Layers[0].LayerHeight - 1))
						{
							Game1.player.position.Y -= 32f;
						}
						if (Game1.player.position.Y / 64f >= (float)(Game1.currentLocation.map.Layers[0].LayerHeight - 2))
						{
							Game1.player.position.X -= 48f;
						}
					}
					if (MineShaft.IsGeneratedLevel(previousLocation) && Game1.currentLocation != null && !MineShaft.IsGeneratedLevel(Game1.currentLocation))
					{
						MineShaft.OnLeftMines();
					}
					Game1.player.OnWarp();
					should_halt = true;
				}
			}
			if (Game1.newDay)
			{
				Game1.newDayAfterFade(new Action(Game1.<onFadeToBlackComplete>g__AfterNewDay|746_0));
				return true;
			}
			if (Game1.eventOver)
			{
				Game1.eventFinished();
				if (Game1.dayOfMonth == 0)
				{
					Game1.newDayAfterFade(new Action(Game1.<onFadeToBlackComplete>g__AfterEventOver|746_1));
				}
				return true;
			}
			ICue currentSong = Game1.currentSong;
			if (((currentSong != null) ? currentSong.Name : null) == "rain" && Game1.currentLocation.IsRainingHere())
			{
				if (Game1.currentLocation.IsOutdoors)
				{
					Game1.currentSong.SetVariable("Frequency", 100f);
				}
				else if (!MineShaft.IsGeneratedLevel(Game1.currentLocation.Name))
				{
					Game1.currentSong.SetVariable("Frequency", 15f);
				}
			}
			return should_halt;
		}

		// Token: 0x06000B42 RID: 2882 RVA: 0x00077F54 File Offset: 0x00076154
		public static void OnLocationChanged(GameLocation oldLocation, GameLocation newLocation)
		{
			if (!Game1.hasLoadedGame)
			{
				return;
			}
			Game1.eventsSeenSinceLastLocationChange.Clear();
			if (newLocation.Name != null && !MineShaft.IsGeneratedLevel(newLocation) && !VolcanoDungeon.IsGeneratedLevel(newLocation.Name))
			{
				Game1.player.locationsVisited.Add(newLocation.Name);
			}
			if (newLocation.IsOutdoors && !newLocation.ignoreDebrisWeather.Value && newLocation.IsDebrisWeatherHere())
			{
				Season seasonForLocation = Game1.GetSeasonForLocation(newLocation);
				Season? season = Game1.debrisWeatherSeason;
				if (!(seasonForLocation == season.GetValueOrDefault() & season != null))
				{
					Game1.windGust = 0f;
					WeatherDebris.globalWind = 0f;
					Game1.populateDebrisWeatherArray();
					if (Game1.wind != null)
					{
						Game1.wind.Stop(AudioStopOptions.AsAuthored);
						Game1.wind = null;
					}
				}
			}
			GameLocation.HandleMusicChange(oldLocation, newLocation);
			TriggerActionManager.Raise("LocationChanged", null, null, null, null, null);
		}

		// Token: 0x06000B43 RID: 2883 RVA: 0x0007802C File Offset: 0x0007622C
		private static void onFadedBackInComplete()
		{
			if (Game1.killScreen)
			{
				Game1.pauseThenMessage(1500, "..." + Game1.player.Name + "?");
			}
			else if (!Game1.eventUp)
			{
				Game1.player.CanMove = true;
			}
			Game1.checkForRunButton(Game1.oldKBState, true);
		}

		// Token: 0x06000B44 RID: 2884 RVA: 0x00078084 File Offset: 0x00076284
		public static void UpdateOther(GameTime time)
		{
			if (Game1.currentLocation == null)
			{
				return;
			}
			if (!Game1.player.passedOut && Game1.screenFade.UpdateFade(time))
			{
				return;
			}
			if (Game1.dialogueUp)
			{
				Game1.player.CanMove = false;
			}
			for (int i = Game1.delayedActions.Count - 1; i >= 0; i--)
			{
				DelayedAction action = Game1.delayedActions[i];
				if (action.update(time) && Game1.delayedActions.Contains(action))
				{
					Game1.delayedActions.Remove(action);
				}
			}
			if (Game1.timeOfDay >= 2600 || Game1.player.stamina <= -15f)
			{
				if (Game1.currentMinigame != null && Game1.currentMinigame.forceQuit())
				{
					Game1.currentMinigame = null;
				}
				if (Game1.currentMinigame == null && Game1.player.canMove && Game1.player.freezePause <= 0 && !Game1.player.UsingTool && !Game1.eventUp && (Game1.IsMasterGame || Game1.player.isCustomized.Value) && Game1.locationRequest == null && Game1.activeClickableMenu == null)
				{
					Game1.player.startToPassOut();
					Game1.player.freezePause = 7000;
				}
			}
			Game1.screenOverlayTempSprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.update(time));
			Game1.uiOverlayTempSprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.update(time));
			if ((Game1.player.CanMove || Game1.player.UsingTool) && Game1.shouldTimePass(false))
			{
				Game1.buffsDisplay.update(time);
			}
			Item currentItem = Game1.player.CurrentItem;
			if (currentItem != null)
			{
				currentItem.actionWhenBeingHeld(Game1.player);
			}
			float tmp = Game1.dialogueButtonScale;
			Game1.dialogueButtonScale = (float)(16.0 * Math.Sin(time.TotalGameTime.TotalMilliseconds % 1570.0 / 500.0));
			if (tmp > Game1.dialogueButtonScale && !Game1.dialogueButtonShrinking)
			{
				Game1.dialogueButtonShrinking = true;
			}
			else if (tmp < Game1.dialogueButtonScale && Game1.dialogueButtonShrinking)
			{
				Game1.dialogueButtonShrinking = false;
			}
			if (Game1.screenGlow)
			{
				if (Game1.screenGlowUp || Game1.screenGlowHold)
				{
					if (Game1.screenGlowHold)
					{
						Game1.screenGlowAlpha = Math.Min(Game1.screenGlowAlpha + Game1.screenGlowRate, Game1.screenGlowMax);
					}
					else
					{
						Game1.screenGlowAlpha = Math.Min(Game1.screenGlowAlpha + 0.03f, 0.6f);
						if (Game1.screenGlowAlpha >= 0.6f)
						{
							Game1.screenGlowUp = false;
						}
					}
				}
				else
				{
					Game1.screenGlowAlpha -= 0.01f;
					if (Game1.screenGlowAlpha <= 0f)
					{
						Game1.screenGlow = false;
					}
				}
			}
			Game1.hudMessages.RemoveAll((HUDMessage hudMessage) => hudMessage.update(time));
			Game1.updateWeather(time);
			if (!Game1.fadeToBlack)
			{
				Game1.currentLocation.checkForMusic(time);
			}
			if (Game1.debrisSoundInterval > 0f)
			{
				Game1.debrisSoundInterval -= (float)time.ElapsedGameTime.Milliseconds;
			}
			Game1.noteBlockTimer += (float)time.ElapsedGameTime.Milliseconds;
			if (Game1.noteBlockTimer > 1000f)
			{
				Game1.noteBlockTimer = 0f;
				if (Game1.player.health < 20 && Game1.CurrentEvent == null)
				{
					Game1.hitShakeTimer = 250;
					if (Game1.player.health <= 10)
					{
						Game1.hitShakeTimer = 500;
						if (Game1.showingHealthBar && Game1.fadeToBlackAlpha <= 0f)
						{
							for (int j = 0; j < 3; j++)
							{
								Game1.uiOverlayTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(366, 412, 5, 6), new Vector2((float)(Game1.random.Next(32) + Game1.uiViewport.Width - 112), (float)(Game1.uiViewport.Height - 224 - (Game1.player.maxHealth - 100) - 16 + 4)), false, 0.017f, Color.Red)
								{
									motion = new Vector2(-1.5f, (float)(-8 + Game1.random.Next(-1, 2))),
									acceleration = new Vector2(0f, 0.5f),
									local = true,
									scale = 4f,
									delayBeforeAnimationStart = j * 150
								});
							}
						}
					}
				}
			}
			Game1.drawLighting = ((Game1.currentLocation.IsOutdoors && !Game1.outdoorLight.Equals(Color.White)) || !Game1.ambientLight.Equals(Color.White) || (Game1.currentLocation is MineShaft && !((MineShaft)Game1.currentLocation).getLightingColor(time).Equals(Color.White)));
			if (Game1.player.hasBuff("26"))
			{
				Game1.drawLighting = true;
			}
			if (Game1.hitShakeTimer > 0)
			{
				Game1.hitShakeTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (Game1.staminaShakeTimer > 0)
			{
				Game1.staminaShakeTimer -= time.ElapsedGameTime.Milliseconds;
			}
			Background background = Game1.background;
			if (background != null)
			{
				background.update(Game1.viewport);
			}
			Game1.cursorTileHintCheckTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			Game1.currentCursorTile.X = (float)((Game1.viewport.X + Game1.getOldMouseX()) / 64);
			Game1.currentCursorTile.Y = (float)((Game1.viewport.Y + Game1.getOldMouseY()) / 64);
			if (Game1.cursorTileHintCheckTimer <= 0 || !Game1.currentCursorTile.Equals(Game1.lastCursorTile))
			{
				Game1.cursorTileHintCheckTimer = 250;
				Game1.updateCursorTileHint();
				if (Game1.player.CanMove)
				{
					Game1.checkForRunButton(Game1.oldKBState, true);
				}
			}
			if (!MineShaft.IsGeneratedLevel(Game1.currentLocation.Name))
			{
				MineShaft.timeSinceLastMusic = 200000;
			}
			if (Game1.activeClickableMenu == null && Game1.farmEvent == null && Game1.keyboardDispatcher != null && !Game1.IsChatting)
			{
				Game1.keyboardDispatcher.Subscriber = null;
			}
		}

		// Token: 0x06000B45 RID: 2885 RVA: 0x000786DC File Offset: 0x000768DC
		public static void updateWeather(GameTime time)
		{
			if (Game1.currentLocation.IsOutdoors && Game1.currentLocation.IsSnowingHere())
			{
				Vector2 currentViewport = new Vector2((float)Game1.viewport.X, (float)Game1.viewport.Y);
				Game1.snowPos = Game1.updateFloatingObjectPositionForMovement(Game1.snowPos, currentViewport, Game1.previousViewportPosition, -1f);
				return;
			}
			if (Game1.currentLocation.IsOutdoors && Game1.currentLocation.IsRainingHere())
			{
				for (int i = 0; i < Game1.rainDrops.Length; i++)
				{
					if (Game1.rainDrops[i].frame == 0)
					{
						RainDrop[] array = Game1.rainDrops;
						int num = i;
						array[num].accumulator = array[num].accumulator + time.ElapsedGameTime.Milliseconds;
						if (Game1.rainDrops[i].accumulator >= 70)
						{
							RainDrop[] array2 = Game1.rainDrops;
							int num2 = i;
							array2[num2].position = array2[num2].position + new Vector2((float)(-16 + i * 8 / Game1.rainDrops.Length), (float)(32 - i * 8 / Game1.rainDrops.Length));
							Game1.rainDrops[i].accumulator = 0;
							if (Game1.random.NextDouble() < 0.1)
							{
								RainDrop[] array3 = Game1.rainDrops;
								int num3 = i;
								array3[num3].frame = array3[num3].frame + 1;
							}
							if (Game1.currentLocation is IslandNorth || Game1.currentLocation is Caldera)
							{
								Point p = new Point((int)(Game1.rainDrops[i].position.X + (float)Game1.viewport.X) / 64, (int)(Game1.rainDrops[i].position.Y + (float)Game1.viewport.Y) / 64);
								p.Y--;
								if (Game1.currentLocation.isTileOnMap(p.X, p.Y) && !Game1.currentLocation.hasTileAt(p, "Back", null) && !Game1.currentLocation.hasTileAt(p, "Buildings", null))
								{
									Game1.rainDrops[i].frame = 0;
								}
							}
							if (Game1.rainDrops[i].position.Y > (float)(Game1.viewport.Height + 64))
							{
								Game1.rainDrops[i].position.Y = -64f;
							}
						}
					}
					else
					{
						RainDrop[] array4 = Game1.rainDrops;
						int num4 = i;
						array4[num4].accumulator = array4[num4].accumulator + time.ElapsedGameTime.Milliseconds;
						if (Game1.rainDrops[i].accumulator > 70)
						{
							Game1.rainDrops[i].frame = (Game1.rainDrops[i].frame + 1) % 4;
							Game1.rainDrops[i].accumulator = 0;
							if (Game1.rainDrops[i].frame == 0)
							{
								Game1.rainDrops[i].position = new Vector2((float)Game1.random.Next(Game1.viewport.Width), (float)Game1.random.Next(Game1.viewport.Height));
							}
						}
					}
				}
				return;
			}
			if (Game1.currentLocation.IsOutdoors && !Game1.currentLocation.ignoreDebrisWeather.Value && Game1.currentLocation.IsDebrisWeatherHere())
			{
				if (Game1.currentLocation.GetSeason() == Season.Fall)
				{
					if (WeatherDebris.globalWind == 0f)
					{
						WeatherDebris.globalWind = -0.5f;
					}
					if (Game1.random.NextDouble() < 0.001 && Game1.windGust == 0f && WeatherDebris.globalWind >= -0.5f)
					{
						Game1.windGust += (float)Game1.random.Next(-10, -1) / 100f;
						Game1.playSound("wind", out Game1.wind);
					}
					else if (Game1.windGust != 0f)
					{
						Game1.windGust = Math.Max(-5f, Game1.windGust * 1.02f);
						WeatherDebris.globalWind = -0.5f + Game1.windGust;
						if (Game1.windGust < -0.2f && Game1.random.NextDouble() < 0.007)
						{
							Game1.windGust = 0f;
						}
					}
					if (WeatherDebris.globalWind < -0.5f)
					{
						WeatherDebris.globalWind = Math.Min(-0.5f, WeatherDebris.globalWind + 0.015f);
						if (Game1.wind != null)
						{
							Game1.wind.SetVariable("Volume", -WeatherDebris.globalWind * 20f);
							Game1.wind.SetVariable("Frequency", -WeatherDebris.globalWind * 20f);
							if (WeatherDebris.globalWind == -0.5f)
							{
								Game1.wind.Stop(AudioStopOptions.AsAuthored);
							}
						}
					}
				}
				else
				{
					if (WeatherDebris.globalWind == 0f)
					{
						WeatherDebris.globalWind = -0.25f;
					}
					if (Game1.wind != null)
					{
						Game1.wind.Stop(AudioStopOptions.AsAuthored);
						Game1.wind = null;
					}
				}
				using (List<WeatherDebris>.Enumerator enumerator = Game1.debrisWeather.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						WeatherDebris weatherDebris = enumerator.Current;
						weatherDebris.update();
					}
					return;
				}
			}
			if (Game1.wind != null)
			{
				Game1.wind.Stop(AudioStopOptions.AsAuthored);
				Game1.wind = null;
			}
		}

		// Token: 0x06000B46 RID: 2886 RVA: 0x00078C28 File Offset: 0x00076E28
		public static void updateCursorTileHint()
		{
			if (Game1.activeClickableMenu == null)
			{
				Game1.mouseCursorTransparency = 1f;
				Game1.isActionAtCurrentCursorTile = false;
				Game1.isInspectionAtCurrentCursorTile = false;
				Game1.isSpeechAtCurrentCursorTile = false;
				int xTile = (Game1.viewport.X + Game1.getOldMouseX()) / 64;
				int yTile = (Game1.viewport.Y + Game1.getOldMouseY()) / 64;
				if (Game1.currentLocation != null)
				{
					Game1.isActionAtCurrentCursorTile = Game1.currentLocation.isActionableTile(xTile, yTile, Game1.player);
					if (!Game1.isActionAtCurrentCursorTile)
					{
						Game1.isActionAtCurrentCursorTile = Game1.currentLocation.isActionableTile(xTile, yTile + 1, Game1.player);
					}
				}
				Game1.lastCursorTile = Game1.currentCursorTile;
			}
		}

		// Token: 0x06000B47 RID: 2887 RVA: 0x00078CCC File Offset: 0x00076ECC
		public static void updateMusic()
		{
			if (Game1.game1.IsMainInstance)
			{
				Game1 important_music_instance = null;
				string important_instance_music = null;
				int num = 0;
				int sub_location_priority = 1;
				int non_ambient_world_priority = 2;
				int minigame_priority = 5;
				int event_priority = 6;
				int mermaid_show = 7;
				int priority = num;
				float default_context_priority = (float)Game1.GetDefaultSongPriority(Game1.getMusicTrackName(MusicContext.Default), Game1.game1.instanceIsOverridingTrack, Game1.game1);
				MusicContext primary_music_context = MusicContext.Default;
				foreach (Game1 instance in GameRunner.instance.gameInstances)
				{
					MusicContext active_context = instance._instanceActiveMusicContext;
					if (instance.IsMainInstance)
					{
						primary_music_context = active_context;
					}
					string track_name = null;
					string actual_track_name = null;
					KeyValuePair<string, bool> trackData;
					if (instance._instanceRequestedMusicTracks.TryGetValue(active_context, out trackData))
					{
						track_name = trackData.Key;
					}
					if (instance.instanceIsOverridingTrack && instance.instanceCurrentSong != null)
					{
						actual_track_name = instance.instanceCurrentSong.Name;
					}
					switch (active_context)
					{
					case MusicContext.Default:
						if (track_name == "mermaidSong")
						{
							priority = mermaid_show;
							important_music_instance = instance;
							important_instance_music = track_name;
						}
						if (primary_music_context <= active_context && track_name != null)
						{
							float instance_default_context_priority = (float)Game1.GetDefaultSongPriority(track_name, instance.instanceIsOverridingTrack, instance);
							if (default_context_priority < instance_default_context_priority)
							{
								default_context_priority = instance_default_context_priority;
								priority = non_ambient_world_priority;
								important_music_instance = instance;
								if (actual_track_name != null)
								{
									important_instance_music = actual_track_name;
								}
								else
								{
									important_instance_music = track_name;
								}
							}
						}
						break;
					case MusicContext.SubLocation:
						if (priority < sub_location_priority && track_name != null)
						{
							priority = sub_location_priority;
							important_music_instance = instance;
							if (actual_track_name != null)
							{
								important_instance_music = actual_track_name;
							}
							else
							{
								important_instance_music = track_name;
							}
						}
						break;
					case MusicContext.Event:
						if (priority < event_priority && track_name != null)
						{
							priority = event_priority;
							important_music_instance = instance;
							important_instance_music = track_name;
						}
						break;
					case MusicContext.MiniGame:
						if (priority < minigame_priority && track_name != null)
						{
							priority = minigame_priority;
							important_music_instance = instance;
							important_instance_music = track_name;
						}
						break;
					}
				}
				if (important_music_instance == null || important_music_instance == Game1.game1)
				{
					if (Game1.doesMusicContextHaveTrack(MusicContext.ImportantSplitScreenMusic))
					{
						Game1.stopMusicTrack(MusicContext.ImportantSplitScreenMusic);
					}
				}
				else if (important_instance_music == null && Game1.doesMusicContextHaveTrack(MusicContext.ImportantSplitScreenMusic))
				{
					Game1.stopMusicTrack(MusicContext.ImportantSplitScreenMusic);
				}
				else if (important_instance_music != null && Game1.getMusicTrackName(MusicContext.ImportantSplitScreenMusic) != important_instance_music)
				{
					Game1.changeMusicTrack(important_instance_music, false, MusicContext.ImportantSplitScreenMusic);
				}
			}
			string song_to_play = null;
			bool track_overrideable = false;
			bool song_overridden = false;
			if (Game1.currentLocation != null && Game1.currentLocation.IsMiniJukeboxPlaying() && (!Game1.requestedMusicDirty || Game1.requestedMusicTrackOverrideable) && Game1.currentTrackOverrideable)
			{
				song_to_play = null;
				song_overridden = true;
				string mini_jukebox_track = Game1.currentLocation.miniJukeboxTrack.Value;
				if (mini_jukebox_track == "random")
				{
					if (Game1.currentLocation.randomMiniJukeboxTrack.Value == null)
					{
						mini_jukebox_track = "";
					}
					else
					{
						mini_jukebox_track = Game1.currentLocation.randomMiniJukeboxTrack.Value;
					}
				}
				if (Game1.currentSong == null || !Game1.currentSong.IsPlaying || Game1.currentSong.Name != mini_jukebox_track)
				{
					if (!Game1.soundBank.Exists(mini_jukebox_track))
					{
						IGameLogger gameLogger = Game1.log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(69, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Location ");
						defaultInterpolatedStringHandler.AppendFormatted(Game1.currentLocation.NameOrUniqueName);
						defaultInterpolatedStringHandler.AppendLiteral(" has invalid jukebox track '");
						defaultInterpolatedStringHandler.AppendFormatted(mini_jukebox_track);
						defaultInterpolatedStringHandler.AppendLiteral("' selected, turning off jukebox.");
						gameLogger.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
						Game1.player.currentLocation.miniJukeboxTrack.Value = "";
					}
					else
					{
						song_to_play = mini_jukebox_track;
						Game1.requestedMusicDirty = false;
						track_overrideable = true;
					}
				}
			}
			if (Game1.isOverridingTrack != song_overridden)
			{
				Game1.isOverridingTrack = song_overridden;
				if (!Game1.isOverridingTrack)
				{
					Game1.requestedMusicDirty = true;
				}
			}
			if (Game1.requestedMusicDirty)
			{
				song_to_play = Game1.requestedMusicTrack;
				track_overrideable = Game1.requestedMusicTrackOverrideable;
			}
			if (!string.IsNullOrEmpty(song_to_play))
			{
				Game1.musicPlayerVolume = Math.Max(0f, Math.Min(Game1.options.musicVolumeLevel, Game1.musicPlayerVolume - 0.01f));
				Game1.ambientPlayerVolume = Math.Max(0f, Math.Min(Game1.options.musicVolumeLevel, Game1.ambientPlayerVolume - 0.01f));
				if (Game1.game1.IsMainInstance)
				{
					Game1.musicCategory.SetVolume(Game1.musicPlayerVolume);
					Game1.ambientCategory.SetVolume(Game1.ambientPlayerVolume);
				}
				if (Game1.musicPlayerVolume == 0f && Game1.ambientPlayerVolume == 0f)
				{
					if (song_to_play == "none" || song_to_play == "silence")
					{
						if (Game1.game1.IsMainInstance && Game1.currentSong != null)
						{
							Game1.currentSong.Stop(AudioStopOptions.Immediate);
							Game1.currentSong.Dispose();
							Game1.currentSong = null;
						}
					}
					else if ((Game1.options.musicVolumeLevel != 0f || Game1.options.ambientVolumeLevel != 0f) && (song_to_play != "rain" || Game1.endOfNightMenus.Count == 0))
					{
						if (Game1.game1.IsMainInstance && Game1.currentSong != null)
						{
							Game1.currentSong.Stop(AudioStopOptions.Immediate);
							Game1.currentSong.Dispose();
							Game1.currentSong = null;
						}
						Game1.currentSong = Game1.soundBank.GetCue(song_to_play);
						if (Game1.game1.IsMainInstance)
						{
							Game1.currentSong.Play();
						}
						if (Game1.game1.IsMainInstance && Game1.currentSong != null && Game1.currentSong.Name == "rain" && Game1.currentLocation != null)
						{
							if (Game1.IsRainingHere(null))
							{
								if (Game1.currentLocation.IsOutdoors)
								{
									Game1.currentSong.SetVariable("Frequency", 100f);
								}
								else if (!MineShaft.IsGeneratedLevel(Game1.currentLocation))
								{
									Game1.currentSong.SetVariable("Frequency", 15f);
								}
							}
							else if (Game1.eventUp)
							{
								Game1.currentSong.SetVariable("Frequency", 100f);
							}
						}
					}
					else
					{
						ICue currentSong = Game1.currentSong;
						if (currentSong != null)
						{
							currentSong.Stop(AudioStopOptions.Immediate);
						}
					}
					Game1.currentTrackOverrideable = track_overrideable;
					Game1.requestedMusicDirty = false;
					return;
				}
			}
			else if (Game1.MusicDuckTimer > 0f)
			{
				Game1.MusicDuckTimer -= (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
				Game1.musicPlayerVolume = Math.Max(Game1.musicPlayerVolume - Game1.options.musicVolumeLevel / 33f, Game1.options.musicVolumeLevel / 12f);
				if (Game1.game1.IsMainInstance)
				{
					Game1.musicCategory.SetVolume(Game1.musicPlayerVolume);
					return;
				}
			}
			else if (Game1.musicPlayerVolume < Game1.options.musicVolumeLevel || Game1.ambientPlayerVolume < Game1.options.ambientVolumeLevel)
			{
				if (Game1.musicPlayerVolume < Game1.options.musicVolumeLevel)
				{
					Game1.musicPlayerVolume = Math.Min(1f, Game1.musicPlayerVolume += 0.01f);
					if (Game1.game1.IsMainInstance)
					{
						Game1.musicCategory.SetVolume(Game1.musicPlayerVolume);
					}
				}
				if (Game1.ambientPlayerVolume < Game1.options.ambientVolumeLevel)
				{
					Game1.ambientPlayerVolume = Math.Min(1f, Game1.ambientPlayerVolume += 0.015f);
					if (Game1.game1.IsMainInstance)
					{
						Game1.ambientCategory.SetVolume(Game1.ambientPlayerVolume);
						return;
					}
				}
			}
			else if (Game1.currentSong != null && !Game1.currentSong.IsPlaying && !Game1.currentSong.IsStopped)
			{
				Game1.currentSong = Game1.soundBank.GetCue(Game1.currentSong.Name);
				if (Game1.game1.IsMainInstance)
				{
					Game1.currentSong.Play();
				}
			}
		}

		// Token: 0x06000B48 RID: 2888 RVA: 0x00079430 File Offset: 0x00077630
		public static int GetDefaultSongPriority(string song_name, bool is_playing_override, Game1 instance)
		{
			if (is_playing_override)
			{
				return 9;
			}
			if (song_name == "none")
			{
				return 0;
			}
			if (instance._instanceIsPlayingOutdoorsAmbience || instance._instanceIsPlayingNightAmbience || song_name == "rain")
			{
				return 1;
			}
			if (instance._instanceIsPlayingMorningSong)
			{
				return 2;
			}
			if (instance._instanceIsPlayingTownMusic)
			{
				return 3;
			}
			if (song_name == "jungle_ambience")
			{
				return 7;
			}
			if (instance._instanceIsPlayingBackgroundMusic)
			{
				return 8;
			}
			if (instance.instanceGameLocation is MineShaft)
			{
				if (song_name.Contains("Ambient"))
				{
					return 7;
				}
				if (song_name.EndsWith("Mine"))
				{
					return 20;
				}
			}
			return 10;
		}

		// Token: 0x06000B49 RID: 2889 RVA: 0x000794CC File Offset: 0x000776CC
		public static void updateRainDropPositionForPlayerMovement(int direction, float speed)
		{
			if (Game1.currentLocation.IsRainingHere())
			{
				for (int i = 0; i < Game1.rainDrops.Length; i++)
				{
					switch (direction)
					{
					case 0:
					{
						RainDrop[] array = Game1.rainDrops;
						int num = i;
						array[num].position.Y = array[num].position.Y + speed;
						if (Game1.rainDrops[i].position.Y > (float)(Game1.viewport.Height + 64))
						{
							Game1.rainDrops[i].position.Y = -64f;
						}
						break;
					}
					case 1:
					{
						RainDrop[] array2 = Game1.rainDrops;
						int num2 = i;
						array2[num2].position.X = array2[num2].position.X - speed;
						if (Game1.rainDrops[i].position.X < -64f)
						{
							Game1.rainDrops[i].position.X = (float)Game1.viewport.Width;
						}
						break;
					}
					case 2:
					{
						RainDrop[] array3 = Game1.rainDrops;
						int num3 = i;
						array3[num3].position.Y = array3[num3].position.Y - speed;
						if (Game1.rainDrops[i].position.Y < -64f)
						{
							Game1.rainDrops[i].position.Y = (float)Game1.viewport.Height;
						}
						break;
					}
					case 3:
					{
						RainDrop[] array4 = Game1.rainDrops;
						int num4 = i;
						array4[num4].position.X = array4[num4].position.X + speed;
						if (Game1.rainDrops[i].position.X > (float)(Game1.viewport.Width + 64))
						{
							Game1.rainDrops[i].position.X = -64f;
						}
						break;
					}
					}
				}
				return;
			}
			Game1.updateDebrisWeatherForMovement(Game1.debrisWeather, direction, speed);
		}

		// Token: 0x06000B4A RID: 2890 RVA: 0x00079698 File Offset: 0x00077898
		public static void initializeVolumeLevels()
		{
			if (LocalMultiplayer.IsLocalMultiplayer(false) && !Game1.game1.IsMainInstance)
			{
				return;
			}
			Game1.soundCategory.SetVolume(Game1.options.soundVolumeLevel);
			Game1.musicCategory.SetVolume(Game1.options.musicVolumeLevel);
			Game1.ambientCategory.SetVolume(Game1.options.ambientVolumeLevel);
			Game1.footstepCategory.SetVolume(Game1.options.footstepVolumeLevel);
		}

		// Token: 0x06000B4B RID: 2891 RVA: 0x0007970C File Offset: 0x0007790C
		public static void updateDebrisWeatherForMovement(List<WeatherDebris> debris, int direction, float speed)
		{
			if (Game1.fadeToBlackAlpha <= 0f && debris != null)
			{
				foreach (WeatherDebris w in debris)
				{
					switch (direction)
					{
					case 0:
					{
						WeatherDebris weatherDebris = w;
						weatherDebris.position.Y = weatherDebris.position.Y + speed;
						if (w.position.Y > (float)(Game1.viewport.Height + 64))
						{
							w.position.Y = -64f;
						}
						break;
					}
					case 1:
					{
						WeatherDebris weatherDebris2 = w;
						weatherDebris2.position.X = weatherDebris2.position.X - speed;
						if (w.position.X < -64f)
						{
							w.position.X = (float)Game1.viewport.Width;
						}
						break;
					}
					case 2:
					{
						WeatherDebris weatherDebris3 = w;
						weatherDebris3.position.Y = weatherDebris3.position.Y - speed;
						if (w.position.Y < -64f)
						{
							w.position.Y = (float)Game1.viewport.Height;
						}
						break;
					}
					case 3:
					{
						WeatherDebris weatherDebris4 = w;
						weatherDebris4.position.X = weatherDebris4.position.X + speed;
						if (w.position.X > (float)(Game1.viewport.Width + 64))
						{
							w.position.X = -64f;
						}
						break;
					}
					}
				}
			}
		}

		// Token: 0x06000B4C RID: 2892 RVA: 0x0007988C File Offset: 0x00077A8C
		public static Vector2 updateFloatingObjectPositionForMovement(Vector2 w, Vector2 current, Vector2 previous, float speed)
		{
			if (current.Y < previous.Y)
			{
				w.Y -= Math.Abs(current.Y - previous.Y) * speed;
			}
			else if (current.Y > previous.Y)
			{
				w.Y += Math.Abs(current.Y - previous.Y) * speed;
			}
			if (current.X > previous.X)
			{
				w.X += Math.Abs(current.X - previous.X) * speed;
			}
			else if (current.X < previous.X)
			{
				w.X -= Math.Abs(current.X - previous.X) * speed;
			}
			return w;
		}

		// Token: 0x06000B4D RID: 2893 RVA: 0x00079954 File Offset: 0x00077B54
		public static void updateRaindropPosition()
		{
			if (Game1.HostPaused)
			{
				return;
			}
			if (Game1.IsRainingHere(null))
			{
				int xOffset = Game1.viewport.X - (int)Game1.previousViewportPosition.X;
				int yOffset = Game1.viewport.Y - (int)Game1.previousViewportPosition.Y;
				for (int i = 0; i < Game1.rainDrops.Length; i++)
				{
					RainDrop[] array = Game1.rainDrops;
					int num = i;
					array[num].position.X = array[num].position.X - (float)xOffset * 1f;
					RainDrop[] array2 = Game1.rainDrops;
					int num2 = i;
					array2[num2].position.Y = array2[num2].position.Y - (float)yOffset * 1f;
					if (Game1.rainDrops[i].position.Y > (float)(Game1.viewport.Height + 64))
					{
						Game1.rainDrops[i].position.Y = -64f;
					}
					else if (Game1.rainDrops[i].position.X < -64f)
					{
						Game1.rainDrops[i].position.X = (float)Game1.viewport.Width;
					}
					else if (Game1.rainDrops[i].position.Y < -64f)
					{
						Game1.rainDrops[i].position.Y = (float)Game1.viewport.Height;
					}
					else if (Game1.rainDrops[i].position.X > (float)(Game1.viewport.Width + 64))
					{
						Game1.rainDrops[i].position.X = -64f;
					}
				}
				return;
			}
			Game1.updateDebrisWeatherForMovement(Game1.debrisWeather);
		}

		// Token: 0x06000B4E RID: 2894 RVA: 0x00079B08 File Offset: 0x00077D08
		public static void updateDebrisWeatherForMovement(List<WeatherDebris> debris)
		{
			if (Game1.HostPaused)
			{
				return;
			}
			if (debris != null && Game1.fadeToBlackAlpha < 1f)
			{
				int xOffset = Game1.viewport.X - (int)Game1.previousViewportPosition.X;
				int yOffset = Game1.viewport.Y - (int)Game1.previousViewportPosition.Y;
				if (Math.Abs(xOffset) > 100 || Math.Abs(yOffset) > 80)
				{
					return;
				}
				int wrapBuffer = 16;
				foreach (WeatherDebris w in debris)
				{
					WeatherDebris weatherDebris = w;
					weatherDebris.position.X = weatherDebris.position.X - (float)xOffset * 1f;
					WeatherDebris weatherDebris2 = w;
					weatherDebris2.position.Y = weatherDebris2.position.Y - (float)yOffset * 1f;
					if (w.position.Y > (float)(Game1.viewport.Height + 64 + wrapBuffer))
					{
						w.position.Y = -64f;
					}
					else if (w.position.X < (float)(-64 - wrapBuffer))
					{
						w.position.X = (float)Game1.viewport.Width;
					}
					else if (w.position.Y < (float)(-64 - wrapBuffer))
					{
						w.position.Y = (float)Game1.viewport.Height;
					}
					else if (w.position.X > (float)(Game1.viewport.Width + 64 + wrapBuffer))
					{
						w.position.X = -64f;
					}
				}
			}
		}

		// Token: 0x06000B4F RID: 2895 RVA: 0x00079CB0 File Offset: 0x00077EB0
		public static void randomizeRainPositions()
		{
			for (int i = 0; i < 70; i++)
			{
				Game1.rainDrops[i] = new RainDrop(Game1.random.Next(Game1.viewport.Width), Game1.random.Next(Game1.viewport.Height), Game1.random.Next(4), Game1.random.Next(70));
			}
		}

		// Token: 0x06000B50 RID: 2896 RVA: 0x00079D1C File Offset: 0x00077F1C
		public static void randomizeDebrisWeatherPositions(List<WeatherDebris> debris)
		{
			if (debris != null)
			{
				foreach (WeatherDebris weatherDebris in debris)
				{
					weatherDebris.position = Utility.getRandomPositionOnScreen();
				}
			}
		}

		// Token: 0x06000B51 RID: 2897 RVA: 0x00079D70 File Offset: 0x00077F70
		public static void eventFinished()
		{
			Game1.player.canOnlyWalk = false;
			if (Game1.player.bathingClothes.Value)
			{
				Game1.player.canOnlyWalk = true;
			}
			Game1.eventOver = false;
			Game1.eventUp = false;
			Game1.player.CanMove = true;
			Game1.displayHUD = true;
			Game1.player.faceDirection(Game1.player.orientationBeforeEvent);
			Game1.player.completelyStopAnimatingOrDoingAction();
			Game1.viewportFreeze = false;
			Action callback = null;
			Event currentEvent = Game1.currentLocation.currentEvent;
			if (((currentEvent != null) ? currentEvent.onEventFinished : null) != null)
			{
				callback = Game1.currentLocation.currentEvent.onEventFinished;
				Game1.currentLocation.currentEvent.onEventFinished = null;
			}
			LocationRequest exitLocation = null;
			if (Game1.currentLocation.currentEvent != null)
			{
				exitLocation = Game1.currentLocation.currentEvent.exitLocation;
				Game1.currentLocation.currentEvent.cleanup();
				Game1.currentLocation.currentEvent = null;
			}
			if (Game1.player.ActiveObject != null)
			{
				Game1.player.showCarrying();
			}
			if (Game1.dayOfMonth != 0)
			{
				Game1.currentLightSources.Clear();
			}
			if (exitLocation == null && Game1.currentLocation != null && Game1.locationRequest == null)
			{
				exitLocation = new LocationRequest(Game1.currentLocation.NameOrUniqueName, Game1.currentLocation.isStructure.Value, Game1.currentLocation);
			}
			if (exitLocation != null)
			{
				if (exitLocation.Location is Farm && Game1.player.positionBeforeEvent.Y == 64f)
				{
					Farmer player = Game1.player;
					player.positionBeforeEvent.X = player.positionBeforeEvent.X + 1f;
				}
				exitLocation.OnWarp += delegate()
				{
					Game1.player.locationBeforeForcedEvent.Value = null;
				};
				if (exitLocation.Location == Game1.currentLocation)
				{
					GameLocation.HandleMusicChange(Game1.currentLocation, Game1.currentLocation);
				}
				Game1.warpFarmer(exitLocation, (int)Game1.player.positionBeforeEvent.X, (int)Game1.player.positionBeforeEvent.Y, Game1.player.orientationBeforeEvent);
			}
			else
			{
				GameLocation.HandleMusicChange(Game1.currentLocation, Game1.currentLocation);
				Game1.player.setTileLocation(Game1.player.positionBeforeEvent);
				Game1.player.locationBeforeForcedEvent.Value = null;
			}
			Game1.nonWarpFade = false;
			Game1.fadeToBlackAlpha = 1f;
			if (callback != null)
			{
				callback();
			}
		}

		// Token: 0x06000B52 RID: 2898 RVA: 0x00079FB4 File Offset: 0x000781B4
		public static void populateDebrisWeatherArray()
		{
			Season season = Game1.GetSeasonForLocation(Game1.currentLocation);
			int debrisToMake = Game1.random.Next(16, 64);
			int baseIndex;
			switch (season)
			{
			case Season.Summer:
				baseIndex = 1;
				break;
			case Season.Fall:
				baseIndex = 2;
				break;
			case Season.Winter:
				baseIndex = 3;
				break;
			default:
				baseIndex = 0;
				break;
			}
			Game1.isDebrisWeather = true;
			Game1.debrisWeatherSeason = new Season?(season);
			Game1.debrisWeather.Clear();
			for (int i = 0; i < debrisToMake; i++)
			{
				Game1.debrisWeather.Add(new WeatherDebris(new Vector2((float)Game1.random.Next(0, Game1.viewport.Width), (float)Game1.random.Next(0, Game1.viewport.Height)), baseIndex, (float)Game1.random.Next(15) / 500f, (float)Game1.random.Next(-10, 0) / 50f, (float)Game1.random.Next(10) / 50f));
			}
		}

		// Token: 0x06000B53 RID: 2899 RVA: 0x0007A0A4 File Offset: 0x000782A4
		private static void OnNewSeason()
		{
			Game1.setGraphicsForSeason(false);
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				location.seasonUpdate(false);
				return true;
			}, true, false);
		}

		// Token: 0x06000B54 RID: 2900 RVA: 0x0007A0D4 File Offset: 0x000782D4
		public static void prepareSpouseForWedding(Farmer farmer)
		{
			NPC npc = Game1.RequireCharacter(farmer.spouse, true);
			npc.ClearSchedule();
			npc.DefaultMap = farmer.homeLocation.Value;
			npc.DefaultPosition = Utility.PointToVector2(Game1.RequireLocation<FarmHouse>(farmer.homeLocation.Value, false).getSpouseBedSpot(farmer.spouse)) * 64f;
			npc.DefaultFacingDirection = 2;
		}

		// Token: 0x06000B55 RID: 2901 RVA: 0x0007A13C File Offset: 0x0007833C
		public static bool AddCharacterIfNecessary(string characterId, bool bypassConditions = false)
		{
			CharacterData data;
			if (!NPC.TryGetData(characterId, out data))
			{
				return false;
			}
			bool characterAdded = false;
			if (Game1.getCharacterFromName(characterId, true, false) == null)
			{
				if (!bypassConditions && !GameStateQuery.CheckConditions(data.UnlockConditions, null, null, null, null, null, null))
				{
					return false;
				}
				string homeName;
				Point homeTile;
				int direction;
				NPC.ReadNpcHomeData(data, null, out homeName, out homeTile, out direction);
				bool datable = data.CanBeRomanced;
				Point size = data.Size;
				GameLocation homeLocation = Game1.getLocationFromNameInLocationsList(homeName, false);
				if (homeLocation == null)
				{
					return false;
				}
				string characterTextureName = NPC.getTextureNameForCharacter(characterId);
				NPC character;
				try
				{
					character = new NPC(new AnimatedSprite("Characters\\" + characterTextureName, 0, size.X, size.Y), new Vector2((float)(homeTile.X * 64), (float)(homeTile.Y * 64)), homeName, direction, characterId, datable, Game1.content.Load<Texture2D>("Portraits\\" + characterTextureName));
				}
				catch (Exception ex)
				{
					Game1.log.Error("Failed to spawn NPC '" + characterId + "'.", ex);
					return false;
				}
				character.Breather = data.Breather;
				homeLocation.addCharacter(character);
				characterAdded = true;
			}
			if (data.SocialTab == SocialTabBehavior.AlwaysShown && !Game1.player.friendshipData.ContainsKey(characterId))
			{
				Game1.player.friendshipData.Add(characterId, new Friendship());
			}
			return characterAdded;
		}

		// Token: 0x06000B56 RID: 2902 RVA: 0x0007A290 File Offset: 0x00078490
		public static GameLocation CreateGameLocation(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				return null;
			}
			LocationData locationData;
			CreateLocationData createData = Game1.locationData.TryGetValue(id, out locationData) ? locationData.CreateOnLoad : null;
			return Game1.CreateGameLocation(id, createData);
		}

		// Token: 0x06000B57 RID: 2903 RVA: 0x0007A2C8 File Offset: 0x000784C8
		public static GameLocation CreateGameLocation(string id, CreateLocationData createData)
		{
			if (createData == null)
			{
				return null;
			}
			GameLocation location;
			if (createData.Type != null)
			{
				Type type = Type.GetType(createData.Type);
				if (type == null)
				{
					throw new Exception("Invalid type for location " + id + ": " + createData.Type);
				}
				location = (GameLocation)Activator.CreateInstance(type, new object[]
				{
					createData.MapPath,
					id
				});
			}
			else
			{
				location = new GameLocation(createData.MapPath, id);
			}
			location.isAlwaysActive.Value = createData.AlwaysActive;
			return location;
		}

		// Token: 0x06000B58 RID: 2904 RVA: 0x0007A34C File Offset: 0x0007854C
		public static void AddLocations()
		{
			bool currentLocationSet = false;
			foreach (KeyValuePair<string, LocationData> pair in Game1.locationData)
			{
				if (pair.Value.CreateOnLoad != null)
				{
					GameLocation location;
					try
					{
						location = Game1.CreateGameLocation(pair.Key, pair.Value.CreateOnLoad);
					}
					catch (Exception ex)
					{
						Game1.log.Error("Couldn't create the '" + pair.Key + "' location. Is its data in Data/Locations invalid?", ex);
						continue;
					}
					if (location == null)
					{
						Game1.log.Error("Couldn't create the '" + pair.Key + "' location. Is its data in Data/Locations invalid?", null);
					}
					else
					{
						if (!currentLocationSet)
						{
							try
							{
								location.map.LoadTileSheets(Game1.mapDisplayDevice);
								Game1.currentLocation = location;
								currentLocationSet = true;
							}
							catch (Exception ex2)
							{
								Game1.log.Error("Couldn't load tilesheets for the '" + pair.Key + "' location.", ex2);
							}
						}
						Game1.locations.Add(location);
					}
				}
			}
			for (int i = 1; i < Game1.netWorldState.Value.HighestPlayerLimit; i++)
			{
				GameLocation cellar = Game1.CreateGameLocation("Cellar");
				NetString name = cellar.name;
				name.Value += (i + 1).ToString();
				Game1.locations.Add(cellar);
			}
		}

		// Token: 0x06000B59 RID: 2905 RVA: 0x0007A4D4 File Offset: 0x000786D4
		public static void AddNPCs()
		{
			foreach (KeyValuePair<string, CharacterData> entry in Game1.characterData)
			{
				if (entry.Value.SpawnIfMissing)
				{
					Game1.AddCharacterIfNecessary(entry.Key, false);
				}
			}
			GameLocation location = Game1.getLocationFromNameInLocationsList("QiNutRoom", false);
			if (location.getCharacterFromName("Mister Qi") == null)
			{
				AnimatedSprite sprite = new AnimatedSprite("Characters\\MrQi", 0, 16, 32);
				location.addCharacter(new NPC(sprite, new Vector2(448f, 256f), "QiNutRoom", 0, "Mister Qi", false, Game1.content.Load<Texture2D>("Portraits\\MrQi")));
			}
		}

		// Token: 0x06000B5A RID: 2906 RVA: 0x0007A594 File Offset: 0x00078794
		public static void AddModNPCs()
		{
		}

		// Token: 0x06000B5B RID: 2907 RVA: 0x0007A598 File Offset: 0x00078798
		public static void fixProblems()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				farmer.LearnDefaultRecipes();
				farmer.AddMissedMailAndRecipes();
				LevelUpMenu.RevalidateHealth(farmer);
				LevelUpMenu.AddMissedProfessionChoices(farmer);
			}
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				location.characters.RemoveWhere(delegate(NPC npc)
				{
					if (npc == null)
					{
						Game1.log.Warn("Removed broken NPC in " + location.NameOrUniqueName + ": null instance.");
						return true;
					}
					if (npc.IsVillager && npc.GetData() == null)
					{
						try
						{
							if (npc.Sprite.Texture == null)
							{
								IGameLogger gameLogger = Game1.log;
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(60, 2);
								defaultInterpolatedStringHandler.AppendLiteral("Removed broken NPC '");
								defaultInterpolatedStringHandler.AppendFormatted(npc.Name);
								defaultInterpolatedStringHandler.AppendLiteral("' in ");
								defaultInterpolatedStringHandler.AppendFormatted(location.NameOrUniqueName);
								defaultInterpolatedStringHandler.AppendLiteral(": villager with no data or sprites.");
								gameLogger.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
								return true;
							}
						}
						catch
						{
							IGameLogger gameLogger2 = Game1.log;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(60, 2);
							defaultInterpolatedStringHandler.AppendLiteral("Removed broken NPC '");
							defaultInterpolatedStringHandler.AppendFormatted(npc.Name);
							defaultInterpolatedStringHandler.AppendLiteral("' in ");
							defaultInterpolatedStringHandler.AppendFormatted(location.NameOrUniqueName);
							defaultInterpolatedStringHandler.AppendLiteral(": villager with no data or sprites.");
							gameLogger2.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
							return true;
						}
						return false;
					}
					return false;
				});
				return true;
			}, true, false);
			Game1.AddNPCs();
			List<NPC> divorced = null;
			int n;
			Utility.ForEachVillager(delegate(NPC n)
			{
				if (!n.datable.Value || n.getSpouse() != null)
				{
					return true;
				}
				if (n.DefaultMap == null || !n.DefaultMap.ContainsIgnoreCase("cabin") || n.DefaultMap != "FarmHouse")
				{
					return true;
				}
				CharacterData data = n.GetData();
				if (data == null)
				{
					return true;
				}
				string homeLocation;
				Point point;
				int num2;
				NPC.ReadNpcHomeData(data, n.currentLocation, out homeLocation, out point, out num2);
				if (n.DefaultMap != homeLocation)
				{
					if (divorced == null)
					{
						divorced = new List<NPC>();
					}
					divorced.Add(n);
				}
				return true;
			}, false);
			if (divorced != null)
			{
				foreach (NPC i in divorced)
				{
					Game1.log.Warn("Fixing " + i.Name + " who was improperly divorced and left stranded");
					i.PerformDivorce();
				}
			}
			foreach (Farmer player in Game1.getAllFarmers())
			{
				if (player.hasQuest("130"))
				{
					HashSet<string> requiredQuestItems = new HashSet<string>
					{
						"(O)864",
						"(O)865",
						"(O)866",
						"(O)867",
						"(O)868",
						"(O)869",
						"(O)870"
					};
					bool found = false;
					foreach (string itemId in requiredQuestItems)
					{
						if (player.Items.ContainsId(itemId))
						{
							found = true;
							break;
						}
					}
					if (!found)
					{
						Utility.ForEachItem(delegate(Item item)
						{
							found = requiredQuestItems.Contains(item.QualifiedItemId);
							return !found;
						});
					}
					if (!found)
					{
						Object item2 = ItemRegistry.Create<Object>("(O)864", 1, 0, false);
						item2.specialItem = true;
						item2.questItem.Value = true;
						if (!player.addItemToInventoryBool(item2, false))
						{
							player.team.returnedDonations.Add(item2);
							player.team.newLostAndFoundItems.Value = true;
						}
					}
				}
				else if (!player.craftingRecipes.ContainsKey("Fairy Dust") && player.mailReceived.Contains("birdieQuestBegun"))
				{
					player.mailReceived.Remove("birdieQuestBegun");
				}
			}
			Game1.<>c__DisplayClass772_3 CS$<>8__locals3 = new Game1.<>c__DisplayClass772_3();
			int playerCount = Game1.getAllFarmers().Count<Farmer>();
			Game1.<>c__DisplayClass772_3 CS$<>8__locals4 = CS$<>8__locals3;
			Dictionary<Type, int> dictionary = new Dictionary<Type, int>();
			Type typeFromHandle = typeof(Axe);
			dictionary[typeFromHandle] = playerCount;
			Type typeFromHandle2 = typeof(Pickaxe);
			dictionary[typeFromHandle2] = playerCount;
			Type typeFromHandle3 = typeof(Hoe);
			dictionary[typeFromHandle3] = playerCount;
			Type typeFromHandle4 = typeof(WateringCan);
			dictionary[typeFromHandle4] = playerCount;
			Type typeFromHandle5 = typeof(Wand);
			dictionary[typeFromHandle5] = 0;
			CS$<>8__locals4.missingTools = dictionary;
			using (IEnumerator<Farmer> enumerator = Game1.getAllFarmers().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.hasOrWillReceiveMail("ReturnScepter"))
					{
						Dictionary<Type, int> missingTools = CS$<>8__locals3.missingTools;
						typeFromHandle5 = typeof(Wand);
						int num = missingTools[typeFromHandle5];
						missingTools[typeFromHandle5] = num + 1;
					}
				}
			}
			CS$<>8__locals3.missingScythes = playerCount;
			foreach (Farmer who in Game1.getAllFarmers())
			{
				if (who.toolBeingUpgraded.Value != null)
				{
					who.toolBeingUpgraded.Value.FixStackSize();
					Type key = who.toolBeingUpgraded.Value.GetType();
					int count;
					if (CS$<>8__locals3.missingTools.TryGetValue(key, out count))
					{
						CS$<>8__locals3.missingTools[key] = count - 1;
					}
				}
				for (int j = 0; j < who.Items.Count; j++)
				{
					if (who.Items[j] != null)
					{
						Game1.checkIsMissingTool(CS$<>8__locals3.missingTools, ref CS$<>8__locals3.missingScythes, who.Items[j]);
					}
				}
			}
			bool allFound = true;
			using (Dictionary<Type, int>.ValueCollection.Enumerator enumerator4 = CS$<>8__locals3.missingTools.Values.GetEnumerator())
			{
				while (enumerator4.MoveNext())
				{
					if (enumerator4.Current > 0)
					{
						allFound = false;
						break;
					}
				}
			}
			if (CS$<>8__locals3.missingScythes > 0)
			{
				allFound = false;
			}
			if (!allFound)
			{
				Utility.ForEachLocation(delegate(GameLocation l)
				{
					List<Debris> debrisToDelete = new List<Debris>();
					foreach (Debris d in l.debris)
					{
						Item item3 = d.item;
						if (item3 != null)
						{
							foreach (Type type in CS$<>8__locals3.missingTools.Keys)
							{
								if (item3.GetType() == type)
								{
									debrisToDelete.Add(d);
								}
							}
							if (item3.QualifiedItemId == "(W)47")
							{
								debrisToDelete.Add(d);
							}
						}
					}
					foreach (Debris d2 in debrisToDelete)
					{
						l.debris.Remove(d2);
					}
					return true;
				}, true, false);
				Utility.iterateChestsAndStorage(delegate(Item item)
				{
					Game1.checkIsMissingTool(CS$<>8__locals3.missingTools, ref CS$<>8__locals3.missingScythes, item);
				});
				List<string> toAdd = new List<string>();
				foreach (KeyValuePair<Type, int> pair in CS$<>8__locals3.missingTools)
				{
					if (pair.Value > 0)
					{
						for (int k = 0; k < pair.Value; k++)
						{
							toAdd.Add(pair.Key.ToString());
						}
					}
				}
				for (n = 0; n < CS$<>8__locals3.missingScythes; n++)
				{
					toAdd.Add("Scythe");
				}
				if (toAdd.Count > 0)
				{
					Game1.addMailForTomorrow("foundLostTools", false, false);
				}
				for (int m = 0; m < toAdd.Count; m++)
				{
					Item tool = null;
					string a = toAdd[m];
					if (!(a == "StardewValley.Tools.Axe"))
					{
						if (!(a == "StardewValley.Tools.Hoe"))
						{
							if (!(a == "StardewValley.Tools.WateringCan"))
							{
								if (!(a == "Scythe"))
								{
									if (!(a == "StardewValley.Tools.Pickaxe"))
									{
										if (a == "StardewValley.Tools.Wand")
										{
											tool = ItemRegistry.Create("(T)ReturnScepter", 1, 0, false);
										}
									}
									else
									{
										tool = ItemRegistry.Create("(T)Pickaxe", 1, 0, false);
									}
								}
								else
								{
									tool = ItemRegistry.Create("(W)47", 1, 0, false);
								}
							}
							else
							{
								tool = ItemRegistry.Create("(T)WateringCan", 1, 0, false);
							}
						}
						else
						{
							tool = ItemRegistry.Create("(T)Hoe", 1, 0, false);
						}
					}
					else
					{
						tool = ItemRegistry.Create("(T)Axe", 1, 0, false);
					}
					if (tool != null)
					{
						if (Game1.newDaySync.hasInstance())
						{
							Game1.player.team.newLostAndFoundItems.Value = true;
						}
						Game1.player.team.returnedDonations.Add(tool);
					}
				}
			}
		}

		// Token: 0x06000B5C RID: 2908 RVA: 0x0007AD08 File Offset: 0x00078F08
		private static void checkIsMissingTool(Dictionary<Type, int> missingTools, ref int missingScythes, Item item)
		{
			foreach (Type key in missingTools.Keys)
			{
				if (item.GetType() == key)
				{
					Type key2 = key;
					int num = missingTools[key2];
					missingTools[key2] = num - 1;
				}
			}
			if (item.QualifiedItemId == "(W)47")
			{
				missingScythes--;
			}
		}

		// Token: 0x06000B5D RID: 2909 RVA: 0x0007AD90 File Offset: 0x00078F90
		public static void newDayAfterFade(Action after)
		{
			if (Game1.player.currentLocation != null)
			{
				if (Game1.player.rightRing.Value != null)
				{
					Game1.player.rightRing.Value.onLeaveLocation(Game1.player, Game1.player.currentLocation);
				}
				if (Game1.player.leftRing.Value != null)
				{
					Game1.player.leftRing.Value.onLeaveLocation(Game1.player, Game1.player.currentLocation);
				}
			}
			if (LocalMultiplayer.IsLocalMultiplayer(false))
			{
				Game1.hooks.OnGame1_NewDayAfterFade(delegate
				{
					Game1.game1.isLocalMultiplayerNewDayActive = true;
					Game1._afterNewDayAction = after;
					GameRunner.instance.activeNewDayProcesses.Add(new KeyValuePair<Game1, IEnumerator<int>>(Game1.game1, Game1._newDayAfterFade()));
				});
				return;
			}
			Game1.hooks.OnGame1_NewDayAfterFade(delegate
			{
				Game1._afterNewDayAction = after;
				if (Game1._newDayTask != null)
				{
					Game1.log.Warn("Warning: There is already a _newDayTask; unusual code path.\n" + StackTraceHelper.StackTrace);
					return;
				}
				Game1._newDayTask = new Task(delegate()
				{
					Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
					IEnumerator<int> new_day_task = Game1._newDayAfterFade();
					while (new_day_task.MoveNext())
					{
					}
				});
			});
		}

		// Token: 0x06000B5E RID: 2910 RVA: 0x0007AE53 File Offset: 0x00079053
		public static bool CanAcceptDailyQuest()
		{
			return Game1.questOfTheDay != null && !Game1.player.acceptedDailyQuest.Value && !string.IsNullOrEmpty(Game1.questOfTheDay.questDescription);
		}

		// Token: 0x06000B5F RID: 2911 RVA: 0x0007AE85 File Offset: 0x00079085
		private static IEnumerator<int> _newDayAfterFade()
		{
			TriggerActionManager.Raise("DayEnding", null, null, null, null, null);
			Game1.newDaySync.start();
			while (!Game1.newDaySync.hasStarted())
			{
				yield return 0;
			}
			int timeWentToSleep = Game1.timeOfDay;
			Game1.newDaySync.barrier("start");
			while (!Game1.newDaySync.isBarrierReady("start"))
			{
				yield return 0;
			}
			Game1.<>c__DisplayClass784_0 CS$<>8__locals1 = new Game1.<>c__DisplayClass784_0();
			CS$<>8__locals1.overnightMinutesElapsed = Utility.CalculateMinutesUntilMorning(timeWentToSleep);
			Game1.stats.AverageBedtime = (uint)timeWentToSleep;
			if (Game1.IsMasterGame)
			{
				Game1.dayOfMonth++;
				Stats stats = Game1.stats;
				uint daysPlayed = stats.DaysPlayed;
				stats.DaysPlayed = daysPlayed + 1U;
				if (Game1.dayOfMonth > 28)
				{
					Game1.dayOfMonth = 1;
					switch (Game1.season)
					{
					case Season.Spring:
						Game1.season = Season.Summer;
						break;
					case Season.Summer:
						Game1.season = Season.Fall;
						break;
					case Season.Fall:
						Game1.season = Season.Winter;
						break;
					case Season.Winter:
						Game1.season = Season.Spring;
						Game1.year++;
						MineShaft.yearUpdate();
						break;
					}
				}
				Game1.timeOfDay = 600;
				Game1.netWorldState.Value.UpdateFromGame1();
			}
			Game1.newDaySync.barrier("date");
			while (!Game1.newDaySync.isBarrierReady("date"))
			{
				yield return 0;
			}
			Game1.player.dayOfMonthForSaveGame = new int?(Game1.dayOfMonth);
			Game1.player.seasonForSaveGame = new int?(Game1.seasonIndex);
			Game1.player.yearForSaveGame = new int?(Game1.year);
			Game1.flushLocationLookup();
			Event.OnNewDay();
			try
			{
				Game1.fixProblems();
			}
			catch (Exception)
			{
			}
			foreach (Farmer farmer3 in Game1.getAllFarmers())
			{
				farmer3.FarmerSprite.PauseForSingleAnimation = false;
			}
			Game1.whereIsTodaysFest = null;
			if (Game1.wind != null)
			{
				Game1.wind.Stop(AudioStopOptions.Immediate);
				Game1.wind = null;
			}
			Game1.player.chestConsumedMineLevels.RemoveWhere((KeyValuePair<int, bool> pair) => pair.Key > 120);
			Game1.player.currentEyes = 0;
			int seed;
			if (Game1.IsMasterGame)
			{
				Game1.player.team.announcedSleepingFarmers.Clear();
				seed = Utility.CreateRandomSeed(Game1.uniqueIDForThisGame / 100UL, Game1.stats.DaysPlayed * 10U + 1U, Game1.stats.StepsTaken, 0.0, 0.0);
				Game1.newDaySync.sendVar<NetInt, int>("seed", seed);
			}
			else
			{
				while (!Game1.newDaySync.isVarReady("seed"))
				{
					yield return 0;
				}
				seed = Game1.newDaySync.waitForVar<NetInt, int>("seed");
			}
			Game1.random = Utility.CreateRandom((double)seed, 0.0, 0.0, 0.0, 0.0);
			for (int i = 0; i < Game1.dayOfMonth; i++)
			{
				Game1.random.Next();
			}
			Game1.player.team.endOfNightStatus.UpdateState("sleep");
			Game1.newDaySync.barrier("sleep");
			while (!Game1.newDaySync.isBarrierReady("sleep"))
			{
				yield return 0;
			}
			Game1.gameTimeInterval = 0;
			Game1.game1.wasAskedLeoMemory = false;
			Game1.player.team.Update();
			Game1.player.team.NewDay();
			Game1.player.passedOut = false;
			Game1.player.CanMove = true;
			Game1.player.FarmerSprite.PauseForSingleAnimation = false;
			Game1.player.FarmerSprite.StopAnimation();
			Game1.player.completelyStopAnimatingOrDoingAction();
			Game1.changeMusicTrack("silence", false, MusicContext.Default);
			if (Game1.IsMasterGame)
			{
				Game1.UpdateDishOfTheDay();
			}
			Game1.newDaySync.barrier("dishOfTheDay");
			while (!Game1.newDaySync.isBarrierReady("dishOfTheDay"))
			{
				yield return 0;
			}
			Game1.npcDialogues = null;
			Utility.ForEachCharacter(delegate(NPC n)
			{
				n.updatedDialogueYet = false;
				return true;
			}, false);
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				location.currentEvent = null;
				if (Game1.IsMasterGame)
				{
					location.passTimeForObjects(CS$<>8__locals1.overnightMinutesElapsed);
				}
				return true;
			}, true, false);
			Game1.outdoorLight = Color.White;
			Game1.ambientLight = Color.White;
			if (Game1.isLightning && Game1.IsMasterGame)
			{
				Utility.overnightLightning(timeWentToSleep);
			}
			if (Game1.MasterPlayer.hasOrWillReceiveMail("ccBulletinThankYou") && !Game1.player.hasOrWillReceiveMail("ccBulletinThankYou"))
			{
				Game1.addMailForTomorrow("ccBulletinThankYou", false, false);
			}
			Game1.ReceiveMailForTomorrow(null);
			string whichFriend;
			Friendship friendship;
			if (Utility.TryGetRandom<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>(Game1.player.friendshipData, out whichFriend, out friendship, null) && Game1.random.NextBool((double)(friendship.Points / 250) * 0.1) && Game1.player.spouse != whichFriend && DataLoader.Mail(Game1.content).ContainsKey(whichFriend))
			{
				Game1.mailbox.Add(whichFriend);
			}
			MineShaft.clearActiveMines();
			VolcanoDungeon.ClearAllLevels();
			Game1.netWorldState.Value.CheckedGarbage.Clear();
			for (int j = Game1.player.enchantments.Count - 1; j >= 0; j--)
			{
				Game1.player.enchantments[j].OnUnequip(Game1.player);
			}
			Game1.player.dayupdate(timeWentToSleep);
			if (Game1.IsMasterGame)
			{
				Game1.player.team.sharedDailyLuck.Value = Math.Min(0.10000000149011612, (double)Game1.random.Next(-100, 101) / 1000.0);
			}
			Game1.player.showToolUpgradeAvailability();
			if (Game1.IsMasterGame)
			{
				Game1.queueWeddingsForToday();
				Game1.newDaySync.sendVar<NetRef<NetLongList>, NetLongList>("weddingsToday", new NetLongList(Game1.weddingsToday));
			}
			else
			{
				while (!Game1.newDaySync.isVarReady("weddingsToday"))
				{
					yield return 0;
				}
				Game1.weddingsToday = new List<long>(Game1.newDaySync.waitForVar<NetRef<NetLongList>, NetLongList>("weddingsToday"));
			}
			Game1.weddingToday = false;
			foreach (long id2 in Game1.weddingsToday)
			{
				Farmer spouse_farmer = Game1.GetPlayer(id2, false);
				if (spouse_farmer != null && !spouse_farmer.hasCurrentOrPendingRoommate())
				{
					Game1.weddingToday = true;
					break;
				}
			}
			if (Game1.player.spouse != null && Game1.player.isEngaged() && Game1.weddingsToday.Contains(Game1.player.UniqueMultiplayerID))
			{
				Friendship friendship2 = Game1.player.friendshipData[Game1.player.spouse];
				friendship2.Status = FriendshipStatus.Married;
				friendship2.WeddingDate = new WorldDate(Game1.Date);
				Game1.prepareSpouseForWedding(Game1.player);
				if (!Game1.player.getSpouse().isRoommate())
				{
					Game1.player.autoGenerateActiveDialogueEvent("married_" + Game1.player.spouse, 4);
					if (!Game1.player.autoGenerateActiveDialogueEvent("married", 4))
					{
						Game1.player.autoGenerateActiveDialogueEvent("married_twice", 4);
					}
				}
				else
				{
					Game1.player.autoGenerateActiveDialogueEvent("roommates_" + Game1.player.spouse, 4);
				}
			}
			CS$<>8__locals1.additional_shipped_items = new NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>();
			if (Game1.IsMasterGame)
			{
				Utility.ForEachLocation(delegate(GameLocation location)
				{
					foreach (Object @object in location.objects.Values)
					{
						Chest chest = @object as Chest;
						if (chest != null && chest.SpecialChestType == Chest.SpecialChestTypes.MiniShippingBin)
						{
							chest.clearNulls();
							if (Game1.player.team.useSeparateWallets.Value)
							{
								using (NetDictionary<long, Inventory, NetRef<Inventory>, SerializableDictionary<long, Inventory>, NetLongDictionary<Inventory, NetRef<Inventory>>>.KeysCollection.Enumerator enumerator12 = chest.separateWalletItems.Keys.GetEnumerator())
								{
									while (enumerator12.MoveNext())
									{
										long id3 = enumerator12.Current;
										if (!CS$<>8__locals1.additional_shipped_items.ContainsKey(id3))
										{
											CS$<>8__locals1.additional_shipped_items[id3] = new NetList<Item, NetRef<Item>>();
										}
										List<Item> list = new List<Item>(chest.separateWalletItems[id3]);
										chest.separateWalletItems[id3].Clear();
										foreach (Item item4 in list)
										{
											item4.onDetachedFromParent();
											CS$<>8__locals1.additional_shipped_items[id3].Add(item4);
										}
									}
									goto IL_174;
								}
								goto IL_11D;
							}
							goto IL_11D;
							IL_174:
							chest.Items.Clear();
							chest.separateWalletItems.Clear();
							continue;
							IL_11D:
							IInventory shipping_bin2 = Game1.getFarm().getShippingBin(Game1.player);
							shipping_bin2.RemoveEmptySlots();
							foreach (Item item5 in chest.Items)
							{
								item5.onDetachedFromParent();
								shipping_bin2.Add(item5);
							}
							goto IL_174;
						}
					}
					return true;
				}, true, false);
			}
			if (Game1.IsMasterGame)
			{
				Game1.newDaySync.sendVar<NetRef<NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>>, NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>>("additional_shipped_items", CS$<>8__locals1.additional_shipped_items);
			}
			else
			{
				while (!Game1.newDaySync.isVarReady("additional_shipped_items"))
				{
					yield return 0;
				}
				CS$<>8__locals1.additional_shipped_items = Game1.newDaySync.waitForVar<NetRef<NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>>, NetLongDictionary<NetList<Item, NetRef<Item>>, NetRef<NetList<Item, NetRef<Item>>>>>("additional_shipped_items");
			}
			if (Game1.player.team.useSeparateWallets.Value)
			{
				IInventory shipping_bin = Game1.getFarm().getShippingBin(Game1.player);
				NetList<Item, NetRef<Item>> item_list;
				if (CS$<>8__locals1.additional_shipped_items.TryGetValue(Game1.player.UniqueMultiplayerID, out item_list))
				{
					foreach (Item item in item_list)
					{
						shipping_bin.Add(item);
					}
				}
			}
			Game1.newDaySync.barrier("handleMiniShippingBins");
			while (!Game1.newDaySync.isBarrierReady("handleMiniShippingBins"))
			{
				yield return 0;
			}
			IInventory shippingBin = Game1.getFarm().getShippingBin(Game1.player);
			shippingBin.RemoveEmptySlots();
			foreach (Item k in shippingBin)
			{
				Game1.player.displayedShippedItems.Add(k);
			}
			if (Game1.player.useSeparateWallets || Game1.player.IsMainPlayer)
			{
				int total = 0;
				foreach (Item item2 in shippingBin)
				{
					int item_value = 0;
					Object obj = item2 as Object;
					if (obj != null)
					{
						item_value = obj.sellToStorePrice(-1L) * obj.Stack;
						total += item_value;
					}
					if (Game1.player.team.specialOrders != null)
					{
						foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
						{
							Action<Farmer, Item, int> onItemShipped = specialOrder.onItemShipped;
							if (onItemShipped != null)
							{
								onItemShipped(Game1.player, item2, item_value);
							}
						}
					}
				}
				Game1.player.Money += total;
			}
			if (Game1.IsMasterGame)
			{
				if (Game1.IsWinter && Game1.dayOfMonth == 18)
				{
					GameLocation source = Game1.RequireLocation("Submarine", false);
					if (source.objects.Length >= 0)
					{
						Utility.transferPlacedObjectsFromOneLocationToAnother(source, null, new Vector2?(new Vector2(20f, 20f)), Game1.getLocationFromName("Beach"));
					}
					source = Game1.RequireLocation("MermaidHouse", false);
					if (source.objects.Length >= 0)
					{
						Utility.transferPlacedObjectsFromOneLocationToAnother(source, null, new Vector2?(new Vector2(21f, 20f)), Game1.getLocationFromName("Beach"));
					}
				}
				if (Game1.player.hasOrWillReceiveMail("pamHouseUpgrade") && !Game1.player.hasOrWillReceiveMail("transferredObjectsPamHouse"))
				{
					Game1.addMailForTomorrow("transferredObjectsPamHouse", true, false);
					GameLocation source2 = Game1.RequireLocation("Trailer", false);
					GameLocation destination = Game1.getLocationFromName("Trailer_Big");
					if (source2.objects.Length >= 0)
					{
						Utility.transferPlacedObjectsFromOneLocationToAnother(source2, destination, new Vector2?(new Vector2(14f, 23f)), null);
					}
				}
				if (Utility.HasAnyPlayerSeenEvent("191393") && !Game1.player.hasOrWillReceiveMail("transferredObjectsJojaMart"))
				{
					Game1.addMailForTomorrow("transferredObjectsJojaMart", true, false);
					GameLocation source3 = Game1.RequireLocation("JojaMart", false);
					if (source3.objects.Length >= 0)
					{
						Utility.transferPlacedObjectsFromOneLocationToAnother(source3, null, new Vector2?(new Vector2(89f, 51f)), Game1.getLocationFromName("Town"));
					}
				}
			}
			if (Game1.player.useSeparateWallets && Game1.player.IsMainPlayer)
			{
				foreach (Farmer who in Game1.getOfflineFarmhands())
				{
					if (!who.isUnclaimedFarmhand)
					{
						int total2 = 0;
						IInventory farmhandShippingBin = Game1.getFarm().getShippingBin(who);
						farmhandShippingBin.RemoveEmptySlots();
						foreach (Item item3 in farmhandShippingBin)
						{
							int item_value2 = 0;
							Object obj2 = item3 as Object;
							if (obj2 != null)
							{
								item_value2 = obj2.sellToStorePrice(who.UniqueMultiplayerID) * obj2.Stack;
								total2 += item_value2;
							}
							if (Game1.player.team.specialOrders != null)
							{
								foreach (SpecialOrder specialOrder2 in Game1.player.team.specialOrders)
								{
									Action<Farmer, Item, int> onItemShipped2 = specialOrder2.onItemShipped;
									if (onItemShipped2 != null)
									{
										onItemShipped2(Game1.player, item3, item_value2);
									}
								}
							}
						}
						Game1.player.team.AddIndividualMoney(who, total2);
						farmhandShippingBin.Clear();
					}
				}
			}
			List<NPC> divorceNPCs = new List<NPC>();
			if (Game1.IsMasterGame)
			{
				foreach (Farmer who2 in Game1.getAllFarmers())
				{
					if (who2.isActive() && who2.divorceTonight.Value && who2.getSpouse() != null)
					{
						divorceNPCs.Add(who2.getSpouse());
					}
				}
			}
			Game1.newDaySync.barrier("player.dayupdate");
			while (!Game1.newDaySync.isBarrierReady("player.dayupdate"))
			{
				yield return 0;
			}
			if (Game1.player.divorceTonight.Value)
			{
				Game1.player.doDivorce();
			}
			Game1.newDaySync.barrier("player.divorce");
			while (!Game1.newDaySync.isBarrierReady("player.divorce"))
			{
				yield return 0;
			}
			if (Game1.IsMasterGame)
			{
				foreach (NPC npc in divorceNPCs)
				{
					if (npc.getSpouse() == null)
					{
						npc.PerformDivorce();
					}
				}
			}
			Game1.newDaySync.barrier("player.finishDivorce");
			while (!Game1.newDaySync.isBarrierReady("player.finishDivorce"))
			{
				yield return 0;
			}
			if (Game1.IsMasterGame)
			{
				Utility.ForEachBuilding(delegate(Building building)
				{
					Cabin cabin = ((building != null) ? building.GetIndoors() : null) as Cabin;
					if (cabin != null)
					{
						cabin.updateFarmLayout();
					}
					return true;
				}, true);
			}
			Game1.newDaySync.barrier("updateFarmLayout");
			while (!Game1.newDaySync.isBarrierReady("updateFarmLayout"))
			{
				yield return 0;
			}
			if (Game1.IsMasterGame && Game1.player.changeWalletTypeTonight.Value)
			{
				if (Game1.player.useSeparateWallets)
				{
					ManorHouse.MergeWallets();
				}
				else
				{
					ManorHouse.SeparateWallets();
				}
			}
			Game1.newDaySync.barrier("player.wallets");
			while (!Game1.newDaySync.isBarrierReady("player.wallets"))
			{
				yield return 0;
			}
			Game1.getFarm().lastItemShipped = null;
			Game1.getFarm().getShippingBin(Game1.player).Clear();
			Game1.newDaySync.barrier("clearShipping");
			while (!Game1.newDaySync.isBarrierReady("clearShipping"))
			{
				yield return 0;
			}
			if (Game1.IsClient)
			{
				Game1.multiplayer.sendFarmhand();
				Game1.newDaySync.processMessages();
			}
			Game1.newDaySync.barrier("sendFarmhands");
			while (!Game1.newDaySync.isBarrierReady("sendFarmhands"))
			{
				yield return 0;
			}
			if (Game1.IsMasterGame)
			{
				Game1.multiplayer.saveFarmhands();
			}
			Game1.newDaySync.barrier("saveFarmhands");
			while (!Game1.newDaySync.isBarrierReady("saveFarmhands"))
			{
				yield return 0;
			}
			int num;
			if (Game1.IsMasterGame)
			{
				Game1.UpdatePassiveFestivalStates();
				if (Utility.IsPassiveFestivalDay("NightMarket") && Game1.IsMasterGame && Game1.netWorldState.Value.VisitsUntilY1Guarantee >= 0)
				{
					NetWorldState value = Game1.netWorldState.Value;
					num = value.VisitsUntilY1Guarantee;
					value.VisitsUntilY1Guarantee = num - 1;
				}
			}
			if (Game1.dayOfMonth == 1)
			{
				Game1.OnNewSeason();
			}
			if (Game1.IsMasterGame && (Game1.dayOfMonth == 1 || Game1.dayOfMonth == 8 || Game1.dayOfMonth == 15 || Game1.dayOfMonth == 22))
			{
				SpecialOrder.UpdateAvailableSpecialOrders("", true);
				SpecialOrder.UpdateAvailableSpecialOrders("Qi", true);
			}
			if (Game1.IsMasterGame)
			{
				Game1.netWorldState.Value.UpdateFromGame1();
			}
			Game1.newDaySync.barrier("specialOrders");
			while (!Game1.newDaySync.isBarrierReady("specialOrders"))
			{
				yield return 0;
			}
			if (Game1.IsMasterGame)
			{
				Game1.player.team.specialOrders.RemoveWhere(delegate(SpecialOrder order)
				{
					if (order.questState.Value != SpecialOrderStatus.Complete && order.GetDaysLeft() <= 0)
					{
						order.OnFail();
						return true;
					}
					return false;
				});
			}
			Game1.newDaySync.barrier("processOrders");
			while (!Game1.newDaySync.isBarrierReady("processOrders"))
			{
				yield return 0;
			}
			if (Game1.IsMasterGame)
			{
				foreach (string rule in Game1.player.team.specialRulesRemovedToday)
				{
					SpecialOrder.RemoveSpecialRuleAtEndOfDay(rule);
				}
			}
			Game1.player.team.specialRulesRemovedToday.Clear();
			if (DataLoader.Mail(Game1.content).ContainsKey(string.Concat(new string[]
			{
				Game1.currentSeason,
				"_",
				Game1.dayOfMonth.ToString(),
				"_",
				Game1.year.ToString()
			})))
			{
				Game1.mailbox.Add(string.Concat(new string[]
				{
					Game1.currentSeason,
					"_",
					Game1.dayOfMonth.ToString(),
					"_",
					Game1.year.ToString()
				}));
			}
			else if (DataLoader.Mail(Game1.content).ContainsKey(Game1.currentSeason + "_" + Game1.dayOfMonth.ToString()))
			{
				Game1.mailbox.Add(Game1.currentSeason + "_" + Game1.dayOfMonth.ToString());
			}
			if (Game1.MasterPlayer.mailReceived.Contains("ccVault") && Game1.IsSpring && Game1.dayOfMonth == 14)
			{
				Game1.mailbox.Add("DesertFestival");
			}
			if (Game1.IsMasterGame)
			{
				if (Game1.player.team.toggleMineShrineOvernight.Value)
				{
					Game1.player.team.toggleMineShrineOvernight.Value = false;
					Game1.player.team.mineShrineActivated.Value = !Game1.player.team.mineShrineActivated.Value;
					if (Game1.player.team.mineShrineActivated.Value)
					{
						NetWorldState value2 = Game1.netWorldState.Value;
						num = value2.MinesDifficulty;
						value2.MinesDifficulty = num + 1;
					}
					else
					{
						NetWorldState value3 = Game1.netWorldState.Value;
						num = value3.MinesDifficulty;
						value3.MinesDifficulty = num - 1;
					}
				}
				if (Game1.player.team.toggleSkullShrineOvernight.Value)
				{
					Game1.player.team.toggleSkullShrineOvernight.Value = false;
					Game1.player.team.skullShrineActivated.Value = !Game1.player.team.skullShrineActivated.Value;
					if (Game1.player.team.skullShrineActivated.Value)
					{
						NetWorldState value4 = Game1.netWorldState.Value;
						num = value4.SkullCavesDifficulty;
						value4.SkullCavesDifficulty = num + 1;
					}
					else
					{
						NetWorldState value5 = Game1.netWorldState.Value;
						num = value5.SkullCavesDifficulty;
						value5.SkullCavesDifficulty = num - 1;
					}
				}
			}
			if (Game1.IsMasterGame)
			{
				if (!Game1.player.team.SpecialOrderRuleActive("MINE_HARD", null) && Game1.netWorldState.Value.MinesDifficulty > 1)
				{
					Game1.netWorldState.Value.MinesDifficulty = 1;
				}
				if (!Game1.player.team.SpecialOrderRuleActive("SC_HARD", null) && Game1.netWorldState.Value.SkullCavesDifficulty > 1)
				{
					Game1.netWorldState.Value.SkullCavesDifficulty = 1;
				}
			}
			if (Game1.IsMasterGame)
			{
				Game1.RefreshQuestOfTheDay();
			}
			Game1.newDaySync.barrier("questOfTheDay");
			while (!Game1.newDaySync.isBarrierReady("questOfTheDay"))
			{
				yield return 0;
			}
			bool yesterdayWasGreenRain = Game1.wasGreenRain;
			Game1.wasGreenRain = false;
			Game1.UpdateWeatherForNewDay();
			Game1.newDaySync.barrier("updateWeather");
			while (!Game1.newDaySync.isBarrierReady("updateWeather"))
			{
				yield return 0;
			}
			Game1.ApplyWeatherForNewDay();
			if (Game1.isGreenRain)
			{
				Game1.morningQueue.Enqueue(delegate
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:greenrainmessage"));
				});
				if (Game1.year == 1 && !Game1.player.hasOrWillReceiveMail("GreenRainGus"))
				{
					Game1.mailbox.Add("GreenRainGus");
				}
				if (Game1.IsMasterGame)
				{
					Utility.ForEachLocation(delegate(GameLocation location)
					{
						location.performGreenRainUpdate();
						return true;
					}, true, false);
				}
			}
			else if (yesterdayWasGreenRain)
			{
				if (Game1.IsMasterGame)
				{
					Utility.ForEachLocation(delegate(GameLocation location)
					{
						location.performDayAfterGreenRainUpdate();
						return true;
					}, true, false);
				}
				if (Game1.year == 1)
				{
					Game1.player.activeDialogueEvents.TryAdd("GreenRainFinished", 1);
				}
			}
			if (Utility.getDaysOfBooksellerThisSeason().Contains(Game1.dayOfMonth))
			{
				Game1.addMorningFluffFunction(delegate
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:BooksellerInTown"));
				});
			}
			WeatherDebris.globalWind = 0f;
			Game1.windGust = 0f;
			Game1.AddNPCs();
			Utility.ForEachVillager(delegate(NPC n)
			{
				Game1.player.mailReceived.Remove(n.Name);
				Game1.player.mailReceived.Remove(n.Name + "Cooking");
				n.drawOffset = Vector2.Zero;
				if (!Game1.IsMasterGame)
				{
					n.ChooseAppearance(null);
				}
				return true;
			}, false);
			FarmAnimal.reservedGrass.Clear();
			if (Game1.IsMasterGame)
			{
				NPC.hasSomeoneWateredCrops = (NPC.hasSomeoneFedThePet = (NPC.hasSomeoneFedTheAnimals = (NPC.hasSomeoneRepairedTheFences = false)));
				foreach (GameLocation gameLocation in Game1.locations)
				{
					gameLocation.ResetCharacterDialogues();
					gameLocation.DayUpdate(Game1.dayOfMonth);
				}
				Game1.netWorldState.Value.UpdateUnderConstruction();
				Game1.UpdateHorseOwnership();
				foreach (NPC l in Utility.getAllCharacters())
				{
					if (l.IsVillager)
					{
						l.islandScheduleName.Value = null;
						l.currentScheduleDelay = 0f;
					}
					l.dayUpdate(Game1.dayOfMonth);
				}
				IslandSouth.SetupIslandSchedules();
				HashSet<NPC> purchased_item_npcs = new HashSet<NPC>();
				Game1.UpdateShopPlayerItemInventory("SeedShop", purchased_item_npcs);
				Game1.UpdateShopPlayerItemInventory("FishShop", purchased_item_npcs);
			}
			if (Game1.IsMasterGame && Game1.netWorldState.Value.GetWeatherForLocation("Island").IsRaining)
			{
				Vector2 tile_location = new Vector2(0f, 0f);
				IslandLocation island_location = null;
				List<int> order2 = new List<int>();
				for (int m = 0; m < 4; m++)
				{
					order2.Add(m);
				}
				Utility.Shuffle<int>(Utility.CreateRandom(Game1.uniqueIDForThisGame, 0.0, 0.0, 0.0, 0.0), order2);
				switch (order2[Game1.currentGemBirdIndex])
				{
				case 0:
					island_location = (Game1.getLocationFromName("IslandSouth") as IslandLocation);
					tile_location = new Vector2(10f, 30f);
					break;
				case 1:
					island_location = (Game1.getLocationFromName("IslandNorth") as IslandLocation);
					tile_location = new Vector2(56f, 56f);
					break;
				case 2:
					island_location = (Game1.getLocationFromName("Islandwest") as IslandLocation);
					tile_location = new Vector2(53f, 51f);
					break;
				case 3:
					island_location = (Game1.getLocationFromName("IslandEast") as IslandLocation);
					tile_location = new Vector2(21f, 35f);
					break;
				}
				Game1.currentGemBirdIndex = (Game1.currentGemBirdIndex + 1) % 4;
				if (island_location != null)
				{
					island_location.locationGemBird.Value = new IslandGemBird(tile_location, IslandGemBird.GetBirdTypeForLocation(island_location.Name));
				}
			}
			if (Game1.IsMasterGame)
			{
				Utility.ForEachLocation(delegate(GameLocation location)
				{
					if (location.IsOutdoors && location.IsRainingHere())
					{
						foreach (Building building in location.buildings)
						{
							PetBowl bowl = building as PetBowl;
							if (bowl != null)
							{
								bowl.watered.Value = true;
							}
						}
						foreach (KeyValuePair<Vector2, TerrainFeature> kvp in location.terrainFeatures.Pairs)
						{
							HoeDirt dirt = kvp.Value as HoeDirt;
							if (dirt != null && dirt.state.Value != 2)
							{
								dirt.state.Value = 1;
							}
						}
					}
					return true;
				}, true, false);
			}
			WorldDate yesterday = new WorldDate(Game1.Date);
			WorldDate worldDate = yesterday;
			num = worldDate.TotalDays;
			worldDate.TotalDays = num - 1;
			foreach (KeyValuePair<string, PassiveFestivalData> pair2 in DataLoader.PassiveFestivals(Game1.content))
			{
				string id = pair2.Key;
				PassiveFestivalData festival = pair2.Value;
				if (yesterday.DayOfMonth == festival.EndDay && yesterday.Season == festival.Season && GameStateQuery.CheckConditions(festival.Condition, null, null, null, null, null, null) && festival != null && festival.CleanupMethod != null)
				{
					FestivalCleanupDelegate method;
					string error;
					if (StaticDelegateBuilder.TryCreateDelegate<FestivalCleanupDelegate>(festival.CleanupMethod, out method, out error))
					{
						method();
					}
					else
					{
						IGameLogger gameLogger = Game1.log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(51, 3);
						defaultInterpolatedStringHandler.AppendLiteral("Passive festival '");
						defaultInterpolatedStringHandler.AppendFormatted(id);
						defaultInterpolatedStringHandler.AppendLiteral("' has invalid cleanup method '");
						defaultInterpolatedStringHandler.AppendFormatted(festival.CleanupMethod);
						defaultInterpolatedStringHandler.AppendLiteral("': ");
						defaultInterpolatedStringHandler.AppendFormatted(error);
						gameLogger.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
					}
				}
			}
			Game1.PerformPassiveFestivalSetup();
			Game1.newDaySync.barrier("buildingUpgrades");
			while (!Game1.newDaySync.isBarrierReady("buildingUpgrades"))
			{
				yield return 0;
			}
			List<string> mailToRemoveOvernight = new List<string>(Game1.player.team.mailToRemoveOvernight);
			foreach (string index in new List<string>(Game1.player.team.itemsToRemoveOvernight))
			{
				if (Game1.IsMasterGame)
				{
					Game1.game1._PerformRemoveNormalItemFromWorldOvernight(index);
					foreach (Farmer farmer in Game1.getOfflineFarmhands())
					{
						Game1.game1._PerformRemoveNormalItemFromFarmerOvernight(farmer, index);
					}
				}
				Game1.game1._PerformRemoveNormalItemFromFarmerOvernight(Game1.player, index);
			}
			foreach (string mail_key in mailToRemoveOvernight)
			{
				if (Game1.IsMasterGame)
				{
					using (IEnumerator<Farmer> enumerator = Game1.getAllFarmers().GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Farmer farmer2 = enumerator.Current;
							farmer2.RemoveMail(mail_key, farmer2 == Game1.MasterPlayer);
						}
						continue;
					}
				}
				Game1.player.RemoveMail(mail_key, false);
			}
			Game1.newDaySync.barrier("removeItemsFromWorld");
			while (!Game1.newDaySync.isBarrierReady("removeItemsFromWorld"))
			{
				yield return 0;
			}
			if (Game1.IsMasterGame)
			{
				Game1.player.team.itemsToRemoveOvernight.Clear();
				Game1.player.team.mailToRemoveOvernight.Clear();
			}
			Game1.newDay = false;
			if (Game1.IsMasterGame)
			{
				Game1.netWorldState.Value.UpdateFromGame1();
			}
			if (Game1.player.currentLocation != null)
			{
				Game1.player.currentLocation.resetForPlayerEntry();
				BedFurniture.ApplyWakeUpPosition(Game1.player);
				Game1.forceSnapOnNextViewportUpdate = true;
				Game1.UpdateViewPort(false, Game1.player.StandingPixel);
				Game1.previousViewportPosition = new Vector2((float)Game1.viewport.X, (float)Game1.viewport.Y);
			}
			Game1.displayFarmer = true;
			Game1.updateWeatherIcon();
			Game1.freezeControls = false;
			if (Game1.stats.DaysPlayed > 1U || !Game1.IsMasterGame)
			{
				Game1.farmEvent = null;
				if (Game1.IsMasterGame)
				{
					Game1.farmEvent = (Utility.pickFarmEvent() ?? Game1.farmEventOverride);
					Game1.farmEventOverride = null;
					Game1.newDaySync.sendVar<NetRef<FarmEvent>, FarmEvent>("farmEvent", Game1.farmEvent);
				}
				else
				{
					while (!Game1.newDaySync.isVarReady("farmEvent"))
					{
						yield return 0;
					}
					Game1.farmEvent = Game1.newDaySync.waitForVar<NetRef<FarmEvent>, FarmEvent>("farmEvent");
				}
				if (Game1.farmEvent == null)
				{
					Game1.farmEvent = Utility.pickPersonalFarmEvent();
				}
				if (Game1.farmEvent != null && Game1.farmEvent.setUp())
				{
					Game1.farmEvent = null;
				}
			}
			if (Game1.farmEvent == null)
			{
				Game1.RemoveDeliveredMailForTomorrow();
			}
			if (Game1.player.team.newLostAndFoundItems.Value)
			{
				Game1.morningQueue.Enqueue(delegate
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:NewLostAndFoundItems"));
				});
			}
			Game1.newDaySync.barrier("mail");
			while (!Game1.newDaySync.isBarrierReady("mail"))
			{
				yield return 0;
			}
			if (Game1.IsMasterGame)
			{
				Game1.player.team.newLostAndFoundItems.Value = false;
			}
			Utility.ForEachBuilding(delegate(Building building)
			{
				if (building.GetIndoors() is Cabin)
				{
					Game1.player.slotCanHost = true;
					return false;
				}
				return true;
			}, true);
			if (Utility.percentGameComplete() + (float)Game1.netWorldState.Value.PerfectionWaivers * 0.01f >= 1f)
			{
				Game1.player.team.farmPerfect.Value = true;
			}
			Game1.newDaySync.barrier("checkcompletion");
			while (!Game1.newDaySync.isBarrierReady("checkcompletion"))
			{
				yield return 0;
			}
			Game1.UpdateFarmPerfection();
			if (Game1.farmEvent == null)
			{
				Game1.handlePostFarmEventActions();
				Game1.showEndOfNightStuff();
			}
			if (Game1.server != null)
			{
				Game1.server.updateLobbyData();
			}
			CS$<>8__locals1 = null;
			divorceNPCs = null;
			yield break;
		}

		// Token: 0x06000B60 RID: 2912 RVA: 0x0007AE90 File Offset: 0x00079090
		public static void UpdateDishOfTheDay()
		{
			string itemId;
			do
			{
				itemId = Game1.random.Next(194, 240).ToString();
			}
			while (Utility.IsForbiddenDishOfTheDay(itemId));
			int count = Game1.random.Next(1, 4 + ((Game1.random.NextDouble() < 0.08) ? 10 : 0));
			Game1.dishOfTheDay = ItemRegistry.Create<Object>("(O)" + itemId, count, 0, false);
		}

		// Token: 0x06000B61 RID: 2913 RVA: 0x0007AF04 File Offset: 0x00079104
		public static void UpdateFarmPerfection()
		{
			if (!Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal") && (Game1.MasterPlayer.hasCompletedCommunityCenter() || Utility.hasFinishedJojaRoute()) && Game1.player.team.farmPerfect.Value)
			{
				Game1.addMorningFluffFunction(delegate
				{
					Game1.changeMusicTrack("none", true, MusicContext.Default);
					if (Game1.IsMasterGame)
					{
						Game1.multiplayer.globalChatInfoMessageEvenInSinglePlayer("Eternal1", Array.Empty<string>());
					}
					Game1.playSound("discoverMineral", null);
					if (Game1.IsMasterGame)
					{
						DelayedAction.functionAfterDelay(delegate
						{
							Game1.multiplayer.globalChatInfoMessageEvenInSinglePlayer("Eternal2", new string[]
							{
								Game1.MasterPlayer.farmName.Value
							});
						}, 4000);
					}
					Game1.player.mailReceived.Add("Farm_Eternal");
					DelayedAction.functionAfterDelay(delegate
					{
						Game1.playSound("thunder_small", null);
						if (Game1.IsMultiplayer)
						{
							if (Game1.IsMasterGame)
							{
								Game1.multiplayer.globalChatInfoMessage("Eternal3", Array.Empty<string>());
								return;
							}
						}
						else
						{
							Game1.showGlobalMessage(Game1.content.LoadString("Strings\\UI:Chat_Eternal3"));
						}
					}, 12000);
				});
			}
		}

		// Token: 0x06000B62 RID: 2914 RVA: 0x0007AF74 File Offset: 0x00079174
		public static bool IsGreenRainingHere(GameLocation location = null)
		{
			if (location == null)
			{
				location = Game1.currentLocation;
			}
			return location != null && Game1.netWorldState != null && location.IsGreenRainingHere();
		}

		// Token: 0x06000B63 RID: 2915 RVA: 0x0007AF98 File Offset: 0x00079198
		public static bool IsRainingHere(GameLocation location = null)
		{
			if (location == null)
			{
				location = Game1.currentLocation;
			}
			return location != null && Game1.netWorldState != null && location.IsRainingHere();
		}

		// Token: 0x06000B64 RID: 2916 RVA: 0x0007AFBC File Offset: 0x000791BC
		public static bool IsLightningHere(GameLocation location = null)
		{
			if (location == null)
			{
				location = Game1.currentLocation;
			}
			return location != null && Game1.netWorldState != null && location.IsLightningHere();
		}

		// Token: 0x06000B65 RID: 2917 RVA: 0x0007AFE0 File Offset: 0x000791E0
		public static bool IsSnowingHere(GameLocation location = null)
		{
			if (location == null)
			{
				location = Game1.currentLocation;
			}
			return location != null && Game1.netWorldState != null && location.IsSnowingHere();
		}

		// Token: 0x06000B66 RID: 2918 RVA: 0x0007B004 File Offset: 0x00079204
		public static bool IsDebrisWeatherHere(GameLocation location = null)
		{
			if (location == null)
			{
				location = Game1.currentLocation;
			}
			return location != null && Game1.netWorldState != null && location.IsDebrisWeatherHere();
		}

		// Token: 0x06000B67 RID: 2919 RVA: 0x0007B028 File Offset: 0x00079228
		public static string getWeatherModificationsForDate(WorldDate date, string default_weather)
		{
			string weather = default_weather;
			int day_offset = date.TotalDays - Game1.Date.TotalDays;
			if (date.DayOfMonth == 1 || (ulong)Game1.stats.DaysPlayed + (ulong)((long)day_offset) <= 4UL)
			{
				weather = "Sun";
			}
			if ((ulong)Game1.stats.DaysPlayed + (ulong)((long)day_offset) == 3UL)
			{
				weather = "Rain";
			}
			if (Utility.isGreenRainDay(date.DayOfMonth, date.Season))
			{
				weather = "GreenRain";
			}
			if (date.Season == Season.Summer && date.DayOfMonth % 13 == 0)
			{
				weather = "Storm";
			}
			if (Utility.isFestivalDay(date.DayOfMonth, date.Season))
			{
				weather = "Festival";
			}
			foreach (PassiveFestivalData festival in DataLoader.PassiveFestivals(Game1.content).Values)
			{
				if (date.DayOfMonth >= festival.StartDay && date.DayOfMonth <= festival.EndDay && date.Season == festival.Season && GameStateQuery.CheckConditions(festival.Condition, null, null, null, null, null, null) && festival.MapReplacements != null)
				{
					foreach (string name in festival.MapReplacements.Keys)
					{
						GameLocation replacedLocation = Game1.getLocationFromName(name);
						if (replacedLocation != null && replacedLocation.InValleyContext())
						{
							weather = "Sun";
							break;
						}
					}
				}
			}
			return weather;
		}

		// Token: 0x06000B68 RID: 2920 RVA: 0x0007B1C4 File Offset: 0x000793C4
		public static void UpdateWeatherForNewDay()
		{
			Game1.weatherForTomorrow = Game1.getWeatherModificationsForDate(Game1.Date, Game1.weatherForTomorrow);
			if (Game1.weddingToday)
			{
				Game1.weatherForTomorrow = "Wedding";
			}
			if (Game1.IsMasterGame)
			{
				Game1.netWorldState.Value.GetWeatherForLocation("Default").WeatherForTomorrow = Game1.weatherForTomorrow;
			}
			Game1.wasRainingYesterday = (Game1.isRaining || Game1.isLightning);
			Game1.debrisWeather.Clear();
			if (Game1.IsMasterGame)
			{
				foreach (KeyValuePair<string, LocationContextData> pair in Game1.locationContextData)
				{
					Game1.netWorldState.Value.GetWeatherForLocation(pair.Key).UpdateDailyWeather(pair.Key, pair.Value, Game1.random);
				}
				foreach (KeyValuePair<string, LocationContextData> pair2 in Game1.locationContextData)
				{
					string contextToCopy = pair2.Value.CopyWeatherFromLocation;
					if (contextToCopy != null)
					{
						try
						{
							LocationWeather weatherForLocation = Game1.netWorldState.Value.GetWeatherForLocation(pair2.Key);
							LocationWeather otherLocationWeather = Game1.netWorldState.Value.GetWeatherForLocation(contextToCopy);
							weatherForLocation.CopyFrom(otherLocationWeather);
						}
						catch
						{
						}
					}
				}
			}
		}

		// Token: 0x06000B69 RID: 2921 RVA: 0x0007B330 File Offset: 0x00079530
		public static void ApplyWeatherForNewDay()
		{
			LocationWeather weatherForLocation = Game1.netWorldState.Value.GetWeatherForLocation("Default");
			Game1.weatherForTomorrow = weatherForLocation.WeatherForTomorrow;
			Game1.isRaining = weatherForLocation.IsRaining;
			Game1.isSnowing = weatherForLocation.IsSnowing;
			Game1.isLightning = weatherForLocation.IsLightning;
			Game1.isDebrisWeather = weatherForLocation.IsDebrisWeather;
			Game1.isGreenRain = weatherForLocation.IsGreenRain;
			if (Game1.isDebrisWeather)
			{
				Game1.populateDebrisWeatherArray();
			}
			if (Game1.IsMasterGame)
			{
				foreach (string key in Game1.netWorldState.Value.LocationWeather.Keys)
				{
					LocationWeather locationWeather = Game1.netWorldState.Value.LocationWeather[key];
					if (Game1.dayOfMonth == 1)
					{
						locationWeather.monthlyNonRainyDayCount.Value = 0;
					}
					if (!locationWeather.IsRaining)
					{
						NetInt monthlyNonRainyDayCount = locationWeather.monthlyNonRainyDayCount;
						int value = monthlyNonRainyDayCount.Value;
						monthlyNonRainyDayCount.Value = value + 1;
					}
				}
			}
		}

		// Token: 0x06000B6A RID: 2922 RVA: 0x0007B444 File Offset: 0x00079644
		public static void UpdateShopPlayerItemInventory(string location_name, HashSet<NPC> purchased_item_npcs)
		{
			ShopLocation shopLocation = Game1.getLocationFromName(location_name) as ShopLocation;
			if (shopLocation != null)
			{
				NetObjectList<Item> items = shopLocation.itemsFromPlayerToSell;
				for (int i = items.Count - 1; i >= 0; i--)
				{
					Object item = items[i] as Object;
					if (item == null)
					{
						items.RemoveAt(i);
					}
					else
					{
						for (int j = 0; j < item.Stack; j++)
						{
							bool soldItem = false;
							if (item.edibility.Value != -300 && Game1.random.NextDouble() < 0.04)
							{
								NPC k = Utility.GetRandomNpc(delegate(string name, CharacterData data)
								{
									bool? canCommentOnPurchasedShopItems = data.CanCommentOnPurchasedShopItems;
									if (canCommentOnPurchasedShopItems == null)
									{
										return data.HomeRegion == "Town";
									}
									return canCommentOnPurchasedShopItems.GetValueOrDefault();
								}, null, true);
								if (k.Age != 2 && k.getSpouse() == null)
								{
									if (!purchased_item_npcs.Contains(k))
									{
										Dialogue dialogue = shopLocation.getPurchasedItemDialogueForNPC(item, k);
										if (dialogue != null)
										{
											k.addExtraDialogue(dialogue);
											purchased_item_npcs.Add(k);
										}
									}
									items[i] = item.ConsumeStack(1);
									soldItem = true;
								}
							}
							if (!soldItem && Game1.random.NextDouble() < 0.15)
							{
								items[i] = item.ConsumeStack(1);
							}
							if (items[i] == null)
							{
								items.RemoveAt(i);
								break;
							}
						}
					}
				}
			}
		}

		// Token: 0x06000B6B RID: 2923 RVA: 0x0007B598 File Offset: 0x00079798
		private static void handlePostFarmEventActions()
		{
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				foreach (Action action in location.postFarmEventOvernightActions)
				{
					action();
				}
				location.postFarmEventOvernightActions.Clear();
				return true;
			}, true, false);
			if (Game1.IsMasterGame)
			{
				Mountain mountain = Game1.RequireLocation<Mountain>("Mountain", false);
				mountain.ApplyTreehouseIfNecessary();
				if (mountain.treehouseDoorDirty)
				{
					mountain.treehouseDoorDirty = false;
					WarpPathfindingCache.PopulateCache();
				}
			}
		}

		// Token: 0x06000B6C RID: 2924 RVA: 0x0007B5F8 File Offset: 0x000797F8
		public static void ReceiveMailForTomorrow(string mail_to_transfer = null)
		{
			foreach (string s in Game1.player.mailForTomorrow)
			{
				if (s != null)
				{
					string stripped = s.Replace("%&NL&%", "");
					if (mail_to_transfer == null || !(mail_to_transfer != s) || !(mail_to_transfer != stripped))
					{
						Game1.mailDeliveredFromMailForTomorrow.Add(s);
						if (s.Contains("%&NL&%"))
						{
							Game1.player.mailReceived.Add(stripped);
						}
						else
						{
							Game1.mailbox.Add(s);
						}
					}
				}
			}
		}

		// Token: 0x06000B6D RID: 2925 RVA: 0x0007B6A4 File Offset: 0x000798A4
		public static void RemoveDeliveredMailForTomorrow()
		{
			Game1.ReceiveMailForTomorrow("abandonedJojaMartAccessible");
			foreach (string s in Game1.mailDeliveredFromMailForTomorrow)
			{
				Game1.player.mailForTomorrow.Remove(s);
			}
			Game1.mailDeliveredFromMailForTomorrow.Clear();
		}

		// Token: 0x06000B6E RID: 2926 RVA: 0x0007B714 File Offset: 0x00079914
		public static void queueWeddingsForToday()
		{
			Game1.weddingsToday.Clear();
			Game1.weddingToday = false;
			if (!Game1.canHaveWeddingOnDay(Game1.dayOfMonth, Game1.season))
			{
				return;
			}
			foreach (Farmer farmer2 in from farmer in Game1.getOnlineFarmers()
			orderby farmer.UniqueMultiplayerID
			select farmer)
			{
				if (farmer2.spouse != null && farmer2.isEngaged() && farmer2.friendshipData[farmer2.spouse].CountdownToWedding < 1)
				{
					Game1.weddingsToday.Add(farmer2.UniqueMultiplayerID);
				}
				if (farmer2.team.IsEngaged(farmer2.UniqueMultiplayerID))
				{
					long? spouse = farmer2.team.GetSpouse(farmer2.UniqueMultiplayerID);
					if (spouse != null && !Game1.weddingsToday.Contains(spouse.Value))
					{
						Farmer spouse_farmer = Game1.GetPlayer(spouse.Value, false);
						if (spouse_farmer != null && Game1.getOnlineFarmers().Contains(spouse_farmer) && Game1.getOnlineFarmers().Contains(farmer2) && Game1.player.team.GetFriendship(farmer2.UniqueMultiplayerID, spouse.Value).CountdownToWedding < 1)
						{
							Game1.weddingsToday.Add(farmer2.UniqueMultiplayerID);
						}
					}
				}
			}
		}

		// Token: 0x06000B6F RID: 2927 RVA: 0x0007B884 File Offset: 0x00079A84
		public static bool PollForEndOfNewDaySync()
		{
			if (!Game1.IsMultiplayer)
			{
				Game1.newDaySync.destroy();
				Game1.currentLocation.resetForPlayerEntry();
				return true;
			}
			if (Game1.newDaySync.readyForFinish())
			{
				if (Game1.IsMasterGame && Game1.newDaySync.hasInstance() && !Game1.newDaySync.hasFinished())
				{
					Game1.newDaySync.finish();
				}
				if (Game1.IsClient)
				{
					Game1.player.sleptInTemporaryBed.Value = false;
				}
				if (Game1.newDaySync.hasInstance() && Game1.newDaySync.hasFinished())
				{
					Game1.newDaySync.destroy();
					Game1.currentLocation.resetForPlayerEntry();
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000B70 RID: 2928 RVA: 0x0007B92C File Offset: 0x00079B2C
		public static void updateWeatherIcon()
		{
			if (Game1.IsSnowingHere(null))
			{
				Game1.weatherIcon = 7;
			}
			else if (Game1.IsRainingHere(null))
			{
				Game1.weatherIcon = 4;
			}
			else if (Game1.IsDebrisWeatherHere(null) && Game1.IsSpring)
			{
				Game1.weatherIcon = 3;
			}
			else if (Game1.IsDebrisWeatherHere(null) && Game1.IsFall)
			{
				Game1.weatherIcon = 6;
			}
			else if (Game1.IsDebrisWeatherHere(null) && Game1.IsWinter)
			{
				Game1.weatherIcon = 7;
			}
			else if (Game1.weddingToday)
			{
				Game1.weatherIcon = 0;
			}
			else
			{
				Game1.weatherIcon = 2;
			}
			if (Game1.IsLightningHere(null))
			{
				Game1.weatherIcon = 5;
			}
			if (Utility.isFestivalDay())
			{
				Game1.weatherIcon = 1;
			}
			if (Game1.IsGreenRainingHere(null))
			{
				Game1.weatherIcon = 999;
			}
		}

		// Token: 0x06000B71 RID: 2929 RVA: 0x0007B9E0 File Offset: 0x00079BE0
		public static void showEndOfNightStuff()
		{
			Game1.hooks.OnGame1_ShowEndOfNightStuff(delegate
			{
				if (!Game1.IsDedicatedHost)
				{
					bool shippingMenu = false;
					if (Game1.player.displayedShippedItems.Count > 0)
					{
						Game1.endOfNightMenus.Push(new ShippingMenu(Game1.player.displayedShippedItems));
						Game1.player.displayedShippedItems.Clear();
						shippingMenu = true;
					}
					bool levelUp = false;
					if (Game1.player.newLevels.Count > 0 && !shippingMenu)
					{
						Game1.endOfNightMenus.Push(new SaveGameMenu());
					}
					for (int i = Game1.player.newLevels.Count - 1; i >= 0; i--)
					{
						Game1.endOfNightMenus.Push(new LevelUpMenu(Game1.player.newLevels[i].X, Game1.player.newLevels[i].Y));
						levelUp = true;
					}
					if (Game1.player.farmingLevel.Value == 10 && Game1.player.miningLevel.Value == 10 && Game1.player.fishingLevel.Value == 10 && Game1.player.foragingLevel.Value == 10 && Game1.player.combatLevel.Value == 10 && Game1.player.mailReceived.Add("gotMasteryHint") && !Game1.player.locationsVisited.Contains("MasteryCave"))
					{
						Game1.morningQueue.Enqueue(delegate
						{
							Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:MasteryHint"));
						});
					}
					if (levelUp)
					{
						Game1.playSound("newRecord", null);
					}
					if (Game1.client != null && Game1.client.timedOut)
					{
						return;
					}
				}
				if (Game1.endOfNightMenus.Count > 0)
				{
					Game1.showingEndOfNightStuff = true;
					Game1.activeClickableMenu = Game1.endOfNightMenus.Pop();
					return;
				}
				Game1.showingEndOfNightStuff = true;
				Game1.activeClickableMenu = new SaveGameMenu();
			});
		}

		// Token: 0x06000B72 RID: 2930 RVA: 0x0007BA0C File Offset: 0x00079C0C
		public static void setGraphicsForSeason(bool onLoad = false)
		{
			foreach (GameLocation i in Game1.locations)
			{
				Season season = i.GetSeason();
				i.seasonUpdate(onLoad);
				i.updateSeasonalTileSheets(null);
				if (i.IsOutdoors)
				{
					switch (season)
					{
					case Season.Spring:
						Game1.eveningColor = new Color(255, 255, 0);
						continue;
					case Season.Summer:
						foreach (Object o in i.Objects.Values)
						{
							if (o.IsWeeds())
							{
								string qualifiedItemId = o.QualifiedItemId;
								if (!(qualifiedItemId == "(O)882") && !(qualifiedItemId == "(O)883") && !(qualifiedItemId == "(O)884"))
								{
									if (qualifiedItemId == "(O)792")
									{
										o.SetIdAndSprite(o.ParentSheetIndex + 1);
									}
									else if (Game1.random.NextDouble() < 0.3)
									{
										o.SetIdAndSprite(676);
									}
									else if (Game1.random.NextDouble() < 0.3)
									{
										o.SetIdAndSprite(677);
									}
								}
							}
						}
						Game1.eveningColor = new Color(255, 255, 0);
						continue;
					case Season.Fall:
						foreach (Object o2 in i.Objects.Values)
						{
							if (o2.IsWeeds())
							{
								string qualifiedItemId = o2.QualifiedItemId;
								if (!(qualifiedItemId == "(O)882") && !(qualifiedItemId == "(O)883") && !(qualifiedItemId == "(O)884"))
								{
									if (qualifiedItemId == "(O)793")
									{
										o2.SetIdAndSprite(o2.ParentSheetIndex + 1);
									}
									else
									{
										o2.SetIdAndSprite(Game1.random.Choose(678, 679));
									}
								}
							}
						}
						Game1.eveningColor = new Color(255, 255, 0);
						using (List<WeatherDebris>.Enumerator enumerator3 = Game1.debrisWeather.GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								WeatherDebris weatherDebris = enumerator3.Current;
								weatherDebris.which = 2;
							}
							continue;
						}
						break;
					case Season.Winter:
						break;
					default:
						continue;
					}
					foreach (KeyValuePair<Vector2, Object> pair in i.Objects.Pairs.ToArray<KeyValuePair<Vector2, Object>>())
					{
						Object o3 = pair.Value;
						if (o3.IsWeeds())
						{
							string qualifiedItemId = o3.QualifiedItemId;
							if (!(qualifiedItemId == "(O)882") && !(qualifiedItemId == "(O)883") && !(qualifiedItemId == "(O)884"))
							{
								i.Objects.Remove(pair.Key);
							}
						}
					}
					foreach (WeatherDebris weatherDebris2 in Game1.debrisWeather)
					{
						weatherDebris2.which = 3;
					}
					Game1.eveningColor = new Color(245, 225, 170);
				}
			}
		}

		// Token: 0x06000B73 RID: 2931 RVA: 0x0007BDE8 File Offset: 0x00079FE8
		public static void pauseThenMessage(int millisecondsPause, string message)
		{
			Game1.messageAfterPause = message;
			Game1.pauseTime = (float)millisecondsPause;
		}

		// Token: 0x06000B74 RID: 2932 RVA: 0x0007BDF7 File Offset: 0x00079FF7
		public static bool IsVisitingIslandToday(string npc_name)
		{
			return Game1.netWorldState.Value.IslandVisitors.Contains(npc_name);
		}

		// Token: 0x06000B75 RID: 2933 RVA: 0x0007BE10 File Offset: 0x0007A010
		public static bool shouldTimePass(bool ignore_multiplayer = false)
		{
			if (Game1.isFestival())
			{
				return false;
			}
			if (Game1.CurrentEvent != null && Game1.CurrentEvent.isWedding)
			{
				return false;
			}
			if (Game1.farmEvent != null)
			{
				return false;
			}
			if (Game1.IsMultiplayer && !ignore_multiplayer)
			{
				return !Game1.netWorldState.Value.IsTimePaused;
			}
			return !Game1.paused && !Game1.freezeControls && Game1.overlayMenu == null && !Game1.isTimePaused && !Game1.eventUp && (Game1.activeClickableMenu == null || Game1.activeClickableMenu is BobberBar) && (Game1.player.CanMove || Game1.player.UsingTool || Game1.player.forceTimePass);
		}

		// Token: 0x06000B76 RID: 2934 RVA: 0x0007BEC1 File Offset: 0x0007A0C1
		public static Farmer getPlayerOrEventFarmer()
		{
			if (Game1.eventUp && Game1.CurrentEvent != null && !Game1.CurrentEvent.isFestival && Game1.CurrentEvent.farmer != null)
			{
				return Game1.CurrentEvent.farmer;
			}
			return Game1.player;
		}

		// Token: 0x06000B77 RID: 2935 RVA: 0x0007BEFC File Offset: 0x0007A0FC
		public static void UpdateViewPort(bool overrideFreeze, Point centerPoint)
		{
			Game1.previousViewportPosition.X = (float)Game1.viewport.X;
			Game1.previousViewportPosition.Y = (float)Game1.viewport.Y;
			Farmer farmer = Game1.getPlayerOrEventFarmer();
			if (Game1.currentLocation == null)
			{
				return;
			}
			if (!Game1.viewportFreeze || overrideFreeze)
			{
				Microsoft.Xna.Framework.Rectangle viewportBounds = (Game1.viewportClampArea == Microsoft.Xna.Framework.Rectangle.Empty) ? new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.currentLocation.Map.DisplayWidth, Game1.currentLocation.Map.DisplayHeight) : Game1.viewportClampArea;
				Point playerPixel = farmer.StandingPixel;
				bool snapBack = Game1.forceSnapOnNextViewportUpdate || Math.Abs(Game1.currentViewportTarget.X + (float)(Game1.viewport.Width / 2) + (float)viewportBounds.X - (float)playerPixel.X) > 64f || Math.Abs(Game1.currentViewportTarget.Y + (float)(Game1.viewport.Height / 2) + (float)viewportBounds.Y - (float)playerPixel.Y) > 64f;
				if (centerPoint.X >= viewportBounds.X + Game1.viewport.Width / 2 && centerPoint.X <= viewportBounds.X + viewportBounds.Width - Game1.viewport.Width / 2)
				{
					if (farmer.isRafting || snapBack)
					{
						Game1.currentViewportTarget.X = (float)(centerPoint.X - Game1.viewport.Width / 2);
					}
					else if (Math.Abs(Game1.currentViewportTarget.X - (Game1.currentViewportTarget.X = (float)(centerPoint.X - Game1.viewport.Width / 2 + viewportBounds.X))) > farmer.getMovementSpeed())
					{
						Game1.currentViewportTarget.X = Game1.currentViewportTarget.X + (float)Math.Sign(Game1.currentViewportTarget.X - (Game1.currentViewportTarget.X = (float)(centerPoint.X - Game1.viewport.Width / 2 + viewportBounds.X))) * farmer.getMovementSpeed();
					}
				}
				else if (centerPoint.X < Game1.viewport.Width / 2 + viewportBounds.X && Game1.viewport.Width <= viewportBounds.Width)
				{
					if (farmer.isRafting || snapBack)
					{
						Game1.currentViewportTarget.X = (float)viewportBounds.X;
					}
					else if (Math.Abs(Game1.currentViewportTarget.X - (float)viewportBounds.X) > farmer.getMovementSpeed())
					{
						Game1.currentViewportTarget.X = Game1.currentViewportTarget.X - (float)Math.Sign(Game1.currentViewportTarget.X - (float)viewportBounds.X) * farmer.getMovementSpeed();
					}
				}
				else if (Game1.viewport.Width <= viewportBounds.Width)
				{
					if (farmer.isRafting || snapBack)
					{
						Game1.currentViewportTarget.X = (float)(viewportBounds.X + viewportBounds.Width - Game1.viewport.Width);
					}
					else if (Math.Abs(Game1.currentViewportTarget.X - (float)(viewportBounds.Width - Game1.viewport.Width)) > farmer.getMovementSpeed())
					{
					}
				}
				else if (viewportBounds.Width < Game1.viewport.Width)
				{
					if (farmer.isRafting || snapBack)
					{
						Game1.currentViewportTarget.X = (float)((viewportBounds.Width - Game1.viewport.Width) / 2 + viewportBounds.X);
					}
					else
					{
						Math.Abs(Game1.currentViewportTarget.X - (float)((viewportBounds.Width + viewportBounds.X - Game1.viewport.Width) / 2));
						farmer.getMovementSpeed();
					}
				}
				if (centerPoint.Y >= Game1.viewport.Height / 2 && centerPoint.Y <= Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height / 2)
				{
					if (farmer.isRafting || snapBack)
					{
						Game1.currentViewportTarget.Y = (float)(centerPoint.Y - Game1.viewport.Height / 2);
					}
					else if (Math.Abs(Game1.currentViewportTarget.Y - (float)(centerPoint.Y - Game1.viewport.Height / 2)) >= farmer.getMovementSpeed())
					{
						Game1.currentViewportTarget.Y = Game1.currentViewportTarget.Y - (float)Math.Sign(Game1.currentViewportTarget.Y - (float)(centerPoint.Y - Game1.viewport.Height / 2)) * farmer.getMovementSpeed();
					}
				}
				else if (centerPoint.Y < Game1.viewport.Height / 2 && Game1.viewport.Height <= Game1.currentLocation.Map.DisplayHeight)
				{
					if (farmer.isRafting || snapBack)
					{
						Game1.currentViewportTarget.Y = 0f;
					}
					else if (Math.Abs(Game1.currentViewportTarget.Y - 0f) > farmer.getMovementSpeed())
					{
						Game1.currentViewportTarget.Y = Game1.currentViewportTarget.Y - (float)Math.Sign(Game1.currentViewportTarget.Y - 0f) * farmer.getMovementSpeed();
					}
					Game1.currentViewportTarget.Y = 0f;
				}
				else if (Game1.viewport.Height <= Game1.currentLocation.Map.DisplayHeight)
				{
					if (farmer.isRafting || snapBack)
					{
						Game1.currentViewportTarget.Y = (float)(Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height);
					}
					else if (Math.Abs(Game1.currentViewportTarget.Y - (float)(Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height)) > farmer.getMovementSpeed())
					{
						Game1.currentViewportTarget.Y = Game1.currentViewportTarget.Y - (float)Math.Sign(Game1.currentViewportTarget.Y - (float)(Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height)) * farmer.getMovementSpeed();
					}
				}
				else if (Game1.currentLocation.Map.DisplayHeight < Game1.viewport.Height)
				{
					if (farmer.isRafting || snapBack)
					{
						Game1.currentViewportTarget.Y = (float)((Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height) / 2);
					}
					else if (Math.Abs(Game1.currentViewportTarget.Y - (float)((Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height) / 2)) > farmer.getMovementSpeed())
					{
						Game1.currentViewportTarget.Y = Game1.currentViewportTarget.Y - (float)Math.Sign(Game1.currentViewportTarget.Y - (float)((Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height) / 2)) * farmer.getMovementSpeed();
					}
				}
			}
			if (Game1.currentLocation.forceViewportPlayerFollow)
			{
				Game1.currentViewportTarget.X = farmer.Position.X - (float)(Game1.viewport.Width / 2);
				Game1.currentViewportTarget.Y = farmer.Position.Y - (float)(Game1.viewport.Height / 2);
			}
			bool forceSnap = Game1.forceSnapOnNextViewportUpdate;
			Game1.forceSnapOnNextViewportUpdate = false;
			if (Game1.currentViewportTarget.X == -2.1474836E+09f || (Game1.viewportFreeze && !overrideFreeze))
			{
				return;
			}
			int difference = (int)(Game1.currentViewportTarget.X - (float)Game1.viewport.X);
			if (Math.Abs(difference) > 128)
			{
				Game1.viewportPositionLerp.X = Game1.currentViewportTarget.X;
			}
			else
			{
				Game1.viewportPositionLerp.X = Game1.viewportPositionLerp.X + (float)difference * farmer.getMovementSpeed() * 0.03f;
			}
			difference = (int)(Game1.currentViewportTarget.Y - (float)Game1.viewport.Y);
			if (Math.Abs(difference) > 128)
			{
				Game1.viewportPositionLerp.Y = (float)((int)Game1.currentViewportTarget.Y);
			}
			else
			{
				Game1.viewportPositionLerp.Y = Game1.viewportPositionLerp.Y + (float)difference * farmer.getMovementSpeed() * 0.03f;
			}
			if (forceSnap)
			{
				Game1.viewportPositionLerp.X = (float)((int)Game1.currentViewportTarget.X);
				Game1.viewportPositionLerp.Y = (float)((int)Game1.currentViewportTarget.Y);
			}
			Game1.viewport.X = (int)Game1.viewportPositionLerp.X;
			Game1.viewport.Y = (int)Game1.viewportPositionLerp.Y;
		}

		// Token: 0x06000B78 RID: 2936 RVA: 0x0007C748 File Offset: 0x0007A948
		private void UpdateCharacters(GameTime time)
		{
			Event currentEvent = Game1.CurrentEvent;
			if (((currentEvent != null) ? currentEvent.farmer : null) != null && Game1.CurrentEvent.farmer != Game1.player)
			{
				Game1.CurrentEvent.farmer.Update(time, Game1.currentLocation);
			}
			Game1.player.Update(time, Game1.currentLocation);
			foreach (KeyValuePair<long, Farmer> v in Game1.otherFarmers)
			{
				if (v.Key != Game1.player.UniqueMultiplayerID)
				{
					v.Value.UpdateIfOtherPlayer(time);
				}
			}
		}

		// Token: 0x06000B79 RID: 2937 RVA: 0x0007C7FC File Offset: 0x0007A9FC
		public static void addMail(string mailName, bool noLetter = false, bool sendToEveryone = false)
		{
			if (sendToEveryone)
			{
				Game1.multiplayer.broadcastPartyWideMail(mailName, Multiplayer.PartyWideMessageQueue.SeenMail, noLetter);
				return;
			}
			mailName = mailName.Trim();
			mailName = mailName.Replace(Environment.NewLine, "");
			if (!Game1.player.hasOrWillReceiveMail(mailName))
			{
				if (noLetter)
				{
					Game1.player.mailReceived.Add(mailName);
					return;
				}
				Game1.player.mailbox.Add(mailName);
			}
		}

		// Token: 0x06000B7A RID: 2938 RVA: 0x0007C868 File Offset: 0x0007AA68
		public static void addMailForTomorrow(string mailName, bool noLetter = false, bool sendToEveryone = false)
		{
			if (sendToEveryone)
			{
				Game1.multiplayer.broadcastPartyWideMail(mailName, Multiplayer.PartyWideMessageQueue.MailForTomorrow, noLetter);
				return;
			}
			mailName = mailName.Trim();
			mailName = mailName.Replace(Environment.NewLine, "");
			if (!Game1.player.hasOrWillReceiveMail(mailName))
			{
				if (noLetter)
				{
					mailName += "%&NL&%";
				}
				Game1.player.mailForTomorrow.Add(mailName);
				if (sendToEveryone && Game1.IsMultiplayer)
				{
					foreach (Farmer farmer in Game1.otherFarmers.Values)
					{
						if (farmer != Game1.player && !Game1.player.hasOrWillReceiveMail(mailName))
						{
							farmer.mailForTomorrow.Add(mailName);
						}
					}
				}
			}
		}

		// Token: 0x06000B7B RID: 2939 RVA: 0x0007C938 File Offset: 0x0007AB38
		public static void drawDialogue(NPC speaker)
		{
			if (speaker.CurrentDialogue.Count == 0)
			{
				return;
			}
			Game1.activeClickableMenu = new DialogueBox(speaker.CurrentDialogue.Peek());
			DialogueBox dialogueBox = Game1.activeClickableMenu as DialogueBox;
			if (dialogueBox != null && dialogueBox.dialogueFinished)
			{
				Game1.activeClickableMenu = null;
				return;
			}
			Game1.dialogueUp = true;
			if (!Game1.eventUp)
			{
				Game1.player.Halt();
				Game1.player.CanMove = false;
			}
			if (speaker != null)
			{
				Game1.currentSpeaker = speaker;
			}
		}

		// Token: 0x06000B7C RID: 2940 RVA: 0x0007C9B0 File Offset: 0x0007ABB0
		public static void multipleDialogues(string[] messages)
		{
			Game1.activeClickableMenu = new DialogueBox(messages.ToList<string>());
			Game1.dialogueUp = true;
			Game1.player.CanMove = false;
		}

		// Token: 0x06000B7D RID: 2941 RVA: 0x0007C9D4 File Offset: 0x0007ABD4
		public static void drawDialogueNoTyping(string dialogue)
		{
			Game1.drawObjectDialogue(dialogue);
			DialogueBox dialogueBox = Game1.activeClickableMenu as DialogueBox;
			if (dialogueBox != null)
			{
				dialogueBox.showTyping = false;
			}
		}

		// Token: 0x06000B7E RID: 2942 RVA: 0x0007C9FC File Offset: 0x0007ABFC
		public static void drawDialogueNoTyping(List<string> dialogues)
		{
			Game1.drawObjectDialogue(dialogues);
			DialogueBox dialogueBox = Game1.activeClickableMenu as DialogueBox;
			if (dialogueBox != null)
			{
				dialogueBox.showTyping = false;
			}
		}

		// Token: 0x06000B7F RID: 2943 RVA: 0x0007CA24 File Offset: 0x0007AC24
		public static void DrawAnsweringMachineDialogue(NPC npc, string translationKey, params object[] substitutions)
		{
			Dialogue dialogue = Dialogue.FromTranslation(npc, translationKey, substitutions);
			dialogue.overridePortrait = Game1.temporaryContent.Load<Texture2D>("Portraits\\AnsweringMachine");
			Game1.DrawDialogue(dialogue);
		}

		// Token: 0x06000B80 RID: 2944 RVA: 0x0007CA48 File Offset: 0x0007AC48
		public static void DrawDialogue(NPC npc, string translationKey)
		{
			Game1.DrawDialogue(new Dialogue(npc, translationKey, false));
		}

		// Token: 0x06000B81 RID: 2945 RVA: 0x0007CA57 File Offset: 0x0007AC57
		public static void DrawDialogue(NPC npc, string translationKey, params object[] substitutions)
		{
			Game1.DrawDialogue(Dialogue.FromTranslation(npc, translationKey, substitutions));
		}

		// Token: 0x06000B82 RID: 2946 RVA: 0x0007CA68 File Offset: 0x0007AC68
		public static void DrawDialogue(Dialogue dialogue)
		{
			if (dialogue.speaker != null)
			{
				dialogue.speaker.CurrentDialogue.Push(dialogue);
				Game1.drawDialogue(dialogue.speaker);
				return;
			}
			Game1.activeClickableMenu = new DialogueBox(dialogue);
			Game1.dialogueUp = true;
			if (!Game1.eventUp)
			{
				Game1.player.Halt();
				Game1.player.CanMove = false;
			}
		}

		// Token: 0x06000B83 RID: 2947 RVA: 0x0007CAC8 File Offset: 0x0007ACC8
		private static void checkIfDialogueIsQuestion()
		{
			if (Game1.currentSpeaker != null && Game1.currentSpeaker.CurrentDialogue.Count > 0 && Game1.currentSpeaker.CurrentDialogue.Peek().isCurrentDialogueAQuestion())
			{
				Game1.questionChoices.Clear();
				Game1.isQuestion = true;
				List<NPCDialogueResponse> questions = Game1.currentSpeaker.CurrentDialogue.Peek().getNPCResponseOptions();
				for (int i = 0; i < questions.Count; i++)
				{
					Game1.questionChoices.Add(questions[i]);
				}
			}
		}

		// Token: 0x06000B84 RID: 2948 RVA: 0x0007CB4B File Offset: 0x0007AD4B
		public static void drawLetterMessage(string message)
		{
			Game1.activeClickableMenu = new LetterViewerMenu(message);
		}

		// Token: 0x06000B85 RID: 2949 RVA: 0x0007CB58 File Offset: 0x0007AD58
		public static void drawObjectDialogue(string dialogue)
		{
			IClickableMenu activeClickableMenu = Game1.activeClickableMenu;
			if (activeClickableMenu != null)
			{
				activeClickableMenu.emergencyShutDown();
			}
			Game1.activeClickableMenu = new DialogueBox(dialogue);
			Game1.player.CanMove = false;
			Game1.dialogueUp = true;
		}

		// Token: 0x06000B86 RID: 2950 RVA: 0x0007CB86 File Offset: 0x0007AD86
		public static void drawObjectDialogue(List<string> dialogue)
		{
			IClickableMenu activeClickableMenu = Game1.activeClickableMenu;
			if (activeClickableMenu != null)
			{
				activeClickableMenu.emergencyShutDown();
			}
			Game1.activeClickableMenu = new DialogueBox(dialogue);
			Game1.player.CanMove = false;
			Game1.dialogueUp = true;
		}

		// Token: 0x06000B87 RID: 2951 RVA: 0x0007CBB4 File Offset: 0x0007ADB4
		public static void drawObjectQuestionDialogue(string dialogue, Response[] choices, int width)
		{
			Game1.activeClickableMenu = new DialogueBox(dialogue, choices, width);
			Game1.dialogueUp = true;
			Game1.player.CanMove = false;
		}

		// Token: 0x06000B88 RID: 2952 RVA: 0x0007CBD4 File Offset: 0x0007ADD4
		public static void drawObjectQuestionDialogue(string dialogue, Response[] choices)
		{
			Game1.activeClickableMenu = new DialogueBox(dialogue, choices, 1200);
			Game1.dialogueUp = true;
			Game1.player.CanMove = false;
		}

		// Token: 0x1700018C RID: 396
		// (get) Token: 0x06000B89 RID: 2953 RVA: 0x0007CBF8 File Offset: 0x0007ADF8
		public static bool IsSummer
		{
			get
			{
				return Game1.season == Season.Summer;
			}
		}

		// Token: 0x1700018D RID: 397
		// (get) Token: 0x06000B8A RID: 2954 RVA: 0x0007CC02 File Offset: 0x0007AE02
		public static bool IsSpring
		{
			get
			{
				return Game1.season == Season.Spring;
			}
		}

		// Token: 0x1700018E RID: 398
		// (get) Token: 0x06000B8B RID: 2955 RVA: 0x0007CC0C File Offset: 0x0007AE0C
		public static bool IsFall
		{
			get
			{
				return Game1.season == Season.Fall;
			}
		}

		// Token: 0x1700018F RID: 399
		// (get) Token: 0x06000B8C RID: 2956 RVA: 0x0007CC16 File Offset: 0x0007AE16
		public static bool IsWinter
		{
			get
			{
				return Game1.season == Season.Winter;
			}
		}

		// Token: 0x06000B8D RID: 2957 RVA: 0x0007CC20 File Offset: 0x0007AE20
		public static void warpCharacter(NPC character, string targetLocationName, Point position)
		{
			Game1.warpCharacter(character, targetLocationName, new Vector2((float)position.X, (float)position.Y));
		}

		// Token: 0x06000B8E RID: 2958 RVA: 0x0007CC3C File Offset: 0x0007AE3C
		public static void warpCharacter(NPC character, string targetLocationName, Vector2 position)
		{
			Game1.warpCharacter(character, Game1.RequireLocation(targetLocationName, false), position);
		}

		// Token: 0x06000B8F RID: 2959 RVA: 0x0007CC4C File Offset: 0x0007AE4C
		public static void warpCharacter(NPC character, GameLocation targetLocation, Vector2 position)
		{
			using (IEnumerator<string> enumerator = Game1.netWorldState.Value.ActivePassiveFestivals.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					PassiveFestivalData festival;
					string newName;
					if (Utility.TryGetPassiveFestivalData(enumerator.Current, out festival) && Game1.dayOfMonth >= festival.StartDay && Game1.dayOfMonth <= festival.EndDay && festival.Season == Game1.season && festival.MapReplacements != null && festival.MapReplacements.TryGetValue(targetLocation.name.Value, out newName))
					{
						targetLocation = Game1.RequireLocation(newName, false);
					}
				}
			}
			if (targetLocation.name.Equals("Trailer") && Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
			{
				targetLocation = Game1.RequireLocation("Trailer_Big", false);
				if (position.X == 12f && position.Y == 9f)
				{
					position.X = 13f;
					position.Y = 24f;
				}
			}
			if (Game1.IsClient)
			{
				Game1.multiplayer.requestCharacterWarp(character, targetLocation, position);
				return;
			}
			if (!targetLocation.characters.Contains(character))
			{
				GameLocation currentLocation = character.currentLocation;
				if (currentLocation != null)
				{
					currentLocation.characters.Remove(character);
				}
				targetLocation.addCharacter(character);
			}
			character.isCharging = false;
			character.speed = 2;
			character.blockedInterval = 0;
			NPC.getTextureNameForCharacter(character.Name);
			character.position.X = position.X * 64f;
			character.position.Y = position.Y * 64f;
			if (character.CurrentDialogue.Count > 0 && character.CurrentDialogue.Peek().removeOnNextMove && character.Tile != character.DefaultPosition / 64f)
			{
				character.CurrentDialogue.Pop();
			}
			FarmHouse farmHouse = targetLocation as FarmHouse;
			if (farmHouse != null)
			{
				character.arriveAtFarmHouse(farmHouse);
			}
			else
			{
				character.arriveAt(targetLocation);
			}
			if (character.currentLocation != null && !character.currentLocation.Equals(targetLocation))
			{
				character.currentLocation.characters.Remove(character);
			}
			character.currentLocation = targetLocation;
		}

		// Token: 0x06000B90 RID: 2960 RVA: 0x0007CE80 File Offset: 0x0007B080
		public static LocationRequest getLocationRequest(string locationName, bool isStructure = false)
		{
			if (locationName == null)
			{
				throw new ArgumentException();
			}
			return new LocationRequest(locationName, isStructure, Game1.getLocationFromName(locationName, isStructure));
		}

		// Token: 0x06000B91 RID: 2961 RVA: 0x0007CE9C File Offset: 0x0007B09C
		public static void warpHome()
		{
			LocationRequest locationRequest = Game1.getLocationRequest(Game1.player.homeLocation.Value, false);
			locationRequest.OnWarp += delegate()
			{
				Game1.player.position.Set(Utility.PointToVector2((Game1.currentLocation as FarmHouse).GetPlayerBedSpot()) * 64f);
			};
			Game1.warpFarmer(locationRequest, 5, 9, Game1.player.FacingDirection);
		}

		// Token: 0x06000B92 RID: 2962 RVA: 0x0007CEF5 File Offset: 0x0007B0F5
		public static void warpFarmer(string locationName, int tileX, int tileY, bool flip)
		{
			Game1.warpFarmer(Game1.getLocationRequest(locationName, false), tileX, tileY, flip ? ((Game1.player.FacingDirection + 2) % 4) : Game1.player.FacingDirection);
		}

		// Token: 0x06000B93 RID: 2963 RVA: 0x0007CF22 File Offset: 0x0007B122
		public static void warpFarmer(string locationName, int tileX, int tileY, int facingDirectionAfterWarp)
		{
			Game1.warpFarmer(Game1.getLocationRequest(locationName, false), tileX, tileY, facingDirectionAfterWarp);
		}

		// Token: 0x06000B94 RID: 2964 RVA: 0x0007CF33 File Offset: 0x0007B133
		public static void warpFarmer(string locationName, int tileX, int tileY, int facingDirectionAfterWarp, bool isStructure)
		{
			Game1.warpFarmer(Game1.getLocationRequest(locationName, isStructure), tileX, tileY, facingDirectionAfterWarp);
		}

		// Token: 0x06000B95 RID: 2965 RVA: 0x0007CF45 File Offset: 0x0007B145
		public virtual bool ShouldDismountOnWarp(Horse mount, GameLocation old_location, GameLocation new_location)
		{
			return mount != null && (Game1.currentLocation != null && Game1.currentLocation.IsOutdoors && new_location != null) && !new_location.IsOutdoors;
		}

		// Token: 0x06000B96 RID: 2966 RVA: 0x0007CF70 File Offset: 0x0007B170
		public static void warpFarmer(LocationRequest locationRequest, int tileX, int tileY, int facingDirectionAfterWarp)
		{
			int warp_offset_x = Game1.nextFarmerWarpOffsetX;
			int warp_offset_y = Game1.nextFarmerWarpOffsetY;
			Game1.nextFarmerWarpOffsetX = 0;
			Game1.nextFarmerWarpOffsetY = 0;
			using (IEnumerator<string> enumerator = Game1.netWorldState.Value.ActivePassiveFestivals.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					PassiveFestivalData festival;
					string newName;
					if (Utility.TryGetPassiveFestivalData(enumerator.Current, out festival) && Game1.dayOfMonth >= festival.StartDay && Game1.dayOfMonth <= festival.EndDay && festival.Season == Game1.season && festival.MapReplacements != null && festival.MapReplacements.TryGetValue(locationRequest.Name, out newName))
					{
						locationRequest = Game1.getLocationRequest(newName, false);
					}
				}
			}
			string name = locationRequest.Name;
			if (!(name == "BusStop"))
			{
				if (!(name == "Farm"))
				{
					if (!(name == "IslandSouth"))
					{
						if (!(name == "Trailer"))
						{
							if (name == "Club")
							{
								if (!Game1.player.hasClubCard)
								{
									locationRequest = Game1.getLocationRequest("SandyHouse", false);
									locationRequest.OnWarp += delegate()
									{
										NPC i = Game1.currentLocation.getCharacterFromName("Bouncer");
										if (i != null)
										{
											Vector2 placementTile = new Vector2(17f, 4f);
											i.showTextAboveHead(Game1.content.LoadString("Strings\\Locations:Club_Bouncer_TextAboveHead" + (Game1.random.Next(2) + 1).ToString()), null, 2, 3000, 0);
											int idNum = Game1.random.Next();
											Game1.currentLocation.playSound("thudStep", null, null, SoundContext.Default);
											Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite[]
											{
												new TemporaryAnimatedSprite(288, 100f, 1, 24, placementTile * 64f, true, false, Game1.currentLocation, Game1.player)
												{
													shakeIntensity = 0.5f,
													shakeIntensityChange = 0.002f,
													extraInfoForEndBehavior = idNum,
													endFunction = new TemporaryAnimatedSprite.endBehavior(Game1.currentLocation.removeTemporarySpritesWithID)
												},
												new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 0f) * 4f, true, false, 0.0263f, 0f, Color.Yellow, 4f, 0f, 0f, 0f, false)
												{
													id = idNum
												},
												new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 0f) * 4f, true, true, 0.0263f, 0f, Color.Orange, 4f, 0f, 0f, 0f, false)
												{
													delayBeforeAnimationStart = 100,
													id = idNum
												},
												new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 0f) * 4f, true, false, 0.0263f, 0f, Color.White, 3f, 0f, 0f, 0f, false)
												{
													delayBeforeAnimationStart = 200,
													id = idNum
												}
											});
											Game1.currentLocation.netAudio.StartPlaying("fuse");
										}
									};
									tileX = 17;
									tileY = 4;
								}
							}
						}
						else if (Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
						{
							locationRequest = Game1.getLocationRequest("Trailer_Big", false);
							tileX = 13;
							tileY = 24;
						}
					}
					else if (tileX <= 15 && tileY <= 6)
					{
						tileX = 21;
						tileY = 43;
					}
				}
				else
				{
					GameLocation currentLocation = Game1.currentLocation;
					string a = (currentLocation != null) ? currentLocation.NameOrUniqueName : null;
					if (!(a == "FarmCave"))
					{
						if (!(a == "Forest"))
						{
							Point tile2;
							if (!(a == "BusStop"))
							{
								if (a == "Backwoods")
								{
									Point tile;
									if (tileX == 40 && tileY == 0 && Game1.getFarm().TryGetMapPropertyAs("BackwoodsEntry", out tile, false))
									{
										tileX = tile.X;
										tileY = tile.Y;
									}
								}
							}
							else if (tileX == 79 && tileY == 17 && Game1.getFarm().TryGetMapPropertyAs("BusStopEntry", out tile2, false))
							{
								tileX = tile2.X;
								tileY = tile2.Y;
							}
						}
						else if (tileX == 41 && tileY == 64)
						{
							Point tile3;
							if (Game1.getFarm().TryGetMapPropertyAs("ForestEntry", out tile3, false))
							{
								tileX = tile3.X;
								tileY = tile3.Y;
							}
							else
							{
								int num = Game1.whichFarm;
								if (num != 5)
								{
									if (num == 6)
									{
										tileX = 82;
										tileY = 103;
									}
								}
								else
								{
									tileX = 40;
									tileY = 64;
								}
							}
						}
					}
					else if (tileX == 34 && tileY == 6)
					{
						Point tile4;
						if (Game1.getFarm().TryGetMapPropertyAs("FarmCaveEntry", out tile4, false))
						{
							tileX = tile4.X;
							tileY = tile4.Y;
						}
						else
						{
							int num = Game1.whichFarm;
							if (num != 5)
							{
								if (num == 6)
								{
									tileX = 34;
									tileY = 16;
								}
							}
							else
							{
								tileX = 30;
								tileY = 36;
							}
						}
					}
				}
			}
			else if (tileX < 10)
			{
				tileX = 10;
			}
			if (VolcanoDungeon.IsGeneratedLevel(locationRequest.Name))
			{
				warp_offset_x = 0;
				warp_offset_y = 0;
			}
			if (Game1.player.isRidingHorse() && Game1.currentLocation != null)
			{
				GameLocation next_location = locationRequest.Location;
				if (next_location == null)
				{
					next_location = Game1.getLocationFromName(locationRequest.Name);
				}
				if (Game1.game1.ShouldDismountOnWarp(Game1.player.mount, Game1.currentLocation, next_location))
				{
					Game1.player.mount.dismount(false);
					warp_offset_x = 0;
					warp_offset_y = 0;
				}
			}
			if (Game1.weatherIcon == 1 && Game1.whereIsTodaysFest != null && locationRequest.Name.Equals(Game1.whereIsTodaysFest) && !Game1.warpingForForcedRemoteEvent)
			{
				string[] timeParts = ArgUtility.SplitBySpace(Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + Game1.currentSeason + Game1.dayOfMonth.ToString())["conditions"].Split('/', StringSplitOptions.None)[1]);
				if (Game1.timeOfDay <= Convert.ToInt32(timeParts[1]))
				{
					if (Game1.timeOfDay < Convert.ToInt32(timeParts[0]))
					{
						GameLocation currentLocation2 = Game1.currentLocation;
						if (!(((currentLocation2 != null) ? currentLocation2.Name : null) == "Hospital"))
						{
							Game1.player.Position = Game1.player.lastPosition;
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2973"));
							return;
						}
						locationRequest = Game1.getLocationRequest("BusStop", false);
						tileX = 34;
						tileY = 23;
					}
					else
					{
						if (Game1.IsMultiplayer)
						{
							Game1.netReady.SetLocalReady("festivalStart", true);
							Game1.activeClickableMenu = new ReadyCheckDialog("festivalStart", true, delegate(Farmer who)
							{
								Game1.exitActiveMenu();
								if (Game1.player.mount != null)
								{
									Game1.player.mount.dismount(false);
									warp_offset_x = 0;
									warp_offset_y = 0;
								}
								Game1.performWarpFarmer(locationRequest, tileX, tileY, facingDirectionAfterWarp);
							}, null);
							return;
						}
						if (Game1.player.mount != null)
						{
							Game1.player.mount.dismount(false);
							warp_offset_x = 0;
							warp_offset_y = 0;
						}
					}
				}
			}
			tileX += warp_offset_x;
			tileY += warp_offset_y;
			Game1.performWarpFarmer(locationRequest, tileX, tileY, facingDirectionAfterWarp);
		}

		// Token: 0x06000B97 RID: 2967 RVA: 0x0007D5E0 File Offset: 0x0007B7E0
		private static void performWarpFarmer(LocationRequest locationRequest, int tileX, int tileY, int facingDirectionAfterWarp)
		{
			if (locationRequest.Location != null)
			{
				if (tileX >= locationRequest.Location.Map.Layers[0].LayerWidth - 1)
				{
					tileX--;
				}
				if (Game1.IsMasterGame)
				{
					locationRequest.Location.hostSetup();
				}
			}
			Game1.log.Verbose("Warping to " + locationRequest.Name);
			if (Game1.player.IsSitting())
			{
				Game1.player.StopSitting(false);
			}
			if (Game1.player.UsingTool)
			{
				Game1.player.completelyStopAnimatingOrDoingAction();
			}
			Game1.player.previousLocationName = ((Game1.player.currentLocation != null) ? Game1.player.currentLocation.name.Value : "");
			Game1.locationRequest = locationRequest;
			Game1.xLocationAfterWarp = tileX;
			Game1.yLocationAfterWarp = tileY;
			Game1._isWarping = true;
			Game1.facingDirectionAfterWarp = facingDirectionAfterWarp;
			Game1.fadeScreenToBlack();
			Game1.setRichPresence("location", locationRequest.Name);
			if (Game1.IsDedicatedHost)
			{
				Game1.fadeToBlackAlpha = 1.1f;
				Game1.fadeToBlack = true;
				Game1.nonWarpFade = false;
			}
		}

		// Token: 0x06000B98 RID: 2968 RVA: 0x0007D6F4 File Offset: 0x0007B8F4
		private static void notifyServerOfWarp(bool needsLocationInfo)
		{
			if (Game1.locationRequest == null)
			{
				return;
			}
			byte flags = (byte)(((Game1.locationRequest.IsStructure > false) ? 1 : 0) | (Game1.warpingForForcedRemoteEvent ? 2 : 0) | (needsLocationInfo ? 4 : 0));
			switch (Game1.facingDirectionAfterWarp)
			{
			case 1:
				flags |= 16;
				goto IL_66;
			case 2:
				flags |= 32;
				goto IL_66;
			case 3:
				flags |= 64;
				goto IL_66;
			}
			flags |= 8;
			IL_66:
			Game1.client.sendMessage(5, new object[]
			{
				(short)Game1.xLocationAfterWarp,
				(short)Game1.yLocationAfterWarp,
				Game1.locationRequest.Name,
				flags
			});
		}

		// Token: 0x06000B99 RID: 2969 RVA: 0x0007D7AC File Offset: 0x0007B9AC
		public static void requestLocationInfoFromServer()
		{
			Game1.notifyServerOfWarp(true);
			Game1.currentLocation = null;
			Game1.player.Position = new Vector2((float)(Game1.xLocationAfterWarp * 64), (float)(Game1.yLocationAfterWarp * 64 - (Game1.player.Sprite.getHeight() - 32) + 16));
			Game1.player.faceDirection(Game1.facingDirectionAfterWarp);
		}

		// Token: 0x06000B9A RID: 2970 RVA: 0x0007D80C File Offset: 0x0007BA0C
		public static T GetCharacterWhere<T>(Func<T, bool> check, bool includeEventActors = false) where T : NPC
		{
			T match = default(T);
			T fallback = default(T);
			Utility.ForEachCharacter(delegate(NPC rawNpc)
			{
				T npc = rawNpc as T;
				if (npc != null && check(npc))
				{
					GameLocation currentLocation = npc.currentLocation;
					bool? flag = (currentLocation != null) ? new bool?(currentLocation.IsActiveLocation()) : null;
					if (flag != null && flag.GetValueOrDefault())
					{
						match = npc;
						return false;
					}
					fallback = npc;
				}
				return true;
			}, includeEventActors);
			T result;
			if ((result = match) == null)
			{
				result = fallback;
			}
			return result;
		}

		// Token: 0x06000B9B RID: 2971 RVA: 0x0007D868 File Offset: 0x0007BA68
		public static T GetCharacterOfType<T>(bool includeEventActors = false) where T : NPC
		{
			T match = default(T);
			T fallback = default(T);
			Utility.ForEachCharacter(delegate(NPC rawNpc)
			{
				T npc = rawNpc as T;
				if (npc != null)
				{
					GameLocation currentLocation = rawNpc.currentLocation;
					bool? flag = (currentLocation != null) ? new bool?(currentLocation.IsActiveLocation()) : null;
					if (flag != null && flag.GetValueOrDefault())
					{
						match = npc;
						return false;
					}
					fallback = npc;
				}
				return true;
			}, includeEventActors);
			T result;
			if ((result = match) == null)
			{
				result = fallback;
			}
			return result;
		}

		// Token: 0x06000B9C RID: 2972 RVA: 0x0007D8BC File Offset: 0x0007BABC
		public static T getCharacterFromName<T>(string name, bool mustBeVillager = true, bool includeEventActors = false) where T : NPC
		{
			T match = default(T);
			T fallback = default(T);
			Utility.ForEachCharacter(delegate(NPC rawNpc)
			{
				T npc = rawNpc as T;
				if (npc != null && npc.Name == name && (!mustBeVillager || npc.IsVillager))
				{
					GameLocation currentLocation = npc.currentLocation;
					bool? flag = (currentLocation != null) ? new bool?(currentLocation.IsActiveLocation()) : null;
					if (flag != null && flag.GetValueOrDefault())
					{
						match = npc;
						return false;
					}
					fallback = npc;
				}
				return true;
			}, includeEventActors);
			T result;
			if ((result = match) == null)
			{
				result = fallback;
			}
			return result;
		}

		// Token: 0x06000B9D RID: 2973 RVA: 0x0007D91C File Offset: 0x0007BB1C
		public static NPC getCharacterFromName(string name, bool mustBeVillager = true, bool includeEventActors = false)
		{
			NPC match = null;
			NPC fallback = null;
			Utility.ForEachCharacter(delegate(NPC npc)
			{
				if (npc.Name == name && (!mustBeVillager || npc.IsVillager))
				{
					GameLocation currentLocation = npc.currentLocation;
					bool? flag = (currentLocation != null) ? new bool?(currentLocation.IsActiveLocation()) : null;
					if (flag != null && flag.GetValueOrDefault())
					{
						match = npc;
						return false;
					}
					fallback = npc;
				}
				return true;
			}, includeEventActors);
			return match ?? fallback;
		}

		// Token: 0x06000B9E RID: 2974 RVA: 0x0007D970 File Offset: 0x0007BB70
		public static NPC RequireCharacter(string name, bool mustBeVillager = true)
		{
			NPC characterFromName = Game1.getCharacterFromName(name, mustBeVillager, false);
			if (characterFromName == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Required ");
				defaultInterpolatedStringHandler.AppendFormatted(mustBeVillager ? "villager" : "NPC");
				defaultInterpolatedStringHandler.AppendLiteral(" '");
				defaultInterpolatedStringHandler.AppendFormatted(name);
				defaultInterpolatedStringHandler.AppendLiteral("' not found.");
				throw new KeyNotFoundException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			return characterFromName;
		}

		// Token: 0x06000B9F RID: 2975 RVA: 0x0007D9E4 File Offset: 0x0007BBE4
		public static T RequireCharacter<T>(string name, bool mustBeVillager = true) where T : NPC
		{
			NPC npc = Game1.getCharacterFromName(name, mustBeVillager, false);
			T cast = npc as T;
			if (cast != null)
			{
				return cast;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler;
			if (npc != null)
			{
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(48, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Can't convert NPC '");
				defaultInterpolatedStringHandler.AppendFormatted(name);
				defaultInterpolatedStringHandler.AppendLiteral("' from '");
				defaultInterpolatedStringHandler.AppendFormatted((npc != null) ? npc.GetType().FullName : null);
				defaultInterpolatedStringHandler.AppendLiteral("' to the required '");
				defaultInterpolatedStringHandler.AppendFormatted(typeof(T).FullName);
				defaultInterpolatedStringHandler.AppendLiteral("'.");
				throw new InvalidCastException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 2);
			defaultInterpolatedStringHandler.AppendLiteral("Required ");
			defaultInterpolatedStringHandler.AppendFormatted(mustBeVillager ? "villager" : "NPC");
			defaultInterpolatedStringHandler.AppendLiteral(" '");
			defaultInterpolatedStringHandler.AppendFormatted(name);
			defaultInterpolatedStringHandler.AppendLiteral("' not found.");
			throw new KeyNotFoundException(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x06000BA0 RID: 2976 RVA: 0x0007DAEC File Offset: 0x0007BCEC
		public static GameLocation RequireLocation(string name, bool isStructure = false)
		{
			GameLocation locationFromName = Game1.getLocationFromName(name, isStructure);
			if (locationFromName == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Required ");
				defaultInterpolatedStringHandler.AppendFormatted(isStructure ? "structure " : "");
				defaultInterpolatedStringHandler.AppendLiteral("location '");
				defaultInterpolatedStringHandler.AppendFormatted(name);
				defaultInterpolatedStringHandler.AppendLiteral("' not found.");
				throw new KeyNotFoundException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			return locationFromName;
		}

		// Token: 0x06000BA1 RID: 2977 RVA: 0x0007DB60 File Offset: 0x0007BD60
		public static TLocation RequireLocation<TLocation>(string name, bool isStructure = false) where TLocation : GameLocation
		{
			GameLocation location = Game1.getLocationFromName(name, isStructure);
			TLocation cast = location as TLocation;
			if (cast != null)
			{
				return cast;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler;
			if (location != null)
			{
				defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(51, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Can't convert location ");
				defaultInterpolatedStringHandler.AppendFormatted(name);
				defaultInterpolatedStringHandler.AppendLiteral(" from '");
				defaultInterpolatedStringHandler.AppendFormatted((location != null) ? location.GetType().FullName : null);
				defaultInterpolatedStringHandler.AppendLiteral("' to the required '");
				defaultInterpolatedStringHandler.AppendFormatted(typeof(TLocation).FullName);
				defaultInterpolatedStringHandler.AppendLiteral("'.");
				throw new InvalidCastException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 2);
			defaultInterpolatedStringHandler.AppendLiteral("Required ");
			defaultInterpolatedStringHandler.AppendFormatted(isStructure ? "structure " : "");
			defaultInterpolatedStringHandler.AppendLiteral("location '");
			defaultInterpolatedStringHandler.AppendFormatted(name);
			defaultInterpolatedStringHandler.AppendLiteral("' not found.");
			throw new KeyNotFoundException(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x06000BA2 RID: 2978 RVA: 0x0007DC65 File Offset: 0x0007BE65
		public static GameLocation getLocationFromName(string name)
		{
			return Game1.getLocationFromName(name, false);
		}

		// Token: 0x06000BA3 RID: 2979 RVA: 0x0007DC70 File Offset: 0x0007BE70
		public static GameLocation getLocationFromName(string name, bool isStructure)
		{
			if (string.IsNullOrEmpty(name))
			{
				return null;
			}
			if (Game1.currentLocation != null)
			{
				if (!isStructure)
				{
					if (Game1.currentLocation.name.Value.EqualsIgnoreCase(name))
					{
						return Game1.currentLocation;
					}
					if (Game1.currentLocation.isStructure.Value && Game1.currentLocation.Root != null && Game1.currentLocation.Root.Value.NameOrUniqueName.EqualsIgnoreCase(name))
					{
						return Game1.currentLocation.Root.Value;
					}
				}
				else if (Game1.currentLocation.NameOrUniqueName == name)
				{
					return Game1.currentLocation;
				}
			}
			GameLocation cached_location;
			if (Game1._locationLookup.TryGetValue(name, out cached_location))
			{
				return cached_location;
			}
			return Game1.getLocationFromNameInLocationsList(name, isStructure);
		}

		// Token: 0x06000BA4 RID: 2980 RVA: 0x0007DD30 File Offset: 0x0007BF30
		public static GameLocation getLocationFromNameInLocationsList(string name, bool isStructure = false)
		{
			for (int i = 0; i < Game1.locations.Count; i++)
			{
				GameLocation location = Game1.locations[i];
				if (!isStructure)
				{
					if (location.Name.EqualsIgnoreCase(name))
					{
						Game1._locationLookup[location.Name] = location;
						return location;
					}
				}
				else
				{
					GameLocation buildingIndoors = Game1.findStructure(location, name);
					if (buildingIndoors != null)
					{
						Game1._locationLookup[name] = buildingIndoors;
						return buildingIndoors;
					}
				}
			}
			if (MineShaft.IsGeneratedLevel(name))
			{
				return MineShaft.GetMine(name);
			}
			if (VolcanoDungeon.IsGeneratedLevel(name))
			{
				return VolcanoDungeon.GetLevel(name, false);
			}
			if (!isStructure)
			{
				return Game1.getLocationFromName(name, true);
			}
			return null;
		}

		// Token: 0x06000BA5 RID: 2981 RVA: 0x0007DDC6 File Offset: 0x0007BFC6
		public static void flushLocationLookup()
		{
			Game1._locationLookup.Clear();
		}

		// Token: 0x06000BA6 RID: 2982 RVA: 0x0007DDD4 File Offset: 0x0007BFD4
		public static void removeLocationFromLocationLookup(string nameOrUniqueName)
		{
			Game1._locationLookup.RemoveWhere((KeyValuePair<string, GameLocation> p) => p.Value.NameOrUniqueName == nameOrUniqueName);
		}

		// Token: 0x06000BA7 RID: 2983 RVA: 0x0007DE08 File Offset: 0x0007C008
		public static void removeLocationFromLocationLookup(GameLocation location)
		{
			Game1._locationLookup.RemoveWhere((KeyValuePair<string, GameLocation> p) => p.Value == location);
		}

		// Token: 0x06000BA8 RID: 2984 RVA: 0x0007DE3C File Offset: 0x0007C03C
		public static GameLocation findStructure(GameLocation parentLocation, string name)
		{
			foreach (Building building in parentLocation.buildings)
			{
				if (building.HasIndoorsName(name))
				{
					return building.GetIndoors();
				}
			}
			return null;
		}

		// Token: 0x06000BA9 RID: 2985 RVA: 0x0007DEA0 File Offset: 0x0007C0A0
		public static void addNewFarmBuildingMaps()
		{
			FarmHouse home = Utility.getHomeOfFarmer(Game1.player);
			if (Game1.player.HouseUpgradeLevel >= 1 && home.Map.Id.Equals("FarmHouse"))
			{
				home.updateMap();
			}
		}

		// Token: 0x06000BAA RID: 2986 RVA: 0x0007DEE4 File Offset: 0x0007C0E4
		public static void PassOutNewDay()
		{
			Game1.player.lastSleepLocation.Value = Game1.currentLocation.NameOrUniqueName;
			Game1.player.lastSleepPoint.Value = Game1.player.TilePoint;
			if (!Game1.IsMultiplayer)
			{
				Game1.NewDay(0f);
				return;
			}
			Game1.player.FarmerSprite.setCurrentSingleFrame(5, 3000, false, false);
			Game1.player.FarmerSprite.PauseForSingleAnimation = true;
			Game1.player.passedOut = true;
			if (Game1.activeClickableMenu != null)
			{
				Game1.activeClickableMenu.emergencyShutDown();
				Game1.exitActiveMenu();
			}
			Game1.activeClickableMenu = new ReadyCheckDialog("sleep", false, delegate(Farmer _)
			{
				Game1.NewDay(0f);
			}, null);
		}

		// Token: 0x06000BAB RID: 2987 RVA: 0x0007DFB0 File Offset: 0x0007C1B0
		public static void NewDay(float timeToPause)
		{
			ReadyCheckDialog readyCheckDialog = Game1.activeClickableMenu as ReadyCheckDialog;
			if (readyCheckDialog != null && readyCheckDialog.checkName == "sleep" && !readyCheckDialog.isCancelable())
			{
				readyCheckDialog.confirm();
			}
			Game1.currentMinigame = null;
			Game1.newDay = true;
			Game1.newDaySync.create();
			if (Game1.player.isInBed.Value || Game1.player.passedOut)
			{
				Game1.nonWarpFade = true;
				Game1.screenFade.FadeScreenToBlack(Game1.player.passedOut ? 1.1f : 0f, true);
				Game1.player.Halt();
				Game1.player.currentEyes = 1;
				Game1.player.blinkTimer = -4000;
				Game1.player.CanMove = false;
				Game1.player.passedOut = false;
				Game1.pauseTime = timeToPause;
			}
			if (Game1.activeClickableMenu != null && !Game1.dialogueUp)
			{
				Game1.activeClickableMenu.emergencyShutDown();
				Game1.exitActiveMenu();
			}
		}

		// Token: 0x06000BAC RID: 2988 RVA: 0x0007E0A4 File Offset: 0x0007C2A4
		public static void screenGlowOnce(Color glowColor, bool hold, float rate = 0.005f, float maxAlpha = 0.3f)
		{
			Game1.screenGlowMax = maxAlpha;
			Game1.screenGlowRate = rate;
			Game1.screenGlowAlpha = 0f;
			Game1.screenGlowUp = true;
			Game1.screenGlowColor = glowColor;
			Game1.screenGlow = true;
			Game1.screenGlowHold = hold;
		}

		// Token: 0x06000BAD RID: 2989 RVA: 0x0007E0D4 File Offset: 0x0007C2D4
		public static string shortDayNameFromDayOfSeason(int dayOfSeason)
		{
			switch (dayOfSeason % 7)
			{
			case 0:
				return "Sun";
			case 1:
				return "Mon";
			case 2:
				return "Tue";
			case 3:
				return "Wed";
			case 4:
				return "Thu";
			case 5:
				return "Fri";
			case 6:
				return "Sat";
			default:
				return "";
			}
		}

		// Token: 0x06000BAE RID: 2990 RVA: 0x0007E138 File Offset: 0x0007C338
		public static string shortDayDisplayNameFromDayOfSeason(int dayOfSeason)
		{
			if (dayOfSeason < 0)
			{
				return string.Empty;
			}
			return Game1._shortDayDisplayName[dayOfSeason % 7];
		}

		// Token: 0x06000BAF RID: 2991 RVA: 0x0007E150 File Offset: 0x0007C350
		public static void runTestEvent()
		{
			StreamReader file = new StreamReader("test_event.txt");
			string locationName = file.ReadLine();
			string event_string = file.ReadToEnd();
			event_string = event_string.Replace("\r\n", "/").Replace("\n", "/");
			Game1.log.Verbose("Running test event: " + event_string);
			LocationRequest location_request = Game1.getLocationRequest(locationName, false);
			location_request.OnWarp += delegate()
			{
				Game1.currentLocation.currentEvent = new Event(event_string, null);
				Game1.currentLocation.checkForEvents();
			};
			int x = 8;
			int y = 8;
			Utility.getDefaultWarpLocation(locationName, ref x, ref y);
			Game1.warpFarmer(location_request, x, y, Game1.player.FacingDirection);
		}

		// Token: 0x06000BB0 RID: 2992 RVA: 0x0007E200 File Offset: 0x0007C400
		public static bool isMusicContextActiveButNotPlaying(MusicContext music_context = MusicContext.Default)
		{
			if (Game1._activeMusicContext != music_context)
			{
				return false;
			}
			if (Game1.morningSongPlayAction != null)
			{
				return false;
			}
			string currentTrack = Game1.getMusicTrackName(music_context);
			return currentTrack == "none" || (Game1.currentSong != null && Game1.currentSong.Name == currentTrack && !Game1.currentSong.IsPlaying);
		}

		// Token: 0x06000BB1 RID: 2993 RVA: 0x0007E25E File Offset: 0x0007C45E
		public static bool IsMusicContextActive(MusicContext music_context = MusicContext.Default)
		{
			return Game1._activeMusicContext != music_context;
		}

		// Token: 0x06000BB2 RID: 2994 RVA: 0x0007E26B File Offset: 0x0007C46B
		public static bool doesMusicContextHaveTrack(MusicContext music_context = MusicContext.Default)
		{
			return Game1._requestedMusicTracks.ContainsKey(music_context);
		}

		// Token: 0x06000BB3 RID: 2995 RVA: 0x0007E278 File Offset: 0x0007C478
		public static string getMusicTrackName(MusicContext music_context = MusicContext.Default)
		{
			KeyValuePair<string, bool> trackData;
			if (Game1._requestedMusicTracks.TryGetValue(music_context, out trackData))
			{
				return trackData.Key;
			}
			if (music_context == MusicContext.Default)
			{
				return Game1.getMusicTrackName(MusicContext.SubLocation);
			}
			return "none";
		}

		// Token: 0x06000BB4 RID: 2996 RVA: 0x0007E2AB File Offset: 0x0007C4AB
		public static void stopMusicTrack(MusicContext music_context)
		{
			if (Game1._requestedMusicTracks.Remove(music_context))
			{
				if (music_context == MusicContext.Default)
				{
					Game1.stopMusicTrack(MusicContext.SubLocation);
				}
				Game1.UpdateRequestedMusicTrack();
			}
		}

		// Token: 0x06000BB5 RID: 2997 RVA: 0x0007E2C8 File Offset: 0x0007C4C8
		public static void changeMusicTrack(string newTrackName, bool track_interruptable = false, MusicContext music_context = MusicContext.Default)
		{
			if (newTrackName == null)
			{
				return;
			}
			if (music_context == MusicContext.Default)
			{
				if (Game1.morningSongPlayAction != null)
				{
					if (Game1.delayedActions.Contains(Game1.morningSongPlayAction))
					{
						Game1.delayedActions.Remove(Game1.morningSongPlayAction);
					}
					Game1.morningSongPlayAction = null;
				}
				if (Game1.IsGreenRainingHere(null) && !Game1.currentLocation.InIslandContext() && Game1.IsRainingHere(Game1.currentLocation) && !newTrackName.Equals("rain"))
				{
					return;
				}
			}
			if (music_context == MusicContext.Default || music_context == MusicContext.SubLocation)
			{
				Game1.IsPlayingBackgroundMusic = false;
				Game1.IsPlayingOutdoorsAmbience = false;
				Game1.IsPlayingNightAmbience = false;
				Game1.IsPlayingTownMusic = false;
				Game1.IsPlayingMorningSong = false;
			}
			if (music_context != MusicContext.ImportantSplitScreenMusic && !Game1.player.songsHeard.Contains(newTrackName))
			{
				Utility.farmerHeardSong(newTrackName);
			}
			Game1._requestedMusicTracks[music_context] = new KeyValuePair<string, bool>(newTrackName, track_interruptable);
			Game1.UpdateRequestedMusicTrack();
		}

		// Token: 0x06000BB6 RID: 2998 RVA: 0x0007E390 File Offset: 0x0007C590
		public static void UpdateRequestedMusicTrack()
		{
			Game1._activeMusicContext = MusicContext.Default;
			KeyValuePair<string, bool> requested_track_data = new KeyValuePair<string, bool>("none", true);
			for (int i = 0; i < 6; i++)
			{
				MusicContext context = (MusicContext)i;
				KeyValuePair<string, bool> trackData;
				if (Game1._requestedMusicTracks.TryGetValue(context, out trackData))
				{
					if (context != MusicContext.ImportantSplitScreenMusic)
					{
						Game1._activeMusicContext = context;
					}
					requested_track_data = trackData;
				}
			}
			if (requested_track_data.Key != Game1.requestedMusicTrack || requested_track_data.Value != Game1.requestedMusicTrackOverrideable)
			{
				Game1.requestedMusicDirty = true;
				Game1.requestedMusicTrack = requested_track_data.Key;
				Game1.requestedMusicTrackOverrideable = requested_track_data.Value;
			}
		}

		// Token: 0x06000BB7 RID: 2999 RVA: 0x0007E418 File Offset: 0x0007C618
		public static void enterMine(int whatLevel, int? forceLayout = null)
		{
			Game1.warpFarmer(MineShaft.GetLevelName(whatLevel, forceLayout), 6, 6, 2);
			Game1.player.temporarilyInvincible = true;
			Game1.player.temporaryInvincibilityTimer = 0;
			Game1.player.flashDuringThisTemporaryInvincibility = false;
			Game1.player.currentTemporaryInvincibilityDuration = 1000;
		}

		// Token: 0x06000BB8 RID: 3000 RVA: 0x0007E464 File Offset: 0x0007C664
		public static Season GetSeasonForLocation(GameLocation location)
		{
			if (location == null)
			{
				return Game1.season;
			}
			return location.GetSeason();
		}

		// Token: 0x06000BB9 RID: 3001 RVA: 0x0007E475 File Offset: 0x0007C675
		public static int GetSeasonIndexForLocation(GameLocation location)
		{
			if (location == null)
			{
				return Game1.seasonIndex;
			}
			return location.GetSeasonIndex();
		}

		// Token: 0x06000BBA RID: 3002 RVA: 0x0007E486 File Offset: 0x0007C686
		public static string GetSeasonKeyForLocation(GameLocation location)
		{
			return ((location != null) ? location.GetSeasonKey() : null) ?? Game1.currentSeason;
		}

		// Token: 0x06000BBB RID: 3003 RVA: 0x0007E49D File Offset: 0x0007C69D
		public static void getPlatformAchievement(string which)
		{
			Program.sdk.GetAchievement(which);
		}

		// Token: 0x06000BBC RID: 3004 RVA: 0x0007E4AA File Offset: 0x0007C6AA
		public static void getSteamAchievement(string which)
		{
			if (which.Equals("0"))
			{
				which = "a0";
			}
			Game1.getPlatformAchievement(which);
		}

		// Token: 0x06000BBD RID: 3005 RVA: 0x0007E4C8 File Offset: 0x0007C6C8
		public static void getAchievement(int which, bool allowBroadcasting = true)
		{
			string rawData;
			if (!Game1.player.achievements.Contains(which) && Game1.gameMode == 3 && Game1.achievements.TryGetValue(which, out rawData))
			{
				string achievementName = rawData.Split('^', StringSplitOptions.None)[0];
				Game1.player.achievements.Add(which);
				if (which < 32 && allowBroadcasting)
				{
					if (Game1.stats.isSharedAchievement(which))
					{
						Game1.multiplayer.sendSharedAchievementMessage(which);
					}
					else
					{
						string farmerName = Game1.player.Name;
						if (farmerName == "")
						{
							farmerName = TokenStringBuilder.LocalizedText("Strings\\UI:Chat_PlayerJoinedNewName");
						}
						Game1.multiplayer.globalChatInfoMessage("Achievement", new string[]
						{
							farmerName,
							TokenStringBuilder.AchievementName(which)
						});
					}
				}
				Game1.playSound("achievement", null);
				Game1.addHUDMessage(HUDMessage.ForAchievement(achievementName));
				Game1.player.autoGenerateActiveDialogueEvent("achievement_" + which.ToString(), 4);
				Game1.getPlatformAchievement(which.ToString());
				if (!Game1.player.hasOrWillReceiveMail("hatter"))
				{
					Game1.addMailForTomorrow("hatter", false, false);
				}
			}
		}

		// Token: 0x06000BBE RID: 3006 RVA: 0x0007E5F4 File Offset: 0x0007C7F4
		public static void createMultipleObjectDebris(string id, int xTile, int yTile, int number)
		{
			for (int i = 0; i < number; i++)
			{
				Game1.createObjectDebris(id, xTile, yTile, -1, 0, 1f, null);
			}
		}

		// Token: 0x06000BBF RID: 3007 RVA: 0x0007E620 File Offset: 0x0007C820
		public static void createMultipleObjectDebris(string id, int xTile, int yTile, int number, GameLocation location)
		{
			for (int i = 0; i < number; i++)
			{
				Game1.createObjectDebris(id, xTile, yTile, -1, 0, 1f, location);
			}
		}

		// Token: 0x06000BC0 RID: 3008 RVA: 0x0007E64C File Offset: 0x0007C84C
		public static void createMultipleObjectDebris(string id, int xTile, int yTile, int number, float velocityMultiplier)
		{
			for (int i = 0; i < number; i++)
			{
				Game1.createObjectDebris(id, xTile, yTile, -1, 0, velocityMultiplier, null);
			}
		}

		// Token: 0x06000BC1 RID: 3009 RVA: 0x0007E674 File Offset: 0x0007C874
		public static void createMultipleObjectDebris(string id, int xTile, int yTile, int number, long who)
		{
			for (int i = 0; i < number; i++)
			{
				Game1.createObjectDebris(id, xTile, yTile, who);
			}
		}

		// Token: 0x06000BC2 RID: 3010 RVA: 0x0007E698 File Offset: 0x0007C898
		public static void createMultipleObjectDebris(string id, int xTile, int yTile, int number, long who, GameLocation location)
		{
			for (int i = 0; i < number; i++)
			{
				Game1.createObjectDebris(id, xTile, yTile, who, location);
			}
		}

		// Token: 0x06000BC3 RID: 3011 RVA: 0x0007E6BD File Offset: 0x0007C8BD
		public static void createDebris(int debrisType, int xTile, int yTile, int numberOfChunks)
		{
			Game1.createDebris(debrisType, xTile, yTile, numberOfChunks, Game1.currentLocation);
		}

		// Token: 0x06000BC4 RID: 3012 RVA: 0x0007E6D0 File Offset: 0x0007C8D0
		public static void createDebris(int debrisType, int xTile, int yTile, int numberOfChunks, GameLocation location)
		{
			if (location == null)
			{
				location = Game1.currentLocation;
			}
			location.debris.Add(new Debris(debrisType, numberOfChunks, new Vector2((float)(xTile * 64 + 32), (float)(yTile * 64 + 32)), Game1.player.getStandingPosition(), 1f));
		}

		// Token: 0x06000BC5 RID: 3013 RVA: 0x0007E720 File Offset: 0x0007C920
		public static Debris createItemDebris(Item item, Vector2 pixelOrigin, int direction, GameLocation location = null, int groundLevel = -1, bool flopFish = false)
		{
			if (location == null)
			{
				location = Game1.currentLocation;
			}
			Vector2 targetLocation = new Vector2(pixelOrigin.X, pixelOrigin.Y);
			switch (direction)
			{
			case -1:
				targetLocation = Game1.player.getStandingPosition();
				break;
			case 0:
				pixelOrigin.Y -= 16f + (float)Game1.recentMultiplayerRandom.Next(32);
				targetLocation.Y -= 35.2f;
				break;
			case 1:
				pixelOrigin.X += 16f;
				pixelOrigin.Y -= (float)(32 - Game1.recentMultiplayerRandom.Next(8));
				targetLocation.X += 128f;
				break;
			case 2:
				pixelOrigin.Y += (float)Game1.recentMultiplayerRandom.Next(16);
				targetLocation.Y += 64f;
				break;
			case 3:
				pixelOrigin.X -= 16f;
				pixelOrigin.Y -= (float)(32 - Game1.recentMultiplayerRandom.Next(8));
				targetLocation.X -= 128f;
				break;
			}
			Debris d = new Debris(item, pixelOrigin, targetLocation);
			if (flopFish && item.Category == -4)
			{
				d.floppingFish.Value = true;
			}
			if (groundLevel != -1)
			{
				d.chunkFinalYLevel = groundLevel;
			}
			location.debris.Add(d);
			return d;
		}

		// Token: 0x06000BC6 RID: 3014 RVA: 0x0007E888 File Offset: 0x0007CA88
		public static void createMultipleItemDebris(Item item, Vector2 pixelOrigin, int direction, GameLocation location = null, int groundLevel = -1, bool flopFish = false)
		{
			int stack = item.Stack;
			item.Stack = 1;
			Game1.createItemDebris(item, pixelOrigin, (direction == -1) ? Game1.random.Next(4) : direction, location, groundLevel, flopFish);
			for (int i = 1; i < stack; i++)
			{
				Game1.createItemDebris(item.getOne(), pixelOrigin, (direction == -1) ? Game1.random.Next(4) : direction, location, groundLevel, flopFish);
			}
		}

		// Token: 0x06000BC7 RID: 3015 RVA: 0x0007E8F4 File Offset: 0x0007CAF4
		public static void createRadialDebris(GameLocation location, int debrisType, int xTile, int yTile, int numberOfChunks, bool resource, int groundLevel = -1, bool item = false, Color? color = null)
		{
			if (groundLevel == -1)
			{
				groundLevel = yTile * 64 + 32;
			}
			Vector2 debrisOrigin = new Vector2((float)(xTile * 64 + 64), (float)(yTile * 64 + 64));
			if (item)
			{
				while (numberOfChunks > 0)
				{
					Vector2 offset;
					switch (Game1.random.Next(4))
					{
					case 0:
						offset = new Vector2(-64f, 0f);
						break;
					case 1:
						offset = new Vector2(64f, 0f);
						break;
					case 2:
						offset = new Vector2(0f, 64f);
						break;
					default:
						offset = new Vector2(0f, -64f);
						break;
					}
					Item debris = ItemRegistry.Create("(O)" + debrisType.ToString(), 1, 0, false);
					location.debris.Add(new Debris(debris, debrisOrigin, debrisOrigin + offset));
					numberOfChunks--;
				}
			}
			if (resource)
			{
				location.debris.Add(new Debris(debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(-64f, 0f), 1f));
				numberOfChunks++;
				location.debris.Add(new Debris(debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(64f, 0f), 1f));
				numberOfChunks++;
				location.debris.Add(new Debris(debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(0f, -64f), 1f));
				numberOfChunks++;
				location.debris.Add(new Debris(debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(0f, 64f), 1f));
				return;
			}
			location.debris.Add(new Debris(debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(-64f, 0f), groundLevel, color));
			numberOfChunks++;
			location.debris.Add(new Debris(debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(64f, 0f), groundLevel, color));
			numberOfChunks++;
			location.debris.Add(new Debris(debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(0f, -64f), groundLevel, color));
			numberOfChunks++;
			location.debris.Add(new Debris(debrisType, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(0f, 64f), groundLevel, color));
		}

		// Token: 0x06000BC8 RID: 3016 RVA: 0x0007EB85 File Offset: 0x0007CD85
		public static void createRadialDebris(GameLocation location, string texture, Microsoft.Xna.Framework.Rectangle sourcerectangle, int xTile, int yTile, int numberOfChunks)
		{
			Game1.createRadialDebris(location, texture, sourcerectangle, xTile, yTile, numberOfChunks, yTile);
		}

		// Token: 0x06000BC9 RID: 3017 RVA: 0x0007EB96 File Offset: 0x0007CD96
		public static void createRadialDebris(GameLocation location, string texture, Microsoft.Xna.Framework.Rectangle sourcerectangle, int xTile, int yTile, int numberOfChunks, int groundLevelTile)
		{
			Game1.createRadialDebris(location, texture, sourcerectangle, 8, xTile * 64 + 32 + Game1.random.Next(32), yTile * 64 + 32 + Game1.random.Next(32), numberOfChunks, groundLevelTile);
		}

		// Token: 0x06000BCA RID: 3018 RVA: 0x0007EBD0 File Offset: 0x0007CDD0
		public static void createRadialDebris(GameLocation location, string texture, Microsoft.Xna.Framework.Rectangle sourcerectangle, int sizeOfSourceRectSquares, int xPosition, int yPosition, int numberOfChunks, int groundLevelTile)
		{
			Vector2 debrisOrigin = new Vector2((float)xPosition, (float)yPosition);
			location.debris.Add(new Debris(texture, sourcerectangle, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(-64f, 0f), groundLevelTile * 64, sizeOfSourceRectSquares));
			location.debris.Add(new Debris(texture, sourcerectangle, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(64f, 0f), groundLevelTile * 64, sizeOfSourceRectSquares));
			location.debris.Add(new Debris(texture, sourcerectangle, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(0f, -64f), groundLevelTile * 64, sizeOfSourceRectSquares));
			location.debris.Add(new Debris(texture, sourcerectangle, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2(0f, 64f), groundLevelTile * 64, sizeOfSourceRectSquares));
		}

		// Token: 0x06000BCB RID: 3019 RVA: 0x0007ECB4 File Offset: 0x0007CEB4
		public static void createRadialDebris_MoreNatural(GameLocation location, string texture, Microsoft.Xna.Framework.Rectangle sourcerectangle, int sizeOfSourceRectSquares, int xPosition, int yPosition, int numberOfChunks, int groundLevel)
		{
			Vector2 debrisOrigin = new Vector2((float)xPosition, (float)yPosition);
			for (int i = 0; i < numberOfChunks; i++)
			{
				location.debris.Add(new Debris(texture, sourcerectangle, numberOfChunks / 4, debrisOrigin, debrisOrigin + new Vector2((float)Game1.random.Next(-64, 64), (float)Game1.random.Next(-64, 64)), groundLevel + Game1.random.Next(-32, 32), sizeOfSourceRectSquares));
			}
		}

		// Token: 0x06000BCC RID: 3020 RVA: 0x0007ED30 File Offset: 0x0007CF30
		public static void createRadialDebris(GameLocation location, string texture, Microsoft.Xna.Framework.Rectangle sourcerectangle, int sizeOfSourceRectSquares, int xPosition, int yPosition, int numberOfChunks, int groundLevelTile, Color color)
		{
			Game1.createRadialDebris(location, texture, sourcerectangle, sizeOfSourceRectSquares, xPosition, yPosition, numberOfChunks, groundLevelTile, color, 1f);
		}

		// Token: 0x06000BCD RID: 3021 RVA: 0x0007ED58 File Offset: 0x0007CF58
		public static void createRadialDebris(GameLocation location, string texture, Microsoft.Xna.Framework.Rectangle sourcerectangle, int sizeOfSourceRectSquares, int xPosition, int yPosition, int numberOfChunks, int groundLevelTile, Color color, float scale)
		{
			Vector2 debrisOrigin = new Vector2((float)xPosition, (float)yPosition);
			while (numberOfChunks > 0)
			{
				switch (Game1.random.Next(4))
				{
				case 0:
				{
					Debris d = new Debris(texture, sourcerectangle, 1, debrisOrigin, debrisOrigin + new Vector2(-64f, 0f), groundLevelTile * 64, sizeOfSourceRectSquares);
					d.nonSpriteChunkColor.Value = color;
					if (location != null)
					{
						location.debris.Add(d);
					}
					d.Chunks[0].scale = scale;
					break;
				}
				case 1:
				{
					Debris d = new Debris(texture, sourcerectangle, 1, debrisOrigin, debrisOrigin + new Vector2(64f, 0f), groundLevelTile * 64, sizeOfSourceRectSquares);
					d.nonSpriteChunkColor.Value = color;
					if (location != null)
					{
						location.debris.Add(d);
					}
					d.Chunks[0].scale = scale;
					break;
				}
				case 2:
				{
					Debris d = new Debris(texture, sourcerectangle, 1, debrisOrigin, debrisOrigin + new Vector2((float)Game1.random.Next(-64, 64), -64f), groundLevelTile * 64, sizeOfSourceRectSquares);
					d.nonSpriteChunkColor.Value = color;
					if (location != null)
					{
						location.debris.Add(d);
					}
					d.Chunks[0].scale = scale;
					break;
				}
				case 3:
				{
					Debris d = new Debris(texture, sourcerectangle, 1, debrisOrigin, debrisOrigin + new Vector2((float)Game1.random.Next(-64, 64), 64f), groundLevelTile * 64, sizeOfSourceRectSquares);
					d.nonSpriteChunkColor.Value = color;
					if (location != null)
					{
						location.debris.Add(d);
					}
					d.Chunks[0].scale = scale;
					break;
				}
				}
				numberOfChunks--;
			}
		}

		// Token: 0x06000BCE RID: 3022 RVA: 0x0007EF1C File Offset: 0x0007D11C
		public static void createObjectDebris(string id, int xTile, int yTile, long whichPlayer)
		{
			Farmer forPlayer = Game1.GetPlayer(whichPlayer, false) ?? Game1.player;
			Game1.currentLocation.debris.Add(new Debris(id, new Vector2((float)(xTile * 64 + 32), (float)(yTile * 64 + 32)), forPlayer.getStandingPosition()));
		}

		// Token: 0x06000BCF RID: 3023 RVA: 0x0007EF6C File Offset: 0x0007D16C
		public static void createObjectDebris(string id, int xTile, int yTile, long whichPlayer, GameLocation location)
		{
			Farmer forPlayer = Game1.GetPlayer(whichPlayer, false) ?? Game1.player;
			location.debris.Add(new Debris(id, new Vector2((float)(xTile * 64 + 32), (float)(yTile * 64 + 32)), forPlayer.getStandingPosition()));
		}

		// Token: 0x06000BD0 RID: 3024 RVA: 0x0007EFB7 File Offset: 0x0007D1B7
		public static void createObjectDebris(string id, int xTile, int yTile, GameLocation location)
		{
			Game1.createObjectDebris(id, xTile, yTile, -1, 0, 1f, location);
		}

		// Token: 0x06000BD1 RID: 3025 RVA: 0x0007EFCC File Offset: 0x0007D1CC
		public static void createObjectDebris(string id, int xTile, int yTile, int groundLevel = -1, int itemQuality = 0, float velocityMultiplyer = 1f, GameLocation location = null)
		{
			if (location == null)
			{
				location = Game1.currentLocation;
			}
			Debris d = new Debris(id, new Vector2((float)(xTile * 64 + 32), (float)(yTile * 64 + 32)), Game1.player.getStandingPosition())
			{
				itemQuality = itemQuality
			};
			foreach (Chunk chunk in d.Chunks)
			{
				chunk.xVelocity.Value *= velocityMultiplyer;
				chunk.yVelocity.Value *= velocityMultiplyer;
			}
			if (groundLevel != -1)
			{
				d.chunkFinalYLevel = groundLevel;
			}
			location.debris.Add(d);
		}

		// Token: 0x06000BD2 RID: 3026 RVA: 0x0007F090 File Offset: 0x0007D290
		[Obsolete("Use GetPlayer instead. Equivalent usage: `GetPlayer(id, onlineOnly: true) ?? Game1.MasterPlayer`.")]
		public static Farmer getFarmer(long id)
		{
			return Game1.GetPlayer(id, true) ?? Game1.MasterPlayer;
		}

		// Token: 0x06000BD3 RID: 3027 RVA: 0x0007F0A2 File Offset: 0x0007D2A2
		[Obsolete("Use GetPlayer instead.")]
		public static Farmer getFarmerMaybeOffline(long id)
		{
			return Game1.GetPlayer(id, false);
		}

		// Token: 0x06000BD4 RID: 3028 RVA: 0x0007F0AC File Offset: 0x0007D2AC
		[NullableContext(2)]
		public static Farmer GetPlayer(long id, bool onlyOnline = false)
		{
			if (Game1.MasterPlayer.UniqueMultiplayerID == id)
			{
				return Game1.MasterPlayer;
			}
			Farmer onlineFarmhand;
			if (Game1.otherFarmers.TryGetValue(id, out onlineFarmhand))
			{
				return onlineFarmhand;
			}
			Farmer offlineFarmhand;
			if (!onlyOnline && Game1.netWorldState.Value.farmhandData.TryGetValue(id, out offlineFarmhand))
			{
				return offlineFarmhand;
			}
			return null;
		}

		// Token: 0x06000BD5 RID: 3029 RVA: 0x0007F0FC File Offset: 0x0007D2FC
		public static IEnumerable<Farmer> getAllFarmers()
		{
			return Enumerable.Repeat<Farmer>(Game1.MasterPlayer, 1).Concat(Game1.getAllFarmhands());
		}

		// Token: 0x06000BD6 RID: 3030 RVA: 0x0007F113 File Offset: 0x0007D313
		public static FarmerCollection getOnlineFarmers()
		{
			return Game1._onlineFarmers;
		}

		// Token: 0x06000BD7 RID: 3031 RVA: 0x0007F11A File Offset: 0x0007D31A
		public static IEnumerable<Farmer> getAllFarmhands()
		{
			foreach (Farmer farmer in Game1.netWorldState.Value.farmhandData.Values)
			{
				if (farmer.isActive())
				{
					yield return Game1.otherFarmers[farmer.UniqueMultiplayerID];
				}
				else
				{
					yield return farmer;
				}
			}
			NetDictionary<long, Farmer, NetRef<Farmer>, SerializableDictionary<long, Farmer>, NetLongDictionary<Farmer, NetRef<Farmer>>>.ValuesCollection.Enumerator enumerator = default(NetDictionary<long, Farmer, NetRef<Farmer>, SerializableDictionary<long, Farmer>, NetLongDictionary<Farmer, NetRef<Farmer>>>.ValuesCollection.Enumerator);
			yield break;
			yield break;
		}

		// Token: 0x06000BD8 RID: 3032 RVA: 0x0007F123 File Offset: 0x0007D323
		public static IEnumerable<Farmer> getOfflineFarmhands()
		{
			foreach (Farmer farmer in Game1.netWorldState.Value.farmhandData.Values)
			{
				if (!farmer.isActive())
				{
					yield return farmer;
				}
			}
			NetDictionary<long, Farmer, NetRef<Farmer>, SerializableDictionary<long, Farmer>, NetLongDictionary<Farmer, NetRef<Farmer>>>.ValuesCollection.Enumerator enumerator = default(NetDictionary<long, Farmer, NetRef<Farmer>, SerializableDictionary<long, Farmer>, NetLongDictionary<Farmer, NetRef<Farmer>>>.ValuesCollection.Enumerator);
			yield break;
			yield break;
		}

		// Token: 0x06000BD9 RID: 3033 RVA: 0x0007F12C File Offset: 0x0007D32C
		public static void farmerFindsArtifact(string itemId)
		{
			Item item = ItemRegistry.Create(itemId, 1, 0, false);
			Game1.player.addItemToInventoryBool(item, false);
		}

		// Token: 0x06000BDA RID: 3034 RVA: 0x0007F150 File Offset: 0x0007D350
		public static bool doesHUDMessageExist(string s)
		{
			for (int i = 0; i < Game1.hudMessages.Count; i++)
			{
				if (s.Equals(Game1.hudMessages[i].message))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000BDB RID: 3035 RVA: 0x0007F190 File Offset: 0x0007D390
		public static void addHUDMessage(HUDMessage message)
		{
			if (message.type != null || message.whatType != 0)
			{
				for (int i = 0; i < Game1.hudMessages.Count; i++)
				{
					if (message.type != null && message.type == Game1.hudMessages[i].type)
					{
						Game1.hudMessages[i].number = Game1.hudMessages[i].number + message.number;
						Game1.hudMessages[i].timeLeft = 3500f;
						Game1.hudMessages[i].transparency = 1f;
						if (Game1.hudMessages[i].number > 50000)
						{
							HUDMessage.numbersEasterEgg(Game1.hudMessages[i].number);
						}
						return;
					}
					if (message.whatType == Game1.hudMessages[i].whatType && message.whatType != 1 && message.message != null && message.message.Equals(Game1.hudMessages[i].message))
					{
						Game1.hudMessages[i].timeLeft = message.timeLeft;
						Game1.hudMessages[i].transparency = 1f;
						return;
					}
				}
			}
			Game1.hudMessages.Add(message);
			for (int j = Game1.hudMessages.Count - 1; j >= 0; j--)
			{
				if (Game1.hudMessages[j].noIcon)
				{
					HUDMessage tmp = Game1.hudMessages[j];
					Game1.hudMessages.RemoveAt(j);
					Game1.hudMessages.Add(tmp);
				}
			}
		}

		// Token: 0x06000BDC RID: 3036 RVA: 0x0007F33C File Offset: 0x0007D53C
		public static void showSwordswipeAnimation(int direction, Vector2 source, float animationSpeed, bool flip)
		{
			switch (direction)
			{
			case 0:
				Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(-1, animationSpeed, 5, 1, new Vector2(source.X + 32f, source.Y), false, false, !flip, -1.5707964f));
				return;
			case 1:
				Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(-1, animationSpeed, 5, 1, new Vector2(source.X + 96f + 16f, source.Y + 48f), false, flip, false, flip ? -3.1415927f : 0f));
				return;
			case 2:
				Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(-1, animationSpeed, 5, 1, new Vector2(source.X + 32f, source.Y + 128f), false, false, !flip, 1.5707964f));
				return;
			case 3:
				Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(-1, animationSpeed, 5, 1, new Vector2(source.X - 32f - 16f, source.Y + 48f), false, !flip, false, flip ? -3.1415927f : 0f));
				return;
			default:
				return;
			}
		}

		// Token: 0x06000BDD RID: 3037 RVA: 0x0007F47C File Offset: 0x0007D67C
		public static void removeDebris(Debris.DebrisType type)
		{
			Game1.currentLocation.debris.RemoveWhere((Debris debris) => debris.debrisType.Value == type);
		}

		// Token: 0x06000BDE RID: 3038 RVA: 0x0007F4B4 File Offset: 0x0007D6B4
		public static void toolAnimationDone(Farmer who)
		{
			float oldStamina = Game1.player.Stamina;
			if (who.CurrentTool == null)
			{
				return;
			}
			if (who.Stamina > 0f)
			{
				int powerupLevel = 1;
				Vector2 actionTile = who.GetToolLocation(false);
				FishingRod rod = who.CurrentTool as FishingRod;
				if (rod != null && rod.isFishing)
				{
					who.canReleaseTool = false;
				}
				else if (!(who.CurrentTool is FishingRod))
				{
					who.UsingTool = false;
					if (who.CurrentTool.QualifiedItemId == "(T)WateringCan")
					{
						switch (who.FacingDirection)
						{
						case 0:
						case 2:
							who.CurrentTool.DoFunction(Game1.currentLocation, (int)actionTile.X, (int)actionTile.Y, powerupLevel, who);
							break;
						case 1:
						case 3:
							who.CurrentTool.DoFunction(Game1.currentLocation, (int)actionTile.X, (int)actionTile.Y, powerupLevel, who);
							break;
						}
					}
					else if (who.CurrentTool is MeleeWeapon)
					{
						who.CurrentTool.CurrentParentTileIndex = who.CurrentTool.IndexOfMenuItemView;
					}
					else
					{
						if (who.CurrentTool.QualifiedItemId == "(T)ReturnScepter")
						{
							who.CurrentTool.CurrentParentTileIndex = who.CurrentTool.IndexOfMenuItemView;
						}
						who.CurrentTool.DoFunction(Game1.currentLocation, (int)actionTile.X, (int)actionTile.Y, powerupLevel, who);
					}
				}
				else
				{
					who.UsingTool = false;
				}
			}
			else if (who.CurrentTool.instantUse.Value)
			{
				who.CurrentTool.DoFunction(Game1.currentLocation, 0, 0, 0, who);
			}
			else
			{
				who.UsingTool = false;
			}
			who.lastClick = Vector2.Zero;
			if (who.IsLocalPlayer && !Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift))
			{
				who.setRunning(Game1.options.autoRun, false);
			}
			if (!who.UsingTool && who.FarmerSprite.PauseForSingleAnimation)
			{
				who.FarmerSprite.StopAnimation();
			}
			if (Game1.player.Stamina <= 0f && oldStamina > 0f)
			{
				Game1.player.doEmote(36);
			}
		}

		// Token: 0x06000BDF RID: 3039 RVA: 0x0007F6DC File Offset: 0x0007D8DC
		public static bool pressActionButton(KeyboardState currentKBState, MouseState currentMouseState, GamePadState currentPadState)
		{
			if (Game1.IsChatting)
			{
				currentKBState = default(KeyboardState);
			}
			if (Game1.dialogueTyping)
			{
				bool consume = true;
				Game1.dialogueTyping = false;
				if (Game1.currentSpeaker != null)
				{
					Game1.currentDialogueCharacterIndex = Game1.currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue().Length;
				}
				else if (Game1.currentObjectDialogue.Count > 0)
				{
					Game1.currentDialogueCharacterIndex = Game1.currentObjectDialogue.Peek().Length;
				}
				else
				{
					consume = false;
				}
				Game1.dialogueTypingInterval = 0;
				Game1.oldKBState = currentKBState;
				Game1.oldMouseState = Game1.input.GetMouseState();
				Game1.oldPadState = currentPadState;
				if (consume)
				{
					Game1.playSound("dialogueCharacterClose", null);
					return false;
				}
			}
			if (Game1.dialogueUp)
			{
				if (Game1.isQuestion)
				{
					Game1.isQuestion = false;
					if (Game1.currentSpeaker != null)
					{
						if (Game1.currentSpeaker.CurrentDialogue.Peek().chooseResponse(Game1.questionChoices[Game1.currentQuestionChoice]))
						{
							Game1.currentDialogueCharacterIndex = 1;
							Game1.dialogueTyping = true;
							Game1.oldKBState = currentKBState;
							Game1.oldMouseState = Game1.input.GetMouseState();
							Game1.oldPadState = currentPadState;
							return false;
						}
					}
					else
					{
						Game1.dialogueUp = false;
						if (Game1.eventUp && Game1.currentLocation.afterQuestion == null)
						{
							Game1.currentLocation.currentEvent.answerDialogue(Game1.currentLocation.lastQuestionKey, Game1.currentQuestionChoice);
							Game1.currentQuestionChoice = 0;
							Game1.oldKBState = currentKBState;
							Game1.oldMouseState = Game1.input.GetMouseState();
							Game1.oldPadState = currentPadState;
						}
						else if (Game1.currentLocation.answerDialogue(Game1.questionChoices[Game1.currentQuestionChoice]))
						{
							Game1.currentQuestionChoice = 0;
							Game1.oldKBState = currentKBState;
							Game1.oldMouseState = Game1.input.GetMouseState();
							Game1.oldPadState = currentPadState;
							return false;
						}
						if (Game1.dialogueUp)
						{
							Game1.currentDialogueCharacterIndex = 1;
							Game1.dialogueTyping = true;
							Game1.oldKBState = currentKBState;
							Game1.oldMouseState = Game1.input.GetMouseState();
							Game1.oldPadState = currentPadState;
							return false;
						}
					}
					Game1.currentQuestionChoice = 0;
				}
				string exitDialogue = null;
				if (Game1.currentSpeaker != null)
				{
					if (Game1.currentSpeaker.immediateSpeak)
					{
						Game1.currentSpeaker.immediateSpeak = false;
						return false;
					}
					exitDialogue = ((Game1.currentSpeaker.CurrentDialogue.Count > 0) ? Game1.currentSpeaker.CurrentDialogue.Peek().exitCurrentDialogue() : null);
				}
				if (exitDialogue == null)
				{
					if (Game1.currentSpeaker != null && Game1.currentSpeaker.CurrentDialogue.Count > 0 && Game1.currentSpeaker.CurrentDialogue.Peek().isOnFinalDialogue() && Game1.currentSpeaker.CurrentDialogue.Count > 0)
					{
						Game1.currentSpeaker.CurrentDialogue.Pop();
					}
					Game1.dialogueUp = false;
					if (Game1.messagePause)
					{
						Game1.pauseTime = 500f;
					}
					if (Game1.currentObjectDialogue.Count > 0)
					{
						Game1.currentObjectDialogue.Dequeue();
					}
					Game1.currentDialogueCharacterIndex = 0;
					if (Game1.currentObjectDialogue.Count > 0)
					{
						Game1.dialogueUp = true;
						Game1.questionChoices.Clear();
						Game1.oldKBState = currentKBState;
						Game1.oldMouseState = Game1.input.GetMouseState();
						Game1.oldPadState = currentPadState;
						Game1.dialogueTyping = true;
						return false;
					}
					if (Game1.currentSpeaker != null && !Game1.currentSpeaker.Name.Equals("Gunther") && !Game1.eventUp && !Game1.currentSpeaker.doingEndOfRouteAnimation.Value)
					{
						Game1.currentSpeaker.doneFacingPlayer(Game1.player);
					}
					Game1.currentSpeaker = null;
					if (!Game1.eventUp)
					{
						Game1.player.CanMove = true;
					}
					else if (Game1.currentLocation.currentEvent.CurrentCommand > 0 || Game1.currentLocation.currentEvent.specialEventVariable1)
					{
						if (!Game1.isFestival() || !Game1.currentLocation.currentEvent.canMoveAfterDialogue())
						{
							Event currentEvent = Game1.currentLocation.currentEvent;
							int currentCommand = currentEvent.CurrentCommand;
							currentEvent.CurrentCommand = currentCommand + 1;
						}
						else
						{
							Game1.player.CanMove = true;
						}
					}
					Game1.questionChoices.Clear();
					Game1.playSound("smallSelect", null);
				}
				else
				{
					Game1.playSound("smallSelect", null);
					Game1.currentDialogueCharacterIndex = 0;
					Game1.dialogueTyping = true;
					Game1.checkIfDialogueIsQuestion();
				}
				Game1.oldKBState = currentKBState;
				Game1.oldMouseState = Game1.input.GetMouseState();
				Game1.oldPadState = currentPadState;
				return false;
			}
			if (!Game1.player.UsingTool && (!Game1.eventUp || (Game1.currentLocation.currentEvent != null && Game1.currentLocation.currentEvent.playerControlSequence)) && !Game1.fadeToBlack)
			{
				if (Game1.wasMouseVisibleThisFrame && Game1.currentLocation.animals.Length > 0)
				{
					Vector2 mousePosition = new Vector2((float)(Game1.getOldMouseX() + Game1.viewport.X), (float)(Game1.getOldMouseY() + Game1.viewport.Y));
					if (Utility.withinRadiusOfPlayer((int)mousePosition.X, (int)mousePosition.Y, 1, Game1.player))
					{
						if (Game1.currentLocation.CheckPetAnimal(mousePosition, Game1.player))
						{
							return true;
						}
						if (Game1.didPlayerJustRightClick(true) && Game1.currentLocation.CheckInspectAnimal(mousePosition, Game1.player))
						{
							return true;
						}
					}
				}
				Vector2 grabTile = new Vector2((float)(Game1.getOldMouseX() + Game1.viewport.X), (float)(Game1.getOldMouseY() + Game1.viewport.Y)) / 64f;
				Vector2 cursorTile = grabTile;
				bool non_directed_tile = false;
				if (!Game1.wasMouseVisibleThisFrame || Game1.mouseCursorTransparency == 0f || !Utility.tileWithinRadiusOfPlayer((int)grabTile.X, (int)grabTile.Y, 1, Game1.player))
				{
					grabTile = Game1.player.GetGrabTile();
					non_directed_tile = true;
				}
				bool was_character_at_grab_tile = false;
				if (Game1.eventUp && !Game1.isFestival())
				{
					Event currentEvent2 = Game1.CurrentEvent;
					if (currentEvent2 != null)
					{
						currentEvent2.receiveActionPress((int)grabTile.X, (int)grabTile.Y);
					}
					Game1.oldKBState = currentKBState;
					Game1.oldMouseState = Game1.input.GetMouseState();
					Game1.oldPadState = currentPadState;
					return false;
				}
				if (Game1.tryToCheckAt(grabTile, Game1.player))
				{
					return false;
				}
				if (Game1.player.isRidingHorse())
				{
					Game1.player.mount.checkAction(Game1.player, Game1.player.currentLocation);
					return false;
				}
				if (!Game1.player.canMove)
				{
					return false;
				}
				if (!was_character_at_grab_tile && Game1.player.currentLocation.isCharacterAtTile(grabTile) != null)
				{
					was_character_at_grab_tile = true;
				}
				bool isPlacingObject = false;
				if (Game1.player.ActiveObject != null && !(Game1.player.ActiveObject is Furniture))
				{
					if (Game1.player.ActiveObject.performUseAction(Game1.currentLocation))
					{
						Game1.player.reduceActiveItemByOne();
						Game1.oldKBState = currentKBState;
						Game1.oldMouseState = Game1.input.GetMouseState();
						Game1.oldPadState = currentPadState;
						return false;
					}
					int stack = Game1.player.ActiveObject.Stack;
					Game1.isCheckingNonMousePlacement = !Game1.IsPerformingMousePlacement();
					if (non_directed_tile)
					{
						Game1.isCheckingNonMousePlacement = true;
					}
					if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.actionButton))
					{
						Game1.isCheckingNonMousePlacement = true;
					}
					Vector2 valid_position = Utility.GetNearbyValidPlacementPosition(Game1.player, Game1.currentLocation, Game1.player.ActiveObject, (int)grabTile.X * 64 + 32, (int)grabTile.Y * 64 + 32);
					if (!Game1.isCheckingNonMousePlacement && Game1.player.ActiveObject is Wallpaper && Utility.tryToPlaceItem(Game1.currentLocation, Game1.player.ActiveObject, (int)cursorTile.X * 64, (int)cursorTile.Y * 64))
					{
						Game1.isCheckingNonMousePlacement = false;
						return true;
					}
					if (Utility.tryToPlaceItem(Game1.currentLocation, Game1.player.ActiveObject, (int)valid_position.X, (int)valid_position.Y))
					{
						Game1.isCheckingNonMousePlacement = false;
						return true;
					}
					if (!Game1.eventUp && (Game1.player.ActiveObject == null || Game1.player.ActiveObject.Stack < stack || Game1.player.ActiveObject.isPlaceable()))
					{
						isPlacingObject = true;
					}
					Game1.isCheckingNonMousePlacement = false;
				}
				if (!isPlacingObject && !was_character_at_grab_tile)
				{
					grabTile.Y += 1f;
					if (Game1.player.FacingDirection >= 0 && Game1.player.FacingDirection <= 3)
					{
						Vector2 normalized_offset = grabTile - Game1.player.Tile;
						if (normalized_offset.X > 0f || normalized_offset.Y > 0f)
						{
							normalized_offset.Normalize();
						}
						if (Vector2.Dot(Utility.DirectionsTileVectors[Game1.player.FacingDirection], normalized_offset) >= 0f && Game1.tryToCheckAt(grabTile, Game1.player))
						{
							return false;
						}
					}
					if (!Game1.eventUp)
					{
						Furniture furniture = Game1.player.ActiveObject as Furniture;
						if (furniture != null)
						{
							furniture.rotate();
							Game1.playSound("dwoop", null);
							Game1.oldKBState = currentKBState;
							Game1.oldMouseState = Game1.input.GetMouseState();
							Game1.oldPadState = currentPadState;
							return false;
						}
					}
					grabTile.Y -= 2f;
					if (Game1.player.FacingDirection >= 0 && Game1.player.FacingDirection <= 3 && !was_character_at_grab_tile)
					{
						Vector2 normalized_offset2 = grabTile - Game1.player.Tile;
						if (normalized_offset2.X > 0f || normalized_offset2.Y > 0f)
						{
							normalized_offset2.Normalize();
						}
						if (Vector2.Dot(Utility.DirectionsTileVectors[Game1.player.FacingDirection], normalized_offset2) >= 0f && Game1.tryToCheckAt(grabTile, Game1.player))
						{
							return false;
						}
					}
					if (!Game1.eventUp)
					{
						Furniture furniture2 = Game1.player.ActiveObject as Furniture;
						if (furniture2 != null)
						{
							furniture2.rotate();
							Game1.playSound("dwoop", null);
							Game1.oldKBState = currentKBState;
							Game1.oldMouseState = Game1.input.GetMouseState();
							Game1.oldPadState = currentPadState;
							return false;
						}
					}
					grabTile = Game1.player.Tile;
					if (Game1.tryToCheckAt(grabTile, Game1.player))
					{
						return false;
					}
					if (!Game1.eventUp)
					{
						Furniture furniture3 = Game1.player.ActiveObject as Furniture;
						if (furniture3 != null)
						{
							furniture3.rotate();
							Game1.playSound("dwoop", null);
							Game1.oldKBState = currentKBState;
							Game1.oldMouseState = Game1.input.GetMouseState();
							Game1.oldPadState = currentPadState;
							return false;
						}
					}
				}
				if (!Game1.player.isEating && Game1.player.ActiveObject != null && !Game1.dialogueUp && !Game1.eventUp && !Game1.player.canOnlyWalk && !Game1.player.FarmerSprite.PauseForSingleAnimation && !Game1.fadeToBlack && Game1.player.ActiveObject.Edibility != -300 && Game1.didPlayerJustRightClick(true))
				{
					if (Game1.player.team.SpecialOrderRuleActive("SC_NO_FOOD", null))
					{
						MineShaft mineShaft = Game1.player.currentLocation as MineShaft;
						if (mineShaft != null && mineShaft.getMineArea(-1) == 121)
						{
							Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"), 3));
							return false;
						}
					}
					if (Game1.player.hasBuff("25") && Game1.player.ActiveObject != null && !Game1.player.ActiveObject.HasContextTag("ginger_item"))
					{
						Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Nauseous_CantEat"), 3));
						return false;
					}
					Game1.player.faceDirection(2);
					Game1.player.itemToEat = Game1.player.ActiveObject;
					Game1.player.FarmerSprite.setCurrentSingleAnimation(304);
					ObjectData objectData;
					if (Game1.objectData.TryGetValue(Game1.player.ActiveObject.ItemId, out objectData))
					{
						Game1.currentLocation.createQuestionDialogue((objectData.IsDrink && Game1.player.ActiveObject.preserve.Value.GetValueOrDefault() != Object.PreserveType.Pickle) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3159", Game1.player.ActiveObject.DisplayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3160", Game1.player.ActiveObject.DisplayName), Game1.currentLocation.createYesNoResponses(), "Eat");
					}
					Game1.oldKBState = currentKBState;
					Game1.oldMouseState = Game1.input.GetMouseState();
					Game1.oldPadState = currentPadState;
					return false;
				}
			}
			if (Game1.player.CurrentTool is MeleeWeapon && Game1.player.CanMove && !Game1.player.canOnlyWalk && !Game1.eventUp && !Game1.player.onBridge.Value && Game1.didPlayerJustRightClick(true))
			{
				((MeleeWeapon)Game1.player.CurrentTool).animateSpecialMove(Game1.player);
				return false;
			}
			return true;
		}

		// Token: 0x06000BE0 RID: 3040 RVA: 0x00080370 File Offset: 0x0007E570
		public static bool IsPerformingMousePlacement()
		{
			return Game1.mouseCursorTransparency != 0f && Game1.wasMouseVisibleThisFrame && (Game1.lastCursorMotionWasMouse || (Game1.player.ActiveObject != null && (Game1.player.ActiveObject.isPlaceable() || Game1.player.ActiveObject.Category == -74 || Game1.player.ActiveObject.isSapling())));
		}

		// Token: 0x06000BE1 RID: 3041 RVA: 0x000803DC File Offset: 0x0007E5DC
		public static Vector2 GetPlacementGrabTile()
		{
			if (!Game1.IsPerformingMousePlacement())
			{
				return Game1.player.GetGrabTile();
			}
			return new Vector2((float)(Game1.getOldMouseX() + Game1.viewport.X), (float)(Game1.getOldMouseY() + Game1.viewport.Y)) / 64f;
		}

		// Token: 0x06000BE2 RID: 3042 RVA: 0x0008042C File Offset: 0x0007E62C
		public static bool tryToCheckAt(Vector2 grabTile, Farmer who)
		{
			if (Game1.player.onBridge.Value)
			{
				return false;
			}
			Game1.haltAfterCheck = true;
			if (Utility.tileWithinRadiusOfPlayer((int)grabTile.X, (int)grabTile.Y, 1, Game1.player) && Game1.hooks.OnGameLocation_CheckAction(Game1.currentLocation, new Location((int)grabTile.X, (int)grabTile.Y), Game1.viewport, who, () => Game1.currentLocation.checkAction(new Location((int)grabTile.X, (int)grabTile.Y), Game1.viewport, who)))
			{
				Game1.updateCursorTileHint();
				who.lastGrabTile = grabTile;
				if (who.CanMove && Game1.haltAfterCheck)
				{
					who.faceGeneralDirection(grabTile * 64f, 0, false);
					who.Halt();
				}
				Game1.oldKBState = Game1.GetKeyboardState();
				Game1.oldMouseState = Game1.input.GetMouseState();
				Game1.oldPadState = Game1.input.GetGamePadState();
				return true;
			}
			return false;
		}

		// Token: 0x06000BE3 RID: 3043 RVA: 0x00080550 File Offset: 0x0007E750
		public static void pressSwitchToolButton()
		{
			if (Game1.player.netItemStowed.Value)
			{
				Game1.player.netItemStowed.Set(false);
				Game1.player.UpdateItemStow();
			}
			int whichWay = (Game1.input.GetMouseState().ScrollWheelValue > Game1.oldMouseState.ScrollWheelValue) ? -1 : ((Game1.input.GetMouseState().ScrollWheelValue < Game1.oldMouseState.ScrollWheelValue) ? 1 : 0);
			if (Game1.options.gamepadControls && whichWay == 0)
			{
				if (Game1.input.GetGamePadState().IsButtonDown(Buttons.LeftTrigger))
				{
					whichWay = -1;
				}
				else if (Game1.input.GetGamePadState().IsButtonDown(Buttons.RightTrigger))
				{
					whichWay = 1;
				}
			}
			if (Game1.options.invertScrollDirection)
			{
				whichWay *= -1;
			}
			if (whichWay == 0)
			{
				return;
			}
			Game1.player.CurrentToolIndex = (Game1.player.CurrentToolIndex + whichWay) % 12;
			if (Game1.player.CurrentToolIndex < 0)
			{
				Game1.player.CurrentToolIndex = 11;
			}
			int i = 0;
			while (i < 12 && Game1.player.CurrentItem == null)
			{
				Game1.player.CurrentToolIndex = (whichWay + Game1.player.CurrentToolIndex) % 12;
				if (Game1.player.CurrentToolIndex < 0)
				{
					Game1.player.CurrentToolIndex = 11;
				}
				i++;
			}
			Game1.playSound("toolSwap", null);
			if (Game1.player.ActiveObject != null)
			{
				Game1.player.showCarrying();
				return;
			}
			Game1.player.showNotCarrying();
		}

		// Token: 0x06000BE4 RID: 3044 RVA: 0x000806D8 File Offset: 0x0007E8D8
		public static bool pressUseToolButton()
		{
			bool stow_was_initialized = Game1.game1._didInitiateItemStow;
			Game1.game1._didInitiateItemStow = false;
			if (Game1.fadeToBlack)
			{
				return false;
			}
			Game1.player.toolPower.Value = 0;
			Game1.player.toolHold.Value = 0;
			bool did_attempt_object_removal = false;
			if (Game1.player.CurrentTool == null && Game1.player.ActiveObject == null)
			{
				Vector2 c = Game1.player.GetToolLocation(false) / 64f;
				c.X = (float)((int)c.X);
				c.Y = (float)((int)c.Y);
				Object o;
				if (Game1.currentLocation.Objects.TryGetValue(c, out o) && !o.readyForHarvest.Value && o.heldObject.Value == null && !(o is Fence) && !(o is CrabPot) && (o.Type == "Crafting" || o.Type == "interactive") && !o.IsTwig())
				{
					did_attempt_object_removal = true;
					o.setHealth(o.getHealth() - 1);
					o.shakeTimer = 300;
					o.playNearbySoundAll("hammer", null, SoundContext.Default);
					if (o.getHealth() < 2)
					{
						o.playNearbySoundAll("hammer", null, SoundContext.Default);
						if (o.getHealth() < 1)
						{
							Tool t = ItemRegistry.Create<Tool>("(T)Pickaxe", 1, 0, false);
							t.DoFunction(Game1.currentLocation, -1, -1, 0, Game1.player);
							if (o.performToolAction(t))
							{
								o.performRemoveAction();
								if (o.Type == "Crafting" && o.fragility.Value != 2)
								{
									Game1.currentLocation.debris.Add(new Debris(o.QualifiedItemId, Game1.player.GetToolLocation(false), Utility.PointToVector2(Game1.player.StandingPixel)));
								}
								Game1.currentLocation.Objects.Remove(c);
								return true;
							}
						}
					}
				}
			}
			if (Game1.currentMinigame == null && !Game1.player.UsingTool)
			{
				if (!Game1.player.IsSitting() && !Game1.player.isRidingHorse() && !Game1.player.onBridge.Value && !Game1.dialogueUp && (!Game1.eventUp || Game1.CurrentEvent.canPlayerUseTool() || (Game1.currentLocation.currentEvent.playerControlSequence && (Game1.activeClickableMenu != null || Game1.currentMinigame != null))))
				{
					if (Game1.player.CurrentTool == null)
					{
						goto IL_323;
					}
					NPC npc = Game1.currentLocation.doesPositionCollideWithCharacter(Utility.getRectangleCenteredAt(Game1.player.GetToolLocation(false), 64), true);
					bool? flag = (npc != null) ? new bool?(npc.IsVillager) : null;
					if (flag == null || !flag.GetValueOrDefault())
					{
						goto IL_323;
					}
				}
				Game1.pressActionButton(Game1.GetKeyboardState(), Game1.input.GetMouseState(), Game1.input.GetGamePadState());
				return false;
			}
			IL_323:
			if (Game1.player.canOnlyWalk)
			{
				return true;
			}
			Vector2 position = (!Game1.wasMouseVisibleThisFrame) ? Game1.player.GetToolLocation(false) : new Vector2((float)(Game1.getOldMouseX() + Game1.viewport.X), (float)(Game1.getOldMouseY() + Game1.viewport.Y));
			if (Utility.canGrabSomethingFromHere((int)position.X, (int)position.Y, Game1.player))
			{
				Vector2 tile = new Vector2(position.X / 64f, position.Y / 64f);
				if (Game1.hooks.OnGameLocation_CheckAction(Game1.currentLocation, new Location((int)tile.X, (int)tile.Y), Game1.viewport, Game1.player, () => Game1.currentLocation.checkAction(new Location((int)tile.X, (int)tile.Y), Game1.viewport, Game1.player)))
				{
					Game1.updateCursorTileHint();
					return true;
				}
				TerrainFeature terrainFeature;
				if (Game1.currentLocation.terrainFeatures.TryGetValue(tile, out terrainFeature))
				{
					terrainFeature.performUseAction(tile);
					return true;
				}
				return false;
			}
			else
			{
				if (Game1.currentLocation.leftClick((int)position.X, (int)position.Y, Game1.player))
				{
					return true;
				}
				Game1.isCheckingNonMousePlacement = !Game1.IsPerformingMousePlacement();
				if (Game1.player.ActiveObject != null)
				{
					if (Game1.options.allowStowing && Game1.CanPlayerStowItem(Game1.GetPlacementGrabTile()))
					{
						if (Game1.didPlayerJustLeftClick(false) || stow_was_initialized)
						{
							Game1.game1._didInitiateItemStow = true;
							Game1.playSound("stoneStep", null);
							Game1.player.netItemStowed.Set(true);
							return true;
						}
						return true;
					}
					else
					{
						if (Utility.withinRadiusOfPlayer((int)position.X, (int)position.Y, 1, Game1.player) && Game1.hooks.OnGameLocation_CheckAction(Game1.currentLocation, new Location((int)position.X / 64, (int)position.Y / 64), Game1.viewport, Game1.player, () => Game1.currentLocation.checkAction(new Location((int)position.X / 64, (int)position.Y / 64), Game1.viewport, Game1.player)))
						{
							return true;
						}
						Vector2 grabTile = Game1.GetPlacementGrabTile();
						Vector2 valid_position = Utility.GetNearbyValidPlacementPosition(Game1.player, Game1.currentLocation, Game1.player.ActiveObject, (int)grabTile.X * 64, (int)grabTile.Y * 64);
						if (Utility.tryToPlaceItem(Game1.currentLocation, Game1.player.ActiveObject, (int)valid_position.X, (int)valid_position.Y))
						{
							Game1.isCheckingNonMousePlacement = false;
							return true;
						}
						Game1.isCheckingNonMousePlacement = false;
					}
				}
				if (Game1.currentLocation.LowPriorityLeftClick((int)position.X, (int)position.Y, Game1.player))
				{
					return true;
				}
				if (Game1.options.allowStowing && Game1.player.netItemStowed.Value && !did_attempt_object_removal && (stow_was_initialized || Game1.didPlayerJustLeftClick(true)))
				{
					Game1.game1._didInitiateItemStow = true;
					Game1.playSound("toolSwap", null);
					Game1.player.netItemStowed.Set(false);
					return true;
				}
				if (Game1.player.UsingTool)
				{
					Game1.player.lastClick = new Vector2((float)((int)position.X), (float)((int)position.Y));
					Game1.player.CurrentTool.DoFunction(Game1.player.currentLocation, (int)Game1.player.lastClick.X, (int)Game1.player.lastClick.Y, 1, Game1.player);
					return true;
				}
				if (Game1.player.ActiveObject == null && !Game1.player.isEating && Game1.player.CurrentTool != null)
				{
					if (Game1.player.Stamina <= 20f && Game1.player.CurrentTool != null && !(Game1.player.CurrentTool is MeleeWeapon) && !Game1.eventUp)
					{
						Game1.staminaShakeTimer = 1000;
						for (int i = 0; i < 4; i++)
						{
							Game1.uiOverlayTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(366, 412, 5, 6), new Vector2((float)(Game1.random.Next(32) + Game1.uiViewport.Width - 56), (float)(Game1.uiViewport.Height - 224 - 16 - (int)((double)(Game1.player.MaxStamina - 270) * 0.715))), false, 0.012f, Color.SkyBlue)
							{
								motion = new Vector2(-2f, -10f),
								acceleration = new Vector2(0f, 0.5f),
								local = true,
								scale = (float)(4 + Game1.random.Next(-1, 0)),
								delayBeforeAnimationStart = i * 30
							});
						}
					}
					if (!(Game1.player.CurrentTool is MeleeWeapon) || Game1.didPlayerJustLeftClick(true))
					{
						int old_direction = Game1.player.FacingDirection;
						Vector2 tool_location = Game1.player.GetToolLocation(position, false);
						Game1.player.FacingDirection = Game1.player.getGeneralDirectionTowards(new Vector2((float)((int)tool_location.X), (float)((int)tool_location.Y)), 0, false, true);
						Game1.player.lastClick = new Vector2((float)((int)position.X), (float)((int)position.Y));
						Game1.player.BeginUsingTool();
						if (!Game1.player.usingTool.Value)
						{
							Game1.player.FacingDirection = old_direction;
						}
						else if (Game1.player.FarmerSprite.IsPlayingBasicAnimation(old_direction, true) || Game1.player.FarmerSprite.IsPlayingBasicAnimation(old_direction, false))
						{
							Game1.player.FarmerSprite.StopAnimation();
						}
					}
				}
				return false;
			}
		}

		// Token: 0x06000BE5 RID: 3045 RVA: 0x00080FFC File Offset: 0x0007F1FC
		public static bool CanPlayerStowItem(Vector2 position)
		{
			if (Game1.player.ActiveObject == null)
			{
				return false;
			}
			if (Game1.player.ActiveObject.bigCraftable.Value)
			{
				return false;
			}
			Object activeObject = Game1.player.ActiveObject;
			if (!(activeObject is Furniture))
			{
				if (activeObject != null)
				{
					if (Game1.player.ActiveObject.Category == -74 || Game1.player.ActiveObject.Category == -19)
					{
						Vector2 valid_position = Utility.GetNearbyValidPlacementPosition(Game1.player, Game1.currentLocation, Game1.player.ActiveObject, (int)position.X * 64, (int)position.Y * 64);
						if (Utility.playerCanPlaceItemHere(Game1.player.currentLocation, Game1.player.ActiveObject, (int)valid_position.X, (int)valid_position.Y, Game1.player, false) && (!Game1.player.ActiveObject.isSapling() || Game1.IsPerformingMousePlacement()))
						{
							return false;
						}
					}
				}
				return true;
			}
			return false;
		}

		// Token: 0x06000BE6 RID: 3046 RVA: 0x000810EC File Offset: 0x0007F2EC
		public static int getMouseXRaw()
		{
			return Game1.input.GetMouseState().X;
		}

		// Token: 0x06000BE7 RID: 3047 RVA: 0x0008110C File Offset: 0x0007F30C
		public static int getMouseYRaw()
		{
			return Game1.input.GetMouseState().Y;
		}

		// Token: 0x06000BE8 RID: 3048 RVA: 0x0008112B File Offset: 0x0007F32B
		public static bool IsOnMainThread()
		{
			return Thread.CurrentThread != null && !Thread.CurrentThread.IsBackground;
		}

		// Token: 0x06000BE9 RID: 3049 RVA: 0x00081144 File Offset: 0x0007F344
		public static void PushUIMode()
		{
			if (!Game1.IsOnMainThread())
			{
				return;
			}
			Game1.uiModeCount++;
			if (Game1.uiModeCount > 0 && !Game1.uiMode)
			{
				Game1.uiMode = true;
				if (Game1.game1.isDrawing && Game1.IsOnMainThread())
				{
					if (Game1.game1.uiScreen != null && !Game1.game1.uiScreen.IsDisposed)
					{
						RenderTargetBinding[] render_targets = Game1.graphics.GraphicsDevice.GetRenderTargets();
						if (render_targets.Length != 0)
						{
							Game1.nonUIRenderTarget = (render_targets[0].RenderTarget as RenderTarget2D);
						}
						else
						{
							Game1.nonUIRenderTarget = null;
						}
						Game1.SetRenderTarget(Game1.game1.uiScreen);
					}
					if (Game1.isRenderingScreenBuffer)
					{
						Game1.SetRenderTarget(null);
					}
				}
				Game1.uiViewport = new xTile.Dimensions.Rectangle(0, 0, (int)Math.Ceiling((double)((float)Game1.viewport.Width * Game1.options.zoomLevel / Game1.options.uiScale)), (int)Math.Ceiling((double)((float)Game1.viewport.Height * Game1.options.zoomLevel / Game1.options.uiScale)))
				{
					X = Game1.viewport.X,
					Y = Game1.viewport.Y
				};
			}
		}

		// Token: 0x06000BEA RID: 3050 RVA: 0x00081280 File Offset: 0x0007F480
		public static void PopUIMode()
		{
			if (!Game1.IsOnMainThread())
			{
				return;
			}
			Game1.uiModeCount--;
			if (Game1.uiModeCount <= 0 && Game1.uiMode)
			{
				if (Game1.game1.isDrawing)
				{
					if (Game1.graphics.GraphicsDevice.GetRenderTargets().Length != 0 && Game1.graphics.GraphicsDevice.GetRenderTargets()[0].RenderTarget == Game1.game1.uiScreen)
					{
						if (Game1.nonUIRenderTarget != null && !Game1.nonUIRenderTarget.IsDisposed)
						{
							Game1.SetRenderTarget(Game1.nonUIRenderTarget);
						}
						else
						{
							Game1.SetRenderTarget(null);
						}
					}
					if (Game1.isRenderingScreenBuffer)
					{
						Game1.SetRenderTarget(null);
					}
				}
				Game1.nonUIRenderTarget = null;
				Game1.uiMode = false;
			}
		}

		// Token: 0x06000BEB RID: 3051 RVA: 0x00081338 File Offset: 0x0007F538
		public static void SetRenderTarget(RenderTarget2D target)
		{
			if (Game1.isRenderingScreenBuffer)
			{
				return;
			}
			if (!Game1.IsOnMainThread())
			{
				return;
			}
			Game1.graphics.GraphicsDevice.SetRenderTarget(target);
		}

		// Token: 0x06000BEC RID: 3052 RVA: 0x0008135C File Offset: 0x0007F55C
		public static void InUIMode(Action action)
		{
			Game1.PushUIMode();
			try
			{
				action();
			}
			finally
			{
				Game1.PopUIMode();
			}
		}

		// Token: 0x06000BED RID: 3053 RVA: 0x0008138C File Offset: 0x0007F58C
		public static void StartWorldDrawInUI(SpriteBatch b)
		{
			Game1._oldUIModeCount = 0;
			if (Game1.uiMode)
			{
				Game1._oldUIModeCount = Game1.uiModeCount;
				if (b != null)
				{
					b.End();
				}
				while (Game1.uiModeCount > 0)
				{
					Game1.PopUIMode();
				}
				if (b != null)
				{
					b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
				}
			}
		}

		// Token: 0x06000BEE RID: 3054 RVA: 0x000813E8 File Offset: 0x0007F5E8
		public static void EndWorldDrawInUI(SpriteBatch b)
		{
			if (Game1._oldUIModeCount > 0)
			{
				if (b != null)
				{
					b.End();
				}
				for (int i = 0; i < Game1._oldUIModeCount; i++)
				{
					Game1.PushUIMode();
				}
				if (b != null)
				{
					b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
				}
			}
			Game1._oldUIModeCount = 0;
		}

		// Token: 0x06000BEF RID: 3055 RVA: 0x00081441 File Offset: 0x0007F641
		public static int getMouseX()
		{
			return Game1.getMouseX(Game1.uiMode);
		}

		// Token: 0x06000BF0 RID: 3056 RVA: 0x00081450 File Offset: 0x0007F650
		public static int getMouseX(bool ui_scale)
		{
			if (ui_scale)
			{
				return (int)((float)Game1.input.GetMouseState().X / Game1.options.uiScale);
			}
			return (int)((float)Game1.input.GetMouseState().X * (1f / Game1.options.zoomLevel));
		}

		// Token: 0x06000BF1 RID: 3057 RVA: 0x000814A5 File Offset: 0x0007F6A5
		public static int getOldMouseX()
		{
			return Game1.getOldMouseX(Game1.uiMode);
		}

		// Token: 0x06000BF2 RID: 3058 RVA: 0x000814B1 File Offset: 0x0007F6B1
		public static int getOldMouseX(bool ui_scale)
		{
			if (ui_scale)
			{
				return (int)((float)Game1.oldMouseState.X / Game1.options.uiScale);
			}
			return (int)((float)Game1.oldMouseState.X * (1f / Game1.options.zoomLevel));
		}

		// Token: 0x06000BF3 RID: 3059 RVA: 0x000814EB File Offset: 0x0007F6EB
		public static int getMouseY()
		{
			return Game1.getMouseY(Game1.uiMode);
		}

		// Token: 0x06000BF4 RID: 3060 RVA: 0x000814F8 File Offset: 0x0007F6F8
		public static int getMouseY(bool ui_scale)
		{
			if (ui_scale)
			{
				return (int)((float)Game1.input.GetMouseState().Y / Game1.options.uiScale);
			}
			return (int)((float)Game1.input.GetMouseState().Y * (1f / Game1.options.zoomLevel));
		}

		// Token: 0x06000BF5 RID: 3061 RVA: 0x0008154D File Offset: 0x0007F74D
		public static int getOldMouseY()
		{
			return Game1.getOldMouseY(Game1.uiMode);
		}

		// Token: 0x06000BF6 RID: 3062 RVA: 0x00081559 File Offset: 0x0007F759
		public static int getOldMouseY(bool ui_scale)
		{
			if (ui_scale)
			{
				return (int)((float)Game1.oldMouseState.Y / Game1.options.uiScale);
			}
			return (int)((float)Game1.oldMouseState.Y * (1f / Game1.options.zoomLevel));
		}

		// Token: 0x06000BF7 RID: 3063 RVA: 0x00081594 File Offset: 0x0007F794
		public static bool PlayEvent(string eventId, GameLocation location, out bool validEvent, bool checkPreconditions = true, bool checkSeen = true)
		{
			Dictionary<string, string> locationEvents;
			string eventAssetName;
			try
			{
				if (!location.TryGetLocationEvents(out eventAssetName, out locationEvents))
				{
					validEvent = false;
					return false;
				}
			}
			catch
			{
				validEvent = false;
				return false;
			}
			if (locationEvents == null)
			{
				validEvent = false;
				return false;
			}
			using (Dictionary<string, string>.KeyCollection.Enumerator enumerator = locationEvents.Keys.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					string key = enumerator.Current;
					if (key.Split('/', StringSplitOptions.None)[0] == eventId)
					{
						validEvent = true;
						if (checkSeen && (Game1.player.eventsSeen.Contains(eventId) || Game1.eventsSeenSinceLastLocationChange.Contains(eventId)))
						{
							return false;
						}
						string id = eventId;
						if (checkPreconditions)
						{
							id = location.checkEventPrecondition(key, false);
						}
						if (!string.IsNullOrEmpty(id) && id != "-1")
						{
							if (location.Name != Game1.currentLocation.Name)
							{
								LocationRequest locationRequest = Game1.getLocationRequest(location.Name, false);
								locationRequest.OnLoad += delegate()
								{
									Game1.currentLocation.currentEvent = new Event(locationEvents[key], eventAssetName, id, null);
								};
								int x = 8;
								int y = 8;
								Utility.getDefaultWarpLocation(locationRequest.Name, ref x, ref y);
								Game1.warpFarmer(locationRequest, x, y, Game1.player.FacingDirection);
							}
							else
							{
								Game1.globalFadeToBlack(delegate
								{
									Game1.forceSnapOnNextViewportUpdate = true;
									Game1.currentLocation.startEvent(new Event(locationEvents[key], eventAssetName, id, null));
									Game1.globalFadeToClear(null, 0.02f);
								}, 0.02f);
							}
							return true;
						}
						return false;
					}
				}
			}
			validEvent = false;
			return false;
		}

		// Token: 0x06000BF8 RID: 3064 RVA: 0x0008176C File Offset: 0x0007F96C
		public static bool PlayEvent(string eventId, bool checkPreconditions = true, bool checkSeen = true)
		{
			if (checkSeen && (Game1.player.eventsSeen.Contains(eventId) || Game1.eventsSeenSinceLastLocationChange.Contains(eventId)))
			{
				return false;
			}
			bool validEvent;
			if (Game1.PlayEvent(eventId, Game1.currentLocation, out validEvent, checkPreconditions, checkSeen))
			{
				return true;
			}
			if (validEvent)
			{
				return false;
			}
			foreach (GameLocation location in Game1.locations)
			{
				if (location != Game1.currentLocation)
				{
					if (Game1.PlayEvent(eventId, location, out validEvent, checkPreconditions, checkSeen))
					{
						return true;
					}
					if (validEvent)
					{
						return false;
					}
				}
			}
			return false;
		}

		// Token: 0x06000BF9 RID: 3065 RVA: 0x00081810 File Offset: 0x0007FA10
		public static int numberOfPlayers()
		{
			return Game1._onlineFarmers.Count;
		}

		// Token: 0x06000BFA RID: 3066 RVA: 0x0008181C File Offset: 0x0007FA1C
		public static bool isFestival()
		{
			GameLocation currentLocation = Game1.currentLocation;
			bool? flag;
			if (currentLocation == null)
			{
				flag = null;
			}
			else
			{
				Event currentEvent = currentLocation.currentEvent;
				flag = ((currentEvent != null) ? new bool?(currentEvent.isFestival) : null);
			}
			return flag ?? false;
		}

		// Token: 0x06000BFB RID: 3067 RVA: 0x00081870 File Offset: 0x0007FA70
		public bool parseDebugInput(string debugInput, IGameLogger log = null)
		{
			debugInput = debugInput.Trim();
			string[] command = ArgUtility.SplitBySpaceQuoteAware(debugInput);
			bool result;
			try
			{
				result = DebugCommands.TryHandle(command, log);
			}
			catch (Exception e)
			{
				Game1.log.Error("Debug command error.", e);
				Game1.debugOutput = e.Message;
				result = false;
			}
			return result;
		}

		// Token: 0x06000BFC RID: 3068 RVA: 0x000818C8 File Offset: 0x0007FAC8
		public void RecountWalnuts()
		{
			if (!Game1.IsMasterGame || Game1.netWorldState.Value.ActivatedGoldenParrot)
			{
				return;
			}
			IslandHut hut = Game1.getLocationFromName("IslandHut") as IslandHut;
			if (hut != null)
			{
				int num = 130;
				int missing_nuts = hut.ShowNutHint();
				int current_nut_count = num - missing_nuts;
				Game1.netWorldState.Value.GoldenWalnutsFound = current_nut_count;
				foreach (GameLocation gameLocation in Game1.locations)
				{
					IslandLocation island_location = gameLocation as IslandLocation;
					if (island_location != null)
					{
						foreach (ParrotUpgradePerch perch in island_location.parrotUpgradePerches)
						{
							if (perch.currentState.Value == ParrotUpgradePerch.UpgradeState.Complete)
							{
								current_nut_count -= perch.requiredNuts.Value;
							}
						}
					}
				}
				if (Game1.MasterPlayer.hasOrWillReceiveMail("Island_VolcanoShortcutOut"))
				{
					current_nut_count -= 5;
				}
				if (Game1.MasterPlayer.hasOrWillReceiveMail("Island_VolcanoBridge"))
				{
					current_nut_count -= 5;
				}
				Game1.netWorldState.Value.GoldenWalnuts = current_nut_count;
			}
		}

		// Token: 0x06000BFD RID: 3069 RVA: 0x000819FC File Offset: 0x0007FBFC
		public void ResetIslandLocations()
		{
			Game1.netWorldState.Value.GoldenWalnutsFound = 0;
			Game1.player.team.collectedNutTracker.Clear();
			foreach (NetStringHashSet netStringHashSet in new NetStringHashSet[]
			{
				Game1.player.mailReceived,
				Game1.player.mailForTomorrow,
				Game1.player.team.broadcastedMail
			})
			{
				netStringHashSet.Remove("birdieQuestBegun");
				netStringHashSet.Remove("birdieQuestFinished");
				netStringHashSet.Remove("tigerSlimeNut");
				netStringHashSet.Remove("Island_W_BuriedTreasureNut");
				netStringHashSet.Remove("Island_W_BuriedTreasure");
				netStringHashSet.Remove("islandNorthCaveOpened");
				netStringHashSet.Remove("Saw_Flame_Sprite_North_North");
				netStringHashSet.Remove("Saw_Flame_Sprite_North_South");
				netStringHashSet.Remove("Island_N_BuriedTreasureNut");
				netStringHashSet.Remove("Island_W_BuriedTreasure");
				netStringHashSet.Remove("Saw_Flame_Sprite_South");
				netStringHashSet.Remove("Visited_Island");
				netStringHashSet.Remove("Island_FirstParrot");
				netStringHashSet.Remove("gotBirdieReward");
				netStringHashSet.RemoveWhere((string key) => key.StartsWith("Island_Upgrade"));
			}
			Game1.player.secretNotesSeen.RemoveWhere((int id) => id >= GameLocation.JOURNAL_INDEX);
			Game1.player.team.limitedNutDrops.Clear();
			Game1.netWorldState.Value.GoldenCoconutCracked = false;
			Game1.netWorldState.Value.GoldenWalnuts = 0;
			Game1.netWorldState.Value.ParrotPlatformsUnlocked = false;
			Game1.netWorldState.Value.FoundBuriedNuts.Clear();
			for (int i = 0; i < Game1.locations.Count; i++)
			{
				GameLocation location = Game1.locations[i];
				if (location.InIslandContext())
				{
					Game1._locationLookup.Clear();
					string map_path = location.mapPath.Value;
					string location_name = location.name.Value;
					object[] args = new object[]
					{
						map_path,
						location_name
					};
					try
					{
						Game1.locations[i] = (Activator.CreateInstance(location.GetType(), args) as GameLocation);
					}
					catch
					{
						Game1.locations[i] = (Activator.CreateInstance(location.GetType()) as GameLocation);
					}
					Game1._locationLookup.Clear();
				}
			}
			Game1.AddCharacterIfNecessary("Birdie", false);
		}

		// Token: 0x06000BFE RID: 3070 RVA: 0x00081C94 File Offset: 0x0007FE94
		public void ShowTelephoneMenu()
		{
			Game1.playSound("openBox", null);
			if (Game1.IsGreenRainingHere(null))
			{
				Game1.drawObjectDialogue("...................");
				return;
			}
			List<KeyValuePair<string, string>> responses = new List<KeyValuePair<string, string>>();
			foreach (IPhoneHandler handler in Phone.PhoneHandlers)
			{
				responses.AddRange(handler.GetOutgoingNumbers());
			}
			responses.Add(new KeyValuePair<string, string>("HangUp", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel")));
			Game1.currentLocation.ShowPagedResponses(Game1.content.LoadString("Strings\\Characters:Phone_SelectNumber"), responses, delegate(string callId)
			{
				if (callId == "HangUp")
				{
					Phone.HangUp();
					return;
				}
				using (List<IPhoneHandler>.Enumerator enumerator2 = Phone.PhoneHandlers.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						if (enumerator2.Current.TryHandleOutgoingCall(callId))
						{
							return;
						}
					}
				}
				Phone.HangUp();
			}, false, false, 6);
		}

		// Token: 0x06000BFF RID: 3071 RVA: 0x00081D74 File Offset: 0x0007FF74
		public void requestDebugInput()
		{
			Game1.chatBox.activate();
			Game1.chatBox.setText("/");
		}

		// Token: 0x06000C00 RID: 3072 RVA: 0x00081D90 File Offset: 0x0007FF90
		private void panModeSuccess(KeyboardState currentKBState)
		{
			this.panFacingDirectionWait = false;
			Game1.playSound("smallSelect", null);
			if (currentKBState.IsKeyDown(Keys.LeftShift))
			{
				this.panModeString += " (animation_name_here)";
			}
			Game1.debugOutput = this.panModeString;
		}

		// Token: 0x06000C01 RID: 3073 RVA: 0x00081DE8 File Offset: 0x0007FFE8
		private void updatePanModeControls(MouseState currentMouseState, KeyboardState currentKBState)
		{
			if (currentKBState.IsKeyDown(Keys.F8) && !Game1.oldKBState.IsKeyDown(Keys.F8))
			{
				this.requestDebugInput();
				return;
			}
			if (!this.panFacingDirectionWait)
			{
				if (currentKBState.IsKeyDown(Keys.W))
				{
					Game1.viewport.Y = Game1.viewport.Y - 16;
				}
				if (currentKBState.IsKeyDown(Keys.A))
				{
					Game1.viewport.X = Game1.viewport.X - 16;
				}
				if (currentKBState.IsKeyDown(Keys.S))
				{
					Game1.viewport.Y = Game1.viewport.Y + 16;
				}
				if (currentKBState.IsKeyDown(Keys.D))
				{
					Game1.viewport.X = Game1.viewport.X + 16;
				}
			}
			else
			{
				if (currentKBState.IsKeyDown(Keys.W))
				{
					this.panModeString += "0";
					this.panModeSuccess(currentKBState);
				}
				if (currentKBState.IsKeyDown(Keys.A))
				{
					this.panModeString += "3";
					this.panModeSuccess(currentKBState);
				}
				if (currentKBState.IsKeyDown(Keys.S))
				{
					this.panModeString += "2";
					this.panModeSuccess(currentKBState);
				}
				if (currentKBState.IsKeyDown(Keys.D))
				{
					this.panModeString += "1";
					this.panModeSuccess(currentKBState);
				}
			}
			if (Game1.getMouseX(false) < 192)
			{
				Game1.viewport.X = Game1.viewport.X - 8;
				Game1.viewport.X = Game1.viewport.X - (192 - Game1.getMouseX()) / 8;
			}
			if (Game1.getMouseX(false) > Game1.viewport.Width - 192)
			{
				Game1.viewport.X = Game1.viewport.X + 8;
				Game1.viewport.X = Game1.viewport.X + (Game1.getMouseX() - Game1.viewport.Width + 192) / 8;
			}
			if (Game1.getMouseY(false) < 192)
			{
				Game1.viewport.Y = Game1.viewport.Y - 8;
				Game1.viewport.Y = Game1.viewport.Y - (192 - Game1.getMouseY()) / 8;
			}
			if (Game1.getMouseY(false) > Game1.viewport.Height - 192)
			{
				Game1.viewport.Y = Game1.viewport.Y + 8;
				Game1.viewport.Y = Game1.viewport.Y + (Game1.getMouseY() - Game1.viewport.Height + 192) / 8;
			}
			if (currentMouseState.LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton == ButtonState.Released)
			{
				string text = this.panModeString;
				if (text != null && text.Length > 0)
				{
					int x = (Game1.getMouseX() + Game1.viewport.X) / 64;
					int y = (Game1.getMouseY() + Game1.viewport.Y) / 64;
					this.panModeString = string.Concat(new string[]
					{
						this.panModeString,
						Game1.currentLocation.Name,
						" ",
						x.ToString(),
						" ",
						y.ToString(),
						" "
					});
					this.panFacingDirectionWait = true;
					Game1.currentLocation.playTerrainSound(new Vector2((float)x, (float)y), null, true);
					Game1.debugOutput = this.panModeString;
				}
			}
			if (currentMouseState.RightButton == ButtonState.Pressed && Game1.oldMouseState.RightButton == ButtonState.Released)
			{
				int x2 = Game1.getMouseX() + Game1.viewport.X;
				int y2 = Game1.getMouseY() + Game1.viewport.Y;
				Warp w = Game1.currentLocation.isCollidingWithWarpOrDoor(new Microsoft.Xna.Framework.Rectangle(x2, y2, 1, 1), null);
				if (w != null)
				{
					Game1.currentLocation = Game1.RequireLocation(w.TargetName, false);
					Game1.currentLocation.map.LoadTileSheets(Game1.mapDisplayDevice);
					Game1.viewport.X = w.TargetX * 64 - Game1.viewport.Width / 2;
					Game1.viewport.Y = w.TargetY * 64 - Game1.viewport.Height / 2;
					Game1.playSound("dwop", null);
				}
			}
			if (currentKBState.IsKeyDown(Keys.Escape) && !Game1.oldKBState.IsKeyDown(Keys.Escape))
			{
				Warp w2 = Game1.currentLocation.warps[0];
				Game1.currentLocation = Game1.RequireLocation(w2.TargetName, false);
				Game1.currentLocation.map.LoadTileSheets(Game1.mapDisplayDevice);
				Game1.viewport.X = w2.TargetX * 64 - Game1.viewport.Width / 2;
				Game1.viewport.Y = w2.TargetY * 64 - Game1.viewport.Height / 2;
				Game1.playSound("dwop", null);
			}
			if (Game1.viewport.X < -64)
			{
				Game1.viewport.X = -64;
			}
			if (Game1.viewport.X + Game1.viewport.Width > Game1.currentLocation.Map.Layers[0].LayerWidth * 64 + 128)
			{
				Game1.viewport.X = Game1.currentLocation.Map.Layers[0].LayerWidth * 64 + 128 - Game1.viewport.Width;
			}
			if (Game1.viewport.Y < -64)
			{
				Game1.viewport.Y = -64;
			}
			if (Game1.viewport.Y + Game1.viewport.Height > Game1.currentLocation.Map.Layers[0].LayerHeight * 64 + 128)
			{
				Game1.viewport.Y = Game1.currentLocation.Map.Layers[0].LayerHeight * 64 + 128 - Game1.viewport.Height;
			}
			Game1.oldMouseState = Game1.input.GetMouseState();
			Game1.oldKBState = currentKBState;
		}

		// Token: 0x06000C02 RID: 3074 RVA: 0x000823E8 File Offset: 0x000805E8
		public static bool isLocationAccessible(string locationName)
		{
			if (!(locationName == "Desert"))
			{
				if (!(locationName == "CommunityCenter"))
				{
					if (!(locationName == "JojaMart"))
					{
						if (!(locationName == "Railroad"))
						{
							return true;
						}
						if (Game1.stats.DaysPlayed > 31U)
						{
							return true;
						}
					}
					else if (!Utility.HasAnyPlayerSeenEvent("191393"))
					{
						return true;
					}
				}
				else if (Game1.player.eventsSeen.Contains("191393"))
				{
					return true;
				}
			}
			else if (Game1.MasterPlayer.mailReceived.Contains("ccVault"))
			{
				return true;
			}
			return false;
		}

		// Token: 0x06000C03 RID: 3075 RVA: 0x0008247C File Offset: 0x0008067C
		public static bool isDPadPressed()
		{
			return Game1.isDPadPressed(Game1.input.GetGamePadState());
		}

		// Token: 0x06000C04 RID: 3076 RVA: 0x00082490 File Offset: 0x00080690
		public static bool isDPadPressed(GamePadState pad_state)
		{
			return pad_state.DPad.Up == ButtonState.Pressed || pad_state.DPad.Down == ButtonState.Pressed || pad_state.DPad.Left == ButtonState.Pressed || pad_state.DPad.Right == ButtonState.Pressed;
		}

		// Token: 0x06000C05 RID: 3077 RVA: 0x000824E8 File Offset: 0x000806E8
		public static bool isGamePadThumbstickInMotion(double threshold = 0.2)
		{
			bool inMotion = false;
			GamePadState p = Game1.input.GetGamePadState();
			if ((double)p.ThumbSticks.Left.X < -threshold || p.IsButtonDown(Buttons.LeftThumbstickLeft))
			{
				inMotion = true;
			}
			if ((double)p.ThumbSticks.Left.X > threshold || p.IsButtonDown(Buttons.LeftThumbstickRight))
			{
				inMotion = true;
			}
			if ((double)p.ThumbSticks.Left.Y < -threshold || p.IsButtonDown(Buttons.LeftThumbstickUp))
			{
				inMotion = true;
			}
			if ((double)p.ThumbSticks.Left.Y > threshold || p.IsButtonDown(Buttons.LeftThumbstickDown))
			{
				inMotion = true;
			}
			if ((double)p.ThumbSticks.Right.X < -threshold)
			{
				inMotion = true;
			}
			if ((double)p.ThumbSticks.Right.X > threshold)
			{
				inMotion = true;
			}
			if ((double)p.ThumbSticks.Right.Y < -threshold)
			{
				inMotion = true;
			}
			if ((double)p.ThumbSticks.Right.Y > threshold)
			{
				inMotion = true;
			}
			if (inMotion)
			{
				Game1.thumbstickMotionMargin = 50;
			}
			return Game1.thumbstickMotionMargin > 0;
		}

		// Token: 0x06000C06 RID: 3078 RVA: 0x00082620 File Offset: 0x00080820
		public static bool isAnyGamePadButtonBeingPressed()
		{
			return Utility.getPressedButtons(Game1.input.GetGamePadState(), Game1.oldPadState).Count > 0;
		}

		// Token: 0x06000C07 RID: 3079 RVA: 0x0008264C File Offset: 0x0008084C
		public static bool isAnyGamePadButtonBeingHeld()
		{
			return Utility.getHeldButtons(Game1.input.GetGamePadState()).Count > 0;
		}

		// Token: 0x06000C08 RID: 3080 RVA: 0x00082674 File Offset: 0x00080874
		private static void UpdateChatBox()
		{
			if (Game1.chatBox == null)
			{
				return;
			}
			KeyboardState keyState = Game1.input.GetKeyboardState();
			GamePadState padState = Game1.input.GetGamePadState();
			if (Game1.IsChatting)
			{
				if (Game1.textEntry == null)
				{
					if (padState.IsButtonDown(Buttons.A))
					{
						MouseState mouse = Game1.input.GetMouseState();
						if (Game1.chatBox != null && Game1.chatBox.isActive() && !Game1.chatBox.isHoveringOverClickable(mouse.X, mouse.Y))
						{
							Game1.oldPadState = padState;
							Game1.oldKBState = keyState;
							Game1.showTextEntry(Game1.chatBox.chatBox);
						}
					}
					if (keyState.IsKeyDown(Keys.Escape) || padState.IsButtonDown(Buttons.B) || padState.IsButtonDown(Buttons.Back))
					{
						Game1.chatBox.clickAway();
						Game1.oldKBState = keyState;
						return;
					}
				}
			}
			else if (Game1.keyboardDispatcher.Subscriber == null && ((Game1.isOneOfTheseKeysDown(keyState, Game1.options.chatButton) && Game1.game1.HasKeyboardFocus()) || (!padState.IsButtonDown(Buttons.RightStick) && Game1.rightStickHoldTime > 0 && Game1.rightStickHoldTime < Game1.emoteMenuShowTime)))
			{
				Game1.chatBox.activate();
				if (keyState.IsKeyDown(Keys.OemQuestion))
				{
					Game1.chatBox.setText("/");
				}
			}
		}

		// Token: 0x06000C09 RID: 3081 RVA: 0x000827C0 File Offset: 0x000809C0
		public static KeyboardState GetKeyboardState()
		{
			KeyboardState keyState = Game1.input.GetKeyboardState();
			if (Game1.chatBox != null)
			{
				if (Game1.IsChatting)
				{
					return default(KeyboardState);
				}
				if (Game1.keyboardDispatcher.Subscriber == null && Game1.isOneOfTheseKeysDown(keyState, Game1.options.chatButton) && Game1.game1.HasKeyboardFocus())
				{
					return default(KeyboardState);
				}
			}
			return keyState;
		}

		// Token: 0x06000C0A RID: 3082 RVA: 0x00082828 File Offset: 0x00080A28
		private void UpdateControlInput(GameTime time)
		{
			KeyboardState currentKBState = Game1.GetKeyboardState();
			MouseState currentMouseState = Game1.input.GetMouseState();
			GamePadState currentPadState = Game1.input.GetGamePadState();
			if (Game1.ticks < Game1._activatedTick + 2 && Game1.oldKBState.IsKeyDown(Keys.Tab) != currentKBState.IsKeyDown(Keys.Tab))
			{
				List<Keys> keys = Game1.oldKBState.GetPressedKeys().ToList<Keys>();
				if (currentKBState.IsKeyDown(Keys.Tab))
				{
					keys.Add(Keys.Tab);
				}
				else
				{
					keys.Remove(Keys.Tab);
				}
				Game1.oldKBState = new KeyboardState(keys.ToArray());
			}
			Game1.hooks.OnGame1_UpdateControlInput(ref currentKBState, ref currentMouseState, ref currentPadState, delegate
			{
				if (Game1.options.gamepadControls)
				{
					bool cursor_moved_by_right_thumbstick = false;
					if (Math.Abs(currentPadState.ThumbSticks.Right.X) > 0f || Math.Abs(currentPadState.ThumbSticks.Right.Y) > 0f)
					{
						Game1.setMousePositionRaw((int)((float)currentMouseState.X + currentPadState.ThumbSticks.Right.X * Game1.thumbstickToMouseModifier), (int)((float)currentMouseState.Y - currentPadState.ThumbSticks.Right.Y * Game1.thumbstickToMouseModifier));
						cursor_moved_by_right_thumbstick = true;
					}
					if (Game1.IsChatting)
					{
						cursor_moved_by_right_thumbstick = true;
					}
					if (((Game1.getMouseX() != Game1.getOldMouseX() || Game1.getMouseY() != Game1.getOldMouseY()) && Game1.getMouseX() != 0 && Game1.getMouseY() != 0) || cursor_moved_by_right_thumbstick)
					{
						if (cursor_moved_by_right_thumbstick)
						{
							if (Game1.timerUntilMouseFade <= 0)
							{
								Game1.lastMousePositionBeforeFade = new Point(this.localMultiplayerWindow.Width / 2, this.localMultiplayerWindow.Height / 2);
							}
						}
						else
						{
							Game1.lastCursorMotionWasMouse = true;
						}
						if (Game1.timerUntilMouseFade <= 0 && !Game1.lastCursorMotionWasMouse)
						{
							Game1.setMousePositionRaw(Game1.lastMousePositionBeforeFade.X, Game1.lastMousePositionBeforeFade.Y);
						}
						Game1.timerUntilMouseFade = 4000;
					}
				}
				else if (Game1.getMouseX() != Game1.getOldMouseX() || Game1.getMouseY() != Game1.getOldMouseY())
				{
					Game1.lastCursorMotionWasMouse = true;
				}
				bool actionButtonPressed = false;
				bool switchToolButtonPressed = false;
				bool useToolButtonPressed = false;
				bool useToolButtonReleased = false;
				bool addItemToInventoryButtonPressed = false;
				bool cancelButtonPressed = false;
				bool moveupPressed = false;
				bool moverightPressed = false;
				bool moveleftPressed = false;
				bool movedownPressed = false;
				bool moveupReleased = false;
				bool moverightReleased = false;
				bool movedownReleased = false;
				bool moveleftReleased = false;
				bool moveupHeld = false;
				bool moverightHeld = false;
				bool movedownHeld = false;
				bool moveleftHeld = false;
				bool useToolHeld = false;
				if ((Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.actionButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.actionButton)) || (currentMouseState.RightButton == ButtonState.Pressed && Game1.oldMouseState.RightButton == ButtonState.Released))
				{
					actionButtonPressed = true;
					Game1.rightClickPolling = 250;
				}
				if ((Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.useToolButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.useToolButton)) || (currentMouseState.LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton == ButtonState.Released))
				{
					useToolButtonPressed = true;
				}
				if ((Game1.areAllOfTheseKeysUp(currentKBState, Game1.options.useToolButton) && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.useToolButton)) || (currentMouseState.LeftButton == ButtonState.Released && Game1.oldMouseState.LeftButton == ButtonState.Pressed))
				{
					useToolButtonReleased = true;
				}
				if (currentMouseState.ScrollWheelValue != Game1.oldMouseState.ScrollWheelValue)
				{
					switchToolButtonPressed = true;
				}
				if ((Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.cancelButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.cancelButton)) || (currentMouseState.RightButton == ButtonState.Pressed && Game1.oldMouseState.RightButton == ButtonState.Released))
				{
					cancelButtonPressed = true;
				}
				if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveUpButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveUpButton))
				{
					moveupPressed = true;
				}
				if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveRightButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveRightButton))
				{
					moverightPressed = true;
				}
				if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveDownButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveDownButton))
				{
					movedownPressed = true;
				}
				if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveLeftButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.moveLeftButton))
				{
					moveleftPressed = true;
				}
				if (Game1.areAllOfTheseKeysUp(currentKBState, Game1.options.moveUpButton) && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveUpButton))
				{
					moveupReleased = true;
				}
				if (Game1.areAllOfTheseKeysUp(currentKBState, Game1.options.moveRightButton) && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveRightButton))
				{
					moverightReleased = true;
				}
				if (Game1.areAllOfTheseKeysUp(currentKBState, Game1.options.moveDownButton) && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveDownButton))
				{
					movedownReleased = true;
				}
				if (Game1.areAllOfTheseKeysUp(currentKBState, Game1.options.moveLeftButton) && Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveLeftButton))
				{
					moveleftReleased = true;
				}
				if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveUpButton))
				{
					moveupHeld = true;
				}
				if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveRightButton))
				{
					moverightHeld = true;
				}
				if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveDownButton))
				{
					movedownHeld = true;
				}
				if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveLeftButton))
				{
					moveleftHeld = true;
				}
				if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.useToolButton) || currentMouseState.LeftButton == ButtonState.Pressed)
				{
					useToolHeld = true;
				}
				if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.actionButton) || currentMouseState.RightButton == ButtonState.Pressed)
				{
					Game1.rightClickPolling -= time.ElapsedGameTime.Milliseconds;
					if (Game1.rightClickPolling <= 0)
					{
						Game1.rightClickPolling = 100;
						actionButtonPressed = true;
					}
				}
				if (Game1.options.gamepadControls)
				{
					if (currentKBState.GetPressedKeys().Length != 0 || currentMouseState.LeftButton == ButtonState.Pressed || currentMouseState.RightButton == ButtonState.Pressed)
					{
						Game1.timerUntilMouseFade = 4000;
					}
					if (currentPadState.IsButtonDown(Buttons.A) && !Game1.oldPadState.IsButtonDown(Buttons.A))
					{
						actionButtonPressed = true;
						Game1.lastCursorMotionWasMouse = false;
						Game1.rightClickPolling = 250;
					}
					if (currentPadState.IsButtonDown(Buttons.X) && !Game1.oldPadState.IsButtonDown(Buttons.X))
					{
						useToolButtonPressed = true;
						Game1.lastCursorMotionWasMouse = false;
					}
					if (!currentPadState.IsButtonDown(Buttons.X) && Game1.oldPadState.IsButtonDown(Buttons.X))
					{
						useToolButtonReleased = true;
					}
					if (currentPadState.IsButtonDown(Buttons.RightTrigger) && !Game1.oldPadState.IsButtonDown(Buttons.RightTrigger))
					{
						switchToolButtonPressed = true;
						Game1.triggerPolling = 300;
					}
					else if (currentPadState.IsButtonDown(Buttons.LeftTrigger) && !Game1.oldPadState.IsButtonDown(Buttons.LeftTrigger))
					{
						switchToolButtonPressed = true;
						Game1.triggerPolling = 300;
					}
					if (currentPadState.IsButtonDown(Buttons.X))
					{
						useToolHeld = true;
					}
					if (currentPadState.IsButtonDown(Buttons.A))
					{
						Game1.rightClickPolling -= time.ElapsedGameTime.Milliseconds;
						if (Game1.rightClickPolling <= 0)
						{
							Game1.rightClickPolling = 100;
							actionButtonPressed = true;
						}
					}
					if (currentPadState.IsButtonDown(Buttons.RightTrigger) || currentPadState.IsButtonDown(Buttons.LeftTrigger))
					{
						Game1.triggerPolling -= time.ElapsedGameTime.Milliseconds;
						if (Game1.triggerPolling <= 0)
						{
							Game1.triggerPolling = 100;
							switchToolButtonPressed = true;
						}
					}
					if (currentPadState.IsButtonDown(Buttons.RightShoulder) && !Game1.oldPadState.IsButtonDown(Buttons.RightShoulder) && Game1.IsHudDrawn)
					{
						Game1.player.shiftToolbar(true);
					}
					if (currentPadState.IsButtonDown(Buttons.LeftShoulder) && !Game1.oldPadState.IsButtonDown(Buttons.LeftShoulder) && Game1.IsHudDrawn)
					{
						Game1.player.shiftToolbar(false);
					}
					if (currentPadState.IsButtonDown(Buttons.DPadUp) && !Game1.oldPadState.IsButtonDown(Buttons.DPadUp))
					{
						moveupPressed = true;
					}
					else if (!currentPadState.IsButtonDown(Buttons.DPadUp) && Game1.oldPadState.IsButtonDown(Buttons.DPadUp))
					{
						moveupReleased = true;
					}
					if (currentPadState.IsButtonDown(Buttons.DPadRight) && !Game1.oldPadState.IsButtonDown(Buttons.DPadRight))
					{
						moverightPressed = true;
					}
					else if (!currentPadState.IsButtonDown(Buttons.DPadRight) && Game1.oldPadState.IsButtonDown(Buttons.DPadRight))
					{
						moverightReleased = true;
					}
					if (currentPadState.IsButtonDown(Buttons.DPadDown) && !Game1.oldPadState.IsButtonDown(Buttons.DPadDown))
					{
						movedownPressed = true;
					}
					else if (!currentPadState.IsButtonDown(Buttons.DPadDown) && Game1.oldPadState.IsButtonDown(Buttons.DPadDown))
					{
						movedownReleased = true;
					}
					if (currentPadState.IsButtonDown(Buttons.DPadLeft) && !Game1.oldPadState.IsButtonDown(Buttons.DPadLeft))
					{
						moveleftPressed = true;
					}
					else if (!currentPadState.IsButtonDown(Buttons.DPadLeft) && Game1.oldPadState.IsButtonDown(Buttons.DPadLeft))
					{
						moveleftReleased = true;
					}
					if (currentPadState.IsButtonDown(Buttons.DPadUp))
					{
						moveupHeld = true;
					}
					if (currentPadState.IsButtonDown(Buttons.DPadRight))
					{
						moverightHeld = true;
					}
					if (currentPadState.IsButtonDown(Buttons.DPadDown))
					{
						movedownHeld = true;
					}
					if (currentPadState.IsButtonDown(Buttons.DPadLeft))
					{
						moveleftHeld = true;
					}
					if ((double)currentPadState.ThumbSticks.Left.X < -0.2)
					{
						moveleftPressed = true;
						moveleftHeld = true;
					}
					else if ((double)currentPadState.ThumbSticks.Left.X > 0.2)
					{
						moverightPressed = true;
						moverightHeld = true;
					}
					if ((double)currentPadState.ThumbSticks.Left.Y < -0.2)
					{
						movedownPressed = true;
						movedownHeld = true;
					}
					else if ((double)currentPadState.ThumbSticks.Left.Y > 0.2)
					{
						moveupPressed = true;
						moveupHeld = true;
					}
					if ((double)Game1.oldPadState.ThumbSticks.Left.X < -0.2 && !moveleftHeld)
					{
						moveleftReleased = true;
					}
					if ((double)Game1.oldPadState.ThumbSticks.Left.X > 0.2 && !moverightHeld)
					{
						moverightReleased = true;
					}
					if ((double)Game1.oldPadState.ThumbSticks.Left.Y < -0.2 && !movedownHeld)
					{
						movedownReleased = true;
					}
					if ((double)Game1.oldPadState.ThumbSticks.Left.Y > 0.2 && !moveupHeld)
					{
						moveupReleased = true;
					}
					if (this.controllerSlingshotSafeTime > 0f)
					{
						if (!currentPadState.IsButtonDown(Buttons.DPadUp) && !currentPadState.IsButtonDown(Buttons.DPadDown) && !currentPadState.IsButtonDown(Buttons.DPadLeft) && !currentPadState.IsButtonDown(Buttons.DPadRight) && (double)Math.Abs(currentPadState.ThumbSticks.Left.X) < 0.04 && (double)Math.Abs(currentPadState.ThumbSticks.Left.Y) < 0.04)
						{
							this.controllerSlingshotSafeTime = 0f;
						}
						if (this.controllerSlingshotSafeTime <= 0f)
						{
							this.controllerSlingshotSafeTime = 0f;
						}
						else
						{
							this.controllerSlingshotSafeTime -= (float)time.ElapsedGameTime.TotalSeconds;
							moveupPressed = false;
							movedownPressed = false;
							moveleftPressed = false;
							moverightPressed = false;
							moveupHeld = false;
							movedownHeld = false;
							moveleftHeld = false;
							moverightHeld = false;
						}
					}
				}
				else
				{
					this.controllerSlingshotSafeTime = 0f;
				}
				Game1.ResetFreeCursorDrag();
				if (useToolHeld)
				{
					Game1.mouseClickPolling += time.ElapsedGameTime.Milliseconds;
				}
				else
				{
					Game1.mouseClickPolling = 0;
				}
				if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.toolbarSwap) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.toolbarSwap) && Game1.IsHudDrawn)
				{
					Game1.player.shiftToolbar(!currentKBState.IsKeyDown(Keys.LeftControl));
				}
				if (Game1.mouseClickPolling > 250 && (!(Game1.player.CurrentTool is FishingRod) || Game1.player.CurrentTool.upgradeLevel.Value <= 0))
				{
					useToolButtonPressed = true;
					Game1.mouseClickPolling = 100;
				}
				Game1.PushUIMode();
				foreach (IClickableMenu menu in Game1.onScreenMenus)
				{
					if ((Game1.IsHudDrawn || menu == Game1.chatBox) && Game1.wasMouseVisibleThisFrame && menu.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()))
					{
						menu.performHoverAction(Game1.getMouseX(), Game1.getMouseY());
					}
				}
				Game1.PopUIMode();
				if (Game1.chatBox != null && Game1.chatBox.chatBox.Selected && Game1.oldMouseState.ScrollWheelValue != currentMouseState.ScrollWheelValue)
				{
					Game1.chatBox.receiveScrollWheelAction(currentMouseState.ScrollWheelValue - Game1.oldMouseState.ScrollWheelValue);
				}
				if (Game1.panMode)
				{
					this.updatePanModeControls(currentMouseState, currentKBState);
					return;
				}
				if (Game1.inputSimulator != null)
				{
					if (currentKBState.IsKeyDown(Keys.Escape))
					{
						Game1.inputSimulator = null;
					}
					else
					{
						Game1.inputSimulator.SimulateInput(ref actionButtonPressed, ref switchToolButtonPressed, ref useToolButtonPressed, ref useToolButtonReleased, ref addItemToInventoryButtonPressed, ref cancelButtonPressed, ref moveupPressed, ref moverightPressed, ref moveleftPressed, ref movedownPressed, ref moveupReleased, ref moverightReleased, ref moveleftReleased, ref movedownReleased, ref moveupHeld, ref moverightHeld, ref moveleftHeld, ref movedownHeld);
					}
				}
				if (useToolButtonReleased && Game1.player.CurrentTool != null && Game1.CurrentEvent == null && Game1.pauseTime <= 0f && Game1.player.CurrentTool.onRelease(Game1.currentLocation, Game1.getMouseX(), Game1.getMouseY(), Game1.player))
				{
					Game1.oldMouseState = Game1.input.GetMouseState();
					Game1.oldKBState = currentKBState;
					Game1.oldPadState = currentPadState;
					Game1.player.usingSlingshot = false;
					Game1.player.canReleaseTool = true;
					Game1.player.UsingTool = false;
					Game1.player.CanMove = true;
					return;
				}
				if (((useToolButtonPressed && !Game1.isAnyGamePadButtonBeingPressed()) || (actionButtonPressed && Game1.isAnyGamePadButtonBeingPressed())) && Game1.pauseTime <= 0f && Game1.wasMouseVisibleThisFrame)
				{
					if (Game1.debugMode)
					{
						Console.WriteLine((Game1.getMouseX() + Game1.viewport.X).ToString() + ", " + (Game1.getMouseY() + Game1.viewport.Y).ToString());
					}
					Game1.PushUIMode();
					foreach (IClickableMenu menu2 in Game1.onScreenMenus)
					{
						if (Game1.IsHudDrawn || menu2 == Game1.chatBox)
						{
							if (!Game1.IsChatting || menu2 == Game1.chatBox)
							{
								LevelUpMenu levelUpMenu = menu2 as LevelUpMenu;
								if ((levelUpMenu == null || levelUpMenu.informationUp) && menu2.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()))
								{
									menu2.receiveLeftClick(Game1.getMouseX(), Game1.getMouseY(), true);
									Game1.PopUIMode();
									Game1.oldMouseState = Game1.input.GetMouseState();
									Game1.oldKBState = currentKBState;
									Game1.oldPadState = currentPadState;
									return;
								}
							}
							if (menu2 == Game1.chatBox && Game1.options.gamepadControls && Game1.IsChatting)
							{
								Game1.oldMouseState = Game1.input.GetMouseState();
								Game1.oldKBState = currentKBState;
								Game1.oldPadState = currentPadState;
								Game1.PopUIMode();
								return;
							}
							menu2.clickAway();
						}
					}
					Game1.PopUIMode();
				}
				if (Game1.IsChatting || Game1.player.freezePause > 0)
				{
					if (Game1.IsChatting)
					{
						foreach (Buttons b in Utility.getPressedButtons(currentPadState, Game1.oldPadState))
						{
							Game1.chatBox.receiveGamePadButton(b);
						}
					}
					Game1.oldMouseState = Game1.input.GetMouseState();
					Game1.oldKBState = currentKBState;
					Game1.oldPadState = currentPadState;
					return;
				}
				if (Game1.paused || Game1.HostPaused)
				{
					if (!Game1.HostPaused || !Game1.IsMasterGame || (!Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.menuButton) && !currentPadState.IsButtonDown(Buttons.B) && !currentPadState.IsButtonDown(Buttons.Back)))
					{
						Game1.oldMouseState = Game1.input.GetMouseState();
						return;
					}
					Game1.netWorldState.Value.IsPaused = false;
					ChatBox chatBox = Game1.chatBox;
					if (chatBox != null)
					{
						chatBox.globalInfoMessage("Resumed", Array.Empty<string>());
					}
				}
				if (Game1.eventUp)
				{
					if (Game1.currentLocation.currentEvent == null && Game1.locationRequest == null)
					{
						Game1.eventUp = false;
					}
					else if (actionButtonPressed || useToolButtonPressed)
					{
						Event currentEvent = Game1.CurrentEvent;
						if (currentEvent != null)
						{
							currentEvent.receiveMouseClick(Game1.getMouseX(), Game1.getMouseY());
						}
					}
				}
				bool event_or_farm_event_up = Game1.eventUp || Game1.farmEvent != null;
				if (actionButtonPressed || (Game1.dialogueUp && useToolButtonPressed))
				{
					Game1.PushUIMode();
					foreach (IClickableMenu menu3 in Game1.onScreenMenus)
					{
						if (Game1.wasMouseVisibleThisFrame && (Game1.IsHudDrawn || menu3 == Game1.chatBox) && menu3.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()))
						{
							LevelUpMenu levelUpMenu2 = menu3 as LevelUpMenu;
							if (levelUpMenu2 == null || levelUpMenu2.informationUp)
							{
								menu3.receiveRightClick(Game1.getMouseX(), Game1.getMouseY(), true);
								Game1.oldMouseState = Game1.input.GetMouseState();
								if (!Game1.isAnyGamePadButtonBeingPressed())
								{
									Game1.PopUIMode();
									Game1.oldKBState = currentKBState;
									Game1.oldPadState = currentPadState;
									return;
								}
							}
						}
					}
					Game1.PopUIMode();
					if (!Game1.pressActionButton(currentKBState, currentMouseState, currentPadState))
					{
						Game1.oldKBState = currentKBState;
						Game1.oldMouseState = Game1.input.GetMouseState();
						Game1.oldPadState = currentPadState;
						return;
					}
				}
				if (useToolButtonPressed && (!Game1.player.UsingTool || Game1.player.CurrentTool is MeleeWeapon) && !Game1.player.isEating && !Game1.dialogueUp && Game1.farmEvent == null && (Game1.player.CanMove || Game1.player.CurrentTool is MeleeWeapon))
				{
					if (Game1.player.CurrentTool != null && (!(Game1.player.CurrentTool is MeleeWeapon) || Game1.didPlayerJustLeftClick(true)))
					{
						Game1.player.FireTool();
					}
					if (!Game1.pressUseToolButton() && Game1.player.canReleaseTool && Game1.player.UsingTool)
					{
						Tool currentTool = Game1.player.CurrentTool;
					}
					if (Game1.player.UsingTool)
					{
						Game1.oldMouseState = Game1.input.GetMouseState();
						Game1.oldKBState = currentKBState;
						Game1.oldPadState = currentPadState;
						return;
					}
				}
				if (useToolButtonReleased && this._didInitiateItemStow)
				{
					this._didInitiateItemStow = false;
				}
				if (useToolButtonReleased && Game1.player.canReleaseTool && Game1.player.UsingTool && Game1.player.CurrentTool != null)
				{
					Game1.player.EndUsingTool();
				}
				if (switchToolButtonPressed && !Game1.player.UsingTool && !Game1.dialogueUp && Game1.player.CanMove && Game1.player.Items.HasAny() && !event_or_farm_event_up)
				{
					Game1.pressSwitchToolButton();
				}
				if (Game1.player.CurrentTool != null && useToolHeld && Game1.player.canReleaseTool && !event_or_farm_event_up && !Game1.dialogueUp && Game1.player.Stamina >= 1f && !(Game1.player.CurrentTool is FishingRod))
				{
					int extraUpgradeLevel = (Game1.player.CurrentTool.hasEnchantmentOfType<ReachingToolEnchantment>() > false) ? 1 : 0;
					if (Game1.player.toolHold.Value <= 0 && Game1.player.CurrentTool.upgradeLevel.Value + extraUpgradeLevel > Game1.player.toolPower.Value)
					{
						float modifier = 1f;
						if (Game1.player.CurrentTool != null)
						{
							modifier = Game1.player.CurrentTool.AnimationSpeedModifier;
						}
						Game1.player.toolHold.Value = (int)(600f * modifier);
						Game1.player.toolHoldStartTime.Value = Game1.player.toolHold.Value;
					}
					else if (Game1.player.CurrentTool.upgradeLevel.Value + extraUpgradeLevel > Game1.player.toolPower.Value)
					{
						Game1.player.toolHold.Value -= time.ElapsedGameTime.Milliseconds;
						if (Game1.player.toolHold.Value <= 0)
						{
							Game1.player.toolPowerIncrease();
						}
					}
				}
				if (Game1.upPolling >= 650f)
				{
					moveupPressed = true;
					Game1.upPolling -= 100f;
				}
				else if (Game1.downPolling >= 650f)
				{
					movedownPressed = true;
					Game1.downPolling -= 100f;
				}
				else if (Game1.rightPolling >= 650f)
				{
					moverightPressed = true;
					Game1.rightPolling -= 100f;
				}
				else if (Game1.leftPolling >= 650f)
				{
					moveleftPressed = true;
					Game1.leftPolling -= 100f;
				}
				else if (Game1.pauseTime <= 0f && Game1.locationRequest == null && (!Game1.player.UsingTool || Game1.player.canStrafeForToolUse()) && (!event_or_farm_event_up || (Game1.CurrentEvent != null && Game1.CurrentEvent.playerControlSequence)))
				{
					if (Game1.player.movementDirections.Count < 2)
					{
						if (moveupHeld)
						{
							Game1.player.setMoving(1);
						}
						if (moverightHeld)
						{
							Game1.player.setMoving(2);
						}
						if (movedownHeld)
						{
							Game1.player.setMoving(4);
						}
						if (moveleftHeld)
						{
							Game1.player.setMoving(8);
						}
					}
					if (moveupReleased || (Game1.player.movementDirections.Contains(0) && !moveupHeld))
					{
						Game1.player.setMoving(33);
						if (Game1.player.movementDirections.Count == 0)
						{
							Game1.player.setMoving(64);
						}
					}
					if (moverightReleased || (Game1.player.movementDirections.Contains(1) && !moverightHeld))
					{
						Game1.player.setMoving(34);
						if (Game1.player.movementDirections.Count == 0)
						{
							Game1.player.setMoving(64);
						}
					}
					if (movedownReleased || (Game1.player.movementDirections.Contains(2) && !movedownHeld))
					{
						Game1.player.setMoving(36);
						if (Game1.player.movementDirections.Count == 0)
						{
							Game1.player.setMoving(64);
						}
					}
					if (moveleftReleased || (Game1.player.movementDirections.Contains(3) && !moveleftHeld))
					{
						Game1.player.setMoving(40);
						if (Game1.player.movementDirections.Count == 0)
						{
							Game1.player.setMoving(64);
						}
					}
					if ((!moveupHeld && !moverightHeld && !movedownHeld && !moveleftHeld && !Game1.player.UsingTool) || Game1.activeClickableMenu != null)
					{
						Game1.player.Halt();
					}
				}
				else if (Game1.isQuestion)
				{
					if (moveupPressed)
					{
						Game1.currentQuestionChoice = Math.Max(Game1.currentQuestionChoice - 1, 0);
						Game1.playSound("toolSwap", null);
					}
					else if (movedownPressed)
					{
						Game1.currentQuestionChoice = Math.Min(Game1.currentQuestionChoice + 1, Game1.questionChoices.Count - 1);
						Game1.playSound("toolSwap", null);
					}
				}
				if (moveupHeld && !Game1.player.CanMove)
				{
					Game1.upPolling += (float)time.ElapsedGameTime.Milliseconds;
				}
				else if (movedownHeld && !Game1.player.CanMove)
				{
					Game1.downPolling += (float)time.ElapsedGameTime.Milliseconds;
				}
				else if (moverightHeld && !Game1.player.CanMove)
				{
					Game1.rightPolling += (float)time.ElapsedGameTime.Milliseconds;
				}
				else if (moveleftHeld && !Game1.player.CanMove)
				{
					Game1.leftPolling += (float)time.ElapsedGameTime.Milliseconds;
				}
				else if (moveupReleased)
				{
					Game1.upPolling = 0f;
				}
				else if (movedownReleased)
				{
					Game1.downPolling = 0f;
				}
				else if (moverightReleased)
				{
					Game1.rightPolling = 0f;
				}
				else if (moveleftReleased)
				{
					Game1.leftPolling = 0f;
				}
				if (Game1.debugMode)
				{
					if (currentKBState.IsKeyDown(Keys.Q))
					{
						Game1.oldKBState.IsKeyDown(Keys.Q);
					}
					if (currentKBState.IsKeyDown(Keys.P) && !Game1.oldKBState.IsKeyDown(Keys.P))
					{
						Game1.NewDay(0f);
					}
					if (currentKBState.IsKeyDown(Keys.M) && !Game1.oldKBState.IsKeyDown(Keys.M))
					{
						Game1.dayOfMonth = 28;
						Game1.NewDay(0f);
					}
					if (currentKBState.IsKeyDown(Keys.T) && !Game1.oldKBState.IsKeyDown(Keys.T))
					{
						Game1.addHour();
					}
					if (currentKBState.IsKeyDown(Keys.Y) && !Game1.oldKBState.IsKeyDown(Keys.Y))
					{
						Game1.addMinute();
					}
					if (currentKBState.IsKeyDown(Keys.D1) && !Game1.oldKBState.IsKeyDown(Keys.D1))
					{
						Game1.warpFarmer("Mountain", 15, 35, false);
					}
					if (currentKBState.IsKeyDown(Keys.D2) && !Game1.oldKBState.IsKeyDown(Keys.D2))
					{
						Game1.warpFarmer("Town", 35, 35, false);
					}
					if (currentKBState.IsKeyDown(Keys.D3) && !Game1.oldKBState.IsKeyDown(Keys.D3))
					{
						Game1.warpFarmer("Farm", 64, 15, false);
					}
					if (currentKBState.IsKeyDown(Keys.D4) && !Game1.oldKBState.IsKeyDown(Keys.D4))
					{
						Game1.warpFarmer("Forest", 34, 13, false);
					}
					if (currentKBState.IsKeyDown(Keys.D5) && !Game1.oldKBState.IsKeyDown(Keys.D4))
					{
						Game1.warpFarmer("Beach", 34, 10, false);
					}
					if (currentKBState.IsKeyDown(Keys.D6) && !Game1.oldKBState.IsKeyDown(Keys.D6))
					{
						Game1.warpFarmer("Mine", 18, 12, false);
					}
					if (currentKBState.IsKeyDown(Keys.D7) && !Game1.oldKBState.IsKeyDown(Keys.D7))
					{
						Game1.warpFarmer("SandyHouse", 16, 3, false);
					}
					if (currentKBState.IsKeyDown(Keys.K) && !Game1.oldKBState.IsKeyDown(Keys.K))
					{
						Game1.enterMine(Game1.mine.mineLevel + 1, null);
					}
					if (currentKBState.IsKeyDown(Keys.H) && !Game1.oldKBState.IsKeyDown(Keys.H))
					{
						Game1.player.changeHat(Game1.random.Next(FarmerRenderer.hatsTexture.Height / 80 * 12));
					}
					if (currentKBState.IsKeyDown(Keys.I) && !Game1.oldKBState.IsKeyDown(Keys.I))
					{
						Game1.player.changeHairStyle(Game1.random.Next(FarmerRenderer.hairStylesTexture.Height / 96 * 8));
					}
					if (currentKBState.IsKeyDown(Keys.J) && !Game1.oldKBState.IsKeyDown(Keys.J))
					{
						Game1.player.changeShirt(Game1.random.Next(1000, 1040).ToString());
						Game1.player.changePantsColor(new Color(Game1.random.Next(255), Game1.random.Next(255), Game1.random.Next(255)));
					}
					if (currentKBState.IsKeyDown(Keys.L) && !Game1.oldKBState.IsKeyDown(Keys.L))
					{
						Game1.player.changeShirt(Game1.random.Next(1000, 1040).ToString());
						Game1.player.changePantsColor(new Color(Game1.random.Next(255), Game1.random.Next(255), Game1.random.Next(255)));
						Game1.player.changeHairStyle(Game1.random.Next(FarmerRenderer.hairStylesTexture.Height / 96 * 8));
						if (Game1.random.NextBool())
						{
							Game1.player.changeHat(Game1.random.Next(-1, FarmerRenderer.hatsTexture.Height / 80 * 12));
						}
						else
						{
							Game1.player.changeHat(-1);
						}
						Game1.player.changeHairColor(new Color(Game1.random.Next(255), Game1.random.Next(255), Game1.random.Next(255)));
						Game1.player.changeSkinColor(Game1.random.Next(16), false);
					}
					if (currentKBState.IsKeyDown(Keys.U) && !Game1.oldKBState.IsKeyDown(Keys.U))
					{
						FarmHouse farmHouse = Game1.RequireLocation<FarmHouse>("FarmHouse", false);
						farmHouse.SetWallpaper(Game1.random.Next(112).ToString(), null);
						farmHouse.SetFloor(Game1.random.Next(40).ToString(), null);
					}
					if (currentKBState.IsKeyDown(Keys.F2))
					{
						Game1.oldKBState.IsKeyDown(Keys.F2);
					}
					if (currentKBState.IsKeyDown(Keys.F5) && !Game1.oldKBState.IsKeyDown(Keys.F5))
					{
						Game1.displayFarmer = !Game1.displayFarmer;
					}
					if (currentKBState.IsKeyDown(Keys.F6))
					{
						Game1.oldKBState.IsKeyDown(Keys.F6);
					}
					if (currentKBState.IsKeyDown(Keys.F7) && !Game1.oldKBState.IsKeyDown(Keys.F7))
					{
						Game1.drawGrid = !Game1.drawGrid;
					}
					if (currentKBState.IsKeyDown(Keys.B) && !Game1.oldKBState.IsKeyDown(Keys.B) && Game1.IsHudDrawn)
					{
						Game1.player.shiftToolbar(false);
					}
					if (currentKBState.IsKeyDown(Keys.N) && !Game1.oldKBState.IsKeyDown(Keys.N) && Game1.IsHudDrawn)
					{
						Game1.player.shiftToolbar(true);
					}
					if (currentKBState.IsKeyDown(Keys.F10) && !Game1.oldKBState.IsKeyDown(Keys.F10) && Game1.server == null)
					{
						Game1.multiplayer.StartServer();
					}
				}
				else if (!Game1.player.UsingTool)
				{
					if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.inventorySlot1) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot1))
					{
						Game1.player.CurrentToolIndex = 0;
					}
					else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.inventorySlot2) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot2))
					{
						Game1.player.CurrentToolIndex = 1;
					}
					else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.inventorySlot3) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot3))
					{
						Game1.player.CurrentToolIndex = 2;
					}
					else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.inventorySlot4) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot4))
					{
						Game1.player.CurrentToolIndex = 3;
					}
					else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.inventorySlot5) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot5))
					{
						Game1.player.CurrentToolIndex = 4;
					}
					else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.inventorySlot6) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot6))
					{
						Game1.player.CurrentToolIndex = 5;
					}
					else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.inventorySlot7) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot7))
					{
						Game1.player.CurrentToolIndex = 6;
					}
					else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.inventorySlot8) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot8))
					{
						Game1.player.CurrentToolIndex = 7;
					}
					else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.inventorySlot9) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot9))
					{
						Game1.player.CurrentToolIndex = 8;
					}
					else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.inventorySlot10) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot10))
					{
						Game1.player.CurrentToolIndex = 9;
					}
					else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.inventorySlot11) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot11))
					{
						Game1.player.CurrentToolIndex = 10;
					}
					else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.inventorySlot12) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.inventorySlot12))
					{
						Game1.player.CurrentToolIndex = 11;
					}
				}
				if (((Game1.options.gamepadControls && Game1.rightStickHoldTime >= Game1.emoteMenuShowTime && Game1.activeClickableMenu == null) || (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.emoteButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.emoteButton))) && !Game1.debugMode && Game1.player.CanEmote())
				{
					if (Game1.player.CanMove)
					{
						Game1.player.Halt();
					}
					Game1.emoteMenu = new EmoteMenu();
					Game1.emoteMenu.gamepadMode = (Game1.options.gamepadControls && Game1.rightStickHoldTime >= Game1.emoteMenuShowTime);
					Game1.timerUntilMouseFade = 0;
				}
				if (!Program.releaseBuild)
				{
					if (Game1.IsPressEvent(ref currentKBState, Keys.F3) || Game1.IsPressEvent(ref currentPadState, Buttons.LeftStick))
					{
						Game1.debugMode = !Game1.debugMode;
						if (Game1.gameMode == 11)
						{
							Game1.gameMode = 3;
						}
					}
					if (Game1.IsPressEvent(ref currentKBState, Keys.F8))
					{
						this.requestDebugInput();
					}
				}
				if (currentKBState.IsKeyDown(Keys.F4) && !Game1.oldKBState.IsKeyDown(Keys.F4))
				{
					Game1.displayHUD = !Game1.displayHUD;
					Game1.playSound("smallSelect", null);
					if (!Game1.displayHUD)
					{
						Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3666"));
					}
				}
				bool menuButtonPressed = Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.menuButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.menuButton);
				bool journalButtonPressed = Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.journalButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.journalButton);
				bool mapButtonPressed = Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.mapButton) && Game1.areAllOfTheseKeysUp(Game1.oldKBState, Game1.options.mapButton);
				if (Game1.options.gamepadControls && !menuButtonPressed)
				{
					menuButtonPressed = ((currentPadState.IsButtonDown(Buttons.Start) && !Game1.oldPadState.IsButtonDown(Buttons.Start)) || (currentPadState.IsButtonDown(Buttons.B) && !Game1.oldPadState.IsButtonDown(Buttons.B)));
				}
				if (Game1.options.gamepadControls && !journalButtonPressed)
				{
					journalButtonPressed = (currentPadState.IsButtonDown(Buttons.Back) && !Game1.oldPadState.IsButtonDown(Buttons.Back));
				}
				if (Game1.options.gamepadControls && !mapButtonPressed)
				{
					mapButtonPressed = (currentPadState.IsButtonDown(Buttons.Y) && !Game1.oldPadState.IsButtonDown(Buttons.Y));
				}
				if (menuButtonPressed && Game1.CanShowPauseMenu())
				{
					if (Game1.activeClickableMenu == null)
					{
						Game1.PushUIMode();
						Game1.activeClickableMenu = new GameMenu(true);
						Game1.PopUIMode();
					}
					else if (Game1.activeClickableMenu.readyToClose())
					{
						Game1.exitActiveMenu();
					}
				}
				if (Game1.dayOfMonth > 0 && Game1.player.CanMove && journalButtonPressed && !Game1.dialogueUp && !event_or_farm_event_up)
				{
					if (Game1.activeClickableMenu == null)
					{
						Game1.activeClickableMenu = new QuestLog();
					}
				}
				else if (event_or_farm_event_up && Game1.CurrentEvent != null && journalButtonPressed && !Game1.CurrentEvent.skipped && Game1.CurrentEvent.skippable)
				{
					Game1.CurrentEvent.skipped = true;
					Game1.CurrentEvent.skipEvent();
					Game1.freezeControls = false;
				}
				if (Game1.options.gamepadControls && Game1.dayOfMonth > 0 && Game1.player.CanMove && Game1.isAnyGamePadButtonBeingPressed() && mapButtonPressed && !Game1.dialogueUp && !event_or_farm_event_up)
				{
					if (Game1.activeClickableMenu == null)
					{
						Game1.PushUIMode();
						Game1.activeClickableMenu = new GameMenu(GameMenu.craftingTab, -1, true);
						Game1.PopUIMode();
					}
				}
				else if (Game1.dayOfMonth > 0 && Game1.player.CanMove && mapButtonPressed && !Game1.dialogueUp && !event_or_farm_event_up && Game1.activeClickableMenu == null)
				{
					Game1.PushUIMode();
					Game1.activeClickableMenu = new GameMenu(GameMenu.mapTab, -1, true);
					Game1.PopUIMode();
				}
				Game1.checkForRunButton(currentKBState, false);
				Game1.oldKBState = currentKBState;
				Game1.oldMouseState = Game1.input.GetMouseState();
				Game1.oldPadState = currentPadState;
			});
		}

		// Token: 0x06000C0B RID: 3083 RVA: 0x00082908 File Offset: 0x00080B08
		public static bool CanShowPauseMenu()
		{
			return Game1.dayOfMonth > 0 && Game1.player.CanMove && !Game1.dialogueUp && (!Game1.eventUp || (Game1.isFestival() && Game1.CurrentEvent.festivalTimer <= 0)) && Game1.currentMinigame == null && Game1.farmEvent == null;
		}

		// Token: 0x06000C0C RID: 3084 RVA: 0x0008295C File Offset: 0x00080B5C
		internal static void addHour()
		{
			Game1.timeOfDay += 100;
			foreach (GameLocation g in Game1.locations)
			{
				for (int i = 0; i < g.characters.Count; i++)
				{
					NPC npc = g.characters[i];
					npc.checkSchedule(Game1.timeOfDay);
					npc.checkSchedule(Game1.timeOfDay - 50);
					npc.checkSchedule(Game1.timeOfDay - 60);
					npc.checkSchedule(Game1.timeOfDay - 70);
					npc.checkSchedule(Game1.timeOfDay - 80);
					npc.checkSchedule(Game1.timeOfDay - 90);
				}
			}
			int num = Game1.timeOfDay;
			if (num == 1900)
			{
				Game1.currentLocation.switchOutNightTiles();
				return;
			}
			if (num != 2000)
			{
				return;
			}
			if (!Game1.currentLocation.IsRainingHere())
			{
				Game1.changeMusicTrack("none", false, MusicContext.Default);
			}
		}

		// Token: 0x06000C0D RID: 3085 RVA: 0x00082A60 File Offset: 0x00080C60
		internal static void addMinute()
		{
			if (Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift))
			{
				Game1.timeOfDay -= 10;
			}
			else
			{
				Game1.timeOfDay += 10;
			}
			if (Game1.timeOfDay % 100 == 60)
			{
				Game1.timeOfDay += 40;
			}
			if (Game1.timeOfDay % 100 == 90)
			{
				Game1.timeOfDay -= 40;
			}
			Game1.currentLocation.performTenMinuteUpdate(Game1.timeOfDay);
			foreach (GameLocation g in Game1.locations)
			{
				for (int i = 0; i < g.characters.Count; i++)
				{
					g.characters[i].checkSchedule(Game1.timeOfDay);
				}
			}
			if (Game1.isLightning && Game1.IsMasterGame)
			{
				Utility.performLightningUpdate(Game1.timeOfDay);
			}
			int num = Game1.timeOfDay;
			if (num == 1750)
			{
				Game1.outdoorLight = Color.White;
				return;
			}
			if (num == 1900)
			{
				Game1.currentLocation.switchOutNightTiles();
				return;
			}
			if (num != 2000)
			{
				return;
			}
			if (!Game1.currentLocation.IsRainingHere())
			{
				Game1.changeMusicTrack("none", false, MusicContext.Default);
			}
		}

		// Token: 0x06000C0E RID: 3086 RVA: 0x00082BAC File Offset: 0x00080DAC
		public static void checkForRunButton(KeyboardState kbState, bool ignoreKeyPressQualifier = false)
		{
			bool wasRunning = Game1.player.running;
			bool runPressed = Game1.isOneOfTheseKeysDown(kbState, Game1.options.runButton) && (!Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.runButton) || ignoreKeyPressQualifier);
			bool runReleased = !Game1.isOneOfTheseKeysDown(kbState, Game1.options.runButton) && (Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.runButton) || ignoreKeyPressQualifier);
			if (Game1.options.gamepadControls)
			{
				if (!Game1.options.autoRun && Math.Abs(Vector2.Distance(Game1.input.GetGamePadState().ThumbSticks.Left, Vector2.Zero)) > 0.9f)
				{
					runPressed = true;
				}
				else if (Math.Abs(Vector2.Distance(Game1.oldPadState.ThumbSticks.Left, Vector2.Zero)) > 0.9f && Math.Abs(Vector2.Distance(Game1.input.GetGamePadState().ThumbSticks.Left, Vector2.Zero)) <= 0.9f)
				{
					runReleased = true;
				}
			}
			if (runPressed && !Game1.player.canOnlyWalk)
			{
				Game1.player.setRunning(!Game1.options.autoRun, false);
				Game1.player.setMoving(Game1.player.running ? 16 : 48);
			}
			else if (runReleased && !Game1.player.canOnlyWalk)
			{
				Game1.player.setRunning(Game1.options.autoRun, false);
				Game1.player.setMoving(Game1.player.running ? 16 : 48);
			}
			if (Game1.player.running != wasRunning && !Game1.player.UsingTool)
			{
				Game1.player.Halt();
			}
		}

		// Token: 0x06000C0F RID: 3087 RVA: 0x00082D75 File Offset: 0x00080F75
		public static Vector2 getMostRecentViewportMotion()
		{
			return new Vector2((float)Game1.viewport.X - Game1.previousViewportPosition.X, (float)Game1.viewport.Y - Game1.previousViewportPosition.Y);
		}

		// Token: 0x17000190 RID: 400
		// (get) Token: 0x06000C10 RID: 3088 RVA: 0x00082DA8 File Offset: 0x00080FA8
		// (set) Token: 0x06000C11 RID: 3089 RVA: 0x00082DB0 File Offset: 0x00080FB0
		public RenderTarget2D screen
		{
			get
			{
				return this._screen;
			}
			set
			{
				if (this._screen != null)
				{
					this._screen.Dispose();
					this._screen = null;
				}
				this._screen = value;
			}
		}

		// Token: 0x17000191 RID: 401
		// (get) Token: 0x06000C12 RID: 3090 RVA: 0x00082DD3 File Offset: 0x00080FD3
		// (set) Token: 0x06000C13 RID: 3091 RVA: 0x00082DDB File Offset: 0x00080FDB
		public RenderTarget2D uiScreen
		{
			get
			{
				return this._uiScreen;
			}
			set
			{
				if (this._uiScreen != null)
				{
					this._uiScreen.Dispose();
					this._uiScreen = null;
				}
				this._uiScreen = value;
			}
		}

		// Token: 0x06000C14 RID: 3092 RVA: 0x00082E00 File Offset: 0x00081000
		protected virtual void DrawOverlays(GameTime time, RenderTarget2D target_screen)
		{
			if (this.takingMapScreenshot)
			{
				return;
			}
			Game1.PushUIMode();
			Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			if (Game1.hooks.OnRendering(RenderSteps.Overlays, Game1.spriteBatch, time, target_screen))
			{
				SpecialCurrencyDisplay specialCurrencyDisplay = Game1.specialCurrencyDisplay;
				if (specialCurrencyDisplay != null)
				{
					specialCurrencyDisplay.Draw(Game1.spriteBatch);
				}
				EmoteMenu emoteMenu = Game1.emoteMenu;
				if (emoteMenu != null)
				{
					emoteMenu.draw(Game1.spriteBatch);
				}
				GameLocation currentLocation = Game1.currentLocation;
				if (currentLocation != null)
				{
					currentLocation.drawOverlays(Game1.spriteBatch);
				}
				if (Game1.HostPaused && !this.takingMapScreenshot)
				{
					string msg = Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10378");
					SpriteText.drawStringWithScrollBackground(Game1.spriteBatch, msg, 96, 32, "", 1f, null, SpriteText.ScrollTextAlignment.Left);
				}
				if (Game1.overlayMenu != null)
				{
					if (Game1.hooks.OnRendering(RenderSteps.Overlays_OverlayMenu, Game1.spriteBatch, time, target_screen))
					{
						Game1.overlayMenu.draw(Game1.spriteBatch);
					}
					Game1.hooks.OnRendered(RenderSteps.Overlays_OverlayMenu, Game1.spriteBatch, time, target_screen);
				}
				if (Game1.chatBox != null)
				{
					if (Game1.hooks.OnRendering(RenderSteps.Overlays_Chatbox, Game1.spriteBatch, time, target_screen))
					{
						Game1.chatBox.update(Game1.currentGameTime);
						Game1.chatBox.draw(Game1.spriteBatch);
					}
					Game1.hooks.OnRendered(RenderSteps.Overlays_Chatbox, Game1.spriteBatch, time, target_screen);
				}
				if (Game1.textEntry != null)
				{
					if (Game1.hooks.OnRendering(RenderSteps.Overlays_OnscreenKeyboard, Game1.spriteBatch, time, target_screen))
					{
						Game1.textEntry.draw(Game1.spriteBatch);
					}
					Game1.hooks.OnRendered(RenderSteps.Overlays_OnscreenKeyboard, Game1.spriteBatch, time, target_screen);
				}
				if ((Game1.displayHUD || Game1.eventUp || Game1.currentLocation is Summit) && Game1.gameMode == 3 && !Game1.freezeControls && !Game1.panMode)
				{
					this.drawMouseCursor();
				}
			}
			Game1.hooks.OnRendered(RenderSteps.Overlays, Game1.spriteBatch, time, target_screen);
			Game1.spriteBatch.End();
			Game1.PopUIMode();
		}

		// Token: 0x06000C15 RID: 3093 RVA: 0x00082FF6 File Offset: 0x000811F6
		public static void setBGColor(byte r, byte g, byte b)
		{
			Game1.bgColor.R = r;
			Game1.bgColor.G = g;
			Game1.bgColor.B = b;
		}

		// Token: 0x06000C16 RID: 3094 RVA: 0x00083019 File Offset: 0x00081219
		public void Instance_Draw(GameTime gameTime)
		{
			this.Draw(gameTime);
		}

		// Token: 0x06000C17 RID: 3095 RVA: 0x00083024 File Offset: 0x00081224
		protected override void Draw(GameTime gameTime)
		{
			this.isDrawing = true;
			RenderTarget2D target_screen = null;
			if (this.ShouldDrawOnBuffer())
			{
				target_screen = this.screen;
			}
			if (this.uiScreen != null)
			{
				Game1.SetRenderTarget(this.uiScreen);
				base.GraphicsDevice.Clear(Color.Transparent);
				Game1.SetRenderTarget(target_screen);
			}
			GameTime time = gameTime;
			DebugTools.BeforeGameDraw(this, ref time);
			this._draw(time, target_screen);
			Game1.isRenderingScreenBuffer = true;
			this.renderScreenBuffer(target_screen);
			Game1.isRenderingScreenBuffer = false;
			if (Game1.uiModeCount != 0)
			{
				Game1.log.Warn("WARNING: Mismatched UI Mode Push/Pop counts. Correcting.");
				while (Game1.uiModeCount < 0)
				{
					Game1.PushUIMode();
				}
				while (Game1.uiModeCount > 0)
				{
					Game1.PopUIMode();
				}
			}
			base.Draw(gameTime);
			this.isDrawing = false;
		}

		// Token: 0x06000C18 RID: 3096 RVA: 0x000830D9 File Offset: 0x000812D9
		public virtual bool ShouldDrawOnBuffer()
		{
			return LocalMultiplayer.IsLocalMultiplayer(false) || Game1.options.zoomLevel != 1f;
		}

		// Token: 0x06000C19 RID: 3097 RVA: 0x000830F9 File Offset: 0x000812F9
		public static bool ShouldShowOnscreenUsernames()
		{
			return false;
		}

		// Token: 0x06000C1A RID: 3098 RVA: 0x000830FC File Offset: 0x000812FC
		public virtual bool checkCharacterTilesForShadowDrawFlag(Character character)
		{
			Farmer farmer = character as Farmer;
			if (farmer != null && farmer.onBridge.Value)
			{
				return true;
			}
			Microsoft.Xna.Framework.Rectangle bounding_box = character.GetBoundingBox();
			bounding_box.Height += 8;
			int right = bounding_box.Right / 64;
			int bottom = bounding_box.Bottom / 64;
			int num = bounding_box.Left / 64;
			int top = bounding_box.Top / 64;
			for (int x = num; x <= right; x++)
			{
				for (int y = top; y <= bottom; y++)
				{
					if (Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(new Vector2((float)x, (float)y)))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06000C1B RID: 3099 RVA: 0x0008319C File Offset: 0x0008139C
		protected virtual void _draw(GameTime gameTime, RenderTarget2D target_screen)
		{
			Game1.debugTimings.StartDrawTimer();
			Game1.showingHealthBar = false;
			if (Game1._newDayTask != null || this.isLocalMultiplayerNewDayActive || this.ShouldLoadIncrementally)
			{
				base.GraphicsDevice.Clear(Game1.bgColor);
				return;
			}
			if (target_screen != null)
			{
				Game1.SetRenderTarget(target_screen);
			}
			if (this.IsSaving)
			{
				base.GraphicsDevice.Clear(Game1.bgColor);
				this.DrawMenu(gameTime, target_screen);
				Game1.PushUIMode();
				if (Game1.overlayMenu != null)
				{
					Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
					Game1.overlayMenu.draw(Game1.spriteBatch);
					Game1.spriteBatch.End();
				}
				Game1.PopUIMode();
				return;
			}
			base.GraphicsDevice.Clear(Game1.bgColor);
			if (Game1.hooks.OnRendering(RenderSteps.FullScene, Game1.spriteBatch, gameTime, target_screen))
			{
				if (Game1.gameMode == 11)
				{
					Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
					Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3685"), new Vector2(16f, 16f), Color.HotPink);
					Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3686"), new Vector2(16f, 32f), new Color(0, 255, 0));
					Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.parseText(Game1.errorMessage, Game1.dialogueFont, Game1.graphics.GraphicsDevice.Viewport.Width), new Vector2(16f, 48f), Color.White);
					Game1.spriteBatch.End();
					return;
				}
				bool draw_world = true;
				if (Game1.activeClickableMenu != null && Game1.options.showMenuBackground && Game1.activeClickableMenu.showWithoutTransparencyIfOptionIsSet() && !this.takingMapScreenshot)
				{
					Game1.PushUIMode();
					Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
					if (Game1.hooks.OnRendering(RenderSteps.MenuBackground, Game1.spriteBatch, gameTime, target_screen))
					{
						Game1.activeClickableMenu.drawBackground(Game1.spriteBatch);
						draw_world = false;
					}
					Game1.hooks.OnRendered(RenderSteps.MenuBackground, Game1.spriteBatch, gameTime, target_screen);
					Game1.spriteBatch.End();
					Game1.PopUIMode();
				}
				if (Game1.currentMinigame != null)
				{
					if (Game1.hooks.OnRendering(RenderSteps.Minigame, Game1.spriteBatch, gameTime, target_screen))
					{
						Game1.currentMinigame.draw(Game1.spriteBatch);
						draw_world = false;
					}
					Game1.hooks.OnRendered(RenderSteps.Minigame, Game1.spriteBatch, gameTime, target_screen);
				}
				if (Game1.gameMode == 6 || (Game1.gameMode == 3 && Game1.currentLocation == null))
				{
					if (Game1.hooks.OnRendering(RenderSteps.LoadingScreen, Game1.spriteBatch, gameTime, target_screen))
					{
						this.DrawLoadScreen(gameTime, target_screen);
					}
					Game1.hooks.OnRendered(RenderSteps.LoadingScreen, Game1.spriteBatch, gameTime, target_screen);
					draw_world = false;
				}
				if (Game1.showingEndOfNightStuff)
				{
					draw_world = false;
				}
				else if (Game1.gameMode == 0)
				{
					draw_world = false;
				}
				if (Game1.gameMode == 3 && Game1.dayOfMonth == 0 && Game1.newDay)
				{
					base.Draw(gameTime);
					return;
				}
				if (draw_world)
				{
					this.DrawWorld(gameTime, target_screen);
					Game1.PushUIMode();
					Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
					if (Game1.hooks.OnRendering(RenderSteps.HUD, Game1.spriteBatch, gameTime, target_screen))
					{
						if (Game1.IsHudDrawn)
						{
							this.drawHUD();
						}
						if (Game1.hudMessages.Count > 0 && !this.takingMapScreenshot)
						{
							int heightUsed = 0;
							for (int i = Game1.hudMessages.Count - 1; i >= 0; i--)
							{
								Game1.hudMessages[i].draw(Game1.spriteBatch, i, ref heightUsed);
							}
						}
					}
					Game1.hooks.OnRendered(RenderSteps.HUD, Game1.spriteBatch, gameTime, target_screen);
					Game1.debugTimings.Draw();
					Game1.spriteBatch.End();
					Game1.PopUIMode();
				}
				bool draw_dialogue_box_after_fade = false;
				if (!this.takingMapScreenshot)
				{
					Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
					Game1.PushUIMode();
					if ((Game1.messagePause || Game1.globalFade) && Game1.dialogueUp)
					{
						draw_dialogue_box_after_fade = true;
					}
					else if (Game1.dialogueUp && !Game1.messagePause && (Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is DialogueBox)))
					{
						if (Game1.hooks.OnRendering(RenderSteps.DialogueBox, Game1.spriteBatch, gameTime, target_screen))
						{
							this.drawDialogueBox();
						}
						Game1.hooks.OnRendered(RenderSteps.DialogueBox, Game1.spriteBatch, gameTime, target_screen);
					}
					Game1.spriteBatch.End();
					Game1.PopUIMode();
					this.DrawGlobalFade(gameTime, target_screen);
					if (draw_dialogue_box_after_fade)
					{
						Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
						Game1.PushUIMode();
						if (Game1.hooks.OnRendering(RenderSteps.DialogueBox, Game1.spriteBatch, gameTime, target_screen))
						{
							this.drawDialogueBox();
						}
						Game1.hooks.OnRendered(RenderSteps.DialogueBox, Game1.spriteBatch, gameTime, target_screen);
						Game1.spriteBatch.End();
						Game1.PopUIMode();
					}
					this.DrawScreenOverlaySprites(gameTime, target_screen);
					if (Game1.debugMode)
					{
						this.DrawDebugUIs(gameTime, target_screen);
					}
					this.DrawMenu(gameTime, target_screen);
				}
				FarmEvent farmEvent = Game1.farmEvent;
				if (farmEvent != null)
				{
					farmEvent.drawAboveEverything(Game1.spriteBatch);
				}
				this.DrawOverlays(gameTime, target_screen);
			}
			Game1.hooks.OnRendered(RenderSteps.FullScene, Game1.spriteBatch, gameTime, target_screen);
			Game1.debugTimings.StopDrawTimer();
		}

		// Token: 0x06000C1C RID: 3100 RVA: 0x00083700 File Offset: 0x00081900
		public virtual void DrawLoadScreen(GameTime time, RenderTarget2D target_screen)
		{
			Game1.PushUIMode();
			base.GraphicsDevice.Clear(Game1.bgColor);
			Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			string addOn = "".PadRight((int)Math.Ceiling(time.TotalGameTime.TotalMilliseconds % 999.0 / 333.0), '.');
			string str = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3688");
			string msg = str + addOn;
			string largestMessage = str + "... ";
			int msgw = SpriteText.getWidthOfString(largestMessage, 999999);
			int msgh = 64;
			int msgx = 64;
			int msgy = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - msgh;
			SpriteText.drawString(Game1.spriteBatch, msg, msgx, msgy, 999999, msgw, msgh, 1f, 0.88f, false, 0, largestMessage, null, SpriteText.ScrollTextAlignment.Left);
			Game1.spriteBatch.End();
			Game1.PopUIMode();
		}

		// Token: 0x06000C1D RID: 3101 RVA: 0x00083810 File Offset: 0x00081A10
		public virtual void DrawMenu(GameTime time, RenderTarget2D target_screen)
		{
			Game1.PushUIMode();
			Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			if (Game1.hooks.OnRendering(RenderSteps.Menu, Game1.spriteBatch, time, target_screen))
			{
				IClickableMenu menu = Game1.activeClickableMenu;
				while (menu != null && Game1.hooks.TryDrawMenu(menu, delegate
				{
					menu.draw(Game1.spriteBatch);
				}))
				{
					menu = menu.GetChildMenu();
				}
			}
			Game1.hooks.OnRendered(RenderSteps.Menu, Game1.spriteBatch, time, target_screen);
			Game1.spriteBatch.End();
			Game1.PopUIMode();
		}

		// Token: 0x06000C1E RID: 3102 RVA: 0x000838C4 File Offset: 0x00081AC4
		public virtual void DrawScreenOverlaySprites(GameTime time, RenderTarget2D target_screen)
		{
			if (Game1.hooks.OnRendering(RenderSteps.OverlayTemporarySprites, Game1.spriteBatch, time, target_screen))
			{
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
				foreach (TemporaryAnimatedSprite temporaryAnimatedSprite in Game1.screenOverlayTempSprites)
				{
					temporaryAnimatedSprite.draw(Game1.spriteBatch, true, 0, 0, 1f);
				}
				Game1.spriteBatch.End();
				Game1.PushUIMode();
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
				foreach (TemporaryAnimatedSprite temporaryAnimatedSprite2 in Game1.uiOverlayTempSprites)
				{
					temporaryAnimatedSprite2.draw(Game1.spriteBatch, true, 0, 0, 1f);
				}
				Game1.spriteBatch.End();
				Game1.PopUIMode();
			}
			Game1.hooks.OnRendered(RenderSteps.OverlayTemporarySprites, Game1.spriteBatch, time, target_screen);
		}

		// Token: 0x06000C1F RID: 3103 RVA: 0x000839EC File Offset: 0x00081BEC
		public virtual void DrawWorld(GameTime time, RenderTarget2D target_screen)
		{
			if (Game1.hooks.OnRendering(RenderSteps.World, Game1.spriteBatch, time, target_screen))
			{
				Game1.mapDisplayDevice.BeginScene(Game1.spriteBatch);
				if (Game1.drawLighting)
				{
					this.DrawLighting(time, target_screen);
				}
				base.GraphicsDevice.Clear(Game1.bgColor);
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
				if (Game1.hooks.OnRendering(RenderSteps.World_Background, Game1.spriteBatch, time, target_screen))
				{
					Background background = Game1.background;
					if (background != null)
					{
						background.draw(Game1.spriteBatch);
					}
					Game1.currentLocation.drawBackground(Game1.spriteBatch);
					Game1.spriteBatch.End();
					for (int i = 0; i < Game1.currentLocation.backgroundLayers.Count; i++)
					{
						Game1.spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
						Game1.currentLocation.backgroundLayers[i].Key.Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, false, 4, -1f);
						Game1.spriteBatch.End();
					}
					Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
					Game1.currentLocation.drawWater(Game1.spriteBatch);
					Game1.spriteBatch.End();
					Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
					Game1.currentLocation.drawFloorDecorations(Game1.spriteBatch);
					Game1.spriteBatch.End();
				}
				Game1.hooks.OnRendered(RenderSteps.World_Background, Game1.spriteBatch, time, target_screen);
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
				this._farmerShadows.Clear();
				if (Game1.currentLocation.currentEvent != null && !Game1.currentLocation.currentEvent.isFestival && Game1.currentLocation.currentEvent.farmerActors.Count > 0)
				{
					using (List<Farmer>.Enumerator enumerator = Game1.currentLocation.currentEvent.farmerActors.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Farmer f = enumerator.Current;
							if ((f.IsLocalPlayer && Game1.displayFarmer) || !f.hidden.Value)
							{
								this._farmerShadows.Add(f);
							}
						}
						goto IL_2D7;
					}
				}
				foreach (Farmer f2 in Game1.currentLocation.farmers)
				{
					if ((f2.IsLocalPlayer && Game1.displayFarmer) || !f2.hidden.Value)
					{
						this._farmerShadows.Add(f2);
					}
				}
				IL_2D7:
				if (!Game1.currentLocation.shouldHideCharacters())
				{
					if (Game1.CurrentEvent == null)
					{
						using (List<NPC>.Enumerator enumerator3 = Game1.currentLocation.characters.GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								NPC j = enumerator3.Current;
								if (!j.swimming.Value && !j.HideShadow && !j.IsInvisible && !this.checkCharacterTilesForShadowDrawFlag(j))
								{
									j.DrawShadow(Game1.spriteBatch);
								}
							}
							goto IL_3D2;
						}
					}
					foreach (NPC k in Game1.CurrentEvent.actors)
					{
						if ((Game1.CurrentEvent == null || !Game1.CurrentEvent.ShouldHideCharacter(k)) && !k.swimming.Value && !k.HideShadow && !this.checkCharacterTilesForShadowDrawFlag(k))
						{
							k.DrawShadow(Game1.spriteBatch);
						}
					}
					IL_3D2:
					foreach (Farmer f3 in this._farmerShadows)
					{
						if (!Game1.multiplayer.isDisconnecting(f3.UniqueMultiplayerID) && !f3.swimming.Value && !f3.isRidingHorse() && !f3.IsSitting() && (Game1.currentLocation == null || !this.checkCharacterTilesForShadowDrawFlag(f3)))
						{
							f3.DrawShadow(Game1.spriteBatch);
						}
					}
				}
				float layer_sub_sort = 0.1f;
				for (int l = 0; l < Game1.currentLocation.buildingLayers.Count; l++)
				{
					float layer = 0f;
					if (Game1.currentLocation.buildingLayers.Count > 1)
					{
						layer = (float)l / (float)(Game1.currentLocation.buildingLayers.Count - 1);
					}
					Game1.currentLocation.buildingLayers[l].Key.Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, false, 4, layer_sub_sort * layer);
				}
				Layer building_layer = Game1.currentLocation.Map.RequireLayer("Buildings");
				Game1.spriteBatch.End();
				Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
				if (Game1.hooks.OnRendering(RenderSteps.World_Sorted, Game1.spriteBatch, time, target_screen))
				{
					if (!Game1.currentLocation.shouldHideCharacters())
					{
						if (Game1.CurrentEvent == null)
						{
							using (List<NPC>.Enumerator enumerator3 = Game1.currentLocation.characters.GetEnumerator())
							{
								while (enumerator3.MoveNext())
								{
									NPC m = enumerator3.Current;
									if (!m.swimming.Value && !m.HideShadow && !m.isInvisible.Value && this.checkCharacterTilesForShadowDrawFlag(m))
									{
										m.DrawShadow(Game1.spriteBatch);
									}
								}
								goto IL_633;
							}
						}
						foreach (NPC n in Game1.CurrentEvent.actors)
						{
							if ((Game1.CurrentEvent == null || !Game1.CurrentEvent.ShouldHideCharacter(n)) && !n.swimming.Value && !n.HideShadow && this.checkCharacterTilesForShadowDrawFlag(n))
							{
								n.DrawShadow(Game1.spriteBatch);
							}
						}
						IL_633:
						foreach (Farmer f4 in this._farmerShadows)
						{
							if (!f4.swimming.Value && !f4.isRidingHorse() && !f4.IsSitting() && Game1.currentLocation != null && this.checkCharacterTilesForShadowDrawFlag(f4))
							{
								f4.DrawShadow(Game1.spriteBatch);
							}
						}
					}
					if ((Game1.eventUp || Game1.killScreen) && !Game1.killScreen && Game1.currentLocation.currentEvent != null)
					{
						Game1.currentLocation.currentEvent.draw(Game1.spriteBatch);
					}
					Game1.currentLocation.draw(Game1.spriteBatch);
					foreach (Vector2 tile_position in Game1.crabPotOverlayTiles.Keys)
					{
						Tile tile = building_layer.Tiles[(int)tile_position.X, (int)tile_position.Y];
						if (tile != null)
						{
							Vector2 vector_draw_position = Game1.GlobalToLocal(Game1.viewport, tile_position * 64f);
							Location draw_location = new Location((int)vector_draw_position.X, (int)vector_draw_position.Y);
							Game1.mapDisplayDevice.DrawTile(tile, draw_location, (tile_position.Y * 64f - 1f) / 10000f);
						}
					}
					if (Game1.player.ActiveObject == null && Game1.player.UsingTool && Game1.player.CurrentTool != null)
					{
						Game1.drawTool(Game1.player);
					}
					if (Game1.panMode)
					{
						Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle((int)Math.Floor((double)(Game1.getOldMouseX() + Game1.viewport.X) / 64.0) * 64 - Game1.viewport.X, (int)Math.Floor((double)(Game1.getOldMouseY() + Game1.viewport.Y) / 64.0) * 64 - Game1.viewport.Y, 64, 64), Color.Lime * 0.75f);
						foreach (Warp w in Game1.currentLocation.warps)
						{
							Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(w.X * 64 - Game1.viewport.X, w.Y * 64 - Game1.viewport.Y, 64, 64), Color.Red * 0.75f);
						}
					}
					for (int i2 = 0; i2 < Game1.currentLocation.frontLayers.Count; i2++)
					{
						float layer2 = 0f;
						if (Game1.currentLocation.frontLayers.Count > 1)
						{
							layer2 = (float)i2 / (float)(Game1.currentLocation.frontLayers.Count - 1);
						}
						Game1.currentLocation.frontLayers[i2].Key.Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, false, 4, 64f + layer_sub_sort * layer2);
					}
					Game1.currentLocation.drawAboveFrontLayer(Game1.spriteBatch);
				}
				Game1.hooks.OnRendered(RenderSteps.World_Sorted, Game1.spriteBatch, time, target_screen);
				Game1.spriteBatch.End();
				if (Game1.hooks.OnRendering(RenderSteps.World_AlwaysFront, Game1.spriteBatch, time, target_screen))
				{
					for (int i3 = 0; i3 < Game1.currentLocation.alwaysFrontLayers.Count; i3++)
					{
						Game1.spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
						Game1.currentLocation.alwaysFrontLayers[i3].Key.Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, false, 4, -1f);
						Game1.spriteBatch.End();
					}
				}
				Game1.hooks.OnRendered(RenderSteps.World_AlwaysFront, Game1.spriteBatch, time, target_screen);
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
				if (Game1.currentLocation.LightLevel > 0f && Game1.timeOfDay < 2000)
				{
					Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * Game1.currentLocation.LightLevel);
				}
				if (Game1.screenGlow)
				{
					Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Game1.screenGlowColor * Game1.screenGlowAlpha);
				}
				Game1.spriteBatch.End();
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
				Game1.currentLocation.drawAboveAlwaysFrontLayer(Game1.spriteBatch);
				if (!Game1.IsFakedBlackScreen())
				{
					Game1.spriteBatch.End();
					this.drawWeather(time, target_screen);
					Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
				}
				FishingRod rod = Game1.player.CurrentTool as FishingRod;
				if (rod != null && (rod.isTimingCast || rod.castingChosenCountdown > 0f || rod.fishCaught || rod.showingTreasure))
				{
					Game1.player.CurrentTool.draw(Game1.spriteBatch);
				}
				Game1.spriteBatch.End();
				this.DrawCharacterEmotes(time, target_screen);
				Game1.mapDisplayDevice.EndScene();
				if (Game1.drawLighting && !Game1.IsFakedBlackScreen())
				{
					this.DrawLightmapOnScreen(time, target_screen);
				}
				if (!Game1.eventUp && Game1.farmEvent == null && Game1.gameMode == 3 && !this.takingMapScreenshot && Game1.isOutdoorMapSmallerThanViewport())
				{
					Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
					Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, 0, -Game1.viewport.X, Game1.graphics.GraphicsDevice.Viewport.Height), Color.Black);
					Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(-Game1.viewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64, 0, Game1.graphics.GraphicsDevice.Viewport.Width - (-Game1.viewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64), Game1.graphics.GraphicsDevice.Viewport.Height), Color.Black);
					Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, -Game1.viewport.Y), Color.Black);
					Game1.spriteBatch.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, -Game1.viewport.Y + Game1.currentLocation.map.Layers[0].LayerHeight * 64, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height - (-Game1.viewport.Y + Game1.currentLocation.map.Layers[0].LayerHeight * 64)), Color.Black);
					Game1.spriteBatch.End();
				}
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
				if (Game1.currentLocation != null && Game1.currentLocation.isOutdoors.Value && !Game1.IsFakedBlackScreen() && Game1.currentLocation.IsRainingHere())
				{
					bool isGreenRain = Game1.IsGreenRainingHere(null);
					Game1.spriteBatch.Draw(Game1.staminaRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, isGreenRain ? (new Color(0, 120, 150) * 0.22f) : (Color.Blue * 0.2f));
				}
				Game1.spriteBatch.End();
				if (Game1.farmEvent != null)
				{
					Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
					Game1.farmEvent.draw(Game1.spriteBatch);
					Game1.spriteBatch.End();
				}
				if (Game1.eventUp)
				{
					GameLocation currentLocation = Game1.currentLocation;
					if (((currentLocation != null) ? currentLocation.currentEvent : null) != null)
					{
						Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
						Game1.currentLocation.currentEvent.drawAfterMap(Game1.spriteBatch);
						Game1.spriteBatch.End();
					}
				}
				if (!this.takingMapScreenshot)
				{
					if (Game1.drawGrid)
					{
						Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
						int startingX = -Game1.viewport.X % 64;
						float startingY = (float)(-(float)Game1.viewport.Y % 64);
						for (int x = startingX; x < Game1.graphics.GraphicsDevice.Viewport.Width; x += 64)
						{
							Game1.spriteBatch.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle(x, (int)startingY, 1, Game1.graphics.GraphicsDevice.Viewport.Height), Color.Red * 0.5f);
						}
						for (float y = startingY; y < (float)Game1.graphics.GraphicsDevice.Viewport.Height; y += 64f)
						{
							Game1.spriteBatch.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle(startingX, (int)y, Game1.graphics.GraphicsDevice.Viewport.Width, 1), Color.Red * 0.5f);
						}
						Game1.spriteBatch.End();
					}
					if (Game1.ShouldShowOnscreenUsernames() && Game1.currentLocation != null)
					{
						Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
						Game1.currentLocation.DrawFarmerUsernames(Game1.spriteBatch);
						Game1.spriteBatch.End();
					}
					if (Game1.flashAlpha > 0f)
					{
						if (Game1.options.screenFlash)
						{
							Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
							Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.White * Math.Min(1f, Game1.flashAlpha));
							Game1.spriteBatch.End();
						}
						Game1.flashAlpha -= 0.1f;
					}
				}
			}
			Game1.hooks.OnRendered(RenderSteps.World, Game1.spriteBatch, time, target_screen);
		}

		// Token: 0x06000C20 RID: 3104 RVA: 0x00084BDC File Offset: 0x00082DDC
		public virtual void DrawCharacterEmotes(GameTime time, RenderTarget2D target_screen)
		{
			if (Game1.eventUp && Game1.currentLocation.currentEvent != null)
			{
				Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
				foreach (NPC npc in Game1.currentLocation.currentEvent.actors)
				{
					npc.DrawEmote(Game1.spriteBatch);
				}
				Game1.spriteBatch.End();
			}
		}

		// Token: 0x06000C21 RID: 3105 RVA: 0x00084C78 File Offset: 0x00082E78
		public virtual void DrawLightmapOnScreen(GameTime time, RenderTarget2D target_screen)
		{
			if (Game1.hooks.OnRendering(RenderSteps.World_DrawLightmapOnScreen, Game1.spriteBatch, time, target_screen))
			{
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, this.lightingBlend, SamplerState.LinearClamp, null, null, null, null);
				Viewport vp = base.GraphicsDevice.Viewport;
				vp.Bounds = ((target_screen != null) ? target_screen.Bounds : base.GraphicsDevice.PresentationParameters.Bounds);
				base.GraphicsDevice.Viewport = vp;
				float render_zoom = (float)(Game1.options.lightingQuality / 2);
				if (this.useUnscaledLighting)
				{
					render_zoom /= Game1.options.zoomLevel;
				}
				Game1.spriteBatch.Draw(Game1.lightmap, Vector2.Zero, new Microsoft.Xna.Framework.Rectangle?(Game1.lightmap.Bounds), Color.White, 0f, Vector2.Zero, render_zoom, SpriteEffects.None, 1f);
				if (Game1.currentLocation.isOutdoors.Value && Game1.currentLocation.IsRainingHere())
				{
					Game1.spriteBatch.Draw(Game1.lightingRect, vp.Bounds, Color.OrangeRed * 0.45f);
				}
			}
			Game1.hooks.OnRendered(RenderSteps.World_DrawLightmapOnScreen, Game1.spriteBatch, time, target_screen);
			Game1.spriteBatch.End();
		}

		// Token: 0x06000C22 RID: 3106 RVA: 0x00084DB4 File Offset: 0x00082FB4
		public virtual void DrawDebugUIs(GameTime time, RenderTarget2D target_screen)
		{
			StringBuilder sb = Game1._debugStringBuilder;
			sb.Clear();
			if (Game1.panMode)
			{
				sb.Append((Game1.getOldMouseX() + Game1.viewport.X) / 64);
				sb.Append(",");
				sb.Append((Game1.getOldMouseY() + Game1.viewport.Y) / 64);
			}
			else
			{
				Point playerPixel = Game1.player.StandingPixel;
				sb.Append("player: ");
				sb.Append(playerPixel.X / 64);
				sb.Append(", ");
				sb.Append(playerPixel.Y / 64);
			}
			sb.Append(" mouseTransparency: ");
			sb.Append(Game1.mouseCursorTransparency);
			sb.Append(" mousePosition: ");
			sb.Append(Game1.getMouseX());
			sb.Append(",");
			sb.Append(Game1.getMouseY());
			sb.Append(Environment.NewLine);
			sb.Append(" mouseWorldPosition: ");
			sb.Append(Game1.getMouseX() + Game1.viewport.X);
			sb.Append(",");
			sb.Append(Game1.getMouseY() + Game1.viewport.Y);
			sb.Append("  debugOutput: ");
			sb.Append(Game1.debugOutput);
			Game1.PushUIMode();
			Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			Game1.spriteBatch.DrawString(Game1.smallFont, sb, new Vector2((float)base.GraphicsDevice.Viewport.GetTitleSafeArea().X, (float)(base.GraphicsDevice.Viewport.GetTitleSafeArea().Y + Game1.smallFont.LineSpacing * 8)), Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
			Game1.spriteBatch.End();
			Game1.PopUIMode();
		}

		// Token: 0x06000C23 RID: 3107 RVA: 0x00084FAC File Offset: 0x000831AC
		public virtual void DrawGlobalFade(GameTime time, RenderTarget2D target_screen)
		{
			if ((Game1.fadeToBlack || Game1.globalFade) && !this.takingMapScreenshot)
			{
				Game1.PushUIMode();
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
				if (Game1.hooks.OnRendering(RenderSteps.GlobalFade, Game1.spriteBatch, time, target_screen))
				{
					Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * ((Game1.gameMode == 0) ? (1f - Game1.fadeToBlackAlpha) : Game1.fadeToBlackAlpha));
				}
				Game1.hooks.OnRendered(RenderSteps.GlobalFade, Game1.spriteBatch, time, target_screen);
				Game1.spriteBatch.End();
				Game1.PopUIMode();
			}
		}

		// Token: 0x06000C24 RID: 3108 RVA: 0x0008507C File Offset: 0x0008327C
		public virtual void DrawLighting(GameTime time, RenderTarget2D target_screen)
		{
			Game1.SetRenderTarget(Game1.lightmap);
			base.GraphicsDevice.Clear(Color.White * 0f);
			Matrix lighting_matrix = Matrix.Identity;
			if (this.useUnscaledLighting)
			{
				lighting_matrix = Matrix.CreateScale(Game1.options.zoomLevel);
			}
			Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, new Matrix?(lighting_matrix));
			if (Game1.hooks.OnRendering(RenderSteps.World_RenderLightmap, Game1.spriteBatch, time, target_screen))
			{
				MineShaft mine = Game1.currentLocation as MineShaft;
				Color lighting;
				if (mine != null)
				{
					lighting = mine.getLightingColor(time);
				}
				else if (!Game1.ambientLight.Equals(Color.White) && (!Game1.currentLocation.isOutdoors.Value || !Game1.currentLocation.IsRainingHere()))
				{
					lighting = Game1.ambientLight;
				}
				else
				{
					lighting = Game1.outdoorLight;
				}
				float light_multiplier = 1f;
				if (Game1.player.hasBuff("26"))
				{
					if (lighting == Color.White)
					{
						lighting = new Color(0.75f, 0.75f, 0.75f);
					}
					else
					{
						lighting.R = (byte)Utility.Lerp((float)lighting.R, 255f, 0.5f);
						lighting.G = (byte)Utility.Lerp((float)lighting.G, 255f, 0.5f);
						lighting.B = (byte)Utility.Lerp((float)lighting.B, 255f, 0.5f);
					}
					light_multiplier = 0.33f;
				}
				if (Game1.IsGreenRainingHere(null))
				{
					lighting.R = (byte)Utility.Lerp((float)lighting.R, 255f, 0.25f);
					lighting.G = (byte)Utility.Lerp((float)lighting.R, 0f, 0.25f);
				}
				Game1.spriteBatch.Draw(Game1.staminaRect, Game1.lightmap.Bounds, lighting);
				foreach (KeyValuePair<string, LightSource> pair in Game1.currentLightSources)
				{
					pair.Value.Draw(Game1.spriteBatch, Game1.currentLocation, light_multiplier);
				}
			}
			Game1.hooks.OnRendered(RenderSteps.World_RenderLightmap, Game1.spriteBatch, time, target_screen);
			Game1.spriteBatch.End();
			Game1.SetRenderTarget(target_screen);
		}

		// Token: 0x06000C25 RID: 3109 RVA: 0x000852D0 File Offset: 0x000834D0
		public virtual void drawWeather(GameTime time, RenderTarget2D target_screen)
		{
			Game1.spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			if (Game1.hooks.OnRendering(RenderSteps.World_Weather, Game1.spriteBatch, time, target_screen) && Game1.currentLocation.IsOutdoors)
			{
				if (Game1.currentLocation.IsSnowingHere())
				{
					Game1.snowPos.X = Game1.snowPos.X % 64f;
					Vector2 v = default(Vector2);
					for (float x = -64f + Game1.snowPos.X % 64f; x < (float)Game1.viewport.Width; x += 64f)
					{
						for (float y = -64f + Game1.snowPos.Y % 64f; y < (float)Game1.viewport.Height; y += 64f)
						{
							v.X = (float)((int)x);
							v.Y = (float)((int)y);
							Game1.spriteBatch.Draw(Game1.mouseCursors, v, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(368 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1200.0) / 75 * 16, 192, 16, 16)), Color.White * 0.8f * Game1.options.snowTransparency, 0f, Vector2.Zero, 4.001f, SpriteEffects.None, 1f);
						}
					}
				}
				if (!Game1.currentLocation.ignoreDebrisWeather.Value && Game1.currentLocation.IsDebrisWeatherHere())
				{
					if (this.takingMapScreenshot)
					{
						if (Game1.debrisWeather == null)
						{
							goto IL_2A4;
						}
						using (List<WeatherDebris>.Enumerator enumerator = Game1.debrisWeather.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								WeatherDebris w = enumerator.Current;
								Vector2 position = w.position;
								w.position = new Vector2((float)Game1.random.Next(Game1.viewport.Width - w.sourceRect.Width * 3), (float)Game1.random.Next(Game1.viewport.Height - w.sourceRect.Height * 3));
								w.draw(Game1.spriteBatch);
								w.position = position;
							}
							goto IL_2A4;
						}
					}
					if (Game1.viewport.X > -Game1.viewport.Width)
					{
						foreach (WeatherDebris weatherDebris in Game1.debrisWeather)
						{
							weatherDebris.draw(Game1.spriteBatch);
						}
					}
				}
				IL_2A4:
				if (Game1.currentLocation.IsRainingHere() && !(Game1.currentLocation is Summit) && (!Game1.eventUp || Game1.currentLocation.isTileOnMap(new Vector2((float)(Game1.viewport.X / 64), (float)(Game1.viewport.Y / 64)))))
				{
					bool isGreenRain = Game1.IsGreenRainingHere(null);
					Color rainColor = isGreenRain ? Color.LimeGreen : Color.White;
					int vibrancy = isGreenRain ? 2 : 1;
					for (int i = 0; i < Game1.rainDrops.Length; i++)
					{
						for (int v2 = 0; v2 < vibrancy; v2++)
						{
							Game1.spriteBatch.Draw(Game1.rainTexture, Game1.rainDrops[i].position, new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.rainTexture, Game1.rainDrops[i].frame + (isGreenRain ? 4 : 0), 16, 16)), rainColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
						}
					}
				}
			}
			Game1.hooks.OnRendered(RenderSteps.World_Weather, Game1.spriteBatch, time, target_screen);
			Game1.spriteBatch.End();
		}

		// Token: 0x06000C26 RID: 3110 RVA: 0x000856C0 File Offset: 0x000838C0
		protected virtual void renderScreenBuffer(RenderTarget2D target_screen)
		{
			Game1.graphics.GraphicsDevice.SetRenderTarget(null);
			if (!this.takingMapScreenshot)
			{
				if (LocalMultiplayer.IsLocalMultiplayer(false))
				{
					return;
				}
				if (target_screen != null && target_screen.IsContentLost)
				{
					return;
				}
				if (this.ShouldDrawOnBuffer() && target_screen != null)
				{
					base.GraphicsDevice.Clear(Game1.bgColor);
					Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, null);
					Game1.spriteBatch.Draw(target_screen, new Vector2(0f, 0f), new Microsoft.Xna.Framework.Rectangle?(target_screen.Bounds), Color.White, 0f, Vector2.Zero, Game1.options.zoomLevel, SpriteEffects.None, 1f);
					Game1.spriteBatch.End();
					Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, null);
					Game1.spriteBatch.Draw(this.uiScreen, new Vector2(0f, 0f), new Microsoft.Xna.Framework.Rectangle?(this.uiScreen.Bounds), Color.White, 0f, Vector2.Zero, Game1.options.uiScale, SpriteEffects.None, 1f);
					Game1.spriteBatch.End();
					return;
				}
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, null);
				Game1.spriteBatch.Draw(this.uiScreen, new Vector2(0f, 0f), new Microsoft.Xna.Framework.Rectangle?(this.uiScreen.Bounds), Color.White, 0f, Vector2.Zero, Game1.options.uiScale, SpriteEffects.None, 1f);
				Game1.spriteBatch.End();
			}
		}

		// Token: 0x06000C27 RID: 3111 RVA: 0x00085898 File Offset: 0x00083A98
		public virtual void DrawSplitScreenWindow()
		{
			if (!LocalMultiplayer.IsLocalMultiplayer(false))
			{
				return;
			}
			Game1.graphics.GraphicsDevice.SetRenderTarget(null);
			if (this.screen != null && this.screen.IsContentLost)
			{
				return;
			}
			Viewport old_viewport = base.GraphicsDevice.Viewport;
			base.GraphicsDevice.Viewport = (base.GraphicsDevice.Viewport = Game1.defaultDeviceViewport);
			Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, null);
			Game1.spriteBatch.Draw(this.screen, new Vector2((float)this.localMultiplayerWindow.X, (float)this.localMultiplayerWindow.Y), new Microsoft.Xna.Framework.Rectangle?(this.screen.Bounds), Color.White, 0f, Vector2.Zero, this.instanceOptions.zoomLevel, SpriteEffects.None, 1f);
			if (this.uiScreen != null)
			{
				Game1.spriteBatch.Draw(this.uiScreen, new Vector2((float)this.localMultiplayerWindow.X, (float)this.localMultiplayerWindow.Y), new Microsoft.Xna.Framework.Rectangle?(this.uiScreen.Bounds), Color.White, 0f, Vector2.Zero, this.instanceOptions.uiScale, SpriteEffects.None, 1f);
			}
			Game1.spriteBatch.End();
			base.GraphicsDevice.Viewport = old_viewport;
		}

		// Token: 0x06000C28 RID: 3112 RVA: 0x000859FF File Offset: 0x00083BFF
		public static void drawWithBorder(string message, Color borderColor, Color insideColor, Vector2 position)
		{
			Game1.drawWithBorder(message, borderColor, insideColor, position, 0f, 1f, 1f, false);
		}

		// Token: 0x06000C29 RID: 3113 RVA: 0x00085A1A File Offset: 0x00083C1A
		public static void drawWithBorder(string message, Color borderColor, Color insideColor, Vector2 position, float rotate, float scale, float layerDepth)
		{
			Game1.drawWithBorder(message, borderColor, insideColor, position, rotate, scale, layerDepth, false);
		}

		// Token: 0x06000C2A RID: 3114 RVA: 0x00085A2C File Offset: 0x00083C2C
		public static void drawWithBorder(string message, Color borderColor, Color insideColor, Vector2 position, float rotate, float scale, float layerDepth, bool tiny)
		{
			string[] words = ArgUtility.SplitBySpace(message);
			int offset = 0;
			for (int i = 0; i < words.Length; i++)
			{
				if (words[i].Contains('='))
				{
					Game1.spriteBatch.DrawString(tiny ? Game1.tinyFont : Game1.dialogueFont, words[i], new Vector2(position.X + (float)offset, position.Y), Color.Purple, rotate, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
					offset += (int)((tiny ? Game1.tinyFont : Game1.dialogueFont).MeasureString(words[i]).X + 8f);
				}
				else
				{
					Game1.spriteBatch.DrawString(tiny ? Game1.tinyFont : Game1.dialogueFont, words[i], new Vector2(position.X + (float)offset, position.Y), insideColor, rotate, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
					offset += (int)((tiny ? Game1.tinyFont : Game1.dialogueFont).MeasureString(words[i]).X + 8f);
				}
			}
		}

		// Token: 0x06000C2B RID: 3115 RVA: 0x00085B34 File Offset: 0x00083D34
		public static bool isOutdoorMapSmallerThanViewport()
		{
			return !Game1.uiMode && (Game1.currentLocation != null && Game1.currentLocation.IsOutdoors && !(Game1.currentLocation is Summit)) && (Game1.currentLocation.map.Layers[0].LayerWidth * 64 < Game1.viewport.Width || Game1.currentLocation.map.Layers[0].LayerHeight * 64 < Game1.viewport.Height);
		}

		// Token: 0x06000C2C RID: 3116 RVA: 0x00085BC0 File Offset: 0x00083DC0
		protected virtual void drawHUD()
		{
			if (!Game1.eventUp && Game1.farmEvent == null)
			{
				float modifier = 0.625f;
				Vector2 topOfBar = new Vector2((float)(Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Right - 48 - 8), (float)(Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 224 - 16 - (int)((float)(Game1.player.MaxStamina - 270) * modifier)));
				if (Game1.isOutdoorMapSmallerThanViewport())
				{
					topOfBar.X = Math.Min(topOfBar.X, (float)(-(float)Game1.viewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64 - 48));
				}
				if (Game1.staminaShakeTimer > 0)
				{
					topOfBar.X += (float)Game1.random.Next(-3, 4);
					topOfBar.Y += (float)Game1.random.Next(-3, 4);
				}
				Game1.spriteBatch.Draw(Game1.mouseCursors, topOfBar, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(256, 408, 12, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				Game1.spriteBatch.Draw(Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle((int)topOfBar.X, (int)(topOfBar.Y + 64f), 48, Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 64 - 16 - (int)(topOfBar.Y + 64f - 8f)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(256, 424, 12, 16)), Color.White);
				Game1.spriteBatch.Draw(Game1.mouseCursors, new Vector2(topOfBar.X, topOfBar.Y + 224f + (float)((int)((float)(Game1.player.MaxStamina - 270) * modifier)) - 64f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(256, 448, 12, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				Microsoft.Xna.Framework.Rectangle r = new Microsoft.Xna.Framework.Rectangle((int)topOfBar.X + 12, (int)topOfBar.Y + 16 + 32 + (int)((float)Game1.player.MaxStamina * modifier) - (int)(Math.Max(0f, Game1.player.Stamina) * modifier), 24, (int)(Game1.player.Stamina * modifier) - 1);
				if ((float)Game1.getOldMouseX() >= topOfBar.X && (float)Game1.getOldMouseY() >= topOfBar.Y)
				{
					Game1.drawWithBorder(((int)Math.Max(0f, Game1.player.Stamina)).ToString() + "/" + Game1.player.MaxStamina.ToString(), Color.Black * 0f, Color.White, topOfBar + new Vector2(-Game1.dialogueFont.MeasureString("999/999").X - 16f - (float)(Game1.showingHealth ? 64 : 0), 64f));
				}
				Color c = Utility.getRedToGreenLerpColor(Game1.player.stamina / (float)Game1.player.maxStamina.Value);
				Game1.spriteBatch.Draw(Game1.staminaRect, r, c);
				r.Height = 4;
				c.R = (byte)Math.Max(0, (int)(c.R - 50));
				c.G = (byte)Math.Max(0, (int)(c.G - 50));
				Game1.spriteBatch.Draw(Game1.staminaRect, r, c);
				if (Game1.player.exhausted.Value)
				{
					Game1.spriteBatch.Draw(Game1.mouseCursors, topOfBar - new Vector2(0f, 11f) * 4f, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(191, 406, 12, 11)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					if ((float)Game1.getOldMouseX() >= topOfBar.X && (float)Game1.getOldMouseY() >= topOfBar.Y - 44f)
					{
						Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3747"), Color.Black * 0f, Color.White, topOfBar + new Vector2(-Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3747")).X - 16f - (float)(Game1.showingHealth ? 64 : 0), 96f));
					}
				}
				if (Game1.currentLocation is MineShaft || Game1.currentLocation is Woods || Game1.currentLocation is SlimeHutch || Game1.currentLocation is VolcanoDungeon || Game1.player.health < Game1.player.maxHealth)
				{
					Game1.showingHealthBar = true;
					Game1.showingHealth = true;
					int bar_full_height = 168 + (Game1.player.maxHealth - 100);
					int height = (int)((float)Game1.player.health / (float)Game1.player.maxHealth * (float)bar_full_height);
					topOfBar.X -= (float)(56 + ((Game1.hitShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0));
					topOfBar.Y = (float)(Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 224 - 16 - (Game1.player.maxHealth - 100));
					Game1.spriteBatch.Draw(Game1.mouseCursors, topOfBar, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(268, 408, 12, 16)), (Game1.player.health < 20) ? (Color.Pink * ((float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / (double)((float)Game1.player.health * 50f)) / 4f + 0.9f)) : Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					Game1.spriteBatch.Draw(Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle((int)topOfBar.X, (int)(topOfBar.Y + 64f), 48, Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - 64 - 16 - (int)(topOfBar.Y + 64f)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(268, 424, 12, 16)), (Game1.player.health < 20) ? (Color.Pink * ((float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / (double)((float)Game1.player.health * 50f)) / 4f + 0.9f)) : Color.White);
					Game1.spriteBatch.Draw(Game1.mouseCursors, new Vector2(topOfBar.X, topOfBar.Y + 224f + (float)(Game1.player.maxHealth - 100) - 64f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(268, 448, 12, 16)), (Game1.player.health < 20) ? (Color.Pink * ((float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / (double)((float)Game1.player.health * 50f)) / 4f + 0.9f)) : Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					Microsoft.Xna.Framework.Rectangle health_bar_rect = new Microsoft.Xna.Framework.Rectangle((int)topOfBar.X + 12, (int)topOfBar.Y + 16 + 32 + bar_full_height - height, 24, height);
					c = Utility.getRedToGreenLerpColor((float)Game1.player.health / (float)Game1.player.maxHealth);
					Game1.spriteBatch.Draw(Game1.staminaRect, health_bar_rect, new Microsoft.Xna.Framework.Rectangle?(Game1.staminaRect.Bounds), c, 0f, Vector2.Zero, SpriteEffects.None, 1f);
					c.R = (byte)Math.Max(0, (int)(c.R - 50));
					c.G = (byte)Math.Max(0, (int)(c.G - 50));
					if ((float)Game1.getOldMouseX() >= topOfBar.X && (float)Game1.getOldMouseY() >= topOfBar.Y && (float)Game1.getOldMouseX() < topOfBar.X + 32f)
					{
						Game1.drawWithBorder(Math.Max(0, Game1.player.health).ToString() + "/" + Game1.player.maxHealth.ToString(), Color.Black * 0f, Color.Red, topOfBar + new Vector2(-Game1.dialogueFont.MeasureString("999/999").X - 32f, 64f));
					}
					health_bar_rect.Height = 4;
					Game1.spriteBatch.Draw(Game1.staminaRect, health_bar_rect, new Microsoft.Xna.Framework.Rectangle?(Game1.staminaRect.Bounds), c, 0f, Vector2.Zero, SpriteEffects.None, 1f);
				}
				else
				{
					Game1.showingHealth = false;
				}
				foreach (IClickableMenu menu in Game1.onScreenMenus)
				{
					if (menu != Game1.chatBox)
					{
						menu.update(Game1.currentGameTime);
						menu.draw(Game1.spriteBatch);
					}
				}
				if (Game1.player.professions.Contains(17) && Game1.currentLocation.IsOutdoors)
				{
					foreach (KeyValuePair<Vector2, Object> v in Game1.currentLocation.objects.Pairs)
					{
						if ((v.Value.isSpawnedObject.Value || v.Value.QualifiedItemId == "(O)590") && !Utility.isOnScreen(v.Key * 64f + new Vector2(32f, 32f), 64))
						{
							Microsoft.Xna.Framework.Rectangle vpbounds = Game1.graphics.GraphicsDevice.Viewport.Bounds;
							Vector2 onScreenPosition = default(Vector2);
							float rotation = 0f;
							if (v.Key.X * 64f > (float)(Game1.viewport.MaxCorner.X - 64))
							{
								onScreenPosition.X = (float)(vpbounds.Right - 8);
								rotation = 1.5707964f;
							}
							else if (v.Key.X * 64f < (float)Game1.viewport.X)
							{
								onScreenPosition.X = 8f;
								rotation = -1.5707964f;
							}
							else
							{
								onScreenPosition.X = v.Key.X * 64f - (float)Game1.viewport.X;
							}
							if (v.Key.Y * 64f > (float)(Game1.viewport.MaxCorner.Y - 64))
							{
								onScreenPosition.Y = (float)(vpbounds.Bottom - 8);
								rotation = 3.1415927f;
							}
							else if (v.Key.Y * 64f < (float)Game1.viewport.Y)
							{
								onScreenPosition.Y = 8f;
							}
							else
							{
								onScreenPosition.Y = v.Key.Y * 64f - (float)Game1.viewport.Y;
							}
							if (onScreenPosition.X == 8f && onScreenPosition.Y == 8f)
							{
								rotation += 0.7853982f;
							}
							if (onScreenPosition.X == 8f && onScreenPosition.Y == (float)(vpbounds.Bottom - 8))
							{
								rotation += 0.7853982f;
							}
							if (onScreenPosition.X == (float)(vpbounds.Right - 8) && onScreenPosition.Y == 8f)
							{
								rotation -= 0.7853982f;
							}
							if (onScreenPosition.X == (float)(vpbounds.Right - 8) && onScreenPosition.Y == (float)(vpbounds.Bottom - 8))
							{
								rotation -= 0.7853982f;
							}
							Microsoft.Xna.Framework.Rectangle srcRect = new Microsoft.Xna.Framework.Rectangle(412, 495, 5, 4);
							float renderScale = 4f;
							Vector2 onScreenSize = new Vector2((float)srcRect.Width * renderScale, (float)srcRect.Height * renderScale);
							Vector2 safePos = Utility.makeSafe(onScreenPosition, onScreenSize);
							Game1.spriteBatch.Draw(Game1.mouseCursors, safePos, new Microsoft.Xna.Framework.Rectangle?(srcRect), Color.White, rotation, new Vector2(2f, 2f), renderScale, SpriteEffects.None, 1f);
						}
					}
					if (!Game1.currentLocation.orePanPoint.Equals(Point.Zero) && !Utility.isOnScreen(Utility.PointToVector2(Game1.currentLocation.orePanPoint.Value) * 64f + new Vector2(32f, 32f), 64))
					{
						Vector2 onScreenPosition2 = default(Vector2);
						float rotation2 = 0f;
						if (Game1.currentLocation.orePanPoint.X * 64 > Game1.viewport.MaxCorner.X - 64)
						{
							onScreenPosition2.X = (float)(Game1.graphics.GraphicsDevice.Viewport.Bounds.Right - 8);
							rotation2 = 1.5707964f;
						}
						else if (Game1.currentLocation.orePanPoint.X * 64 < Game1.viewport.X)
						{
							onScreenPosition2.X = 8f;
							rotation2 = -1.5707964f;
						}
						else
						{
							onScreenPosition2.X = (float)(Game1.currentLocation.orePanPoint.X * 64 - Game1.viewport.X);
						}
						if (Game1.currentLocation.orePanPoint.Y * 64 > Game1.viewport.MaxCorner.Y - 64)
						{
							onScreenPosition2.Y = (float)(Game1.graphics.GraphicsDevice.Viewport.Bounds.Bottom - 8);
							rotation2 = 3.1415927f;
						}
						else if (Game1.currentLocation.orePanPoint.Y * 64 < Game1.viewport.Y)
						{
							onScreenPosition2.Y = 8f;
						}
						else
						{
							onScreenPosition2.Y = (float)(Game1.currentLocation.orePanPoint.Y * 64 - Game1.viewport.Y);
						}
						if (onScreenPosition2.X == 8f && onScreenPosition2.Y == 8f)
						{
							rotation2 += 0.7853982f;
						}
						if (onScreenPosition2.X == 8f && onScreenPosition2.Y == (float)(Game1.graphics.GraphicsDevice.Viewport.Bounds.Bottom - 8))
						{
							rotation2 += 0.7853982f;
						}
						if (onScreenPosition2.X == (float)(Game1.graphics.GraphicsDevice.Viewport.Bounds.Right - 8) && onScreenPosition2.Y == 8f)
						{
							rotation2 -= 0.7853982f;
						}
						if (onScreenPosition2.X == (float)(Game1.graphics.GraphicsDevice.Viewport.Bounds.Right - 8) && onScreenPosition2.Y == (float)(Game1.graphics.GraphicsDevice.Viewport.Bounds.Bottom - 8))
						{
							rotation2 -= 0.7853982f;
						}
						Game1.spriteBatch.Draw(Game1.mouseCursors, onScreenPosition2, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(412, 495, 5, 4)), Color.Cyan, rotation2, new Vector2(2f, 2f), 4f, SpriteEffects.None, 1f);
					}
				}
			}
		}

		// Token: 0x06000C2D RID: 3117 RVA: 0x00086C04 File Offset: 0x00084E04
		public static void InvalidateOldMouseMovement()
		{
			MouseState input = Game1.input.GetMouseState();
			Game1.oldMouseState = new MouseState(input.X, input.Y, Game1.oldMouseState.ScrollWheelValue, Game1.oldMouseState.LeftButton, Game1.oldMouseState.MiddleButton, Game1.oldMouseState.RightButton, Game1.oldMouseState.XButton1, Game1.oldMouseState.XButton2);
		}

		// Token: 0x06000C2E RID: 3118 RVA: 0x00086C70 File Offset: 0x00084E70
		public static bool IsRenderingNonNativeUIScale()
		{
			return Game1.options.uiScale != Game1.options.zoomLevel;
		}

		// Token: 0x06000C2F RID: 3119 RVA: 0x00086C8C File Offset: 0x00084E8C
		public virtual void drawMouseCursor()
		{
			if (Game1.activeClickableMenu == null && Game1.timerUntilMouseFade > 0)
			{
				Game1.timerUntilMouseFade -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
				Game1.lastMousePositionBeforeFade = Game1.getMousePosition();
			}
			if (Game1.options.gamepadControls && Game1.timerUntilMouseFade <= 0 && Game1.activeClickableMenu == null && (Game1.emoteMenu == null || Game1.emoteMenu.gamepadMode))
			{
				Game1.mouseCursorTransparency = 0f;
			}
			if (Game1.activeClickableMenu == null && Game1.mouseCursor > Game1.cursor_none && Game1.currentLocation != null)
			{
				if (Game1.IsRenderingNonNativeUIScale())
				{
					Game1.spriteBatch.End();
					Game1.PopUIMode();
					if (this.ShouldDrawOnBuffer())
					{
						Game1.SetRenderTarget(this.screen);
					}
					else
					{
						Game1.SetRenderTarget(null);
					}
					Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
				}
				if (Game1.mouseCursorTransparency <= 0f || !Utility.canGrabSomethingFromHere(Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y, Game1.player) || Game1.mouseCursor == Game1.cursor_gift)
				{
					if (Game1.player.ActiveObject != null && Game1.mouseCursor != Game1.cursor_gift && !Game1.eventUp && Game1.currentMinigame == null && !Game1.player.isRidingHorse() && Game1.player.CanMove && Game1.displayFarmer)
					{
						if (Game1.mouseCursorTransparency > 0f || Game1.options.showPlacementTileForGamepad)
						{
							Game1.player.ActiveObject.drawPlacementBounds(Game1.spriteBatch, Game1.currentLocation);
							if (Game1.mouseCursorTransparency > 0f)
							{
								Game1.spriteBatch.End();
								Game1.PushUIMode();
								Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
								bool canPlace = Utility.playerCanPlaceItemHere(Game1.currentLocation, Game1.player.CurrentItem, Game1.getMouseX() + Game1.viewport.X, Game1.getMouseY() + Game1.viewport.Y, Game1.player, false) || (Utility.isThereAnObjectHereWhichAcceptsThisItem(Game1.currentLocation, Game1.player.CurrentItem, Game1.getMouseX() + Game1.viewport.X, Game1.getMouseY() + Game1.viewport.Y) && Utility.withinRadiusOfPlayer(Game1.getMouseX() + Game1.viewport.X, Game1.getMouseY() + Game1.viewport.Y, 1, Game1.player));
								Item currentItem = Game1.player.CurrentItem;
								if (currentItem != null)
								{
									currentItem.drawInMenu(Game1.spriteBatch, new Vector2((float)(Game1.getMouseX() + 16), (float)(Game1.getMouseY() + 16)), canPlace ? (Game1.dialogueButtonScale / 75f + 1f) : 1f, canPlace ? 1f : 0.5f, 0.999f);
								}
								Game1.spriteBatch.End();
								Game1.PopUIMode();
								Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
							}
						}
					}
					else if (Game1.mouseCursor == Game1.cursor_default && Game1.isActionAtCurrentCursorTile && Game1.currentMinigame == null)
					{
						Game1.mouseCursor = (Game1.isSpeechAtCurrentCursorTile ? Game1.cursor_talk : (Game1.isInspectionAtCurrentCursorTile ? Game1.cursor_look : Game1.cursor_grab));
					}
					else if (Game1.mouseCursorTransparency > 0f)
					{
						NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> animals = Game1.currentLocation.animals;
						if (animals != null)
						{
							Vector2 mousePos = new Vector2((float)(Game1.getOldMouseX() + Game1.uiViewport.X), (float)(Game1.getOldMouseY() + Game1.uiViewport.Y));
							bool mouseWithinRadiusOfPlayer = Utility.withinRadiusOfPlayer((int)mousePos.X, (int)mousePos.Y, 1, Game1.player);
							foreach (KeyValuePair<long, FarmAnimal> kvp in animals.Pairs)
							{
								Microsoft.Xna.Framework.Rectangle animalBounds = kvp.Value.GetCursorPetBoundingBox();
								if (!kvp.Value.wasPet.Value && animalBounds.Contains((int)mousePos.X, (int)mousePos.Y))
								{
									Game1.mouseCursor = Game1.cursor_grab;
									if (!mouseWithinRadiusOfPlayer)
									{
										Game1.mouseCursorTransparency = 0.5f;
										break;
									}
									break;
								}
							}
						}
					}
				}
				if (Game1.IsRenderingNonNativeUIScale())
				{
					Game1.spriteBatch.End();
					Game1.PushUIMode();
					Game1.SetRenderTarget(this.uiScreen);
					Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
				}
				if (Game1.currentMinigame != null)
				{
					Game1.mouseCursor = Game1.cursor_default;
				}
				if (!Game1.freezeControls && !Game1.options.hardwareCursor)
				{
					Game1.spriteBatch.Draw(Game1.mouseCursors, new Vector2((float)Game1.getMouseX(), (float)Game1.getMouseY()), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, Game1.mouseCursor, 16, 16)), Color.White * Game1.mouseCursorTransparency, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
				}
				Game1.wasMouseVisibleThisFrame = (Game1.mouseCursorTransparency > 0f);
				this._lastDrewMouseCursor = Game1.wasMouseVisibleThisFrame;
			}
			Game1.mouseCursor = Game1.cursor_default;
			if (!Game1.isActionAtCurrentCursorTile && Game1.activeClickableMenu == null)
			{
				Game1.mouseCursorTransparency = 1f;
			}
		}

		// Token: 0x17000192 RID: 402
		// (get) Token: 0x06000C30 RID: 3120 RVA: 0x0008721C File Offset: 0x0008541C
		// (set) Token: 0x06000C31 RID: 3121 RVA: 0x00087223 File Offset: 0x00085423
		public static float mouseCursorTransparency
		{
			get
			{
				return Game1._mouseCursorTransparency;
			}
			set
			{
				Game1._mouseCursorTransparency = value;
			}
		}

		// Token: 0x06000C32 RID: 3122 RVA: 0x0008722C File Offset: 0x0008542C
		public static void panScreen(int x, int y)
		{
			int old_ui_mode_count = Game1.uiModeCount;
			while (Game1.uiModeCount > 0)
			{
				Game1.PopUIMode();
			}
			Game1.previousViewportPosition.X = (float)Game1.viewport.Location.X;
			Game1.previousViewportPosition.Y = (float)Game1.viewport.Location.Y;
			Game1.viewport.X = Game1.viewport.X + x;
			Game1.viewport.Y = Game1.viewport.Y + y;
			Game1.clampViewportToGameMap();
			Game1.updateRaindropPosition();
			for (int i = 0; i < old_ui_mode_count; i++)
			{
				Game1.PushUIMode();
			}
		}

		// Token: 0x06000C33 RID: 3123 RVA: 0x000872C4 File Offset: 0x000854C4
		public static void clampViewportToGameMap()
		{
			if (Game1.viewport.X < 0)
			{
				Game1.viewport.X = 0;
			}
			if (Game1.viewport.X > Game1.currentLocation.map.DisplayWidth - Game1.viewport.Width)
			{
				Game1.viewport.X = Game1.currentLocation.map.DisplayWidth - Game1.viewport.Width;
			}
			if (Game1.viewport.Y < 0)
			{
				Game1.viewport.Y = 0;
			}
			if (Game1.viewport.Y > Game1.currentLocation.map.DisplayHeight - Game1.viewport.Height)
			{
				Game1.viewport.Y = Game1.currentLocation.map.DisplayHeight - Game1.viewport.Height;
			}
		}

		// Token: 0x06000C34 RID: 3124 RVA: 0x00087398 File Offset: 0x00085598
		protected void drawDialogueBox()
		{
			if (Game1.currentSpeaker == null)
			{
				return;
			}
			int messageHeight = (int)Game1.dialogueFont.MeasureString(Game1.currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue()).Y;
			messageHeight = Math.Max(messageHeight, 320);
			Game1.drawDialogueBox((base.GraphicsDevice.Viewport.GetTitleSafeArea().Width - Math.Min(1280, base.GraphicsDevice.Viewport.GetTitleSafeArea().Width - 128)) / 2, base.GraphicsDevice.Viewport.GetTitleSafeArea().Height - messageHeight, Math.Min(1280, base.GraphicsDevice.Viewport.GetTitleSafeArea().Width - 128), messageHeight, true, false, null, Game1.objectDialoguePortraitPerson != null && Game1.currentSpeaker == null, true, -1, -1, -1);
		}

		// Token: 0x06000C35 RID: 3125 RVA: 0x00087476 File Offset: 0x00085676
		public static void drawDialogueBox(string message)
		{
			Game1.drawDialogueBox(Game1.viewport.Width / 2, Game1.viewport.Height / 2, false, false, message);
		}

		// Token: 0x06000C36 RID: 3126 RVA: 0x00087498 File Offset: 0x00085698
		public static void drawDialogueBox(int centerX, int centerY, bool speaker, bool drawOnlyBox, string message)
		{
			string text = null;
			if (speaker && Game1.currentSpeaker != null)
			{
				text = Game1.currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue();
			}
			else if (message != null)
			{
				text = message;
			}
			else if (Game1.currentObjectDialogue.Count > 0)
			{
				text = Game1.currentObjectDialogue.Peek();
			}
			if (text == null)
			{
				return;
			}
			Vector2 vector = Game1.dialogueFont.MeasureString(text);
			int width = (int)vector.X + 128;
			int height = (int)vector.Y + 128;
			int x = centerX - width / 2;
			int y = centerY - height / 2;
			Game1.drawDialogueBox(x, y, width, height, speaker, drawOnlyBox, message, Game1.objectDialoguePortraitPerson != null && !speaker, true, -1, -1, -1);
		}

		// Token: 0x06000C37 RID: 3127 RVA: 0x0008753C File Offset: 0x0008573C
		public static void DrawBox(int x, int y, int width, int height, Color? color = null)
		{
			Microsoft.Xna.Framework.Rectangle sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64);
			sourceRect.X = 64;
			sourceRect.Y = 128;
			Texture2D menu_texture = Game1.menuTexture;
			Color draw_color = Color.White;
			Color inner_color = Color.White;
			if (color != null)
			{
				draw_color = color.Value;
				menu_texture = Game1.uncoloredMenuTexture;
				inner_color = new Color((int)Utility.Lerp((float)draw_color.R, (float)Math.Min(255, (int)(draw_color.R + 150)), 0.65f), (int)Utility.Lerp((float)draw_color.G, (float)Math.Min(255, (int)(draw_color.G + 150)), 0.65f), (int)Utility.Lerp((float)draw_color.B, (float)Math.Min(255, (int)(draw_color.B + 150)), 0.65f));
			}
			Game1.spriteBatch.Draw(menu_texture, new Microsoft.Xna.Framework.Rectangle(x, y, width, height), new Microsoft.Xna.Framework.Rectangle?(sourceRect), inner_color);
			sourceRect.Y = 0;
			Vector2 offset = new Vector2((float)(-(float)sourceRect.Width) * 0.5f, (float)(-(float)sourceRect.Height) * 0.5f);
			sourceRect.X = 0;
			Game1.spriteBatch.Draw(menu_texture, new Vector2((float)x + offset.X, (float)y + offset.Y), new Microsoft.Xna.Framework.Rectangle?(sourceRect), draw_color);
			sourceRect.X = 192;
			Game1.spriteBatch.Draw(menu_texture, new Vector2((float)x + offset.X + (float)width, (float)y + offset.Y), new Microsoft.Xna.Framework.Rectangle?(sourceRect), draw_color);
			sourceRect.Y = 192;
			Game1.spriteBatch.Draw(menu_texture, new Vector2((float)(x + width) + offset.X, (float)(y + height) + offset.Y), new Microsoft.Xna.Framework.Rectangle?(sourceRect), draw_color);
			sourceRect.X = 0;
			Game1.spriteBatch.Draw(menu_texture, new Vector2((float)x + offset.X, (float)(y + height) + offset.Y), new Microsoft.Xna.Framework.Rectangle?(sourceRect), draw_color);
			sourceRect.X = 128;
			sourceRect.Y = 0;
			Game1.spriteBatch.Draw(menu_texture, new Microsoft.Xna.Framework.Rectangle(64 + x + (int)offset.X, y + (int)offset.Y, width - 64, 64), new Microsoft.Xna.Framework.Rectangle?(sourceRect), draw_color);
			sourceRect.Y = 192;
			Game1.spriteBatch.Draw(menu_texture, new Microsoft.Xna.Framework.Rectangle(64 + x + (int)offset.X, y + (int)offset.Y + height, width - 64, 64), new Microsoft.Xna.Framework.Rectangle?(sourceRect), draw_color);
			sourceRect.Y = 128;
			sourceRect.X = 0;
			Game1.spriteBatch.Draw(menu_texture, new Microsoft.Xna.Framework.Rectangle(x + (int)offset.X, y + (int)offset.Y + 64, 64, height - 64), new Microsoft.Xna.Framework.Rectangle?(sourceRect), draw_color);
			sourceRect.X = 192;
			Game1.spriteBatch.Draw(menu_texture, new Microsoft.Xna.Framework.Rectangle(x + width + (int)offset.X, y + (int)offset.Y + 64, 64, height - 64), new Microsoft.Xna.Framework.Rectangle?(sourceRect), draw_color);
		}

		// Token: 0x06000C38 RID: 3128 RVA: 0x00087858 File Offset: 0x00085A58
		public static void drawDialogueBox(int x, int y, int width, int height, bool speaker, bool drawOnlyBox, string message = null, bool objectDialogueWithPortrait = false, bool ignoreTitleSafe = true, int r = -1, int g = -1, int b = -1)
		{
			if (!drawOnlyBox)
			{
				return;
			}
			Microsoft.Xna.Framework.Rectangle titleSafeArea = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea();
			int screenHeight = titleSafeArea.Height;
			int screenWidth = titleSafeArea.Width;
			int dialogueX = 0;
			int dialogueY = 0;
			if (!ignoreTitleSafe)
			{
				dialogueY = ((y > titleSafeArea.Y) ? 0 : (titleSafeArea.Y - y));
			}
			int everythingYOffset = 0;
			width = Math.Min(titleSafeArea.Width, width);
			if (!Game1.isQuestion && Game1.currentSpeaker == null && Game1.currentObjectDialogue.Count > 0 && !drawOnlyBox)
			{
				width = (int)Game1.dialogueFont.MeasureString(Game1.currentObjectDialogue.Peek()).X + 128;
				height = (int)Game1.dialogueFont.MeasureString(Game1.currentObjectDialogue.Peek()).Y + 64;
				x = screenWidth / 2 - width / 2;
				everythingYOffset = ((height > 256) ? (-(height - 256)) : 0);
			}
			Microsoft.Xna.Framework.Rectangle sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64);
			int addedTileHeightForQuestions = -1;
			if (Game1.questionChoices.Count >= 3)
			{
				addedTileHeightForQuestions = Game1.questionChoices.Count - 3;
			}
			if (!drawOnlyBox && Game1.currentObjectDialogue.Count > 0)
			{
				if (Game1.dialogueFont.MeasureString(Game1.currentObjectDialogue.Peek()).Y >= (float)(height - 128))
				{
					addedTileHeightForQuestions -= (int)(((float)(height - 128) - Game1.dialogueFont.MeasureString(Game1.currentObjectDialogue.Peek()).Y) / 64f) - 1;
				}
				else
				{
					height += (int)Game1.dialogueFont.MeasureString(Game1.currentObjectDialogue.Peek()).Y / 2;
					everythingYOffset -= (int)Game1.dialogueFont.MeasureString(Game1.currentObjectDialogue.Peek()).Y / 2;
					if ((int)Game1.dialogueFont.MeasureString(Game1.currentObjectDialogue.Peek()).Y / 2 > 64)
					{
						addedTileHeightForQuestions = 0;
					}
				}
			}
			if (Game1.currentSpeaker != null && Game1.isQuestion && Game1.currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue().Substring(0, Game1.currentDialogueCharacterIndex).Contains(Environment.NewLine))
			{
				addedTileHeightForQuestions++;
			}
			sourceRect.Width = 64;
			sourceRect.Height = 64;
			sourceRect.X = 64;
			sourceRect.Y = 128;
			Color tint = (r == -1) ? Color.White : new Color(r, g, b);
			Texture2D texture = (r == -1) ? Game1.menuTexture : Game1.uncoloredMenuTexture;
			Game1.spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(28 + x + dialogueX, 28 + y - 64 * addedTileHeightForQuestions + dialogueY + everythingYOffset, width - 64, height - 64 + addedTileHeightForQuestions * 64), new Microsoft.Xna.Framework.Rectangle?(sourceRect), (r == -1) ? tint : new Color((int)Utility.Lerp((float)r, (float)Math.Min(255, r + 150), 0.65f), (int)Utility.Lerp((float)g, (float)Math.Min(255, g + 150), 0.65f), (int)Utility.Lerp((float)b, (float)Math.Min(255, b + 150), 0.65f)));
			sourceRect.Y = 0;
			sourceRect.X = 0;
			Game1.spriteBatch.Draw(texture, new Vector2((float)(x + dialogueX), (float)(y - 64 * addedTileHeightForQuestions + dialogueY + everythingYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRect), tint);
			sourceRect.X = 192;
			Game1.spriteBatch.Draw(texture, new Vector2((float)(x + width + dialogueX - 64), (float)(y - 64 * addedTileHeightForQuestions + dialogueY + everythingYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRect), tint);
			sourceRect.Y = 192;
			Game1.spriteBatch.Draw(texture, new Vector2((float)(x + width + dialogueX - 64), (float)(y + height + dialogueY - 64 + everythingYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRect), tint);
			sourceRect.X = 0;
			Game1.spriteBatch.Draw(texture, new Vector2((float)(x + dialogueX), (float)(y + height + dialogueY - 64 + everythingYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRect), tint);
			sourceRect.X = 128;
			sourceRect.Y = 0;
			Game1.spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(64 + x + dialogueX, y - 64 * addedTileHeightForQuestions + dialogueY + everythingYOffset, width - 128, 64), new Microsoft.Xna.Framework.Rectangle?(sourceRect), tint);
			sourceRect.Y = 192;
			Game1.spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(64 + x + dialogueX, y + height + dialogueY - 64 + everythingYOffset, width - 128, 64), new Microsoft.Xna.Framework.Rectangle?(sourceRect), tint);
			sourceRect.Y = 128;
			sourceRect.X = 0;
			Game1.spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(x + dialogueX, y - 64 * addedTileHeightForQuestions + dialogueY + 64 + everythingYOffset, 64, height - 128 + addedTileHeightForQuestions * 64), new Microsoft.Xna.Framework.Rectangle?(sourceRect), tint);
			sourceRect.X = 192;
			Game1.spriteBatch.Draw(texture, new Microsoft.Xna.Framework.Rectangle(x + width + dialogueX - 64, y - 64 * addedTileHeightForQuestions + dialogueY + 64 + everythingYOffset, 64, height - 128 + addedTileHeightForQuestions * 64), new Microsoft.Xna.Framework.Rectangle?(sourceRect), tint);
			if ((objectDialogueWithPortrait && Game1.objectDialoguePortraitPerson != null) || (speaker && Game1.currentSpeaker != null && Game1.currentSpeaker.CurrentDialogue.Count > 0 && Game1.currentSpeaker.CurrentDialogue.Peek().showPortrait))
			{
				NPC theSpeaker = objectDialogueWithPortrait ? Game1.objectDialoguePortraitPerson : Game1.currentSpeaker;
				string text2 = objectDialogueWithPortrait ? ((Game1.objectDialoguePortraitPerson.Name == Game1.player.spouse) ? "$l" : "$neutral") : theSpeaker.CurrentDialogue.Peek().CurrentEmotion;
				Microsoft.Xna.Framework.Rectangle portraitRect;
				if (text2 != null)
				{
					int length = text2.Length;
					if (length != 2)
					{
						if (length != 8)
						{
							goto IL_714;
						}
						if (!(text2 == "$neutral"))
						{
							goto IL_714;
						}
					}
					else
					{
						char c = text2[1];
						if (c <= 'l')
						{
							if (c != 'a')
							{
								switch (c)
								{
								case 'h':
									if (!(text2 == "$h"))
									{
										goto IL_714;
									}
									portraitRect = new Microsoft.Xna.Framework.Rectangle(64, 0, 64, 64);
									goto IL_740;
								case 'i':
								case 'j':
									goto IL_714;
								case 'k':
									if (!(text2 == "$k"))
									{
										goto IL_714;
									}
									break;
								case 'l':
									if (!(text2 == "$l"))
									{
										goto IL_714;
									}
									portraitRect = new Microsoft.Xna.Framework.Rectangle(0, 128, 64, 64);
									goto IL_740;
								default:
									goto IL_714;
								}
							}
							else
							{
								if (!(text2 == "$a"))
								{
									goto IL_714;
								}
								portraitRect = new Microsoft.Xna.Framework.Rectangle(64, 128, 64, 64);
								goto IL_740;
							}
						}
						else if (c != 's')
						{
							if (c != 'u')
							{
								goto IL_714;
							}
							if (!(text2 == "$u"))
							{
								goto IL_714;
							}
							portraitRect = new Microsoft.Xna.Framework.Rectangle(64, 64, 64, 64);
							goto IL_740;
						}
						else
						{
							if (!(text2 == "$s"))
							{
								goto IL_714;
							}
							portraitRect = new Microsoft.Xna.Framework.Rectangle(0, 64, 64, 64);
							goto IL_740;
						}
					}
					portraitRect = new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64);
					goto IL_740;
				}
				IL_714:
				portraitRect = Game1.getSourceRectForStandardTileSheet(theSpeaker.Portrait, Convert.ToInt32(theSpeaker.CurrentDialogue.Peek().CurrentEmotion.Substring(1)), -1, -1);
				IL_740:
				Game1.spriteBatch.End();
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, null);
				if (theSpeaker.Portrait != null)
				{
					Game1.spriteBatch.Draw(Game1.mouseCursors, new Vector2((float)(dialogueX + x + 768), (float)(screenHeight - 320 - 64 * addedTileHeightForQuestions - 256 + dialogueY + 16 - 60 + everythingYOffset)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(333, 305, 80, 87)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.98f);
					Game1.spriteBatch.Draw(theSpeaker.Portrait, new Vector2((float)(dialogueX + x + 768 + 32), (float)(screenHeight - 320 - 64 * addedTileHeightForQuestions - 256 + dialogueY + 16 - 60 + everythingYOffset)), new Microsoft.Xna.Framework.Rectangle?(portraitRect), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
				}
				Game1.spriteBatch.End();
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
				if (Game1.isQuestion)
				{
					Game1.spriteBatch.DrawString(Game1.dialogueFont, theSpeaker.displayName, new Vector2(928f - Game1.dialogueFont.MeasureString(theSpeaker.displayName).X / 2f + (float)dialogueX + (float)x, (float)(screenHeight - 320 - 64 * addedTileHeightForQuestions) - Game1.dialogueFont.MeasureString(theSpeaker.displayName).Y + (float)dialogueY + 21f + (float)everythingYOffset) + new Vector2(2f, 2f), new Color(150, 150, 150));
				}
				Game1.spriteBatch.DrawString(Game1.dialogueFont, theSpeaker.Name.Equals("Lewis") ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3756") : theSpeaker.displayName, new Vector2((float)(dialogueX + x + 896 + 32) - Game1.dialogueFont.MeasureString(theSpeaker.Name.Equals("Lewis") ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3756") : theSpeaker.displayName).X / 2f, (float)(screenHeight - 320 - 64 * addedTileHeightForQuestions) - Game1.dialogueFont.MeasureString(theSpeaker.Name.Equals("Lewis") ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3756") : theSpeaker.displayName).Y + (float)dialogueY + 21f + 8f + (float)everythingYOffset), Game1.textColor);
			}
			if (!drawOnlyBox)
			{
				string text = "";
				if (Game1.currentSpeaker != null && Game1.currentSpeaker.CurrentDialogue.Count > 0)
				{
					if (Game1.currentSpeaker.CurrentDialogue.Peek() == null || Game1.currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue().Length < Game1.currentDialogueCharacterIndex - 1)
					{
						Game1.dialogueUp = false;
						Game1.currentDialogueCharacterIndex = 0;
						Game1.playSound("dialogueCharacterClose", null);
						Game1.player.forceCanMove();
						return;
					}
					text = Game1.currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue().Substring(0, Game1.currentDialogueCharacterIndex);
				}
				else if (message != null)
				{
					text = message;
				}
				else if (Game1.currentObjectDialogue.Count > 0)
				{
					text = ((Game1.currentObjectDialogue.Peek().Length <= 1) ? "" : Game1.currentObjectDialogue.Peek().Substring(0, Game1.currentDialogueCharacterIndex));
				}
				Vector2 textPosition;
				if (Game1.dialogueFont.MeasureString(text).X > (float)(screenWidth - 256 - dialogueX))
				{
					textPosition = new Vector2((float)(128 + dialogueX), (float)(screenHeight - 64 * addedTileHeightForQuestions - 256 - 16 + dialogueY + everythingYOffset));
				}
				else if (Game1.currentSpeaker != null && Game1.currentSpeaker.CurrentDialogue.Count > 0)
				{
					textPosition = new Vector2((float)(screenWidth / 2) - Game1.dialogueFont.MeasureString(Game1.currentSpeaker.CurrentDialogue.Peek().getCurrentDialogue()).X / 2f + (float)dialogueX, (float)(screenHeight - 64 * addedTileHeightForQuestions - 256 - 16 + dialogueY + everythingYOffset));
				}
				else if (message != null)
				{
					textPosition = new Vector2((float)(screenWidth / 2) - Game1.dialogueFont.MeasureString(text).X / 2f + (float)dialogueX, (float)(y + 96 + 4));
				}
				else if (Game1.isQuestion)
				{
					textPosition = new Vector2((float)(screenWidth / 2) - Game1.dialogueFont.MeasureString((Game1.currentObjectDialogue.Count == 0) ? "" : Game1.currentObjectDialogue.Peek()).X / 2f + (float)dialogueX, (float)(screenHeight - 64 * addedTileHeightForQuestions - 256 - (16 + (Game1.questionChoices.Count - 2) * 64) + dialogueY + everythingYOffset));
				}
				else
				{
					textPosition = new Vector2((float)(screenWidth / 2) - Game1.dialogueFont.MeasureString((Game1.currentObjectDialogue.Count == 0) ? "" : Game1.currentObjectDialogue.Peek()).X / 2f + (float)dialogueX, (float)(y + 4 + everythingYOffset));
				}
				if (!drawOnlyBox)
				{
					Game1.spriteBatch.DrawString(Game1.dialogueFont, text, textPosition + new Vector2(3f, 0f), Game1.textShadowColor);
					Game1.spriteBatch.DrawString(Game1.dialogueFont, text, textPosition + new Vector2(3f, 3f), Game1.textShadowColor);
					Game1.spriteBatch.DrawString(Game1.dialogueFont, text, textPosition + new Vector2(0f, 3f), Game1.textShadowColor);
					Game1.spriteBatch.DrawString(Game1.dialogueFont, text, textPosition, Game1.textColor);
				}
				if (Game1.dialogueFont.MeasureString(text).Y <= 64f)
				{
					dialogueY += 64;
				}
				if (Game1.isQuestion && !Game1.dialogueTyping)
				{
					for (int i = 0; i < Game1.questionChoices.Count; i++)
					{
						if (Game1.currentQuestionChoice == i)
						{
							textPosition.X = (float)(80 + dialogueX + x);
							textPosition.Y = (float)(screenHeight - (5 + addedTileHeightForQuestions + 1) * 64) + ((text.Trim().Length > 0) ? Game1.dialogueFont.MeasureString(text).Y : 0f) + 128f + (float)(48 * i) - (float)(16 + (Game1.questionChoices.Count - 2) * 64) + (float)dialogueY + (float)everythingYOffset;
							Game1.spriteBatch.End();
							Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, null);
							Game1.spriteBatch.Draw(Game1.objectSpriteSheet, textPosition + new Vector2((float)Math.Cos((double)Game1.currentGameTime.TotalGameTime.Milliseconds * 3.141592653589793 / 512.0) * 3f, 0f), new Microsoft.Xna.Framework.Rectangle?(GameLocation.getSourceRectForObject(26)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
							Game1.spriteBatch.End();
							Game1.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
							textPosition.X = (float)(160 + dialogueX + x);
							textPosition.Y = (float)(screenHeight - (5 + addedTileHeightForQuestions + 1) * 64) + ((text.Trim().Length > 1) ? Game1.dialogueFont.MeasureString(text).Y : 0f) + 128f - (float)((Game1.questionChoices.Count - 2) * 64) + (float)(48 * i) + (float)dialogueY + (float)everythingYOffset;
							Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.questionChoices[i].responseText, textPosition, Game1.textColor);
						}
						else
						{
							textPosition.X = (float)(128 + dialogueX + x);
							textPosition.Y = (float)(screenHeight - (5 + addedTileHeightForQuestions + 1) * 64) + ((text.Trim().Length > 1) ? Game1.dialogueFont.MeasureString(text).Y : 0f) + 128f - (float)((Game1.questionChoices.Count - 2) * 64) + (float)(48 * i) + (float)dialogueY + (float)everythingYOffset;
							Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.questionChoices[i].responseText, textPosition, Game1.unselectedOptionColor);
						}
					}
				}
				if (!drawOnlyBox && !Game1.dialogueTyping && message == null)
				{
					Game1.spriteBatch.Draw(Game1.mouseCursors, new Vector2((float)(x + dialogueX + width - 96), (float)(y + height + dialogueY + everythingYOffset - 96) - Game1.dialogueButtonScale), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, (!Game1.dialogueButtonShrinking && Game1.dialogueButtonScale < 8f) ? 3 : 2, -1, -1)), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
				}
			}
		}

		// Token: 0x06000C39 RID: 3129 RVA: 0x000888FC File Offset: 0x00086AFC
		public static void drawPlayerHeldObject(Farmer f)
		{
			if ((!Game1.eventUp || (Game1.currentLocation.currentEvent != null && Game1.currentLocation.currentEvent.showActiveObject)) && !f.FarmerSprite.PauseForSingleAnimation && !f.isRidingHorse() && !f.bathingClothes.Value && !f.onBridge.Value)
			{
				float xPosition = f.getLocalPosition(Game1.viewport).X + (float)((f.rotation < 0f) ? -8 : ((f.rotation > 0f) ? 8 : 0)) + (float)(f.FarmerSprite.CurrentAnimationFrame.xOffset * 4);
				float objectYLoc = f.getLocalPosition(Game1.viewport).Y - 128f + (float)(f.FarmerSprite.CurrentAnimationFrame.positionOffset * 4) + (float)(FarmerRenderer.featureYOffsetPerFrame[f.FarmerSprite.CurrentFrame] * 4);
				if (f.ActiveObject.bigCraftable.Value)
				{
					objectYLoc -= 64f;
				}
				if (f.isEating)
				{
					xPosition = f.getLocalPosition(Game1.viewport).X - 21f;
					objectYLoc = f.getLocalPosition(Game1.viewport).Y - 128f + 12f;
				}
				if (!f.isEating || (f.isEating && f.Sprite.currentFrame <= 218))
				{
					f.ActiveObject.drawWhenHeld(Game1.spriteBatch, new Vector2((float)((int)xPosition), (float)((int)objectYLoc)), f);
				}
			}
		}

		// Token: 0x06000C3A RID: 3130 RVA: 0x00088A8B File Offset: 0x00086C8B
		public static void drawTool(Farmer f)
		{
			Game1.drawTool(f, f.CurrentTool.CurrentParentTileIndex);
		}

		// Token: 0x06000C3B RID: 3131 RVA: 0x00088AA0 File Offset: 0x00086CA0
		public static void drawTool(Farmer f, int currentToolIndex)
		{
			Vector2 fPosition = f.getLocalPosition(Game1.viewport) + f.jitter + f.armOffset;
			FarmerSprite farmerSprite = (FarmerSprite)f.Sprite;
			MeleeWeapon weapon = f.CurrentTool as MeleeWeapon;
			if (weapon != null)
			{
				weapon.drawDuringUse(farmerSprite.currentAnimationIndex, f.FacingDirection, Game1.spriteBatch, fPosition, f);
				return;
			}
			if (f.FarmerSprite.isUsingWeapon())
			{
				MeleeWeapon.drawDuringUse(farmerSprite.currentAnimationIndex, f.FacingDirection, Game1.spriteBatch, fPosition, f, f.FarmerSprite.CurrentToolIndex.ToString(), f.FarmerSprite.getWeaponTypeFromAnimation(), false);
				return;
			}
			Tool currentTool = f.CurrentTool;
			if (!(currentTool is Slingshot) && !(currentTool is Shears) && !(currentTool is MilkPail) && !(currentTool is Pan))
			{
				if (!(currentTool is FishingRod) && !(currentTool is WateringCan) && f != Game1.player)
				{
					if (farmerSprite.currentSingleAnimation < 160 || farmerSprite.currentSingleAnimation >= 192)
					{
						return;
					}
					if (f.CurrentTool != null)
					{
						f.CurrentTool.Update(f.FacingDirection, 0, f);
						currentToolIndex = f.CurrentTool.CurrentParentTileIndex;
					}
				}
				Tool currentTool2 = f.CurrentTool;
				ParsedItemData data = ItemRegistry.GetData((currentTool2 != null) ? currentTool2.QualifiedItemId : null);
				Texture2D spritesheet = ((data != null) ? data.GetTexture() : null) ?? Game1.toolSpriteSheet;
				Microsoft.Xna.Framework.Rectangle sourceRectangleForTool = new Microsoft.Xna.Framework.Rectangle(currentToolIndex * 16 % spritesheet.Width, currentToolIndex * 16 / spritesheet.Width * 16, 16, 32);
				float base_layer_depth = f.getDrawLayer();
				FishingRod rod = f.CurrentTool as FishingRod;
				if (rod != null)
				{
					if (rod.fishCaught || rod.showingTreasure)
					{
						f.CurrentTool.draw(Game1.spriteBatch);
						return;
					}
					sourceRectangleForTool = new Microsoft.Xna.Framework.Rectangle(farmerSprite.currentAnimationIndex * 48, 288, 48, 48);
					if (f.FacingDirection == 2 || f.FacingDirection == 0)
					{
						sourceRectangleForTool.Y += 48;
					}
					else if (rod.isFishing && (!rod.isReeling || rod.hit))
					{
						fPosition.Y += 8f;
					}
					if (rod.isFishing)
					{
						sourceRectangleForTool.X += (5 - farmerSprite.currentAnimationIndex) * 48;
					}
					if (rod.isReeling)
					{
						if (f.FacingDirection == 2 || f.FacingDirection == 0)
						{
							sourceRectangleForTool.X = 288;
							if (f.IsLocalPlayer && Game1.didPlayerJustClickAtAll(false))
							{
								sourceRectangleForTool.X = 0;
							}
						}
						else
						{
							sourceRectangleForTool.X = 288;
							sourceRectangleForTool.Y = 240;
							if (f.IsLocalPlayer && Game1.didPlayerJustClickAtAll(false))
							{
								sourceRectangleForTool.Y += 48;
							}
						}
					}
					if (f.FarmerSprite.CurrentFrame == 57)
					{
						sourceRectangleForTool.Height = 0;
					}
					if (f.FacingDirection == 0)
					{
						fPosition.X += 16f;
					}
				}
				Tool currentTool3 = f.CurrentTool;
				if (currentTool3 != null)
				{
					currentTool3.draw(Game1.spriteBatch);
				}
				int toolYOffset = 0;
				int toolXOffset = 0;
				if (f.CurrentTool is WateringCan)
				{
					toolYOffset += 80;
					toolXOffset = ((f.FacingDirection == 1) ? 32 : ((f.FacingDirection == 3) ? -32 : 0));
					if (farmerSprite.currentAnimationIndex == 0 || farmerSprite.currentAnimationIndex == 1)
					{
						toolXOffset = toolXOffset * 3 / 2;
					}
				}
				toolYOffset += f.yJumpOffset;
				int facingDirection = f.FacingDirection;
				FarmerRenderer.FarmerSpriteLayers tool_layer;
				if (facingDirection != 0)
				{
					if (facingDirection != 2)
					{
						tool_layer = FarmerRenderer.FarmerSpriteLayers.TOOL_IN_USE_SIDE;
					}
					else
					{
						tool_layer = FarmerRenderer.FarmerSpriteLayers.ToolDown;
					}
				}
				else
				{
					tool_layer = FarmerRenderer.FarmerSpriteLayers.ToolUp;
				}
				float layerDepth = FarmerRenderer.GetLayerDepth(base_layer_depth, tool_layer, false);
				facingDirection = f.FacingDirection;
				if (facingDirection != 1)
				{
					if (facingDirection != 3)
					{
						if (farmerSprite.currentAnimationIndex > 2)
						{
							FishingRod rod2 = f.CurrentTool as FishingRod;
							if (rod2 == null || rod2.isCasting || rod2.castedButBobberStillInAir || rod2.isTimingCast)
							{
								Point tileLocation = f.TilePoint;
								if (f.currentLocation.hasTileAt(tileLocation, "Front", null) && f.Position.Y % 64f < 32f && f.Position.Y % 64f > 16f)
								{
									return;
								}
							}
						}
						currentTool = f.CurrentTool;
						FishingRod fishingRod = currentTool as FishingRod;
						if (fishingRod == null)
						{
							if (!(currentTool is WateringCan))
							{
								switch (farmerSprite.currentAnimationIndex)
								{
								case 0:
									if (f.FacingDirection == 0)
									{
										Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset, fPosition.Y - 128f - 8f + (float)toolYOffset + (float)Math.Min(8, f.toolPower.Value * 4))), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
										return;
									}
									Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset - 20f, fPosition.Y - 128f + 12f + (float)toolYOffset + (float)Math.Min(8, f.toolPower.Value * 4))), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
									return;
								case 1:
									if (f.FacingDirection == 0)
									{
										Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset + 4f, fPosition.Y - 128f + 40f + (float)toolYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
										return;
									}
									Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset - 12f, fPosition.Y - 128f + 32f + (float)toolYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, -0.1308997f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
									return;
								case 2:
									Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset, fPosition.Y - 128f + 64f + (float)toolYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
									return;
								case 3:
									if (f.FacingDirection != 0)
									{
										Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset, fPosition.Y - 64f + 44f + (float)toolYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
										return;
									}
									break;
								case 4:
									if (f.FacingDirection != 0)
									{
										Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset, fPosition.Y - 64f + 48f + (float)toolYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
										return;
									}
									break;
								case 5:
									Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset, fPosition.Y - 64f + 32f + (float)toolYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
									break;
								default:
									return;
								}
							}
							else
							{
								switch (farmerSprite.currentAnimationIndex)
								{
								case 0:
								case 1:
									Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset, fPosition.Y - 128f + 16f + (float)toolYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
									return;
								case 2:
									Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset, fPosition.Y - 128f - (float)((f.FacingDirection == 2) ? -4 : 32) + (float)toolYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
									return;
								case 3:
									if (f.FacingDirection == 2)
									{
										sourceRectangleForTool.X += 16;
									}
									Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset - (float)((f.FacingDirection == 2) ? 4 : 0), fPosition.Y - 128f - (float)((f.FacingDirection == 2) ? -24 : 64) + (float)toolYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
									return;
								default:
									return;
								}
							}
						}
						else
						{
							if (farmerSprite.currentAnimationIndex <= 2)
							{
								Point tileLocation2 = f.TilePoint;
								tileLocation2.Y--;
								if (f.currentLocation.hasTileAt(tileLocation2, "Front", null))
								{
									return;
								}
							}
							if (f.FacingDirection == 2)
							{
								layerDepth += 0.01f;
							}
							Color color = fishingRod.getColor();
							switch (farmerSprite.currentAnimationIndex)
							{
							case 0:
								if (!fishingRod.showingTreasure && !fishingRod.fishCaught && (f.FacingDirection != 0 || !fishingRod.isFishing || fishingRod.isReeling))
								{
									Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f, fPosition.Y - 128f + 4f)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), color, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
									return;
								}
								break;
							case 1:
								Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f, fPosition.Y - 128f + 4f)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), color, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
								return;
							case 2:
								Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f, fPosition.Y - 128f + 4f)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), color, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
								return;
							case 3:
								if (f.FacingDirection == 2)
								{
									Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f, fPosition.Y - 128f + 4f)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), color, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
									return;
								}
								break;
							case 4:
								if (f.FacingDirection == 0 && fishingRod.isFishing)
								{
									Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 80f, fPosition.Y - 96f)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), color, 0f, Vector2.Zero, 4f, SpriteEffects.FlipVertically, layerDepth);
									return;
								}
								if (f.FacingDirection == 2)
								{
									Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f, fPosition.Y - 128f + 4f)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), color, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
									return;
								}
								break;
							case 5:
								if (f.FacingDirection == 2 && !fishingRod.showingTreasure && !fishingRod.fishCaught)
								{
									Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f, fPosition.Y - 128f + 4f)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), color, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
									return;
								}
								break;
							default:
								return;
							}
						}
					}
					else
					{
						if (farmerSprite.currentAnimationIndex > 2)
						{
							Point tileLocation3 = f.TilePoint;
							tileLocation3.X--;
							tileLocation3.Y--;
							if (!(f.CurrentTool is WateringCan) && f.currentLocation.hasTileAt(tileLocation3, "Front", null) && f.Position.Y % 64f < 32f)
							{
								return;
							}
							tileLocation3.Y++;
						}
						currentTool = f.CurrentTool;
						FishingRod rod3 = currentTool as FishingRod;
						if (rod3 == null)
						{
							if (!(currentTool is WateringCan))
							{
								switch (farmerSprite.currentAnimationIndex)
								{
								case 0:
									Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + 32f + 8f + (float)toolXOffset + (float)Math.Min(8, f.toolPower.Value * 4), fPosition.Y - 128f + 8f + (float)toolYOffset + (float)Math.Min(8, f.toolPower.Value * 4))), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, 0.2617994f + (float)Math.Min(f.toolPower.Value, 2) * 0.049087387f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, layerDepth);
									return;
								case 1:
									Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 16f + (float)toolXOffset, fPosition.Y - 128f + 16f + (float)toolYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, -0.2617994f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, layerDepth);
									return;
								case 2:
									Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f + 4f + (float)toolXOffset, fPosition.Y - 128f + 60f + (float)toolYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, -0.7853982f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, layerDepth);
									return;
								case 3:
									Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f + 20f + (float)toolXOffset, fPosition.Y - 64f + 76f + (float)toolYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, -1.8325958f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, layerDepth);
									return;
								case 4:
									Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f + 24f + (float)toolXOffset, fPosition.Y + 24f + (float)toolYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, -1.8325958f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, layerDepth);
									return;
								default:
									return;
								}
							}
							else
							{
								if (farmerSprite.currentAnimationIndex == 1)
								{
									Point tileLocation4 = f.TilePoint;
									tileLocation4.X--;
									tileLocation4.Y--;
									if (f.currentLocation.hasTileAt(tileLocation4, "Front", null) && f.Position.Y % 64f < 32f)
									{
										return;
									}
								}
								switch (farmerSprite.currentAnimationIndex)
								{
								case 0:
								case 1:
									Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset - 4f, fPosition.Y - 128f + 8f + (float)toolYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, layerDepth);
									return;
								case 2:
									Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset - 16f, fPosition.Y - 128f + (float)toolYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, -0.2617994f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, layerDepth);
									return;
								case 3:
									sourceRectangleForTool.X += 16;
									Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + (float)toolXOffset - 16f, fPosition.Y - 128f - 24f + (float)toolYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.FlipHorizontally, layerDepth);
									return;
								default:
									return;
								}
							}
						}
						else
						{
							Color color2 = rod3.getColor();
							switch (farmerSprite.currentAnimationIndex)
							{
							case 0:
								if (rod3.isReeling || rod3.isFishing || rod3.doneWithAnimation || !rod3.hasDoneFucntionYet || rod3.pullingOutOfWater)
								{
									Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + (float)toolYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), color2, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, layerDepth);
									return;
								}
								break;
							case 1:
								Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + 8f + (float)toolYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), color2, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, layerDepth);
								return;
							case 2:
								Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 96f + 32f + (float)toolXOffset, fPosition.Y - 128f - 24f + (float)toolYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), color2, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, layerDepth);
								return;
							case 3:
								Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 96f + 24f + (float)toolXOffset, fPosition.Y - 128f - 32f + (float)toolYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), color2, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, layerDepth);
								return;
							case 4:
								if (rod3.isFishing || rod3.doneWithAnimation)
								{
									Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + (float)toolYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), color2, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, layerDepth);
									return;
								}
								Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + 4f + (float)toolYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), color2, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, layerDepth);
								return;
							case 5:
								Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + (float)toolYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), color2, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, layerDepth);
								return;
							default:
								return;
							}
						}
					}
				}
				else
				{
					if (farmerSprite.currentAnimationIndex > 2)
					{
						Point tileLocation5 = f.TilePoint;
						tileLocation5.X++;
						tileLocation5.Y--;
						if (!(f.CurrentTool is WateringCan) && f.currentLocation.hasTileAt(tileLocation5, "Front", null))
						{
							return;
						}
						tileLocation5.Y++;
					}
					currentTool = f.CurrentTool;
					FishingRod rod4 = currentTool as FishingRod;
					if (rod4 == null)
					{
						if (!(currentTool is WateringCan))
						{
							switch (farmerSprite.currentAnimationIndex)
							{
							case 0:
								Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X - 32f - 4f + (float)toolXOffset - (float)Math.Min(8, f.toolPower.Value * 4), fPosition.Y - 128f + 24f + (float)toolYOffset + (float)Math.Min(8, f.toolPower.Value * 4))), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, -0.2617994f - (float)Math.Min(f.toolPower.Value, 2) * 0.049087387f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
								return;
							case 1:
								Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + 32f - 24f + (float)toolXOffset, fPosition.Y - 124f + (float)toolYOffset + 64f)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, 0.2617994f, new Vector2(0f, 32f), 4f, SpriteEffects.None, layerDepth);
								return;
							case 2:
								Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + 32f + (float)toolXOffset - 4f, fPosition.Y - 132f + (float)toolYOffset + 64f)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, 0.7853982f, new Vector2(0f, 32f), 4f, SpriteEffects.None, layerDepth);
								return;
							case 3:
								Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + 32f + 28f + (float)toolXOffset, fPosition.Y - 64f + (float)toolYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, 1.8325958f, new Vector2(0f, 32f), 4f, SpriteEffects.None, layerDepth);
								return;
							case 4:
								Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + 32f + 28f + (float)toolXOffset, fPosition.Y - 64f + 4f + (float)toolYOffset)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, 1.8325958f, new Vector2(0f, 32f), 4f, SpriteEffects.None, layerDepth);
								return;
							case 5:
								Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + 64f + 12f + (float)toolXOffset, fPosition.Y - 128f + 32f + (float)toolYOffset + 128f)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, 0.7853982f, new Vector2(0f, 32f), 4f, SpriteEffects.None, layerDepth);
								return;
							case 6:
								Game1.spriteBatch.Draw(spritesheet, Utility.snapToInt(new Vector2(fPosition.X + 42f + 8f + (float)toolXOffset, fPosition.Y - 64f + 24f + (float)toolYOffset + 128f)), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, 0f, new Vector2(0f, 128f), 4f, SpriteEffects.None, layerDepth);
								return;
							default:
								return;
							}
						}
						else
						{
							if (farmerSprite.currentAnimationIndex == 1)
							{
								Point tileLocation6 = f.TilePoint;
								tileLocation6.X--;
								tileLocation6.Y--;
								if (f.currentLocation.hasTileAt(tileLocation6, "Front", null) && f.Position.Y % 64f < 32f)
								{
									return;
								}
							}
							switch (farmerSprite.currentAnimationIndex)
							{
							case 0:
							case 1:
								Game1.spriteBatch.Draw(spritesheet, new Vector2((float)((int)(fPosition.X + (float)toolXOffset - 4f)), (float)((int)(fPosition.Y - 128f + 8f + (float)toolYOffset))), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
								return;
							case 2:
								Game1.spriteBatch.Draw(spritesheet, new Vector2((float)((int)fPosition.X + toolXOffset + 24), (float)((int)(fPosition.Y - 128f - 8f + (float)toolYOffset))), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, 0.2617994f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
								return;
							case 3:
								sourceRectangleForTool.X += 16;
								Game1.spriteBatch.Draw(spritesheet, new Vector2((float)((int)(fPosition.X + (float)toolXOffset + 8f)), (float)((int)(fPosition.Y - 128f - 24f + (float)toolYOffset))), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, layerDepth);
								return;
							default:
								return;
							}
						}
					}
					else
					{
						Color color3 = rod4.getColor();
						switch (farmerSprite.currentAnimationIndex)
						{
						case 0:
							if (rod4.isReeling || rod4.isFishing || rod4.doneWithAnimation || !rod4.hasDoneFucntionYet || rod4.pullingOutOfWater)
							{
								Game1.spriteBatch.Draw(spritesheet, new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + (float)toolYOffset), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), color3, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
								return;
							}
							break;
						case 1:
							Game1.spriteBatch.Draw(spritesheet, new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + 8f + (float)toolYOffset), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), color3, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
							return;
						case 2:
							Game1.spriteBatch.Draw(spritesheet, new Vector2(fPosition.X - 96f + 32f + (float)toolXOffset, fPosition.Y - 128f - 24f + (float)toolYOffset), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), color3, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
							return;
						case 3:
							Game1.spriteBatch.Draw(spritesheet, new Vector2(fPosition.X - 96f + 24f + (float)toolXOffset, fPosition.Y - 128f - 32f + (float)toolYOffset), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), color3, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
							return;
						case 4:
							if (rod4.isFishing || rod4.doneWithAnimation)
							{
								Game1.spriteBatch.Draw(spritesheet, new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + (float)toolYOffset), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), color3, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
								return;
							}
							Game1.spriteBatch.Draw(spritesheet, new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + 4f + (float)toolYOffset), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), color3, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
							return;
						case 5:
							Game1.spriteBatch.Draw(spritesheet, new Vector2(fPosition.X - 64f + (float)toolXOffset, fPosition.Y - 160f + (float)toolYOffset), new Microsoft.Xna.Framework.Rectangle?(sourceRectangleForTool), color3, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
							return;
						default:
							return;
						}
					}
				}
				return;
			}
			f.CurrentTool.draw(Game1.spriteBatch);
		}

		// Token: 0x06000C3C RID: 3132 RVA: 0x0008A6B1 File Offset: 0x000888B1
		public static Vector2 GlobalToLocal(xTile.Dimensions.Rectangle viewport, Vector2 globalPosition)
		{
			return new Vector2(globalPosition.X - (float)viewport.X, globalPosition.Y - (float)viewport.Y);
		}

		// Token: 0x06000C3D RID: 3133 RVA: 0x0008A6D6 File Offset: 0x000888D6
		public static bool IsEnglish()
		{
			return Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en;
		}

		// Token: 0x06000C3E RID: 3134 RVA: 0x0008A6E5 File Offset: 0x000888E5
		public static Vector2 GlobalToLocal(Vector2 globalPosition)
		{
			return new Vector2(globalPosition.X - (float)Game1.viewport.X, globalPosition.Y - (float)Game1.viewport.Y);
		}

		// Token: 0x06000C3F RID: 3135 RVA: 0x0008A710 File Offset: 0x00088910
		public static Microsoft.Xna.Framework.Rectangle GlobalToLocal(xTile.Dimensions.Rectangle viewport, Microsoft.Xna.Framework.Rectangle globalPosition)
		{
			return new Microsoft.Xna.Framework.Rectangle(globalPosition.X - viewport.X, globalPosition.Y - viewport.Y, globalPosition.Width, globalPosition.Height);
		}

		// Token: 0x06000C40 RID: 3136 RVA: 0x0008A740 File Offset: 0x00088940
		public static string parseText(string text, SpriteFont whichFont, int width)
		{
			if (text == null)
			{
				return "";
			}
			text = Dialogue.applyGenderSwitchBlocks(Game1.player.Gender, text);
			Game1._ParseTextStringBuilder.Clear();
			Game1._ParseTextStringBuilderLine.Clear();
			Game1._ParseTextStringBuilderWord.Clear();
			float current_width = 0f;
			LocalizedContentManager.LanguageCode currentLanguageCode = LocalizedContentManager.CurrentLanguageCode;
			if (currentLanguageCode == LocalizedContentManager.LanguageCode.ja || currentLanguageCode == LocalizedContentManager.LanguageCode.zh || currentLanguageCode == LocalizedContentManager.LanguageCode.th)
			{
				foreach (object obj in Game1.asianSpacingRegex.Matches(text))
				{
					string s = obj.ToString();
					float character_width = whichFont.MeasureString(s).X + whichFont.Spacing;
					if (current_width + character_width > (float)width || s.Equals(Environment.NewLine) || s.Equals("\n"))
					{
						Game1._ParseTextStringBuilder.Append(Game1._ParseTextStringBuilderLine);
						Game1._ParseTextStringBuilder.Append(Environment.NewLine);
						Game1._ParseTextStringBuilderLine.Clear();
						current_width = 0f;
					}
					if (!s.Equals(Environment.NewLine) && !s.Equals("\n"))
					{
						Game1._ParseTextStringBuilderLine.Append(s);
						current_width += character_width;
					}
				}
				Game1._ParseTextStringBuilder.Append(Game1._ParseTextStringBuilderLine);
				return Game1._ParseTextStringBuilder.ToString();
			}
			current_width = 0f;
			int i = 0;
			while (i < text.Length)
			{
				char c = text[i];
				bool check_width;
				if (c == '\n')
				{
					check_width = true;
					goto IL_19A;
				}
				if (c != '\r')
				{
					if (c == ' ')
					{
						check_width = true;
						goto IL_19A;
					}
					Game1._ParseTextStringBuilderWord.Append(c);
					check_width = (i == text.Length - 1);
					goto IL_19A;
				}
				IL_2AA:
				i++;
				continue;
				IL_19A:
				if (check_width)
				{
					try
					{
						float word_width = whichFont.MeasureString(Game1._ParseTextStringBuilderWord).X + whichFont.Spacing;
						if (current_width + word_width > (float)width)
						{
							Game1._ParseTextStringBuilder.Append(Game1._ParseTextStringBuilderLine);
							Game1._ParseTextStringBuilder.Append(Environment.NewLine);
							Game1._ParseTextStringBuilderLine.Clear();
							current_width = 0f;
						}
						if (c == '\n')
						{
							Game1._ParseTextStringBuilderLine.Append(Game1._ParseTextStringBuilderWord);
							Game1._ParseTextStringBuilder.Append(Game1._ParseTextStringBuilderLine);
							Game1._ParseTextStringBuilder.Append(Environment.NewLine);
							Game1._ParseTextStringBuilderLine.Clear();
							Game1._ParseTextStringBuilderWord.Clear();
							current_width = 0f;
							goto IL_2AA;
						}
						Game1._ParseTextStringBuilderLine.Append(Game1._ParseTextStringBuilderWord);
						Game1._ParseTextStringBuilderLine.Append(" ");
						float space_width = whichFont.MeasureString(" ").X + whichFont.Spacing;
						current_width += word_width + space_width;
					}
					catch (Exception e)
					{
						Game1.log.Error("Exception measuring string: ", e);
					}
					Game1._ParseTextStringBuilderWord.Clear();
					goto IL_2AA;
				}
				goto IL_2AA;
			}
			Game1._ParseTextStringBuilderLine.Append(Game1._ParseTextStringBuilderWord);
			Game1._ParseTextStringBuilder.Append(Game1._ParseTextStringBuilderLine);
			return Game1._ParseTextStringBuilder.ToString();
		}

		// Token: 0x06000C41 RID: 3137 RVA: 0x0008AA50 File Offset: 0x00088C50
		public static void UpdateHorseOwnership()
		{
			bool verbose = false;
			Dictionary<long, Horse> horse_lookup = new Dictionary<long, Horse>();
			HashSet<Horse> claimed_horses = new HashSet<Horse>();
			List<Stable> stables = new List<Stable>();
			Utility.ForEachBuilding<Stable>(delegate(Stable stable)
			{
				stables.Add(stable);
				return true;
			}, true);
			foreach (Stable stable6 in stables)
			{
				if (stable6.owner.Value == -6666666L && Game1.GetPlayer(-6666666L, false) == null)
				{
					stable6.owner.Value = Game1.player.UniqueMultiplayerID;
				}
				stable6.grabHorse();
			}
			foreach (Stable stable5 in stables)
			{
				Horse horse = stable5.getStableHorse();
				if (horse != null && !claimed_horses.Contains(horse) && horse.getOwner() != null && !horse_lookup.ContainsKey(horse.getOwner().UniqueMultiplayerID) && horse.getOwner().horseName.Value != null && horse.getOwner().horseName.Value.Length > 0 && horse.Name == horse.getOwner().horseName.Value)
				{
					horse_lookup[horse.getOwner().UniqueMultiplayerID] = horse;
					claimed_horses.Add(horse);
					if (verbose)
					{
						Game1.log.Verbose(string.Concat(new string[]
						{
							"Assigned horse ",
							horse.Name,
							" to ",
							horse.getOwner().Name,
							" (Exact match)"
						}));
					}
				}
			}
			Dictionary<string, Farmer> horse_name_lookup = new Dictionary<string, Farmer>();
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				if (!string.IsNullOrEmpty((farmer != null) ? farmer.horseName.Value : null))
				{
					bool fail = false;
					using (HashSet<Horse>.Enumerator enumerator3 = claimed_horses.GetEnumerator())
					{
						while (enumerator3.MoveNext())
						{
							if (enumerator3.Current.getOwner() == farmer)
							{
								fail = true;
								break;
							}
						}
					}
					if (!fail)
					{
						horse_name_lookup[farmer.horseName.Value] = farmer;
					}
				}
			}
			foreach (Stable stable2 in stables)
			{
				Horse horse2 = stable2.getStableHorse();
				Farmer owner;
				if (horse2 != null && !claimed_horses.Contains(horse2) && horse2.getOwner() != null && horse2.Name != null && horse2.Name.Length > 0 && horse_name_lookup.TryGetValue(horse2.Name, out owner) && !horse_lookup.ContainsKey(owner.UniqueMultiplayerID))
				{
					stable2.owner.Value = owner.UniqueMultiplayerID;
					stable2.updateHorseOwnership();
					horse_lookup[horse2.getOwner().UniqueMultiplayerID] = horse2;
					claimed_horses.Add(horse2);
					if (verbose)
					{
						Game1.log.Verbose(string.Concat(new string[]
						{
							"Assigned horse ",
							horse2.Name,
							" to ",
							horse2.getOwner().Name,
							" (Name match from different owner.)"
						}));
					}
				}
			}
			foreach (Stable stable3 in stables)
			{
				Horse horse3 = stable3.getStableHorse();
				if (horse3 != null && !claimed_horses.Contains(horse3) && horse3.getOwner() != null && !horse_lookup.ContainsKey(horse3.getOwner().UniqueMultiplayerID))
				{
					horse_lookup[horse3.getOwner().UniqueMultiplayerID] = horse3;
					claimed_horses.Add(horse3);
					stable3.updateHorseOwnership();
					if (verbose)
					{
						Game1.log.Verbose(string.Concat(new string[]
						{
							"Assigned horse ",
							horse3.Name,
							" to ",
							horse3.getOwner().Name,
							" (Owner's only stable)"
						}));
					}
				}
			}
			foreach (Stable stable4 in stables)
			{
				Horse horse4 = stable4.getStableHorse();
				if (horse4 != null && !claimed_horses.Contains(horse4))
				{
					foreach (Horse claimed_horse in claimed_horses)
					{
						if (horse4.ownerId == claimed_horse.ownerId)
						{
							stable4.owner.Value = 0L;
							stable4.updateHorseOwnership();
							if (verbose)
							{
								Game1.log.Verbose("Unassigned horse (stable owner already has a horse).");
								break;
							}
							break;
						}
					}
				}
			}
		}

		// Token: 0x06000C42 RID: 3138 RVA: 0x0008B050 File Offset: 0x00089250
		public static string LoadStringByGender(Gender npcGender, string key)
		{
			if (npcGender == Gender.Male)
			{
				return Game1.content.LoadString(key).Split('/', StringSplitOptions.None)[0];
			}
			return Game1.content.LoadString(key).Split('/', StringSplitOptions.None).Last<string>();
		}

		// Token: 0x06000C43 RID: 3139 RVA: 0x0008B084 File Offset: 0x00089284
		public static string LoadStringByGender(Gender npcGender, string key, params object[] substitutions)
		{
			string sentence;
			if (npcGender == Gender.Male)
			{
				sentence = Game1.content.LoadString(key).Split('/', StringSplitOptions.None)[0];
				if (substitutions.Length != 0)
				{
					try
					{
						return string.Format(sentence, substitutions);
					}
					catch
					{
						return sentence;
					}
				}
			}
			sentence = Game1.content.LoadString(key).Split('/', StringSplitOptions.None).Last<string>();
			if (substitutions.Length != 0)
			{
				try
				{
					return string.Format(sentence, substitutions);
				}
				catch
				{
					return sentence;
				}
			}
			return sentence;
		}

		// Token: 0x06000C44 RID: 3140 RVA: 0x0008B108 File Offset: 0x00089308
		public static string parseText(string text)
		{
			return Game1.parseText(text, Game1.dialogueFont, Game1.dialogueWidth);
		}

		// Token: 0x06000C45 RID: 3141 RVA: 0x0008B11A File Offset: 0x0008931A
		public static Microsoft.Xna.Framework.Rectangle getSourceRectForStandardTileSheet(Texture2D tileSheet, int tilePosition, int width = -1, int height = -1)
		{
			if (width == -1)
			{
				width = 64;
			}
			if (height == -1)
			{
				height = 64;
			}
			return new Microsoft.Xna.Framework.Rectangle(tilePosition * width % tileSheet.Width, tilePosition * width / tileSheet.Width * height, width, height);
		}

		// Token: 0x06000C46 RID: 3142 RVA: 0x0008B149 File Offset: 0x00089349
		public static Microsoft.Xna.Framework.Rectangle getSquareSourceRectForNonStandardTileSheet(Texture2D tileSheet, int tileWidth, int tileHeight, int tilePosition)
		{
			return new Microsoft.Xna.Framework.Rectangle(tilePosition * tileWidth % tileSheet.Width, tilePosition * tileWidth / tileSheet.Width * tileHeight, tileWidth, tileHeight);
		}

		// Token: 0x06000C47 RID: 3143 RVA: 0x0008B168 File Offset: 0x00089368
		public static Microsoft.Xna.Framework.Rectangle getArbitrarySourceRect(Texture2D tileSheet, int tileWidth, int tileHeight, int tilePosition)
		{
			if (tileSheet != null)
			{
				return new Microsoft.Xna.Framework.Rectangle(tilePosition * tileWidth % tileSheet.Width, tilePosition * tileWidth / tileSheet.Width * tileHeight, tileWidth, tileHeight);
			}
			return Microsoft.Xna.Framework.Rectangle.Empty;
		}

		// Token: 0x06000C48 RID: 3144 RVA: 0x0008B190 File Offset: 0x00089390
		public static string getTimeOfDayString(int time)
		{
			string zeroPad = (time % 100 == 0) ? "0" : string.Empty;
			string hours;
			switch (LocalizedContentManager.CurrentLanguageCode)
			{
			default:
				hours = ((time / 100 % 12 == 0) ? "12" : (time / 100 % 12).ToString());
				break;
			case LocalizedContentManager.LanguageCode.ja:
				hours = ((time / 100 % 12 == 0) ? "0" : (time / 100 % 12).ToString());
				break;
			case LocalizedContentManager.LanguageCode.ru:
			case LocalizedContentManager.LanguageCode.pt:
			case LocalizedContentManager.LanguageCode.es:
			case LocalizedContentManager.LanguageCode.de:
			case LocalizedContentManager.LanguageCode.th:
			case LocalizedContentManager.LanguageCode.fr:
			case LocalizedContentManager.LanguageCode.tr:
			case LocalizedContentManager.LanguageCode.hu:
				hours = (time / 100 % 24).ToString();
				hours = ((time / 100 % 24 <= 9) ? ("0" + hours) : hours);
				break;
			case LocalizedContentManager.LanguageCode.zh:
				hours = (time / 100 % 24).ToString();
				break;
			}
			string timeText = string.Concat(new object[]
			{
				hours,
				":",
				time % 100,
				zeroPad
			});
			LocalizedContentManager.LanguageCode currentLanguageCode = LocalizedContentManager.CurrentLanguageCode;
			if (currentLanguageCode <= LocalizedContentManager.LanguageCode.ja)
			{
				if (currentLanguageCode == LocalizedContentManager.LanguageCode.en)
				{
					return timeText + " " + ((time < 1200 || time >= 2400) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10370") : Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10371"));
				}
				if (currentLanguageCode == LocalizedContentManager.LanguageCode.ja)
				{
					if (time >= 1200 && time < 2400)
					{
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10371") + " " + timeText;
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10370") + " " + timeText;
				}
			}
			else if (currentLanguageCode != LocalizedContentManager.LanguageCode.fr)
			{
				if (currentLanguageCode == LocalizedContentManager.LanguageCode.mod)
				{
					return LocalizedContentManager.FormatTimeString(time, LocalizedContentManager.CurrentModLanguage.TimeFormat).ToString();
				}
			}
			else
			{
				if (time % 100 != 0)
				{
					return hours + "h" + (time % 100).ToString();
				}
				return hours + "h";
			}
			return timeText;
		}

		// Token: 0x06000C49 RID: 3145 RVA: 0x0008B38C File Offset: 0x0008958C
		public static bool[,] getCircleOutlineGrid(int radius)
		{
			bool[,] circleGrid = new bool[radius * 2 + 1, radius * 2 + 1];
			int f = 1 - radius;
			int ddF_x = 1;
			int ddF_y = -2 * radius;
			int x = 0;
			int y = radius;
			circleGrid[radius, radius + radius] = true;
			circleGrid[radius, radius - radius] = true;
			circleGrid[radius + radius, radius] = true;
			circleGrid[radius - radius, radius] = true;
			while (x < y)
			{
				if (f >= 0)
				{
					y--;
					ddF_y += 2;
					f += ddF_y;
				}
				x++;
				ddF_x += 2;
				f += ddF_x;
				circleGrid[radius + x, radius + y] = true;
				circleGrid[radius - x, radius + y] = true;
				circleGrid[radius + x, radius - y] = true;
				circleGrid[radius - x, radius - y] = true;
				circleGrid[radius + y, radius + x] = true;
				circleGrid[radius - y, radius + x] = true;
				circleGrid[radius + y, radius - x] = true;
				circleGrid[radius - y, radius - x] = true;
			}
			return circleGrid;
		}

		// Token: 0x06000C4A RID: 3146 RVA: 0x0008B4AB File Offset: 0x000896AB
		public static string GetFarmTypeID()
		{
			if (Game1.whichFarm != 7 || Game1.whichModFarm == null)
			{
				return Game1.whichFarm.ToString();
			}
			return Game1.whichModFarm.Id;
		}

		// Token: 0x06000C4B RID: 3147 RVA: 0x0008B4D4 File Offset: 0x000896D4
		public static string GetFarmTypeKey()
		{
			switch (Game1.whichFarm)
			{
			case 0:
				return "Standard";
			case 1:
				return "Riverland";
			case 2:
				return "Forest";
			case 3:
				return "Hilltop";
			case 4:
				return "Wilderness";
			case 5:
				return "FourCorners";
			case 6:
				return "Beach";
			default:
				return Game1.GetFarmTypeID();
			}
		}

		// Token: 0x06000C4C RID: 3148 RVA: 0x0008B53C File Offset: 0x0008973C
		public void _PerformRemoveNormalItemFromWorldOvernight(string itemId)
		{
			Utility.ForEachLocation(delegate(GameLocation location)
			{
				this._RecursiveRemoveThisNormalItemLocation(location, itemId);
				return true;
			}, true, true);
			Game1.player.team.returnedDonations.RemoveWhere((Item item) => this._RecursiveRemoveThisNormalItemItem(item, itemId));
			Predicate<Item> <>9__2;
			foreach (IList<Item> list in Game1.player.team.globalInventories.Values)
			{
				Predicate<Item> match;
				if ((match = <>9__2) == null)
				{
					match = (<>9__2 = ((Item item) => this._RecursiveRemoveThisNormalItemItem(item, itemId)));
				}
				list.RemoveWhere(match);
			}
			foreach (SpecialOrder order in Game1.player.team.specialOrders)
			{
				for (int i = 0; i < order.donatedItems.Count; i++)
				{
					Item item2 = order.donatedItems[i];
					if (this._RecursiveRemoveThisNormalItemItem(item2, itemId))
					{
						order.donatedItems[i] = null;
					}
				}
			}
		}

		// Token: 0x06000C4D RID: 3149 RVA: 0x0008B698 File Offset: 0x00089898
		protected virtual void _PerformRemoveNormalItemFromFarmerOvernight(Farmer farmer, string itemId)
		{
			for (int i = 0; i < farmer.Items.Count; i++)
			{
				if (this._RecursiveRemoveThisNormalItemItem(farmer.Items[i], itemId))
				{
					farmer.Items[i] = null;
				}
			}
			farmer.itemsLostLastDeath.RemoveWhere((Item item) => this._RecursiveRemoveThisNormalItemItem(item, itemId));
			if (farmer.recoveredItem != null && this._RecursiveRemoveThisNormalItemItem(farmer.recoveredItem, itemId))
			{
				farmer.recoveredItem = null;
				farmer.mailbox.Remove("MarlonRecovery");
				farmer.mailForTomorrow.Remove("MarlonRecovery");
			}
			if (farmer.toolBeingUpgraded.Value != null && this._RecursiveRemoveThisNormalItemItem(farmer.toolBeingUpgraded.Value, itemId))
			{
				farmer.toolBeingUpgraded.Value = null;
			}
		}

		// Token: 0x06000C4E RID: 3150 RVA: 0x0008B788 File Offset: 0x00089988
		protected virtual bool _RecursiveRemoveThisNormalItemItem(Item this_item, string itemId)
		{
			if (this_item != null)
			{
				Object o = this_item as Object;
				if (o != null)
				{
					if (o.heldObject.Value != null && this._RecursiveRemoveThisNormalItemItem(o.heldObject.Value, itemId))
					{
						o.ResetParentSheetIndex();
						o.heldObject.Value = null;
						o.readyForHarvest.Value = false;
						o.showNextIndex.Value = false;
					}
					StorageFurniture furniture = o as StorageFurniture;
					if (furniture == null)
					{
						IndoorPot pot = o as IndoorPot;
						if (pot == null)
						{
							Chest chest = o as Chest;
							if (chest != null)
							{
								bool removed_item = false;
								IInventory items = chest.Items;
								for (int i = 0; i < items.Count; i++)
								{
									Item item = items[i];
									if (item != null && this._RecursiveRemoveThisNormalItemItem(item, itemId))
									{
										items[i] = null;
										removed_item = true;
									}
								}
								if (removed_item)
								{
									chest.clearNulls();
								}
							}
						}
						else if (pot.hoeDirt.Value != null)
						{
							this._RecursiveRemoveThisNormalItemDirt(pot.hoeDirt.Value, null, Vector2.Zero, itemId);
						}
					}
					else
					{
						bool removed_item2 = false;
						for (int j = 0; j < furniture.heldItems.Count; j++)
						{
							Item item2 = furniture.heldItems[j];
							if (item2 != null && this._RecursiveRemoveThisNormalItemItem(item2, itemId))
							{
								furniture.heldItems[j] = null;
								removed_item2 = true;
							}
						}
						if (removed_item2)
						{
							furniture.ClearNulls();
						}
					}
					if (o.heldObject.Value != null && this._RecursiveRemoveThisNormalItemItem(o.heldObject.Value, itemId))
					{
						o.heldObject.Value = null;
					}
				}
				return Utility.IsNormalObjectAtParentSheetIndex(this_item, itemId);
			}
			return false;
		}

		// Token: 0x06000C4F RID: 3151 RVA: 0x0008B922 File Offset: 0x00089B22
		protected virtual void _RecursiveRemoveThisNormalItemDirt(HoeDirt dirt, GameLocation location, Vector2 coord, string itemId)
		{
			if (dirt.crop != null && dirt.crop.indexOfHarvest.Value == itemId)
			{
				dirt.destroyCrop(false);
			}
		}

		// Token: 0x06000C50 RID: 3152 RVA: 0x0008B94C File Offset: 0x00089B4C
		protected virtual void _RecursiveRemoveThisNormalItemLocation(GameLocation l, string itemId)
		{
			if (l == null)
			{
				return;
			}
			List<Guid> removed_items = new List<Guid>();
			foreach (Furniture furniture in l.furniture)
			{
				if (this._RecursiveRemoveThisNormalItemItem(furniture, itemId))
				{
					removed_items.Add(l.furniture.GuidOf(furniture));
				}
			}
			foreach (Guid guid in removed_items)
			{
				l.furniture.Remove(guid);
			}
			Func<string, bool> <>9__3;
			foreach (NPC npc in l.characters)
			{
				Monster monster = npc as Monster;
				if (monster != null)
				{
					NetStringList objectsToDrop = monster.objectsToDrop;
					if (objectsToDrop != null)
					{
						Func<string, bool> match;
						if ((match = <>9__3) == null)
						{
							match = (<>9__3 = ((string id) => id == itemId));
						}
						objectsToDrop.RemoveWhere(match);
					}
				}
			}
			Chest fridge = l.GetFridge(false);
			if (fridge != null)
			{
				IInventory fridgeItems = fridge.Items;
				for (int i = 0; i < fridgeItems.Count; i++)
				{
					Item item3 = fridgeItems[i];
					if (item3 != null && this._RecursiveRemoveThisNormalItemItem(item3, itemId))
					{
						fridgeItems[i] = null;
					}
				}
			}
			foreach (Vector2 coord in l.terrainFeatures.Keys)
			{
				HoeDirt dirt = l.terrainFeatures[coord] as HoeDirt;
				if (dirt != null)
				{
					this._RecursiveRemoveThisNormalItemDirt(dirt, l, coord, itemId);
				}
			}
			foreach (Building building in l.buildings)
			{
				foreach (Chest chest in building.buildingChests)
				{
					bool anyRemoved = false;
					for (int j = 0; j < chest.Items.Count; j++)
					{
						Item item2 = chest.Items[j];
						if (item2 != null && this._RecursiveRemoveThisNormalItemItem(item2, itemId))
						{
							chest.Items[j] = null;
							anyRemoved = true;
						}
					}
					if (anyRemoved)
					{
						chest.clearNulls();
					}
				}
			}
			foreach (Vector2 key in l.objects.Keys.ToArray<Vector2>())
			{
				Object obj = l.objects[key];
				if (obj != fridge && this._RecursiveRemoveThisNormalItemItem(obj, itemId))
				{
					l.objects.Remove(key);
				}
			}
			l.debris.RemoveWhere((Debris debris) => debris.item != null && this._RecursiveRemoveThisNormalItemItem(debris.item, itemId));
			ShopLocation shopLocation = l as ShopLocation;
			if (shopLocation != null)
			{
				shopLocation.itemsFromPlayerToSell.RemoveWhere((Item item) => this._RecursiveRemoveThisNormalItemItem(item, itemId));
				shopLocation.itemsToStartSellingTomorrow.RemoveWhere((Item item) => this._RecursiveRemoveThisNormalItemItem(item, itemId));
			}
		}

		// Token: 0x06000C51 RID: 3153 RVA: 0x0008BCEC File Offset: 0x00089EEC
		public static bool GetHasRoomAnotherFarm()
		{
			return true;
		}

		// Token: 0x06000C52 RID: 3154 RVA: 0x0008BCF0 File Offset: 0x00089EF0
		public virtual void ResetGameStateOnTitleScreen()
		{
			LocalizedContentManager.localizedAssetNames.Clear();
			Event.invalidFestivals.Clear();
			NPC.invalidDialogueFiles.Clear();
			SaveGame.CancelToTitle = false;
			Game1.overlayMenu = null;
			Game1.multiplayer.cachedMultiplayerMaps.Clear();
			Game1.keyboardFocusInstance = null;
			BuildingPaintMenu.savedColors = null;
			Game1.startingGameSeed = null;
			Game1.UseLegacyRandom = false;
			Game1._afterNewDayAction = null;
			Game1._currentMinigame = null;
			Game1.gameMode = 0;
			this._isSaving = false;
			Game1._mouseCursorTransparency = 1f;
			Game1._newDayTask = null;
			Game1.newDaySync.destroy();
			Game1.netReady.Reset();
			Game1.dedicatedServer.Reset();
			Game1.resetPlayer();
			Game1.afterDialogues = null;
			Game1.afterFade = null;
			Game1.afterPause = null;
			Game1.afterViewport = null;
			Game1.ambientLight = new Color(0, 0, 0, 0);
			Game1.background = null;
			Game1.chatBox = null;
			SpecialCurrencyDisplay specialCurrencyDisplay = Game1.specialCurrencyDisplay;
			if (specialCurrencyDisplay != null)
			{
				specialCurrencyDisplay.Cleanup();
			}
			GameLocation.PlayedNewLocationContextMusic = false;
			Game1.IsPlayingBackgroundMusic = false;
			Game1.IsPlayingNightAmbience = false;
			Game1.IsPlayingOutdoorsAmbience = false;
			Game1.IsPlayingMorningSong = false;
			Game1.IsPlayingTownMusic = false;
			Game1.specialCurrencyDisplay = null;
			Game1.conventionMode = false;
			Game1.currentCursorTile = Vector2.Zero;
			Game1.currentDialogueCharacterIndex = 0;
			Game1.currentLightSources.Clear();
			Game1.currentLoader = null;
			Game1.currentLocation = null;
			Game1._PreviousNonNullLocation = null;
			Game1.currentObjectDialogue.Clear();
			Game1.currentQuestionChoice = 0;
			Game1.season = Season.Spring;
			Game1.currentSpeaker = null;
			Game1.currentViewportTarget = Vector2.Zero;
			Game1.cursorTileHintCheckTimer = 0;
			Game1.CustomData = new SerializableDictionary<string, string>();
			Game1.player.team.sharedDailyLuck.Value = 0.001;
			Game1.dayOfMonth = 0;
			Game1.debrisSoundInterval = 0f;
			Game1.debrisWeather.Clear();
			Game1.debugMode = false;
			Game1.debugOutput = null;
			Game1.debugPresenceString = "In menus";
			Game1.delayedActions.Clear();
			Game1.morningSongPlayAction = null;
			Game1.dialogueButtonScale = 1f;
			Game1.dialogueButtonShrinking = false;
			Game1.dialogueTyping = false;
			Game1.dialogueTypingInterval = 0;
			Game1.dialogueUp = false;
			Game1.dialogueWidth = 1024;
			Game1.displayFarmer = true;
			Game1.displayHUD = true;
			Game1.downPolling = 0f;
			Game1.drawGrid = false;
			Game1.drawLighting = false;
			Game1.elliottBookName = "Blue Tower";
			Game1.endOfNightMenus.Clear();
			Game1.errorMessage = "";
			Game1.eveningColor = new Color(255, 255, 0, 255);
			Game1.eventOver = false;
			Game1.eventUp = false;
			Game1.exitToTitle = false;
			Game1.facingDirectionAfterWarp = 0;
			Game1.fadeIn = true;
			Game1.fadeToBlack = false;
			Game1.fadeToBlackAlpha = 1.02f;
			Game1.farmEvent = null;
			Game1.flashAlpha = 0f;
			Game1.freezeControls = false;
			Game1.gamePadAButtonPolling = 0;
			Game1.gameTimeInterval = 0;
			Game1.globalFade = false;
			Game1.globalFadeSpeed = 0f;
			Game1.haltAfterCheck = false;
			Game1.hasLoadedGame = false;
			Game1.hasStartedDay = false;
			Game1.hitShakeTimer = 0;
			Game1.hudMessages.Clear();
			Game1.isActionAtCurrentCursorTile = false;
			Game1.isDebrisWeather = false;
			Game1.isInspectionAtCurrentCursorTile = false;
			Game1.isLightning = false;
			Game1.isQuestion = false;
			Game1.isRaining = false;
			Game1.wasGreenRain = false;
			Game1.isSnowing = false;
			Game1.killScreen = false;
			Game1.lastCursorMotionWasMouse = true;
			Game1.lastCursorTile = Vector2.Zero;
			Game1.lastMousePositionBeforeFade = Point.Zero;
			Game1.leftPolling = 0f;
			Game1.loadingMessage = "";
			Game1.locationRequest = null;
			Game1.warpingForForcedRemoteEvent = false;
			Game1.locations.Clear();
			Game1.mailbox.Clear();
			Game1.mapDisplayDevice = this.CreateDisplayDevice(Game1.content, base.GraphicsDevice);
			Game1.messageAfterPause = "";
			Game1.messagePause = false;
			Game1.mouseClickPolling = 0;
			Game1.mouseCursor = Game1.cursor_default;
			Game1.multiplayerMode = 0;
			Game1.netWorldState = new NetRoot<NetWorldState>(new NetWorldState());
			Game1.newDay = false;
			Game1.nonWarpFade = false;
			Game1.noteBlockTimer = 0f;
			Game1.npcDialogues = null;
			Game1.objectDialoguePortraitPerson = null;
			Game1.hasApplied1_3_UpdateChanges = false;
			Game1.hasApplied1_4_UpdateChanges = false;
			Game1.remoteEventQueue.Clear();
			SerializableDictionary<string, string> serializableDictionary = Game1.bannedUsers;
			if (serializableDictionary != null)
			{
				serializableDictionary.Clear();
			}
			Game1.nextClickableMenu.Clear();
			Game1.actionsWhenPlayerFree.Clear();
			Game1.onScreenMenus.Clear();
			Game1.onScreenMenus.Add(new Toolbar());
			Game1.dayTimeMoneyBox = new DayTimeMoneyBox();
			Game1.onScreenMenus.Add(Game1.dayTimeMoneyBox);
			Game1.buffsDisplay = new BuffsDisplay();
			Game1.onScreenMenus.Add(Game1.buffsDisplay);
			bool gamepad_controls = Game1.options.gamepadControls;
			bool snappy_menus = Game1.options.snappyMenus;
			Game1.options = new Options();
			Game1.options.gamepadControls = gamepad_controls;
			Game1.options.snappyMenus = snappy_menus;
			foreach (KeyValuePair<long, Farmer> f in Game1.otherFarmers)
			{
				f.Value.unload();
			}
			Game1.otherFarmers.Clear();
			Game1.outdoorLight = new Color(255, 255, 0, 255);
			Game1.overlayMenu = null;
			this.panFacingDirectionWait = false;
			Game1.panMode = false;
			this.panModeString = null;
			Game1.pauseAccumulator = 0f;
			Game1.paused = false;
			Game1.pauseThenDoFunctionTimer = 0;
			Game1.pauseTime = 0f;
			Game1.previousViewportPosition = Vector2.Zero;
			Game1.questionChoices.Clear();
			Game1.quit = false;
			Game1.rightClickPolling = 0;
			Game1.rightPolling = 0f;
			Game1.runThreshold = 0.5f;
			Game1.samBandName = "The Alfalfas";
			Game1.saveOnNewDay = true;
			Game1.startingCabins = 0;
			Game1.cabinsSeparate = false;
			Game1.screenGlow = false;
			Game1.screenGlowAlpha = 0f;
			Game1.screenGlowColor = new Color(0, 0, 0, 0);
			Game1.screenGlowHold = false;
			Game1.screenGlowMax = 0f;
			Game1.screenGlowRate = 0.005f;
			Game1.screenGlowUp = false;
			Game1.screenOverlayTempSprites.Clear();
			Game1.uiOverlayTempSprites.Clear();
			this.newGameSetupOptions.Clear();
			Game1.showingEndOfNightStuff = false;
			Game1.spawnMonstersAtNight = false;
			Game1.staminaShakeTimer = 0;
			Game1.textColor = new Color(34, 17, 34, 255);
			Game1.textShadowColor = new Color(206, 156, 95, 255);
			Game1.thumbstickMotionAccell = 1f;
			Game1.thumbstickMotionMargin = 0;
			Game1.thumbstickPollingTimer = 0;
			Game1.thumbStickSensitivity = 0.1f;
			Game1.timeOfDay = 600;
			Game1.timeOfDayAfterFade = -1;
			Game1.timerUntilMouseFade = 0;
			Game1.toggleFullScreen = false;
			Game1.ResetToolSpriteSheet();
			Game1.triggerPolling = 0;
			Game1.uniqueIDForThisGame = (ulong)(DateTime.UtcNow - new DateTime(2012, 6, 22)).TotalSeconds;
			Game1.upPolling = 0f;
			Game1.viewportFreeze = false;
			Game1.viewportHold = 0;
			Game1.viewportPositionLerp = Vector2.Zero;
			Game1.viewportReachedTarget = null;
			Game1.viewportSpeed = 2f;
			Game1.viewportTarget = new Vector2(-2.1474836E+09f, -2.1474836E+09f);
			Game1.wasMouseVisibleThisFrame = true;
			Game1.wasRainingYesterday = false;
			Game1.weatherForTomorrow = "Sun";
			Game1.elliottPiano = 0;
			Game1.weatherIcon = 0;
			Game1.weddingToday = false;
			Game1.whereIsTodaysFest = null;
			Game1.worldStateIDs.Clear();
			Game1.whichFarm = 0;
			Game1.whichModFarm = null;
			Game1.windGust = 0f;
			Game1.xLocationAfterWarp = 0;
			Game1.game1.xTileContent.Dispose();
			Game1.game1.xTileContent = this.CreateContentManager(Game1.content.ServiceProvider, Game1.content.RootDirectory);
			Game1.year = 1;
			Game1.yLocationAfterWarp = 0;
			Game1.mailDeliveredFromMailForTomorrow.Clear();
			Game1.bundleType = Game1.BundleType.Default;
			JojaMart.Morris = null;
			AmbientLocationSounds.onLocationLeave();
			WeatherDebris.globalWind = -0.25f;
			Utility.killAllStaticLoopingSoundCues();
			OptionsDropDown.selected = null;
			JunimoNoteMenu.tempSprites.Clear();
			JunimoNoteMenu.screenSwipe = null;
			JunimoNoteMenu.canClick = true;
			GameMenu.forcePreventClose = false;
			Club.timesPlayedCalicoJack = 0;
			MineShaft.activeMines.RemoveAll(delegate(MineShaft level)
			{
				level.OnRemoved();
				return true;
			});
			MineShaft.permanentMineChanges.Clear();
			MineShaft.numberOfCraftedStairsUsedThisRun = 0;
			MineShaft.mushroomLevelsGeneratedToday.Clear();
			VolcanoDungeon.activeLevels.RemoveAll(delegate(VolcanoDungeon level)
			{
				level.OnRemoved();
				return true;
			});
			ItemRegistry.ResetCache();
			Rumble.stopRumbling();
		}

		// Token: 0x06000C53 RID: 3155 RVA: 0x0008C52C File Offset: 0x0008A72C
		public virtual void CleanupReturningToTitle()
		{
			if (Game1.game1.IsMainInstance)
			{
				using (List<Game1>.Enumerator enumerator = GameRunner.instance.gameInstances.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Game1 instance = enumerator.Current;
						if (instance != this)
						{
							GameRunner.instance.RemoveGameInstance(instance);
						}
					}
					goto IL_59;
				}
			}
			GameRunner.instance.RemoveGameInstance(this);
			IL_59:
			Game1.multiplayer.Disconnect(Multiplayer.DisconnectType.ExitedToMainMenu);
			this.ResetGameStateOnTitleScreen();
			Game1.serverHost = null;
			Game1.client = null;
			Game1.server = null;
			TitleMenu.subMenu = null;
			Game1.game1.refreshWindowSettings();
			TitleMenu titleMenu = Game1.activeClickableMenu as TitleMenu;
			if (titleMenu != null)
			{
				titleMenu.applyPreferences();
				Game1.activeClickableMenu.gameWindowSizeChanged(Game1.graphics.GraphicsDevice.Viewport.Bounds, Game1.graphics.GraphicsDevice.Viewport.Bounds);
			}
		}

		// Token: 0x06000C54 RID: 3156 RVA: 0x0008C624 File Offset: 0x0008A824
		public bool CanTakeScreenshots()
		{
			return true;
		}

		// Token: 0x06000C55 RID: 3157 RVA: 0x0008C627 File Offset: 0x0008A827
		public string GetScreenshotFolder(bool createIfMissing = true)
		{
			return Program.GetLocalAppDataFolder("Screenshots", createIfMissing);
		}

		// Token: 0x06000C56 RID: 3158 RVA: 0x0008C634 File Offset: 0x0008A834
		public bool CanBrowseScreenshots()
		{
			return Directory.Exists(this.GetScreenshotFolder(false));
		}

		// Token: 0x06000C57 RID: 3159 RVA: 0x0008C642 File Offset: 0x0008A842
		public bool CanZoomScreenshots()
		{
			return true;
		}

		// Token: 0x06000C58 RID: 3160 RVA: 0x0008C648 File Offset: 0x0008A848
		public void BrowseScreenshots()
		{
			string folderPath = this.GetScreenshotFolder(false);
			if (Directory.Exists(folderPath))
			{
				try
				{
					Process.Start(new ProcessStartInfo
					{
						FileName = folderPath,
						UseShellExecute = true,
						Verb = "open"
					});
				}
				catch (Exception e)
				{
					Game1.log.Error("Failed to open screenshot folder.", e);
				}
			}
		}

		// Token: 0x06000C59 RID: 3161 RVA: 0x0008C6B0 File Offset: 0x0008A8B0
		public string takeMapScreenshot(float? in_scale, string screenshot_name, Action onDone)
		{
			if (Game1.currentLocation == null)
			{
				return null;
			}
			float scale = 1f;
			if (in_scale != null)
			{
				scale = in_scale.Value;
			}
			string screenshotName = screenshot_name;
			if (string.IsNullOrWhiteSpace(screenshot_name))
			{
				DateTime now = DateTime.UtcNow;
				string str = SaveGame.FilterFileName(Game1.player.name.Value);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(4, 4);
				defaultInterpolatedStringHandler.AppendLiteral("_");
				defaultInterpolatedStringHandler.AppendFormatted<int>(now.Month);
				defaultInterpolatedStringHandler.AppendLiteral("-");
				defaultInterpolatedStringHandler.AppendFormatted<int>(now.Day);
				defaultInterpolatedStringHandler.AppendLiteral("-");
				defaultInterpolatedStringHandler.AppendFormatted<int>(now.Year);
				defaultInterpolatedStringHandler.AppendLiteral("_");
				defaultInterpolatedStringHandler.AppendFormatted<int>((int)now.TimeOfDay.TotalMilliseconds);
				screenshotName = str + defaultInterpolatedStringHandler.ToStringAndClear();
			}
			return this.takeMapScreenshot(Game1.currentLocation, scale, screenshotName, onDone);
		}

		// Token: 0x06000C5A RID: 3162 RVA: 0x0008C79C File Offset: 0x0008A99C
		private unsafe string takeMapScreenshot(GameLocation screenshotLocation, float scale, string screenshot_name, Action onDone)
		{
			string filename = screenshot_name + ".png";
			int start_x;
			int start_y;
			int width;
			int height;
			Game1.GetScreenshotRegion(screenshotLocation, out start_x, out start_y, out width, out height);
			SKSurface map_bitmap = null;
			for (;;)
			{
				bool failed = false;
				int scaled_width = (int)((float)width * scale);
				int scaled_height = (int)((float)height * scale);
				try
				{
					map_bitmap = SKSurface.Create(scaled_width, scaled_height, SKColorType.Rgb888x, SKAlphaType.Opaque);
				}
				catch (Exception e)
				{
					Game1.log.Error("Map Screenshot: Error trying to create Bitmap.", e);
					failed = true;
				}
				if (failed)
				{
					scale -= 0.25f;
				}
				if (scale <= 0f)
				{
					break;
				}
				if (!failed)
				{
					goto Block_4;
				}
			}
			return null;
			Block_4:
			int chunk_size = 2048;
			int scaled_chunk_size = (int)((float)chunk_size * scale);
			xTile.Dimensions.Rectangle old_viewport = Game1.viewport;
			bool old_display_hud = Game1.displayHUD;
			this.takingMapScreenshot = true;
			float old_zoom_level = Game1.options.baseZoomLevel;
			Game1.options.baseZoomLevel = 1f;
			RenderTarget2D cached_lightmap = Game1._lightmap;
			Game1._lightmap = null;
			bool fail = false;
			try
			{
				Game1.allocateLightmap(chunk_size, chunk_size);
				int scaled_width;
				int chunks_wide = (int)Math.Ceiling((double)((float)scaled_width / (float)scaled_chunk_size));
				int scaled_height;
				int chunks_high = (int)Math.Ceiling((double)((float)scaled_height / (float)scaled_chunk_size));
				for (int y_offset = 0; y_offset < chunks_high; y_offset++)
				{
					for (int x_offset = 0; x_offset < chunks_wide; x_offset++)
					{
						int current_width = scaled_chunk_size;
						int current_height = scaled_chunk_size;
						int current_x = x_offset * scaled_chunk_size;
						int current_y = y_offset * scaled_chunk_size;
						if (current_x + scaled_chunk_size > scaled_width)
						{
							current_width += scaled_width - (current_x + scaled_chunk_size);
						}
						if (current_y + scaled_chunk_size > scaled_height)
						{
							current_height += scaled_height - (current_y + scaled_chunk_size);
						}
						if (current_height > 0 && current_width > 0)
						{
							Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(current_x, current_y, current_width, current_height);
							RenderTarget2D render_target = new RenderTarget2D(Game1.graphics.GraphicsDevice, chunk_size, chunk_size, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
							Game1.viewport = new xTile.Dimensions.Rectangle(x_offset * chunk_size + start_x, y_offset * chunk_size + start_y, chunk_size, chunk_size);
							this._draw(Game1.currentGameTime, render_target);
							RenderTarget2D scaled_render_target = new RenderTarget2D(Game1.graphics.GraphicsDevice, current_width, current_height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
							base.GraphicsDevice.SetRenderTarget(scaled_render_target);
							Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, null);
							Color color = Color.White;
							Game1.spriteBatch.Draw(render_target, Vector2.Zero, new Microsoft.Xna.Framework.Rectangle?(render_target.Bounds), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
							Game1.spriteBatch.End();
							render_target.Dispose();
							base.GraphicsDevice.SetRenderTarget(null);
							Color[] colors = new Color[current_width * current_height];
							scaled_render_target.GetData<Color>(colors);
							SKBitmap portion_bitmap = new SKBitmap(rect.Width, rect.Height, SKColorType.Rgb888x, SKAlphaType.Opaque);
							byte* ptr = (byte*)portion_bitmap.GetPixels().ToPointer();
							for (int row = 0; row < current_height; row++)
							{
								for (int col = 0; col < current_width; col++)
								{
									*(ptr++) = colors[col + row * current_width].R;
									*(ptr++) = colors[col + row * current_width].G;
									*(ptr++) = colors[col + row * current_width].B;
									*(ptr++) = byte.MaxValue;
								}
							}
							SKPaint paint = new SKPaint();
							map_bitmap.Canvas.DrawBitmap(portion_bitmap, SKRect.Create((float)rect.X, (float)rect.Y, (float)current_width, (float)current_height), paint);
							portion_bitmap.Dispose();
							scaled_render_target.Dispose();
						}
					}
				}
				string fullFilePath = Path.Combine(this.GetScreenshotFolder(true), filename);
				map_bitmap.Snapshot().Encode(SKEncodedImageFormat.Png, 100).SaveTo(new FileStream(fullFilePath, FileMode.OpenOrCreate));
				map_bitmap.Dispose();
			}
			catch (Exception e2)
			{
				Game1.log.Error("Map Screenshot: Error taking screenshot.", e2);
				base.GraphicsDevice.SetRenderTarget(null);
				fail = true;
			}
			if (Game1._lightmap != null)
			{
				Game1._lightmap.Dispose();
				Game1._lightmap = null;
			}
			Game1._lightmap = cached_lightmap;
			Game1.options.baseZoomLevel = old_zoom_level;
			this.takingMapScreenshot = false;
			Game1.displayHUD = old_display_hud;
			Game1.viewport = old_viewport;
			if (fail)
			{
				return null;
			}
			return filename;
		}

		// Token: 0x06000C5B RID: 3163 RVA: 0x0008CBDC File Offset: 0x0008ADDC
		private static void GetScreenshotRegion(GameLocation screenshotLocation, out int startX, out int startY, out int width, out int height)
		{
			startX = 0;
			startY = 0;
			width = screenshotLocation.map.DisplayWidth;
			height = screenshotLocation.map.DisplayHeight;
			try
			{
				string[] fields = screenshotLocation.GetMapPropertySplitBySpaces("ScreenshotRegion");
				if (fields.Length != 0)
				{
					int topLeftX;
					string error;
					int topLeftY;
					int bottomRightX;
					int bottomRightY;
					if (!ArgUtility.TryGetInt(fields, 0, out topLeftX, out error, "int topLeftX") || !ArgUtility.TryGetInt(fields, 1, out topLeftY, out error, "int topLeftY") || !ArgUtility.TryGetInt(fields, 2, out bottomRightX, out error, "int bottomRightX") || !ArgUtility.TryGetInt(fields, 3, out bottomRightY, out error, "int bottomRightY"))
					{
						screenshotLocation.LogMapPropertyError("ScreenshotRegion", fields, error, ' ');
					}
					else
					{
						startX = topLeftX * 64;
						startY = topLeftY * 64;
						width = (bottomRightX + 1) * 64 - startX;
						height = (bottomRightY + 1) * 64 - startY;
					}
				}
			}
			catch (Exception ex)
			{
				Game1.log.Error("GetScreenshotRegion failed with exception:", ex);
			}
		}

		// Token: 0x06000C5C RID: 3164 RVA: 0x0008CCC0 File Offset: 0x0008AEC0
		[CompilerGenerated]
		internal static void <playMorningSong>g__PlayRain|678_0()
		{
			Game1.changeMusicTrack("rain", true, MusicContext.Default);
		}

		// Token: 0x06000C5D RID: 3165 RVA: 0x0008CCCE File Offset: 0x0008AECE
		[CompilerGenerated]
		internal static void <playMorningSong>g__PlayDefault|678_2()
		{
			Game1.changeMusicTrack(Game1.currentLocation.GetMorningSong(), true, MusicContext.Default);
			Game1.IsPlayingBackgroundMusic = true;
			Game1.IsPlayingMorningSong = true;
		}

		// Token: 0x06000C5E RID: 3166 RVA: 0x0008CCF0 File Offset: 0x0008AEF0
		[CompilerGenerated]
		internal static void <onFadeToBlackComplete>g__AfterNewDay|746_0()
		{
			if (Game1.eventOver)
			{
				Game1.eventFinished();
				if (Game1.dayOfMonth == 0)
				{
					Game1.newDayAfterFade(delegate
					{
						Game1.player.Position = new Vector2(320f, 320f);
					});
				}
			}
			Game1.nonWarpFade = false;
			Game1.fadeIn = false;
		}

		// Token: 0x06000C5F RID: 3167 RVA: 0x0008CD40 File Offset: 0x0008AF40
		[CompilerGenerated]
		internal static void <onFadeToBlackComplete>g__AfterEventOver|746_1()
		{
			Game1.currentLocation.resetForPlayerEntry();
			Game1.nonWarpFade = false;
			Game1.fadeIn = false;
		}

		// Token: 0x0400069B RID: 1691
		public const bool IncrementalLoadEnabled = false;

		// Token: 0x0400069C RID: 1692
		public const int defaultResolutionX = 1280;

		// Token: 0x0400069D RID: 1693
		public const int defaultResolutionY = 720;

		// Token: 0x0400069E RID: 1694
		public const int pixelZoom = 4;

		// Token: 0x0400069F RID: 1695
		public const int tileSize = 64;

		// Token: 0x040006A0 RID: 1696
		public const int smallestTileSize = 16;

		// Token: 0x040006A1 RID: 1697
		public const int up = 0;

		// Token: 0x040006A2 RID: 1698
		public const int right = 1;

		// Token: 0x040006A3 RID: 1699
		public const int down = 2;

		// Token: 0x040006A4 RID: 1700
		public const int left = 3;

		// Token: 0x040006A5 RID: 1701
		public const int dialogueBoxTileHeight = 5;

		// Token: 0x040006A6 RID: 1702
		public static int realMilliSecondsPerGameMinute = 700;

		// Token: 0x040006A7 RID: 1703
		public static int realMilliSecondsPerGameTenMinutes = Game1.realMilliSecondsPerGameMinute * 10;

		// Token: 0x040006A8 RID: 1704
		public const int rainDensity = 70;

		// Token: 0x040006A9 RID: 1705
		public const int rainLoopLength = 70;

		// Token: 0x040006AA RID: 1706
		public static readonly int cursor_none = -1;

		// Token: 0x040006AB RID: 1707
		public static readonly int cursor_default = 0;

		// Token: 0x040006AC RID: 1708
		public static readonly int cursor_wait = 1;

		// Token: 0x040006AD RID: 1709
		public static readonly int cursor_grab = 2;

		// Token: 0x040006AE RID: 1710
		public static readonly int cursor_gift = 3;

		// Token: 0x040006AF RID: 1711
		public static readonly int cursor_talk = 4;

		// Token: 0x040006B0 RID: 1712
		public static readonly int cursor_look = 5;

		// Token: 0x040006B1 RID: 1713
		public static readonly int cursor_harvest = 6;

		// Token: 0x040006B2 RID: 1714
		public static readonly int cursor_gamepad_pointer = 44;

		// Token: 0x040006B3 RID: 1715
		public static readonly string asianSpacingRegexString = "\\s|[（《“‘「『(](?:[\\w,%％]+|[^…—])[々ぁぃぅぇぉっゃゅょゎゕゖァィゥェォッャュョヶー]*[）》”’」』)，。、？！：；·～,.?!:;~…]*|.[々ぁぃぅぇぉっゃゅょゎゕゖァィゥェォッャュョヶー]*[·・].[々ぁぃぅぇぉっゃゅょゎゕゖァィゥェォッャュョヶー]*|(?:[\\w,%％]+|[^…—])[々ぁぃぅぇぉっゃゅょゎゕゖァィゥェォッャュョヶー]*[）》”’」』)]?(?:[，。、？！：；·～,.?!:;~…]{1,2}[）》”’」』)]?)?|[\\w,%％]+|.[々ぁぃぅぇぉっゃゅょゎゕゖァィゥェォッャュョヶー]+|……|——|.";

		// Token: 0x040006B4 RID: 1716
		public const int legacy_weather_sunny = 0;

		// Token: 0x040006B5 RID: 1717
		public const int legacy_weather_rain = 1;

		// Token: 0x040006B6 RID: 1718
		public const int legacy_weather_debris = 2;

		// Token: 0x040006B7 RID: 1719
		public const int legacy_weather_lightning = 3;

		// Token: 0x040006B8 RID: 1720
		public const int legacy_weather_festival = 4;

		// Token: 0x040006B9 RID: 1721
		public const int legacy_weather_snow = 5;

		// Token: 0x040006BA RID: 1722
		public const int legacy_weather_wedding = 6;

		// Token: 0x040006BB RID: 1723
		public const string weather_sunny = "Sun";

		// Token: 0x040006BC RID: 1724
		public const string weather_rain = "Rain";

		// Token: 0x040006BD RID: 1725
		public const string weather_green_rain = "GreenRain";

		// Token: 0x040006BE RID: 1726
		public const string weather_debris = "Wind";

		// Token: 0x040006BF RID: 1727
		public const string weather_lightning = "Storm";

		// Token: 0x040006C0 RID: 1728
		public const string weather_festival = "Festival";

		// Token: 0x040006C1 RID: 1729
		public const string weather_snow = "Snow";

		// Token: 0x040006C2 RID: 1730
		public const string weather_wedding = "Wedding";

		// Token: 0x040006C3 RID: 1731
		public const string builder_robin = "Robin";

		// Token: 0x040006C4 RID: 1732
		public const string builder_wizard = "Wizard";

		// Token: 0x040006C5 RID: 1733
		public const string shop_adventurersGuild = "AdventureShop";

		// Token: 0x040006C6 RID: 1734
		public const string shop_adventurersGuildItemRecovery = "AdventureGuildRecovery";

		// Token: 0x040006C7 RID: 1735
		public const string shop_animalSupplies = "AnimalShop";

		// Token: 0x040006C8 RID: 1736
		public const string shop_blacksmith = "Blacksmith";

		// Token: 0x040006C9 RID: 1737
		public const string shop_blacksmithUpgrades = "ClintUpgrade";

		// Token: 0x040006CA RID: 1738
		public const string shop_boxOffice = "BoxOffice";

		// Token: 0x040006CB RID: 1739
		public const string shop_catalogue = "Catalogue";

		// Token: 0x040006CC RID: 1740
		public const string shop_carpenter = "Carpenter";

		// Token: 0x040006CD RID: 1741
		public const string shop_casino = "Casino";

		// Token: 0x040006CE RID: 1742
		public const string shop_desertTrader = "DesertTrade";

		// Token: 0x040006CF RID: 1743
		public const string shop_dwarf = "Dwarf";

		// Token: 0x040006D0 RID: 1744
		public const string shop_fish = "FishShop";

		// Token: 0x040006D1 RID: 1745
		public const string shop_furnitureCatalogue = "Furniture Catalogue";

		// Token: 0x040006D2 RID: 1746
		public const string shop_generalStore = "SeedShop";

		// Token: 0x040006D3 RID: 1747
		public const string shop_hatMouse = "HatMouse";

		// Token: 0x040006D4 RID: 1748
		public const string shop_hospital = "Hospital";

		// Token: 0x040006D5 RID: 1749
		public const string shop_iceCreamStand = "IceCreamStand";

		// Token: 0x040006D6 RID: 1750
		public const string shop_islandTrader = "IslandTrade";

		// Token: 0x040006D7 RID: 1751
		public const string shop_jojaMart = "Joja";

		// Token: 0x040006D8 RID: 1752
		public const string shop_krobus = "ShadowShop";

		// Token: 0x040006D9 RID: 1753
		public const string shop_qiGemShop = "QiGemShop";

		// Token: 0x040006DA RID: 1754
		public const string shop_resortBar = "ResortBar";

		// Token: 0x040006DB RID: 1755
		public const string shop_sandy = "Sandy";

		// Token: 0x040006DC RID: 1756
		public const string shop_saloon = "Saloon";

		// Token: 0x040006DD RID: 1757
		public const string shop_travelingCart = "Traveler";

		// Token: 0x040006DE RID: 1758
		public const string shop_volcanoShop = "VolcanoShop";

		// Token: 0x040006DF RID: 1759
		public const string shop_bookseller = "Bookseller";

		// Token: 0x040006E0 RID: 1760
		public const string shop_bookseller_trade = "BooksellerTrade";

		// Token: 0x040006E1 RID: 1761
		public const string shop_jojaCatalogue = "JojaFurnitureCatalogue";

		// Token: 0x040006E2 RID: 1762
		public const string shop_wizardCatalogue = "WizardFurnitureCatalogue";

		// Token: 0x040006E3 RID: 1763
		public const string shop_junimoCatalogue = "JunimoFurnitureCatalogue";

		// Token: 0x040006E4 RID: 1764
		public const string shop_retroCatalogue = "RetroFurnitureCatalogue";

		// Token: 0x040006E5 RID: 1765
		public const string shop_trashCatalogue = "TrashFurnitureCatalogue";

		// Token: 0x040006E6 RID: 1766
		public const string shop_petAdoption = "PetAdoption";

		// Token: 0x040006E7 RID: 1767
		public const byte singlePlayer = 0;

		// Token: 0x040006E8 RID: 1768
		public const byte multiplayerClient = 1;

		// Token: 0x040006E9 RID: 1769
		public const byte multiplayerServer = 2;

		// Token: 0x040006EA RID: 1770
		public const byte logoScreenGameMode = 4;

		// Token: 0x040006EB RID: 1771
		public const byte titleScreenGameMode = 0;

		// Token: 0x040006EC RID: 1772
		public const byte loadScreenGameMode = 1;

		// Token: 0x040006ED RID: 1773
		public const byte newGameMode = 2;

		// Token: 0x040006EE RID: 1774
		public const byte playingGameMode = 3;

		// Token: 0x040006EF RID: 1775
		public const byte loadingMode = 6;

		// Token: 0x040006F0 RID: 1776
		public const byte saveMode = 7;

		// Token: 0x040006F1 RID: 1777
		public const byte saveCompleteMode = 8;

		// Token: 0x040006F2 RID: 1778
		public const byte selectGameScreen = 9;

		// Token: 0x040006F3 RID: 1779
		public const byte creditsMode = 10;

		// Token: 0x040006F4 RID: 1780
		public const byte errorLogMode = 11;

		// Token: 0x040006F5 RID: 1781
		public static readonly string GameAssemblyName;

		// Token: 0x040006F6 RID: 1782
		public static readonly string version;

		// Token: 0x040006F7 RID: 1783
		public static readonly string versionLabel;

		// Token: 0x040006F8 RID: 1784
		public static readonly int versionBuildNumber;

		// Token: 0x040006F9 RID: 1785
		public const float keyPollingThreshold = 650f;

		// Token: 0x040006FA RID: 1786
		public const float toolHoldPerPowerupLevel = 600f;

		// Token: 0x040006FB RID: 1787
		public const float startingMusicVolume = 1f;

		// Token: 0x040006FC RID: 1788
		public LocalizedContentManager xTileContent;

		// Token: 0x040006FD RID: 1789
		public static DelayedAction morningSongPlayAction;

		// Token: 0x040006FE RID: 1790
		private static LocalizedContentManager _temporaryContent;

		// Token: 0x040006FF RID: 1791
		[NonInstancedStatic]
		private static bool FinishedIncrementalLoad = false;

		// Token: 0x04000700 RID: 1792
		[NonInstancedStatic]
		private static bool FinishedFirstLoadContent = false;

		// Token: 0x04000701 RID: 1793
		[NonInstancedStatic]
		private static volatile bool FinishedFirstInitSounds = false;

		// Token: 0x04000702 RID: 1794
		[NonInstancedStatic]
		private static volatile bool FinishedFirstInitSerializers = false;

		// Token: 0x04000703 RID: 1795
		[NonInstancedStatic]
		private static IEnumerator<int> LoadContentEnumerator;

		// Token: 0x04000704 RID: 1796
		[NonInstancedStatic]
		public static GraphicsDeviceManager graphics;

		// Token: 0x04000705 RID: 1797
		[NonInstancedStatic]
		public static LocalizedContentManager content;

		// Token: 0x04000706 RID: 1798
		public static SpriteBatch spriteBatch;

		// Token: 0x04000707 RID: 1799
		public static float MusicDuckTimer = 0f;

		// Token: 0x04000708 RID: 1800
		public static GamePadState oldPadState;

		// Token: 0x04000709 RID: 1801
		public static float thumbStickSensitivity = 0.1f;

		// Token: 0x0400070A RID: 1802
		public static float runThreshold = 0.5f;

		// Token: 0x0400070B RID: 1803
		public static int rightStickHoldTime = 0;

		// Token: 0x0400070C RID: 1804
		public static int emoteMenuShowTime = 250;

		// Token: 0x0400070D RID: 1805
		public static int nextFarmerWarpOffsetX = 0;

		// Token: 0x0400070E RID: 1806
		public static int nextFarmerWarpOffsetY = 0;

		// Token: 0x0400070F RID: 1807
		public static KeyboardState oldKBState;

		// Token: 0x04000710 RID: 1808
		public static MouseState oldMouseState;

		// Token: 0x04000711 RID: 1809
		[NonInstancedStatic]
		public static Game1 keyboardFocusInstance = null;

		// Token: 0x04000712 RID: 1810
		private static Farmer _player;

		// Token: 0x04000713 RID: 1811
		public static NetFarmerRoot serverHost;

		// Token: 0x04000714 RID: 1812
		protected static bool _isWarping = false;

		// Token: 0x04000715 RID: 1813
		[NonInstancedStatic]
		public static bool hasLocalClientsOnly = false;

		// Token: 0x04000716 RID: 1814
		protected bool _instanceIsPlayingBackgroundMusic;

		// Token: 0x04000717 RID: 1815
		protected bool _instanceIsPlayingOutdoorsAmbience;

		// Token: 0x04000718 RID: 1816
		protected bool _instanceIsPlayingNightAmbience;

		// Token: 0x04000719 RID: 1817
		protected bool _instanceIsPlayingTownMusic;

		// Token: 0x0400071A RID: 1818
		protected bool _instanceIsPlayingMorningSong;

		// Token: 0x0400071B RID: 1819
		public static bool isUsingBackToFrontSorting = false;

		// Token: 0x0400071C RID: 1820
		protected static StringBuilder _debugStringBuilder = new StringBuilder();

		// Token: 0x0400071D RID: 1821
		[NonInstancedStatic]
		internal static readonly DebugTimings debugTimings = new DebugTimings();

		// Token: 0x0400071E RID: 1822
		public static Dictionary<string, GameLocation> _locationLookup = new Dictionary<string, GameLocation>(StringComparer.OrdinalIgnoreCase);

		// Token: 0x0400071F RID: 1823
		public IList<GameLocation> _locations = new List<GameLocation>();

		// Token: 0x04000720 RID: 1824
		public static Regex asianSpacingRegex = new Regex(Game1.asianSpacingRegexString, RegexOptions.ECMAScript);

		// Token: 0x04000721 RID: 1825
		public static Viewport defaultDeviceViewport;

		// Token: 0x04000722 RID: 1826
		public static LocationRequest locationRequest;

		// Token: 0x04000723 RID: 1827
		public static bool warpingForForcedRemoteEvent = false;

		// Token: 0x04000724 RID: 1828
		protected static GameLocation _PreviousNonNullLocation = null;

		// Token: 0x04000725 RID: 1829
		public GameLocation instanceGameLocation;

		// Token: 0x04000726 RID: 1830
		public static IDisplayDevice mapDisplayDevice;

		// Token: 0x04000727 RID: 1831
		public static xTile.Dimensions.Rectangle viewport;

		// Token: 0x04000728 RID: 1832
		public static xTile.Dimensions.Rectangle uiViewport;

		// Token: 0x04000729 RID: 1833
		public static Texture2D objectSpriteSheet;

		// Token: 0x0400072A RID: 1834
		public static Texture2D cropSpriteSheet;

		// Token: 0x0400072B RID: 1835
		public static Texture2D emoteSpriteSheet;

		// Token: 0x0400072C RID: 1836
		public static Texture2D debrisSpriteSheet;

		// Token: 0x0400072D RID: 1837
		public static Texture2D rainTexture;

		// Token: 0x0400072E RID: 1838
		public static Texture2D bigCraftableSpriteSheet;

		// Token: 0x0400072F RID: 1839
		public static Texture2D buffsIcons;

		// Token: 0x04000730 RID: 1840
		public static Texture2D daybg;

		// Token: 0x04000731 RID: 1841
		public static Texture2D nightbg;

		// Token: 0x04000732 RID: 1842
		public static Texture2D menuTexture;

		// Token: 0x04000733 RID: 1843
		public static Texture2D uncoloredMenuTexture;

		// Token: 0x04000734 RID: 1844
		public static Texture2D lantern;

		// Token: 0x04000735 RID: 1845
		public static Texture2D windowLight;

		// Token: 0x04000736 RID: 1846
		public static Texture2D sconceLight;

		// Token: 0x04000737 RID: 1847
		public static Texture2D cauldronLight;

		// Token: 0x04000738 RID: 1848
		public static Texture2D shadowTexture;

		// Token: 0x04000739 RID: 1849
		public static Texture2D mouseCursors;

		// Token: 0x0400073A RID: 1850
		public static Texture2D mouseCursors2;

		// Token: 0x0400073B RID: 1851
		public static Texture2D mouseCursors_1_6;

		// Token: 0x0400073C RID: 1852
		public static Texture2D giftboxTexture;

		// Token: 0x0400073D RID: 1853
		public static Texture2D controllerMaps;

		// Token: 0x0400073E RID: 1854
		public static Texture2D indoorWindowLight;

		// Token: 0x0400073F RID: 1855
		public static Texture2D animations;

		// Token: 0x04000740 RID: 1856
		public static Texture2D concessionsSpriteSheet;

		// Token: 0x04000741 RID: 1857
		public static Texture2D birdsSpriteSheet;

		// Token: 0x04000742 RID: 1858
		public static Texture2D objectSpriteSheet_2;

		// Token: 0x04000743 RID: 1859
		public static Texture2D bobbersTexture;

		// Token: 0x04000744 RID: 1860
		public static Dictionary<string, Stack<Dialogue>> npcDialogues = new Dictionary<string, Stack<Dialogue>>();

		// Token: 0x04000745 RID: 1861
		protected readonly List<Farmer> _farmerShadows = new List<Farmer>();

		// Token: 0x04000746 RID: 1862
		public static Queue<Action> morningQueue = new Queue<Action>();

		// Token: 0x04000747 RID: 1863
		[NonInstancedStatic]
		protected internal static ModHooks hooks = new ModHooks();

		// Token: 0x04000748 RID: 1864
		public static InputState input = new InputState();

		// Token: 0x04000749 RID: 1865
		protected internal static IInputSimulator inputSimulator = null;

		// Token: 0x0400074A RID: 1866
		public const string concessionsSpriteSheetName = "LooseSprites\\Concessions";

		// Token: 0x0400074B RID: 1867
		public const string cropSpriteSheetName = "TileSheets\\crops";

		// Token: 0x0400074C RID: 1868
		public const string objectSpriteSheetName = "Maps\\springobjects";

		// Token: 0x0400074D RID: 1869
		public const string animationsName = "TileSheets\\animations";

		// Token: 0x0400074E RID: 1870
		public const string mouseCursorsName = "LooseSprites\\Cursors";

		// Token: 0x0400074F RID: 1871
		public const string mouseCursors2Name = "LooseSprites\\Cursors2";

		// Token: 0x04000750 RID: 1872
		public const string mouseCursors1_6Name = "LooseSprites\\Cursors_1_6";

		// Token: 0x04000751 RID: 1873
		public const string giftboxName = "LooseSprites\\Giftbox";

		// Token: 0x04000752 RID: 1874
		public const string toolSpriteSheetName = "TileSheets\\tools";

		// Token: 0x04000753 RID: 1875
		public const string bigCraftableSpriteSheetName = "TileSheets\\Craftables";

		// Token: 0x04000754 RID: 1876
		public const string debrisSpriteSheetName = "TileSheets\\debris";

		// Token: 0x04000755 RID: 1877
		public const string parrotSheetName = "LooseSprites\\parrots";

		// Token: 0x04000756 RID: 1878
		public const string hatsSheetName = "Characters\\Farmer\\hats";

		// Token: 0x04000757 RID: 1879
		public const string bobbersTextureName = "TileSheets\\bobbers";

		// Token: 0x04000758 RID: 1880
		private static Texture2D _toolSpriteSheet = null;

		// Token: 0x04000759 RID: 1881
		public static Dictionary<Vector2, int> crabPotOverlayTiles = new Dictionary<Vector2, int>();

		// Token: 0x0400075A RID: 1882
		protected static bool _setSaveName = false;

		// Token: 0x0400075B RID: 1883
		protected static string _currentSaveName = "";

		// Token: 0x0400075C RID: 1884
		public static List<string> mailDeliveredFromMailForTomorrow = new List<string>();

		// Token: 0x0400075D RID: 1885
		private static RenderTarget2D _lightmap;

		// Token: 0x0400075E RID: 1886
		public static Texture2D[] dynamicPixelRects = new Texture2D[3];

		// Token: 0x0400075F RID: 1887
		public static Texture2D fadeToBlackRect;

		// Token: 0x04000760 RID: 1888
		public static Texture2D staminaRect;

		// Token: 0x04000761 RID: 1889
		public static Texture2D lightingRect;

		// Token: 0x04000762 RID: 1890
		public static SpriteFont dialogueFont;

		// Token: 0x04000763 RID: 1891
		public static SpriteFont smallFont;

		// Token: 0x04000764 RID: 1892
		public static SpriteFont tinyFont;

		// Token: 0x04000765 RID: 1893
		public static float screenGlowAlpha = 0f;

		// Token: 0x04000766 RID: 1894
		public static float flashAlpha = 0f;

		// Token: 0x04000767 RID: 1895
		public static float noteBlockTimer;

		// Token: 0x04000768 RID: 1896
		public static int currentGemBirdIndex = 0;

		// Token: 0x04000769 RID: 1897
		public Dictionary<string, object> newGameSetupOptions = new Dictionary<string, object>();

		// Token: 0x0400076A RID: 1898
		public static bool dialogueUp = false;

		// Token: 0x0400076B RID: 1899
		public static bool dialogueTyping = false;

		// Token: 0x0400076C RID: 1900
		public static bool isQuestion = false;

		// Token: 0x0400076D RID: 1901
		public static bool newDay = false;

		// Token: 0x0400076E RID: 1902
		public static bool eventUp = false;

		// Token: 0x0400076F RID: 1903
		public static bool viewportFreeze = false;

		// Token: 0x04000770 RID: 1904
		public static bool eventOver = false;

		// Token: 0x04000771 RID: 1905
		public static bool screenGlow = false;

		// Token: 0x04000772 RID: 1906
		public static bool screenGlowHold = false;

		// Token: 0x04000773 RID: 1907
		public static bool screenGlowUp;

		// Token: 0x04000774 RID: 1908
		public static bool killScreen = false;

		// Token: 0x04000775 RID: 1909
		public static bool messagePause;

		// Token: 0x04000776 RID: 1910
		public static bool weddingToday;

		// Token: 0x04000777 RID: 1911
		public static bool exitToTitle;

		// Token: 0x04000778 RID: 1912
		public static bool debugMode;

		// Token: 0x04000779 RID: 1913
		public static bool displayHUD = true;

		// Token: 0x0400077A RID: 1914
		public static bool displayFarmer = true;

		// Token: 0x0400077B RID: 1915
		public static bool dialogueButtonShrinking;

		// Token: 0x0400077C RID: 1916
		public static bool drawLighting;

		// Token: 0x0400077D RID: 1917
		public static bool quit;

		// Token: 0x0400077E RID: 1918
		public static bool drawGrid;

		// Token: 0x0400077F RID: 1919
		public static bool freezeControls;

		// Token: 0x04000780 RID: 1920
		public static bool saveOnNewDay;

		// Token: 0x04000781 RID: 1921
		public static bool panMode;

		// Token: 0x04000782 RID: 1922
		public static bool showingEndOfNightStuff;

		// Token: 0x04000783 RID: 1923
		public static bool wasRainingYesterday;

		// Token: 0x04000784 RID: 1924
		public static bool hasLoadedGame;

		// Token: 0x04000785 RID: 1925
		public static bool isActionAtCurrentCursorTile;

		// Token: 0x04000786 RID: 1926
		public static bool isInspectionAtCurrentCursorTile;

		// Token: 0x04000787 RID: 1927
		public static bool isSpeechAtCurrentCursorTile;

		// Token: 0x04000788 RID: 1928
		public static bool paused;

		// Token: 0x04000789 RID: 1929
		public static bool isTimePaused;

		// Token: 0x0400078A RID: 1930
		public static bool frameByFrame;

		// Token: 0x0400078B RID: 1931
		public static bool lastCursorMotionWasMouse;

		// Token: 0x0400078C RID: 1932
		public static bool showingHealth = false;

		// Token: 0x0400078D RID: 1933
		public static bool cabinsSeparate = false;

		// Token: 0x0400078E RID: 1934
		public static bool showingHealthBar = false;

		// Token: 0x0400078F RID: 1935
		public static bool hasStartedDay = false;

		// Token: 0x04000790 RID: 1936
		public static HashSet<string> eventsSeenSinceLastLocationChange = new HashSet<string>();

		// Token: 0x04000791 RID: 1937
		internal static bool hasApplied1_3_UpdateChanges = false;

		// Token: 0x04000792 RID: 1938
		internal static bool hasApplied1_4_UpdateChanges = false;

		// Token: 0x04000793 RID: 1939
		private static Action postExitToTitleCallback = null;

		// Token: 0x04000794 RID: 1940
		protected int _lastUsedDisplay = -1;

		// Token: 0x04000795 RID: 1941
		public bool wasAskedLeoMemory;

		// Token: 0x04000796 RID: 1942
		public float controllerSlingshotSafeTime;

		// Token: 0x04000797 RID: 1943
		public static Game1.BundleType bundleType = Game1.BundleType.Default;

		// Token: 0x04000798 RID: 1944
		public static bool isRaining = false;

		// Token: 0x04000799 RID: 1945
		public static bool isSnowing = false;

		// Token: 0x0400079A RID: 1946
		public static bool isLightning = false;

		// Token: 0x0400079B RID: 1947
		public static bool isDebrisWeather = false;

		// Token: 0x0400079C RID: 1948
		private static bool _isGreenRain = false;

		// Token: 0x0400079D RID: 1949
		internal static bool wasGreenRain = false;

		// Token: 0x0400079E RID: 1950
		internal static bool greenRainNeedsCleanup = false;

		// Token: 0x0400079F RID: 1951
		public static Season? debrisWeatherSeason;

		// Token: 0x040007A0 RID: 1952
		public static string weatherForTomorrow;

		// Token: 0x040007A1 RID: 1953
		public float zoomModifier = 1f;

		// Token: 0x040007A2 RID: 1954
		private static ScreenFade screenFade;

		// Token: 0x040007A3 RID: 1955
		public static Season season = Season.Spring;

		// Token: 0x040007A4 RID: 1956
		public static SerializableDictionary<string, string> bannedUsers = new SerializableDictionary<string, string>();

		// Token: 0x040007A5 RID: 1957
		private static object _debugOutputLock = new object();

		// Token: 0x040007A6 RID: 1958
		private static string _debugOutput;

		// Token: 0x040007A7 RID: 1959
		public static string requestedMusicTrack = "";

		// Token: 0x040007A8 RID: 1960
		public static string messageAfterPause = "";

		// Token: 0x040007A9 RID: 1961
		public static string samBandName = "The Alfalfas";

		// Token: 0x040007AA RID: 1962
		public static string loadingMessage = "";

		// Token: 0x040007AB RID: 1963
		public static string errorMessage = "";

		// Token: 0x040007AC RID: 1964
		protected Dictionary<MusicContext, KeyValuePair<string, bool>> _instanceRequestedMusicTracks = new Dictionary<MusicContext, KeyValuePair<string, bool>>();

		// Token: 0x040007AD RID: 1965
		protected MusicContext _instanceActiveMusicContext;

		// Token: 0x040007AE RID: 1966
		public static bool requestedMusicTrackOverrideable;

		// Token: 0x040007AF RID: 1967
		public static bool currentTrackOverrideable;

		// Token: 0x040007B0 RID: 1968
		public static bool requestedMusicDirty = false;

		// Token: 0x040007B1 RID: 1969
		protected bool _useUnscaledLighting;

		// Token: 0x040007B2 RID: 1970
		protected bool _didInitiateItemStow;

		// Token: 0x040007B3 RID: 1971
		public bool instanceIsOverridingTrack;

		// Token: 0x040007B4 RID: 1972
		private static string[] _shortDayDisplayName = new string[7];

		// Token: 0x040007B5 RID: 1973
		public static Queue<string> currentObjectDialogue = new Queue<string>();

		// Token: 0x040007B6 RID: 1974
		public static HashSet<string> worldStateIDs = new HashSet<string>();

		// Token: 0x040007B7 RID: 1975
		public static List<Response> questionChoices = new List<Response>();

		// Token: 0x040007B8 RID: 1976
		public static int xLocationAfterWarp;

		// Token: 0x040007B9 RID: 1977
		public static int yLocationAfterWarp;

		// Token: 0x040007BA RID: 1978
		public static int gameTimeInterval;

		// Token: 0x040007BB RID: 1979
		public static int currentQuestionChoice;

		// Token: 0x040007BC RID: 1980
		public static int currentDialogueCharacterIndex;

		// Token: 0x040007BD RID: 1981
		public static int dialogueTypingInterval;

		// Token: 0x040007BE RID: 1982
		public static int dayOfMonth = 0;

		// Token: 0x040007BF RID: 1983
		public static int year = 1;

		// Token: 0x040007C0 RID: 1984
		public static int timeOfDay = 600;

		// Token: 0x040007C1 RID: 1985
		public static int timeOfDayAfterFade = -1;

		// Token: 0x040007C2 RID: 1986
		public static int dialogueWidth;

		// Token: 0x040007C3 RID: 1987
		public static int facingDirectionAfterWarp;

		// Token: 0x040007C4 RID: 1988
		public static int mouseClickPolling;

		// Token: 0x040007C5 RID: 1989
		public static int gamePadXButtonPolling;

		// Token: 0x040007C6 RID: 1990
		public static int gamePadAButtonPolling;

		// Token: 0x040007C7 RID: 1991
		public static int weatherIcon;

		// Token: 0x040007C8 RID: 1992
		public static int hitShakeTimer;

		// Token: 0x040007C9 RID: 1993
		public static int staminaShakeTimer;

		// Token: 0x040007CA RID: 1994
		public static int pauseThenDoFunctionTimer;

		// Token: 0x040007CB RID: 1995
		public static int cursorTileHintCheckTimer;

		// Token: 0x040007CC RID: 1996
		public static int timerUntilMouseFade;

		// Token: 0x040007CD RID: 1997
		public static int whichFarm;

		// Token: 0x040007CE RID: 1998
		public static int startingCabins;

		// Token: 0x040007CF RID: 1999
		public static ModFarmType whichModFarm = null;

		// Token: 0x040007D0 RID: 2000
		public static ulong? startingGameSeed = null;

		// Token: 0x040007D1 RID: 2001
		public static int elliottPiano = 0;

		// Token: 0x040007D2 RID: 2002
		public static Microsoft.Xna.Framework.Rectangle viewportClampArea = Microsoft.Xna.Framework.Rectangle.Empty;

		// Token: 0x040007D3 RID: 2003
		public static SaveFixes lastAppliedSaveFix;

		// Token: 0x040007D4 RID: 2004
		public static Color eveningColor = new Color(255, 255, 0);

		// Token: 0x040007D5 RID: 2005
		public static Color unselectedOptionColor = new Color(100, 100, 100);

		// Token: 0x040007D6 RID: 2006
		public static Color screenGlowColor;

		// Token: 0x040007D7 RID: 2007
		public static NPC currentSpeaker;

		// Token: 0x040007D8 RID: 2008
		public static Random random = new Random();

		// Token: 0x040007D9 RID: 2009
		public static Random recentMultiplayerRandom = new Random();

		// Token: 0x040007DA RID: 2010
		public static Dictionary<int, string> achievements;

		// Token: 0x040007DB RID: 2011
		public static IDictionary<string, BigCraftableData> bigCraftableData;

		// Token: 0x040007DC RID: 2012
		public static IDictionary<string, BuildingData> buildingData;

		// Token: 0x040007DD RID: 2013
		public static IDictionary<string, CharacterData> characterData;

		// Token: 0x040007DE RID: 2014
		public static IDictionary<string, CropData> cropData;

		// Token: 0x040007DF RID: 2015
		public static IDictionary<string, FarmAnimalData> farmAnimalData;

		// Token: 0x040007E0 RID: 2016
		public static IDictionary<string, FloorPathData> floorPathData;

		// Token: 0x040007E1 RID: 2017
		public static IDictionary<string, FruitTreeData> fruitTreeData;

		// Token: 0x040007E2 RID: 2018
		public static IDictionary<string, JukeboxTrackData> jukeboxTrackData;

		// Token: 0x040007E3 RID: 2019
		public static IDictionary<string, LocationData> locationData;

		// Token: 0x040007E4 RID: 2020
		public static IDictionary<string, LocationContextData> locationContextData;

		// Token: 0x040007E5 RID: 2021
		public static IDictionary<string, string> NPCGiftTastes;

		// Token: 0x040007E6 RID: 2022
		public static IDictionary<string, ObjectData> objectData;

		// Token: 0x040007E7 RID: 2023
		public static IDictionary<string, PantsData> pantsData;

		// Token: 0x040007E8 RID: 2024
		public static IDictionary<string, PetData> petData;

		// Token: 0x040007E9 RID: 2025
		public static IDictionary<string, ShirtData> shirtData;

		// Token: 0x040007EA RID: 2026
		public static IDictionary<string, ToolData> toolData;

		// Token: 0x040007EB RID: 2027
		public static IDictionary<string, WeaponData> weaponData;

		// Token: 0x040007EC RID: 2028
		public static List<HUDMessage> hudMessages = new List<HUDMessage>();

		// Token: 0x040007ED RID: 2029
		public static float musicPlayerVolume;

		// Token: 0x040007EE RID: 2030
		public static float ambientPlayerVolume;

		// Token: 0x040007EF RID: 2031
		public static float pauseAccumulator;

		// Token: 0x040007F0 RID: 2032
		public static float pauseTime;

		// Token: 0x040007F1 RID: 2033
		public static float upPolling;

		// Token: 0x040007F2 RID: 2034
		public static float downPolling;

		// Token: 0x040007F3 RID: 2035
		public static float rightPolling;

		// Token: 0x040007F4 RID: 2036
		public static float leftPolling;

		// Token: 0x040007F5 RID: 2037
		public static float debrisSoundInterval;

		// Token: 0x040007F6 RID: 2038
		public static float windGust;

		// Token: 0x040007F7 RID: 2039
		public static float dialogueButtonScale = 1f;

		// Token: 0x040007F8 RID: 2040
		public ICue instanceCurrentSong;

		// Token: 0x040007F9 RID: 2041
		public static IAudioCategory musicCategory;

		// Token: 0x040007FA RID: 2042
		public static IAudioCategory soundCategory;

		// Token: 0x040007FB RID: 2043
		public static IAudioCategory ambientCategory;

		// Token: 0x040007FC RID: 2044
		public static IAudioCategory footstepCategory;

		// Token: 0x040007FD RID: 2045
		public PlayerIndex instancePlayerOneIndex;

		// Token: 0x040007FE RID: 2046
		[NonInstancedStatic]
		public static IAudioEngine audioEngine;

		// Token: 0x040007FF RID: 2047
		[NonInstancedStatic]
		public static WaveBank waveBank;

		// Token: 0x04000800 RID: 2048
		[NonInstancedStatic]
		public static WaveBank waveBank1_4;

		// Token: 0x04000801 RID: 2049
		[NonInstancedStatic]
		public static ISoundBank soundBank;

		// Token: 0x04000802 RID: 2050
		public static Vector2 previousViewportPosition;

		// Token: 0x04000803 RID: 2051
		public static Vector2 currentCursorTile;

		// Token: 0x04000804 RID: 2052
		public static Vector2 lastCursorTile = Vector2.Zero;

		// Token: 0x04000805 RID: 2053
		public static Vector2 snowPos;

		// Token: 0x04000806 RID: 2054
		public Microsoft.Xna.Framework.Rectangle localMultiplayerWindow;

		// Token: 0x04000807 RID: 2055
		public static RainDrop[] rainDrops = new RainDrop[70];

		// Token: 0x04000808 RID: 2056
		public static ICue chargeUpSound;

		// Token: 0x04000809 RID: 2057
		public static ICue wind;

		// Token: 0x0400080A RID: 2058
		public static LoopingCueManager loopingLocationCues = new LoopingCueManager();

		// Token: 0x0400080B RID: 2059
		public static ISoundsHelper sounds = new SoundsHelper();

		// Token: 0x0400080C RID: 2060
		[NonInstancedStatic]
		public static AudioCueModificationManager CueModification = new AudioCueModificationManager();

		// Token: 0x0400080D RID: 2061
		public static List<WeatherDebris> debrisWeather = new List<WeatherDebris>();

		// Token: 0x0400080E RID: 2062
		public static TemporaryAnimatedSpriteList screenOverlayTempSprites = new TemporaryAnimatedSpriteList();

		// Token: 0x0400080F RID: 2063
		public static TemporaryAnimatedSpriteList uiOverlayTempSprites = new TemporaryAnimatedSpriteList();

		// Token: 0x04000810 RID: 2064
		private static byte _gameMode;

		// Token: 0x04000812 RID: 2066
		private bool _isSaving;

		// Token: 0x04000813 RID: 2067
		[NonInstancedStatic]
		protected internal static IGameLogger log = new DefaultLogger(!Program.releaseBuild, false);

		// Token: 0x04000814 RID: 2068
		[NonInstancedStatic]
		public static IHashUtility hash = new HashUtility();

		// Token: 0x04000815 RID: 2069
		protected internal static Multiplayer multiplayer = new Multiplayer();

		// Token: 0x04000816 RID: 2070
		public static byte multiplayerMode;

		// Token: 0x04000817 RID: 2071
		public static IEnumerator<int> currentLoader;

		// Token: 0x04000818 RID: 2072
		public static ulong uniqueIDForThisGame = Utility.NewUniqueIdForThisGame();

		// Token: 0x04000819 RID: 2073
		public static int[] directionKeyPolling = new int[4];

		// Token: 0x0400081A RID: 2074
		public static Dictionary<string, LightSource> currentLightSources = new Dictionary<string, LightSource>();

		// Token: 0x0400081B RID: 2075
		public static Color ambientLight;

		// Token: 0x0400081C RID: 2076
		public static Color outdoorLight = new Color(255, 255, 0);

		// Token: 0x0400081D RID: 2077
		public static Color textColor = new Color(34, 17, 34);

		// Token: 0x0400081E RID: 2078
		public static Color textShadowColor = new Color(206, 156, 95);

		// Token: 0x0400081F RID: 2079
		public static Color textShadowDarkerColor = new Color(221, 148, 84);

		// Token: 0x04000820 RID: 2080
		public static IClickableMenu overlayMenu;

		// Token: 0x04000821 RID: 2081
		private static IClickableMenu _activeClickableMenu;

		// Token: 0x04000822 RID: 2082
		public static List<IClickableMenu> nextClickableMenu = new List<IClickableMenu>();

		// Token: 0x04000823 RID: 2083
		public static List<Action> actionsWhenPlayerFree = new List<Action>();

		// Token: 0x04000824 RID: 2084
		public static bool isCheckingNonMousePlacement = false;

		// Token: 0x04000825 RID: 2085
		private static IMinigame _currentMinigame = null;

		// Token: 0x04000826 RID: 2086
		public static IList<IClickableMenu> onScreenMenus = new List<IClickableMenu>();

		// Token: 0x04000827 RID: 2087
		public static BuffsDisplay buffsDisplay;

		// Token: 0x04000828 RID: 2088
		public static DayTimeMoneyBox dayTimeMoneyBox;

		// Token: 0x04000829 RID: 2089
		public static NetRootDictionary<long, Farmer> otherFarmers;

		// Token: 0x0400082A RID: 2090
		private static readonly FarmerCollection _onlineFarmers = new FarmerCollection(null);

		// Token: 0x0400082B RID: 2091
		public static IGameServer server;

		// Token: 0x0400082C RID: 2092
		public static Client client;

		// Token: 0x0400082D RID: 2093
		public KeyboardDispatcher instanceKeyboardDispatcher;

		// Token: 0x0400082E RID: 2094
		public static Background background;

		// Token: 0x0400082F RID: 2095
		public static FarmEvent farmEvent;

		// Token: 0x04000830 RID: 2096
		public static FarmEvent farmEventOverride;

		// Token: 0x04000831 RID: 2097
		public static Game1.afterFadeFunction afterFade;

		// Token: 0x04000832 RID: 2098
		public static Game1.afterFadeFunction afterDialogues;

		// Token: 0x04000833 RID: 2099
		public static Game1.afterFadeFunction afterViewport;

		// Token: 0x04000834 RID: 2100
		public static Game1.afterFadeFunction viewportReachedTarget;

		// Token: 0x04000835 RID: 2101
		public static Game1.afterFadeFunction afterPause;

		// Token: 0x04000836 RID: 2102
		public static GameTime currentGameTime;

		// Token: 0x04000837 RID: 2103
		public static IList<DelayedAction> delayedActions = new List<DelayedAction>();

		// Token: 0x04000838 RID: 2104
		public static Stack<IClickableMenu> endOfNightMenus = new Stack<IClickableMenu>();

		// Token: 0x04000839 RID: 2105
		public Options instanceOptions;

		// Token: 0x0400083A RID: 2106
		[NonInstancedStatic]
		public static SerializableDictionary<long, Options> splitscreenOptions = new SerializableDictionary<long, Options>();

		// Token: 0x0400083B RID: 2107
		public static Game1 game1;

		// Token: 0x0400083C RID: 2108
		public static Point lastMousePositionBeforeFade;

		// Token: 0x0400083D RID: 2109
		public static int ticks;

		// Token: 0x0400083E RID: 2110
		public static EmoteMenu emoteMenu;

		// Token: 0x0400083F RID: 2111
		[NonInstancedStatic]
		public static SerializableDictionary<string, string> CustomData = new SerializableDictionary<string, string>();

		// Token: 0x04000840 RID: 2112
		public static ReadySynchronizer netReady = new ReadySynchronizer();

		// Token: 0x04000841 RID: 2113
		public static DedicatedServer dedicatedServer = new DedicatedServer();

		// Token: 0x04000842 RID: 2114
		public static NetRoot<NetWorldState> netWorldState;

		// Token: 0x04000843 RID: 2115
		public static ChatBox chatBox;

		// Token: 0x04000844 RID: 2116
		public TextEntryMenu instanceTextEntry;

		// Token: 0x04000845 RID: 2117
		public static SpecialCurrencyDisplay specialCurrencyDisplay = null;

		// Token: 0x04000846 RID: 2118
		private static string debugPresenceString;

		// Token: 0x04000847 RID: 2119
		public static List<Action> remoteEventQueue = new List<Action>();

		// Token: 0x04000848 RID: 2120
		public static List<long> weddingsToday = new List<long>();

		// Token: 0x04000849 RID: 2121
		public int instanceIndex;

		// Token: 0x0400084A RID: 2122
		public int instanceId;

		// Token: 0x0400084B RID: 2123
		public static bool overrideGameMenuReset;

		// Token: 0x0400084C RID: 2124
		protected bool _windowResizing;

		// Token: 0x0400084D RID: 2125
		protected Point _oldMousePosition;

		// Token: 0x0400084E RID: 2126
		protected bool _oldGamepadConnectedState;

		// Token: 0x0400084F RID: 2127
		protected int _oldScrollWheelValue;

		// Token: 0x04000850 RID: 2128
		public static Point viewportCenter;

		// Token: 0x04000851 RID: 2129
		public static Vector2 viewportTarget = new Vector2(-2.1474836E+09f, -2.1474836E+09f);

		// Token: 0x04000852 RID: 2130
		public static float viewportSpeed = 2f;

		// Token: 0x04000853 RID: 2131
		public static int viewportHold;

		// Token: 0x04000854 RID: 2132
		private static bool _cursorDragEnabled = false;

		// Token: 0x04000855 RID: 2133
		private static bool _cursorDragPrevEnabled = false;

		// Token: 0x04000856 RID: 2134
		private static bool _cursorSpeedDirty = true;

		// Token: 0x04000857 RID: 2135
		private const float CursorBaseSpeed = 16f;

		// Token: 0x04000858 RID: 2136
		private static float _cursorSpeed = 16f;

		// Token: 0x04000859 RID: 2137
		private static float _cursorSpeedScale = 1f;

		// Token: 0x0400085A RID: 2138
		private static float _cursorUpdateElapsedSec = 0f;

		// Token: 0x0400085B RID: 2139
		private static int thumbstickPollingTimer;

		// Token: 0x0400085C RID: 2140
		public static bool toggleFullScreen;

		// Token: 0x0400085D RID: 2141
		public static string whereIsTodaysFest;

		// Token: 0x0400085E RID: 2142
		public const string NO_LETTER_MAIL = "%&NL&%";

		// Token: 0x0400085F RID: 2143
		public const string BROADCAST_MAIL_FOR_TOMORROW_PREFIX = "%&MFT&%";

		// Token: 0x04000860 RID: 2144
		public const string BROADCAST_SEEN_MAIL_PREFIX = "%&SM&%";

		// Token: 0x04000861 RID: 2145
		public const string BROADCAST_MAILBOX_PREFIX = "%&MB&%";

		// Token: 0x04000862 RID: 2146
		public bool isLocalMultiplayerNewDayActive;

		// Token: 0x04000863 RID: 2147
		protected static Task _newDayTask;

		// Token: 0x04000864 RID: 2148
		private static Action _afterNewDayAction;

		// Token: 0x04000865 RID: 2149
		public static NewDaySynchronizer newDaySync = new NewDaySynchronizer();

		// Token: 0x04000866 RID: 2150
		public static bool forceSnapOnNextViewportUpdate = false;

		// Token: 0x04000867 RID: 2151
		public static Vector2 currentViewportTarget;

		// Token: 0x04000868 RID: 2152
		public static Vector2 viewportPositionLerp;

		// Token: 0x04000869 RID: 2153
		public static float screenGlowRate = 0.005f;

		// Token: 0x0400086A RID: 2154
		public static float screenGlowMax;

		// Token: 0x0400086B RID: 2155
		public static bool haltAfterCheck = false;

		// Token: 0x0400086C RID: 2156
		public static bool uiMode = false;

		// Token: 0x0400086D RID: 2157
		public static RenderTarget2D nonUIRenderTarget = null;

		// Token: 0x0400086E RID: 2158
		public static int uiModeCount = 0;

		// Token: 0x0400086F RID: 2159
		protected static int _oldUIModeCount = 0;

		// Token: 0x04000870 RID: 2160
		internal string panModeString;

		// Token: 0x04000871 RID: 2161
		public static bool conventionMode = false;

		// Token: 0x04000872 RID: 2162
		internal static EventTest eventTest;

		// Token: 0x04000873 RID: 2163
		internal bool panFacingDirectionWait;

		// Token: 0x04000874 RID: 2164
		public static bool isRunningMacro = false;

		// Token: 0x04000875 RID: 2165
		public static int thumbstickMotionMargin;

		// Token: 0x04000876 RID: 2166
		public static float thumbstickMotionAccell = 1f;

		// Token: 0x04000877 RID: 2167
		public static int triggerPolling;

		// Token: 0x04000878 RID: 2168
		public static int rightClickPolling;

		// Token: 0x04000879 RID: 2169
		private RenderTarget2D _screen;

		// Token: 0x0400087A RID: 2170
		private RenderTarget2D _uiScreen;

		// Token: 0x0400087B RID: 2171
		public static Color bgColor = new Color(5, 3, 4);

		// Token: 0x0400087C RID: 2172
		protected readonly BlendState lightingBlend = new BlendState
		{
			ColorBlendFunction = BlendFunction.ReverseSubtract,
			ColorDestinationBlend = Blend.One,
			ColorSourceBlend = Blend.SourceColor
		};

		// Token: 0x0400087D RID: 2173
		public bool isDrawing;

		// Token: 0x0400087E RID: 2174
		[NonInstancedStatic]
		public static bool isRenderingScreenBuffer = false;

		// Token: 0x0400087F RID: 2175
		protected bool _lastDrewMouseCursor;

		// Token: 0x04000880 RID: 2176
		protected static int _activatedTick = 0;

		// Token: 0x04000881 RID: 2177
		public static int mouseCursor = Game1.cursor_default;

		// Token: 0x04000882 RID: 2178
		private static float _mouseCursorTransparency = 1f;

		// Token: 0x04000883 RID: 2179
		public static bool wasMouseVisibleThisFrame = true;

		// Token: 0x04000884 RID: 2180
		public static NPC objectDialoguePortraitPerson;

		// Token: 0x04000885 RID: 2181
		protected static StringBuilder _ParseTextStringBuilder = new StringBuilder(2408);

		// Token: 0x04000886 RID: 2182
		protected static StringBuilder _ParseTextStringBuilderLine = new StringBuilder(1024);

		// Token: 0x04000887 RID: 2183
		protected static StringBuilder _ParseTextStringBuilderWord = new StringBuilder(256);

		// Token: 0x04000888 RID: 2184
		public bool ScreenshotBusy;

		// Token: 0x04000889 RID: 2185
		public bool takingMapScreenshot;

		// Token: 0x02000438 RID: 1080
		public enum BundleType
		{
			// Token: 0x0400279A RID: 10138
			Default,
			// Token: 0x0400279B RID: 10139
			Remixed
		}

		// Token: 0x02000439 RID: 1081
		public enum MineChestType
		{
			// Token: 0x0400279D RID: 10141
			Default,
			// Token: 0x0400279E RID: 10142
			Remixed
		}

		// Token: 0x0200043A RID: 1082
		// (Invoke) Token: 0x06003D08 RID: 15624
		public delegate void afterFadeFunction();
	}
}

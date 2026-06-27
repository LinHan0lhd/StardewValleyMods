using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;
using StardewValley.Monsters;
using xTile.Layers;

namespace StardewValley.Locations
{
	// Token: 0x020002C3 RID: 707
	public class BugLand : GameLocation
	{
		// Token: 0x06002DE7 RID: 11751 RVA: 0x0023EB30 File Offset: 0x0023CD30
		public BugLand()
		{
		}

		// Token: 0x06002DE8 RID: 11752 RVA: 0x0023EB38 File Offset: 0x0023CD38
		public BugLand(string map, string name) : base(map, name)
		{
		}

		// Token: 0x06002DE9 RID: 11753 RVA: 0x0023EB44 File Offset: 0x0023CD44
		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			BugLand bugLand = l as BugLand;
			if (bugLand != null)
			{
				this.hasSpawnedBugsToday = bugLand.hasSpawnedBugsToday;
			}
			base.TransferDataFromSavedLocation(l);
		}

		// Token: 0x06002DEA RID: 11754 RVA: 0x0023EB6E File Offset: 0x0023CD6E
		public override void hostSetup()
		{
			base.hostSetup();
			if (Game1.IsMasterGame && !this.hasSpawnedBugsToday)
			{
				this.InitializeBugLand();
			}
		}

		// Token: 0x06002DEB RID: 11755 RVA: 0x0023EB8B File Offset: 0x0023CD8B
		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			this.characters.RemoveWhere((NPC npc) => npc is Grub || npc is Fly);
			this.hasSpawnedBugsToday = false;
		}

		// Token: 0x06002DEC RID: 11756 RVA: 0x0023EBC8 File Offset: 0x0023CDC8
		public virtual void InitializeBugLand()
		{
			if (this.hasSpawnedBugsToday)
			{
				return;
			}
			this.hasSpawnedBugsToday = true;
			Layer pathsLayer = this.map.RequireLayer("Paths");
			for (int x = 0; x < this.map.Layers[0].LayerWidth; x++)
			{
				for (int y = 0; y < this.map.Layers[0].LayerHeight; y++)
				{
					if (Game1.random.NextDouble() < 0.33)
					{
						int tileIndex = pathsLayer.GetTileIndexAt(x, y, null);
						if (tileIndex != -1)
						{
							Vector2 tile = new Vector2((float)x, (float)y);
							switch (tileIndex)
							{
							case 13:
							case 14:
							case 15:
								if (!this.objects.ContainsKey(tile))
								{
									this.objects.Add(tile, ItemRegistry.Create<Object>(GameLocation.getWeedForSeason(Game1.random, Season.Spring), 1, 0, false));
								}
								break;
							case 16:
								if (!this.objects.ContainsKey(tile))
								{
									this.objects.Add(tile, ItemRegistry.Create<Object>(Game1.random.Choose("(O)343", "(O)450"), 1, 0, false));
								}
								break;
							case 17:
								if (!this.objects.ContainsKey(tile))
								{
									this.objects.Add(tile, ItemRegistry.Create<Object>(Game1.random.Choose("(O)343", "(O)450"), 1, 0, false));
								}
								break;
							case 18:
								if (!this.objects.ContainsKey(tile))
								{
									this.objects.Add(tile, ItemRegistry.Create<Object>(Game1.random.Choose("(O)294", "(O)295"), 1, 0, false));
								}
								break;
							default:
								if (tileIndex == 28)
								{
									if (this.CanSpawnCharacterHere(tile) && this.characters.Count < 50)
									{
										this.characters.Add(new Grub(new Vector2(tile.X * 64f, tile.Y * 64f), true));
									}
								}
								break;
							}
						}
					}
				}
			}
		}

		// Token: 0x04001F76 RID: 8054
		[XmlElement("hasSpawnedBugsToday")]
		public bool hasSpawnedBugsToday;
	}
}

using System;
using System.Collections.Generic;
using StardewValley.Locations;

namespace StardewValley.Pathfinding
{
	// Token: 0x020001A0 RID: 416
	public static class WarpPathfindingCache
	{
		// Token: 0x06001D6D RID: 7533 RVA: 0x00151058 File Offset: 0x0014F258
		public static void PopulateCache()
		{
			for (int i = 1; i <= Game1.netWorldState.Value.HighestPlayerLimit; i++)
			{
				WarpPathfindingCache.IgnoreLocationNames.Add("Cellar" + i.ToString());
			}
			WarpPathfindingCache.Routes.Clear();
			foreach (GameLocation j in Game1.locations)
			{
				if (!WarpPathfindingCache.IgnoreLocationNames.Contains(j.NameOrUniqueName))
				{
					WarpPathfindingCache.ExploreWarpPoints(j, new List<string>(), null);
				}
			}
		}

		// Token: 0x06001D6E RID: 7534 RVA: 0x00151104 File Offset: 0x0014F304
		public static string[] GetLocationRoute(string startingLocation, string endingLocation, Gender gender)
		{
			List<LocationWarpRoute> routes;
			if (WarpPathfindingCache.Routes.TryGetValue(startingLocation, out routes))
			{
				foreach (LocationWarpRoute route in routes)
				{
					if (route.LocationNames[route.LocationNames.Length - 1] == endingLocation)
					{
						if (route.OnlyGender != null)
						{
							Gender? onlyGender = route.OnlyGender;
							if (!(onlyGender.GetValueOrDefault() == gender & onlyGender != null) && gender != Gender.Undefined)
							{
								continue;
							}
						}
						return route.LocationNames;
					}
				}
			}
			return null;
		}

		// Token: 0x06001D6F RID: 7535 RVA: 0x001511B8 File Offset: 0x0014F3B8
		private static void ExploreWarpPoints(GameLocation location, List<string> route, Gender? genderRestriction)
		{
			string locationName = (location != null) ? location.name.Value : null;
			if (locationName == null || location.ShouldExcludeFromNpcPathfinding() || route.Contains(locationName))
			{
				return;
			}
			Gender newGenderRestriction;
			if (WarpPathfindingCache.GenderRestrictions.TryGetValue(locationName, out newGenderRestriction))
			{
				if (genderRestriction != null && genderRestriction.Value != newGenderRestriction)
				{
					return;
				}
				genderRestriction = new Gender?(newGenderRestriction);
			}
			route.Add(locationName);
			if (route.Count > 1)
			{
				WarpPathfindingCache.AddRoute(route, genderRestriction);
			}
			bool hasWarps = location.warps.Count > 0;
			bool hasDoors = location.doors.Length > 0;
			if (hasWarps || hasDoors)
			{
				HashSet<string> exploredTargets = new HashSet<string>
				{
					locationName
				};
				if (route.Count > 1)
				{
					exploredTargets.Add(route[route.Count - 2]);
				}
				if (hasWarps)
				{
					foreach (Warp warp in location.warps)
					{
						WarpPathfindingCache.ExploreWarpPoints(warp.TargetName, route, genderRestriction, exploredTargets);
					}
				}
				if (hasDoors)
				{
					foreach (string locationName2 in location.doors.Values)
					{
						WarpPathfindingCache.ExploreWarpPoints(locationName2, route, genderRestriction, exploredTargets);
					}
				}
			}
			if (route.Count > 0)
			{
				route.RemoveAt(route.Count - 1);
			}
		}

		// Token: 0x06001D70 RID: 7536 RVA: 0x0015133C File Offset: 0x0014F53C
		private static void ExploreWarpPoints(string locationName, List<string> route, Gender? genderRestriction, HashSet<string> seenTargets)
		{
			string newLocationName;
			if (WarpPathfindingCache.OverrideTargetNames.TryGetValue(locationName, out newLocationName))
			{
				locationName = newLocationName;
			}
			if (seenTargets.Add(locationName) && !WarpPathfindingCache.IgnoreLocationNames.Contains(locationName) && !MineShaft.IsGeneratedLevel(locationName) && !VolcanoDungeon.IsGeneratedLevel(locationName))
			{
				WarpPathfindingCache.ExploreWarpPoints(Game1.getLocationFromName(locationName), route, genderRestriction);
			}
		}

		// Token: 0x06001D71 RID: 7537 RVA: 0x00151390 File Offset: 0x0014F590
		private static void AddRoute(List<string> route, Gender? onlyGender)
		{
			List<LocationWarpRoute> routes;
			if (!WarpPathfindingCache.Routes.TryGetValue(route[0], out routes))
			{
				routes = (WarpPathfindingCache.Routes[route[0]] = new List<LocationWarpRoute>());
			}
			routes.Add(new LocationWarpRoute(route.ToArray(), onlyGender));
		}

		// Token: 0x06001D72 RID: 7538 RVA: 0x001513DC File Offset: 0x0014F5DC
		// Note: this type is marked as 'beforefieldinit'.
		static WarpPathfindingCache()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary["BoatTunnel"] = "IslandSouth";
			WarpPathfindingCache.OverrideTargetNames = dictionary;
			Dictionary<string, Gender> dictionary2 = new Dictionary<string, Gender>();
			dictionary2["BathHouse_MensLocker"] = Gender.Male;
			dictionary2["BathHouse_WomensLocker"] = Gender.Female;
			WarpPathfindingCache.GenderRestrictions = dictionary2;
		}

		// Token: 0x04001237 RID: 4663
		private static readonly Dictionary<string, List<LocationWarpRoute>> Routes = new Dictionary<string, List<LocationWarpRoute>>();

		// Token: 0x04001238 RID: 4664
		public static readonly HashSet<string> IgnoreLocationNames = new HashSet<string>
		{
			"Backwoods",
			"Cellar",
			"Farm"
		};

		// Token: 0x04001239 RID: 4665
		public static readonly Dictionary<string, string> OverrideTargetNames;

		// Token: 0x0400123A RID: 4666
		public static readonly Dictionary<string, Gender> GenderRestrictions;
	}
}

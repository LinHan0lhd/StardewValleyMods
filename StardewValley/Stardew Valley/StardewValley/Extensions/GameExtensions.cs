using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Netcode;
using StardewValley.Network;

namespace StardewValley.Extensions
{
	// Token: 0x0200031C RID: 796
	public static class GameExtensions
	{
		// Token: 0x06003467 RID: 13415 RVA: 0x0029CB58 File Offset: 0x0029AD58
		public static void Add(this IDictionary<string, LightSource> dictionary, LightSource lightSource)
		{
			if (lightSource != null)
			{
				if (string.IsNullOrWhiteSpace(lightSource.Id))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 1);
					defaultInterpolatedStringHandler.AppendLiteral("LightSource_TempId_");
					defaultInterpolatedStringHandler.AppendFormatted<int>(Game1.random.Next());
					lightSource.Id = defaultInterpolatedStringHandler.ToStringAndClear();
					Game1.log.Warn("Light source has no ID; assigning ID '" + lightSource.Id + "'.");
				}
				dictionary[lightSource.Id] = lightSource;
			}
		}

		// Token: 0x06003468 RID: 13416 RVA: 0x0029CBD8 File Offset: 0x0029ADD8
		public static void AddLight(this NetStringDictionary<LightSource, NetRef<LightSource>> dictionary, LightSource lightSource)
		{
			if (lightSource != null)
			{
				if (string.IsNullOrWhiteSpace(lightSource.Id))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 1);
					defaultInterpolatedStringHandler.AppendLiteral("LightSource_TempId_");
					defaultInterpolatedStringHandler.AppendFormatted<int>(Game1.random.Next());
					lightSource.Id = defaultInterpolatedStringHandler.ToStringAndClear();
					Game1.log.Warn("Light source has no ID; assigning ID '" + lightSource.Id + "'.");
				}
				dictionary[lightSource.Id] = lightSource;
			}
		}
	}
}

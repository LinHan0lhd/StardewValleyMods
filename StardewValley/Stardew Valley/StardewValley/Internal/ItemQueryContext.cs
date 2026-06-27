using System;
using System.Collections.Generic;

namespace StardewValley.Internal
{
	// Token: 0x02000310 RID: 784
	public class ItemQueryContext
	{
		// Token: 0x17000453 RID: 1107
		// (get) Token: 0x06003418 RID: 13336 RVA: 0x0029B03F File Offset: 0x0029923F
		public GameLocation Location { get; }

		// Token: 0x17000454 RID: 1108
		// (get) Token: 0x06003419 RID: 13337 RVA: 0x0029B047 File Offset: 0x00299247
		public Farmer Player { get; }

		// Token: 0x17000455 RID: 1109
		// (get) Token: 0x0600341A RID: 13338 RVA: 0x0029B04F File Offset: 0x0029924F
		public Random Random { get; }

		// Token: 0x17000456 RID: 1110
		// (get) Token: 0x0600341B RID: 13339 RVA: 0x0029B057 File Offset: 0x00299257
		// (set) Token: 0x0600341C RID: 13340 RVA: 0x0029B05F File Offset: 0x0029925F
		public string QueryString { get; internal set; }

		// Token: 0x17000457 RID: 1111
		// (get) Token: 0x0600341D RID: 13341 RVA: 0x0029B068 File Offset: 0x00299268
		public ItemQueryContext ParentContext { get; }

		// Token: 0x17000458 RID: 1112
		// (get) Token: 0x0600341E RID: 13342 RVA: 0x0029B070 File Offset: 0x00299270
		// (set) Token: 0x0600341F RID: 13343 RVA: 0x0029B078 File Offset: 0x00299278
		public string SourcePhrase { get; set; }

		// Token: 0x17000459 RID: 1113
		// (get) Token: 0x06003420 RID: 13344 RVA: 0x0029B081 File Offset: 0x00299281
		// (set) Token: 0x06003421 RID: 13345 RVA: 0x0029B089 File Offset: 0x00299289
		public Dictionary<string, object> CustomFields { get; set; }

		// Token: 0x06003422 RID: 13346 RVA: 0x0029B092 File Offset: 0x00299292
		public ItemQueryContext() : this(null, null, null, null)
		{
		}

		// Token: 0x06003423 RID: 13347 RVA: 0x0029B0A0 File Offset: 0x002992A0
		public ItemQueryContext(ItemQueryContext parentContext, string sourceLabel = null) : this((parentContext != null) ? parentContext.Location : null, (parentContext != null) ? parentContext.Player : null, (parentContext != null) ? parentContext.Random : null, (parentContext != null) ? parentContext.SourcePhrase : null)
		{
			this.ParentContext = parentContext;
			if (sourceLabel != null)
			{
				this.SourcePhrase = ((parentContext != null && parentContext.SourcePhrase != null) ? (parentContext.SourcePhrase + " > " + sourceLabel) : sourceLabel);
			}
		}

		// Token: 0x06003424 RID: 13348 RVA: 0x0029B112 File Offset: 0x00299312
		public ItemQueryContext(GameLocation location, Farmer player, Random random, string sourcePhrase)
		{
			this.Location = (location ?? Game1.currentLocation);
			this.Player = (player ?? Game1.player);
			this.Random = random;
			this.SourcePhrase = sourcePhrase;
		}
	}
}

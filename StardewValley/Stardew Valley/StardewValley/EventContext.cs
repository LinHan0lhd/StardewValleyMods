using System;
using Microsoft.Xna.Framework;

namespace StardewValley
{
	// Token: 0x020000A1 RID: 161
	public class EventContext
	{
		// Token: 0x170000E4 RID: 228
		// (get) Token: 0x06000792 RID: 1938 RVA: 0x0004D899 File Offset: 0x0004BA99
		public Event Event { get; }

		// Token: 0x170000E5 RID: 229
		// (get) Token: 0x06000793 RID: 1939 RVA: 0x0004D8A1 File Offset: 0x0004BAA1
		public GameLocation Location { get; }

		// Token: 0x170000E6 RID: 230
		// (get) Token: 0x06000794 RID: 1940 RVA: 0x0004D8A9 File Offset: 0x0004BAA9
		public GameTime Time { get; }

		// Token: 0x170000E7 RID: 231
		// (get) Token: 0x06000795 RID: 1941 RVA: 0x0004D8B1 File Offset: 0x0004BAB1
		public string[] Args { get; }

		// Token: 0x06000796 RID: 1942 RVA: 0x0004D8B9 File Offset: 0x0004BAB9
		public EventContext(Event @event, GameLocation location, GameTime time, string[] args)
		{
			this.Event = @event;
			this.Location = location;
			this.Time = time;
			this.Args = args;
		}

		// Token: 0x06000797 RID: 1943 RVA: 0x0004D8DE File Offset: 0x0004BADE
		public void LogError(string error, bool willSkip = false)
		{
			this.Event.LogCommandError(this.Args, error, willSkip);
		}

		// Token: 0x06000798 RID: 1944 RVA: 0x0004D8F3 File Offset: 0x0004BAF3
		public void LogErrorAndSkip(string error, bool hideError = false)
		{
			this.Event.LogCommandErrorAndSkip(this.Args, error, hideError);
		}
	}
}

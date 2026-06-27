using System;
using System.Collections.Generic;
using System.IO;

namespace StardewValley
{
	// Token: 0x020000E4 RID: 228
	public class NetLogger
	{
		// Token: 0x170001F2 RID: 498
		// (get) Token: 0x06001159 RID: 4441 RVA: 0x000CB50C File Offset: 0x000C970C
		// (set) Token: 0x0600115A RID: 4442 RVA: 0x000CB514 File Offset: 0x000C9714
		public bool IsLogging
		{
			get
			{
				return this.isLogging;
			}
			set
			{
				if (value == this.isLogging)
				{
					return;
				}
				this.isLogging = value;
				if (this.isLogging)
				{
					this.timeLastStarted = DateTime.UtcNow;
					return;
				}
				this.priorMillis += (DateTime.UtcNow - this.timeLastStarted).TotalMilliseconds;
			}
		}

		// Token: 0x170001F3 RID: 499
		// (get) Token: 0x0600115B RID: 4443 RVA: 0x000CB56C File Offset: 0x000C976C
		public double LogDuration
		{
			get
			{
				if (this.isLogging)
				{
					return this.priorMillis + (DateTime.UtcNow - this.timeLastStarted).TotalMilliseconds;
				}
				return this.priorMillis;
			}
		}

		// Token: 0x0600115C RID: 4444 RVA: 0x000CB5A8 File Offset: 0x000C97A8
		public void LogWrite(string path, long length)
		{
			if (!this.IsLogging)
			{
				return;
			}
			NetLogRecord record;
			this.loggedWrites.TryGetValue(path, out record);
			record.Path = path;
			record.Count++;
			record.Bytes += length;
			this.loggedWrites[path] = record;
		}

		// Token: 0x0600115D RID: 4445 RVA: 0x000CB5FA File Offset: 0x000C97FA
		public void Clear()
		{
			this.loggedWrites.Clear();
			this.priorMillis = 0.0;
			this.timeLastStarted = DateTime.UtcNow;
		}

		// Token: 0x0600115E RID: 4446 RVA: 0x000CB624 File Offset: 0x000C9824
		public string Dump()
		{
			string path = Path.Combine(Program.GetLocalAppDataFolder("Profiling", true), DateTime.UtcNow.Ticks.ToString() + ".csv");
			using (StreamWriter writer = File.CreateText(path))
			{
				double duration = this.LogDuration / 1000.0;
				writer.WriteLine("Profile Duration: {0:F2}", duration);
				writer.WriteLine("Stack,Deltas,Bytes,Deltas/s,Bytes/s,Bytes/Delta");
				foreach (NetLogRecord record in this.loggedWrites.Values)
				{
					writer.WriteLine("{0:F2},{1:F2},{2:F2},{3:F2},{4:F2},{5:F2}", new object[]
					{
						record.Path,
						record.Count,
						record.Bytes,
						(double)record.Count / duration,
						(double)record.Bytes / duration,
						(double)record.Bytes / (double)record.Count
					});
				}
			}
			return path;
		}

		// Token: 0x04000A5B RID: 2651
		private Dictionary<string, NetLogRecord> loggedWrites = new Dictionary<string, NetLogRecord>();

		// Token: 0x04000A5C RID: 2652
		private DateTime timeLastStarted;

		// Token: 0x04000A5D RID: 2653
		private double priorMillis;

		// Token: 0x04000A5E RID: 2654
		private bool isLogging;
	}
}

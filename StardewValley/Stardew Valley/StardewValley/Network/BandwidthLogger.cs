using System;
using System.Collections.Generic;

namespace StardewValley.Network
{
	// Token: 0x020001C6 RID: 454
	public class BandwidthLogger
	{
		// Token: 0x06002012 RID: 8210 RVA: 0x0016E16C File Offset: 0x0016C36C
		public void Update()
		{
			double msElapsed = (DateTime.UtcNow - this.lastUpdateTime).TotalMilliseconds;
			if (msElapsed > 1000.0)
			{
				this.lastBitsDownPerSecond = (double)this.bitsDownSinceLastUpdate / msElapsed * 1000.0;
				this.lastBitsUpPerSecond = (double)this.bitsUpSinceLastUpdate / msElapsed * 1000.0;
				double num = this.avgBitsDownPerSecond * (double)this.bitsDownPerSecondCount + this.lastBitsDownPerSecond;
				long num2 = this.bitsDownPerSecondCount + 1L;
				this.bitsDownPerSecondCount = num2;
				this.avgBitsDownPerSecond = num / (double)num2;
				double num3 = this.avgBitsUpPerSecond * (double)this.bitsUpPerSecondCount + this.lastBitsUpPerSecond;
				num2 = this.bitsUpPerSecondCount + 1L;
				this.bitsUpPerSecondCount = num2;
				this.avgBitsUpPerSecond = num3 / (double)num2;
				this.lastUpdateTime = DateTime.UtcNow;
				this.bitsDownSinceLastUpdate = 0L;
				this.bitsUpSinceLastUpdate = 0L;
				this.totalMs += msElapsed;
				if (this.bitsUp.Count >= this.queueCapacity)
				{
					this.bitsUp.Dequeue();
				}
				if (this.bitsDown.Count >= this.queueCapacity)
				{
					this.bitsDown.Dequeue();
				}
				this.bitsUp.Enqueue(this.lastBitsUpPerSecond);
				this.bitsDown.Enqueue(this.lastBitsDownPerSecond);
			}
		}

		// Token: 0x1700033B RID: 827
		// (get) Token: 0x06002013 RID: 8211 RVA: 0x0016E2BA File Offset: 0x0016C4BA
		public double AvgBitsDownPerSecond
		{
			get
			{
				return this.avgBitsDownPerSecond;
			}
		}

		// Token: 0x1700033C RID: 828
		// (get) Token: 0x06002014 RID: 8212 RVA: 0x0016E2C2 File Offset: 0x0016C4C2
		public double AvgBitsUpPerSecond
		{
			get
			{
				return this.avgBitsUpPerSecond;
			}
		}

		// Token: 0x1700033D RID: 829
		// (get) Token: 0x06002015 RID: 8213 RVA: 0x0016E2CA File Offset: 0x0016C4CA
		public double BitsDownPerSecond
		{
			get
			{
				return this.lastBitsDownPerSecond;
			}
		}

		// Token: 0x1700033E RID: 830
		// (get) Token: 0x06002016 RID: 8214 RVA: 0x0016E2D2 File Offset: 0x0016C4D2
		public double BitsUpPerSecond
		{
			get
			{
				return this.lastBitsUpPerSecond;
			}
		}

		// Token: 0x1700033F RID: 831
		// (get) Token: 0x06002017 RID: 8215 RVA: 0x0016E2DA File Offset: 0x0016C4DA
		public double TotalBitsDown
		{
			get
			{
				return (double)this.totalBitsDown;
			}
		}

		// Token: 0x17000340 RID: 832
		// (get) Token: 0x06002018 RID: 8216 RVA: 0x0016E2E3 File Offset: 0x0016C4E3
		public double TotalBitsUp
		{
			get
			{
				return (double)this.totalBitsUp;
			}
		}

		// Token: 0x17000341 RID: 833
		// (get) Token: 0x06002019 RID: 8217 RVA: 0x0016E2EC File Offset: 0x0016C4EC
		public double TotalMs
		{
			get
			{
				return this.totalMs;
			}
		}

		// Token: 0x17000342 RID: 834
		// (get) Token: 0x0600201A RID: 8218 RVA: 0x0016E2F4 File Offset: 0x0016C4F4
		public Queue<double> LoggedAvgBitsUp
		{
			get
			{
				return this.bitsUp;
			}
		}

		// Token: 0x17000343 RID: 835
		// (get) Token: 0x0600201B RID: 8219 RVA: 0x0016E2FC File Offset: 0x0016C4FC
		public Queue<double> LoggedAvgBitsDown
		{
			get
			{
				return this.bitsDown;
			}
		}

		// Token: 0x0600201C RID: 8220 RVA: 0x0016E304 File Offset: 0x0016C504
		public void RecordBytesDown(long bytes)
		{
			this.bitsDownSinceLastUpdate += bytes * 8L;
			this.totalBitsDown += bytes * 8L;
		}

		// Token: 0x0600201D RID: 8221 RVA: 0x0016E328 File Offset: 0x0016C528
		public void RecordBytesUp(long bytes)
		{
			this.bitsUpSinceLastUpdate += bytes * 8L;
			this.totalBitsUp += bytes * 8L;
		}

		// Token: 0x0400138D RID: 5005
		private long bitsDownSinceLastUpdate;

		// Token: 0x0400138E RID: 5006
		private long bitsUpSinceLastUpdate;

		// Token: 0x0400138F RID: 5007
		private DateTime lastUpdateTime = DateTime.UtcNow;

		// Token: 0x04001390 RID: 5008
		private double lastBitsDownPerSecond;

		// Token: 0x04001391 RID: 5009
		private double lastBitsUpPerSecond;

		// Token: 0x04001392 RID: 5010
		private double avgBitsUpPerSecond;

		// Token: 0x04001393 RID: 5011
		private long bitsUpPerSecondCount;

		// Token: 0x04001394 RID: 5012
		private double avgBitsDownPerSecond;

		// Token: 0x04001395 RID: 5013
		private long bitsDownPerSecondCount;

		// Token: 0x04001396 RID: 5014
		private long totalBitsDown;

		// Token: 0x04001397 RID: 5015
		private long totalBitsUp;

		// Token: 0x04001398 RID: 5016
		private double totalMs;

		// Token: 0x04001399 RID: 5017
		private int queueCapacity = 100;

		// Token: 0x0400139A RID: 5018
		private Queue<double> bitsUp = new Queue<double>();

		// Token: 0x0400139B RID: 5019
		private Queue<double> bitsDown = new Queue<double>();
	}
}

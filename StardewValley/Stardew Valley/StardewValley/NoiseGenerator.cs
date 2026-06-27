using System;

namespace StardewValley
{
	// Token: 0x020000E9 RID: 233
	[InstanceStatics]
	internal static class NoiseGenerator
	{
		// Token: 0x170001F4 RID: 500
		// (get) Token: 0x06001188 RID: 4488 RVA: 0x000CC108 File Offset: 0x000CA308
		// (set) Token: 0x06001189 RID: 4489 RVA: 0x000CC10F File Offset: 0x000CA30F
		public static int Seed { get; set; } = new Random().Next(int.MaxValue);

		// Token: 0x170001F5 RID: 501
		// (get) Token: 0x0600118A RID: 4490 RVA: 0x000CC117 File Offset: 0x000CA317
		// (set) Token: 0x0600118B RID: 4491 RVA: 0x000CC11E File Offset: 0x000CA31E
		public static int Octaves { get; set; } = 8;

		// Token: 0x170001F6 RID: 502
		// (get) Token: 0x0600118C RID: 4492 RVA: 0x000CC126 File Offset: 0x000CA326
		// (set) Token: 0x0600118D RID: 4493 RVA: 0x000CC12D File Offset: 0x000CA32D
		public static double Amplitude { get; set; } = 1.0;

		// Token: 0x170001F7 RID: 503
		// (get) Token: 0x0600118E RID: 4494 RVA: 0x000CC135 File Offset: 0x000CA335
		// (set) Token: 0x0600118F RID: 4495 RVA: 0x000CC13C File Offset: 0x000CA33C
		public static double Persistence { get; set; }

		// Token: 0x170001F8 RID: 504
		// (get) Token: 0x06001190 RID: 4496 RVA: 0x000CC144 File Offset: 0x000CA344
		// (set) Token: 0x06001191 RID: 4497 RVA: 0x000CC14B File Offset: 0x000CA34B
		public static double Frequency { get; set; } = 0.015;

		// Token: 0x06001192 RID: 4498 RVA: 0x000CC154 File Offset: 0x000CA354
		static NoiseGenerator()
		{
			NoiseGenerator.Persistence = 0.65;
		}

		// Token: 0x06001193 RID: 4499 RVA: 0x000CC1A8 File Offset: 0x000CA3A8
		public static double Noise(int x, int y)
		{
			double total = 0.0;
			double freq = NoiseGenerator.Frequency;
			double amp = NoiseGenerator.Amplitude;
			for (int i = 0; i < NoiseGenerator.Octaves; i++)
			{
				total += NoiseGenerator.Smooth((double)x * freq, (double)y * freq) * amp;
				freq *= 2.0;
				amp *= NoiseGenerator.Persistence;
			}
			if (total < -2.4)
			{
				total = -2.4;
			}
			else if (total > 2.4)
			{
				total = 2.4;
			}
			return total / 2.4;
		}

		// Token: 0x06001194 RID: 4500 RVA: 0x000CC23C File Offset: 0x000CA43C
		public static double NoiseGeneration(int x, int y)
		{
			int i = x + y * 57;
			i = (i << 13 ^ i);
			return 1.0 - (double)(i * (i * i * 15731 + 789221) + NoiseGenerator.Seed & int.MaxValue) / 1073741824.0;
		}

		// Token: 0x06001195 RID: 4501 RVA: 0x000CC28C File Offset: 0x000CA48C
		private static double Interpolate(double x, double y, double a)
		{
			double value = (1.0 - Math.Cos(a * 3.141592653589793)) * 0.5;
			return x * (1.0 - value) + y * value;
		}

		// Token: 0x06001196 RID: 4502 RVA: 0x000CC2D0 File Offset: 0x000CA4D0
		private static double Smooth(double x, double y)
		{
			double x2 = NoiseGenerator.NoiseGeneration((int)x, (int)y);
			double n2 = NoiseGenerator.NoiseGeneration((int)x + 1, (int)y);
			double n3 = NoiseGenerator.NoiseGeneration((int)x, (int)y + 1);
			double n4 = NoiseGenerator.NoiseGeneration((int)x + 1, (int)y + 1);
			double x3 = NoiseGenerator.Interpolate(x2, n2, x - (double)((int)x));
			double i2 = NoiseGenerator.Interpolate(n3, n4, x - (double)((int)x));
			return NoiseGenerator.Interpolate(x3, i2, y - (double)((int)y));
		}
	}
}

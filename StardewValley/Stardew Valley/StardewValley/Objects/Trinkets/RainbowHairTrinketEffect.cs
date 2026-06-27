using System;

namespace StardewValley.Objects.Trinkets
{
	// Token: 0x020001C4 RID: 452
	public class RainbowHairTrinketEffect : TrinketEffect
	{
		// Token: 0x06002006 RID: 8198 RVA: 0x0016DF59 File Offset: 0x0016C159
		public RainbowHairTrinketEffect(Trinket trinket) : base(trinket)
		{
		}

		// Token: 0x06002007 RID: 8199 RVA: 0x0016DF62 File Offset: 0x0016C162
		public override void Apply(Farmer farmer)
		{
			farmer.prismaticHair.Value = true;
		}

		// Token: 0x06002008 RID: 8200 RVA: 0x0016DF70 File Offset: 0x0016C170
		public override void Unapply(Farmer farmer)
		{
			farmer.prismaticHair.Value = false;
		}
	}
}

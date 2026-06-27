using System;
using Microsoft.Xna.Framework;

namespace StardewValley.Menus
{
	// Token: 0x0200026E RID: 622
	internal class FarmerBoxButton : ClickableComponent
	{
		// Token: 0x06002925 RID: 10533 RVA: 0x001E3CF3 File Offset: 0x001E1EF3
		public FarmerBoxButton(string name) : base(Rectangle.Empty, name)
		{
		}

		// Token: 0x04001AE7 RID: 6887
		public bool Selected;
	}
}

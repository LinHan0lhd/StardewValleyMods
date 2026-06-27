using System;

namespace StardewValley.Characters
{
	// Token: 0x02000377 RID: 887
	[Obsolete("All dogs now use the Pet class.")]
	public class Dog : Pet
	{
		// Token: 0x06003636 RID: 13878 RVA: 0x002AC723 File Offset: 0x002AA923
		public Dog()
		{
			this.Sprite = new AnimatedSprite(this.getPetTextureName(), 0, 32, 32);
			base.HideShadow = true;
			base.Breather = false;
			base.willDestroyObjectsUnderfoot = false;
		}
	}
}

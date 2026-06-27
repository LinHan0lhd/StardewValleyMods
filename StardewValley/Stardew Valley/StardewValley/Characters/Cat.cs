using System;

namespace StardewValley.Characters
{
	// Token: 0x02000375 RID: 885
	[Obsolete("All cats now use the Pet class.")]
	public class Cat : Pet
	{
		// Token: 0x0600360F RID: 13839 RVA: 0x002A9D41 File Offset: 0x002A7F41
		public Cat()
		{
			this.Sprite = new AnimatedSprite(this.getPetTextureName(), 0, 32, 32);
			base.HideShadow = true;
			base.Breather = false;
			base.willDestroyObjectsUnderfoot = false;
		}
	}
}

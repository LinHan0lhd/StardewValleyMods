using System;
using Microsoft.Xna.Framework.Input;

namespace StardewValley
{
	// Token: 0x020000C2 RID: 194
	public interface IKeyboardSubscriber
	{
		// Token: 0x06000D75 RID: 3445
		void RecieveTextInput(char inputChar);

		// Token: 0x06000D76 RID: 3446
		void RecieveTextInput(string text);

		// Token: 0x06000D77 RID: 3447
		void RecieveCommandInput(char command);

		// Token: 0x06000D78 RID: 3448
		void RecieveSpecialInput(Keys key);

		// Token: 0x170001BA RID: 442
		// (get) Token: 0x06000D79 RID: 3449
		// (set) Token: 0x06000D7A RID: 3450
		bool Selected { get; set; }
	}
}

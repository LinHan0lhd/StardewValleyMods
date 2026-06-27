using System;
using Microsoft.Xna.Framework;
using StardewValley.Menus;

namespace StardewValley.Logging
{
	// Token: 0x020002BA RID: 698
	public class CheatCommandChatLogger : IGameLogger
	{
		// Token: 0x06002D75 RID: 11637 RVA: 0x002392B4 File Offset: 0x002374B4
		public CheatCommandChatLogger(ChatBox chatBox)
		{
			this.ChatBox = chatBox;
		}

		// Token: 0x06002D76 RID: 11638 RVA: 0x002392C3 File Offset: 0x002374C3
		public void Verbose(string message)
		{
			Game1.log.Verbose(message);
		}

		// Token: 0x06002D77 RID: 11639 RVA: 0x002392D0 File Offset: 0x002374D0
		public void Debug(string message)
		{
			this.ChatBox.addMessage(message, Color.Gray);
			Game1.log.Debug(message);
		}

		// Token: 0x06002D78 RID: 11640 RVA: 0x002392EE File Offset: 0x002374EE
		public void Info(string message)
		{
			this.ChatBox.addInfoMessage(message);
			Game1.log.Info(message);
		}

		// Token: 0x06002D79 RID: 11641 RVA: 0x00239307 File Offset: 0x00237507
		public void Warn(string message)
		{
			this.ChatBox.addErrorMessage(message);
			Game1.log.Warn("[Warn] " + message);
		}

		// Token: 0x06002D7A RID: 11642 RVA: 0x0023932C File Offset: 0x0023752C
		public void Error(string error, Exception exception = null)
		{
			string message = "[Error] " + error;
			if (exception != null)
			{
				message = message + ": " + exception.Message;
			}
			this.ChatBox.addErrorMessage(message);
			Game1.log.Error(error, exception);
		}

		// Token: 0x04001F42 RID: 8002
		private readonly ChatBox ChatBox;
	}
}

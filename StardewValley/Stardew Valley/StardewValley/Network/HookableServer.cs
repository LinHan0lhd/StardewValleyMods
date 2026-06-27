using System;

namespace StardewValley.Network
{
	// Token: 0x020001CF RID: 463
	public abstract class HookableServer : Server, IHookableServer
	{
		// Token: 0x17000353 RID: 851
		// (get) Token: 0x060020AA RID: 8362 RVA: 0x00170EEA File Offset: 0x0016F0EA
		// (set) Token: 0x060020AB RID: 8363 RVA: 0x00170EF2 File Offset: 0x0016F0F2
		public Action<IncomingMessage, Action<OutgoingMessage>, Action> OnProcessingMessage { get; set; }

		// Token: 0x060020AC RID: 8364 RVA: 0x00170EFB File Offset: 0x0016F0FB
		public HookableServer(IGameServer gameServer) : base(gameServer)
		{
			this.OnProcessingMessage = new Action<IncomingMessage, Action<OutgoingMessage>, Action>(this.OnServerProcessingMessage);
		}

		// Token: 0x060020AD RID: 8365 RVA: 0x00170F16 File Offset: 0x0016F116
		private void OnServerProcessingMessage(IncomingMessage message, Action<OutgoingMessage> sendMessage, Action resume)
		{
			resume();
		}
	}
}

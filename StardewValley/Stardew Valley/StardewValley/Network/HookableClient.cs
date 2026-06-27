using System;

namespace StardewValley.Network
{
	// Token: 0x020001CE RID: 462
	public abstract class HookableClient : Client, IHookableClient
	{
		// Token: 0x17000351 RID: 849
		// (get) Token: 0x060020A3 RID: 8355 RVA: 0x00170E8C File Offset: 0x0016F08C
		// (set) Token: 0x060020A4 RID: 8356 RVA: 0x00170E94 File Offset: 0x0016F094
		public Action<IncomingMessage, Action<OutgoingMessage>, Action> OnProcessingMessage { get; set; }

		// Token: 0x17000352 RID: 850
		// (get) Token: 0x060020A5 RID: 8357 RVA: 0x00170E9D File Offset: 0x0016F09D
		// (set) Token: 0x060020A6 RID: 8358 RVA: 0x00170EA5 File Offset: 0x0016F0A5
		public Action<OutgoingMessage, Action<OutgoingMessage>, Action> OnSendingMessage { get; set; }

		// Token: 0x060020A7 RID: 8359 RVA: 0x00170EAE File Offset: 0x0016F0AE
		public HookableClient()
		{
			this.OnProcessingMessage = new Action<IncomingMessage, Action<OutgoingMessage>, Action>(this.OnClientProcessingMessage);
			this.OnSendingMessage = new Action<OutgoingMessage, Action<OutgoingMessage>, Action>(this.OnClientSendingMessage);
		}

		// Token: 0x060020A8 RID: 8360 RVA: 0x00170EDA File Offset: 0x0016F0DA
		private void OnClientProcessingMessage(IncomingMessage message, Action<OutgoingMessage> sendMessage, Action resume)
		{
			resume();
		}

		// Token: 0x060020A9 RID: 8361 RVA: 0x00170EE2 File Offset: 0x0016F0E2
		private void OnClientSendingMessage(OutgoingMessage message, Action<OutgoingMessage> sendMessage, Action resume)
		{
			resume();
		}
	}
}

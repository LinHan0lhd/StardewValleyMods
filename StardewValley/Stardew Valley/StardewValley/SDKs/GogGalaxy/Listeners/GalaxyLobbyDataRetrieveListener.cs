using System;
using Galaxy.Api;

namespace StardewValley.SDKs.GogGalaxy.Listeners
{
	// Token: 0x02000175 RID: 373
	internal sealed class GalaxyLobbyDataRetrieveListener : ILobbyDataRetrieveListener
	{
		// Token: 0x06001C3A RID: 7226 RVA: 0x00140D25 File Offset: 0x0013EF25
		public GalaxyLobbyDataRetrieveListener(Action<GalaxyID> success, Action<GalaxyID, ILobbyDataRetrieveListener.FailureReason> failure)
		{
			this.OnSuccess = success;
			this.OnFailure = failure;
		}

		// Token: 0x06001C3B RID: 7227 RVA: 0x00140D3B File Offset: 0x0013EF3B
		public override void OnLobbyDataRetrieveSuccess(GalaxyID lobbyID)
		{
			Action<GalaxyID> onSuccess = this.OnSuccess;
			if (onSuccess == null)
			{
				return;
			}
			onSuccess(lobbyID);
		}

		// Token: 0x06001C3C RID: 7228 RVA: 0x00140D4E File Offset: 0x0013EF4E
		public override void OnLobbyDataRetrieveFailure(GalaxyID lobbyID, ILobbyDataRetrieveListener.FailureReason failureReason)
		{
			Action<GalaxyID, ILobbyDataRetrieveListener.FailureReason> onFailure = this.OnFailure;
			if (onFailure == null)
			{
				return;
			}
			onFailure(lobbyID, failureReason);
		}

		// Token: 0x0400110E RID: 4366
		private readonly Action<GalaxyID> OnSuccess;

		// Token: 0x0400110F RID: 4367
		private readonly Action<GalaxyID, ILobbyDataRetrieveListener.FailureReason> OnFailure;
	}
}

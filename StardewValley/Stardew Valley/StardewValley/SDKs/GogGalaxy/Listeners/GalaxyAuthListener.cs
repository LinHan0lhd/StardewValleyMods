using System;
using Galaxy.Api;

namespace StardewValley.SDKs.GogGalaxy.Listeners
{
	// Token: 0x02000171 RID: 369
	internal sealed class GalaxyAuthListener : IAuthListener
	{
		// Token: 0x06001C2C RID: 7212 RVA: 0x00140BC8 File Offset: 0x0013EDC8
		public GalaxyAuthListener(Action success, Action<IAuthListener.FailureReason> failure, Action lost)
		{
			this.OnSuccess = success;
			this.OnFailure = failure;
			this.OnLost = lost;
			GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerAuth.GetListenerType(), this);
		}

		// Token: 0x06001C2D RID: 7213 RVA: 0x00140BF5 File Offset: 0x0013EDF5
		public override void OnAuthSuccess()
		{
			Action onSuccess = this.OnSuccess;
			if (onSuccess == null)
			{
				return;
			}
			onSuccess();
		}

		// Token: 0x06001C2E RID: 7214 RVA: 0x00140C07 File Offset: 0x0013EE07
		public override void OnAuthFailure(IAuthListener.FailureReason reason)
		{
			Action<IAuthListener.FailureReason> onFailure = this.OnFailure;
			if (onFailure == null)
			{
				return;
			}
			onFailure(reason);
		}

		// Token: 0x06001C2F RID: 7215 RVA: 0x00140C1A File Offset: 0x0013EE1A
		public override void OnAuthLost()
		{
			Action onLost = this.OnLost;
			if (onLost == null)
			{
				return;
			}
			onLost();
		}

		// Token: 0x06001C30 RID: 7216 RVA: 0x00140C2C File Offset: 0x0013EE2C
		public override void Dispose()
		{
			GalaxyInstance.ListenerRegistrar().Unregister(GalaxyTypeAwareListenerAuth.GetListenerType(), this);
			base.Dispose();
		}

		// Token: 0x04001108 RID: 4360
		private readonly Action OnSuccess;

		// Token: 0x04001109 RID: 4361
		private readonly Action<IAuthListener.FailureReason> OnFailure;

		// Token: 0x0400110A RID: 4362
		private readonly Action OnLost;
	}
}

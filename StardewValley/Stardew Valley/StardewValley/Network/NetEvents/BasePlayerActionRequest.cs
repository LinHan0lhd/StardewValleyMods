using System;
using System.IO;
using System.Runtime.CompilerServices;
using Netcode;
using StardewValley.Logging;

namespace StardewValley.Network.NetEvents
{
	// Token: 0x020001F9 RID: 505
	public abstract class BasePlayerActionRequest : NetEventArg
	{
		// Token: 0x170003CC RID: 972
		// (get) Token: 0x060022A5 RID: 8869 RVA: 0x00176FC4 File Offset: 0x001751C4
		// (set) Token: 0x060022A6 RID: 8870 RVA: 0x00176FCC File Offset: 0x001751CC
		public PlayerActionTarget Target { get; private set; }

		// Token: 0x170003CD RID: 973
		// (get) Token: 0x060022A7 RID: 8871 RVA: 0x00176FD5 File Offset: 0x001751D5
		// (set) Token: 0x060022A8 RID: 8872 RVA: 0x00176FDD File Offset: 0x001751DD
		public long? OnlyPlayerId { get; private set; }

		// Token: 0x060022A9 RID: 8873 RVA: 0x00176FE8 File Offset: 0x001751E8
		public virtual void Read(BinaryReader reader)
		{
			this.Target = (PlayerActionTarget)reader.ReadByte();
			this.OnlyPlayerId = (reader.ReadBoolean() ? new long?(reader.ReadInt64()) : null);
		}

		// Token: 0x060022AA RID: 8874 RVA: 0x00177028 File Offset: 0x00175228
		public virtual void Write(BinaryWriter writer)
		{
			writer.Write((byte)this.Target);
			writer.Write(this.OnlyPlayerId != null);
			if (this.OnlyPlayerId != null)
			{
				writer.Write(this.OnlyPlayerId.Value);
			}
		}

		// Token: 0x060022AB RID: 8875 RVA: 0x0017707C File Offset: 0x0017527C
		public bool MatchesPlayer(Farmer player)
		{
			if (this.OnlyPlayerId != null && player.UniqueMultiplayerID != this.OnlyPlayerId.Value)
			{
				return false;
			}
			switch (this.Target)
			{
			case PlayerActionTarget.Current:
				return true;
			case PlayerActionTarget.Host:
				return Game1.IsMasterGame;
			case PlayerActionTarget.All:
				return true;
			default:
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(45, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Can't process net request ");
				defaultInterpolatedStringHandler.AppendFormatted(base.GetType().AssemblyQualifiedName);
				defaultInterpolatedStringHandler.AppendLiteral(": Invalid target '");
				defaultInterpolatedStringHandler.AppendFormatted<PlayerActionTarget>(this.Target);
				defaultInterpolatedStringHandler.AppendLiteral("'");
				log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
				return false;
			}
			}
		}

		// Token: 0x060022AC RID: 8876 RVA: 0x00177138 File Offset: 0x00175338
		public bool OnlyForLocalPlayer()
		{
			if (this.OnlyPlayerId != null)
			{
				return this.MatchesPlayer(Game1.player);
			}
			switch (this.Target)
			{
			case PlayerActionTarget.Current:
				return true;
			case PlayerActionTarget.Host:
				return Game1.IsMasterGame;
			case PlayerActionTarget.All:
				return Game1.IsMasterGame && Game1.netWorldState.Value.farmhandData.Length == 0;
			default:
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(45, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Can't process net request ");
				defaultInterpolatedStringHandler.AppendFormatted(base.GetType().AssemblyQualifiedName);
				defaultInterpolatedStringHandler.AppendLiteral(": Invalid target '");
				defaultInterpolatedStringHandler.AppendFormatted<PlayerActionTarget>(this.Target);
				defaultInterpolatedStringHandler.AppendLiteral("'");
				log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
				return false;
			}
			}
		}

		// Token: 0x060022AD RID: 8877
		public abstract void PerformAction(Farmer farmer);

		// Token: 0x060022AE RID: 8878 RVA: 0x00177204 File Offset: 0x00175404
		protected BasePlayerActionRequest() : this(PlayerActionTarget.Current, null)
		{
		}

		// Token: 0x060022AF RID: 8879 RVA: 0x00177221 File Offset: 0x00175421
		protected BasePlayerActionRequest(PlayerActionTarget target, long? onlyPlayerId)
		{
			this.Target = target;
			this.OnlyPlayerId = onlyPlayerId;
		}
	}
}

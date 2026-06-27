using System;
using System.IO;
using StardewValley.Extensions;

namespace StardewValley.Network.NetEvents
{
	// Token: 0x020001FF RID: 511
	public sealed class SetSimpleFlagRequest : BaseSetFlagRequest
	{
		// Token: 0x170003D6 RID: 982
		// (get) Token: 0x060022CE RID: 8910 RVA: 0x001775FB File Offset: 0x001757FB
		// (set) Token: 0x060022CF RID: 8911 RVA: 0x00177603 File Offset: 0x00175803
		public SimpleFlagType FlagType { get; private set; }

		// Token: 0x060022D0 RID: 8912 RVA: 0x0017760C File Offset: 0x0017580C
		public SetSimpleFlagRequest()
		{
		}

		// Token: 0x060022D1 RID: 8913 RVA: 0x00177614 File Offset: 0x00175814
		public SetSimpleFlagRequest(SimpleFlagType flagType, PlayerActionTarget target, string flagId, bool flagState, long? onlyPlayerId) : base(target, flagId, flagState, onlyPlayerId)
		{
			this.FlagType = flagType;
		}

		// Token: 0x060022D2 RID: 8914 RVA: 0x00177629 File Offset: 0x00175829
		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			this.FlagType = (SimpleFlagType)reader.ReadByte();
		}

		// Token: 0x060022D3 RID: 8915 RVA: 0x0017763E File Offset: 0x0017583E
		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write((byte)this.FlagType);
		}

		// Token: 0x060022D4 RID: 8916 RVA: 0x00177654 File Offset: 0x00175854
		public override void PerformAction(Farmer farmer)
		{
			switch (this.FlagType)
			{
			case SimpleFlagType.ActionApplied:
				farmer.triggerActionsRun.Toggle(base.FlagId, base.FlagState);
				return;
			case SimpleFlagType.CookingRecipeKnown:
				if (base.FlagState)
				{
					farmer.cookingRecipes.TryAdd(base.FlagId, 0);
					return;
				}
				farmer.cookingRecipes.Remove(base.FlagId);
				return;
			case SimpleFlagType.CraftingRecipeKnown:
				if (base.FlagState)
				{
					farmer.craftingRecipes.TryAdd(base.FlagId, 0);
					return;
				}
				farmer.craftingRecipes.Remove(base.FlagId);
				return;
			case SimpleFlagType.DialogueAnswerSelected:
				farmer.dialogueQuestionsAnswered.Toggle(base.FlagId, base.FlagState);
				return;
			case SimpleFlagType.EventSeen:
				farmer.eventsSeen.Toggle(base.FlagId, base.FlagState);
				return;
			case SimpleFlagType.HasQuest:
				if (base.FlagState)
				{
					farmer.addQuest(base.FlagId);
					return;
				}
				farmer.removeQuest(base.FlagId);
				return;
			case SimpleFlagType.SongHeard:
				farmer.songsHeard.Toggle(base.FlagId, base.FlagState);
				return;
			default:
				return;
			}
		}
	}
}

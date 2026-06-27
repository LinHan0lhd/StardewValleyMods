using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Netcode;
using StardewValley.Logging;

namespace StardewValley
{
	// Token: 0x020000EB RID: 235
	public class MarriageDialogueReference : INetObject<NetFields>, IEquatable<MarriageDialogueReference>
	{
		// Token: 0x17000213 RID: 531
		// (get) Token: 0x0600127B RID: 4731 RVA: 0x000DB85E File Offset: 0x000D9A5E
		public NetFields NetFields { get; } = new NetFields("MarriageDialogueReference");

		// Token: 0x0600127C RID: 4732 RVA: 0x000DB868 File Offset: 0x000D9A68
		public MarriageDialogueReference()
		{
			this.NetFields.SetOwner(this).AddField(this._dialogueFile, "_dialogueFile").AddField(this._dialogueKey, "_dialogueKey").AddField(this._isGendered, "_isGendered").AddField(this._substitutions, "_substitutions");
		}

		// Token: 0x0600127D RID: 4733 RVA: 0x000DB90F File Offset: 0x000D9B0F
		public MarriageDialogueReference(string dialogue_file, string dialogue_key, bool gendered = false, params string[] substitutions) : this()
		{
			this._dialogueFile.Value = dialogue_file;
			this._dialogueKey.Value = dialogue_key;
			this._isGendered.Value = gendered;
			if (substitutions.Length != 0)
			{
				this._substitutions.AddRange(substitutions);
			}
		}

		// Token: 0x0600127E RID: 4734 RVA: 0x000DB94D File Offset: 0x000D9B4D
		public string GetText()
		{
			return "";
		}

		// Token: 0x0600127F RID: 4735 RVA: 0x000DB954 File Offset: 0x000D9B54
		public bool IsItemGrabDialogue(NPC n)
		{
			return this.GetDialogue(n).isItemGrabDialogue();
		}

		// Token: 0x06001280 RID: 4736 RVA: 0x000DB964 File Offset: 0x000D9B64
		protected void _ReplaceTokens(Dialogue dialogue, NPC npc)
		{
			for (int i = 0; i < dialogue.dialogues.Count; i++)
			{
				dialogue.dialogues[i].Text = this._ReplaceTokens(dialogue.dialogues[i].Text, npc);
			}
		}

		// Token: 0x06001281 RID: 4737 RVA: 0x000DB9B0 File Offset: 0x000D9BB0
		protected string _ReplaceTokens(string text, NPC npc)
		{
			text = text.Replace("%endearmentlower", npc.getTermOfSpousalEndearment(true).ToLower());
			text = text.Replace("%endearment", npc.getTermOfSpousalEndearment(true));
			return text;
		}

		// Token: 0x06001282 RID: 4738 RVA: 0x000DB9E0 File Offset: 0x000D9BE0
		public Dialogue GetDialogue(NPC n)
		{
			if (this._dialogueFile.Value.Contains("Marriage"))
			{
				Dialogue dialogue = n.tryToGetMarriageSpecificDialogue(this._dialogueKey.Value);
				if (dialogue == null)
				{
					IGameLogger log = Game1.log;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(56, 2);
					defaultInterpolatedStringHandler.AppendLiteral("NPC '");
					defaultInterpolatedStringHandler.AppendFormatted(n.Name);
					defaultInterpolatedStringHandler.AppendLiteral("' couldn't get marriage dialogue key '");
					defaultInterpolatedStringHandler.AppendFormatted(this._dialogueKey.Value);
					defaultInterpolatedStringHandler.AppendLiteral("': not found.");
					log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
					dialogue = Dialogue.GetFallbackForError(n);
				}
				dialogue.removeOnNextMove = true;
				this._ReplaceTokens(dialogue, n);
				return dialogue;
			}
			string key = this._dialogueFile.Value + ":" + this._dialogueKey.Value;
			string rawText = this._isGendered.Value ? Game1.LoadStringByGender(n.Gender, key, new object[]
			{
				this._substitutions
			}) : Game1.content.LoadString(key, this._substitutions);
			return new Dialogue(n, key, this._ReplaceTokens(rawText, n))
			{
				removeOnNextMove = true
			};
		}

		// Token: 0x17000214 RID: 532
		// (get) Token: 0x06001283 RID: 4739 RVA: 0x000DBB05 File Offset: 0x000D9D05
		public string DialogueFile
		{
			get
			{
				return this._dialogueFile.Value;
			}
		}

		// Token: 0x17000215 RID: 533
		// (get) Token: 0x06001284 RID: 4740 RVA: 0x000DBB12 File Offset: 0x000D9D12
		public string DialogueKey
		{
			get
			{
				return this._dialogueKey.Value;
			}
		}

		// Token: 0x17000216 RID: 534
		// (get) Token: 0x06001285 RID: 4741 RVA: 0x000DBB1F File Offset: 0x000D9D1F
		public bool IsGendered
		{
			get
			{
				return this._isGendered.Value;
			}
		}

		// Token: 0x17000217 RID: 535
		// (get) Token: 0x06001286 RID: 4742 RVA: 0x000DBB2C File Offset: 0x000D9D2C
		public string[] Substitutions
		{
			get
			{
				return this._substitutions.ToArray<string>();
			}
		}

		// Token: 0x06001287 RID: 4743 RVA: 0x000DBB3C File Offset: 0x000D9D3C
		public bool Equals(MarriageDialogueReference other)
		{
			return object.Equals(this._dialogueFile.Value, other._dialogueFile.Value) && object.Equals(this._dialogueKey.Value, other._dialogueKey.Value) && object.Equals(this._isGendered.Value, other._isGendered.Value) && this._substitutions.SequenceEqual(other._substitutions);
		}

		// Token: 0x06001288 RID: 4744 RVA: 0x000DBBC0 File Offset: 0x000D9DC0
		public override bool Equals(object obj)
		{
			MarriageDialogueReference dialogue = obj as MarriageDialogueReference;
			return dialogue != null && this.Equals(dialogue);
		}

		// Token: 0x06001289 RID: 4745 RVA: 0x000DBBE0 File Offset: 0x000D9DE0
		public override int GetHashCode()
		{
			int hash = 13;
			hash = hash * 7 + ((this._dialogueFile.Value == null) ? 0 : this._dialogueFile.Value.GetHashCode());
			hash = hash * 7 + ((this._dialogueKey.Value == null) ? 0 : this._dialogueFile.Value.GetHashCode());
			hash = hash * 7 + ((!this._isGendered.Value) ? 1 : 0);
			foreach (string substitution in this._substitutions)
			{
				hash = hash * 7 + substitution.GetHashCode();
			}
			return hash;
		}

		// Token: 0x04000B02 RID: 2818
		public const string ENDEARMENT_TOKEN = "%endearment";

		// Token: 0x04000B03 RID: 2819
		public const string ENDEARMENT_TOKEN_LOWER = "%endearmentlower";

		// Token: 0x04000B05 RID: 2821
		private readonly NetString _dialogueFile = new NetString("");

		// Token: 0x04000B06 RID: 2822
		private readonly NetString _dialogueKey = new NetString("");

		// Token: 0x04000B07 RID: 2823
		private readonly NetBool _isGendered = new NetBool(false);

		// Token: 0x04000B08 RID: 2824
		private readonly NetStringList _substitutions = new NetStringList();
	}
}

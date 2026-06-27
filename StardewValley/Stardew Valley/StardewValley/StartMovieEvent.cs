using System;
using System.Collections.Generic;
using System.IO;
using Netcode;

namespace StardewValley
{
	// Token: 0x020000D6 RID: 214
	public class StartMovieEvent : NetEventArg
	{
		// Token: 0x0600107B RID: 4219 RVA: 0x000C696B File Offset: 0x000C4B6B
		public StartMovieEvent()
		{
		}

		// Token: 0x0600107C RID: 4220 RVA: 0x000C6973 File Offset: 0x000C4B73
		public StartMovieEvent(long farmer_uid, List<List<Character>> player_groups, List<List<Character>> npc_groups)
		{
			this.uid = farmer_uid;
			this.playerGroups = player_groups;
			this.npcGroups = npc_groups;
		}

		// Token: 0x0600107D RID: 4221 RVA: 0x000C6990 File Offset: 0x000C4B90
		public void Read(BinaryReader reader)
		{
			this.uid = reader.ReadInt64();
			this.playerGroups = this.ReadCharacterList(reader);
			this.npcGroups = this.ReadCharacterList(reader);
		}

		// Token: 0x0600107E RID: 4222 RVA: 0x000C69B8 File Offset: 0x000C4BB8
		public void Write(BinaryWriter writer)
		{
			writer.Write(this.uid);
			this.WriteCharacterList(writer, this.playerGroups);
			this.WriteCharacterList(writer, this.npcGroups);
		}

		// Token: 0x0600107F RID: 4223 RVA: 0x000C69E0 File Offset: 0x000C4BE0
		public List<List<Character>> ReadCharacterList(BinaryReader reader)
		{
			List<List<Character>> group_list = new List<List<Character>>();
			int group_list_count = reader.ReadInt32();
			for (int i = 0; i < group_list_count; i++)
			{
				List<Character> group = new List<Character>();
				int group_count = reader.ReadInt32();
				for (int j = 0; j < group_count; j++)
				{
					Character character = (reader.ReadInt32() == 1) ? (Game1.GetPlayer(reader.ReadInt64(), true) ?? Game1.MasterPlayer) : Game1.getCharacterFromName(reader.ReadString(), true, false);
					group.Add(character);
				}
				group_list.Add(group);
			}
			return group_list;
		}

		// Token: 0x06001080 RID: 4224 RVA: 0x000C6A64 File Offset: 0x000C4C64
		public void WriteCharacterList(BinaryWriter writer, List<List<Character>> group_list)
		{
			writer.Write(group_list.Count);
			foreach (List<Character> group in group_list)
			{
				writer.Write(group.Count);
				foreach (Character character in group)
				{
					Farmer player = character as Farmer;
					if (player != null)
					{
						writer.Write(1);
						writer.Write(player.UniqueMultiplayerID);
					}
					else
					{
						writer.Write(0);
						writer.Write(character.Name);
					}
				}
			}
		}

		// Token: 0x04000A04 RID: 2564
		public long uid;

		// Token: 0x04000A05 RID: 2565
		public List<List<Character>> playerGroups;

		// Token: 0x04000A06 RID: 2566
		public List<List<Character>> npcGroups;
	}
}

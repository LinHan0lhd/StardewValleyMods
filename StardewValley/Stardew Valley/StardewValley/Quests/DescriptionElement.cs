using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Netcode;
using StardewValley.Monsters;
using StardewValley.SaveSerialization;

namespace StardewValley.Quests
{
	// Token: 0x0200018B RID: 395
	[XmlInclude(typeof(Item))]
	[XmlInclude(typeof(Character))]
	public class DescriptionElement : INetObject<NetFields>
	{
		// Token: 0x17000306 RID: 774
		// (get) Token: 0x06001C93 RID: 7315 RVA: 0x00146D2F File Offset: 0x00144F2F
		[XmlIgnore]
		public NetFields NetFields { get; } = new NetFields("DescriptionElement");

		// Token: 0x06001C94 RID: 7316 RVA: 0x00146D37 File Offset: 0x00144F37
		public DescriptionElement() : this(string.Empty, Array.Empty<object>())
		{
		}

		// Token: 0x06001C95 RID: 7317 RVA: 0x00146D4C File Offset: 0x00144F4C
		public DescriptionElement(string key, params object[] substitutions)
		{
			this.NetFields.SetOwner(this);
			this.translationKey = key;
			this.substitutions = new List<object>();
			this.substitutions.AddRange(substitutions);
		}

		// Token: 0x06001C96 RID: 7318 RVA: 0x00146D9C File Offset: 0x00144F9C
		public string loadDescriptionElement()
		{
			if (string.IsNullOrWhiteSpace(this.translationKey))
			{
				return string.Empty;
			}
			object[] substitutions = this.substitutions.ToArray();
			for (int i = 0; i < substitutions.Length; i++)
			{
				object obj2 = substitutions[i];
				DescriptionElement element = obj2 as DescriptionElement;
				if (element == null)
				{
					Object obj = obj2 as Object;
					if (obj == null)
					{
						Monster monster = obj2 as Monster;
						if (monster == null)
						{
							NPC npc = obj2 as NPC;
							if (npc != null)
							{
								substitutions[i] = NPC.GetDisplayName(npc.name.Value);
							}
						}
						else
						{
							DescriptionElement d;
							if (monster.name.Value == "Frost Jelly")
							{
								d = new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13772", Array.Empty<object>());
								substitutions[i] = d.loadDescriptionElement();
							}
							else
							{
								d = new DescriptionElement("Data\\Monsters:" + monster.name.Value, Array.Empty<object>());
								substitutions[i] = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) ? (d.loadDescriptionElement().Split('/', StringSplitOptions.None).Last<string>() + "s") : d.loadDescriptionElement().Split('/', StringSplitOptions.None).Last<string>());
							}
							substitutions[i] = d.loadDescriptionElement().Split('/', StringSplitOptions.None).Last<string>();
						}
					}
					else
					{
						substitutions[i] = ItemRegistry.GetDataOrErrorItem(obj.QualifiedItemId).DisplayName;
					}
				}
				else
				{
					substitutions[i] = element.loadDescriptionElement();
				}
			}
			switch (substitutions.Length)
			{
			case 0:
				if (!this.translationKey.Contains("Dialogue.cs.7") && !this.translationKey.Contains("Dialogue.cs.8"))
				{
					return Game1.content.LoadString(this.translationKey);
				}
				return Game1.content.LoadString(this.translationKey).Replace("/", " ").TrimStart(' ');
			case 1:
				return Game1.content.LoadString(this.translationKey, substitutions[0]);
			case 2:
				return Game1.content.LoadString(this.translationKey, substitutions[0], substitutions[1]);
			case 3:
				return Game1.content.LoadString(this.translationKey, substitutions[0], substitutions[1], substitutions[2]);
			default:
				return Game1.content.LoadString(this.translationKey, substitutions);
			}
		}

		// Token: 0x04001170 RID: 4464
		public static XmlSerializer serializer = SaveSerializer.GetSerializer(typeof(DescriptionElement));

		// Token: 0x04001171 RID: 4465
		[XmlElement("xmlKey")]
		public string translationKey;

		// Token: 0x04001172 RID: 4466
		[XmlElement("param")]
		public List<object> substitutions;
	}
}

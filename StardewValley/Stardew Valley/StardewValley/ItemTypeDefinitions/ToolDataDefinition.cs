using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;
using StardewValley.GameData.Tools;
using StardewValley.Logging;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;

namespace StardewValley.ItemTypeDefinitions
{
	// Token: 0x02000305 RID: 773
	public class ToolDataDefinition : BaseItemDataDefinition
	{
		// Token: 0x17000445 RID: 1093
		// (get) Token: 0x06003390 RID: 13200 RVA: 0x00298E91 File Offset: 0x00297091
		public override string Identifier
		{
			get
			{
				return "(T)";
			}
		}

		// Token: 0x06003391 RID: 13201 RVA: 0x00298E98 File Offset: 0x00297098
		public override IEnumerable<string> GetAllIds()
		{
			return Game1.toolData.Keys;
		}

		// Token: 0x06003392 RID: 13202 RVA: 0x00298EA4 File Offset: 0x002970A4
		public override bool Exists(string itemId)
		{
			return itemId != null && Game1.toolData.ContainsKey(itemId);
		}

		// Token: 0x06003393 RID: 13203 RVA: 0x00298EB8 File Offset: 0x002970B8
		public override ParsedItemData GetData(string itemId)
		{
			ToolData data = this.GetRawData(itemId);
			if (data == null)
			{
				return null;
			}
			return new ParsedItemData(this, itemId, (data.MenuSpriteIndex > -1) ? data.MenuSpriteIndex : data.SpriteIndex, data.Texture, itemId, TokenParser.ParseText(data.DisplayName, null, null, null), TokenParser.ParseText(data.Description, null, null, null), -99, null, data, false, false);
		}

		// Token: 0x06003394 RID: 13204 RVA: 0x00298F19 File Offset: 0x00297119
		public override Rectangle GetSourceRect(ParsedItemData data, Texture2D texture, int spriteIndex)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			if (texture == null)
			{
				throw new ArgumentNullException("texture");
			}
			return Game1.getSquareSourceRectForNonStandardTileSheet(texture, 16, 16, spriteIndex);
		}

		// Token: 0x06003395 RID: 13205 RVA: 0x00298F44 File Offset: 0x00297144
		public override Item CreateItem(ParsedItemData data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			ToolData rawData = this.GetRawData(data.ItemId);
			Tool tool = this.CreateToolInstance(data, rawData);
			if (tool == null)
			{
				return this.GetErrorTool(data);
			}
			tool.ItemId = data.ItemId;
			tool.SetSpriteIndex(rawData.SpriteIndex);
			if (rawData.MenuSpriteIndex > -1)
			{
				tool.IndexOfMenuItemView = rawData.MenuSpriteIndex;
			}
			tool.Name = rawData.Name;
			if (rawData.UpgradeLevel > -1)
			{
				tool.UpgradeLevel = rawData.UpgradeLevel;
			}
			if (rawData.AttachmentSlots > -1)
			{
				tool.AttachmentSlotsCount = rawData.AttachmentSlots;
			}
			if (rawData.SetProperties != null)
			{
				Type type = tool.GetType();
				foreach (KeyValuePair<string, string> pair in rawData.SetProperties)
				{
					this.TrySetProperty(type, tool, pair.Key, pair.Value);
				}
			}
			if (rawData.ModData != null)
			{
				foreach (KeyValuePair<string, string> pair2 in rawData.ModData)
				{
					tool.modData[pair2.Key] = pair2.Value;
				}
			}
			return tool;
		}

		// Token: 0x06003396 RID: 13206 RVA: 0x002990A8 File Offset: 0x002972A8
		protected ToolData GetRawData(string itemId)
		{
			ToolData data;
			if (itemId == null || !Game1.toolData.TryGetValue(itemId, out data))
			{
				return null;
			}
			return data;
		}

		// Token: 0x06003397 RID: 13207 RVA: 0x002990CC File Offset: 0x002972CC
		protected Tool CreateToolInstance(ParsedItemData itemData, ToolData toolData)
		{
			if (itemData != null && toolData != null)
			{
				Type type = typeof(Tool).Assembly.GetType("StardewValley.Tools." + toolData.ClassName);
				if (type != null)
				{
					Tool tool = (Tool)Activator.CreateInstance(type);
					if (tool != null)
					{
						return tool;
					}
				}
			}
			return this.GetErrorTool(itemData);
		}

		// Token: 0x06003398 RID: 13208 RVA: 0x00299125 File Offset: 0x00297325
		protected Tool GetErrorTool(ParsedItemData data)
		{
			return new ErrorTool(data.ItemId, 0, 0);
		}

		// Token: 0x06003399 RID: 13209 RVA: 0x00299134 File Offset: 0x00297334
		protected void TrySetProperty(Type type, Tool tool, string name, string rawValue)
		{
			MemberInfo member = type.GetProperty(name) ?? type.GetField(name);
			if (member == null)
			{
				IGameLogger log = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(85, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Can't set field or property '");
				defaultInterpolatedStringHandler.AppendFormatted(name);
				defaultInterpolatedStringHandler.AppendLiteral("' for tool '");
				defaultInterpolatedStringHandler.AppendFormatted(tool.QualifiedItemId);
				defaultInterpolatedStringHandler.AppendLiteral("': the ");
				defaultInterpolatedStringHandler.AppendFormatted(type.FullName);
				defaultInterpolatedStringHandler.AppendLiteral(" class has none public with that name");
				log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				return;
			}
			string error;
			if (!member.TrySetValueFromString(tool, rawValue, null, out error))
			{
				IGameLogger log2 = Game1.log;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 4);
				defaultInterpolatedStringHandler.AppendLiteral("Can't set ");
				defaultInterpolatedStringHandler.AppendFormatted((member is FieldInfo) ? "field" : "property");
				defaultInterpolatedStringHandler.AppendLiteral(" '");
				defaultInterpolatedStringHandler.AppendFormatted(name);
				defaultInterpolatedStringHandler.AppendLiteral("' for tool '");
				defaultInterpolatedStringHandler.AppendFormatted(tool.QualifiedItemId);
				defaultInterpolatedStringHandler.AppendLiteral("': ");
				defaultInterpolatedStringHandler.AppendFormatted(error);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				log2.Error(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				return;
			}
		}
	}
}

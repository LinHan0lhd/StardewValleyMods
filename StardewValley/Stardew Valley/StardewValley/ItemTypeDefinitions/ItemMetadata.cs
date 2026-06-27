using System;
using System.Runtime.CompilerServices;
using StardewValley.Logging;

namespace StardewValley.ItemTypeDefinitions
{
	// Token: 0x020002FF RID: 767
	public class ItemMetadata
	{
		// Token: 0x1700043A RID: 1082
		// (get) Token: 0x0600334A RID: 13130 RVA: 0x00297A51 File Offset: 0x00295C51
		public string LocalItemId { get; }

		// Token: 0x1700043B RID: 1083
		// (get) Token: 0x0600334B RID: 13131 RVA: 0x00297A59 File Offset: 0x00295C59
		public string QualifiedItemId { get; }

		// Token: 0x1700043C RID: 1084
		// (get) Token: 0x0600334C RID: 13132 RVA: 0x00297A61 File Offset: 0x00295C61
		// (set) Token: 0x0600334D RID: 13133 RVA: 0x00297A69 File Offset: 0x00295C69
		public string TypeIdentifier { get; private set; }

		// Token: 0x0600334E RID: 13134 RVA: 0x00297A72 File Offset: 0x00295C72
		public ItemMetadata(string qualifiedItemId, string localItemId, string typeIdentifier)
		{
			this.QualifiedItemId = qualifiedItemId;
			this.LocalItemId = localItemId;
			this.TypeIdentifier = typeIdentifier;
		}

		// Token: 0x0600334F RID: 13135 RVA: 0x00297A90 File Offset: 0x00295C90
		internal void SetTypeDefinition(string typeIdentifier, IItemDataDefinition typeDefinition, bool? itemExists = null)
		{
			this.TypeIdentifier = typeIdentifier;
			this.TypeDefinition = typeDefinition;
			this.IsTypeResolveAttempted = true;
			this.TypeDefinitionContainsItem = (itemExists ?? (typeDefinition != null && typeDefinition.Exists(this.LocalItemId)));
		}

		// Token: 0x06003350 RID: 13136 RVA: 0x00297AE0 File Offset: 0x00295CE0
		public IItemDataDefinition GetTypeDefinition()
		{
			if (!this.IsTypeResolveAttempted)
			{
				IItemDataDefinition definition = ItemRegistry.GetTypeDefinitionFor(this);
				this.SetTypeDefinition(((definition != null) ? definition.Identifier : null) ?? this.TypeIdentifier, definition, null);
			}
			return this.TypeDefinition;
		}

		// Token: 0x06003351 RID: 13137 RVA: 0x00297B28 File Offset: 0x00295D28
		public ParsedItemData GetParsedData()
		{
			if (!this.IsParsedDataLoaded)
			{
				if (!this.IsTypeResolveAttempted)
				{
					this.GetTypeDefinition();
				}
				if (this.TypeDefinition != null)
				{
					try
					{
						this.ParsedData = this.TypeDefinition.GetData(this.LocalItemId);
						goto IL_B9;
					}
					catch (Exception ex)
					{
						IGameLogger log = Game1.log;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(70, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Item type '");
						defaultInterpolatedStringHandler.AppendFormatted(this.TypeIdentifier);
						defaultInterpolatedStringHandler.AppendLiteral("' failed parsing item with ID '");
						defaultInterpolatedStringHandler.AppendFormatted(this.LocalItemId);
						defaultInterpolatedStringHandler.AppendLiteral("', defaulting to error item.");
						log.Error(defaultInterpolatedStringHandler.ToStringAndClear(), ex);
						this.ParsedData = this.TypeDefinition.GetErrorData(this.LocalItemId);
						goto IL_B9;
					}
				}
				this.ParsedData = null;
				IL_B9:
				this.IsParsedDataLoaded = true;
			}
			return this.ParsedData;
		}

		// Token: 0x06003352 RID: 13138 RVA: 0x00297C0C File Offset: 0x00295E0C
		public ParsedItemData GetParsedOrErrorData()
		{
			return this.GetParsedData() ?? this.TypeDefinition.GetErrorData(this.LocalItemId);
		}

		// Token: 0x06003353 RID: 13139 RVA: 0x00297C29 File Offset: 0x00295E29
		public bool Exists()
		{
			if (!this.IsTypeResolveAttempted)
			{
				this.GetTypeDefinition();
			}
			return this.TypeDefinitionContainsItem;
		}

		// Token: 0x06003354 RID: 13140 RVA: 0x00297C40 File Offset: 0x00295E40
		public Item CreateItem(int amount = 1, int quality = 0)
		{
			if (!this.Exists())
			{
				return null;
			}
			return ItemRegistry.Create(this.QualifiedItemId, amount, quality, false);
		}

		// Token: 0x06003355 RID: 13141 RVA: 0x00297C5A File Offset: 0x00295E5A
		public Item CreateItemOrErrorItem(int amount = 1, int quality = 0)
		{
			return ItemRegistry.Create(this.QualifiedItemId, amount, quality, false);
		}

		// Token: 0x040021FB RID: 8699
		private ParsedItemData ParsedData;

		// Token: 0x040021FC RID: 8700
		private bool IsParsedDataLoaded;

		// Token: 0x040021FD RID: 8701
		private IItemDataDefinition TypeDefinition;

		// Token: 0x040021FE RID: 8702
		private bool IsTypeResolveAttempted;

		// Token: 0x040021FF RID: 8703
		private bool TypeDefinitionContainsItem;
	}
}

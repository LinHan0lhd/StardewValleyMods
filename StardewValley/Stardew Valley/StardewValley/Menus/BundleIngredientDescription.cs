using System;

namespace StardewValley.Menus
{
	// Token: 0x02000281 RID: 641
	public struct BundleIngredientDescription
	{
		// Token: 0x06002A72 RID: 10866 RVA: 0x001FD5FC File Offset: 0x001FB7FC
		public BundleIngredientDescription(string idOrCategory, int stack, int quality, bool completed, string preservesId = null)
		{
			this.stack = stack;
			this.quality = quality;
			this.completed = completed;
			this.preservesId = preservesId;
			int categoryValue;
			if (int.TryParse(idOrCategory, out categoryValue) && categoryValue < 0)
			{
				this.id = null;
				this.category = new int?(categoryValue);
				return;
			}
			this.id = idOrCategory;
			this.category = null;
		}

		// Token: 0x06002A73 RID: 10867 RVA: 0x001FD65C File Offset: 0x001FB85C
		public BundleIngredientDescription(BundleIngredientDescription other, bool completed)
		{
			this.id = other.id;
			this.category = other.category;
			this.stack = other.stack;
			this.quality = other.quality;
			this.preservesId = other.preservesId;
			this.completed = completed;
		}

		// Token: 0x04001C20 RID: 7200
		public readonly string id;

		// Token: 0x04001C21 RID: 7201
		public string preservesId;

		// Token: 0x04001C22 RID: 7202
		public readonly int? category;

		// Token: 0x04001C23 RID: 7203
		public readonly int stack;

		// Token: 0x04001C24 RID: 7204
		public readonly int quality;

		// Token: 0x04001C25 RID: 7205
		public bool completed;
	}
}

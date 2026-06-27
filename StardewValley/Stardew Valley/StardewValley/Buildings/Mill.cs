using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewValley.Inventories;
using StardewValley.Objects;

namespace StardewValley.Buildings
{
	// Token: 0x02000387 RID: 903
	[Obsolete("The Mill class is only used to preserve data from old save files. All mills were converted into plain Building instances based on the rules in Data/Buildings. The input and output items are now stored in Building.buildingChests with the 'Input' and 'Output' keys respectively.")]
	public class Mill : Building
	{
		// Token: 0x060037C7 RID: 14279 RVA: 0x002C3958 File Offset: 0x002C1B58
		public Mill(Vector2 tileLocation) : base("Mill", tileLocation)
		{
		}

		// Token: 0x060037C8 RID: 14280 RVA: 0x002C3966 File Offset: 0x002C1B66
		public Mill() : this(Vector2.Zero)
		{
		}

		// Token: 0x060037C9 RID: 14281 RVA: 0x002C3974 File Offset: 0x002C1B74
		public void TransferValuesToNewBuilding(Building targetBuilding)
		{
			Chest chest = this.obsolete_input;
			bool flag;
			if (chest == null)
			{
				flag = false;
			}
			else
			{
				Inventory items = chest.Items;
				int? num = (items != null) ? new int?(items.Count) : null;
				int num2 = 0;
				flag = (num.GetValueOrDefault() > num2 & num != null);
			}
			if (flag)
			{
				IInventory source = this.obsolete_input.Items;
				Chest target = targetBuilding.GetBuildingChest("Input");
				for (int i = 0; i < source.Count; i++)
				{
					Item item = source[i];
					if (item != null)
					{
						source[i] = null;
						target.addItem(item);
					}
				}
				this.obsolete_input = null;
			}
			Chest chest2 = this.obsolete_output;
			bool flag2;
			if (chest2 == null)
			{
				flag2 = false;
			}
			else
			{
				Inventory items2 = chest2.Items;
				int? num = (items2 != null) ? new int?(items2.Count) : null;
				int num2 = 0;
				flag2 = (num.GetValueOrDefault() > num2 & num != null);
			}
			if (flag2)
			{
				IInventory source2 = this.obsolete_output.Items;
				Chest target2 = targetBuilding.GetBuildingChest("Output");
				for (int j = 0; j < source2.Count; j++)
				{
					Item item2 = source2[j];
					if (item2 != null)
					{
						source2[j] = null;
						target2.addItem(item2);
					}
				}
				this.obsolete_output = null;
			}
		}

		// Token: 0x04002441 RID: 9281
		[XmlElement("input")]
		public Chest obsolete_input;

		// Token: 0x04002442 RID: 9282
		[XmlElement("output")]
		public Chest obsolete_output;
	}
}

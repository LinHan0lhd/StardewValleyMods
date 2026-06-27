using System;
using System.IO;
using System.Linq;
using Netcode;
using StardewValley.Menus;

namespace StardewValley.Network
{
	// Token: 0x020001DB RID: 475
	public class NetBundles : NetDictionary<int, bool[], NetArray<bool, NetBool>, SerializableDictionary<int, bool[]>, NetBundles>
	{
		// Token: 0x0600211C RID: 8476 RVA: 0x00172758 File Offset: 0x00170958
		protected override int ReadKey(BinaryReader reader)
		{
			int result = reader.ReadInt32();
			JunimoNoteMenu menu = Game1.activeClickableMenu as JunimoNoteMenu;
			if (menu != null)
			{
				menu.bundlesChanged = true;
			}
			return result;
		}

		// Token: 0x0600211D RID: 8477 RVA: 0x00172780 File Offset: 0x00170980
		protected override void WriteKey(BinaryWriter writer, int key)
		{
			writer.Write(key);
		}

		// Token: 0x0600211E RID: 8478 RVA: 0x00172789 File Offset: 0x00170989
		protected override void setFieldValue(NetArray<bool, NetBool> field, int key, bool[] value)
		{
			field.Set(value);
		}

		// Token: 0x0600211F RID: 8479 RVA: 0x00172792 File Offset: 0x00170992
		protected override bool[] getFieldValue(NetArray<bool, NetBool> field)
		{
			return field.ToArray<bool>();
		}

		// Token: 0x06002120 RID: 8480 RVA: 0x0017279A File Offset: 0x0017099A
		protected override bool[] getFieldTargetValue(NetArray<bool, NetBool> field)
		{
			return field.ToArray<bool>();
		}
	}
}

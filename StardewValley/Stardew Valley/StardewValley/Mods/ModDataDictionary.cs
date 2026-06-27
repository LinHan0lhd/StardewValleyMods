using System;
using Netcode;
using StardewValley.Network;

namespace StardewValley.Mods
{
	// Token: 0x0200022E RID: 558
	public class ModDataDictionary : NetStringDictionary<string, NetString>
	{
		// Token: 0x060024CF RID: 9423 RVA: 0x00192C50 File Offset: 0x00190E50
		public ModDataDictionary()
		{
			this.InterpolationWait = false;
		}

		// Token: 0x060024D0 RID: 9424 RVA: 0x00192C60 File Offset: 0x00190E60
		public virtual void SetFromSerialization(ModDataDictionary source)
		{
			base.Clear();
			if (source == null)
			{
				return;
			}
			foreach (string key in source.Keys)
			{
				base[key] = source[key];
			}
		}

		// Token: 0x060024D1 RID: 9425 RVA: 0x00192CC8 File Offset: 0x00190EC8
		public ModDataDictionary GetForSerialization()
		{
			if (Game1.game1 != null && Game1.game1.IsSaving && base.Length == 0)
			{
				return null;
			}
			return this;
		}

		// Token: 0x060024D2 RID: 9426 RVA: 0x00192CE8 File Offset: 0x00190EE8
		public void CopyFrom(ModDataDictionary dict)
		{
			base.CopyFrom(dict.Pairs);
		}
	}
}

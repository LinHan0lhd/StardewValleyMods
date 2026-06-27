using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace Netcode
{
	// Token: 0x02000051 RID: 81
	public class NetVector2HashSet : NetHashSet<Vector2>
	{
		// Token: 0x06000348 RID: 840 RVA: 0x00010A38 File Offset: 0x0000EC38
		public override Vector2 ReadValue(BinaryReader reader)
		{
			float x = reader.ReadSingle();
			float y = reader.ReadSingle();
			return new Vector2(x, y);
		}

		// Token: 0x06000349 RID: 841 RVA: 0x00010A58 File Offset: 0x0000EC58
		public override void WriteValue(BinaryWriter writer, Vector2 value)
		{
			writer.Write(value.X);
			writer.Write(value.Y);
		}
	}
}

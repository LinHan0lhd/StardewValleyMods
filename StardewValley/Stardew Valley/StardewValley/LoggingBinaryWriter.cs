using System;
using System.Collections.Generic;
using System.IO;
using Netcode;

namespace StardewValley
{
	// Token: 0x020000E0 RID: 224
	public class LoggingBinaryWriter : BinaryWriter, ILoggingWriter
	{
		// Token: 0x170001EF RID: 495
		// (get) Token: 0x060010D0 RID: 4304 RVA: 0x000C8493 File Offset: 0x000C6693
		public override Stream BaseStream
		{
			get
			{
				return this.writer.BaseStream;
			}
		}

		// Token: 0x060010D1 RID: 4305 RVA: 0x000C84A0 File Offset: 0x000C66A0
		public LoggingBinaryWriter(BinaryWriter writer)
		{
			this.writer = writer;
		}

		// Token: 0x060010D2 RID: 4306 RVA: 0x000C84BC File Offset: 0x000C66BC
		private string currentPath()
		{
			if (this.stack.Count == 0)
			{
				return "";
			}
			return this.stack[this.stack.Count - 1].Key;
		}

		// Token: 0x060010D3 RID: 4307 RVA: 0x000C84FC File Offset: 0x000C66FC
		public void Push(string name)
		{
			this.stack.Add(new KeyValuePair<string, long>(this.currentPath() + "/" + name, this.BaseStream.Position));
		}

		// Token: 0x060010D4 RID: 4308 RVA: 0x000C852C File Offset: 0x000C672C
		public void Pop()
		{
			KeyValuePair<string, long> pair = this.stack[this.stack.Count - 1];
			string path = pair.Key;
			long start = pair.Value;
			long length = this.BaseStream.Position - start;
			this.stack.RemoveAt(this.stack.Count - 1);
			Game1.multiplayer.logging.LogWrite(path, length);
		}

		// Token: 0x060010D5 RID: 4309 RVA: 0x000C8599 File Offset: 0x000C6799
		public override void Close()
		{
			base.Close();
			this.writer.Close();
		}

		// Token: 0x060010D6 RID: 4310 RVA: 0x000C85AC File Offset: 0x000C67AC
		public override void Flush()
		{
			this.writer.Flush();
		}

		// Token: 0x060010D7 RID: 4311 RVA: 0x000C85B9 File Offset: 0x000C67B9
		public override long Seek(int offset, SeekOrigin origin)
		{
			return this.writer.Seek(offset, origin);
		}

		// Token: 0x060010D8 RID: 4312 RVA: 0x000C85C8 File Offset: 0x000C67C8
		public override void Write(short value)
		{
			this.writer.Write(value);
		}

		// Token: 0x060010D9 RID: 4313 RVA: 0x000C85D6 File Offset: 0x000C67D6
		public override void Write(ushort value)
		{
			this.writer.Write(value);
		}

		// Token: 0x060010DA RID: 4314 RVA: 0x000C85E4 File Offset: 0x000C67E4
		public override void Write(int value)
		{
			this.writer.Write(value);
		}

		// Token: 0x060010DB RID: 4315 RVA: 0x000C85F2 File Offset: 0x000C67F2
		public override void Write(uint value)
		{
			this.writer.Write(value);
		}

		// Token: 0x060010DC RID: 4316 RVA: 0x000C8600 File Offset: 0x000C6800
		public override void Write(long value)
		{
			this.writer.Write(value);
		}

		// Token: 0x060010DD RID: 4317 RVA: 0x000C860E File Offset: 0x000C680E
		public override void Write(ulong value)
		{
			this.writer.Write(value);
		}

		// Token: 0x060010DE RID: 4318 RVA: 0x000C861C File Offset: 0x000C681C
		public override void Write(float value)
		{
			this.writer.Write(value);
		}

		// Token: 0x060010DF RID: 4319 RVA: 0x000C862A File Offset: 0x000C682A
		public override void Write(string value)
		{
			this.writer.Write(value);
		}

		// Token: 0x060010E0 RID: 4320 RVA: 0x000C8638 File Offset: 0x000C6838
		public override void Write(decimal value)
		{
			this.writer.Write(value);
		}

		// Token: 0x060010E1 RID: 4321 RVA: 0x000C8646 File Offset: 0x000C6846
		public override void Write(bool value)
		{
			this.writer.Write(value);
		}

		// Token: 0x060010E2 RID: 4322 RVA: 0x000C8654 File Offset: 0x000C6854
		public override void Write(byte value)
		{
			this.writer.Write(value);
		}

		// Token: 0x060010E3 RID: 4323 RVA: 0x000C8662 File Offset: 0x000C6862
		public override void Write(sbyte value)
		{
			this.writer.Write(value);
		}

		// Token: 0x060010E4 RID: 4324 RVA: 0x000C8670 File Offset: 0x000C6870
		public override void Write(byte[] buffer)
		{
			this.writer.Write(buffer);
		}

		// Token: 0x060010E5 RID: 4325 RVA: 0x000C867E File Offset: 0x000C687E
		public override void Write(byte[] buffer, int index, int count)
		{
			this.writer.Write(buffer, index, count);
		}

		// Token: 0x060010E6 RID: 4326 RVA: 0x000C868E File Offset: 0x000C688E
		public override void Write(char ch)
		{
			this.writer.Write(ch);
		}

		// Token: 0x060010E7 RID: 4327 RVA: 0x000C869C File Offset: 0x000C689C
		public override void Write(char[] chars)
		{
			this.writer.Write(chars);
		}

		// Token: 0x060010E8 RID: 4328 RVA: 0x000C86AA File Offset: 0x000C68AA
		public override void Write(char[] chars, int index, int count)
		{
			this.writer.Write(chars, index, count);
		}

		// Token: 0x060010E9 RID: 4329 RVA: 0x000C86BA File Offset: 0x000C68BA
		public override void Write(double value)
		{
			this.writer.Write(value);
		}

		// Token: 0x04000A17 RID: 2583
		protected BinaryWriter writer;

		// Token: 0x04000A18 RID: 2584
		protected List<KeyValuePair<string, long>> stack = new List<KeyValuePair<string, long>>();
	}
}

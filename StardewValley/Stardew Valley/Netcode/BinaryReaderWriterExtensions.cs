using System;
using System.Collections;
using System.IO;
using Microsoft.Xna.Framework;

namespace Netcode
{
	// Token: 0x02000027 RID: 39
	public static class BinaryReaderWriterExtensions
	{
		// Token: 0x06000121 RID: 289 RVA: 0x0000B010 File Offset: 0x00009210
		public static void ReadSkippable(this BinaryReader reader, Action readAction)
		{
			uint size = reader.ReadUInt32();
			long startPosition = reader.BaseStream.Position;
			readAction();
			if (reader.BaseStream.Position > startPosition + (long)((ulong)size))
			{
				throw new InvalidOperationException();
			}
			reader.BaseStream.Position = startPosition + (long)((ulong)size);
		}

		// Token: 0x06000122 RID: 290 RVA: 0x0000B05C File Offset: 0x0000925C
		public static byte[] ReadSkippableBytes(this BinaryReader reader)
		{
			uint dataLength = reader.ReadUInt32();
			return reader.ReadBytes((int)dataLength);
		}

		// Token: 0x06000123 RID: 291 RVA: 0x0000B077 File Offset: 0x00009277
		public static void Skip(this BinaryReader reader)
		{
			reader.ReadSkippable(delegate
			{
			});
		}

		// Token: 0x06000124 RID: 292 RVA: 0x0000B0A0 File Offset: 0x000092A0
		public static void WriteSkippable(this BinaryWriter writer, Action writeAction)
		{
			long sizePosition = writer.BaseStream.Position;
			writer.Write(0U);
			long startPosition = writer.BaseStream.Position;
			writeAction();
			long endPosition = writer.BaseStream.Position;
			long size = endPosition - startPosition;
			writer.BaseStream.Position = sizePosition;
			writer.Write((uint)size);
			writer.BaseStream.Position = endPosition;
		}

		// Token: 0x06000125 RID: 293 RVA: 0x0000B104 File Offset: 0x00009304
		public static BitArray ReadBitArray(this BinaryReader reader)
		{
			int length = (int)reader.Read7BitEncoded();
			return new BitArray(reader.ReadBytes((length + 7) / 8))
			{
				Length = length
			};
		}

		// Token: 0x06000126 RID: 294 RVA: 0x0000B130 File Offset: 0x00009330
		public static void WriteBitArray(this BinaryWriter writer, BitArray bits)
		{
			byte[] buf = new byte[(bits.Length + 7) / 8];
			bits.CopyTo(buf, 0);
			writer.Write7BitEncoded((uint)bits.Length);
			writer.Write(buf);
		}

		// Token: 0x06000127 RID: 295 RVA: 0x0000B168 File Offset: 0x00009368
		public static void Write7BitEncoded(this BinaryWriter writer, uint value)
		{
			do
			{
				byte chunk = (byte)(value & 127U);
				value >>= 7;
				if (value != 0U)
				{
					chunk |= 128;
				}
				writer.Write(chunk);
			}
			while (value != 0U);
		}

		// Token: 0x06000128 RID: 296 RVA: 0x0000B198 File Offset: 0x00009398
		public static uint Read7BitEncoded(this BinaryReader reader)
		{
			uint value = 0U;
			byte chunk = reader.ReadByte();
			int shift = 0;
			while ((chunk & 128) != 0)
			{
				value |= (uint)((uint)(chunk & 127) << shift);
				shift += 7;
				chunk = reader.ReadByte();
			}
			return value | (uint)((uint)(chunk & 127) << shift);
		}

		// Token: 0x06000129 RID: 297 RVA: 0x0000B1DF File Offset: 0x000093DF
		public static Guid ReadGuid(this BinaryReader reader)
		{
			return new Guid(reader.ReadBytes(16));
		}

		// Token: 0x0600012A RID: 298 RVA: 0x0000B1EE File Offset: 0x000093EE
		public static void WriteGuid(this BinaryWriter writer, Guid guid)
		{
			writer.Write(guid.ToByteArray());
		}

		// Token: 0x0600012B RID: 299 RVA: 0x0000B200 File Offset: 0x00009400
		public static Vector2 ReadVector2(this BinaryReader reader)
		{
			float x = reader.ReadSingle();
			float y = reader.ReadSingle();
			return new Vector2(x, y);
		}

		// Token: 0x0600012C RID: 300 RVA: 0x0000B220 File Offset: 0x00009420
		public static void WriteVector2(this BinaryWriter writer, Vector2 vec)
		{
			writer.Write(vec.X);
			writer.Write(vec.Y);
		}

		// Token: 0x0600012D RID: 301 RVA: 0x0000B23C File Offset: 0x0000943C
		public static Point ReadPoint(this BinaryReader reader)
		{
			int x = reader.ReadInt32();
			int y = reader.ReadInt32();
			return new Point(x, y);
		}

		// Token: 0x0600012E RID: 302 RVA: 0x0000B25C File Offset: 0x0000945C
		public static void WritePoint(this BinaryWriter writer, Point p)
		{
			writer.Write(p.X);
			writer.Write(p.Y);
		}

		// Token: 0x0600012F RID: 303 RVA: 0x0000B278 File Offset: 0x00009478
		public static Rectangle ReadRectangle(this BinaryReader reader)
		{
			Point pos = reader.ReadPoint();
			Point size = reader.ReadPoint();
			return new Rectangle(pos.X, pos.Y, size.X, size.Y);
		}

		// Token: 0x06000130 RID: 304 RVA: 0x0000B2B0 File Offset: 0x000094B0
		public static void WriteRectangle(this BinaryWriter writer, Rectangle rect)
		{
			writer.WritePoint(rect.Location);
			writer.WritePoint(new Point(rect.Width, rect.Height));
		}

		// Token: 0x06000131 RID: 305 RVA: 0x0000B2D8 File Offset: 0x000094D8
		public static Color ReadColor(this BinaryReader reader)
		{
			return new Color
			{
				PackedValue = reader.ReadUInt32()
			};
		}

		// Token: 0x06000132 RID: 306 RVA: 0x0000B2FB File Offset: 0x000094FB
		public static void WriteColor(this BinaryWriter writer, Color color)
		{
			writer.Write(color.PackedValue);
		}

		// Token: 0x06000133 RID: 307 RVA: 0x0000B30A File Offset: 0x0000950A
		public static T ReadEnum<T>(this BinaryReader reader) where T : struct, IConvertible
		{
			return (T)((object)Enum.ToObject(typeof(T), reader.ReadInt16()));
		}

		// Token: 0x06000134 RID: 308 RVA: 0x0000B326 File Offset: 0x00009526
		public static void WriteEnum<T>(this BinaryWriter writer, T enumValue) where T : struct, IConvertible
		{
			writer.Write(Convert.ToInt16(enumValue));
		}

		// Token: 0x06000135 RID: 309 RVA: 0x0000B339 File Offset: 0x00009539
		public static void WriteEnum(this BinaryWriter writer, object enumValue)
		{
			writer.Write(Convert.ToInt16(enumValue));
		}

		// Token: 0x06000136 RID: 310 RVA: 0x0000B348 File Offset: 0x00009548
		public static void Push(this BinaryWriter writer, string name)
		{
			ILoggingWriter loggingWriter = writer as ILoggingWriter;
			if (loggingWriter != null)
			{
				loggingWriter.Push(name);
			}
		}

		// Token: 0x06000137 RID: 311 RVA: 0x0000B368 File Offset: 0x00009568
		public static void Pop(this BinaryWriter writer)
		{
			ILoggingWriter loggingWriter = writer as ILoggingWriter;
			if (loggingWriter != null)
			{
				loggingWriter.Pop();
			}
		}
	}
}

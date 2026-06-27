using System;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Xna.Framework;
using Netcode;

namespace StardewValley.Network
{
	// Token: 0x020001EF RID: 495
	public struct OutgoingMessage
	{
		// Token: 0x170003B3 RID: 947
		// (get) Token: 0x0600221A RID: 8730 RVA: 0x001759F4 File Offset: 0x00173BF4
		public byte MessageType
		{
			get
			{
				return this.messageType;
			}
		}

		// Token: 0x170003B4 RID: 948
		// (get) Token: 0x0600221B RID: 8731 RVA: 0x001759FC File Offset: 0x00173BFC
		public long FarmerID
		{
			get
			{
				return this.farmerID;
			}
		}

		// Token: 0x170003B5 RID: 949
		// (get) Token: 0x0600221C RID: 8732 RVA: 0x00175A04 File Offset: 0x00173C04
		public Farmer SourceFarmer
		{
			get
			{
				return Game1.GetPlayer(this.farmerID, false) ?? Game1.MasterPlayer;
			}
		}

		// Token: 0x170003B6 RID: 950
		// (get) Token: 0x0600221D RID: 8733 RVA: 0x00175A1B File Offset: 0x00173C1B
		public ReadOnlyCollection<object> Data
		{
			get
			{
				return Array.AsReadOnly<object>(this.data);
			}
		}

		// Token: 0x0600221E RID: 8734 RVA: 0x00175A28 File Offset: 0x00173C28
		public OutgoingMessage(byte messageType, long farmerID, params object[] data)
		{
			this.messageType = messageType;
			this.farmerID = farmerID;
			this.data = data;
		}

		// Token: 0x0600221F RID: 8735 RVA: 0x00175A3F File Offset: 0x00173C3F
		public OutgoingMessage(byte messageType, Farmer sourceFarmer, params object[] data)
		{
			this = new OutgoingMessage(messageType, sourceFarmer.UniqueMultiplayerID, data);
		}

		// Token: 0x06002220 RID: 8736 RVA: 0x00175A4F File Offset: 0x00173C4F
		public OutgoingMessage(IncomingMessage message)
		{
			this = new OutgoingMessage(message.MessageType, message.FarmerID, new object[]
			{
				message.Data
			});
		}

		// Token: 0x06002221 RID: 8737 RVA: 0x00175A74 File Offset: 0x00173C74
		public void Write(BinaryWriter writer)
		{
			writer.Write(this.messageType);
			writer.Write(this.farmerID);
			object[] data = this.data;
			writer.WriteSkippable(delegate
			{
				foreach (object value in data)
				{
					if (value is Vector2)
					{
						Vector2 vector = (Vector2)value;
						writer.Write(vector.X);
						writer.Write(vector.Y);
					}
					else if (value is Guid)
					{
						Guid guid = (Guid)value;
						writer.Write(guid.ToByteArray());
					}
					else
					{
						byte[] bytes = value as byte[];
						if (bytes == null)
						{
							if (value is bool)
							{
								bool boolVal = (bool)value;
								writer.Write((boolVal > false) ? 1 : 0);
							}
							else if (value is byte)
							{
								byte byteVal = (byte)value;
								writer.Write(byteVal);
							}
							else if (value is int)
							{
								int intVal = (int)value;
								writer.Write(intVal);
							}
							else if (value is short)
							{
								short shortVal = (short)value;
								writer.Write(shortVal);
							}
							else if (value is float)
							{
								float floatVal = (float)value;
								writer.Write(floatVal);
							}
							else if (value is long)
							{
								long longVal = (long)value;
								writer.Write(longVal);
							}
							else
							{
								string str = value as string;
								if (str == null)
								{
									string[] array = value as string[];
									if (array == null)
									{
										if (!(value is IConvertible))
										{
											throw new InvalidDataException();
										}
										if (!value.GetType().IsValueType)
										{
											throw new InvalidDataException();
										}
										writer.WriteEnum(value);
									}
									else
									{
										writer.Write((byte)array.Length);
										for (int i = 0; i < array.Length; i++)
										{
											writer.Write(array[i]);
										}
									}
								}
								else
								{
									writer.Write(str);
								}
							}
						}
						else
						{
							writer.Write(bytes);
						}
					}
				}
			});
		}

		// Token: 0x04001457 RID: 5207
		private byte messageType;

		// Token: 0x04001458 RID: 5208
		private long farmerID;

		// Token: 0x04001459 RID: 5209
		private object[] data;
	}
}

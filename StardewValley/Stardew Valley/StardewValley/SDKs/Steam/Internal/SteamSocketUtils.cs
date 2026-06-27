using System;
using System.IO;
using System.Runtime.InteropServices;
using StardewValley.Network;
using Steamworks;

namespace StardewValley.SDKs.Steam.Internal
{
	// Token: 0x0200016B RID: 363
	internal static class SteamSocketUtils
	{
		// Token: 0x06001BAF RID: 7087 RVA: 0x0013E7A4 File Offset: 0x0013C9A4
		internal static SteamNetworkingConfigValue_t[] GetNetworkingOptions()
		{
			SteamNetworkingConfigValue_t[] array = new SteamNetworkingConfigValue_t[1];
			int num = 0;
			SteamNetworkingConfigValue_t steamNetworkingConfigValue_t = default(SteamNetworkingConfigValue_t);
			steamNetworkingConfigValue_t.m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_SendBufferSize;
			steamNetworkingConfigValue_t.m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32;
			steamNetworkingConfigValue_t.m_val.m_int32 = 1048576;
			array[num] = steamNetworkingConfigValue_t;
			return array;
		}

		// Token: 0x06001BB0 RID: 7088 RVA: 0x0013E7EC File Offset: 0x0013C9EC
		internal static void ProcessSteamMessage(IntPtr messagePtr, IncomingMessage message, out HSteamNetConnection messageConnection, BandwidthLogger bandwidthLogger)
		{
			SteamNetworkingMessage_t messageSteam = (SteamNetworkingMessage_t)Marshal.PtrToStructure(messagePtr, typeof(SteamNetworkingMessage_t));
			messageConnection = messageSteam.m_conn;
			byte[] rawData = new byte[messageSteam.m_cbSize];
			Marshal.Copy(messageSteam.m_pData, rawData, 0, rawData.Length);
			using (MemoryStream messageStream = new MemoryStream(Program.netCompression.DecompressBytes(rawData)))
			{
				messageStream.Position = 0L;
				using (BinaryReader messageReader = new BinaryReader(messageStream))
				{
					message.Read(messageReader);
				}
			}
			SteamNetworkingMessage_t.Release(messagePtr);
			if (bandwidthLogger != null)
			{
				bandwidthLogger.RecordBytesDown((long)rawData.Length);
			}
		}

		// Token: 0x06001BB1 RID: 7089 RVA: 0x0013E8A4 File Offset: 0x0013CAA4
		internal unsafe static void SendMessage(HSteamNetConnection messageConnection, OutgoingMessage message, BandwidthLogger bandwidthLogger, Action<HSteamNetConnection> onDisconnected = null)
		{
			byte[] messageBytes = null;
			using (MemoryStream messageStream = new MemoryStream())
			{
				using (BinaryWriter messageWriter = new BinaryWriter(messageStream))
				{
					message.Write(messageWriter);
					messageStream.Seek(0L, SeekOrigin.Begin);
					messageBytes = messageStream.ToArray();
				}
			}
			byte[] data = Program.netCompression.CompressAbove(messageBytes, 1024);
			byte[] array;
			byte* ptr;
			if ((array = data) == null || array.Length == 0)
			{
				ptr = null;
			}
			else
			{
				ptr = &array[0];
			}
			long num;
			EResult result = SteamNetworkingSockets.SendMessageToConnection(messageConnection, (IntPtr)((void*)ptr), Convert.ToUInt32(data.Length), 8, out num);
			array = null;
			if (result != EResult.k_EResultOK)
			{
				Game1.log.Warn("Failed to send message (" + result.ToString() + "). Closing connection.");
				SteamSocketUtils.CloseConnection(messageConnection, onDisconnected);
				return;
			}
			if (bandwidthLogger != null)
			{
				bandwidthLogger.RecordBytesUp((long)data.Length);
			}
		}

		// Token: 0x06001BB2 RID: 7090 RVA: 0x0013E998 File Offset: 0x0013CB98
		internal static void CloseConnection(HSteamNetConnection connection, Action<HSteamNetConnection> onDisconnected = null)
		{
			if (connection == HSteamNetConnection.Invalid)
			{
				return;
			}
			SteamNetworkingSockets.SetConnectionPollGroup(connection, HSteamNetPollGroup.Invalid);
			if (onDisconnected != null)
			{
				onDisconnected(connection);
			}
			SteamNetworkingSockets.CloseConnection(connection, 1000, null, true);
		}
	}
}

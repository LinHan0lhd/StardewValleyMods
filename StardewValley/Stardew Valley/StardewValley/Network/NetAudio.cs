using System;
using System.IO;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Audio;

namespace StardewValley.Network
{
	// Token: 0x020001D8 RID: 472
	public class NetAudio : INetObject<NetFields>
	{
		// Token: 0x17000367 RID: 871
		// (get) Token: 0x060020FB RID: 8443 RVA: 0x001723F6 File Offset: 0x001705F6
		public NetFields NetFields { get; } = new NetFields("NetAudio");

		// Token: 0x17000368 RID: 872
		// (get) Token: 0x060020FC RID: 8444 RVA: 0x001723FE File Offset: 0x001705FE
		public NetDictionary<string, bool, NetBool, SerializableDictionary<string, bool>, NetStringDictionary<bool, NetBool>>.KeysCollection ActiveCues
		{
			get
			{
				return this.activeCues.Keys;
			}
		}

		// Token: 0x060020FD RID: 8445 RVA: 0x0017240C File Offset: 0x0017060C
		public NetAudio(GameLocation location)
		{
			this.location = location;
			this.NetFields.SetOwner(this).AddField(this.audioEvent, "audioEvent").AddField(this.activeCues, "activeCues");
			this.audioEvent.AddReaderHandler(new Action<BinaryReader>(this.handleAudioEvent));
		}

		// Token: 0x060020FE RID: 8446 RVA: 0x00172490 File Offset: 0x00170690
		private void handleAudioEvent(BinaryReader reader)
		{
			string audioName;
			Vector2? position;
			int? pitch;
			SoundContext context;
			this.Read(reader, out audioName, out position, out pitch, out context);
			ICue cue;
			Game1.sounds.PlayLocal(audioName, this.location, position, pitch, context, out cue);
		}

		// Token: 0x060020FF RID: 8447 RVA: 0x001724C3 File Offset: 0x001706C3
		public void Update()
		{
			this.audioEvent.Poll();
		}

		// Token: 0x06002100 RID: 8448 RVA: 0x001724D0 File Offset: 0x001706D0
		public void Fire(string audioName, Vector2? position, int? pitch, SoundContext context)
		{
			this.audioEvent.Fire(delegate(BinaryWriter writer)
			{
				writer.Write(audioName);
				writer.WriteVector2(position ?? new Vector2(-2.1474836E+09f));
				writer.Write(pitch.GetValueOrDefault(int.MinValue));
				writer.Write((int)context);
			});
			this.audioEvent.Poll();
		}

		// Token: 0x06002101 RID: 8449 RVA: 0x00172524 File Offset: 0x00170724
		public void Read(BinaryReader reader, out string audioName, out Vector2? position, out int? pitch, out SoundContext context)
		{
			audioName = reader.ReadString();
			position = new Vector2?(reader.ReadVector2());
			pitch = new int?(reader.ReadInt32());
			context = (SoundContext)reader.ReadInt32();
			if ((int)position.Value.X == -2147483648 && (int)position.Value.Y == -2147483648)
			{
				position = null;
			}
			if (pitch.GetValueOrDefault() == -2147483648)
			{
				pitch = null;
			}
		}

		// Token: 0x06002102 RID: 8450 RVA: 0x001725A8 File Offset: 0x001707A8
		public void StartPlaying(string cueName)
		{
			this.activeCues[cueName] = false;
		}

		// Token: 0x06002103 RID: 8451 RVA: 0x001725B7 File Offset: 0x001707B7
		public void StopPlaying(string cueName)
		{
			this.activeCues.Remove(cueName);
		}

		// Token: 0x040013E0 RID: 5088
		private readonly NetEventBinary audioEvent = new NetEventBinary();

		// Token: 0x040013E1 RID: 5089
		private readonly NetStringDictionary<bool, NetBool> activeCues = new NetStringDictionary<bool, NetBool>();

		// Token: 0x040013E2 RID: 5090
		private GameLocation location;
	}
}

using System;
using System.Collections.Generic;
using System.IO;

namespace Netcode
{
	// Token: 0x02000038 RID: 56
	public abstract class AbstractNetEvent1<T> : AbstractNetSerializable
	{
		// Token: 0x14000009 RID: 9
		// (add) Token: 0x06000240 RID: 576 RVA: 0x0000DC3C File Offset: 0x0000BE3C
		// (remove) Token: 0x06000241 RID: 577 RVA: 0x0000DC74 File Offset: 0x0000BE74
		public event AbstractNetEvent1<T>.Event onEvent;

		// Token: 0x06000242 RID: 578 RVA: 0x0000DCAC File Offset: 0x0000BEAC
		public bool HasPendingEvent(Predicate<T> match)
		{
			return this.incomingEvents.Exists((AbstractNetEvent1<T>.EventRecording e) => match(e.arg));
		}

		// Token: 0x06000243 RID: 579 RVA: 0x0000DCDD File Offset: 0x0000BEDD
		public void Clear()
		{
			this.outgoingEvents.Clear();
			this.incomingEvents.Clear();
		}

		// Token: 0x06000244 RID: 580 RVA: 0x0000DCF8 File Offset: 0x0000BEF8
		public void Fire(T arg)
		{
			AbstractNetEvent1<T>.EventRecording recording = new AbstractNetEvent1<T>.EventRecording(arg, base.GetLocalTick());
			this.outgoingEvents.Add(recording);
			this.incomingEvents.Add(recording);
			base.MarkDirty();
			this.Poll();
		}

		// Token: 0x06000245 RID: 581 RVA: 0x0000DD38 File Offset: 0x0000BF38
		public void Poll()
		{
			List<AbstractNetEvent1<T>.EventRecording> triggeredEvents = null;
			foreach (AbstractNetEvent1<T>.EventRecording e in this.incomingEvents)
			{
				if (base.Root != null && base.GetLocalTick() < e.timestamp)
				{
					break;
				}
				if (triggeredEvents == null)
				{
					triggeredEvents = new List<AbstractNetEvent1<T>.EventRecording>();
				}
				triggeredEvents.Add(e);
			}
			if (triggeredEvents != null && triggeredEvents.Count > 0)
			{
				this.incomingEvents.RemoveAll(new Predicate<AbstractNetEvent1<T>.EventRecording>(triggeredEvents.Contains));
				if (this.onEvent != null)
				{
					foreach (AbstractNetEvent1<T>.EventRecording e2 in triggeredEvents)
					{
						this.onEvent(e2.arg);
					}
				}
			}
		}

		// Token: 0x06000246 RID: 582
		protected abstract T readEventArg(BinaryReader reader, NetVersion version);

		// Token: 0x06000247 RID: 583
		protected abstract void writeEventArg(BinaryWriter writer, T arg);

		// Token: 0x06000248 RID: 584 RVA: 0x0000DE20 File Offset: 0x0000C020
		public override void Read(BinaryReader reader, NetVersion version)
		{
			uint count = reader.Read7BitEncoded();
			uint timestamp = base.GetLocalTick();
			if (this.InterpolationWait)
			{
				timestamp += (uint)base.Root.Clock.InterpolationTicks;
			}
			for (uint i = 0U; i < count; i += 1U)
			{
				uint delay = reader.ReadUInt32();
				this.incomingEvents.Add(new AbstractNetEvent1<T>.EventRecording(this.readEventArg(reader, version), timestamp + delay));
			}
			this.ChangeVersion.Merge(version);
		}

		// Token: 0x06000249 RID: 585 RVA: 0x0000DE90 File Offset: 0x0000C090
		public override void ReadFull(BinaryReader reader, NetVersion version)
		{
			this.ChangeVersion.Merge(version);
		}

		// Token: 0x0600024A RID: 586 RVA: 0x0000DEA0 File Offset: 0x0000C0A0
		public override void Write(BinaryWriter writer)
		{
			writer.Write7BitEncoded((uint)this.outgoingEvents.Count);
			if (this.outgoingEvents.Count > 0)
			{
				uint baseTime = this.outgoingEvents[0].timestamp;
				foreach (AbstractNetEvent1<T>.EventRecording e in this.outgoingEvents)
				{
					writer.Write(e.timestamp - baseTime);
					this.writeEventArg(writer, e.arg);
				}
			}
			this.outgoingEvents.Clear();
		}

		// Token: 0x0600024B RID: 587 RVA: 0x0000DF44 File Offset: 0x0000C144
		protected override void CleanImpl()
		{
			base.CleanImpl();
			this.outgoingEvents.Clear();
		}

		// Token: 0x0600024C RID: 588 RVA: 0x0000DF57 File Offset: 0x0000C157
		public override void WriteFull(BinaryWriter writer)
		{
		}

		// Token: 0x04000163 RID: 355
		public bool InterpolationWait = true;

		// Token: 0x04000164 RID: 356
		private List<AbstractNetEvent1<T>.EventRecording> outgoingEvents = new List<AbstractNetEvent1<T>.EventRecording>();

		// Token: 0x04000165 RID: 357
		private List<AbstractNetEvent1<T>.EventRecording> incomingEvents = new List<AbstractNetEvent1<T>.EventRecording>();

		// Token: 0x020003DB RID: 987
		public class EventRecording
		{
			// Token: 0x060039CD RID: 14797 RVA: 0x002D7BEF File Offset: 0x002D5DEF
			public EventRecording(T arg, uint timestamp)
			{
				this.arg = arg;
				this.timestamp = timestamp;
			}

			// Token: 0x040026AA RID: 9898
			public T arg;

			// Token: 0x040026AB RID: 9899
			public uint timestamp;
		}

		// Token: 0x020003DC RID: 988
		// (Invoke) Token: 0x060039CF RID: 14799
		public delegate void Event(T arg);
	}
}

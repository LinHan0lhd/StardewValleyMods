using System;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley
{
	// Token: 0x020000B0 RID: 176
	public class Friendship : INetObject<NetFields>
	{
		// Token: 0x1700013C RID: 316
		// (get) Token: 0x06000A3C RID: 2620 RVA: 0x0006EFAA File Offset: 0x0006D1AA
		[XmlIgnore]
		public NetFields NetFields { get; } = new NetFields("Friendship");

		// Token: 0x1700013D RID: 317
		// (get) Token: 0x06000A3D RID: 2621 RVA: 0x0006EFB2 File Offset: 0x0006D1B2
		// (set) Token: 0x06000A3E RID: 2622 RVA: 0x0006EFBF File Offset: 0x0006D1BF
		public int Points
		{
			get
			{
				return this.points.Value;
			}
			set
			{
				this.points.Value = value;
			}
		}

		// Token: 0x1700013E RID: 318
		// (get) Token: 0x06000A3F RID: 2623 RVA: 0x0006EFCD File Offset: 0x0006D1CD
		// (set) Token: 0x06000A40 RID: 2624 RVA: 0x0006EFDA File Offset: 0x0006D1DA
		public int GiftsThisWeek
		{
			get
			{
				return this.giftsThisWeek.Value;
			}
			set
			{
				this.giftsThisWeek.Value = value;
			}
		}

		// Token: 0x1700013F RID: 319
		// (get) Token: 0x06000A41 RID: 2625 RVA: 0x0006EFE8 File Offset: 0x0006D1E8
		// (set) Token: 0x06000A42 RID: 2626 RVA: 0x0006EFF5 File Offset: 0x0006D1F5
		public int GiftsToday
		{
			get
			{
				return this.giftsToday.Value;
			}
			set
			{
				this.giftsToday.Value = value;
			}
		}

		// Token: 0x17000140 RID: 320
		// (get) Token: 0x06000A43 RID: 2627 RVA: 0x0006F003 File Offset: 0x0006D203
		// (set) Token: 0x06000A44 RID: 2628 RVA: 0x0006F010 File Offset: 0x0006D210
		public WorldDate LastGiftDate
		{
			get
			{
				return this.lastGiftDate.Value;
			}
			set
			{
				this.lastGiftDate.Value = value;
			}
		}

		// Token: 0x17000141 RID: 321
		// (get) Token: 0x06000A45 RID: 2629 RVA: 0x0006F01E File Offset: 0x0006D21E
		// (set) Token: 0x06000A46 RID: 2630 RVA: 0x0006F02B File Offset: 0x0006D22B
		public bool TalkedToToday
		{
			get
			{
				return this.talkedToToday.Value;
			}
			set
			{
				this.talkedToToday.Value = value;
			}
		}

		// Token: 0x17000142 RID: 322
		// (get) Token: 0x06000A47 RID: 2631 RVA: 0x0006F039 File Offset: 0x0006D239
		// (set) Token: 0x06000A48 RID: 2632 RVA: 0x0006F046 File Offset: 0x0006D246
		public bool ProposalRejected
		{
			get
			{
				return this.proposalRejected.Value;
			}
			set
			{
				this.proposalRejected.Value = value;
			}
		}

		// Token: 0x17000143 RID: 323
		// (get) Token: 0x06000A49 RID: 2633 RVA: 0x0006F054 File Offset: 0x0006D254
		// (set) Token: 0x06000A4A RID: 2634 RVA: 0x0006F061 File Offset: 0x0006D261
		public WorldDate WeddingDate
		{
			get
			{
				return this.weddingDate.Value;
			}
			set
			{
				this.weddingDate.Value = value;
			}
		}

		// Token: 0x17000144 RID: 324
		// (get) Token: 0x06000A4B RID: 2635 RVA: 0x0006F06F File Offset: 0x0006D26F
		// (set) Token: 0x06000A4C RID: 2636 RVA: 0x0006F07C File Offset: 0x0006D27C
		public WorldDate NextBirthingDate
		{
			get
			{
				return this.nextBirthingDate.Value;
			}
			set
			{
				this.nextBirthingDate.Value = value;
			}
		}

		// Token: 0x17000145 RID: 325
		// (get) Token: 0x06000A4D RID: 2637 RVA: 0x0006F08A File Offset: 0x0006D28A
		// (set) Token: 0x06000A4E RID: 2638 RVA: 0x0006F097 File Offset: 0x0006D297
		public FriendshipStatus Status
		{
			get
			{
				return this.status.Value;
			}
			set
			{
				this.status.Value = value;
			}
		}

		// Token: 0x17000146 RID: 326
		// (get) Token: 0x06000A4F RID: 2639 RVA: 0x0006F0A5 File Offset: 0x0006D2A5
		// (set) Token: 0x06000A50 RID: 2640 RVA: 0x0006F0B2 File Offset: 0x0006D2B2
		public long Proposer
		{
			get
			{
				return this.proposer.Value;
			}
			set
			{
				this.proposer.Value = value;
			}
		}

		// Token: 0x17000147 RID: 327
		// (get) Token: 0x06000A51 RID: 2641 RVA: 0x0006F0C0 File Offset: 0x0006D2C0
		// (set) Token: 0x06000A52 RID: 2642 RVA: 0x0006F0CD File Offset: 0x0006D2CD
		public bool RoommateMarriage
		{
			get
			{
				return this.roommateMarriage.Value;
			}
			set
			{
				this.roommateMarriage.Value = value;
			}
		}

		// Token: 0x17000148 RID: 328
		// (get) Token: 0x06000A53 RID: 2643 RVA: 0x0006F0DB File Offset: 0x0006D2DB
		public int DaysMarried
		{
			get
			{
				if (this.WeddingDate == null || this.WeddingDate.TotalDays > Game1.Date.TotalDays)
				{
					return 0;
				}
				return Game1.Date.TotalDays - this.WeddingDate.TotalDays;
			}
		}

		// Token: 0x17000149 RID: 329
		// (get) Token: 0x06000A54 RID: 2644 RVA: 0x0006F11A File Offset: 0x0006D31A
		public int CountdownToWedding
		{
			get
			{
				if (this.WeddingDate == null || this.WeddingDate.TotalDays < Game1.Date.TotalDays)
				{
					return 0;
				}
				return this.WeddingDate.TotalDays - Game1.Date.TotalDays;
			}
		}

		// Token: 0x1700014A RID: 330
		// (get) Token: 0x06000A55 RID: 2645 RVA: 0x0006F159 File Offset: 0x0006D359
		public int DaysUntilBirthing
		{
			get
			{
				if (this.NextBirthingDate == null)
				{
					return -1;
				}
				return Math.Max(0, this.NextBirthingDate.TotalDays - Game1.Date.TotalDays);
			}
		}

		// Token: 0x06000A56 RID: 2646 RVA: 0x0006F188 File Offset: 0x0006D388
		public Friendship()
		{
			this.NetFields.SetOwner(this).AddField(this.points, "points").AddField(this.giftsThisWeek, "giftsThisWeek").AddField(this.giftsToday, "giftsToday").AddField(this.lastGiftDate, "lastGiftDate").AddField(this.talkedToToday, "talkedToToday").AddField(this.proposalRejected, "proposalRejected").AddField(this.weddingDate, "weddingDate").AddField(this.nextBirthingDate, "nextBirthingDate").AddField(this.status, "status").AddField(this.proposer, "proposer").AddField(this.roommateMarriage, "roommateMarriage");
		}

		// Token: 0x06000A57 RID: 2647 RVA: 0x0006F2E3 File Offset: 0x0006D4E3
		public Friendship(int startingPoints) : this()
		{
			this.Points = startingPoints;
		}

		// Token: 0x06000A58 RID: 2648 RVA: 0x0006F2F4 File Offset: 0x0006D4F4
		public void Clear()
		{
			this.points.Value = 0;
			this.giftsThisWeek.Value = 0;
			this.giftsToday.Value = 0;
			this.lastGiftDate.Value = null;
			this.talkedToToday.Value = false;
			this.proposalRejected.Value = false;
			this.roommateMarriage.Value = false;
			this.weddingDate.Value = null;
			this.nextBirthingDate.Value = null;
			this.status.Value = FriendshipStatus.Friendly;
			this.proposer.Value = 0L;
		}

		// Token: 0x06000A59 RID: 2649 RVA: 0x0006F386 File Offset: 0x0006D586
		public bool IsDating()
		{
			return this.Status == FriendshipStatus.Dating || this.Status == FriendshipStatus.Engaged || this.Status == FriendshipStatus.Married;
		}

		// Token: 0x06000A5A RID: 2650 RVA: 0x0006F3A5 File Offset: 0x0006D5A5
		public bool IsEngaged()
		{
			return this.Status == FriendshipStatus.Engaged;
		}

		// Token: 0x06000A5B RID: 2651 RVA: 0x0006F3B0 File Offset: 0x0006D5B0
		public bool IsMarried()
		{
			return this.Status == FriendshipStatus.Married;
		}

		// Token: 0x06000A5C RID: 2652 RVA: 0x0006F3BB File Offset: 0x0006D5BB
		public bool IsDivorced()
		{
			return this.Status == FriendshipStatus.Divorced;
		}

		// Token: 0x06000A5D RID: 2653 RVA: 0x0006F3C6 File Offset: 0x0006D5C6
		public bool IsRoommate()
		{
			return this.IsMarried() && this.roommateMarriage.Value;
		}

		// Token: 0x0400068F RID: 1679
		private readonly NetInt points = new NetInt();

		// Token: 0x04000690 RID: 1680
		private readonly NetInt giftsThisWeek = new NetInt();

		// Token: 0x04000691 RID: 1681
		private readonly NetInt giftsToday = new NetInt();

		// Token: 0x04000692 RID: 1682
		private readonly NetRef<WorldDate> lastGiftDate = new NetRef<WorldDate>();

		// Token: 0x04000693 RID: 1683
		private readonly NetBool talkedToToday = new NetBool();

		// Token: 0x04000694 RID: 1684
		private readonly NetBool proposalRejected = new NetBool();

		// Token: 0x04000695 RID: 1685
		private readonly NetRef<WorldDate> weddingDate = new NetRef<WorldDate>();

		// Token: 0x04000696 RID: 1686
		private readonly NetRef<WorldDate> nextBirthingDate = new NetRef<WorldDate>();

		// Token: 0x04000697 RID: 1687
		private readonly NetEnum<FriendshipStatus> status = new NetEnum<FriendshipStatus>(FriendshipStatus.Friendly);

		// Token: 0x04000698 RID: 1688
		private readonly NetLong proposer = new NetLong();

		// Token: 0x04000699 RID: 1689
		private readonly NetBool roommateMarriage = new NetBool(false);
	}
}

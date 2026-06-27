using System;
using Netcode;

namespace StardewValley.Network
{
	// Token: 0x020001DD RID: 477
	public class NetDancePartner : INetObject<NetFields>
	{
		// Token: 0x17000374 RID: 884
		// (get) Token: 0x06002127 RID: 8487 RVA: 0x001728C3 File Offset: 0x00170AC3
		// (set) Token: 0x06002128 RID: 8488 RVA: 0x001728CB File Offset: 0x00170ACB
		public Character Value
		{
			get
			{
				return this.GetCharacter();
			}
			set
			{
				this.SetCharacter(value);
			}
		}

		// Token: 0x17000375 RID: 885
		// (get) Token: 0x06002129 RID: 8489 RVA: 0x001728D4 File Offset: 0x00170AD4
		public NetFields NetFields { get; } = new NetFields("NetDancePartner");

		// Token: 0x0600212A RID: 8490 RVA: 0x001728DC File Offset: 0x00170ADC
		public NetDancePartner()
		{
			this.NetFields.SetOwner(this).AddField(this.farmer.NetFields, "farmer.NetFields").AddField(this.villager, "villager");
		}

		// Token: 0x0600212B RID: 8491 RVA: 0x00172947 File Offset: 0x00170B47
		public NetDancePartner(Farmer farmer)
		{
			this.farmer.Value = farmer;
		}

		// Token: 0x0600212C RID: 8492 RVA: 0x00172981 File Offset: 0x00170B81
		public NetDancePartner(string villagerName)
		{
			this.villager.Value = villagerName;
		}

		// Token: 0x0600212D RID: 8493 RVA: 0x001729BC File Offset: 0x00170BBC
		public Character GetCharacter()
		{
			if (this.farmer.Value != null)
			{
				return this.farmer.Value;
			}
			if (Game1.CurrentEvent != null && this.villager.Value != null)
			{
				return Game1.CurrentEvent.getActorByName(this.villager.Value, false);
			}
			return null;
		}

		// Token: 0x0600212E RID: 8494 RVA: 0x00172A10 File Offset: 0x00170C10
		public void SetCharacter(Character value)
		{
			if (value == null)
			{
				this.farmer.Value = null;
				this.villager.Value = null;
				return;
			}
			Farmer curFarmer = value as Farmer;
			if (curFarmer != null)
			{
				this.farmer.Value = curFarmer;
				this.villager.Value = null;
				return;
			}
			NPC npc = value as NPC;
			if (npc == null)
			{
				throw new ArgumentException(value.ToString());
			}
			if (npc.IsVillager)
			{
				this.farmer.Value = null;
				this.villager.Value = npc.Name;
				return;
			}
			throw new ArgumentException(value.ToString());
		}

		// Token: 0x0600212F RID: 8495 RVA: 0x00172AA5 File Offset: 0x00170CA5
		public NPC TryGetVillager()
		{
			if (this.farmer.Value != null)
			{
				return null;
			}
			if (Game1.CurrentEvent != null && this.villager.Value != null)
			{
				return Game1.CurrentEvent.getActorByName(this.villager.Value, false);
			}
			return null;
		}

		// Token: 0x06002130 RID: 8496 RVA: 0x00172AE2 File Offset: 0x00170CE2
		public Farmer TryGetFarmer()
		{
			return this.farmer.Value;
		}

		// Token: 0x06002131 RID: 8497 RVA: 0x00172AEF File Offset: 0x00170CEF
		public bool IsFarmer()
		{
			return this.TryGetFarmer() != null;
		}

		// Token: 0x06002132 RID: 8498 RVA: 0x00172AFA File Offset: 0x00170CFA
		public bool IsVillager()
		{
			return this.TryGetVillager() != null;
		}

		// Token: 0x06002133 RID: 8499 RVA: 0x00172B05 File Offset: 0x00170D05
		public Gender GetGender()
		{
			if (this.IsFarmer())
			{
				return this.TryGetFarmer().Gender;
			}
			if (this.IsVillager())
			{
				return this.TryGetVillager().Gender;
			}
			return Gender.Undefined;
		}

		// Token: 0x040013EB RID: 5099
		private readonly NetFarmerRef farmer = new NetFarmerRef();

		// Token: 0x040013EC RID: 5100
		private readonly NetString villager = new NetString();
	}
}

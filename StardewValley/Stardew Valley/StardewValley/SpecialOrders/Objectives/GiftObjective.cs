using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.SpecialOrders.Objectives
{
	// Token: 0x02000159 RID: 345
	public class GiftObjective : OrderObjective
	{
		// Token: 0x06001AD1 RID: 6865 RVA: 0x0013B4F8 File Offset: 0x001396F8
		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			string rawValue;
			if (data.TryGetValue("AcceptedContextTags", out rawValue))
			{
				this.acceptableContextTagSets.Add(order.Parse(rawValue));
			}
			if (data.TryGetValue("MinimumLikeLevel", out rawValue))
			{
				this.minimumLikeLevel.Value = (GiftObjective.LikeLevels)Enum.Parse(typeof(GiftObjective.LikeLevels), rawValue);
			}
		}

		// Token: 0x06001AD2 RID: 6866 RVA: 0x0013B555 File Offset: 0x00139755
		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddField(this.acceptableContextTagSets, "acceptableContextTagSets").AddField(this.minimumLikeLevel, "minimumLikeLevel");
		}

		// Token: 0x06001AD3 RID: 6867 RVA: 0x0013B584 File Offset: 0x00139784
		protected override void _Register()
		{
			base._Register();
			SpecialOrder order = this._order;
			order.onGiftGiven = (Action<Farmer, NPC, Item>)Delegate.Combine(order.onGiftGiven, new Action<Farmer, NPC, Item>(this.OnGiftGiven));
		}

		// Token: 0x06001AD4 RID: 6868 RVA: 0x0013B5B4 File Offset: 0x001397B4
		protected override void _Unregister()
		{
			base._Unregister();
			SpecialOrder order = this._order;
			order.onGiftGiven = (Action<Farmer, NPC, Item>)Delegate.Remove(order.onGiftGiven, new Action<Farmer, NPC, Item>(this.OnGiftGiven));
		}

		// Token: 0x06001AD5 RID: 6869 RVA: 0x0013B5E4 File Offset: 0x001397E4
		public virtual void OnGiftGiven(Farmer farmer, NPC npc, Item item)
		{
			bool is_valid_gift = true;
			foreach (string text in this.acceptableContextTagSets)
			{
				is_valid_gift = false;
				bool fail = false;
				string[] array = text.Split(',', StringSplitOptions.None);
				for (int i = 0; i < array.Length; i++)
				{
					if (!ItemContextTagManager.DoAnyTagsMatch(array[i].Split('/', StringSplitOptions.None), item.GetContextTags()))
					{
						fail = true;
						break;
					}
				}
				if (!fail)
				{
					is_valid_gift = true;
					break;
				}
			}
			if (!is_valid_gift)
			{
				return;
			}
			if (this.minimumLikeLevel.Value > GiftObjective.LikeLevels.None)
			{
				int like_level = npc.getGiftTasteForThisItem(item);
				GiftObjective.LikeLevels gift_like_level = GiftObjective.LikeLevels.None;
				switch (like_level)
				{
				case 0:
					gift_like_level = GiftObjective.LikeLevels.Loved;
					break;
				case 2:
					gift_like_level = GiftObjective.LikeLevels.Liked;
					break;
				case 4:
					gift_like_level = GiftObjective.LikeLevels.Disliked;
					break;
				case 6:
					gift_like_level = GiftObjective.LikeLevels.Hated;
					break;
				case 8:
					gift_like_level = GiftObjective.LikeLevels.Neutral;
					break;
				}
				if (gift_like_level < this.minimumLikeLevel.Value)
				{
					return;
				}
			}
			this.IncrementCount(1);
		}

		// Token: 0x0400107D RID: 4221
		[XmlElement("acceptableContextTagSets")]
		public NetStringList acceptableContextTagSets = new NetStringList();

		// Token: 0x0400107E RID: 4222
		[XmlElement("minimumLikeLevel")]
		public NetEnum<GiftObjective.LikeLevels> minimumLikeLevel = new NetEnum<GiftObjective.LikeLevels>(GiftObjective.LikeLevels.None);

		// Token: 0x0200052B RID: 1323
		public enum LikeLevels
		{
			// Token: 0x04002AB8 RID: 10936
			None,
			// Token: 0x04002AB9 RID: 10937
			Hated,
			// Token: 0x04002ABA RID: 10938
			Disliked,
			// Token: 0x04002ABB RID: 10939
			Neutral,
			// Token: 0x04002ABC RID: 10940
			Liked,
			// Token: 0x04002ABD RID: 10941
			Loved
		}
	}
}

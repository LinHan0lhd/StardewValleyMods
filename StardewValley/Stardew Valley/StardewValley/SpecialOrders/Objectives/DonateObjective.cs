using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Objects;

namespace StardewValley.SpecialOrders.Objectives
{
	// Token: 0x02000157 RID: 343
	public class DonateObjective : OrderObjective
	{
		// Token: 0x06001AC0 RID: 6848 RVA: 0x0013B001 File Offset: 0x00139201
		public virtual string GetDropboxLocationName()
		{
			if (this.dropBoxGameLocation.Value == "Trailer" && Game1.MasterPlayer.hasOrWillReceiveMail("pamHouseUpgrade"))
			{
				return "Trailer_Big";
			}
			return this.dropBoxGameLocation.Value;
		}

		// Token: 0x06001AC1 RID: 6849 RVA: 0x0013B03C File Offset: 0x0013923C
		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			string rawValue;
			if (data.TryGetValue("AcceptedContextTags", out rawValue))
			{
				this.acceptableContextTagSets.Add(order.Parse(rawValue.Trim()));
			}
			if (data.TryGetValue("DropBox", out rawValue))
			{
				this.dropBox.Value = order.Parse(rawValue.Trim());
			}
			if (data.TryGetValue("DropBoxGameLocation", out rawValue))
			{
				this.dropBoxGameLocation.Value = order.Parse(rawValue.Trim());
			}
			if (data.TryGetValue("DropBoxIndicatorLocation", out rawValue))
			{
				string[] coordinates = ArgUtility.SplitBySpace(order.Parse(rawValue));
				this.dropBoxTileLocation.Value = new Vector2((float)Convert.ToDouble(coordinates[0]), (float)Convert.ToDouble(coordinates[1]));
			}
			if (data.TryGetValue("MinimumCapacity", out rawValue))
			{
				this.minimumCapacity.Value = int.Parse(order.Parse(rawValue));
			}
		}

		// Token: 0x06001AC2 RID: 6850 RVA: 0x0013B11F File Offset: 0x0013931F
		public int GetAcceptCount(Item item, int stack_count)
		{
			if (this.IsValidItem(item))
			{
				return Math.Min(base.GetMaxCount() - base.GetCount(), stack_count);
			}
			return 0;
		}

		// Token: 0x06001AC3 RID: 6851 RVA: 0x0013B140 File Offset: 0x00139340
		public override void OnCompletion()
		{
			base.OnCompletion();
			if (!string.IsNullOrEmpty(this.dropBoxGameLocation.Value))
			{
				GameLocation i = Game1.getLocationFromName(this.GetDropboxLocationName());
				if (i != null)
				{
					i.showDropboxIndicator = false;
				}
			}
		}

		// Token: 0x06001AC4 RID: 6852 RVA: 0x0013B17B File Offset: 0x0013937B
		public override bool CanComplete()
		{
			return this.confirmed.Value;
		}

		// Token: 0x06001AC5 RID: 6853 RVA: 0x0013B188 File Offset: 0x00139388
		public virtual void Confirm()
		{
			if (base.GetCount() >= base.GetMaxCount())
			{
				this.confirmed.Value = true;
				return;
			}
			this.confirmed.Value = false;
		}

		// Token: 0x06001AC6 RID: 6854 RVA: 0x0013B1B1 File Offset: 0x001393B1
		public override bool CanUncomplete()
		{
			return true;
		}

		// Token: 0x06001AC7 RID: 6855 RVA: 0x0013B1B4 File Offset: 0x001393B4
		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddField(this.acceptableContextTagSets, "acceptableContextTagSets").AddField(this.dropBox, "dropBox").AddField(this.dropBoxGameLocation, "dropBoxGameLocation").AddField(this.dropBoxTileLocation, "dropBoxTileLocation").AddField(this.minimumCapacity, "minimumCapacity").AddField(this.confirmed, "confirmed");
			this.confirmed.fieldChangeVisibleEvent += this.OnConfirmed;
		}

		// Token: 0x06001AC8 RID: 6856 RVA: 0x0013B245 File Offset: 0x00139445
		protected void OnConfirmed(NetBool field, bool oldValue, bool newValue)
		{
			if (Utility.ShouldIgnoreValueChangeCallback())
			{
				return;
			}
			this.CheckCompletion(true);
		}

		// Token: 0x06001AC9 RID: 6857 RVA: 0x0013B258 File Offset: 0x00139458
		public virtual bool IsValidItem(Item item)
		{
			if (item == null)
			{
				return false;
			}
			foreach (string text in this.acceptableContextTagSets)
			{
				bool fail = false;
				foreach (string acceptable_tags in text.Split(',', StringSplitOptions.None))
				{
					if (acceptable_tags.StartsWith("color"))
					{
						ColoredObject colorObject = item as ColoredObject;
						if (colorObject != null && colorObject.preservedParentSheetIndex.Value != null)
						{
							if (ItemContextTagManager.DoAnyTagsMatch(acceptable_tags.Split('/', StringSplitOptions.None), ItemContextTagManager.GetBaseContextTags(colorObject.preservedParentSheetIndex.Value)))
							{
								return true;
							}
							fail = true;
							break;
						}
					}
					if (!ItemContextTagManager.DoAnyTagsMatch(acceptable_tags.Split('/', StringSplitOptions.None), item.GetContextTags()))
					{
						fail = true;
						break;
					}
				}
				if (!fail)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x04001076 RID: 4214
		[XmlElement("dropBox")]
		public NetString dropBox = new NetString();

		// Token: 0x04001077 RID: 4215
		[XmlElement("dropBoxGameLocation")]
		public NetString dropBoxGameLocation = new NetString();

		// Token: 0x04001078 RID: 4216
		[XmlElement("dropBoxTileLocation")]
		public NetVector2 dropBoxTileLocation = new NetVector2();

		// Token: 0x04001079 RID: 4217
		[XmlElement("acceptableContextTagSets")]
		public NetStringList acceptableContextTagSets = new NetStringList();

		// Token: 0x0400107A RID: 4218
		[XmlElement("minimumCapacity")]
		public NetInt minimumCapacity = new NetInt(-1);

		// Token: 0x0400107B RID: 4219
		[XmlElement("confirmed")]
		public NetBool confirmed = new NetBool(false);
	}
}

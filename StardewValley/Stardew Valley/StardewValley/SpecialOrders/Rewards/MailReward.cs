using System;
using System.Collections.Generic;
using Netcode;

namespace StardewValley.SpecialOrders.Rewards
{
	// Token: 0x02000150 RID: 336
	public class MailReward : OrderReward
	{
		// Token: 0x06001A9D RID: 6813 RVA: 0x0013A67F File Offset: 0x0013887F
		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddField(this.noLetter, "noLetter").AddField(this.grantedMails, "grantedMails").AddField(this.host, "host");
		}

		// Token: 0x06001A9E RID: 6814 RVA: 0x0013A6C0 File Offset: 0x001388C0
		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			string raw = order.Parse(data["MailReceived"]);
			this.grantedMails.AddRange(ArgUtility.SplitBySpace(raw));
			string rawValue;
			if (data.TryGetValue("NoLetter", out rawValue))
			{
				this.noLetter.Value = Convert.ToBoolean(order.Parse(rawValue));
			}
			if (data.TryGetValue("Host", out rawValue))
			{
				this.host.Value = Convert.ToBoolean(order.Parse(rawValue));
			}
		}

		// Token: 0x06001A9F RID: 6815 RVA: 0x0013A73C File Offset: 0x0013893C
		public override void Grant()
		{
			foreach (string mail in this.grantedMails)
			{
				if (this.host.Value)
				{
					if (Game1.IsMasterGame)
					{
						if (Game1.newDaySync.hasInstance())
						{
							Game1.addMail(mail, this.noLetter.Value, true);
						}
						else
						{
							string actualMail = mail;
							if (actualMail == "ClintReward" && Game1.player.mailReceived.Contains("ClintReward"))
							{
								Game1.player.mailReceived.Remove("ClintReward2");
								actualMail = "ClintReward2";
							}
							Game1.addMailForTomorrow(actualMail, this.noLetter.Value, true);
						}
					}
				}
				else if (Game1.newDaySync.hasInstance())
				{
					Game1.addMail(mail, this.noLetter.Value, true);
				}
				else
				{
					string actualMail2 = mail;
					if (actualMail2 == "ClintReward" && Game1.player.mailReceived.Contains("ClintReward"))
					{
						Game1.player.mailReceived.Remove("ClintReward2");
						actualMail2 = "ClintReward2";
					}
					Game1.addMailForTomorrow(actualMail2, this.noLetter.Value, true);
				}
			}
		}

		// Token: 0x04001068 RID: 4200
		public NetBool noLetter = new NetBool(true);

		// Token: 0x04001069 RID: 4201
		public NetStringList grantedMails = new NetStringList();

		// Token: 0x0400106A RID: 4202
		public NetBool host = new NetBool(false);
	}
}

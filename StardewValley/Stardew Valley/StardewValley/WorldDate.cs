using System;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley
{
	// Token: 0x02000110 RID: 272
	public class WorldDate : INetObject<NetFields>
	{
		// Token: 0x17000290 RID: 656
		// (get) Token: 0x06001751 RID: 5969 RVA: 0x0010FDBB File Offset: 0x0010DFBB
		// (set) Token: 0x06001752 RID: 5970 RVA: 0x0010FDC8 File Offset: 0x0010DFC8
		public int Year
		{
			get
			{
				return this.year.Value;
			}
			set
			{
				this.year.Value = value;
			}
		}

		// Token: 0x17000291 RID: 657
		// (get) Token: 0x06001753 RID: 5971 RVA: 0x0010FDD6 File Offset: 0x0010DFD6
		[XmlIgnore]
		public int SeasonIndex
		{
			get
			{
				return (int)this.season.Value;
			}
		}

		// Token: 0x17000292 RID: 658
		// (get) Token: 0x06001754 RID: 5972 RVA: 0x0010FDE3 File Offset: 0x0010DFE3
		// (set) Token: 0x06001755 RID: 5973 RVA: 0x0010FDF0 File Offset: 0x0010DFF0
		public int DayOfMonth
		{
			get
			{
				return this.dayOfMonth.Value;
			}
			set
			{
				this.dayOfMonth.Value = value;
			}
		}

		// Token: 0x17000293 RID: 659
		// (get) Token: 0x06001756 RID: 5974 RVA: 0x0010FDFE File Offset: 0x0010DFFE
		public DayOfWeek DayOfWeek
		{
			get
			{
				return WorldDate.GetDayOfWeekFor(this.DayOfMonth);
			}
		}

		// Token: 0x17000294 RID: 660
		// (get) Token: 0x06001757 RID: 5975 RVA: 0x0010FE0B File Offset: 0x0010E00B
		// (set) Token: 0x06001758 RID: 5976 RVA: 0x0010FE18 File Offset: 0x0010E018
		[XmlIgnore]
		public Season Season
		{
			get
			{
				return this.season.Value;
			}
			set
			{
				this.season.Value = value;
			}
		}

		// Token: 0x17000295 RID: 661
		// (get) Token: 0x06001759 RID: 5977 RVA: 0x0010FE26 File Offset: 0x0010E026
		// (set) Token: 0x0600175A RID: 5978 RVA: 0x0010FE38 File Offset: 0x0010E038
		[XmlElement("Season")]
		public string SeasonKey
		{
			get
			{
				return Utility.getSeasonKey(this.season.Value);
			}
			set
			{
				Season parsedSeason;
				if (!Utility.TryParseEnum<Season>(value, out parsedSeason))
				{
					throw new ArgumentException("Can't parse '" + value + "' as a season key.", "value");
				}
				this.season.Value = parsedSeason;
			}
		}

		// Token: 0x17000296 RID: 662
		// (get) Token: 0x0600175B RID: 5979 RVA: 0x0010FE76 File Offset: 0x0010E076
		// (set) Token: 0x0600175C RID: 5980 RVA: 0x0010FE90 File Offset: 0x0010E090
		[XmlIgnore]
		public int TotalDays
		{
			get
			{
				return WorldDate.GetDaysPlayed(this.Year, this.Season, this.DayOfMonth);
			}
			set
			{
				int totalMonths = value / 28;
				this.DayOfMonth = value % 28 + 1;
				this.Season = (Season)(totalMonths % 4);
				this.Year = totalMonths / 4 + 1;
			}
		}

		// Token: 0x17000297 RID: 663
		// (get) Token: 0x0600175D RID: 5981 RVA: 0x0010FEC2 File Offset: 0x0010E0C2
		public int TotalWeeks
		{
			get
			{
				return this.TotalDays / 7;
			}
		}

		// Token: 0x17000298 RID: 664
		// (get) Token: 0x0600175E RID: 5982 RVA: 0x0010FECC File Offset: 0x0010E0CC
		public int TotalSundayWeeks
		{
			get
			{
				return (this.TotalDays + 1) / 7;
			}
		}

		// Token: 0x17000299 RID: 665
		// (get) Token: 0x0600175F RID: 5983 RVA: 0x0010FED8 File Offset: 0x0010E0D8
		public NetFields NetFields { get; } = new NetFields("WorldDate");

		// Token: 0x06001760 RID: 5984 RVA: 0x0010FEE0 File Offset: 0x0010E0E0
		public WorldDate()
		{
			this.NetFields.SetOwner(this).AddField(this.year, "year").AddField(this.season, "season").AddField(this.dayOfMonth, "dayOfMonth");
		}

		// Token: 0x06001761 RID: 5985 RVA: 0x0010FF64 File Offset: 0x0010E164
		public WorldDate(WorldDate other) : this()
		{
			this.Year = other.Year;
			this.Season = other.Season;
			this.DayOfMonth = other.DayOfMonth;
		}

		// Token: 0x06001762 RID: 5986 RVA: 0x0010FF90 File Offset: 0x0010E190
		public WorldDate(int year, Season season, int dayOfMonth) : this()
		{
			this.Year = year;
			this.Season = season;
			this.DayOfMonth = dayOfMonth;
		}

		// Token: 0x06001763 RID: 5987 RVA: 0x0010FFAD File Offset: 0x0010E1AD
		public WorldDate(int year, string seasonKey, int dayOfMonth) : this()
		{
			this.Year = year;
			this.SeasonKey = seasonKey;
			this.DayOfMonth = dayOfMonth;
		}

		// Token: 0x06001764 RID: 5988 RVA: 0x0010FFCA File Offset: 0x0010E1CA
		public string Localize()
		{
			return Utility.getDateStringFor(this.DayOfMonth, this.SeasonIndex, this.Year);
		}

		// Token: 0x06001765 RID: 5989 RVA: 0x0010FFE4 File Offset: 0x0010E1E4
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 4);
			defaultInterpolatedStringHandler.AppendLiteral("Year ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.Year);
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendFormatted(this.SeasonKey);
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(this.DayOfMonth);
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendFormatted<DayOfWeek>(this.DayOfWeek);
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06001766 RID: 5990 RVA: 0x00110068 File Offset: 0x0010E268
		public override bool Equals(object obj)
		{
			WorldDate other = obj as WorldDate;
			return other != null && this.TotalDays == other.TotalDays;
		}

		// Token: 0x06001767 RID: 5991 RVA: 0x0011008F File Offset: 0x0010E28F
		public override int GetHashCode()
		{
			return this.TotalDays;
		}

		// Token: 0x06001768 RID: 5992 RVA: 0x00110098 File Offset: 0x0010E298
		public static bool operator ==(WorldDate a, WorldDate b)
		{
			int? num = (a != null) ? new int?(a.TotalDays) : null;
			int? num2 = (b != null) ? new int?(b.TotalDays) : null;
			return num.GetValueOrDefault() == num2.GetValueOrDefault() & num != null == (num2 != null);
		}

		// Token: 0x06001769 RID: 5993 RVA: 0x001100FC File Offset: 0x0010E2FC
		public static bool operator !=(WorldDate a, WorldDate b)
		{
			int? num = (a != null) ? new int?(a.TotalDays) : null;
			int? num2 = (b != null) ? new int?(b.TotalDays) : null;
			return !(num.GetValueOrDefault() == num2.GetValueOrDefault() & num != null == (num2 != null));
		}

		// Token: 0x0600176A RID: 5994 RVA: 0x00110164 File Offset: 0x0010E364
		public static bool operator <(WorldDate a, WorldDate b)
		{
			int? num = (a != null) ? new int?(a.TotalDays) : null;
			int? num2 = (b != null) ? new int?(b.TotalDays) : null;
			return num.GetValueOrDefault() < num2.GetValueOrDefault() & (num != null & num2 != null);
		}

		// Token: 0x0600176B RID: 5995 RVA: 0x001101C8 File Offset: 0x0010E3C8
		public static bool operator >(WorldDate a, WorldDate b)
		{
			int? num = (a != null) ? new int?(a.TotalDays) : null;
			int? num2 = (b != null) ? new int?(b.TotalDays) : null;
			return num.GetValueOrDefault() > num2.GetValueOrDefault() & (num != null & num2 != null);
		}

		// Token: 0x0600176C RID: 5996 RVA: 0x0011022C File Offset: 0x0010E42C
		public static bool operator <=(WorldDate a, WorldDate b)
		{
			int? num = (a != null) ? new int?(a.TotalDays) : null;
			int? num2 = (b != null) ? new int?(b.TotalDays) : null;
			return num.GetValueOrDefault() <= num2.GetValueOrDefault() & (num != null & num2 != null);
		}

		// Token: 0x0600176D RID: 5997 RVA: 0x00110290 File Offset: 0x0010E490
		public static bool operator >=(WorldDate a, WorldDate b)
		{
			int? num = (a != null) ? new int?(a.TotalDays) : null;
			int? num2 = (b != null) ? new int?(b.TotalDays) : null;
			return num.GetValueOrDefault() >= num2.GetValueOrDefault() & (num != null & num2 != null);
		}

		// Token: 0x0600176E RID: 5998 RVA: 0x001102F4 File Offset: 0x0010E4F4
		public static DayOfWeek GetDayOfWeekFor(int dayOfMonth)
		{
			return (DayOfWeek)(dayOfMonth % 7);
		}

		// Token: 0x0600176F RID: 5999 RVA: 0x001102F9 File Offset: 0x0010E4F9
		public static WorldDate Now()
		{
			return new WorldDate(Game1.year, Game1.season, Game1.dayOfMonth);
		}

		// Token: 0x06001770 RID: 6000 RVA: 0x0011030F File Offset: 0x0010E50F
		public static WorldDate ForDaysPlayed(int daysPlayed)
		{
			return new WorldDate
			{
				TotalDays = daysPlayed
			};
		}

		// Token: 0x06001771 RID: 6001 RVA: 0x0011031D File Offset: 0x0010E51D
		public static int GetDaysPlayed(int year, Season season, int dayOfMonth)
		{
			return (int)(((year - 1) * 4 + season) * (Season)28 + (dayOfMonth - 1));
		}

		// Token: 0x06001772 RID: 6002 RVA: 0x00110330 File Offset: 0x0010E530
		public static bool TryGetDayOfWeekFor(string day, out DayOfWeek dayOfWeek)
		{
			int numeric;
			if (int.TryParse(day, out numeric))
			{
				dayOfWeek = WorldDate.GetDayOfWeekFor(numeric);
				return true;
			}
			string text = (day != null) ? day.ToLower() : null;
			if (text != null)
			{
				switch (text.Length)
				{
				case 3:
				{
					char c = text[1];
					if (c <= 'h')
					{
						if (c != 'a')
						{
							if (c != 'e')
							{
								if (c != 'h')
								{
									goto IL_1F4;
								}
								if (!(text == "thu"))
								{
									goto IL_1F4;
								}
								goto IL_1E0;
							}
							else
							{
								if (!(text == "wed"))
								{
									goto IL_1F4;
								}
								goto IL_1DB;
							}
						}
						else
						{
							if (!(text == "sat"))
							{
								goto IL_1F4;
							}
							goto IL_1EA;
						}
					}
					else if (c != 'o')
					{
						if (c != 'r')
						{
							if (c != 'u')
							{
								goto IL_1F4;
							}
							if (text == "tue")
							{
								goto IL_1D6;
							}
							if (!(text == "sun"))
							{
								goto IL_1F4;
							}
							goto IL_1EF;
						}
						else
						{
							if (!(text == "fri"))
							{
								goto IL_1F4;
							}
							goto IL_1E5;
						}
					}
					else if (!(text == "mon"))
					{
						goto IL_1F4;
					}
					break;
				}
				case 4:
				case 5:
					goto IL_1F4;
				case 6:
				{
					char c = text[0];
					if (c != 'f')
					{
						if (c != 'm')
						{
							if (c != 's')
							{
								goto IL_1F4;
							}
							if (!(text == "sunday"))
							{
								goto IL_1F4;
							}
							goto IL_1EF;
						}
						else if (!(text == "monday"))
						{
							goto IL_1F4;
						}
					}
					else
					{
						if (!(text == "friday"))
						{
							goto IL_1F4;
						}
						goto IL_1E5;
					}
					break;
				}
				case 7:
					if (!(text == "tuesday"))
					{
						goto IL_1F4;
					}
					goto IL_1D6;
				case 8:
				{
					char c = text[0];
					if (c != 's')
					{
						if (c != 't')
						{
							goto IL_1F4;
						}
						if (!(text == "thursday"))
						{
							goto IL_1F4;
						}
						goto IL_1E0;
					}
					else
					{
						if (!(text == "saturday"))
						{
							goto IL_1F4;
						}
						goto IL_1EA;
					}
					break;
				}
				case 9:
					if (!(text == "wednesday"))
					{
						goto IL_1F4;
					}
					goto IL_1DB;
				default:
					goto IL_1F4;
				}
				dayOfWeek = DayOfWeek.Monday;
				return true;
				IL_1D6:
				dayOfWeek = DayOfWeek.Tuesday;
				return true;
				IL_1DB:
				dayOfWeek = DayOfWeek.Wednesday;
				return true;
				IL_1E0:
				dayOfWeek = DayOfWeek.Thursday;
				return true;
				IL_1E5:
				dayOfWeek = DayOfWeek.Friday;
				return true;
				IL_1EA:
				dayOfWeek = DayOfWeek.Saturday;
				return true;
				IL_1EF:
				dayOfWeek = DayOfWeek.Sunday;
				return true;
			}
			IL_1F4:
			dayOfWeek = DayOfWeek.Sunday;
			return false;
		}

		// Token: 0x04000E1F RID: 3615
		public const int MonthsPerYear = 4;

		// Token: 0x04000E20 RID: 3616
		public const int DaysPerMonth = 28;

		// Token: 0x04000E21 RID: 3617
		public const int DaysPerYear = 112;

		// Token: 0x04000E22 RID: 3618
		private readonly NetInt year = new NetInt(1);

		// Token: 0x04000E23 RID: 3619
		private readonly NetEnum<Season> season = new NetEnum<Season>(Season.Spring);

		// Token: 0x04000E24 RID: 3620
		private readonly NetInt dayOfMonth = new NetInt(1);
	}
}

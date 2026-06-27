using System;
using Netcode;
using StardewValley.GameData;
using StardewValley.GameData.LocationContexts;

namespace StardewValley.Network
{
	// Token: 0x020001D7 RID: 471
	public class LocationWeather : INetObject<NetFields>
	{
		// Token: 0x1700035F RID: 863
		// (get) Token: 0x060020E8 RID: 8424 RVA: 0x00171FB1 File Offset: 0x001701B1
		public NetFields NetFields { get; } = new NetFields("LocationWeather");

		// Token: 0x17000360 RID: 864
		// (get) Token: 0x060020E9 RID: 8425 RVA: 0x00171FB9 File Offset: 0x001701B9
		// (set) Token: 0x060020EA RID: 8426 RVA: 0x00171FC6 File Offset: 0x001701C6
		public string WeatherForTomorrow
		{
			get
			{
				return this.weatherForTomorrow.Value;
			}
			set
			{
				this.weatherForTomorrow.Value = value;
			}
		}

		// Token: 0x17000361 RID: 865
		// (get) Token: 0x060020EB RID: 8427 RVA: 0x00171FD4 File Offset: 0x001701D4
		// (set) Token: 0x060020EC RID: 8428 RVA: 0x00171FE1 File Offset: 0x001701E1
		public string Weather
		{
			get
			{
				return this.weather.Value;
			}
			set
			{
				this.weather.Value = value;
			}
		}

		// Token: 0x17000362 RID: 866
		// (get) Token: 0x060020ED RID: 8429 RVA: 0x00171FEF File Offset: 0x001701EF
		// (set) Token: 0x060020EE RID: 8430 RVA: 0x00171FFC File Offset: 0x001701FC
		public bool IsRaining
		{
			get
			{
				return this.isRaining.Value;
			}
			set
			{
				this.isRaining.Value = value;
			}
		}

		// Token: 0x17000363 RID: 867
		// (get) Token: 0x060020EF RID: 8431 RVA: 0x0017200A File Offset: 0x0017020A
		// (set) Token: 0x060020F0 RID: 8432 RVA: 0x00172017 File Offset: 0x00170217
		public bool IsSnowing
		{
			get
			{
				return this.isSnowing.Value;
			}
			set
			{
				this.isSnowing.Value = value;
			}
		}

		// Token: 0x17000364 RID: 868
		// (get) Token: 0x060020F1 RID: 8433 RVA: 0x00172025 File Offset: 0x00170225
		// (set) Token: 0x060020F2 RID: 8434 RVA: 0x00172032 File Offset: 0x00170232
		public bool IsLightning
		{
			get
			{
				return this.isLightning.Value;
			}
			set
			{
				this.isLightning.Value = value;
			}
		}

		// Token: 0x17000365 RID: 869
		// (get) Token: 0x060020F3 RID: 8435 RVA: 0x00172040 File Offset: 0x00170240
		// (set) Token: 0x060020F4 RID: 8436 RVA: 0x0017204D File Offset: 0x0017024D
		public bool IsDebrisWeather
		{
			get
			{
				return this.isDebrisWeather.Value;
			}
			set
			{
				this.isDebrisWeather.Value = value;
			}
		}

		// Token: 0x17000366 RID: 870
		// (get) Token: 0x060020F5 RID: 8437 RVA: 0x0017205B File Offset: 0x0017025B
		// (set) Token: 0x060020F6 RID: 8438 RVA: 0x00172068 File Offset: 0x00170268
		public bool IsGreenRain
		{
			get
			{
				return this.isGreenRain.Value;
			}
			set
			{
				this.isGreenRain.Value = value;
				if (value)
				{
					this.IsRaining = true;
				}
			}
		}

		// Token: 0x060020F7 RID: 8439 RVA: 0x00172080 File Offset: 0x00170280
		public LocationWeather()
		{
			this.NetFields.SetOwner(this).AddField(this.weatherForTomorrow, "weatherForTomorrow").AddField(this.weather, "weather").AddField(this.isRaining, "isRaining").AddField(this.isSnowing, "isSnowing").AddField(this.isLightning, "isLightning").AddField(this.isDebrisWeather, "isDebrisWeather").AddField(this.isGreenRain, "isGreenRain").AddField(this.monthlyNonRainyDayCount, "monthlyNonRainyDayCount");
		}

		// Token: 0x060020F8 RID: 8440 RVA: 0x00172188 File Offset: 0x00170388
		public void InitializeDayWeather()
		{
			this.Weather = this.WeatherForTomorrow;
			this.IsRaining = false;
			this.IsSnowing = false;
			this.IsLightning = false;
			this.IsDebrisWeather = false;
			this.IsGreenRain = false;
		}

		// Token: 0x060020F9 RID: 8441 RVA: 0x001721BC File Offset: 0x001703BC
		public void UpdateDailyWeather(string locationContextId, LocationContextData data, Random random)
		{
			this.InitializeDayWeather();
			string a = this.WeatherForTomorrow;
			if (!(a == "Rain"))
			{
				if (!(a == "GreenRain"))
				{
					if (!(a == "Storm"))
					{
						if (!(a == "Wind"))
						{
							if (a == "Snow")
							{
								this.IsSnowing = true;
							}
						}
						else
						{
							this.IsDebrisWeather = true;
						}
					}
					else
					{
						this.IsRaining = true;
						this.IsLightning = true;
					}
				}
				else
				{
					this.IsGreenRain = true;
				}
			}
			else
			{
				this.IsRaining = true;
			}
			this.WeatherForTomorrow = "Sun";
			WorldDate tomorrow = new WorldDate(Game1.Date);
			WorldDate worldDate = tomorrow;
			int totalDays = worldDate.TotalDays;
			worldDate.TotalDays = totalDays + 1;
			if (Utility.isFestivalDay(tomorrow.DayOfMonth, tomorrow.Season, locationContextId))
			{
				this.WeatherForTomorrow = "Festival";
				return;
			}
			PassiveFestivalData passiveFestivalData;
			if (Utility.TryGetPassiveFestivalDataForDay(tomorrow.DayOfMonth, tomorrow.Season, locationContextId, out a, out passiveFestivalData, false))
			{
				this.WeatherForTomorrow = "Sun";
				return;
			}
			foreach (WeatherCondition weatherCondition in data.WeatherConditions)
			{
				if (GameStateQuery.CheckConditions(weatherCondition.Condition, null, null, null, null, random, null))
				{
					this.WeatherForTomorrow = weatherCondition.Weather;
					break;
				}
			}
		}

		// Token: 0x060020FA RID: 8442 RVA: 0x0017231C File Offset: 0x0017051C
		public void CopyFrom(LocationWeather other)
		{
			this.Weather = other.Weather;
			this.IsRaining = other.IsRaining;
			this.IsSnowing = other.IsSnowing;
			this.IsLightning = other.IsLightning;
			this.IsDebrisWeather = other.IsDebrisWeather;
			this.IsGreenRain = other.IsGreenRain;
			this.WeatherForTomorrow = other.WeatherForTomorrow;
			this.monthlyNonRainyDayCount.Value = other.monthlyNonRainyDayCount.Value;
			if (this.Weather == null)
			{
				if (this.IsLightning)
				{
					this.Weather = "Storm";
					return;
				}
				if (this.IsRaining)
				{
					this.Weather = "Rain";
					return;
				}
				if (this.IsSnowing)
				{
					this.Weather = "Snow";
					return;
				}
				if (this.IsDebrisWeather)
				{
					this.Weather = "Wind";
					return;
				}
				this.Weather = "Sun";
			}
		}

		// Token: 0x040013D8 RID: 5080
		public readonly NetString weatherForTomorrow = new NetString();

		// Token: 0x040013D9 RID: 5081
		public readonly NetString weather = new NetString();

		// Token: 0x040013DA RID: 5082
		public readonly NetBool isRaining = new NetBool();

		// Token: 0x040013DB RID: 5083
		public readonly NetBool isSnowing = new NetBool();

		// Token: 0x040013DC RID: 5084
		public readonly NetBool isLightning = new NetBool();

		// Token: 0x040013DD RID: 5085
		public readonly NetBool isDebrisWeather = new NetBool();

		// Token: 0x040013DE RID: 5086
		public readonly NetBool isGreenRain = new NetBool();

		// Token: 0x040013DF RID: 5087
		public readonly NetInt monthlyNonRainyDayCount = new NetInt();
	}
}

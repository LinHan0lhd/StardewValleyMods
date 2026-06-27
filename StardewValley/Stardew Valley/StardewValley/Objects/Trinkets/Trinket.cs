using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.GameData;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Logging;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.TokenizableStrings;

namespace StardewValley.Objects.Trinkets
{
	// Token: 0x020001BF RID: 447
	public class Trinket : Object
	{
		// Token: 0x1700033A RID: 826
		// (get) Token: 0x06001FCE RID: 8142 RVA: 0x0016CCD0 File Offset: 0x0016AED0
		public override string TypeDefinitionId { get; } = "(TR)";

		// Token: 0x06001FCF RID: 8143 RVA: 0x0016CCD8 File Offset: 0x0016AED8
		public Trinket()
		{
		}

		// Token: 0x06001FD0 RID: 8144 RVA: 0x0016CD18 File Offset: 0x0016AF18
		public Trinket(string itemId, int generationSeed) : this()
		{
			base.ItemId = itemId;
			base.name = itemId;
			this.generationSeed.Value = generationSeed;
			ParsedItemData data = ItemRegistry.GetDataOrErrorItem(itemId);
			base.ParentSheetIndex = data.SpriteIndex;
			TrinketData trinketData = this.GetTrinketData();
			Dictionary<string, string> fromModData = (trinketData != null) ? trinketData.ModData : null;
			if (fromModData != null && fromModData.Count > 0)
			{
				foreach (KeyValuePair<string, string> pair in fromModData)
				{
					base.modData.Add(pair.Key, pair.Value);
				}
			}
			TrinketEffect effect = this.GetEffect();
			if (effect == null)
			{
				return;
			}
			effect.GenerateRandomStats(this);
		}

		// Token: 0x06001FD1 RID: 8145 RVA: 0x0016CDDC File Offset: 0x0016AFDC
		public static bool CanSpawnTrinket(Farmer f)
		{
			return f.stats.Get("trinketSlots") > 0U;
		}

		// Token: 0x06001FD2 RID: 8146 RVA: 0x0016CDF4 File Offset: 0x0016AFF4
		public static void SpawnTrinket(GameLocation location, Vector2 spawnPoint)
		{
			Trinket t = Trinket.GetRandomTrinket();
			if (t != null)
			{
				Game1.createItemDebris(t, spawnPoint, Game1.random.Next(4), location, -1, false);
			}
		}

		// Token: 0x06001FD3 RID: 8147 RVA: 0x0016CE20 File Offset: 0x0016B020
		public bool RerollStats(int newSeed)
		{
			this.generationSeed.Value = newSeed;
			TrinketEffect effect = this.GetEffect();
			return effect != null && effect.GenerateRandomStats(this);
		}

		// Token: 0x06001FD4 RID: 8148 RVA: 0x0016CE4C File Offset: 0x0016B04C
		public override bool canBeShipped()
		{
			return false;
		}

		// Token: 0x06001FD5 RID: 8149 RVA: 0x0016CE4F File Offset: 0x0016B04F
		public override int sellToStorePrice(long specificPlayerID = -1L)
		{
			return 1000;
		}

		// Token: 0x06001FD6 RID: 8150 RVA: 0x0016CE58 File Offset: 0x0016B058
		public static void TrySpawnTrinket(GameLocation location, Monster monster, Vector2 spawnPosition, double chanceModifier = 1.0)
		{
			if (Trinket.CanSpawnTrinket(Game1.player))
			{
				double baseChance = 0.004;
				if (monster != null)
				{
					baseChance += (double)monster.MaxHealth * 1E-05;
					if (monster.isGlider.Value && monster.MaxHealth >= 150)
					{
						baseChance += 0.002;
					}
					if (monster is Leaper)
					{
						baseChance -= 0.005;
					}
				}
				baseChance = Math.Min(0.025, baseChance);
				baseChance += Game1.player.DailyLuck / 25.0;
				baseChance += (double)((float)Game1.player.LuckLevel * 0.00133f);
				baseChance *= chanceModifier;
				if (Game1.random.NextDouble() < baseChance)
				{
					Trinket.SpawnTrinket(location, spawnPosition);
				}
			}
		}

		// Token: 0x06001FD7 RID: 8151 RVA: 0x0016CF24 File Offset: 0x0016B124
		public static Trinket GetRandomTrinket()
		{
			Dictionary<string, TrinketData> data_sheet = DataLoader.Trinkets(Game1.content);
			Trinket t = null;
			while (t == null)
			{
				int which = Game1.random.Next(data_sheet.Count);
				int i = 0;
				foreach (KeyValuePair<string, TrinketData> pair in data_sheet)
				{
					if (which == i && pair.Value.DropsNaturally)
					{
						t = ItemRegistry.Create<Trinket>("(TR)" + pair.Key, 1, 0, false);
						break;
					}
					i++;
				}
			}
			return t;
		}

		// Token: 0x06001FD8 RID: 8152 RVA: 0x0016CFC8 File Offset: 0x0016B1C8
		public override bool canBeGivenAsGift()
		{
			return true;
		}

		// Token: 0x06001FD9 RID: 8153 RVA: 0x0016CFCB File Offset: 0x0016B1CB
		public override void reloadSprite()
		{
			base.reloadSprite();
			TrinketEffect effect = this.GetEffect();
			if (effect == null)
			{
				return;
			}
			effect.GenerateRandomStats(this);
		}

		// Token: 0x06001FDA RID: 8154 RVA: 0x0016CFE8 File Offset: 0x0016B1E8
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.trinketMetadata, "trinketMetadata").AddField(this.generationSeed, "generationSeed").AddField(this.displayNameOverrideTemplate, "displayNameOverrideTemplate").AddField(this.descriptionSubstitutionTemplates, "descriptionSubstitutionTemplates");
			this.displayNameOverrideTemplate.fieldChangeVisibleEvent += delegate(NetString field, string oldValue, string newValue)
			{
				this.displayNameOverride = TokenParser.ParseText(newValue, null, null, null);
			};
			this.descriptionSubstitutionTemplates.OnElementChanged += delegate(NetList<string, NetString> <p0>, int <p1>, string <p2>, string <p3>)
			{
				this._description = null;
			};
			this.descriptionSubstitutionTemplates.OnArrayReplaced += delegate(NetList<string, NetString> <p0>, IList<string> <p1>, IList<string> <p2>)
			{
				this._description = null;
			};
		}

		// Token: 0x06001FDB RID: 8155 RVA: 0x0016D087 File Offset: 0x0016B287
		public TrinketData GetTrinketData()
		{
			if (this._data == null)
			{
				this._data = DataLoader.Trinkets(Game1.content).GetValueOrDefault(base.ItemId);
			}
			return this._data;
		}

		// Token: 0x06001FDC RID: 8156 RVA: 0x0016D0B4 File Offset: 0x0016B2B4
		public virtual TrinketEffect GetEffect()
		{
			if (this._trinketEffect == null)
			{
				TrinketData data = this.GetTrinketData();
				if (data != null && this._trinketEffectClassName != data.TrinketEffectClass)
				{
					this._trinketEffectClassName = data.TrinketEffectClass;
					if (data.TrinketEffectClass != null)
					{
						Type trinketEffectType = System.Type.GetType(data.TrinketEffectClass);
						if (trinketEffectType != null)
						{
							this._trinketEffect = (TrinketEffect)Activator.CreateInstance(trinketEffectType, new object[]
							{
								this
							});
						}
						else
						{
							IGameLogger log = Game1.log;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(59, 2);
							defaultInterpolatedStringHandler.AppendLiteral("Failed loading effects for trinket ");
							defaultInterpolatedStringHandler.AppendFormatted(base.QualifiedItemId);
							defaultInterpolatedStringHandler.AppendLiteral(": invalid class type '");
							defaultInterpolatedStringHandler.AppendFormatted(data.TrinketEffectClass);
							defaultInterpolatedStringHandler.AppendLiteral("'.");
							log.Warn(defaultInterpolatedStringHandler.ToStringAndClear());
						}
					}
				}
			}
			return this._trinketEffect;
		}

		// Token: 0x06001FDD RID: 8157 RVA: 0x0016D198 File Offset: 0x0016B398
		protected override string loadDisplayName()
		{
			ParsedItemData data = ItemRegistry.GetDataOrErrorItem(base.ItemId);
			return this.displayNameOverride ?? data.DisplayName;
		}

		// Token: 0x06001FDE RID: 8158 RVA: 0x0016D1C1 File Offset: 0x0016B3C1
		public override int maximumStackSize()
		{
			return 1;
		}

		// Token: 0x06001FDF RID: 8159 RVA: 0x0016D1C4 File Offset: 0x0016B3C4
		public override string getDescription()
		{
			if (this._description == null)
			{
				string description = TokenParser.ParseText(ItemRegistry.GetDataOrErrorItem(base.ItemId).Description, null, null, null);
				if (this.descriptionSubstitutionTemplates.Count > 0)
				{
					object[] tokens = new object[this.descriptionSubstitutionTemplates.Count];
					for (int i = 0; i < this.descriptionSubstitutionTemplates.Count; i++)
					{
						tokens[i] = TokenParser.ParseText(this.descriptionSubstitutionTemplates[i], null, null, null);
					}
					description = string.Format(description, tokens);
				}
				this._description = Game1.parseText(description, Game1.smallFont, this.getDescriptionWidth());
			}
			return this._description;
		}

		// Token: 0x06001FE0 RID: 8160 RVA: 0x0016D266 File Offset: 0x0016B466
		public override string getCategoryName()
		{
			return Game1.content.LoadString("Strings\\1_6_Strings:Trinket");
		}

		// Token: 0x06001FE1 RID: 8161 RVA: 0x0016D277 File Offset: 0x0016B477
		public override Color getCategoryColor()
		{
			return new Color(96, 81, 255);
		}

		// Token: 0x06001FE2 RID: 8162 RVA: 0x0016D287 File Offset: 0x0016B487
		public override bool isPlaceable()
		{
			return false;
		}

		// Token: 0x06001FE3 RID: 8163 RVA: 0x0016D28A File Offset: 0x0016B48A
		public override bool performUseAction(GameLocation location)
		{
			this.GetEffect().OnUse(Game1.player);
			return false;
		}

		// Token: 0x06001FE4 RID: 8164 RVA: 0x0016D29D File Offset: 0x0016B49D
		public override bool performToolAction(Tool t)
		{
			return false;
		}

		// Token: 0x06001FE5 RID: 8165 RVA: 0x0016D2A0 File Offset: 0x0016B4A0
		protected override Item GetOneNew()
		{
			return new Trinket(base.ItemId, this.generationSeed.Value);
		}

		// Token: 0x06001FE6 RID: 8166 RVA: 0x0016D2B8 File Offset: 0x0016B4B8
		protected override void GetOneCopyFrom(Item source)
		{
			base.GetOneCopyFrom(source);
			Trinket other = source as Trinket;
			if (other != null)
			{
				this.displayNameOverrideTemplate.Value = other.displayNameOverrideTemplate.Value;
				this.descriptionSubstitutionTemplates.Set(other.descriptionSubstitutionTemplates);
				this.trinketMetadata.Set(other.trinketMetadata.Pairs);
				this.generationSeed.Value = other.generationSeed.Value;
			}
		}

		// Token: 0x06001FE7 RID: 8167 RVA: 0x0016D32E File Offset: 0x0016B52E
		public override bool IsHeldOverHead()
		{
			return false;
		}

		// Token: 0x06001FE8 RID: 8168 RVA: 0x0016D331 File Offset: 0x0016B531
		public virtual void Apply(Farmer farmer)
		{
			TrinketEffect effect = this.GetEffect();
			if (effect == null)
			{
				return;
			}
			effect.Apply(farmer);
		}

		// Token: 0x06001FE9 RID: 8169 RVA: 0x0016D344 File Offset: 0x0016B544
		public virtual void Unapply(Farmer farmer)
		{
			TrinketEffect effect = this.GetEffect();
			if (effect == null)
			{
				return;
			}
			effect.Unapply(farmer);
		}

		// Token: 0x06001FEA RID: 8170 RVA: 0x0016D357 File Offset: 0x0016B557
		public virtual void Update(Farmer farmer, GameTime time, GameLocation location)
		{
			TrinketEffect effect = this.GetEffect();
			if (effect == null)
			{
				return;
			}
			effect.Update(farmer, time, location);
		}

		// Token: 0x06001FEB RID: 8171 RVA: 0x0016D36C File Offset: 0x0016B56C
		public virtual void OnFootstep(Farmer farmer)
		{
			TrinketEffect effect = this.GetEffect();
			if (effect == null)
			{
				return;
			}
			effect.OnFootstep(farmer);
		}

		// Token: 0x06001FEC RID: 8172 RVA: 0x0016D37F File Offset: 0x0016B57F
		public virtual void OnReceiveDamage(Farmer farmer, int damageAmount)
		{
			TrinketEffect effect = this.GetEffect();
			if (effect == null)
			{
				return;
			}
			effect.OnReceiveDamage(farmer, damageAmount);
		}

		// Token: 0x06001FED RID: 8173 RVA: 0x0016D393 File Offset: 0x0016B593
		public virtual void OnDamageMonster(Farmer farmer, Monster monster, int damageAmount, bool isBomb, bool isCriticalHit)
		{
			TrinketEffect effect = this.GetEffect();
			if (effect == null)
			{
				return;
			}
			effect.OnDamageMonster(farmer, monster, damageAmount, isBomb, isCriticalHit);
		}

		// Token: 0x04001370 RID: 4976
		protected string _description;

		// Token: 0x04001371 RID: 4977
		protected TrinketData _data;

		// Token: 0x04001372 RID: 4978
		protected TrinketEffect _trinketEffect;

		// Token: 0x04001373 RID: 4979
		protected string _trinketEffectClassName;

		// Token: 0x04001374 RID: 4980
		protected string displayNameOverride;

		// Token: 0x04001375 RID: 4981
		public readonly NetString displayNameOverrideTemplate = new NetString();

		// Token: 0x04001376 RID: 4982
		public readonly NetStringList descriptionSubstitutionTemplates = new NetStringList();

		// Token: 0x04001377 RID: 4983
		public readonly NetStringDictionary<string, NetString> trinketMetadata = new NetStringDictionary<string, NetString>();

		// Token: 0x04001378 RID: 4984
		[XmlElement("generationSeed")]
		public readonly NetInt generationSeed = new NetInt();
	}
}

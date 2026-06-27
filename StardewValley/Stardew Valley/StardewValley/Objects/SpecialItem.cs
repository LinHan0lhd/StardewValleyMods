using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace StardewValley.Objects
{
	// Token: 0x020001B7 RID: 439
	public class SpecialItem : Item
	{
		// Token: 0x17000332 RID: 818
		// (get) Token: 0x06001F55 RID: 8021 RVA: 0x00167E07 File Offset: 0x00166007
		public override string TypeDefinitionId { get; } = "(O)";

		// Token: 0x17000333 RID: 819
		// (get) Token: 0x06001F56 RID: 8022 RVA: 0x00167E10 File Offset: 0x00166010
		// (set) Token: 0x06001F57 RID: 8023 RVA: 0x00167F40 File Offset: 0x00166140
		[XmlIgnore]
		private string displayName
		{
			get
			{
				if (string.IsNullOrEmpty(this._displayName))
				{
					int value = this.which.Value;
					switch (value)
					{
					case 2:
						this._displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:SpecialItem.cs.13089");
						break;
					case 3:
						this._displayName = Game1.content.LoadString("Strings\\Objects:SpecialCharm");
						break;
					case 4:
						this._displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:SpecialItem.cs.13088");
						break;
					case 5:
						this._displayName = Game1.content.LoadString("Strings\\Objects:MagnifyingGlass");
						break;
					case 6:
						this._displayName = Game1.content.LoadString("Strings\\Objects:DarkTalisman");
						break;
					case 7:
						this._displayName = Game1.content.LoadString("Strings\\Objects:MagicInk");
						break;
					default:
						if (value == 99)
						{
							if (Game1.player.maxItems.Value == 36)
							{
								this._displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8709");
							}
							else
							{
								this._displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8708");
							}
						}
						break;
					}
				}
				return this._displayName;
			}
			set
			{
				if (!string.IsNullOrEmpty(value) || string.IsNullOrEmpty(this._displayName))
				{
					this._displayName = value;
					return;
				}
				int value2 = this.which.Value;
				switch (value2)
				{
				case 2:
					this._displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:SpecialItem.cs.13089");
					return;
				case 3:
					this._displayName = Game1.content.LoadString("Strings\\Objects:SpecialCharm");
					return;
				case 4:
					this._displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:SpecialItem.cs.13088");
					return;
				case 5:
					this._displayName = Game1.content.LoadString("Strings\\Objects:MagnifyingGlass");
					return;
				case 6:
					this._displayName = Game1.content.LoadString("Strings\\Objects:DarkTalisman");
					return;
				case 7:
					this._displayName = Game1.content.LoadString("Strings\\Objects:MagicInk");
					return;
				default:
					if (value2 != 99)
					{
						return;
					}
					if (Game1.player.maxItems.Value == 36)
					{
						this._displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8709");
						return;
					}
					this._displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8708");
					return;
				}
			}
		}

		// Token: 0x06001F58 RID: 8024 RVA: 0x00168068 File Offset: 0x00166268
		public SpecialItem()
		{
			this.which.Value = this.which.Value;
			if (this.netName.Value == "Error Item" || this.Name.Length < 1)
			{
				switch (this.which.Value)
				{
				case 2:
					this.displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:SpecialItem.cs.13089");
					return;
				case 3:
					this.displayName = Game1.content.LoadString("Strings\\Objects:SpecialCharm");
					break;
				case 4:
					this.displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:SpecialItem.cs.13088");
					return;
				case 5:
					this.displayName = Game1.content.LoadString("Strings\\Objects:MagnifyingGlass");
					return;
				case 6:
					this.displayName = Game1.content.LoadString("Strings\\Objects:DarkTalisman");
					return;
				case 7:
					this.displayName = Game1.content.LoadString("Strings\\Objects:MagicInk");
					return;
				default:
					return;
				}
			}
		}

		// Token: 0x06001F59 RID: 8025 RVA: 0x00168180 File Offset: 0x00166380
		public SpecialItem(int which, string name = "") : this()
		{
			this.which.Value = which;
			this.Name = name;
			if (name.Length < 1)
			{
				switch (which)
				{
				case 2:
					this.Name = "Club Card";
					return;
				case 3:
					this.Name = Game1.content.LoadString("Strings\\Objects:SpecialCharm");
					break;
				case 4:
					this.Name = "Skull Key";
					return;
				case 5:
					this.Name = Game1.content.LoadString("Strings\\Objects:MagnifyingGlass");
					return;
				case 6:
					this.Name = Game1.content.LoadString("Strings\\Objects:DarkTalisman");
					return;
				case 7:
					this.Name = Game1.content.LoadString("Strings\\Objects:MagicInk");
					return;
				default:
					return;
				}
			}
		}

		// Token: 0x06001F5A RID: 8026 RVA: 0x00168242 File Offset: 0x00166442
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.which, "which");
		}

		// Token: 0x06001F5B RID: 8027 RVA: 0x00168264 File Offset: 0x00166464
		public void actionWhenReceived(Farmer who)
		{
			switch (this.which.Value)
			{
			case 3:
				who.hasSpecialCharm = true;
				return;
			case 4:
				who.hasSkullKey = true;
				who.addQuest("19");
				return;
			case 5:
				who.hasMagnifyingGlass = true;
				return;
			case 6:
				who.hasDarkTalisman = true;
				return;
			case 7:
				who.hasMagicInk = true;
				return;
			default:
				return;
			}
		}

		// Token: 0x06001F5C RID: 8028 RVA: 0x001682CC File Offset: 0x001664CC
		public TemporaryAnimatedSprite getTemporarySpriteForHoldingUp(Vector2 position)
		{
			if (this.which.Value == 99)
			{
				return new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle((Game1.player.maxItems.Value == 36) ? 268 : 257, 1436, (Game1.player.maxItems.Value == 36) ? 11 : 9, 13), position + new Vector2(16f, 0f), false, 0f, Color.White)
				{
					scale = 4f,
					layerDepth = 1f
				};
			}
			return new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(129 + 16 * this.which.Value, 320, 16, 16), position, false, 0f, Color.White)
			{
				layerDepth = 1f
			};
		}

		// Token: 0x06001F5D RID: 8029 RVA: 0x001683B4 File Offset: 0x001665B4
		public override string checkForSpecialItemHoldUpMeessage()
		{
			switch (this.which.Value)
			{
			case 2:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:SpecialItem.cs.13090", this.displayName);
			case 3:
				return Game1.content.LoadString("Strings\\Objects:SpecialCharmDescription", this.displayName);
			case 4:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:SpecialItem.cs.13092", this.displayName);
			case 5:
				return Game1.content.LoadString("Strings\\Objects:MagnifyingGlassDescription", this.displayName);
			case 6:
				return Game1.content.LoadString("Strings\\Objects:DarkTalismanDescription", this.displayName);
			case 7:
				return Game1.content.LoadString("Strings\\Objects:MagicInkDescription", this.displayName);
			default:
				if (this.which.Value == 99)
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:SpecialItem.cs.13094", this.displayName, Game1.player.maxItems);
				}
				return base.checkForSpecialItemHoldUpMeessage();
			}
		}

		// Token: 0x06001F5E RID: 8030 RVA: 0x001684AB File Offset: 0x001666AB
		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
		}

		// Token: 0x06001F5F RID: 8031 RVA: 0x001684AD File Offset: 0x001666AD
		public override int maximumStackSize()
		{
			return 1;
		}

		// Token: 0x06001F60 RID: 8032 RVA: 0x001684B0 File Offset: 0x001666B0
		public override string getDescription()
		{
			return null;
		}

		// Token: 0x06001F61 RID: 8033 RVA: 0x001684B3 File Offset: 0x001666B3
		public override bool isPlaceable()
		{
			return false;
		}

		// Token: 0x17000334 RID: 820
		// (get) Token: 0x06001F62 RID: 8034 RVA: 0x001684B6 File Offset: 0x001666B6
		[XmlIgnore]
		public override string DisplayName
		{
			get
			{
				return this.displayName;
			}
		}

		// Token: 0x17000335 RID: 821
		// (get) Token: 0x06001F63 RID: 8035 RVA: 0x001684C0 File Offset: 0x001666C0
		// (set) Token: 0x06001F64 RID: 8036 RVA: 0x0016857C File Offset: 0x0016677C
		[XmlIgnore]
		public override string Name
		{
			get
			{
				if (this.netName.Value.Length < 1 || this.netName.Value == "Error Item")
				{
					switch (this.which.Value)
					{
					case 2:
						return "Club Card";
					case 3:
						return Game1.content.LoadString("Strings\\Objects:SpecialCharm");
					case 4:
						return "Skull Key";
					case 5:
						return Game1.content.LoadString("Strings\\Objects:MagnifyingGlass");
					case 6:
						return Game1.content.LoadString("Strings\\Objects:DarkTalisman");
					case 7:
						return Game1.content.LoadString("Strings\\Objects:MagicInk");
					}
				}
				return this.netName.Value;
			}
			set
			{
				this.netName.Value = value;
			}
		}

		// Token: 0x06001F65 RID: 8037 RVA: 0x0016858A File Offset: 0x0016678A
		protected override Item GetOneNew()
		{
			throw new NotImplementedException();
		}

		// Token: 0x0400134E RID: 4942
		public const int skullKey = 4;

		// Token: 0x0400134F RID: 4943
		public const int clubCard = 2;

		// Token: 0x04001350 RID: 4944
		public const int specialCharm = 3;

		// Token: 0x04001351 RID: 4945
		public const int backpack = 99;

		// Token: 0x04001352 RID: 4946
		public const int magnifyingGlass = 5;

		// Token: 0x04001353 RID: 4947
		public const int darkTalisman = 6;

		// Token: 0x04001354 RID: 4948
		public const int magicInk = 7;

		// Token: 0x04001355 RID: 4949
		[XmlElement("which")]
		public readonly NetInt which = new NetInt();

		// Token: 0x04001357 RID: 4951
		[XmlIgnore]
		private string _displayName;
	}
}

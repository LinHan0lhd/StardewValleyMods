using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Enchantments;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Objects
{
	// Token: 0x020001AD RID: 429
	public class Hat : Item
	{
		// Token: 0x17000325 RID: 805
		// (get) Token: 0x06001EC0 RID: 7872 RVA: 0x00162B78 File Offset: 0x00160D78
		public override string TypeDefinitionId { get; } = "(H)";

		// Token: 0x17000326 RID: 806
		// (get) Token: 0x06001EC1 RID: 7873 RVA: 0x00162B80 File Offset: 0x00160D80
		[XmlIgnore]
		public bool isMask
		{
			get
			{
				if (this._isMask == -1)
				{
					if (this.Name.Contains("Mask"))
					{
						this._isMask = 1;
					}
					else
					{
						this._isMask = 0;
					}
					if (this.hairDrawType.Value == 2)
					{
						this._isMask = 0;
					}
				}
				return this._isMask == 1;
			}
		}

		// Token: 0x06001EC2 RID: 7874 RVA: 0x00162BD8 File Offset: 0x00160DD8
		protected override void MigrateLegacyItemId()
		{
			base.ItemId = (((this.obsolete_which != null) ? this.obsolete_which.GetValueOrDefault().ToString() : null) ?? "0");
			this.obsolete_which = null;
		}

		// Token: 0x06001EC3 RID: 7875 RVA: 0x00162C20 File Offset: 0x00160E20
		public Hat()
		{
		}

		// Token: 0x06001EC4 RID: 7876 RVA: 0x00162C80 File Offset: 0x00160E80
		public Hat(string itemId)
		{
			itemId = base.ValidateUnqualifiedItemId(itemId);
			base.ItemId = itemId;
			this.load(base.ItemId);
		}

		// Token: 0x06001EC5 RID: 7877 RVA: 0x00162CFC File Offset: 0x00160EFC
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.ignoreHairstyleOffset, "ignoreHairstyleOffset").AddField(this.hairDrawType, "hairDrawType").AddField(this.isPrismatic, "isPrismatic");
			this.itemId.fieldChangeVisibleEvent += delegate(NetString <p0>, string <p1>, string <p2>)
			{
				this.load(this.itemId.Value);
			};
		}

		// Token: 0x06001EC6 RID: 7878 RVA: 0x00162D60 File Offset: 0x00160F60
		public void load(string id)
		{
			Dictionary<string, string> hatInfo = DataLoader.Hats(Game1.content);
			string rawData;
			if (!hatInfo.TryGetValue(id, out rawData))
			{
				id = "0";
				rawData = hatInfo[id];
			}
			string[] split = rawData.Split('/', StringSplitOptions.None);
			this.Name = (ArgUtility.Get(split, 0, null, false) ?? ItemRegistry.GetDataOrErrorItem("(H)" + id).InternalName);
			string showFullHair = split[2];
			if (showFullHair == "hide")
			{
				this.hairDrawType.Set(2);
			}
			else if (Convert.ToBoolean(showFullHair))
			{
				this.hairDrawType.Set(0);
			}
			else
			{
				this.hairDrawType.Set(1);
			}
			if (this.skipHairDraw)
			{
				this.skipHairDraw = false;
				this.hairDrawType.Set(0);
			}
			string[] array = ArgUtility.SplitBySpace(ArgUtility.Get(split, 4, null, true));
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == "Prismatic")
				{
					this.isPrismatic.Value = true;
				}
			}
			this.ignoreHairstyleOffset.Value = Convert.ToBoolean(split[3]);
			base.Category = -95;
		}

		// Token: 0x06001EC7 RID: 7879 RVA: 0x00162E7C File Offset: 0x0016107C
		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			base.AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
			scaleSize *= 0.75f;
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			int spriteIndex = itemData.SpriteIndex;
			Texture2D texture = itemData.GetTexture();
			Rectangle drawnSourceRect = new Rectangle(spriteIndex * 20 % texture.Width, spriteIndex * 20 / texture.Width * 20 * 4, 20, 20);
			if (itemData.IsErrorItem)
			{
				drawnSourceRect = itemData.GetSourceRect(0, null);
			}
			spriteBatch.Draw(texture, location + new Vector2(32f, 32f), new Rectangle?(drawnSourceRect), this.isPrismatic.Value ? (Utility.GetPrismaticColor(0, 1f) * transparency) : (color * transparency), 0f, new Vector2(10f, 10f), 4f * scaleSize, SpriteEffects.None, layerDepth);
			this.DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
		}

		// Token: 0x06001EC8 RID: 7880 RVA: 0x00162F74 File Offset: 0x00161174
		public void draw(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, int direction, bool useAnimalTexture = false)
		{
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			int spriteIndex = itemData.SpriteIndex;
			Texture2D texture;
			if (useAnimalTexture)
			{
				string textureName = itemData.GetTextureName();
				if (Game1.content.DoesAssetExist<Texture2D>(textureName + "_animals"))
				{
					textureName += "_animals";
				}
				texture = Game1.content.Load<Texture2D>(textureName);
			}
			else
			{
				texture = itemData.GetTexture();
			}
			switch (direction)
			{
			case 0:
				direction = 3;
				break;
			case 2:
				direction = 0;
				break;
			case 3:
				direction = 2;
				break;
			}
			Rectangle drawnSourceRect = (!itemData.IsErrorItem) ? new Rectangle(spriteIndex * 20 % texture.Width, spriteIndex * 20 / texture.Width * 20 * 4 + direction * 20, 20, 20) : itemData.GetSourceRect(0, null);
			spriteBatch.Draw(texture, location + new Vector2(10f, 10f), new Rectangle?(drawnSourceRect), this.isPrismatic.Value ? (Utility.GetPrismaticColor(0, 1f) * transparency) : (Color.White * transparency), 0f, new Vector2(3f, 3f), 3f * scaleSize, SpriteEffects.None, layerDepth);
		}

		// Token: 0x06001EC9 RID: 7881 RVA: 0x001630B5 File Offset: 0x001612B5
		public override string getDescription()
		{
			if (this.description == null)
			{
				this.loadDisplayFields();
			}
			return Game1.parseText(this.description, Game1.smallFont, this.getDescriptionWidth());
		}

		// Token: 0x06001ECA RID: 7882 RVA: 0x001630DC File Offset: 0x001612DC
		public override int maximumStackSize()
		{
			return 1;
		}

		// Token: 0x06001ECB RID: 7883 RVA: 0x001630DF File Offset: 0x001612DF
		public override bool isPlaceable()
		{
			return false;
		}

		// Token: 0x17000327 RID: 807
		// (get) Token: 0x06001ECC RID: 7884 RVA: 0x001630E2 File Offset: 0x001612E2
		[XmlIgnore]
		public override string DisplayName
		{
			get
			{
				if (this.displayName == null)
				{
					this.loadDisplayFields();
				}
				return this.displayName;
			}
		}

		// Token: 0x06001ECD RID: 7885 RVA: 0x001630F9 File Offset: 0x001612F9
		protected override Item GetOneNew()
		{
			return new Hat(base.ItemId);
		}

		// Token: 0x06001ECE RID: 7886 RVA: 0x00163108 File Offset: 0x00161308
		private bool loadDisplayFields()
		{
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			if (this.Name != null && this.Name != "Error Item" && itemData.IsErrorItem)
			{
				foreach (KeyValuePair<string, string> kvp in DataLoader.Hats(Game1.content))
				{
					if (kvp.Value.Split('/', StringSplitOptions.None)[0] == this.Name)
					{
						itemData = ItemRegistry.GetDataOrErrorItem(this.TypeDefinitionId + kvp.Key);
						break;
					}
				}
			}
			this.displayName = itemData.DisplayName;
			this.description = itemData.Description;
			return true;
		}

		// Token: 0x040012E8 RID: 4840
		public const int widthOfTileSheetSquare = 20;

		// Token: 0x040012E9 RID: 4841
		public const int heightOfTileSheetSquare = 20;

		// Token: 0x040012EA RID: 4842
		public const int data_index_internalName = 0;

		// Token: 0x040012EB RID: 4843
		public const int data_index_description = 1;

		// Token: 0x040012EC RID: 4844
		public const int data_index_showFullHair = 2;

		// Token: 0x040012ED RID: 4845
		public const int data_index_ignoreHairOffset = 3;

		// Token: 0x040012EE RID: 4846
		public const int data_index_tags = 4;

		// Token: 0x040012EF RID: 4847
		public const int data_index_displayName = 5;

		// Token: 0x040012F0 RID: 4848
		public const int data_index_texture = 7;

		// Token: 0x040012F1 RID: 4849
		[XmlElement("which")]
		public int? obsolete_which;

		// Token: 0x040012F2 RID: 4850
		[XmlElement("skipHairDraw")]
		public bool skipHairDraw;

		// Token: 0x040012F3 RID: 4851
		[XmlElement("ignoreHairstyleOffset")]
		public readonly NetBool ignoreHairstyleOffset = new NetBool();

		// Token: 0x040012F4 RID: 4852
		[XmlElement("hairDrawType")]
		public readonly NetInt hairDrawType = new NetInt();

		// Token: 0x040012F5 RID: 4853
		[XmlElement("isPrismatic")]
		public readonly NetBool isPrismatic = new NetBool(false);

		// Token: 0x040012F6 RID: 4854
		[XmlIgnore]
		protected int _isMask = -1;

		// Token: 0x040012F7 RID: 4855
		[XmlElement("enchantments")]
		public List<BaseEnchantment> enchantments = new List<BaseEnchantment>();

		// Token: 0x040012F8 RID: 4856
		[XmlElement("previousEnchantments")]
		public List<string> previousEnchantments = new List<string>();

		// Token: 0x040012FA RID: 4858
		[XmlIgnore]
		public string displayName;

		// Token: 0x040012FB RID: 4859
		[XmlIgnore]
		public string description;

		// Token: 0x02000557 RID: 1367
		public enum HairDrawType
		{
			// Token: 0x04002B4F RID: 11087
			DrawFullHair,
			// Token: 0x04002B50 RID: 11088
			DrawObscuredHair,
			// Token: 0x04002B51 RID: 11089
			HideHair
		}
	}
}

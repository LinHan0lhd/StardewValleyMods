using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.GameData.Pets;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewValley.Objects
{
	// Token: 0x020001B2 RID: 434
	public class PetLicense : Object
	{
		// Token: 0x06001F12 RID: 7954 RVA: 0x00165AE1 File Offset: 0x00163CE1
		public PetLicense() : base("PetLicense", 1, false, -1, 0)
		{
		}

		// Token: 0x06001F13 RID: 7955 RVA: 0x00165AF4 File Offset: 0x00163CF4
		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			base.AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
			if (drawShadow && !this.bigCraftable.Value && base.QualifiedItemId != "(O)590" && base.QualifiedItemId != "(O)SeedSpot")
			{
				spriteBatch.Draw(Game1.shadowTexture, location + new Vector2(32f, 48f), new Rectangle?(Game1.shadowTexture.Bounds), color * 0.5f, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, layerDepth - 0.0001f);
			}
			ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			float drawnScale = scaleSize;
			if (this.bigCraftable.Value && drawnScale > 0.2f)
			{
				drawnScale /= 2f;
			}
			string[] split = this.Name.Split('|', StringSplitOptions.None);
			PetData petData;
			if (Game1.petData.TryGetValue(split[0], out petData))
			{
				PetBreed breed = petData.GetBreedById(split[1], false);
				if (breed != null)
				{
					Rectangle sourceRect = breed.IconSourceRect;
					spriteBatch.Draw(Game1.content.Load<Texture2D>(breed.IconTexture), location + new Vector2(32f, 32f), new Rectangle?(sourceRect), color * transparency, 0f, new Vector2((float)(sourceRect.Width / 2), (float)(sourceRect.Height / 2)), 4f * drawnScale, SpriteEffects.None, layerDepth);
				}
			}
			this.DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
		}

		// Token: 0x06001F14 RID: 7956 RVA: 0x00165CA8 File Offset: 0x00163EA8
		public override bool actionWhenPurchased(string shopId)
		{
			Game1.exitActiveMenu();
			string title = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1236");
			Game1.activeClickableMenu = new NamingMenu(new NamingMenu.doneNamingBehavior(this.namePet), title, Dialogue.randomName());
			Game1.playSound("purchaseClick", null);
			return true;
		}

		// Token: 0x06001F15 RID: 7957 RVA: 0x00165CFC File Offset: 0x00163EFC
		private void namePet(string name)
		{
			string[] split = this.Name.Split('|', StringSplitOptions.None);
			FarmHouse home = Utility.getHomeOfFarmer(Game1.player);
			Point petTile = new Point(3, 7);
			if (home.upgradeLevel == 1)
			{
				petTile = new Point(9, 7);
			}
			else if (home.upgradeLevel >= 2)
			{
				petTile = new Point(27, 26);
			}
			Pet p = new Pet(petTile.X, petTile.Y, split[1], split[0]);
			p.currentLocation = home;
			home.characters.Add(p);
			p.warpToFarmHouse(Game1.player);
			p.Name = name;
			p.displayName = p.name.Value;
			foreach (Building building in Game1.getFarm().buildings)
			{
				PetBowl bowl = building as PetBowl;
				if (bowl != null && !bowl.HasPet())
				{
					bowl.AssignPet(p);
					break;
				}
			}
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				farmer.autoGenerateActiveDialogueEvent("gotPet", 4);
			}
			Game1.exitActiveMenu();
			if (Game1.currentLocation.getCharacterFromName("Marnie") != null)
			{
				Game1.DrawDialogue(Game1.currentLocation.getCharacterFromName("Marnie"), "Strings\\1_6_Strings:AdoptedPet_Marnie", new object[]
				{
					name
				});
				return;
			}
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:AdoptedPet", name));
		}

		// Token: 0x04001315 RID: 4885
		public const char Delimiter = '|';
	}
}

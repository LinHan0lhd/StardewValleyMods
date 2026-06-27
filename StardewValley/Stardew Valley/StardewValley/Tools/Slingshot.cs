using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.Audio;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Projectiles;

namespace StardewValley.Tools
{
	// Token: 0x02000133 RID: 307
	public class Slingshot : Tool
	{
		// Token: 0x170002BA RID: 698
		// (get) Token: 0x060018A6 RID: 6310 RVA: 0x00122A50 File Offset: 0x00120C50
		public override string TypeDefinitionId { get; } = "(W)";

		// Token: 0x060018A7 RID: 6311 RVA: 0x00122A58 File Offset: 0x00120C58
		public Slingshot() : this("32")
		{
		}

		// Token: 0x060018A8 RID: 6312 RVA: 0x00122A68 File Offset: 0x00120C68
		protected override void MigrateLegacyItemId()
		{
			base.ItemId = base.InitialParentTileIndex.ToString();
		}

		// Token: 0x060018A9 RID: 6313 RVA: 0x00122A89 File Offset: 0x00120C89
		protected override Item GetOneNew()
		{
			return new Slingshot(base.ItemId);
		}

		// Token: 0x060018AA RID: 6314 RVA: 0x00122A96 File Offset: 0x00120C96
		protected override string loadDisplayName()
		{
			return ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).DisplayName;
		}

		// Token: 0x060018AB RID: 6315 RVA: 0x00122AA8 File Offset: 0x00120CA8
		protected override string loadDescription()
		{
			return ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).Description;
		}

		// Token: 0x060018AC RID: 6316 RVA: 0x00122ABA File Offset: 0x00120CBA
		public override bool doesShowTileLocationMarker()
		{
			return false;
		}

		// Token: 0x060018AD RID: 6317 RVA: 0x00122AC0 File Offset: 0x00120CC0
		public Slingshot(string itemId = "32")
		{
			itemId = base.ValidateUnqualifiedItemId(itemId);
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem("(W)" + itemId);
			base.ItemId = itemId;
			this.Name = itemData.InternalName;
			base.InitialParentTileIndex = itemData.SpriteIndex;
			base.CurrentParentTileIndex = itemData.SpriteIndex;
			base.IndexOfMenuItemView = itemData.SpriteIndex;
			this.numAttachmentSlots.Value = 1;
			this.attachments.SetCount(1);
		}

		// Token: 0x060018AE RID: 6318 RVA: 0x00122B80 File Offset: 0x00120D80
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.finishEvent, "finishEvent").AddField(this.aimPos, "aimPos");
			this.finishEvent.onEvent += this.doFinish;
		}

		// Token: 0x060018AF RID: 6319 RVA: 0x00122BD4 File Offset: 0x00120DD4
		public int GetBackArmDistance(Farmer who)
		{
			if (this.CanAutoFire() && this.nextAutoFire > 0f)
			{
				return (int)Utility.Lerp(20f, 0f, this.nextAutoFire / this.GetAutoFireRate());
			}
			if (!Game1.options.useLegacySlingshotFiring)
			{
				return (int)(20f * this.GetSlingshotChargeTime());
			}
			return Math.Min(20, (int)Vector2.Distance(who.getStandingPosition(), new Vector2((float)this.aimPos.X, (float)this.aimPos.Y)) / 20);
		}

		// Token: 0x060018B0 RID: 6320 RVA: 0x00122C62 File Offset: 0x00120E62
		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			base.IndexOfMenuItemView = base.InitialParentTileIndex;
			if (!this.CanAutoFire())
			{
				this.PerformFire(location, who);
			}
			this.finish();
		}

		// Token: 0x060018B1 RID: 6321 RVA: 0x00122C88 File Offset: 0x00120E88
		public virtual void PerformFire(GameLocation location, Farmer who)
		{
			Object ammoSlot = this.attachments[0];
			if (ammoSlot != null)
			{
				this.updateAimPos();
				int mouseX = this.aimPos.X;
				int mouseY = this.aimPos.Y;
				int backArmDistance = this.GetBackArmDistance(who);
				Vector2 shoot_origin = this.GetShootOrigin(who);
				Vector2 v = Utility.getVelocityTowardPoint(this.GetShootOrigin(who), this.AdjustForHeight(new Vector2((float)mouseX, (float)mouseY), true), (float)(15 + Game1.random.Next(4, 6)) * (1f + who.buffs.WeaponSpeedMultiplier));
				if (backArmDistance > 4 && !this.canPlaySound)
				{
					Object ammunition = (Object)ammoSlot.getOne();
					if (ammoSlot.ConsumeStack(1) == null)
					{
						this.attachments[0] = null;
					}
					string itemId = base.ItemId;
					float damageMod;
					if (!(itemId == "33"))
					{
						if (!(itemId == "34"))
						{
							damageMod = 1f;
						}
						else
						{
							damageMod = 4f;
						}
					}
					else
					{
						damageMod = 2f;
					}
					int damage = this.GetAmmoDamage(ammunition);
					string collisionSound = this.GetAmmoCollisionSound(ammunition);
					BasicProjectile.onCollisionBehavior collisionBehavior = this.GetAmmoCollisionBehavior(ammunition);
					if (!Game1.options.useLegacySlingshotFiring)
					{
						v.X *= -1f;
						v.Y *= -1f;
					}
					location.projectiles.Add(new BasicProjectile((int)(damageMod * (float)(damage + Game1.random.Next(-(damage / 2), damage + 2)) * (1f + who.buffs.AttackMultiplier)), -1, 0, 0, (float)(3.141592653589793 / (double)(64f + (float)Game1.random.Next(-63, 64))), -v.X, -v.Y, shoot_origin - new Vector2(32f, 32f), collisionSound, null, null, false, true, location, who, collisionBehavior, ammunition.ItemId)
					{
						IgnoreLocationCollision = (Game1.currentLocation.currentEvent != null || Game1.currentMinigame != null)
					});
				}
			}
			else
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Slingshot.cs.14254"), true);
			}
			this.canPlaySound = true;
		}

		// Token: 0x060018B2 RID: 6322 RVA: 0x00122EA4 File Offset: 0x001210A4
		public virtual int GetAmmoDamage(Object ammunition)
		{
			string text = (ammunition != null) ? ammunition.QualifiedItemId : null;
			if (text != null)
			{
				int length = text.Length;
				if (length == 6)
				{
					switch (text[5])
					{
					case '0':
						if (text == "(O)390")
						{
							return 5;
						}
						if (text == "(O)380")
						{
							return 20;
						}
						break;
					case '1':
						if (text == "(O)441")
						{
							return 20;
						}
						break;
					case '2':
						if (text == "(O)382")
						{
							return 15;
						}
						break;
					case '4':
						if (text == "(O)384")
						{
							return 30;
						}
						break;
					case '6':
						if (text == "(O)386")
						{
							return 50;
						}
						break;
					case '8':
						if (text == "(O)388")
						{
							return 2;
						}
						if (text == "(O)378")
						{
							return 10;
						}
						break;
					}
				}
			}
			return 1;
		}

		// Token: 0x060018B3 RID: 6323 RVA: 0x00122F97 File Offset: 0x00121197
		public virtual string GetAmmoCollisionSound(Object ammunition)
		{
			if (((ammunition != null) ? ammunition.QualifiedItemId : null) == "(O)441")
			{
				return "explosion";
			}
			if (ammunition != null && ammunition.Category == -5)
			{
				return "slimedead";
			}
			return "hammer";
		}

		// Token: 0x060018B4 RID: 6324 RVA: 0x00122FCF File Offset: 0x001211CF
		public virtual BasicProjectile.onCollisionBehavior GetAmmoCollisionBehavior(Object ammunition)
		{
			if (ammunition.QualifiedItemId == "(O)441")
			{
				return new BasicProjectile.onCollisionBehavior(BasicProjectile.explodeOnImpact);
			}
			return null;
		}

		// Token: 0x060018B5 RID: 6325 RVA: 0x00122FF1 File Offset: 0x001211F1
		public Vector2 GetShootOrigin(Farmer who)
		{
			return this.AdjustForHeight(who.getStandingPosition(), false);
		}

		// Token: 0x060018B6 RID: 6326 RVA: 0x00123000 File Offset: 0x00121200
		public Vector2 AdjustForHeight(Vector2 position, bool for_cursor = true)
		{
			if (!Game1.options.useLegacySlingshotFiring && for_cursor)
			{
				return new Vector2(position.X, position.Y);
			}
			return new Vector2(position.X, position.Y - 32f - 8f);
		}

		// Token: 0x060018B7 RID: 6327 RVA: 0x0012304D File Offset: 0x0012124D
		public void finish()
		{
			this.finishEvent.Fire();
		}

		// Token: 0x060018B8 RID: 6328 RVA: 0x0012305C File Offset: 0x0012125C
		private void doFinish()
		{
			if (this.lastUser == null)
			{
				return;
			}
			this.lastUser.usingSlingshot = false;
			this.lastUser.canReleaseTool = true;
			this.lastUser.UsingTool = false;
			this.lastUser.canMove = true;
			this.lastUser.Halt();
			if (this.lastUser == Game1.player && Game1.options.gamepadControls)
			{
				Game1.game1.controllerSlingshotSafeTime = 0.2f;
			}
		}

		// Token: 0x060018B9 RID: 6329 RVA: 0x001230D8 File Offset: 0x001212D8
		protected override bool canThisBeAttached(Object o, int slot)
		{
			string qualifiedItemId = o.QualifiedItemId;
			if (qualifiedItemId != null)
			{
				int length = qualifiedItemId.Length;
				if (length == 6)
				{
					switch (qualifiedItemId[5])
					{
					case '0':
						if (!(qualifiedItemId == "(O)380") && !(qualifiedItemId == "(O)390"))
						{
							goto IL_C6;
						}
						break;
					case '1':
						if (!(qualifiedItemId == "(O)441"))
						{
							goto IL_C6;
						}
						break;
					case '2':
						if (!(qualifiedItemId == "(O)382"))
						{
							goto IL_C6;
						}
						break;
					case '3':
					case '5':
					case '7':
						goto IL_C6;
					case '4':
						if (!(qualifiedItemId == "(O)384"))
						{
							goto IL_C6;
						}
						break;
					case '6':
						if (!(qualifiedItemId == "(O)386"))
						{
							goto IL_C6;
						}
						break;
					case '8':
						if (!(qualifiedItemId == "(O)378") && !(qualifiedItemId == "(O)388"))
						{
							goto IL_C6;
						}
						break;
					default:
						goto IL_C6;
					}
					return true;
				}
			}
			IL_C6:
			return !o.bigCraftable.Value && (o.Category == -5 || o.Category == -79 || o.Category == -75);
		}

		// Token: 0x060018BA RID: 6330 RVA: 0x001231DC File Offset: 0x001213DC
		public override string getHoverBoxText(Item hoveredItem)
		{
			Object obj = hoveredItem as Object;
			if (obj != null && this.canThisBeAttached(obj))
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Slingshot.cs.14256", this.DisplayName, obj.DisplayName);
			}
			if (hoveredItem == null)
			{
				NetObjectArray<Object> attachments = this.attachments;
				if (((attachments != null) ? attachments[0] : null) != null)
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Slingshot.cs.14258", this.attachments[0].DisplayName);
				}
			}
			return null;
		}

		// Token: 0x060018BB RID: 6331 RVA: 0x00123252 File Offset: 0x00121452
		public override bool onRelease(GameLocation location, int x, int y, Farmer who)
		{
			this.DoFunction(location, x, y, 1, who);
			return true;
		}

		// Token: 0x060018BC RID: 6332 RVA: 0x00123264 File Offset: 0x00121464
		public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
		{
			who.usingSlingshot = true;
			who.canReleaseTool = false;
			this.mouseDragAmount = 0;
			int offset = (who.FacingDirection == 3 || who.FacingDirection == 1) ? 1 : ((who.FacingDirection == 0) ? 2 : 0);
			who.FarmerSprite.setCurrentFrame(42 + offset);
			if (!who.IsLocalPlayer)
			{
				return true;
			}
			Game1.oldMouseState = Game1.input.GetMouseState();
			Game1.lastMousePositionBeforeFade = Game1.getMousePosition();
			this.lastClickX = Game1.getOldMouseX() + Game1.viewport.X;
			this.lastClickY = Game1.getOldMouseY() + Game1.viewport.Y;
			this.pullStartTime = Game1.currentGameTime.TotalGameTime.TotalSeconds;
			if (this.CanAutoFire())
			{
				this.nextAutoFire = -1f;
			}
			this.updateAimPos();
			return true;
		}

		// Token: 0x060018BD RID: 6333 RVA: 0x0012333F File Offset: 0x0012153F
		public virtual float GetAutoFireRate()
		{
			return 0.3f;
		}

		// Token: 0x060018BE RID: 6334 RVA: 0x00123346 File Offset: 0x00121546
		public virtual bool CanAutoFire()
		{
			return false;
		}

		// Token: 0x060018BF RID: 6335 RVA: 0x0012334C File Offset: 0x0012154C
		private void updateAimPos()
		{
			if (this.lastUser == null || !this.lastUser.IsLocalPlayer)
			{
				return;
			}
			Point mousePos = Game1.getMousePosition();
			if (Game1.options.gamepadControls && !Game1.lastCursorMotionWasMouse)
			{
				Vector2 stick = Game1.oldPadState.ThumbSticks.Left;
				if (stick.Length() < 0.25f)
				{
					stick.X = 0f;
					stick.Y = 0f;
					if (Game1.oldPadState.DPad.Down == ButtonState.Pressed)
					{
						stick.Y = -1f;
					}
					else if (Game1.oldPadState.DPad.Up == ButtonState.Pressed)
					{
						stick.Y = 1f;
					}
					if (Game1.oldPadState.DPad.Left == ButtonState.Pressed)
					{
						stick.X = -1f;
					}
					if (Game1.oldPadState.DPad.Right == ButtonState.Pressed)
					{
						stick.X = 1f;
					}
					if (stick.X != 0f && stick.Y != 0f)
					{
						stick.Normalize();
						stick *= 1f;
					}
				}
				Vector2 shoot_origin = this.GetShootOrigin(this.lastUser);
				if (!Game1.options.useLegacySlingshotFiring && stick.Length() < 0.25f)
				{
					switch (this.lastUser.FacingDirection)
					{
					case 0:
						stick = new Vector2(0f, 1f);
						break;
					case 1:
						stick = new Vector2(1f, 0f);
						break;
					case 2:
						stick = new Vector2(0f, -1f);
						break;
					case 3:
						stick = new Vector2(-1f, 0f);
						break;
					}
				}
				mousePos = Utility.Vector2ToPoint(shoot_origin + new Vector2(stick.X, -stick.Y) * 600f);
				mousePos.X -= Game1.viewport.X;
				mousePos.Y -= Game1.viewport.Y;
			}
			int mouseX = mousePos.X + Game1.viewport.X;
			int mouseY = mousePos.Y + Game1.viewport.Y;
			this.aimPos.X = mouseX;
			this.aimPos.Y = mouseY;
		}

		// Token: 0x060018C0 RID: 6336 RVA: 0x001235AC File Offset: 0x001217AC
		public override void tickUpdate(GameTime time, Farmer who)
		{
			this.lastUser = who;
			this.finishEvent.Poll();
			if (who.usingSlingshot)
			{
				if (who.IsLocalPlayer)
				{
					this.updateAimPos();
					int mouseX = this.aimPos.X;
					int mouseY = this.aimPos.Y;
					this.mouseDragAmount++;
					if (!Game1.options.useLegacySlingshotFiring)
					{
						Vector2 shoot_origin = this.GetShootOrigin(who);
						Vector2 aim_offset = this.AdjustForHeight(new Vector2((float)mouseX, (float)mouseY), true) - shoot_origin;
						if (Math.Abs(aim_offset.X) > Math.Abs(aim_offset.Y))
						{
							if (aim_offset.X < 0f)
							{
								who.faceDirection(3);
							}
							if (aim_offset.X > 0f)
							{
								who.faceDirection(1);
							}
						}
						else
						{
							if (aim_offset.Y < 0f)
							{
								who.faceDirection(0);
							}
							if (aim_offset.Y > 0f)
							{
								who.faceDirection(2);
							}
						}
					}
					else
					{
						who.faceGeneralDirection(new Vector2((float)mouseX, (float)mouseY), 0, true);
					}
					if (!Game1.options.useLegacySlingshotFiring)
					{
						if (this.canPlaySound && this.GetSlingshotChargeTime() >= 1f)
						{
							if (this.PlayUseSounds)
							{
								who.playNearbySoundAll("slingshot", null, SoundContext.Default);
							}
							this.canPlaySound = false;
						}
					}
					else if (this.canPlaySound && (Math.Abs(mouseX - this.lastClickX) > 8 || Math.Abs(mouseY - this.lastClickY) > 8) && this.mouseDragAmount > 4)
					{
						if (this.PlayUseSounds)
						{
							who.playNearbySoundAll("slingshot", null, SoundContext.Default);
						}
						this.canPlaySound = false;
					}
					if (!this.CanAutoFire())
					{
						this.lastClickX = mouseX;
						this.lastClickY = mouseY;
					}
					if (Game1.options.useLegacySlingshotFiring)
					{
						Game1.mouseCursor = Game1.cursor_none;
					}
					if (this.CanAutoFire())
					{
						bool first_fire = false;
						if (this.GetBackArmDistance(who) >= 20 && this.nextAutoFire < 0f)
						{
							this.nextAutoFire = 0f;
							first_fire = true;
						}
						if (this.nextAutoFire > 0f || first_fire)
						{
							this.nextAutoFire -= (float)time.ElapsedGameTime.TotalSeconds;
							if (this.nextAutoFire <= 0f)
							{
								this.PerformFire(who.currentLocation, who);
								this.nextAutoFire = this.GetAutoFireRate();
							}
						}
					}
				}
				int offset = (who.FacingDirection == 3 || who.FacingDirection == 1) ? 1 : ((who.FacingDirection == 0) ? 2 : 0);
				who.FarmerSprite.setCurrentFrame(42 + offset);
			}
		}

		// Token: 0x060018C1 RID: 6337 RVA: 0x00123843 File Offset: 0x00121A43
		protected override void GetAttachmentSlotSprite(int slot, out Texture2D texture, out Rectangle sourceRect)
		{
			base.GetAttachmentSlotSprite(slot, out texture, out sourceRect);
			if (this.attachments[0] == null)
			{
				sourceRect = Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 43, -1, -1);
			}
		}

		// Token: 0x060018C2 RID: 6338 RVA: 0x00123870 File Offset: 0x00121A70
		public float GetSlingshotChargeTime()
		{
			if (this.pullStartTime < 0.0)
			{
				return 0f;
			}
			return Utility.Clamp((float)((Game1.currentGameTime.TotalGameTime.TotalSeconds - this.pullStartTime) / (double)this.GetRequiredChargeTime()), 0f, 1f);
		}

		// Token: 0x060018C3 RID: 6339 RVA: 0x001238C5 File Offset: 0x00121AC5
		public float GetRequiredChargeTime()
		{
			return 0.3f;
		}

		// Token: 0x060018C4 RID: 6340 RVA: 0x001238CC File Offset: 0x00121ACC
		public override void draw(SpriteBatch b)
		{
			if (this.lastUser.usingSlingshot && this.lastUser.IsLocalPlayer)
			{
				int mouseX = this.aimPos.X;
				int mouseY = this.aimPos.Y;
				Vector2 shoot_origin = this.GetShootOrigin(this.lastUser);
				Vector2 v = Utility.getVelocityTowardPoint(shoot_origin, this.AdjustForHeight(new Vector2((float)mouseX, (float)mouseY), true), 256f);
				double distanceBetweenRadiusAndSquare = Math.Sqrt((double)(v.X * v.X + v.Y * v.Y)) - 181.0;
				double xPercent = (double)(v.X / 256f);
				double yPercent = (double)(v.Y / 256f);
				int x = (int)((double)v.X - distanceBetweenRadiusAndSquare * xPercent);
				int y = (int)((double)v.Y - distanceBetweenRadiusAndSquare * yPercent);
				if (!Game1.options.useLegacySlingshotFiring)
				{
					x *= -1;
					y *= -1;
				}
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(shoot_origin.X - (float)x, shoot_origin.Y - (float)y)), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 43, -1, -1)), Color.White, 0f, new Vector2(32f, 32f), 1f, SpriteEffects.None, 0.999999f);
			}
		}

		// Token: 0x060018C5 RID: 6341 RVA: 0x00123A24 File Offset: 0x00121C24
		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			base.AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
			if (base.IndexOfMenuItemView == 0 || base.IndexOfMenuItemView == 21 || base.ItemId == "47")
			{
				string name = this.Name;
				if (!(name == "Slingshot"))
				{
					if (!(name == "Master Slingshot"))
					{
						if (name == "Galaxy Slingshot")
						{
							base.CurrentParentTileIndex = int.Parse("34");
						}
					}
					else
					{
						base.CurrentParentTileIndex = int.Parse("33");
					}
				}
				else
				{
					base.CurrentParentTileIndex = int.Parse("32");
				}
				base.IndexOfMenuItemView = base.CurrentParentTileIndex;
			}
			spriteBatch.Draw(Tool.weaponsTexture, location + new Vector2(32f, 29f), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Tool.weaponsTexture, base.IndexOfMenuItemView, 16, 16)), color * transparency, 0f, new Vector2(8f, 8f), scaleSize * 4f, SpriteEffects.None, layerDepth);
			if (drawStackNumber != StackDrawType.Hide)
			{
				NetObjectArray<Object> attachments = this.attachments;
				if (((attachments != null) ? attachments[0] : null) != null)
				{
					Utility.drawTinyDigits(this.attachments[0].Stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(this.attachments[0].Stack, 3f * scaleSize)) + 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, Color.White);
				}
			}
			this.DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
		}

		// Token: 0x04000EEE RID: 3822
		public const int basicDamage = 5;

		// Token: 0x04000EEF RID: 3823
		public const string basicSlingshotId = "32";

		// Token: 0x04000EF0 RID: 3824
		public const string masterSlingshotId = "33";

		// Token: 0x04000EF1 RID: 3825
		public const string galaxySlingshotId = "34";

		// Token: 0x04000EF2 RID: 3826
		public const int drawBackSoundThreshold = 8;

		// Token: 0x04000EF3 RID: 3827
		[XmlIgnore]
		public int lastClickX;

		// Token: 0x04000EF4 RID: 3828
		[XmlIgnore]
		public int lastClickY;

		// Token: 0x04000EF5 RID: 3829
		[XmlIgnore]
		public int mouseDragAmount;

		// Token: 0x04000EF6 RID: 3830
		[XmlIgnore]
		public double pullStartTime = -1.0;

		// Token: 0x04000EF7 RID: 3831
		[XmlIgnore]
		public float nextAutoFire = -1f;

		// Token: 0x04000EF8 RID: 3832
		[XmlIgnore]
		public bool canPlaySound;

		// Token: 0x04000EF9 RID: 3833
		[XmlIgnore]
		private readonly NetEvent0 finishEvent = new NetEvent0(false);

		// Token: 0x04000EFA RID: 3834
		[XmlIgnore]
		public readonly NetPoint aimPos = new NetPoint().Interpolated(true, true);
	}
}

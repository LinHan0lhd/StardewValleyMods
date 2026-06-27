using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Audio;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Network;

namespace StardewValley.Objects
{
	// Token: 0x020001AF RID: 431
	public class ItemPedestal : Object
	{
		// Token: 0x1700032B RID: 811
		// (get) Token: 0x06001EE2 RID: 7906 RVA: 0x00163C31 File Offset: 0x00161E31
		public override string TypeDefinitionId
		{
			get
			{
				return "(BC)";
			}
		}

		// Token: 0x06001EE3 RID: 7907 RVA: 0x00163C38 File Offset: 0x00161E38
		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(this.itemModifyMutex.NetFields, "itemModifyMutex.NetFields").AddField(this.requiredItem, "requiredItem").AddField(this.successColor, "successColor").AddField(this.lockOnSuccess, "lockOnSuccess").AddField(this.locked, "locked").AddField(this.match, "match").AddField(this.isIslandShrinePedestal, "isIslandShrinePedestal");
			this.heldObject.InterpolationWait = false;
		}

		// Token: 0x06001EE4 RID: 7908 RVA: 0x00163CD4 File Offset: 0x00161ED4
		public ItemPedestal()
		{
		}

		// Token: 0x06001EE5 RID: 7909 RVA: 0x00163D34 File Offset: 0x00161F34
		public ItemPedestal(Vector2 tile, Object required_item, bool lock_on_success, Color success_color, string itemId = "221") : base(tile, itemId, false)
		{
			this.requiredItem.Value = required_item;
			this.lockOnSuccess.Value = lock_on_success;
			this.successColor.Value = success_color;
		}

		// Token: 0x06001EE6 RID: 7910 RVA: 0x00163DC0 File Offset: 0x00161FC0
		protected override Item GetOneNew()
		{
			Vector2 tileLocation = this.TileLocation;
			Object value = this.requiredItem.Value;
			return new ItemPedestal(tileLocation, (Object)((value != null) ? value.getOne() : null), this.lockOnSuccess.Value, this.successColor.Value, base.ItemId);
		}

		// Token: 0x06001EE7 RID: 7911 RVA: 0x00163E10 File Offset: 0x00162010
		protected override void GetOneCopyFrom(Item source)
		{
			base.GetOneCopyFrom(source);
			ItemPedestal fromPedestal = source as ItemPedestal;
			if (fromPedestal != null)
			{
				this.isIslandShrinePedestal.Value = fromPedestal.isIslandShrinePedestal.Value;
			}
		}

		// Token: 0x06001EE8 RID: 7912 RVA: 0x00163E44 File Offset: 0x00162044
		public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed = false)
		{
			GameLocation location = this.Location;
			if (location == null)
			{
				return false;
			}
			if (this.locked.Value)
			{
				return false;
			}
			if (!dropInItem.canBeTrashed())
			{
				return false;
			}
			if (this.heldObject.Value != null && !probe)
			{
				this.DropObject(who);
				return false;
			}
			if (dropInItem.GetType() == typeof(Object))
			{
				if (!probe)
				{
					Object placed_object = dropInItem.getOne() as Object;
					this.itemModifyMutex.RequestLock(delegate
					{
						location.playSound("woodyStep", null, null, SoundContext.Default);
						this.heldObject.Value = placed_object;
						this.UpdateItemMatch();
						this.itemModifyMutex.ReleaseLock();
					}, delegate
					{
						if (placed_object != this.heldObject.Value)
						{
							Game1.createItemDebris(placed_object, (this.TileLocation + new Vector2(0.5f, 0.5f)) * 64f, -1, location, -1, false);
						}
					});
				}
				return true;
			}
			return false;
		}

		// Token: 0x06001EE9 RID: 7913 RVA: 0x00163EF8 File Offset: 0x001620F8
		public virtual void UpdateItemMatch()
		{
			bool success = false;
			if (this.heldObject.Value != null && this.requiredItem.Value != null && Utility.getStandardDescriptionFromItem(this.heldObject.Value, 1, ' ') == Utility.getStandardDescriptionFromItem(this.requiredItem.Value, 1, ' '))
			{
				success = true;
			}
			if (success != this.match.Value)
			{
				this.match.Value = success;
				if (this.match.Value && this.lockOnSuccess.Value)
				{
					this.locked.Value = true;
				}
			}
		}

		// Token: 0x06001EEA RID: 7914 RVA: 0x00163F90 File Offset: 0x00162190
		public override bool checkForAction(Farmer who, bool checking_for_activity = false)
		{
			return !this.locked.Value && (checking_for_activity || this.DropObject(who));
		}

		// Token: 0x06001EEB RID: 7915 RVA: 0x00163FB4 File Offset: 0x001621B4
		public bool DropObject(Farmer who)
		{
			if (this.heldObject.Value != null)
			{
				this.itemModifyMutex.RequestLock(delegate
				{
					Object item = this.heldObject.Value;
					this.heldObject.Value = null;
					if (who.addItemToInventoryBool(item, false))
					{
						item.performRemoveAction();
						Game1.playSound("coin", null);
					}
					else
					{
						this.heldObject.Value = item;
					}
					this.UpdateItemMatch();
					this.itemModifyMutex.ReleaseLock();
				}, null);
				return true;
			}
			return false;
		}

		// Token: 0x06001EEC RID: 7916 RVA: 0x00163FFD File Offset: 0x001621FD
		public override bool performToolAction(Tool t)
		{
			return !this.isIslandShrinePedestal.Value && base.performToolAction(t);
		}

		// Token: 0x06001EED RID: 7917 RVA: 0x00164018 File Offset: 0x00162218
		public override void updateWhenCurrentLocation(GameTime time)
		{
			GameLocation location = this.Location;
			if (location != null)
			{
				this.itemModifyMutex.Update(location);
			}
		}

		// Token: 0x06001EEE RID: 7918 RVA: 0x0016403B File Offset: 0x0016223B
		public override bool onExplosion(Farmer who)
		{
			return !this.isIslandShrinePedestal.Value && base.onExplosion(who);
		}

		// Token: 0x06001EEF RID: 7919 RVA: 0x00164053 File Offset: 0x00162253
		public override void DayUpdate()
		{
			base.DayUpdate();
			this.itemModifyMutex.ReleaseLock();
		}

		// Token: 0x06001EF0 RID: 7920 RVA: 0x00164068 File Offset: 0x00162268
		public override void draw(SpriteBatch b, int x, int y, float alpha = 1f)
		{
			Vector2 position = new Vector2((float)(x * 64), (float)(y * 64));
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			b.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, position), new Rectangle?(itemData.GetSourceRect(0, null)), Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, Math.Max(0f, (position.Y - 2f) / 10000f));
			if (this.match.Value)
			{
				b.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, position), new Rectangle?(itemData.GetSourceRect(1, null)), this.successColor.Value, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, Math.Max(0f, (position.Y - 1f) / 10000f));
			}
			if (this.heldObject.Value != null)
			{
				Vector2 draw_position = new Vector2((float)x, (float)y);
				if (this.heldObject.Value.bigCraftable.Value)
				{
					draw_position.Y -= 1f;
				}
				this.heldObject.Value.draw(b, (int)draw_position.X * 64, (int)((draw_position.Y - 0.2f) * 64f) - 64, position.Y / 10000f, 1f);
			}
		}

		// Token: 0x040012FF RID: 4863
		[XmlIgnore]
		public NetMutex itemModifyMutex = new NetMutex();

		// Token: 0x04001300 RID: 4864
		[XmlElement("requiredItem")]
		public NetRef<Object> requiredItem = new NetRef<Object>();

		// Token: 0x04001301 RID: 4865
		[XmlElement("successColor")]
		public NetColor successColor = new NetColor();

		// Token: 0x04001302 RID: 4866
		[XmlElement("lockOnSuccess")]
		public NetBool lockOnSuccess = new NetBool();

		// Token: 0x04001303 RID: 4867
		[XmlElement("locked")]
		public NetBool locked = new NetBool();

		// Token: 0x04001304 RID: 4868
		[XmlElement("match")]
		public NetBool match = new NetBool();

		// Token: 0x04001305 RID: 4869
		[XmlElement("isIslandShrinePedestal")]
		public readonly NetBool isIslandShrinePedestal = new NetBool();

		// Token: 0x04001306 RID: 4870
		[XmlIgnore]
		public Texture2D texture;
	}
}

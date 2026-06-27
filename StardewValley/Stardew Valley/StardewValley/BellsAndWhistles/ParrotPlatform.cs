using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Network;
using xTile.Dimensions;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x0200039E RID: 926
	public class ParrotPlatform
	{
		// Token: 0x0600387E RID: 14462 RVA: 0x002CB6B4 File Offset: 0x002C98B4
		public static List<KeyValuePair<string, KeyValuePair<string, Point>>> GetDestinations(bool only_show_accessible = true)
		{
			List<KeyValuePair<string, KeyValuePair<string, Point>>> destinations = new List<KeyValuePair<string, KeyValuePair<string, Point>>>();
			destinations.Add(new KeyValuePair<string, KeyValuePair<string, Point>>("Volcano", new KeyValuePair<string, Point>("IslandNorth", new Point(60, 17))));
			if (Game1.MasterPlayer.hasOrWillReceiveMail("Island_UpgradeBridge") || !only_show_accessible)
			{
				destinations.Add(new KeyValuePair<string, KeyValuePair<string, Point>>("Archaeology", new KeyValuePair<string, Point>("IslandNorth", new Point(5, 49))));
			}
			destinations.Add(new KeyValuePair<string, KeyValuePair<string, Point>>("Farm", new KeyValuePair<string, Point>("IslandWest", new Point(74, 10))));
			destinations.Add(new KeyValuePair<string, KeyValuePair<string, Point>>("Forest", new KeyValuePair<string, Point>("IslandEast", new Point(28, 29))));
			destinations.Add(new KeyValuePair<string, KeyValuePair<string, Point>>("Docks", new KeyValuePair<string, Point>("IslandSouth", new Point(6, 32))));
			return destinations;
		}

		// Token: 0x0600387F RID: 14463 RVA: 0x002CB78C File Offset: 0x002C998C
		public static List<ParrotPlatform> CreateParrotPlatformsForArea(GameLocation location)
		{
			List<ParrotPlatform> parrot_platforms = new List<ParrotPlatform>();
			foreach (KeyValuePair<string, KeyValuePair<string, Point>> destination in ParrotPlatform.GetDestinations(false))
			{
				if (location.Name == destination.Value.Key)
				{
					parrot_platforms.Add(new ParrotPlatform(destination.Value.Value.X - 1, destination.Value.Value.Y - 2, destination.Key));
				}
			}
			return parrot_platforms;
		}

		// Token: 0x06003880 RID: 14464 RVA: 0x002CB83C File Offset: 0x002C9A3C
		public ParrotPlatform()
		{
			this.texture = Game1.content.Load<Texture2D>("LooseSprites\\ParrotPlatform");
		}

		// Token: 0x06003881 RID: 14465 RVA: 0x002CB87C File Offset: 0x002C9A7C
		public ParrotPlatform(int tile_x, int tile_y, string key) : this()
		{
			this.currentLocationKey = key;
			this.position = new Vector2((float)(tile_x * 64), (float)(tile_y * 64));
			this.parrots.Add(new ParrotPlatform.Parrot(this, 15, 20, false, false));
			this.parrots.Add(new ParrotPlatform.Parrot(this, 33, 20, true, false));
		}

		// Token: 0x06003882 RID: 14466 RVA: 0x002CB8DC File Offset: 0x002C9ADC
		public virtual void StartDeparture()
		{
			this.takeoffState = ParrotPlatform.TakeoffState.Boarding;
			Game1.playSound("parrot", null);
			foreach (ParrotPlatform.Parrot parrot in this.parrots)
			{
				parrot.squawkTime = 0.25f;
			}
			this.stateTimer = 0.5f;
			Game1.player.shouldShadowBeOffset = true;
			xTile.Dimensions.Rectangle viewport = Game1.viewport;
			Vector2 farmer_position = Game1.player.Position;
			string[] array = new string[7];
			array[0] = "continue/follow/farmer ";
			int num = 1;
			Point tilePoint = Game1.player.TilePoint;
			array[num] = tilePoint.X.ToString();
			array[2] = " ";
			int num2 = 3;
			tilePoint = Game1.player.TilePoint;
			array[num2] = tilePoint.Y.ToString();
			array[4] = " ";
			int num3 = 5;
			NetDirection facingDirection = Game1.player.facingDirection;
			array[num3] = ((facingDirection != null) ? facingDirection.ToString() : null);
			array[6] = "/playerControl parrotRide";
			this._takeoffEvent = new Event(string.Concat(array), null, "-1", null)
			{
				showWorldCharacters = true,
				showGroundObjects = true
			};
			Game1.currentLocation.currentEvent = this._takeoffEvent;
			this._takeoffEvent.Update(Game1.player.currentLocation, Game1.currentGameTime);
			Game1.player.Position = farmer_position;
			Game1.eventUp = true;
			Game1.viewport = viewport;
			foreach (ParrotPlatform.Parrot parrot2 in this.parrots)
			{
				parrot2.height = 21f;
				parrot2.position = parrot2.anchorPosition;
			}
		}

		// Token: 0x06003883 RID: 14467 RVA: 0x002CBAA0 File Offset: 0x002C9CA0
		public virtual void Update(GameTime time)
		{
			if (this.takeoffState == ParrotPlatform.TakeoffState.Idle && !Game1.player.IsBusyDoingSomething())
			{
				Microsoft.Xna.Framework.Rectangle activation_tiles = new Microsoft.Xna.Framework.Rectangle((int)this.position.X / 64, (int)this.position.Y / 64, 3, 1);
				bool on_activation_tile = activation_tiles.Contains(Game1.player.TilePoint);
				if (this._onActivationTile != on_activation_tile)
				{
					this._onActivationTile = on_activation_tile;
					if (this._onActivationTile && Game1.netWorldState.Value.ParrotPlatformsUnlocked)
					{
						this.Activate();
					}
				}
			}
			this.shake = Vector2.Zero;
			if (this.takeoffState == ParrotPlatform.TakeoffState.Liftoff)
			{
				this.shake.X = Utility.RandomFloat(-0.5f, 0.5f, null) * 4f;
				this.shake.Y = Utility.RandomFloat(-0.5f, 0.5f, null) * 4f;
			}
			if (this.stateTimer > 0f)
			{
				this.stateTimer -= (float)time.ElapsedGameTime.TotalSeconds;
			}
			if (this.takeoffState == ParrotPlatform.TakeoffState.Boarding && this.stateTimer <= 0f)
			{
				this.takeoffState = ParrotPlatform.TakeoffState.BeginFlying;
				Game1.playSound("dwoop", null);
			}
			if (this.takeoffState == ParrotPlatform.TakeoffState.BeginFlying && this.parrots[0].height >= 64f && this.stateTimer <= 0f)
			{
				this.takeoffState = ParrotPlatform.TakeoffState.Liftoff;
				this.stateTimer = 0.5f;
				Game1.playSound("treethud", null);
			}
			if (this.takeoffState == ParrotPlatform.TakeoffState.Liftoff && this.stateTimer <= 0f)
			{
				this.takeoffState = ParrotPlatform.TakeoffState.Flying;
			}
			if (this.takeoffState >= ParrotPlatform.TakeoffState.Flying && this.parrots[0].height >= 64f)
			{
				this.height += this.liftSpeed;
				this.liftSpeed += 0.025f;
				Game1.player.drawOffset = new Vector2(0f, -this.height * 4f);
				if (this.height >= 128f && this.takeoffState != ParrotPlatform.TakeoffState.Finished)
				{
					this.takeoffState = ParrotPlatform.TakeoffState.Finished;
					this._takeoffEvent.endBehaviors(null);
					this._takeoffEvent = null;
					LocationRequest locationRequest = Game1.getLocationRequest(this.currentDestination.Value.Key, false);
					locationRequest.OnWarp += delegate()
					{
						this.takeoffState = ParrotPlatform.TakeoffState.Idle;
						Game1.player.shouldShadowBeOffset = false;
						Game1.player.drawOffset = Vector2.Zero;
					};
					Game1.warpFarmer(locationRequest, this.currentDestination.Value.Value.X, this.currentDestination.Value.Value.Y, 2);
				}
			}
			foreach (ParrotPlatform.Parrot parrot in this.parrots)
			{
				parrot.Update(time);
			}
		}

		// Token: 0x06003884 RID: 14468 RVA: 0x002CBD94 File Offset: 0x002C9F94
		public virtual void Activate()
		{
			List<Response> responses = new List<Response>();
			foreach (KeyValuePair<string, KeyValuePair<string, Point>> destination in ParrotPlatform.GetDestinations(true))
			{
				if (destination.Key != this.currentLocationKey)
				{
					responses.Add(new Response("Go" + destination.Key, Game1.content.LoadString("Strings\\UI:ParrotPlatform_" + destination.Key)));
				}
			}
			responses.Add(new Response("Cancel", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel")));
			Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\UI:ParrotPlatform_Question"), responses.ToArray(), "ParrotPlatform");
			ParrotPlatform.activePlatform = this;
		}

		// Token: 0x06003885 RID: 14469 RVA: 0x002CBE7C File Offset: 0x002CA07C
		public virtual bool AnswerQuestion(Response answer)
		{
			if (this == ParrotPlatform.activePlatform)
			{
				if (Game1.currentLocation.lastQuestionKey != null && Game1.currentLocation.afterQuestion == null && (ArgUtility.SplitBySpace(Game1.currentLocation.lastQuestionKey)[0] + "_" + answer.responseKey).StartsWith("ParrotPlatform_Go"))
				{
					string destination_key = answer.responseKey.Substring(2);
					foreach (KeyValuePair<string, KeyValuePair<string, Point>> destination in ParrotPlatform.GetDestinations(true))
					{
						if (destination.Key == destination_key)
						{
							this.currentDestination = destination;
							break;
						}
					}
					this.StartDeparture();
					return true;
				}
				ParrotPlatform.activePlatform = null;
			}
			return false;
		}

		// Token: 0x06003886 RID: 14470 RVA: 0x002CBF54 File Offset: 0x002CA154
		public virtual void Cleanup()
		{
			ParrotPlatform.activePlatform = null;
		}

		// Token: 0x06003887 RID: 14471 RVA: 0x002CBF5C File Offset: 0x002CA15C
		public virtual bool CheckCollisions(Microsoft.Xna.Framework.Rectangle rectangle)
		{
			int wall_width = 16;
			return rectangle.Intersects(new Microsoft.Xna.Framework.Rectangle((int)this.position.X, (int)this.position.Y, 192, wall_width)) || rectangle.Intersects(new Microsoft.Xna.Framework.Rectangle((int)this.position.X, (int)this.position.Y + 128 - wall_width, 64, wall_width)) || rectangle.Intersects(new Microsoft.Xna.Framework.Rectangle((int)this.position.X + 128, (int)this.position.Y + 128 - wall_width, 64, wall_width)) || (this.takeoffState > ParrotPlatform.TakeoffState.Idle && rectangle.Intersects(new Microsoft.Xna.Framework.Rectangle((int)this.position.X + 64, (int)this.position.Y + 128 - wall_width, 64, wall_width))) || rectangle.Intersects(new Microsoft.Xna.Framework.Rectangle((int)this.position.X, (int)this.position.Y, wall_width, 128)) || rectangle.Intersects(new Microsoft.Xna.Framework.Rectangle((int)this.position.X + 192 - wall_width, (int)this.position.Y, wall_width, 128));
		}

		// Token: 0x06003888 RID: 14472 RVA: 0x002CC0AC File Offset: 0x002CA2AC
		public virtual bool OccupiesTile(Vector2 tile_pos)
		{
			return tile_pos.X >= this.position.X / 64f && tile_pos.X < this.position.X / 64f + 3f && tile_pos.Y >= this.position.Y / 64f && tile_pos.Y < this.position.Y / 64f + 2f;
		}

		// Token: 0x06003889 RID: 14473 RVA: 0x002CC12C File Offset: 0x002CA32C
		public virtual Vector2 GetDrawPosition()
		{
			return this.position - new Vector2(0f, 128f + this.height * 4f) + this.shake;
		}

		// Token: 0x0600388A RID: 14474 RVA: 0x002CC160 File Offset: 0x002CA360
		public virtual void Draw(SpriteBatch b)
		{
			b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, this.position - new Vector2(0f, 128f) + new Vector2(-2f, 38f) * 4f + new Vector2(48f, 32f) * 4f / 2f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(48, 73, 48, 32)), Color.White, 0f, new Vector2(48f, 32f) / 2f, 4f * (1f - Math.Min(1f, this.height / 480f)), SpriteEffects.None, 0f);
			b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, this.GetDrawPosition()), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, 0, 48, 68)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, this.position.Y / 10000f);
			b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, this.GetDrawPosition()), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(48, 0, 48, 68)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (this.position.Y + 128f) / 10000f);
			if (Game1.netWorldState.Value.ParrotPlatformsUnlocked)
			{
				foreach (ParrotPlatform.Parrot parrot in this.parrots)
				{
					parrot.Draw(b);
				}
			}
		}

		// Token: 0x040024FA RID: 9466
		[XmlIgnore]
		[InstancedStatic]
		public static ParrotPlatform activePlatform;

		// Token: 0x040024FB RID: 9467
		[XmlIgnore]
		public Vector2 position;

		// Token: 0x040024FC RID: 9468
		[XmlIgnore]
		public Texture2D texture;

		// Token: 0x040024FD RID: 9469
		[XmlIgnore]
		public List<ParrotPlatform.Parrot> parrots = new List<ParrotPlatform.Parrot>();

		// Token: 0x040024FE RID: 9470
		[XmlIgnore]
		public float height;

		// Token: 0x040024FF RID: 9471
		[XmlIgnore]
		protected Event _takeoffEvent;

		// Token: 0x04002500 RID: 9472
		[XmlIgnore]
		public ParrotPlatform.TakeoffState takeoffState;

		// Token: 0x04002501 RID: 9473
		[XmlIgnore]
		public float stateTimer;

		// Token: 0x04002502 RID: 9474
		[XmlIgnore]
		public float liftSpeed;

		// Token: 0x04002503 RID: 9475
		[XmlIgnore]
		protected bool _onActivationTile;

		// Token: 0x04002504 RID: 9476
		public Vector2 shake = Vector2.Zero;

		// Token: 0x04002505 RID: 9477
		public string currentLocationKey = "";

		// Token: 0x04002506 RID: 9478
		public KeyValuePair<string, KeyValuePair<string, Point>> currentDestination;

		// Token: 0x020006B4 RID: 1716
		public enum TakeoffState
		{
			// Token: 0x04003063 RID: 12387
			Idle,
			// Token: 0x04003064 RID: 12388
			Boarding,
			// Token: 0x04003065 RID: 12389
			BeginFlying,
			// Token: 0x04003066 RID: 12390
			Liftoff,
			// Token: 0x04003067 RID: 12391
			Flying,
			// Token: 0x04003068 RID: 12392
			Finished
		}

		// Token: 0x020006B5 RID: 1717
		public class Parrot
		{
			// Token: 0x06004649 RID: 17993 RVA: 0x00322358 File Offset: 0x00320558
			public Parrot(ParrotPlatform platform, int x, int y, bool facing_right, bool facing_up)
			{
				this._platform = platform;
				this.texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\parrots");
				this.position = new Vector2((float)x, (float)y);
				this.anchorPosition = this.position;
				this.facingRight = facing_right;
				this.facingUp = facing_up;
				this.swayOffset = Utility.RandomFloat(0f, 100f, null);
			}

			// Token: 0x0600464A RID: 17994 RVA: 0x003223E0 File Offset: 0x003205E0
			public virtual void UpdateLine(Vector2 start, Vector2 end)
			{
				float sag = Utility.Lerp(15f, 0f, (this.height - 21f) / 43f);
				for (int i = 0; i < this.points.Length; i++)
				{
					Vector2 point = new Vector2(Utility.Lerp(start.X, end.X, (float)i / (float)(this.points.Length - 1)), Utility.Lerp(start.Y, end.Y, (float)i / (float)(this.points.Length - 1)));
					point.Y -= ((float)Math.Pow((double)(2f * ((float)i / (float)(this.points.Length - 1)) - 1f), 2.0) - 1f) * sag;
					this.points[i] = point;
				}
			}

			// Token: 0x0600464B RID: 17995 RVA: 0x003224B8 File Offset: 0x003206B8
			public virtual void Update(GameTime time)
			{
				if (this.squawkTime > 0f)
				{
					this.squawkTime -= (float)time.ElapsedGameTime.TotalSeconds;
				}
				if (this._platform.takeoffState >= ParrotPlatform.TakeoffState.BeginFlying)
				{
					this.nextFlap -= (float)time.ElapsedGameTime.TotalSeconds;
					if (this.nextFlap <= 0f)
					{
						this.flapping = !this.flapping;
						if (this.flapping)
						{
							Game1.playSound("batFlap", null);
							this.nextFlap = Utility.RandomFloat(0.025f, 0.1f, null);
						}
						else
						{
							this.nextFlap = Utility.RandomFloat(0.075f, 0.15f, null);
						}
					}
					if (this.height < 64f)
					{
						this.height += this.liftSpeed;
						this.liftSpeed += 0.025f;
						if (this.facingRight)
						{
							this.position.X = this.position.X + 0.15f;
						}
						else
						{
							this.position.X = this.position.X - 0.15f;
						}
						if (this.facingUp)
						{
							this.position.Y = this.position.Y - 0.15f;
							return;
						}
						this.position.Y = this.position.Y + 0.15f;
					}
				}
			}

			// Token: 0x0600464C RID: 17996 RVA: 0x0032261C File Offset: 0x0032081C
			public virtual void Draw(SpriteBatch b)
			{
				Vector2 draw_position = this._platform.GetDrawPosition() + this.position * 4f;
				float radius = Utility.Lerp(0f, 2f, (this.height - 21f) / 43f);
				Vector2 draw_offset = new Vector2((float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalSeconds * 4.0 + (double)this.swayOffset) * radius, (float)Math.Cos(Game1.currentGameTime.TotalGameTime.TotalSeconds * 16.0 + (double)this.swayOffset) * radius);
				if (this._platform.takeoffState <= ParrotPlatform.TakeoffState.Boarding)
				{
					int base_frame = 0;
					if (this.squawkTime > 0f)
					{
						draw_offset.X += Utility.RandomFloat(-0.15f, 0.15f, null) * 4f;
						draw_offset.Y += Utility.RandomFloat(-0.15f, 0.15f, null) * 4f;
						base_frame = 1;
					}
					b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, draw_position - new Vector2(0f, this.height * 4f) + draw_offset * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(base_frame * 24, 0, 24, 24)), Color.White, 0f, new Vector2(12f, 19f), 4f, this.facingRight ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (draw_position.Y + 0.1f + 192f) / 10000f);
					return;
				}
				int frame_off = (this.flapping > false) ? 1 : 0;
				if (this.flapping && this.nextFlap <= 0.05f)
				{
					frame_off = 2;
				}
				int base_frame2 = 5;
				if (this.facingUp)
				{
					base_frame2 = 8;
				}
				b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, draw_position - new Vector2(0f, this.height * 4f) + draw_offset * 4f), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle((base_frame2 + frame_off) * 24, 0, 24, 24)), Color.White, 0f, new Vector2(12f, 19f), 4f, this.facingRight ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (draw_position.Y + 0.1f + 128f) / 10000f);
				Vector2 anchor_draw_position = this._platform.position + this.anchorPosition * 4f;
				Vector2 drawPosition = this._platform.GetDrawPosition();
				Vector2 start = Utility.snapDrawPosition(Game1.GlobalToLocal(drawPosition + (this.anchorPosition - new Vector2(0f, 21f)) * 4f));
				Vector2 end = Utility.snapDrawPosition(Game1.GlobalToLocal(drawPosition + (this.position - new Vector2(0f, this.height) + draw_offset) * 4f));
				this.UpdateLine(start + new Vector2(2f, 0f), end);
				if (this.points != null)
				{
					Vector2? last_position = null;
					float sort_step = 1E-06f;
					float sort_offset = 0f;
					float sort_layer = (anchor_draw_position.Y + 0.05f) / 10000f;
					foreach (Vector2 current_point in this.points)
					{
						b.Draw(this._platform.texture, current_point, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(16, 68, 16, 16)), Color.White, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, sort_layer + sort_offset);
						sort_offset += sort_step;
						if (last_position != null)
						{
							Vector2 offset = current_point - last_position.Value;
							int distance = (int)Math.Ceiling((double)(offset.Length() / 4f));
							float rotation = -(float)Math.Atan2((double)offset.X, (double)offset.Y) + 1.5707964f;
							b.Draw(this._platform.texture, last_position.Value, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, 68, 16, 16)), Color.White, rotation, new Vector2(0f, 8f), new Vector2((float)(4 * distance) / 16f, 4f), SpriteEffects.None, sort_layer + sort_offset);
							sort_offset += sort_step;
						}
						last_position = new Vector2?(current_point);
					}
				}
			}

			// Token: 0x04003069 RID: 12393
			public Vector2 position;

			// Token: 0x0400306A RID: 12394
			public Vector2 anchorPosition;

			// Token: 0x0400306B RID: 12395
			public Texture2D texture;

			// Token: 0x0400306C RID: 12396
			protected ParrotPlatform _platform;

			// Token: 0x0400306D RID: 12397
			protected bool facingRight;

			// Token: 0x0400306E RID: 12398
			protected bool facingUp;

			// Token: 0x0400306F RID: 12399
			public const int START_HEIGHT = 21;

			// Token: 0x04003070 RID: 12400
			public const int END_HEIGHT = 64;

			// Token: 0x04003071 RID: 12401
			public float height = 21f;

			// Token: 0x04003072 RID: 12402
			public bool flapping;

			// Token: 0x04003073 RID: 12403
			public float nextFlap;

			// Token: 0x04003074 RID: 12404
			public float slack;

			// Token: 0x04003075 RID: 12405
			public Vector2[] points = new Vector2[4];

			// Token: 0x04003076 RID: 12406
			public float swayOffset;

			// Token: 0x04003077 RID: 12407
			public float liftSpeed;

			// Token: 0x04003078 RID: 12408
			public float squawkTime;
		}
	}
}

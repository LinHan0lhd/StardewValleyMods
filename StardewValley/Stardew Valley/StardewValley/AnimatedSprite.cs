using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace StardewValley
{
	// Token: 0x0200007F RID: 127
	public class AnimatedSprite : INetObject<NetFields>
	{
		// Token: 0x17000087 RID: 135
		// (get) Token: 0x0600048B RID: 1163 RVA: 0x00015AEA File Offset: 0x00013CEA
		public NetFields NetFields { get; } = new NetFields("AnimatedSprite");

		// Token: 0x17000088 RID: 136
		// (get) Token: 0x0600048C RID: 1164 RVA: 0x00015AF2 File Offset: 0x00013CF2
		public Texture2D Texture
		{
			get
			{
				this.loadTexture();
				return this.spriteTexture;
			}
		}

		// Token: 0x17000089 RID: 137
		// (get) Token: 0x0600048D RID: 1165 RVA: 0x00015B00 File Offset: 0x00013D00
		protected int textureWidth
		{
			get
			{
				Texture2D texture = this.Texture;
				if (texture == null)
				{
					return 96;
				}
				return texture.Width;
			}
		}

		// Token: 0x1700008A RID: 138
		// (get) Token: 0x0600048E RID: 1166 RVA: 0x00015B14 File Offset: 0x00013D14
		protected int textureHeight
		{
			get
			{
				Texture2D texture = this.Texture;
				if (texture == null)
				{
					return 128;
				}
				return texture.Height;
			}
		}

		// Token: 0x1700008B RID: 139
		// (get) Token: 0x0600048F RID: 1167 RVA: 0x00015B2B File Offset: 0x00013D2B
		// (set) Token: 0x06000490 RID: 1168 RVA: 0x00015B38 File Offset: 0x00013D38
		public int SpriteWidth
		{
			get
			{
				return this.spriteWidth.Get();
			}
			set
			{
				this.spriteWidth.Value = value;
			}
		}

		// Token: 0x1700008C RID: 140
		// (get) Token: 0x06000491 RID: 1169 RVA: 0x00015B46 File Offset: 0x00013D46
		// (set) Token: 0x06000492 RID: 1170 RVA: 0x00015B63 File Offset: 0x00013D63
		public int SpriteHeight
		{
			get
			{
				if (this.tempSpriteHeight != -1)
				{
					return this.tempSpriteHeight;
				}
				return this.spriteHeight.Get();
			}
			set
			{
				this.spriteHeight.Value = value;
				this.tempSpriteHeight = -1;
			}
		}

		// Token: 0x1700008D RID: 141
		// (get) Token: 0x06000493 RID: 1171 RVA: 0x00015B78 File Offset: 0x00013D78
		// (set) Token: 0x06000494 RID: 1172 RVA: 0x00015B80 File Offset: 0x00013D80
		public virtual int CurrentFrame
		{
			get
			{
				return this.currentFrame;
			}
			set
			{
				this.currentFrame = value;
				this.UpdateSourceRect();
			}
		}

		// Token: 0x1700008E RID: 142
		// (get) Token: 0x06000495 RID: 1173 RVA: 0x00015B8F File Offset: 0x00013D8F
		// (set) Token: 0x06000496 RID: 1174 RVA: 0x00015BA6 File Offset: 0x00013DA6
		public List<FarmerSprite.AnimationFrame> CurrentAnimation
		{
			get
			{
				if (this.currentAnimation.Count == 0)
				{
					return null;
				}
				return this.currentAnimation;
			}
			set
			{
				this.currentAnimation.Clear();
				if (value == null)
				{
					return;
				}
				this.currentAnimation.AddRange(value);
			}
		}

		// Token: 0x1700008F RID: 143
		// (get) Token: 0x06000497 RID: 1175 RVA: 0x00015BC3 File Offset: 0x00013DC3
		// (set) Token: 0x06000498 RID: 1176 RVA: 0x00015BCB File Offset: 0x00013DCB
		public Rectangle SourceRect
		{
			get
			{
				return this.sourceRect;
			}
			set
			{
				this.sourceRect = value;
			}
		}

		// Token: 0x17000090 RID: 144
		// (get) Token: 0x06000499 RID: 1177 RVA: 0x00015BD4 File Offset: 0x00013DD4
		// (set) Token: 0x0600049A RID: 1178 RVA: 0x00015BDC File Offset: 0x00013DDC
		public virtual Character Owner { get; protected set; }

		// Token: 0x0600049B RID: 1179 RVA: 0x00015BE8 File Offset: 0x00013DE8
		public AnimatedSprite()
		{
			this.initNetFields();
			this.contentManager = Game1.content;
		}

		// Token: 0x0600049C RID: 1180 RVA: 0x00015C6E File Offset: 0x00013E6E
		public AnimatedSprite(ContentManager contentManager, string textureName, int currentFrame, int spriteWidth, int spriteHeight) : this()
		{
			this.contentManager = contentManager;
			this.currentFrame = currentFrame;
			this.SpriteWidth = spriteWidth;
			this.SpriteHeight = spriteHeight;
			this.LoadTexture(textureName, true);
		}

		// Token: 0x0600049D RID: 1181 RVA: 0x00015C9C File Offset: 0x00013E9C
		public AnimatedSprite(ContentManager contentManager, string textureName) : this()
		{
			this.contentManager = contentManager;
			this.LoadTexture(textureName, true);
		}

		// Token: 0x0600049E RID: 1182 RVA: 0x00015CB3 File Offset: 0x00013EB3
		public AnimatedSprite(string textureName, int currentFrame, int spriteWidth, int spriteHeight) : this(Game1.content, textureName, currentFrame, spriteWidth, spriteHeight)
		{
		}

		// Token: 0x0600049F RID: 1183 RVA: 0x00015CC5 File Offset: 0x00013EC5
		public AnimatedSprite(string textureName) : this(Game1.content, textureName)
		{
		}

		// Token: 0x060004A0 RID: 1184 RVA: 0x00015CD3 File Offset: 0x00013ED3
		protected virtual void initNetFields()
		{
			this.NetFields.SetOwner(this).AddField(this.textureName, "textureName").AddField(this.spriteWidth, "spriteWidth").AddField(this.spriteHeight, "spriteHeight");
		}

		// Token: 0x060004A1 RID: 1185 RVA: 0x00015D12 File Offset: 0x00013F12
		public virtual void SetOwner(Character owner)
		{
			this.Owner = owner;
		}

		// Token: 0x060004A2 RID: 1186 RVA: 0x00015D1B File Offset: 0x00013F1B
		public virtual void LoadTexture(string textureName, bool syncTextureName = true)
		{
			if (Game1.content.DoesAssetExist<Texture2D>(textureName))
			{
				if (syncTextureName)
				{
					this.textureName.Value = textureName;
					this.overrideTextureName = null;
				}
				else
				{
					this.overrideTextureName = textureName;
				}
				this.loadTexture();
			}
		}

		// Token: 0x060004A3 RID: 1187 RVA: 0x00015D50 File Offset: 0x00013F50
		private void loadTexture()
		{
			string assetName = this.overrideTextureName ?? this.textureName.Value;
			if (this.loadedTexture == assetName)
			{
				return;
			}
			this.spriteTexture = ((assetName != null) ? this.contentManager.Load<Texture2D>(assetName) : null);
			this.loadedTexture = assetName;
			if (this.spriteTexture != null)
			{
				this.UpdateSourceRect();
			}
		}

		// Token: 0x060004A4 RID: 1188 RVA: 0x00015DAF File Offset: 0x00013FAF
		public int getHeight()
		{
			return this.SpriteHeight;
		}

		// Token: 0x060004A5 RID: 1189 RVA: 0x00015DB7 File Offset: 0x00013FB7
		public int getWidth()
		{
			return this.SpriteWidth;
		}

		// Token: 0x060004A6 RID: 1190 RVA: 0x00015DC0 File Offset: 0x00013FC0
		public virtual void StopAnimation()
		{
			if (this.ignoreStopAnimation)
			{
				return;
			}
			if (this.CurrentAnimation != null)
			{
				this.CurrentAnimation = null;
				this.currentFrame = this.oldFrame;
				this.UpdateSourceRect();
				return;
			}
			if (this is FarmerSprite && this.currentFrame >= 232)
			{
				this.currentFrame -= 8;
			}
			if (this.currentFrame >= 64 && this.currentFrame <= 155)
			{
				this.currentFrame = (this.currentFrame - this.currentFrame % (this.textureWidth / this.SpriteWidth)) % 32 + 96;
			}
			else if (this.textureUsesFlippedRightForLeft && this.currentFrame >= this.textureWidth / this.SpriteWidth * 3)
			{
				if (this.currentFrame == 14 && this.textureWidth / this.SpriteWidth == 4)
				{
					this.currentFrame = 4;
				}
			}
			else
			{
				this.currentFrame = (this.currentFrame - this.currentFrame % (this.textureWidth / this.SpriteWidth)) % 32;
			}
			this.UpdateSourceRect();
		}

		// Token: 0x060004A7 RID: 1191 RVA: 0x00015EC8 File Offset: 0x000140C8
		public virtual void standAndFaceDirection(int direction)
		{
			switch (direction)
			{
			case 0:
				this.currentFrame = 12;
				break;
			case 1:
				this.currentFrame = 6;
				break;
			case 2:
				this.currentFrame = 0;
				break;
			case 3:
				this.currentFrame = 6;
				break;
			}
			this.UpdateSourceRect();
		}

		// Token: 0x060004A8 RID: 1192 RVA: 0x00015F16 File Offset: 0x00014116
		public virtual void faceDirectionStandard(int direction)
		{
			if (direction != 0)
			{
				if (direction == 2)
				{
					direction = 0;
				}
			}
			else
			{
				direction = 2;
			}
			this.currentFrame = direction * 4;
			this.UpdateSourceRect();
		}

		// Token: 0x060004A9 RID: 1193 RVA: 0x00015F38 File Offset: 0x00014138
		public virtual void faceDirection(int direction)
		{
			if (this.ignoreStopAnimation)
			{
				return;
			}
			if (this.CurrentAnimation != null)
			{
				return;
			}
			try
			{
				switch (direction)
				{
				case 0:
					this.currentFrame = this.textureWidth / this.SpriteWidth * 2 + this.currentFrame % (this.textureWidth / this.SpriteWidth);
					break;
				case 1:
					this.currentFrame = this.textureWidth / this.SpriteWidth + this.currentFrame % (this.textureWidth / this.SpriteWidth);
					break;
				case 2:
					this.currentFrame %= this.textureWidth / this.SpriteWidth;
					break;
				case 3:
					if (this.textureUsesFlippedRightForLeft)
					{
						this.currentFrame = this.textureWidth / this.SpriteWidth + this.currentFrame % (this.textureWidth / this.SpriteWidth);
					}
					else
					{
						this.currentFrame = this.textureWidth / this.SpriteWidth * 3 + this.currentFrame % (this.textureWidth / this.SpriteWidth);
					}
					break;
				}
			}
			catch (Exception)
			{
			}
			this.UpdateSourceRect();
		}

		// Token: 0x060004AA RID: 1194 RVA: 0x00016060 File Offset: 0x00014260
		public virtual void AnimateRight(GameTime gameTime, int intervalOffset = 0, string soundForFootstep = "")
		{
			if (this.currentFrame >= this.framesPerAnimation * 2 || this.currentFrame < this.framesPerAnimation)
			{
				this.currentFrame = this.framesPerAnimation + this.currentFrame % this.framesPerAnimation;
			}
			this.timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
			if (this.timer > this.interval + (float)intervalOffset)
			{
				this.currentFrame++;
				this.timer = 0f;
				if (this.currentFrame % 2 != 0 && soundForFootstep.Length > 0 && (Game1.currentSong == null || Game1.currentSong.IsStopped))
				{
					Game1.playSound(soundForFootstep, null);
				}
				if (this.currentFrame >= this.framesPerAnimation * 2 && this.loop)
				{
					this.currentFrame = this.framesPerAnimation;
				}
			}
			this.UpdateSourceRect();
		}

		// Token: 0x060004AB RID: 1195 RVA: 0x0001614C File Offset: 0x0001434C
		public virtual void AnimateUp(GameTime gameTime, int intervalOffset = 0, string soundForFootstep = "")
		{
			if (this.currentFrame >= this.framesPerAnimation * 3 || this.currentFrame < this.framesPerAnimation * 2)
			{
				this.currentFrame = this.framesPerAnimation * 2 + this.currentFrame % this.framesPerAnimation;
			}
			this.timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
			if (this.timer > this.interval + (float)intervalOffset)
			{
				this.currentFrame++;
				this.timer = 0f;
				if (this.currentFrame % 2 != 0 && soundForFootstep.Length > 0 && (Game1.currentSong == null || Game1.currentSong.IsStopped))
				{
					Game1.playSound(soundForFootstep, null);
				}
				if (this.currentFrame >= this.framesPerAnimation * 3 && this.loop)
				{
					this.currentFrame = this.framesPerAnimation * 2;
				}
			}
			this.UpdateSourceRect();
		}

		// Token: 0x060004AC RID: 1196 RVA: 0x00016240 File Offset: 0x00014440
		public virtual void AnimateDown(GameTime gameTime, int intervalOffset = 0, string soundForFootstep = "")
		{
			if (this.currentFrame >= this.framesPerAnimation || this.currentFrame < 0)
			{
				this.currentFrame %= this.framesPerAnimation;
			}
			this.timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
			if (this.timer > this.interval + (float)intervalOffset)
			{
				this.currentFrame++;
				this.timer = 0f;
				if (this.currentFrame % 2 != 0 && soundForFootstep.Length > 0 && (Game1.currentSong == null || Game1.currentSong.IsStopped))
				{
					Game1.playSound(soundForFootstep, null);
				}
				if (this.currentFrame >= this.framesPerAnimation && this.loop)
				{
					this.currentFrame = 0;
				}
			}
			this.UpdateSourceRect();
		}

		// Token: 0x060004AD RID: 1197 RVA: 0x00016318 File Offset: 0x00014518
		public virtual void AnimateLeft(GameTime gameTime, int intervalOffset = 0, string soundForFootstep = "")
		{
			if (this.currentFrame >= this.framesPerAnimation * 4 || this.currentFrame < this.framesPerAnimation * 3)
			{
				this.currentFrame = this.framesPerAnimation * 3 + this.currentFrame % this.framesPerAnimation;
			}
			this.timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
			if (this.timer > this.interval + (float)intervalOffset)
			{
				this.currentFrame++;
				this.timer = 0f;
				if (this.currentFrame % 2 != 0 && soundForFootstep.Length > 0 && (Game1.currentSong == null || Game1.currentSong.IsStopped))
				{
					Game1.playSound(soundForFootstep, null);
				}
				if (this.currentFrame >= this.framesPerAnimation * 4 && this.loop)
				{
					this.currentFrame = this.framesPerAnimation * 3;
				}
			}
			this.UpdateSourceRect();
		}

		// Token: 0x060004AE RID: 1198 RVA: 0x0001640C File Offset: 0x0001460C
		public virtual bool Animate(GameTime gameTime, int startFrame, int numberOfFrames, float interval)
		{
			if (this.currentFrame >= startFrame + numberOfFrames || this.currentFrame < startFrame)
			{
				this.currentFrame = startFrame + this.currentFrame % numberOfFrames;
			}
			this.timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
			if (this.timer > interval)
			{
				this.currentFrame++;
				this.timer = 0f;
				if (this.currentFrame >= startFrame + numberOfFrames)
				{
					if (this.loop)
					{
						this.currentFrame = startFrame;
					}
					this.UpdateSourceRect();
					return true;
				}
			}
			this.UpdateSourceRect();
			return false;
		}

		// Token: 0x060004AF RID: 1199 RVA: 0x000164A5 File Offset: 0x000146A5
		public virtual void ClearAnimation()
		{
			this.currentAnimation.Clear();
			this.oldFrame = this.currentFrame;
			this.currentAnimationIndex = 0;
		}

		// Token: 0x060004B0 RID: 1200 RVA: 0x000164C5 File Offset: 0x000146C5
		public virtual void AddFrame(FarmerSprite.AnimationFrame frame)
		{
			if (this.currentAnimation.Count == 0)
			{
				this.timer = (float)frame.milliseconds;
				this.currentFrame = frame.frame;
			}
			this.currentAnimation.Add(frame);
		}

		// Token: 0x060004B1 RID: 1201 RVA: 0x000164FC File Offset: 0x000146FC
		public virtual void setCurrentAnimation(List<FarmerSprite.AnimationFrame> animation)
		{
			this.currentAnimation.Clear();
			this.currentAnimation.AddRange(animation);
			this.oldFrame = this.currentFrame;
			this.currentAnimationIndex = 0;
			if (this.CurrentAnimation.Count > 0)
			{
				this.timer = (float)this.CurrentAnimation[0].milliseconds;
				this.currentFrame = this.CurrentAnimation[0].frame;
			}
		}

		// Token: 0x060004B2 RID: 1202 RVA: 0x00016570 File Offset: 0x00014770
		public virtual bool animateOnce(GameTime time)
		{
			if (this.CurrentAnimation != null)
			{
				this.timer -= (float)time.ElapsedGameTime.Milliseconds;
				if (this.timer <= 0f)
				{
					if (this.CurrentAnimation[this.currentAnimationIndex].frameEndBehavior != null)
					{
						this.CurrentAnimation[this.currentAnimationIndex].frameEndBehavior(null);
						if (this.CurrentAnimation == null)
						{
							this.currentFrame = this.oldFrame;
							this.CurrentAnimation = null;
							this.UpdateSourceRect();
							return true;
						}
					}
					this.currentAnimationIndex++;
					if (this.currentAnimationIndex >= this.CurrentAnimation.Count)
					{
						if (!this.loop)
						{
							this.currentFrame = this.oldFrame;
							this.CurrentAnimation = null;
							this.UpdateSourceRect();
							return true;
						}
						this.currentAnimationIndex = 0;
					}
					if (this.CurrentAnimation[this.currentAnimationIndex].frameStartBehavior != null)
					{
						this.CurrentAnimation[this.currentAnimationIndex].frameStartBehavior(null);
					}
					if (this.CurrentAnimation != null)
					{
						this.timer = (float)this.CurrentAnimation[this.currentAnimationIndex].milliseconds;
						this.currentFrame = this.CurrentAnimation[this.currentAnimationIndex].frame;
					}
				}
				this.UpdateSourceRect();
				return false;
			}
			this.UpdateSourceRect();
			return true;
		}

		// Token: 0x060004B3 RID: 1203 RVA: 0x000166DC File Offset: 0x000148DC
		public virtual void UpdateSourceRect()
		{
			if (!this.ignoreSourceRectUpdates)
			{
				int curSpriteWidth = this.SpriteWidth;
				int curSpriteHeight = this.SpriteHeight;
				int curTextureWidth = this.textureWidth;
				int curTextureHeight = this.textureHeight;
				this.SourceRect = AnimatedSprite.GetSourceRect(curTextureWidth, curSpriteWidth, curSpriteHeight, this.currentFrame);
				if (this.Texture != null && (this.SourceRect.Right > curTextureWidth || this.SourceRect.Bottom > curTextureHeight))
				{
					this.currentFrame = 0;
					this.SourceRect = AnimatedSprite.GetSourceRect(curTextureWidth, curSpriteWidth, curSpriteHeight, this.currentFrame);
				}
			}
		}

		// Token: 0x060004B4 RID: 1204 RVA: 0x00016768 File Offset: 0x00014968
		public virtual void draw(SpriteBatch b, Vector2 screenPosition, float layerDepth)
		{
			if (this.Texture != null)
			{
				b.Draw(this.Texture, screenPosition, new Rectangle?(this.sourceRect), Color.White, 0f, Vector2.Zero, 4f, (this.CurrentAnimation != null && this.CurrentAnimation[this.currentAnimationIndex].flip) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
			}
		}

		// Token: 0x060004B5 RID: 1205 RVA: 0x000167D0 File Offset: 0x000149D0
		public virtual void draw(SpriteBatch b, Vector2 screenPosition, float layerDepth, int xOffset, int yOffset, Color c, bool flip = false, float scale = 1f, float rotation = 0f, bool characterSourceRectOffset = false)
		{
			if (this.Texture != null)
			{
				b.Draw(this.Texture, screenPosition, new Rectangle?(new Rectangle(this.sourceRect.X + xOffset, this.sourceRect.Y + yOffset, this.sourceRect.Width, this.sourceRect.Height)), c, rotation, characterSourceRectOffset ? new Vector2((float)(this.SpriteWidth / 2), (float)this.SpriteHeight * 3f / 4f) : Vector2.Zero, scale, (flip || (this.CurrentAnimation != null && this.CurrentAnimation[this.currentAnimationIndex].flip)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
			}
		}

		// Token: 0x060004B6 RID: 1206 RVA: 0x0001688C File Offset: 0x00014A8C
		public virtual void drawShadow(SpriteBatch b, Vector2 screenPosition, float scale = 4f, float alpha = 1f)
		{
			b.Draw(Game1.shadowTexture, screenPosition + new Vector2((float)(this.SpriteWidth / 2 * 4) - scale, (float)(this.SpriteHeight * 4) - scale), new Rectangle?(Game1.shadowTexture.Bounds), Color.White * alpha, 0f, Utility.PointToVector2(Game1.shadowTexture.Bounds.Center), scale, SpriteEffects.None, 1E-05f);
		}

		// Token: 0x060004B7 RID: 1207 RVA: 0x00016905 File Offset: 0x00014B05
		public virtual void drawShadow(SpriteBatch b, Vector2 screenPosition, float scale = 4f)
		{
			this.drawShadow(b, screenPosition, scale, 1f);
		}

		// Token: 0x060004B8 RID: 1208 RVA: 0x00016918 File Offset: 0x00014B18
		public virtual AnimatedSprite Clone()
		{
			AnimatedSprite animatedSprite = new AnimatedSprite();
			animatedSprite.spriteWidth.Set(this.spriteWidth.Value);
			animatedSprite.spriteHeight.Set(this.spriteHeight.Value);
			animatedSprite.spriteTexture = this.spriteTexture;
			animatedSprite.loadedTexture = this.loadedTexture;
			animatedSprite.textureName.Set(this.textureName.Value);
			animatedSprite.timer = this.timer;
			animatedSprite.interval = this.interval;
			animatedSprite.framesPerAnimation = this.framesPerAnimation;
			animatedSprite.currentFrame = this.currentFrame;
			animatedSprite.tempSpriteHeight = this.tempSpriteHeight;
			animatedSprite.sourceRect = new Rectangle(this.sourceRect.X, this.sourceRect.Y, this.sourceRect.Width, this.sourceRect.Height);
			animatedSprite.loop = this.loop;
			animatedSprite.ignoreStopAnimation = this.ignoreStopAnimation;
			animatedSprite.textureUsesFlippedRightForLeft = this.textureUsesFlippedRightForLeft;
			animatedSprite.CurrentAnimation = this.CurrentAnimation;
			animatedSprite.oldFrame = this.oldFrame;
			animatedSprite.currentAnimationIndex = this.currentAnimationIndex;
			animatedSprite.contentManager = this.contentManager;
			animatedSprite.UpdateSourceRect();
			return animatedSprite;
		}

		// Token: 0x060004B9 RID: 1209 RVA: 0x00016A51 File Offset: 0x00014C51
		public static Rectangle GetSourceRect(int textureWidth, int spriteWidth, int spriteHeight, int frame)
		{
			return new Rectangle(frame * spriteWidth % textureWidth, frame * spriteWidth / textureWidth * spriteHeight, spriteWidth, spriteHeight);
		}

		// Token: 0x040001C0 RID: 448
		public Texture2D spriteTexture;

		// Token: 0x040001C1 RID: 449
		public string loadedTexture;

		// Token: 0x040001C2 RID: 450
		public string overrideTextureName;

		// Token: 0x040001C3 RID: 451
		public readonly NetString textureName = new NetString();

		// Token: 0x040001C4 RID: 452
		public float timer;

		// Token: 0x040001C5 RID: 453
		public float interval = 175f;

		// Token: 0x040001C6 RID: 454
		public int framesPerAnimation = 4;

		// Token: 0x040001C7 RID: 455
		public int currentFrame;

		// Token: 0x040001C8 RID: 456
		public readonly NetInt spriteWidth = new NetInt(16);

		// Token: 0x040001C9 RID: 457
		public readonly NetInt spriteHeight = new NetInt(24);

		// Token: 0x040001CA RID: 458
		public int tempSpriteHeight = -1;

		// Token: 0x040001CB RID: 459
		public Rectangle sourceRect;

		// Token: 0x040001CC RID: 460
		public bool loop = true;

		// Token: 0x040001CD RID: 461
		public bool ignoreStopAnimation;

		// Token: 0x040001CE RID: 462
		public bool textureUsesFlippedRightForLeft;

		// Token: 0x040001CF RID: 463
		public AnimatedSprite.endOfAnimationBehavior endOfAnimationFunction;

		// Token: 0x040001D0 RID: 464
		public readonly List<FarmerSprite.AnimationFrame> currentAnimation = new List<FarmerSprite.AnimationFrame>(12);

		// Token: 0x040001D1 RID: 465
		public int oldFrame;

		// Token: 0x040001D2 RID: 466
		public int currentAnimationIndex;

		// Token: 0x040001D3 RID: 467
		protected ContentManager contentManager;

		// Token: 0x040001D6 RID: 470
		public bool ignoreSourceRectUpdates;

		// Token: 0x02000401 RID: 1025
		// (Invoke) Token: 0x06003A49 RID: 14921
		public delegate void endOfAnimationBehavior(Farmer who);
	}
}

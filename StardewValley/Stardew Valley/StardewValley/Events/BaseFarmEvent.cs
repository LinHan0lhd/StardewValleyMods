using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace StardewValley.Events
{
	// Token: 0x02000322 RID: 802
	public abstract class BaseFarmEvent : FarmEvent, INetObject<NetFields>
	{
		// Token: 0x1700045C RID: 1116
		// (get) Token: 0x0600347B RID: 13435 RVA: 0x0029CF1E File Offset: 0x0029B11E
		// (set) Token: 0x0600347C RID: 13436 RVA: 0x0029CF26 File Offset: 0x0029B126
		public NetFields NetFields { get; private set; }

		// Token: 0x0600347D RID: 13437 RVA: 0x0029CF2F File Offset: 0x0029B12F
		protected BaseFarmEvent()
		{
			this.initNetFields();
		}

		// Token: 0x0600347E RID: 13438 RVA: 0x0029CF3D File Offset: 0x0029B13D
		public virtual void initNetFields()
		{
			this.NetFields = new NetFields(base.GetType().Name).SetOwner(this);
		}

		// Token: 0x0600347F RID: 13439 RVA: 0x0029CF5B File Offset: 0x0029B15B
		public virtual bool setUp()
		{
			return false;
		}

		// Token: 0x06003480 RID: 13440 RVA: 0x0029CF5E File Offset: 0x0029B15E
		public virtual bool tickUpdate(GameTime time)
		{
			return true;
		}

		// Token: 0x06003481 RID: 13441 RVA: 0x0029CF61 File Offset: 0x0029B161
		public virtual void draw(SpriteBatch b)
		{
		}

		// Token: 0x06003482 RID: 13442 RVA: 0x0029CF63 File Offset: 0x0029B163
		public virtual void drawAboveEverything(SpriteBatch b)
		{
		}

		// Token: 0x06003483 RID: 13443 RVA: 0x0029CF65 File Offset: 0x0029B165
		public virtual void makeChangesToLocation()
		{
		}

		// Token: 0x06003484 RID: 13444 RVA: 0x0029CF67 File Offset: 0x0029B167
		protected virtual string GenerateLightSourceId()
		{
			return base.GetType().Name;
		}
	}
}

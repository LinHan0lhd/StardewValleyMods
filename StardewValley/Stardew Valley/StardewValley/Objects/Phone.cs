using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Audio;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Objects
{
	// Token: 0x020001B3 RID: 435
	[InstanceStatics]
	public class Phone : Object
	{
		// Token: 0x1700032E RID: 814
		// (get) Token: 0x06001F16 RID: 7958 RVA: 0x00165E98 File Offset: 0x00164098
		public override string TypeDefinitionId
		{
			get
			{
				return "(BC)";
			}
		}

		// Token: 0x06001F17 RID: 7959 RVA: 0x00165E9F File Offset: 0x0016409F
		public Phone()
		{
		}

		// Token: 0x06001F18 RID: 7960 RVA: 0x00165EA8 File Offset: 0x001640A8
		public Phone(Vector2 position) : base(position, "214", false)
		{
			this.Name = "Telephone";
			this.type.Value = "Crafting";
			this.bigCraftable.Value = true;
			this.canBeSetDown.Value = true;
		}

		// Token: 0x06001F19 RID: 7961 RVA: 0x00165EF8 File Offset: 0x001640F8
		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			string callId = Phone.whichPhoneCall;
			Phone.StopRinging();
			if (callId == null)
			{
				Game1.game1.ShowTelephoneMenu();
			}
			else if (!Phone.HandleIncomingCall(callId))
			{
				Phone.HangUp();
			}
			return true;
		}

		// Token: 0x06001F1A RID: 7962 RVA: 0x00165F34 File Offset: 0x00164134
		public static bool HandleIncomingCall(string callId)
		{
			Action showDialogue = Phone.GetIncomingCallAction(callId);
			if (showDialogue == null)
			{
				return false;
			}
			Game1.playSound("openBox", null);
			Game1.player.freezePause = 500;
			DelayedAction.functionAfterDelay(showDialogue, 500);
			int count;
			if (!Game1.player.callsReceived.TryGetValue(callId, out count))
			{
				count = 0;
			}
			Game1.player.callsReceived[callId] = count + 1;
			return true;
		}

		// Token: 0x06001F1B RID: 7963 RVA: 0x00165FA8 File Offset: 0x001641A8
		public override void updateWhenCurrentLocation(GameTime time)
		{
			if (this.Location != Game1.currentLocation)
			{
				return;
			}
			if ((long)Game1.ticks != Phone.lastRunTick)
			{
				if (Game1.eventUp)
				{
					return;
				}
				Phone.lastRunTick = (long)Game1.ticks;
				if (Phone.whichPhoneCall != null && Game1.shouldTimePass(false))
				{
					if (Phone.ringingTimer == 0)
					{
						Game1.playSound("phone", null);
						Phone._phoneSoundPlaying = true;
					}
					Phone.ringingTimer += (int)time.ElapsedGameTime.TotalMilliseconds;
					if (Phone.ringingTimer >= 1800)
					{
						Phone.ringingTimer = 0;
						Phone._phoneSoundPlaying = false;
					}
				}
			}
			base.updateWhenCurrentLocation(time);
		}

		// Token: 0x06001F1C RID: 7964 RVA: 0x0016604C File Offset: 0x0016424C
		public override void DayUpdate()
		{
			base.DayUpdate();
			Phone.r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, 0.0, 0.0, 0.0);
			Phone._phoneSoundPlaying = false;
			Phone.ringingTimer = 0;
			Phone.whichPhoneCall = null;
			Phone.intervalsToRing = 0;
		}

		// Token: 0x06001F1D RID: 7965 RVA: 0x001660B0 File Offset: 0x001642B0
		public override bool minutesElapsed(int minutes)
		{
			if (!Game1.IsMasterGame)
			{
				return false;
			}
			if (Phone.lastMinutesElapsedTick != (long)Game1.ticks)
			{
				Phone.lastMinutesElapsedTick = (long)Game1.ticks;
				if (Phone.intervalsToRing == 0)
				{
					if (Phone.r == null)
					{
						Phone.r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, 0.0, 0.0, 0.0);
					}
					using (List<IPhoneHandler>.Enumerator enumerator = Phone.PhoneHandlers.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							IPhoneHandler phoneHandler = enumerator.Current;
							string callId = phoneHandler.CheckForIncomingCall(Phone.r);
							if (callId != null)
							{
								Phone.intervalsToRing = 3;
								Game1.player.team.ringPhoneEvent.Fire(callId);
								break;
							}
						}
						goto IL_EE;
					}
				}
				Phone.intervalsToRing--;
				if (Phone.intervalsToRing <= 0)
				{
					Game1.player.team.ringPhoneEvent.Fire(null);
				}
			}
			IL_EE:
			return base.minutesElapsed(minutes);
		}

		// Token: 0x06001F1E RID: 7966 RVA: 0x001661C4 File Offset: 0x001643C4
		public static bool IsRinging()
		{
			return Phone._phoneSoundPlaying;
		}

		// Token: 0x06001F1F RID: 7967 RVA: 0x001661CB File Offset: 0x001643CB
		public static void Ring(string callId)
		{
			if (string.IsNullOrWhiteSpace(callId))
			{
				Phone.StopRinging();
				return;
			}
			if (Phone.GetIncomingCallAction(callId) != null)
			{
				Phone.whichPhoneCall = callId;
				Phone.ringingTimer = 0;
				Phone._phoneSoundPlaying = false;
			}
		}

		// Token: 0x06001F20 RID: 7968 RVA: 0x001661F5 File Offset: 0x001643F5
		public static void StopRinging()
		{
			Phone.whichPhoneCall = null;
			Phone.ringingTimer = 0;
			Phone.intervalsToRing = 0;
			if (Phone.IsRinging())
			{
				Game1.soundBank.GetCue("phone").Stop(AudioStopOptions.Immediate);
				Phone._phoneSoundPlaying = false;
			}
		}

		// Token: 0x06001F21 RID: 7969 RVA: 0x0016622C File Offset: 0x0016442C
		public static void HangUp()
		{
			Phone.StopRinging();
			Game1.currentLocation.playSound("openBox", null, null, SoundContext.Default);
		}

		// Token: 0x06001F22 RID: 7970 RVA: 0x00166260 File Offset: 0x00164460
		public static Action GetIncomingCallAction(string callId)
		{
			using (List<IPhoneHandler>.Enumerator enumerator = Phone.PhoneHandlers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Action showDialogue;
					if (enumerator.Current.TryHandleIncomingCall(callId, out showDialogue))
					{
						return showDialogue;
					}
				}
			}
			return null;
		}

		// Token: 0x06001F23 RID: 7971 RVA: 0x001662BC File Offset: 0x001644BC
		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			if (this.isTemporarilyInvisible)
			{
				return;
			}
			base.draw(spriteBatch, x, y, alpha);
			bool ringing = Phone.ringingTimer > 0 && Phone.ringingTimer < 600;
			Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - 64)));
			Rectangle destination = new Rectangle((int)position.X + ((ringing || this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)position.Y + ((ringing || this.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), 64, 128);
			float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 20) / 10000f) + (float)x * 1E-05f;
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			spriteBatch.Draw(itemData.GetTexture(), destination, new Rectangle?(itemData.GetSourceRect(1, new int?(base.ParentSheetIndex))), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
		}

		// Token: 0x04001316 RID: 4886
		public static List<IPhoneHandler> PhoneHandlers = new List<IPhoneHandler>
		{
			new DefaultPhoneHandler()
		};

		// Token: 0x04001317 RID: 4887
		public const int RING_DURATION = 600;

		// Token: 0x04001318 RID: 4888
		public const int RING_CYCLE_TIME = 1800;

		// Token: 0x04001319 RID: 4889
		public static Random r;

		// Token: 0x0400131A RID: 4890
		protected static bool _phoneSoundPlaying = false;

		// Token: 0x0400131B RID: 4891
		public static int ringingTimer;

		// Token: 0x0400131C RID: 4892
		public static string whichPhoneCall = null;

		// Token: 0x0400131D RID: 4893
		public static long lastRunTick = -1L;

		// Token: 0x0400131E RID: 4894
		public static long lastMinutesElapsedTick = -1L;

		// Token: 0x0400131F RID: 4895
		public static int intervalsToRing = 0;
	}
}

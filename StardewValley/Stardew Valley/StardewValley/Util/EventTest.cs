using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley.Menus;

namespace StardewValley.Util
{
	// Token: 0x0200011C RID: 284
	public class EventTest
	{
		// Token: 0x060017BD RID: 6077 RVA: 0x00111CAC File Offset: 0x0010FEAC
		public EventTest(string startingLocationName = "", int startingEventIndex = 0)
		{
			this.currentLocationIndex = 0;
			if (startingLocationName.Length > 0)
			{
				for (int i = 0; i < Game1.locations.Count; i++)
				{
					if (Game1.locations[i].Name.Equals(startingLocationName))
					{
						this.currentLocationIndex = i;
						break;
					}
				}
			}
			this.currentEventIndex = startingEventIndex;
		}

		// Token: 0x060017BE RID: 6078 RVA: 0x00111D18 File Offset: 0x0010FF18
		public EventTest(string[] whichEvents)
		{
			for (int i = 1; i < whichEvents.Length; i += 2)
			{
				this.specificEventsToDo.Add(whichEvents[i] + " " + whichEvents[i + 1]);
			}
			this.doingSpecifics = true;
			this.currentLocationIndex = -1;
		}

		// Token: 0x060017BF RID: 6079 RVA: 0x00111D70 File Offset: 0x0010FF70
		public void update()
		{
			if (!Game1.eventUp && !Game1.fadeToBlack)
			{
				if (this.currentLocationIndex < Game1.locations.Count)
				{
					if (this.doingSpecifics && this.currentLocationIndex == -1)
					{
						if (this.specificEventsToDo.Count == 0)
						{
							return;
						}
						for (int i = 0; i < Game1.locations.Count; i++)
						{
							string lastEvent = this.specificEventsToDo.Last<string>();
							string[] lastEventParts = ArgUtility.SplitBySpace(lastEvent);
							if (Game1.locations[i].Name.Equals(lastEventParts[0]))
							{
								this.currentLocationIndex = i;
								int j = -1;
								foreach (KeyValuePair<string, string> pair in Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + Game1.locations[i].Name))
								{
									j++;
									int result;
									if (int.TryParse(pair.Key.Split('/', StringSplitOptions.None)[0], out result) && result == Convert.ToInt32(lastEventParts[1]))
									{
										this.currentEventIndex = j;
										break;
									}
								}
								this.specificEventsToDo.Remove(lastEvent);
								break;
							}
						}
					}
					GameLocation k = Game1.locations[this.currentLocationIndex];
					if (k.currentEvent == null)
					{
						string locationName = k.name.Value;
						if (locationName == "Pool")
						{
							locationName = "BathHouse_Pool";
						}
						bool exists = true;
						Dictionary<string, string> data = null;
						try
						{
							data = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + locationName);
						}
						catch (Exception)
						{
							exists = false;
						}
						if (exists && this.currentEventIndex < data.Count)
						{
							KeyValuePair<string, string> entry = data.ElementAt(this.currentEventIndex);
							string key = entry.Key;
							string script = entry.Value;
							if (key.Contains('/') && !script.Equals("null"))
							{
								if (Game1.currentLocation.Name.Equals(locationName))
								{
									Game1.eventUp = true;
									Game1.currentLocation.currentEvent = new Event(script, null);
								}
								else
								{
									LocationRequest locationRequest = Game1.getLocationRequest(locationName, false);
									locationRequest.OnLoad += delegate()
									{
										Game1.currentLocation.currentEvent = new Event(script, null);
									};
									Game1.warpFarmer(locationRequest, 8, 8, Game1.player.FacingDirection);
								}
							}
						}
						this.currentEventIndex++;
						if (!exists || this.currentEventIndex >= data.Count)
						{
							this.currentEventIndex = 0;
							this.currentLocationIndex++;
						}
						if (this.doingSpecifics)
						{
							this.currentLocationIndex = -1;
							return;
						}
					}
				}
			}
			else
			{
				this.aButtonTimer -= (int)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
				if (this.aButtonTimer < 0)
				{
					this.aButtonTimer = 100;
					DialogueBox dialogueBox = Game1.activeClickableMenu as DialogueBox;
					if (dialogueBox != null)
					{
						dialogueBox.performHoverAction(Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.graphics.GraphicsDevice.Viewport.Height - 64 - Game1.random.Next(300));
						dialogueBox.receiveLeftClick(Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.graphics.GraphicsDevice.Viewport.Height - 64 - Game1.random.Next(300), true);
					}
				}
			}
		}

		// Token: 0x04000E49 RID: 3657
		private int currentEventIndex;

		// Token: 0x04000E4A RID: 3658
		private int currentLocationIndex;

		// Token: 0x04000E4B RID: 3659
		private int aButtonTimer;

		// Token: 0x04000E4C RID: 3660
		private List<string> specificEventsToDo = new List<string>();

		// Token: 0x04000E4D RID: 3661
		private bool doingSpecifics;
	}
}

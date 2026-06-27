using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;

namespace StardewValley.Locations
{
	// Token: 0x020002F4 RID: 756
	public class DwarfGate : INetObject<NetFields>
	{
		// Token: 0x1700042C RID: 1068
		// (get) Token: 0x060032D4 RID: 13012 RVA: 0x00294F97 File Offset: 0x00293197
		public NetFields NetFields { get; } = new NetFields("DwarfGate");

		// Token: 0x060032D5 RID: 13013 RVA: 0x00294FA0 File Offset: 0x002931A0
		public DwarfGate()
		{
			this.InitNetFields();
		}

		// Token: 0x060032D6 RID: 13014 RVA: 0x00295048 File Offset: 0x00293248
		public DwarfGate(VolcanoDungeon location, int gate_index, int x, int y, int seed) : this()
		{
			this.locationRef.Value = location;
			this.tilePosition.X = x;
			this.tilePosition.Y = y;
			this.gateIndex.Value = gate_index;
			Random r = Utility.CreateRandom((double)seed, 0.0, 0.0, 0.0, 0.0);
			List<Point> positions;
			if (location.possibleSwitchPositions.TryGetValue(gate_index, out positions))
			{
				int max_points = Math.Min(positions.Count, 3);
				if (gate_index > 0)
				{
					max_points = 1;
				}
				List<Point> points = new List<Point>(positions);
				Utility.Shuffle<Point>(r, points);
				int points_to_choose = r.Next(1, Math.Max(1, max_points));
				points_to_choose = Math.Min(points_to_choose, max_points);
				if (location.isMonsterLevel())
				{
					points_to_choose = max_points;
				}
				for (int i = 0; i < points_to_choose; i++)
				{
					this.switches[points[i]] = false;
				}
			}
			this.UpdateLocalStates();
			this.ApplyTiles();
		}

		// Token: 0x060032D7 RID: 13015 RVA: 0x00295140 File Offset: 0x00293340
		public virtual void InitNetFields()
		{
			this.NetFields.SetOwner(this).AddField(this.tilePosition, "tilePosition").AddField(this.locationRef.NetFields, "locationRef.NetFields").AddField(this.switches, "switches").AddField(this.pressedSwitches, "pressedSwitches").AddField(this.openEvent.NetFields, "openEvent.NetFields").AddField(this.opened, "opened").AddField(this.pressEvent.NetFields, "pressEvent.NetFields").AddField(this.gateIndex, "gateIndex");
			this.pressEvent.onEvent += this.OnPress;
			this.openEvent.onEvent += this.OpenGate;
		}

		// Token: 0x060032D8 RID: 13016 RVA: 0x0029521C File Offset: 0x0029341C
		public virtual void OnPress(Point point)
		{
			bool wasPressed;
			if (Game1.IsMasterGame && this.switches.TryGetValue(point, out wasPressed) && !wasPressed)
			{
				this.switches[point] = true;
				NetInt netInt = this.pressedSwitches;
				int value = netInt.Value;
				netInt.Value = value + 1;
			}
			if (Game1.currentLocation == this.locationRef.Value)
			{
				Game1.playSound("openBox", null);
			}
			this.localSwitches[point] = true;
			this.ApplyTiles();
		}

		// Token: 0x060032D9 RID: 13017 RVA: 0x002952A0 File Offset: 0x002934A0
		public virtual void OpenGate()
		{
			if (Game1.currentLocation == this.locationRef.Value)
			{
				Game1.playSound("cowboy_gunload", null);
			}
			if (Game1.IsMasterGame)
			{
				if (this.gateIndex.Value == -1 && !Game1.MasterPlayer.hasOrWillReceiveMail("volcanoShortcutUnlocked"))
				{
					Game1.addMailForTomorrow("volcanoShortcutUnlocked", true, false);
				}
				this.opened.Value = true;
			}
			this.localOpened = true;
			this.ApplyTiles();
		}

		// Token: 0x060032DA RID: 13018 RVA: 0x0029531E File Offset: 0x0029351E
		public virtual void ResetLocalState()
		{
			this.UpdateLocalStates();
			this.ApplyTiles();
		}

		// Token: 0x060032DB RID: 13019 RVA: 0x0029532C File Offset: 0x0029352C
		public virtual void UpdateLocalStates()
		{
			this.localOpened = this.opened.Value;
			this.localPressedSwitches = this.pressedSwitches.Value;
			foreach (Point key in this.switches.Keys)
			{
				this.localSwitches[key] = this.switches[key];
			}
		}

		// Token: 0x060032DC RID: 13020 RVA: 0x002953BC File Offset: 0x002935BC
		public virtual void Draw(SpriteBatch b)
		{
			if (!this.localOpened)
			{
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)this.tilePosition.X, (float)this.tilePosition.Y) * 64f + new Vector2(1f, -5f) * 4f), new Rectangle?(new Rectangle(178, 189, 14, 34)), Color.White, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, (float)((this.tilePosition.Y + 2) * 64) / 10000f);
			}
		}

		// Token: 0x060032DD RID: 13021 RVA: 0x0029547C File Offset: 0x0029367C
		public virtual void UpdateWhenCurrentLocation(GameTime time, GameLocation location)
		{
			this.openEvent.Poll();
			this.pressEvent.Poll();
			if (this.localPressedSwitches != this.pressedSwitches.Value)
			{
				this.localPressedSwitches = this.pressedSwitches.Value;
				this.ApplyTiles();
			}
			if (!this.localOpened && this.opened.Value)
			{
				this.localOpened = true;
				this.ApplyTiles();
			}
			foreach (Point key in this.switches.Keys)
			{
				if (this.switches[key] && !this.localSwitches[key])
				{
					this.localSwitches[key] = true;
					this.ApplyTiles();
				}
			}
		}

		// Token: 0x060032DE RID: 13022 RVA: 0x00295564 File Offset: 0x00293764
		public virtual void ApplyTiles()
		{
			int total_switches = 0;
			int local_pressed_switches = 0;
			int pressed_switches = 0;
			foreach (Point point in this.localSwitches.Keys)
			{
				total_switches++;
				if (this.switches[point])
				{
					pressed_switches++;
				}
				if (this.localSwitches[point])
				{
					local_pressed_switches++;
					this.locationRef.Value.setMapTile(point.X, point.Y, VolcanoDungeon.GetTileIndex(1, 31), "Back", "dungeon", null, true).Properties.Remove("TouchAction");
				}
				else
				{
					this.locationRef.Value.setMapTile(point.X, point.Y, VolcanoDungeon.GetTileIndex(0, 31), "Back", "dungeon", null, true).Properties["TouchAction"] = "DwarfSwitch";
				}
			}
			switch (total_switches)
			{
			case 1:
				this.locationRef.Value.setMapTile(this.tilePosition.X - 1, this.tilePosition.Y, VolcanoDungeon.GetTileIndex(10 + local_pressed_switches, 23), "Buildings", "dungeon", null, true);
				break;
			case 2:
				this.locationRef.Value.setMapTile(this.tilePosition.X - 1, this.tilePosition.Y, VolcanoDungeon.GetTileIndex(12 + local_pressed_switches, 23), "Buildings", "dungeon", null, true);
				break;
			case 3:
				this.locationRef.Value.setMapTile(this.tilePosition.X - 1, this.tilePosition.Y, VolcanoDungeon.GetTileIndex(10 + local_pressed_switches, 22), "Buildings", "dungeon", null, true);
				break;
			}
			if (!this.triggeredOpen && pressed_switches >= total_switches)
			{
				this.triggeredOpen = true;
				if (Game1.IsMasterGame)
				{
					DelayedAction.functionAfterDelay(new Action(this.openEvent.Fire), 500);
				}
			}
			if (this.localOpened)
			{
				this.locationRef.Value.removeTile(this.tilePosition.X, this.tilePosition.Y + 1, "Buildings");
				return;
			}
			this.locationRef.Value.setMapTile(this.tilePosition.X, this.tilePosition.Y + 1, 0, "Buildings", "dungeon", null, true);
		}

		// Token: 0x040021E3 RID: 8675
		public NetPoint tilePosition = new NetPoint();

		// Token: 0x040021E5 RID: 8677
		public NetLocationRef locationRef = new NetLocationRef();

		// Token: 0x040021E6 RID: 8678
		public bool triggeredOpen;

		// Token: 0x040021E7 RID: 8679
		public NetPointDictionary<bool, NetBool> switches = new NetPointDictionary<bool, NetBool>
		{
			InterpolationWait = false
		};

		// Token: 0x040021E8 RID: 8680
		public Dictionary<Point, bool> localSwitches = new Dictionary<Point, bool>();

		// Token: 0x040021E9 RID: 8681
		public NetBool opened = new NetBool(false);

		// Token: 0x040021EA RID: 8682
		public bool localOpened;

		// Token: 0x040021EB RID: 8683
		public NetInt pressedSwitches = new NetInt(0)
		{
			InterpolationWait = false
		};

		// Token: 0x040021EC RID: 8684
		public int localPressedSwitches;

		// Token: 0x040021ED RID: 8685
		public NetInt gateIndex = new NetInt(0);

		// Token: 0x040021EE RID: 8686
		public NetEvent0 openEvent = new NetEvent0(false);

		// Token: 0x040021EF RID: 8687
		public NetEvent1Field<Point, NetPoint> pressEvent = new NetEvent1Field<Point, NetPoint>
		{
			InterpolationWait = false
		};
	}
}

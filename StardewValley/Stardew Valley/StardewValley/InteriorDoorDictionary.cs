using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Network;

namespace StardewValley
{
	// Token: 0x020000D3 RID: 211
	public class InteriorDoorDictionary : NetPointDictionary<bool, InteriorDoor>
	{
		// Token: 0x170001DE RID: 478
		// (get) Token: 0x06001066 RID: 4198 RVA: 0x000C6593 File Offset: 0x000C4793
		public InteriorDoorDictionary.DoorCollection Doors
		{
			get
			{
				return new InteriorDoorDictionary.DoorCollection(this);
			}
		}

		// Token: 0x06001067 RID: 4199 RVA: 0x000C659B File Offset: 0x000C479B
		public InteriorDoorDictionary(GameLocation location)
		{
			this.location = location;
		}

		// Token: 0x06001068 RID: 4200 RVA: 0x000C65AA File Offset: 0x000C47AA
		protected override void setFieldValue(InteriorDoor door, Point position, bool open)
		{
			door.Location = this.location;
			door.Position = position;
			base.setFieldValue(door, position, open);
		}

		// Token: 0x06001069 RID: 4201 RVA: 0x000C65C8 File Offset: 0x000C47C8
		public void ResetSharedState()
		{
			if (this.location.isOutdoors.Value)
			{
				return;
			}
			foreach (Point tile in InteriorDoorDictionary.GetDoorTilesFromMapProperty(this.location))
			{
				base[tile] = false;
			}
		}

		// Token: 0x0600106A RID: 4202 RVA: 0x000C6630 File Offset: 0x000C4830
		public void ResetLocalState()
		{
			if (this.location.isOutdoors.Value)
			{
				return;
			}
			foreach (Point doorPoint in InteriorDoorDictionary.GetDoorTilesFromMapProperty(this.location))
			{
				if (base.ContainsKey(doorPoint))
				{
					InteriorDoor interiorDoor = base.FieldDict[doorPoint];
					interiorDoor.Location = this.location;
					interiorDoor.Position = doorPoint;
					interiorDoor.ResetLocalState();
				}
			}
		}

		// Token: 0x0600106B RID: 4203 RVA: 0x000C66BC File Offset: 0x000C48BC
		public static IEnumerable<Point> GetDoorTilesFromMapProperty(GameLocation location)
		{
			string[] fields = location.GetMapPropertySplitBySpaces("Doors");
			for (int i = 0; i < fields.Length; i += 4)
			{
				Point tile;
				string error;
				if (ArgUtility.TryGetPoint(fields, i, out tile, out error, "Point tile"))
				{
					yield return tile;
				}
				else
				{
					location.LogMapPropertyError("Doors", fields, error, ' ');
				}
			}
			yield break;
		}

		// Token: 0x0600106C RID: 4204 RVA: 0x000C66CC File Offset: 0x000C48CC
		public void MakeMapModifications()
		{
			foreach (InteriorDoor interiorDoor in this.Doors)
			{
				interiorDoor.ApplyMapModifications();
			}
		}

		// Token: 0x0600106D RID: 4205 RVA: 0x000C6720 File Offset: 0x000C4920
		public void CleanUpLocalState()
		{
			foreach (InteriorDoor interiorDoor in this.Doors)
			{
				interiorDoor.CleanUpLocalState();
			}
		}

		// Token: 0x0600106E RID: 4206 RVA: 0x000C6774 File Offset: 0x000C4974
		public void Update(GameTime time)
		{
			foreach (InteriorDoor interiorDoor in this.Doors)
			{
				interiorDoor.Update(time);
			}
		}

		// Token: 0x0600106F RID: 4207 RVA: 0x000C67C8 File Offset: 0x000C49C8
		public void Draw(SpriteBatch b)
		{
			foreach (InteriorDoor interiorDoor in this.Doors)
			{
				interiorDoor.Draw(b);
			}
		}

		// Token: 0x040009FC RID: 2556
		private GameLocation location;

		// Token: 0x020004A6 RID: 1190
		public struct DoorCollection : IEnumerable<InteriorDoor>, IEnumerable
		{
			// Token: 0x06003ED2 RID: 16082 RVA: 0x002FB427 File Offset: 0x002F9627
			public DoorCollection(InteriorDoorDictionary dict)
			{
				this._dict = dict;
			}

			// Token: 0x06003ED3 RID: 16083 RVA: 0x002FB430 File Offset: 0x002F9630
			public InteriorDoorDictionary.DoorCollection.Enumerator GetEnumerator()
			{
				return new InteriorDoorDictionary.DoorCollection.Enumerator(this._dict);
			}

			// Token: 0x06003ED4 RID: 16084 RVA: 0x002FB43D File Offset: 0x002F963D
			IEnumerator<InteriorDoor> IEnumerable<InteriorDoor>.GetEnumerator()
			{
				return new InteriorDoorDictionary.DoorCollection.Enumerator(this._dict);
			}

			// Token: 0x06003ED5 RID: 16085 RVA: 0x002FB44F File Offset: 0x002F964F
			IEnumerator IEnumerable.GetEnumerator()
			{
				return new InteriorDoorDictionary.DoorCollection.Enumerator(this._dict);
			}

			// Token: 0x040028E3 RID: 10467
			private InteriorDoorDictionary _dict;

			// Token: 0x0200074B RID: 1867
			public struct Enumerator : IEnumerator<InteriorDoor>, IEnumerator, IDisposable
			{
				// Token: 0x06004772 RID: 18290 RVA: 0x003258E2 File Offset: 0x00323AE2
				public Enumerator(InteriorDoorDictionary dict)
				{
					this._dict = dict;
					this._enumerator = this._dict.FieldDict.GetEnumerator();
					this._current = null;
					this._done = false;
				}

				// Token: 0x06004773 RID: 18291 RVA: 0x00325910 File Offset: 0x00323B10
				public bool MoveNext()
				{
					if (!this._enumerator.MoveNext())
					{
						this._done = true;
						this._current = null;
						return false;
					}
					KeyValuePair<Point, InteriorDoor> pair = this._enumerator.Current;
					this._current = pair.Value;
					this._current.Location = this._dict.location;
					this._current.Position = pair.Key;
					return true;
				}

				// Token: 0x17000532 RID: 1330
				// (get) Token: 0x06004774 RID: 18292 RVA: 0x0032597E File Offset: 0x00323B7E
				public InteriorDoor Current
				{
					get
					{
						return this._current;
					}
				}

				// Token: 0x06004775 RID: 18293 RVA: 0x00325986 File Offset: 0x00323B86
				public void Dispose()
				{
				}

				// Token: 0x17000533 RID: 1331
				// (get) Token: 0x06004776 RID: 18294 RVA: 0x00325988 File Offset: 0x00323B88
				object IEnumerator.Current
				{
					get
					{
						if (this._done)
						{
							throw new InvalidOperationException();
						}
						return this._current;
					}
				}

				// Token: 0x06004777 RID: 18295 RVA: 0x0032599E File Offset: 0x00323B9E
				void IEnumerator.Reset()
				{
					this._enumerator = this._dict.FieldDict.GetEnumerator();
					this._current = null;
					this._done = false;
				}

				// Token: 0x0400318F RID: 12687
				private readonly InteriorDoorDictionary _dict;

				// Token: 0x04003190 RID: 12688
				private Dictionary<Point, InteriorDoor>.Enumerator _enumerator;

				// Token: 0x04003191 RID: 12689
				private InteriorDoor _current;

				// Token: 0x04003192 RID: 12690
				private bool _done;
			}
		}
	}
}

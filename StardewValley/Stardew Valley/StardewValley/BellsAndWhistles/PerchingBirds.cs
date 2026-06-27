using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;

namespace StardewValley.BellsAndWhistles
{
	// Token: 0x020003A0 RID: 928
	public class PerchingBirds
	{
		// Token: 0x060038A4 RID: 14500 RVA: 0x002CE610 File Offset: 0x002CC810
		public PerchingBirds(Texture2D bird_texture, int flap_frames, int width, int height, Vector2 origin, Point[] perch_locations, Point[] roost_locations)
		{
			this._birdSheet = bird_texture;
			this._birdWidth = width;
			this._birdHeight = height;
			this._birdOrigin = origin;
			this._flapFrames = flap_frames;
			this._birdPointOccupancy = new Dictionary<Point, Bird>();
			this._birdLocations = perch_locations;
			this._birdRoostLocations = roost_locations;
			this.ResetLocalState();
		}

		// Token: 0x060038A5 RID: 14501 RVA: 0x002CE68D File Offset: 0x002CC88D
		public int GetBirdWidth()
		{
			return this._birdWidth;
		}

		// Token: 0x060038A6 RID: 14502 RVA: 0x002CE695 File Offset: 0x002CC895
		public int GetBirdHeight()
		{
			return this._birdHeight;
		}

		// Token: 0x060038A7 RID: 14503 RVA: 0x002CE69D File Offset: 0x002CC89D
		public Vector2 GetBirdOrigin()
		{
			return this._birdOrigin;
		}

		// Token: 0x060038A8 RID: 14504 RVA: 0x002CE6A5 File Offset: 0x002CC8A5
		public Texture2D GetTexture()
		{
			return this._birdSheet;
		}

		// Token: 0x060038A9 RID: 14505 RVA: 0x002CE6B0 File Offset: 0x002CC8B0
		public Point GetFreeBirdPoint(Bird bird = null, int clearance = 200)
		{
			List<Point> points = new List<Point>();
			foreach (Point point in this.GetCurrentBirdLocationList())
			{
				if (this._birdPointOccupancy[point] == null)
				{
					bool fail = false;
					if (bird != null)
					{
						foreach (Farmer farmer in Game1.currentLocation.farmers)
						{
							if (Utility.distance(farmer.position.X, (float)(point.X * 64) + 32f, farmer.position.Y, (float)(point.Y * 64) + 32f) < 200f)
							{
								fail = true;
							}
						}
					}
					if (!fail)
					{
						points.Add(point);
					}
				}
			}
			return Game1.random.ChooseFrom(points);
		}

		// Token: 0x060038AA RID: 14506 RVA: 0x002CE7A0 File Offset: 0x002CC9A0
		public void ReserveBirdPoint(Bird bird, Point point)
		{
			if (this._birdPointOccupancy.ContainsKey(bird.endPosition))
			{
				this._birdPointOccupancy[bird.endPosition] = null;
			}
			if (this._birdPointOccupancy.ContainsKey(point))
			{
				this._birdPointOccupancy[point] = bird;
			}
		}

		// Token: 0x060038AB RID: 14507 RVA: 0x002CE7ED File Offset: 0x002CC9ED
		public bool ShouldBirdsRoost()
		{
			return this.roosting;
		}

		// Token: 0x060038AC RID: 14508 RVA: 0x002CE7F5 File Offset: 0x002CC9F5
		public Point[] GetCurrentBirdLocationList()
		{
			if (this.ShouldBirdsRoost())
			{
				return this._birdRoostLocations;
			}
			return this._birdLocations;
		}

		// Token: 0x060038AD RID: 14509 RVA: 0x002CE80C File Offset: 0x002CCA0C
		public virtual void Update(GameTime time)
		{
			for (int i = 0; i < this._birds.Count; i++)
			{
				this._birds[i].Update(time);
			}
		}

		// Token: 0x060038AE RID: 14510 RVA: 0x002CE844 File Offset: 0x002CCA44
		public virtual void Draw(SpriteBatch b)
		{
			b.End();
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
			for (int i = 0; i < this._birds.Count; i++)
			{
				this._birds[i].Draw(b);
			}
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
		}

		// Token: 0x060038AF RID: 14511 RVA: 0x002CE8C0 File Offset: 0x002CCAC0
		public virtual void ResetLocalState()
		{
			this._birds.Clear();
			this._birdPointOccupancy = new Dictionary<Point, Bird>();
			foreach (Point point in this._birdLocations)
			{
				this._birdPointOccupancy[point] = null;
			}
			foreach (Point point2 in this._birdRoostLocations)
			{
				this._birdPointOccupancy[point2] = null;
			}
		}

		// Token: 0x060038B0 RID: 14512 RVA: 0x002CE938 File Offset: 0x002CCB38
		public virtual void AddBird(int bird_type)
		{
			Bird bird = new Bird(this.GetFreeBirdPoint(null, 200), this, bird_type, this._flapFrames);
			this._birds.Add(bird);
			this.ReserveBirdPoint(bird, bird.endPosition);
		}

		// Token: 0x04002522 RID: 9506
		public const int BIRD_STARTLE_DISTANCE = 200;

		// Token: 0x04002523 RID: 9507
		[XmlIgnore]
		public List<Bird> _birds = new List<Bird>();

		// Token: 0x04002524 RID: 9508
		[XmlIgnore]
		protected Point[] _birdLocations;

		// Token: 0x04002525 RID: 9509
		protected Point[] _birdRoostLocations;

		// Token: 0x04002526 RID: 9510
		[XmlIgnore]
		public Dictionary<Point, Bird> _birdPointOccupancy;

		// Token: 0x04002527 RID: 9511
		public bool roosting;

		// Token: 0x04002528 RID: 9512
		protected Texture2D _birdSheet;

		// Token: 0x04002529 RID: 9513
		protected int _birdWidth;

		// Token: 0x0400252A RID: 9514
		protected int _birdHeight;

		// Token: 0x0400252B RID: 9515
		protected int _flapFrames = 2;

		// Token: 0x0400252C RID: 9516
		protected Vector2 _birdOrigin;

		// Token: 0x0400252D RID: 9517
		public int peckDuration = 5;

		// Token: 0x0400252E RID: 9518
		public float birdSpeed = 5f;
	}
}

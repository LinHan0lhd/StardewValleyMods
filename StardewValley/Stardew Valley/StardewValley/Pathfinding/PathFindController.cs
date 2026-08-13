using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using xTile.Tiles;

namespace StardewValley.Pathfinding
{
	// Token: 0x0200019C RID: 412
	[InstanceStatics]
	public class PathFindController
	{
		// Token: 0x06001D4B RID: 7499 RVA: 0x0014FA60 File Offset: 0x0014DC60
		public PathFindController(Character c, GameLocation location, Point endPoint, int finalFacingDirection) : this(c, location, new PathFindController.isAtEnd(PathFindController.isAtEndPoint), finalFacingDirection, null, 10000, endPoint, true)
		{
		}

		// Token: 0x06001D4C RID: 7500 RVA: 0x0014FA8C File Offset: 0x0014DC8C
		public PathFindController(Character c, GameLocation location, Point endPoint, int finalFacingDirection, PathFindController.endBehavior endBehaviorFunction) : this(c, location, new PathFindController.isAtEnd(PathFindController.isAtEndPoint), finalFacingDirection, null, 10000, endPoint, true)
		{
			this.endPoint = endPoint;
			this.endBehaviorFunction = endBehaviorFunction;
		}

		// Token: 0x06001D4D RID: 7501 RVA: 0x0014FAC8 File Offset: 0x0014DCC8
		public PathFindController(Character c, GameLocation location, Point endPoint, int finalFacingDirection, PathFindController.endBehavior endBehaviorFunction, int limit) : this(c, location, new PathFindController.isAtEnd(PathFindController.isAtEndPoint), finalFacingDirection, null, limit, endPoint, true)
		{
			this.endPoint = endPoint;
			this.endBehaviorFunction = endBehaviorFunction;
		}

		// Token: 0x06001D4E RID: 7502 RVA: 0x0014FB00 File Offset: 0x0014DD00
		public PathFindController(Character c, GameLocation location, Point endPoint, int finalFacingDirection, bool clearMarriageDialogues = true) : this(c, location, new PathFindController.isAtEnd(PathFindController.isAtEndPoint), finalFacingDirection, null, 10000, endPoint, clearMarriageDialogues)
		{
		}

		// Token: 0x06001D4F RID: 7503 RVA: 0x0014FB2C File Offset: 0x0014DD2C
		public static bool isAtEndPoint(PathNode currentNode, Point endPoint, GameLocation location, Character c)
		{
			return currentNode.x == endPoint.X && currentNode.y == endPoint.Y;
		}

		// Token: 0x06001D50 RID: 7504 RVA: 0x0014FB4C File Offset: 0x0014DD4C
		public PathFindController(Stack<Point> pathToEndPoint, GameLocation location, Character c, Point endPoint)
		{
			this.pathToEndPoint = pathToEndPoint;
			this.location = location;
			this.character = c;
			this.endPoint = endPoint;
		}

		// Token: 0x06001D51 RID: 7505 RVA: 0x0014FB71 File Offset: 0x0014DD71
		public PathFindController(Stack<Point> pathToEndPoint, Character c, GameLocation l)
		{
			this.pathToEndPoint = pathToEndPoint;
			this.character = c;
			this.location = l;
			this.NPCSchedule = true;
		}

		// Token: 0x06001D52 RID: 7506 RVA: 0x0014FB98 File Offset: 0x0014DD98
		public PathFindController(Character c, GameLocation location, PathFindController.isAtEnd endFunction, int finalFacingDirection, PathFindController.endBehavior endBehaviorFunction, int limit, Point endPoint, bool clearMarriageDialogues = true)
		{
			this.character = c;
			NPC npc = c as NPC;
			if (npc != null && npc.CurrentDialogue.Count > 0 && npc.CurrentDialogue.Peek().removeOnNextMove)
			{
				npc.CurrentDialogue.Pop();
			}
			if (npc != null && clearMarriageDialogues)
			{
				if (npc.currentMarriageDialogue.Count > 0)
				{
					npc.currentMarriageDialogue.Clear();
				}
				npc.shouldSayMarriageDialogue.Value = false;
			}
			this.location = location;
			this.endBehaviorFunction = endBehaviorFunction;
			if (endPoint == Point.Zero)
			{
				endPoint = c.TilePoint;
			}
			this.finalFacingDirection = finalFacingDirection;
			if (!(this.character is NPC) && !this.isPlayerPresent() && endFunction == new PathFindController.isAtEnd(PathFindController.isAtEndPoint) && endPoint.X > 0 && endPoint.Y > 0)
			{
				this.character.Position = new Vector2((float)(endPoint.X * 64), (float)(endPoint.Y * 64 - 32));
				return;
			}
			this.pathToEndPoint = PathFindController.findPath(c.TilePoint, endPoint, endFunction, location, this.character, limit);
		}

		// Token: 0x06001D53 RID: 7507 RVA: 0x0014FCC7 File Offset: 0x0014DEC7
		public bool isPlayerPresent()
		{
			return this.location.farmers.Any();
		}

		// Token: 0x06001D54 RID: 7508 RVA: 0x0014FCDC File Offset: 0x0014DEDC
		public virtual bool update(GameTime time)
		{
			if (this.pathToEndPoint == null || this.pathToEndPoint.Count == 0)
			{
				return true;
			}
			if (!this.NPCSchedule && !this.isPlayerPresent() && this.endPoint.X > 0 && this.endPoint.Y > 0)
			{
				this.character.Position = new Vector2((float)(this.endPoint.X * 64), (float)(this.endPoint.Y * 64 - 32));
				return true;
			}
			if (Game1.activeClickableMenu == null || Game1.IsMultiplayer)
			{
				this.timerSinceLastCheckPoint += time.ElapsedGameTime.Milliseconds;
				Vector2 position = this.character.Position;
				this.moveCharacter(time);
				if (this.character.Position.Equals(position))
				{
					this.pausedTimer += time.ElapsedGameTime.Milliseconds;
				}
				else
				{
					this.pausedTimer = 0;
				}
				if (!this.NPCSchedule && this.pausedTimer > 5000)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001D55 RID: 7509 RVA: 0x0014FDF0 File Offset: 0x0014DFF0
		public static Stack<Point> findPath(Point startPoint, Point endPoint, PathFindController.isAtEnd endPointFunction, GameLocation location, Character character, int limit)
		{
			if (Interlocked.Increment(ref PathFindController._counter) != 1)
			{
				throw new Exception();
			}
			Stack<Point> result;
			try
			{
				FarmAnimal animal = character as FarmAnimal;
				bool ignore_obstructions = animal != null && animal.CanSwim() && animal.isSwimming.Value;
				PathFindController._openList.Clear();
				PathFindController._closedList.Clear();
				PriorityQueue openList = PathFindController._openList;
				HashSet<int> closedList = PathFindController._closedList;
				int iterations = 0;
				openList.Enqueue(new PathNode(startPoint.X, startPoint.Y, 0, null), Math.Abs(endPoint.X - startPoint.X) + Math.Abs(endPoint.Y - startPoint.Y));
				int layerWidth = location.map.Layers[0].LayerWidth;
				int layerHeight = location.map.Layers[0].LayerHeight;
				while (!openList.IsEmpty())
				{
					PathNode currentNode = openList.Dequeue();
					if (endPointFunction(currentNode, endPoint, location, character))
					{
						return PathFindController.reconstructPath(currentNode);
					}
					closedList.Add(currentNode.id);
					int ng = (int)(currentNode.g + 1);
					for (int i = 0; i < 4; i++)
					{
						int nx = currentNode.x + (int)PathFindController.Directions[i, 0];
						int ny = currentNode.y + (int)PathFindController.Directions[i, 1];
						int nid = PathNode.ComputeHash(nx, ny);
						if (!closedList.Contains(nid))
						{
							if ((nx != endPoint.X || ny != endPoint.Y) && (nx < 0 || ny < 0 || nx >= layerWidth || ny >= layerHeight))
							{
								closedList.Add(nid);
							}
							else
							{
								PathNode neighbor = new PathNode(nx, ny, currentNode);
								neighbor.g = currentNode.g + 1;
								if (!ignore_obstructions && location.isCollidingPosition(new Rectangle(neighbor.x * 64 + 1, neighbor.y * 64 + 1, 62, 62), Game1.viewport, character is Farmer, 0, false, character, true, false, false, false))
								{
									closedList.Add(nid);
								}
								else
								{
									int f = ng + (Math.Abs(endPoint.X - nx) + Math.Abs(endPoint.Y - ny));
									closedList.Add(nid);
									openList.Enqueue(neighbor, f);
								}
							}
						}
					}
					iterations++;
					if (iterations >= limit)
					{
						return null;
					}
				}
				result = null;
			}
			finally
			{
				if (Interlocked.Decrement(ref PathFindController._counter) != 0)
				{
					throw new Exception();
				}
			}
			return result;
		}

		// Token: 0x06001D56 RID: 7510 RVA: 0x00150090 File Offset: 0x0014E290
		public static Stack<Point> reconstructPath(PathNode finalNode)
		{
			Stack<Point> path = new Stack<Point>();
			path.Push(new Point(finalNode.x, finalNode.y));
			for (PathNode walk = finalNode.parent; walk != null; walk = walk.parent)
			{
				path.Push(new Point(walk.x, walk.y));
			}
			return path;
		}

		// Token: 0x06001D57 RID: 7511 RVA: 0x001500E8 File Offset: 0x0014E2E8
		protected virtual void moveCharacter(GameTime time)
		{
			Point peek = this.pathToEndPoint.Peek();
			Rectangle targetTile = new Rectangle(peek.X * 64, peek.Y * 64, 64, 64);
			targetTile.Inflate(-2, 0);
			Rectangle bbox = this.character.GetBoundingBox();
			if ((targetTile.Contains(bbox) || (bbox.Width > targetTile.Width && targetTile.Contains(bbox.Center))) && targetTile.Bottom - bbox.Bottom >= 2)
			{
				this.timerSinceLastCheckPoint = 0;
				this.pathToEndPoint.Pop();
				this.character.stopWithoutChangingFrame();
				if (this.pathToEndPoint.Count == 0)
				{
					this.character.Halt();
					if (this.finalFacingDirection != -1)
					{
						this.character.faceDirection(this.finalFacingDirection);
					}
					if (this.NPCSchedule)
					{
						NPC npc = this.character as NPC;
						npc.DirectionsToNewLocation = null;
						npc.endOfRouteMessage.Value = npc.nextEndOfRouteMessage;
					}
					PathFindController.endBehavior endBehavior = this.endBehaviorFunction;
					if (endBehavior == null)
					{
						return;
					}
					endBehavior(this.character, this.location);
					return;
				}
			}
			else
			{
				Farmer farmer = this.character as Farmer;
				if (farmer != null)
				{
					farmer.movementDirections.Clear();
				}
				else if (!(this.location is MovieTheater))
				{
					string name = this.character.Name;
					for (int i = 0; i < this.location.characters.Count; i++)
					{
						NPC c = this.location.characters[i];
						if (!c.Equals(this.character) && c.GetBoundingBox().Intersects(bbox) && c.isMoving() && string.Compare(c.Name, name, StringComparison.Ordinal) < 0)
						{
							this.character.Halt();
							return;
						}
					}
				}
				if (bbox.Left < targetTile.Left && bbox.Right < targetTile.Right)
				{
					this.character.SetMovingRight(true);
				}
				else if (bbox.Right > targetTile.Right && bbox.Left > targetTile.Left)
				{
					this.character.SetMovingLeft(true);
				}
				else if (bbox.Top <= targetTile.Top)
				{
					this.character.SetMovingDown(true);
				}
				else if (bbox.Bottom >= targetTile.Bottom - 2)
				{
					this.character.SetMovingUp(true);
				}
				this.character.MovePosition(time, Game1.viewport, this.location);
				if (this.nonDestructivePathing)
				{
					if (targetTile.Intersects(this.character.nextPosition(this.character.FacingDirection)))
					{
						Vector2 next_position = this.character.nextPositionVector2();
						Object next_tile_object = this.location.getObjectAt((int)next_position.X, (int)next_position.Y, false);
						if (next_tile_object != null)
						{
							Fence fence = next_tile_object as Fence;
							if (fence != null && fence.isGate.Value)
							{
								fence.toggleGate(true, false, null);
							}
							else if (!next_tile_object.isPassable())
							{
								this.character.Halt();
								this.character.controller = null;
								return;
							}
						}
					}
					this.handleWarps(this.character.nextPosition(this.character.getDirection()));
					return;
				}
				if (this.NPCSchedule)
				{
					this.handleWarps(this.character.nextPosition(this.character.getDirection()));
				}
			}
		}

		// Token: 0x06001D58 RID: 7512 RVA: 0x00150460 File Offset: 0x0014E660
		public void handleWarps(Rectangle position)
		{
			Warp w = this.location.isCollidingWithWarpOrDoor(position, this.character);
			if (w != null)
			{
				if (w.TargetName == "Trailer" && Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
				{
					w = new Warp(w.X, w.Y, "Trailer_Big", 13, 24, false, false);
				}
				NPC spouse = this.character as NPC;
				if (spouse != null && spouse.isMarried() && spouse.followSchedule)
				{
					GameLocation gameLocation = this.location;
					if (!(gameLocation is FarmHouse))
					{
						if (gameLocation is BusStop)
						{
							if (w.X <= 9)
							{
								GameLocation home = spouse.getHome();
								Point homeEntry = ((FarmHouse)home).getEntryLocation();
								w = new Warp(w.X, w.Y, home.name.Value, homeEntry.X, homeEntry.Y, false, false);
							}
						}
					}
					else
					{
						w = new Warp(w.X, w.Y, "BusStop", 10, 23, false, false);
					}
					if (spouse.temporaryController != null && spouse.controller != null)
					{
						spouse.controller.location = Game1.RequireLocation(w.TargetName, false);
					}
				}
				string targetLocationName = w.TargetName;
				using (IEnumerator<string> enumerator = Game1.netWorldState.Value.ActivePassiveFestivals.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						PassiveFestivalData data;
						string newName;
						if (Utility.TryGetPassiveFestivalData(enumerator.Current, out data) && data.MapReplacements != null && data.MapReplacements.TryGetValue(targetLocationName, out newName))
						{
							targetLocationName = newName;
							break;
						}
					}
				}
				NPC npc = this.character as NPC;
				if (npc != null && (w.TargetName == "FarmHouse" || w.TargetName == "Cabin") && npc.isMarried() && npc.getSpouse() != null)
				{
					this.location = Utility.getHomeOfFarmer(npc.getSpouse());
					Point entryPoint = ((FarmHouse)this.location).getEntryLocation();
					w = new Warp(w.X, w.Y, this.location.name.Value, entryPoint.X, entryPoint.Y, false, false);
					if (npc.temporaryController != null && npc.controller != null)
					{
						npc.controller.location = this.location;
					}
					Game1.warpCharacter(npc, this.location, new Vector2((float)w.TargetX, (float)w.TargetY));
				}
				else
				{
					this.location = Game1.RequireLocation(targetLocationName, false);
					Game1.warpCharacter(this.character as NPC, w.TargetName, new Vector2((float)w.TargetX, (float)w.TargetY));
				}
				if (this.isPlayerPresent() && this.location.doors.ContainsKey(new Point(w.X, w.Y)))
				{
					this.location.playSound("doorClose", new Vector2?(new Vector2((float)w.X, (float)w.Y)), null, SoundContext.NPC);
				}
				if (this.isPlayerPresent() && this.location.doors.ContainsKey(new Point(w.TargetX, w.TargetY - 1)))
				{
					this.location.playSound("doorClose", new Vector2?(new Vector2((float)w.TargetX, (float)w.TargetY)), null, SoundContext.NPC);
				}
				if (this.pathToEndPoint.Count > 0)
				{
					this.pathToEndPoint.Pop();
				}
				Point tile = this.character.TilePoint;
				while (this.pathToEndPoint.Count > 0 && (Math.Abs(this.pathToEndPoint.Peek().X - tile.X) > 1 || Math.Abs(this.pathToEndPoint.Peek().Y - tile.Y) > 1))
				{
					this.pathToEndPoint.Pop();
				}
				// After warping, regenerate path to target location if needed
				// This handles the case where NPC is restored from a save and needs to continue walking
				if (this.NPCSchedule)
				{
					NPC npc2 = this.character as NPC;
					if (npc2 != null && npc2.DirectionsToNewLocation != null)
					{
						Point targetTile = npc2.DirectionsToNewLocation.targetTile;
						// Always try to regenerate path after warp if targetTile is valid
						// This ensures NPC can continue walking to the target location
						if (targetTile != Point.Zero && targetTile != tile)
						{
							Stack<Point> newPath = PathFindController.findPathForNPCSchedules(tile, targetTile, this.location, 30000, npc2);
							if (newPath != null && newPath.Count > 0)
							{
								this.pathToEndPoint = newPath;
							}
						}
					}
				}
			}
		}

		// Token: 0x06001D59 RID: 7513 RVA: 0x00150878 File Offset: 0x0014EA78
		[Obsolete("Use findPathForNPCSchedules overload with 'npc' parameter.")]
		public static Stack<Point> findPathForNPCSchedules(Point startPoint, Point endPoint, GameLocation location, int limit)
		{
			return PathFindController.findPathForNPCSchedules(startPoint, endPoint, location, limit, null);
		}

		// Token: 0x06001D5A RID: 7514 RVA: 0x00150884 File Offset: 0x0014EA84
		public static Stack<Point> findPathForNPCSchedules(Point startPoint, Point endPoint, GameLocation location, int limit, Character npc)
		{
			PriorityQueue openList = new PriorityQueue();
			HashSet<int> closedList = new HashSet<int>();
			int iterations = 0;
			openList.Enqueue(new PathNode(startPoint.X, startPoint.Y, 0, null), Math.Abs(endPoint.X - startPoint.X) + Math.Abs(endPoint.Y - startPoint.Y));
			PathNode previousNode = (PathNode)openList.Peek();
			int layerWidth = location.map.Layers[0].LayerWidth;
			int layerHeight = location.map.Layers[0].LayerHeight;
			while (!openList.IsEmpty())
			{
				PathNode currentNode = openList.Dequeue();
				if (currentNode.x == endPoint.X && currentNode.y == endPoint.Y)
				{
					return PathFindController.reconstructPath(currentNode);
				}
				closedList.Add(currentNode.id);
				for (int i = 0; i < 4; i++)
				{
					int neighbor_tile_x = currentNode.x + (int)PathFindController.Directions[i, 0];
					int neighbor_tile_y = currentNode.y + (int)PathFindController.Directions[i, 1];
					int nid = PathNode.ComputeHash(neighbor_tile_x, neighbor_tile_y);
					if (!closedList.Contains(nid))
					{
						PathNode neighbor = new PathNode(neighbor_tile_x, neighbor_tile_y, currentNode);
						neighbor.g = currentNode.g + 1;
						if ((neighbor.x == endPoint.X && neighbor.y == endPoint.Y) || (neighbor.x >= 0 && neighbor.y >= 0 && neighbor.x < layerWidth && neighbor.y < layerHeight && !PathFindController.isPositionImpassableForNPCSchedule(location, neighbor.x, neighbor.y, npc)))
						{
							int f = (int)neighbor.g + PathFindController.getPreferenceValueForTerrainType(location, neighbor.x, neighbor.y) + (Math.Abs(endPoint.X - neighbor.x) + Math.Abs(endPoint.Y - neighbor.y) + (((neighbor.x == currentNode.x && neighbor.x == previousNode.x) || (neighbor.y == currentNode.y && neighbor.y == previousNode.y)) ? -2 : 0));
							if (!openList.Contains(neighbor, f))
							{
								openList.Enqueue(neighbor, f);
							}
						}
					}
				}
				previousNode = currentNode;
				iterations++;
				if (iterations >= limit)
				{
					return null;
				}
			}
			return null;
		}

		// Token: 0x06001D5B RID: 7515 RVA: 0x00150AFC File Offset: 0x0014ECFC
		protected static bool isPositionImpassableForNPCSchedule(GameLocation loc, int x, int y, Character npc)
		{
			Tile tile = loc.Map.RequireLayer("Buildings").Tiles[x, y];
			if (tile != null && tile.TileIndex != -1)
			{
				string value;
				if (tile.TileIndexProperties.TryGetValue("Action", out value) || tile.Properties.TryGetValue("Action", out value))
				{
					if (value.StartsWith("LockedDoorWarp"))
					{
						return true;
					}
					if (!value.Contains("Door") && !value.Contains("Passable"))
					{
						return true;
					}
				}
				else if (loc.doesTileHaveProperty(x, y, "Passable", "Buildings", false) == null && loc.doesTileHaveProperty(x, y, "NPCPassable", "Buildings", false) == null)
				{
					return true;
				}
			}
			if (loc.doesTileHaveProperty(x, y, "NoPath", "Back", false) != null)
			{
				return true;
			}
			foreach (Warp warp in loc.warps)
			{
				if (warp.X == x && warp.Y == y)
				{
					return true;
				}
			}
			TerrainFeature valueOrDefault = loc.terrainFeatures.GetValueOrDefault(new Vector2((float)x, (float)y), null);
			bool? flag = (valueOrDefault != null) ? new bool?(valueOrDefault.isPassable(npc)) : null;
			if (flag == null || flag.GetValueOrDefault())
			{
				LargeTerrainFeature largeTerrainFeatureAt = loc.getLargeTerrainFeatureAt(x, y);
				if (((largeTerrainFeatureAt != null) ? new bool?(largeTerrainFeatureAt.isPassable(npc)) : null) ?? true)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06001D5C RID: 7516 RVA: 0x00150CA8 File Offset: 0x0014EEA8
		protected static int getPreferenceValueForTerrainType(GameLocation l, int x, int y)
		{
			string text = l.doesTileHaveProperty(x, y, "Type", "Back", false);
			string a = (text != null) ? text.ToLower() : null;
			if (a == "stone")
			{
				return -7;
			}
			if (a == "wood")
			{
				return -4;
			}
			if (a == "dirt")
			{
				return -2;
			}
			if (!(a == "grass"))
			{
				return 0;
			}
			return -1;
		}

		// Token: 0x04001218 RID: 4632
		public const byte impassable = 255;

		// Token: 0x04001219 RID: 4633
		public const int timeToWaitBeforeCancelling = 5000;

		// Token: 0x0400121A RID: 4634
		private Character character;

		// Token: 0x0400121B RID: 4635
		public GameLocation location;

		// Token: 0x0400121C RID: 4636
		public Stack<Point> pathToEndPoint;

		// Token: 0x0400121D RID: 4637
		public Point endPoint;

		// Token: 0x0400121E RID: 4638
		public int finalFacingDirection;

		// Token: 0x0400121F RID: 4639
		public int pausedTimer;

		// Token: 0x04001220 RID: 4640
		public PathFindController.endBehavior endBehaviorFunction;

		// Token: 0x04001221 RID: 4641
		public bool nonDestructivePathing;

		// Token: 0x04001222 RID: 4642
		public bool allowPlayerPathingInEvent;

		// Token: 0x04001223 RID: 4643
		public bool NPCSchedule;

		// Token: 0x04001224 RID: 4644
		protected static readonly sbyte[,] Directions = new sbyte[,]
		{
			{
				-1,
				0
			},
			{
				1,
				0
			},
			{
				0,
				1
			},
			{
				0,
				-1
			}
		};

		// Token: 0x04001225 RID: 4645
		protected static PriorityQueue _openList = new PriorityQueue();

		// Token: 0x04001226 RID: 4646
		protected static HashSet<int> _closedList = new HashSet<int>();

		// Token: 0x04001227 RID: 4647
		protected static int _counter = 0;

		// Token: 0x04001228 RID: 4648
		public int timerSinceLastCheckPoint;

		// Token: 0x02000549 RID: 1353
		// (Invoke) Token: 0x06004130 RID: 16688
		public delegate bool isAtEnd(PathNode currentNode, Point endPoint, GameLocation location, Character c);

		// Token: 0x0200054A RID: 1354
		// (Invoke) Token: 0x06004134 RID: 16692
		public delegate void endBehavior(Character c, GameLocation location);
	}
}

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace StardewValley.Pathfinding
{
	// Token: 0x0200019F RID: 415
	public class SchedulePathDescription
	{
		// Token: 0x06001D6C RID: 7532 RVA: 0x00151020 File Offset: 0x0014F220
		public SchedulePathDescription(Stack<Point> route, int facingDirection, string endBehavior, string endMessage, string targetLocationName, Point targetTile)
		{
			this.endOfRouteMessage = endMessage;
			this.route = route;
			this.facingDirection = facingDirection;
			this.endOfRouteBehavior = endBehavior;
			this.targetLocationName = targetLocationName;
			this.targetTile = targetTile;
		}

		// Token: 0x04001230 RID: 4656
		public Stack<Point> route;

		// Token: 0x04001231 RID: 4657
		public int time;

		// Token: 0x04001232 RID: 4658
		public int facingDirection;

		// Token: 0x04001233 RID: 4659
		public string endOfRouteBehavior;

		// Token: 0x04001234 RID: 4660
		public string endOfRouteMessage;

		// Token: 0x04001235 RID: 4661
		public string targetLocationName;

		// Token: 0x04001236 RID: 4662
		public Point targetTile;
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Delegates;
using StardewValley.Inventories;
using StardewValley.Network;

namespace StardewValley.Internal
{
	// Token: 0x0200030E RID: 782
	public readonly struct ForEachItemContext
	{
		// Token: 0x0600340D RID: 13325 RVA: 0x0029A20F File Offset: 0x0029840F
		public ForEachItemContext(Item item, Action remove, Action<Item> replaceWith, GetForEachItemPathDelegate getPath)
		{
			this.Item = item;
			this.RemoveItem = remove;
			this.ReplaceItemWith = replaceWith;
			this.GetPath = getPath;
		}

		// Token: 0x0600340E RID: 13326 RVA: 0x0029A230 File Offset: 0x00298430
		public IList<string> GetDisplayPath(bool includeItem = false)
		{
			List<string> path = new List<string>();
			foreach (object pathValue in this.GetPath())
			{
				this.AddDisplayPath(path, pathValue);
			}
			if (includeItem)
			{
				this.AddDisplayPath(path, this.Item);
			}
			return path;
		}

		// Token: 0x0600340F RID: 13327 RVA: 0x0029A29C File Offset: 0x0029849C
		private void AddDisplayPath(IList<string> path, object pathValue)
		{
			GameLocation location = pathValue as GameLocation;
			if (location != null)
			{
				if (path.Count == 0 && location.ParentBuilding != null)
				{
					this.AddDisplayPath(path, location.ParentBuilding);
				}
				path.Add(location.NameOrUniqueName);
				return;
			}
			Building building = pathValue as Building;
			if (building != null)
			{
				if (path.Count == 0)
				{
					GameLocation location2 = building.GetParentLocation();
					if (location2 != null)
					{
						this.AddDisplayPath(path, location2);
					}
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(6, 3);
				defaultInterpolatedStringHandler.AppendFormatted(building.buildingType.Value);
				defaultInterpolatedStringHandler.AppendLiteral(" at ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(building.tileX.Value);
				defaultInterpolatedStringHandler.AppendLiteral(", ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(building.tileY.Value);
				path.Add(defaultInterpolatedStringHandler.ToStringAndClear());
				return;
			}
			Object parentObj = pathValue as Object;
			if (parentObj != null)
			{
				if (path.Count == 0 && parentObj.Location != null)
				{
					this.AddDisplayPath(path, parentObj.Location);
				}
				string item;
				if (!(parentObj.TileLocation != Vector2.Zero))
				{
					item = parentObj.Name;
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(6, 3);
					defaultInterpolatedStringHandler.AppendFormatted(parentObj.Name);
					defaultInterpolatedStringHandler.AppendLiteral(" at ");
					defaultInterpolatedStringHandler.AppendFormatted<float>(parentObj.TileLocation.X);
					defaultInterpolatedStringHandler.AppendLiteral(", ");
					defaultInterpolatedStringHandler.AppendFormatted<float>(parentObj.TileLocation.Y);
					item = defaultInterpolatedStringHandler.ToStringAndClear();
				}
				path.Add(item);
				return;
			}
			Farmer player = pathValue as Farmer;
			if (player != null)
			{
				path.Add("player '" + player.Name + "'");
				return;
			}
			Item parentItem = pathValue as Item;
			if (parentItem != null)
			{
				path.Add(parentItem.Name);
				return;
			}
			INetSerializable field = pathValue as INetSerializable;
			if (field == null)
			{
				if (!(pathValue is IInventory) && !(pathValue is OverlaidDictionary))
				{
					path.Add(pathValue.ToString());
				}
				return;
			}
			path.Add(field.Name);
		}

		// Token: 0x0400221F RID: 8735
		public readonly Item Item;

		// Token: 0x04002220 RID: 8736
		public readonly Action RemoveItem;

		// Token: 0x04002221 RID: 8737
		public readonly Action<Item> ReplaceItemWith;

		// Token: 0x04002222 RID: 8738
		public readonly GetForEachItemPathDelegate GetPath;
	}
}

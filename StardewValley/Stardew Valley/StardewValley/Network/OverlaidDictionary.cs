using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Netcode;

namespace StardewValley.Network
{
	// Token: 0x020001F0 RID: 496
	public class OverlaidDictionary : IEnumerable<SerializableDictionary<Vector2, Object>>, IEnumerable
	{
		// Token: 0x170003B7 RID: 951
		// (get) Token: 0x06002222 RID: 8738 RVA: 0x00175AD3 File Offset: 0x00173CD3
		public int Length
		{
			get
			{
				return this.compositeDict.Count;
			}
		}

		// Token: 0x170003B8 RID: 952
		public Object this[Vector2 key]
		{
			get
			{
				Object overlaid;
				if (this.overlayDict.TryGetValue(key, out overlaid))
				{
					return overlaid;
				}
				Object o;
				if (!this._locked || !this._changes.TryGetValue(key, out o))
				{
					return this.baseDict[key];
				}
				if (o == null)
				{
					throw new KeyNotFoundException();
				}
				return o;
			}
			set
			{
				if (this._locked)
				{
					this._changes[key] = value;
					return;
				}
				this.baseDict[key] = value;
			}
		}

		// Token: 0x06002225 RID: 8741 RVA: 0x00175B54 File Offset: 0x00173D54
		public void OnValueAdded(Vector2 key, Object value)
		{
			Object o;
			if (this.overlayDict.TryGetValue(key, out o))
			{
				this.compositeDict[key] = o;
				return;
			}
			if (this.baseDict.TryGetValue(key, out o))
			{
				this.compositeDict[key] = o;
			}
		}

		// Token: 0x06002226 RID: 8742 RVA: 0x00175B9C File Offset: 0x00173D9C
		public void OnValueRemoved(Vector2 key, Object value)
		{
			Object o;
			if (this.overlayDict.TryGetValue(key, out o))
			{
				this.compositeDict[key] = o;
				return;
			}
			if (this.baseDict.TryGetValue(key, out o))
			{
				this.compositeDict[key] = o;
				return;
			}
			this.compositeDict.Remove(key);
		}

		// Token: 0x170003B9 RID: 953
		// (get) Token: 0x06002227 RID: 8743 RVA: 0x00175BF2 File Offset: 0x00173DF2
		public Dictionary<Vector2, Object>.KeyCollection Keys
		{
			get
			{
				return this.compositeDict.Keys;
			}
		}

		// Token: 0x170003BA RID: 954
		// (get) Token: 0x06002228 RID: 8744 RVA: 0x00175BFF File Offset: 0x00173DFF
		public Dictionary<Vector2, Object>.ValueCollection Values
		{
			get
			{
				return this.compositeDict.Values;
			}
		}

		// Token: 0x170003BB RID: 955
		// (get) Token: 0x06002229 RID: 8745 RVA: 0x00175C0C File Offset: 0x00173E0C
		public IEnumerable<KeyValuePair<Vector2, Object>> Pairs
		{
			get
			{
				return this.compositeDict;
			}
		}

		// Token: 0x0600222A RID: 8746 RVA: 0x00175C14 File Offset: 0x00173E14
		public void SetEqualityComparer(IEqualityComparer<Vector2> comparer, ref NetVector2Dictionary<Object, NetRef<Object>> base_dict, ref OverlayDictionary<Vector2, Object> overlay_dict)
		{
			this.baseDict.SetEqualityComparer(comparer);
			this.overlayDict.onValueAdded -= this.OnValueAdded;
			this.overlayDict.onValueRemoved -= this.OnValueRemoved;
			this.overlayDict = new OverlayDictionary<Vector2, Object>(this.overlayDict, comparer);
			this.compositeDict = new Dictionary<Vector2, Object>(this.compositeDict, comparer);
			this.overlayDict.onValueAdded += this.OnValueAdded;
			this.overlayDict.onValueRemoved += this.OnValueRemoved;
			this.overlayDict.onValueAdded += this.OnValueAdded;
			this.overlayDict.onValueRemoved += this.OnValueRemoved;
			base_dict = this.baseDict;
			overlay_dict = this.overlayDict;
		}

		// Token: 0x0600222B RID: 8747 RVA: 0x00175CEC File Offset: 0x00173EEC
		public OverlaidDictionary(NetVector2Dictionary<Object, NetRef<Object>> baseDict, OverlayDictionary<Vector2, Object> overlayDict)
		{
			this.baseDict = baseDict;
			this.overlayDict = overlayDict;
			this.compositeDict = new Dictionary<Vector2, Object>();
			foreach (KeyValuePair<Vector2, Object> pair in overlayDict)
			{
				this.OnValueAdded(pair.Key, pair.Value);
			}
			foreach (KeyValuePair<Vector2, Object> pair2 in baseDict.Pairs)
			{
				this.OnValueAdded(pair2.Key, pair2.Value);
			}
			baseDict.OnValueAdded += this.OnValueAdded;
			baseDict.OnConflictResolve += delegate(Vector2 key, NetRef<Object> rejected, NetRef<Object> accepted)
			{
				this.OnValueRemoved(key, rejected.Value);
				this.OnValueAdded(key, accepted.Value);
			};
			baseDict.OnValueRemoved += this.OnValueRemoved;
		}

		// Token: 0x0600222C RID: 8748 RVA: 0x00175DF4 File Offset: 0x00173FF4
		public bool Any()
		{
			return this.compositeDict.Count > 0;
		}

		// Token: 0x0600222D RID: 8749 RVA: 0x00175E04 File Offset: 0x00174004
		public int Count()
		{
			return this.compositeDict.Count;
		}

		// Token: 0x0600222E RID: 8750 RVA: 0x00175E11 File Offset: 0x00174011
		public void Lock()
		{
			this._locked = true;
		}

		// Token: 0x0600222F RID: 8751 RVA: 0x00175E1C File Offset: 0x0017401C
		public void Unlock()
		{
			if (this._locked)
			{
				this._locked = false;
				if (this._changes.Count > 0)
				{
					foreach (KeyValuePair<Vector2, Object> kvp in this._changes)
					{
						if (kvp.Value != null)
						{
							this.baseDict[kvp.Key] = kvp.Value;
						}
						else
						{
							this.baseDict.Remove(kvp.Key);
						}
					}
					this._changes.Clear();
				}
			}
		}

		// Token: 0x06002230 RID: 8752 RVA: 0x00175ECC File Offset: 0x001740CC
		public void Add(Vector2 key, Object value)
		{
			if (!this._locked)
			{
				this.baseDict.Add(key, value);
				return;
			}
			Object existingValue;
			if (this._changes.TryGetValue(key, out existingValue))
			{
				if (existingValue == null)
				{
					this._changes[key] = value;
					return;
				}
				throw new ArgumentException();
			}
			else
			{
				if (this.baseDict.ContainsKey(key))
				{
					throw new ArgumentException();
				}
				this._changes[key] = value;
				return;
			}
		}

		// Token: 0x06002231 RID: 8753 RVA: 0x00175F37 File Offset: 0x00174137
		public bool TryAdd(Vector2 key, Object value)
		{
			if (this.ContainsKey(key))
			{
				return false;
			}
			this.Add(key, value);
			return true;
		}

		// Token: 0x06002232 RID: 8754 RVA: 0x00175F4D File Offset: 0x0017414D
		public void Clear()
		{
			if (this._locked)
			{
				throw new NotImplementedException();
			}
			this.baseDict.Clear();
			this.overlayDict.Clear();
			this.compositeDict.Clear();
		}

		// Token: 0x06002233 RID: 8755 RVA: 0x00175F80 File Offset: 0x00174180
		public bool ContainsKey(Vector2 key)
		{
			Object value;
			if (this._locked && this._changes.TryGetValue(key, out value))
			{
				return value != null;
			}
			return this.compositeDict.ContainsKey(key);
		}

		// Token: 0x06002234 RID: 8756 RVA: 0x00175FB8 File Offset: 0x001741B8
		public bool Remove(Vector2 key)
		{
			if (this.overlayDict.Remove(key))
			{
				return true;
			}
			if (!this._locked)
			{
				return this.baseDict.Remove(key);
			}
			Object value;
			if (this._changes.TryGetValue(key, out value))
			{
				this._changes[key] = null;
				return value != null;
			}
			if (this.baseDict.ContainsKey(key))
			{
				this._changes[key] = null;
				return true;
			}
			return false;
		}

		// Token: 0x06002235 RID: 8757 RVA: 0x0017602A File Offset: 0x0017422A
		public bool TryGetValue(Vector2 key, out Object value)
		{
			return this.compositeDict.TryGetValue(key, out value);
		}

		// Token: 0x06002236 RID: 8758 RVA: 0x00176039 File Offset: 0x00174239
		public Object GetValueOrDefault(Vector2 key, Object defaultValue = null)
		{
			return this.compositeDict.GetValueOrDefault(key, defaultValue);
		}

		// Token: 0x06002237 RID: 8759 RVA: 0x00176048 File Offset: 0x00174248
		public IEnumerator<SerializableDictionary<Vector2, Object>> GetEnumerator()
		{
			return this.baseDict.GetEnumerator();
		}

		// Token: 0x06002238 RID: 8760 RVA: 0x00176055 File Offset: 0x00174255
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.baseDict.GetEnumerator();
		}

		// Token: 0x06002239 RID: 8761 RVA: 0x00176064 File Offset: 0x00174264
		public void Add(SerializableDictionary<Vector2, Object> dict)
		{
			foreach (KeyValuePair<Vector2, Object> pair in dict)
			{
				if (pair.Value != null)
				{
					this.Add(pair.Key, pair.Value);
				}
			}
		}

		// Token: 0x0400145A RID: 5210
		private NetVector2Dictionary<Object, NetRef<Object>> baseDict;

		// Token: 0x0400145B RID: 5211
		private OverlayDictionary<Vector2, Object> overlayDict;

		// Token: 0x0400145C RID: 5212
		private Dictionary<Vector2, Object> compositeDict;

		// Token: 0x0400145D RID: 5213
		private bool _locked;

		// Token: 0x0400145E RID: 5214
		private Dictionary<Vector2, Object> _changes = new Dictionary<Vector2, Object>();
	}
}

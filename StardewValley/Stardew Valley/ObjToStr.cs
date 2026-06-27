using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Sickhead.Engine.Util;

// Token: 0x0200000A RID: 10
public static class ObjToStr
{
	// Token: 0x06000020 RID: 32 RVA: 0x000026A0 File Offset: 0x000008A0
	public static string Format(object obj, ObjToStr.Style style)
	{
		Type type = obj.GetType();
		ObjToStr._cache.Clear();
		ObjToStr.ToStringDescription desc;
		if (!ObjToStr._cache.TryGetValue(obj.GetType(), out desc))
		{
			desc = new ObjToStr.ToStringDescription
			{
				Type = type,
				Members = new List<ObjToStr.ToStringMember>()
			};
			ObjToStr._cache.Add(type, desc);
			BindingFlags attrs = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			foreach (FieldInfo i in type.GetFields(attrs))
			{
				ObjToStr.ToStringMember item = new ObjToStr.ToStringMember
				{
					Member = i,
					Name = i.Name
				};
				Type dataType2 = i.GetDataType();
				if (dataType2 == typeof(string))
				{
					item.Format = "\"{0}\"";
				}
				if (dataType2.HasElementType)
				{
					item.Format = "{1}[{2}] {0}";
				}
				desc.Members.Add(item);
			}
			desc.Members.Sort(new Comparison<ObjToStr.ToStringMember>(ObjToStr.CompareToStringMembers));
		}
		StringBuilder stringBuilder = ObjToStr._stringBuilder;
		string result;
		lock (stringBuilder)
		{
			ObjToStr._stringBuilder.Clear();
			if (style.ShowRootObjectType)
			{
				ObjToStr._stringBuilder.Append(desc.Type.Name);
				ObjToStr._stringBuilder.Append(style.ObjectDelimiter);
			}
			for (int j = 0; j < desc.Members.Count; j++)
			{
				ObjToStr.ToStringMember k = desc.Members[j];
				Type dataType = k.Member.GetDataType();
				object val = k.Member.GetValue(obj);
				ObjToStr._stringBuilder.Append(dataType.Name);
				ObjToStr._stringBuilder.Append(" ");
				ObjToStr._stringBuilder.Append(k.Name);
				ObjToStr._stringBuilder.Append(style.MemberNameValueDelimiter);
				if (val == null)
				{
					ObjToStr._stringBuilder.Append("null");
				}
				else
				{
					Type vtype = val.GetType();
					if (vtype.HasElementType)
					{
						Type etype = vtype.GetElementType();
						string ecount = "?";
						ObjToStr._stringBuilder.AppendFormat(k.Format, val, etype, ecount);
					}
					else
					{
						ObjToStr._stringBuilder.AppendFormat(k.Format, val);
					}
				}
				if (j != desc.Members.Count - 1)
				{
					ObjToStr._stringBuilder.Append(style.MemberDelimiter);
				}
			}
			result = ObjToStr._stringBuilder.ToString();
		}
		return result;
	}

	// Token: 0x06000021 RID: 33 RVA: 0x00002944 File Offset: 0x00000B44
	private static int CompareToStringMembers(ObjToStr.ToStringMember a, ObjToStr.ToStringMember b)
	{
		return a.Name.CompareTo(b.Name);
	}

	// Token: 0x04000011 RID: 17
	private static readonly StringBuilder _stringBuilder = new StringBuilder();

	// Token: 0x04000012 RID: 18
	private static readonly Dictionary<Type, ObjToStr.ToStringDescription> _cache = new Dictionary<Type, ObjToStr.ToStringDescription>();

	// Token: 0x020003BC RID: 956
	private struct ToStringDescription
	{
		// Token: 0x04002653 RID: 9811
		public Type Type;

		// Token: 0x04002654 RID: 9812
		public List<ObjToStr.ToStringMember> Members;
	}

	// Token: 0x020003BD RID: 957
	private struct ToStringMember
	{
		// Token: 0x170004AD RID: 1197
		// (get) Token: 0x06003968 RID: 14696 RVA: 0x002D716C File Offset: 0x002D536C
		// (set) Token: 0x06003969 RID: 14697 RVA: 0x002D718D File Offset: 0x002D538D
		public string Name
		{
			get
			{
				if (!string.IsNullOrEmpty(this._name))
				{
					return this._name;
				}
				return this.Member.Name;
			}
			set
			{
				this._name = value;
			}
		}

		// Token: 0x170004AE RID: 1198
		// (get) Token: 0x0600396A RID: 14698 RVA: 0x002D7196 File Offset: 0x002D5396
		// (set) Token: 0x0600396B RID: 14699 RVA: 0x002D71B1 File Offset: 0x002D53B1
		public string Format
		{
			get
			{
				if (!string.IsNullOrEmpty(this._format))
				{
					return this._format;
				}
				return "{0}";
			}
			set
			{
				this._format = value;
			}
		}

		// Token: 0x04002655 RID: 9813
		public MemberInfo Member;

		// Token: 0x04002656 RID: 9814
		private string _name;

		// Token: 0x04002657 RID: 9815
		private string _format;
	}

	// Token: 0x020003BE RID: 958
	public class Style
	{
		// Token: 0x0600396C RID: 14700 RVA: 0x002D71BA File Offset: 0x002D53BA
		public Style()
		{
			this.ShowRootObjectType = true;
			this.ObjectDelimiter = ":";
			this.MemberDelimiter = ",";
			this.MemberNameValueDelimiter = "=";
		}

		// Token: 0x04002658 RID: 9816
		public bool ShowRootObjectType;

		// Token: 0x04002659 RID: 9817
		public string ObjectDelimiter;

		// Token: 0x0400265A RID: 9818
		public string MemberDelimiter;

		// Token: 0x0400265B RID: 9819
		public string MemberNameValueDelimiter;

		// Token: 0x0400265C RID: 9820
		public bool TrailingNewline;

		// Token: 0x0400265D RID: 9821
		public static ObjToStr.Style TypeAndMembersSingleLine = new ObjToStr.Style
		{
			ShowRootObjectType = true,
			ObjectDelimiter = ":",
			MemberDelimiter = ",",
			MemberNameValueDelimiter = "="
		};

		// Token: 0x0400265E RID: 9822
		public static ObjToStr.Style MembersOnlyMultiline = new ObjToStr.Style
		{
			ShowRootObjectType = false,
			ObjectDelimiter = "",
			MemberDelimiter = "\n",
			MemberNameValueDelimiter = "="
		};
	}
}

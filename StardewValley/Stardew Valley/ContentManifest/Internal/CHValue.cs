using System;

namespace ContentManifest.Internal
{
	// Token: 0x0200007C RID: 124
	internal class CHValue : CHParsable
	{
		// Token: 0x06000488 RID: 1160 RVA: 0x0001589B File Offset: 0x00013A9B
		public CHValue()
		{
			this.RawValue.ValueNull = null;
		}

		// Token: 0x06000489 RID: 1161 RVA: 0x000158B8 File Offset: 0x00013AB8
		public void Parse(CHJsonParserContext context)
		{
			if (context.ReadHead >= context.JsonText.Length)
			{
				throw new InvalidOperationException();
			}
			char prefixChar = context.JsonText[context.ReadHead];
			CHParsable parsable;
			if (prefixChar <= 'f')
			{
				if (prefixChar == '"')
				{
					parsable = (this.RawValue.ValueString = new CHString());
					this.ValueType = CHValueEnum.String;
					goto IL_17D;
				}
				if (prefixChar == '[')
				{
					parsable = (this.RawValue.ValueArray = new CHArray());
					this.ValueType = CHValueEnum.Array;
					goto IL_17D;
				}
				if (prefixChar != 'f')
				{
					goto IL_150;
				}
			}
			else if (prefixChar != 'n')
			{
				if (prefixChar != 't')
				{
					if (prefixChar == '{')
					{
						parsable = (this.RawValue.ValueObject = new CHObject());
						this.ValueType = CHValueEnum.Object;
						goto IL_17D;
					}
					goto IL_150;
				}
			}
			else
			{
				if (context.ReadHead + 3 >= context.JsonText.Length)
				{
					throw new InvalidOperationException();
				}
				if (context.JsonText[context.ReadHead + 1] != 'u' || context.JsonText[context.ReadHead + 2] != 'l' || context.JsonText[context.ReadHead + 3] != 'l')
				{
					throw new InvalidOperationException();
				}
				parsable = null;
				this.ValueType = CHValueEnum.Null;
				goto IL_17D;
			}
			parsable = (this.RawValue.ValueBoolean = new CHBoolean());
			this.ValueType = CHValueEnum.Boolean;
			goto IL_17D;
			IL_150:
			if (!CHNumber.IsValidPrefix(prefixChar))
			{
				throw new InvalidOperationException();
			}
			parsable = (this.RawValue.ValueNumber = new CHNumber());
			this.ValueType = CHValueEnum.Number;
			IL_17D:
			if (parsable != null)
			{
				parsable.Parse(context);
			}
		}

		// Token: 0x0600048A RID: 1162 RVA: 0x00015A4C File Offset: 0x00013C4C
		public object GetManagedObject()
		{
			switch (this.ValueType)
			{
			case CHValueEnum.Object:
				return this.RawValue.ValueObject.Members;
			case CHValueEnum.Array:
				return this.RawValue.ValueArray.Elements;
			case CHValueEnum.String:
				return this.RawValue.ValueString.RawString;
			case CHValueEnum.Number:
				return this.RawValue.ValueNumber.RawDouble;
			case CHValueEnum.Boolean:
				return this.RawValue.ValueBoolean.RawBoolean;
			case CHValueEnum.Null:
				return null;
			}
			throw new InvalidOperationException();
		}

		// Token: 0x040001B0 RID: 432
		public CHValueUnion RawValue;

		// Token: 0x040001B1 RID: 433
		public CHValueEnum ValueType = CHValueEnum.Unknown;
	}
}

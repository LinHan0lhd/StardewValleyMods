using System;
using System.Collections.Generic;
using System.IO;

namespace ContentManifest
{
	// Token: 0x02000072 RID: 114
	public class ContentHashParser
	{
		// Token: 0x0600046F RID: 1135 RVA: 0x00014F57 File Offset: 0x00013157
		public static Dictionary<string, object> ParseFromFile(string contentHashPath)
		{
			return CHJsonParser.ParseJson(File.ReadAllText(contentHashPath)) as Dictionary<string, object>;
		}
	}
}

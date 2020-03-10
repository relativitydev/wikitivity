using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Relativity.Services.Objects;

namespace Wikitivity.CustomPage
{
	public class WikiConstants
	{
		public class WikiRequest
		{
			public string RequestIDGuid { get; set; }
			public string Page { get; set; }
			public IObjectManager Proxy { get; set; }
			public int workspaceID { get; set; }
			public string prefixText { get; set; }
			public int count { get; set; }

		}
	}

}



using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace Wikitivity.CustomPage
{
	public class WikitivityHelper
	{

		public static Guid WikitivityRequestHistoryRDOGuid = new Guid("EF1DF3CD-2E84-48BE-9E02-AC07DD0481C1");
		public static Guid WikitivityRDOGuid = new Guid("C6196733-E2A6-48F4-9443-37990972EBA3");

		#region classes
		public class CurrentJob
		{
			public string Requests { get; set; }
			public string workspaceID { get; set; }
			public string artifactID { get; set; }
		}
		public partial class WikiCategoryMemberRequest
		{
			public string Batchcomplete { get; set; }
			public Continue Continue { get; set; }
			public Query query { get; set; }
		}
		public partial class Continue
		{
			public string cmcontinue { get; set; }
			public string continueContinue { get; set; }
		}
		public partial class Query
		{
			public List<Categorymember> categorymembers { get; set; }
		}
		public partial class Categorymember
		{
			public long pageid { get; set; }
			public long ns { get; set; }
			public string title { get; set; }
		}
		public partial class WikiCategoryRequest2
		{
			public string Batchcomplete { get; set; }
			public Query2 Query { get; set; }
			public Limits Limits { get; set; }
		}
		public partial class Limits
		{
			public long Extracts { get; set; }
		}
		public partial class Query2
		{
			public Dictionary<string, pageval> pages { get; set; }
		}
		public class pageval
		{
			public int pageid { get; set; }
			public int ns { get; set; }
			public string title { get; set; }
			public string extract { get; set; }
		}
		public class WikiEntry
		{
			public string title { get; set; }
			public string extract { get; set; }
			public int pageID { get; set; }
		}
		public class WikiRequest
		{
			public string ArtifactID { get; set; }
			public string WorkspaceID { get; set; }
			public string RequestURL { get; set; }
			public string Status { get; set; }

		}

		#endregion

		public List<string> previewRequest(string requestTerm)
		{
			List<string> TitleCollection = new List<string>();
			int categoryCounter = 0;
			var client = new WebClient();

			var response = client.DownloadString($"https://en.wikipedia.org/w/api.php?format=json&action=query&list=categorymembers&cmtitle=Category:{requestTerm}&cmlimit=500");
			var responseJson = JsonConvert.DeserializeObject<WikiCategoryMemberRequest>(response);

			if (responseJson.query.categorymembers.Count > 0)
			{
				foreach (Categorymember singleCategoryMember in responseJson.query.categorymembers)
				{
					if (singleCategoryMember.title.Contains("Category:"))
					{
						categoryCounter += 1;
						if (categoryCounter <= 100) // Modify this to control how the max number of categories to return
						{
							GetPagesFromCategories(singleCategoryMember, client, TitleCollection);
						}
					}
					else
					{
						if (!singleCategoryMember.title.Contains("Category:") && !singleCategoryMember.title.Contains("File:") && !singleCategoryMember.title.Contains("Template:"))
						{
							TitleCollection.Add(singleCategoryMember.title);
						}
					}
				}
			}
			return TitleCollection;
		}

		public void GetPagesFromCategories(Categorymember singleCategoryMember, WebClient client, List<string> TitleCollection)
		{
			var retrievePageMembers = singleCategoryMember.title.Replace(' ', '_');
			var response4 = client.DownloadString($"https://en.wikipedia.org/w/api.php?format=json&action=query&list=categorymembers&cmtitle={retrievePageMembers}&cmlimit=500");
			WikiCategoryMemberRequest additionalCategories = JsonConvert.DeserializeObject<WikiCategoryMemberRequest>(response4);
			if (additionalCategories.query.categorymembers.Count > 0)
			{
				foreach (Categorymember singleCatMem in additionalCategories.query.categorymembers)
				{
					if (!singleCatMem.title.Contains("Category:") && !singleCatMem.title.Contains("File:") && !singleCategoryMember.title.Contains("Template:"))
					{
						TitleCollection.Add(singleCatMem.title);
					}
				}
			}
		}


		public async Task UpdateRequestHistoryOM(IObjectManager proxy, string requestID, string requestUser, int articleCount, int workspaceID, string requestedCategories, string prefix)
		{

			//TODO: Refactor to OM
			CreateRequest createRequestHistory = new CreateRequest();


			var RequestIDField = new FieldRefValuePair()
			{
				Field = new FieldRef() { Name = "Request ID" },
				Value = requestID

			};
			var RequestUserField = new FieldRefValuePair()
			{
				Field = new FieldRef() { Name = "Request User" },
				Value = requestUser

			};
			var RequestDateField = new FieldRefValuePair()
			{
				Field = new FieldRef() { Name = "Request Date" },
				Value = DateTime.Today.ToShortDateString()

			};

			var ArticleCountField = new FieldRefValuePair()
			{
				Field = new FieldRef() { Name = "Article Count" },
				Value = articleCount

			};
			var RequestedCategoriesField = new FieldRefValuePair()
			{
				Field = new FieldRef() { Name = "Requested Categories" },
				Value = requestedCategories
			};
			var PrefixField = new FieldRefValuePair() { Field = new FieldRef() { Name = "Prefix" }, Value = prefix };

			IEnumerable<FieldRefValuePair> FullFieldListWithValues = new List<FieldRefValuePair>()
					{
						RequestIDField, RequestUserField, RequestDateField, ArticleCountField, RequestedCategoriesField, PrefixField

					};

			createRequestHistory.FieldValues = FullFieldListWithValues;
			createRequestHistory.ObjectType = new ObjectTypeRef() { Guid = WikitivityRequestHistoryRDOGuid };

			try
			{
				await proxy.CreateAsync(workspaceID, createRequestHistory);
			}
			catch (Exception ex)
			{
				throw ex;

			}
		}
		public void UpdateRequestHistory(IRSAPIClient proxy, string requestID, string requestUser, int articleCount, int workspaceID, string requestedCategories, string prefix)
		{

			proxy.APIOptions.WorkspaceID = workspaceID;
			RDO wikitivityRequestHistoryRDO = new RDO(WikitivityRequestHistoryRDOGuid);
			List<Guid> guidList = new List<Guid>();
			guidList.Add(WikitivityRequestHistoryRDOGuid);
			wikitivityRequestHistoryRDO.ArtifactTypeGuids = guidList;
			wikitivityRequestHistoryRDO.Fields.Add(new FieldValue() { Name = "Request ID", Value = requestID });
			wikitivityRequestHistoryRDO.Fields.Add(new FieldValue() { Name = "Request User", Value = requestUser });
			wikitivityRequestHistoryRDO.Fields.Add(new FieldValue() { Name = "Request Date", Value = DateTime.Today.ToShortDateString() });
			wikitivityRequestHistoryRDO.Fields.Add(new FieldValue() { Name = "Article Count", Value = articleCount });
			wikitivityRequestHistoryRDO.Fields.Add(new FieldValue() { Name = "Requested Categories", Value = requestedCategories });
			wikitivityRequestHistoryRDO.Fields.Add(new FieldValue() { Name = "Prefix", Value = prefix });

			try
			{
				WriteResultSet<RDO> writeResultSet = proxy.Repositories.RDO.Create(wikitivityRequestHistoryRDO);

			}
			catch (Exception ex)
			{

			}
		}
		public void WriteToTable(string requestID, string pageTitle, IRSAPIClient proxy, int workspaceID, string prefix, int count)
		{
			string requestUrl = $"https://en.wikipedia.org/w/api.php?format=json&action=query&prop=extracts&exlimit=max&explaintext&titles={pageTitle}&redirects=";
			//TODO: Refactor to OM
			proxy.APIOptions.WorkspaceID = workspaceID;
			RDO wikitivityRequestRDO = new RDO(WikitivityRDOGuid);

			List<Guid> guidList = new List<Guid>();
			guidList.Add(WikitivityRDOGuid);
			wikitivityRequestRDO.ArtifactTypeGuids = guidList;
			wikitivityRequestRDO.Fields.Add(new FieldValue() { Name = "Request ID", Value = requestID });
			wikitivityRequestRDO.Fields.Add(new FieldValue() { Name = "Request Url", Value = requestUrl });

			if (prefix == String.Empty)
			{
				prefix = "WIKI";
			}
			string docID = prefix + count.ToString("D7");

			wikitivityRequestRDO.Fields.Add(new FieldValue() { Name = "Name", Value = docID });
			wikitivityRequestRDO.Fields.Add(new FieldValue() { Name = "Page Title", Value = pageTitle });

			try
			{
				WriteResultSet<RDO> writeResultSet = proxy.Repositories.RDO.Create(wikitivityRequestRDO);
			}
			catch (Exception ex)
			{

			}
		}


		public async Task WriteToTableOM(List<WikiConstants.WikiRequest> ListofRequests)
		{
			//Now I need to construct the individual values to be added in the masscreate call?
			var proxy = ListofRequests.FirstOrDefault().Proxy;
			var workspaceID = ListofRequests.FirstOrDefault().workspaceID;
			//TODO: Refactor to OM
			//	proxy.APIOptions.WorkspaceID = workspaceID;

			var massCreateRequest = new MassCreateRequest();
			massCreateRequest.ObjectType = new ObjectTypeRef() { Guid = WikitivityRDOGuid };

			//Construct the fields
			FieldRef RequestID = new FieldRef();
			//field1.Guid = DocumentCountFieldGuid;
			RequestID.Name = "Request ID";
			FieldRef RequestUrl = new FieldRef();
			//field2.Guid = UserCountFieldGuid;
			RequestUrl.Name = "Request Url";
			FieldRef PageTitle = new FieldRef();
			PageTitle.Name = "Page Title";
			//field3.ArtifactID = 1084090;
			FieldRef RequestName = new FieldRef();
			RequestName.Name = "Name";
			List<FieldRef> fieldList = new List<FieldRef>() { RequestID, RequestUrl, RequestName, PageTitle };
			massCreateRequest.Fields = fieldList;

			// Individual operations now?




			//Construct the values THESE MUST BE IN ORDER?


			//	List<object> FieldVals = new List<object>();

			//	List<FieldRef> test = new List<FieldRef>();


			var preppedlistofRequests = new List<List<object>> { };



			foreach (var singleReq in ListofRequests)
			{

				string requestUrl = $"https://en.wikipedia.org/w/api.php?format=json&action=query&prop=extracts&exlimit=max&explaintext&titles={singleReq.Page}&redirects=";

				if (singleReq.prefixText == String.Empty)
				{
					singleReq.prefixText = "WIKI";
				}

				string docID = singleReq.prefixText + singleReq.count.ToString("D7");

				preppedlistofRequests.Add(new List<object>()
				{

						singleReq.RequestIDGuid,
						requestUrl,
						docID,
						singleReq.Page
				 });


				//var singleRequestList = new List<List<object>>() { new List<object>() { singleReq.RequestIDGuid, requestUrl, singleReq.Page.ToString(), docID } };
				//FieldVals.Add(singleRequestList);
			}
			//IReadOnlyList<IReadOnlyList<object>> FieldValues = new List<IReadOnlyList<object>>() { FieldVals };
			massCreateRequest.ValueLists = preppedlistofRequests;


			//List<Guid> guidList = new List<Guid>();
			//guidList.Add(WikitivityRDOGuid);
			//wikitivityRequestRDO.ArtifactTypeGuids = guidList;
			//test.Add(new FieldRef() { Name = "Request ID", Value = requestID });
			//test.Add(new FieldValue() { Name = "Request Url", Value = requestUrl });

			//if (prefix == String.Empty)
			//{
			//	prefix = "WIKI";
			//}
			////string docID = prefix + count.ToString("D7");

			//wikitivityRequestRDO.Fields.Add(new FieldValue() { Name = "Name", Value = docID });
			//wikitivityRequestRDO.Fields.Add(new FieldValue() { Name = "Page Title", Value = pageTitle });
			try
			{
				await proxy.CreateAsync(workspaceID, massCreateRequest);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
	}
}

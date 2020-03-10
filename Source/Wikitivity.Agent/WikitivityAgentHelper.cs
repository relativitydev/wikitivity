using kCura.Agent;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using Newtonsoft.Json;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Threading.Tasks;
using ObjectType = kCura.Relativity.Client.DTOs.ObjectType;

namespace Wikitivity.Agent
{
	class WikitivityAgentHelper
	{
		public static string IAPIMessages = "";
		public static Guid WikitivityRDOGuid = new Guid("C6196733-E2A6-48F4-9443-37990972EBA3");
		
		public bool DeleteUploadedRequestsFromRelativityOM(IObjectManager proxy, int workspaceID, List<WikitivityUploadsAgent.SingleRequestObject> listOfCompletedUploads)
		{
			bool success = false;
			try
			{
				List<RelativityObjectRef> listofObjectstoDelete = new List<RelativityObjectRef>();
				foreach (var singleDoc in listOfCompletedUploads)
				{
					listofObjectstoDelete.Add(new RelativityObjectRef() { ArtifactID = singleDoc.ArtifactID });
				}

				var deleteRequest = new MassDeleteByObjectIdentifiersRequest();
				// Represents a list of RelativityObjects to be deleted.
				deleteRequest.Objects = listofObjectstoDelete;
				Task.Run(async () =>
					{
						await proxy.DeleteAsync(workspaceID, deleteRequest);
					}).Wait();
				return success = true;
			}
			catch (Exception ex)
			{
				throw new Exception("An error an occurred when deleting completed requests", ex);
			}
		}

		public static async Task<List<int>> FindWikitivityCasesOM(IObjectManager proxy, int workspaceID)
		{
			QueryResultSlim test = new QueryResultSlim();
			List<int> CasesWithWikitivity = new List<int>();

			QueryRequest omGetInitialJobQuery = new QueryRequest()
			{
				ObjectType = new ObjectTypeRef() { Guid = WikitivityRDOGuid },
				Condition = "('Request Url' ISSET)",
				IncludeIDWindow = false,
				RelationalField = null, 
				SampleParameters = null,
				SearchProviderCondition = null,
				Sorts = null,
				Fields = new List<FieldRef>()
				{
					new FieldRef() {Name = "Request Url"},
					new FieldRef() {Name = "Request ID"},
					new FieldRef() {Name = "Page Title"}

				}
			};
			try
			{
				test = await proxy.QuerySlimAsync(workspaceID, omGetInitialJobQuery, 1, 10000);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}

			foreach (var singleWorkspace in test.Objects)
			{
				CasesWithWikitivity.Add(singleWorkspace.ArtifactID);
			}
			return CasesWithWikitivity;
		}
		
		public static async Task<QueryResultSlim> FindAllWikiCases(IObjectManager proxy, int workspaceID)
		{


			QueryRequest omGetInitialJobQuery = new QueryRequest()
			{
				ObjectType = new ObjectTypeRef() { Guid = WikitivityRDOGuid },
				Condition = "('Request Url' ISSET)",
				IncludeIDWindow = false,
				RelationalField = null, //name of relational field to expand query results to related objects
				SampleParameters = null,
				SearchProviderCondition = null,
				Sorts = null,
				Fields = new List<FieldRef>()
				{
					new FieldRef() {Name = "Request Url"},
					new FieldRef() {Name = "Request ID"},
					new FieldRef() {Name = "Page Title"}
				}
			};

			try
			{
				QueryResultSlim queryResults = await proxy.QuerySlimAsync(workspaceID, omGetInitialJobQuery, 1, 10000);
				return queryResults;
			}
			catch (Exception ex)
			{
				return new QueryResultSlim();
			}
		}

		public static async Task<QueryResultSlim> FindAllRequestsbyID(IObjectManager proxy, int workspaceID, string requestGuid)
		{

			QueryRequest findAllRequestsbyIDQuery = new QueryRequest()
			{
				ObjectType = new ObjectTypeRef() { Guid = WikitivityRDOGuid },
				Condition = "('Request ID' IN ['"+requestGuid+"'])",
				IncludeIDWindow = false,
				RelationalField = null, //name of relational field to expand query results to related objects
				SampleParameters = null,
				SearchProviderCondition = null,
				Sorts = null,
				Fields = new List<FieldRef>()
				{
					new FieldRef() {Name = "Request Url"},
					new FieldRef() {Name = "Request ID"},
					new FieldRef() {Name = "Page Title"},
					new FieldRef() {Name = "Name"}
				}
			};

			try
			{
				QueryResultSlim queryResults = await proxy.QuerySlimAsync(workspaceID, findAllRequestsbyIDQuery, 1, 10000);
				return queryResults;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		/// <summary>
		/// No RSAPI code
		/// </summary>
		/// <param name="singleRequestObjects"></param>
		/// <param name="agentBase"></param>
		/// <returns></returns>
		public List<WikitivityUploadsAgent.DataObtainedSingleRequestObject> PopulateArticleData(List<WikitivityUploadsAgent.SingleRequestObject> singleRequestObjects, AgentBase agentBase)
		{
			List<WikitivityUploadsAgent.DataObtainedSingleRequestObject> ListForUpload = new List<WikitivityUploadsAgent.DataObtainedSingleRequestObject>();
			agentBase.RaiseMessage("Populating Article Data...", 1);

			try
			{
				foreach (var singlePage in singleRequestObjects)
				{
					var client = new WebClient();

					string newTitleURL = singlePage.RequestUrl;

					// DEBUG


					agentBase.RaiseMessage("Working on: " + singlePage.PageTitle, 1);

					//
					try
					{

						var response2 = client.DownloadString(newTitleURL);

						var response2Json = JsonConvert.DeserializeObject<WikiCategoryRequest2>(response2);

						foreach (var singleArticleWithInfo in response2Json.Query.pages.Values)
						{
							try
							{
								ListForUpload.Add(new WikitivityUploadsAgent.DataObtainedSingleRequestObject
								{
									extractedText = singleArticleWithInfo.extract,
									PageTitle = singleArticleWithInfo.title,
									ControlNumVal = singlePage.ControlNumVal
								});
							}
							catch (Exception e)
							{
								agentBase.RaiseMessage(e.ToString(), 1);
							}
						}
					}
					catch (Exception e)
					{
						agentBase.RaiseMessage(e.ToString(), 1);
					}
				}
				return ListForUpload;
			}
			catch (Exception e)
			{
				agentBase.RaiseMessage(e.ToString(), 1);
				return ListForUpload;
			}
		}

		public static string GetInstanceUrl(IAgentHelper helper, AgentBase aB)
		{
			try
			{
				aB.RaiseMessage("Attempting to get URL...", 1);
				//instanceUrl = helper.GetInstanceSettingBundle().GetString("Relativity.Core", "KeplerServicesUri");
				string instanceUrl = helper.GetInstanceSettingBundle().GetStringAsync("Relativity.Wikitivity", "WikitivityWebApi").Result;

				aB.RaiseMessage("Acquired URL: " + instanceUrl, 1);
				return instanceUrl;
			}
			catch (Exception e)
			{
				aB.RaiseMessage("Failed obtaining the webAPI value:\r\n " + e.ToString(), 1);
				throw;
			}



		}

		public static string GetWebApiUrl(IAgentHelper helper, AgentBase aB)
		{
			try
			{
				string baseUrl = GetInstanceUrl(helper, aB);
				baseUrl = baseUrl.ToLower();
				//baseUrl = baseUrl.Replace(".rest/api/", "webapi/");
				return baseUrl;
			}
			catch (Exception e)
			{

				aB.RaiseMessage("Failed obtaining the webAPI value:\r\n " + e.ToString(), 1);
				throw;
			}

		}
		public bool ImportDocument(IAgentHelper helper, int workspaceID, List<WikitivityUploadsAgent.DataObtainedSingleRequestObject> batchedArticleList, AgentBase aB)
		{
			var success = false;

			Int32 workspaceArtifactID = workspaceID;
			Int32 identifyFieldArtifactID = 1003667;    // 'Control Number' Field
			String relativityWebAPIUrl = GetWebApiUrl(helper, aB);
			aB.RaiseMessage("Obtained RelativityWebAPI: " + relativityWebAPIUrl, 1);
			String relativityFolderName = "Name of the Destination Folder";
			var url = relativityWebAPIUrl;
			ImportAPI iapi = ImportAPI.CreateByRsaBearerToken(url);

			try
			{
				var importJob = iapi.NewNativeDocumentImportJob();
				importJob.OnMessage += ImportJobOnMessage;
				importJob.OnComplete += ImportJobOnComplete;
				importJob.OnFatalException += ImportJobOnFatalException;
				importJob.Settings.CaseArtifactId = workspaceArtifactID;
				importJob.Settings.ExtractedTextFieldContainsFilePath = false;

				// Utilize these fields to set up native import
				// importJob.Settings.DisableNativeLocationValidation = true; Use these two lines for disabling native import & validation.
				// importJob.Settings.DisableNativeValidation = true;
				// importJob.Settings.NativeFilePathSourceFieldName = "Original Folder Path";
				importJob.Settings.NativeFileCopyMode = NativeFileCopyModeEnum.DoNotImportNativeFiles; // NativeFileCopyModeEnum.CopyFiles; NativeFileCopyModeEnum.DoNotImportNativeFiles
				importJob.Settings.OverwriteMode = OverwriteModeEnum.Append;
				importJob.Settings.IdentityFieldId = identifyFieldArtifactID;
				importJob.SourceData.SourceData = GetDocumentDataTable(batchedArticleList, aB).CreateDataReader();

				importJob.Execute();

				success = true;
			}
			catch (Exception ex)
			{
				aB.RaiseMessage(ex.ToString(), 1);
			}
			return success;
		}

		public static DataTable GetDocumentDataTable(List<WikitivityUploadsAgent.DataObtainedSingleRequestObject> batchedArticleEnum, AgentBase aB)
		{
			DataTable table = new DataTable();

			// The document identifer column name must match the field name in the workspace.
			table.Columns.Add("Control Number", typeof(string));
			table.Columns.Add("Extracted Text", typeof(string));
			table.Columns.Add("Article Title", typeof(string));


			foreach (var singleArticle in batchedArticleEnum)
			{
				table.Rows.Add(singleArticle.ControlNumVal, singleArticle.extractedText, singleArticle.PageTitle);
			}
			return table;
		}
		public static void ImportJobOnMessage(Status status)
		{
			IAPIMessages += status.Message;
		}

		public static void ImportJobOnFatalException(JobReport jobReport)
		{
			IAPIMessages += jobReport.ToString();
		}

		public static void ImportJobOnComplete(JobReport jobReport)
		{
			IAPIMessages += jobReport.ToString();
		}

		

		public async Task<List<int>> GetCasesWithWikitivityOM(IObjectManager proxy)
		{
			//TODO: Refactor to OM
			List<int> CasesWithWikitivity = new List<int>();

			QueryRequest getWorkspaceList = new QueryRequest()
			{
				ObjectType = new ObjectTypeRef() { Name = "Workspace" },
				Condition = "('ArtifactID' ISSET)",
				IncludeIDWindow = false,
				RelationalField = null, 
				SampleParameters = null,
				SearchProviderCondition = null,
				Sorts = null,
				Fields = new List<FieldRef>()
				{
					new FieldRef() {Name = "ArtifactID"},
				}
			};

			try
			{
				Relativity.Services.Objects.DataContracts.QueryResult queryResults = await proxy.QueryAsync(-1, getWorkspaceList, 0, 10000);
				foreach (var singleWorkspace in queryResults.Objects)
				{
					CasesWithWikitivity.Add(singleWorkspace.ArtifactID);
				}
				return CasesWithWikitivity;
			}
			catch (Exception ex)
			{
				throw ex;

			}
		}

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
			public string controlNumVal { get; set; }
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
	}
}
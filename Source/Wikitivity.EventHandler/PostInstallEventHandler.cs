using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace Wikitivity.EventHandler
{
	[RunOnce(true)]
	[RunTarget(kCura.EventHandler.Helper.RunTargets.Workspace)]
	[kCura.EventHandler.CustomAttributes.Description("Wikitivity_Post_Install")]
	[System.Runtime.InteropServices.Guid("584fae0a-bc8b-4574-8c92-e55bc7c84251")]
	public class PostInstallEventHandler : kCura.EventHandler.PostInstallEventHandler
	{
		public override Response Execute()
		{
			//TODO: Refactor to OM
			IAPILog logger = Helper.GetLoggerFactory().GetLogger();
			logger.LogVerbose("Log information throughout execution.");
            IFieldManager proxyFM = Helper.GetServicesManager().CreateProxy<IFieldManager>(ExecutionIdentity.System);
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

			Response retVal = new Response();
			retVal.Success = true;
			retVal.Message = string.Empty;
			try
			{
				Int32 currentWorkspaceArtifactID = this.Helper.GetActiveCaseID();

				using (IObjectManager proxyOM = Helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
				{
					//Add code for working with RSAPIClient
					var fieldMatchCount = QueryForArticleTitleField(proxyOM, currentWorkspaceArtifactID).Result;
					if (fieldMatchCount.TotalCount < 1)
					{
						bool createSuccess = CreateLongText_Async(proxyFM,FieldNamesList, currentWorkspaceArtifactID).Result;
						if (!createSuccess)
						{
							retVal.Success = false;
							retVal.Message = "Failed to create the field!";
							logger.LogInformation("Failed to create");
						}
					}
				}
			}
			catch (Exception ex)
			{
				retVal.Success = false;
				retVal.Message = ex.ToString();
			}

			return retVal;
		}


        public static async Task<QueryResultSlim> QueryForArticleTitleField(IObjectManager proxy, int workspaceID)
        {
            QueryRequest omGetInitialJobQuery = new QueryRequest()
            {
                ObjectType = new ObjectTypeRef() { Name = "Field"},
                Condition = "('Name == 'Article Title')",
                IncludeIDWindow = false,
                RelationalField = null,
                SampleParameters = null,
                SearchProviderCondition = null,
                Sorts = null,
                Fields = new List<FieldRef>()
                {
                    new FieldRef() {Name = "Name"}
                }
            };

            try
            {
                QueryResultSlim queryResults = await proxy.QuerySlimAsync(workspaceID, omGetInitialJobQuery, 1, 1);
                return queryResults;
            }
            catch (Exception ex)
            {
                return new QueryResultSlim();
            }
        }
		public static async Task<bool> CreateLongText_Async(IFieldManager proxy, List<String> fieldNames, int workspaceId)
        {
            bool success = false;
			foreach (var item in fieldNames)
			{
				try
				{
					LongTextFieldRequest request = new LongTextFieldRequest();
					request.ObjectType = new ObjectTypeIdentifier() { ArtifactTypeID = 10 };
					request.Name = item;
					request.AllowHtml = false;
					request.ALT = false;
					request.AvailableInViewer = false;
					request.CTRL = false;
					request.EnableDataGrid = false;
					request.HasUnicode = true;
					request.IncludeInTextIndex = false;
					request.OpenToAssociations = false;
					request.SHIFT = false;
					request.Wrapping = false;

					int newFieldArtifactId = await proxy.CreateLongTextFieldAsync(workspaceId, request);
					//string info = string.Format("Created Long text field with Artifact ID {0}", newFieldArtifactId);
                    
                }
				catch (Exception ex)
				{
					throw ex;
				}
			}

            success = true;
            return success;
        }
		// add any number of string field names here. These fields are created on the object created prior when the post-install executes.  
		public static List<string> FieldNamesList = new List<string> { "Article Title" };
    }
}
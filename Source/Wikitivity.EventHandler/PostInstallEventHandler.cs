using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;
using System;
using System.Net;
using Field = kCura.Relativity.Client.DTOs.Field;
using FieldType = kCura.Relativity.Client.FieldType;
using FieldValue = kCura.Relativity.Client.DTOs.FieldValue;
using ObjectType = kCura.Relativity.Client.DTOs.ObjectType;

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
			IAPILog logger = Helper.GetLoggerFactory().GetLogger();
			logger.LogVerbose("Log information throughout execution.");

			ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

			Response retVal = new Response();
			retVal.Success = true;
			retVal.Message = string.Empty;
			try
			{
				Int32 currentWorkspaceArtifactID = this.Helper.GetActiveCaseID();

				using (IRSAPIClient proxy = Helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System))
				{
					proxy.APIOptions.WorkspaceID = currentWorkspaceArtifactID;
					//Add code for working with RSAPIClient
					int fieldMatchCount = QueryForArticleTitleField(proxy, currentWorkspaceArtifactID);
					if (fieldMatchCount < 1)
					{
						int createSuccess = CreateDocumentFieldArticleTitle(proxy, currentWorkspaceArtifactID);
						if (createSuccess <= 0)
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

		public static int CreateDocumentFieldArticleTitle(IRSAPIClient proxy, int workspaceId)
		{
			Field createField = new Field()
			{
				Name = "Article Title",
				FieldTypeID = FieldType.LongText,
				IsRequired = false,
				Unicode = true,
				OpenToAssociations = true,
				Linked = false,
				AllowSortTally = true,
				Wrapping = true,
				AllowGroupBy = true,
				AllowPivot = true,
				IgnoreWarnings = true,
				IncludeInTextIndex = false,
				AvailableInViewer = false,
				AllowHTML = false,
				Width = "123",
				FilterType = "TextBox",
				ObjectType = new ObjectType
				{
					DescriptorArtifactTypeID = 10
				}
			};
			proxy.APIOptions.WorkspaceID = workspaceId;
			int artifactIdCreatedField = proxy.Repositories.Field.CreateSingle(createField);
			return artifactIdCreatedField;
		}
		public static int QueryForArticleTitleField(IRSAPIClient proxy, int workspaceId)
		{
			Query<Field> queryForArticleTitle = new Query<Field>
			{
				ArtifactTypeName = "Field",
				Condition = new TextCondition("Name", TextConditionEnum.EqualTo, "Article Title"),
				Fields = FieldValue.AllFields
			};
			proxy.APIOptions.WorkspaceID = workspaceId;
			var results = proxy.Repositories.Field.Query(queryForArticleTitle, 0);
			return results.TotalCount;

		}
	}
}
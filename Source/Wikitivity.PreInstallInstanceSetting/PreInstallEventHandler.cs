using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Services.Interfaces.InstanceSetting;
using Relativity.Services.Interfaces.InstanceSetting.Model;
using Relativity.Services.Objects;

namespace Wikitivity.PreInstallInstanceSetting
{
	[RunOnce(true)]
	[RunTarget(kCura.EventHandler.Helper.RunTargets.Workspace)]
	[kCura.EventHandler.CustomAttributes.Description("Create Wikitivity WebAPI Instance Setting")]
	[System.Runtime.InteropServices.Guid("b7918424-59a0-4374-af45-17e033270213")]
	public class PreInstallEventHandler : kCura.EventHandler.PreInstallEventHandler
	{
		public override Response Execute()
		{
			IAPILog logger = Helper.GetLoggerFactory().GetLogger();
			logger.LogVerbose("Log information throughout execution.");
			// Update Security Protocol
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

			//Construct a response object with default values.
			kCura.EventHandler.Response retVal = new kCura.EventHandler.Response();

			try
			{

				var proxy = Helper.GetServicesManager().CreateProxy<IInstanceSettingManager>(ExecutionIdentity.System);
				int operationResults = CreateWikitivityWebAPIInstanceSetting(proxy, retVal).Result;
				if (operationResults > 0)
				{
					retVal.Success = true;
					retVal.Message = "Successfully created the webAPI instance setting";
				}


			}
			catch (Exception ex)
			{
				//Change the response Success property to false to let the user know an error occurred
				retVal.Success = false;
				retVal.Message = ex.ToString();
			}

			return retVal;
		}

		public static async Task<int> CreateWikitivityWebAPIInstanceSetting(IInstanceSettingManager proxy, Response retVal)
		{
			int results = 0;

			try
			{
				InstanceSettingRequest createSetting = new InstanceSettingRequest()
				{
					Name = "WikitivityWebApi",
					Section = "kCura.Wikitivity",
					Description = "Set the Relativity Web API to be used by Wikitivity",
					ValueType = InstanceSettingValueTypeEnum.Text
				};
				results = await proxy.CreateAsync(-1, createSetting);

			}
			catch (Exception ex)
			{
				retVal.Success = false;
				retVal.Message = ex.ToString();
			}
			return results;
		}
	}
}
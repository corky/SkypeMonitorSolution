using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;

namespace SkypeAzureRestService
{
    //Public interface for defining your ASP.net WCF rest service
    [ServiceContract]
    public interface ISkypeStatus
    {
        //define the GET call for retreiving the current skype status
        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "currentstatus")]
        StatusData GetStatusData();

        //define the POST call for updating the current skype status 
        [OperationContract]
        [WebInvoke(UriTemplate = "currentstatus", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        void UpdateStatusData(StatusData statusData);
    }
}

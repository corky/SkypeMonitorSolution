using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

//THis is the implementation of the ASP.net WCF Rest service defined by the Interface
namespace SkypeAzureRestService
{
   public class SkypeStatus : ISkypeStatus
    {
        //create a local static variable to store the latest Skype status
        private static String myStatus = "";

        //GET /currentstatus 
        //This method will return the latest status that has been sent to the service
        public StatusData GetStatusData()
        {
            return new StatusData()
            {
                Status = myStatus
            };
        }

        //POST /currentstatus
        //This method will update the latest status to the value in the JSON payload sent to the service
        public void UpdateStatusData(StatusData statusData)
        {
            if (statusData != null)
            {
                if (statusData.Status != null)
                {
                    myStatus = statusData.Status;
                }
            }
        }
    }
}

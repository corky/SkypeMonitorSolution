using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//This class defines the data contract to the rest service.   Its  a simple object with one property ("status")
namespace SkypeAzureRestService
{
    public class StatusData
    {
        public string Status { get; set; }
    }
}

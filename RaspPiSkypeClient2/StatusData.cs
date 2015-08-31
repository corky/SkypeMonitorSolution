using System.Runtime.Serialization;

namespace RaspPiSkypeClient2
{
    [DataContract]
    public class StatusData
    {
        [DataMember(Name = "Status")]
        public string Status { get; set; }
    }
}

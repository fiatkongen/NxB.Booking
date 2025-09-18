using System;
using System.Collections.Generic;

namespace NxB.Dto.TenantApi
{
    public class POIStatus
    {
        public string SerialNumber { get; set; }
        public string POIID { get; set; }
        public string POIState { get; set; }
        public DateTime? LastDateTimeConnected { get; set; }
        public DateTime? LastDateTimeActive { get; set; }
        public string LastConnectedPOS { get; set; }
        public Guid EntityUID { get; set; }
        public string DeviceSoftwareVersion { get; set; }
    }

    public class POSStatus
    {
        public string SaleID { get; set; }
        public string POSState { get; set; }
        public DateTime LastDateTimeConnected { get; set; }
        public DateTime LastDateTimeActive { get; set; }
        public Guid EntityUID { get; set; }
    }

    public class VerifoneStatusDto
    {
        public class Response_
        {
            public string Result { get; set; }
            public object ErrorCondition { get; set; }
            public string AdditionalResponse { get; set; }
        }

        public Response_ Response { get; set; }
        public string POSCloudVersion { get; set; }
        public List<POIStatus> POIStatus { get; set; }
        public List<POSStatus> POSStatus { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public class MessageHeader
    {
        public string MessageClass { get; set; }
        public string MessageCategory { get; set; }
        public string MessageType { get; set; }
        public string ServiceID { get; set; }
        public string SaleID { get; set; }
        public string POIID { get; set; }
    }
     
    public class OutputText
    {
        public string Text { get; set; }
        public int? CharacterSet { get; set; }
        public string Font { get; set; }
        public int? StartRow { get; set; }
        public int? StartColumn { get; set; }
        public string Color { get; set; }
        public string CharacterWidth { get; set; }
        public string CharacterHeight { get; set; }
        public string CharacterStyle { get; set; }
        public string Alignment { get; set; }
        public bool? EndOfLineFlag { get; set; }
    }

    public class OutputContent
    {
        public string OutputFormat { get; set; }
        public object PredefinedContent { get; set; }
        public List<OutputText> OutputText { get; set; }
        public string OutputXHTML { get; set; }
        public OutputBarcode OutputBarcode { get; set; }
    }

    //public class MessageReference
    //{
    //    public string MessageCategory { get; set; }
    //    public string ServiceID { get; set; }
    //    public string DeviceID { get; set; }
    //    public string SaleID { get; set; }
    //    public string POIID { get; set; }
    //}

    public class MessageReference
    {
        public string MessageCategory { get; set; }
        public string ServiceID { get; set; }
        public string DeviceID { get; set; }
        public string SaleID { get; set; }
        public string POIID { get; set; }
    }
}
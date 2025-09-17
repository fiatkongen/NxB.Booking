using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    using System;

    public class AbortRequest
    {
        public MessageReference MessageReference { get; set; }
        public string AbortReason { get; set; }
        public DisplayOutput DisplayOutput { get; set; }
    }

    
    public class DisplayOutput
    {
        public bool ResponseRequiredFlag { get; set; }
        public int MinimumDisplayTime { get; set; }
        public string Device { get; set; }
        public string InfoQualify { get; set; }
        public OutputContent OutputContent { get; set; }
        public List<MenuEntry> MenuEntry { get; set; }
        public string OutputSignature { get; set; }
    }

    public class OutputBarcode
    {
        public string BarcodeType { get; set; }
        public string BarcodeValue { get; set; }
        public string QRCodeBinaryValue { get; set; }
        public string QRCodeErrorCorrection { get; set; }
        public string QRCodeVersion { get; set; }
        public string QRCodeEncodingMode { get; set; }
    }

    public class MenuEntry
    {
        public string MenuEntryTag { get; set; }
        public string OutputFormat { get; set; }
        public bool DefaultSelectedFlag { get; set; }
        public PredefinedContent PredefinedContent { get; set; }
        public List<OutputText> OutputText { get; set; }
        public string OutputXHTML { get; set; }
    }

    public class PredefinedContent
    {
        public string ReferenceID { get; set; }
        public string Language { get; set; }
    }


    public class VerifoneAbortRequest
    {
        public MessageHeader MessageHeader { get; set; }
        public AbortRequest AbortRequest { get; set; }
    }
}
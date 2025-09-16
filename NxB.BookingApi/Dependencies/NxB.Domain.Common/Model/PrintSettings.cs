namespace NxB.Domain.Common.Model
{
    public class PrintBasicSettings
    {
        public PrintBasicSettings PrimaryPrintSettings { get; set; }
        public int? MarginLeft;
        public int? MarginTop;
        public int? MarginRight;
        public int? MarginBottom;
        public bool? IsBlackWhite;
        public int? FontSize;
        public string PageSize { get; set; }
        public int? MaxHeight;
        public int? MaxWidth;

        public string GetMarginLeftString()
        {
            if (PrimaryPrintSettings?.MarginLeft != null) return PrimaryPrintSettings.GetMarginLeftString();
            return this.MarginLeft != null ? this.MarginLeft.ToString() : "10";
        }

        public string GetMarginTopString()
        {
            if (PrimaryPrintSettings?.MarginTop != null) return PrimaryPrintSettings.GetMarginTopString();
            return this.MarginTop != null ? this.MarginTop.ToString() : "10";
        }

        public string GetMarginRightString()
        {
            if (PrimaryPrintSettings?.MarginRight != null) return PrimaryPrintSettings.GetMarginRightString();
            return this.MarginRight != null ? this.MarginRight.ToString() : "10";
        }

        public string GetMarginBottomString()
        {
            if (PrimaryPrintSettings?.MarginBottom != null) return PrimaryPrintSettings.GetMarginBottomString();
            return this.MarginBottom != null ? this.MarginBottom.ToString() : "10";
        }

        public string GetIsBlackWhite()
        {
            if (PrimaryPrintSettings?.IsBlackWhite != null) return PrimaryPrintSettings.GetIsBlackWhite();
            return this.IsBlackWhite != null ? this.IsBlackWhite.ToString() : "false";
        }

        public string GetFontSizeString(int? overrideFontSize = null)
        {
            if (PrimaryPrintSettings?.FontSize != null) return PrimaryPrintSettings.GetFontSizeString();
            return (overrideFontSize ?? FontSize) + "pt";
        }

        public string GetPageSize()
        {
            if (PrimaryPrintSettings.PageSize != null) return PrimaryPrintSettings.GetPageSize();
            return string.IsNullOrWhiteSpace(this.PageSize) ? "A4" : this.PageSize;
        }
    }

    public class PrintSettingsExtended : PrintBasicSettings
    {
        public string HeaderLeft = "";
        public string HeaderRight = "";
        public string FooterLeft = "";
        public string FooterRight = "";
    }
}

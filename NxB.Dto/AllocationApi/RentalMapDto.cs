using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Dto.AllocationApi
{
    public class CreateRentalMapDto
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public int? Sort { get; set; }
        public bool IsAvailableOnline { get; set; }
        public MapType MapType { get; set; }

        public List<MapSymbolDto> MapSymbols { get; set; } = new();
    }

    public class RentalMapDto : CreateRentalMapDto
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }

        public Dictionary<string, dynamic> CssMap { get; set; } = new();
        public Dictionary<string, dynamic> CssSymbol { get; set; } = new();
        public Dictionary<string, dynamic> CssSymbolFree { get; set; } = new();
        public Dictionary<string, dynamic> CssSymbolOccupied { get; set; } = new();
    }

    public class MapSymbolDto
    {
        public Guid ResourceId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Text { get; set; }
        public bool IsHidden { get; set; }
        public string Description { get; set; }

        public Dictionary<string, dynamic> Css { get; set; } = new();
    }

    public class ModifyRentalMapDto : CreateRentalMapDto
    {
        public Guid Id { get; set; }
        public Dictionary<string, dynamic> CssSymbol { get; set; } = new();
        public Dictionary<string, dynamic> CssSymbolFree { get; set; } = new();
        public Dictionary<string, dynamic> CssSymbolOccupied { get; set; } = new();
    }

    public enum MapType
    {
        All = 0,
        OnlyInternal = 1,
        OnlyOnline = 2,
    }
}

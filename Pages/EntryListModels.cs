namespace App_FrontEnd.Pages
{
    public class EntryItem
    {
        public string CarNumber { get; set; } = string.Empty;
        public string Driver { get; set; } = string.Empty;
        public string CoDriver { get; set; } = string.Empty;
        public CarInfo Car { get; set; } = new();
        public string Registration { get; set; } = string.Empty;
        public string RaceClass { get; set; } = string.Empty;
        public int? StartingOrder { get; set; }
    }

    public class CarInfo
    {
        public string LogoUrl { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
    }

    public class EntryListResponse
    {
        public string RallyRowKey { get; set; } = string.Empty;
        public int Count { get; set; }
        public List<EntryItem> Items { get; set; } = new();
    }

    public class RallyProfileItem
    {
        public string RallyName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double LengthKm { get; set; }
        public int StageCount { get; set; }
        public int EntryCount { get; set; }
        public string RallyRowKey { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public List<StageInfo>? Stages { get; set; }
        public string? Country { get; set; }
        public string? Region { get; set; }
        public string? FlagUrl { get; set; }
        public string? LogoUrl { get; set; }
    }

    public class StageInfo
    {
        public int StageNumber { get; set; }
        public string StageName { get; set; } = string.Empty;
        public string StageLength { get; set; } = string.Empty;
        public string? FlagUrl { get; set; }
        public DateTimeOffset? StageDate { get; set; }
        public string? Leg { get; set; }
    }
}

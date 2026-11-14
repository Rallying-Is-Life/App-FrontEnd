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
        public int FinishCount { get; set; }
        public int RetireCount { get; set; }
    }

    public class StageInfo
    {
        public int StageNumber { get; set; }
        public string StageName { get; set; } = string.Empty;
        public string StageLength { get; set; } = string.Empty;
        public string StageType { get; set; } = string.Empty;
        public string? FlagUrl { get; set; }
        public DateTimeOffset? StageDate { get; set; }
        public string? Leg { get; set; }
    }

    // Leaderboard models
    public class LeaderboardItem
    {
        public int? OverallPosition { get; set; }
        public int? ClassPosition { get; set; }
        public string EntrantId { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public string CoDriverName { get; set; } = string.Empty;
        public string RaceNumber { get; set; } = string.Empty;
        public string RaceClass { get; set; } = string.Empty;
        public string CarMake { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;
        public int? TotalTimeSeconds { get; set; }
        public int StagesCompleted { get; set; }
        public int TotalStages { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? GapToOverallLeaderSeconds { get; set; }
        public int? GapToCarInFrontSeconds { get; set; }
        public int? GapToClassLeaderSeconds { get; set; }
        public int? GapToNextInClassSeconds { get; set; }
        public string DriverNat { get; set; } = string.Empty;
        public string CoDriverNat { get; set; } = string.Empty;
    }

    public class LeaderboardResponse
    {
        public string RallyRowKey { get; set; } = string.Empty;
        public int Count { get; set; }
        public List<LeaderboardItem> Items { get; set; } = new();
    }

    // Crew detail models
    public class StageTimeDetail
    {
        public int StageSequence { get; set; }
        public string StageName { get; set; } = string.Empty;
        public string StageType { get; set; } = string.Empty;
        public int? TimeSeconds { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RetirementReason { get; set; }
        public bool? BogeyTimeHit { get; set; }
        public bool? MaxTimeHit { get; set; }
        public int? BogeyTimeSeconds { get; set; }
        public int? MaxTimeSeconds { get; set; }
    }

    public class CrewDetailResponse
    {
        public string RallyRowKey { get; set; } = string.Empty;
        public string EntrantId { get; set; } = string.Empty;
        public int? TotalTimeSeconds { get; set; }
        public int StagesCompleted { get; set; }
        public int TotalStages { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<StageTimeDetail> Stages { get; set; } = new();
    }

    // Stage classification models
    public class StageClassificationItem
    {
        public int Position { get; set; }
        public string EntrantId { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public string CoDriverName { get; set; } = string.Empty;
        public string RaceNumber { get; set; } = string.Empty;
        public string RaceClass { get; set; } = string.Empty;
        public int? TimeSeconds { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class StageClassificationResponse
    {
        public int StageSequence { get; set; }
        public string StageName { get; set; } = string.Empty;
        public List<StageClassificationItem> Items { get; set; } = new();
    }

    // Retirements models
    public class RetirementItem
    {
        public string EntrantId { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public string CoDriverName { get; set; } = string.Empty;
        public string RaceNumber { get; set; } = string.Empty;
        public string RaceClass { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? RetirementReason { get; set; }
        public int StagesCompleted { get; set; }
    }

    public class RetirementsResponse
    {
        public string RallyRowKey { get; set; } = string.Empty;
        public int Count { get; set; }
        public List<RetirementItem> Items { get; set; } = new();
    }

    // For stage selector dropdown
    public class StageSelectorItem
    {
        public int StageNumber { get; set; }
        public string StageName { get; set; } = string.Empty;
    }
}
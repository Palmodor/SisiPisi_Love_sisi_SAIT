namespace LoveSushiPMR.Models.ViewModels
{
    public enum StatsPeriod
    {
        Week,
        Month
    }

    public class CourierStatsRowViewModel
    {
        public int CourierId { get; set; }
        public string CourierName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int DeliveredOrdersCount { get; set; }
        public decimal DeliveredOrdersSum { get; set; }
        public bool IsBonusEligible { get; set; }
        public int RemainingToBonus { get; set; }
    }

    public class CourierStatsViewModel
    {
        public StatsPeriod Period { get; set; } = StatsPeriod.Week;
        public DateTime DateFromUtc { get; set; }
        public DateTime DateToUtc { get; set; }
        public int MinDeliveriesForBonus { get; set; }
        public List<CourierStatsRowViewModel> Rows { get; set; } = new();
    }
}


namespace VehicleRepairTracker.Models
{
    public class RepairDetails
    {
        public int Id { get; set; }
        public DateTime? Date { get; set; } = DateTime.Now;
        public int VehicleId { get; set; }
        public KeyValuePair<int, string> Vehicle { get; set; }
        public int ShopId { get; set; }
        public KeyValuePair<int, string> Shop { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }
}

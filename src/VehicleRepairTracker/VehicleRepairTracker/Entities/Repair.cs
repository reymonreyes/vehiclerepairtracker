namespace VehicleRepairTracker.Entities
{
    public class Repair
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
        public int ShopId { get; set; }
        public Shop Shop { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }
}

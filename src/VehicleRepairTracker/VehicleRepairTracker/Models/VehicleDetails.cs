namespace VehicleRepairTracker.Models
{
    public class VehicleDetails
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
        public int? ManufacturerId { get; set; }
        public string? ManufacturerName { get; set; }
        public string? Model { get; set; }
        public string? Year { get; set; }
        public string? Description { get; set; }
    }
}

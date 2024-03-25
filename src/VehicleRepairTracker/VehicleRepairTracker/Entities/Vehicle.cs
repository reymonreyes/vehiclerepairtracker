namespace VehicleRepairTracker.Entities
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
        public int? ManufacturerId { get; set; }
        public Manufacturer? Manufacturer { get; set; }
        public string? Model { get; set; }
        public string? Year { get; set; }
        public string? Description { get; set; }
    }
}

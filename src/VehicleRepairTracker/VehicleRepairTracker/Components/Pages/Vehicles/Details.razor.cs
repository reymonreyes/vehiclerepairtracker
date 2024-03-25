using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using VehicleRepairTracker.Data;
using VehicleRepairTracker.Entities;
using VehicleRepairTracker.Models;

namespace VehicleRepairTracker.Components.Pages.Vehicles
{
    public partial class Details
    {
        VehicleDetails? _model = new VehicleDetails();
        MudForm _form;
        bool _isFormValid;
        List<KeyValuePair<int?, string>>? _manufacturers = new List<KeyValuePair<int?, string>>();

        [Inject]
        public ISnackbar Snackbar { get; set; }
        [Inject]
        public IDialogService DialogService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        [Inject]
        public IDbContextFactory<DataContext> DataContext { get; set; }
        [Parameter]
        public int Id { get; set; }

        protected override void OnInitialized()
        {
            Console.WriteLine("OnInitialized");

            _manufacturers = GetManufacturersList();

            if (Id > 0)
            {
                _model = GetVehicleDetails(Id);
            }
        }

        private List<KeyValuePair<int?, string>> GetManufacturersList()
        {
            using var dbContext = DataContext.CreateDbContext();
            var result = dbContext.Manufacturers.Select(manufacturer => new KeyValuePair<int?, string>(manufacturer.Id, manufacturer.Name)).ToList();
            result.Insert(0, new KeyValuePair<int?, string>(0, ""));

            return result;
        }

        private VehicleDetails? GetVehicleDetails(int id)
        {
            using var dbContext = DataContext.CreateDbContext();
            var vehicle = dbContext.Vehicles.Find(id);
            if (vehicle != null)
                return new VehicleDetails { Id = vehicle.Id, Name = vehicle.Name, Value = vehicle.Value, ManufacturerId = vehicle.ManufacturerId, Model = vehicle.Model, Year = vehicle.Year, Description = vehicle.Description };

            return null;
        }

        private async Task SaveAsync()
        {
            await _form.Validate();
            if (_isFormValid)
            {
                SaveVehicleDetails();
            }
        }

        private void SaveVehicleDetails()
        {
            try
            {
                using var dbContext = DataContext.CreateDbContext();
                Vehicle newVehicle = null;
                if (_model.Id == 0)
                {
                    newVehicle = new Vehicle
                    {
                        Name = _model.Name,
                        Value = _model.Value,
                        ManufacturerId = _model.ManufacturerId,
                        Model = _model.Model,
                        Year = _model.Year,
                        Description = _model.Description
                    };
                    dbContext.Vehicles.Add(newVehicle);
                }
                else
                {
                    var vehicle = dbContext.Vehicles.Find(_model.Id);
                    if (vehicle != null)
                    {
                        vehicle.Name = _model.Name;
                        vehicle.Value = _model.Value;
                        vehicle.ManufacturerId = _model.ManufacturerId;
                        vehicle.Model = _model.Model;
                        vehicle.Year = _model.Year;
                        vehicle.Description = _model.Description;
                    }
                }

                dbContext.SaveChanges();

                if (newVehicle != null)
                {
                    _model.Id = newVehicle.Id;
                    NavigationManager.NavigateTo($"vehicles/{newVehicle.Id}", false, true);
                }

                Snackbar.Add("Vehicle details saved successfully.", Severity.Success);
            }
            catch (Exception ex)
            {
                Snackbar.Add("An error occured while processing the request.", Severity.Error);
            }
        }

        private async Task DeleteAsync()
        {
            var response = await DialogService.ShowMessageBox("Delete Vehicle",
            (MarkupString)$"Are you sure you want to delete this vehicle?<br/><b>{_model.Name}</b>",
            yesText: "OK", cancelText: "Cancel");

            if (response.HasValue && response.Value)
            {
                using var dbContext = DataContext.CreateDbContext();
                var vehicle = dbContext.Vehicles.Find(_model.Id);
                if (vehicle != null)
                {
                    dbContext.Vehicles.Remove(vehicle);
                    dbContext.SaveChanges();
                    Snackbar.Add("Vehicle details deleted successfully.", Severity.Success);
                    NavigationManager.NavigateTo("/vehicles");
                }
            }
        }
    }
}

using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using VehicleRepairTracker.Data;
using VehicleRepairTracker.Models;

namespace VehicleRepairTracker.Components.Pages.Vehicles
{
    public partial class Vehicles
    {
        private List<VehicleDetails> _vehicles = new List<VehicleDetails>();
        private int _totalPages, _pageSize = 10, _page;
        private string _searchQuery = string.Empty;

        [Inject]
        public IDialogService DialogService { get; set; }
        [Inject]
        public ISnackbar Snackbar { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        [Inject]
        public IDbContextFactory<DataContext> DataContext { get; set; }

        protected override void OnInitialized()
        {
            LoadVehicles(1);
        }

        private void LoadVehicles(int page, string searchQuery = "")
        {
            _page = page;
            using var dbContext = DataContext.CreateDbContext();

            var query = dbContext.Vehicles.Include("Manufacturer").AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
                query = query.Where(x => x.Name.ToLower().Contains(_searchQuery));

            _vehicles = query.Skip((_page - 1) * _pageSize).Take(_pageSize).Select(x => new VehicleDetails
            {
                Id = x.Id,
                Name = x.Name,
                Value = x.Value,
                ManufacturerId = x.ManufacturerId,
                ManufacturerName = x.Manufacturer.Name,
                Model = x.Model,
                Year = x.Year,
                Description = x.Description
            }).ToList();

            _totalPages = (int)Math.Ceiling(query.Count() / (decimal)_pageSize);
        }

        private async Task Delete(VehicleDetails vehicle)
        {
            var response = await DialogService.ShowMessageBox("Delete Shop",
            (MarkupString)$"Are you sure you want to delete this vehicle?<br/><b>{vehicle.Name}</b>",
            yesText: "OK", cancelText: "Cancel");

            if (response.HasValue && response.Value)
            {
                using var dbContext = DataContext.CreateDbContext();
                var vehileEntity = dbContext.Vehicles.Find(vehicle.Id);
                if (vehileEntity != null)
                {
                    dbContext.Vehicles.Remove(vehileEntity);
                    dbContext.SaveChanges();
                    Snackbar.Add("Vehicle details deleted successfully.", Severity.Success);
                    LoadVehicles(_page);
                }
            }
        }

        private void PageChanged(int page)
        {
            _page = page;
            LoadVehicles(_page, _searchQuery);
        }

        private void SearchShops()
        {
            LoadVehicles(_page, _searchQuery);
        }
    }
}

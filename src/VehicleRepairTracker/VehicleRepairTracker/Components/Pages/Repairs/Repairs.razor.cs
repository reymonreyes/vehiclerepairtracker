using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using VehicleRepairTracker.Data;
using VehicleRepairTracker.Models;

namespace VehicleRepairTracker.Components.Pages.Repairs
{
    public partial class Repairs
    {
        private int _totalPages, _pageSize = 10, _page;
        private DateRange? _dateRangeFilter = null;
        private List<KeyValuePair<int, string>> _vehiclesList = new List<KeyValuePair<int, string>>();
        private KeyValuePair<int, string>? _vehicleFilter = null;
        private List<RepairDetails> _repairs = new List<RepairDetails>();
        [Inject]
        public IDialogService DialogService { get; set; }
        [Inject]
        public ISnackbar Snackbar { get; set; }
        [Inject]
        public IDbContextFactory<DataContext> DataContext { get; set; }

        protected override void OnInitialized()
        {
            LoadVehicles();
            LoadRepairs();
        }

        private void LoadVehicles()
        {
            using var dbContext = DataContext.CreateDbContext();
            var vehicles = dbContext.Vehicles.ToList();
            _vehiclesList = vehicles.Select(vehicle => new KeyValuePair<int, string>(vehicle.Id, vehicle.Name)).ToList();
        }

        private void FilterByDateRange(DateRange? dateRange)
        {
            _dateRangeFilter = dateRange;

            LoadRepairs();
        }

        private void FilterByVehicle(KeyValuePair<int, string> vehicleFilter)
        {
            _vehicleFilter = vehicleFilter;

            LoadRepairs();
        }

        private void LoadRepairs()
        {
            using var dbContext = DataContext.CreateDbContext();
            var query = dbContext.Repairs.AsQueryable();

            if (_dateRangeFilter != null && _dateRangeFilter.Start != null && _dateRangeFilter.End != null)
                query = query.Where(x => x.Date >= _dateRangeFilter.Start && x.Date <= _dateRangeFilter.End);

            if (_vehicleFilter != null && _vehicleFilter.Value.Key > 0)
                query = query.Where(x => x.VehicleId == _vehicleFilter.Value.Key);

            _repairs = query.Skip((_page - 1) * _pageSize).Take(_pageSize).Select(x => new RepairDetails
            {
                Id = x.Id,
                Date = x.Date,
                VehicleId = x.VehicleId,
                Vehicle = new KeyValuePair<int, string>(x.Vehicle.Id, x.Vehicle.Name),
                ShopId = x.ShopId,
                Shop = new KeyValuePair<int, string>(x.ShopId, x.Shop.Name),
                Amount = x.Amount,
                Description = x.Description
            }).ToList();

            _totalPages = (int)Math.Ceiling(query.Count() / (decimal)_pageSize);
        }

        private Task<IEnumerable<KeyValuePair<int, string>>> SearchVehicles(string vehicleName = "")
        {
            if (string.IsNullOrWhiteSpace(vehicleName))
                return Task.FromResult<IEnumerable<KeyValuePair<int, string>>>(new List<KeyValuePair<int, string>>());

            var result = _vehiclesList.Where(x => x.Value.ToLower().Contains(vehicleName.ToLower())).ToList();

            return Task.FromResult<IEnumerable<KeyValuePair<int, string>>>(result);
        }

        private void PageChanged(int page)
        {
            _page = page;
            LoadRepairs();
        }

        private async Task Delete(RepairDetails repairDetails)
        {
            var response = await DialogService.ShowMessageBox("Delete Repair Details",
            (MarkupString)@$"Are you sure you want to delete this repair details?<br/><b>{repairDetails.Date.Value.ToShortDateString()}</b><br/>
                {repairDetails.Vehicle.Value}<br/>{repairDetails.Shop.Value}",
            yesText: "OK", cancelText: "Cancel");

            if (response.HasValue && response.Value)
            {
                using var dbContext = DataContext.CreateDbContext();
                var repairDetailsEntity = dbContext.Repairs.Find(repairDetails.Id);
                if (repairDetailsEntity != null)
                {
                    dbContext.Repairs.Remove(repairDetailsEntity);
                    dbContext.SaveChanges();
                    Snackbar.Add("Repair details deleted successfully.", Severity.Success);
                    LoadRepairs();
                }
            }
        }
    }
}

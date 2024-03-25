using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using VehicleRepairTracker.Data;
using VehicleRepairTracker.Entities;
using VehicleRepairTracker.Models;

namespace VehicleRepairTracker.Components.Pages.Repairs
{
    public partial class Details
    {
        private RepairDetails? _model = new RepairDetails();
        private List<KeyValuePair<int, string>> _vehiclesList = new List<KeyValuePair<int, string>>();
        private bool _isFormValid;
        private MudForm _form;

        [Inject]
        public NavigationManager NavigationManager { get; set; }
        [Inject]
        public ISnackbar Snackbar { get; set; }
        [Inject]
        public IDialogService DialogService { get; set; }
        [Inject]
        public IDbContextFactory<DataContext> DataContext { get; set; }
        [Parameter]
        public int Id { get; set; }

        protected override void OnInitialized()
        {
            LoadVehicles();

            if (Id > 0)
                _model = LoadRepairDetails();
            //else
            //    _model.Date = DateTime.Now;
        }

        private RepairDetails? LoadRepairDetails()
        {
            using var dbContext = DataContext.CreateDbContext();
            var repair = dbContext.Repairs.Find(Id);
            if (repair != null)
            {
                var result = new RepairDetails { Id = repair.Id, Date = repair.Date, Amount = repair.Amount, VehicleId = repair.VehicleId, ShopId = repair.ShopId, Description = repair.Description };
                result.Vehicle = _vehiclesList.FirstOrDefault(x => x.Key == result.VehicleId);

                var shop = dbContext.Shops.Find(result.ShopId);
                if (shop != null)
                    result.Shop = new KeyValuePair<int, string>(shop.Id, shop.Name);

                return result;
            }

            return null;
        }

        private void LoadVehicles()
        {
            using var dbContext = DataContext.CreateDbContext();
            var vehicles = dbContext.Vehicles.ToList();
            _vehiclesList = vehicles.Select(vehicle => new KeyValuePair<int, string>(vehicle.Id, vehicle.Name)).ToList();
        }

        private Task<IEnumerable<KeyValuePair<int, string>>> SearchVehicles(string vehicleName = "")
        {
            if (string.IsNullOrWhiteSpace(vehicleName))
                return Task.FromResult<IEnumerable<KeyValuePair<int, string>>>(new List<KeyValuePair<int, string>>());

            var result = _vehiclesList.Where(x => x.Value.ToLower().Contains(vehicleName.ToLower())).ToList();

            return Task.FromResult<IEnumerable<KeyValuePair<int, string>>>(result);
        }

        private Task<IEnumerable<KeyValuePair<int, string>>> SearchShops(string shopName = "")
        {
            if (string.IsNullOrWhiteSpace(shopName))
                return Task.FromResult<IEnumerable<KeyValuePair<int, string>>>(new List<KeyValuePair<int, string>>());

            using var dbContext = DataContext.CreateDbContext();
            var shops = dbContext.Shops.Where(x => x.Name.ToLower().Contains(shopName.ToLower())).ToList();

            return Task.FromResult<IEnumerable<KeyValuePair<int, string>>>(shops.Select(shop => new KeyValuePair<int, string>(shop.Id, shop.Name)).ToList());
        }

        private async Task SaveAsync()
        {
            await _form.Validate();

            if (!_isFormValid)
                return;

            using var dbContext = DataContext.CreateDbContext();

            Repair newRepair = null;
            if (_model.Id == 0)
            {
                newRepair = new Repair
                {
                    Date = _model.Date.Value,
                    VehicleId = _model.Vehicle.Key,
                    ShopId = _model.Shop.Key,
                    Amount = _model.Amount,
                    Description = _model.Description
                };

                dbContext.Repairs.Add(newRepair);
            }
            else
            {
                var repair = dbContext.Repairs.Find(_model.Id);
                if (repair != null)
                {
                    repair.Date = _model.Date.Value;
                    repair.VehicleId = _model.Vehicle.Key;
                    repair.ShopId = _model.Shop.Key;
                    repair.Amount = _model.Amount;
                    repair.Description = _model.Description;
                }
            }

            dbContext.SaveChanges();

            if (newRepair != null)
            {
                _model.Id = newRepair.Id;
                NavigationManager.NavigateTo($"repairs/{newRepair.Id}", false, true);
            }

            Snackbar.Add("Vehicle details saved successfully.", Severity.Success);
        }

        private async Task DeleteAsync()
        {
            var response = await DialogService.ShowMessageBox("Delete Repair",
            (MarkupString)$"Are you sure you want to delete this repair?",
            yesText: "OK", cancelText: "Cancel");

            if (response.HasValue && response.Value)
            {
                using var dbContext = DataContext.CreateDbContext();
                var repair = dbContext.Repairs.Find(_model.Id);
                if (repair != null)
                {
                    dbContext.Repairs.Remove(repair);
                    dbContext.SaveChanges();
                    Snackbar.Add("Repair details deleted successfully.", Severity.Success);
                    NavigationManager.NavigateTo("/repairs");
                }
            }
        }

        private string Validate(KeyValuePair<int, string> value)
        {
            if (value.Value is null)
                return "Required";

            return null;
        }
    }
}

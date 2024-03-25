using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using VehicleRepairTracker.Data;
using VehicleRepairTracker.Entities;
using VehicleRepairTracker.Models;

namespace VehicleRepairTracker.Components.Pages.Shops
{
    public partial class Details
    {
        ShopDetails? _model = new ShopDetails();
        MudForm _form;
        bool _isFormValid;

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
            if (Id > 0)
            {
                _model = GetShopDetails(Id);
            }
        }

        private ShopDetails? GetShopDetails(int id)
        {
            using var dbContext = DataContext.CreateDbContext();
            var shop = dbContext.Shops.Find(id);
            if (shop != null)
                return new ShopDetails { Id = shop.Id, Name = shop.Name, Address = shop.Address, Phone = shop.Phone, ContactPerson = shop.ContactPerson };

            return null;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            Console.WriteLine($"OnAfterRender: {firstRender}");
        }

        protected override void OnParametersSet()
        {
            Console.WriteLine("OnParametersSet");
        }

        private async Task SaveAsync()
        {
            await _form.Validate();
            if (_isFormValid)
            {
                SaveShopDetails();
            }
        }

        private void SaveShopDetails()
        {
            try
            {
                using var dbContext = DataContext.CreateDbContext();
                Shop newShop = null;
                if (_model.Id == 0)
                {
                    newShop = new Shop
                    {
                        Name = _model.Name,
                        Address = _model.Address,
                        Phone = _model.Phone,
                        ContactPerson = _model.ContactPerson,
                    };
                    dbContext.Shops.Add(newShop);
                }
                else
                {
                    var shop = dbContext.Shops.Find(_model.Id);
                    if (shop != null)
                    {
                        shop.Name = _model.Name;
                        shop.Address = _model.Address;
                        shop.Phone = _model.Phone;
                        shop.ContactPerson = _model.ContactPerson;
                    }
                }

                dbContext.SaveChanges();
                if (newShop != null)
                {
                    _model.Id = newShop.Id;
                    NavigationManager.NavigateTo($"shops/{newShop.Id}", false, true);
                }

                Snackbar.Add("Shop details saved successfully.", Severity.Success);
            }
            catch (Exception ex)
            {
                Snackbar.Add("An error occured while processing the request.", Severity.Error);
            }
        }

        private async Task DeleteAsync()
        {
            var response = await DialogService.ShowMessageBox("Delete Shop",
            (MarkupString)$"Are you sure you want to delete this shop?<br/><b>{_model.Name}</b>",
            yesText: "OK", cancelText: "Cancel");

            if (response.HasValue && response.Value)
            {
                using var dbContext = DataContext.CreateDbContext();
                var shop = dbContext.Shops.Find(_model.Id);
                if (shop != null)
                {
                    dbContext.Shops.Remove(shop);
                    dbContext.SaveChanges();
                    Snackbar.Add("Shop details deleted successfully.", Severity.Success);
                    NavigationManager.NavigateTo("/shops");
                }
            }
        }
    }
}

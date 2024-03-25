using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using VehicleRepairTracker.Data;
using VehicleRepairTracker.Models;

namespace VehicleRepairTracker.Components.Pages.Shops
{
    public partial class Shops
    {
        private List<ShopDetails> _shops = new List<ShopDetails>();
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
            LoadShops(1);
        }

        private void LoadShops(int page, string searchQuery = "")
        {
            _page = page;
            using var dbContext = DataContext.CreateDbContext();

            var query = dbContext.Shops.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
                query = query.Where(x => x.Name.ToLower().Contains(_searchQuery));

            _shops = query.Skip((_page - 1) * _pageSize).Take(_pageSize).Select(x => new ShopDetails
            {
                Id = x.Id,
                Name = x.Name,
                Address = x.Address,
                Phone = x.Phone,
                ContactPerson = x.ContactPerson
            }).ToList();

            _totalPages = (int)Math.Ceiling(query.Count() / (decimal)_pageSize);
        }

        private async Task Delete(ShopDetails shop)
        {
            var response = await DialogService.ShowMessageBox("Delete Shop",
            (MarkupString)$"Are you sure you want to delete this shop?<br/><b>{shop.Name}</b>",
            yesText: "OK", cancelText: "Cancel");

            if (response.HasValue && response.Value)
            {
                using var dbContext = DataContext.CreateDbContext();
                var shopEntity = dbContext.Shops.Find(shop.Id);
                if (shopEntity != null)
                {
                    dbContext.Shops.Remove(shopEntity);
                    dbContext.SaveChanges();
                    Snackbar.Add("Shop details deleted successfully.", Severity.Success);
                    LoadShops(_page);
                }
            }
        }

        private void PageChanged(int page)
        {
            _page = page;
            LoadShops(_page, _searchQuery);
        }

        private void SearchShops()
        {
            LoadShops(_page, _searchQuery);
        }
    }
}

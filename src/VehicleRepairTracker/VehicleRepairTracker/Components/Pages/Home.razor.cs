using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using VehicleRepairTracker.Data;
using VehicleRepairTracker.Models;

namespace VehicleRepairTracker.Components.Pages
{
    public partial class Home
    {
        [Inject]
        public IDbContextFactory<DataContext> DataContext { get; set; }

        private int Index = -1;
        private List<VehicleRepairCostSum> _repairCostsByVehicle = new List<VehicleRepairCostSum>();
        public ChartOptions Options = new ChartOptions() { YAxisTicks = 10, YAxisLines = true, XAxisLines = true };
        public List<ChartSeries> Series = new List<ChartSeries>();

        public string[] XAxisLabels = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep" };

        protected override void OnInitialized()
        {
            LoadDailyRepairCostChart();
            LoadTop5VehiclesForLast7Days();
        }

        private void LoadDailyRepairCostChart()
        {
            string[] last7Days = new string[7];
            var currentDate = DateTime.Now;

            for (int i = 0; i < 7; i++)
            {
                if (i == 0)
                    last7Days[i] = currentDate.AddDays(i).ToShortDateString();

                last7Days[i] = currentDate.AddDays(-i).ToShortDateString();
            }

            last7Days = last7Days.Reverse().ToArray();

            var dbContext = DataContext.CreateDbContext();
            var repairs = dbContext.Repairs.AsQueryable();
            var repairsByDate = repairs.Where(x => x.Date >= currentDate.AddDays(-7) && x.Date <= currentDate).OrderBy(x => x.Date).GroupBy(x => x.Date).ToArray();
            var maxDays = 7;
            maxDays = repairsByDate.Length < maxDays ? repairsByDate.Length : maxDays;
            double[] dailyRepairTotals = new double[maxDays];
            for (int i = 0; i < maxDays; i++)
            {
                dailyRepairTotals[i] = (double)repairsByDate[i].Sum(x => x.Amount);
            }
            Series.Add(new ChartSeries() { Name = "Daily Repair Cost (last 7 days)", Data = dailyRepairTotals });
            XAxisLabels = last7Days;
        }

        private void LoadTop5VehiclesForLast7Days()
        {
            var currentDate = DateTime.Now;
            using var dbContext = DataContext.CreateDbContext();
            var repairs = dbContext.Repairs.Include("Vehicle").AsQueryable();
            var repairsByVehicle = repairs.Where(x => x.Date >= currentDate.AddDays(-7) && x.Date <= currentDate).GroupBy(x => x.VehicleId).ToArray();
            _repairCostsByVehicle = repairsByVehicle.Select(x => new VehicleRepairCostSum { Name = x.FirstOrDefault().Vehicle.Name, Value = x.FirstOrDefault().Vehicle.Value, TotalCost = x.Sum(s => s.Amount) })
                .OrderByDescending(o => o.TotalCost).Take(5).ToList();
        }
    }
}

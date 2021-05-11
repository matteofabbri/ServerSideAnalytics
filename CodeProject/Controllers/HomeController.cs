using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CodeProject.Models;
using ServerSideAnalytics;
using ServerSideAnalytics.Extensions;
using WebRequest = ServerSideAnalytics.WebRequest;

namespace CodeProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAnalyticStore _analyticStore;

        public HomeController(IAnalyticStore analyticStore)
        {
            _analyticStore = analyticStore;
        }

        public async Task<IActionResult> Index()
        {
            var from = DateTime.MinValue;
            var to = DateTime.MaxValue;

            var total = await _analyticStore.CountAsync(from, to);

            if (total == 0)
            {
                await LoadDemoData(_analyticStore);
            }

            var stat = new WebStat
            {
                TotalServed = await _analyticStore.CountAsync(from, to),
                UniqueVisitors = await _analyticStore.CountUniqueIdentitiesAsync(from, to),
                DailyAverage = await _analyticStore.DailyAverage(from, to),
                DailyServed = await _analyticStore.DailyServed(from, to),
                HourlyServed = await _analyticStore.HourlyServed(from, to),
                ServedByCountry = await _analyticStore.ServedByCountry(from, to),
                UrlServed = await _analyticStore.UrlServed(from, to),
                Requests = await _analyticStore.InTimeRange(DateTime.Now - TimeSpan.FromDays(1), DateTime.Now)
            };

            return View(stat);
        }

        private static async Task LoadDemoData(IAnalyticStore analyticStore)
        {
            var countries = Enum.GetValues(typeof(CountryCode)).Cast<CountryCode>().ToArray();
            var methods = new[] {"GET", "POST", "DELETE", "CHICKEN"};
            var urls = new[] {"/", "/home", "/contact", "/about"};
            var rand = new Random();

            for (int i = 0; i < 250; i++)
            {
                await analyticStore.StoreWebRequestAsync(new WebRequest
                {
                    CountryCode = countries[rand.Next(countries.Length-1)],
                    Identity = Guid.NewGuid().ToString("N"),
                    IsWebSocket = rand.Next()%2 == 0,
                    Method = methods[rand.Next(methods.Length-1)],
                    Path = urls[rand.Next(urls.Length - 1)],
                    Referer = "",
                    RemoteIpAddress = IPAddress.Loopback,
                    Timestamp = DateTime.Now - TimeSpan.FromSeconds(rand.Next(3600*24*12)),
                    UserAgent = "Demo data"
                });
            }

        }

        public async Task<ActionResult> Identity(string id)
        {
            return View(new WebStat
            {
                Identity = id,
                Requests = (await _analyticStore.RequestByIdentityAsync(id)).ToArray(),
            });
        }


        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }
    }
}

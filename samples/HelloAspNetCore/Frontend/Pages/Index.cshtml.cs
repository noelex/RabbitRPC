using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shared;

namespace Frontend.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public WeatherForecast[] Forecasts { get; set; } = Array.Empty<WeatherForecast>();

    public async Task OnGetAsync([FromServices]IWeatherForecastService weatherForecastService,CancellationToken cancellationToken=default)
    {
        Forecasts = await weatherForecastService.GetWeatherForecastsAsync(cancellationToken);
    }
}

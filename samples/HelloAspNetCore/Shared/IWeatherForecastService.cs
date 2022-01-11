using RabbitRPC;
using System.Threading;
using System.Threading.Tasks;

namespace Shared
{
    public interface IWeatherForecastService : IRabbitService
    {
        Task<WeatherForecast[]> GetWeatherForecastsAsync(CancellationToken cancellationToken = default);
    }
}

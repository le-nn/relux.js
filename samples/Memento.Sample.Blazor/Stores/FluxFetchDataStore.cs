using System.Collections.Immutable;
using System.Net.Http.Json;

namespace Memento.Sample.Blazor.Stores;

public record FluxFetchDataState {
    public ImmutableArray<WeatherForecast>? WeatherForecasts { get; init; }
}

public record FluxFetchDataCommands : Command {
    public record SetWeatherForecast(ImmutableArray<WeatherForecast> Items) : FluxFetchDataCommands;
}

public class FluxFetchDataStore : FluxStore<FetchDataState, FetchDataCommands> {
    readonly HttpClient _httpClient;

    public FluxFetchDataStore(
        HttpClient httpClient
    ) : base(() => new(), Reducer) {
        _httpClient = httpClient;
    }

    static FetchDataState Reducer(FetchDataState state, FetchDataCommands command) {
        return command switch {
            FetchDataCommands.SetWeatherForecast payload => state with {
                WeatherForecasts = payload.Items,
            },
            _ => throw new Exception("The command is not handled."),
        };
    }

    public async Task FetchAsync() {
        var forecasts = await _httpClient.GetFromJsonAsync<WeatherForecast[]>("sample-data/weather.json")
            ?? throw new Exception("Failed to fetch data.");
        Dispatch(new FetchDataCommands.SetWeatherForecast(forecasts.ToImmutableArray()));
    }
}
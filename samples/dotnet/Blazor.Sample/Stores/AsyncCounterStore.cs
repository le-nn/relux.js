using Memento.Core;
using System.Collections.Immutable;
using static Blazor.Sample.Stores.AsyncCounterCommands;

namespace Blazor.Sample.Stores;

public record AsyncCounterState {
    public int Count { get; init; } = 0;

    public bool IsLoading { get; init; } = false;

    public ImmutableArray<int> Histories { get; init; } = ImmutableArray.Create<int>();
}

public record AsyncCounterCommands : Command {
    public record IncrementAndEndLoading : AsyncCounterCommands;
    public record Increment : AsyncCounterCommands;
    public record SetCount(int Count) : AsyncCounterCommands;
    public record BeginLoading : AsyncCounterCommands;
}

public class AsyncCounterStore : Store<AsyncCounterState, AsyncCounterCommands> {
    public AsyncCounterStore() : base(() => new(), Reducer) { }

    static AsyncCounterState Reducer(AsyncCounterState state, AsyncCounterCommands command) {
        return command switch {
            IncrementAndEndLoading => state with {
                Count = state.Count + 1,
                IsLoading = false,
                Histories = state.Histories.Add(state.Count + 1),
            },
            SetCount payload => state with {
                Count = payload.Count,
            },
            Increment => state with {
                Count = state.Count + 1,
            },
            BeginLoading => state with {
                IsLoading = true,
            },
            _ => throw new CommandNotHandledException(command),
        };
    }

    public void CountUp() {
        Dispatch(new Increment());
    }

    public async Task CountUpAsync() {
        Dispatch(new BeginLoading());
        await Task.Delay(800);
        Dispatch(new IncrementAndEndLoading());
    }

    public void CountUpManyTimes(int count) {
        for (var i = 0; i < count; i++) {
            Dispatch(new Increment());
        }
    }

    public void SetCount(int c) {
        Dispatch(new SetCount(c));
    }
}
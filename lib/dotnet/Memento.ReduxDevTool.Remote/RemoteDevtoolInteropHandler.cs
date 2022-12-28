﻿using Fleck;
using Memento.Core;
using Memento.ReduxDevTool.Internal;
using Newtonsoft.Json.Linq;
using ScClient;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Memento.ReduxDevTool.Remote;

internal class RemoteDevtoolInteropHandler : IDevtoolInteropHandler, IAsyncDisposable {
    readonly string _instanceId = Guid.NewGuid().ToString();
    readonly DevtoolWebSocketConnection _webSocketConnection;


    public RemoteDevtoolInteropHandler(DevtoolWebSocketConnection webSocketConnection) {
        _webSocketConnection = webSocketConnection;

        _webSocketConnection.MessageHandled += (id, json) => {
            if (IsMatch(id)) {
                MessageHandled?.Invoke(json);
            }
        };
        _webSocketConnection.SyncRequested += id => {
            if (IsMatch(id)) {
                SyncRequested?.Invoke();
            }
        };
        _webSocketConnection.SendStartRequested += () => {
            _ = _webSocketConnection.SendStartAsync(_instanceId);
        };

        _webSocketConnection.HandshakeRequested += cid => {
            _ = _webSocketConnection.ReplyHandshakeAsync(cid, _instanceId);
        };
    }

    public Action<string>? MessageHandled { get; set; }

    public Action? SyncRequested { get; set; }


    public void HandleMessage(string json) {
        MessageHandled?.Invoke(json);
    }

    public async Task InitializeAsync(ImmutableDictionary<string, object> state) {
        await _webSocketConnection.InitializeAsync(state);
    }

    public async Task SendAsync(Command? command, HistoryStateContextJson context) {
        await _webSocketConnection.SendAsync(_instanceId, context);
    }

    public async ValueTask DisposeAsync() {
        await _webSocketConnection.DisconnectAsync(_instanceId);
    }

    bool IsMatch(string id) => id == $"sc-{_instanceId}";
}
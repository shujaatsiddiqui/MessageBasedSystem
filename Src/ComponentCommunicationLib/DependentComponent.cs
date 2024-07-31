using MessageBasedService.Model;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace ComponentCommunicationLib
{
    public class DependentComponent
    {
        private readonly HubConnection _hubConnection;
        private readonly string _componentId;
        public string ComponentId { get { return _componentId; } }

        public DependentComponent(string url, string componentId)
        {
            _componentId = componentId;
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(url)
                .Build();

            _hubConnection.On<MessagePayload>("ReceiveMessage", HandleMessage);
        }

        private async Task HandleMessage(MessagePayload payload)
        {
            Console.WriteLine(JsonConvert.SerializeObject(payload));
            if(payload == null) return;
            if(payload.State == State.Completed)
            {
                //var recipientId = payload.Header.SenderId;
                MessagePayload messagePayload = new MessagePayload();
                messagePayload.Header = new Header(payload.Header.CorrelationId);
                messagePayload.Header.MessageId = Guid.NewGuid().ToString();
                messagePayload.Header.SenderId = _componentId;
                messagePayload.Header.RecipientId = payload.Header.SenderId;
                messagePayload.State = State.Ok;
                await SendMessageAsync(messagePayload);
            }
        }

        public async Task StartAsync()
        {
            await _hubConnection.StartAsync();
            await _hubConnection.SendAsync("RegisterComponent", _componentId);
        }

        public async Task StopAsync()
        {
            await _hubConnection.SendAsync("UnregisterComponent", _componentId);
            await _hubConnection.StopAsync();
        }

        public async Task SendMessageAsync(MessagePayload payload)
        {
            await _hubConnection.SendAsync("SendMessageToComponent", payload);
        }
    }
}
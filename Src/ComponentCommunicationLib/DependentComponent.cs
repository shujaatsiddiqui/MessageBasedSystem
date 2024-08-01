using MessageBasedService.Model;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace ComponentCommunicationLib
{
    public class DependentComponent
    {
        private readonly HubConnection _hubConnection;
        private readonly string _componentId;
        bool _processQueue = false;
        public string ComponentId { get { return _componentId; } }
        PriorityQueue<MessagePayload, int> _priorityQueue; // new PriorityQueue<MessagePayload, int>();

        public DependentComponent(string url, string componentId, PriorityQueue<MessagePayload, int> priorityQueue)
        {
            _componentId = componentId;
            _priorityQueue = priorityQueue;
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(url)
                .Build();

            _hubConnection.On<MessagePayload>("ReceiveMessage", EnqueuePayload);
        }

        private async Task EnqueuePayload(MessagePayload payload)
        {
            await Task.Run(() =>
            {
                Console.WriteLine(JsonConvert.SerializeObject(payload));
                if (payload == null) return;
                _priorityQueue.Enqueue(payload, (int)payload.Priority);
            });
        }

        public async Task StartAsync()
        {
            await _hubConnection.StartAsync();
            await _hubConnection.SendAsync("RegisterComponent", _componentId);
            await _startProcessingQueue();
            _processQueue = true;
        }

        private async Task _startProcessingQueue()
        {
            while (_processQueue)
            {
                while (_priorityQueue.Count > 0)
                {
                    if (!_processQueue)
                        break;
                    var payload = _priorityQueue.Dequeue();
                    await HandleResponse(payload);
                }
                Thread.Sleep(1000); // wait for one second and start processing again.
            }
        }

        private async Task HandleResponse(MessagePayload payload)
        {
            if (payload.State == State.Completed)
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
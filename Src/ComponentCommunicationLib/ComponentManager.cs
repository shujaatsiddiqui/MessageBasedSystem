using MessageBasedService.Model;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComponentCommunicationLib
{
    /// <summary>
    /// Core component class
    /// </summary>
    public class ComponentManager
    {
        ILogger<ComponentManager> logger;
        private readonly List<DependentComponent> _dependentComponents = new List<DependentComponent>();
        private readonly HubConnection _hubConnection;
        private readonly string _hubUrl;
        private readonly string _coreComponentId = "CoreComponent";
        PriorityQueue<MessagePayload, int> corePriorityQueue = new PriorityQueue<MessagePayload, int>();
        Dictionary<string, PriorityQueue<MessagePayload, int>> dicPriorityQueue = new Dictionary<string, PriorityQueue<MessagePayload, int>>();
        bool _processQueue = false;
        public string CoreComponentId
        {
            get
            {
                return _coreComponentId;
            }
        }
        public int NumbersOfDependentComponent { get; private set; }

        #region Constructor
        public ComponentManager(string hubUrl, int numbersOfDependentComponent)
        {
            _hubUrl = hubUrl;
            NumbersOfDependentComponent = Math.Max(numbersOfDependentComponent, 3); // Ensure minimum value is 3

            _hubConnection = new HubConnectionBuilder()
             .WithUrl(hubUrl)
            .Build();

            _hubConnection.On<MessagePayload>("ReceiveMessage", EnqueuePayload);

            CreateDependentComponents();
        }
        #endregion

        #region Public

        public async Task StartAllComponentsAsync()
        {
            await _hubConnection.StartAsync();
            await _hubConnection.SendAsync("RegisterComponent", _coreComponentId);
            foreach (var component in _dependentComponents)
            {
                await component.StartAsync();
            }
            _processQueue = true;
            dicPriorityQueue.Add(CoreComponentId, corePriorityQueue);
            _startProcessingQueue();
        }

        public async Task StopAllComponentsAsync()
        {
            await _hubConnection.StartAsync();
            await _hubConnection.SendAsync("UnRegisterComponent", _coreComponentId);
            foreach (var component in _dependentComponents)
            {
                await component.StopAsync();
            }
            _processQueue = false;
        }

        public async Task SendMessageToAllDependentComponentsAsync(MessagePayload message)
        {
            foreach (var component in _dependentComponents)
            {
                message.Header.RecipientId = component.ComponentId;
                await SendMessageAsync(message);
            }
        }

        public async Task SendMessageAsync(MessagePayload payload)
        {
            await _hubConnection.SendAsync("SendMessageToComponent", payload);
        }

        /// <summary>
        /// Get a dependent component by its ID
        /// </summary>
        /// <param name="componentId"></param>
        /// <returns></returns>
        public DependentComponent GetDependentComponentById(string componentId)
        {
            return _dependentComponents.FirstOrDefault(c => c.ComponentId == componentId);
        }
        #endregion

        #region EventHandler
        private async Task EnqueuePayload(MessagePayload payload)
        {
            await Task.Run(() =>
            {
                Console.WriteLine(JsonConvert.SerializeObject(payload));
                if (payload == null) return;
                corePriorityQueue.Enqueue(payload, (int)payload.Priority);
            });
        }
        #endregion

        #region Private
        private void CreateDependentComponents()
        {
            for (int i = 1; i <= NumbersOfDependentComponent; i++)
            {
                PriorityQueue<MessagePayload, int> priorityQueue = new PriorityQueue<MessagePayload, int>();
                var componentId = $"Dep_Component{i}";
                var component = new DependentComponent(_hubUrl, componentId, priorityQueue);
                _dependentComponents.Add(component);
                dicPriorityQueue.Add(componentId, priorityQueue);
            }
        }

        private async Task _startProcessingQueue()
        {
            while (_processQueue)
            {
                while (corePriorityQueue.Count > 0)
                {
                    if (!_processQueue)
                        break;
                    var payload = corePriorityQueue.Dequeue();
                    await HandleResponse(payload);
                }
                Thread.Sleep(1000); // wait for one second and start processing again.
            }
        }

        private async Task HandleResponse(MessagePayload payload)
        {
            // can further modify to handle different states
            await Task.Run(() =>
            {
                Console.WriteLine(JsonConvert.SerializeObject(payload));
                return Task.CompletedTask;
            });
        }
        #endregion
    }
}

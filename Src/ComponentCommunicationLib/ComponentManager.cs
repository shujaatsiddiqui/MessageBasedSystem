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
        public string CoreComponentId { get { 
                return _coreComponentId;
            } }
        public int NumbersOfDependentComponent { get; private set; }

        #region Constructor
        public ComponentManager(string hubUrl, int numbersOfDependentComponent)
        {
            _hubUrl = hubUrl;
            NumbersOfDependentComponent = Math.Max(numbersOfDependentComponent, 3); // Ensure minimum value is 3

            _hubConnection = new HubConnectionBuilder()
             .WithUrl(hubUrl)
            .Build();

            _hubConnection.On<MessagePayload>("ReceiveMessage", HandleMessage);

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
        }

        public async Task StopAllComponentsAsync()
        {
            await _hubConnection.StartAsync();
            await _hubConnection.SendAsync("UnRegisterComponent", _coreComponentId);
            foreach (var component in _dependentComponents)
            {
                await component.StopAsync();
            }
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
        #endregion

        #region EventHandler
        private async Task HandleMessage(MessagePayload payload)
        {
            Console.WriteLine(JsonConvert.SerializeObject(payload));
        }
        #endregion

        #region Private
        private void CreateDependentComponents()
        {
            for (int i = 1; i <= NumbersOfDependentComponent; i++)
            {
                var componentId = $"Dep_Component{i}";
                var component = new DependentComponent(_hubUrl, componentId);
                _dependentComponents.Add(component);
            }
        } 
        #endregion
    }
}

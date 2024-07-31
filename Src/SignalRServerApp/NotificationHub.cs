using MessageBasedService.Model;
using Microsoft.AspNetCore.SignalR;


public class NotificationHub : Hub
{
    public async Task SendMessageToComponent(MessagePayload payload)
    {
        // Send message to a specific component
        await Clients.Group(payload.Header.RecipientId).SendAsync("ReceiveMessage", payload);
    }

    public async Task RegisterComponent(string componentId)
    {
        // Add the component to a group
        await Groups.AddToGroupAsync(Context.ConnectionId, componentId);
    }

    public async Task UnregisterComponent(string componentId)
    {
        // Remove the component from a group
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, componentId);
    }
}



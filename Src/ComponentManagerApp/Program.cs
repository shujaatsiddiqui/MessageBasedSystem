// See https://aka.ms/new-console-template for more information

using ComponentCommunicationLib;
using MessageBasedService.Model;
using System.Data;



var hubUrl = "https://localhost:5001/notificationHub";
var componentManager = new ComponentManager(hubUrl, 3); // Specify the number of dependent components

await componentManager.StartAllComponentsAsync();

//Console.WriteLine("All components started. You can send messages.");
Thread.Sleep(3000);
//wait for 3 seconds

MessagePayload payload = new MessagePayload();
payload.Header = new Header()
{
    CorrelationId = Guid.NewGuid().ToString(),
    SenderId = componentManager.CoreComponentId,
    MessageId = Guid.NewGuid().ToString()
};
payload.Payload = "All components started. You can send messages";
payload.CommandType = MessageCommandType.AllComponentsStarted;
payload.State = State.Completed;
await componentManager.SendMessageToAllDependentComponentsAsync(payload);


//while (true)
//{
//    //var message = Console.ReadLine();
   
//    Thread.Sleep(10000);
//}
Console.ReadLine();

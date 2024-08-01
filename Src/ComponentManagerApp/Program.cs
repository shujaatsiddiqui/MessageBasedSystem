// See https://aka.ms/new-console-template for more information

using ComponentCommunicationLib;
using MessageBasedService.Model;
using Microsoft.Extensions.Configuration;


var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Read values from the configuration
var hubUrl = configuration["CoreCompSettings:HubUrl"];
var numbersOfDependentComponent = int.Parse(configuration["CoreCompSettings:NumbersOfDependentComponent"]);

// Use the configuration values
Console.WriteLine($"Hub URL: {hubUrl}");
Console.WriteLine($"Number of Dependent Components: {numbersOfDependentComponent}");


var componentManager = new ComponentManager(hubUrl, numbersOfDependentComponent); // Specify the number of dependent components

await componentManager.StartAllComponentsAsync();

//Console.WriteLine("All components started. You can send messages.");
Thread.Sleep(3000);
//wait for 3 seconds

#region SendingMessageFromCoreToAllDepComp
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
#endregion

#region SendMessageDirectlyBetweenDepComponents
var dependentComp1 = componentManager.GetDependentComponentById("Dep_Component1");
// send message to dep component 2
payload = new MessagePayload();
payload.Header = new Header()
{
    CorrelationId = Guid.NewGuid().ToString(),
    SenderId = dependentComp1.ComponentId,
    RecipientId = "Dep_Component2",
    MessageId = Guid.NewGuid().ToString()
};
payload.Payload = "Hi";
//payload.CommandType = MessageCommandType.;
payload.State = State.Completed;

await dependentComp1.SendMessageAsync(payload); 
#endregion


//while (true)
//{
//    //var message = Console.ReadLine();

//    Thread.Sleep(10000);
//}
Console.ReadLine();

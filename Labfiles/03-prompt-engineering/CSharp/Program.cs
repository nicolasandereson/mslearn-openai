// Implicit using statements are included
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Azure;

// Add Azure OpenAI package
using Azure.AI.OpenAI;


// Build a config object and retrieve user settings.
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();
string? oaiEndpoint = config["AzureOAIEndpoint"];
string? oaiKey = config["AzureOAIKey"];
string? oaiDeploymentName = config["AzureOAIDeploymentName"];

bool printFullResponse = false;
/// <summary>
/// Pauses the application to allow changes to the system prompt, reads the updated system message from "system.txt",
/// and processes user input to communicate with the OpenAI API. Continues until the user types 'quit'.
/// </summary>

do
{
    // Pause for system message update
    Console.WriteLine("-----------\nPausing the app to allow you to change the system prompt.\nPress any key to continue...");
    Console.ReadKey();

    Console.WriteLine("\nUsing system message from system.txt");
    /// <summary>
    /// Reads the contents of the "system.txt" file and stores it in a string variable.
    /// </summary>
    string systemMessage = System.IO.File.ReadAllText("system.txt");
    systemMessage = systemMessage.Trim();

    Console.WriteLine("\nEnter user message or type 'quit' to exit:");
    string userMessage = Console.ReadLine() ?? "";
    userMessage = userMessage.Trim();

    if (systemMessage.ToLower() == "quit" || userMessage.ToLower() == "quit")
    {
        break;
    }
    else if (string.IsNullOrEmpty(systemMessage) || string.IsNullOrEmpty(userMessage))
    {
        Console.WriteLine("Please enter a system and user message.");
        continue;
    }
    else
    {
        await GetResponseFromOpenAI(systemMessage, userMessage);
    }
} while (true);

/// <summary>
/// Sends the system and user messages to the Azure OpenAI endpoint, retrieves the response,
/// and prints the response to the console. Validates the configuration settings before making the request.
/// </summary>
/// <param name="systemMessage">The system message read from the "system.txt" file.</param>
/// <param name="userMessage">The user message input from the console.</param>
/// <returns>A Task representing the asynchronous operation.</returns>

async Task GetResponseFromOpenAI(string systemMessage, string userMessage)
{
    Console.WriteLine("\nSending prompt to Azure OpenAI endpoint...\n\n");

    if (string.IsNullOrEmpty(oaiEndpoint) || string.IsNullOrEmpty(oaiKey) || string.IsNullOrEmpty(oaiDeploymentName))
    {
        Console.WriteLine("Please check your appsettings.json file for missing or incorrect values.");
        return;
    }

    // Configure the Azure OpenAI client
    OpenAIClient client = new OpenAIClient(new Uri(oaiEndpoint), new AzureKeyCredential(oaiKey));


    // Format and send the request to the model
    var chatCompletionsOptions = new ChatCompletionsOptions()
    {
        Messages =
     {
         new ChatRequestSystemMessage(systemMessage),
         new ChatRequestUserMessage(userMessage)
     },
        Temperature = 0.7f,
        MaxTokens = 800,
        DeploymentName = oaiDeploymentName
    };

    // Get response from Azure OpenAI
    Response<ChatCompletions> response = await client.GetChatCompletionsAsync(chatCompletionsOptions);


    ChatCompletions completions = response.Value;
    string completion = completions.Choices[0].Message.Content;

    // Write response full response to console, if requested
    if (printFullResponse)
    {
        Console.WriteLine($"\nFull response: {JsonSerializer.Serialize(completions, new JsonSerializerOptions { WriteIndented = true })}\n\n");
    }

    // Write response to console
    Console.WriteLine($"\nResponse:\n{completion}\n\n");
}
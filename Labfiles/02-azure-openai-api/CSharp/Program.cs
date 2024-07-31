// Implicit using statements are included
using Microsoft.Extensions.Configuration;
using Azure;

// Add Azure OpenAI package
using Azure.AI.OpenAI;


// Build the configuration object from the appsettings.json file
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

// Retrieve the Azure OpenAI endpoint from the configuration
string? oaiEndpoint = config["AzureOAIEndpoint"];
// Retrieve the Azure OpenAI key from the configuration
string? oaiKey = config["AzureOAIKey"];
// Retrieve the Azure OpenAI deployment name from the configuration
string? oaiDeploymentName = config["AzureOAIDeploymentName"];

// Check if any of the required configuration values are missing or empty
if (string.IsNullOrEmpty(oaiEndpoint) || string.IsNullOrEmpty(oaiKey) || string.IsNullOrEmpty(oaiDeploymentName))
{
    // Print an error message and exit if any configuration values are missing
    Console.WriteLine("Please check your appsettings.json file for missing or incorrect values.");
    return;
}

// Initialize the Azure OpenAI client with the endpoint and key
OpenAIClient client = new OpenAIClient(new Uri(oaiEndpoint), new AzureKeyCredential(oaiKey));

// Define a system message to provide context to the model
string systemMessage = "I am a hiking enthusiast named Forest who helps people discover hikes in their area. If no area is specified, I will default to near Rainier National Park. I will then provide three suggestions for nearby hikes that vary in length. I will also share an interesting fact about the local nature on the hikes when making a recommendation.";

do
{
    // Prompt the user to enter their prompt text
    Console.WriteLine("Enter your prompt text (or type 'quit' to exit): ");
    // Read the user's input
    string? inputText = Console.ReadLine();
    // Exit the loop if the user types 'quit'
    if (inputText == "quit") break;

    // Check if the user entered a prompt
    if (inputText == null)
    {
        // Prompt the user to enter a prompt if none was provided
        Console.WriteLine("Please enter a prompt.");
        continue;
    }

    // Inform the user that the request is being sent to the Azure OpenAI endpoint
    Console.WriteLine("\nSending request for summary to Azure OpenAI endpoint...\n\n");

    // Build the completion options object for the request
    ChatCompletionsOptions chatCompletionsOptions = new ChatCompletionsOptions()
    {
        // Add the system message and user message to the request
        Messages =
        {
            new ChatRequestSystemMessage(systemMessage),
            new ChatRequestUserMessage(inputText),
        },
        // Set the maximum number of tokens for the response
        MaxTokens = 400,
        // Set the temperature for the response generation
        Temperature = 0.7f,
        // Set the deployment name for the request
        DeploymentName = oaiDeploymentName
    };

    // Send the request to the Azure OpenAI model and get the response
    ChatCompletions response = client.GetChatCompletions(chatCompletionsOptions);

    // Get the content of the response message
    string completion = response.Choices[0].Message.Content;
    // Print the response to the console
    Console.WriteLine("Response: " + completion + "\n");

} while (true); // Continue the loop until the user types 'quit'

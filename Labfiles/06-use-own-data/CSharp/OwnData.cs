using Microsoft.Extensions.Configuration;
using Azure;

// Add Azure OpenAI package
using Azure.AI.OpenAI;

// Flag to show citations
bool showCitations = false;

// Get configuration settings  
IConfiguration config = new ConfigurationBuilder()
    // Add settings from appsettings.json file
    .AddJsonFile("appsettings.json")
    // Build the configuration
    .Build();

// Retrieve Azure OpenAI endpoint from configuration
string oaiEndpoint = config["AzureOAIEndpoint"] ?? "";
// Retrieve Azure OpenAI key from configuration
string oaiKey = config["AzureOAIKey"] ?? "";
// Retrieve Azure OpenAI deployment name from configuration
string oaiDeploymentName = config["AzureOAIDeploymentName"] ?? "";
// Retrieve Azure Search endpoint from configuration
string azureSearchEndpoint = config["AzureSearchEndpoint"] ?? "";
// Retrieve Azure Search key from configuration
string azureSearchKey = config["AzureSearchKey"] ?? "";
// Retrieve Azure Search index name from configuration
string azureSearchIndex = config["AzureSearchIndex"] ?? "";

// Initialize the Azure OpenAI client
OpenAIClient client = new OpenAIClient(new Uri(oaiEndpoint), new AzureKeyCredential(oaiKey));

// Prompt the user to enter a question
Console.WriteLine("Enter a question:");
// Read the user's input
string text = Console.ReadLine() ?? "";

// Configure your data source for Azure Search
AzureSearchChatExtensionConfiguration ownDataConfig = new()
{
    // Set the search endpoint
    SearchEndpoint = new Uri(azureSearchEndpoint),
    // Set the authentication options
    Authentication = new OnYourDataApiKeyAuthenticationOptions(azureSearchKey),
    // Set the index name
    IndexName = azureSearchIndex
};

// Inform the user that the request is being sent
Console.WriteLine("...Sending the following request to Azure OpenAI endpoint...");
// Display the request text
Console.WriteLine("Request: " + text + "\n");

// Configure the options for the chat completions request
ChatCompletionsOptions chatCompletionsOptions = new ChatCompletionsOptions()
{
    // Set the messages for the request
    Messages =
    {
        new ChatRequestUserMessage(text)
    },
    // Set the maximum number of tokens for the response
    MaxTokens = 600,
    // Set the temperature for the response
    Temperature = 0.9f,
    // Set the deployment name
    DeploymentName = oaiDeploymentName,
    // Specify extension options
    AzureExtensionsOptions = new AzureChatExtensionsOptions()
    {
        // Add the data source configuration
        Extensions = { ownDataConfig }
    }
};

// Send the request to the Azure OpenAI model and get the response
ChatCompletions response = client.GetChatCompletions(chatCompletionsOptions);
// Get the message from the response
ChatResponseMessage responseMessage = response.Choices[0].Message;

// Print the response content
Console.WriteLine("Response: " + responseMessage.Content + "\n");
// Print the intent from the response context
Console.WriteLine("  Intent: " + responseMessage.AzureExtensionsContext.Intent);

// If citations should be shown
if (showCitations)
{
    // Print the citations header
    Console.WriteLine($"\n  Citations of data used:");

    // Iterate through each citation in the response context
    foreach (AzureChatExtensionDataSourceResponseCitation citation in responseMessage.AzureExtensionsContext.Citations)
    {
        // Print the citation title and URL
        Console.WriteLine($"    Citation: {citation.Title} - {citation.Url}");
    }
}


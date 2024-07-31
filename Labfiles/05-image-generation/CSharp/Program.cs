using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace generate_image
{
    class Program
    {
        // Declare a nullable string variable to hold the Azure OpenAI endpoint
        private static string? aoaiEndpoint;
        // Declare a nullable string variable to hold the Azure OpenAI key
        private static string? aoaiKey;
        
        static async Task Main(string[] args)
        {
            try
            {
                // Get config settings from AppSettings
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                // Build the configuration object
                IConfigurationRoot configuration = builder.Build();
                // Retrieve the Azure OpenAI endpoint from the configuration
                aoaiEndpoint = configuration["AzureOAIEndpoint"] ?? "";
                // Retrieve the Azure OpenAI key from the configuration
                aoaiKey = configuration["AzureOAIKey"] ?? "";
        
                // Clear the console screen
                Console.Clear();
                // Prompt the user to enter a prompt for image generation
                Console.WriteLine("Enter a prompt to request an image:");
                // Read the user's input
                string prompt = Console.ReadLine() ?? "";
        
                // Call the DALL-E model
                using (var client = new HttpClient())
                {
                    // Set the content type to application/json
                    var contentType = new MediaTypeWithQualityHeaderValue("application/json");
                    // Define the API endpoint for image generation
                    var api = "openai/deployments/dalle3/images/generations?api-version=2024-02-15-preview";
                    // Set the base address of the HTTP client
                    client.BaseAddress = new Uri(aoaiEndpoint);
                    // Add the content type to the request headers
                    client.DefaultRequestHeaders.Accept.Add(contentType);
                    // Add the API key to the request headers
                    client.DefaultRequestHeaders.Add("api-key", aoaiKey);
        
                    // Create the data object to be sent in the request body
                    var data = new
                    {
                        prompt = prompt, // The prompt entered by the user
                        n = 1,           // Number of images to generate
                        size = "1024x1024" // Size of the generated image
                    };
        
                    // Serialize the data object to JSON
                    var jsonData = JsonSerializer.Serialize(data);
                    // Create the content for the HTTP request
                    var contentData = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    // Send the POST request to the API endpoint
                    var response = await client.PostAsync(api, contentData);
        
                    // Get the response content as a string
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    // Parse the response content to a JSON node
                    JsonNode contentNode = JsonNode.Parse(stringResponse)!;
                    // Get the data collection node from the response
                    JsonNode dataCollectionNode = contentNode!["data"];
                    // Get the first data node from the collection
                    JsonNode dataNode = dataCollectionNode[0]!;
                    // Get the revised prompt from the data node
                    JsonNode revisedPrompt = dataNode!["revised_prompt"];
                    // Get the image URL from the data node
                    JsonNode url = dataNode!["url"];
                    // Print the revised prompt to the console
                    Console.WriteLine(revisedPrompt.ToJsonString());
                    // Print the image URL to the console, replacing escaped characters
                    Console.WriteLine(url.ToJsonString().Replace(@"\u0026", "&"));
                }
            }
            catch (Exception ex)
            {
                // Print any exceptions to the console
                Console.WriteLine(ex.Message);
            }
        }
    }

}

namespace BulkCarnageIQ.Infrastructure.ExternalServices
{
    using BulkCarnageIQ.Core.Carnage;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;

    /// <summary>
    /// Service to interact with the Food Finder external API.
    /// </summary>
    /// <remarks>
    /// This is a simplified example. In a real-world scenario, you would handle
    /// authentication, error handling, logging, and possibly rate limiting.
    /// </remarks>
    public class FoodFinderService
    {
        private readonly HttpClient _httpClient;

        public FoodFinderService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new System.Uri("https://api.foodfinder.com/"); // Example base address
        }

        public async Task<FoodItem?> SearchFoodAsync(string foodName)
        {
            var response = await _httpClient.GetFromJsonAsync<FoodItem>($"search?query={foodName}");
            return response;
        }
    }
}
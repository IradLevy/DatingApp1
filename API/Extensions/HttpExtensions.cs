using System.Text.Json;
using API.Helpers;

namespace API.Extensions
{
    public static class HttpExtensions
    {
        public static void AddPaginationHeader(this HttpResponse response, PaginationHeader header)
        {
            var jsonOptions = new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase};
            // Add an header for pagination with a json that contains the PaginationHeader
            response.Headers.Add("Pagination", JsonSerializer.Serialize(header, jsonOptions));
            // Add cors for pagination header
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }

    }
}
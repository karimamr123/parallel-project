using System;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    private static readonly HttpClient client = new HttpClient();

    static async Task Main(string[] args)
    {
        // Replace these placeholders with your actual values
        
        string apiUrl = $"https://graph.facebook.com/v12.0/17841447127103880?fields=id,username&access_token=EAASDR6ICHTcBOzoMFbyheIgF0YbEHz7GM4TdZCCARskW0wnzkfdQvsPpYWwKovuqZBYRWWcrsEDD7DHWTeE6CILHM0BFnMcNabSbglXmPyM4BeppefJdm622oL24h64ZALN8oQ4lkdOfzXnTPIgr5ZA4jRYURFtVTYlr3gZBWBpSkTufwqwZAOn0ov2NWwPy1Q";

        try
        {
            Console.WriteLine("Fetching Instagram account data...");

            var response = await client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode(); // Throw exception if status is not 200
            var jsonResponse = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Response:");
            Console.WriteLine(jsonResponse);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("Request error:");
            Console.WriteLine(e.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("General error:");
            Console.WriteLine(ex.Message);
        }
    }
}

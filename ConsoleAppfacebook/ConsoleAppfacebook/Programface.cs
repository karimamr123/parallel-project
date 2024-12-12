using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SocialMediaFetcherApp
{
    public class Post
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    public class SocialMediaFetcher
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task<List<Post>> FetchFacebookPostsAsync()
        {
            try
            {
                string url = "https://graph.facebook.com/v17.0/497339600134456/posts?access_token=EAASDR6ICHTcBOzoMFbyheIgF0YbEHz7GM4TdZCCARskW0wnzkfdQvsPpYWwKovuqZBYRWWcrsEDD7DHWTeE6CILHM0BFnMcNabSbglXmPyM4BeppefJdm622oL24h64ZALN8oQ4lkdOfzXnTPIgr5ZA4jRYURFtVTYlr3gZBWBpSkTufwqwZAOn0ov2NWwPy1Q";
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to fetch posts. Status code: {response.StatusCode}");
                    return new List<Post>();
                }

                var jsonData = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(jsonData))
                {
                    Console.WriteLine("No data received from the Facebook API.");
                    return new List<Post>();
                }

                Console.WriteLine("Received JSON: " + jsonData); // Log raw JSON data

                return ParsePosts(jsonData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Facebook posts: {ex.Message}");
                return new List<Post>();
            }
        }

        private List<Post> ParsePosts(string jsonData)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<FacebookResponse>(jsonData, options);

                if (data == null || data.Data == null)
                {
                    Console.WriteLine("Error: No posts data in the response.");
                    return new List<Post>();
                }

                var posts = new List<Post>();
                foreach (var item in data.Data)
                {
                    var post = new Post
                    {
                        Id = item.Id,
                        Content = item.Message ?? "No message available",
                        CreatedTime = DateTime.TryParse(item.CreatedTime, out DateTime createdTime) ? createdTime : DateTime.MinValue
                    };

                    posts.Add(post);
                }

                return posts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing Facebook posts: {ex.Message}");
                return new List<Post>();
            }
        }
    }

    public class FacebookResponse
    {
        public List<FacebookPost> Data { get; set; }
    }

    public class FacebookPost
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public string CreatedTime { get; set; }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Fetching Facebook posts...");

            var fetcher = new SocialMediaFetcher();
            var posts = await fetcher.FetchFacebookPostsAsync();

            Console.WriteLine("Posts fetched:");

            if (posts.Count == 0)
            {
                Console.WriteLine("No posts found.");
            }
            else
            {
                foreach (var post in posts)
                {
                    Console.WriteLine($"[{post.CreatedTime}] {post.Content}");
                }
            }
        }
    }
}
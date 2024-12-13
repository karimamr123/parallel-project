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

        // Fetch posts from Facebook
        public async Task<List<Post>> FetchFacebookPostsAsync(string facebookPageId, string accessToken)
        {
            try
            {
                string url = $"https://graph.facebook.com/v17.0/{facebookPageId}/posts?access_token={accessToken}";
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

                return ParseFacebookPosts(jsonData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Facebook posts: {ex.Message}");
                return new List<Post>();
            }
        }

        private List<Post> ParseFacebookPosts(string jsonData)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<FacebookResponse>(jsonData, options);

                if (data?.Data == null)
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

        // Fetch posts from Instagram
        public async Task FetchInstagramPostsAsync(string accessToken)
        {
            try
            {
                string url = $"https://graph.facebook.com/v12.0/17841447127103880/media?fields=id,media_type,media_url,thumbnail_url,caption&access_token={accessToken}";
                var response = await client.GetAsync(url);
                var jsonData = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to fetch Instagram media. Status code: {response.StatusCode}");
                    Console.WriteLine($"Error response: {jsonData}");
                    return;
                }

                ParseInstagramMedia(jsonData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Instagram posts: {ex.Message}");
            }
        }

        private void ParseInstagramMedia(string jsonData)
        {
            try
            {
                var mediaResponse = JsonSerializer.Deserialize<JsonElement>(jsonData);

                if (!mediaResponse.TryGetProperty("data", out var mediaData))
                {
                    Console.WriteLine("Error: No media data found in the response.");
                    return;
                }

                foreach (var mediaItem in mediaData.EnumerateArray())
                {
                    string postId = string.Empty;
                    string mediaUrl = string.Empty;
                    string timestamp = string.Empty;

                    // Safely get the properties if they exist
                    if (mediaItem.TryGetProperty("id", out var idProperty))
                        postId = idProperty.GetString();

                    if (mediaItem.TryGetProperty("media_url", out var mediaUrlProperty))
                        mediaUrl = mediaUrlProperty.GetString();

                    if (mediaItem.TryGetProperty("timestamp", out var timestampProperty))
                        timestamp = timestampProperty.GetString();

                    // Display the properties
                    Console.WriteLine($"Post ID: {postId}");
                    Console.WriteLine($"Media URL: {mediaUrl}");
                    Console.WriteLine($"Timestamp: {timestamp}");

                    // Ensure timestamp is in a DateTime format
                    if (DateTime.TryParse(timestamp, out DateTime postTimestamp))
                    {
                        Console.WriteLine($"Post Timestamp: {postTimestamp}");
                    }
                    else
                    {
                        Console.WriteLine($"Invalid timestamp for Post ID: {postId}");
                    }

                    Console.WriteLine("----------------------------");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing Instagram media: {ex.Message}");
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
                Console.WriteLine("Fetching social media posts...");

                var fetcher = new SocialMediaFetcher();

                // Facebook configuration
                string facebookPageId = "497339600134456"; // Replace with your Facebook Page ID
                string facebookAccessToken = "EAASDR6ICHTcBOzoMFbyheIgF0YbEHz7GM4TdZCCARskW0wnzkfdQvsPpYWwKovuqZBYRWWcrsEDD7DHWTeE6CILHM0BFnMcNabSbglXmPyM4BeppefJdm622oL24h64ZALN8oQ4lkdOfzXnTPIgr5ZA4jRYURFtVTYlr3gZBWBpSkTufwqwZAOn0ov2NWwPy1Q";

                var facebookPosts = await fetcher.FetchFacebookPostsAsync(facebookPageId, facebookAccessToken);

                Console.WriteLine("Facebook Posts:");
                if (facebookPosts.Count == 0)
                {
                    Console.WriteLine("No Facebook posts found.");
                }
                else
                {
                    foreach (var post in facebookPosts)
                    {
                        Console.WriteLine($"[{post.CreatedTime}] {post.Content}");
                    }
                }

                // Instagram configuration
                string instagramAccessToken = "EAASDR6ICHTcBOzoMFbyheIgF0YbEHz7GM4TdZCCARskW0wnzkfdQvsPpYWwKovuqZBYRWWcrsEDD7DHWTeE6CILHM0BFnMcNabSbglXmPyM4BeppefJdm622oL24h64ZALN8oQ4lkdOfzXnTPIgr5ZA4jRYURFtVTYlr3gZBWBpSkTufwqwZAOn0ov2NWwPy1Q"; // Replace with your Instagram Access Token
                Console.WriteLine("\nFetching Instagram posts...");
                await fetcher.FetchInstagramPostsAsync(instagramAccessToken);
            }
        }
    }
}

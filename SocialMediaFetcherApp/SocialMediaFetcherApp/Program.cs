using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
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
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Enter the number of threads:");
                if (!int.TryParse(Console.ReadLine(), out int numberOfThreads) || numberOfThreads <= 0)
                {
                    Console.WriteLine("Invalid number of threads. Please enter a positive integer.");
                    return;
                }

                Console.WriteLine("Fetching social media posts in parallel...");

                var fetcher = new SocialMediaFetcher();

                // Facebook configuration
                string facebookPageId = "497339600134456"; // Replace with your Facebook Page ID
                string facebookAccessToken = "EAASDR6ICHTcBOzoMFbyheIgF0YbEHz7GM4TdZCCARskW0wnzkfdQvsPpYWwKovuqZBYRWWcrsEDD7DHWTeE6CILHM0BFnMcNabSbglXmPyM4BeppefJdm622oL24h64ZALN8oQ4lkdOfzXnTPIgr5ZA4jRYURFtVTYlr3gZBWBpSkTufwqwZAOn0ov2NWwPy1Q"; // Replace with your actual access token

                // Fetch all posts first
                var facebookPosts = await fetcher.FetchFacebookPostsAsync(facebookPageId, facebookAccessToken);

                if (facebookPosts.Count == 0)
                {
                    Console.WriteLine("No Facebook posts found.");
                    return;
                }

                // Divide posts among threads
                int chunkSize = (int)Math.Ceiling((double)facebookPosts.Count / numberOfThreads);
                var tasks = new List<Task>();

                for (int i = 0; i < numberOfThreads; i++)
                {
                    int threadIndex = i;

                    // Assign a chunk of posts to this thread
                    var threadPosts = facebookPosts
                        .Skip(threadIndex * chunkSize)
                        .Take(chunkSize)
                        .ToList();

                    tasks.Add(Task.Run(() =>
                    {
                        Console.WriteLine($"Thread {threadIndex + 1} started.");

                        foreach (var post in threadPosts)
                        {
                            lock (Console.Out) // Synchronize console output
                            {
                                Console.WriteLine($"Thread {threadIndex + 1}: [{post.CreatedTime}] {post.Content}");
                            }
                        }

                        Console.WriteLine($"Thread {threadIndex + 1} finished.");
                    }));
                }

                await Task.WhenAll(tasks);

                Console.WriteLine("\nFinished fetching social media posts.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            // Wait for user input to keep the console window open
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}

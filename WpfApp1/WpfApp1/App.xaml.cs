using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private SocialMediaFetcher fetcher;

        public MainWindow()
        {
            InitializeComponent();
            fetcher = new SocialMediaFetcher();
        }

        private async void FetchPostsButton_Click(object sender, RoutedEventArgs e)
        {
            string facebookPageId = facebookPageIdTextBox.Text;
            string instagramUserId = instagramUserIdTextBox.Text;
            string accessToken = accessTokenTextBox.Text;

            var fetchFacebookPostsTask = fetcher.FetchFacebookPostsAsync(facebookPageId, accessToken);
            var fetchInstagramPostsTask = fetcher.FetchInstagramPostsAsync(instagramUserId, accessToken);

            await Task.WhenAll(fetchFacebookPostsTask, fetchInstagramPostsTask);

            var facebookPosts = await fetchFacebookPostsTask;
            var instagramPosts = await fetchInstagramPostsTask;

            DisplayPosts(facebookPosts, facebookPostsTextBox);
            DisplayPosts(instagramPosts, instagramPostsTextBox);
        }

        private void DisplayPosts(List<Post> posts, TextBox textBox)
        {
            textBox.Clear();
            if (posts.Count == 0)
            {
                textBox.AppendText("No posts found.\n");
                return;
            }

            foreach (var post in posts)
            {
                textBox.AppendText($"[{post.CreatedTime}] {post.Content}\n\n");
            }
        }
    }

    public class SocialMediaFetcher
    {
        private static readonly HttpClient client = new HttpClient();

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

        public async Task<List<Post>> FetchInstagramPostsAsync(string instagramUserId, string accessToken)
        {
            try
            {
                string url = $"https://graph.facebook.com/v12.0/{instagramUserId}/media?fields=id,media_type,media_url,thumbnail_url,caption&access_token={accessToken}";
                var response = await client.GetAsync(url);
                var jsonData = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to fetch Instagram media. Status code: {response.StatusCode}");
                    Console.WriteLine($"Error response: {jsonData}");
                    return new List<Post>();
                }

                return ParseInstagramMedia(jsonData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Instagram posts: {ex.Message}");
                return new List<Post>();
            }
        }

        private List<Post> ParseInstagramMedia(string jsonData)
        {
            try
            {
                var mediaResponse = JsonSerializer.Deserialize<JsonElement>(jsonData);

                if (!mediaResponse.TryGetProperty("data", out var mediaData))
                {
                    Console.WriteLine("Error: No media data found in the response.");
                    return new List<Post>();
                }

                var posts = new List<Post>();
                foreach (var mediaItem in mediaData.EnumerateArray())
                {
                    string postId = mediaItem.GetProperty("id").GetString();
                    string mediaUrl = mediaItem.TryGetProperty("media_url", out var mediaUrlProperty) ? mediaUrlProperty.GetString() : "No media URL available";
                    string timestamp = mediaItem.TryGetProperty("timestamp", out var timestampProperty) ? timestampProperty.GetString() : "No timestamp available";
                    string caption = mediaItem.TryGetProperty("caption", out var captionProperty) ? captionProperty.GetString() : "No caption available";

                    // Ensure timestamp is in a DateTime format
                    DateTime createdTime = DateTime.TryParse(timestamp, out var createdTimeResult) ? createdTimeResult : DateTime.MinValue;

                    var post = new Post
                    {
                        Id = postId,
                        Content = $"{caption}\nMedia URL: {mediaUrl}",
                        CreatedTime = createdTime
                    };

                    posts.Add(post);
                }

                return posts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing Instagram media: {ex.Message}");
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

    public class Post
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}

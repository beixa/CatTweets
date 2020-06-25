﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TweetSharp;

namespace CatTweets
{
    class Program
    {
        private static readonly string apiURL = "https://api.giphy.com/v1/gifs/random?api_key=" + ConfigurationManager.AppSettings.Get("apiKey") + "&tag=cat&rating=R";

        private static readonly string randomGif = @"C:\Users\beiti\Desktop\Dev\CatTweets\CatTweets\Images\random.gif";

        private static TwitterService service = new TwitterService(ConfigurationManager.AppSettings.Get("customerKey"), ConfigurationManager.AppSettings.Get("customerSecretKey"));

        static void Main(string[] args)
        {
            service.AuthenticateWith(ConfigurationManager.AppSettings.Get("accessToken"), ConfigurationManager.AppSettings.Get("accessSecretToken"));

            var url = GetCatGifAsync(apiURL).GetAwaiter().GetResult();
            sendMediaTweet("meow!", url);
            Console.ReadKey();
        }

        static async Task<string> GetCatGifAsync(string path)
        {
            using (var httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.GetAsync(path);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    dynamic gif = JsonConvert.DeserializeObject(json);
                    return gif.data.images.downsized_large.url;
                }
            }
            return "<ERROR!!> ";
        }

        static void sendMediaTweet(string status, string url)
        {
            /* I need to download the gif, I cannot get to run the code below to create an online stream to the gif URL
                var req = WebRequest.Create(url);
                using (var stream = req.GetResponse().GetResponseStream())
             */

            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri(url), randomGif);
            }

            using (var stream = new FileStream(randomGif, FileMode.Open))
            {
                var Media = service.UploadMedia(new UploadMediaOptions()
                {
                    Media = new MediaFile()
                    {
                        FileName = url,
                        Content = stream
                    }
                });

                List<string> MediaIds = new List<string>();
                MediaIds.Add(Media.Media_Id);

                service.SendTweet(new SendTweetOptions
                {
                    Status = status,
                    MediaIds = MediaIds
                });
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Tweet sent!");
            Console.ResetColor();
        }
    }
}
using Movies.Client.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Client.Services
{
    public class StreamService : IIntegrationService
    {
        private static HttpClient _httpClient = new HttpClient();

        public StreamService()
        {
            // set up HttpClient instance
            _httpClient.BaseAddress = new Uri("http://localhost:57863");
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
            _httpClient.DefaultRequestHeaders.Clear();
        }
        public async Task Run()
        {
            // await GetPosterWithStream();
            // await GetPosterWithStreamAndCompletionMode();
            await PostPosterWithStream();
        }          

        private async Task GetPosterWithStream()
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                response.EnsureSuccessStatusCode();

                using (var streamReader = new StreamReader(stream))
                {
                    using (var jsonTextReader = new JsonTextReader(streamReader))
                    {
                        var jsonSerializer = new JsonSerializer();
                        var poster = jsonSerializer.Deserialize<Poster>(jsonTextReader);
                    }

                }
            }
        }

        private async Task GetPosterWithStreamAndCompletionMode()
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
               $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request,
                HttpCompletionOption.ResponseHeadersRead); // to start working with the response once headers are processed not the whole content
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                response.EnsureSuccessStatusCode();
                var poster = stream.ReadAndDeserializeFromJson<Poster>();
               
            }
        }

        private async Task PostPosterWithStream()
        {
            var random = new Random();
            var generateBytes = new Byte[524288];
            random.NextBytes(generateBytes);

            var posterforCreation = new PosterForCreation()
            {
                Name = "Just a new poster",
                Bytes = generateBytes
            };

            // creating stream
            var memoryContentStream = new MemoryStream();
            memoryContentStream.SerializeToJsonAndWrite(posterforCreation);
            memoryContentStream.Seek(0, SeekOrigin.Begin);

            using (var request = new HttpRequestMessage(HttpMethod.Post, 
                "api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters"))
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                using (var streamContent = new StreamContent(memoryContentStream))
                {
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    request.Content = streamContent;

                    var response = await _httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    var createdContent = await response.Content.ReadAsStringAsync();
                    var createdPoster = JsonConvert.DeserializeObject<Poster>(createdContent);

                    // do something with the newly created poster     
                }
            }
        }
    }
}

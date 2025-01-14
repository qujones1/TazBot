﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using TazBot.Service.Models;
using TazBot.Service.Options;

namespace TazBot.Service.Services
{
    public class GeneralService
    {
        private const string KSOFT_BASE = "https://api.ksoft.si";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;
        private readonly GiphyOptions _giphyOptions;
        private readonly KsoftOptions _ksoftOptions;

        public GeneralService(ILogger<GeneralService> logger, IHttpClientFactory httpClientFactory, IOptions<GiphyOptions> giphyoptions, IOptions<KsoftOptions> ksoftoptions)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _giphyOptions = giphyoptions.Value;
            _ksoftOptions = ksoftoptions.Value;
        }

        public async Task<string> GetInsult()
        {
            var response = await _httpClientFactory.CreateClient().GetAsync("https://evilinsult.com/generate_insult.php?lang=en&type=json");
            var insult = JsonSerializer.Deserialize<Insult>(await response.Content.ReadAsStringAsync()).insult;
            return insult;
        }

        public async Task<string> GetRandomGifByTag(string tag)
        {
            var build = new UriBuilder("api.giphy.com/v1/gifs/random");
            var query = HttpUtility.ParseQueryString(build.Query);
            query["api_key"] = _giphyOptions.Apikey;
            query["tag"] = tag;
            query["rating"] = "r";
            build.Query = query.ToString();
            var response = await _httpClientFactory
                .CreateClient()
                .GetAsync(build.ToString());

            var paani = await response.Content.ReadAsStringAsync();

            
            var parse = JsonSerializer.Deserialize<Giphy>(paani);

            return parse.data.embed_url;
        }
        
        public async Task<string> GetRandomNSFWGifByTag()
        {
            var build = new UriBuilder(KSOFT_BASE + "/images/random-nsfw");
            var host = _httpClientFactory.CreateClient();

            var query = HttpUtility.ParseQueryString(build.Query);
            query["gifs"] = "true";
            build.Query = query.ToString();

            host.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _ksoftOptions.Apitoken);

            var gonk = await host.GetAsync(build.ToString());

            var paani = await gonk.Content.ReadAsStringAsync();

            var parse = JsonSerializer.Deserialize<KsoftImageNSFW>(paani);

            return parse.image_url;
        }

        public async Task<string> GetRandomMeme()
        {
            var build = new UriBuilder(KSOFT_BASE + "/images/random-meme");
            var host = _httpClientFactory.CreateClient();

            var query = HttpUtility.ParseQueryString(build.Query);
            build.Query = query.ToString();

            host.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _ksoftOptions.Apitoken);

            var gonk = await host.GetAsync(build.ToString());

            var paani = await gonk.Content.ReadAsStringAsync();

            var parse = JsonSerializer.Deserialize<KsoftImageNSFW>(paani);

            return parse.image_url;
        }
    }
}

﻿using Discord.Commands;
using System.Threading.Tasks;
using TazBot.Service.Services;
using System.IO;
using Discord;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using TazBot.Service.Messages;
using Discord.WebSocket;

namespace TazBot.Service.CommandModules
{
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        public DadJokeService DadJokeService { get; set; }

        public GeneralService GeneralService { get; set; }

        [Command("source")]
        public Task SourceAsync()
        {
            return ReplyAsync("https://github.com/TazmamzaT/TazBot");
        }

        [Command("imgay")]
        public Task ImGay()
        {
            return ReplyAsync("https://c.tenor.com/eoiC7yT87qgAAAAC/idubbbz-im-gay.gif");
        }

        [Command("help")]
        public Task HelpAsync()
        {
            return ReplyAsync(embed: new HelpMessageBuilder().Build());
        }

        [Command("thanks")]
        public Task ThanksAsync() => ReplyAsync("You're welcome.");

        [Command("joke")]
        public async Task JokeAsync() => await ReplyAsync(await DadJokeService.GetRandomDadJokeAsync());

        [Command("insult")]
        public async Task InsultAsync(SocketUser user)
        {
            var insult = await GeneralService.GetInsult();

            await ReplyAsync(user.Mention + ", " + (char.ToLower(insult[0]) + insult.Substring(1)).Replace("&amp;", "&"));
        }

        [Command("spongebob-gif")]
        public async Task RandomSpongeBobGif()
        {
            await ReplyAsync(await GeneralService.GetRandomGifByTag("spongebob"));
        }

        [Command("randommeme")]
        public async Task RandomMeme()
        {
            await ReplyAsync(await GeneralService.GetRandomMeme());
        }

        [Group("gif")]
        public class Giphy : ModuleBase<SocketCommandContext>
        {
            public GeneralService GeneralService { get; set; }

            [Command("random")]
            public async Task RandomByTag(string tag)
            {
                await ReplyAsync(await GeneralService.GetRandomGifByTag(tag));
            }

            [Command("randomnsfwgif")]
            public async Task RandomNSFWGIF()
            {
                await ReplyAsync(await GeneralService.GetRandomNSFWGifByTag());
            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TazBot.Service.CommandModules;
using TazBot.Service.Interfaces;
using FuzzySharp;

namespace TazBot.Service.Services
{
    public class CommandHandlerService : ICommandHandlerService
    {
        // TODO: This is a configuration and probably belongs in some configuration thing
        public const string COMMAND_PREFIX = "!taz";

        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public CommandHandlerService(CommandService commands, DiscordSocketClient client, IServiceProvider services)
        {
            _commands = commands;
            _client = client;
            _services = services;

            // Hook CommandExecuted to handle post-command-execution logic.
            _commands.CommandExecuted += CommandExecutedAsync;
            // Hook MessageReceived so we can process each message to see
            // if it qualifies as a command.
            _client.MessageReceived += HandleCommandAsync;

        }

        public async Task InstallCommandsAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Ignore system messages, or messages from other bots
            if (messageParam is not SocketUserMessage message) return;
            if (message.Source != MessageSource.User) return;

            // This value holds the offset where the prefix ends
            var argPos = 0;
            // Perform prefix check. You may want to replace this with
            // (!message.HasCharPrefix('!', ref argPos))
            // for a more traditional command format like !help.
            //if (!message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;
            if (!message.HasStringPrefix(COMMAND_PREFIX, ref argPos))
            {
                if (message.ToString().ToLower().Contains("taz")) await message.Channel.SendMessageAsync("Fuck you!");
                if (Fuzz.Ratio(message.ToString(), "The bot provides.") > 50)
                {
                    string provides = char.ToUpper(message.ToString()[0]) + message.ToString().Substring(1);
                    if (message.ToString().EndsWith('.'))
                    {
                        await message.Channel.SendMessageAsync(provides);
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync(provides + ".");
                    }
                }
                return;
            }

            argPos += 1;

            var context = new SocketCommandContext(_client, message);
            // Perform the execution of the command. In this method,
            // the command service will perform precondition and parsing check
            // then execute the command if one is matched.
            await _commands.ExecuteAsync(context, argPos, _services);
            // Note that normally a result will be returned by this format, but here
            // we will handle the result in CommandExecutedAsync,
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // command is unspecified when there was a search failure (command not found); we don't care about these errors
            if (!command.IsSpecified || command.Value == null)
            {
                await context.Channel.SendMessageAsync("I have no idea what the fuck you are trying to ask me to do.");
                return;

            }

            // the command was successful, we don't care about this result, unless we want to log that a command succeeded.
            if (result.IsSuccess)
                return;


            await context.Channel.SendMessageAsync(CommandHandle(result.Error, result));
        }

        public static string CommandHandle(CommandError? error, IResult result) => error switch
        {
            CommandError.UnknownCommand => "What the fuck is this command?",
            CommandError.ParseFailed => "I'm not able to parse something.",
            CommandError.BadArgCount => "The ammount of arguments I was given was bad.",
            CommandError.ObjectNotFound => "I'm not able to parse something",
            CommandError.MultipleMatches => "Taz is a moron and coded multiple methods for the same command.",
            CommandError.UnmetPrecondition => "There's some precondition that wasn't met",
            CommandError.Exception => $"Either Taz really messed up or seomthing is broke. Here's the exception I got: {result}",
            CommandError.Unsuccessful => $"I was not successful.",
            _ => $"Something really wacky happened with me: {result}",
        };
    }
}

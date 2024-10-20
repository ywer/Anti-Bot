using Anti_Bot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anti_Bot.Interfaces;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Anti_Bot.States;

namespace Anti_Bot.Internals
{
    internal class DiscordBotNet : IDiscordBot
    {
        #region Data
        SerLogging _log = new SerLogging();
        string DCAPIKey;
        static int _keyLenght;
        private static readonly char[] _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_".ToCharArray();
        static public DiscordSocketClient _client;
        static private CommandService _commands;
        static private IServiceProvider _services;
        static private InteractionService _interactionService;
        int Re = 0;
        #endregion


        public DiscordBotNet()
        {

        }

        #region Startup
        public async void Start()
        {
            Program Main = new Program();
            try
            {
                CheckAPIKey();
                await StartBot();
            }catch(Exception ex)
            {
                if (Re < Main.MaxTrys)
                {
                    _log.Log("DC: " + ex, SerLogging.MessageLevel.Error);
                    //todo: hier status das es nicht restartet wen es beendet wurde einfügen
                    Re++;
                    Start();
                    return;
                }
                else
                {
                    Main.Shutdown(true);
                }
            }
        }

        #region ApiKeyCheck
        private void CheckAPIKey()
        {
            ProgrammSettings Settings = new ProgrammSettings();
            string Key = Settings.ReturnDiscordAPIKey();
            if(string.IsNullOrEmpty(Key))
            {
                _log.Log("Discord API Key not found", SerLogging.MessageLevel.Warning);
                APIKeyInput();
                return;
            }
            else
            {
                DCAPIKey = Key;
                return;
            }



        }

        private void APIKeyInput()
        {
            Console.WriteLine("Please enter a Disckord API Key");
            string Key = Console.ReadLine().ToLower();
            if(string.IsNullOrEmpty(Key) && !IsValidAPIKey(Key))
            {
                Console.WriteLine("Please enter a valid Key or write exit!");
                APIKeyInput();
                return;
            }
            if(Key == "exit")
            {
                return;
            }
            ProgrammSettings Settings = new ProgrammSettings();
            Settings.ChangeDiscordAPIKey(Key);
            Start();
            return;
        }

        private bool IsValidAPIKey(string Key)
        {
            int ExcactLenght = _keyLenght;
            char[] AllowedChars = _chars;
            if(Key.Length != ExcactLenght)
            {
                return false;
            }
            foreach(char  c in Key)
            {
                if(!AllowedChars.Contains(c))
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        private async Task StartBot()
        {
            if(string.IsNullOrEmpty(DCAPIKey))
            {
                Start();
                return;
            }
            ProgrammSettings Settings = new ProgrammSettings();
            DiscordSocketConfig config = new DiscordSocketConfig()
            {
                UseInteractionSnowflakeDate = false,
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.Guilds | GatewayIntents.GuildVoiceStates
            };
            _client = new DiscordSocketClient(config);
            _commands = new CommandService();
            _services = new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_commands)
             .BuildServiceProvider();

            _client.Log += BotLog;
            _client.SlashCommandExecuted += SlashCommandHandle;
            _client.Ready += Client_Ready;
            _client.JoinedGuild += Guild_Joined;
            _client.LeftGuild += Guild_Leaved;
            _client.Disconnected += OnDisconnect;
            _client.Connected += OnConnect;
            await _client.LoginAsync(TokenType.Bot, DCAPIKey);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        #region Events
        private Task BotLog(LogMessage msg)
        {
            DiscordState SH = new DiscordState();
            var EX = msg.Exception;
            if (EX != null)
            {
                if (EX is not GatewayReconnectException)
                {
                    if (EX is CommandException cmdException)
                    {
                        _log.Log($"[Command/{msg.Severity}] {cmdException.Command.Aliases.First()}"
                + $" failed to execute in {cmdException.Context.Channel}." + cmdException.Message, SerLogging.MessageLevel.Error);
                    }

                    if (msg.Source.Contains("Audio"))
                    {


                        _log.Log("Voicebot Log Error: " + EX, SerLogging.MessageLevel.Error);
                        return Task.CompletedTask;

                    }

                    _log.Log("Discord Error: " + msg.Exception, SerLogging.MessageLevel.Error);
                    return Task.CompletedTask;


                }

            }
            if (msg.Severity == LogSeverity.Error)
            {
                _log.Log(msg.ToString(), SerLogging.MessageLevel.Error);
            }
            else if (msg.Severity == LogSeverity.Critical)
            {
                _log.Log(msg.ToString(), SerLogging.MessageLevel.Error);
            }
            else if (msg.Severity == LogSeverity.Warning)
            {
                _log.Log(msg.ToString(), SerLogging.MessageLevel.Warning);
            }
            else
            {

                _log.Log(msg.ToString(), SerLogging.MessageLevel.Info);
            }

            return Task.CompletedTask;
        }


        private async Task Guild_Joined(SocketGuild guild)
        {
            Settings SET = new Settings();
            SET.AddServer(guild.Id);
            await _interactionService.RegisterCommandsToGuildAsync(guild.Id, true);
            return;
        }

        private async Task Guild_Leaved(SocketGuild guild)
        {
            Settings SET = new Settings();
            SET.RemoveServer(guild.Id);

            return;
        }


        private async Task Client_Ready()
        {
            Settings SET = new Settings();
            Logging _log = new Logging();


            Thread.Sleep(900);
            try
            {
                StatusHandler SH = new StatusHandler();
                Settings Set = new Settings();
                SH.SetDiscordBotStatus(StatusHandler.SytemStatusCodes.Connected);
                //_interactionService = new InteractionService(_client);
                //await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
                // await _interactionService.AddModuleAsync<DiscordCommandsNet>(_services);
                //await _interactionService.RegisterCommandsToGuildAsync(GuildID)
                // await _interactionService.RegisterCommandsGloballyAsync();

                IDictionary<ulong, Settings.ServerSettings> ServerSettings = Set.ReturnALLServerSettings();
                Settings.ProgrammSettingsData ProgrammSettings = Set.ReturnProgrammSettings();

                foreach (var Key in ServerSettings.Keys)
                {
                    _interactionService = new InteractionService(_client);
                    if (ProgrammSettings.AdminServers.Contains(Key))
                    {
                        await _interactionService.AddModuleAsync<AdminCommandsNet>(_services);  //https://github.com/discord-net/Discord.Net/blob/dev/samples/InteractionFramework/Program.cs
                        await _interactionService.AddModuleAsync<DiscordCommandsNet>(_services);
                        await _interactionService.RegisterCommandsToGuildAsync(Key, true);
                    }
                    else
                    {
                        await _interactionService.AddModuleAsync<DiscordCommandsNet>(_services);
                        await _interactionService.RegisterCommandsToGuildAsync(Key, true);
                    }
                    // await _interactionService.RegisterCommandsToGuildAsync(Key,true);
                }



                _client.InteractionCreated += async interaction =>
                {
                    var scope = _services.CreateScope();
                    var ctx = new SocketInteractionContext(_client, interaction);
                    await _interactionService.ExecuteCommandAsync(ctx, scope.ServiceProvider);
                };

            }
            catch (Exception ex)
            {
                // Console.WriteLine($"{ex}");
                _log.Log("Client Ready Error: " + ex, Logging.MessageLevel.Error);
                return;
            }


            return;

        }

        public async Task AddCMDToServer(ulong ServerID)
        {
            Logging _log = new Logging();
            Settings Set = new Settings();
            try
            {

                _interactionService = new InteractionService(_client);
                Settings.ProgrammSettingsData ProgrammSettings = Set.ReturnProgrammSettings();

                if (ProgrammSettings.AdminServers.Contains(ServerID))
                {
                    await _interactionService.AddModuleAsync<AdminCommandsNet>(_services);
                    await _interactionService.AddModuleAsync<DiscordCommandsNet>(_services);
                    await _interactionService.RegisterCommandsToGuildAsync(ServerID, true);
                }
                else
                {
                    await _interactionService.AddModuleAsync<DiscordCommandsNet>(_services);
                    await _interactionService.RegisterCommandsToGuildAsync(ServerID, true);
                }


                _client.InteractionCreated += async interaction =>
                {
                    var scope = _services.CreateScope();
                    var ctx = new SocketInteractionContext(_client, interaction);
                    await _interactionService.ExecuteCommandAsync(ctx, scope.ServiceProvider);
                };

                return;
            }
            catch (Exception ex)
            {
                _log.Log("Error Register CMDs: " + ex, Logging.MessageLevel.Error, ServerID);
                return;


            }

        }


        private Task OnDisconnect(Exception Ex)
        {
            StatusHandler SH = new StatusHandler();
            StatusHandler.SytemStatusCodes DCStatus = SH.ReturnDiscordStatus();
            if (DCStatus != StatusHandler.SytemStatusCodes.Quit)
            {
                SH.SetDiscordBotStatus(StatusHandler.SytemStatusCodes.Disconnected);
                if (Ex != null)
                {
                    SH.SetLastDiscordException(Ex);
                }
            }
            return Task.CompletedTask;
        }

        private Task OnConnect()
        {


            StatusHandler SH = new StatusHandler();
            SH.SetDiscordBotStatus(StatusHandler.SytemStatusCodes.Connected);

            return Task.CompletedTask;
        }

        private async Task SlashCommandHandle(SocketSlashCommand command)
        {
            Logging _log = new Logging();
            var User = command.User;
            string Username = User.Username;
            SocketSlashCommandData message = command.Data;
            if (message == null) return;
            if (User.IsBot)
            {
                return;
            }
            _log.Log("Command: " + command.CommandName + " By: " + User, Logging.MessageLevel.Info, (ulong)command.GuildId);

            return;

        }

        #endregion




        #endregion

    }
}

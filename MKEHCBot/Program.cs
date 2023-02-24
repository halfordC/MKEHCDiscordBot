// See https://aka.ms/new-console-template for more information


using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace MKEHCBot // Note: actual namespace depends on the project name.
{
    //YES I RIPPED THIS STRAIGHT FROM THE EXAMPLE. WE GET THINGS UP AND RUNNING FAST AROUND HERE. 
    internal class Program
    {

        // Non-static readonly fields can only be assigned in a constructor.
        // If you want to assign it elsewhere, consider removing the readonly keyword.
        private readonly DiscordSocketClient _client;
        private readonly DateTime _currentTime;
        private readonly List<PunisherUser> currentPunishers;

        // Discord.Net heavily utilizes TAP for async, so we create
        // an asynchronous context from the beginning.
        static void Main(string[] args)
            => new Program()
                .MainAsync()
                .GetAwaiter()
                .GetResult();

        public Program()
        {
            // Config used by DiscordSocketClient
            // Define intents for the client
            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All,
                MessageCacheSize = 100,
                AlwaysDownloadUsers = true
            };

            // It is recommended to Dispose of a client when you are finished
            // using it, at the end of your app's lifetime.
            _client = new DiscordSocketClient(config);
            _currentTime = DateTime.Now;
            currentPunishers = new List<PunisherUser>();

            // Subscribing to client events, so that we may receive them whenever they're invoked.
            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;
            _client.MessageReceived += MessageReceivedAsync;
            _client.ReactionAdded += ReactionMadeAsync;
        }

        public async Task MainAsync()
        {
            var json = string.Empty;

            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            // Tokens should be considered secret data, and never hard-coded.
            //await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("token"));
            await _client.LoginAsync(TokenType.Bot, configJson.Token);
            // Different approaches to making your token a secret is by putting them in local .json, .yaml, .xml or .txt files, then reading them on startup.

            await _client.StartAsync();

            // Block the program until it is closed.
            await Task.Delay(Timeout.Infinite);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        // The Ready event indicates that the client has opened a
        // connection and it is now safe to access the cache.
        private Task ReadyAsync()
        {
            Console.WriteLine($"{_client.CurrentUser} is connected!");

            return Task.CompletedTask;
        }

        // This is not the recommended way to write a bot - consider
        // reading over the Commands Framework sample.
        private async Task MessageReceivedAsync(SocketMessage message)
        {
            //Pre-check: see if the punisher users are ready to no be punishers anymore. 
            if (currentPunishers.Count > 0)
            {

                var currentTime = DateTime.Now;
                if (currentPunishers.ElementAt(0).unpunishmentStamp <= currentTime)
                {
                    //first, get the role.
                    var channelGet = message.Channel;
                    var role = (channelGet as SocketGuildChannel).Guild.Roles.FirstOrDefault(x => x.Name == "Punisher");

                    //user has served their time in punisher jail, release them. 
                    await currentPunishers.ElementAt(0).userName.RemoveRoleAsync(role);

                    //now pop user out of the que
                    currentPunishers.RemoveAt(0);

                }

            }

            // The bot should never respond to itself.
            if (message.Author.Id == _client.CurrentUser.Id)
                return;

            /*
            if (message.Content == "!ping")
            {
                // Create a new componentbuilder, in which dropdowns & buttons can be created.
                var cb = new ComponentBuilder()
                    .WithButton("Click me!", "unique-id", ButtonStyle.Primary);

                // Send a message with content 'pong', including a button.
                // This button needs to be build by calling .Build() before being passed into the call.
                await message.Channel.SendMessageAsync("pong!", components: cb.Build());
            }
            */
        }

        // For better functionality & a more developer-friendly approach to handling any kind of interaction, refer to:
        // https://discordnet.dev/guides/int_framework/intro.html

        //No Plans for this bot to listen to Interactions at this time. Taken away from the client. 
        private async Task InteractionCreatedAsync(SocketInteraction interaction)
        {
            var interactionType = interaction.Type;
            // safety-casting is the best way to prevent something being cast from being null.
            // If this check does not pass, it could not be cast to said type.
            if (interaction is SocketMessageComponent component)
            {
                // Check for the ID created in the button mentioned above.
                if (component.Data.CustomId == "unique-id")
                    await interaction.RespondAsync("Thank you for clicking my button!");

                else Console.WriteLine("An ID has been received that has no handler!");
            }
            else
            {

            }
        }

        //private async Task 

        private async Task ReactionMadeAsync(Cacheable<IUserMessage, ulong> messageIn, Cacheable<IMessageChannel, ulong> channelIn, SocketReaction reaction)
        {
            //Pre-check, see if current Date time is equal to or greater than the date time of the user action. 
            if (currentPunishers.Count > 0)
            {
                var currentTime = DateTime.Now;
                if (currentPunishers.ElementAt(0).unpunishmentStamp <= currentTime)
                {
                    IMessageChannel tempChannelGet = await channelIn.GetOrDownloadAsync();
                    //first, get the role.
                    var tempRole = (tempChannelGet as SocketGuildChannel).Guild.Roles.FirstOrDefault(x => x.Name == "Punisher");

                    //user has served their time in punisher jail, release them. 
                    await currentPunishers.ElementAt(0).userName.RemoveRoleAsync(tempRole);

                    //now pop user out of the que
                    currentPunishers.RemoveAt(0);

                }

            }



            //first, we want to see what the emoji is. If it is not a punisher, we are not concerned. 
            string reactionName = reaction.Emote.Name.ToString();
            if (reactionName != "Punisher")
            {
                //Console.WriteLine("not punisher emoji, break out");
                return;
            }
            int punisherCounter = 0;

            IUserMessage messageGet = await messageIn.GetOrDownloadAsync();

            IMessageChannel channelGet = await channelIn.GetOrDownloadAsync();

            ulong messageUserId = messageGet.Author.Id;

            IReadOnlyDictionary<IEmote, ReactionMetadata> allReactions = messageGet.Reactions;

            var messageUser = await channelGet.GetUserAsync(messageUserId, CacheMode.AllowDownload) as SocketGuildUser;

            var role = (channelGet as SocketGuildChannel).Guild.Roles.FirstOrDefault(x => x.Name == "Punisher");


            var punisherEmoji = Emote.Parse("<:Punisher:1067645434056683550>");
            foreach (var emote in allReactions)
            {
                string emooteValue = emote.Key.Name.ToString();
                if (emooteValue == "Punisher")
                {
                    punisherCounter = emote.Value.ReactionCount;
                }
            }

            if (punisherCounter >= 6)
            {

                //This is where we assign this person the role of punisher. 
                await messageUser.AddRoleAsync(role);

                //we also need to start a timer for getting rid of their role in a day.
                //We can also assume that if it's not time for the first time in the struct, that all subsequent members will also not be time. 
                PunisherUser newPunisher = new PunisherUser(messageUser);
                this.currentPunishers.Add(newPunisher);

                //we also need to clear the origian message of all punisher reactions.
                for (int i = 0; i < punisherCounter; i++)
                {
                    await messageGet.RemoveReactionAsync(punisherEmoji, messageUser);
                }



            }


        }

    }
}
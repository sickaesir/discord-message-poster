using DSharpPlus;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace DiscordMessagePoster
{
    class Program
    {
        class ConfigChannel
        {
            public ulong ChannelId;
            public int Cooldown;
        }
        class Config
        {
            public string DiscordToken;
            public List<ConfigChannel> Channels;
        }

        static DiscordClient client;
        static Config config;
        static string messageText;

        static async Task ChannelWorker(ConfigChannel channelCfg)
        {
            ulong? lastMessage = null;
            while(true)
            {
                DiscordChannel channel = null;

                try
                {
                    channel = await client.GetChannelAsync(channelCfg.ChannelId);
                }
                catch(Exception)
                {
                }

                if(channel == null)
                {
                    Console.WriteLine($"[Error] Failed to find channel {channelCfg.ChannelId}, please check your config.json!", Color.Red);

                    return;
                }

                if(lastMessage.HasValue)
                {
                    try
                    {
                        DiscordMessage message = await channel.GetMessageAsync(lastMessage.Value);
                        await message.DeleteAsync();
                    }
                    catch(Exception)
                    {

                    }
                }

                try
                {
                    DiscordMessage createdMessage = await channel.SendMessageAsync(messageText);
                    lastMessage = createdMessage.Id;
                }
                catch(Exception)
                {
                    Console.WriteLine($"[Error] Failed to post message on {channel.Name} in {channel.Guild.Name} server, please check your permissions!", Color.Red);
                }

                Console.WriteLine($"[Info] Posted message on {channel.Name} channel in {channel.Guild.Name} server!", Color.AliceBlue);

                await Task.Delay(channelCfg.Cooldown * 1000);
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Discord Message Poster - by Aesir - [ Nulled: SickAesir | Telegram: @sickaesir | Discord: Aesir#1337 ]", Color.Cyan);

            if(!File.Exists("message.txt"))
            {
                Console.WriteLine($"[Error] Add the message you would like to send in a file called message.txt!", Color.Red);
                Console.ReadLine();
                return;
            }

            messageText = File.ReadAllText("message.txt");

            if(!File.Exists("config.json"))
            {
                config = new Config()
                {
                    DiscordToken = "your_discord_token",
                    Channels = new List<ConfigChannel>()
                     {
                         new ConfigChannel()
                         {
                              ChannelId = 1234567890,
                              Cooldown = 5
                         },
                         new ConfigChannel()
                         {
                              ChannelId = 1234567890,
                              Cooldown = 5
                         },
                         new ConfigChannel()
                         {
                              ChannelId = 1234567890,
                              Cooldown = 5
                         },
                         new ConfigChannel()
                         {
                              ChannelId = 1234567890,
                              Cooldown = 5
                         }
                     }
                };

                string configText = JsonConvert.SerializeObject(config, Formatting.Indented);

                File.WriteAllText("config.json", configText);

                Console.WriteLine($"[Info] Seems like this is the first time you ran this program, edit the config.json file and run it again!", Color.Orange);

                Console.ReadLine();
                return;
            }

            config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));

            client = new DiscordClient(new DiscordConfiguration()
            {
                Token = config.DiscordToken,
                TokenType = TokenType.User
            });


            Console.WriteLine($"[Discord] Authenticating...", Color.Yellow);
            try
            {
                client.ConnectAsync().Wait();
            }
            catch(Exception)
            {
                Console.WriteLine($"[Error] Failed to connect to Discord, please check your token and try again!", Color.Red);
                Console.ReadLine();
                return;
            }

            Console.WriteLine($"[Discord] Authentication successful, logged in as {client.CurrentUser.Username}#{client.CurrentUser.Discriminator}!", Color.Green);

            Task.Delay(5000).Wait();

            List<Task> tasks = new List<Task>();

            foreach(var channelConfig in config.Channels)
            {
                tasks.Add(ChannelWorker(channelConfig));
               // Task.Delay(60000).Wait();
            }

            Task.WhenAll(tasks).Wait();

        }
    }
}

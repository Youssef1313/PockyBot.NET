using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GlobalX.ChatBots.Core.Messages;
using Microsoft.Extensions.Options;
using PockyBot.NET.Configuration;
using PockyBot.NET.Constants;
using PockyBot.NET.Persistence.Models;
using PockyBot.NET.Persistence.Repositories;

namespace PockyBot.NET.Services.Triggers
{
    internal class Help : ITrigger
    {
        private readonly IPockyUserRepository _pockyUserRepository;
        private readonly IConfigRepository _configRepository;
        private readonly PockyBotSettings _pockyBotSettings;
        public string Command => Commands.Help;
        public bool DirectMessageAllowed => true;
        public bool CanHaveArgs => true;
        public Role[] Permissions => Array.Empty<Role>();

        public Help(IPockyUserRepository pockyUserRepository, IOptions<PockyBotSettings> pockySettings, IConfigRepository configRepository)
        {
            _pockyUserRepository = pockyUserRepository;
            _configRepository = configRepository;
            _pockyBotSettings = pockySettings.Value;
        }

        public Task<Message> Respond(Message message)
        {
            var partsToSkip = message.MessageParts[0].MessageType == MessageType.PersonMention ? 1 : 0;
            var command = string.Join("", message.MessageParts.Skip(partsToSkip).Select(x => x.Text)).Trim().Remove(0, 4).Trim();
            var user = _pockyUserRepository.GetUser(message.Sender.UserId);
            var newMessage = CreateHelpResponseMessage(command, user);
            return Task.FromResult(new Message
            {
                Text = newMessage
            });
        }

        private string CreateHelpResponseMessage(string command, PockyUser user)
        {
            if (string.IsNullOrEmpty(command)) {
                return CreateCommandListMessage(user);
            }
            switch (command.ToLower(CultureInfo.InvariantCulture)) {
                case Commands.Peg:
                    return CreatePegHelpMessage();
                case Commands.Status:
                    return CreateStatusHelpMessage();
                case Commands.Keywords:
                    return CreateKeywordsHelpMessage();
                case Commands.Ping:
                    return CreatePingHelpMessage();
                case Commands.Welcome:
                    return CreateWelcomeHelpMessage();
                case Commands.Rotation:
                    return CreateRotationHelpMessage();
                case Commands.Results:
                    return CreateResultsHelpMessage(user);
                case Commands.Reset:
                    return CreateResetHelpMessage(user);
                case Commands.Finish:
                    return CreateFinishHelpMessage(user);
                case Commands.NumberConfig:
                    return CreateNumberConfigHelpMessage(user);
                case Commands.StringConfig:
                    return CreateStringConfigHelpMessage(user);
                case Commands.RoleConfig:
                    return CreateRoleConfigHelpMessage(user);
                case Commands.LocationConfig:
                    return CreateLocationConfigHelpMessage(user);
                case Commands.UserLocation:
                    return CreateUserLocationHelpMessage(user);
                case Commands.LocationWeight:
                    return CreateLocationWeightHelpMessage(user);
                case Commands.RemoveUser:
                    return CreateRemoveUserHelpMessage(user);
                default:
                    return CreateDefaultHelpMessage();
            }
        }

        private string CreateDefaultHelpMessage()
        {
            return $"Command not found. To see a full list of commands type `@{_pockyBotSettings.BotName} help` or direct message me with `help`.";
        }

        private string CreateCommandListMessage(PockyUser user)
        {
            var newMessage = "## What I can do (List of Commands)\n\n" +
                 $"* {Commands.Peg}\n" +
                 $"* {Commands.Status}\n" +
                 $"* {Commands.Keywords}\n" +
                 $"* {Commands.Ping}\n" +
                 $"* {Commands.Welcome}\n" +
                 $"* {Commands.Rotation}\n" +
                 $"* {Commands.LocationConfig}\n" +
                 $"* {Commands.UserLocation}\n";

            if (HasPermission(user, new []{Role.Admin, Role.Results})) {
                newMessage += $"* {Commands.Results}\n";
            }


            if (HasPermission(user, new []{Role.Admin, Role.Reset})) {
                newMessage += $"* {Commands.Reset}\n";
            }

            if (HasPermission(user, new []{Role.Admin, Role.Finish})) {
                newMessage += $"* {Commands.Finish}\n";
            }

            if (HasPermission(user, new []{Role.Admin, Role.Config})) {
                newMessage += $"* {Commands.NumberConfig}\n";
                newMessage += $"* {Commands.StringConfig}\n";
                newMessage += $"* {Commands.RoleConfig}\n";
                newMessage += $"* {Commands.LocationWeight}\n";
            }

            if (HasPermission(user, new []{Role.Admin, Role.RemoveUser})) {
                newMessage += $"* {Commands.RemoveUser}\n";
            }

            newMessage += $"\nFor more information on a command type `@{_pockyBotSettings.BotName} help command-name` or direct message me with `help command-name`\n";
            newMessage += "\nI am still being worked on, so more features to come.";
            return newMessage;
        }

        private string CreatePegHelpMessage()
        {
            var keywordsRequired = _configRepository.GetGeneralConfig("requireValues") == 1;
            var newMessage = "### How to give a peg 🎁!\n" +
                $"1. To give someone a peg type: `@{_pockyBotSettings.BotName} {Commands.Peg} @bob {{comment}}`.\n";

            if (keywordsRequired) {
                newMessage += "1. Note that your comment MUST include a keyword.";
            }
            return newMessage;
        }

        private string CreateStatusHelpMessage()
        {
            return "### How to check your status 📈!\n" +
                $"1. To get a PM type: `@{_pockyBotSettings.BotName} {Commands.Status}` OR direct message me with `{Commands.Status}`.\n" +
                "1. I will PM you number of pegs you have left and who you gave it to.";
        }

        private string CreateKeywordsHelpMessage()
        {
            return "### How to check the available keywords 🔑!\n" +
                $"1. To get a list of the available keywords, type: `@{_pockyBotSettings.BotName} {Commands.Keywords}` OR direct message me with `{Commands.Keywords}`.\n" +
                "1. I will respond in the room you messaged me in with a list of keywords.";
        }

        private string CreatePingHelpMessage()
        {
            return "### How to ping me 🏓!\n" +
                $"1. To check whether I'm alive, type: `@{_pockyBotSettings.BotName} {Commands.Ping}` OR direct message me with `{Commands.Ping}`.\n" +
                "1. I will respond in the room you messaged me in if I am alive.";
        }

        private string CreateWelcomeHelpMessage()
        {
            return "### How to welcome someone 👐!\n" +
                $"1. To get a welcome message from me, type `@{_pockyBotSettings.BotName} {Commands.Welcome}` OR direct message me with `{Commands.Welcome}`.\n" +
                "1. I will respond in the room you messaged me in.";
        }

        private string CreateRotationHelpMessage()
        {
            return "### How to check the rotation 🔄!\n" +
                $"1. To check the rotation of teams responsible for buying snacks, type `@{_pockyBotSettings.BotName} {Commands.Rotation}` OR direct message me with `{Commands.Rotation}`.\n" +
                "1. I will respond in the room you messaged me in.";
        }

        private string CreateResultsHelpMessage(PockyUser user)
        {
            if (HasPermission(user, new[] {Role.Admin, Role.Results})) {
                return "### How to display the results 📃!\n" +
                    $"1. To display results, type `@{_pockyBotSettings.BotName} {Commands.Results}`.\n" +
                    "1. I will respond in the room you messaged me in.";
            }
            return CreateDefaultHelpMessage();
        }

        private string CreateResetHelpMessage(PockyUser user)
        {
            if (HasPermission(user, new[] {Role.Admin, Role.Reset})) {
                return "### How to reset all pegs 🙅!\n" +
                    $"1. To clear all pegs, type `@{_pockyBotSettings.BotName} {Commands.Reset}`.\n" +
                    "1. I will respond in the room you messaged me in.";
            }
            return CreateDefaultHelpMessage();
        }

        private string CreateFinishHelpMessage(PockyUser user)
        {
            if (HasPermission(user, new[] {Role.Admin, Role.Finish})) {
                return "### How to complete the cycle 🚲!\n" +
                    $"1. To display winners and results and clear the database, type `@{_pockyBotSettings.BotName} {Commands.Finish}`.\n" +
                    "1. I will respond in the room you messaged me in.";
            }
            return CreateDefaultHelpMessage();
        }

        private string CreateNumberConfigHelpMessage(PockyUser user)
        {
            if (HasPermission(user, new[] {Role.Admin, Role.Config})) {
                return "### How to configure number config values 🔢!\n" +
                    $"1. To get/edit/refresh/delete number config values, type `@{_pockyBotSettings.BotName} {Commands.NumberConfig} {Actions.Get}|{Actions.Add}|{Actions.Delete} {{name}} {{number}}`\n" +
                    "1. I will respond in the room you messaged me in.";
            }
            return CreateDefaultHelpMessage();
        }

        private string CreateStringConfigHelpMessage(PockyUser user)
        {
            if (HasPermission(user, new[] {Role.Admin, Role.Config})) {
                return "### How to configure string config values 🎻!\n" +
                    $"1. To get/add/delete string config values, type `@{_pockyBotSettings.BotName} {Commands.StringConfig} {string.Join("|",ConfigActions.All())} {{name}} {{value}}`\n" +
                    $"    * Example 1: To add a keyword called \"amazing\", type `@{_pockyBotSettings.BotName} {Commands.StringConfig} {ConfigActions.Add} keyword amazing`\n" +
                    $"    * Example 2: To add a linked keyword called \"awesome\" to the \"amazing\" keyword, type `@{_pockyBotSettings.BotName} {Commands.StringConfig} {ConfigActions.Add} linkedKeyword amazing:awesome`\n" +
                    "1. I will respond in the room you messaged me in.";
            }
            return CreateDefaultHelpMessage();
        }

        private string CreateRoleConfigHelpMessage(PockyUser user)
        {
            if (HasPermission(user, new[] {Role.Admin, Role.Config})) {
                return "### How to configure role config values 🗞️!\n" +
                    $"1. To get/set/delete user roles, type `@{_pockyBotSettings.BotName} {Commands.RoleConfig} {Actions.Get}|{Actions.Set}|{Actions.Delete} {{@User}} {{role}}`\n" +
                    "1. I will respond in the room you messaged me in.";
            }
            return CreateDefaultHelpMessage();
        }

        private string CreateLocationConfigHelpMessage(PockyUser user)
        {
            if (HasPermission(user, new[] {Role.Admin, Role.Config})) {
                return "### How to configure location config values 🌏!\n" +
                    $"1. To get/edit/delete locations, type `@{_pockyBotSettings.BotName} {Commands.LocationConfig} {Actions.Get}|{Actions.Add}|{Actions.Delete} {{location}}`\n" +
                    "1. I will respond in the room you messaged me in.";
            }
            return "### How to get location values 🌏!\n" +
                $"1. To get a list of locations, type `@{_pockyBotSettings.BotName} {Commands.LocationConfig} {Actions.Get}`\n" +
                "    * To configure locations, please ask an admin.\n" +
                "1. I will respond in the room you messaged me in.";
        }

        private string CreateUserLocationHelpMessage(PockyUser user)
        {
            if (HasPermission(user, new[] { Role.Admin, Role.Config })) {
                return "### How to configure user location values!\n" +
                    $"1. To get user locations for yourself or others, type `@{_pockyBotSettings.BotName} {Commands.UserLocation} {Actions.Get} me|all|unset|@User`\n" +
                    $"1. To set user locations, type `@{_pockyBotSettings.BotName} {Commands.UserLocation} {Actions.Set} {{location}} me|@User1 @User2`\n" +
                    $"1. To delete user locations, type `@{_pockyBotSettings.BotName} {Commands.UserLocation} {Actions.Delete} me|@User1 @User2`\n" +
                    "1. I will respond in the room you messaged me in.";
            }
            return "### How to config your user location value!\n" +
                $"1. To get user locations for yourself or others, type `@{_pockyBotSettings.BotName} {Commands.UserLocation} {Actions.Get} me|all|unset|@User`\n" +
                $"1. To set your user location, type `@{_pockyBotSettings.BotName} {Commands.UserLocation} {Actions.Set} {{location}} me`\n" +
                "    * To bulk configure user locations, please ask an admin.\n" +
                $"1. To delete your user location, type `@{_pockyBotSettings.BotName} {Commands.UserLocation} {Actions.Delete} me`\n" +
                "1. I will respond in the room you messaged me in.";
        }

        private string CreateLocationWeightHelpMessage(PockyUser user)
        {
            if (HasPermission(user, new[] {Role.Admin, Role.Config})) {
                return "### How to configure location weight values ⚖️!\n" +
                    $"1. To get/edit/delete location weight values, type `@{_pockyBotSettings.BotName} {Commands.LocationWeight} {Actions.Get}|{Actions.Set}|{Actions.Delete} {{location1}} {{location2}} {{weight}}`\n" +
                    "1. I will respond in the room you messaged me in.";
            }
            return CreateDefaultHelpMessage();
        }

        private string CreateRemoveUserHelpMessage(PockyUser user)
        {
            if (HasPermission(user, new[] { Role.Admin, Role.RemoveUser }))
            {
                return "### How to remove users 🛑!\n" +
                       $"1. To remove a user, type `@{_pockyBotSettings.BotName} {Commands.RemoveUser} {{@User}}|'{{username}}'`\n" +
                       "1. I will respond in the room you messaged me in.";
            }
            return CreateDefaultHelpMessage();
        }

        private static bool HasPermission(PockyUser user, Role[] permissions)
        {
            return user != null && user.Roles.Any(x =>
                       permissions.Contains(x.Role));
        }
    }
}

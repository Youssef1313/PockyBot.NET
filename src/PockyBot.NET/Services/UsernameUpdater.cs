using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GlobalX.ChatBots.Core.People;
using Microsoft.Extensions.Logging;
using PockyBot.NET.Persistence.Models;
using PockyBot.NET.Persistence.Repositories;

namespace PockyBot.NET.Services
{
    internal class UsernameUpdater : IUsernameUpdater
    {
        private readonly IPersonHandler _personHandler;
        private readonly IPockyUserRepository _pockyUserRepository;
        private readonly ILogger<UsernameUpdater> _logger;

        public UsernameUpdater(IPersonHandler personHandler, IPockyUserRepository pockyUserRepository, ILogger<UsernameUpdater> logger)
        {
            _personHandler = personHandler;
            _pockyUserRepository = pockyUserRepository;
            _logger = logger;
        }

        public async Task<List<PockyUser>> UpdateUsernames(List<PockyUser> users)
        {
            var usersDetails = await Task.WhenAll(users.Select(x => GetUser(x.UserId, x.Username)));
            var dbUpdates = new List<Task>();

            var updatedUsers = users.Select(u =>
            {
                var userDetails = usersDetails.FirstOrDefault(x => x.UserId == u.UserId);
                if (userDetails?.Username != u.Username)
                {
                    u.Username = userDetails?.Username;
                    dbUpdates.Add(_pockyUserRepository.UpdateUsernameAsync(u.UserId, u.Username));
                }

                return u;
            }).ToList();

            await Task.WhenAll(dbUpdates).ConfigureAwait(false);
            return updatedUsers;
        }

        private async Task<Person> GetUser(string userId, string username)
        {
            try
            {
                return await _personHandler.GetPersonAsync(userId);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error retrieving details for user {userId}", ex);
                return new Person{UserId = userId, Username = username};
            }
        }
    }
}

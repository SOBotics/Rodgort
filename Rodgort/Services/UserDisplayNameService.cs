using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rodgort.ApiUtilities;
using Rodgort.Data;
using Rodgort.Data.Tables;
using Rodgort.Utilities;
using StackExchangeApi;
using StackExchangeApi.Responses;

namespace Rodgort.Services
{
    public class UserDisplayNameService
    {
        public const string SERVICE_NAME = "Sync usernames";

        private readonly DbContextOptions<RodgortContext> _dbContextOptions;
        private readonly ApiClient _apiClient;
        

        public UserDisplayNameService(DbContextOptions<RodgortContext> dbContextOptions, ApiClient apiClient)
        {
            _dbContextOptions = dbContextOptions;
            _apiClient = apiClient;
        }

        public async Task SyncUsers()
        {
            using (var context = new RodgortContext(_dbContextOptions))
            {
                var dbUsers = context.SiteUsers.ToList();
                var userLookup = dbUsers.ToDictionary(u => u.Id, u => u);

                var siteUsers = await _apiClient.Users("stackoverflow.com", dbUsers.Select(u => u.Id));
                foreach (var siteUser in siteUsers.Items)
                {
                    if (userLookup.ContainsKey(siteUser.UserId))
                        userLookup[siteUser.UserId].DisplayName = siteUser.DisplayName;
                }

                context.SaveChanges();
            }
        }
    }
}

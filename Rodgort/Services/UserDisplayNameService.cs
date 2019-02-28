using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using Microsoft.EntityFrameworkCore;
using Rodgort.Data;
using Rodgort.Data.Tables;
using StackExchangeApi;

namespace Rodgort.Services
{
    public class UserDisplayNameService
    {
        public const string SYNC_ALL_USERS = "Sync all users";
        public const string SYNC_USERS_NO_NAME = "Sync users with no name";

        private readonly DbContextOptions<RodgortContext> _dbContextOptions;
        private readonly ApiClient _apiClient;
        
        public UserDisplayNameService(DbContextOptions<RodgortContext> dbContextOptions, ApiClient apiClient)
        {
            _dbContextOptions = dbContextOptions;
            _apiClient = apiClient;
        }

        public void SyncAllUsersSync()
        {
            SyncAllUsers().Wait();
        }

        private async Task SyncAllUsers()
        {
            await SyncUsers();
        }

        public void SyncUsersWithNoNameSync()
        {
            SyncUsersWithNoName().Wait();
        }

        private async Task SyncUsersWithNoName()
        {
            await SyncUsers(su => su.DisplayName == null);
        }

        private async Task SyncUsers(Expression<Func<DbSiteUser, bool>> userPredicate = null)
        {
            using (var context = new RodgortContext(_dbContextOptions))
            {
                IQueryable<DbSiteUser> usersQuery = context.SiteUsers;
                if (userPredicate != null)
                    usersQuery = usersQuery.Where(userPredicate);

                var dbUsers = usersQuery.ToList();
                if (!dbUsers.Any())
                    return;

                var userLookup = dbUsers.ToDictionary(u => u.Id, u => u);

                var siteUsers = await _apiClient.Users("stackoverflow.com", dbUsers.Select(u => u.Id));
                foreach (var siteUser in siteUsers.Items)
                {
                    if (userLookup.ContainsKey(siteUser.UserId))
                    {
                        var user = userLookup[siteUser.UserId];
                        user.DisplayName = HttpUtility.HtmlDecode(siteUser.DisplayName);
                        user.IsModerator = string.Equals("moderator", siteUser.UserType);
                        user.Reputation = siteUser.Reputation;
                    }
                }

                context.SaveChanges();
            }
        }
    }
}

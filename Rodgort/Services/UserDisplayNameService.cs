using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.EntityFrameworkCore;
using Rodgort.Data;
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
            using (var context = new RodgortContext(_dbContextOptions))
            {
                var dbUsers = context.SiteUsers.ToList();
                if (!dbUsers.Any())
                    return;

                var userLookup = dbUsers.ToDictionary(u => u.Id, u => u);

                var siteUsers = await _apiClient.Users("stackoverflow.com", dbUsers.Select(u => u.Id));
                foreach (var siteUser in siteUsers.Items)
                {
                    if (userLookup.ContainsKey(siteUser.UserId))
                        userLookup[siteUser.UserId].DisplayName = HttpUtility.HtmlDecode(siteUser.DisplayName);
                }

                context.SaveChanges();
            }
        }

        public void SyncUsersWithNoNameSync()
        {
            SyncUsersWithNoName().Wait();
        }

        private async Task SyncUsersWithNoName()
        {
            using (var context = new RodgortContext(_dbContextOptions))
            {
                var dbUsers = context.SiteUsers.Where(su => su.DisplayName == null).ToList();
                if (!dbUsers.Any())
                    return;

                var userLookup = dbUsers.ToDictionary(u => u.Id, u => u);

                var siteUsers = await _apiClient.Users("stackoverflow.com", dbUsers.Select(u => u.Id));
                foreach (var siteUser in siteUsers.Items)
                {
                    if (userLookup.ContainsKey(siteUser.UserId))
                        userLookup[siteUser.UserId].DisplayName = HttpUtility.HtmlDecode(siteUser.DisplayName);
                }

                context.SaveChanges();
            }
        }
    }
}

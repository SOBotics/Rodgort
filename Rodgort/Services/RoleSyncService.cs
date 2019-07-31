using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hangfire;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using RestSharp;
using Rodgort.Data;
using Rodgort.Data.Tables;
using StackExchangeApi;

namespace Rodgort.Services
{
    public class RoleSyncService
    {
        private readonly RodgortContext _context;
        private readonly DateService _dateService;
        private readonly ApiClient _apiClient;
        private readonly ILogger<RoleSyncService> _logger;
        public const string SERVICE_NAME = "Sync roles from main site";
        private const string STACKOVERFLOW_CHAT_URL = "https://chat.stackoverflow.com";
        private const string TROGDOR_ROOM_INFO_PAGE_PATH = "rooms/info/165597/trogdor";

        public RoleSyncService(
            RodgortContext context, 
            DateService dateService,
            ApiClient apiClient,
            ILogger<RoleSyncService> logger)
        {
            _context = context;
            _dateService = dateService;
            _apiClient = apiClient;
            _logger = logger;
        }

        public void SyncRolesSync()
        {
            SyncRoles().Wait();
        }

        private async Task SyncRoles()
        {
            var hasNewUser = await SyncTrogdorRoomOwners();
            hasNewUser |= await SyncModerators();

            _context.SaveChanges();

            if (hasNewUser)
                RecurringJob.Trigger(UserDisplayNameService.SYNC_USERS_NO_NAME);
        }

        private async Task<bool> SyncTrogdorRoomOwners()
        {
            var restClient = new RestClient(STACKOVERFLOW_CHAT_URL);
            var request = new RestRequest(TROGDOR_ROOM_INFO_PAGE_PATH);

            var pageResult = await restClient.ExecuteTaskAsync(request);
            var pageContent = pageResult.Content;

            var doc = new HtmlDocument();
            doc.LoadHtml(pageContent);

            var roomOwnerDivs = doc.DocumentNode.SelectNodes("//div[@id='room-ownercards']/div[contains(@class, 'usercard')]");

            var userIdRegex = new Regex(@"owner\-user\-(\d+)");

            var hasNewUser = false;
            foreach (var roomOwnerDiv in roomOwnerDivs)
            {
                var userId = int.Parse(userIdRegex.Match(roomOwnerDiv.Id).Groups[1].Value);
                var userAlreadyExists = _context.SiteUsers.Any(su => su.Id == userId);
                if (!userAlreadyExists)
                {
                    _context.SiteUsers.Add(new DbSiteUser {Id = userId});
                    hasNewUser = true;
                }

                AddRole(userId, DbRole.TRIAGER);
            }

            return hasNewUser;
        }

        private async Task<bool> SyncModerators()
        {
            var moderators = await _apiClient.Moderators("stackoverflow.com");
            var hasNewUser = false;
            foreach (var moderator in moderators.Items)
            {
                var userAlreadyExists = _context.SiteUsers.Any(su => su.Id == moderator.UserId);
                if (!userAlreadyExists)
                {
                    _context.SiteUsers.Add(new DbSiteUser { Id = moderator.UserId, IsModerator = true });
                    hasNewUser = true;
                }

                AddRole(moderator.UserId, DbRole.TRUSTED);
            }

            return hasNewUser;
        }

        private void AddRole(int userId, int roleId)
        {
            var existingUserRole = _context.SiteUserRoles.FirstOrDefault(sur => sur.RoleId == roleId && sur.UserId == userId);
            if (existingUserRole == null || !existingUserRole.Enabled)
                return;

            var existingRole = _context.Roles.FirstOrDefault(r => r.Id == roleId);
            if (existingRole == null)
                return;

            var existingUser = _context.SiteUsers.FirstOrDefault(u => u.Id == userId);
            if (existingUser == null)
                return;

            _logger.LogWarning($"{existingUser.DisplayName} ({existingUser.Id}) was automatically granted role '{existingRole.Name}'");
            _context.SiteUserRoles.Add(new DbSiteUserRole
            {
                UserId = userId,
                RoleId = roleId,
                AddedByUserId = -1,
                Enabled = true,
                DateAdded = _dateService.UtcNow
            });

            _context.SiteUserRoleAudits.Add(new DbSiteUserRoleAudit
            {
                Added = true,
                ChangedByUserId = -1,
                DateChanged = _dateService.UtcNow,
                RoleId = roleId,
                UserId = userId
            });
        }
    }
}

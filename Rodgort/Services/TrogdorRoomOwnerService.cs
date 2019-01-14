using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hangfire;
using HtmlAgilityPack;
using RestSharp;
using Rodgort.Data;
using Rodgort.Data.Tables;

namespace Rodgort.Services
{
    public class TrogdorRoomOwnerService
    {
        private readonly RodgortContext _context;
        public const string SERVICE_NAME = "Sync trogdor room owners";
        private const string STACKOVERFLOW_CHAT_URL = "https://chat.stackoverflow.com";
        private const string TROGDOR_ROOM_INFO_PAGE_PATH = "rooms/info/165597/trogdor";

        public TrogdorRoomOwnerService(RodgortContext context)
        {
            _context = context;
        }

        public void SyncTrogdorRoomOwnersSync()
        {
            SyncTrogdorRoomOwners().Wait();
        }

        private async Task SyncTrogdorRoomOwners()
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

                var roleAlreadyExists = _context.SiteUserRoles.Any(sur => sur.UserId == userId && sur.RoleName == DbRole.TROGDOR_ROOM_OWNER);
                if (!roleAlreadyExists)
                {
                    _context.SiteUserRoles.Add(new DbSiteUserRole { UserId = userId, RoleName = DbRole.TROGDOR_ROOM_OWNER});
                }
            }

            _context.SaveChanges();

            if (hasNewUser)
                RecurringJob.Trigger(UserDisplayNameService.SYNC_USERS_NO_NAME);
        }
    }
}

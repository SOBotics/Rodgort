using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RestSharp;
using Rodgort.Data;
using Rodgort.Data.Constants;
using Rodgort.Data.Tables;
using StackExchangeApi;

namespace Rodgort.Controllers
{
    [Route("api/[controller]")]
    public class AuthenticationController : Controller
    {
        private readonly string _oauthRedirect;
        private readonly string _jwtSigningKey;
        private readonly bool _bypassLoopbackAuth;
        private readonly IStackExchangeApiCredentials _stackExchangeApiCredentials;
        private readonly RodgortContext _dbContext;

        public AuthenticationController(IConfiguration configuration, IStackExchangeApiCredentials stackExchangeApiCredentials, RodgortContext dbContext)
        {
            _stackExchangeApiCredentials = stackExchangeApiCredentials;
            _dbContext = dbContext;
            _oauthRedirect = $"{configuration["HostName"]}/api/Authentication/OAuthRedirect";
            _jwtSigningKey = configuration["JwtSigningKey"];
            _bypassLoopbackAuth = configuration.GetValue<bool>("BypassLoopbackAuth");
        }

        private static string EncodeBase64(string str)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(plainTextBytes);
        }

        private static string DecodeBase64(string str)
        {
            var plainTextBytes = Convert.FromBase64String(str);
            return Encoding.UTF8.GetString(plainTextBytes);
        }

        [HttpGet("Login")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public RedirectResult Login([FromQuery(Name = "redirect_uri")] string redirectUri)
        {
            if (Request.Host.Host == "localhost" && _bypassLoopbackAuth)
            {
                // We can just login the user immediately, if we have one.
                var user = _dbContext.SiteUsers.Include(u => u.Roles).FirstOrDefault(u => u.Id == ChatUserIds.ROB);
                if (user != null)
                {
                    var userRoles = user.Roles.Where(r => r.Enabled);

                    var claims = new[]
                    {
                        new Claim(ClaimTypes.Name, user.DisplayName),
                        new Claim("accountId", user.Id.ToString())
                    }.Concat(userRoles.Select(r => new Claim(ClaimTypes.Role, r.RoleId.ToString())));
                    
                    var signingKey = GetSigningKey();
                    var token = CreateJwtToken(claims, signingKey);

                    Response.Cookies.Append("access_token", token, new CookieOptions { Path = "/Hangfire" });

                    var uriBuilder = new UriBuilder(redirectUri);
                    var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                    query["access_token"] = token;
                    uriBuilder.Query = query.ToString();
                    var redirectUrl = uriBuilder.ToString();

                    return Redirect(redirectUrl);
                }
            }

            var clientId = _stackExchangeApiCredentials.ClientId;
            var payload = JsonConvert.SerializeObject(new LoginState { RedirectUri = redirectUri });
            var encodedPayload = EncodeBase64(payload);

            return Redirect($"https://stackexchange.com/oauth?client_id={clientId}&scope=&redirect_uri={_oauthRedirect}&state={encodedPayload}");
        }

        /// <summary>
        ///     Redirect endpoint for OAuth flow
        /// </summary>
        [HttpGet("OAuthRedirect")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> OAuthRedirect(string code, string state, string error,
            [FromQuery(Name = "error_description")] string errorDescription,
            [FromQuery(Name = "access_token")] string accessToken)
        {
            if (!string.IsNullOrEmpty(error))
                return Json(new
                {
                    error,
                    error_description = errorDescription
                });

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                // Get the access token
                var stackExchangeClient = new RestClient("https://stackexchange.com/");
                var oauthRequest = new RestRequest("oauth/access_token/json", Method.POST);
                oauthRequest.AddParameter("client_id", _stackExchangeApiCredentials.ClientId);
                oauthRequest.AddParameter("client_secret", _stackExchangeApiCredentials.ClientSecret);
                oauthRequest.AddParameter("code", code);
                oauthRequest.AddParameter("redirect_uri", _oauthRedirect);

                var oauthResponse = await stackExchangeClient.ExecuteTaskAsync(oauthRequest, CancellationToken.None);
                var oauthContent = JsonConvert.DeserializeObject<dynamic>(oauthResponse.Content);
                accessToken = oauthContent.access_token;
            }

            // Query the SE.API with their access token, to get their user details.
            var stackExchangeApiClient = new RestClient("https://api.stackexchange.com/");
            var apiRequest = new RestRequest("2.2/me");
            apiRequest.AddParameter("key", _stackExchangeApiCredentials.AppKey);
            apiRequest.AddParameter("site", "stackoverflow");
            apiRequest.AddParameter("access_token", accessToken);
            apiRequest.AddParameter("filter", "!)iua4.KHF.lCb61RIH7hp");
            var apiResponse = await stackExchangeApiClient.ExecuteTaskAsync(apiRequest);
            var apiContent = JsonConvert.DeserializeObject<dynamic>(apiResponse.Content);

            var userDetails = apiContent.items[0];
            int userId = userDetails.user_id;
            string displayName = userDetails.display_name;
            string userType = userDetails.user_type;

            var signingKey = GetSigningKey();

            var user = _dbContext.SiteUsers.Include(su => su.Roles).FirstOrDefault(su => su.Id == userId);
            if (user == null)
            {
                user = new DbSiteUser
                {
                    Id = userId,
                    DisplayName = displayName,
                    IsModerator = string.Equals("moderator", userType),
                    Roles = new List<DbSiteUserRole>()
                };
                _dbContext.SiteUsers.Add(user);
                _dbContext.SaveChanges();
            }
            
            var decodedState = DecodeBase64(state);
            var loginState = JsonConvert.DeserializeObject<LoginState>(decodedState);
            
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, displayName),
                new Claim("accountId", userId.ToString())
            }.Concat(user.Roles.Where(r => r.Enabled).Select(r => new Claim(ClaimTypes.Role, r.RoleId.ToString())));

            var token = CreateJwtToken(claims, signingKey);

            var uriBuilder = new UriBuilder(loginState.RedirectUri);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["access_token"] = token;
            uriBuilder.Query = query.ToString();
            var redirectUrl = uriBuilder.ToString();

            Response.Cookies.Append("access_token", token, new CookieOptions { Path = "/Hangfire" });

            return Redirect(redirectUrl);
        }

        private byte[] GetSigningKey()
        {
            var signingKey = Convert.FromBase64String(_jwtSigningKey);
            return signingKey;
        }

        [HttpPost("RefreshToken")]
        public string RefreshToken()
        {
            if (User == null)
                throw new HttpStatusException(HttpStatusCode.Unauthorized);

            var signingKey = GetSigningKey();
            var newToken = CreateJwtToken(User.Claims, signingKey);

            return newToken;
        }

        public static string CreateJwtToken(IEnumerable<Claim> claims, byte[] symmetricKey)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),

                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(symmetricKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var stoken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(stoken);
            return token;
        }

        private class LoginState
        {
            public string RedirectUri { get; set; }
        }
    }
}

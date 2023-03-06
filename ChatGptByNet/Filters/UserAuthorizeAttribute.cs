using ChatGptByNet.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using System.Security.Claims;

namespace ChatGptByNet.Filters
{
    public class UserAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            context.HttpContext.Request.Headers.TryGetValue(HeaderNames.Authorization, out var authorizations);
            var authorization = authorizations.FirstOrDefault();

            if (authorization == null)
            {
                ReturnUnauthorizedResult(context, "AuthHeaderMissingMessage");
                return;
            }

            var parts = authorization.Split(" ");
            if (parts.Length != 2)
            {
                ReturnUnauthorizedResult(context, "AuthHeaderWrongMessage");
                return;
            }

            var schema = parts[0];
            if (!schema.Equals("Bearer"))
            {
                ReturnUnauthorizedResult(context, "SchemaWrongMessage");
                return;
            }

            var token = parts[1];
            if (string.IsNullOrWhiteSpace(token))
            {
                ReturnUnauthorizedResult(context, "TokenWrongMessage");
                return;
            }

            string userInfo = EncryptAndDecryptHelper.DecryptString(token, "abddccaaxxmmnnqqoowwdd");

            string userEmail = userInfo.Split("&&")[0];
            string userLoginTime = userInfo.Split("&&")[1];

            if(Convert.ToDateTime(userLoginTime).AddHours(2)<= DateTime.Now)
            {
                ReturnUnauthorizedResult(context, "TokenExpireWrongMessage");
                return;
            }


            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userEmail, ClaimValueTypes.String),
                new Claim(ClaimTypes.Name, userEmail, ClaimValueTypes.String),
                new Claim(ClaimTypes.Role, "1", ClaimValueTypes.String)
            };
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims);
            context.HttpContext.User = new ClaimsPrincipal(claimsIdentity);
        }

        private void ReturnUnauthorizedResult(AuthorizationFilterContext context, string message)
        {
            context.Result = new UnauthorizedResult();
            context.HttpContext.Response.Headers.Add(HeaderNames.WWWAuthenticate, string.Format("Error", message));
        }
    }
}

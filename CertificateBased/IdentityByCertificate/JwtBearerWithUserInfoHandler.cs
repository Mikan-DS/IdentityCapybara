using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text.Encodings.Web;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IdentityByCertificate
{

    public class JwtBearerWithUserInfoHandler : AuthenticationHandler<JwtBearerOptions>
    {
        public JwtBearerWithUserInfoHandler(
            IOptionsMonitor<JwtBearerOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock
            ) : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {

            try
            {
                if (Request.Headers["Authorization"].IsNullOrEmpty())
                {
                    return AuthenticateResult.NoResult();
                }


                var token = TryGetBearerFromHeader(Request, out var tokenValue) ? tokenValue : null;
                if (string.IsNullOrEmpty(token))
                {
                    return AuthenticateResult.NoResult();
                }

                var tokenHandler = new JwtSecurityTokenHandler();

                JwtSecurityToken jwtSecurityToken = (JwtSecurityToken)tokenHandler.ReadToken(token);

                X509Certificate2 certificate; // Основной сертификат
                X509Certificate2? refreshCertificate = null; // Старый сертификат необходимый для обновления нового
                string clientId;

                JwtPayload payload = jwtSecurityToken.Payload;

                if (payload.ContainsKey("ClientId"))
                {
                    clientId = (string)payload["ClientId"];
                }
                else
                {
                    return AuthenticateResult.Fail("В токене не хватает индификатора пользователя");
                }

                if (payload.ContainsKey("X509Certificate"))
                {
                    certificate = new X509Certificate2(Convert.FromBase64String((string)payload["X509Certificate"]));
                }
                else {
                    return AuthenticateResult.Fail("В токене не хватает сертификата");
                }

                if (payload.ContainsKey("RefreshX509Certificate"))
                {
                    refreshCertificate = new X509Certificate2(Convert.FromBase64String((string)payload["RefreshX509Certificate"]));
                }


                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    //ValidIssuer = Options.TokenValidationParameters.ValidIssuer,
                    ValidateAudience = false,
                    //ValidAudience = Options.TokenValidationParameters.ValidAudience,
                    ValidateIssuerSigningKey = false,
                    //IssuerSigningKey = Options.TokenValidationParameters.IssuerSigningKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new X509SecurityKey(certificate)
                };


                ClaimsPrincipal claimsPrincipal;
                try
                {
                    claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                }
                catch (Exception)
                {
                    return AuthenticateResult.Fail("Валидация токена не удалась.");
                }

                var ticket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);

                Request.HttpContext.Items["ClientId"] = clientId;
                Request.HttpContext.Items["x509_certificate"] = certificate;

                if (refreshCertificate != null)
                {
                    Request.HttpContext.Items["refresh_x509_certificate"] = refreshCertificate;
                }

                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail($"Произошла ошибка: {ex.Message}");
            }
        }

        private static bool TryGetBearerFromHeader(HttpRequest request, out string tokenValue)
        {
            tokenValue = null;

            if (request.Headers.ContainsKey("Authorization"))
            {
                var authHeader = request.Headers["Authorization"].ToString();
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    tokenValue = authHeader.Substring("Bearer ".Length).Trim();
                    return true;
                }
            }

            return false;
        }
    }
}

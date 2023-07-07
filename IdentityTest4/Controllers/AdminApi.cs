using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityTest4.Controllers
{
    [Route("[Controller]/[Action]")]
    [Authorize(Policy = "AdminScope")]
    public class AdminApi : Controller
    {

        private readonly ConfigurationDbContext _context;

        public AdminApi(ConfigurationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Create([FromForm] string clientId, [FromForm] string secret)
        {

            _context.Add(new Client()
            {

                AllowedGrantTypes = GrantTypes.ClientCredentials,

                ClientId = clientId,
                ClientSecrets =
                {
                    new Secret(secret.Sha256())
                },
                AllowedScopes = { "CapibarAPI" }
            }.ToEntity());

            _context.SaveChanges();


            return Ok($"ACCESS OK: new {clientId} {secret}");
        }
    }
}

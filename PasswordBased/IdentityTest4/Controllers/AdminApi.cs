using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;

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

        /// <summary>
        /// Создает нового пользователя с заданным идентификатором и секретом.
        /// </summary>
        /// <param name="user">Идентификатор пользователя.</param>
        /// <param name="secret">Секрет пользователя.</param>
        /// <param name="scopes">Разрешенные scopes (необязательный параметр).</param>
        /// <returns>Результат выполнения операции.</returns>
        [HttpPost]
        public IActionResult CreateUser([FromForm] string user, [FromForm] string secret, [FromForm] string scopes = "CapibarAPI")
        {
            if (string.IsNullOrWhiteSpace(user))
            {
                return BadRequest("Идентификатор пользователя не может быть пустым.");
            }

            if (string.IsNullOrWhiteSpace(secret))
            {
                return BadRequest("Секрет пользователя не может быть пустым.");
            }

            var existingClient = _context.Clients.FirstOrDefault(x => x.ClientId == user);
            if (existingClient != null)
            {
                return Conflict("Указанный пользователь уже существует.");
            }


            var client = new Client()
            {
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientId = user,
                ClientSecrets =
                    {
                        new Secret(secret.Sha256())
                    },
                AllowedScopes = scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            };

            _context.Clients.Add(client.ToEntity());
            _context.SaveChanges();

            return Ok($"ACCESS OK: новый клиент {user} {secret}");
        }

        /// <summary>
        /// Создает новый scope с заданным названием.
        /// </summary>
        /// <param name="scopeName">Название нового scope.</param>
        /// <returns>Результат выполнения операции.</returns>
        [HttpPost]
        public IActionResult CreateScope([FromForm] string scopeName)
        {
            if (string.IsNullOrEmpty(scopeName))
            {
                return BadRequest("Название scope не может быть пустым.");
            }

            var existingScope = _context.ApiScopes.FirstOrDefault(x => x.Name == scopeName);
            if (existingScope != null)
            {
                return Conflict("Указанный scope уже существует.");
            }

            var newScope = new ApiScope
            {
                Name = scopeName
            };

            _context.ApiScopes.Add(newScope.ToEntity());
            _context.SaveChanges();

            return Ok($"ACCESS OK: создан новый scope {scopeName}");
        }

        /// <summary>
        /// Возвращает JSON со списком всех клиентов и доступных им Scope.
        /// </summary>
        /// <returns>JSON со списком клиентов и доступных им Scope.</returns>
        [HttpPost]
        public IActionResult GetClients()
        {
            var result = _context.Clients.Select(client => new
            {
                client.ClientId,
                Scopes = client.AllowedScopes.Select(scope => scope.Scope)
            }); ;

            return Json(result);
        }

        //TODO Могут быть созданы дополнительные эндпоинты
    }
}
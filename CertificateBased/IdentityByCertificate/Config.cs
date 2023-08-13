using Duende.IdentityServer.Models;

namespace IdentityByCertificate
{
    public static class Config
    {
        /// <summary>
        /// Возвращает список идентификационных ресурсов.
        /// </summary>
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
            new IdentityResources.OpenId()
            };

        /// <summary>
        /// Возвращает список областей API.
        /// </summary>
        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
            new ApiScope("CapibarAPI"),
            new ApiScope("AdminAPI")
            };

        /// <summary>
        /// Возвращает список клиентов, имеющих доступ к API.
        /// </summary>
        public static IEnumerable<Client> Clients =>
            new Client[]
            {

            };
    }

}
using Duende.IdentityServer.Models;

namespace IdentityTest4
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
                new Client
                {
                    ClientId = "client",
                
                    // Нет интерактивного пользователя, аутентификация выполняется с помощью идентификатора клиента и секретного ключа
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    // Секретный ключ для аутентификации
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    // Области, к которым у клиента есть доступ
                    AllowedScopes = { "CapibarAPI", "AdminAPI" }
                },

                new Client
                {
                    ClientId = "nocapy",

                    // Нет интерактивного пользователя, аутентификация выполняется с помощью идентификатора клиента и секретного ключа
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    // Секретный ключ для аутентификации
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    // Области, к которым у клиента есть доступ
                    AllowedScopes = { "AdminAPI" }
                }
            };
    }

}
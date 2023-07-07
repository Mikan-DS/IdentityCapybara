using Duende.IdentityServer.Models;

namespace IdentityTest4
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
            new IdentityResources.OpenId()
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
                {
                
                    new ApiScope("CapibarAPI"),
                    new ApiScope("AdminAPI"),
                
                };

        public static IEnumerable<Client> Clients =>
            new Client[]
                {

                    new Client(){

                             ClientId = "client",

                            // no interactive user, use the clientid/secret for authentication
                            AllowedGrantTypes = GrantTypes.ClientCredentials,

                            // secret for authentication
                            ClientSecrets =
                            {
                                new Secret("secret".Sha256())
                            },

                            // scopes that client has access to
                            AllowedScopes = { "CapibarAPI", "AdminAPI" }

                        },

                    new Client(){

                             ClientId = "nocapy",

                            // no interactive user, use the clientid/secret for authentication
                            AllowedGrantTypes = GrantTypes.ClientCredentials,

                            // secret for authentication
                            ClientSecrets =
                            {
                                new Secret("secret".Sha256())
                            },

                            // scopes that client has access to
                            AllowedScopes = { "AdminAPI" }

                        }



                };
    }
}
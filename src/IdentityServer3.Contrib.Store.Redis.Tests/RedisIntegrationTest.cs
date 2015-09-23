using System;
using System.Collections.Generic;
using System.Security.Claims;
using IdentityServer3.Core;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services.InMemory;
using Xunit;

namespace IdentityServer3.Contrib.Store.Redis.Tests
{
    /// <summary>
    /// Integration test for redis stores
    /// </summary>
    /// <remarks>You will need to have a redis server to test with.</remarks>
    public class RedisIntegrationTest
    {
        private const string RedisServer = "localhost";

        [Fact]
        public void AuthorizationCodePersists()
        {
            var subClaim = new Claim("sub", "kyle@tester.com");
            var emailClaim = new Claim("email", "kyle@tester.com");
            var code = new AuthorizationCode
            {
                Client = new Client
                {
                    ClientId = "cid"
                },
                RequestedScopes = new List<Scope> { new Scope { Description = "this is description", Enabled = true, Name = "sname", DisplayName = "This is Name!" } },
                Subject = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { subClaim,emailClaim}))
            };

            var clients = new List<Client>
            {
                new Client
                {
                    ClientId = "cid",
                    ClientName = "cname",
                    Enabled = true,
                    SlidingRefreshTokenLifetime = 100,
                    AccessTokenType = AccessTokenType.Jwt,
                    Flow = Flows.Implicit
                }
            };
            var clientStore = new InMemoryClientStore(clients);

            var scopes = new List<Scope>
            {
                new Scope
                {
                    Description = "sdescription",
                    Name = "sname",
                    Enabled = true,
                    Emphasize = false,
                    IncludeAllClaimsForUser = true,
                    Required = false,
                    Type = ScopeType.Identity
                }
            };
            var scopeStore = new InMemoryScopeStore(scopes);
            
            var store = new RedisAuthorizationCodeStore(clientStore,scopeStore, RedisServer);
            store.StoreAsync("key1", code).Wait();
             
            var result = store.GetAsync("key1").Result;
            Assert.Equal(code.SubjectId, result.SubjectId);
            Assert.Equal(code.ClientId, result.ClientId);  
        }

        [Fact]
        public void RefreshTokenPersists()
        {
            var subClaim = new Claim("sub", "kyle@tester.com");
            var emailClaim = new Claim("email", "kyle@tester.com");

            var token = new RefreshToken
            {
                AccessToken = new Token
                {
                    
                    CreationTime = DateTimeOffset.Now,
                    Audience = "aud",
                    Claims = new List<Claim> {  subClaim, emailClaim},
                    Client = new Client
                    {
                        ClientId = "cid",
                        ClientName = "cname",
                        Enabled = true,
                        SlidingRefreshTokenLifetime = 100,
                        AccessTokenType = AccessTokenType.Jwt,
                        Flow = Flows.Implicit
                    },
                    Issuer = "iss",
                    Lifetime = 1234567,
                    Type = Constants.TokenTypes.RefreshToken,
                    Version = 1,
                },
                
                CreationTime = DateTimeOffset.Now,
                Version = 1,
                LifeTime = 1234567,
                Subject = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { subClaim, emailClaim }))
            };

            var clients = new List<Client>
            {
                new Client
                {
                    ClientId = "cid",
                    ClientName = "cname",
                    Enabled = true,
                    SlidingRefreshTokenLifetime = 100,
                    AccessTokenType = AccessTokenType.Jwt,
                    Flow = Flows.Implicit
                }
            };
            var clientStore = new InMemoryClientStore(clients);

            var scopes = new List<Scope>
            {
                new Scope
                {
                    Description = "sdescription",
                    Name = "sname",
                    Enabled = true,
                    Emphasize = false,
                    IncludeAllClaimsForUser = true,
                    Required = false,
                    Type = ScopeType.Identity
                }
            };
            var scopeStore = new InMemoryScopeStore(scopes);

            var store = new RedisRefreshTokenStore(clientStore, scopeStore, RedisServer);
            store.StoreAsync("key2", token).Wait();

            var result = store.GetAsync("key2").Result;
            Assert.Equal(token.SubjectId, result.SubjectId);
            Assert.Equal(token.ClientId, result.ClientId);
        }
    }
}

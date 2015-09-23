using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using StackExchange.Redis;

namespace IdentityServer3.Contrib.Store.Redis
{
    /// <summary>
    /// A redis backed refresh token store for Identity Server 3
    /// </summary>
    public class RedisRefreshTokenStore : BaseTokenStore<RefreshToken>, IRefreshTokenStore
    {
        private readonly IDatabase _db;

        /// <summary>
        /// Creates a new instance of the Redis Refresh Token Store
        /// </summary>
        /// <param name="clientStore">Needed because we don't serialize the whole AuthroizationCode. It is looked up by id from the store.</param>
        /// <param name="scopeStore">Needed because we don't serialize the whole AuthorizationCode. It is looked up by id from the store.</param>
        /// <param name="config">The configuration for the connection to redis</param>
        /// <param name="db">The redis db to use to store the tokens</param>
        public RedisRefreshTokenStore(IClientStore clientStore, IScopeStore scopeStore, string config, int db = 0) :base(clientStore,scopeStore)
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(config);
            _db = connectionMultiplexer.GetDatabase(db);
        }

        public async Task StoreAsync(string key, RefreshToken value)
        {
            var json = ToJson(value);
            await _db.StringSetAsync(key, json); 
        }

        public async Task<RefreshToken> GetAsync(string key)
        {
            var json = await _db.StringGetAsync(key);
            var token = FromJson(json);
            return token;
        }

        public async Task RemoveAsync(string key)
        {
            await _db.KeyDeleteAsync(key);
        }

        public Task<IEnumerable<ITokenMetadata>> GetAllAsync(string subject)
        {
            throw new NotImplementedException();
        }

        public Task RevokeAsync(string subject, string client)
        {
           throw new NotImplementedException();
        }
    }
}

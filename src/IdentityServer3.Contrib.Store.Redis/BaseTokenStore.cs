using System;
using IdentityServer3.Contrib.Store.Redis.Serialization;
using IdentityServer3.Core.Services;
using Newtonsoft.Json;

namespace IdentityServer3.Contrib.Store.Redis
{
    public abstract class BaseTokenStore<T> where T : class
    {
        protected readonly IClientStore ClientStore;
        protected readonly IScopeStore ScopeStore;

        protected BaseTokenStore(IClientStore clientStore, IScopeStore scopeStore)
        {
            if (clientStore == null) throw new ArgumentNullException(nameof(clientStore));
            if (scopeStore == null) throw new ArgumentNullException(nameof(scopeStore));

            ClientStore = clientStore;
            ScopeStore = scopeStore;
        }

        protected string ToJson(T value)
        {
            return JsonConvert.SerializeObject(value, GetJsonSerializerSettings());
        }

        protected T FromJson(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, GetJsonSerializerSettings());
        }

        private JsonSerializerSettings GetJsonSerializerSettings()
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ClaimConverter());
            settings.Converters.Add(new ClaimsPrincipalConverter());
            settings.Converters.Add(new ClientConverter(ClientStore));
            settings.Converters.Add(new ScopeConverter(ScopeStore));
            return settings;
        }
    }
}
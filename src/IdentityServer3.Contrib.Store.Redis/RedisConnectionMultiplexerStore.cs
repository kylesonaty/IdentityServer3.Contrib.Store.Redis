using System.Collections.Generic;
using StackExchange.Redis;

namespace IdentityServer3.Contrib.Store.Redis
{
    internal class RedisConnectionMultiplexerStore
    {
        static readonly Dictionary<int, ConnectionMultiplexer> Multiplexers = new Dictionary<int, ConnectionMultiplexer>();

        internal static ConnectionMultiplexer GetConnectionMultiplexer(string config)
        {
            var hash = config.GetHashCode();
            ConnectionMultiplexer multiplexer;
            Multiplexers.TryGetValue(hash, out multiplexer);

            if (multiplexer == null || !multiplexer.IsConnected)
            {
                multiplexer = ConnectionMultiplexer.Connect(config);
                Multiplexers.Remove(hash);
                Multiplexers.Add(hash, multiplexer);
            }

            return multiplexer;
        }
    }
}
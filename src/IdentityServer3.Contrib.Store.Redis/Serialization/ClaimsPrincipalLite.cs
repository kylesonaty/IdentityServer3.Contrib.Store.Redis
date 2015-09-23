namespace IdentityServer3.Contrib.Store.Redis.Serialization
{
    internal class ClaimsPrincipalLite
    {
        public string AuthenticationType { get; set; }
        public ClaimLite[] Claims { get; set; }
    }
}

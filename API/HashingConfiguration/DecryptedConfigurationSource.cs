namespace API.HashingConfiguration
{
    public class DecryptedConfigurationSource : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new DecryptedConfigurationProvider(builder);
        }
    }
}

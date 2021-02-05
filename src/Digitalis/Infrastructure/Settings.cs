namespace Digitalis.Infrastructure
{
    public class Settings
    {
        public DatabaseSettings Database { get; set; }
        
        public string SuperAdmin { get; set; }

        public GoogleSettings Google { get; set; }

        public class DatabaseSettings
        {
            public string[] Urls { get; set; }

            public string DatabaseName { get; set; }

            public string CertPath { get; set; }

            public string CertPass { get; set; }
        }

        public class GoogleSettings
        {
            public string ClientId { get; set; }
        }
    }
}

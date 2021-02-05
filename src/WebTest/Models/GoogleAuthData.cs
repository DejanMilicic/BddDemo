namespace WebTest.Models
{
    public class GoogleAuthData
    {
        public string id_token { get; set; }
    }

    public class GoogleAuthRequest
    {
        public string code { get; set; }

        public string grant_type { get; set; }

        public string client_id { get; set; }

        public string client_secret { get; set; }

        public string redirect_uri { get; set; }

    }
}

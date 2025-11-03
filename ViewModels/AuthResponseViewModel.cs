namespace serverless_auth.ViewModels
{
    public class AuthResponseViewModel
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string Email { get; set; }
        public string ApplicationID { get; set; }
        public long ExpiresIn { get; set; }
    }
}

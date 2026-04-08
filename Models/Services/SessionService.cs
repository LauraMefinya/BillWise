namespace BillWise.Models.Services
{
    // Handles session persistence using device Preferences
    public class SessionService
    {
        private const string AccessTokenKey = "supabase_access_token";
        private const string RefreshTokenKey = "supabase_refresh_token";

        public void SaveSession(string accessToken, string refreshToken)
        {
            Preferences.Default.Set(AccessTokenKey, accessToken);
            Preferences.Default.Set(RefreshTokenKey, refreshToken);
        }

        public (string? AccessToken, string? RefreshToken) LoadSession()
        {
            var access = Preferences.Default.Get<string?>(AccessTokenKey, null);
            var refresh = Preferences.Default.Get<string?>(RefreshTokenKey, null);
            return (access, refresh);
        }

        public void ClearSession()
        {
            Preferences.Default.Remove(AccessTokenKey);
            Preferences.Default.Remove(RefreshTokenKey);
        }

        public bool HasSession()
        {
            var (access, refresh) = LoadSession();
            return !string.IsNullOrEmpty(access) && !string.IsNullOrEmpty(refresh);
        }
    }
}
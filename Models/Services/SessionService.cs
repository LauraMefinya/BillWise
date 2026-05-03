namespace BillWise.Models.Services
{
    /// <summary>
    /// Handles session persistence using the device's secure Preferences.
    /// It stores the Supabase access and refresh tokens locally to keep the user logged in across sessions.
    /// </summary>
    public class SessionService
    {
        private const string AccessTokenKey = "supabase_access_token";
        private const string RefreshTokenKey = "supabase_refresh_token";

        /// <summary>
        /// Saves the given access and refresh tokens into the device's preferences.
        /// </summary>
        /// <param name="accessToken">The JWT access token.</param>
        /// <param name="refreshToken">The refresh token used to renew the session.</param>
        public void SaveSession(string accessToken, string refreshToken)
        {
            Preferences.Default.Set(AccessTokenKey, accessToken);
            Preferences.Default.Set(RefreshTokenKey, refreshToken);
        }

        /// <summary>
        /// Loads the saved access and refresh tokens from the device's preferences.
        /// </summary>
        /// <returns>A tuple containing the access token and the refresh token, if they exist.</returns>
        public (string? AccessToken, string? RefreshToken) LoadSession()
        {
            var access = Preferences.Default.Get<string?>(AccessTokenKey, null);
            var refresh = Preferences.Default.Get<string?>(RefreshTokenKey, null);
            return (access, refresh);
        }

        /// <summary>
        /// Clears the saved session tokens from the device's preferences.
        /// This is typically called upon logout or when the session expires.
        /// </summary>
        public void ClearSession()
        {
            Preferences.Default.Remove(AccessTokenKey);
            Preferences.Default.Remove(RefreshTokenKey);
        }

        /// <summary>
        /// Checks if a valid session exists in the device's preferences.
        /// </summary>
        /// <returns>True if both access and refresh tokens are present; otherwise, false.</returns>
        public bool HasSession()
        {
            var (access, refresh) = LoadSession();
            return !string.IsNullOrEmpty(access) && !string.IsNullOrEmpty(refresh);
        }
    }
}
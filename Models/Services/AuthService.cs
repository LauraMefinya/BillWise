using Supabase;

namespace BillWise.Models.Services
{
    public class AuthService
    {
        private readonly Supabase.Client _client;
        private readonly SessionService _sessionService;

        public AuthService(Supabase.Client client, SessionService sessionService)
        {
            _client = client;
            _sessionService = sessionService;
        }

        // Restore session from saved tokens on app start
        public async Task<bool> RestoreSessionAsync()
        {
            try
            {
                if (!_sessionService.HasSession()) return false;

                var (accessToken, refreshToken) = _sessionService.LoadSession();
                if (accessToken == null || refreshToken == null) return false;

                var session = await _client.Auth.SetSession(accessToken, refreshToken);
                return session?.AccessToken != null;
            }
            catch
            {
                _sessionService.ClearSession();
                return false;
            }
        }

        public async Task<(bool Success, string ErrorMessage)> LoginAsync(
            string email, string password)
        {
            try
            {
                var session = await _client.Auth.SignIn(email, password);
                if (session?.AccessToken != null)
                {
                    // Save session after successful login
                    _sessionService.SaveSession(
                        session.AccessToken,
                        session.RefreshToken ?? "");
                    return (true, string.Empty);
                }
                return (false, "Login failed.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string ErrorMessage)> RegisterAsync(
            string email, string password)
        {
            try
            {
                var session = await _client.Auth.SignUp(email, password);
                if (session?.AccessToken != null)
                {
                    _sessionService.SaveSession(
                        session.AccessToken,
                        session.RefreshToken ?? "");
                }
                return (session != null, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _client.Auth.SignOut();
            }
            catch { }
            finally
            {
                // Always clear saved session on logout
                _sessionService.ClearSession();
            }
        }

        public bool IsUserLoggedIn() =>
            _client.Auth.CurrentSession != null;

        public string GetCurrentUserId() =>
            _client.Auth.CurrentUser?.Id ?? string.Empty;

        public string GetCurrentUserEmail() =>
            _client.Auth.CurrentUser?.Email ?? string.Empty;
    }
}
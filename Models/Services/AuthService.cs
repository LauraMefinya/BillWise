using Supabase;

namespace BillWise.Models.Services
{
    public class AuthService
    {
        private readonly Supabase.Client _client;

        public AuthService(Supabase.Client client)
        {
            _client = client;
        }

        public async Task<(bool Success, string ErrorMessage)> LoginAsync(string email, string password)
        {
            try
            {
                var session = await _client.Auth.SignIn(email, password);
                return (session != null, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string ErrorMessage)> RegisterAsync(string email, string password)
        {
            try
            {
                var session = await _client.Auth.SignUp(email, password);
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
            catch
            {
                // Ignore any error on sign out globally just in case user session was already invalidated
            }
        }

        public bool IsUserLoggedIn()
        {
            return _client.Auth.CurrentSession != null;
        }

        public string GetCurrentUserId()
        {
            return _client.Auth.CurrentUser?.Id ?? string.Empty;
        }

        public string GetCurrentUserEmail()
        {
            return _client.Auth.CurrentUser?.Email ?? string.Empty;
        }
    }
}

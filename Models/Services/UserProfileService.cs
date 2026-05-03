using BillWise.Models.Entities;
using Supabase;

namespace BillWise.Models.Services
{
    /// <summary>
    /// Service to interact with the 'profiles' table in the Supabase database.
    /// Used for saving and fetching user profile details independently of authentication tokens.
    /// </summary>
    public class UserProfileService
    {
        private readonly Client _client;

        public UserProfileService(Client client)
        {
            _client = client;
        }

        /// <summary>
        /// Inserts or updates the user's profile information in the database.
        /// </summary>
        /// <param name="userId">The unique identifier from auth.users.</param>
        /// <param name="fullName">The full name of the user.</param>
        /// <param name="email">The email of the user.</param>
        public async Task UpsertAsync(string userId, string fullName, string email)
        {
            try
            {
                var profile = new UserProfile
                {
                    Id = userId,
                    FullName = fullName,
                    Email = email
                };
                await _client.From<UserProfile>().Upsert(profile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProfileService] Upsert error: {ex.Message}");
            }
        }

        /// <summary>
        /// Fetches the user's full name from the database based on their user ID.
        /// </summary>
        /// <param name="userId">The unique identifier from auth.users.</param>
        /// <returns>The user's full name, or null if it cannot be found.</returns>
        public async Task<string?> FetchNameAsync(string userId)
        {
            try
            {
                var result = await _client.From<UserProfile>()
                    .Where(p => p.Id == userId)
                    .Single();
                return result?.FullName;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProfileService] Fetch error: {ex.Message}");
                return null;
            }
        }
    }
}

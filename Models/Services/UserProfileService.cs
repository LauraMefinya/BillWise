using BillWise.Models.Entities;
using Supabase;

namespace BillWise.Models.Services
{
    public class UserProfileService
    {
        private readonly Client _client;

        public UserProfileService(Client client)
        {
            _client = client;
        }

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

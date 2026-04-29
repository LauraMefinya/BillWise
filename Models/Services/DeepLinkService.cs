namespace BillWise.Models.Services
{
    public static class DeepLinkService
    {
        private static string? _pendingUrl;

        public static event Func<string, string, Task>? RecoveryTokenReceived;

        public static void QueueUrl(string url)
        {
            _pendingUrl = url;
        }

        public static async Task ProcessPendingAsync()
        {
            if (_pendingUrl == null) return;
            var url = _pendingUrl;
            _pendingUrl = null;
            await HandleUrlAsync(url);
        }

        public static async Task HandleUrlAsync(string url)
        {
            if (!url.StartsWith("billwise://")) return;

            var tokens = ParseParams(url);

            if (!tokens.TryGetValue("access_token", out var at) ||
                !tokens.TryGetValue("refresh_token", out var rt))
                return;

            if (tokens.TryGetValue("type", out var type) && type == "recovery" &&
                RecoveryTokenReceived != null)
            {
                await MainThread.InvokeOnMainThreadAsync(
                    () => RecoveryTokenReceived.Invoke(at, rt));
            }
        }

        // Parses both fragment (#) and query (?) params
        private static Dictionary<string, string> ParseParams(string url)
        {
            var result = new Dictionary<string, string>();
            var idx = url.IndexOf('#');
            if (idx < 0) idx = url.IndexOf('?');
            if (idx < 0) return result;

            foreach (var pair in url[(idx + 1)..].Split('&'))
            {
                var eq = pair.IndexOf('=');
                if (eq < 0) continue;
                result[Uri.UnescapeDataString(pair[..eq])] =
                    Uri.UnescapeDataString(pair[(eq + 1)..]);
            }
            return result;
        }
    }
}

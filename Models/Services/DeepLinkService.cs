namespace BillWise.Models.Services
{
    /// <summary>
    /// Service responsible for handling incoming deep links (e.g., from email password resets).
    /// </summary>
    public static class DeepLinkService
    {
        private static string? _pendingUrl;

        public static event Func<string, string, Task>? RecoveryTokenReceived;

        /// <summary>
        /// Queues a deep link URL to be processed later when the app is fully initialized.
        /// </summary>
        /// <param name="url">The deep link URL.</param>
        public static void QueueUrl(string url)
        {
            _pendingUrl = url;
        }

        /// <summary>
        /// Processes any previously queued deep link URL.
        /// Typically called during app startup or resume.
        /// </summary>
        public static async Task ProcessPendingAsync()
        {
            if (_pendingUrl == null) return;
            var url = _pendingUrl;
            _pendingUrl = null;
            await HandleUrlAsync(url);
        }

        /// <summary>
        /// Parses and handles a given deep link URL, extracting authentication tokens if present.
        /// </summary>
        /// <param name="url">The deep link URL to handle.</param>
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

using CommunityCore.Events;
using CommunityCore.Storage;

namespace PolkadotRoots.Helpers
{
    public class ImageHelper
    {
        public static async Task<string?> ResolveImageAsync(EventDto ev, StorageApiClient? storageApi = null)
        {
            if (string.IsNullOrWhiteSpace(ev.Image))
            {
                return null;
            }

            storageApi ??= CommunityClientHelper.StorageApi;

            try
            {
                var url = await storageApi.GetImageAsync(ev.Image);
                if (!string.IsNullOrWhiteSpace(url)) return url;
            }
            catch { /* ignore */ }

            return null;
        }

    }
}

using CommunityCore;
using CommunityCore.Admins;
using CommunityCore.Dotback;
using CommunityCore.Events;
using CommunityCore.Interest;
using CommunityCore.Storage;

namespace PolkadotRoots.Helpers
{
    public class CommunityClientHelper
    {
        public static HttpClient HttpClient { get; } = new HttpClient();
        public static CommunityEventsApiClient EventsApi { get; } = new CommunityEventsApiClient(HttpClient, new CommunityApiOptions());
        public static CommunityDotbacksApiClient DotbacksApi { get; } = new CommunityDotbacksApiClient(HttpClient, new CommunityApiOptions());
        public static StorageApiClient StorageApi { get; } = new StorageApiClient(HttpClient, new CommunityApiOptions());
        public static CommunityInterestApiClient InterestApi { get; } = new CommunityInterestApiClient(HttpClient, new CommunityApiOptions());
        public static CommunityAdminsApiClient AdminsApi { get; } = new CommunityAdminsApiClient(HttpClient, new CommunityApiOptions());

    }
}

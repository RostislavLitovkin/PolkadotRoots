using System.Text;
using CommunityCore;
using CommunityCore.Dotback;
using CommunityCore.Events;
using Substrate.NetApi.Model.Types;
using Uri = System.Uri;

namespace CommunityTests
{
    public class DotbackStatisticsTests
    {
        private CommunityDotbackStatisticsApiClient statisticsClient = null!;
        private CommunityDotbacksApiClient dotbacksClient = null!;
        private CommunityEventsApiClient eventsClient = null!;
        private Account admin = null!;

        [SetUp]
        public void Setup()
        {
            var httpClient = new HttpClient();
            var options = new CommunityApiOptions
            {
                BaseAddress = new Uri("http://localhost:8080")
            };

            statisticsClient = new CommunityDotbackStatisticsApiClient(httpClient, options);
            dotbacksClient = new CommunityDotbacksApiClient(httpClient, options);
            eventsClient = new CommunityEventsApiClient(httpClient, options);
            admin = Helpers.GenerateAdmin();
        }

        private async Task<EventDto> CreateTestEventAsync()
        {
            return await eventsClient.CreateAsync(
                admin,
                new EventDto
                {
                    OrganizatorAddresses = [admin.Value],
                    Name = "Dotback Stats Event",
                    Description = "Event for dotback statistics tests",
                    Image = "test/communityimage.png",
                    Price = "FREE with App",
                    Country = "CZ",
                    Address = "Test Café 16, Prague, 120 00",
                    MapsUrl = "https://maps.app.goo.gl/awTVBhDe2czcHCy6A",
                    Capacity = 10,
                    TimeStart = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds(),
                    TimeEnd = DateTimeOffset.UtcNow.AddDays(1).AddHours(2).ToUnixTimeSeconds(),
                });
        }

        [Test]
        public async Task ExportEventCsvAsync_ReturnsContent()
        {
            EventDto created = null!;
            try
            {
                created = await CreateTestEventAsync();
                var eventId = created.Id!.Value;
                var requestTs = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                var dotbackRegistration = new DotbackRegistrationDto
                {
                    Address = admin.Value,
                    EventId = eventId,
                    UsdAmount = 9.99,
                    ImageUrl = "test/communityimage.png",
                    UnixDateOfRequest = requestTs,
                };

                await dotbacksClient.UpsertAsync(admin, dotbackRegistration);

                var csvBytes = await statisticsClient.ExportEventCsvAsync(eventId, dotPriceUsd: 5.0);

                Assert.That(csvBytes, Is.Not.Null);
                Assert.That(csvBytes!.Length, Is.GreaterThan(0));

                var csvText = Encoding.UTF8.GetString(csvBytes);
                Assert.That(csvText, Does.Contain("DOT,USD,DotAmountPaid"));
                Assert.That(csvText.Split('\n').Length, Is.GreaterThanOrEqualTo(2));
            }
            finally
            {
                if (created?.Id is long id)
                    await eventsClient.DeleteAsync(admin, id);
            }
        }

        [Test]
        public async Task ExportEventCsvAsync_NoDotbacksReturnsNull()
        {
            EventDto created = null!;
            try
            {
                created = await CreateTestEventAsync();
                var eventId = created.Id!.Value;

                var csvBytes = await statisticsClient.ExportEventCsvAsync(eventId);

                Assert.That(csvBytes, Is.Null);
            }
            finally
            {
                if (created?.Id is long id)
                    await eventsClient.DeleteAsync(admin, id);
            }
        }
    }
}

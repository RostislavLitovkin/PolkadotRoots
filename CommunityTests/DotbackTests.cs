using CommunityCore;
using CommunityCore.Dotback;
using CommunityCore.Events;
using Substrate.NetApi.Model.Types;
using Uri = System.Uri;

namespace CommunityTests
{
    public class DotbackTests
    {
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
                    Name = "Dotback Event",
                    Description = "Event for dotback tests",
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
        public async Task UpsertCreateAsync()
        {
            EventDto created = null!;
            try
            {
                created = await CreateTestEventAsync();
                var eventId = created.Id!.Value;

                var dotbackRegistration = new DotbackRegistrationDto
                {
                    Address = admin.Value,
                    EventId  = eventId,
                    UsdAmount = 12.34,
                    ImageUrl = "test/communityimage.png",
                };

                var d = await dotbacksClient.UpsertAsync(admin, dotbackRegistration);

                Assert.That(d.EventId, Is.EqualTo(eventId));
                Assert.That(d.Address, Is.EqualTo(admin.Value));
                Assert.That(d.UsdAmount, Is.EqualTo(12.34).Within(0.0001));
                Assert.That(d.ImageUrl, Is.EqualTo("test/communityimage.png"));
                Assert.That(d.Paid, Is.False);
                Assert.That(d.Rejected, Is.False);
            }
            finally
            {
                if (created?.Id is long id)
                    await eventsClient.DeleteAsync(admin, id);
            }
        }

        [Test]
        public async Task UpsertUpdateAsync()
        {
            var account = Helpers.GenerateAccount();
            EventDto created = null!;
            try
            {
                created = await CreateTestEventAsync();
                var eventId = created.Id!.Value;

                var dotbackRegistration = new DotbackRegistrationDto
                {
                    Address = account.Value,
                    EventId  = eventId,
                    UsdAmount = 5.0,
                    ImageUrl = "test/communityimage.png",
                };

                var d1 = await dotbacksClient.UpsertAsync(account, dotbackRegistration);

                dotbackRegistration.UsdAmount = 20.50;
                var d2 = await dotbacksClient.UpsertAsync(account, dotbackRegistration);

                Assert.That(d2.EventId, Is.EqualTo(eventId));
                Assert.That(d2.Address, Is.EqualTo(account.Value));
                Assert.That(d2.UsdAmount, Is.EqualTo(20.50).Within(0.0001));
                Assert.That(d2.ImageUrl, Is.EqualTo("test/communityimage.png"));
            }
            finally
            {
                if (created?.Id is long id)
                    await eventsClient.DeleteAsync(admin, id);
            }
        }

        [Test]
        public async Task GetOneAsync()
        {
            var account = Helpers.GenerateAccount();

            EventDto created = null!;
            try
            {
                created = await CreateTestEventAsync();
                var eventId = created.Id!.Value;

                var dotbackRegistration = new DotbackRegistrationDto
                {
                    Address = account.Value,
                    EventId  = eventId,
                    UsdAmount = 5.0,
                    ImageUrl = "test/communityimage.png",
                };

                await dotbacksClient.UpsertAsync(account, dotbackRegistration);

                var fetched = await dotbacksClient.GetAsync(eventId, account.Value);
                Assert.That(fetched, Is.Not.Null);
                Assert.That(fetched!.EventId, Is.EqualTo(eventId));
                Assert.That(fetched.Address, Is.EqualTo(account.Value));
            }
            finally
            {
                if (created?.Id is long id)
                    await eventsClient.DeleteAsync(admin, id);
            }
        }

        [Test]
        public async Task SearchByEventAsync()
        {
            EventDto created = null!;
            try
            {
                created = await CreateTestEventAsync();
                var eventId = created.Id!.Value;

                var dotbackRegistration = new DotbackRegistrationDto
                {
                    Address = admin.Value,
                    EventId  = eventId,
                    UsdAmount = 3.21,
                    ImageUrl = "test/communityimage.png",
                };

                await dotbacksClient.UpsertAsync(admin, dotbackRegistration);

                var list = await dotbacksClient.ListByEventAsync(eventId);
                Assert.That(list, Is.Not.Null);
                Assert.That(list.Any(d => d.Address == admin.Value), "Expected admin to be in dotbacks list for the event.");
            }
            finally
            {
                if (created?.Id is long id)
                    await eventsClient.DeleteAsync(admin, id);
            }
        }

        [Test]
        public async Task SearchByAddressAsync()
        {
            EventDto created = null!;
            try
            {
                created = await CreateTestEventAsync();
                var eventId = created.Id!.Value;

                var dotbackRegistration = new DotbackRegistrationDto
                {
                    Address = admin.Value,
                    EventId  = eventId,
                    UsdAmount = 1.23,
                    ImageUrl = "test/communityimage.png",
                };

                await dotbacksClient.UpsertAsync(admin, dotbackRegistration);

                var list = await dotbacksClient.ListByAddressAsync(admin.Value);
                Assert.That(list, Is.Not.Null);
                Assert.That(list.Count, Is.GreaterThan(0));
                Assert.That(list.Any(d => d.Address == admin.Value));
            }
            finally
            {
                if (created?.Id is long id)
                    await eventsClient.DeleteAsync(admin, id);
            }
        }

        [Test]
        public async Task UpdateStatusAsync()
        {
            EventDto created = null!;
            try
            {
                created = await CreateTestEventAsync();
                var eventId = created.Id!.Value;

                var dotbackRegistration = new DotbackRegistrationDto
                {
                    Address = admin.Value,
                    EventId  = eventId,
                    UsdAmount = 4.56,
                    ImageUrl = "test/communityimage.png",
                };

                await dotbacksClient.UpsertAsync(admin, dotbackRegistration);

                var updated = await dotbacksClient.UpdateStatusAsync(admin, eventId, admin.Value, paid: true, rejected: null, subscanUrl: "https://subscan.io/");
                Assert.That(updated, Is.Not.Null);
                Assert.That(updated!.Paid, Is.True);
                Assert.That(updated.Rejected, Is.False);
                Assert.That(updated.SubscanUrl, Is.EqualTo("https://subscan.io/"));
            }
            finally
            {
                if (created?.Id is long id)
                    await eventsClient.DeleteAsync(admin, id);
            }
        }
    }
}

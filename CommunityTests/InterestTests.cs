using CommunityCore;
using CommunityCore.Events;
using CommunityCore.Interest;
using Substrate.NetApi.Model.Types;
using Uri = System.Uri;

namespace CommunityTests
{
    public class InterestTests
    {
        private CommunityInterestApiClient interestClient = null!;
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

            interestClient = new CommunityInterestApiClient(httpClient, options);
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
                    Name = "Interest Event",
                    Description = "Event for interest tests",
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
        public async Task RegisterInterestToEvent23Async()
        {
            var account = Helpers.GenerateAccount();
            var eventId = 23;
            var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var interest = await interestClient.RegisterAsync(account, eventId, ts);

            Assert.That(interest.EventId, Is.EqualTo(eventId));
            Assert.That(interest.Address, Is.EqualTo(account.Value));
            Assert.That(interest.InterestedAt, Is.EqualTo(ts));
        }

        [Test]
        public async Task RegisterInterestCreateAsync()
        {
            EventDto created = null!;
            try
            {
                created = await CreateTestEventAsync();
                var eventId = created.Id!.Value;
                var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                var interest = await interestClient.RegisterAsync(admin, eventId, ts);

                Assert.That(interest.EventId, Is.EqualTo(eventId));
                Assert.That(interest.Address, Is.EqualTo(admin.Value));
                Assert.That(interest.InterestedAt, Is.EqualTo(ts));
            }
            finally
            {
                if (created?.Id is long id)
                    await eventsClient.DeleteAsync(admin, id);
            }
        }

        [Test]
        public async Task RegisterInterestUpdateAsync()
        {
            EventDto created = null!;
            try
            {
                created = await CreateTestEventAsync();
                var eventId = created.Id!.Value;

                var ts1 = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var i1 = await interestClient.RegisterAsync(admin, eventId, ts1);

                var ts2 = ts1 + 60; // update timestamp
                var i2 = await interestClient.RegisterAsync(admin, eventId, ts2);

                Assert.That(i2.InterestedAt, Is.EqualTo(ts2));
                Assert.That(i2.Address, Is.EqualTo(i1.Address));
                Assert.That(i2.EventId, Is.EqualTo(eventId));
            }
            finally
            {
                if (created?.Id is long id)
                    await eventsClient.DeleteAsync(admin, id);
            }
        }

        [Test]
        public async Task ListInterestForEventAsync()
        {
            EventDto created = null!;
            try
            {
                created = await CreateTestEventAsync();
                var eventId = created.Id!.Value;

                var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                await interestClient.RegisterAsync(admin, eventId, ts);

                var list = await interestClient.ListAsync(eventId);
                Assert.That(list, Is.Not.Null);
                Assert.That(list.Any(i => i.Address == admin.Value), "Expected admin to be in interest list.");
            }
            finally
            {
                if (created?.Id is long id)
                    await eventsClient.DeleteAsync(admin, id);
            }
        }

        [Test]
        public async Task InterestCountAsync()
        {
            EventDto created = null!;
            try
            {
                created = await CreateTestEventAsync();
                var eventId = created.Id!.Value;

                var before = await interestClient.CountAsync(eventId);
                var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                await interestClient.RegisterAsync(admin, eventId, ts);
                var after = await interestClient.CountAsync(eventId);

                Assert.That(after, Is.EqualTo(before + 1));
            }
            finally
            {
                if (created?.Id is long id)
                    await eventsClient.DeleteAsync(admin, id);
            }
        }

        [Test]
        public async Task DeleteInterestAsync()
        {
            EventDto created = null!;
            try
            {
                created = await CreateTestEventAsync();
                var eventId = created.Id!.Value;

                var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                await interestClient.RegisterAsync(admin, eventId, ts);

                var deleted1 = await interestClient.DeleteAsync(admin, eventId);
                Assert.That(deleted1, "First delete should return true.");

                var deleted2 = await interestClient.DeleteAsync(admin, eventId);
                Assert.That(!deleted2, "Second delete should return false.");
            }
            finally
            {
                if (created?.Id is long id)
                    await eventsClient.DeleteAsync(admin, id);
            }
        }

        [Test]
        public void RegisterAsyncNullAccountThrows()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () => await interestClient.RegisterAsync(null!, 1, DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
        }

        [Test]
        public void DeleteAsyncNullAccountThrows()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () => await interestClient.DeleteAsync(null!, 1));
        }
    }
}

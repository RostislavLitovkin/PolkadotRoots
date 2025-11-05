using CommunityCore;
using CommunityCore.Events;
using Substrate.NetApi.Model.Types;
using Uri = System.Uri;

namespace CommunityTests
{
    public class EventTests
    {
        private CommunityEventsApiClient client;
        private Account admin;

        [SetUp]
        public void Setup()
        {
            var httpClient = new HttpClient();
            var options = new CommunityApiOptions
            {
                BaseAddress = new Uri("http://localhost:8080")
            };

            client = new CommunityEventsApiClient(httpClient, options);
            admin = Helpers.GenerateAdmin();
        }

        [Test]
        public async Task GetAllEventsAsync()
        {
            var events = await client.GetAllAsync();
            Console.WriteLine($"Total events: {events.Count}");
            foreach (var e in events)
            {
                Console.WriteLine($"Event: {e.Id} - {e.Name}");
            }
        }

        [Test]
        public async Task ListEventsPagedWithFiltersAsync()
        {
            var testCountry = $"TEST";
            var createdIds = new List<long>();

            try
            {
                // Create 3 events under same unique country to test paging deterministically
                for (int i = 0; i < 3; i++)
                {
                    var created = await client.CreateAsync(
                        admin,
                        new EventDto
                        {
                            OrganizatorAddresses = [admin.Value],
                            Name = $"Paged Event {i}",
                            Description = "Paged events test",
                            Country = testCountry,
                            Address = "Test Café",
                            Image = "test/communityimage.png",
                            Price = "FREE with App",
                            MapsUrl = "https://maps.app.goo.gl/awTVBhDe2czcHCy6A",
                            LumaUrl = "https://luma.com/91yecn2o",
                            Website = "https://community.plutolabs.app/",
                            Capacity = 10,
                            TimeStart = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds(),
                            TimeEnd = DateTimeOffset.UtcNow.AddDays(1).AddHours(2).ToUnixTimeSeconds(),
                        });
                    createdIds.Add(created.Id!.Value);
                }

                var page0 = await client.GetPageAsync(page: 0, size: 2, hasEnded: null, country: testCountry);
                Assert.That(page0.Content.Count, Is.LessThanOrEqualTo(2));
                Assert.That(page0.TotalElements, Is.GreaterThanOrEqualTo(3));
                Assert.That(page0.Number, Is.EqualTo(0));

                var page1 = await client.GetPageAsync(page: 1, size: 2, hasEnded: null, country: testCountry);
                Assert.That(page1.Number, Is.EqualTo(1));
                // second page should contain the remainder (at least 1 item)
                Assert.That(page1.Content.Count, Is.GreaterThanOrEqualTo(1));
            }
            finally
            {
                foreach (var id in createdIds)
                {
                    await client.DeleteAsync(admin, id);
                }
            }
        }

        [Test]
        public async Task FilterEventsByHasEndedAsync()
        {
            var testCountry = "TEST";
            EventDto ended = null!;
            EventDto upcoming = null!;

            try
            {
                // Ended event (past)
                ended = await client.CreateAsync(
                    admin,
                    new EventDto
                    {
                        OrganizatorAddresses = [admin.Value],
                        Name = "Ended Event",
                        Description = "Ended",
                        Country = testCountry,
                        Image = "test/communityimage.png",
                        Price = "FREE with App",
                        Address = "Test Café 16, Prague, 120 00",
                        MapsUrl = "https://maps.app.goo.gl/awTVBhDe2czcHCy6A",
                        LumaUrl = "https://luma.com/91yecn2o",
                        Website = "https://community.plutolabs.app/",
                        Capacity = 5,
                        TimeStart = DateTimeOffset.UtcNow.AddHours(-10).ToUnixTimeSeconds(),
                        TimeEnd = DateTimeOffset.UtcNow.AddHours(-8).ToUnixTimeSeconds(),
                    });

                // Upcoming event (future)
                upcoming = await client.CreateAsync(
                    admin,
                    new EventDto
                    {
                        OrganizatorAddresses = [admin.Value],
                        Name = "Upcoming Event",
                        Description = "Upcoming",
                        Country = testCountry,
                        Image = "test/communityimage.png",
                        Price = "FREE with App",
                        Address = "Test Café 16, Prague, 120 00",
                        MapsUrl = "https://maps.app.goo.gl/awTVBhDe2czcHCy6A",
                        LumaUrl = "https://luma.com/91yecn2o",
                        Website = "https://community.plutolabs.app/",
                        Capacity = 5,
                        TimeStart = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
                        TimeEnd = DateTimeOffset.UtcNow.AddHours(2).ToUnixTimeSeconds(),
                    });

                var endedList = await client.GetAllAsync(hasEnded: true, country: testCountry);
                Assert.That(endedList.Any(e => e.Id == ended.Id), "Ended event should be returned when hasEnded=true");
                Assert.That(!endedList.Any(e => e.Id == upcoming.Id), "Upcoming event should not be returned when hasEnded=true");

                var upcomingList = await client.GetAllAsync(hasEnded: false, country: testCountry);
                Assert.That(upcomingList.Any(e => e.Id == upcoming.Id), "Upcoming event should be returned when hasEnded=false");
                Assert.That(!upcomingList.Any(e => e.Id == ended.Id), "Ended event should not be returned when hasEnded=false");
            }
            finally
            {
                if (ended?.Id is long id1) await client.DeleteAsync(admin, id1);
                if (upcoming?.Id is long id2) await client.DeleteAsync(admin, id2);
            }
        }

        [Test]
        public async Task CreateEventAsync()
        {
            EventDto created = await client.CreateAsync(
                    admin,
                    new EventDto
                    {
                        OrganizatorAddresses = [admin.Value],
                        Name = "Test Event",
                        Description = "This is a test event. Delete later",
                        Image = "test/communityimage.png",
                        Price = "FREE with App",
                        Country = "CZ",
                        Address = "Test Café 16, Prague, 120 00",
                        MapsUrl = "https://maps.app.goo.gl/awTVBhDe2czcHCy6A",
                        LumaUrl = "https://luma.com/91yecn2o",
                        Website = "https://community.plutolabs.app/",
                        Capacity = 100,
                        TimeStart = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds(),
                        TimeEnd = DateTimeOffset.UtcNow.AddDays(1).AddHours(2).ToUnixTimeSeconds(),
                    });

            Assert.That(created.Id.HasValue, "Created event must have an Id");
            Console.WriteLine($"Created event: {created.Id} - {created.Name}");
        }

        [Test]
        public async Task GetEventByIdAsync()
        {
            EventDto created = null!;
            try
            {
                created = await client.CreateAsync(
                    admin,
                    new EventDto
                    {
                        OrganizatorAddresses = [admin.Value],
                        Name = "Fetchable Event",
                        Description = "Created for fetching",
                        Country = "CZ",
                        Address = "Test Café",
                        MapsUrl = "https://maps.app.goo.gl/awTVBhDe2czcHCy6A",
                        Capacity = 100,
                        TimeStart = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds(),
                        TimeEnd = DateTimeOffset.UtcNow.AddDays(1).AddHours(2).ToUnixTimeSeconds(),
                    });

                var fetched = await client.GetAsync((long)created.Id!);
                Assert.That(fetched is not null, "Expected event to be found.");
                Assert.That(fetched!.Id, Is.EqualTo(created.Id));
                Assert.That(fetched.Name, Is.EqualTo("Fetchable Event"));
            }
            finally
            {
                if (created?.Id is long id)
                    await client.DeleteAsync(admin, id);
            }
        }

        [Test]
        public async Task GetEventByIdNotFoundAsync()
        {
            var ev = await client.GetAsync(long.MaxValue);
            Assert.That(ev is null, "Expected null for non-existent event.");
        }

        [Test]
        public async Task PutEventUpdateExistingAsync()
        {
            EventDto created = null!;
            try
            {
                created = await client.CreateAsync(admin, new EventDto
                {
                    OrganizatorAddresses = [admin.Value],
                    Name = "Original Event",
                    Description = "Before update",
                    Country = "CZ",
                    Address = "Test Café",
                    MapsUrl = "https://maps.app.goo.gl/awTVBhDe2czcHCy6A",
                    Capacity = 100,
                    TimeStart = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds(),
                    TimeEnd = DateTimeOffset.UtcNow.AddDays(1).AddHours(2).ToUnixTimeSeconds(),
                });

                var updated = await client.PutAsync(admin, created.Id!.Value, new EventDto
                {
                    Id = created.Id,
                    OrganizatorAddresses = [admin.Value],
                    Name = "Updated Event",
                    Description = "After full update",
                    Country = "CZ",
                    Address = "Test Café",
                    MapsUrl = "https://maps.app.goo.gl/awTVBhDe2czcHCy6A",
                    Capacity = 200,
                    TimeStart = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds(),
                    TimeEnd = DateTimeOffset.UtcNow.AddDays(1).AddHours(2).ToUnixTimeSeconds(),
                });

                Assert.That(updated.Id, Is.EqualTo(created.Id));
                Assert.That(updated.Name, Is.EqualTo("Updated Event"));
                Assert.That(updated.Description, Is.EqualTo("After full update"));
                Assert.That(updated.Capacity, Is.EqualTo(200));
            }
            finally
            {
                if (created?.Id is long id)
                    await client.DeleteAsync(admin, id);
            }
        }

        [Test]
        public async Task PatchEventAsync()
        {
            EventDto created = null!;
            try
            {
                created = await client.CreateAsync(admin, new EventDto
                {
                    OrganizatorAddresses = [admin.Value],
                    Name = "Patchable Event",
                    Description = "This is a test event. Delete later",
                    Country = "CZ",
                    Address = "Test Café",
                    MapsUrl = "https://maps.app.goo.gl/awTVBhDe2czcHCy6A",
                    Capacity = 10,
                    TimeStart = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds(),
                    TimeEnd = DateTimeOffset.UtcNow.AddDays(1).AddHours(2).ToUnixTimeSeconds(),
                });

                var patched = await client.PatchAsync(admin, (long)created.Id!, new EventDto
                {
                    Id = created.Id,
                    Description = "Patched description",
                    Capacity = 25
                });

                Assert.That(patched is not null);
                Assert.That(patched!.Id, Is.EqualTo(created.Id));
                Assert.That(patched.Description, Is.EqualTo("Patched description"));
                Assert.That(patched.Capacity, Is.EqualTo(25));
                Assert.That(patched.Name, Is.EqualTo("Patchable Event"), "Name should remain unchanged");
            }
            finally
            {
                if (created.Id is long id)
                    await client.DeleteAsync(admin, id);
            }
        }

        [Test]
        public async Task PatchEventNotFoundAsync()
        {
            var id = long.MaxValue;
            var patched = await client.PatchAsync(admin, id, new EventDto
            {
                Id = id,
                Description = "Should not exist",
            });

            Assert.That(patched is null, "Expected null when patching a missing event.");
        }

        [Test]
        public async Task DeleteEventAsync()
        {
            var created = await client.CreateAsync(admin, new EventDto
            {
                OrganizatorAddresses = [admin.Value],
                Name = "Deletable Event",
                Description = "This is a test event. Delete later",
                Country = "CZ",
                Address = "Test Café",
                MapsUrl = "https://maps.app.goo.gl/awTVBhDe2czcHCy6A",
                Capacity = 100,
                TimeStart = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds(),
                TimeEnd = DateTimeOffset.UtcNow.AddDays(1).AddHours(2).ToUnixTimeSeconds(),
            });

            var id = created.Id!.Value;

            // First delete should succeed
            var deleted1 = await client.DeleteAsync(admin, id);
            Assert.That(deleted1, "First delete should return true.");

            // Second delete should be idempotent (false)
            var deleted2 = await client.DeleteAsync(admin, id);
            Assert.That(!deleted2, "Second delete should return false.");

            var after = await client.GetAsync(id);
            Assert.That(after is null, "Event should no longer exist.");
        }

        [Test]
        public void CreateAsyncNullEventThrows()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () => await client.CreateAsync(admin, null!));
        }
    }
}

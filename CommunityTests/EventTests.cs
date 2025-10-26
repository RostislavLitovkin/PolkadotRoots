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

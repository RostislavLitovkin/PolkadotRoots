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

        private EventDto CreateBaseEvent(string name, string country = "CZ")
        {
            return new EventDto
            {
                OrganizatorAddresses = [admin.Value],
                Name = name,
                Description = $"{name} description",
                Country = country,
                Address = "Test Café",
                Image = "test/communityimage.png",
                Price = "FREE with App",
                MapsUrl = null,
                GoogleMeetsUrl = "https://meet.google.com/example",
                ZoomUrl = "https://zoom.us/j/123456789",
                LumaUrl = "https://luma.com/91yecn2o",
                Website = "https://community.plutolabs.app/",
                Capacity = 10,
                SupportsDotback = true,
                Telegram = "https://t.me/example",
                Discord = "https://discord.gg/example",
                X = "https://x.com/example",
                Youtube = "https://youtube.com/@example",
                Eventbrite = "https://eventbrite.com/e/example",
                ImportedFrom = "test-import"
            };
        }

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
            var testCountry = "CZ";
            var createdIds = new List<long>();

            try
            {
                // Create 3 events under same unique country to test paging deterministically
                for (int i = 0; i < 3; i++)
                {
                    var dto = CreateBaseEvent($"Paged Event {i}", testCountry);
                    dto.Country = testCountry;
                    dto.TimeStart = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();
                    dto.TimeEnd = DateTimeOffset.UtcNow.AddDays(1).AddHours(2).ToUnixTimeSeconds();
                    dto.Capacity = 10;

                    var created = await client.CreateAsync(
                        admin,
                        dto);
                    createdIds.Add(created.Id!.Value);
                }

                // Verify all via non-paged endpoint
                var all = await client.GetAllAsync(hasEnded: null, country: testCountry);
                Assert.That(all.Count, Is.GreaterThanOrEqualTo(3));
                Assert.That(createdIds.All(id => all.Any(e => e.Id == id)), "All created events should be returned by country filter.");

                var page0 = await client.GetPageAsync(page: 0, size: 2, hasEnded: null, country: testCountry);
                Assert.That(page0.Content.Count, Is.LessThanOrEqualTo(2));
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
                var dtoEnded = CreateBaseEvent("Ended Event", testCountry);
                dtoEnded.TimeStart = DateTimeOffset.UtcNow.AddHours(-10).ToUnixTimeSeconds();
                dtoEnded.TimeEnd = DateTimeOffset.UtcNow.AddHours(-8).ToUnixTimeSeconds();
                dtoEnded.Capacity = 5;
                ended = await client.CreateAsync(admin, dtoEnded);

                // Upcoming event (future)
                var dtoUpcoming = CreateBaseEvent("Upcoming Event", testCountry);
                dtoUpcoming.TimeStart = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
                dtoUpcoming.TimeEnd = DateTimeOffset.UtcNow.AddHours(2).ToUnixTimeSeconds();
                dtoUpcoming.Capacity = 5;
                upcoming = await client.CreateAsync(admin, dtoUpcoming);

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
            var dtoCreate = CreateBaseEvent("Test Event");
            dtoCreate.Description = "This is a test event. Delete later";
            dtoCreate.TimeStart = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();
            dtoCreate.TimeEnd = DateTimeOffset.UtcNow.AddDays(1).AddHours(2).ToUnixTimeSeconds();
            dtoCreate.Capacity = 100;
            EventDto created = await client.CreateAsync(
                    admin,
                    dtoCreate);

            Assert.That(created.Id.HasValue, "Created event must have an Id");
            Console.WriteLine($"Created event: {created.Id} - {created.Name}");
        }

        [Test]
        public async Task GetEventByIdAsync()
        {
            EventDto created = null!;
            try
            {
                var dtoFetch = CreateBaseEvent("Fetchable Event");
                dtoFetch.Description = "Created for fetching";
                dtoFetch.Capacity = 100;
                dtoFetch.TimeStart = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();
                dtoFetch.TimeEnd = DateTimeOffset.UtcNow.AddDays(1).AddHours(2).ToUnixTimeSeconds();
                created = await client.CreateAsync(
                    admin,
                    dtoFetch);

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
                var dtoOriginal = CreateBaseEvent("Original Event");
                dtoOriginal.Description = "Before update";
                dtoOriginal.Capacity = 100;
                dtoOriginal.TimeStart = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();
                dtoOriginal.TimeEnd = DateTimeOffset.UtcNow.AddDays(1).AddHours(2).ToUnixTimeSeconds();
                created = await client.CreateAsync(admin, dtoOriginal);

                var dtoUpdate = CreateBaseEvent("Updated Event");
                dtoUpdate.Id = created.Id;
                dtoUpdate.Description = "After full update";
                dtoUpdate.Capacity = 200;
                dtoUpdate.TimeStart = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();
                dtoUpdate.TimeEnd = DateTimeOffset.UtcNow.AddDays(1).AddHours(2).ToUnixTimeSeconds();
                dtoUpdate.GoogleMeetsUrl = "https://meet.google.com/updated";
                dtoUpdate.ZoomUrl = "https://zoom.us/j/updated";

                var updated = await client.PutAsync(admin, created.Id!.Value, dtoUpdate);

                Assert.That(updated.Id, Is.EqualTo(created.Id));
                Assert.That(updated.Name, Is.EqualTo("Updated Event"));
                Assert.That(updated.Description, Is.EqualTo("After full update"));
                Assert.That(updated.Capacity, Is.EqualTo(200));
                Assert.That(updated.SupportsDotback, Is.True);
                Assert.That(updated.Telegram, Is.EqualTo("https://t.me/example"));
                Assert.That(updated.GoogleMeetsUrl, Is.EqualTo("https://meet.google.com/updated"));
                Assert.That(updated.ZoomUrl, Is.EqualTo("https://zoom.us/j/updated"));
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
                var dtoPatchBase = CreateBaseEvent("Patchable Event");
                dtoPatchBase.Description = "This is a test event. Delete later";
                dtoPatchBase.Capacity = 10;
                dtoPatchBase.TimeStart = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();
                dtoPatchBase.TimeEnd = DateTimeOffset.UtcNow.AddDays(1).AddHours(2).ToUnixTimeSeconds();
                created = await client.CreateAsync(admin, dtoPatchBase);

                var patched = await client.PatchAsync(admin, (long)created.Id!, new EventDto
                {
                    Id = created.Id,
                    Description = "Patched description",
                    Capacity = 25,
                    Telegram = "https://t.me/updated",
                    ZoomUrl = "https://zoom.us/j/patched"
                });

                Assert.That(patched is not null);
                Assert.That(patched!.Id, Is.EqualTo(created.Id));
                Assert.That(patched.Description, Is.EqualTo("Patched description"));
                Assert.That(patched.Capacity, Is.EqualTo(25));
                Assert.That(patched.Name, Is.EqualTo("Patchable Event"), "Name should remain unchanged");
                Assert.That(patched.Telegram, Is.EqualTo("https://t.me/updated"));
                Assert.That(patched.ZoomUrl, Is.EqualTo("https://zoom.us/j/patched"));
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
            var dtoDelete = CreateBaseEvent("Deletable Event");
            dtoDelete.Description = "This is a test event. Delete later";
            dtoDelete.Capacity = 100;
            dtoDelete.TimeStart = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();
            dtoDelete.TimeEnd = DateTimeOffset.UtcNow.AddDays(1).AddHours(2).ToUnixTimeSeconds();
            var created = await client.CreateAsync(admin, dtoDelete);

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

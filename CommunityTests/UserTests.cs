using CommunityCore;
using CommunityCore.Users;
using Substrate.NetApi.Model.Types;
using Uri = System.Uri;

namespace CommunityTests
{
    public class UserTests
    {
        private CommunityUsersApiClient client;

        private Account admin;

        [SetUp]
        public void Setup()
        {
            var httpClient = new HttpClient();

            var options = new CommunityApiOptions
            {
                BaseAddress = new Uri("http://localhost:8080")
            };

            client = new CommunityUsersApiClient(httpClient, options);

            admin = Helpers.GenerateAdmin();
        }

        [Test]
        public async Task GetAllUsersAsync()
        {
            var users = await client.GetAllAsync();

            Console.WriteLine($"Total users: {users.Count}");

            foreach (var user in users)
            {
                Console.WriteLine($"User: {user.Address}, Name: {user.FullName}");
            }
        }

        [Test]
        public async Task CreateUserAsync()
        {
            var account = Helpers.GenerateAccount();

            var user = await client.CreateAsync(
                account,
                new UserDto
                {
                    Address = account.Value,
                    FirstName = "Pepa",
                    LastName = "Franta",
                    Description = "This is a test user. Delete later"
                });

            Console.WriteLine($"User: {user.Address}, Name: {user.FullName}");
        }

        // The API currently does not support any Admin authentication
        [Test]
        public async Task CreateUserConflictAsync()
        {
            var sharedAddress = "5EU6EyEq6RhqYed1gCYyQRVttdy6FC9yAtUUGzPe3gfpFX8y";

            await client.CreateAsync(
                admin,
                new UserDto
                {
                    Address = sharedAddress,
                    FirstName = "Pepa",
                    LastName = "Franta",
                    Description = "This is a test user. Delete later",
                });

            try
            {
                await client.CreateAsync(
                    admin,
                    new UserDto
                    {
                        Address = sharedAddress,
                        FirstName = "Pepa",
                        LastName = "Franta",
                        Description = "This is a test user. Delete later",
                    });

                Assert.Fail("Not 409 Conflict");
            }
            catch (CommunityApiException ex)
            {
                Assert.That(ex.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Conflict));
            }
            catch
            {
                Assert.Fail("Not 409 Conflict");
            }

            await client.DeleteAsync(admin, sharedAddress);
        }

        [Test]
        public async Task GetUserByAddressAsync()
        {
            var account = Helpers.GenerateAccount();

            var created = await client.CreateAsync(
                account,
                new UserDto
                {
                    Address = account.Value,
                    FirstName = "Alice",
                    LastName = "Tester",
                    Description = "This is a test user. Delete later"
                });

            try
            {
                var fetched = await client.GetAsync(account.Value);

                Assert.That(fetched is not null, "Expected user to be found.");
                Assert.That(fetched!.Address, Is.EqualTo(account.Value));
                Assert.That(fetched.FullName, Is.EqualTo("Alice Tester"));
                Console.WriteLine($"Fetched: {fetched.Address}, Name: {fetched.FullName}");
            }
            finally
            {
                await client.DeleteAsync(admin, account.Value);
            }
        }

        [Test]
        public async Task GetUserByAddressNotFoundAsync()
        {
            var randomAddress = Helpers.GenerateAccount().Value;
            var user = await client.GetAsync(randomAddress);
            Assert.That(user is null, "Expected null for non-existent user.");
        }

        [Test]
        public async Task PutUserUpsertCreateAsync()
        {
            var account = Helpers.GenerateAccount();

            try
            {
                var upserted = await client.PutAsync(account, new UserDto
                {
                    Address = account.Value,
                    FirstName = "Bob",
                    LastName = "Builder",
                    Description = "This is a test user. Delete later"
                });

                Assert.That(upserted.Address, Is.EqualTo(account.Value));
                Assert.That(upserted.FullName, Is.EqualTo("Bob Builder"));

                var fetched = await client.GetAsync(account.Value);
                Assert.That(fetched is not null);
                Assert.That(fetched!.FullName, Is.EqualTo("Bob Builder"));
                Console.WriteLine($"Upserted via PUT: {fetched.Address}, Name: {fetched.FullName}");
            }
            finally
            {
                await client.DeleteAsync(admin, account.Value);
            }
        }

        [Test]
        public async Task PutUserUpdateExistingAsync()
        {
            var account = Helpers.GenerateAccount();
            try
            {
                var created = await client.CreateAsync(account, new UserDto
                {
                    Address = account.Value,
                    FirstName = "Carl",
                    LastName = "Original",
                    Description = "Before update"
                });

                var updated = await client.PutAsync(account, new UserDto
                {
                    Address = account.Value,
                    FirstName = "Carl",
                    LastName = "Updated",
                    Description = "After full update"
                });

                Assert.That(updated.Address, Is.EqualTo(account.Value));
                Assert.That(updated.FullName, Is.EqualTo("Carl Updated"));
                Assert.That(updated.Description, Is.EqualTo("After full update"));
            }
            finally
            {
                await client.DeleteAsync(admin, account.Value);
            }
        }

        [Test]
        public async Task PatchUserAsync()
        {
            var account = Helpers.GenerateAccount();

            try
            {
                var created = await client.CreateAsync(account, new UserDto
                {
                    Address = account.Value,
                    FirstName = "Dana",
                    LastName = "Doe",
                    Description = "Initial description"
                });

                // Only change Description
                var patched = await client.PatchAsync(account, new UserDto
                {
                    Address = account.Value,
                    Description = "Patched description"
                });

                Assert.That(patched is not null);
                Assert.That(patched!.Address, Is.EqualTo(account.Value));
                Assert.That(patched.FullName, Is.EqualTo("Dana Doe"), "Names should be unchanged by partial patch.");
                Assert.That(patched.Description, Is.EqualTo("Patched description"));
                Console.WriteLine($"Patched user: {patched.Address}, Desc: {patched.Description}");
            }
            finally
            {
                await client.DeleteAsync(account, account.Value);
            }
        }

        [Test]
        public async Task PatchUserNotFoundAsync()
        {
            var randomAddress = Helpers.GenerateAccount().Value;

            var patched = await client.PatchAsync(admin, new UserDto
            {
                Address = randomAddress,
                Description = "Should not exist"
            });

            Assert.That(patched is null, "Expected null when patching a missing user.");
        }

        [Test]
        public async Task DeleteUserAsync()
        {
            var account = Helpers.GenerateAccount();

            var created = await client.CreateAsync(account, new UserDto
            {
                Address = account.Value,
                FirstName = "Eve",
                LastName = "Erasable",
                Description = "This is a test user. Delete later"
            });

            // First delete should succeed
            var deleted1 = await client.DeleteAsync(account, account.Value);
            Assert.That(deleted1, "First delete should return true.");

            // Second delete should be idempotent (false)
            var deleted2 = await client.DeleteAsync(account, account.Value);
            Assert.That(!deleted2, "Second delete should return false.");

            var after = await client.GetAsync(account.Value);
            Assert.That(after is null, "User should no longer exist.");
        }

        [Test]
        public void GetAsyncEmptyAddressThrows()
        {
            Assert.ThrowsAsync<ArgumentException>(async () => await client.GetAsync(""));
            Assert.ThrowsAsync<ArgumentException>(async () => await client.GetAsync("   "));
            Assert.ThrowsAsync<ArgumentException>(async () => await client.GetAsync(null!));
        }

        [Test]
        public void CreateAsyncNullUserThrows()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () => await client.CreateAsync(admin, null!));
        }

        [Test]
        public void DeleteAsyncEmptyAddressThrows()
        {
            Assert.ThrowsAsync<ArgumentException>(async () => await client.DeleteAsync(admin, ""));
            Assert.ThrowsAsync<ArgumentException>(async () => await client.DeleteAsync(admin, "   "));
            Assert.ThrowsAsync<ArgumentException>(async () => await client.DeleteAsync(admin, null!));
        }
    }
}

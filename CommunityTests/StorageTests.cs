using CommunityCore;
using CommunityCore.Storage;

namespace CommunityTests
{
    public class StorageTests
    {
        private StorageApiClient client;

        [SetUp]
        public void Setup()
        {
            var httpClient = new HttpClient();

            var options = new CommunityApiOptions
            {
                BaseAddress = new Uri("http://localhost:8080")
            };

            client = new StorageApiClient(httpClient, options);
        }

        [Test]
        public async Task UploadImageAsync()
        {
            var imagePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "Files", "communityimage.png"));
            await using var stream = System.IO.File.OpenRead(imagePath);
            var result = await client.UploadImageAsync(stream, "communityimage.png", "image/png", folder: "test");
        }

        [Test]
        public async Task GetImageAsync()
        {
            var image = await client.GetImageAsync("communityimage.png", "test");

            Console.WriteLine(image);

            // You need to manually see 
        }
    }
}

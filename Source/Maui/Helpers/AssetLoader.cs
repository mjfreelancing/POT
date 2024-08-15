namespace Pot.Helpers
{
    public static class AssetLoader
    {
        public static async Task<string> GetAsStringAsync(string assetName)
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(assetName);
            using var reader = new StreamReader(stream);

            return await reader.ReadToEndAsync();
        }
    }
}

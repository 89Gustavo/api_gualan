namespace api_gualan.Helpers;

public static class FileStorageHelper
{
    public static async Task<string> SaveAsync(
        IFormFile file,
        string folder)
    {
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{file.FileName}";
        string fullPath = Path.Combine(folder, fileName);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return fullPath;
    }
}

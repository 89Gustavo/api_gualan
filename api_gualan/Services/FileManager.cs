namespace api_gualan.Services
{
    public static class FileManager
    {
        public static void Move(string file, string target)
        {
            Directory.CreateDirectory(target);
            File.Move(file, Path.Combine(target, Path.GetFileName(file)), true);
        }
    }
}

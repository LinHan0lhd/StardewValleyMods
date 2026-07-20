namespace AutoServerPro.Utils
{
    public static class DirectoryHelper
    {
        public static void CopyDirectory(string sourceDir, string destDir, bool overwrite)
        {
            Directory.CreateDirectory(destDir);
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string target = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, target, overwrite);
            }
            foreach (var sub in Directory.GetDirectories(sourceDir))
                CopyDirectory(sub, Path.Combine(destDir, Path.GetFileName(sub)), overwrite);
        }
    }
}
using SharpCompress.Archive;
using SharpCompress.Common;

namespace Updater.Classes
{
    public class ZipHelper
    {
        public static void UnZip(string zipFile, string folderPath)
        {
            var archive = ArchiveFactory.Open(zipFile);
            foreach (var entry in archive.Entries)
            {
                if (!entry.IsDirectory)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine(entry.Key.ToString());
#endif
                    entry.WriteToDirectory(folderPath, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
                }
            }
        }
    }
}

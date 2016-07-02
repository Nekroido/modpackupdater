using System;
using System.IO;
using System.Security.Cryptography;

namespace Updater.Classes.Helpers
{
    public class StringHelper
    {
        /// <summary>
        /// Calculates MD5 checksum of a given file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }
    }
}

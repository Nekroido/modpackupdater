using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Updater.Models;

namespace Updater.Classes
{
    public class Api
    {
        public static async Task<VersionModel> GetModpackVersion()
        {
            return await Request<VersionModel>(Config.MODPACK_URL + Config.MODPACK_INFO);
        }

        private static async Task<T> Request<T>(string url)
        {
            var response = await GetRequest(url);

            if (response != null)
            {
                var result = JsonConvert.DeserializeObject<T>(response);

                if (result != null)
                {
                    return result;
                }
            }

            return default(T);
        }

        private static async Task<string> GetRequest(string url)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(url);
#endif
            try
            {
                using (var c = new HttpClient())
                {
                    //c.Timeout = new TimeSpan(5000);
                    var response = await c.GetStringAsync(url);
                    
                    return response;
                }
            }
            catch (Exception e)
            {
                // Log and continue
#if DEBUG
                System.Diagnostics.Debug.WriteLine(e.ToString());
#endif
            }

            return null;
        }
    }
}

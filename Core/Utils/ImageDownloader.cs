using System.IO;
using System.Net;
using System;
using System.Threading.Tasks;

namespace Core.Utils
{
    class ImageDownloader
    {
        public async Task<byte[]> Download(string url)
        {
            if (url.EndsWith(".jpg") || url.EndsWith(".gif") || url.EndsWith(".png") || url.EndsWith(".jpeg"))
            {
                using (WebClient webClient = new WebClient())
                {
                    Console.WriteLine("Downloading from URL '{0}'", url);

                    try
                    {
                        var data = await webClient.DownloadDataTaskAsync(url);

                        if (data != null)
                            Console.WriteLine("Downloaded {0} bytes", data.Length);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Failed to download from URL '{0}'. Reason: {1}", url, ex.Message);
                    }
                    return null;
                }
            }
            return null;
        }
    }
}

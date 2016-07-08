using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;

namespace Core.Utils
{
    public class ImageDownloader
    {
        readonly List<string> validExtensions;

        public ImageDownloader()
        {
            validExtensions = new List<string>();
            validExtensions.Add("jpg");
            validExtensions.Add("gif");
            validExtensions.Add("png");
            validExtensions.Add("jpeg");
        }

        public async Task<byte[]> Download(string url)
        {
            if (IsUrlValid(url))
            {
                using (WebClient webClient = new WebClient())
                {
                    Console.WriteLine("Downloading from URL '{0}'", url);

                    try
                    {
                        var data = await webClient.DownloadDataTaskAsync(url);
                        Console.WriteLine("Downloaded {0} bytes", data.Length);
                        return data;
                    }

                    catch(Exception ex)
                    {
                        Console.WriteLine("Failed to download from URL '{0}'. Reason: {1}", url, ex.Message);
                    }
                }
            }
            else
            {
                Console.WriteLine("URL '{0}' is invalid. It must end with an image extension.");
            }

            return null;
        }

        bool IsUrlValid(string URL)
        {
            foreach (var ext in validExtensions)
            {
                if (URL.EndsWith(ext))
                    return true;
            }
            return false;
        }
    }
}

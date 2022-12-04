using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Helper class to download and store videos from the cross promotion
    /// configuration
    /// </summary>
    public class CrossPromotionVideoDownloader
    {
        public async Task DownloadItem(CrossPromotionItem item)
        {
            // Check if device has enouth space
            int availableDiskSpaceKb = DiskUtility.GetAvailableKiloBytes(CrossPromotionManager.CrossPromotionVideoPath);
            // If available space has a valid value and video size is higher than that value, return
            if (availableDiskSpaceKb > 0 && item.CreativeSizeKb > availableDiskSpaceKb)
            {
                HomaGamesLog.Debug($"[Cross Promotion] Could not download item {item.AppId} -- Not enough disk space. Required: {item.CreativeSizeKb}KB. Available: {availableDiskSpaceKb}KB");
                return;
            }

            using (HttpClient client = HttpCaller.GetHttpClient())
            {
                string outputPath = GetOutputPath(item);
                HttpResponseMessage response = await client.GetAsync(item.CreativeUrl, HttpCompletionOption.ResponseHeadersRead);
                if (response.IsSuccessStatusCode)
                {
                    using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                    {
                        using (Stream streamToWriteTo = File.Open(outputPath, FileMode.Create))
                        {
                            await streamToReadFrom.CopyToAsync(streamToWriteTo);
                            HomaGamesLog.Debug($"[Cross Promotion] File downloaded to {outputPath}");
                        }
                    }
                }
                else
                {
                    HomaGamesLog.Warning($"[Cross Promotion] Could not download file {item.CreativeUrl}. ERROR: {response.StatusCode} - {response.ReasonPhrase}");
                    throw new FileNotFoundException(response.ReasonPhrase);
                }
            }
        }

        private string GetOutputPath(CrossPromotionItem item)
        {
            string outputPath = item.LocalVideoFilePath;
            FileUtilities.CreateIntermediateDirectoriesIfNecessary(outputPath);
            return outputPath;
        }
    }
}
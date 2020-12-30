using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using MuzeiClient.Interfaces;
using MuzeiClient.Models;
using Newtonsoft.Json;

namespace MuzeiClient.Services
{
    public class MuzeiService : IMuzeiService
    {
        private readonly ILogger<MuzeiService> _logger;
        private readonly WorkerOptions _options;

        private readonly string _imageSaveLocation;
            
        public MuzeiService(WorkerOptions options, ILogger<MuzeiService> logger)
        {
            _options = options;
            _logger = logger;
            _imageSaveLocation = $@"{Registry.GetValue(_options.HKeyShellFolders,
                Constants.HKeyShellFoldersValueName, null)}{Constants.MuzeiSaveLocationPath}";
        }

        public async Task ProcessMuzeiRequest()
        {
            var jsonString = await GetPageHtml(_options.MuzeiUrl);
            var post = JsonConvert.DeserializeAnonymousType(jsonString, new
            {
                byline = "",
                detailsUri = "",
                imageUri = "",
                nextTime = "",
                thumbUri = "",
                title = ""
            });

            var imageName = post.imageUri[(post.imageUri.LastIndexOf("/", StringComparison.Ordinal) + 1)..];

            if (!File.Exists($"{_imageSaveLocation}{imageName}"))
            {
                await DownloadFile(post.imageUri, imageName);

                _logger.LogInformation($"Download image {imageName} from Muzei");

                SetWallpaper($"{_imageSaveLocation}{imageName}");
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(
            uint action, uint uParam, string vParam, uint winIni);

        private void SetWallpaper(string path)
        {
            using var key = Registry.CurrentUser.OpenSubKey(Constants.CurrentUserSubKey, true);
            key.SetValue("WallpaperStyle", 6.ToString());
            key.SetValue("TileWallpaper", 0.ToString());

            SystemParametersInfo(Constants.SpiSetdeskwallpaper, 0, path, Constants.SpifUpdateinifile | Constants.SpifSendwininichange);

            _logger.LogInformation($"Change wallpaper to image locate on {path}");
        }

        private async Task DownloadFile(string url, string fileName)
        {
            var file = $"{_imageSaveLocation}{fileName}";
            if (File.Exists(file)) return;

            Directory.CreateDirectory(_imageSaveLocation);

            using var objWc = new WebClient();
            await objWc.DownloadFileTaskAsync(new Uri(url), file);
        }

        private async Task<string> GetPageHtml(string url)
        {
            using var objWc = new WebClient();
            var data = await objWc.DownloadDataTaskAsync(new Uri(url));
            return new UTF8Encoding().GetString(data);
        }
    }
}
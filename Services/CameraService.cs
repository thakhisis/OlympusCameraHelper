using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using OlympusCameraHelper.Types;
using File = System.IO.File;

namespace OlympusCameraHelper.Services
{
    public class CameraService
    {
        Uri BaseUrl { get; }
        DirectoryInfo BasePath { get; }
        Func<string, Uri> GetDirectoryEndpoint => (dirname) => new Uri(BaseUrl, $"/get_imglist.cgi?DIR={dirname}");

        Func<string, string, Uri> ThumbnailEndpoint => (dirname, filename) =>
            new Uri(BaseUrl, $"/get_thumbnail.cgi?DIR={dirname}/{filename}");

        Func<string, string, Uri> ImageEndpoint =>
            (dirname, filename) => new Uri(BaseUrl, $"/DCIM/100OLYMP/{filename}");

        Dictionary<string, List<NavigationEntity>> NavigationCache { get; set; }
        Dictionary<string, object> MetadataCache { get; set; }
        Dictionary<string, string> ImageCache { get; set; }

        public CameraService(AppState state)
        {
            this.BaseUrl = state.Configuration.DefaultUri;
            this.BasePath = state.Configuration.LocalPath;
            this.NavigationCache = new Dictionary<string, List<NavigationEntity>>();
            this.MetadataCache = new Dictionary<string, object>();
            this.ImageCache = new Dictionary<string, string>();
        }

        public async Task<string> GetModel()
        {
            var url = new Uri(this.BaseUrl, "/get_caminfo.cgi");
            return await new WebClient().DownloadStringTaskAsync(url);
        }

        public async IAsyncEnumerable<NavigationEntity> GetDirectory(string dirname, int skip, int take)
        {
            if (!NavigationCache.ContainsKey(dirname))
            {
                var url = this.GetDirectoryEndpoint(dirname);
                var data = await new WebClient().DownloadStringTaskAsync(url);
                var result = ParseDirectory(data);
                NavigationCache.Add(dirname, result);
            }

            var entities = NavigationCache[dirname];
            foreach (var entity in entities.Skip(skip).Take(take * 2))
            {
                if (entity.Name.EndsWith("ORF"))
                    continue;
                yield return entity;
            }
        }

        private List<NavigationEntity> ParseDirectory(string html)
        {
            /* wlansd[741]="/DCIM/100OLYMP,P4150387.JPG,9981179,0,20623,33384"; */
            //var pattern2 = "wlansd\\[(?<index>\\d+)\\]=\"(?<dirname>.+?),(?<name>.+?),\\d+,\\d+,\\d+,\\d+\";";
            var pattern = "(?<dirname>.+?),(?<name>.+?),(?<num1>\\d+),(?<num2>\\d+),(?<num3>\\d+),(?<num4>\\d+)";
            var regex = new System.Text.RegularExpressions.Regex(pattern);
            var entities = new List<NavigationEntity>();
            for (var m = regex.Match(html); m.Success; m = m.NextMatch())
            {
                var name = m.Groups["name"].Value;
                var dirname = m.Groups["dirname"].Value;
                var num1 = int.Parse(m.Groups["num1"].Value);
                var num2 = int.Parse(m.Groups["num2"].Value);
                var num3 = int.Parse(m.Groups["num3"].Value);
                var num4 = int.Parse(m.Groups["num4"].Value);
                NavigationEntity entity;
                if (name.EndsWithAny(new[] {".JPG", ".ORF", ".PNG", ".JPEG"}))
                {
                    entity = new Image(dirname, name, num1, num2, num3, num4);
                }
                else if (!name.Contains("."))
                {
                    entity = new Types.Directory(dirname, name, num1, num2, num3, num4);
                }
                else
                {
                    entity = new Types.File(dirname, name, num1, num2, num3, num4);
                }

                entities.Add(entity);
            }

            return entities;
        }

        public async Task<string> GetThumbnail(string dirname, string imageName)
        {
            var url = this.ThumbnailEndpoint(dirname, imageName);
            var metadataKey = $"{dirname}/{imageName}/thumbnail";
            if (!MetadataCache.ContainsKey(metadataKey))
            {
                var data = await new WebClient().DownloadDataTaskAsync(url);
                MetadataCache[metadataKey] = System.Convert.ToBase64String(data);
            }

            return (string) MetadataCache[metadataKey];
        }

        public async Task<long> GetImageSize(string dirname, string imageName)
        {
            var url = this.ImageEndpoint(dirname, imageName);
            var request = WebRequest.Create(url);

            var metadataKey = $"{dirname}/{imageName}/size";
            if (!MetadataCache.ContainsKey(metadataKey))
            {
                using (var content = await request.GetResponseAsync())
                    MetadataCache[metadataKey] = content.ContentLength;
            }

            return (long) MetadataCache[metadataKey];
        }

        public async Task SaveImage(Image image, Action<long, long?> report, Action<string> done)
        {
            var url = this.ImageEndpoint(image.DirectoryName, image.Name);
            var buffer = new byte[80 * 1024];
            var read = 0;
            var index = 0l;

            var request = WebRequest.Create(url);
            var response = await request.GetResponseAsync();

            try
            {
                using (var content = response.GetResponseStream())
                {
                    using (var target = File.OpenWrite($@"C:\temp\{image.Name}"))
                    {
                        while ((read = await content.ReadAsync(buffer, 0, buffer.Length,
                            new System.Threading.CancellationToken())) > 0)
                        {
                            await target.WriteAsync(buffer, 0, read, new System.Threading.CancellationToken());
                            index += read;
                            report(index, response.ContentLength);
                        }
                    }
                }
                done(null);
            }
            catch (Exception e)
            {
                done(e.Message);
                return;
            }
        }
    }


    public static class StringExtention
    {
        public static bool EndsWithAny(this System.String str, IEnumerable<string> endings)
        {
            return endings.Any(e => str.EndsWith(e));
        }
    }
}
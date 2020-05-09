using Microsoft.Extensions.DependencyInjection;
using WebWindows.Blazor;

namespace OlympusCameraHelper
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<OlympusCameraHelper.Services.AppState>();
            services.AddScoped<OlympusCameraHelper.Services.TaskRunner>();
            services.AddScoped<OlympusCameraHelper.Services.CameraService>();
            services.AddScoped<OlympusCameraHelper.Services.DownloadService>();
            services.AddTransient<System.Net.Http.HttpClient>();
            services.AddTransient<System.Net.WebClient>();
        }

        public void Configure(DesktopApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}

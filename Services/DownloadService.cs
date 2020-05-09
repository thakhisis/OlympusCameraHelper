using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using OlympusCameraHelper.Types;

namespace OlympusCameraHelper.Services
{
    public class DownloadService {
        public ConcurrentBag<DownloadTask> Downloads { get; private set; }
        private CameraService service;
        private int ConcurrentConnections = 2;
        
        private System.Timers.Timer Timer {get;set;}

        public DownloadService(CameraService service) {
            this.service = service;
            this.Downloads = new ConcurrentBag<DownloadTask>();
            var t = new System.Timers.Timer() {
                Interval = TimeSpan.FromSeconds(10).TotalMilliseconds
            };
            t.Elapsed += async (o,s) => {
                 await Start();
                 Console.Write("1");
            };
            t.Start();
        }

        //public async Task Enqueue(Image image) {
        //    var task = new Task(async () => await service.SaveImage(image.DirectoryName, image.Name, (current, max) => {
        //        if (max != null) {
        //            image.DownloadProgress = (int)((100*current) / max) / 100d;
        //        }
        //        image.DownloadCurrent = current;
        //        this.StateHasChanged();
        //    }, (err) => {
        //        if (string.IsNullOrEmpty(err)) {
        //            image.State = Image.DownloadState.Completed;
        //        } else {
        //            image.State = Image.DownloadState.Failed;
        //            image.Error = err;
        //        }
        //    }));
//
        //    Downloads.Add(new DownloadTask(image, task));
        //    image.State = Image.DownloadState.Enqueued;
        //}

        public async Task<string> TestTask(int delay) {
            await Task.Delay(delay);
            return "hey + " + delay;
        }

        public async Task Cancel() {

        }

        private async Task Start() {
            var currentlyRunning = Downloads.Count(dt => dt.Task.Status == TaskStatus.Running);
            if (currentlyRunning >= ConcurrentConnections) 
                return;

            var startNew = Downloads.FirstOrDefault(dt => dt.Image.State == Image.DownloadState.Enqueued);
            if (startNew == null) 
                return;

            startNew.Image.State = Image.DownloadState.Downloading;
            startNew.Task.Start();
            int i = 0;
            i++;
        }


        public event EventHandler StateChanged;
        private void StateHasChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

    }

    public class DownloadTask {
        public Image Image {get; private set;}
        public Task Task { get; private set; }
        public DownloadTask(Image image, Task task) {
            this.Image = image;
            this.Task = task;
        }
    }
}

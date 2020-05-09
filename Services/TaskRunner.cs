using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using OlympusCameraHelper.Types;
using System.Threading;

namespace OlympusCameraHelper.Services
{
    public class TaskRunner {
        public enum Priority
        {
            Normal = 3, Low = 4, Lowest = 5, High = 2, Highest = 1 
        }
        private enum TaskState {
            Queued, Running, Completed, Failed
        }
        
        public int Count
        {
            get { return this.Tasks.Count(); }
        }

        private class TaskWrapper {
            public DateTime Created { get;  }
            public Priority Priority { get; }
            public TaskWrapper(Func<Task> task, Priority priority, CancellationTokenSource cancellationTokenSource) {
                this.Task = task;
                this.CancellationTokenSource = cancellationTokenSource;
                this.Created = DateTime.Now;
                this.Priority = priority;
            }
            public TaskState State {get;set;}    
            public CancellationTokenSource CancellationTokenSource {get; set;}    
            public Func<Task> Task {get; private set;}
        }

        private ConcurrentBag<TaskWrapper> Tasks { get; set; }
        public int ConcurrentConnections { get; set; }
        private System.Timers.Timer Timer {get;set;}

        public TaskRunner() {
            this.Tasks = new ConcurrentBag<TaskWrapper>();
            var t = new System.Timers.Timer() {
                Interval = TimeSpan.FromMilliseconds(500).TotalMilliseconds
            };
            t.Elapsed += async (o,s) => {
                System.Console.WriteLine("Timer ping");
                Process();
            };
            System.Console.WriteLine("Starting timer");
            this.Timer = t;            
        }

        public void Enqueue(Func<Task> a, Priority priority = Priority.Normal)
        {
            //task.ConfigureAwait(false);
            this.Tasks.Add(new TaskWrapper(a, priority, new CancellationTokenSource()));
            this.StateHasChanged();
            //Process();
        }

        public void Cancel() {

        }

        public void Start(int concurrentConnections)
        {
            this.ConcurrentConnections = concurrentConnections;
            this.Timer.Start();
            //Process();
        }

        public void Stop() {
            this.Timer.Stop();
            foreach (var t in this.Tasks) {
                t.CancellationTokenSource.Cancel();
            }
        }

        private void Process() {
            //Console.WriteLine("Processing");
            if (!this.Timer.Enabled)
                return;
            
            var currentlyRunning = Tasks.Count(t => t.State == TaskState.Running);
            if (currentlyRunning >= ConcurrentConnections) 
                return;

            var startNew = Tasks.Where(t => t.State == TaskState.Queued)
                .OrderBy(tw => tw.Priority)
                .ThenBy(tw => tw.Created)
                .Take(ConcurrentConnections - currentlyRunning);
            
            if (startNew == null || !startNew.Any()) 
                return;
            
            Console.WriteLine($"Starting {startNew.Count()}");
            foreach (var sn in startNew) {
                StartSingle(sn);
            }

            this.StateHasChanged();
        }

        private async Task StartSingle(TaskWrapper tw) {
            tw.State = TaskState.Running;
            //System.Console.WriteLine($"Task {tw.Task.GetHashCode()} started");
            
            await tw.Task();
            tw.State = TaskState.Completed;
            //System.Console.WriteLine($"Task {tw.Task.GetHashCode()} completed");
            Process();
            
            // FIGUURE OUT CANCELLATION TOKENS;
        }
        
        public event EventHandler StateChanged;
        public void StateHasChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

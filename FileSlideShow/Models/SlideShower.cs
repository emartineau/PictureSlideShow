using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace FileSlideShow.Models
{
    /// <summary>
    /// Logic for slideshow controls and generation.
    /// </summary>
    class SlideShower
    {
        private readonly ICollection<string> SupportedExtensions = new string[]
        {
            ".png", ".jpg", ".jpeg", ".bmp"
        };
        private readonly int minTime = 500;// in ms

        private CancellationTokenSource cts;

        public event Action ImageChanged;
        public event Action StatusChanged;

        protected virtual void OnImageChanged() => ImageChanged?.Invoke();
        protected virtual void OnStatusChanged() => StatusChanged?.Invoke();

        public SlideShower(DirectoryInfo directory)
        {
            viewSubDirs = true;
            autoShuffle = true;
            CurrentFolder = directory;
            TimePerFile = 1000;
            Status = Status.Stopped;
        }

        public Status Status { get; private set; }

        private DirectoryInfo _currentFolder;
        public DirectoryInfo CurrentFolder
        {
            get => _currentFolder;
            set
            {
                _currentFolder = value;
                FileList = CurrentFolder.GetFiles();
                FileList = FileList.FilterFiles(SupportedExtensions);

                if (viewSubDirs)
                {
                    FileList = FileList.Concat(CurrentFolder.GetDirectories()
                        .SelectMany((dir => dir.GetFiles().FilterFiles(SupportedExtensions))));
                }

                if (autoShuffle)
                    Shuffle();
            }
        }

        public IEnumerable<FileInfo> FileList { get; private set; }

        private int _fileIndex = 0;
        public int FileIndex
        {
            get => _fileIndex;
            set =>
                _fileIndex = value < 0 ?
                Math.Max(FileList.Count() - 1, 0) :
                value % (FileList.Count() > 0 ? FileList.Count() : 1);
        }

        private int _timePerFile;
        public int TimePerFile
        { get => _timePerFile; set => _timePerFile = value < minTime ? minTime : value; }

        public bool viewSubDirs;
        public bool autoShuffle;

        public void Shuffle()
        {
            FileList = FileList.Shuffle();
        }

        async public Task PlayAsync()
        {
            // Adapted from https://docs.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads
            cts = new CancellationTokenSource();

            Status = Status.Playing;
            OnStatusChanged();

            while (Status == Status.Playing)
            {
                await Task.Delay(TimePerFile);
                try
                {
                    await Task.Run(() => FileIndex++, cts.Token);
                }
                catch (ObjectDisposedException e)
                {
                    // User has paused or stopped slideshow. Expected behavior.
                }
                OnImageChanged();
            }
        }

        public void Pause()
        {
            CancelSlideShow();

            Status = Status.Paused;
            OnStatusChanged();
        }

        public void Stop()
        {
            CancelSlideShow();

            Status = Status.Stopped;
            OnStatusChanged();
            FileIndex = 0;
            OnImageChanged();
        }

        public async Task TogglePlayAsync()
        {
            if (Status != Status.Playing)
            {
                await PlayAsync();
            }
            else
            {
                Pause();
            }
        }

        public void SpeedUp(int ms) => TimePerFile -= ms;
        public void SlowDown(int ms) => TimePerFile += ms;

        private void CancelSlideShow()
        {
            if (Status == Status.Playing)
            {
                cts.Cancel();
                cts.Dispose();
            }
        }
    }
}

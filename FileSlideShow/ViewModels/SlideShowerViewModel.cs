using FileSlideShow.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FileSlideShow.ViewModels
{
    class SlideShowerViewModel : INotifyPropertyChanged
    {
        private SlideShower SlideShower;
        private readonly string defaultDir = Environment.GetFolderPath(
            Environment.SpecialFolder.MyPictures);

        public SlideShowerViewModel()
        {
            SlideShower = new SlideShower(new DirectoryInfo(defaultDir));
            SlideShower.Shuffle();
            InitCommands();

            SlideShower.ImageChanged += () => OnPropertyChanged("CurrentFilePath");
            SlideShower.StatusChanged += () => OnPropertyChanged("CurrentStatus");
        }

        public string DirPath
        {
            get => SlideShower.CurrentFolder?.FullName;
            set => SlideShower.CurrentFolder = new DirectoryInfo(value);
        }
        
        public IEnumerable<string> FPath()
        {
            foreach (var fileInfo in SlideShower.FileList)
            {
                yield return fileInfo.FullName;
            }
        }

        public  string CurrentFilePath { get => FPath().ElementAtOrDefault(FileIndex); }
        public int FileIndex
        {
            get => SlideShower.FileIndex;
            set { SlideShower.FileIndex = value; OnPropertyChanged("CurrentFilePath"); }
        }

        public string Tempo { get => $"{ SlideShower.TimePerFile }ms"; }
        public string CurrentStatus { get => $"STATUS: { SlideShower.Status.ToString() }"; }

        public ICommand Play { get; private set; }
        public ICommand Pause { get; private set; }
        public ICommand Stop { get; private set; }
        public ICommand Shuffle { get; private set; }
        public ICommand PickFolder { get; private set; }
        public ICommand SpeedUp { get; private set; }
        public ICommand SlowDown { get; private set; }
        public ICommand TogglePlay { get; private set; }
        public ICommand Previous { get; private set; }
        public ICommand Next { get; private set; }
        public ICommand ToggleSubFolders { get; private set; }
        public ICommand ToggleAutoShuffle { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void InitCommands()
        {
            Play = new RelayCommand(
                async () => await SlideShower.PlayAsync(),
                () => SlideShower.Status != Status.Playing);

            Pause = new RelayCommand(
                () => SlideShower.Pause(),
                CanExecuteTrue());

            Stop = new RelayCommand(
                () => SlideShower.Stop(),
                CanExecuteTrue());

            TogglePlay = new RelayCommand(
                async () => await SlideShower.TogglePlayAsync(),
                CanExecuteTrue());

            Shuffle = new RelayCommand(
                () => 
                {
                    SlideShower.Shuffle();
                    OnPropertyChanged("CurrentFilePath");
                },
                CanExecuteTrue());

            PickFolder = new RelayCommand(
                OpenFileDialog,
                CanExecuteTrue());

            SlowDown = new RelayCommand(
                () =>
                {
                    SlideShower.SlowDown(100);
                    OnPropertyChanged("Tempo");
                },
                CanExecuteTrue());

            SpeedUp = new RelayCommand(
                () =>
                {
                    SlideShower.SpeedUp(100);
                    OnPropertyChanged("Tempo");
                }, 
                CanExecuteTrue());

            Previous = new RelayCommand(
                () =>
                {
                    FileIndex--;
                    OnPropertyChanged("CurrentFilePath");
                },
                CanExecuteTrue());

            Next = new RelayCommand(
                () =>
                {
                    FileIndex++;
                    OnPropertyChanged("CurrentFilePath");
                },
                CanExecuteTrue());

            ToggleSubFolders = new RelayCommand(
                () => SlideShower.viewSubDirs = !SlideShower.viewSubDirs,
                CanExecuteTrue());

            ToggleAutoShuffle = new RelayCommand(
                () => SlideShower.autoShuffle = !SlideShower.autoShuffle, 
                CanExecuteTrue());
        }

        private Func<bool> CanExecuteTrue() => () => true;

        private void OpenFileDialog()
        {
            var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                RootFolder = Environment.SpecialFolder.MyComputer,
                Description = "Pick a folder to switch to.",
                ShowNewFolderButton = true
            };

            folderBrowserDialog.ShowDialog();

            if (!string.IsNullOrEmpty(folderBrowserDialog.SelectedPath))
            {
                DirPath = folderBrowserDialog.SelectedPath;
            }
            OnPropertyChanged("CurrentFilePath");
        }
    }
}

using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Updater.Classes;

namespace Updater.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private ModpackWorker modpackWorker;
        public enum Status { WAITING, UPDATE_READY, UPDATING, READY, LAUNCHING, ERROR };

        public MainWindowViewModel()
        {
            modpackWorker = new ModpackWorker(this);

            modpackWorker.StartWorker();
        }

        public void RunAction()
        {
            if (UpdaterStatus == Status.UPDATE_READY)
            {
                UpdaterStatus = Status.UPDATING;
                modpackWorker.PerformUpdate();
            }
            else
            {
                UpdaterStatus = Status.LAUNCHING;
                foreach (var launcher in Config.Launchers)
                {
                    var launcherPath = Path.Combine(modpackWorker.GamePath, launcher);
                    if (File.Exists(launcherPath))
                    {
                        System.Diagnostics.Process.Start(launcherPath);
                        Application.Current.Shutdown();
                        break;
                    }
                }
            }
        }

        //private void CompleteUpdate()
        //{
        //    var modDirectory = Path.Combine(gamePath, "mods");
        //    if (Directory.Exists(modDirectory) == false)
        //    {
        //        Directory.CreateDirectory(modDirectory);
        //    }

        //    /** Clearing all mods **/
        //    var di = new DirectoryInfo(modDirectory);
        //    foreach (var file in di.GetFiles())
        //    {
        //        file.Delete();
        //    }
        //    foreach (var dir in di.GetDirectories())
        //    {
        //        dir.Delete(true);
        //    }

        //    ZipHelper.UnZip(Path.Combine(gamePath, "modpack.zip"), modDirectory);

        //    File.WriteAllText(Config.FILE_VERSION, UnixTimeHelper.GetCurrentUnixTimestampSeconds().ToString());

        //    ButtonText = Config.BUTTON_STATE_START;
        //    ButtonEnabled = true;
        //}

        private Status updaterStatus = Status.WAITING;
        public Status UpdaterStatus
        {
            get
            {
                return updaterStatus;
            }

            set
            {
                SetProperty(ref updaterStatus, value);
                switch (value)
                {
                    case Status.WAITING:
                        ButtonEnabled = false;
                        ButtonText = Config.BUTTON_STATE_WAITING;
                        break;
                    case Status.UPDATE_READY:
                        ButtonEnabled = true;
                        ButtonText = Config.BUTTON_STATE_UPDATE_READY;
                        break;
                    case Status.UPDATING:
                        ButtonEnabled = false;
                        ButtonText = Config.BUTTON_STATE_UPDATING;
                        break;
                    case Status.READY:
                        ButtonEnabled = true;
                        ButtonText = Config.BUTTON_STATE_READY;
                        break;
                    case Status.LAUNCHING:
                        ButtonEnabled = false;
                        ButtonText = Config.BUTTON_STATE_LAUNCHING;
                        break;
                    case Status.ERROR:
                        ButtonEnabled = false;
                        InfoText = Config.INFO_STATUS_ERROR;
                        break;
                }
            }
        }
        private string infoText = "";
        public string InfoText
        {
            get
            {
                return infoText;
            }

            set
            {
                SetProperty(ref infoText, value);
            }
        }
        private string buttonText = Config.BUTTON_STATE_WAITING;
        public string ButtonText
        {
            get
            {
                return buttonText;
            }

            set
            {
                SetProperty(ref buttonText, value);
            }
        }
        private bool buttonEnabled = false;
        public bool ButtonEnabled
        {
            get
            {
                return buttonEnabled;
            }

            set
            {
                SetProperty(ref buttonEnabled, value);
            }
        }

        private long downloadedSize = 0;
        public long DownloadedSize
        {
            get
            {
                return downloadedSize;
            }

            set
            {
                SetProperty(ref downloadedSize, value);
                ProgressValue = (int)((double)value / (double)totalSize * (double)100);
            }
        }

        private long totalSize = 0;
        public long TotalSize
        {
            get
            {
                return totalSize;
            }

            set
            {
                SetProperty(ref totalSize, value);
            }
        }

        private int progressValue = 0;
        public int ProgressValue
        {
            get
            {
                return progressValue;
            }

            set
            {
                SetProperty(ref progressValue, value);
            }
        }

        private ICommand clickCommand;
        public ICommand ClickCommand
        {
            get
            {
                return clickCommand ?? (clickCommand = new CommandHelper(() => RunAction(), true));
            }
        }
    }
}
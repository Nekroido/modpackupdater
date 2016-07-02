using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Updater.Classes.Helpers;
using Updater.Models;
using Updater.ViewModels;

namespace Updater.Classes
{
    public class ModpackWorker
    {
        private VersionModel localModpackVersion;
        private VersionModel serverModpackVersion;

        private MainWindowViewModel mainWindowViewModel;

        public string GamePath = "";
        public string ModsDirectory = "";

        private long bytesReceived = 0;

        private Queue<KeyValuePair<string, string>> fileQueue = new Queue<KeyValuePair<string, string>>();

        public ModpackWorker(MainWindowViewModel mainWindowViewModel)
        {
            this.mainWindowViewModel = mainWindowViewModel;
            localModpackVersion = readModpackVersionFile();

            GamePath = Config.USE_GLOBAL_PATH
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Config.GAME_FOLDER)
                : "";

            ModsDirectory = Path.Combine(GamePath, "mods");
            if (Directory.Exists(ModsDirectory) == false)
            {
                Directory.CreateDirectory(ModsDirectory);
            }
        }

        /// <summary>
        /// Initializes the modpack update worker with info from the server and proper status values for view model
        /// </summary>
        /// <returns></returns>
        public async Task StartWorker()
        {
            mainWindowViewModel.UpdaterStatus = MainWindowViewModel.Status.WAITING;
            await getServerModpackVersion();

            if (IsUpdateRequired())
            {
                mainWindowViewModel.UpdaterStatus = MainWindowViewModel.Status.UPDATE_READY;
            }
            else
            {
                VerifyIntegrity();
            }

            if (serverModpackVersion == null)
            {
                mainWindowViewModel.UpdaterStatus = MainWindowViewModel.Status.ERROR;
            }
        }

        /// <summary>
        /// Starts update of modpack files
        /// </summary>
        /// <returns></returns>
        public void PerformUpdate()
        {
            var modsToUpdate = getAndDeleteOutdatedMods();

            enqueueMods(modsToUpdate);

            downloadFile();
        }

        /// <summary>
        /// Completes the update process
        /// </summary>
        public void CompleteUpdate()
        {
            writeModpackVersionFile();

            mainWindowViewModel.InfoText = "";
            mainWindowViewModel.ProgressValue = 100;
            mainWindowViewModel.UpdaterStatus = MainWindowViewModel.Status.READY;
        }

        /// <summary>
        /// Verifies modpack integrity and downloads missing or corrupted files
        /// </summary>
        public void VerifyIntegrity()
        {
            var modsToUpdate = new List<ModModel>();

            foreach (var mod in serverModpackVersion.Mods)
            {
                var modFilePath = Path.Combine(ModsDirectory, mod.File);
                if (!File.Exists(modFilePath) || StringHelper.CalculateMD5(modFilePath) != mod.Checksum)
                    modsToUpdate.Add(mod);
            }

            if (modsToUpdate.Any())
            {
                enqueueMods(modsToUpdate);

                downloadFile();
            }
            else
            {
                CompleteUpdate();
            }
        }

        /// <summary>
        /// Downloads modpack information from the server
        /// </summary>
        /// <returns></returns>
        private async Task getServerModpackVersion()
        {
            serverModpackVersion = await Api.GetModpackVersion();
        }

        /// <summary>
        /// Verify if local modpack is outdated
        /// </summary>
        /// <returns></returns>
        public bool IsUpdateRequired()
        {
            /// Modpack updater data is corrupted or absent
            /// Delete all 'em mods
            if (localModpackVersion?.Updated == null)
            {
                clearModsFolder();
            }

            return localModpackVersion?.Updated == null || serverModpackVersion.Updated != localModpackVersion.Updated;
        }

        private void enqueueMods(List<ModModel> mods)
        {
            bytesReceived = 0;
            mainWindowViewModel.TotalSize = mods.Sum(x => x.FileSize);
            foreach (var mod in mods)
            {
                fileQueue.Enqueue(new KeyValuePair<string, string>(mod.Name, mod.File));
            }
        }

        /// <summary>
        /// Returns list of outdated mods
        /// Deletes outdated or removed mods files
        /// </summary>
        private List<ModModel> getAndDeleteOutdatedMods()
        {
            var modsToUpdate = new List<ModModel>();

            if (localModpackVersion?.Mods == null)
            {
                return serverModpackVersion.Mods;
            }

            List<ModModel> modsToRemove = new List<ModModel>();

            foreach (var mod in serverModpackVersion.Mods)
            {
                var localModVersion = localModpackVersion.Mods.FirstOrDefault(x => x.Name == mod.Name);
                if (localModVersion == null || mod.Updated > localModVersion.Updated)
                {
                    modsToUpdate.Add(mod);

                    if (localModVersion != null)
                    {
                        var modFilePath = Path.Combine(ModsDirectory, localModVersion.File);
                        if (File.Exists(modFilePath))
                            File.Delete(modFilePath);
                    }
                }
            }

            modsToRemove = localModpackVersion.Mods.Where(x => !serverModpackVersion.Mods.Select(y => y.Name).Contains(x.Name)).ToList();

            foreach (var mod in modsToRemove)
            {
                var modFilePath = Path.Combine(ModsDirectory, mod.File);
                if (File.Exists(modFilePath))
                    File.Delete(modFilePath);
            }

            return modsToUpdate;
        }

        #region File download
        /// <summary>
        /// Download an enqueued file
        /// </summary>
        private void downloadFile()
        {
            if (fileQueue.Any())
            {
                var modData = fileQueue.Dequeue();
                var filename = modData.Value;
                var modName = modData.Key;
                mainWindowViewModel.InfoText = modName;

                using (WebClient client = new WebClient())
                {
                    var modFileUrl = Config.MODPACK_URL + Config.MODS_DIR + modName + "/" + filename;
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("Downloading " + modName);
#endif
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(wc_DownloadProgressChanged);
                    client.DownloadFileCompleted += wc_DownloadFileCompleted;
                    client.DownloadFileAsync(new Uri(modFileUrl, UriKind.Absolute), Path.Combine(ModsDirectory, filename));
                }
                return;
            }
            
            VerifyIntegrity();
        }

        /// <summary>
        /// Resets bytes counter and launches download of next file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null || e.Cancelled)
            {
                mainWindowViewModel.UpdaterStatus = MainWindowViewModel.Status.ERROR;
                return;
            }

            bytesReceived = 0;
            downloadFile();
        }

        /// <summary>
        /// Shows progress of a file download
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            mainWindowViewModel.DownloadedSize += (int)((long)e.BytesReceived - bytesReceived);
            bytesReceived = e.BytesReceived;
        }
        #endregion

        #region Version file manipulation
        /// <summary>
        /// Returns local modpack version data
        /// </summary>
        /// <returns></returns>
        private VersionModel readModpackVersionFile()
        {
            if (File.Exists(Config.FILE_VERSION))
            {
                var version = File.ReadAllText(Config.FILE_VERSION);

                try
                {
                    var versionModel = JsonConvert.DeserializeObject<VersionModel>(version);

                    return versionModel;
                }
                catch
                {
                    // Pass further
                }
            }
            else
            {
                File.CreateText(Config.FILE_VERSION);
            }

            return new VersionModel();
        }

        /// <summary>
        /// Update modpack version file with data from the server
        /// </summary>
        private void writeModpackVersionFile()
        {
            File.WriteAllText(Config.FILE_VERSION, JsonConvert.SerializeObject(serverModpackVersion));
        }
        #endregion

        /// <summary>
        /// Clears mods folder
        /// </summary>
        private void clearModsFolder()
        {
            var di = new DirectoryInfo(ModsDirectory);
            foreach (var file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (var dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }
    }
}

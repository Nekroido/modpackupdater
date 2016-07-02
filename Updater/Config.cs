namespace Updater
{
    public sealed class Config
    {
        public const string BUTTON_STATE_WAITING = "Getting ready...";
        public const string BUTTON_STATE_UPDATE_READY = "Start update";
        public const string BUTTON_STATE_UPDATING = "Updating";
        public const string BUTTON_STATE_READY = "Launch";
        public const string BUTTON_STATE_LAUNCHING = "Launching the game";
        
        public const string INFO_STATUS_ERROR = "There was an error connecting to the server";

        public const string FILE_VERSION = "modpackupdated";

        public const string MODPACK_URL = "http://my-minecraft-server/";

        public const string MODPACK_INFO = "version.php";
        public const string MODS_DIR = "mods/";
        public const string EXTRA_FILES = "extra";

        public const bool USE_GLOBAL_PATH = true;
        public const string GAME_FOLDER = ".minecraft";

        public static readonly string[] Launchers = new string[] {
            "Minecraft Launcher.exe",
            "Minecraft.exe",
        };
    }
}

namespace AutoServerPro.Models
{
    public class ModConfig
    {
        // ===== 基础 =====
        public string Language { get; set; } = "zh";
        public string petname { get; set; } = "Fido";
        public bool EnableSceneSync { get; set; } = true;
        public long SyncPlayerId { get; set; } = 0;

        // ===== 存档路径 =====
        public string CustomSavesPath { get; set; } = "";
        public string BackupPath { get; set; } = "";
        public string NewSaveName { get; set; } = "";

        // ===== 自动创建默认值 =====
        public string DefaultFarmName { get; set; } = "联机";
        public string DefaultHostName { get; set; } = "管理员";

        // ===== 新世界创建 =====
        public int FarmType { get; set; } = 0;
        public bool CreateMushroomCave { get; set; } = true;
        public bool UseSeparateWallets { get; set; } = false;
        public bool CabinLayoutNearby { get; set; } = false;
        public bool BundlesRemix { get; set; } = false;
        public bool MinesRemix { get; set; } = false;
        public bool CommunityCenterYear1 { get; set; } = false;
        public bool UseLegacyRandom { get; set; } = false;
        public ulong? RandomSeed { get; set; } = null;
        public int PetBreed { get; set; } = 0;
        public float ProfitMargin { get; set; } = 1.0f;
        public bool? SpawnMonstersAtNight { get; set; } = null;
        public int StartingCabins { get; set; } = 1;

        // ===== CPU优化（无头服务器） =====
        public bool EnableCPUOptimization { get; set; } = true;
        public bool SkipDrawing { get; set; } = true;
        public bool DisableAudio { get; set; } = true;
        public bool DisableWeatherParticles { get; set; } = true;

        // ===== 输入控制（各开关独立） =====
        public bool DisableKeyboardInput { get; set; } = false;
        public bool DisableMouseInput { get; set; } = false;
        public bool DisableGamepadInput { get; set; } = true;

        // ===== 备份 =====
        public int AutoBackupDayInterval { get; set; } = 7;
        public bool AutoCleanOldBackup { get; set; } = true;
        public int MaxBackupCount { get; set; } = 5;
    }
}
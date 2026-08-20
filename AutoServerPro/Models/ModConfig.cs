namespace AutoServerPro.Models;

public class ModConfig
{
    public string Language { get; set; } = "zh";
    public string petname { get; set; } = "Fido";
    public bool EnableSceneSync { get; set; } = true;
    public long SyncPlayerId { get; set; } = 0;

    public string CustomSavesPath { get; set; } = "";
    public string BackupPath { get; set; } = "";
    public string CustomTempSavesPath { get; set; } = "";
    public string NewSaveName { get; set; } = "";

    public string DefaultFarmName { get; set; } = "联机";
    public string DefaultHostName { get; set; } = "管理员";

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

    public bool EnableCPUOptimization { get; set; } = true;
    public bool SkipDrawing { get; set; } = true;
    public bool DisableAudio { get; set; } = true;
    public bool DisableWeatherParticles { get; set; } = true;

    public bool DisableKeyboardInput { get; set; } = true;
    public bool DisableMouseInput { get; set; } = true;
    public bool DisableGamepadInput { get; set; } = true;

    public bool SaveWhenAllPlayersOffline { get; set; } = true;
    public int AutoBackupDayInterval { get; set; } = 7;
    public bool AutoCleanOldBackup { get; set; } = true;
    public int MaxBackupCount { get; set; } = 5;
}

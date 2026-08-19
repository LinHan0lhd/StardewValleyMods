namespace AutoServerPro.Models;

public class CreateWorldResult
{
    public bool Success { get; set; }
    public string? SaveName { get; set; }
    public string? SavePath { get; set; }
    public string? ErrorMessage { get; set; }

    public static CreateWorldResult Ok(string saveName, string path)
    {
        return new CreateWorldResult
        {
            Success = true,
            SaveName = saveName,
            SavePath = path
        };
    }

    public static CreateWorldResult Fail(string error)
    {
        return new CreateWorldResult
        {
            Success = false,
            ErrorMessage = error
        };
    }
}

using System.Linq;
using UnityEngine;

public partial struct EntityAPI
{
    private static void LogError(string area, string message) =>
        Debug.LogError($"[Entity API] {area} Error: {message}");
    private static void Log(string area, string message) =>
        Debug.Log($"[Entity API] {area} {message}");
    
    /// <summary>
    /// Loads the modules added to the function.
    /// </summary>
    public static void Load()
    {
        Load_TextureModule();   
        Load_ECModule();
    }
}
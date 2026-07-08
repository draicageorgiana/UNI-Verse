using System;
using System.IO;
using UnityEngine;

/// <summary>
/// The local JSON serialization layer (§3.4.2) plus the security controls that
/// protect it (§3.8). Replaces any database server with a single gamesave.json
/// file in the host device's local application data folder (§2.6, Appendix D).
///
/// Saving:  the active plain SaveData object is converted into a structured
///          JSON string using Unity's native JsonUtility.ToJson() and written
///          directly to physical storage via standard file I/O.
/// Loading: runs inside structured try-catch blocks. The text is validated to
///          be structurally valid JSON that maps correctly onto the SaveData
///          schema; a tampered/corrupted file is discarded and a clean default
///          profile is generated instead, preventing runtime crashes (§3.8).
/// </summary>
public static class SaveSystem
{
    public const string SaveFileName = "gamesave.json";

    public static string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    public static bool SaveFileExists()
    {
        try { return File.Exists(SaveFilePath); }
        catch { return false; }
    }

    /// <summary>Serializes the immutable state container to gamesave.json (§3.4.2).</summary>
    public static bool Save(SaveData data)
    {
        if (data == null)
        {
            Debug.LogWarning("SaveSystem: refused to save a null SaveData instance.");
            return false;
        }

        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SaveFilePath, json);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveSystem: failed to write {SaveFileName} — {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Validated loading pipeline (§3.8). Never throws and never returns null:
    /// corrupt, tampered or missing saves fall back to a clean default profile.
    /// </summary>
    public static SaveData LoadOrDefault()
    {
        if (!SaveFileExists())
        {
            return SaveData.CreateDefault();
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);

            if (!PassesSchemaValidation(json))
            {
                throw new FormatException("save text failed structural JSON schema validation");
            }

            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (data == null || !data.IsValid())
            {
                throw new FormatException("save text mapped onto an invalid object state");
            }

            return data;
        }
        catch (Exception e)
        {
            // §3.8: the validation check catches the error, discards the damaged
            // save data and automatically generates a clean default profile.
            Debug.LogWarning($"SaveSystem: corrupted or tampered save discarded ({e.Message}). Generating a clean default profile.");
            SaveData fallback = SaveData.CreateDefault();
            Save(fallback); // heal the file on disk so the next boot is clean
            return fallback;
        }
    }

    /// <summary>
    /// Structural pre-check: the text must be a JSON object and must contain the
    /// core Appendix D keys before we even attempt to map it onto SaveData.
    /// </summary>
    private static bool PassesSchemaValidation(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return false;

        string trimmed = json.Trim();
        if (!trimmed.StartsWith("{") || !trimmed.EndsWith("}")) return false;

        return trimmed.Contains("\"PlayerName\"")
            && trimmed.Contains("\"CompletedChapters\"")
            && trimmed.Contains("\"TotalCredits\"");
    }

    public static void Delete()
    {
        try
        {
            if (SaveFileExists())
            {
                File.Delete(SaveFilePath);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SaveSystem: could not delete {SaveFileName} — {e.Message}");
        }
    }
}

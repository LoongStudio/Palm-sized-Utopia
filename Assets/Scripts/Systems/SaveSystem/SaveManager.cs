using UnityEngine;

public class SaveManager : SingletonManager<SaveManager>
{
    private string saveFilePath;
    
    public void Initialize() { }
    
    public void SaveGame() { }
    public bool LoadGame() { return false; }
    public void DeleteSave() { }
    public bool HasSaveFile() { return false; }
    
    private GameSaveData CollectSaveData() { return null; }
    private void ApplySaveData(GameSaveData saveData) { }
}
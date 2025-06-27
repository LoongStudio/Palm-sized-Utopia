public interface ISaveable
{
    SaveData GetSaveData();
    void LoadFromData(SaveData data);
}


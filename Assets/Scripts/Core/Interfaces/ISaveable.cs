public interface ISaveable
{
    GameSaveData GetSaveData();
    void LoadFromData(GameSaveData data);
}


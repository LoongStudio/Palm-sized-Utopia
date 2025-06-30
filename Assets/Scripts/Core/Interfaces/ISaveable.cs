public interface ISaveable
{
    GameSaveData GetSaveData();
    bool LoadFromData(GameSaveData data);
}


public interface ISaveable
{
    SaveData SaveToData();
    void LoadFromData(SaveData data);
}

[System.Serializable]
public class SaveData
{
    // 基础保存数据类，具体类型继承扩展
} 
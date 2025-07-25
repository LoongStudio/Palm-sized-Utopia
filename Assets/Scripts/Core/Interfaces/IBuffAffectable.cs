using System.Collections.Generic;

public interface IBuffAffectable{
    Dictionary<BuffEnums, int> Buffs{get; set;}
    // 被Buff影响
    void OnBuffAffected(Buff buff);
    // 被Buff影响结束
    void OnBuffAffectedEnd(Buff buff);

}
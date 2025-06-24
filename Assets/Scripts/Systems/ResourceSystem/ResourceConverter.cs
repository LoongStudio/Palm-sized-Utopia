using System.Collections.Generic;
[System.Serializable]
public class ConversionRule
{
    public List<ResourceStack> inputs;
    public List<ResourceStack> outputs;
    // public float conversionTime; TODO: 现在 Conversion 时间都交给建筑单独控制
}

public class ResourceConverter
{
    private List<ConversionRule> conversionRules;
    
    public void Initialize() { }
    public void AddConversionRule(ConversionRule rule) { }
    public List<ResourceStack> GetPossibleOutputs(List<ResourceStack> inputs) { return null; }
    public bool CanConvert(List<ResourceStack> inputs) { return false; }
}
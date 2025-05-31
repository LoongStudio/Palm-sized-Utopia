using System.Collections.Generic;
[System.Serializable]
public class ConversionRule
{
    public List<Resource> inputs;
    public List<Resource> outputs;
    public float conversionTime;
}

public class ResourceConverter
{
    private List<ConversionRule> conversionRules;
    
    public void Initialize() { }
    public void AddConversionRule(ConversionRule rule) { }
    public List<Resource> GetPossibleOutputs(List<Resource> inputs) { return null; }
    public bool CanConvert(List<Resource> inputs) { return false; }
}
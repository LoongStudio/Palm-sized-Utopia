public interface IResourceProducer
{
    void StartProduction();
    void StopProduction();
    bool CanProduce();
    void ProduceResources();
} 
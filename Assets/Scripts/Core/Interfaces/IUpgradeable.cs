public interface IUpgradeable
{
    bool CanUpgrade();
    bool Upgrade();
    int GetUpgradePrice();
} 
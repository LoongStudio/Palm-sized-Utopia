using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using Unity.VisualScripting;

public class PlaceableShopPanel : BasePanel
{
    [SerializeField] private Button _1x1Button;
    [SerializeField] private Button _2x2Button;
    [SerializeField] private Button _3x3Button;
    [SerializeField] private Button _1x3Button;
    [SerializeField] private Button _3x1Button;

    private void OnEnable()
    {
        _1x1Button.onClick.AddListener(On1x1ButtonClick);
        _2x2Button.onClick.AddListener(On2x2ButtonClick);
        _3x3Button.onClick.AddListener(On3x3ButtonClick);
        _1x3Button.onClick.AddListener(On1x3ButtonClick);
        _3x1Button.onClick.AddListener(On3x1ButtonClick);
    }
    private void OnDisable()
    {
        _1x1Button.onClick.RemoveListener(On1x1ButtonClick);
        _2x2Button.onClick.RemoveListener(On2x2ButtonClick);
        _3x3Button.onClick.RemoveListener(On3x3ButtonClick);
        _1x3Button.onClick.RemoveListener(On1x3ButtonClick);
        _3x1Button.onClick.RemoveListener(On3x1ButtonClick);
    }
    private void On1x1ButtonClick()
    {
        TriggerLandBoughtEvent(PlaceableType.NormalLand_1x1);
        Hide();
    }
    private void On2x2ButtonClick()
    {
        TriggerLandBoughtEvent(PlaceableType.NormalLand_2x2);
        Hide();
    }
    private void On3x3ButtonClick()
    {
        TriggerLandBoughtEvent(PlaceableType.NormalLand_3x3);
        Hide();
    }
    private void On1x3ButtonClick()
    {
        TriggerLandBoughtEvent(PlaceableType.NormalLand_1x3);
        Hide();
    }
    private void On3x1ButtonClick()
    {
        TriggerLandBoughtEvent(PlaceableType.NormalLand_3x1);
        Hide();
    }
    private void TriggerLandBoughtEvent(PlaceableType type){
        var eventArgs = new BuildingEventArgs(){
            placeableType = type,
            eventType = BuildingEventArgs.BuildingEventType.LandBought,
        };
        GameEvents.TriggerLandBought(eventArgs);
    }
}
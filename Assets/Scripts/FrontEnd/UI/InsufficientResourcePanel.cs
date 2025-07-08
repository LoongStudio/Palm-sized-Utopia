using UnityEngine;
using TMPro;

public class InsufficientResourcePanel : BasePanel{
    private float closeTime = 3f;
    private float timer = 0f;
    [SerializeField] private TextMeshProUGUI insufficientResourceText;

    private void FixedUpdate()
    {
        if(timer >= closeTime)
        {
            Hide();
            timer = 0f;
        }
        else
        {
            timer += Time.deltaTime;
        }
    }
    public void SetInsufficientResourceText(string text)
    {
        insufficientResourceText.text = text;
    }
}
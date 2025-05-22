using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ContextUIAttributes : MonoBehaviour, IPointerClickHandler
{
    public GameObject belongTo;
    public bool stackToBelong = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        List<Button> buttons = GetComponentsInChildren<Button>().ToList();
        foreach (var button in buttons)
        {
            if (button.CompareTag("UI_CLOSE"))
            {
                button.onClick.AddListener(() =>
                {
                    gameObject.SetActive(false);
                    ClickManager.Instance.selectedObject = null;
                });
            }
            if (button.CompareTag("UI_STACK_TO"))
            {
                button.onClick.AddListener(() =>
                {
                    stackToBelong = true;
                });
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) 
            && ClickManager.Instance.GetPointerTouchUI(gameObject))
        {
            transform.SetAsLastSibling();
        }
        if (stackToBelong)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(
                belongTo.transform.position 
                + new Vector3(0.0f, gameObject.transform.localScale.y * 1.5f, 0.0f));
            transform.position = screenPos;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("窗口被点击");
        
    }

}

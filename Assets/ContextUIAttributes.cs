using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ContextUIAttributes : MonoBehaviour, IPointerClickHandler
{
    public GameObject belongTo;
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
                    // ClickManager.Instance.selectedObject = null;
                });
                break;
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
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("窗口被点击");
        
    }

}

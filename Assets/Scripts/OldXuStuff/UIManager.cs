using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button editUIButton;
    public Color buttonActiveColor = Color.red;
    public Color buttonInactiveColor = new Color32(0x2D, 0x2D, 0x2D, 0xFF);
    public EditorManager editorManager;
    
    void Awake()
    {
        if (editUIButton != null && editorManager != null)
        {
            editUIButton.onClick.AddListener((() =>
            {
                editorManager.toggleEditMode();
                EditorManager editor = editorManager.GetComponent<EditorManager>();
                if (editor.inEditMode)
                {
                    editUIButton.GetComponent<Image>().color = buttonActiveColor;
                }
                else
                {
                    editUIButton.GetComponent<Image>().color = buttonInactiveColor;
                }
            }));
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using UnityEngine;

public class AdjustOrthographicCamera : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AdjustOrthographicCameraFunction();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void AdjustOrthographicCameraFunction()
    {
        Camera cam = Camera.main;
        float targetAspect = 1920f / 1080f;
        float windowAspect = (float) Screen.width / (float) Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1.0f)
        {
            cam.orthographicSize = 5.0f / scaleHeight; // 5 是你默认的视野值
        }
        else
        {
            cam.orthographicSize = 5.0f;
        }
    }

}

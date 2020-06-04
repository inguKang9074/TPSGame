using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FPSCheck : MonoBehaviour
{
    private static FPSCheck instance = null;

    public static FPSCheck Instance
    {
        get
        {
            return instance;
        }
    }

    [SerializeField] private Text fpsText;
    float deltaTime = 0.0f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(transform.root.gameObject);    
    }

    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1f / deltaTime;
        float msec = deltaTime * 1000.0f;
        fpsText.text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
    }
}

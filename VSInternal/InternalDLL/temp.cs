using UnityEngine;
using UnityEngine.UIElements;

public class RainbowLoadingBar : MonoBehaviour
{
    private GameObject attached;
    private Color color;
    void Start()
    {
        attached = transform.parent.gameObject;
        color = attached.GetComponent<Image>().tintColor;
    }

    void Update()
    {
        if (attached.activeSelf)
        {
            attached.GetComponent<Image>().tintColor = Color.Lerp(color, Color.HSVToRGB(Time.time / 10, 1, 1), Time.deltaTime);
        }
    }
}
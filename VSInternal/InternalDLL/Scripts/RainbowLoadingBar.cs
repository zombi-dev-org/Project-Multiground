using UnityEngine;
using UnityEngine.UI;

namespace ProjectMultiground.Scripts
{
    public class RainbowLoadingBar : MonoBehaviour
    {
        private GameObject attached;
        private Color color;
        void Start()
        {
            attached = transform.parent.gameObject;
            color = attached.GetComponent<Image>().color;
        }

        void Update()
        {
            if (attached.activeSelf)
            {
                attached.GetComponent<Image>().color = Color.Lerp(color, Color.HSVToRGB(Time.time / 10, 1, 1), Time.deltaTime);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class StatusPresenter : MonoBehaviour
{
    [SerializeField] Text statusText;

    public static Color COLOR_ERROR = new Color(0.6981132f, 0.0f, 0.006702666f, 1.0f);
    public static Color COLOR_SUCCESS = new Color(0.035f, 0.517647059f, 0.0f, 1.0f);

    public static Color COLOR_NOTICE = new Color(0.3215686f, 0.05098039f, 0.05098039f, 1.0f);

    // Start is called before the first frame update
    void Start()
    {
        Reset();
    }

    public void SetStatus(string text, Color color, bool fade = true)
    {
        GetComponent<CanvasRenderer>().SetAlpha(1.0f);
        statusText.color = color;
        statusText.text = text;

        if (fade) {
            statusText.CrossFadeAlpha(0.0f, 3.0f, true);
        }
    }

    public void Reset()
    {
        statusText.text = "";
    }
}

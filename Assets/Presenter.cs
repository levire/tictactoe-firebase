using UnityEngine;
using UnityEngine.UI;

public class Presenter : MonoBehaviour
{
    public bool Interactable
    {
        get {
            return GetComponent<CanvasGroup>();
        }
        set {
            GetComponent<CanvasGroup>().interactable = value;
        }
    }
}

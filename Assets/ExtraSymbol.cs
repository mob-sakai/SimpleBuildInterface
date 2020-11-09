using UnityEngine.UI;
using UnityEngine;

public class ExtraSymbol : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var text = GetComponent<Text>();
#if EXTRA_SYMBOL
        text.text = "EXTRA_SYMBOL is included.";
        text.color = Color.green;
#else
        text.text = "EXTRA_SYMBOL is not included.";
        text.color = Color.red;
#endif
    }
}

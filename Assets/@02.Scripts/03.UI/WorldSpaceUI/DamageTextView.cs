using TMPro;
using UnityEngine;

public sealed class DamageTextView : MonoBehaviour
{
    [SerializeField] TMP_Text mText;
    public TMP_Text Text => mText;

    public void SetText(string text, Color color)
    {
        mText.text = text;
        mText.color = color;
    }

    public void SetText(int amount, Color color)
    {
        mText.text = amount.ToString();
        mText.color = color;
    }
}

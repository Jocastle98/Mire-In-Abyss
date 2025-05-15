using TMPro;
using UnityEngine;

public class StatView : MonoBehaviour
{
    [SerializeField] private TMP_Text mStatName;
    [SerializeField] private TMP_Text mStatValue;

    public void Bind(string name, float value)
    {
        mStatName.text = name;
        mStatValue.text = value.ToString("F1");
    }
}


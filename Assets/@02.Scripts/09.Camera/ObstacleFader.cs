using UnityEngine;

[RequireComponent(typeof(Renderer))]
public sealed class ObstacleFader : MonoBehaviour
{
    const float FADE_SPEED = 5f;      // per second
    const float TARGET_ALPHA = 0.35f; // 투명도

    Material _mat;   float _current = 1f;   bool _shouldFade;

    void Awake()
    {
        _mat = GetComponent<Renderer>().material;   // 인스턴싱
        SetAlpha(1f);
    }

    public void FadeOut() => _shouldFade = true;
    public void FadeIn()  => _shouldFade = false;

    void Update()
    {
        float target = _shouldFade ? TARGET_ALPHA : 1f;
        if (Mathf.Approximately(_current, target)) return;

        _current = Mathf.MoveTowards(_current, target, FADE_SPEED * Time.unscaledDeltaTime);
        SetAlpha(_current);
    }
    void SetAlpha(float a)
    {
        Color c = _mat.color; c.a = a; _mat.color = c;
    }
}

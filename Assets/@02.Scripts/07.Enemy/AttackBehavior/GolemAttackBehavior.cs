using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/Attack Behaviors/Golem")]
public class GolemAttackBehavior : ScriptableObject, IAttackBehavior
{
    [Header("스윙 공격")]
    public float SwingRange = 3f;
    public int SwingDamage = 20;

    [Header("임팩트 공격")]
    public float ImpactRange = 6f;
    public int ImpactDamage = 35;
    public float ImpactChargeTime = 1.5f;
    public float ImpactCooldown = 10f;
    public LayerMask ImpactHitLayer;
    public GameObject IndicatorPrefab;

    private float mLastImpactTime = -Mathf.Infinity;

    // IAttackBehavior 인터페이스 구현 (범위 체크)
    public bool IsInRange(Transform self, Transform target)
    {
        if (target == null) return false;
        float dist = Vector3.Distance(self.position, target.position);
        return dist <= SwingRange || dist <= ImpactRange;
    }

    // 임팩트 공격 가능 여부
    public bool CanImpact(Transform self, Transform target)
    {
        if (target == null) return false;
        float dist = Vector3.Distance(self.position, target.position);
        return Time.time >= mLastImpactTime + ImpactCooldown && dist <= ImpactRange;
    }

    // 스윙 공격 가능 여부
    public bool CanSwing(Transform self, Transform target)
    {
        if (target == null) return false;
        float dist = Vector3.Distance(self.position, target.position);
        return dist <= SwingRange;
    }

    // 공격 호출
    public void Attack(Transform self, Transform target)
    {
        if (target == null) return;
        Animator anim = self.GetComponent<Animator>();

        if (CanImpact(self, target))
        {
            mLastImpactTime = Time.time;
            anim.SetTrigger("AttackImpact");
        }
        else if (CanSwing(self, target))
        {
            anim.SetTrigger("AttackSwing");
        }
    }

    // Animation Event: 임팩트 표시기 시작
    public void OnImpactIndicator(Transform self)
    {
        if (IndicatorPrefab == null) return;
        GameObject go = Instantiate(IndicatorPrefab, self.position, Quaternion.Euler(90f, 0, 0));
        go.transform.localScale = Vector3.zero;
        var renderer = go.GetComponent<Renderer>();
        Material mat = renderer.material;
        self.GetComponent<MonoBehaviour>().StartCoroutine(IndicatorRoutine(mat, go));
    }

    // 범위 표시 및 점점 진해지는 루틴
    private IEnumerator IndicatorRoutine(Material mat, GameObject go)
    {
        float t = 0f;
        while (t < ImpactChargeTime)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / ImpactChargeTime);

            // 스케일: 0에서 Diameter까지 증가
            float diameter = ImpactRange * 2f;
            go.transform.localScale = Vector3.one * (diameter * progress);

            // 투명도: 점점 진하게
            Color c = mat.color;
            c.a = progress;
            mat.color = c;
            yield return null;
        }
        Destroy(go);
    }

    // Animation Event: 임팩트 시 데미지 적용
    public void OnImpactLand(Transform self)
    {
        Collider[] hits = Physics.OverlapSphere(self.position, ImpactRange, ImpactHitLayer);
        foreach (Collider col in hits)
        {
            if (col.TryGetComponent<GolemBTController>(out var golem))
            {
                golem.SetHit(ImpactDamage);
            }
        }
    }

    // Animation Event: 스윙 데미지 적용
    public void OnSwingAttack(Transform self)
    {
        Collider[] hits = Physics.OverlapSphere(self.position, SwingRange, ImpactHitLayer);
        foreach (Collider col in hits)
        {
            if (col.TryGetComponent<GolemBTController>(out var golem))
            {
                golem.SetHit(SwingDamage);
            }
        }
    }
}

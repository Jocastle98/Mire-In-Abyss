// using System.Collections;
// using UnityEngine;
//
// [CreateAssetMenu(menuName="AI/Attack Behaviors/Ranged")]
// public class RangedAttackBehavior : ScriptableObject, IAttackBehavior
// {
//     public float range = 10f;
//     public GameObject projectilePrefab;
//     public Transform firePoint;
//     public LayerMask hitLayer;
//     public int damage = 5;
//     public float cooldown = 1f;
//
//     private bool _ready = true;
//
//     public bool IsInRange(Transform self, Transform target)
//         => Vector3.Distance(self.position, target.position) <= range;
//
//     public void Attack(Transform self, Transform target)
//     {
//         if (!_ready) return;
//         _ready = false;
//         Animator anim = self.GetComponent<Animator>();
//         anim.SetTrigger("Attack");
//
//         // 애니메이션 이벤트로 발사
//         // 또는 바로 Instantiate해도 무방
//         var proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
//         var hb   = proj.AddComponent<HitBox>();
//         hb.hitLayer = hitLayer;
//         hb.damage   = damage;
//         proj.GetComponent<Projectile>()?.Initialize(target.position);
//
//         self.GetComponent<MonoBehaviour>().StartCoroutine(ResetReady());
//     }
//
//     private IEnumerator ResetReady()
//     {
//         yield return new WaitForSeconds(cooldown);
//         _ready = true;
//     }
// }

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackScript : MonoBehaviour
{
    private bool attacking = false;
    private Animator animator;

    [Header("Attack Settings")]
    public Transform attackPoint;
    public LayerMask enemyLayers;
    public float attackRange = 0.5f;
    public int attackDamage = 30;
    public float attackRate = 3f;
    float nextAttackTime = 0f;

    

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {   
        if(Time.time >= nextAttackTime)
        {
            if(Input.GetButton("Fire1") && !attacking)
            {
            Attack();
            nextAttackTime = Time.time + 1f / attackRate;
            }
        }
        
    }
    void Attack(){
        attacking = true;
        animator.SetBool("attacking", true);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach(Collider2D enemy in hitEnemies){

            enemy.GetComponent<Enemy>().TakeDamage(attackDamage);
            Debug.Log("We hit " + enemy.name);
            Debug.Log("Enemy Health: " + enemy.GetComponent<Enemy>().currentHealth);
        }
    }

    public void ResetAttack(){
        animator.SetBool("attacking", false);
        attacking = false;
    }

    private void OnDrawGizmosSelected() {
        if(attackPoint == null){
            return;
        }
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}

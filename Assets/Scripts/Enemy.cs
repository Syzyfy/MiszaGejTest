using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxhealth = 100;
    public int currentHealth;

    EnemyAI enemyAI;
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxhealth;
        enemyAI = GetComponent<EnemyAI>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(int damage){
        currentHealth -= damage;
        if(currentHealth <= 0){
            Die();
        }
    }
    void Die(){
        Debug.Log("Enemy died!");
        enemyAI.enabled = false;

        StartCoroutine(DestroyAfterDelay(8f));
    }
    IEnumerator DestroyAfterDelay(float duration)
    {
            // Pobierz materiał obiektu
        Material material = GetComponent<Renderer>().material;
        Color originalColor = material.color;
        float counter = 0;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, counter / duration);

            material.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // Zniszcz obiekt
        Destroy(gameObject);
    }
}

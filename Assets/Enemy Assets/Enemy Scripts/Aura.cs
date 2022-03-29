using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aura : MonoBehaviour
{
    [HideInInspector] public bool breakAura = false;
    bool breakAuraActive = false;

    public bool shield = false;
    public bool aggro = false;

    EnemyManager enemyManager;

    // Start is called before the first frame update
    void Start()
    {
        enemyManager = transform.root.GetComponent<EnemyManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (breakAuraActive == false && (breakAura == true || enemyManager.enemyWasKilled == true))
        {
            StartCoroutine(BreakShield());
        }
    }

    IEnumerator BreakShield()
    {
        breakAuraActive = true;
        while (enemyManager.hitstopImpactActive == true)
        {
            yield return null;
        }
        Color auraColor = transform.GetComponent<Renderer>().material.color;
        auraColor.a = 1;
        GetComponent<Renderer>().material.color = auraColor;
        while (transform.localScale.magnitude > 0.05 && transform.localScale.x > 0 && auraColor.a > 0.05)
        {
            transform.localScale = new Vector2(transform.localScale.x - 2 * Time.deltaTime, transform.localScale.y - 2 * Time.deltaTime);

            auraColor.a -= Time.deltaTime * 1.5f;
            GetComponent<Renderer>().material.color = auraColor;
            yield return null;
        }
        Destroy(gameObject);
    }
}

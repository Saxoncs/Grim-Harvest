using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [SerializeField] public string type;
    [SerializeField] public float damage;
    [SerializeField] public float duration;


    void Start()
    {
        StartCoroutine(ActiveTimer());
    }

    private IEnumerator ActiveTimer()
    {
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

}
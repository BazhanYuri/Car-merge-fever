using System.Collections;
using UnityEngine;

public class DestroyAfterSpawn : MonoBehaviour
{
    private void OnEnable()
    {
        
    }
    void Start()
    {
        StartCoroutine(Del());
    }


    private IEnumerator Del()
    {
        yield return new WaitForSeconds(5);
        Destroy(gameObject);
    }
   
}

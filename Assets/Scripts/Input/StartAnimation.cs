using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartAnimation : MonoBehaviour
{
    [SerializeField] Animator an;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            an.SetTrigger("start");
        }
    }
}

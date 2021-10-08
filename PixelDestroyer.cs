using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelDestroyer : MonoBehaviour
{
    // Start is called before the first frame update
    void OnEnable()
    {
        Invoke("Destroy", 10.0f);
    }

    void Destroy() 
    {
        gameObject.SetActive(false);
    }

    void OnDisable() 
    {
        CancelInvoke();
    }
}

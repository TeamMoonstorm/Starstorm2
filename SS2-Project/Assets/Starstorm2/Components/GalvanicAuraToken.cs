using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalvanicAuraToken : MonoBehaviour //annoyed im doing it this way. i wish TempVFXAPI let me see if a body had a specific effect on it 
{
    public GameObject aura;
    public float timer = 2;

    void FixedUpdate()
    {
        if(timer >= 0)
        {
            timer -= Time.fixedDeltaTime;
        }
        else if(aura)
        {
            Destroy(aura);
            aura = null;
        }

    }
}

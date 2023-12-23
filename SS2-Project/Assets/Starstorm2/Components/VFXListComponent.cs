using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXListComponent : MonoBehaviour
{
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        
    }

    public void TurnOffVFX()
    {
        foreach(var obj in VFXParticles)
        {
            obj.SetActive(false);
        }
    }

    public void TurnOnVFX()
    {
        foreach (var obj in VFXParticles)
        {
            obj.SetActive(true);
        }
    }

    public List<GameObject> VFXParticles = new List<GameObject>();

}

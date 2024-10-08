using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Collections;

public class Bush : MonoBehaviourPunCallbacks
{
    private Material _bushMaterial;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null){
            Debug.Log("renderer: " + renderer);
            _bushMaterial = renderer.material;
        }
        else{
            Debug.LogWarning("Bush object does not have a Renderer component.");

        }
    }


    public void SetBushTransparency(float alpha)
    {
        
        if (_bushMaterial != null)
        {
            Debug.Log("SetBushTransparency: " + _bushMaterial);
            Color color = _bushMaterial.color;
            color.a = Mathf.Clamp(alpha, 0.1f, 1.0f);
            _bushMaterial.color = color;
        }
        else{
            Debug.LogWarning("Bush object does not have a Renderer component.");
        }
    }

    public void ResetBushTransparency()
    {
        SetBushTransparency(1.0f);
    }
}

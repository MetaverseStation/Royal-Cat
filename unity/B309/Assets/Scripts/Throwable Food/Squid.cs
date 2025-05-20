using UnityEngine;
using Photon.Pun;
using System.Collections;

public class Squid : Food
{

    private void Start()
    {
        hitEffect = new SquidEffect();
    }
}

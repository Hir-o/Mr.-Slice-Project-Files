using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IndieMarc.StealthLOS
{
    public class Invisible : MonoBehaviour
    {
        void Start()
        {
            GetComponentInChildren<MeshRenderer>().enabled = false;
        }

    }
}

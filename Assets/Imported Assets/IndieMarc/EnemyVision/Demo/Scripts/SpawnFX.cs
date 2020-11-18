using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IndieMarc.StealthLOS
{

    public class SpawnFX : MonoBehaviour
    {

        public float lifetime = 5f;

        void Start()
        {
            Destroy(gameObject, lifetime);
        }
    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GfxParent : MonoBehaviour
{
    public Transform GfxHolder, Gfx;

    private void Start() { Gfx.parent = GfxHolder; }
}
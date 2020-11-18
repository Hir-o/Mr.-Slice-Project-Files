using UnityEditor;
using UnityEngine;

public class HardLight2DEditor : MonoBehaviour
{

    [MenuItem ("HardLight2D/Refresh collider references")]
    static void RefreshAll ()
    {
        HardLight2DManager.RefreshAllCollidersReferences ();
    }
}
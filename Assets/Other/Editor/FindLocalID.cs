#if UNITY_EDITOR

using UnityEngine;

public class FindLocalID : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log(FileIDUtil.Compute(typeof(SobelRain)));
    }
}

#endif
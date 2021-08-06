using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DangerObjectType : byte { Saw}

public sealed class DangerObject : MonoBehaviour
{
    [SerializeField] private DangerObjectType type;
    private const float SAW_PUSH_FORCE = 100f;

    public void SpecialDeathEffects(SmallGuy sg)
    {
        switch (type)
        {
            case DangerObjectType.Saw: sg.GetComponent<Rigidbody>().AddForce((sg.transform.forward * (-1) + Vector3.up * Random.value) * SAW_PUSH_FORCE); break; ;
        }
    }
}

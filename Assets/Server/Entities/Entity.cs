using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public static UInt32 MaxID { get; private set; } = 1;
    public UInt32 ID { get; private set; }

    public virtual void Awake()
    {
        this.ID = Entity.MaxID;
        Entity.MaxID += 1;
    }

    public virtual void DirectMove(float x, float y, float dx, float dy)
    {
        throw new Exception("Cannot move Entity with id " + ID);
    }
}

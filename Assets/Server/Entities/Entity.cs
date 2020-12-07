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

    public UInt16 LastUpdate { get; set; } = 0;
    public float X { get; protected set; } = 0f;
    public float Y { get; protected set; } = 0f;
    public float DX { get; protected set; } = 0f;
    public float DY { get; protected set; } = 0f;
    public float Rotation { get; protected set; } = 0f;

    public virtual void Awake()
    {
        this.ID = Entity.MaxID;
        Entity.MaxID += 1;

        this.X = transform.position.x;
        this.Y = transform.position.y;
    }

    public virtual void DirectMove(float x, float y, float dx, float dy)
    {
        throw new Exception("Cannot move Entity with id " + ID);
    }
}

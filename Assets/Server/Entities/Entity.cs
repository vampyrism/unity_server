using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public static UInt32 MaxID { get; private set; } = 0;
    public UInt32 ID { get; private set; }

    public virtual void Start()
    {
        this.ID = Entity.MaxID;
        Entity.MaxID += 1;
    }
}

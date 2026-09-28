using System;
using UnityEngine;

public interface IPoolable
{
    event Action<GameObject> onBeforeReturn;

    void OnSpawn();
    
    void onDespawn();
}

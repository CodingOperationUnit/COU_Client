using System.Collections.Generic;
using UnityEngine;

public class SkillObjectPool : MonoSingleton<SkillObjectPool>
{
    private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();
    private readonly Dictionary<GameObject, GameObject> _instanceToPrefab = new();

    public GameObject Get( GameObject prefab, Vector3 position, Quaternion rotation )
    {
        if( prefab == null )
        {
            return null;
        }

        if( !_pools.TryGetValue( prefab, out Queue<GameObject> pool ) )
        {
            pool = new Queue<GameObject>();
            _pools[prefab] = pool;
        }

        GameObject instance = null;

        while( pool.Count > 0 )
        {
            instance = pool.Dequeue();

            if( instance != null && !instance.activeInHierarchy )
            {
                break;
            }

            instance = null;
        }

        if( instance == null )
        {
            instance = Instantiate( prefab );
        }

        _instanceToPrefab.TryAdd( instance, prefab );

        instance.transform.SetPositionAndRotation( position, rotation );
        instance.SetActive( true );
        instance.GetComponent<IPoolable>()?.OnSpawn();

        return instance;
    }

    public void Return( GameObject instance )
    {
        if( instance == null )
        {
            return;
        }

        if( !_instanceToPrefab.TryGetValue( instance, out GameObject prefab ) )
        {
            Destroy( instance );
            return;
        }

        instance.SetActive( false );
        _pools[prefab].Enqueue( instance );
    }
}

using UnityEngine;

public static class GameManager
{
    public static GameSceneManager Scene
    {
        get { return GameSceneManager.Instance; }
    }
}

public class SingletonManager<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField] private bool isDontDestroy = false;
    
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<T>() ?? new GameObject(typeof(T).Name).AddComponent<T>();

                DontDestroyOnLoad(instance.transform.root.gameObject);
            }

            return instance;
        }
    }
    
    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            
            if (isDontDestroy) DontDestroyOnLoad(transform.root.gameObject);
        }

        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    protected virtual void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}

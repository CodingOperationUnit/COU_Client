using UnityEngine;

public static class GameManager
{
    public static GameSceneManager Scene { get { return GameSceneManager.Instance; } }
    public static ObjectPoolManager ObjectPool { get { return ObjectPoolManager.Instance; } }
    public static LocalSaveLoadManager LocalSaveLoad { get { return LocalSaveLoadManager.Instance; } }
    public static LocalLoginManager LocalLogin { get { return LocalLoginManager.Instance; } }
    public static PlayerDataManager PlayerData { get { return PlayerDataManager.Instance; } }
    public static JsonDataManager JsonData { get { return JsonDataManager.Instance; } }
    public static DropItemManager DropItem { get { return DropItemManager.Instance; } }
    public static UIManager UI { get { return UIManager.Instance; } }
    public static ServerLoginManager ServerLogin { get { return ServerLoginManager.Instance; } }
    public static ServerLoadManager ServerLoad { get { return ServerLoadManager.Instance; } }
    public static ServerBattleManager ServerBattle { get { return ServerBattleManager.Instance; } }
}

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
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
            Transform root = transform.root;
            
            bool cleanUpRoot = isDontDestroy
                && root != transform
                && root != instance.transform.root;

            if (cleanUpRoot) transform.SetParent(null);

            Destroy(gameObject);

            if (cleanUpRoot && root.childCount == 0)
                Destroy(root.gameObject);
        }
    }
    
    protected virtual void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}

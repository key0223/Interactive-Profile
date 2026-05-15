using System;
using UnityEngine;

[Serializable]
public class ManagerInfo
{
    public MonoBehaviour Manager;
    public string ManagerName;
}
public class GameManager : SingletonMonoBehaviour<GameManager>
{
    public static event Action OnAllManagersReady;

    [SerializeField] ManagerInfo[] _managersToWait;
    int _managersReadyCount = 0;
    bool _allManagersReady = false;
  
    public bool AllManagersReady { get { return _allManagersReady; } }
    protected override void Awake()
    {
        base.Awake();
        ValidateManagers();
    }
    public void ManagerReady(string managerName)
    {
        _managersReadyCount++;

        if (_managersReadyCount >= _managersToWait.Length)
        {
            OnAllManagersReady?.Invoke();
            _allManagersReady = true;
        }
    }
    void ValidateManagers()
    {
        foreach (ManagerInfo manager in _managersToWait)
        {
            if (manager.Manager == null)
                Debug.LogError($"GameManager: {manager.ManagerName} 누락");
        }
    }
}

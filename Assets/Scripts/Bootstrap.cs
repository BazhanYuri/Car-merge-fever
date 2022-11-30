using PathCreation;
using UnityEngine;


public class Bootstrap : MonoBehaviour
{
    [SerializeField] private PathCreator _pathCreator;
    [SerializeField] private InputManager _inputManager;
    [SerializeField] private CarMovementManager _carMovement;
    [SerializeField] private MergeGrid _mergeGrid;
    [SerializeField] private CarSpawner _carSpawner;
    [SerializeField] private CarManager _carManager;
    [SerializeField] private MergeManager _mergeManager;
    [SerializeField] private TrackCarManager _trackCarManager;
    [SerializeField] private LapManager _lapManager;
    [SerializeField] private MoneyManager _moneyManager;
    [SerializeField] private PresentManager _presentManager;


    private void Awake()
    {
        _carMovement.Init(_inputManager, _mergeGrid, _mergeManager, _carManager);
        _carSpawner.Init(_mergeGrid, _carManager, _moneyManager, _lapManager);
        _carManager.Init(_carSpawner);
        _mergeManager.Init(_carManager);
        _trackCarManager.Init(_pathCreator, _inputManager, _carManager, _lapManager);
        _lapManager.Init(_moneyManager);
        _presentManager.Init(_carSpawner, _carManager, _moneyManager);
    }
}

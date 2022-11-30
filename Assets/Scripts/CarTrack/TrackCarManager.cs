using PathCreation;
using UnityEngine;

public class TrackCarManager : MonoBehaviour
{
    [SerializeField] private TrackCar _trackCar;

    private LapManager _lapManager;
    private PathCreator _pathCreator;
    private InputManager _inputManager;
    private CarManager _carManager;

    public void Init(PathCreator pathCreator, InputManager inputManager, CarManager carManager, LapManager lapManager)
    {
        _pathCreator = pathCreator;
        _inputManager = inputManager;
        _carManager = carManager;
        _lapManager = lapManager;

        _inputManager.CarSelected += SpawnCar;
    }
    private void OnDisable()
    {
        _inputManager.CarSelected -= SpawnCar;
    }


    public void SpawnCar(Car car)
    {
        TrackCar trackCar = Instantiate(_trackCar);
        trackCar.Init(_lapManager, _pathCreator, car, _carManager);

        car.BlockCar();
    }
}

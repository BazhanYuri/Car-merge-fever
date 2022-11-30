using PathCreation;
using UnityEngine;

public class CarOnTrackMovement : MonoBehaviour
{
    [SerializeField] private GameConfig _gameConfig;

    private CarManager _carManager;
    private TrackCar _car;
    private Car _spawnedFrom;
    private PathCreator _pathCreator;
    private int _levelIndex;
    private float _distanceTravelled = 0;

    private float  _speedimproveIndex;
    private float _lapImproveIndex;

    private int _currentLap = 0;
    private int _maxLapsCount;
    private float _speed;

    private float _passedDistance = 0;
    private float _maxDistance;

    public void Init(TrackCar trackCar, PathCreator pathCreator, Car spawnedFrom, CarManager carManager)
    {
        _carManager = carManager;
        _spawnedFrom = spawnedFrom;
        _car = trackCar;
        _pathCreator = pathCreator;
        _levelIndex = spawnedFrom.CarMerge.LevelIndex;
        _spawnedFrom.LevelIndexText.gameObject.SetActive(false);
        _spawnedFrom.ProgressBar.enabled = true;

         _maxLapsCount = _levelIndex;

        _speedimproveIndex = 1;

        for (int i = 0; i < _levelIndex; i++)
        {
            _speedimproveIndex *= _gameConfig.SpeedImproveIndex;
        }

        _lapImproveIndex = 1;

        for (int i = 0; i < _levelIndex; i++)
        {
            _lapImproveIndex *= _gameConfig.LapImproveIndex;
        }
        CalculateSpeed();
        CalculateLap();
    }
    private void CalculateSpeed()
    {
        _speed = _gameConfig.StandartSpeed;
        _speed *= _speedimproveIndex;

    }
    private void CalculateLap()
    {
        _maxLapsCount = 1;
        _maxLapsCount *= (int)_lapImproveIndex;

        _maxDistance = _pathCreator.path.length * _maxLapsCount;
    }
    private void Update()
    {
        Move();
        UpdateProgressBar();
    }
    private void Move()
    {
        _distanceTravelled += _speed  * Time.deltaTime;
        _passedDistance += _speed * Time.deltaTime;
        _car.transform.position = _pathCreator.path.GetPointAtDistance(_distanceTravelled, EndOfPathInstruction.Loop);

        Quaternion pathRot = _pathCreator.path.GetRotationAtDistance(_distanceTravelled, EndOfPathInstruction.Loop);
        Quaternion rotation = new Quaternion(
             _car.transform.rotation.eulerAngles.x,
            pathRot.y,
             _car.transform.rotation.eulerAngles.z,
            pathRot.w);
        _car.transform.rotation = rotation;
        if (_distanceTravelled > _pathCreator.path.length)
        {
            ResetDistanceTravelled();
        }
        PlaySound();
    }

    private bool _isCanPlay = true;
    private void PlaySound()
    {
        if (_isCanPlay == false)
        {
            return;
        }
        if (_distanceTravelled > (_pathCreator.path.length * 0.85f))
        {
            SoundManager.Instance.PlayLapSound((_speedimproveIndex / 5));
            _isCanPlay = false;
        }
    }
    private void ResetDistanceTravelled()
    {
        _distanceTravelled = 0;
        _car.LapManager.AddLap();
        _isCanPlay = true;

        _currentLap++;
        if (_currentLap >= _maxLapsCount)
        {
            DeleteCar();
        }
    }
    private void DeleteCar()
    {
        _carManager.RemoveCar(_spawnedFrom);
        Destroy(_car.gameObject);
    }
    private void UpdateProgressBar()
    {
        float percent =  _passedDistance / (_maxDistance / 100f);
        _spawnedFrom.ProgressBar.fillAmount = 1 - (percent / 100f);
    }
}

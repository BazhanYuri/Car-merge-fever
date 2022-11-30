using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CarSpawner : MonoBehaviour
{
    [SerializeField] private Button _spawnButton;
    [SerializeField] private Car _carPrefab;


    public event Action<Car> CarSpawned;

    private Car _spawnedCar;
    private MergeGrid _mergeGrid;
    private CarManager _carManager;
    private MoneyManager _moneyManager;
    private LapManager _lapManager;


    private void OnEnable()
    {
        _spawnButton.onClick.AddListener(SpawnCar);
    }
    private void OnDisable()
    {
        _spawnButton.onClick.RemoveListener(SpawnCar);
    }



    public void Init(MergeGrid mergeGrid, CarManager carManager, MoneyManager moneyManager, LapManager lapManager)
    {
        _mergeGrid = mergeGrid;
        _carManager = carManager;
        _moneyManager = moneyManager;
        _lapManager = lapManager;

    }

    private void Start()
    {
        TakeCarsFromMemory();
    }
    public void SpawnPresent()
    {
        ChooseFreeCell(true);
        _spawnedCar.Present.EnablePresent();
    }
    private void SpawnCar()
    {
        if (_moneyManager.MoneyCount >= 10)
        {
            ChooseFreeCell(false);
            _moneyManager.TakeMoney(10);
        }
    }
    private void ChooseFreeCell(bool isRandomLevel)
    {
        if (_carManager.Cars.Count == 16)
        {
            return;
        }
        if (_carManager.Cars.Count == 0)
        {
            SpawnCar(true, _mergeGrid.GetCurrentCellByIndexes(0, 0).position, 0, 0, 0);
            return;
        }

        int z = 0, x = -1;

        while (true)
        {
            x++;
            if (x == 4)
            {
                x = 0;
                z++;
            }

            bool isFree = true;

            for (int i = 0; i < _carManager.Cars.Count; i++)
            {
                if (_carManager.Cars[i].CarMovement.ZPos == x && _carManager.Cars[i].CarMovement.XPos == z)
                {
                    isFree = false;
                }
            }
            if (isFree == false)
            {
                continue;
            }
            else if (isFree == true)
            {
                if (isRandomLevel == false)
                {
                    SpawnCar(true, _mergeGrid.GetCurrentCellByIndexes(z, x).position, x, z, 0);
                }
                else
                {
                    SpawnCar(true, _mergeGrid.GetCurrentCellByIndexes(z, x).position, x, z, ChooseRandomCarLevel());
                }
                return;
            }

            
        }
    }
    public int ChooseRandomCarLevel()
    {
        int maxLevel = 1;
        int laps = _lapManager.LapCount;

        if (laps > 0)
        {
            maxLevel = 1;
        }
        if (laps > 50)
        {
            maxLevel = 2;
        }
        if (laps > 100)
        {
            maxLevel = 3;
        }
        if (laps > 250)
        {
            maxLevel = 4;
        }
        if (laps > 500)
        {
            maxLevel = 5;
        }
        if (laps > 750)
        {
            maxLevel = 6;
        }
        if (laps > 1000)
        {
            maxLevel = 7;
        }


        return UnityEngine.Random.RandomRange(0, maxLevel);
    }
    private Car SpawnCar(bool jumpVisualEffect, Vector3 pos, int z, int x, int level)
    {
        PlayerPrefs.SetInt("Carscount", _carManager.Cars.Count + 1);
        Car car = Instantiate(_carPrefab);
        _spawnedCar = car;
        car.CarMovement.SetPosition(z, x);
        car.CarMerge.SetLevel(level);

        if (jumpVisualEffect)
        {
            car.transform.position = pos + new Vector3(0, 3, 0);
            car.transform.DOMove(pos + new Vector3(0, 0.2f, 0), 0.3f).SetEase(Ease.Linear);
        }
        else
        {
            car.transform.position = pos + new Vector3(0, 0.2f, 0);
        }

        CarSpawned?.Invoke(car);

        return car;
    }
    


    private void TakeCarsFromMemory()
    {
        string[] cars = _carManager.GetInfosRememberedCars();

        for (int i = 0; i < cars.Length; i++)
        {
            int level = Convert.ToInt32(cars[i][3].ToString());
            int x = Convert.ToInt32(cars[i][7].ToString());
            int z = Convert.ToInt32(cars[i][9].ToString());

            SpawnCar(false, _mergeGrid.GetCurrentCellByIndexes(z, x).position, x, z, level);
        }
    }
}

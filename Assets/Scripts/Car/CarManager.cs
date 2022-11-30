using System.Collections.Generic;
using UnityEngine;

public class CarManager : MonoBehaviour
{
    private List<Car> _cars = new List<Car>();

    public List<Car> Cars { get => _cars;}


    private CarSpawner _carSpawner;


   
    private void OnDisable()
    {
        _carSpawner.CarSpawned -= AddCar;
    }

    public void Init(CarSpawner carSpawner)
    {
        _carSpawner = carSpawner;

        _carSpawner.CarSpawned += AddCar;

    }
    public void RemoveCar(Car car)
    {
        Cars.Remove(car);
        Destroy(car.gameObject);

        UpdateMemoryCarsInfo();
    }
    private void AddCar(Car car)
    {
        car.Init(this);
        Cars.Add(car);

        UpdateMemoryCarsInfo();
    }
    private void UpdateMemoryCarsInfo()
    {
        for (int i = 0; i < PlayerPrefs.GetInt("Carscount"); i++)
        {
            PlayerPrefs.DeleteKey("Car" + i);
        }

        PlayerPrefs.SetInt("Carscount", Cars.Count);
        for (int i = 0; i < Cars.Count; i++)
        {
            string levelIndex = "lvl" + Cars[i].CarMerge.LevelIndex;
            string pos = "pos" + Cars[i].CarMovement.ZPos + "," + Cars[i].CarMovement.XPos;

            string infoToRemember = levelIndex + pos;
            PlayerPrefs.SetString("Car" + i, infoToRemember);
        }
    }
    public string[] GetInfosRememberedCars()
    {
        string[] cars = new string[PlayerPrefs.GetInt("Carscount")];

        for (int i = 0; i < cars.Length; i++)
        {
            cars[i] = PlayerPrefs.GetString("Car" + i);
        }

        return cars;
    }
}

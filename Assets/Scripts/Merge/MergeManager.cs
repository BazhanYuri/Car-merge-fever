using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MergeManager : MonoBehaviour
{
    private CarManager _carManager;



    public void Init(CarManager carManager)
    {
        _carManager = carManager;
    }

    public void TryMerge(Car car)
    {
        if (car.IsCarBlocked == true)
        {
            return;
        }
        for (int i = 0; i < _carManager.Cars.Count; i++)
        {
            if (_carManager.Cars[i].IsCarBlocked == true)
            {
                continue;
            }
            if (_carManager.Cars[i].GetHashCode() == car.GetHashCode())
            {
                continue;
            }
            if (_carManager.Cars[i].CarMerge.LevelIndex != car.CarMerge.LevelIndex)
            {
                continue;
            }
            if (_carManager.Cars[i].CarMovement.IsInOneCell(car.CarMovement.XPos, car.CarMovement.ZPos))
            {
                car.CarMerge.ImproveCar();
                _carManager.RemoveCar(_carManager.Cars[i]);
            }
        }
    }
}

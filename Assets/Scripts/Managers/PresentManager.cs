using UnityEngine;
using System.Collections;


public class PresentManager : MonoBehaviour
{
    private CarSpawner _carSpawner;
    private CarManager _carManager;
    private MoneyManager _moneyManager;

    public void Init(CarSpawner carSpawner, CarManager carManager, MoneyManager moneyManager)
    {
        _carSpawner = carSpawner;
        _carManager = carManager;
        _moneyManager = moneyManager;
    }


    private void Start()
    {
        StartCoroutine(SpawnPresent());
    }
    private IEnumerator SpawnPresent()
    {
        while (true)
        {
            if (_carManager.Cars.Count == 0)
            {
                if (_moneyManager.MoneyCount < 10)
                {
                    yield return new WaitForSeconds(1);
                    _carSpawner.SpawnPresent(); 
                }
            }
            else if (_carManager.Cars.Count < 16)
            {
                yield return new WaitForSeconds(7);
                _carSpawner.SpawnPresent();
            }

            yield return null;
        }
    }
        
}

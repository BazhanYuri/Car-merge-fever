using UnityEngine;
using DG.Tweening;

public class CarMovementManager : MonoBehaviour
{
    private InputManager _inputManager;
    private MergeGrid _mergeGrid;
    private MergeManager _mergeManager;
    private CarManager _carManager;

    private Car _car;


    public void Init(InputManager inputManager, MergeGrid mergeGrid, MergeManager mergeManager, CarManager carManager)
    {
        _inputManager = inputManager;
        _mergeGrid = mergeGrid;
        _mergeManager = mergeManager;
        _inputManager.EndSlide += MoveCar;
        _carManager = carManager;
    }

    private void OnDisable()
    {
        _inputManager.EndSlide -= MoveCar;
    }


    private void MoveCar(SlideSide side, Car car)
    {
        Vector2Int moveVector = Vector2Int.zero;

        switch (side)
        {
            case SlideSide.Up:
                moveVector.y = 1;
                break;
            case SlideSide.Down:
                moveVector.y = -1;
                break;
            case SlideSide.Left:
                moveVector.x = -1;
                break;
            case SlideSide.Right:
                moveVector.x = 1;
                break;
            default:
                break;
        }


        _car = car;

        if (CheckIsThereNoCar(new Vector2Int(car.CarMovement.XPos + moveVector.y, car.CarMovement.ZPos + moveVector.x)) == false)
        {
            return;
        }

        car.CarMovement.ChangePosition(moveVector.x, moveVector.y);
        MoveTo(car, _mergeGrid.GetCurrentCellByIndexes(car.CarMovement.XPos, car.CarMovement.ZPos).position);
    }
    public void MoveTo(Car car, Vector3 pos)
    {
        Vector3 posToMove = new Vector3(pos.x, car.transform.position.y, pos.z);
        car.transform.DOMove(posToMove, 0.07f).SetEase(Ease.Linear).OnComplete(() => CheckMovement());
    }
    
    private bool CheckIsThereNoCar(Vector2Int pos)
    {
        for (int i = 0; i < _carManager.Cars.Count; i++)
        {
            if (_carManager.Cars[i].GetHashCode() == _car.GetHashCode())
            {
                continue;
            }
            if (_carManager.Cars[i].CarMovement.XPos == pos.x && _carManager.Cars[i].CarMovement.ZPos == pos.y)
            {
                if (_carManager.Cars[i].IsCarBlocked == true || _car.IsCarBlocked == true)
                {
                    return false;
                }
                if (_carManager.Cars[i].CarMerge.LevelIndex == _car.CarMerge.LevelIndex)
                {
                    if (_car.CarMerge.LevelIndex == 7)
                    {
                        return false;
                    }
                    return true;
                }
                return false;
            }
        }
        return true;
    }
    private void CheckMovement()
    {
        _mergeManager.TryMerge(_car);
    }
}

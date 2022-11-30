using System;
using UnityEngine;
using UnityEngine.EventSystems;



public enum SlideSide
{
    Up,
    Down,
    Left,
    Right
}
public class InputManager : MonoBehaviour
{
    public event Action StartSlide;
    public event Action<SlideSide, Car> EndSlide;
    public event Action<Car> CarSelected;


    private Vector2 _tapPosition;
    private Car _slidedCar;

    private bool _isCarDetected;
    private void Update()
    {
        CheckSlide();
    }

    private void CheckSlide()
    {
        if (Input.touchCount > 0)
        {

            Touch touch = Input.GetTouch(0);


            switch (touch.phase)
            {
                case TouchPhase.Began:
                    DetectLocation(touch);
                    _isCarDetected = Detect(touch);
                    break;
                case TouchPhase.Moved:
                    break;
                case TouchPhase.Stationary:
                    break;
                case TouchPhase.Ended:
                    if (_isCarDetected == false)
                    {
                        return;
                    }
                    if ((_tapPosition - touch.position).magnitude < 20)
                    {
                        CheckTapOnCar(touch);
                        return;
                    }
                    _touchCarCount = 0;
                    GetSide(_tapPosition - touch.position);
                    break;
                case TouchPhase.Canceled:
                    break;
                default:
                    break;
            }
        }

    }


    private Location _previous;
    private bool DetectLocation(Touch touch)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.TryGetComponent(out Location location))
            {
                if (_previous != null)
                {
                    if (_previous.GetHashCode() != location.GetHashCode())
                    {
                        _previous.ResetTouch();
                        location.TryBuyLocation();
                        _previous = location;
                        return false;
                    }
                }
                location.TryBuyLocation();
                _previous = location;

                return true;
            }
            else if (hit.collider.TryGetComponent(out Present present))
            {
                present.DisablePresent();
            }
        }
        _previous?.ResetTouch();

        return false;
    }
    private bool Detect(Touch touch)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.TryGetComponent(out ColliderTypeDetect collideType))
            {
                if (collideType.Type == GameObjectType.Car)
                {
                    _slidedCar = collideType.Root.GetComponent<Car>();
                    _tapPosition = touch.position;
                    
                    return true;
                }
            }
        }
        return false;
    }
    private void GetSide(Vector2 delta)
    {
        SlideSide side;

        if (Math.Abs(delta.x) > Math.Abs(delta.y))
        {
            if (delta.x > 0)
            {
                side = SlideSide.Left;
            }
            else
            {
                side = SlideSide.Right;
            }
        }
        else
        {
            if (delta.y > 0)
            {
                side = SlideSide.Down;
            }
            else
            {
                side = SlideSide.Up;
            }
        }

        EndSlide?.Invoke(side, _slidedCar);
    }



    private int _touchCarCount;
    private Car _touchedCar;
    private void CheckTapOnCar(Touch touch)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.TryGetComponent(out ColliderTypeDetect collideType))
            {
                if (collideType.Type == GameObjectType.Car)
                {
                    Car car = collideType.Root.GetComponent<Car>();
                    if (car.IsCarBlocked == true)
                    {
                        return;
                    }
                    if (car.IsCarOnTrack == true)
                    {
                        return;
                    }
                    
                    if (_touchedCar?.GetHashCode() == car.GetHashCode())
                    {
                        _touchCarCount++;

                        if (_touchCarCount > 0)
                        {
                            CarSelected?.Invoke(_touchedCar);
                            _touchedCar.SetCarAsOnTrack();
                            _touchCarCount = 0;
                        }
                    }
                    if (_touchedCar?.GetHashCode() != car.GetHashCode())
                    {
                        _touchCarCount = 0;
                    }
                    _touchedCar = car;
                }
            }
        }

        
    }
}

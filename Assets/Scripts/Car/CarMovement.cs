using System;
using UnityEngine;
using DG.Tweening;

public class CarMovement : MonoBehaviour
{
    private Car _car;

    private int _zPos = 0;
    private int _xPos = 0;


    public int ZPos { get => _zPos;}
    public int XPos { get => _xPos;}

    public void Init(Car car)
    {
        _car = car;
    }

    public bool IsInOneCell(int x, int z)
    {
        return _xPos == x && _zPos == z;
    }
    public void SetPosition(int zpos, int xpos)
    {
        _zPos = zpos;
        _xPos = xpos;
    }
    public void ChangePosition(int zpos, int xpos)
    {
        _zPos += zpos;
        _xPos += xpos;
    }
    
}

using UnityEngine;
using PathCreation;

public class TrackCar : MonoBehaviour
{
    [SerializeField] private CarOnTrackMovement _movement;
    [SerializeField] private SkinChooser _skinChooser;

    private LapManager _lapManager;

    public LapManager LapManager { get => _lapManager;}

    public void Init(LapManager lapManager, PathCreator pathCreator, Car spawnedFrom, CarManager carManager)
    {
        _lapManager = lapManager;
        _movement.Init(this, pathCreator, spawnedFrom, carManager);
        _skinChooser.ShowSkin(spawnedFrom.CarMerge.LevelIndex);
    }
}

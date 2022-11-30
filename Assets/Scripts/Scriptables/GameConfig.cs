using UnityEngine;




[CreateAssetMenu(fileName = "LevelConfig", menuName = "ScriptableObjects/LevelConfig", order = 1)]
public class GameConfig : ScriptableObject
{
    [SerializeField] private int _moneyPerOneLap;
    [SerializeField] private int _standartSpeed;
    [SerializeField] private float _speedimproveIndex;
    [SerializeField] private float _lapImproveIndex;

    public int MoneyPerOneLap { get => _moneyPerOneLap;}
    public int StandartSpeed { get => _standartSpeed;}
    public float SpeedImproveIndex { get => _speedimproveIndex;}
    public float LapImproveIndex { get => _lapImproveIndex;}
}

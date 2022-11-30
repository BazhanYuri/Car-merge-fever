using System;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class LapManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _lapTextCount;
    public event Action LapPassed;

    private MoneyManager _moneyManager;
    private int _lapCount = 0;

    public int LapCount { get => _lapCount;}

    public void Init(MoneyManager moneyManager)
    {
        _moneyManager = moneyManager;

        _lapCount = PlayerPrefs.GetInt("LapsCount", 0);

        UpdateText();
    }
    public void AddLap()
    {
        _lapCount++;
        PlayerPrefs.SetInt("LapsCount", _lapCount);

        UpdateText();
        _lapTextCount.transform.localScale = Vector3.one;

        _lapTextCount.transform.DORewind();
        _lapTextCount.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 3);
        _moneyManager.AddMoneyPerLap();
        LapPassed?.Invoke();
    }
    private void UpdateText()
    {
        _lapTextCount.text = _lapCount.ToString();
    }
}

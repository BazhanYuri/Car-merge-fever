using UnityEngine;
using DG.Tweening;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    [SerializeField] private GameConfig _gameConfig;
    [SerializeField] private GameObject _moneyEffectUI;
    [SerializeField] private TextMeshProUGUI _moneyText;

    [SerializeField] private Canvas _canvas;


    private int _moneyCount;

    public int MoneyCount { get => _moneyCount; }

    
    public void AddMoney(int count)
    {
        _moneyCount += count;
        UpdateMoneyText();
    }
    public void AddMoneyPerLap()
    {
        GameObject newEffect = Instantiate(_moneyEffectUI);
        newEffect.SetActive(true);
        newEffect.transform.parent = _moneyEffectUI.transform.parent;
        newEffect.transform.position = _moneyEffectUI.transform.position;
        newEffect.transform.rotation = _moneyEffectUI.transform.rotation;
        newEffect.transform.localScale = _moneyEffectUI.transform.localScale;

        newEffect.transform.DOLocalMoveY(newEffect.transform.localPosition.y + 200, 1).OnComplete(() => Destroy(newEffect));

        _moneyCount += _gameConfig.MoneyPerOneLap;
        UpdateMoneyText();
    }
    public void TakeMoney(int count)
    {
        _moneyCount -= count;
        UpdateMoneyText();
    }
    private void UpdateMoneyText()
    {
        PlayerPrefs.SetInt("Money", _moneyCount);
        _moneyText.rectTransform.DORewind();
        _moneyText.text = _moneyCount + "$";
        _moneyText.rectTransform.DOPunchScale(Vector3.one * 0.2f, 0.4f, 3);
    }

    private void Start()
    {
        _moneyCount = PlayerPrefs.GetInt("Money", 10);
        UpdateMoneyText();
    }
}

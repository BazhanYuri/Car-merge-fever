using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class Location : MonoBehaviour
{
    [SerializeField] private ParticleSystem _boughtParticle;
    [SerializeField] private TextMeshProUGUI _moneyEffectUI;
    [SerializeField] private MoneyManager _moneyManager;
    [SerializeField] private LapManager _lapManager;

    [SerializeField] private int _reward;
    [SerializeField] private int _cost;
    [SerializeField] private int _lapsToOpen;
    [SerializeField] private Transform _view;
    [SerializeField] private GameObject _applyBought;
    [SerializeField] private TextMeshProUGUI _price;


    private int _touchCount = 0;
    private Vector3 _standartScale;
    private bool _isBought;

    public void ResetTouch()
    {
        _touchCount = 0;
        _applyBought.transform.DOScale(Vector3.zero, 0.1f).OnComplete(() => _applyBought.gameObject.SetActive(false));

        print("Touchreseted");
    }
    public void TryBuyLocation()
    {
        _price.text = _cost + "$";
        _touchCount++;
        _applyBought.gameObject.SetActive(true);
        _applyBought.transform.localScale = _standartScale;
        _applyBought.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f);
        UpdateView();
        SoundManager.Instance.PlaySound("click");


        if (_touchCount == 2)
        {
            if (_moneyManager.MoneyCount >= _cost)
            {
                PlayerPrefs.SetInt(gameObject.name, 1);
                _applyBought.transform.DOPunchScale(Vector3.one * 0.5f, 0.3f, 2).OnComplete(() => HideUI(true));
                _moneyManager.TakeMoney(_cost);
                _isBought = true;

                SoundManager.Instance.PlaySound("coins");
                
            }
        }
    }

    private void OnEnable()
    {
        _lapManager.LapPassed += CheckIsCanOpen;
        _lapManager.LapPassed += AddMoney;
        _lapManager.LapPassed += UpdateView;
    }
    private void OnDisable()
    {
        _lapManager.LapPassed -= CheckIsCanOpen;
        _lapManager.LapPassed -= AddMoney;
        _lapManager.LapPassed -= UpdateView;
    }
    private void UpdateView()
    {
        if (_moneyManager.MoneyCount >= _cost)
        {
            _price.color = new Color(0.1952918f, 0.9622642f, 0.0944108f);
            _applyBought.GetComponent<Image>().enabled = true;
        }
        else
        {
            _price.color = new Color(0.8679245f, 0.2630538f, 0.1015308f);
            _applyBought.GetComponent<Image>().enabled = false;
        }
    }
    private void HideUI(bool firstTime)
    {
        gameObject.transform.DOScale(Vector3.zero, 0.2f).OnComplete(() => ShowView(firstTime));
    }
    private void ShowView(bool firstTime)
    {
        if (firstTime)
        {
            ParticleSystem particle = Instantiate(_boughtParticle);
            particle.transform.position = transform.position;

        }

        _view.gameObject.SetActive(true);
        _view.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 2).OnComplete(() => HideBuiUI());
    }
    private void HideBuiUI()
    {
        gameObject.transform.GetChild(0).localScale = Vector3.zero;
        gameObject.transform.localScale = Vector3.one;
    }

    private void Start()
    {
        LoadPrefsInfo();

        if (_isBought == true)
        {
            HideUI(false);
            return;
        }
        _standartScale = _applyBought.transform.localScale;
        Sequence mySequence = DOTween.Sequence().SetLoops(-1);
        mySequence.Append(transform.DOPunchPosition(new Vector3(0, 0.2f, 0), 1f, 2));
        mySequence.PrependInterval(2);
        CheckIsCanOpen();
    }
    private void CheckIsCanOpen()
    {
        if (_lapManager.LapCount >= _lapsToOpen)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            transform.DORewind();
            transform.DOPunchPosition(new Vector3(0, 0.1f, 0), 1f, 2);
            _lapManager.LapPassed -= CheckIsCanOpen;
        }
        else
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    private void AddMoney()
    {
        if (_isBought == false)
        {
            return;
        }
        TextMeshProUGUI newEffect = Instantiate(_moneyEffectUI);
        newEffect.gameObject.SetActive(true);
        newEffect.transform.parent = _moneyEffectUI.transform.parent;
        newEffect.transform.position = _moneyEffectUI.transform.position;
        newEffect.transform.rotation = _moneyEffectUI.transform.rotation;
        newEffect.transform.localScale = _moneyEffectUI.transform.localScale;
        newEffect.text = _reward + "$";

        newEffect.transform.DOLocalMoveY(newEffect.transform.localPosition.y + 200, 1).OnComplete(() => Destroy(newEffect));

        _moneyManager.AddMoney(_reward);
    }

    private void LoadPrefsInfo()
    {
        _isBought = PlayerPrefs.HasKey(gameObject.name);
    }
}

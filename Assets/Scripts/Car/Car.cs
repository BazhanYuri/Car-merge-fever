using UnityEngine;
using UnityEngine.UI;
using TMPro;




public class Car : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _levelIndexText;
    [SerializeField] private Image _progressBar;
    [SerializeField] private CarMovement _carMovement;
    [SerializeField] private SkinChooser _skinChooser;
    [SerializeField] private CollisionTrigered _collisionTrigered;
    [SerializeField] private CarMerge _carMerge;
    [SerializeField] private Present _present;


    private CarManager _carManager;
    private bool _isCarBlocked = false;
    private bool _isCarOnTrack = false;

    public CarMovement CarMovement { get => _carMovement;}
    public CollisionTrigered CollisionTrigered { get => _collisionTrigered;}
    public SkinChooser SkinChooser { get => _skinChooser;}
    public CarMerge CarMerge { get => _carMerge;}
    public bool IsCarBlocked { get => _isCarBlocked;}
    public TextMeshProUGUI LevelIndexText { get => _levelIndexText;}
    public Image ProgressBar { get => _progressBar;}
    public Present Present { get => _present;}
    public bool IsCarOnTrack { get => _isCarOnTrack;}

    public void BlockCar()
    {
        _isCarBlocked = true;
    }
    public void DeBlockCar()
    {
        _isCarBlocked = false;
    }
    public void SetCarAsOnTrack()
    {
        _isCarOnTrack = true;
    }


    public void Init(CarManager carManager)
    {
        _carManager = carManager;
    }
    private void Awake()
    {
        _carMovement.Init(this);
        _carMerge.Init(this, _carManager);
        _present.Init(this);
    }
}

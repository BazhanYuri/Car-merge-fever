using UnityEngine;
using TMPro;

public class CarMerge : MonoBehaviour
{
    [SerializeField] private ParticleSystem _mergeParticle;

    private Car _car;
    private CarManager _carManager;

    private int _levelIndex = 0;


    public int LevelIndex { get => _levelIndex; }


    
    public void Init(Car car, CarManager carManager)
    {
        _car = car;
        _carManager = carManager;
    }
    public void SetLevel(int levelIndex)
    {
        _levelIndex = levelIndex;
        _car.SkinChooser.ShowSkin(_levelIndex);
        UpdateText();
    }
    public void ImproveCar()
    {
        _levelIndex++;
        _car.SkinChooser.ShowSkin(_levelIndex);
        UpdateText();
        ParticleSystem mergeparticle = Instantiate(_mergeParticle);
        mergeparticle.transform.position = _car.transform.position;
        mergeparticle.transform.position += new Vector3(0, 0.1f, 0);
        mergeparticle.Play();
        SoundManager.Instance.PlaySound("merged");
    }
    private void UpdateText()
    {
        _car.LevelIndexText.text = (_levelIndex + 1).ToString();

        if (_levelIndex == 7)
        {
            _car.LevelIndexText.text = "M";
        }
    }

}

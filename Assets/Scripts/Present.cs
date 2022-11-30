using UnityEngine;
using DG.Tweening;

public class Present : MonoBehaviour
{
    [SerializeField] private ParticleSystem _openedParticle;
    [SerializeField] private BoxCollider _boxCollider;
    [SerializeField] private Transform _presentModel;
    private Car _car;

    public void Init(Car car)
    {
        _car = car;
    }

    public void EnablePresent()
    {
        _boxCollider.enabled = true;
        _car.BlockCar();
        _presentModel.gameObject.SetActive(true);
        _car.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
    }
    public void DisablePresent()
    {
        
        _presentModel.DOJump(_presentModel.position + new Vector3(0, 0.6f, 0), 0.2f, 1, 0.5f).SetEase(Ease.OutSine).OnComplete(() => OpenPresent());

        _boxCollider.enabled = false;
        _car.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
    }
    private void OpenPresent()
    {
        _presentModel.DOScale((Vector3.one * 20), 0.07f).OnComplete(() => PlayParticle());
        _car.DeBlockCar();
    }
    private void PlayParticle()
    {
        if (_presentModel.gameObject.activeInHierarchy == false)
        {
            return;
        }
        ParticleSystem particleSystem = Instantiate(_openedParticle);
        particleSystem.transform.position = _presentModel.position;
        particleSystem.transform.position += new Vector3(0, 0.5f, 0);
        particleSystem.Play();
        _presentModel.gameObject.SetActive(false);
        SoundManager.Instance.PlaySound("present");
    }
}

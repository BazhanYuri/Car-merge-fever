using UnityEngine;

public class SkinChooser : MonoBehaviour
{
    [SerializeField] private Transform[] _skins;




    public void ShowSkin(int index)
    {

        for (int i = 0; i < _skins.Length; i++)
        {
            _skins[i].gameObject.SetActive(false);
        }
        _skins[index].gameObject.SetActive(true);

    }
}

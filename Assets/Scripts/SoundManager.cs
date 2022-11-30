using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public void PlaySound(string name)
    {
        transform.Find(name).GetComponent<AudioSource>().Play();
    }

    public void PlayLapSound(float speed)
    {
        speed += 0.75f;
        AudioSource audioSource = Instantiate(transform.Find("lap")).GetComponent<AudioSource>();
        audioSource.gameObject.SetActive(true);
        audioSource.pitch = speed;
        audioSource.Play();
        
    }

    private void Awake()
    {
        Instance = this;
    }
}

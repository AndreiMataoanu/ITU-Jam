using UnityEngine;

public class AudioTriggers : MonoBehaviour
{
    bool Play = false;
    [SerializeField] public PowerUpType type;
    AudioSource audio;

    private void Awake()
    {
        audio = GetComponent<AudioSource>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void PlaySound() {
        if (Play) { 
            switch (type)
            {
                case PowerUpType.Knife:
                    audio.Play();
                    Play = true;
                    break;
            }
        }        

    }
}

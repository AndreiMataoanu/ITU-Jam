using UnityEngine;
using UnityEngine.InputSystem;

public class PowerUpInfo : MonoBehaviour
{
    [SerializeField] public int price;
    [SerializeField] public PowerUpType type;
    
    public void Activate()
    {
        switch (type)
        {
            // TODO: Implement power-up methods
            
            case PowerUpType.Knife:
                Debug.Log("Knife used");
                break;
            case PowerUpType.Scissors:
                Debug.Log("Scissors used");
                break;
            case PowerUpType.PrayerBeads:
                Debug.Log("PrayerBeads used");
                break;
            case PowerUpType.Glove:
                Debug.Log("Glove used");
                break;
            case PowerUpType.Sunglasses:
                Debug.Log("Sunglasses used");
                break;
            case PowerUpType.Cuffs:
                Debug.Log("Cuffs used");
                break;
        }
    }
    
}

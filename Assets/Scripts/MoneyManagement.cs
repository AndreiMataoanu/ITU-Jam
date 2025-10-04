using UnityEngine;

public class MoneyManagement : MonoBehaviour
{
    public int currentAmount = 100;

    public void LoseAmount(int amount)
    {
        currentAmount -= amount;
    }
}

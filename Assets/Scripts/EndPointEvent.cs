using UnityEngine;

public class EndPointEvent : MonoBehaviour
{
    public void Trigger()
    {
        GameManager.Instance.WinGame();
    }
}
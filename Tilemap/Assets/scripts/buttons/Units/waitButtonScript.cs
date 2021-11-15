using UnityEngine;

public class waitButtonScript : MonoBehaviour
{
    public eventsScript Wait;
    public void OnClick()
    {
        Wait.Raise();
    }
}

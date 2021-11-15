using UnityEngine;

public class LoadButtonScript : MonoBehaviour
{
    public eventsScript Load;
    public void load()
    {
        Load.Raise();
    }
}

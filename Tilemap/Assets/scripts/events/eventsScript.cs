using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]

public class eventsScript : ScriptableObject
{
    // Start is called before the first frame update
    private readonly List<GameEventListener> eventListeners =
        new List<GameEventListener>();
    public void Raise(Vector3Int attacker, Vector3Int defender)
    {
        for (int i = eventListeners.Count - 1; i >=0;i--)
        {
            eventListeners[i].OnEventRaised(attacker, defender);
        }
    }

    public void Raise()
    {
        for (int i = eventListeners.Count - 1; i >= 0; i--)
        {
            eventListeners[i].OnEventRaised(Vector3Int.zero, Vector3Int.zero);
        }
    }

    public void RegisterListener(GameEventListener listener)
    {
        if (!eventListeners.Contains(listener))
            eventListeners.Add(listener);
    }

    public void UnregisterListener(GameEventListener listener)
    {

        if (eventListeners.Contains(listener))
            eventListeners.Remove(listener);
    }
}

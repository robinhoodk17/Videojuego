using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    [Tooltip("Event to register with")]
    public eventsScript Event;

    [Tooltip("Response to invoke when Event is raised.")]
    public UnityEvent<Vector3Int, Vector3Int> Response;


    private void OnEnable()
    {
        Event.RegisterListener(this);
    }

    private void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    public void OnEventRaised(Vector3Int attacker, Vector3Int defender)
    {
        Response.Invoke(attacker, defender);
    }


}

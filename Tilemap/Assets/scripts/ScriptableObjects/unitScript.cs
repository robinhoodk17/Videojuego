using UnityEngine;

public class unitScript : MonoBehaviour
{
    //To access this unit position, you have to do it from the Mapmanager
    public int owner = 1;
    public string status;
    public int HP;
    public int MP;
    public int maxHP;
    public int movement;
    public string typeOfUnit;
    public string movementtype;
    public int attackrange = 1;
    public int attackdamage = 100;
    public string attacktype = "melee";
    public bool attackandmove = true;
    public bool exhausted = false;
    public int level = 0;
    public int levelcounter = 0;
    public int maxlevel = 10;
    public string state = "idle";
    public float movespeedanimation = 10;
    public string ability = "none";
    public string[] advantages = null;
    public string[] resistances = null;
    public string[] vulnerabilities = null;
    private int activeplayer = 1;
    //public Transform movepoint;
    //public float moveSpeed = 5f;

    public void trackactiveplayer(int player)
    {
        activeplayer = player;
    }
    public void turnEnd()
    {
        if (status == "stunned")
        {
            status = "recovered";
        }
        if(status == "downed" && activeplayer == owner)
        {
            Destroyed();
        }
    }

    public void turnStart()
    {
        if (status == "recovered")
        {
            status = "clear";
        }
        exhausted = false;

        levelcounter++;
        if (levelcounter >= 3)
            if (level < maxlevel) { level++; levelcounter = 0; }

        if(status == "clear")
        {
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
        }
    }

    public void Downed()
    {
        state = "downed";
        status = "downed";
        GetComponent<SpriteRenderer>().color = new Color(.5f, .5f, .5f);
    }

    public void recoverFromDowned()
    {
        state = "idle";
        status = "recovered";
        exhausted = true;
    }

    public void Destroyed()
    {
        Destroy(gameObject);
    }
    // Update is called once per frame
    void Update()
    {
    }
}

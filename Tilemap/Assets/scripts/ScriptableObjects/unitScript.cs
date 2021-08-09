using UnityEngine;
using TMPro;
public class unitScript : MonoBehaviour
{
    //To access this unit position, you have to do it from the Mapmanager
    public string name;
    public int owner = 1;
    public int foodCost = 100;
    public int SUPCost = 1;
    public string status;
    public int HP;
    public int MP;
    public int maxHP;
    public int movement;
    public string typeOfUnit;
    public string movementtype;
    public int attackrange = 1;
    public int attackdamage = 100;
    private string attacktype = "melee";
    public string _attacktype
    {
        get
        {
            return attacktype;
        }
    }
    public bool attackandmove = true;
    public string ability = "none";
    public string[] advantages = null;
    public string[] resistances = null;
    public string[] vulnerabilities = null;

    //do not touch
    public bool exhausted = false;
    public int level = 0;
    public int levelcounter = 0;
    public int maxlevel = 10;
    public string state = "idle";
    public float movespeedanimation = 10;


    private int activeplayer = 1;
    private int initialattack;
    private int initialmaxHP;

    [SerializeField]
    public GameObject healthbar;

    [SerializeField]
    public GameObject attack;

    [SerializeField]
    public GameObject ownerUI;
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
            if (level < maxlevel) 
            {
                level++; 
                levelcounter = 0;
                levelUp();
            }

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
    
    public void healthChanged()
    {
        if(HP < 0)
        {
            HP = 0;
        }
        healthbar.GetComponent<healthBar>().SetHealth(HP, maxHP);
        attack.GetComponent<TextMeshProUGUI>().text = ((int)(attackdamage * HP / maxHP)).ToString();
        healthbar.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = HP.ToString();
    }

    public void levelUp()
    {
        attackdamage += (initialattack / 10);
        maxHP += (initialmaxHP / 10);
        HP += (initialmaxHP / 10);
        healthChanged();
    }
    public void Awake()
    {
        healthbar.GetComponent<healthBar>().SetMaxHealth();
        healthbar.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = HP.ToString();
        initialattack = attackdamage;
        initialmaxHP = maxHP;
        attack.GetComponent<TextMeshProUGUI>().text = ((int)(attackdamage * HP / maxHP)).ToString();
        if(attackrange > 1)
        {
            attacktype = "ranged";
        }
        if(attacktype == "melee")
        {
            attack.transform.GetChild(0).gameObject.SetActive(true);
        }
        else
            attack.transform.GetChild(1).gameObject.SetActive(true);
        ownerUI.transform.GetChild(owner - 1).gameObject.SetActive(true);
        healthChanged();
    }

}

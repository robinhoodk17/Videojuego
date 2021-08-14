using UnityEngine;
using TMPro;
using UnityEngine.Tilemaps;
public class unitScript : MonoBehaviour
{
    //To access this unit position, you have to do it from the Mapmanager
    public string barracksname;
    public string unitname;
    public int owner = 1;
    public int foodCost = 100;
    public int SUPCost = 1;
    //possible status: downed, clear, stunned, recovered 
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

    public Animator animator;

    [SerializeField]
    public GameObject attack;

    public GameObject statusSprite;

    [SerializeField]
    public GameObject ownerUI;

    [SerializeField]
    private Tilemap map;

    private unitScript enemy;
    private string previousStatus = "clear";
    //public Transform movepoint;
    //public float moveSpeed = 5f;

    public void statusChange(string newstatus)
    {
        if (previousStatus == "stunned" || previousStatus == "recovered")
        {
            statusSprite.transform.GetChild(0).gameObject.SetActive(false);
        }

        if (newstatus == "stunned" || newstatus == "recovered")
        {
            statusSprite.transform.GetChild(0).gameObject.SetActive(true);
        }
        previousStatus = status;
        status = newstatus;
    }
    public void trackactiveplayer(int player)
    {
        activeplayer = player;
    }
    public void turnEnd()
    {
        if (status == "stunned")
        {
            statusChange("recovered");
        }
        if (status == "downed" && activeplayer == owner)
        {
            Destroyed();
        }
    }

    public void turnStart()
    {
        if (status == "recovered")
        {
            statusChange("clear");
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

        if (status == "clear")
        {
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
        }
    }

    public void Destroyed()
    {
        Destroy(gameObject);
    }

    public void healthChanged()
    {
        healthbar.SetActive(true);
        if (HP <= 0)
        {
            HP = 0;
            healthbar.SetActive(false);
        }
        healthbar.GetComponent<healthBar>().SetHealth(HP, maxHP);
        attack.GetComponent<TextMeshProUGUI>().text = ((int)(attackdamage * HP / maxHP)).ToString();
        healthbar.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = HP.ToString();
    }
    public void ownerChange(int newowner)
    {
        ownerUI.transform.GetChild(owner - 1).gameObject.SetActive(false);
        owner = newowner;
        ownerUI.transform.GetChild(owner - 1).gameObject.SetActive(true);
    }
    public void Awake()
    {
        healthbar.GetComponent<healthBar>().SetMaxHealth();
        healthbar.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = HP.ToString();
        initialattack = attackdamage;
        initialmaxHP = maxHP;
        attack.GetComponent<TextMeshProUGUI>().text = ((int)(attackdamage * HP / maxHP)).ToString();
        if (attackrange > 1)
        {
            attacktype = "ranged";
        }
        if (attacktype == "melee")
        {
            attack.transform.GetChild(0).gameObject.SetActive(true);
        }
        else
            attack.transform.GetChild(1).gameObject.SetActive(true);
        ownerUI.transform.GetChild(owner - 1).gameObject.SetActive(true);
        healthChanged();
        map = GameObject.FindGameObjectWithTag("builtMap").GetComponent<Tilemap>();
    }

    public void levelUp()
    {
        attackdamage += (initialattack / 10);
        maxHP += (initialmaxHP / 10);
        HP += (initialmaxHP / 10);
        healthChanged();
    }

    public void onCap()
    {
        switch (name)
        {
            case "warrior":
                levelcounter++;
                if (levelcounter >= 3)
                    levelUp();
                break;
        }
    }

    public void onCombat(unitScript defender)
    {
        animator.Play("shoot");
        enemy = defender;
        enemy.counterAttack(this);
        enemy.healthChanged();
    }

    public void counterAttack(unitScript attacker)
    {
        animator.Play("counterAttack");
        enemy = attacker;
    }
    public void damageEnemy()
    {
        enemy.onDamage();
    }
    public void onCombatWOCA(unitScript defender)
    {
        animator.Play("shoot");
        enemy = defender;
        enemy.animator.Play("damage");
        enemy.healthChanged();
    }

    public void Downed()
    {
        state = "idle";
        status = "downed";
        GetComponent<SpriteRenderer>().color = new Color(.5f, .5f, .5f);
        animator.Play("downed");
    }

    public void recoverFromDowned()
    {
        state = "idle";
        status = "recovered";
        exhausted = true;
        animator.Play("idle");
    }
    public void onDamage()
    {
        animator.Play("damage");
        healthChanged();
    }



    public unitScript getunit(Vector3 position, bool screen = true)
    {

        if (!screen)
        {
            position = Camera.main.WorldToScreenPoint(position);
        }
        var ray = Camera.main.ScreenPointToRay(position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var selection = hit.transform.gameObject;
            unitScript unit = selection.GetComponent<unitScript>();
            return unit;
        }
        //returns null if it did not find a unit
        else
        {
            return null;
        }
    }
    //tries to get a unit given a gridposition
    public unitScript getunit(Vector3Int position)
    {
        return getunit(Camera.main.WorldToScreenPoint(map.GetCellCenterWorld(position)));
    }

    public bool abilityCheck(Vector3Int position)
    {
        switch (ability)
        {
            case "none":
                return false;
            case "res":
                if (getunit(position + Vector3Int.left)?.status == "downed" && getunit(position + Vector3Int.left)?.typeOfUnit == "infantry")
                    return true;
                if (getunit(position + Vector3Int.right)?.status == "downed" && getunit(position + Vector3Int.right)?.typeOfUnit == "infantry")
                    return true;
                if (getunit(position + Vector3Int.up)?.status == "downed" && getunit(position + Vector3Int.up)?.typeOfUnit == "infantry")
                    return true;
                if (getunit(position + Vector3Int.down)?.status == "downed" && getunit(position + Vector3Int.down)?.typeOfUnit == "infantry")
                    return true;
                return false;
        }
        return false;
    }
}

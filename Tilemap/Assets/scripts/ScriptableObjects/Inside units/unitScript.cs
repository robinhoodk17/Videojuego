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
    public bool firstStrike = false;
    public string ability = "none";
    public string[] advantages = null;
    public string[] resistances = null;
    public string[] vulnerabilities = null;
    public string[] stuns = null;

    //do not touch
    public bool exhausted = false;
    public int level = 0;
    public int levelcounter = 0;
    public int maxlevel = 10;
    public float movespeedanimation = 25;


    private int activeplayer = 1;
    private int initialattack;
    private int initialmaxHP;

    [SerializeField]
    public GameObject healthbar;

    public Animator animator;

    [SerializeField]
    public GameObject attack;
    public TextMeshProUGUI BarracksNameText;
    public GameObject statusSprite;

    public SpriteRenderer sprite;

    [SerializeField]
    public GameObject ownerUI;

    public bool iscommander = false;

    private Tilemap map;
    private MapManager manager;

    private unitScript enemy;
    public string previousStatus = "clear";
    private int previousOwner;
    private AudioSource[] audios;
    public void customAwake()
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
        if (GameObject.FindGameObjectWithTag("MapManager") != null)
            manager = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
        audios = GetComponentsInChildren<AudioSource>();
        BarracksNameText.text = barracksname;
    }
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
        if(status == "captured")
        {
            statusChange("downed");
            ownerChange(previousOwner);
        }
    }

    public void turnStart()
    {
        if (status == "recovered")
        {
            statusChange("clear");
        }
        exhausted = false;

        gainXP();

        if (status == "clear")
        {
            sprite.color = new Color(1, 1, 1);
        }
    }

    public void Destroyed()
    {
        manager.destroyedunit(this);
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
    /// <summary>
    /// ///////////////////////////////////////////
    /// </summary>
    //animations
    //gainXP is calles whenever the unit gains XP, and it also controls the level up
    public void gainXP()
    {
        if (status != "downed")
        {
            levelcounter++;
        }

        if (levelcounter >= 5)
            if (level < maxlevel)
            {
                level++;
                levelcounter = 0;
                attackdamage += (initialattack / 10);
                maxHP += (initialmaxHP / 10);
                HP += (initialmaxHP / 10);
                healthChanged();
            }
    }

    public void onCap()
    {
        switch (name)
        {
            case "warrior":
                gainXP();
                break;
        }
    }

    public void onCombat(unitScript defender)
    {
        animator.SetTrigger("shoot");
        enemy = defender;
        enemy.counterAttack(this);
        enemy.healthChanged();

        switch (name)
        {
            case "sniper":
                gainXP();
                break;
        }
    }

    public void counterAttack(unitScript attacker)
    {
        animator.SetTrigger("counterAttack");
        enemy = attacker;
    }
    public void damageEnemy()
    {
        enemy.onDamage();
    }
    public void onCombatWOCA(unitScript defender)
    {
        animator.SetTrigger("shoot");
        enemy = defender;
        enemy.animator.Play("damage");
        enemy.healthChanged();
    }

    public void Downed(unitScript attacker)
    {
        status = "downed";
        sprite.color = new Color(.5f, .5f, .5f);
        animator.SetTrigger("downed");

        if(attacker.ability == "drain")
        {
            attacker.HP = attacker.maxHP;
        }
    }

    public void recoverFromDowned()
    {
        status = "clear";
        animator.SetTrigger("idle");
    }
    public void onDamage()
    {
        animator.SetTrigger("damage");
        healthChanged();
    }

    public void onMove()
    {
        animator.SetTrigger("move");
        audios[0].Play();
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
            case "heal":
                if ((getunit(position + Vector3Int.left)?.status == "downed" || getunit(position + Vector3Int.left)?.status == "captured") && getunit(position + Vector3Int.left)?.typeOfUnit == "infantry" && getunit(position + Vector3Int.left)?.owner == owner)
                    return true;
                if ((getunit(position + Vector3Int.right)?.status == "downed" || getunit(position + Vector3Int.right)?.status == "captured") && getunit(position + Vector3Int.right)?.typeOfUnit == "infantry" && getunit(position + Vector3Int.right)?.owner == owner)
                    return true;
                if ((getunit(position + Vector3Int.up)?.status == "downed" || getunit(position + Vector3Int.up)?.status == "captured") && getunit(position + Vector3Int.up)?.typeOfUnit == "infantry" && getunit(position + Vector3Int.up)?.owner == owner)
                    return true;
                if ((getunit(position + Vector3Int.down)?.status == "downed" || getunit(position + Vector3Int.left)?.status == "captured") && getunit(position + Vector3Int.down)?.typeOfUnit == "infantry" && getunit(position + Vector3Int.down)?.owner == owner)
                    return true;
                return false;
        }
        return false;
    }

    public void unitCaptured(int newowner)
    {
        ownerUI.transform.GetChild(owner - 1).gameObject.SetActive(false);
        previousOwner = owner;
        owner = newowner;
        ownerUI.transform.GetChild(owner - 1).gameObject.SetActive(true);
        status = "captured";
    }
}

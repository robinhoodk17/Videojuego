using UnityEngine;
using TMPro;
using UnityEngine.Tilemaps;
using Photon.Pun;
using System.Collections.Generic;
public enum TypeOfUnit
{
    infantry,
    vehicle,
    avatar

}

public enum Movement
{
    wheels,
    foot,
    treads,
    flying

}
public class unitScript : MonoBehaviourPun
{
    public TypeOfUnit typeOfUnit;
    public Movement movementtype;
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
    public int attackrange = 1;
    public int attackdamage = 100;
    private string attacktype = "melee";
    public bool cankill = true;
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
    public bool hasaura = false;
    public bool haswaitingaura = false;
    public int aurarange = 1;
    public bool istransport = false;
    public int unitCarryingCapacity = 1;
    public List<string> advantages;
    public List<string> resistances;
    public List<string> vulnerabilities;
    public List<string> stuns;

    //do not touch
    public bool exhausted = false;
    public int level = 0;
    public int xp = 0;
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
    public List<unitScript> transportedUnits { get; private set; }
    public string previousStatus = "clear";
    private int previousOwner;
    private AudioSource[] audios;
    public int xptoincreaselv = 5;
    public GameObject unLoadButtons;
    public void customAwake()
    {
        photonView.RPC("customAwakeNewtork", RpcTarget.All);
    }

    [PunRPC]
    public void customAwakeNewtork()
    {
        transportedUnits = new List<unitScript>();
        //healthbar.GetComponent<healthBar>().SetMaxHealth();
        healthbar.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = HP.ToString();
        healthbar.GetComponent<healthBar>().SetColor(owner);
        initialattack = attackdamage;
        initialmaxHP = maxHP;
        attack.GetComponent<TextMeshProUGUI>().text = ((int)(attackdamage * HP / maxHP)).ToString();
        if (attackrange > 1)
        {
            attacktype = "ranged";
            attack.transform.GetChild(1).gameObject.SetActive(true);
        }
        else
            attack.transform.GetChild(0).gameObject.SetActive(true);

        ownerUI.transform.GetChild(owner - 1).gameObject.SetActive(true);
        healthChanged();
        map = GameObject.FindGameObjectWithTag("builtMap").GetComponent<Tilemap>();
        if (GameObject.FindGameObjectWithTag("MapManager") != null)
            manager = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
        audios = GetComponentsInChildren<AudioSource>();
        BarracksNameText.text = barracksname;

    }
    //we call this method only when we are loading the unit from a saved game
    public void Load(int o, int m, int h, int l, int lc, string ps, string s, string bs, int ad, int mp)
    {
        photonView.RPC("load", RpcTarget.All, o, m, h, l, lc, ps, s, bs, ad, mp);
    }
    [PunRPC]
    public void load(int o, int m, int h, int l, int lc, string ps, string s, string bs, int ad, int mp)
    {
        owner = o;
        maxHP = m;
        HP = h;
        level = l;
        xp = lc;
        previousStatus = ps;
        status = s;
        barracksname = bs;
        attackdamage = ad;
        MP = mp;
    }
    public void statusChange(string newstatus)
    {
        photonView.RPC("statusChangeNetwork", RpcTarget.All, newstatus);
    }
    [PunRPC]
    public void statusChangeNetwork(string newstatus)
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
            HP = 10;
            recoverFromDowned();
            exhausted = true;
            sprite.color = new Color(.6f, .6f, .6f);
            healthChanged();

        }
        if (status == "captured")
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
        photonView.RPC("DestroyedNetwork", RpcTarget.All);
    }

    [PunRPC]
    public void DestroyedNetwork()
    {
        if (photonView.IsMine)
        {
            manager.destroyedunit(owner);
            PhotonNetwork.Destroy(gameObject);
        }
    }

    public void healthChanged()
    {
        photonView.RPC("healthChangedNetwork", RpcTarget.All, HP);
    }

    [PunRPC]
    public void healthChangedNetwork(int hp)
    {
        HP = hp;
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
    public void ownerChange(int newOwner)
    {
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("ownerChangeNetwork", RpcTarget.All, newOwner);
        }
        else
        {
            ownerChangeNetwork(newOwner);
        }
    }
    [PunRPC]
    public void ownerChangeNetwork(int newOwner)
    {
        ownerUI.transform.GetChild(owner - 1).gameObject.SetActive(false);
        owner = newOwner;
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
            xp++;
        }

        if (xp >= xptoincreaselv)
            if (level < maxlevel)
            {
                level++;
                xp -= xptoincreaselv;
                switch(name)
                {
                    case "warrior":
                        attackdamage += (initialattack / 5);
                        maxHP += (initialmaxHP / 5);
                        HP += (initialmaxHP / 5);
                        healthChanged();
                        break;
                    default:
                        attackdamage += (initialattack / 10);
                        maxHP += (initialmaxHP / 10);
                        HP += (initialmaxHP / 10);
                        healthChanged();
                        break;
                }
            }
        //this else is reached when they get to max level
            else
            {
                switch (name)
                {
                    case "warrior":
                        resistances.Add("melee");
                        break;
                    case "sniper":
                        attackrange = 4;
                        break;
                    default:
                        break;
                }
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

    // this is called only for the attacker
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


    public void Downed()
    {
        photonView.RPC("DownedNetwork", RpcTarget.All);
    }

    [PunRPC]
    public void DownedNetwork()
    {
        status = "downed";
        sprite.color = new Color(.5f, .5f, .5f);
        animator.SetTrigger("downed");
    }

    public void downedanotherUnit()
    {
        photonView.RPC("DownedanotherUnitNetwork", RpcTarget.All);
    }
    [PunRPC]
    public void DownedanotherUnitNetwork()
    {
        if(ability == "drain")
        {
            HP = maxHP;
            healthChanged();
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
        audios = GetComponentsInChildren<AudioSource>();
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
            #region heal
            case "heal":
                if ((getunit(position + Vector3Int.left)?.foodCost <= manager.food[owner - 1]) && getunit(position + Vector3Int.left)?.owner == owner)
                    return true;
                if ((getunit(position + Vector3Int.right)?.foodCost <= manager.food[owner - 1]) && getunit(position + Vector3Int.right)?.owner == owner)
                    return true;
                if ((getunit(position + Vector3Int.up)?.foodCost <= manager.food[owner - 1]) && getunit(position + Vector3Int.up)?.owner == owner)
                    return true;
                if ((getunit(position + Vector3Int.down)?.foodCost <= manager.food[owner - 1]) && getunit(position + Vector3Int.down)?.owner == owner)
                    return true;
                return false;
            #endregion
            #region teach
            case "teach":
                if ((getunit(position + Vector3Int.left)?.foodCost <= manager.food[owner - 1]/10) && getunit(position + Vector3Int.left)?.owner == owner && getunit(position + Vector3Int.left)?.level < maxlevel)
                    return true;
                if ((getunit(position + Vector3Int.right)?.foodCost <= manager.food[owner - 1]/10) && getunit(position + Vector3Int.right)?.owner == owner && getunit(position + Vector3Int.right)?.level < maxlevel)
                    return true;
                if ((getunit(position + Vector3Int.up)?.foodCost <= manager.food[owner - 1]/10) && getunit(position + Vector3Int.up)?.owner == owner && getunit(position + Vector3Int.up)?.level < maxlevel)
                    return true;
                if ((getunit(position + Vector3Int.down)?.foodCost <= manager.food[owner - 1]/10) && getunit(position + Vector3Int.down)?.owner == owner && getunit(position + Vector3Int.down)?.level < maxlevel)
                    return true;
                return false;
            #endregion
            #region unload
            case "unload":
                if (transportedUnits.Count > 0)
                    return true;
                else
                    return false;
            #endregion
        }
        return false;
    }
    public bool buildCheck()
    {
        if(name == "gundam")
        {
            foreach(GameObject unitPrefab in GameObject.FindGameObjectsWithTag("Unit"))
            {
                if(unitPrefab.GetComponent<unitScript>().level >= 10)
                {
                    return true;
                }
            }
            return false;
        }
        return true;
    }
    public bool auracheck(unitScript unit)
    {
        switch(ability)
        {
            case "pistolero":
                if (unit.owner != owner)
                    return true;
                else
                    return false;
            case "deathdealer":
                if (unit.owner != owner && unit.status == "downed")
                    return true;
                else
                    return
                        false;
            default:
                return false;
        }
    }
    public void unitCaptured(int newowner)
    {
        ownerUI.transform.GetChild(owner - 1).gameObject.SetActive(false);
        previousOwner = owner;
        owner = newowner;
        ownerUI.transform.GetChild(owner - 1).gameObject.SetActive(true);
        status = "captured";
    }

    public void deactivateObject(bool OnorOff)
    {
        photonView.RPC("deactivateObjectNetwork", RpcTarget.All, OnorOff);
    }

    [PunRPC]
    public void deactivateObjectNetwork(bool OnorOff)
    {
        gameObject.SetActive(OnorOff);
    }
}

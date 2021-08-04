using UnityEngine;
using UnityEngine.Tilemaps;

public class gridCombat : MonoBehaviour
{
    public Tilemap map, conditions;

    GameObject attacker;
    unitScript attackerScript;
    GameObject defender;
    unitScript defenderScript;

    private void Start()
    {
        //here we register this object as a listener to the combat initiation in the selection manager
        GameObject.FindGameObjectWithTag("SelectionManager").GetComponent<SelectionManager>().Oncombatstart += OncombatHappening;
    }
    public void OncombatHappening(Vector3Int attackposition, Vector3Int defendposition)
    {
        attacker = getunitprefab(worldPosition(attackposition), false);
        defender = getunitprefab(worldPosition(defendposition), false);
        attackerScript = getunit(attackposition);
        defenderScript = getunit(defendposition);
        defenderScript.HP -= calculateDamage(attackerScript, defenderScript);
        if(defenderScript.HP > 0 && defenderScript.attacktype == "melee" && checkifneighbors(attackposition, defendposition))
        {
            attackerScript.HP -= calculateDamage(defenderScript, attackerScript);
            if(attackerScript.HP <= 0)
            {
                attackerScript.Downed();
            }
        }
        if(defenderScript.HP <= 0)
        {
            defenderScript.Downed();
        }
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
    public Vector3 worldPosition(Vector3Int gridposition)
    {
        return map.CellToWorld(gridposition);
    }
    public GameObject getunitprefab(Vector3 position, bool screen = true)
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
            return selection;
        }
        //returns null if it did not find a unit
        else
        {
            return null;
        }
    }
    private bool checkifneighbors(Vector3Int pos1, Vector3Int pos2)
    {
        Vector3Int left = new Vector3Int(-1, 0, 0);
        Vector3Int right = new Vector3Int(1, 0, 0);
        Vector3Int up = new Vector3Int(0, 1, 0);
        Vector3Int down = new Vector3Int(0, -1, 0);

        if (pos1 + left == pos2 || pos1 + right == pos2 || pos1 + up == pos2 || pos1 + down == pos2)
            return true;
        else
            return false;
    }
    public int calculateDamage(unitScript attackingunit, unitScript defendingunit)
    {
        string[] attackingadv = null;
        string[] defendingresist = null;
        string[] defendingvul = null;
        int damage = attackingunit.attackdamage;
        if (attackingunit.advantages!= null)
        {
            attackingadv = attackingunit.advantages;
        }
        if(defendingunit.resistances != null)
        {
            defendingresist = defendingunit.resistances;
        }
        if(defendingunit.vulnerabilities != null)
        {
            defendingvul = defendingunit.vulnerabilities;
        }
        foreach(string adv in attackingadv)
        {
            if(adv == defendingunit.typeOfUnit || adv == defendingunit.movementtype || adv == defendingunit.attacktype)
            {
                damage *= 2;
            }
        }
        foreach(string vul in defendingvul)
        {
            if(vul == attackingunit.typeOfUnit || vul == attackingunit.movementtype || vul == attackingunit.attacktype)
            {
                damage *= 2;
            }
        }
        foreach(string res in defendingresist)
        {
            if(attackingunit.typeOfUnit == res || attackingunit.movementtype == res || attackingunit.attacktype == res)
            damage /= 2;
        }
        damage = damage * attackingunit.HP / attackingunit.maxHP * (attackingunit.level + 1);
        return damage;
    }
}

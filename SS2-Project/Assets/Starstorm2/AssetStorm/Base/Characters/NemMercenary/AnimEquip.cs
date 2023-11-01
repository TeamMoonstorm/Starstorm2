using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimEquip : MonoBehaviour
{
    public Transform equip;
    public Transform unequip;

    private int equipped = -1;
    public void EquipKnife(int equip)
    {
        if (equip == equipped) return;
        equipped = equip;
        bool e = equip == 1;
        if(this.equip) this.equip.gameObject.SetActive(e);
        if (this.unequip) this.unequip.gameObject.SetActive(!e);
    }
}

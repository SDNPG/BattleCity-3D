using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Property : MonoBehaviour {

    public float ArmorValue;
    public bool LastArmor;

    public Type type;
    public enum Type
    {
        Armor,Wall,Barriar
    }

    private void Update()
    {
    }

    private void Message(float T_arms,int shellType)
    {

    }
}

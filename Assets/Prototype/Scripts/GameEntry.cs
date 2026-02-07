using InfSurvivor.Runtime.Manager;
using UnityEngine;

public class GameEntry : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ObjectManager @object = Managers.Object as ObjectManager;
        @object.LoadPlayerResource();
    }
}

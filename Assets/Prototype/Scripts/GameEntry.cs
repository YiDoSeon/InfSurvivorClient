using InfSurvivor.Runtime.Manager;
using UnityEngine;

public class GameEntry : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Managers.Object.LoadPlayerResource();
    }
}

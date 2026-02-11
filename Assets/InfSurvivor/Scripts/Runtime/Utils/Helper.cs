using UnityEngine;

namespace InfSurvivor.Runtime.Utils
{
    public class Helper
    {
        public static GameObject FindChildByName(GameObject go, string name = null, bool recursive = false)
        {
            Transform transform = FindChildByName<Transform>(go, name, recursive);
            if (transform == null)
            {
                return null;
            }

            return transform.gameObject;
        }

        public static T FindChildByName<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
        {
            if (go == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            if (recursive == false)
            {
                for (int i = 0; i < go.transform.childCount; i++)
                {
                    Transform transform = go.transform.GetChild(i);
                    if (transform.name == name)
                    {
                        if (transform.TryGetComponent(out T component))
                        {
                            return component;
                        }
                    }
                }
            }
            else
            {
                foreach (T component in go.GetComponentsInChildren<T>())
                {
                    if (component.name == name)
                    {
                        return component;
                    }
                }
            }

            return null;
        }        
    }
}

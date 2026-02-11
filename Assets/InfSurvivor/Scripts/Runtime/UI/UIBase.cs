using System;
using System.Collections.Generic;
using InfSurvivor.Runtime.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InfSurvivor.Runtime.UI
{
    public abstract class UIBase : MonoBehaviour
    {
        protected Dictionary<Type, UnityEngine.Object[]> objects = new Dictionary<Type, UnityEngine.Object[]>();

        public abstract void Init();

        protected virtual void Awake()
        {
            Init();
        }

        protected void Bind<T>(Type type) where T : UnityEngine.Object
        {
            string[] names = Enum.GetNames(type);
            UnityEngine.Object[] objects = new UnityEngine.Object[name.Length];
            this.objects.Add(typeof(T), objects);

            for (int i = 0; i < names.Length; i++)
            {
                if (typeof(T) == typeof(GameObject))
                {
                    objects[i] = Helper.FindChildByName(gameObject, names[i], true);
                }
                else
                {
                    objects[i] = Helper.FindChildByName<T>(gameObject, names[i], true);
                }
                Debug.Assert(objects[i] != null, $"{names[i]} is not exist for binding");
            }
        }

        protected T Get<T>(int index) where T : UnityEngine.Object
        {
            if (objects.TryGetValue(typeof(T), out UnityEngine.Object[] set))
            {
                return set[index] as T;
            }
            return null;
        }

        protected GameObject GetObject(int index) => Get<GameObject>(index);
        protected Text GetText(int index) => Get<Text>(index);
        protected InputField GetInputField(int index) => Get<InputField>(index);
        protected Image GetImage(int index) => Get<Image>(index);
        protected Button GetButton(int index) => Get<Button>(index);

        protected TextMeshProUGUI GetTMP_Text(int index) => Get<TextMeshProUGUI>(index);
        protected TMP_InputField GetTMP_InputField(int index) => Get<TMP_InputField>(index);
    }
}

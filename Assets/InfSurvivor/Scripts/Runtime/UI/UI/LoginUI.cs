using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InfSurvivor.Runtime
{
    public class LoginUI : UIBase
    {
        #region @Bindings
        public enum Buttons
        {
            Connect_Btn
        }
        public enum TMP_InputFields
        {
            IP_InputField,
            Port_InputField
        }
        #endregion @Bindings
        public override void Init()
        {
            Bind<Button>(typeof(Buttons));
            Bind<TMP_InputField>(typeof(TMP_InputFields));

            GetButton((int)Buttons.Connect_Btn).onClick.AddListener(OnConnectBtnClick);
        }

        private void OnConnectBtnClick()
        {
            string ip = GetTMP_InputField((int)TMP_InputFields.IP_InputField).text;
            string port = GetTMP_InputField((int)TMP_InputFields.Port_InputField).text;
            //Debug.Log($"{ip}:{port}");
        }
    }
}

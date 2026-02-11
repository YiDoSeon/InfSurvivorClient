using InfSurvivor.Runtime.Manager;
using TMPro;
using UnityEngine.UI;

namespace InfSurvivor.Runtime.UI
{
    public class LoginUI : UIBase
    {
        #region @Bindings
        public enum Buttons
        {
            Connect_Btn
        }
        public enum TMP_Texts
        {
            Failed_Text
        }
        public enum TMP_InputFields
        {
            IP_InputField,
            Port_InputField
        }
        #endregion @Bindings

        private TMP_InputField ipInputField;
        private TMP_InputField portInputField;
        private Button connectBtn;
        private TextMeshProUGUI failedText;
        public override void Init()
        {
            Bind<Button>(typeof(Buttons));
            Bind<TextMeshProUGUI>(typeof(TMP_Texts));
            Bind<TMP_InputField>(typeof(TMP_InputFields));

            connectBtn = GetButton((int)Buttons.Connect_Btn);
            failedText = GetTMP_Text((int)TMP_Texts.Failed_Text);
            ipInputField = GetTMP_InputField((int)TMP_InputFields.IP_InputField);
            portInputField = GetTMP_InputField((int)TMP_InputFields.Port_InputField);

            connectBtn.onClick.AddListener(OnConnectBtnClick);
            failedText.text = string.Empty;
        }

        private void OnDisable()
        {
            if (Managers.IsDestroying)
            {
                return;
            }
            Managers.Network.onConnectSuccess -= OnNetworkConnectSuccess;
            Managers.Network.onConnectFailed -= OnNetworkConnectFailed;
        }

        private void OnConnectBtnClick()
        {
            string ipText = ipInputField.text;
            string portText = portInputField.text;
            int port = int.Parse(portText);
            
            failedText.text = string.Empty;
            DeactivateUIs();

            Managers.Network.onConnectSuccess += OnNetworkConnectSuccess;
            Managers.Network.onConnectFailed += OnNetworkConnectFailed;
            Managers.Network.ConnectToGame(ipText, port);
            //Debug.Log($"{ip}:{port}");
        }

        private void ActivateUIs()
        {
            ipInputField.interactable = true;
            portInputField.interactable = true;
            connectBtn.interactable = true;
        }

        private void DeactivateUIs()
        {
            ipInputField.interactable = false;
            portInputField.interactable = false;
            connectBtn.interactable = false;
        }

        public void OnNetworkConnectSuccess()
        {
            //Debug.Log("연결 성공!");
            Managers.Network.onConnectSuccess -= OnNetworkConnectSuccess;
            Managers.Network.onConnectFailed -= OnNetworkConnectFailed;
        }
        
        public void OnNetworkConnectFailed(string reason)
        {
            failedText.text = reason;
            Managers.Network.onConnectSuccess -= OnNetworkConnectSuccess;
            Managers.Network.onConnectFailed -= OnNetworkConnectFailed;
            ActivateUIs();
        }
    }
}

using InfSurvivor.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace InfSurvivor.Runtime
{
    public class LoadingPopup : UIBase
    {
        #region @Bindings
        public enum Images
        {
            ProgressbarValue_Image
        }

        #endregion @Bindings

        private Image progressbarValue;
        public override void Init()
        {
            Bind<Image>(typeof(Images));

            progressbarValue = GetImage((int)Images.ProgressbarValue_Image);
            progressbarValue.fillAmount = 0f;
        }

        public void SetProgressbarValue(float amount)
        {
            progressbarValue.fillAmount = amount;
        }
    }
}

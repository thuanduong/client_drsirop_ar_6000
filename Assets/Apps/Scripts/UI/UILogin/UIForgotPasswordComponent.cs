using System;
using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;

public class UIForgotPasswordComponent : PopupEntity<UIForgotPasswordComponent.Entity>
{
    public class Entity
    {
        public string Email;
        public string OTP;
        public string NewPassword;
        public ButtonEntity btnSend;
        public ButtonEntity btnOtp;
        public ButtonEntity btnMoveToOTP;
        public ButtonEntity btnMoveToEmail;

        public ButtonEntity btnCancel;
    }

    public TMP_InputField inputEmail;
    public TMP_InputField inputOTP;
    public TMP_InputField inputPassword;
    public UIButtonComponent btnSend;
    public UIButtonComponent btnOtp;
    public UIButtonComponent btnCancel;
    public UIButtonComponent btnMoveToOTP;
    public UIButtonComponent btnMoveToEmail;


    private UIForgotPasswordPanelAnimation anim;

    protected override void OnSetEntity()
    {
        inputEmail.text = entity.Email;
        inputEmail.onValueChanged.AddListener(OnChangeInputEmailValue);
        inputOTP.text = entity.OTP;
        inputOTP.onValueChanged.AddListener(OnChangeInputOTPValue);
        
        inputPassword.text = entity.NewPassword;
        inputPassword.onValueChanged.AddListener(OnChangeInputNewPasswordValue);

        btnSend.SetEntity(entity.btnSend);
        btnOtp.SetEntity(entity.btnOtp);
        btnCancel.SetEntity(entity.btnCancel);
        btnMoveToOTP.SetEntity(entity.btnMoveToOTP);
        btnMoveToEmail.SetEntity(entity.btnMoveToEmail);
    }


    void OnChangeInputEmailValue(string value)
    {
        entity.Email = value;
    }
    void OnChangeInputOTPValue(string value)
    {
        entity.OTP = value;
    }
    
    void OnChangeInputNewPasswordValue(string value)
    {
        entity.NewPassword = value;
    }

    public async UniTask ShowEmailPanel()
    {
        anim ??= animation as UIForgotPasswordPanelAnimation;
        await anim.ShowEmailPanel();
    }

    public async UniTask ShowOTPPanel()
    {
        anim ??= animation as UIForgotPasswordPanelAnimation;
        await anim.ShowOTPPanel();
    }
}

using System;
using UnityEngine;
using TMPro;


public class UILoginComponent : PopupEntity<UILoginComponent.Entity>
{
    public class Entity
    {
        public string Name;
        public string Pass;
        public ButtonEntity btnLogin;
        public ButtonEntity btnRegister;
        public ButtonEntity btnForgotPassword;
    }

    public TMP_InputField inputName;
    public TMP_InputField inputPassword;
    public UIButtonComponent btnLogin;
    public UIButtonComponent btnRegister;
    public UIButtonComponent btnForgotPassword;


    protected override void OnSetEntity()
    {
        inputName.text = entity.Name;
        inputName.onValueChanged.AddListener(OnChangeInputNameValue);
        inputPassword.text = entity.Pass;
        inputPassword.onValueChanged.AddListener(OnChangeInputPasswordValue);

    
        
        btnLogin.SetEntity(entity.btnLogin);
        btnRegister.SetEntity(entity.btnRegister);
        btnForgotPassword.SetEntity(entity.btnForgotPassword);
    }

    void OnChangeInputNameValue(string value)
    {
        entity.Name = value;
    }
    void OnChangeInputPasswordValue(string value)
    {
        entity.Pass = value;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIRegisterComponent : PopupEntity<UIRegisterComponent.Entity>
{
    public class Entity
    {
        public string Name;
        public string Email;
        public string Pass;
        public string ConfirmPass;
      
        public ButtonEntity btnCancel;
        public ButtonEntity btnRegister;
    }

    public TMP_InputField inputName;
    public TMP_InputField inputEmail;
    public TMP_InputField inputPassword;
    public TMP_InputField inputConfirmPassword;
    public UIButtonComponent btnRegister;
    public UIButtonComponent btnCancel;
  
   

    protected override void OnSetEntity()
    {
        inputName.text = entity.Name;
        inputName.onValueChanged.AddListener(OnChangeInputNameValue);
        inputEmail.text = entity.Email;
        inputEmail.onValueChanged.AddListener(OnChangeInputEmailValue);
        inputPassword.text = entity.Pass;
        inputPassword.onValueChanged.AddListener(OnChangeInputPasswordValue);
        inputConfirmPassword.text = entity.ConfirmPass;
        inputConfirmPassword.onValueChanged.AddListener(OnChangeInputConfirmPassValue);
        
     
   
        
        btnCancel.SetEntity(entity.btnCancel);
        btnRegister.SetEntity(entity.btnRegister);
    }

    void OnChangeInputNameValue(string value)
    {
        entity.Name = value;
    }

    void OnChangeInputEmailValue(string value)
    {
        entity.Email = value;
    }

    void OnChangeInputPasswordValue(string value)
    {
        entity.Pass = value;
    }

    void OnChangeInputConfirmPassValue(string value)
    {
        entity.ConfirmPass = value;
    }
}

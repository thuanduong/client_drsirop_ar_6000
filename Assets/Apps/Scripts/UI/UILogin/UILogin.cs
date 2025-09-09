using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UILogin : PopupEntity<UILogin.Entity>
{
    public class Entity
    {
        public UILoginComponent.Entity login;
        public UIRegisterComponent.Entity register;
        public UIForgotPasswordComponent.Entity forgotPassword;
    }

    public UILoginComponent login;
    public UIRegisterComponent register;
    public UIForgotPasswordComponent forgotPassword;

    protected override void OnSetEntity()
    {
        if (login != default)
            login.SetEntity(entity.login);
        if (register != default)
            register.SetEntity(entity.register);
        if (forgotPassword != default)
            forgotPassword.SetEntity(entity.forgotPassword);
    }

    public void SetLoginEntity()
    {
        if (login != default)
            login.SetEntity(entity.login);
    }

    public void SetRegisterEntity()
    {
        if (register != default)
            register.SetEntity(entity.register);
    }

    public void SetForgotPasswordEntity()
    {
        if (forgotPassword != default)
            forgotPassword.SetEntity(entity.forgotPassword);
    }
}

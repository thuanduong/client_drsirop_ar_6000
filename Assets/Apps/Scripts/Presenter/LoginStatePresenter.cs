using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Text.RegularExpressions;
using Core;

public class LoginStatePresenter : IDisposable
{
    private readonly IDIContainer container;
    private CancellationTokenSource cts;
    private UILogin uiLogin;
    private UIPopupMessage uiPopupMessage;


    private UniTaskCompletionSource ucs;

    private UserProfileModel profile;
    private UserProfileModel Profile => profile ??= UserData.Instance.GetOne<UserProfileModel>();

    private LoginService loginService;
    private LoginService LoginService => loginService ??= container.Inject<LoginService>();

    
    private bool _isLoginMetamask = false;

    public LoginStatePresenter(IDIContainer container)
    {
        this.container = container;
    }

    public void Dispose()
    {
        cts.SafeCancelAndDispose();
        cts = default;

        UILoader.SafeRelease(ref uiLogin);
        UILoader.SafeRelease(ref uiPopupMessage);

    }

    public async UniTask ConnectAndLoginAsync()
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();
        await LoginAsync();
    }

    private async UniTask LoginAsync()
    {
        if (ucs == default)
        {
            ucs = new UniTaskCompletionSource();
            await loadUI();
        }

        await ucs.Task;

        ucs = default;
    }


    private async UniTask loadUI ()
    {
        uiPopupMessage ??= await UILoader.Instantiate<UIPopupMessage>(UICanvas.UICanvasType.PopUp, token: cts.Token);
        uiLogin ??= await UILoader.Instantiate<UILogin>(token: cts.Token);
        var entity = new UILogin.Entity() {
            login = new UILoginComponent.Entity()
            {
                Name = "",
                Pass = "",
                btnLogin = new ButtonEntity(() => OnLoginClicked().Forget()),
                btnRegister = new ButtonEntity(()=>OnRegisterClicked().Forget()),
                btnForgotPassword = new ButtonEntity(() => OnForgotPasswordClicked().Forget()),
            },
            register = new UIRegisterComponent.Entity()
            {
                Name = "",
                Email= "",
                Pass = "",
                ConfirmPass = "",
                btnRegister = new ButtonEntity(() => OnRegister_RegisterClicked().Forget()),
                btnCancel = new ButtonEntity(() => OnRegister_CancelClicked().Forget()),
            },
            forgotPassword = new UIForgotPasswordComponent.Entity()
            {
                Email = "",
                OTP = "",
                btnSend = new ButtonEntity(() => OnForgotPass_SendEmailClicked().Forget()),
                btnOtp = new ButtonEntity(() => OnForgotPass_SendOTPClicked().Forget()),
                btnCancel = new ButtonEntity(() => OnForgotPass_CancelClicked().Forget()),
                btnMoveToEmail = new ButtonEntity(() => OnForgotPass_MoveToEmailClicked().Forget()),
                btnMoveToOTP = new ButtonEntity(() => OnForgotPass_MoveToOTPClicked().Forget()),
            }
            
        };
        uiLogin.SetEntity(entity);
        await uiLogin.In();
        await uiLogin.login.In();

        //await UniTask.Delay(10000);
    }

    private async UniTask OnLoginClicked()
    {
        ucs.TrySetResult();
        return;

        string mm = ValidateEmail(uiLogin.login.inputName.text);
        if (!string.IsNullOrEmpty(mm))
        {
            //Show error
            Debug.Log(mm);
            await ShowMessagePopUp("NOTICE", mm);
            return;
        }
        string pp = ValidatePassword(uiLogin.login.inputPassword.text);
        if (!string.IsNullOrEmpty(pp))
        {
            //Show error
            Debug.Log(pp);
            await ShowMessagePopUp("NOTICE", pp);
            return;
        }

        bool kq = true;
        profile = UserData.Instance.GetOne<UserProfileModel>();
        if (kq)
        {
            ucs.TrySetResult();
        }
        else
        {
            var message = "login failed";
            await ShowMessagePopUp("NOTICE", message);
        }

    }

    private async UniTask OnRegisterClicked()
    {
        Debug.Log("Register Clicked !");
        await uiLogin.login.Out();
        await uiLogin.register.In();
    }

    private async UniTask OnForgotPasswordClicked()
    {
        await uiLogin.login.Out();
        await uiLogin.forgotPassword.In();
    }

    private async UniTask OnRegister_RegisterClicked()
    {
        string mm = ValidateEmail(uiLogin.register.inputEmail.text);
        if (!string.IsNullOrEmpty(mm))
        {
            //Show error
            Debug.Log(mm);
            await ShowMessagePopUp("NOTICE", mm);
            return;
        }
        string pp = ValidatePassword(uiLogin.register.inputPassword.text);
        if (!string.IsNullOrEmpty(pp))
        {
            //Show error
            Debug.Log(pp);
            await ShowMessagePopUp("NOTICE", pp);
            return;
        }
    }

    private async UniTask OnRegister_CancelClicked()
    {
        await uiLogin.register.Out();
        await uiLogin.login.In();
    }

    private async UniTask OnForgotPass_SendEmailClicked()
    {
        Debug.Log(uiLogin.forgotPassword.inputEmail.text);
        await UniTask.CompletedTask;
    }

    private async UniTask OnForgotPass_SendOTPClicked()
    {
        await UniTask.CompletedTask;
    }

    private async UniTask OnForgotPass_CancelClicked()
    {
        await uiLogin.forgotPassword.Out();
        await uiLogin.login.In();
    }

    private async UniTask OnForgotPass_MoveToOTPClicked()
    {
        await uiLogin.forgotPassword.ShowOTPPanel();
    }

    private async UniTask OnForgotPass_MoveToEmailClicked()
    {
        await uiLogin.forgotPassword.ShowEmailPanel();
    }

    private string ValidateEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return "Email cannot be empty.";
        }

        // Regular expression to check if the email ends with @gmail.com
        string emailPattern = @"^[^@\s]+@gmail\.com$";
        if (!Regex.IsMatch(email, emailPattern))
        {
            return "The email must be a valid Gmail address (e.g., example@gmail.com).";
        }

        return string.Empty;
    }

    private string ValidatePassword(string password)
    {
        if (password.Length < 10)
        {
            return "The password must be at least 10 characters long.";
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Z]"))
        {
            return "The password must contain at least one uppercase letter.";
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[@$!%*?&]"))
        {
            return "The password must contain at least one special character.";
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"\d"))
        {
            return "The password must contain at least one digit.";
        }

        return string.Empty;
    }

    private async UniTask ShowMessagePopUp(string title, string message)
    {
        var uiConfirm = await UILoader.Instantiate<UIPopupMessage>(token: cts.Token);
        bool wait = true;

        uiConfirm.SetEntity(new UIPopupMessage.Entity()
        {
            title = title,
            message = message,
            confirmBtn = new ButtonEntity(() => { wait = false; })
        });
        await uiConfirm.In();
        await UniTask.WaitUntil(() => wait == false);
        UILoader.SafeRelease(ref uiConfirm);
    }
    
}

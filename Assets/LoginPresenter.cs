using UnityEngine;
using UnityEngine.UI;

public class LoginPresenter : Presenter
{
    private MainCoordinator coordinator;
    [SerializeField] private InputField userNameInputField;
    [SerializeField] private InputField passwordInputField;
    [SerializeField] private Button primaryActionButton;

    void Start()
    {
        coordinator = GetComponentInParent<MainCoordinator>();
        Reset();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && primaryActionButton.enabled) {
            OnClickLogin();
        }
    }

    public void Reset()
    {
        userNameInputField.Select();
        userNameInputField.text = "";
        passwordInputField.text = "";
        primaryActionButton.enabled = false;
    }

    public void OnClickCreateAccount()
    {
        coordinator.NavigateToCreateAccount();
    }

    public void OnClickLogin()
    {
        coordinator.Login(userNameInputField.text, passwordInputField.text);
    }

    public void OnInputFieldsChanged()
    {
        primaryActionButton.enabled = userNameInputField.text.Length > 2 && passwordInputField.text.Length > 2;
    }
}
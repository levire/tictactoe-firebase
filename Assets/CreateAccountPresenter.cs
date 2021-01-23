using UnityEngine;
using UnityEngine.UI;

public class CreateAccountPresenter : Presenter
{
    private MainCoordinator coordinator;

    [SerializeField] private InputField userNameInputField;
    [SerializeField] private InputField passwordInputField;
    [SerializeField] private Button primaryActionButton;

    void Start()
    {
        coordinator = GetComponentInParent<MainCoordinator>();
        userNameInputField.Select();
        primaryActionButton.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && primaryActionButton.enabled) {
            OnCreateAccount();
        }
    }

    public void Reset()
    {
        userNameInputField.Select();
        userNameInputField.text = "";
        passwordInputField.text = "";
        primaryActionButton.enabled = false;
    }

    public void OnBack()
    {
        coordinator.NavigateToLogin();
    }

    public void OnCreateAccount()
    {
        coordinator.CreateAccount(userNameInputField.text, passwordInputField.text);
    }

    public void OnInputFieldsChanged()
    {
        primaryActionButton.enabled = userNameInputField.text.Length > 2 && passwordInputField.text.Length > 2;
    }
}

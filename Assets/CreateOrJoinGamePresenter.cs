using UnityEngine;
using UnityEngine.UI;

public class CreateOrJoinGamePresenter : Presenter
{
    private MainCoordinator coordinator;
    [SerializeField] private InputField gameIdInputField;
    [SerializeField] private Button joinActionButton;
    
    void Start()
    {
        coordinator = GetComponentInParent<MainCoordinator>();
        gameIdInputField.Select();
        joinActionButton.enabled = false;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && joinActionButton.enabled) {
            OnJoinGame();
        }
    }

    public void OnInputFieldsChanged()
    {
        gameIdInputField.text = gameIdInputField.text.ToUpper();
        joinActionButton.enabled = gameIdInputField.text.Length == 5;
    }

    public void OnCreateGame()
    {
        coordinator.CreateGame();
    }

    public void OnJoinGame()
    {
        coordinator.JoinGame(gameIdInputField.text);
    }
}

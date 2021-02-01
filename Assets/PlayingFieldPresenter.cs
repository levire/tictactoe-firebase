using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayingFieldPresenter : Presenter
{
    private MainCoordinator coordinator;

    [SerializeField] private List<Button> buttons;
    [SerializeField] private Button closeButton;
    [SerializeField] private Text playerXText;
    [SerializeField] private Text playerOText;
    [SerializeField] private Text gameIdText;
    [SerializeField] private Text statusText;
    [SerializeField] private GameObject xIndicator;
    [SerializeField] private GameObject oIndicator;
    [SerializeField] private Text chatWindow;
    [SerializeField] private InputField chatMessage;
    [SerializeField] private Button sendChatMessageButton;

    Color xColor = new Color(0.07465164f, 0.06786222f, 0.5754717f, 1.0f);
    Color oColor = new Color(0.6981132f, 0.0f, 0.006702666f, 1.0f);
    Color defaultColor = Color.black;

    // Start is called before the first frame update
    void Start()
    {
        coordinator = GetComponentInParent<MainCoordinator>();
        Clear();
        InitFieldClickHandler();
    }

    public void Clear()
    {
        foreach (Button b in buttons) {
            b.GetComponentInChildren<Text>().text = "";
        }

        playerXText.text = "";
        playerOText.text = "";

        SetGameId("");
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && sendChatMessageButton.enabled) {
            OnSendChat();
        }
    }

    private void InitFieldClickHandler()
    {
        Debug.Log("Init playing field click handlers.");
        for (int i = 0; i < 9; i++) {
            int buttonIndex = i;
            buttons[i].onClick.AddListener(() => {
                coordinator.MakeMove(buttonIndex);
            });
        }
    }

    public void SetGameId(string gameId)
    {
        gameIdText.text = "Game ID: " + gameId;
    }

    public void SetGameState(GameState gameState)
    {
        Debug.Log("GameState update recieved for: " + gameState.GameId);

        playerXText.text = gameState.PlayerX;
        playerOText.text = gameState.PlayerO;
        SetGameId(gameState.GameId);

        if (gameState.PlayerO == null || gameState.PlayerX == null || gameState.Winner != null || gameState.FieldFull == true) {
            closeButton.GetComponentInChildren<Text>().text = "Exit";
        } else {
            closeButton.GetComponentInChildren<Text>().text = "Give Up";
        }

        if (gameState.Turn == "x") {
            playerXText.color = xColor;
            xIndicator.SetActive(true);
            playerOText.color = defaultColor;
            oIndicator.SetActive(false);
        } else if (gameState.Turn == "o") {
            playerXText.color = defaultColor;
            xIndicator.SetActive(false);
            playerOText.color = oColor;
            oIndicator.SetActive(true);
        }

        for (int i = 0; i < 9; i++) {
            Text text = buttons[i].GetComponentInChildren<Text>();
            string value = gameState.Field[i];
            if (value != null) {
                text.text = value.ToUpper();
                text.color = value == "x" ? xColor : oColor;
            }
        }
    }

    public void SetChatState(ChatState chatState)
    {
        string messages = "";
        foreach (string message in chatState.ChatMessages) {
            messages += message + "\n";
        }
        chatWindow.text = messages;
    }

    public void OnGiveUp()
    {
        coordinator.LeaveGame();
    }

    public void OnSendChat()
    {
        coordinator.SendChatMessage(chatMessage.text);
        chatMessage.text = "";
    }
}

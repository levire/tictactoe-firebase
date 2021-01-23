using System;
using UnityEngine;

[RequireComponent(typeof(DatabaseService))]
public class MainCoordinator : MonoBehaviour
{
    private DatabaseService databaseService;
    private string currentUser = null;
    private string currentGameId = null;

    [SerializeField] private LoginPresenter loginPresenter;
    [SerializeField] private CreateAccountPresenter createAccountPresenter;
    [SerializeField] private CreateOrJoinGamePresenter createOrJoinGamePresenter;
    [SerializeField] private PlayingFieldPresenter playingFieldPresenter;
    [SerializeField] private StatusPresenter statusPresenter;


    private void DeactivateAllPresenters()
    {
        loginPresenter.gameObject.SetActive(false);
        createAccountPresenter.gameObject.SetActive(false);
        createOrJoinGamePresenter.gameObject.SetActive(false);
        playingFieldPresenter.gameObject.SetActive(false);
    }

    void Start()
    {
        databaseService = GetComponent<DatabaseService>();
        NavigateToLogin();
    }

    public void NavigateToLogin()
    {
        DeactivateAllPresenters();
        loginPresenter.gameObject.SetActive(true);
        loginPresenter.Interactable = true;
        loginPresenter.Reset();
    }

    public void NavigateToCreateAccount()
    {
        DeactivateAllPresenters();
        createAccountPresenter.gameObject.SetActive(true);
        createAccountPresenter.Interactable = true;
        createAccountPresenter.Reset();
    }

    public void NavigateToJoinCreateGame()
    {
        DeactivateAllPresenters();
        createOrJoinGamePresenter.gameObject.SetActive(true);
        createOrJoinGamePresenter.Interactable = true;
    }

    public void NavigateToPlayingField()
    {
        DeactivateAllPresenters();
        playingFieldPresenter.gameObject.SetActive(true);
        playingFieldPresenter.Interactable = true;
    }

    public void CreateAccount(string username, string password)
    {
        createAccountPresenter.Interactable = false;
        databaseService.AddUser(username, password, result => {
            createAccountPresenter.Interactable = true;
            if (result == StatusCode.OK) {
                NavigateToLogin();
                statusPresenter.SetStatus("Account created.", StatusPresenter.COLOR_SUCCESS);
            } else {
                statusPresenter.SetStatus(StatusText.For(result), StatusPresenter.COLOR_ERROR);
            }
        });
    }

    public void Login(string username, string password)
    {
        loginPresenter.Interactable = false;
        databaseService.LoginUser(username, password, result => {
            loginPresenter.Interactable = true;
            if (result == StatusCode.OK) {
                currentUser = username;
                NavigateToJoinCreateGame();
                statusPresenter.SetStatus("Logged in.", StatusPresenter.COLOR_SUCCESS);
            } else {
                statusPresenter.SetStatus(StatusText.For(result), StatusPresenter.COLOR_ERROR);
            }
        });
    }

    public void CreateGame()
    {
        if (currentUser == null) {
            return;
        }

        createOrJoinGamePresenter.Interactable = false;
        databaseService.CreateGame(currentUser, (result, gameId) => {
            createOrJoinGamePresenter.Interactable = true;
            if (result == StatusCode.OK) {
                currentGameId = gameId;
                Debug.Log("Game created: " + gameId);
                playingFieldPresenter.Clear();
                NavigateToPlayingField();
                statusPresenter.SetStatus("Game created.", StatusPresenter.COLOR_SUCCESS);
                databaseService.RegisterForGameUpdates(gameId, gameState => {
                    playingFieldPresenter.SetGameState(gameState);

                    StatusCode status = gameState.CanMakeMove(currentUser);
                    if (status != StatusCode.OK && status != StatusCode.GAME_NOT_YOUR_TURN) {
                        ShowGameStatus(status);
                    }
                });
            } else {
                statusPresenter.SetStatus(StatusText.For(result), StatusPresenter.COLOR_ERROR);
            }
        });
    }

    public void JoinGame(string gameId)
    {
        if (currentUser == null) {
            return;
        }

        createOrJoinGamePresenter.Interactable = false;
        databaseService.JoinGame(currentUser, gameId, result => {
            createOrJoinGamePresenter.Interactable = true;
            if (result == StatusCode.OK) {
                currentGameId = gameId;
                Debug.Log("Successfully joined game.");
                playingFieldPresenter.Clear();
                NavigateToPlayingField();
                statusPresenter.SetStatus("You joined the game.", StatusPresenter.COLOR_SUCCESS);
                databaseService.RegisterForGameUpdates(gameId, gameState => {
                    playingFieldPresenter.SetGameState(gameState);

                    StatusCode status = gameState.CanMakeMove(currentUser);
                    if (status != StatusCode.OK && status != StatusCode.GAME_NOT_YOUR_TURN) {
                        ShowGameStatus(status);
                    }
                });
            } else {
                statusPresenter.SetStatus(StatusText.For(result), StatusPresenter.COLOR_ERROR);
            }
        });
    }

    public void LeaveGame(Action completion = null)
    {
        if (currentUser == null || currentGameId == null) {
            return;
        }

        playingFieldPresenter.Interactable = false;
        databaseService.LeaveGame(currentGameId, currentUser, result => {
            playingFieldPresenter.Interactable = true;
            statusPresenter.Reset();
            currentGameId = null;
            NavigateToJoinCreateGame();
            completion();
        });
    }

    void OnApplicationQuit()
    {
        LeaveGame(() => {
            databaseService.Shutdown();
        });
    }

    public void MakeMove(int fieldIndex)
    {
        if (currentUser == null || currentGameId == null) {
            Debug.LogError("You are not logged in or inside a game.");
            return;
        }

        databaseService.MakeMove(currentUser, currentGameId, fieldIndex, result => {
            if (result != StatusCode.OK) {
                ShowGameStatus(result);
            }
        });
    }

    private void ShowGameStatus(StatusCode status)
    {
        switch (status) {
            case StatusCode.GAME_YOU_LOST:
                statusPresenter.SetStatus(StatusText.For(status), StatusPresenter.COLOR_ERROR, false);
                break;
            case StatusCode.GAME_YOU_WIN:
                statusPresenter.SetStatus(StatusText.For(status), StatusPresenter.COLOR_SUCCESS, false);
                break;
            case StatusCode.GAME_DRAW:
                statusPresenter.SetStatus(StatusText.For(status), StatusPresenter.COLOR_NOTICE, false);
                break;
            default:
                statusPresenter.SetStatus(StatusText.For(status), StatusPresenter.COLOR_ERROR);
                break;
        }
    }
}
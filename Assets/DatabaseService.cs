using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

public class DatabaseService : MonoBehaviour
{
    private FirebaseFirestore db;
    private FirebaseApp app;
    private bool initialized = false;

    void Start()
    {
        StreamReader reader = new StreamReader(Application.streamingAssetsPath + "/google-services-desktop.json");
        AppOptions options = AppOptions.LoadFromJsonConfig(reader.ReadToEnd());
        reader.Close();

        app = FirebaseApp.Create(options, Guid.NewGuid().ToString());
        db = FirebaseFirestore.GetInstance(app);
        initialized = true;
    }
    public void Shutdown()
    {
        db.TerminateAsync();
        db.ClearPersistenceAsync();
    }

    public void AddUser(string username, string password, Action<StatusCode> completion)
    {
        if (!initialized) {
            completion(StatusCode.UNKNWON);
            return;
        }

        DocumentReference userRef = db.Collection("users").Document(username);
        StatusCode transactionStatus = StatusCode.OK;
        db.RunTransactionAsync(async transaction => {
            DocumentSnapshot user = await transaction.GetSnapshotAsync(userRef);
            if (user.Exists) {
                transactionStatus = StatusCode.USER_ALREADY_EXISTS;
                return false;
            }

            transaction.Set(userRef, new Dictionary<string, object> {
                    { "passwordHash", CreateMD5Hash(password) }
            });

            return true;
        }).ContinueWithOnMainThread(task => {
            completion(transactionStatus);
        });
    }

    public void LoginUser(string username, string password, Action<StatusCode> completion)
    {
        if (!initialized) {
            completion(StatusCode.UNKNWON);
            return;
        }

        DocumentReference userRef = db.Collection("users").Document(username);
        userRef.GetSnapshotAsync().ContinueWithOnMainThread(task => {
            DocumentSnapshot user = task.Result;
            if (!user.Exists) {
                completion(StatusCode.CREDENTIALS_WRONG);
                return;
            }

            if (user.GetValue<string>("passwordHash") != CreateMD5Hash(password)) {
                completion(StatusCode.CREDENTIALS_WRONG);
                return;
            }

            completion(StatusCode.OK);
        });
    }

    public void CreateGame(string username, Action<StatusCode, string> completion)
    {
        if (!initialized) {
            completion(StatusCode.UNKNWON, null);
            return;
        }

        string gameId = CreateGameID();
        db.Collection("games").Document(gameId).SetAsync(new GameState(username, gameId)).ContinueWithOnMainThread(task => {
            if (task.IsCompleted) {
                completion(StatusCode.OK, gameId);
            } else {
                completion(StatusCode.UNKNWON, null);
            }
        });

        db.Collection("chats").Document(gameId).SetAsync(new ChatState(gameId, new List<string>())).ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                completion(StatusCode.OK, gameId);
            }
            else
            {
                completion(StatusCode.UNKNWON, null);
            }
        });
    }

    public void JoinGame(string username, string gameId, Action<StatusCode> completion)
    {
        if (!initialized) {
            completion(StatusCode.UNKNWON);
            return;
        }

        DocumentReference gameRef = db.Collection("games").Document(gameId);
        StatusCode transactionStatus = StatusCode.OK;
        db.RunTransactionAsync(async transaction => {
            DocumentSnapshot gameSnapshot = await transaction.GetSnapshotAsync(gameRef);

            if (!gameSnapshot.Exists) {
                transactionStatus = StatusCode.GAME_ID_UNKNOWN;
                return false;
            }

            GameState gameState = gameSnapshot.ConvertTo<GameState>();

            if (gameState.PlayerO != null) {
                transactionStatus = StatusCode.GAME_FULL;
                return false;
            }

            gameState.PlayerO = username;

            transaction.Set(gameRef, gameState);
            return true;
        }).ContinueWithOnMainThread(task => {
            completion(transactionStatus);
        });
    }

    public void MakeMove(string username, string gameId, int fieldIndex, Action<StatusCode> completion)
    {
        DocumentReference gameRef = db.Collection("games").Document(gameId);
        StatusCode transactionStatus = StatusCode.OK;
        db.RunTransactionAsync(async transaction => {
            DocumentSnapshot game = await transaction.GetSnapshotAsync(gameRef);

            if (!game.Exists) {
                transactionStatus = StatusCode.UNKNWON;
                return false;
            }

            GameState gameState = game.ConvertTo<GameState>();

            transactionStatus = gameState.MakeMove(username, fieldIndex);
            if (transactionStatus == StatusCode.GAME_FIELD_ALREADY_FULL || transactionStatus == StatusCode.GAME_NOT_YOUR_TURN) {
                return false;
            }

            transaction.Set(gameRef, gameState);
            return true;
        }).ContinueWithOnMainThread(task => {
            completion(transactionStatus);
        });
    }

    private ListenerRegistration _gameUpdateListener = null;
    public void RegisterForGameUpdates(string gameId, Action<GameState> callback)
    {
        Debug.Log("Registering for Game " + gameId);
        _gameUpdateListener = db.Collection("games").Document(gameId).Listen(snapshot => {
            callback(snapshot.ConvertTo<GameState>());
        });
    }

    public void LeaveGame(string gameId, string username, Action<StatusCode> completion)
    {
        DocumentReference gameRef = db.Collection("games").Document(gameId);

        StatusCode transactionStatus = StatusCode.OK;
        db.RunTransactionAsync(async transaction => {
            DocumentSnapshot game = await transaction.GetSnapshotAsync(gameRef);

            if (!game.Exists) {
                transactionStatus = StatusCode.UNKNWON;
                return false;
            }

            GameState gameState = game.ConvertTo<GameState>();

            if (gameState.PlayerX == username) {
                gameState.PlayerX = null;
            }

            if (gameState.PlayerO == username) {
                gameState.PlayerO = null;
            }

            if (gameState.IsEmpty) {
                if (_gameUpdateListener != null) {
                    _gameUpdateListener.Stop();
                }
                if (_chatUpdateListener != null)
                {
                    _chatUpdateListener.Stop();
                }
                transaction.Delete(gameRef);
            } else {
                transaction.Set(gameRef, gameState);
            }

            return true;
        }).ContinueWithOnMainThread(taks => {
            completion(transactionStatus);
        });
    }

    public void SendChatMessage(string gameId, string message, string username, Action<StatusCode> completion)
    {
        DocumentReference chatRef = db.Collection("chats").Document(gameId);
        StatusCode transactionStatus = StatusCode.OK;
        db.RunTransactionAsync(async transaction => {
            DocumentSnapshot chat = await transaction.GetSnapshotAsync(chatRef);

            if (!chat.Exists)
            {
                transactionStatus = StatusCode.UNKNWON;
                return false;
            }

            ChatState chatState = chat.ConvertTo<ChatState>();
            chatState.AddMessage(username, message);

            transaction.Set(chatRef, chatState);
            return true;
        
        }).ContinueWithOnMainThread(task => {
            completion(transactionStatus);
        });
    }

    private ListenerRegistration _chatUpdateListener = null;
    public void RegisterForChatUpdates(string gameId, Action<ChatState> callback)
    {
        Debug.Log("Registering for Chat " + gameId);
        _chatUpdateListener = db.Collection("chats").Document(gameId).Listen(snapshot => {
            callback(snapshot.ConvertTo<ChatState>());
        });
    }

    private string CreateMD5Hash(string str)
    {
        // create byte sequence from string
        byte[] bytes = new UTF8Encoding().GetBytes(str);

        // use the md5 hash function to create a md5 hash
        byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(bytes);

        // convert bytes back to string and return
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hash.Length; i++) {
            sb.Append(hash[i].ToString("X2")); // "X2" indicates the string should be formatted in Hexadecimal
        }
        return sb.ToString();
    }

    private string CreateGameID()
    {
        System.Random random = new System.Random();
        return random.Next(0xFFFFF).ToString("X5");
    }
}

using System.Collections.Generic;
using Firebase.Firestore;

[FirestoreData]
public class ChatState
{
    [FirestoreProperty("game_id")]
    public string GameId { get; set; }

    [FirestoreProperty("chat_messages")]
    public List<string> ChatMessages { get; set; }

    public ChatState()
    {
        GameId = null;
        ChatMessages = new List<string>();
    }

    public ChatState(string gameId, List<string> messages)
    {
        GameId = gameId;
        ChatMessages = messages;
    }

    public void AddMessage(string userName, string message)
    {
        ChatMessages.Add(userName + " - " + message);
    }
}

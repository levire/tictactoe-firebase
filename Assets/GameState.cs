using System.Collections.Generic;
using Firebase.Firestore;

[FirestoreData]
public class GameState
{
    [FirestoreProperty("game_id")]
    public string GameId { get; set; }

    [FirestoreProperty("player_x")]
    public string PlayerX { get; set; }

    [FirestoreProperty("player_o")]
    public string PlayerO { get; set; }

    [FirestoreProperty("field")]
    public List<string> Field { get; set; }

    [FirestoreProperty("turn")]
    public string Turn { get; set; }

    public GameState(string playerX, string gameId)
    {
        PlayerX = playerX;
        GameId = gameId;
        PlayerO = null;
        Field = new List<string> { null, null, null, null, null, null, null, null, null };
        Turn = "x";
    }

    public GameState()
    {
        PlayerX = null;
        GameId = null;
        PlayerO = null;
        Field = new List<string> { null, null, null, null, null, null, null, null, null };
        Turn = "x";
    }

    public string PlayersSign(string username)
    {
        return PlayerX == username ? "x" : "o";
    }

    public string OtherPlayersSign(string username)
    {
        return PlayerX == username ? "o" : "x";
    }

    public bool IsPlayersTurn(string username)
    {
        return PlayersSign(username) == Turn;
    }

    private string Get(int x, int y)
    {
        return Field[y * 3 + x];
    }

    private void Set(int x, int y, string sign)
    {
        Field[y * 3 + x] = sign;
    }

    public StatusCode CanMakeMove(string username)
    {
        string hasWinner = Winner;
        if (hasWinner != null && hasWinner == PlayersSign(username)) {
            return StatusCode.GAME_YOU_WIN;
        } else if (hasWinner != null && hasWinner != PlayersSign(username)) {
            return StatusCode.GAME_YOU_LOST;
        } else if (FieldFull) {
            return StatusCode.GAME_DRAW;
        } else if (!IsPlayersTurn(username)) {
            return StatusCode.GAME_NOT_YOUR_TURN;
        }
        return StatusCode.OK;
    }

    public StatusCode MakeMove(string username, int fieldIndex)
    {
        StatusCode result = CanMakeMove(username);
        if (result != StatusCode.OK) {
            return result;
        }

        if (Field[fieldIndex] != null) {
            return StatusCode.GAME_FIELD_ALREADY_FULL;
        }

        Field[fieldIndex] = PlayersSign(username);

        if (Winner == "x") {
            Turn = "X_WINS";
        } else if (Winner == "o") {
            Turn = "O_WINS";
        } else if (FieldFull) {
            Turn = "DRAW";
        } else {
            Turn = OtherPlayersSign(username);
        }

        result = CanMakeMove(username);
        if (result != StatusCode.GAME_NOT_YOUR_TURN) {
            return result;
        }

        return StatusCode.OK;
    }

    public bool FieldFull
    {
        get {
            foreach (string value in Field) {
                if (value == null) {
                    return false;
                }
            }
            return true;
        }
    }

    public string Winner
    {
        get {
            // check columns
            for (int x = 0; x < 3; x++) {
                string col_value = Get(x, 0);
                if (col_value != null && Get(x, 1) == col_value && Get(x, 2) == col_value) {
                    return col_value;
                }
            }

            // check rows
            for (int y = 0; y < 3; y++) {
                string row_value = Get(0, y);
                if (row_value != null && Get(1, y) == row_value && Get(2, y) == row_value) {
                    return row_value;
                }
            }

            // check diagonals
            string ltr_value = Get(0, 0);
            if (ltr_value != null && Get(1, 1) == ltr_value && Get(2, 2) == ltr_value) {
                return ltr_value;
            }
            string rtl_value = Get(2, 0);
            if (rtl_value != null && Get(1, 1) == rtl_value && Get(0, 2) == rtl_value) {
                return rtl_value;
            }

            return null;
        }
    }

    public bool IsEmpty
    {
        get {
            return PlayerX == null && PlayerO == null;
        }
    }
}

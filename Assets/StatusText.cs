public class StatusText
{
    public static string For(StatusCode code)
    {
        switch (code) {
            case StatusCode.OK:
                return "All good";
            case StatusCode.CREDENTIALS_WRONG:
                return "Username or password is wrong!";
            case StatusCode.GAME_ID_UNKNOWN:
                return "Unknown Game ID";
            case StatusCode.GAME_DRAW:
                return "Game over! It's a draw!";
            case StatusCode.GAME_NOT_YOUR_TURN:
                return "It's not your turn.";
            case StatusCode.GAME_YOU_LOST:
                return "You lost.";
            case StatusCode.GAME_YOU_WIN:
                return "You win.";
            case StatusCode.GAME_FIELD_ALREADY_FULL:
                return "This field is already full.";
            case StatusCode.GAME_FULL:
                return "This game is already full.";
            case StatusCode.USER_ALREADY_EXISTS:
                return "This username has been already taken.";
            default:
                return "Sorry, something went wrong.";
        }
    }
}

namespace GameData.Network
{
    public enum ResultType
    {
        UnknownError, Success, WrongData, InvalidGameKey, Banned,
        CharacterLimit,
        NameEmpty, NameInvalid, NameExists,
        PasswordEmpty, PasswordInvalid
    }
}

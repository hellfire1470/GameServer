
using GameServer.SQL;
using GameData.Network;

namespace GameServer.Server
{
    public class Character : CharacterData
    {

        // Allowed Chars in name, //todo: put this into settings
        private static readonly char[] _validCharsInName =
            new char[]{'a', 'b', 'c', 'd', 'e', 'f', 'g',
        'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p',
        'q', 'r', 's','t','u','v','w','x','y','z',
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        };

        public override ResultType Create(AccountData account)
        {
            if (GameData.Manager.InputManager.ContainsOnly(_validCharsInName, Name))
            {
                return base.Create(account);
            }
            return ResultType.NameInvalid;
        }
    }
}
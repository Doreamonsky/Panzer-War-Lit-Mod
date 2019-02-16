using ProtoBuf;

namespace Multiplayer.Msg
{
    [ProtoContract]
    public class LoginInfo
    {
        public LoginInfo(string uid)
        {
            Uid = uid;
        }

        private LoginInfo()
        {
        }

        [ProtoMember(1)]
        public string Uid { get; private set; }
    }
}

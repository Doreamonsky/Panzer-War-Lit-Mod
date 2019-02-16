namespace Multiplayer.Msg
{
    [System.Serializable]
    public class TestMsg : BinaryFormatterCustomObject
    {
        public TestMsg(string msg)
        {
            this.msg = msg;
        }

        public string msg { private set; get; }
    }
}

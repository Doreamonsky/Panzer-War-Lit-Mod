using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer
{
    [Serializable]
    public class BinaryFormatterCustomObject
    {
        public int IntValue { get; private set; }
        public string StringValue { get; private set; }

        /// <summary>
        /// Constructor object for BinaryFormatterCustomObject
        /// </summary>
        /// <param name="intValue"></param>
        /// <param name="stringValue"></param>
        public BinaryFormatterCustomObject(int intValue, string stringValue)
        {
            this.IntValue = intValue;
            this.StringValue = stringValue;
        }
    }
}

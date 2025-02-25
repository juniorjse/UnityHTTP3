using System.Collections.Generic;

namespace Wildlife.PitayaCSharp.Protobuf.Util
{
    public class Util
    {
        //Simple type
        string[] _types;
        readonly Dictionary<string, int> _typeMap;

        public Util()
        {
            _typeMap = new Dictionary<string, int>();
            InitTypeMap();
            _types = new string[] {"uInt32", "sInt32", "int32", "uInt64", "sInt64", "float", "double"};
        }

        /// <summary>
        /// Check out the given type. If it is simple, return ture.
        /// </summary>
        /// <returns>
        /// The simple type.
        /// </returns>
        /// <param name='type'>
        /// If set to <c>true</c> type.
        /// </param>
        public bool IsSimpleType(string type)
        {
            int length = _types.Length;
            bool flag = false;
            for (int i = 0; i < length; i++)
            {
                if (type == _types[i])
                {
                    flag = true;
                    break;
                }
            }

            return flag;
        }

        /// <summary>
        /// Check out the given type. If the type exist in typeMap, return true.
        /// </summary>
        /// <returns>
        /// The type.
        /// </returns>
        /// <param name='type'>
        /// Type.
        /// </param>
        public int ContainType(string type)
        {
            int value, returnInt = 2;
            if (_typeMap.TryGetValue(type, out value))
            {
                returnInt = value;
            }

            return returnInt;
        }

        //Init the typeMap
        private void InitTypeMap()
        {
            _typeMap.Add("uInt32", 0);
            _typeMap.Add("sInt32", 0);
            _typeMap.Add("int32", 0);
            _typeMap.Add("double", 1);
            _typeMap.Add("string", 2);
            _typeMap.Add("float", 5);
            _typeMap.Add("message", 2);
        }

        /// <summary>
        /// Reverses the order of bytes in the array
        /// </summary>
        public static void Reverse(byte[] bytes)
        {
            byte temp;
            for (int first = 0, last = bytes.Length - 1; first < last; first++, last--)
            {
                temp = bytes[first];
                bytes[first] = bytes[last];
                bytes[last] = temp;
            }
        }
    }
}

namespace NotifiAlert
{
    [System.Serializable]
    public class PacketException : System.Exception
    {
        public PacketException() { }
        public PacketException(string message) : base(message) { }
        public PacketException(string message, System.Exception inner) : base(message, inner) { }
        protected PacketException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}

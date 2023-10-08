namespace OBSWebsocketSharp
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    internal class EnumElementMetadataAttribute(
        int rpcVersion,
        bool deprecated,
        string initialVersion,
        string enumValue,
        bool fromString) : Attribute
    {
        public int rpcVersion = rpcVersion;
        public bool deprecated = deprecated;
        public string initialVersion = initialVersion;
        public string enumValue = enumValue;
        public bool fromString = fromString;
    }

    [AttributeUsage(AttributeTargets.Event, AllowMultiple = false, Inherited = false)]
    internal class EventDataMetadataAttribute(
        EventSubscription eventSubscription,
        int complexity,
        int rpcVersion,
        bool deprecated,
        string initialVersion,
        string category) : Attribute
    {
        public EventSubscription eventSubscription = eventSubscription;
        public int complexity = complexity;
        public int rpcVersion = rpcVersion;
        public bool deprecated = deprecated;
        public string initialVersion = initialVersion;
        public string category = category;
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal class RequestMetadataAttribute(
        int complexity,
        int rpcVersion,
        bool deprecated,
        string initialVersion,
        string category) : Attribute
    {
        public int complexity = complexity;
        public int rpcVersion = rpcVersion;
        public bool deprecated = deprecated;
        public string initialVersion = initialVersion;
        public string category = category;
    }
}

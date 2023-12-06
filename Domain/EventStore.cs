using Eveneum;
using Microsoft.Azure.Cosmos;

namespace System
{
    internal class EventStore<T> : EventStore, IEventStore<T>
    {
        public EventStore(CosmosClient client, string database, string container, EventStoreOptions options = null) : base(client, database, container, options)
        {
        }
    }
}

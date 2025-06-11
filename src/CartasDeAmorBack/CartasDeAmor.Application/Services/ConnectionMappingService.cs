using CartasDeAmor.Domain.Services;
using System.Collections.Concurrent;

namespace CartasDeAmor.Application.Services;

public class ConnectionMappingService : IConnectionMappingService
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _connections = new();

    public void AddConnection(string userEmail, string connectionId)
    {
        _connections.AddOrUpdate(userEmail, 
            new HashSet<string> { connectionId }, 
            (key, existingConnections) =>
            {
                lock (existingConnections)
                {
                    existingConnections.Add(connectionId);
                    return existingConnections;
                }
            });
    }

    public void RemoveConnection(string userEmail, string connectionId)
    {
        if (_connections.TryGetValue(userEmail, out var connections))
        {
            lock (connections)
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    _connections.TryRemove(userEmail, out _);
                }
            }
        }
    }

    public IEnumerable<string> GetConnections(string userEmail)
    {
        if (_connections.TryGetValue(userEmail, out var connections))
        {
            lock (connections)
            {
                return connections.ToList();
            }
        }
        return Enumerable.Empty<string>();
    }

    public void RemoveConnectionById(string connectionId)
    {
        var userToRemove = _connections.FirstOrDefault(kvp =>
        {
            lock (kvp.Value)
            {
                return kvp.Value.Contains(connectionId);
            }
        });

        if (!userToRemove.Equals(default(KeyValuePair<string, HashSet<string>>)))
        {
            RemoveConnection(userToRemove.Key, connectionId);
        }
    }
}

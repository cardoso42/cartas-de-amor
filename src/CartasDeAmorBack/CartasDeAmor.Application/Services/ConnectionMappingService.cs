using CartasDeAmor.Domain.Services;
using System.Collections.Concurrent;

namespace CartasDeAmor.Application.Services;

public class ConnectionMappingService : IConnectionMappingService
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _emailConnections = new();
    private readonly ConcurrentDictionary<string, Guid> _connectionsRooms = new();

    public void AddConnection(string userEmail, string connectionId, Guid roomId)
    {
        _emailConnections.AddOrUpdate(userEmail,
            new HashSet<string> { connectionId },
            (key, existingConnections) =>
            {
                lock (existingConnections)
                {
                    existingConnections.Add(connectionId);
                    return existingConnections;
                }
            });

        _connectionsRooms.AddOrUpdate(connectionId, roomId, (key, existingRoomId) => roomId);
    }

    public void RemoveConnection(string userEmail, string connectionId)
    {
        if (_emailConnections.TryGetValue(userEmail, out var connections))
        {
            lock (connections)
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    _emailConnections.TryRemove(userEmail, out _);
                }
            }
        }
    }

    public IEnumerable<string> GetConnections(string userEmail)
    {
        if (_emailConnections.TryGetValue(userEmail, out var connections))
        {
            lock (connections)
            {
                return connections.ToList();
            }
        }

        return [];
    }

    public void RemoveConnectionById(string connectionId)
    {
        var userToRemove = _emailConnections.FirstOrDefault(kvp =>
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

    public Guid? GetRoomIdByConnectionId(string connectionId)
    {
        var found = _connectionsRooms.TryGetValue(connectionId, out var roomId);

        return found ? roomId : null;
    }
}

namespace CartasDeAmor.Domain.Services;

public interface IConnectionMappingService
{
    void AddConnection(string userEmail, string connectionId, Guid roomId);
    void RemoveConnection(string userEmail, string connectionId);
    IEnumerable<string> GetConnections(string userEmail);
    void RemoveConnectionById(string connectionId);
    Guid? GetRoomIdByConnectionId(string connectionId);
}

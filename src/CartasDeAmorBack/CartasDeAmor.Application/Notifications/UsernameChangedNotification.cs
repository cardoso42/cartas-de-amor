using MediatR;

namespace CartasDeAmor.Application.Notifications;

public class UsernameChangedNotification : INotification
{
    public string UserEmail { get; set; } = string.Empty;
    public string NewUsername { get; set; } = string.Empty;
    public string OldUsername { get; set; } = string.Empty;
}

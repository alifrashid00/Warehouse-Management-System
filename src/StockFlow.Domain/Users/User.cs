using StockFlow.Domain.Common;

namespace StockFlow.Domain.Users;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Role Role { get; set; }
    public bool IsActive { get; set; } = true;
}

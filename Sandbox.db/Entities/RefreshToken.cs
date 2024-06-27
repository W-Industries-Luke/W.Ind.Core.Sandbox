using W.Ind.Core.Entity;

namespace Sandbox.db.Entities;

// RefreshTokenBase (abstract): Defines Id, Token, Expires, UserId, User

// ISoftDelete (interface): Implement to use .FilterDeleted() in your DbContext.OnModelCreating method
public class RefreshToken : RefreshTokenBase, ISoftDelete
{
    // ISoftDelete property
    public bool IsDeleted { get; set; }
}

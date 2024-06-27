using Sandbox.db.Entities;
using W.Ind.Core.Repository;
using W.Ind.Core.Service;

namespace Sandbox.db.Repositories;

// RefreshTokenRepositoryBase<RefreshToken> (abstract): Implements a repository with base CRUD methods
    // Also implements Generate, Refresh, & Invalidate methods specific to Refresh Tokens

// IRefreshTokenRepository (interface): Wrapper interface for type specialized core interface
    // W.Ind.Core.IRefreshTokenRepsitory<RefreshToken>
public class RefreshTokenRepository : RefreshTokenRepositoryBase<RefreshToken>, IRefreshTokenRepository
{
    // Dependency injected base constructor
    public RefreshTokenRepository(SandboxDbContext context, IJwtService jwtService) : base(context, jwtService) { }
}

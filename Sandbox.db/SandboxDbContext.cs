using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Sandbox.db.Entities;
using W.Ind.Core.Entity;
using W.Ind.Core.Helper;
using W.Ind.Core.Service;

namespace Sandbox.db;

public class SandboxDbContext : IdentityDbContext<CoreUser, CoreRole, long, CoreUserClaim, CoreUserRole, CoreUserLogin, CoreRoleClaim, CoreUserToken> 
{
    // Used to inject services as needed (dotnet ef CLI injects differently from application)
    protected readonly IServiceProvider _serviceProvider;

    // Injected base constructor
    public SandboxDbContext(DbContextOptions options, IServiceProvider serviceProvider) 
        : base(options) 
    {
        _serviceProvider = serviceProvider;
    }

    // Create table for Refresh Tokens
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);


        // .FilterDeleted(): When called on an ISoftDelete entity
            // All Queries will automatically filter out records where IsDeleted == true
        builder.Entity<RefreshToken>().FilterDeleted()
            // .OneToMany(hasOne, configure?, withOne?): Configures relationship without breaking daisychain
            .OneToMany(token => token.User, configure => configure.HasForeignKey(fk => fk.UserId));


        // .SeedFromJson(filePath): Deserialzes a JSON file and includes it as seed data for your migration
            // filePath is relative to your startup project directory
        builder.Entity<CoreUser>().ConfigureAudit("Users").FilterDeleted().SeedFromJson("Seed/Users.json");


        // .ConfigureAudit(tableName?, temporalConfig?): When called on an IAuditable entity:
            // Configures relationships for the properties defined by IAuditable
        builder.Entity<CoreRole>().ConfigureAudit("Roles").FilterDeleted();
        builder.Entity<CoreRoleClaim>().ConfigureAudit("RoleClaims").FilterDeleted();
        builder.Entity<CoreUserClaim>().ConfigureAudit("UserClaims").FilterDeleted();
        builder.Entity<CoreUserRole>().ConfigureAudit("UserRoles").FilterDeleted();


        // The only Core entities that don't implement IAuditable
        builder.Entity<CoreUserLogin>().ToTable("UserLogins").FilterDeleted();
        builder.Entity<CoreUserToken>().ToTable("UserTokens").FilterDeleted();
    }

    public override int SaveChanges()
    {
        // Manually injects IUserService to get the current User's ID
        var userService = _serviceProvider.GetRequiredService<IUserService>();


        // .HandleAudit(userId): Goes through the ChangeTracker's IAuditable entities to:
            // Update columns defined in IAuditable for temporal table
        ChangeTracker.Entries<IAuditable>().HandleAudit(userService.GetCurrent());


        // .HandleSoftDelete(): Goes throught the ChangeTracker's ISoftDelete entities to:
            // Set it's IsDeleted property to true instead of deleting the record
        ChangeTracker.Entries<ISoftDelete>().HandleSoftDelete();

        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userService = _serviceProvider.GetRequiredService<IUserService>();


        // Can also get the current UserId asyncronously
        ChangeTracker.Entries<IAuditable>().HandleAudit(await userService.GetCurrentAsync());
        ChangeTracker.Entries<ISoftDelete>().HandleSoftDelete();

        return await base.SaveChangesAsync(cancellationToken);
    }
}
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace IntelliHome.Cloud.Identity;

[CollectionName("Users")]
public sealed class ApplicationUser : MongoIdentityUser<Guid>
{
}
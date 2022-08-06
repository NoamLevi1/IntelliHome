using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace IntelliHome.Cloud.Identity;

[CollectionName("Roles")]
public sealed class ApplicationRole : MongoIdentityRole<Guid>
{
}
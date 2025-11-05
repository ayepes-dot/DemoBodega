using AuthDomain.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace AuthDatabase.Context
{
    public class AuthServiceDbContext
    {
        private readonly IMongoDatabase _database;

        public AuthServiceDbContext(IOptions<AuthDBSettings> authdbSettings)
        {
            var client = new MongoClient(authdbSettings.Value.ConnectionString);
            _database = client.GetDatabase(authdbSettings.Value.DatabaseName);
        }

        public IMongoCollection<RevokedToken> RevokedTokens =>
            _database.GetCollection<RevokedToken>("RevokedTokens");
    }
}

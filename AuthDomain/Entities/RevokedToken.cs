using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuthDomain.Entities
{
    public class RevokedToken
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string TokenId { get; set; }

        [BsonElement("jti")]
        public string Jti { get; set; }

        [BsonElement("UserId")]
        public int UserId { get; set; }

        [BsonElement("RevokedAT")]
        public DateTime RevokedAt { get; set; }

        [BsonElement("expiresAt")]
        public DateTime ExpiresAt { get; set; }

    }
}

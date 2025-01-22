using elmstoragerefimpl.Models;
using elmstoragerefimpl.Service.Interface;
using MongoDB.Bson;
using MongoDB.Driver;

namespace elmstoragerefimpl.Service
{
    public class MongoService : IStorageService
    {
        private readonly IMongoCollection<BsonDocument> _collection;

        public MongoService(IConfiguration config)
        {
            string mongoConnectionString = config.GetSection("CosmosDB")["ConnectionString"];
            var mongoClient = new MongoClient(mongoConnectionString);
            string databaseName = config.GetSection("CosmosDB")["DatabaseName"];
            string collectionName = config.GetSection("CosmosDB")["CollectionName"];

            var database = mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<BsonDocument>(collectionName);
        }

        public async Task<IEnumerable<Guid>> ListObjectsAsync(string objectType)
        {
            var filter = Builders<BsonDocument>.Filter.Regex("_id", new BsonRegularExpression($"^{objectType}\\."));

            var projection = Builders<BsonDocument>.Projection.Include("_id");
            var objects = await _collection.Find(filter)
                .Project(projection)
                .ToListAsync();

            return objects.Select(o => Guid.Parse(o["_id"].AsString.Split('.')[1]));
        }

        public async Task<object?> GetObjectAsync(string objectType, Guid id)
        {
            string formattedId = GetFormattedId(objectType, id);

            var filter = Builders<BsonDocument>.Filter.Eq("_id", formattedId);

            var projection = Builders<BsonDocument>.Projection.Exclude("_id");
            var obj = await _collection.Find(filter).Project(projection).FirstOrDefaultAsync();
            if (obj != null)
            {
                // Only return the "Data" field
                var data = obj["Data"].AsString;
                return data;
            }
            return null;
        }

        public async Task<IEnumerable<object>> GetAllObjectsAsync(string objectType)
        {
            var filter = Builders<BsonDocument>.Filter.Regex("_id", new BsonRegularExpression($"^{objectType}\\."));
            var objects = await _collection.Find(filter).ToListAsync();

            return objects.Select(o =>
            {
                var formattedId = o["_id"].AsString;
                var id = Guid.Parse(formattedId.Split('.')[1]);
                o.Remove("_id");

                return new
                {
                    Id = id,
                    Object = o["Data"].AsString
                };
            });
        }

        public async Task<Guid> SaveObjectAsync(string objectType, Guid? id, string data)
        {
            var objId = id ?? Guid.NewGuid();
            string formattedId = GetFormattedId(objectType, objId);

            var document = new BsonDocument
                {
                    { "Data", data }
                };

            var filter = Builders<BsonDocument>.Filter.Eq("_id", formattedId);
            var options = new ReplaceOptions { IsUpsert = true };

            await _collection.ReplaceOneAsync(filter, document, options);

            return objId;
        }

        public async Task<bool> DeleteObjectAsync(string objectType, Guid id)
        {
            string formattedId = GetFormattedId(objectType, id);

            var filter = Builders<BsonDocument>.Filter.Eq("_id", formattedId);

            var result = await _collection.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }

        private static string GetFormattedId(string objectType, Guid id)
        {
            return $"{objectType}.{id}";
        }
    }
}

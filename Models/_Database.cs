using System;
using MongoDB.Bson;
using MongoDB.Driver;

namespace cms_api.Models
{
    public class Database
    {
        public string _mongoConnection { get; set; }
        public string _databaseName { get; set; }

        public Database()
        {
            this._mongoConnection = "mongodb://127.0.0.1:27017";
            //this._mongoConnection = "mongodb://202.139.196.4";
            //this._databaseName = "khub_dee_prod";
            this._databaseName = "dsd_e_prod";
        }

        public IMongoCollection<BsonDocument> MongoClient(string collection)
        {
            var dbClient = new MongoClient(this._mongoConnection);
            var db = dbClient.GetDatabase(this._databaseName);
            return db.GetCollection<BsonDocument>(collection);
        }

        public IMongoCollection<T> MongoClient<T>(string collection)
        {
            var dbClient = new MongoClient(this._mongoConnection);
            var db = dbClient.GetDatabase(this._databaseName);
            return db.GetCollection<T>(collection);
        }

        public IMongoCollection<BsonDocument> MongoClient(string database, string collection)
        {
            var dbClient = new MongoClient(this._mongoConnection);
            var db = dbClient.GetDatabase(_databaseName + "_" + database);
            return db.GetCollection<BsonDocument>(collection);
        }

        public IMongoCollection<T> MongoClient<T>(string database, string collection)
        {
            var dbClient = new MongoClient(this._mongoConnection);
            var db = dbClient.GetDatabase(database);
            return db.GetCollection<T>(collection);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;

namespace DbreezeAdapter.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            DBreezeSchema schema = new DBreezeSchema()
            {
                CollectionName = "Entity",
                PrimaryColumnName = "Id",
                SecondaryIndexColumnNameCollection = new List<string>()
                {
                    "Name",
                    "Age",
                    "Gender"
                }
            };

            // clean the existing db
            if (Directory.Exists(@"d:\dbreeze"))
            {
                Directory.Delete(@"d:\dbreeze", true);
            }

            string collectionName = "Entity";

            using (DBreezeAdaptor dbAdaptor = new DBreezeAdaptor(@"d:\dbreeze\"))
            {
                dbAdaptor.AddSchema(schema);

                Entity entity1 = new Entity()
                {
                    Id = 1,
                    Name = "Lokesh",
                    Address = "Address 1",
                    Age = 30,
                    Gender = "Male"
                };
                Entity entity2 = new Entity()
                {
                    Id = 2,
                    Name = "Lucky",
                    Address = "Address 2",
                    Age = 31,
                    Gender = "Male"
                };
                Entity entity3 = new Entity()
                {
                    Id = 3,
                    Name = "Anna",
                    Address = "Address 3",
                    Age = 25,
                    Gender = "Female"
                };
                Entity entity4 = new Entity()
                {
                    Id = 4,
                    Name = "Anna",
                    Address = "Address 1",
                    Age = 30,
                    Gender = "Male"
                };

                dbAdaptor.InsertRecord<Entity>(collectionName, entity1);
                dbAdaptor.InsertRecord<Entity>(collectionName, entity2);
                dbAdaptor.InsertRecord<Entity>(collectionName, entity3);
                dbAdaptor.InsertRecord<Entity>(collectionName, entity4);

                Query query = Query.Eq("Gender", "Male");
                var result = dbAdaptor.Get<Entity>(collectionName, query);
                Console.WriteLine(result.Count == 3);

                query = Query.And(
                    Query.Eq("Gender", "Male"),
                    Query.Eq("Name", "Lucky"));
                result = dbAdaptor.Get<Entity>(collectionName, query);
                Console.WriteLine(result.Count == 1);

                query = Query.Or(
                    Query.StarstWith("Name", "An"),
                    Query.Or(
                        Query.Eq("Gender", "Male"),
                        Query.Eq("Name", "Lucky")));
                result = dbAdaptor.Get<Entity>(collectionName, query);
                Console.WriteLine(result.Count == 4);

                query = Query.Or(
                    Query.StarstWith("Name", "Wrong Name"),
                    Query.Or(
                        Query.Eq("Gender", "Male"),
                        Query.Or(
                            Query.Eq("Name", "Lucky"),
                            Query.Or(
                                Query.Eq("Gender", "Female"),
                                Query.Eq("Age", "30")))));
                result = dbAdaptor.Get<Entity>(collectionName, query);
                Console.WriteLine(result.Count == 4);

                Console.ReadLine();

            }
        }
    }
}

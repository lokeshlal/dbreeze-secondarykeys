using DBreeze;
using DBreeze.DataTypes;
using DBreeze.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DbreezeAdapter
{
    public class DBreezeAdaptor : IDisposable
    {
        #region private fields
        static DBreezeEngine engine = null;
        static List<DBreezeSchema> DBreezeSchemas = null;
        const string separator = "~!";
        bool isEngineRunning = true;
        #endregion

        #region constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="databaseFolderName">database folder name</param>
        public DBreezeAdaptor(string databaseFolderName)
        {
            if (DBreezeSchemas == null)
            {
                DBreezeSchemas = new List<DBreezeSchema>();
            }
            if (engine == null)
            {
                engine = new DBreezeEngine(new DBreezeConfiguration()
                {
                    DBreezeDataFolderName = databaseFolderName
                });

                CustomSerializator.Serializator = (object o) => JsonConvert.SerializeObject(o);
                CustomSerializator.Deserializator = (string o, Type type) => JsonConvert.DeserializeObject(o, type);
            }
        }
        #endregion

        #region public methods
        /// <summary>
        /// add schema of a table in dbreeze engine
        /// </summary>
        /// <param name="schema">schema definition</param>
        public void AddSchema(DBreezeSchema schema)
        {
            if (DBreezeSchemas.Where(s => s.CollectionName == schema.CollectionName).Any())
            {
                DBreezeSchemas.Remove(DBreezeSchemas.Where(s => s.CollectionName == schema.CollectionName).FirstOrDefault());
            }
            DBreezeSchemas.Add(schema);
        }


        /// <summary>
        /// Inserts a record in table
        /// </summary>
        /// <param name="collectionName">Collection name</param>
        /// <param name="primaryKeyColumnName">primary column name</param>
        /// <param name="secondryIndexColumnNameCollection">secondry index column collection name</param>
        /// <param name="data">JSON object</param>
        public void InsertRecord<T>(string collectionName, T data)
        {
            string primaryKeyColumnName = null;
            List<string> secondaryIndexColumnNameCollection = null;

            PopulateKeyInfomationFromSchema(collectionName, ref primaryKeyColumnName, ref secondaryIndexColumnNameCollection);
            //solution can be build using nested tables as well...but might increase the complexity during search
            List<string> lockTables = GetLockTables(collectionName, secondaryIndexColumnNameCollection);


            // start the transaction
            using (var tran = engine.GetTransaction())
            {
                // acquire a lock, on main table and secondry index table as well
                tran.SynchronizeTables(lockTables.ToArray());

                Type primaryPropertyType = data.GetType().GetProperty(primaryKeyColumnName).PropertyType;
                object primaryKey = data.GetType().GetProperty(primaryKeyColumnName).GetValue(data);

                bool wasUpdated = false;
                byte[] refPtr;

                var genericData = new DbCustomSerializer<T>(data);

                var parameters = new object[] { collectionName, primaryKey, genericData, null, null };
                MethodInfo insertMethodGeneric = Helper.CreateGenericInsertMethod<T>(tran, primaryPropertyType);
                insertMethodGeneric.Invoke(tran, parameters);
                refPtr = (byte[])parameters[3];
                wasUpdated = (bool)parameters[4];

                if (wasUpdated)
                {
                    // delete the value from secondary tables
                    secondaryIndexColumnNameCollection.ForEach(indexCol =>
                    {
                        string secondaryIndexTableName = string.Format("{0}{1}", collectionName, indexCol);
                        Type secondaryPropertyType = data.GetType().GetProperty(indexCol).PropertyType;
                        object secondaryIndexColumnValue = data.GetType().GetProperty(indexCol).GetValue(data);
                        string secondaryIndexTableKey = string.Format("{0}{1}{2}", secondaryIndexColumnValue, separator, primaryKey);
                        // always keep key in all lower case
                        secondaryIndexTableKey = secondaryIndexTableKey.ToLower();

                        tran.RemoveKey(secondaryIndexTableName, secondaryIndexTableKey);
                    });
                }

                secondaryIndexColumnNameCollection.ForEach(indexCol =>
                {
                    string secondaryIndexTableName = string.Format("{0}{1}", collectionName, indexCol);

                    Type secondaryPropertyType = data.GetType().GetProperty(indexCol).PropertyType;
                    object secondaryIndexColumnValue = data.GetType().GetProperty(indexCol).GetValue(data);
                    string secondaryIndexTableKey = string.Format("{0}{1}{2}", secondaryIndexColumnValue, separator, primaryKey);
                    // always keep key in all lower case
                    secondaryIndexTableKey = secondaryIndexTableKey.ToLower();
                    tran.Insert(secondaryIndexTableName, secondaryIndexTableKey, refPtr);
                });
                tran.Commit();
            }
        }



        /// <summary>
        /// deletes a record from the collection
        /// </summary>
        /// <param name="collectionName">collection name</param>
        /// <param name="primaryKey">primary key value</param>
        /// <param name="primaryKeyColumnName">primary key column name</param>
        /// <param name="secondaryIndexColumnNameCollection">secondary keys column collection</param>
        public void DeleteRecord<O, T>(string collectionName, T primaryKey)
        {
            string primaryKeyColumnName = null;
            List<string> secondaryIndexColumnNameCollection = null;

            PopulateKeyInfomationFromSchema(collectionName, ref primaryKeyColumnName, ref secondaryIndexColumnNameCollection);
            List<string> lockTables = GetLockTables(collectionName, secondaryIndexColumnNameCollection);

            // start the transaction
            using (var tran = engine.GetTransaction())
            {
                // acquire a lock, on main table and secondry index table as well
                tran.SynchronizeTables(lockTables.ToArray());

                var data = tran.Select<T, DbCustomSerializer<O>>(collectionName, primaryKey).Value.Get;

                // insert in primary table and get the data reference pointer
                tran.RemoveKey(collectionName, primaryKey);

                // delete the value from secondary tables
                secondaryIndexColumnNameCollection.ForEach(indexCol =>
                {
                    string secondaryIndexTableName = string.Format("{0}{1}", collectionName, indexCol);

                    Type secondaryPropertyType = data.GetType().GetProperty(indexCol).PropertyType;
                    object secondaryIndexColumnValue = data.GetType().GetProperty(indexCol).GetValue(data);
                    string secondaryIndexTableKey = string.Format("{0}{1}{2}", secondaryIndexColumnValue, separator, primaryKey);
                    // always keep key in all lower case
                    secondaryIndexTableKey = secondaryIndexTableKey.ToLower();
                    tran.RemoveKey(secondaryIndexTableName, secondaryIndexTableKey);
                });
                tran.Commit();
            }
        }

        /// <summary>
        /// Search the records in the collection
        /// </summary>
        /// <param name="collectionName">collection name</param>
        /// <param name="filters">filters</param>
        /// <param name="recordsPerPage">records per page</param>
        /// <param name="pageNumber">page number</param>
        /// <param name="primaryKeyColumnName">primary key column name</param>
        /// <param name="secondaryIndexColumnNameCollection">secondary indexes collection</param>
        /// <returns>JObject collection</returns>
        public List<T> Get<T>(string collectionName, Query query, int recordsPerPage = 10, int pageNumber = 0)
        {
            List<T> returnValue = new List<T>();
            string primaryKeyColumnName = null;
            List<string> secondaryIndexColumnNameCollection = null;

            PopulateKeyInfomationFromSchema(collectionName, ref primaryKeyColumnName, ref secondaryIndexColumnNameCollection);
            List<string> lockTables = GetLockTables(collectionName, secondaryIndexColumnNameCollection);

            using (var tran = engine.GetTransaction())
            {
                // acquire a lock, on main table and secondry index table as well
                tran.SynchronizeTables(lockTables.ToArray());
                returnValue = QueryExecutor.Run<T>(query, tran, collectionName, primaryKeyColumnName, secondaryIndexColumnNameCollection, null);
            }
            return returnValue.Skip(recordsPerPage * pageNumber).Take(recordsPerPage).ToList();
        }
        #endregion

        #region query support methods
        #endregion

        #region private methods
        /// <summary>
        /// get the primary and secondary table info based on schema present
        /// </summary>
        /// <param name="collectionName">collection name</param>
        /// <param name="primaryKeyColumnName">primary key column name</param>
        /// <param name="secondaryIndexColumnNameCollection">secondary index column name collection</param>
        private static void PopulateKeyInfomationFromSchema(string collectionName, ref string primaryKeyColumnName, ref List<string> secondaryIndexColumnNameCollection)
        {
            if (string.IsNullOrEmpty(primaryKeyColumnName))
            {
                if (!DBreezeSchemas.Where(s => s.CollectionName == collectionName).Any())
                {
                    throw new Exception("schema not found");
                }
                primaryKeyColumnName = DBreezeSchemas.Where(s => s.CollectionName == collectionName).FirstOrDefault().PrimaryColumnName;
                secondaryIndexColumnNameCollection = DBreezeSchemas.Where(s => s.CollectionName == collectionName).FirstOrDefault().SecondaryIndexColumnNameCollection;
            }
        }

        /// <summary>
        /// get the tables to be locked for transaction, including secondary index tables
        /// </summary>
        /// <param name="collectionName">collection name</param>
        /// <param name="secondaryIndexColumnNameCollection">secondary index column name collection</param>
        /// <returns></returns>
        private static List<string> GetLockTables(string collectionName, List<string> secondaryIndexColumnNameCollection)
        {
            List<string> secondaryTableNames = secondaryIndexColumnNameCollection
             .Select(indexCol => string.Format("{0}{1}", collectionName, indexCol)).ToList();

            List<string> lockTables = new List<string> { collectionName };
            lockTables.AddRange(secondaryTableNames);
            return lockTables;
        }
        #endregion
        public void Dispose()
        {
            isEngineRunning = false;
            engine.Dispose();
        }
    }
}

using DBreeze.DataTypes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using DBreeze.Transactions;
using System.Reflection;

namespace DbreezeAdapter
{
    internal class QueryExecutor
    {
        static string separator = "~!";
        public static List<T> Run<T>(Query query, Transaction transaction, string collectionName, string primaryKey, List<string> secondaryKeys, List<T> objects = null)
        {
            string field = "";
            DbreezeValue value = null;
            switch (query.FilterType)
            {
                case FilterType.And:
                    QueryAnd queryAnd = (QueryAnd)query;

                    List<T> objectsAndLeft = Run<T>(queryAnd.left, transaction, collectionName, primaryKey, secondaryKeys, objects);
                    List<T> objectsAndRight = Run<T>(queryAnd.right, transaction, collectionName, primaryKey, secondaryKeys, objectsAndLeft);
                    objects = objectsAndRight;

                    break;

                case FilterType.Or:
                    QueryOr queryOr = (QueryOr)query;

                    List<T> objectsOrLeft = Run<T>(queryOr.left, transaction, collectionName, primaryKey, secondaryKeys, null);
                    List<T> objectsOrRight = Run<T>(queryOr.right, transaction, collectionName, primaryKey, secondaryKeys, null);

                    // merge all records
                    List<T> objects_new_or = new List<T>();
                    Dictionary<int, bool> hashmapOr = new Dictionary<int, bool>();
                    foreach (var obj in objectsOrLeft)
                    {
                        objects_new_or.Add(obj);
                        Type primaryPropertyTypeOr = obj.GetType().GetProperty(primaryKey).PropertyType;
                        object primaryKeyValueOr = obj.GetType().GetProperty(primaryKey).GetValue(obj);
                        hashmapOr.Add(primaryKeyValueOr.GetHashCode(), true);
                    }

                    foreach (var obj in objectsOrRight)
                    {
                        Type primaryPropertyTypeOr = obj.GetType().GetProperty(primaryKey).PropertyType;
                        object primaryKeyValueOr = obj.GetType().GetProperty(primaryKey).GetValue(obj);
                        if (!hashmapOr.ContainsKey(primaryKeyValueOr.GetHashCode()))
                        {
                            objects_new_or.Add(obj);
                            hashmapOr.Add(primaryKeyValueOr.GetHashCode(), true);
                        }
                    }
                    if (objects != null)
                    {
                        foreach (var obj in objects)
                        {
                            Type primaryPropertyTypeOr = obj.GetType().GetProperty(primaryKey).PropertyType;
                            object primaryKeyValueOr = obj.GetType().GetProperty(primaryKey).GetValue(obj);
                            if (!hashmapOr.ContainsKey(primaryKeyValueOr.GetHashCode()))
                            {
                                objects_new_or.Add(obj);
                                hashmapOr.Add(primaryKeyValueOr.GetHashCode(), true);
                            }
                        }
                    }
                    objects = objects_new_or;
                    break;
                case FilterType.Eq:
                    field = ((QueryEq)query).Field;
                    value = ((QueryEq)query).Value;

                    if (objects == null)
                    {
                        objects = new List<T>();
                        if (field == primaryKey)
                        {
                            Type primaryPropertyType = typeof(T).GetProperty(primaryKey).PropertyType;
                            MethodInfo selectMethodGeneric = Helper.CreateGenericSelectMethod<T>(transaction, primaryPropertyType);
                            var resultObject = selectMethodGeneric.Invoke(transaction, new object[] { collectionName, GetValue(value) });
                            object getValue = Helper.GetSelectResultValue(resultObject);
                            var result = (T)getValue;
                            objects.Add(result);
                        }
                        else
                        {
                            string secondaryIndexTableName = string.Format("{0}{1}", collectionName, field);
                            Type secondaryPropertyType = typeof(T).GetProperty(field).PropertyType;
                            string secondaryIndexTableKeyValue = string.Format("{0}{1}", GetValue(value), separator).ToLower();

                            IEnumerable<Row<string, byte[]>> secondaryIndexValueCollection = transaction.SelectForwardStartsWith<string, byte[]>(secondaryIndexTableName, secondaryIndexTableKeyValue);

                            Type primaryPropertyType = typeof(T).GetProperty(primaryKey).PropertyType;
                            MethodInfo selectDirectMethodGeneric = Helper.CreateGenericSelectDirectMethod<T>(transaction, primaryPropertyType);

                            foreach (var secondaryIndexValue in secondaryIndexValueCollection)
                            {
                                var resultObject = selectDirectMethodGeneric.Invoke(transaction, new object[] { collectionName, secondaryIndexValue.Value }); //.Value.Get);
                                object getValue = Helper.GetSelectResultValue(resultObject);
                                var result = (T)getValue;
                                objects.Add(result);
                            }
                        }
                    }
                    else
                    {
                        List<T> objects_new = new List<T>();
                        foreach (var @object in objects)
                        {
                            if (EqCheck(@object.GetType().GetProperty(field).GetValue(@object), value))
                            {
                                objects_new.Add(@object);
                            }
                        }
                        objects = objects_new;
                    }

                    break;
                case FilterType.StartsWith:
                    field = ((QueryStartsWith)query).Field;
                    value = ((QueryStartsWith)query).Value;
                    string strValue = value.AsString;

                    if (objects == null)
                    {
                        objects = new List<T>();
                        if (field == primaryKey)
                        {

                            IEnumerable<Row<string, T>> filterdRecords = transaction.SelectForwardStartsWith<string, T>(collectionName, strValue);
                            foreach (var filterdRecord in filterdRecords)
                            {
                                objects.Add(filterdRecord.Value);
                            }
                        }
                        else
                        {
                            string secondaryIndexTableName = string.Format("{0}{1}", collectionName, field);
                            Type secondaryPropertyType = typeof(T).GetProperty(field).PropertyType;
                            string secondaryIndexTableKeyValue = string.Format("{0}", strValue).ToLower();

                            IEnumerable<Row<string, byte[]>> secondaryIndexValueCollection = transaction.SelectForwardStartsWith<string, byte[]>(secondaryIndexTableName, secondaryIndexTableKeyValue);

                            Type primaryPropertyType = typeof(T).GetProperty(primaryKey).PropertyType;
                            MethodInfo selectDirectMethodGeneric = Helper.CreateGenericSelectDirectMethod<T>(transaction, primaryPropertyType);

                            foreach (var secondaryIndexValue in secondaryIndexValueCollection)
                            {
                                var resultObject = selectDirectMethodGeneric.Invoke(transaction, new object[] { collectionName, secondaryIndexValue.Value }); //.Value.Get);
                                object getValue = Helper.GetSelectResultValue(resultObject);
                                var result = (T)getValue;
                                objects.Add(result);
                            }
                        }
                    }
                    else
                    {
                        List<T> objects_new = new List<T>();
                        foreach (var @object in objects)
                        {
                            if (EqCheck(@object.GetType().GetProperty(field).GetValue(@object), value))
                            {
                                objects_new.Add(@object);
                            }
                        }
                        objects = objects_new;
                    }

                    break;
            }
            return objects;
        }

        private static object GetValue(DbreezeValue value)
        {
            if (value.IsBoolean)
            {
                return value.AsBoolean;
            }
            else if (value.IsDateTime)
            {
                return value.AsDateTime;
            }
            else if (value.IsDecimal)
            {
                return value.AsDecimal;
            }
            else if (value.IsDouble)
            {
                return value.AsDouble;
            }
            else if (value.IsGuid)
            {
                return value.AsGuid;
            }
            else if (value.IsInt32)
            {
                return value.AsInt32;
            }
            else if (value.IsInt64)
            {
                return value.AsInt64;
            }
            else if (value.IsNumber)
            {
                return value.AsDecimal;
            }
            else if (value.IsString)
            {
                return value.AsString;
            }
            return null;
        }

        private static bool EqCheck(object objectValue, DbreezeValue value)
        {
            bool returnValue = false;
            if (value.IsBoolean && value.AsBoolean == (bool)objectValue)
            {
                returnValue = true;
            }
            else if (value.IsDateTime && value.AsDateTime == (DateTime)objectValue)
            {
                returnValue = true;
            }
            else if (value.IsDecimal && value.AsDecimal == (decimal)objectValue)
            {
                returnValue = true;
            }
            else if (value.IsDouble && value.AsDouble == (double)objectValue)
            {
                returnValue = true;
            }
            else if (value.IsGuid && value.AsGuid == (Guid)objectValue)
            {
                returnValue = true;
            }
            else if (value.IsInt32 && value.AsInt32 == (int)objectValue)
            {
                returnValue = true;
            }
            else if (value.IsInt64 && value.AsInt64 == (long)objectValue)
            {
                returnValue = true;
            }
            else if (value.IsNumber && value.AsDecimal == (decimal)objectValue)
            {
                returnValue = true;
            }
            else if (value.IsString && value.AsString == (string)objectValue)
            {
                returnValue = true;
            }
            return returnValue;
        }
        private static bool StartsWithCheck(object objectValue, string value)
        {
            bool returnValue = false;
            if (value.StartsWith((string)objectValue, StringComparison.InvariantCultureIgnoreCase))
            {
                returnValue = true;
            }
            return returnValue;
        }
    }
}

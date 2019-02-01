using DBreeze.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DbreezeAdapter
{
    internal class Helper
    {
        public static MethodInfo CreateGenericInsertMethod<T>(DBreeze.Transactions.Transaction tran, Type primaryPropertyType)
        {
            MethodInfo insertMethod = tran.GetType().GetMethods().Where(m => m.Name == "Insert" && m.GetParameters().Length == 5).First();
            Type valyeType = typeof(DbCustomSerializer<>).MakeGenericType(typeof(T));
            MethodInfo insertMethodGeneric = insertMethod.MakeGenericMethod(primaryPropertyType, valyeType);
            return insertMethodGeneric;
        }

        public static object GetSelectResultValue(object resultObjects)
        {
            var valueObj = resultObjects.GetType().GetProperty("Value").GetValue(resultObjects);
            var getValue = valueObj.GetType().GetProperty("Get").GetValue(valueObj);
            return getValue;
        }
        public static MethodInfo CreateGenericSelectMethod<T>(DBreeze.Transactions.Transaction tran, Type primaryPropertyType)
        {
            MethodInfo insertMethod = tran.GetType().GetMethods().Where(m => m.Name == "Select" && m.GetParameters().Length == 2).First();
            Type valyeType = typeof(DbCustomSerializer<>).MakeGenericType(typeof(T));
            MethodInfo selectMethodGeneric = insertMethod.MakeGenericMethod(primaryPropertyType, valyeType);
            return selectMethodGeneric;
        }

        public static MethodInfo CreateGenericSelectDirectMethod<T>(DBreeze.Transactions.Transaction tran, Type primaryPropertyType)
        {
            MethodInfo insertMethod = tran.GetType().GetMethods().Where(m => m.Name == "SelectDirect" && m.GetParameters().Length == 2).First();
            Type valyeType = typeof(DbCustomSerializer<>).MakeGenericType(typeof(T));
            MethodInfo selectMethodGeneric = insertMethod.MakeGenericMethod(primaryPropertyType, valyeType);
            return selectMethodGeneric;
        }

    }
}

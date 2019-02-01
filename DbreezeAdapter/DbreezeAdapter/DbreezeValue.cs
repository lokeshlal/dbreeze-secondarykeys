using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DbreezeAdapter
{
    /// <summary>
    /// Represent a Dbreeze Value
    /// </summary>
    public class DbreezeValue : IComparable<DbreezeValue>, IEquatable<DbreezeValue>
    {
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Represent a Null dbreeze type
        /// </summary>
        public static readonly DbreezeValue Null = new DbreezeValue();

        /// <summary>
        /// Represent a MinValue dbreeze type
        /// </summary>
        public static readonly DbreezeValue MinValue = new DbreezeValue { Type = DbreezeType.MinValue, RawValue = "-oo" };

        /// <summary>
        /// Represent a MaxValue dbreeze type
        /// </summary>
        public static readonly DbreezeValue MaxValue = new DbreezeValue { Type = DbreezeType.MaxValue, RawValue = "+oo" };

        /// <summary>
        /// Indicate dbreezeType of this dbreezeValue
        /// </summary>
        public DbreezeType Type { get; private set; }

        /// <summary>
        /// Get internal .NET value object
        /// </summary>
        public virtual object RawValue { get; private set; }

        
        #region Constructor

        public DbreezeValue()
        {
            this.Type = DbreezeType.Null;
            this.RawValue = null;
        }

        public DbreezeValue(Int32 value)
        {
            this.Type = DbreezeType.Int32;
            this.RawValue = value;
        }

        public DbreezeValue(Int64 value)
        {
            this.Type = DbreezeType.Int64;
            this.RawValue = value;
        }

        public DbreezeValue(Double value)
        {
            this.Type = DbreezeType.Double;
            this.RawValue = value;
        }

        public DbreezeValue(Decimal value)
        {
            this.Type = DbreezeType.Decimal;
            this.RawValue = value;
        }

        public DbreezeValue(String value)
        {
            this.Type = value == null ? DbreezeType.Null : DbreezeType.String;
            this.RawValue = value;
        }

        public DbreezeValue(Guid value)
        {
            this.Type = DbreezeType.Guid;
            this.RawValue = value;
        }

        public DbreezeValue(Boolean value)
        {
            this.Type = DbreezeType.Boolean;
            this.RawValue = value;
        }

        public DbreezeValue(DateTime value)
        {
            this.Type = DbreezeType.DateTime;
            this.RawValue = value;
        }

        public DbreezeValue(DbreezeValue value)
        {
            this.Type = value == null ? DbreezeType.Null : value.Type;
            this.RawValue = value.RawValue;
        }

        public DbreezeValue(object value)
        {
            this.RawValue = value;

            if (value == null) this.Type = DbreezeType.Null;
            else if (value is Int32) this.Type = DbreezeType.Int32;
            else if (value is Int64) this.Type = DbreezeType.Int64;
            else if (value is Double) this.Type = DbreezeType.Double;
            else if (value is Decimal) this.Type = DbreezeType.Decimal;
            else if (value is String) this.Type = DbreezeType.String;
            else if (value is Guid) this.Type = DbreezeType.Guid;
            else if (value is Boolean) this.Type = DbreezeType.Boolean;
            else if (value is DateTime)
            {
                this.Type = DbreezeType.DateTime;
                this.RawValue = ((DateTime)value);
            }
            else if (value is DbreezeValue)
            {
                var v = (DbreezeValue)value;
                this.Type = v.Type;
                this.RawValue = v.RawValue;
            }
        }

        #endregion

        #region Convert types

        public bool AsBoolean
        {
            get { return this.Type == DbreezeType.Boolean ? (Boolean)this.RawValue : default(Boolean); }
        }

        public string AsString
        {
            get { return this.Type != DbreezeType.Null ? this.RawValue.ToString() : default(String); }
        }

        public int AsInt32
        {
            get { return this.IsNumber ? Convert.ToInt32(this.RawValue) : default(Int32); }
        }

        public long AsInt64
        {
            get { return this.IsNumber ? Convert.ToInt64(this.RawValue) : default(Int64); }
        }

        public double AsDouble
        {
            get { return this.IsNumber ? Convert.ToDouble(this.RawValue) : default(Double); }
        }

        public decimal AsDecimal
        {
            get { return this.IsNumber ? Convert.ToDecimal(this.RawValue) : default(Decimal); }
        }

        public DateTime AsDateTime
        {
            get { return this.Type == DbreezeType.DateTime ? (DateTime)this.RawValue : default(DateTime); }
        }

        public Guid AsGuid
        {
            get { return this.Type == DbreezeType.Guid ? (Guid)this.RawValue : default(Guid); }
        }

        #endregion

        #region IsTypes

        public bool IsNull
        {
            get { return this.Type == DbreezeType.Null; }
        }

        public bool IsInt32
        {
            get { return this.Type == DbreezeType.Int32; }
        }

        public bool IsInt64
        {
            get { return this.Type == DbreezeType.Int64; }
        }

        public bool IsDouble
        {
            get { return this.Type == DbreezeType.Double; }
        }

        public bool IsDecimal
        {
            get { return this.Type == DbreezeType.Decimal; }
        }

        public bool IsNumber
        {
            get { return this.IsInt32 || this.IsInt64 || this.IsDouble || this.IsDecimal; }
        }

        public bool IsBoolean
        {
            get { return this.Type == DbreezeType.Boolean; }
        }

        public bool IsString
        {
            get { return this.Type == DbreezeType.String; }
        }

        public bool IsGuid
        {
            get { return this.Type == DbreezeType.Guid; }
        }

        public bool IsDateTime
        {
            get { return this.Type == DbreezeType.DateTime; }
        }

        public bool IsMinValue
        {
            get { return this.Type == DbreezeType.MinValue; }
        }

        public bool IsMaxValue
        {
            get { return this.Type == DbreezeType.MaxValue; }
        }

        #endregion

        #region Implicit Ctor

        // Int32
        public static implicit operator Int32(DbreezeValue value)
        {
            return (Int32)value.RawValue;
        }

        // Int32
        public static implicit operator DbreezeValue(Int32 value)
        {
            return new DbreezeValue(value);
        }

        // Int64
        public static implicit operator Int64(DbreezeValue value)
        {
            return (Int64)value.RawValue;
        }

        // Int64
        public static implicit operator DbreezeValue(Int64 value)
        {
            return new DbreezeValue(value);
        }

        // Double
        public static implicit operator Double(DbreezeValue value)
        {
            return (Double)value.RawValue;
        }

        // Double
        public static implicit operator DbreezeValue(Double value)
        {
            return new DbreezeValue(value);
        }

        // Decimal
        public static implicit operator Decimal(DbreezeValue value)
        {
            return (Decimal)value.RawValue;
        }

        // Decimal
        public static implicit operator DbreezeValue(Decimal value)
        {
            return new DbreezeValue(value);
        }

        // UInt64 (to avoid ambigous between Double-Decimal)
        public static implicit operator UInt64(DbreezeValue value)
        {
            return (UInt64)value.RawValue;
        }

        // Decimal
        public static implicit operator DbreezeValue(UInt64 value)
        {
            return new DbreezeValue((Double)value);
        }

        // String
        public static implicit operator String(DbreezeValue value)
        {
            return (String)value.RawValue;
        }

        // String
        public static implicit operator DbreezeValue(String value)
        {
            return new DbreezeValue(value);
        }

        // Document
        public static implicit operator Dictionary<string, DbreezeValue>(DbreezeValue value)
        {
            return (Dictionary<string, DbreezeValue>)value.RawValue;
        }

        // Document
        public static implicit operator DbreezeValue(Dictionary<string, DbreezeValue> value)
        {
            return new DbreezeValue(value);
        }

        // Array
        public static implicit operator List<DbreezeValue>(DbreezeValue value)
        {
            return (List<DbreezeValue>)value.RawValue;
        }

        // Array
        public static implicit operator DbreezeValue(List<DbreezeValue> value)
        {
            return new DbreezeValue(value);
        }

        // Binary
        public static implicit operator Byte[] (DbreezeValue value)
        {
            return (Byte[])value.RawValue;
        }

        // Binary
        public static implicit operator DbreezeValue(Byte[] value)
        {
            return new DbreezeValue(value);
        }

        // Guid
        public static implicit operator Guid(DbreezeValue value)
        {
            return (Guid)value.RawValue;
        }

        // Guid
        public static implicit operator DbreezeValue(Guid value)
        {
            return new DbreezeValue(value);
        }

        // Boolean
        public static implicit operator Boolean(DbreezeValue value)
        {
            return (Boolean)value.RawValue;
        }

        // Boolean
        public static implicit operator DbreezeValue(Boolean value)
        {
            return new DbreezeValue(value);
        }

        // DateTime
        public static implicit operator DateTime(DbreezeValue value)
        {
            return (DateTime)value.RawValue;
        }

        // DateTime
        public static implicit operator DbreezeValue(DateTime value)
        {
            return new DbreezeValue(value);
        }

        // +
        public static DbreezeValue operator +(DbreezeValue left, DbreezeValue right)
        {
            if (!left.IsNumber || !right.IsNumber) return DbreezeValue.Null;

            if (left.IsInt32 && right.IsInt32) return left.AsInt32 + right.AsInt32;
            if (left.IsInt64 && right.IsInt64) return left.AsInt64 + right.AsInt64;
            if (left.IsDouble && right.IsDouble) return left.AsDouble + right.AsDouble;
            if (left.IsDecimal && right.IsDecimal) return left.AsDecimal + right.AsDecimal;

            var result = left.AsDecimal + right.AsDecimal;
            var type = (DbreezeType)Math.Max((int)left.Type, (int)right.Type);

            return
                type == DbreezeType.Int64 ? new DbreezeValue((Int64)result) :
                type == DbreezeType.Double ? new DbreezeValue((Double)result) :
                new DbreezeValue(result);
        }

        // -
        public static DbreezeValue operator -(DbreezeValue left, DbreezeValue right)
        {
            if (!left.IsNumber || !right.IsNumber) return DbreezeValue.Null;

            if (left.IsInt32 && right.IsInt32) return left.AsInt32 - right.AsInt32;
            if (left.IsInt64 && right.IsInt64) return left.AsInt64 - right.AsInt64;
            if (left.IsDouble && right.IsDouble) return left.AsDouble - right.AsDouble;
            if (left.IsDecimal && right.IsDecimal) return left.AsDecimal - right.AsDecimal;

            var result = left.AsDecimal - right.AsDecimal;
            var type = (DbreezeType)Math.Max((int)left.Type, (int)right.Type);

            return
                type == DbreezeType.Int64 ? new DbreezeValue((Int64)result) :
                type == DbreezeType.Double ? new DbreezeValue((Double)result) :
                new DbreezeValue(result);
        }

        // *
        public static DbreezeValue operator *(DbreezeValue left, DbreezeValue right)
        {
            if (!left.IsNumber || !right.IsNumber) return DbreezeValue.Null;

            if (left.IsInt32 && right.IsInt32) return left.AsInt32 * right.AsInt32;
            if (left.IsInt64 && right.IsInt64) return left.AsInt64 * right.AsInt64;
            if (left.IsDouble && right.IsDouble) return left.AsDouble * right.AsDouble;
            if (left.IsDecimal && right.IsDecimal) return left.AsDecimal * right.AsDecimal;

            var result = left.AsDecimal * right.AsDecimal;
            var type = (DbreezeType)Math.Max((int)left.Type, (int)right.Type);

            return
                type == DbreezeType.Int64 ? new DbreezeValue((Int64)result) :
                type == DbreezeType.Double ? new DbreezeValue((Double)result) :
                new DbreezeValue(result);
        }

        // /
        public static DbreezeValue operator /(DbreezeValue left, DbreezeValue right)
        {
            if (!left.IsNumber || !right.IsNumber) return DbreezeValue.Null;
            if (left.IsDecimal || right.IsDecimal) return left.AsDecimal / right.AsDecimal;

            return left.AsDouble / right.AsDouble;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        #endregion

        #region IComparable<dbreezeValue>, IEquatable<dbreezeValue>

        public virtual int CompareTo(DbreezeValue other)
        {
            // first, test if types are different
            if (this.Type != other.Type)
            {
                // if both values are number, convert them to Decimal (128 bits) to compare
                // it's the slowest way, but more secure
                if (this.IsNumber && other.IsNumber)
                {
                    return Convert.ToDecimal(this.RawValue).CompareTo(Convert.ToDecimal(other.RawValue));
                }
                // if not, order by sort type order
                else
                {
                    return this.Type.CompareTo(other.Type);
                }
            }

            // for both values with same data type just compare
            switch (this.Type)
            {
                case DbreezeType.Null:
                case DbreezeType.MinValue:
                case DbreezeType.MaxValue:
                    return 0;

                case DbreezeType.Int32: return ((Int32)this.RawValue).CompareTo((Int32)other.RawValue);
                case DbreezeType.Int64: return ((Int64)this.RawValue).CompareTo((Int64)other.RawValue);
                case DbreezeType.Double: return ((Double)this.RawValue).CompareTo((Double)other.RawValue);
                case DbreezeType.Decimal: return ((Decimal)this.RawValue).CompareTo((Decimal)other.RawValue);
                case DbreezeType.String: return string.Compare((String)this.RawValue, (String)other.RawValue);
                case DbreezeType.Guid: return ((Guid)this.RawValue).CompareTo((Guid)other.RawValue);
                case DbreezeType.Boolean: return ((Boolean)this.RawValue).CompareTo((Boolean)other.RawValue);
                case DbreezeType.DateTime:
                    var d0 = (DateTime)this.RawValue;
                    var d1 = (DateTime)other.RawValue;
                    if (d0.Kind != DateTimeKind.Utc) d0 = d0.ToUniversalTime();
                    if (d1.Kind != DateTimeKind.Utc) d1 = d1.ToUniversalTime();
                    return d0.CompareTo(d1);

                default: throw new NotImplementedException();
            }
        }

        public bool Equals(DbreezeValue other)
        {
            return this.CompareTo(other) == 0;
        }

        #endregion

        #region Operators

        public static bool operator ==(DbreezeValue lhs, DbreezeValue rhs)
        {
            if (object.ReferenceEquals(lhs, null)) return object.ReferenceEquals(rhs, null);
            if (object.ReferenceEquals(rhs, null)) return false; // don't check type because sometimes different types can be ==

            return lhs.Equals(rhs);
        }

        public static bool operator !=(DbreezeValue lhs, DbreezeValue rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator >=(DbreezeValue lhs, DbreezeValue rhs)
        {
            return lhs.CompareTo(rhs) >= 0;
        }

        public static bool operator >(DbreezeValue lhs, DbreezeValue rhs)
        {
            return lhs.CompareTo(rhs) > 0;
        }

        public static bool operator <(DbreezeValue lhs, DbreezeValue rhs)
        {
            return lhs.CompareTo(rhs) < 0;
        }

        public static bool operator <=(DbreezeValue lhs, DbreezeValue rhs)
        {
            return lhs.CompareTo(rhs) <= 0;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(new DbreezeValue(obj));
        }

        public override int GetHashCode()
        {
            var hash = 17;
            hash = 37 * hash + this.Type.GetHashCode();
            hash = 37 * hash + this.RawValue.GetHashCode();
            return hash;
        }

        #endregion

        #region GetBytesCount

        internal int? Length = null;

        /// <summary>
        /// Returns how many bytes this dbreezeValue will use to persist in index writes
        /// </summary>
        public int GetBytesCount(bool recalc)
        {
            if (recalc == false && this.Length.HasValue) return this.Length.Value;

            switch (this.Type)
            {
                case DbreezeType.Null:
                case DbreezeType.MinValue:
                case DbreezeType.MaxValue:
                    this.Length = 0; break;

                case DbreezeType.Int32: this.Length = 4; break;
                case DbreezeType.Int64: this.Length = 8; break;
                case DbreezeType.Double: this.Length = 8; break;
                case DbreezeType.Decimal: this.Length = 16; break;

                case DbreezeType.String: this.Length = Encoding.UTF8.GetByteCount((string)this.RawValue); break;

                case DbreezeType.Guid: this.Length = 16; break;

                case DbreezeType.Boolean: this.Length = 1; break;
                case DbreezeType.DateTime: this.Length = 8; break;

                    // for Array/Document calculate from elements
            }

            return this.Length.Value;
        }

        private int GetBytesCountElement(string key, DbreezeValue value, bool recalc)
        {
            return
                1 + // element type
                Encoding.UTF8.GetByteCount(key) + // CString
                1 + // CString 0x00
                value.GetBytesCount(recalc) +
                (value.Type == DbreezeType.String || value.Type == DbreezeType.Guid ? 5 : 0); // bytes.Length + 0x??
        }

        #endregion
    }

}

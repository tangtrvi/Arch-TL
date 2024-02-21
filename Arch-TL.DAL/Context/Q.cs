using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Arch_TL.DAL.Attributes;
using Arch_TL.DAL.Common;
using Newtonsoft.Json.Linq;
using Arch_TL.DAL.Extensions;

namespace Arch_TL.DAL.Context;

public static class Q<T> where T : class
{
    public static string Key()
    {
        return DbCache.Key;
    }
    public static string Table()
    {
        return DbCache.TableName;
    }

    public static List<string> SearchCollumns()
    {
        return DbCache.ColumnsToSearch?.Select(x => x.ColumnName)?.ToList() ?? new List<string>();
    }

    public static Type ActiveColumnType()
    {
        if (DbCache.DisableColumn == null)
            return null;

        return DbCache.DisableColumn.Property.PropertyType;
    }

    public static Type DeleteColumnType()
    {
        if (DbCache.DeleteColumn == null)
            return null;

        return DbCache.DeleteColumn.Property.PropertyType;
    }

    public static string ActiveColumn(string alias)
    {
        if (DbCache.DisableColumn == null)
            return null;

        if (string.IsNullOrEmpty(alias))
            return DbCache.DisableColumn.ColumnName;

        return string.Join(".", DbCache.DisableColumn.ColumnName);
    }

    public static string DeleteColumn(string alias)
    {
        if (DbCache.DeleteColumn == null)
            return null;

        if (string.IsNullOrEmpty(alias))
            return DbCache.DeleteColumn.ColumnName;

        return string.Join(".", DbCache.DeleteColumn.ColumnName);
    }

    public static string Columns(string brief = null, List<string> skip = null)
    {
        var columns = skip != null
            ? DbCache.Columns.Where(column => !skip.Contains(column.Property.Name))
            : DbCache.Columns;

        return brief != null
            ? string.Join(",", columns.Select(column => $"{brief}.{column.ColumnName} as {column.Property.Name}"))
            : string.Join(",", columns.Select(column => $"{column.ColumnName} as {column.Property.Name}"));
    }

    public static string Column<TProperty>(Expression<Func<T, TProperty>> property)
    {
        var propertyInfo = property.GetPropertyInfo();

        return DbCache.Column(propertyInfo).ColumnName;
    }

    public static string GetByColumn(string columnName, string columnValue, string condition = "")
    {
        return $"SELECT {Columns()} FROM {Table()} WHERE {columnName} = '{columnValue}' {condition}";
    }
    public static string GetByColumn(string columnName, int columnValue, string condition = "")
    {
        return $"SELECT {Columns()} FROM {Table()} WHERE {columnName} = {columnValue} {condition}";
    }

    public static string GetById(int id, string condition = "")
    {
        return $"SELECT {Columns()} FROM {Table()} WHERE {Key()} = {id} {condition}";
    }

    public static string Delete(int id, string keyName = null, string condition = "")
    {
        return $"Delete from {Table()} WHERE {keyName ?? Key()} = {id} {condition}";
    }

    public static string Delete(List<int> ids, string keyName = null)
    {
        return $"Delete from {Table()} WHERE {keyName ?? Key()} in (" + string.Join(",", ids) + ")";
    }

    public static string UpdateColumnById(int id, string columnName, string columnValue, string keyName = null)
    {
        return $"UPDATE {Table()} SET {columnName} = '{columnValue}' WHERE {keyName ?? Key()} = {id}";
    }

    public static string UpdateColumnByIds(List<int> ids, string columnName, string columnValue, string keyName = null)
    {
        return $"UPDATE {Table()} SET {columnName} = '{columnValue}' WHERE {keyName ?? Key()} in ({string.Join(",", ids)})";
    }

    public static string UpdateColumnByIds(string ids, string columnName, string columnValue, string keyName = null)
    {
        return $"UPDATE {Table()} SET {columnName} = '{columnValue}' WHERE {keyName ?? Key()} in ({ids})";
    }

    public static string UpdateColumnById(int id, int modifiedBy, string columnName, string columnValue, string keyName = null)
    {
        return $"UPDATE {Table()} SET {columnName} = '{columnValue}', modified_by = {modifiedBy}, modified_date = '{DateTime.UtcNow}' WHERE {keyName ?? Key()} = {id}";
    }

    private static string GenerateInsertQuery(IReadOnlyCollection<ColumnMetadata> columnsToInsert)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendFormat("INSERT INTO {0}(", Table());

        using (var enumerator = columnsToInsert.GetEnumerator())
        {
            if (enumerator.MoveNext())
            {
                var current = enumerator.Current;

                stringBuilder.Append(current.ColumnName);

                while (enumerator.MoveNext())
                {
                    current = enumerator.Current;
                    stringBuilder.AppendFormat(", {0}", current.ColumnName);
                }
            }
        }

        stringBuilder.Append(") Values (");

        using (var enumerator = columnsToInsert.GetEnumerator())
        {
            if (enumerator.MoveNext())
            {
                var current = enumerator.Current;

                stringBuilder.AppendFormat("@{0}", current.Property.Name);

                while (enumerator.MoveNext())
                {
                    current = enumerator.Current;
                    stringBuilder.AppendFormat(", @{0}", current.Property.Name);
                }
            }
        }

        stringBuilder.AppendFormat(") RETURNING ({0});", Key());

        return stringBuilder.ToString();
    }

    public static (string sql, object parameters) GenerateInsertQuery(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var columnsToInsert = Q<T>.DbCache.ColumnsToInsert;

        var sql = GenerateInsertQuery(columnsToInsert);

        var entityParameters = new Dictionary<string, object>(columnsToInsert.Count);
        foreach (var column in columnsToInsert)
        {
            entityParameters.Add(column.Property.Name, column.Get(entity));
        }

        return (sql, entityParameters);
    }

    public static (string sql, object parameters) GenerateInsertQuery(List<T> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        if (entities.Count == 0)
            throw new ArgumentException("Cannot be empty.", nameof(entities));

        var columnsToInsert = DbCache.ColumnsToInsert;

        var sql = GenerateInsertQuery(columnsToInsert);

        var parameters = new List<Dictionary<string, object>>(entities.Count);
        foreach (var entity in entities)
        {
            var entityParameters = new Dictionary<string, object>(columnsToInsert.Count);
            foreach (var column in columnsToInsert)
            {
                entityParameters.Add(column.Property.Name, column.Get(entity));
            }
            parameters.Add(entityParameters);
        }

        return (sql, parameters);
    }

    public static string GenerateUpdateQuery(int id, object obj, string keyName)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        var columnsToUpdate = GetColumnsToUpdate(obj);

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendFormat("UPDATE {0} SET ", Table());
        stringBuilder.AppendFormat(GetBuilderParametersByColumnMetaData(columnsToUpdate));
        stringBuilder.AppendFormat(" WHERE {0} = {1}", keyName, id);

        return stringBuilder.ToString();
    }

    private static string GetBuilderParametersByColumnMetaData(IReadOnlyCollection<ColumnMetadata> columns)
    {
        var stringBuilder = new StringBuilder();

        using (var enumerator = columns.GetEnumerator())
        {
            if (enumerator.MoveNext())
            {
                var current = enumerator.Current;

                stringBuilder.AppendFormat("{0} = @{1}", current.ColumnName, current.Property.Name);

                while (enumerator.MoveNext())
                {
                    current = enumerator.Current;
                    stringBuilder.AppendFormat(", {0} = @{1}", current.ColumnName, current.Property.Name);
                }
            }
        }

        return stringBuilder.ToString();
    }

    public static string GenerateUpdateQuery(int id, IReadOnlyCollection<ColumnMetadata> columnsToUpdate)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendFormat("UPDATE {0} SET ", Table());
        stringBuilder.AppendFormat(GetBuilderParametersByColumnMetaData(columnsToUpdate));
        stringBuilder.AppendFormat(" WHERE {0} = {1}", Key(), id);

        return stringBuilder.ToString();
    }

    public static (string sql, object parameters) GenerateUpdateQuery(int id, T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var columnsToUpdate = DbCache.ColumnsToUpdate;
        var sql = GenerateUpdateQuery(id, columnsToUpdate);

        var entityParameters = new Dictionary<string, object>(columnsToUpdate.Count);
        foreach (var column in columnsToUpdate)
        {
            entityParameters.Add(column.Property.Name, column.Get(entity));
        }

        return (sql, entityParameters);
    }

    public static string GenerateUpdateQuery(List<int> ids, object data, string keyName = null)
    {
        Type tType = typeof(T);
        var props = data.GetType().GetProperties();

        List<string> cols = new List<string>();

        var columnsToUpdate = GetColumnsToUpdate(data);

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendFormat("Update {0} SET ", Table());
        stringBuilder.AppendFormat(GetBuilderParametersByColumnMetaData(columnsToUpdate));
        stringBuilder.AppendFormat(" WHERE {0} in ({1})", keyName ?? Key(), string.Join(",", ids.Select(x => x.ToString())));

        return stringBuilder.ToString();

    }

    public static string SearchByAlias(KeyValuePair<string, object> keyValuePair, string query = null, string prefix = null)
    {
        string searchQuery = "";
        string alias = StringExtensions.FirstCharToUpper(keyValuePair.Key);
        string columnName = "";

        Type tType = typeof(T);
        var property = tType.GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)?.FirstOrDefault(x => x.Name == alias);
        if (property != null)
        {
            var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
            if (columnAttribute != null)
                columnName = (!string.IsNullOrEmpty(prefix) ? $"{prefix}." : "") + columnAttribute.Name;
        }
        if (string.IsNullOrEmpty(columnName))
        {
            try
            {
                if (query.ToLower().StartsWith("select"))
                {
                    Regex rx = new Regex(@"select(.*?)from", RegexOptions.Singleline);
                    query = rx.Match(query).Groups[1].Value.Trim();
                }
                var selectClauses = query.Split(new string[] { " as " }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
                var _selection = selectClauses.FirstOrDefault(x => x.StartsWith(alias));
                var index = selectClauses.IndexOf(_selection);
                if (index > 0)
                    columnName = selectClauses[index - 1].Substring(selectClauses[index - 1].IndexOf(',') + 1);
            }
            catch (Exception)
            {
            }
        }

        if (DataTypeExtensions.IsNumeric(keyValuePair.Value.GetType()))
            searchQuery = $"{columnName} = {keyValuePair.Value}";
        else if (keyValuePair.Value.GetType() == typeof(JArray))
        {
            JArray jArray = (JArray)keyValuePair.Value;

            if (DateTime.TryParse(jArray.First.ToString(), out DateTime from))
                searchQuery = $"DATE({columnName}) between '{jArray.First}' and '{jArray.Last}'";
            else if (DataTypeExtensions.IsNumeric(jArray.First.GetType()))
                searchQuery = $"{columnName} between {jArray.First} and {jArray.Last}";
        }
        else if (keyValuePair.Value.GetType() == typeof(string))
        {
            if (property != null && property.PropertyType.BaseType.Name == "Enum")
            {
                Type type = Type.GetType(property.PropertyType.FullName);
                var value = Enum.Parse(type, keyValuePair.Value.ToString());
                searchQuery = $"{columnName} = {(int)value}";
            }
            else
                searchQuery = $"{columnName} like '%{keyValuePair.Value?.ReplaceSingQuoteByDoubleQuote()}%'";
        }

        return searchQuery;
    }

    public static string Upsert(object obj)
    {
        Type tType = typeof(T);
        var props = obj.GetType().GetProperties();

        List<string> cols = new List<string>();
        List<string> colUpdates = new List<string>();
        List<string> colValueParams = new List<string>();

        foreach (var item in props)
        {
            if (item.CanRead)
            {
                var tProp = tType.GetProperty(item.Name);

                if (tProp != null)
                {
                    ColumnAttribute cAttr = tProp.GetCustomAttribute<ColumnAttribute>();
                    if (cAttr != null)
                    {
                        string name = cAttr.Name;
                        cols.Add(name);
                        colUpdates.Add($"{name} = EXCLUDED.{name}");
                        colValueParams.Add("@" + item.Name);
                    }
                }

            }
        }

        return $"INSERT INTO {Table()} ({string.Join(",", cols)}) VALUES ({string.Join(",", colValueParams)}) ON CONFLICT ({Key()}) DO UPDATE SET {string.Join(",", colUpdates)} ;";
    }

    private static List<ColumnMetadata> GetColumnsToUpdate(object obj)
    {
        var columnsToUpdate = new List<ColumnMetadata>();

        Type tType = typeof(T);
        var properties = obj.GetType().GetProperties();
        foreach (var property in properties)
        {
            if (property.CanRead)
            {
                var tProp = tType.GetProperty(property.Name);
                if (tProp != null)
                {
                    var attribute = tProp.GetCustomAttribute<ColumnAttribute>();
                    if (attribute != null)
                    {
                        columnsToUpdate.Add(new ColumnMetadata(property, attribute.Name));
                    }
                }
            }
        }

        return columnsToUpdate;
    }

    private sealed class DbCache
    {
        private class PropertyInfoEqualityComparer : IEqualityComparer<PropertyInfo>
        {
            public bool Equals(PropertyInfo x, PropertyInfo y)
            {
                if (x == null || y == null)
                {
                    return false;
                }
                return x.Name == y.Name;
            }

            public int GetHashCode([DisallowNull] PropertyInfo obj)
            {
                return obj.Name.GetHashCode();
            }
        }

        private static readonly List<ColumnMetadata> _columns;
        private static readonly List<ColumnMetadata> _columnsToInsert;
        private static readonly List<ColumnMetadata> _columnsToUpdate;
        private static readonly List<ColumnMetadata> _columnsToSearch;

        private static readonly Dictionary<PropertyInfo, ColumnMetadata> _propertyToColumnMap;
        public static string TableName { get; }
        public static string Key { get; }
        public static ColumnMetadata DisableColumn { get; }
        public static ColumnMetadata DeleteColumn { get; }

        public static ReadOnlyCollection<ColumnMetadata> Columns { get; }
        public static ReadOnlyCollection<ColumnMetadata> ColumnsToInsert { get; }
        public static ReadOnlyCollection<ColumnMetadata> ColumnsToUpdate { get; }
        public static ReadOnlyCollection<ColumnMetadata> ColumnsToSearch { get; }

        static DbCache()
        {
            TableName = InitializeTableName();

            _columns = IntializeColumns();

            Key = InitializePrimaryKey(_columns);

            _columnsToInsert = IntializeColumnsToInsert(_columns);
            _columnsToUpdate = IntializeColumnsToUpdate(_columns);
            _columnsToSearch = IntializeColumnsToSearch(_columns);

            Columns = new ReadOnlyCollection<ColumnMetadata>(_columns);
            ColumnsToInsert = new ReadOnlyCollection<ColumnMetadata>(_columnsToInsert);
            ColumnsToUpdate = new ReadOnlyCollection<ColumnMetadata>(_columnsToUpdate);
            ColumnsToSearch = new ReadOnlyCollection<ColumnMetadata>(_columnsToSearch);

            _propertyToColumnMap = _columns.ToDictionary(c => c.Property, comparer: new PropertyInfoEqualityComparer());

            DisableColumn = InitializeDisableColumn(_columns);
            DeleteColumn = InitializeDeleteColumn(_columns);
        }

        public static ColumnMetadata Column(PropertyInfo property)
        {
            return _propertyToColumnMap[property];
        }

        private static string InitializeTableName()
        {
            var type = typeof(T);

            var tableAttibute = type.GetCustomAttribute<TableAttribute>();
            if (tableAttibute == null)
            {
                throw new NotSupportedException($"The type {type.Name} doesn't have a TableAttribute");
            }

            return "`" + tableAttibute.Name + "`";
        }

        private static string InitializePrimaryKey(List<ColumnMetadata> columns)
        {
            foreach (var column in columns)
            {
                var key = column.Property.GetCustomAttribute<KeyAttribute>();
                if (key != null)
                {
                    return column.ColumnName;
                }
            }

            return null;
        }

        private static Type InitializeDisableColumnType(List<ColumnMetadata> columns)
        {
            foreach (var column in columns)
            {
                var disableColumn = column.Property.GetCustomAttribute<ActiveColumnAttribute>();
                if (disableColumn != null)
                {
                    return column.Property.PropertyType;
                }
            }

            return null;
        }

        private static Type InitializeDeleteColumnType(List<ColumnMetadata> columns)
        {
            foreach (var column in columns)
            {
                var deleteColumn = column.Property.GetCustomAttribute<DeleteColumnAttribute>();
                if (deleteColumn != null)
                {
                    return column.Property.PropertyType;
                }
            }

            return null;
        }

        private static ColumnMetadata InitializeDisableColumn(List<ColumnMetadata> columns)
        {
            foreach (var column in columns)
            {
                var disableColumn = column.Property.GetCustomAttribute<ActiveColumnAttribute>();
                if (disableColumn != null)
                {
                    return column;
                }
            }

            return null;
        }

        private static ColumnMetadata InitializeDeleteColumn(List<ColumnMetadata> columns)
        {
            foreach (var column in columns)
            {
                var deleteColumn = column.Property.GetCustomAttribute<DeleteColumnAttribute>();
                if (deleteColumn != null)
                {
                    return column;
                }
            }

            return null;
        }

        private static List<ColumnMetadata> IntializeColumns()
        {
            var result = new List<ColumnMetadata>();

            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public);

            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<ColumnAttribute>();
                if (attribute != null)
                {
                    result.Add(new ColumnMetadata(property, attribute.Name));
                }
            }

            return result;
        }

        private static List<ColumnMetadata> IntializeColumnsToSearch(List<ColumnMetadata> columns)
        {
            var result = new List<ColumnMetadata>();

            foreach (var column in columns)
            {
                var attribute = column.Property.GetCustomAttribute<SearchTextAttribute>();
                if (attribute != null)
                {
                    result.Add(column);
                }
            }

            return result;
        }

        private static List<ColumnMetadata> IntializeColumnsToInsert(List<ColumnMetadata> columns)
        {
            var result = new List<ColumnMetadata>();
            foreach (var column in columns)
            {
                var skipForInsert = column.Property.GetCustomAttribute<SkipForInsertAttribute>();
                if (skipForInsert == null && column.ColumnName != Key)
                {
                    result.Add(column);
                }
            }
            return result;
        }
        private static List<ColumnMetadata> IntializeColumnsToUpdate(List<ColumnMetadata> columns)
        {
            var result = new List<ColumnMetadata>();
            foreach (var column in columns)
            {
                if (column.ColumnName != Key)
                {
                    result.Add(column);
                }
            }
            return result;
        }
    }

    public sealed class ColumnMetadata
    {
        public PropertyInfo Property { get; }
        public string ColumnName { get; }
        private readonly Func<object, object> _getter;
        private readonly Action<object, object> _setter;

        public ColumnMetadata(PropertyInfo property, string columnName)
        {
            Property = property;
            ColumnName = columnName;

            _getter = property.GetGetter();
            _setter = property.GetSetter();
        }

        public object Get(object entity)
        {
            if (_getter == null)
                throw new NotSupportedException($"The property {Property.Name} doesn't have public getter.");

            return _getter(entity);
        }

        public void Set(object entity, object value)
        {
            if (_setter == null)
                throw new NotSupportedException($"The property {Property.Name} doesn't have public setter.");

            _setter(entity, value);
        }
    }
}

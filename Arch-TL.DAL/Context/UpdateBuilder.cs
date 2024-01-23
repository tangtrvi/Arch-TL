using System.Linq.Expressions;
using System.Text;

namespace Arch_TL.DAL.Context
{
    public class UpdateBuilder<TEntity> where TEntity : class
    {
        private readonly IQueryOrm _orm;
        private readonly StringBuilder _set;
        private readonly StringBuilder _where;
        private readonly Dictionary<string, object> _parameters;

        public UpdateBuilder(IQueryOrm orm)
        {
            _orm = orm;
            _set = new StringBuilder();
            _where = new StringBuilder();
            _parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public Dictionary<string, object> GetParameters()
        {
            return _parameters;
        }

        public IQueryOrm GetORM()
        {
            return _orm;
        }

        public PredicateBuilder<TProperty> Where<TProperty>(Expression<Func<TEntity, TProperty>> property)
        {
            var columnyName = Q<TEntity>.Column(property);

            return new PredicateBuilder<TProperty>(this, columnyName);
        }

        public SetBuilder WherePrimaryKey(object value)
        {
            var primaryKey = Q<TEntity>.Key();

            var parameterName = string.Format("p0_{0}", primaryKey);

            _where.AppendFormat(" WHERE {0} = @{1}", primaryKey, parameterName);
            _parameters.Add(parameterName, value);

            return new SetBuilder(this);
        }

        public SetBuilder WherePrimaryKeys(ICollection<int> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (values.Count == 0)
            {
                return new SetBuilder(this);
            }

            if (values.Count == 1)
            {
                return WherePrimaryKey(values.First());
            }
            var primaryKey = Q<TEntity>.Key();
            _where.AppendFormat(" WHERE {0} IN (", primaryKey);

            using (var enumerator = values.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    var i = 0;
                    var current = enumerator.Current;

                    var parameterName = string.Format("p{0}_{1}", i, primaryKey);

                    _where.AppendFormat("@{0}", parameterName);
                    _parameters.Add(parameterName, current);

                    i++;

                    while (enumerator.MoveNext())
                    {
                        current = enumerator.Current;
                        parameterName = string.Format("p{0}_{1}", i, primaryKey);

                        _where.AppendFormat(", @{0}", parameterName);
                        _parameters.Add(parameterName, current);

                        i++;
                    }
                }
            }

            _where.Append(")");

            return new SetBuilder(this);
        }

        public readonly struct PredicateBuilder<TProperty>
        {
            private readonly UpdateBuilder<TEntity> _builder;
            private readonly string _columnName;

            public PredicateBuilder(UpdateBuilder<TEntity> builder, string columnName)
            {
                _builder = builder;
                _columnName = columnName;
            }

            public SetBuilder Equals(TProperty value)
            {
                var parameterName = string.Format("p0_{0}", _columnName);

                _builder._where.AppendFormat(" WHERE {0} = @{1}", _columnName, parameterName);
                _builder._parameters.Add(parameterName, value);

                return new SetBuilder(_builder);
            }

            public SetBuilder In(ICollection<TProperty> values)
            {
                if (values == null)
                    throw new ArgumentNullException(nameof(values));

                if (values.Count == 0)
                {
                    return new SetBuilder(_builder);
                }

                if (values.Count == 1)
                {
                    return Equals(values.First());
                }

                _builder._where.AppendFormat(" WHERE {0} IN (", _columnName);

                using (var enumerator = values.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        var i = 0;
                        var current = enumerator.Current;
                        var parameterName = string.Format("p{0}_{1}", i, _columnName);

                        _builder._where.AppendFormat("@{0}", parameterName);
                        _builder._parameters.Add(parameterName, current);

                        i++;

                        while (enumerator.MoveNext())
                        {
                            current = enumerator.Current;
                            parameterName = string.Format("p{0}_{1}", i, _columnName);

                            _builder._where.AppendFormat(", @{0}", parameterName);
                            _builder._parameters.Add(parameterName, current);

                            i++;
                        }
                    }
                }

                _builder._where.Append(")");

                return new SetBuilder(_builder);
            }
        }

        public readonly struct SetBuilder
        {
            private readonly UpdateBuilder<TEntity> _builder;
            public SetBuilder(UpdateBuilder<TEntity> builder)
            {
                _builder = builder;
            }

            public ExecuteBuilder Set<TProperty>(Expression<Func<TEntity, TProperty>> property, TProperty value)
            {
                var columnyName = Q<TEntity>.Column(property);

                _builder._set.AppendFormat("UPDATE {0} SET ", Q<TEntity>.Table());

                _builder._set.AppendFormat("{0} = @{0}", columnyName);
                _builder._parameters.Add(columnyName, value);

                return new ExecuteBuilder(_builder);
            }
        }

        public readonly struct ExecuteBuilder
        {
            private readonly UpdateBuilder<TEntity> _builder;
            public ExecuteBuilder(UpdateBuilder<TEntity> builder)
            {
                _builder = builder;
            }

            public UpdateBuilder<TEntity> GetBuilder()
            {
                return _builder;
            }

            public ExecuteBuilder Set<TProperty>(Expression<Func<TEntity, TProperty>> property, TProperty value)
            {
                var columnyName = Q<TEntity>.Column(property);

                _builder._set.Append(", ");
                _builder._set.AppendFormat("{0} = @{0}", columnyName);
                _builder._parameters.Add(columnyName, value);

                return new ExecuteBuilder(_builder);
            }
            public Task<int> ExecuteAsync()
            {
                var sql = Sql();
                if (sql == null)
                {
                    return Task.FromResult(0);
                }
                return _builder._orm.ExecuteAsync(sql, _builder._parameters);
            }

            public string Sql()
            {
                if (_builder._where.Length == 0)
                    return null;

                var query = new StringBuilder();

                query.Append(_builder._set);
                query.Append(_builder._where);

                return query.ToString();
            }
        }

    }
}

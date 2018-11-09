using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace Dubonnet
{
    public static partial class DubonMapper
    {
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> _getQueryCache = new ConcurrentDictionary<RuntimeTypeHandle, string>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> _getAllQueryCache = new ConcurrentDictionary<RuntimeTypeHandle, string>();

        /// <summary>
        /// Retrieves the entity of type <typeparamref name="T"/> with the specified id.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="id">The id of the entity in the database.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <returns>The entity with the corresponding id.</returns>
        public static T Get<T>(this IDbConnection connection, object id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var sql = BuildGetById(typeof(T), id, out var parameters);
            LogQuery<T>(sql);
            return connection.QueryFirstOrDefault<T>(sql, parameters, transaction,commandTimeout);
        }

        /// <summary>
        /// Retrieves the entity of type <typeparamref name="T"/> with the specified id.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="id">The id of the entity in the database.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <returns>The entity with the corresponding id.</returns>
        public static Task<T> GetAsync<T>(this IDbConnection connection, object id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var sql = BuildGetById(typeof(T), id, out var parameters);
            LogQuery<T>(sql);
            return connection.QueryFirstOrDefaultAsync<T>(sql, parameters, transaction,commandTimeout);
        }

        private static string BuildGetById(Type type, object id, out DynamicParameters parameters)
        {
            if (!_getQueryCache.TryGetValue(type.TypeHandle, out var sql))
            {
                var tableName = Resolvers.Table(type);
                var keyProperty = Resolvers.KeyProperty(type);
                var keyColumnName = Resolvers.Column(keyProperty);

                sql = $"select * from {tableName} where {keyColumnName} = @Id";
                _getQueryCache.TryAdd(type.TypeHandle, sql);
            }

            parameters = new DynamicParameters();
            parameters.Add("Id", id);

            return sql;
        }

        /// <summary>
        /// Retrieves all the entities of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="buffered">
        /// A value indicating whether the result of the query should be executed directly,
        /// or when the query is materialized (using <c>ToList()</c> for example).
        /// </param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <returns>A collection of entities of type <typeparamref name="T"/>.</returns>
        public static IEnumerable<T> GetAll<T>(this IDbConnection connection, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null) where T : class
        {
            var sql = BuildGetAllQuery(typeof(T));
            LogQuery<T>(sql);
            return connection.Query<T>(sql, transaction: transaction, buffered: buffered,commandTimeout:commandTimeout);
        }

        /// <summary>
        /// Retrieves all the entities of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <returns>A collection of entities of type <typeparamref name="T"/>.</returns>
        public static Task<IEnumerable<T>> GetAllAsync<T>(this IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var sql = BuildGetAllQuery(typeof(T));
            LogQuery<T>(sql);
            return connection.QueryAsync<T>(sql, transaction: transaction,commandTimeout:commandTimeout);
        }

        private static string BuildGetAllQuery(Type type)
        {
            if (!_getAllQueryCache.TryGetValue(type.TypeHandle, out var sql))
            {
                sql = "select * from " + Resolvers.Table(type); ;
                _getAllQueryCache.TryAdd(type.TypeHandle, sql);
            }

            return sql;
        }

        /// <summary>
        /// Retrieves a paged set of entities of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="pageNumber">The number of the page to fetch, starting at 1.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="buffered">
        /// A value indicating whether the result of the query should be executed directly,
        /// or when the query is materialized (using <c>ToList()</c> for example).
        /// </param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <returns>A paged collection of entities of type <typeparamref name="T"/>.</returns>
        public static IEnumerable<T> GetPaged<T>(this IDbConnection connection, int pageNumber, int pageSize, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null) where T : class
        {
            var sql = BuildPagedQuery(connection, typeof(T), pageNumber, pageSize);
            LogQuery<T>(sql);
            return connection.Query<T>(sql, transaction: transaction, buffered: buffered,commandTimeout:commandTimeout);
        }

        /// <summary>
        /// Retrieves a paged set of entities of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="connection">The connection to the database. This can either be open or closed.</param>
        /// <param name="pageNumber">The number of the page to fetch, starting at 1.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="transaction">Optional transaction for the command.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <returns>A paged collection of entities of type <typeparamref name="T"/>.</returns>
        public static Task<IEnumerable<T>> GetPagedAsync<T>(this IDbConnection connection, int pageNumber, int pageSize, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var sql = BuildPagedQuery(connection, typeof(T), pageNumber, pageSize);
            LogQuery<T>(sql);
            return connection.QueryAsync<T>(sql, transaction: transaction,commandTimeout:commandTimeout);
        }

        private static string BuildPagedQuery(IDbConnection connection, Type type, int pageNumber, int pageSize)
        {
            // Start with the select query part.
            var sql = BuildGetAllQuery(type);

            // Append  the paging part including the order by.
            var orderBy = "order by " + Resolvers.Column(Resolvers.KeyProperty(type));
            sql += GetSqlBuilder(connection).BuildPaging(orderBy, pageNumber, pageSize);
            return sql;
        }
    }
}

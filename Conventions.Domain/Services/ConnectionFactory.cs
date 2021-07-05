namespace Conventions.Domain.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Dapper;

    public class ConnectionFactory : IConnectionFactory
    {
        private readonly string fileName;

        public ConnectionFactory(string fileName)
        {
            this.fileName = fileName ?? throw new ArgumentNullException(nameof(this.fileName));

            Task.Run(() => CreateTablesAsync()).Wait(); // todo remove once in cloud ;)
        }

        public async Task<SQLiteConnection> CreateConnectionAsync(CancellationToken token)
        {
            var connection = new SQLiteConnection($"Data Source={fileName}");
            await connection.OpenAsync(token);

            return connection;
        }

        private static async Task ExecuteNonQueryAsync(string sql, SQLiteConnection connection, CancellationToken token = default)
        {
            var command = new SQLiteCommand(sql, connection);

            await command.ExecuteNonQueryAsync(token);
        }

        public static async Task<string> InsertAsync(
            SQLiteConnection connection,
            string tableName,
            IDictionary<string, object> parameters,
            CancellationToken token = default)
        {
            var stringBuilderMain = new StringBuilder($"INSERT INTO {tableName}(");
            var stringBuilderParameters = new StringBuilder();

            var transaction = connection.BeginTransaction();
            try
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    var first = true;
                    foreach (var value in parameters)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            stringBuilderMain.Append(", ");
                            stringBuilderParameters.Append(", ");
                        }

                        stringBuilderMain.Append(value.Key);
                        stringBuilderParameters.Append($"@{value.Key}");
                        cmd.Parameters.AddWithValue($"@{value.Key}", value.Value);
                    }

                    stringBuilderMain.Append($") VALUES ({stringBuilderParameters})");
                    cmd.CommandText = stringBuilderMain.ToString();

                    await cmd.ExecuteNonQueryAsync(token);
                }

                var result = connection.LastInsertRowId.ToString();
                transaction.Commit();

                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public static async Task UpdateAsync(
            SQLiteConnection connection,
            string tableName,
            IDictionary<string, object> parameters,
            KeyValuePair<string, object> condition,
            CancellationToken token = default)
        {
            var stringBuilder = new StringBuilder($"UPDATE {tableName} SET ");

            var transaction = connection.BeginTransaction();
            try
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    var first = true;
                    foreach (var value in parameters)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            stringBuilder.Append(", ");
                        }

                        stringBuilder.Append($"{value.Key} = @{value.Key}");
                        cmd.Parameters.AddWithValue($"@{value.Key}", value.Value);
                    }

                    stringBuilder.Append($" WHERE {condition.Key} = @{condition.Key}");
                    cmd.Parameters.AddWithValue($"@{condition.Key}", condition.Value);

                    cmd.CommandText = stringBuilder.ToString();

                    await cmd.ExecuteNonQueryAsync(token);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public static async Task DeleteAsync(
            SQLiteConnection connection,
            string tableName,
            IDictionary<string, object> conditions,
            CancellationToken token = default)
        {
            var stringBuilder = new StringBuilder($"delete from {tableName}");

            var transaction = connection.BeginTransaction();
            try
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    var first = true;
                    foreach (var value in conditions)
                    {
                        if (first)
                        {
                            first = false;
                            stringBuilder.Append(" where ");
                        }
                        else
                        {
                            stringBuilder.Append(" and ");
                        }

                        stringBuilder.Append($"{value.Key} = @{value.Key}");
                        cmd.Parameters.AddWithValue($"@{value.Key}", value.Value);
                    }

                    cmd.CommandText = stringBuilder.ToString();

                    await cmd.ExecuteNonQueryAsync(token);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public static async Task<long> GetRecordCountAsync(
            SQLiteConnection connection,
            string tableName,
            IDictionary<string, object> parameters,
            CancellationToken token = default)
        {
            var stringBuilder = new StringBuilder($"select count(*) from {tableName}");

            using (var cmd = new SQLiteCommand(connection))
            {
                var first = true;
                foreach (var value in parameters)
                {
                    if (first)
                    {
                        first = false;
                        stringBuilder.Append(" where ");
                    }
                    else
                    {
                        stringBuilder.Append(" and ");
                    }

                    stringBuilder.Append($"{value.Key} = @{value.Key}");
                    cmd.Parameters.AddWithValue($"@{value.Key}", value.Value);
                }

                cmd.CommandText = stringBuilder.ToString();

                var result = await cmd.ExecuteScalarAsync(token);

                return (long)result;
            }
        }

        public static async Task<IEnumerable<T>> ListAsync<T>(
            SQLiteConnection connection,
            string tableName,
            IDictionary<string, object> conditions,
            int page,
            int pageSize,
            CancellationToken token = default)
            where T : class
        {
            var stringBuilder = new StringBuilder($"SELECT * FROM {tableName}");
            var parameters = new DynamicParameters();

            if (conditions != null && conditions.Any())
            {
                var first = true;
                foreach (var value in conditions)
                {
                    if (first)
                    {
                        first = false;
                        stringBuilder.Append(" WHERE ");
                    }
                    else
                    {
                        stringBuilder.Append(" AND ");
                    }

                    stringBuilder.Append($"{value.Key} = @{value.Key}");
                    parameters.Add(value.Key, value.Value);
                }
            }

            var offset = pageSize * (page - 1);
            stringBuilder.Append($" ORDER BY ID LIMIT {offset}, {pageSize}");

            return await connection.QueryAsync<T>(stringBuilder.ToString(), parameters);
        }

        private async Task CreateTablesAsync(CancellationToken token = default)
        {
            using (var connection = await CreateConnectionAsync(token))
            {
                await ExecuteNonQueryAsync(
                    "create table if not exists users("
                    + "id varchar(255) primary key not null unique,"
                    + "name varchar(255) not null,"
                    + "street varchar(255),"
                    + "city varchar(255),"
                    + "country varchar(255),"
                    + "phone varchar(63),"
                    + "mail varchar(255))",
                    connection,
                    token);
                await ExecuteNonQueryAsync(
                    "create table if not exists venues("
                    + "id integer primary key autoincrement,"
                    + "name varchar(255) not null,"
                    + "street varchar(255),"
                    + "city varchar(255),"
                    + "country varchar(255))",
                    connection,
                    token);
                await ExecuteNonQueryAsync(
                    "create table if not exists conventions("
                    + "id integer primary key autoincrement,"
                    + "name varchar(255) not null,"
                    + "venueId int not null,"
                    + "startDate int,"
                    + "endDate int,"
                    + "foreign key(venueId) REFERENCES venues(id) ON UPDATE CASCADE ON DELETE RESTRICT)",
                    connection,
                    token);
                await ExecuteNonQueryAsync(
                    "create table if not exists talks("
                    + "id integer primary key autoincrement,"
                    + "title varchar(255) not null,"
                    + "speakerId varchar(255) not null,"
                    + "conventionId int not null,"
                    + "startTime int,"
                    + "endTime int,"
                    + "capacity int,"
                    + "foreign key(speakerId) REFERENCES users(id) ON UPDATE CASCADE ON DELETE RESTRICT,"
                    + "foreign key(conventionId) REFERENCES conventions(id) ON UPDATE CASCADE ON DELETE RESTRICT)",
                    connection,
                    token);
                await ExecuteNonQueryAsync(
                    "create table if not exists conventionRegistrations("
                    + "userId varchar(255) not null,"
                    + "conventionId int not null,"
                    + "primary key(userId, conventionId),"
                    + "foreign key(userId) REFERENCES users(id) ON UPDATE CASCADE ON DELETE RESTRICT,"
                    + "foreign key(conventionId) REFERENCES conventions(id) ON UPDATE CASCADE ON DELETE RESTRICT)",
                    connection,
                    token);
                await ExecuteNonQueryAsync(
                    "create table if not exists talkRegistrations("
                    + "userId varchar(255) not null,"
                    + "talkId int not null,"
                    + "primary key(userId, talkId),"
                    + "foreign key(userId) REFERENCES users(id) ON UPDATE CASCADE ON DELETE RESTRICT,"
                    + "foreign key(talkId) REFERENCES talks(id) ON UPDATE CASCADE ON DELETE RESTRICT)",
                    connection,
                    token);
                await ExecuteNonQueryAsync(
                    "create view if not exists viewConventionRegistrations as "
                    + "select conventions.*, conventionRegistrations.userId from conventions "
                    + "inner join conventionRegistrations on conventions.id = conventionRegistrations.conventionId",
                    connection,
                    token);
                await ExecuteNonQueryAsync(
                    "create view if not exists viewTalkRegistrations as "
                    + "select talks.*, talkRegistrations.userId from talks "
                    + "inner join talkRegistrations on talks.id = talkRegistrations.talkId",
                    connection,
                    token);
            }
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SLDTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DBContext
{
    public class SLContext : DbContext
    {
        // 多数据库统一配置（外部传入或从配置文件加载）
        private readonly MultiDbConfig _dbConfig;

        #region 构造函数（支持动态传入配置/默认加载配置文件）
        /// <summary>
        /// 动态传入配置（推荐：支持代码切换数据库）
        /// </summary>
        public SLContext(MultiDbConfig dbConfig)
        {
            _dbConfig = dbConfig ?? throw new ArgumentNullException(nameof(dbConfig), "多数据库配置不能为空");
        }

        /// <summary>
        /// 无参构造函数（兼容旧逻辑：从配置文件加载）
        /// </summary>
        public SLContext()
        {
            // 从配置文件加载多数据库配置（需改造 ini 文件，新增数据库类型标识）
            _dbConfig = LoadMultiDbConfigFromIni();
        }
        #endregion

        #region 核心：动态配置数据库提供程序（根据 DbType 切换）
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured) return;

            // 根据数据库类型，动态初始化对应提供程序
            switch (_dbConfig.DbType)
            {
                case DatabaseType.MySql:
                    ConfigureMySql(optionsBuilder);
                    break;
                case DatabaseType.SqlServer:
                    ConfigureSqlServer(optionsBuilder);
                    break;
                case DatabaseType.Sqlite:
                    ConfigureSqlite(optionsBuilder);
                    break;
                default:
                    throw new NotSupportedException($"不支持的数据库类型：{_dbConfig.DbType}");
            }
        }
        #endregion

        #region 各数据库具体配置逻辑（拼接连接串 + 初始化提供程序）
        /// <summary>
        /// 配置 MySQL 连接
        /// </summary>
        private void ConfigureMySql(DbContextOptionsBuilder optionsBuilder)
        {
            var connStr = new StringBuilder()
                .AppendLine($"Server={_dbConfig.MySql_Server};")
                .AppendLine($"Port={_dbConfig.MySql_Port};")
                .AppendLine($"Database={_dbConfig.MySql_Database};")
                .AppendLine($"Uid={_dbConfig.MySql_UserId};")
                .AppendLine($"Pwd={_dbConfig.MySql_Password};")
                .Append(_dbConfig.MySql_AdditionalParams)
                .ToString();

            optionsBuilder.UseMySql(
                connStr,
                ServerVersion.AutoDetect(connStr),
                opt => opt.EnableRetryOnFailure(3) // 可选：重试机制
            );
        }

        /// <summary>
        /// 配置 SQL Server 连接
        /// </summary>
        private void ConfigureSqlServer(DbContextOptionsBuilder optionsBuilder)
        {
            var connStr = new StringBuilder()
                .AppendLine($"Server={_dbConfig.SqlServer_Server};")
                .AppendLine($"Database={_dbConfig.SqlServer_Database};")
                .AppendLine(_dbConfig.SqlServer_IntegratedSecurity
                    ? "Integrated Security=True;"
                    : $"User ID={_dbConfig.SqlServer_UserId};Password={_dbConfig.SqlServer_Password};")
                .Append(_dbConfig.SqlServer_AdditionalParams)
                .ToString();

            optionsBuilder.UseSqlServer(
                connStr,
                opt => opt.EnableRetryOnFailure(3)
            );
        }

        /// <summary>
        /// 配置 SQLite 连接（文件型数据库，路径优先）
        /// </summary>
        private void ConfigureSqlite(DbContextOptionsBuilder optionsBuilder)
        {
            // 处理 SQLite 路径（兼容绝对路径/相对路径）
            string dbPath = _dbConfig.Sqlite_DbPath;
            if (!Path.IsPathRooted(dbPath))
            {
                // 相对路径 -> 转为应用程序目录下的路径
                dbPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), dbPath);
            }

            var connStr = $"Data Source={dbPath};{_dbConfig.Sqlite_AdditionalParams}";
            optionsBuilder.UseSqlite(connStr);
        }
        #endregion

        #region 兼容旧逻辑：从 ini 配置文件加载多数据库配置
        /// <summary>
        /// 改造原有 ini 文件，新增数据库类型标识和各数据库参数
        /// </summary>
        private MultiDbConfig LoadMultiDbConfigFromIni()
        {
            var configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Config", "DbConfig.ini");
            if (!File.Exists(configPath))
                throw new FileNotFoundException("数据库配置文件不存在", configPath);

            var lines = File.ReadAllLines(configPath);
            string GetIniValue(string key) => lines
                .FirstOrDefault(line => line.StartsWith($"{key}=") && !line.StartsWith("#"))
                ?.Split(new[] { '=' }, 2)
                ?.ElementAtOrDefault(1)
                ?.Trim() ?? string.Empty;

            // 解析 ini 中的数据库类型（必填：MySql/SqlServer/Sqlite）
            if (!Enum.TryParse<DatabaseType>(GetIniValue("DatabaseType"), true, out var dbType))
                dbType = DatabaseType.Sqlite; // 默认SQLite

            return new MultiDbConfig
            {
                DbType = dbType,

                // MySQL 配置
                MySql_Server = GetIniValue("MySql_Server") ?? "localhost",
                MySql_Port = int.TryParse(GetIniValue("MySql_Port"), out var mySqlPort) ? mySqlPort : 3306,
                MySql_Database = GetIniValue("MySql_Database"),
                MySql_UserId = GetIniValue("MySql_UserId"),
                MySql_Password = GetIniValue("MySql_Password"),
                MySql_AdditionalParams = GetIniValue("MySql_AdditionalParams") ?? "CharSet=utf8mb4;Connect Timeout=30;",

                // SQL Server 配置
                SqlServer_Server = GetIniValue("SqlServer_Server") ?? "localhost",
                SqlServer_Database = GetIniValue("SqlServer_Database"),
                SqlServer_UserId = GetIniValue("SqlServer_UserId"),
                SqlServer_Password = GetIniValue("SqlServer_Password"),
                SqlServer_IntegratedSecurity = bool.TryParse(GetIniValue("SqlServer_IntegratedSecurity"), out var sqlServerWinAuth) ? sqlServerWinAuth : false,
                SqlServer_AdditionalParams = GetIniValue("SqlServer_AdditionalParams") ?? "Connect Timeout=30;TrustServerCertificate=True;",

                // SQLite 配置
                Sqlite_DbPath = GetIniValue("Sqlite_DbPath") ?? "app.db",
                Sqlite_AdditionalParams = GetIniValue("Sqlite_AdditionalParams") ?? "Cache=Shared;"
            };
        }
        #endregion

        #region 原有业务逻辑（DbSet + 自定义查询方法，保持不变）
        public DbSet<PointtableDTO> pointtable { get; set; } = null!;
        public IEnumerable<ModbusConfigDTO<T>> GetModbusConfigExcact<T>()
        {
            return ModbusConfig.AsEnumerable().Select(x =>
            {
                return new ModbusConfigDTO<T>()
                {
                    id = x.id,
                    code = x.code,
                    name = x.name,
                    ConfigStr = JsonConvert.DeserializeObject<T>(x.ConfigStr),
                    Isdefault = x.Isdefault,
                    sulotionid = x.sulotionid,
                };
            }).AsEnumerable();
        }
        public DbSet<ModbusConfigDTO<string>> ModbusConfig { get; set; } = null!;
        public DbSet<ScanQRBomsDTO> ScanQRBoms { get; set; } = null;
        public IEnumerable<HistoryDTO<T>> GetHistoriesExcact<T>(long? id, long? sulotionid, DateTime? dateTimeFrom = null, DateTime? dateTimeTo = null)
        {
            var q = History.AsEnumerable();
            if (id != null) q = q.Where(x => x.id == id);
            if (sulotionid != null) q = q.Where(x => x.solutionid == sulotionid);
            if (dateTimeFrom != null) q = q.Where(x => x.CreateDate >= dateTimeFrom);
            if (dateTimeTo != null) q = q.Where(x => x.CreateDate <= dateTimeTo);

            return q.Select(x => new HistoryDTO<T>()
            {
                id = x.id,
                CreateDate = x.CreateDate,
                solutionid = x.solutionid,
                jasonvalue = JsonConvert.DeserializeObject<T>(x.jasonvalue)
            }).AsEnumerable();
        }
        public DbSet<HistoryDTO<string>> History { get; set; } = null;
        public IEnumerable<BaseStoreDTO<T>> GetBasestoreExcact<T>(long? id, long? sulotionid) where T : class
        {
            var q = basestore.AsEnumerable();
            if (id != null) q = q.Where(x => x.id == id);
            if (sulotionid != null) q = q.Where(x => x.solutionid == sulotionid);

            return q.Select(x => new BaseStoreDTO<T>()
            {
                id = x.id,
                solutionid = x.solutionid,
                name = x.name,
                remark = x.remark,
                code = x.code,
                jason = JsonConvert.DeserializeObject<T>(x.jason),
                createdate = x.createdate,
            }).AsEnumerable();
        }
        public DbSet<BaseStoreDTO<string>> basestore { get; set; } = null;
        public DbSet<Possession> Possession { get; set; } = null;
        public DbSet<Sulotion> sulotion { get; set; } = null;
        #endregion
    }
}
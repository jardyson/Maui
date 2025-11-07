namespace DBContext
{
    /// <summary>
    /// 多数据库统一配置模型
    /// </summary>
    public class MultiDbConfig
    {
        /// <summary>
        /// 数据库类型（必填，决定使用哪种提供程序）
        /// </summary>
        public DatabaseType DbType { get; set; } = DatabaseType.Sqlite; // 默认SQLite

        #region MySQL 连接参数
        public string MySql_Server { get; set; } = "localhost";
        public int MySql_Port { get; set; } = 3306;
        public string MySql_Database { get; set; } = string.Empty;
        public string MySql_UserId { get; set; } = string.Empty;
        public string MySql_Password { get; set; } = string.Empty;
        public string MySql_AdditionalParams { get; set; } = "CharSet=utf8mb4;Connect Timeout=30;";
        #endregion

        #region SQL Server 连接参数
        public string SqlServer_Server { get; set; } = "localhost"; // 可含端口（如：127.0.0.1,1433）
        public string SqlServer_Database { get; set; } = string.Empty;
        public string SqlServer_UserId { get; set; } = string.Empty;
        public string SqlServer_Password { get; set; } = string.Empty;
        public bool SqlServer_IntegratedSecurity { get; set; } = false; // 是否Windows身份验证
        public string SqlServer_AdditionalParams { get; set; } = "Connect Timeout=30;TrustServerCertificate=True;";
        #endregion

        #region SQLite 连接参数
        public string Sqlite_DbPath { get; set; } = "app.db"; // 数据库文件路径
        public string Sqlite_AdditionalParams { get; set; } = "Cache=Shared;";
        #endregion
    }
}

using FreeSql;
using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Hzexe.Xunlei_Library.Prot
{
    /// <summary>
    /// FreeSQL数据库上下文，替换原来的EF Core SqlContext
    /// </summary>
    public class FreeSqlContext
    {
        private static readonly object _lock = new object();
        private static IFreeSql _freeSql;
        private static string _connectionString;
        
        /// <summary>
        /// 根据配置计算数据库路径
        /// </summary>
        /// <param name="configuration">配置对象</param>
        /// <returns>数据库连接字符串</returns>
        public static string CalculateConnectionString(IConfiguration configuration)
        {
            string thunderFullPath = configuration.GetSection("XunleiPath").Value;
            var xlDir = new FileInfo(thunderFullPath).Directory.Parent.FullName;
            string dbpath = Path.Combine(xlDir, "profiles", "TaskDb.dat");
            if(!File.Exists(dbpath))
                throw new FileNotFoundException($"无法找到数据库文件: {dbpath}");
            return $"Data Source={dbpath};readOnly=true";
        }
        
        /// <summary>
        /// 获取FreeSQL实例
        /// </summary>
        public static IFreeSql FreeSql
        {
            get
            {
                if (_freeSql == null)
                {
                    lock (_lock)
                    {
                        if (_freeSql == null)
                        {
                            // 初始化FreeSQL，使用SQLite
                            _freeSql = new FreeSql.FreeSqlBuilder()
                                .UseConnectionString(DataType.Sqlite, _connectionString)
                                .UseAutoSyncStructure(true) // 自动同步实体结构到数据库
                                .UseNoneCommandParameter(true)
                                .Build();
                        }
                    }
                }
                return _freeSql;
            }
        }
        
        /// <summary>
        /// 获取任务仓储
        /// </summary>
        public static IBaseRepository<TaskBase> TaskRepository => FreeSql.GetRepository<TaskBase>();
    }
}
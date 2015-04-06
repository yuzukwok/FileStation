using FluentNHibernate.Cfg;
using NHibernate;
using NHibernate.Context;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace FileStation.Util.NH
{
    public class NHHelper
    {
        private static object _lock = new object();
        public static ISessionFactory GetCurrentFactory()
        {
            if (sessionFactory == null)
            {
                sessionFactory = CreateSessionFactory();
            }

            return sessionFactory;
        }

        private static ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure()
                .Database(
                    FluentNHibernate.Cfg.Db.MsSqlConfiguration.MsSql2008
                        .ConnectionString(ConfigurationManager.ConnectionStrings["DBStr"].ConnectionString)
                ).Mappings(m =>
                        m.FluentMappings
                        .AddFromAssembly(typeof(NHHelper).Assembly))
                .BuildSessionFactory();
        }
        private static ISessionFactory sessionFactory
        {
            get;
            set;
        }

        #region Session在当前上下文的操作
        private static void BindContext()
        {
            lock (_lock)
            {
                if (!CurrentSessionContext.HasBind(sessionFactory))
                {
                    CurrentSessionContext.Bind(sessionFactory.OpenSession());
                }
            }
        }

        private static void UnBindContext()
        {
            lock (_lock)
            {
                if (CurrentSessionContext.HasBind(sessionFactory))
                {
                    CurrentSessionContext.Unbind(sessionFactory);
                }
            }
        }

        public static void CloseCurrentSession()
        {
            UnBindContext();
        }

        public static ISession GetCurrentSession()
        {
            BindContext();
            return sessionFactory.GetCurrentSession();
        }
        #endregion

        #region 关闭SessionFactory（一般在应用程序结束时操作）
        public static void CloseSessionFactory()
        {
            if (!sessionFactory.IsClosed)
            {
                sessionFactory.Close();
            }
        }
        #endregion

        #region 打开一个新的Session
        public static ISession OpenSession()
        {
            lock (_lock)
            {
                return GetCurrentFactory().OpenSession();
            }
        }
        #endregion
    }
}

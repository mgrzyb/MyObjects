﻿#nullable enable
using System;
using System.Threading.Tasks;
using NHibernate;


namespace MyObjects.Testing
{
    public class TestFixtureBase
    {
        private ISessionFactory sessionFactory;
        private IInterceptor? interceptor;

        protected Dummy Dummy { get; private set; }

        public TestFixtureBase(Action<NHibernateConfigurationBuilder>? options = null)
        {
            var cfgBuilder = new NHibernateConfigurationBuilder(new UnitTestPersistenceStrategy());
            options?.Invoke(cfgBuilder);
            var cfg = cfgBuilder.Build();

            this.sessionFactory = cfg.BuildSessionFactory();            
        }

        protected void GivenInterceptor(IInterceptor interceptor)
        {
            this.interceptor = interceptor;
        }

        protected async Task Given(Func<ISession, Task> given)
        {
            using var session = this.OpenSession();
            using var transaction = session.BeginTransaction();
            var nHibernateSession = new NHibernateSession(session);
            this.Dummy = new Dummy(nHibernateSession);
            try
            {
                await given(nHibernateSession);
                transaction.Commit();
            }
            finally
            {
                this.Dummy = null;
            }
        }

        protected async Task<T> Given<T>(Func<ISession, Task<T>> given)
        {
            using var session = this.OpenSession();
            using var transaction = session.BeginTransaction();
            var nHibernateSession = new NHibernateSession(session);
            this.Dummy = new Dummy(nHibernateSession);
            try
            {
                var result = await given(nHibernateSession);
                transaction.Commit();
                return result;
            }
            finally
            {
                this.Dummy = null;
            }
        }

        protected async Task<T> Given<T>(Func<ISession, Dummy, Task<T>> given)
        {
            using var session = this.OpenSession();
            using var transaction = session.BeginTransaction();
            var nHibernateSession = new NHibernateSession(session);
            this.Dummy = new Dummy(nHibernateSession);
            try
            {
                var result = await given(nHibernateSession, this.Dummy);
                transaction.Commit();
                return result;
            }
            finally
            {
                this.Dummy = null;
            }
        }

        protected async Task When(Func<ISession, Task> when)
        {
            using var session = this.OpenSession();
            using var transaction = session.BeginTransaction();
            var nHibernateSession = new NHibernateSession(session);
            this.Dummy = new Dummy(nHibernateSession);
            try
            {
                await when(nHibernateSession);
            }
            finally
            {
                this.Dummy = null;
            }
            transaction.Commit();
        }

        protected async Task<T> When<T>(Func<ISession, Task<T>> given)
        {
            using var session = this.OpenSession();
            using var transaction = session.BeginTransaction();
            var nHibernateSession = new NHibernateSession(session);
            this.Dummy = new Dummy(nHibernateSession);
            try
            {
                var result = await given(nHibernateSession);
                transaction.Commit();
                return result;
            }
            finally
            {
                this.Dummy = null;
            }
        }

        protected async Task Then(Func<ISession, Task> then)
        {
            using var session = this.OpenSession();
            var nHibernateSession = new NHibernateSession(session);
            this.Dummy = new Dummy(nHibernateSession);
            try
            {
                await then(nHibernateSession);
            }
            finally
            {
                this.Dummy = null;
            }
        }

        private NHibernate.ISession OpenSession()
        {
            if (this.interceptor != null)
            {
                return this.sessionFactory.WithOptions().Interceptor(this.interceptor).OpenSession();
            }
            else
            {
                return this.sessionFactory.OpenSession();
            }
        }
    }
}
using System;
using System.Data.Entity;
using System.Transactions;
using INET;
using INET.Core;
using INET.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace INET.Tests
{
    public class BaseTest
    {
        protected TransactionScope _scope;
        protected INETContext _context;

        private bool _rollback = true;
        protected bool Rollback
        {
            get { return _rollback; }
            set { _rollback = value; }
        }

        [TestInitialize()]
        public void Initialize()
        {
            _context = new INETContext();
            _scope = CreateTransactionScope();            
        }
        
        [TestCleanup()]
        public void Cleanup()
        {
            if (Rollback)
                _scope.Dispose();
            else
                _scope.Complete();

            _context.Dispose();
        }

        public TransactionScope CreateTransactionScope()
        {
            var transactionOptions = new TransactionOptions();
            transactionOptions.IsolationLevel = IsolationLevel.ReadCommitted;
            transactionOptions.Timeout = TimeSpan.MaxValue;
            return new TransactionScope(TransactionScopeOption.Required, transactionOptions);
        }
    }
}

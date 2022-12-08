using AutoFixture;
using CautionaryAlertsListener.Infrastructure;
using Hackney.Shared.CautionaryAlerts.Factories;
using Hackney.Shared.CautionaryAlerts.Infrastructure;
using Hackney.Shared.Tenure.Boundary.Response;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CautionaryAlertsListener.Tests.E2ETests.Fixtures
{
    public class CautionaryAlertFixture : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly CautionaryAlertContext _dbContext;
        private bool _disposed;
        private string _defaultString;
        public PropertyAlertNew DbEntity { get; private set; }
        public int DbEntityId { get; private set; }

        public List<PropertyAlertNew> PersonsDbEntity { get; private set; } = new List<PropertyAlertNew>();

        public CautionaryAlertFixture(CautionaryAlertContext dbContext)
        {
            _dbContext = dbContext;
            _defaultString = string.Join("", _fixture.CreateMany<char>(CreateCautionaryAlertConstants.INCIDENTDESCRIPTIONLENGTH));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                if (null != DbEntity)
                    _dbContext.Remove(DbEntity);

                _disposed = true;
            }
        }

        private PropertyAlertNew ConstructAndSaveCautionaryAlert(int id)
        {
            var cautionaryAlert = CreateCautionaryAlertFixture
                .GenerateValidCreateCautionaryAlertFixture(_defaultString, _fixture);
            var dbEntity = cautionaryAlert.ToDatabase();
            _dbContext.PropertyAlerts.Add(dbEntity);
            _dbContext.SaveChanges();
            return dbEntity;
        }

        public void GivenTheCautionaryAlertAlreadyExist(int id)
        {
            if (null == DbEntity)
            {
                var entity = ConstructAndSaveCautionaryAlert(id);
                DbEntity = entity;
                DbEntityId = entity.Id;
            }
        }

        public void GivenACautionaryAlertDoesNotExist(int id)
        {
            // Nothing to do here
        }
    }
}

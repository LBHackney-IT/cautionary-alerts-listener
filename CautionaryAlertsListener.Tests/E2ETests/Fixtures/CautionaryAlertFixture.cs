using AutoFixture;
using CautionaryAlertsListener.Infrastructure;
using Hackney.Shared.CautionaryAlerts.Factories;
using Hackney.Shared.CautionaryAlerts.Infrastructure;
using Hackney.Shared.CautionaryAlerts;
using System;
using System.Collections.Generic;
using static CautionaryAlertsListener.Tests.CreateCautionaryAlertsFixture;

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
            _defaultString = string.Join("", _fixture.CreateMany<char>(CautionaryAlertConstants.INCIDENTDESCRIPTIONLENGTH));
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

        private PropertyAlertNew ConstructAndSaveCautionaryAlertMMHIDOptionalPropertyReference(Guid mmhId, string propertyReference = null)
        {
            var addressString = string.Join("", _fixture.CreateMany<char>(CautionaryAlertConstants.FULLADDRESSLENGTH));
            var cautionaryAlert = CreateCautionaryAlertFixture.GenerateValidCreateCautionaryAlertFixture(_defaultString, _fixture, addressString, mmhId, propertyReference);
            var dbEntity = cautionaryAlert.ToDatabase(isActive: true, Guid.NewGuid().ToString());
            _dbContext.PropertyAlerts.Add(dbEntity);
            _dbContext.SaveChanges();
            return dbEntity;
        }

        public void GivenTheCautionaryAlertAlreadyExist(Guid mmhId, string propertyReference = null)
        {
            if (DbEntity == null)
            {
                var entity = ConstructAndSaveCautionaryAlertMMHIDOptionalPropertyReference(mmhId, propertyReference);
                DbEntity = entity;
                DbEntityId = entity.Id;
            }
        }

        public void GivenACautionaryAlertDoesNotExistForPerson(Guid mmhId)
        {
            // Nothing to do here
        }
    }
}

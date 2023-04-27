using AutoFixture;
using Bogus;
using CautionaryAlertsListener.Gateway;
using FluentAssertions;
using Hackney.Core.Testing.Shared;
using Hackney.Shared.CautionaryAlerts.Factories;
using Hackney.Shared.CautionaryAlerts.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CautionaryAlertsListener.Tests.CreateCautionaryAlertsFixture;

namespace CautionaryAlertsListener.Tests.Gateway
{
    [TestFixture]
    public class CautionaryAlertGatewayTests : DatabaseTests
    {
        private CautionaryAlertGateway _classUnderTest;

        private Fixture _fixture;
        private readonly Random _random = new Random();
        private readonly Faker _faker = new Faker();
        private Mock<ILogger<CautionaryAlertGateway>> _mockedLogger;
        private string _defaultString;

        [SetUp]
        public void Setup()
        {
            LogCallAspectFixture.SetupLogCallAspect();
            _mockedLogger = new Mock<ILogger<CautionaryAlertGateway>>();
            _classUnderTest = new CautionaryAlertGateway(CautionaryAlertContext, _mockedLogger.Object);
            _fixture = new Fixture();
            _defaultString = string.Join("", _fixture.CreateMany<char>(CautionaryAlertConstants.INCIDENTDESCRIPTIONLENGTH));
        }

        [Test]
        public async Task GetEntitiesByMMHIdAndPropertyReferenceAsyncReturnsNullIfNoneFound()
        {
            var response = await _classUnderTest.GetEntitiesByMMHIdAndPropertyReferenceAsync(_fixture.Create<Guid>().ToString());
            response.Should().BeEmpty();
        }

        [Test]
        public async Task GetEntitiesByMMHIdAndPropertyReferenceAsyncReturnsCollectionIfFoundOnMMHID()
        {
            var dbEntity = await AddAlertToDb();
            var response = await _classUnderTest.GetEntitiesByMMHIdAndPropertyReferenceAsync(dbEntity.MMHID);
            response.Should().NotBeNull();
            response.Should().NotBeEmpty();
            response.Should().BeEquivalentTo(new List<PropertyAlertNew> { dbEntity });
        }

        [Test]
        public async Task GetEntitiesByMMHIdAndPropertyReferenceAsyncReturnsCollectionIfFoundOnMMHIDAndPropertyReference()
        {
            var dbEntity = await AddAlertToDb();
            var response = await _classUnderTest.GetEntitiesByMMHIdAndPropertyReferenceAsync(dbEntity.MMHID, dbEntity.PropertyReference);
            response.Should().NotBeEmpty();
            response.Should().BeEquivalentTo(new List<PropertyAlertNew> { dbEntity });
        }

        [Test]
        public async Task GetEntitiesByMMHIdAndPropertyReferenceAsyncThrowsOnMissingMMHID()
        {
            Func<Task> func = async () => await _classUnderTest.GetEntitiesByMMHIdAndPropertyReferenceAsync(null).ConfigureAwait(false);
            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [Test]
        public async Task SaveCautionaryAlertTestUpdatesDatabase()
        {
            var dbEntity = await AddAlertToDb();
            var updatedPersonName = _fixture.Create<string>();
            dbEntity.PersonName = updatedPersonName;
            await CautionaryAlertContext.SaveChangesAsync();

            var responseCollection = await _classUnderTest.GetEntitiesByMMHIdAndPropertyReferenceAsync(dbEntity.MMHID);
            responseCollection.Should().NotBeEmpty();
            foreach (var item in responseCollection)
            {
                item.PersonName.Should().BeEquivalentTo(updatedPersonName);
            }
        }

        private async Task<PropertyAlertNew> AddAlertToDb()
        {
            var addressString = string.Join("", _fixture.CreateMany<char>(CautionaryAlertConstants.FULLADDRESSLENGTH));
            var alert = CreateCautionaryAlertFixture.GenerateValidCreateCautionaryAlertFixture(_defaultString, _fixture, addressString);
            var dbEntity = alert.ToDatabase(isActive: true, Guid.NewGuid().ToString());
            CautionaryAlertContext.PropertyAlerts.Add(dbEntity);
            await CautionaryAlertContext.SaveChangesAsync();
            return dbEntity;
        }
    }
}

using CautionaryAlertsListener.UseCase.Interfaces;
using Hackney.Core.Sns;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CautionaryAlertsListener.Factories
{
    public static class UseCaseFactory
    {
        public static IMessageProcessing CreateUseCaseForMessage(this EntityEventSns entityEvent, IServiceProvider serviceProvider)
        {
            if (entityEvent is null) throw new ArgumentNullException(nameof(entityEvent));
            if (serviceProvider is null) throw new ArgumentNullException(nameof(serviceProvider));

            switch (entityEvent.EventType)
            {
                case EventTypes.PersonAddedToTenureEvent:
                    {
                        return serviceProvider.GetService<IAddPersonToTenureUseCase>();
                    }
                case EventTypes.PersonRemovedFromTenureEvent:
                    {
                        return serviceProvider.GetService<IRemovePersonFromTenureUseCase>();
                    }
                case EventTypes.PersonUpdatedEvent:
                    {
                        return serviceProvider.GetService<IPersonUpdatedUseCase>();
                    }
                default:
                    throw new ArgumentException($"Unknown event type: {entityEvent.EventType}");
            }
        }
    }
}

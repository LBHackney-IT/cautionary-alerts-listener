using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using CautionaryAlertsListener.Boundary;
using CautionaryAlertsListener.Factories;
using CautionaryAlertsListener.Gateway;
using CautionaryAlertsListener.Gateway.Interfaces;
using CautionaryAlertsListener.Infrastructure;
using CautionaryAlertsListener.UseCase;
using CautionaryAlertsListener.UseCase.Interfaces;
using Hackney.Core.DynamoDb;
using Hackney.Core.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace CautionaryAlertsListener
{
    [ExcludeFromCodeCoverage]
    public class CautionaryAlertsListener : BaseFunction
    {
        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public CautionaryAlertsListener()
        { }


        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddScoped<IAddPersonToTenureUseCase, AddPersonToTenureUseCase>();
            services.AddScoped<IRemovePersonFromTenureUseCase, RemovePersonFromTenureUseCase>();
            services.AddScoped<IPersonUpdatedUseCase, PersonUpdatedUseCase>();
            services.AddScoped<ICautionaryAlertGateway, CautionaryAlertGateway>();
            ConfigureDbContext(services);

            base.ConfigureServices(services);
        }

        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            // Do this in parallel???
            foreach (var message in evnt.Records)
            {
                await ProcessMessageAsync(message, context).ConfigureAwait(false);
            }
        }

        [LogCall(LogLevel.Information)]
        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            context.Logger.LogLine($"Processing message {message.MessageId}");

            var entityEvent = JsonSerializer.Deserialize<EntityEventSns>(message.Body, _jsonOptions);

            using (Logger.BeginScope("CorrelationId: {CorrelationId}", entityEvent.CorrelationId))
            {
                try
                {
                    IMessageProcessing processor = entityEvent.CreateUseCaseForMessage(ServiceProvider);
                    if (processor != null)
                        await processor.ProcessMessageAsync(entityEvent).ConfigureAwait(false);
                    else
                        Logger.LogInformation($"No processors available for message so it will be ignored. " +
                            $"Message id: {message.MessageId}; type: {entityEvent.EventType}; version: {entityEvent.Version}; entity id: {entityEvent.EntityId}");

                    await processor.ProcessMessageAsync(entityEvent).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Exception processing message id: {message.MessageId}; type: {entityEvent.EventType}; entity id: {entityEvent.EntityId}");
                    throw; // AWS will handle retry/moving to the dead letter queue
                }
            }
        }

        private static void ConfigureDbContext(IServiceCollection services)
        {
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

            services.AddDbContext<CautionaryAlertContext>(
                opt => opt.UseNpgsql(connectionString));
        }
    }
}

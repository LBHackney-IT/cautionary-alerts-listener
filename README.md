# Cautionary Alerts Listener

AWS Lambda that keeps cautionary-alert records in Postgres up to date with person and tenure changes published by Housing services at the London Borough of Hackney.

Person and tenure SNS topics deliver events to a FIFO SQS queue. The Lambda reads each message and updates matching rows in `dbo."PropertyAlertNew"` on the `uh_mirror` database. When a person joins or leaves a tenure it also calls the Tenure API for the tenured asset's property reference, UPRN, and address.

Failed messages are retried by SQS and, after three receives, moved to a FIFO dead-letter queue.

## Events

| Event type | Action |
| --- | --- |
| `PersonAddedToTenureEvent` | Finds the household member added to the tenure, loads their cautionary alerts by MMH id, and writes the tenure's property reference, UPRN, and address onto the first matching alert. |
| `PersonRemovedFromTenureEvent` | Finds the household member removed from the tenure and clears address, property reference, and UPRN on alerts for that person at that property. |
| `PersonUpdatedEvent` | When the person's first or last name has changed, updates `person_name` on every alert for that MMH id. |
| `PersonCreatedEvent`, `TenureCreatedEvent`, `TenureUpdatedEvent` | Logged and ignored. |
| Any other type | Throws, so the message is retried and can land on the dead-letter queue. |

If an event has no matching cautionary alerts, processing stops without writing. A missing tenure, or a household-member change that cannot be read from the event payload, fails the message.

## Stack

- .NET 10 AWS Lambda, triggered by SQS (`Amazon.Lambda.SQSEvents`)
- Entity Framework Core and Npgsql against PostgreSQL
- Tenure API via `Hackney.Core.Http`
- Serverless Framework for the Lambda, Terraform for the queue, SNS subscriptions, and CloudWatch dashboard
- xUnit, Moq, FluentAssertions, and BDDfy
- CircleCI for format checks, tests, and deployment

The function runs in `eu-west-2` with 2048 MB of memory and a 30 second timeout.

## Configuration

Deployed values come from SSM Parameter Store and are set in `CautionaryAlertsListener/serverless.yml`.

| Variable | Purpose |
| --- | --- |
| `CONNECTION_STRING` | Postgres connection for `uh_mirror` |
| `TenureApiUrl` | Base URL of the Tenure API |
| `TenureApiToken` | Token used when calling the Tenure API |
| `ENVIRONMENT` | Selects `appsettings.{ENVIRONMENT}.json` when running locally (`LocalDevelopment`) |

Private Hackney NuGet packages need `LBHPACKAGESTOKEN` in the environment before restore, Docker build, or CI.

## Project layout

```
CautionaryAlertsListener/            Lambda project
  CautionaryAlertsListener.cs        SQS handler
  Factories/UseCaseFactory.cs        Maps event type to a use case
  UseCase/                           Person added, removed, and updated
  Gateway/                           Postgres and Tenure API
  Infrastructure/                    DbContext and event helpers
CautionaryAlertsListener.Tests/      Unit and BDDfy end-to-end tests
database/                            Local Postgres schema
terraform/                           development, staging, production
```

## Local development

Install [Docker](https://www.docker.com/products/docker-desktop), the [.NET 10 SDK](https://dotnet.microsoft.com/download), and set `LBHPACKAGESTOKEN`.

Build and run the listener with a local Postgres instance:

```sh
docker compose build cautionary-alerts-listener
docker compose up cautionary-alerts-listener
```

`make build` and `make serve` do the same thing. The local database connection string is set in `docker-compose.yml`.

To invoke the handler from an IDE, use the **Mock Lambda Test Tool** launch profile in `CautionaryAlertsListener/Properties/launchSettings.json`. It expects `dotnet-lambda-test-tool-10.0` on the machine and sets `ENVIRONMENT` to `LocalDevelopment`.

## Tests

Unit tests cover each use case, the gateways, and event routing. End-to-end tests build an SQS event and call `FunctionHandler` against the local Postgres schema.

```sh
docker compose build cautionary-alerts-listener-test
docker compose run cautionary-alerts-listener-test
```

The test image also runs a SonarCloud scan when `SONAR_TOKEN` is set.

Format check (CI treats formatting failures as a failed job):

```sh
dotnet tool install -g dotnet-format
dotnet format --check
```

`make lint` installs or updates `dotnet-format` and applies formatting.

## Deployment

CircleCI runs on pull requests, `master`, and `release`.

1. `dotnet format --check` and the Dockerised test suite run on every branch except `master` and `release`.
2. A merge to `master` plans and applies development Terraform, then deploys the Lambda with `sls deploy --stage development`.
3. A merge to `release` does the same for staging. Production Terraform and the production Lambda deploy each wait for a manual approval in CircleCI.

Terraform (`terraform/development`, `terraform/staging`, `terraform/production`) creates:

- FIFO queue `cautionaryalertsqueue.fifo` and dead-letter queue `cautionaryalertsdeadletterqueue.fifo`
- Subscriptions to the person and tenure SNS topics, with raw message delivery
- The SSM parameter `/sqs-queue/{stage}/cautionary_alerts_listener_queue/arn`
- A CloudWatch listener dashboard

`CautionaryAlertsListener/build.sh` packages the Lambda as `bin/release/net10.0/cautionary-alerts-listener.zip` for Serverless. The deployed function name is `cautionary-alerts-listener-{stage}`.

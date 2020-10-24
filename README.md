# Platform integration testing template

Based on the Nexus concept and the thoughts described here: [Platform Integration Testing Overview](https://docs.nexus.link/docs/platformtesting-overview)

## Template

### Tasks
There are placeholders in the template code starting with "TASK: ", which you should take action upon when  creating your own repo.

* The template uses the authentication role "business-api-caller". Change this to match your platform.

### Running the template 
This template is setup to run for tenant "platform-integration-test-template-service/dev" in Nexus PRDSIM.

1. Run the Mocks project (with dotnet, not IISExpress)
2. Run the Service project (with dotnet, not IISExpress)
3. In swagger of Service, authorize (credentials found in appsettings.Development.json; Service:Username, Service:Password)
4. Try out /api/v1/AllTests
5. Get test results with /api/v1/Tests/{id}

### TODOs

* Logging
* Stubs for Business Api tests
* Stubs for Business Processes tests
* Multi-environment support?

## Support for multiple instances

The template can be used in two different modes, with a physical (sql or table) or a memory storage. When using a database you get support for running the test service on multiple instances.

## Memory storage

Locally and in single instance environments the service can be run with memory storage. This is the default mode.

## Database

You can provide the app setting `SqlConnectionString` to use an Azure SQL database.
(If you also provide a `MasterConnectionString`, the database will be created automatically for you.)

Or provide `StorageConnectionString` to use Azure Table Storage.

### Purging the database

We don't need old test data, so a background process will get rid of test results that are old.

## Test types

### Capability contract tests

Found in `ContractTests` namespace. See also [Test capability contracts](https://docs.nexus.link/docs/platformtesting-capability-contract-testing).

### Business API tests

Found in `BusinessApiTests` namespace. See [Test configuration of the platform](https://docs.nexus.link/docs/platformtesting-configuration-of-the-platform).

### Business processes tests

Found in `BusinessProcessesTests` namespace. See [Test business processes](https://docs.nexus.link/docs/platformtesting-business-processes).

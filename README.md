# Platform integration testing template

Based on the Nexus concept and the thoughts described here: [Testing overview](https://docs.nexus.link/docs/platformtesting-overview)

## Support for multiple instances

The template can be used in two different modes, with a database or a memory storage. When using a database you get support for running the test service on multiple instances.

## Memory storage

Locally and in single instance environments the service can be run with memory storage. This is the default mode.

## Database

TODO: Choose option

OPTION 1: Cosmos DB in MongoDB mode. Or Cassandra mode?

OPTION 2: Nexus DatabasePatcher is used to keep an SQL database schema in sync at startup. To use a database, add the `ConnectionString` app setting.

OPTION 3: Table storage

### Purging the database

We don't need old test data, so a background process will get rid of test results that are old.

## Test types

### Capability contract tests

Found in `ContractTests` namespace. See also [Test capability contracts](https://docs.nexus.link/docs/platformtesting-capability-contract-testing).

### Business API tests

Found in `BusinessApiTests` namespace. See [Test configuration of the platform](https://docs.nexus.link/docs/platformtesting-configuration-of-the-platform).

### Business processes tests

Found in `BusinessProcessesTests` namespace. See [Test business processes](https://docs.nexus.link/docs/platformtesting-business-processes).
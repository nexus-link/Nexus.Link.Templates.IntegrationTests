DECLARE @Organization NVARCHAR(255) = 'platform-integration-test-template-service'
DECLARE @Environment NVARCHAR(255) = 'dev'

--Keep it self contained, delete only what we know we added
DELETE FROM ServiceTenantConfiguration WHERE Organization = @Organization AND Environment = @Environment

INSERT INTO ServiceTenantConfiguration(Organization, Environment, Service, Configuration)
	VALUES (@Organization, @Environment, 'fundamentals', '{
        "AuthenticationConnectionString": "Server=tcp:smoketestingcompany-dbserver.database.windows.net,1433;Initial Catalog=authentication;Persist Security Info=False;User ID=db-admin;Password=NiceChair!Soft99;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
        "LoggerConnectionString": "DefaultEndpointsProtocol=https;AccountName=nexusplinttesttemplate;AccountKey=CGEQuE3eZvX3yeWzH5dYA5U8op206qJsEXQqCMR+RQnKGkUZeRLMW3ppPBqhaEwaXaMOfkuxvBaxRkij6jXfpA==;EndpointSuffix=core.windows.net",
        "QueueName": "platform-integration-test-template-service-logging"
     }')

INSERT INTO ServiceTenantConfiguration(Organization, Environment, Service, Configuration)
	VALUES (@Organization, @Environment, 'logging', '{
        "Version": "1",
        "LoggerConnectionString": "DefaultEndpointsProtocol=https;AccountName=nexusplinttesttemplate;AccountKey=CGEQuE3eZvX3yeWzH5dYA5U8op206qJsEXQqCMR+RQnKGkUZeRLMW3ppPBqhaEwaXaMOfkuxvBaxRkij6jXfpA==;EndpointSuffix=core.windows.net",
        "QueueName": "platform-integration-test-template-service-logging"
     }')

INSERT INTO ServiceTenantConfiguration(Organization, Environment, Service, Configuration)
	VALUES (@Organization, @Environment, 'businessevents', '{
        "ConnectionString": "Server=tcp:smoketestingcompany-dbserver.database.windows.net,1433;Initial Catalog=nexus-platform-integration-test-template-service-businessevents;Persist Security Info=False;User ID=db-admin;Password=NiceChair!Soft99;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
        "VerifyPublications": true
    }')

INSERT INTO ServiceTenantConfiguration(Organization, Environment, Service, Configuration)
	VALUES (@Organization, @Environment, 'keytranslator', '{
        "ConnectionString": "Server=tcp:smoketestingcompany-dbserver.database.windows.net,1433;Initial Catalog=nexus-platform-integration-test-template-service-translations;Persist Security Info=False;User ID=db-admin;Password=NiceChair!Soft99;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
    }')

INSERT INTO ServiceTenantConfiguration(Organization, Environment, Service, Configuration)
    VALUES (@Organization, @Environment, 'asynccaller', '{
        "SchemaVersion": 1,
        "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=nexusplinttesttemplate;AccountKey=CGEQuE3eZvX3yeWzH5dYA5U8op206qJsEXQqCMR+RQnKGkUZeRLMW3ppPBqhaEwaXaMOfkuxvBaxRkij6jXfpA==;EndpointSuffix=core.windows.net",
        "DefaultDeadlineTimeSpanInSeconds": 240,
    }')
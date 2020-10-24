DECLARE @Organization NVARCHAR(255) = 'platform-integration-test-template-service'
DECLARE @Environment NVARCHAR(255) = 'dev'

--Keep it self contained, delete only what we know we added
DELETE FROM ServiceTenantConfiguration WHERE Organization = @Organization AND Environment = @Environment

INSERT INTO ServiceTenantConfiguration(Organization, Environment, Service, Configuration)
	VALUES (@Organization, @Environment, 'fundamentals', '{
        "Service": "fundamentals",
        "Configuration": {
            "AuthenticationConnectionString": "Server=tcp:smoketestingcompany-dbserver.database.windows.net,1433;Initial Catalog=authentication;Persist Security Info=False;User ID=db-admin;Password=NiceChair!Soft99;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
            "LoggerConnectionString": "DefaultEndpointsProtocol=https;AccountName=smoketestcompany;AccountKey=F+QXBiz3KLrUbWrqB8HbZa9y080KImcLHPumw7PuXqLGgttjhTFQ32CqycA8S3UOL0wv3rXN/N31DRZm9x0arQ==;EndpointSuffix=core.windows.net",
            "QueueName": "platform-integration-test-template-service-logging"
          }
      }')


INSERT INTO ServiceTenantConfiguration(Organization, Environment, Service, Configuration)
	VALUES (@Organization, @Environment, 'businessevents', '{
        "Service": "businessevents",
        "Configuration": {
            "ConnectionString": "Server=tcp:smoketestingcompany-dbserver.database.windows.net,1433;Initial Catalog=nexus-platform-integration-test-template-service-businessevents;Persist Security Info=False;User ID=db-admin;Password=NiceChair!Soft99;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
            "VerifyPublications": true
          }
      }')

-- Platform authentication (Authentication as a service)

DECLARE @Organization NVARCHAR(255) = 'platform-integration-test-template-service'
DECLARE @Environment NVARCHAR(255) = 'dev'

-- Keep it self contained, delete only what we know we added
DELETE FROM ApiUser_Role WHERE ApiUserId IN (SELECT Id FROM ApiUser WHERE Organization = @organization AND Environment = @environment)
DELETE FROM ApiUser WHERE Organization = @organization AND Environment = @environment

DECLARE @role NVARCHAR(255) = 'business-api-caller'
IF NOT EXISTS (SELECT 1 FROm Role WHERE Name = @role)
BEGIN
	INSERT INTO Role (Name) VALUES (@role)
END

DECLARE @id UNIQUEIDENTIFIER

-- { "ClientId": "integration-test", "ClientSecret": "90u09khu7" }
SET @id = newid()
INSERT INTO ApiUser (Id, Organization, Environment, Name, Salt, HashedSecret) VALUES (@id, @organization, @environment, 'integration-test', 'SysBfb6NXJP;GA8p', '5tZVVxzbqzhUmQKg9xjmEpHavPx5T0esFK5iRNMK2Sk=')
INSERT INTO ApiUser_Role (ApiUserId, RoleId) SELECT @id, Id FROM Role WHERE Name = 'business-api-caller'

-- { "ClientId": "capability1-mock", "ClientSecret": "lklkf7633k" }
SET @id = newid()
INSERT INTO ApiUser (Id, Organization, Environment, Name, Salt, HashedSecret) VALUES (@id, @organization, @environment, 'capability1-mock', 'KLf83,mfFFffaedfe', 'K5J+My3i7zotQ7mLgmxvAri/xgcnqvaHW8+QbYtLUtg=')
INSERT INTO ApiUser_Role (ApiUserId, RoleId) SELECT @id, Id FROM Role WHERE Name = 'business-api-caller'

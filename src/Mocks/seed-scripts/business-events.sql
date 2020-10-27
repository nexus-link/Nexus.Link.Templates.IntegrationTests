DECLARE @Organization NVARCHAR(255) = 'platform-integration-test-template-service'
DECLARE @Environment NVARCHAR(255) = 'dev'

---------------------------------------------------------------------------------------------------
-- Clients
---------------------------------------------------------------------------------------------------
DECLARE @capability1mock UNIQUEIDENTIFIER
EXEC CreateOrGetClient 'capability1-mock', @capability1mock OUTPUT
DECLARE @dashboard UNIQUEIDENTIFIER
EXEC CreateOrGetClient 'dashboard', @dashboard OUTPUT

---------------------------------------------------------------------------------------------------
-- PUBLICATIONS
---------------------------------------------------------------------------------------------------
DECLARE @majorId UNIQUEIDENTIFIER
DECLARE @minorId UNIQUEIDENTIFIER
DECLARE @publicationId UNIQUEIDENTIFIER
DECLARE @url NVARCHAR(1024)
DECLARE @priority INT

---------------------------------------------------------------------------------------------------
-- Order.Created
---------------------------------------------------------------------------------------------------
EXEC CreateOrGetEventWithVersion 'Order', 'Created', 1, 0, '{
	"OrderId": "string",
	"Items": "int",
	"Status": "order.status"
}', @majorId OUTPUT, @minorId OUTPUT

SET @publicationId = 'E9D8415A-3C06-4831-9CF5-07CD3D275295'
IF ((SELECT count(*) FROM Publication WHERE Id = @publicationId) = 0)
	INSERT INTO Publication (Id, ClientId, EventMinorVersionId) VALUES (@publicationId, @capability1mock, @minorId)

SET @url = 'https://nexus-link-dev-signalr.azurewebsites.net/proxy/v2/platform-integration-tests-demo/https://host.docker.internal:5021/api/Events/Order/Created/1'
EXEC CreateOrUpdateSubscription @dashboard, @publicationId, @url

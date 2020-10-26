---------------------------------------------------------------------------------------------------
-- CLIENTS
---------------------------------------------------------------------------------------------------
DECLARE @capability1mock UNIQUEIDENTIFIER
EXEC CreateOrGetClient 'capability1-mock', @capability1mock OUTPUT

DECLARE @capability2mock UNIQUEIDENTIFIER
EXEC CreateOrGetClient 'capability2-mock', @capability2mock OUTPUT

DECLARE @dashboard UNIQUEIDENTIFIER
EXEC CreateOrGetClient 'dashboard', @dashboard OUTPUT

---------------------------------------------------------------------------------------------------
-- CONCEPTS
---------------------------------------------------------------------------------------------------
DECLARE @conceptId UNIQUEIDENTIFIER
DECLARE @contextId UNIQUEIDENTIFIER

---------------------------------------------------------------------------------------------------
-- order.status
---------------------------------------------------------------------------------------------------
EXEC CreateOrGetEnumConcept 'order.status', @conceptId OUTPUT

-- Forms
DECLARE @orderStatusFormNewId UNIQUEIDENTIFIER
DECLARE @orderStatusFormProcessingId UNIQUEIDENTIFIER
DECLARE @orderStatusFormDoneId UNIQUEIDENTIFIER
EXEC CreateOrGetForm @conceptId, 'OrderStatusNew', @orderStatusFormNewId OUTPUT
EXEC CreateOrGetForm @conceptId, 'OrderStatusProcessing', @orderStatusFormProcessingId OUTPUT
EXEC CreateOrGetForm @conceptId, 'OrderStatusDone', @orderStatusFormDoneId OUTPUT

-- capability 1 context
EXEC CreateOrGetContext @conceptId, 'capability1', 1, @contextId OUTPUT
EXEC CreateOrUpdateClientContext @capability1mock, @conceptId, @contextId
EXEC CreateOrUpdateClientContext @dashboard, @conceptId, @contextId

EXEC CreateOrUpdateInstance @contextId, @orderStatusFormNewId, 'Created'
EXEC CreateOrUpdateInstance @contextId, @orderStatusFormProcessingId, 'Processing'
EXEC CreateOrUpdateInstance @contextId, @orderStatusFormDoneId, 'Done'

-- capability 2 context
EXEC CreateOrGetContext @conceptId, 'capability2', 1, @contextId OUTPUT
EXEC CreateOrUpdateClientContext @capability2mock, @conceptId, @contextId

EXEC CreateOrUpdateInstance @contextId, @orderStatusFormNewId, 'New'
EXEC CreateOrUpdateInstance @contextId, @orderStatusFormProcessingId, 'Doing'
EXEC CreateOrUpdateInstance @contextId, @orderStatusFormDoneId, 'Finished'

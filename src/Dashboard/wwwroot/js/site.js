// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

Site = (() => {
    return {
        resetEvents: () => {
            var xhr = new XMLHttpRequest();
            xhr.open("POST", "api/Events/Reset");
            xhr.send(null);
        },

        sendTestEvent: () => {
            const testEvents = [
                { EntityName: "Person", EventName: "FellAsleep" },
                { EntityName: "Person", EventName: "WokeUp" },
                { EntityName: "ToothFairy", EventName: "DeployedCoin" },
            ];
            const testEvent = testEvents[Math.floor(Math.random() * testEvents.length)];
            var xhr = new XMLHttpRequest();
            xhr.open("POST", `api/Events/${testEvent.EntityName}/${testEvent.EventName}/1`);
            xhr.send(null);
        }
    }
})();
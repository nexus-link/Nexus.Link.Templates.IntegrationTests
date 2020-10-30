const $ = (selector) => {
    const result = document.querySelectorAll(selector);
    if (result.length === 0) return null;
    if (result.length === 1) return result[0];
    return Array.from(result);
}


const Site = (() => {
    return {
        resetEvents: () => {
            var xhr = new XMLHttpRequest();
            xhr.open("POST", "/api/Events/Reset");
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
            xhr.open("POST", `/api/Events/${testEvent.EntityName}/${testEvent.EventName}/1`);
            xhr.send(null);
        }
    }
})();


const Api = (() => {

    return {
        call: (method, url, callbacks) => {
            var xhr = new XMLHttpRequest();
            xhr.onreadystatechange = function () {
                if (xhr.readyState !== 4) return;
                if (xhr.status >= 200 && xhr.status < 300) {
                    if (callbacks && callbacks.ok) callbacks.ok(xhr);
                } else {
                    if (callbacks && callbacks.error) callbacks.error(xhr);
                }
                if (callbacks && callbacks.always) callbacks.always(xhr);
            };
            xhr.open(method, url);
            xhr.send();
        }
    }
})();


const Storage = (() => {

    const storageKey = "testContexts";

    const getTests = () => {
        const testContextsString = sessionStorage.getItem(storageKey);
        if (!testContextsString) return null;
        return JSON.parse(testContextsString);
    }
    const storeTests = (testContexts) => {
        sessionStorage.setItem(storageKey, JSON.stringify(testContexts));
    }
    const addTest = (testId, endPollingAt) => {
        let testContexts = getTests();
        if (!testContexts) testContexts = [];
        testContexts.push({ testId: testId, endPollingAt: endPollingAt });
        storeTests(testContexts);
    }
    const removeTest = (testId) => {
        const testContexts = getTests();
        if (!testContexts) return;
        let spliceAt = null;
        testContexts.forEach((test, index) => {
            if (test.testId === testId) spliceAt = index;
        });
        if (spliceAt != null) {
            testContexts.splice(spliceAt, 1);
            storeTests(testContexts);
        }
    }
    const getTest = (testId) => {
        const testContexts = getTests();
        if (!testContexts) return null;
        for (let i = 0; i < testContexts.length; i++) {
            const context = testContexts[i];
            if (context.testId === testId) return context;
        }
        return null;
    }

    return {
        getTests: getTests,
        storeTests: storeTests,
        addTest: addTest,
        removeTest: removeTest,
        getTest: getTest
    }
})();


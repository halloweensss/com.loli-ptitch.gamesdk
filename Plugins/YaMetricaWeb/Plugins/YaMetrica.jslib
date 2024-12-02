var YaMetrica = {
    $yaMetrica: {
        CounterId: 0,
        DebugMode: false, // Переменная для включения/выключения дебага

        log: function (...args) {
            if (this.DebugMode) {
                console.log("[YaMetrica]:", ...args);
            }
        },
        
        error: function (...args) {
            if (this.DebugMode) {
                console.error("[YaMetrica]:", ...args);
            }
        },

        Initialize: function (data, callback) {
            try {
                const dataStr = UTF8ToString(data);
                const dataObj = JSON.parse(dataStr);

                this.CounterId = dataObj.CounterId;
                this.DebugMode = dataObj.DebugMode || false;

                this.log("YaMetrica initialized with CounterId:", this.CounterId);
                if (callback) dynCall('v', callback, []);
            } catch (error) {
                console.error("Error initializing YaMetrica:", error);
            }
        },

        SendEventParams: function (eventName, parameters) {
            try {
                const eventNameStr = UTF8ToString(eventName);
                const parametersStr = UTF8ToString(parameters);
                const parametersObj = JSON.parse(parametersStr);

                this.log("Sending event:", eventNameStr, parametersObj);

                ym(this.CounterId, 'reachGoal', eventNameStr, parametersObj, () => {
                    this.log("Event sent:", eventNameStr);
                });
            } catch (error) {
                this.error("Error sending event:", error);
            }
        },
        
        SendEvent: function (eventName) {
            try {
                const eventNameStr = UTF8ToString(eventName);

                this.log("Sending event:", eventNameStr);

                ym(this.CounterId, 'reachGoal', eventNameStr, {}, () => {
                    this.log("Event sent:", eventNameStr);
                });
            } catch (error) {
                this.error("Error sending event:", error);
            }
        },
    },

    YaMetricaInitialize: function (data, callback) {
        yaMetrica.Initialize(data, callback);
    },

    YaMetricaSendParamsEvent: function (eventName, data) {
        yaMetrica.SendEventParams(eventName, data);
    },
    
    YaMetricaSendEvent: function (eventName) {
        yaMetrica.SendEvent(eventName);
    },
};

autoAddDeps(YaMetrica, '$yaMetrica');
mergeInto(LibraryManager.library, YaMetrica);
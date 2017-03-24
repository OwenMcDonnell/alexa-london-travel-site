﻿// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

module.exports = function (config) {
    config.set({

        autoWatch: false,
        concurrency: Infinity,

        browsers: ["PhantomJS"],
        frameworks: ["jasmine"],

        files: [
            "wwwroot/lib/**/dist/*.js",
            "assets/scripts/js/martinCostello/martinCostello.js",
            "assets/scripts/ts/martinCostello/londonTravel/Debug.js",
            "assets/scripts/ts/martinCostello/londonTravel/Manage.js",
            "assets/scripts/ts/martinCostello/londonTravel/Preferences.js",
            "assets/scripts/ts/martinCostello/londonTravel/Tracking.js",
            "assets/scripts/**/*.spec.js"
        ],

        htmlDetailed: {
            splitResults: false
        },

        plugins: [
            "karma-appveyor-reporter",
            "karma-chrome-launcher",
            "karma-html-detailed-reporter",
            "karma-jasmine",
            "karma-phantomjs-launcher"
        ],

        reporters: [
            "progress",
            "appveyor"
        ]
    });
};

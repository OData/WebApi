module("EntitySet.js");

(function (global, upshot, undefined) {

    function mockDs() {
        var mockDs = new upshot.DataContext();
        upshot.metadata("mockType", { key: ["mockId"] });
        return mockDs;
    }

    function mockType() {
        return "mockType";
    }

    function mockNewEs() {
        return new upshot.EntitySet(mockDs(), mockType());
    }

    test("EntitySource Bind event", 2, function () {

        var es = mockNewEs(),
            event = "dummyevent",
            eventArg = "dummyarg";

        es.bind(event, function() {
            equal(this, es, "Context matched");
            equal(arguments[0], eventArg, "arg matched");
        })._trigger(event, eventArg);
    });

    test("EntitySource Unbind event", 0, function () {

        var es = mockNewEs(),
            event = "dummyevent",
            eventArg = "dummyarg",
            eventCallback = function() {
                ok(false, "should not callback unbind event");
            };

        es.bind(event, eventCallback).unbind(event, eventCallback)._trigger(event, eventArg);
    });

    test("DataContext Bind event", 2, function () {

        var es = upshot.DataContext("unused"),
            event = "dummyevent",
            eventArg = "dummyarg";

        es.bind(event, function() {
            equal(this, es, "Context matched");
            equal(arguments[0], eventArg, "arg matched");
        })._trigger(event, eventArg);
    });

    test("DataContext Unbind event", 0, function () {

        var es = upshot.DataContext("unused"),
            event = "dummyevent",
            eventArg = "dummyarg",
            eventCallback = function() {
                ok(false, "should not callback unbind event");
            };

        es.bind(event, eventCallback).unbind(event, eventCallback)._trigger(event, eventArg);
    });

})(this, upshot);

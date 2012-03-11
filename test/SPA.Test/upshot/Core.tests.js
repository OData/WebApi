
(function(upshot) {

    module("Core.js");

    var Custom = upshot.defineClass(null);

    test("classof utility test", 50, function () {
        var testCases = [
            [ true, "boolean"],
            [ null, "null"],
            [ 1, "number"],
            [ [], "array"],
            [ "s", "string"],
            [ {}, "object"],
            [ new Custom(), "object" ],
            [ Boolean(true), "boolean" ],
            [ new Boolean(true), "boolean" ],
            [ 1, "number" ],
            [ 1.0, "number" ],
            [ Number(1), "number" ],
            [ new Number(1), "number" ],
            [ Number.MAX_VALUE, "number" ],
            [ Number.MIN_VALUE, "number" ],
            [ Number.NaN, "number" ],
            [ Number.NEGATIVE_INFINITY, "number" ],
            [ Number.POSITIVE_INFINITY, "number" ],
            [ "A", "string" ],
            [ String("A"), "string" ],
            [ new String("A"), "string" ],
            [ new Date(), "date" ],
            [ undefined, "undefined" ],
            [ function () {}, "function" ],
            [ /./, "regexp" ]
        ];

        for(var i = 0; i < testCases.length; i++) {
            // verify our classof function
            var testPair = testCases[i];
            var result = upshot.classof(testPair[0]);
            equal(result, testPair[1]);

            // verify that we produce the same results as the jQuery function
            var jQueryResult = $.type(testPair[0]);
            equal(result, jQueryResult);
        }
    });

    test("isArray utility test", 2, function () {
        equal(upshot.isArray([]), true);
        equal(upshot.isArray(5), false);
    });

    test("HelloWorldNs test", 2, function () {
        equal(typeof upshot.defineNamespace, "function", "pre test");
        upshot.defineNamespace("HelloWorldNs");
        equal(typeof HelloWorldNs, "object", "HelloWorldNs is defined");
    });

    test("HelloWorldCls simple test", 4, function () {
        var HelloWorldCls =  upshot.defineClass(
            function(result) {
                this.result = result; 
            },
            {
                add: function(value) { 
                    this.result += value;
                },
                subtract: function(value) { 
                    this.result -= value;
                }
            }
        );
        equal(typeof HelloWorldCls, "function", "HelloWorldCls is defined");
        var val = 5;
        var calc = new HelloWorldCls(val);
        equal(calc.result, val, "precheck result");
        val += 3;
        calc.add(3);
        equal(calc.result, val, "add check");
        val -= 4;
        calc.subtract(4);
        equal(calc.result, val, "subtract check");
    });

    test("HelloWorldCls inherit test: prototype inheritance", 2, function () {
        var result;        
        var HelloWorldBase =  upshot.defineClass(
            function() {
                this.id = 1;
            },
            {
                foo: function(val) {
                    result += this.id + ".HelloWorldBase.foo(" + val + ")";
                }
            }
        );
        var HelloWorldCls =  upshot.deriveClass(
            HelloWorldBase.prototype,
            function() {
                this.id = 2;
            },
            {
                bar: function(val) {
                    result += this.id + ".HelloWorldCls.bar(" + val + ")";
                }
            }
        );

        var tmp = new HelloWorldCls();
        result = "";
        tmp.foo("hi"); 
        equal(result, "2.HelloWorldBase.foo(hi)", "test foo");
        result = "";
        tmp.bar("hey"); 
        equal(result, "2.HelloWorldCls.bar(hey)", "test bar");
    });

    test("HelloWorldCls inherit test: override ctor", 1, function () {
        var result;        
        var HelloWorldBase =  upshot.defineClass(
            function(val) { 
                result += "HelloWorldBase.ctor(" + val + ")";
            }
        );
        var base = HelloWorldBase.prototype,
            HelloWorldCls =  upshot.deriveClass(
            base,
            function(val) {
                result += "HelloWorldCls.ctor(" + val + ")";
                base.constructor.call(this, val);
            }
        );

        result = "";
        var tmp = new HelloWorldCls("hey");
        equal(result, "HelloWorldCls.ctor(hey)HelloWorldBase.ctor(hey)", "test ctor");
    });

    test("HelloWorldCls inherit test: override method", 1, function () {
        var result;        
        var HelloWorldBase =  upshot.defineClass(
            function() {
                this.id = 1;
            },
            {
                foo: function(val) {
                    result += this.id + ".HelloWorldBase.foo(" + val + ")";
                }
            }
        );
        var base = HelloWorldBase.prototype,
            HelloWorldCls =  upshot.deriveClass(
            base,
            function() {
                this.id = 2;
            },
            {
                foo: function(val) {
                    result += this.id + ".HelloWorldCls.foo(" + val + ")";
                    base.foo.call(this, val);
                }
            }
        );

        var tmp = new HelloWorldCls();
        result = "";
        tmp.foo("hey"); 
        equal(result, "2.HelloWorldCls.foo(hey)2.HelloWorldBase.foo(hey)", "test foo");
    });

    test("HelloWorldCls inherit test: multi prototype inheritance", 3, function () {
        var result;        
        var HelloWorldBase =  upshot.defineClass(
            function() {
                this.id = 1;
            },
            {
                foo: function(val) {
                    result += this.id + ".HelloWorldBase.foo(" + val + ")";
                }
            }
        );
        var HelloWorldInt =  upshot.deriveClass(
            HelloWorldBase.prototype,
            function() {
                this.id = 2;
            },
            {
                fred: function(val) {
                    result += this.id + ".HelloWorldInt.fred(" + val + ")";
                }
            }
        );
        var HelloWorldCls =  upshot.deriveClass(
            HelloWorldInt.prototype,
            function() {
                this.id = 3;
            },
            {
                bar: function(val) {
                    result += this.id + ".HelloWorldCls.bar(" + val + ")";
                }
            }
        );

        var tmp = new HelloWorldCls();
        result = "";
        tmp.foo("hi"); 
        equal(result, "3.HelloWorldBase.foo(hi)", "test foo");
        result = "";
        tmp.fred("huh"); 
        equal(result, "3.HelloWorldInt.fred(huh)", "test fred");
        result = "";
        tmp.bar("hey"); 
        equal(result, "3.HelloWorldCls.bar(hey)", "test bar");
    });

    test("HelloWorldCls inherit test: multi override ctor", 1, function () {
        var result;        
        var HelloWorldBase =  upshot.defineClass(
            function(val) { 
                result += "HelloWorldBase.ctor(" + val + ")";
            }
        );
        var base = HelloWorldBase.prototype,
            HelloWorldInt =  upshot.deriveClass(
            base,
            function(val) { 
                result += "HelloWorldInt.ctor(" + val + ")";
                base.constructor.call(this, val);
            }
        );
        var baseInt = HelloWorldInt.prototype,
            HelloWorldCls =  upshot.deriveClass(
            baseInt,
            function(val) { 
                result += "HelloWorldCls.ctor(" + val + ")";
                baseInt.constructor.call(this, val);
            }
        );

        result = "";
        var tmp = new HelloWorldCls("hey");
        equal(result, "HelloWorldCls.ctor(hey)HelloWorldInt.ctor(hey)HelloWorldBase.ctor(hey)", "test ctor");
    });

    test("HelloWorldCls inherit test: multi override method", 1, function () {
        var result;        
        var HelloWorldBase =  upshot.defineClass(
            function() {
                this.id = 1;
            },
            {
                foo: function(val) {
                    result += this.id + ".HelloWorldBase.foo(" + val + ")";
                }
            }
        );
        var base = HelloWorldBase.prototype,
            HelloWorldInt =  upshot.deriveClass(
            base,
            function() {
                this.id = 2;
            },
            {
                foo: function(val) {
                    result += this.id + ".HelloWorldInt.foo(" + val + ")";
                    base.foo.call(this, val);
                }
            }
        );
        var baseInt = HelloWorldInt.prototype,
            HelloWorldCls =  upshot.deriveClass(
            baseInt,
            function() {
                this.id = 3;
            },
            {
                foo: function(val) {
                    result += this.id + ".HelloWorldCls.foo(" + val + ")";
                    baseInt.foo.call(this, val);
                }
            }
        );

        var tmp = new HelloWorldCls();
        result = "";
        tmp.foo("hey"); 
        equal(result, "3.HelloWorldCls.foo(hey)3.HelloWorldInt.foo(hey)3.HelloWorldBase.foo(hey)", "test foo");
    });

})(upshot);
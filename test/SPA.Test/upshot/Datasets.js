/// <reference path="../Scripts/References.js" />

(function (global, upshot, undefined) {

    var datasets = {
        primitives: {
            create: function (id) {
                return {
                    Id: id || 0,
                    B: true,
                    N: 1,
                    S: "A"
                };
            },
            count: 4
        },

        scalars: {
            create: function (id) {
                return {
                    Id: id || 0,
                    B: true,
                    N: 1,
                    S: "A",
                    D: new Date(2011, 0, 1),
                    _: null
                };
            },
            count: 6
        },

        nested: {
            create: function (id) {
                return {
                    Id: id || 0,
                    B: true,
                    N: 1,
                    S: "A",
                    O: {
                        B: true,
                        N: 1,
                        S: "A"
                    }
                };
            },
            count: 8
        },

        tree: {
            create: function (id) {
                return {
                    Id: id || 0,
                    B: true,
                    N: 1,
                    S: "A",
                    O: {
                        B: true,
                        N: 1,
                        S: "A",
                        O1: {
                            B: true,
                            N: 1,
                            S: "A"
                        },
                        O2: {
                            B: true,
                            N: 1,
                            S: "A"
                        }
                    }
                };
            },
            count: 16
        },

        array: {
            create: function (id) {
                return {
                    Id: id || 0,
                    B: true,
                    N: 1,
                    S: "A",
                    A: [{
                        B: true,
                        N: 1,
                        S: "A"
                    },
                        {
                            B: true,
                            N: 1,
                            S: "A"
                        }
                    ]
                };
            },
            count: 13
        },

        nestedArrays: {
            create: function (id) {
                return {
                    Id: id || 0,
                    B: true,
                    N: 1,
                    S: "A",
                    A: [{
                        B: true,
                        N: 1,
                        S: "A",
                        A: [{
                            B: true,
                            N: 1,
                            S: "A"
                        }
                            ]
                    },
                        {
                            B: true,
                            N: 1,
                            S: "A",
                            A: [{
                                B: true,
                                N: 1,
                                S: "A"
                            }
                            ]
                        }
                    ]
                };
            },
            count: 23
        },

        ko_primitives: function (id, extend) {
            var obj = {
                Id: ko.observable(id || 0),
                B: ko.observable(true),
                N: ko.observable(1),
                S: ko.observable("A")
            };
            if (extend) {
                upshot.addEntityProperties(obj);
                upshot.addUpdatedProperties(obj);
            }
            return obj;
        },

        ko_tree: function (id, extend) {
            var obj = {
                Id: ko.observable(id || 0),
                B: ko.observable(true),
                N: ko.observable(1),
                S: ko.observable("A"),
                O: ko.observable({
                    B: ko.observable(true),
                    N: ko.observable(1),
                    S: ko.observable("A"),
                    O1: ko.observable({
                        B: ko.observable(true),
                        N: ko.observable(1),
                        S: ko.observable("A")
                    }),
                    O2: ko.observable({
                        B: ko.observable(true),
                        N: ko.observable(1),
                        S: ko.observable("A")
                    })
                })
            };
            if (extend) {
                upshot.addEntityProperties(obj);
                upshot.addUpdatedProperties(obj);
                upshot.addUpdatedProperties(obj.O());
                upshot.addUpdatedProperties(obj.O().O1());
                upshot.addUpdatedProperties(obj.O().O2());
            }
            return obj;
        },

        ko_array: function (id, extend) {
            var obj = {
                Id: ko.observable(id || 0),
                B: ko.observable(true),
                N: ko.observable(1),
                S: ko.observable("A"),
                A: ko.observableArray([
                    {
                        B: ko.observable(true),
                        N: ko.observable(1),
                        S: ko.observable("A")
                    },
                    {
                        B: ko.observable(true),
                        N: ko.observable(1),
                        S: ko.observable("A")
                    }
                ])
            };
            if (extend) {
                upshot.addEntityProperties(obj);
                upshot.addUpdatedProperties(obj);
                upshot.addUpdatedProperties(obj.A()[0]);
                upshot.addUpdatedProperties(obj.A()[1]);
            }
            return obj;
        },

        ko_nestedArrays: function (id, extend) {
            var obj = {
                Id: ko.observable(id || 0),
                B: ko.observable(true),
                N: ko.observable(1),
                S: ko.observable("A"),
                A: ko.observableArray([
                    {
                        B: ko.observable(true),
                        N: ko.observable(1),
                        S: ko.observable("A"),
                        A: ko.observableArray([
                            {
                                B: ko.observable(true),
                                N: ko.observable(1),
                                S: ko.observable("A")
                            }
                        ])
                    },
                    {
                        B: ko.observable(true),
                        N: ko.observable(1),
                        S: ko.observable("A"),
                        A: ko.observableArray([
                            {
                                B: ko.observable(true),
                                N: ko.observable(1),
                                S: ko.observable("A")
                            }
                        ])
                    }
                ])
            };
            if (extend) {
                upshot.addEntityProperties(obj);
                upshot.addUpdatedProperties(obj);
                upshot.addUpdatedProperties(obj.A()[0]);
                upshot.addUpdatedProperties(obj.A()[0].A()[0]);
                upshot.addUpdatedProperties(obj.A()[1]);
                upshot.addUpdatedProperties(obj.A()[1].A()[0]);
            }
            return obj;

        }
    };

    upshot.test || (upshot.test = {});
    upshot.test.datasets = datasets;

})(this, upshot);
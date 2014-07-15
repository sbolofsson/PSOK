var kademlia = kademlia || {};
kademlia.reportentry = function (json) {
    json = json || {};
    var self = this;
    self.description = ko.observable(json.Description || '');
    self.value = ko.observable(json.Value || '');
};
kademlia.report = function (json) {
    json = json || {};
    var self = this;
    self.id = ko.observable(json.Id || '');
    self.status = ko.observable(json.Status || '');
    self.header = ko.observable(json.Header || '');
    self.entries = ko.observableArray(json.Entries ? $.map(json.Entries, function (x) { return new kademlia.reportentry(x); }) : []);
};
kademlia.reports = ko.observableArray([]);
kademlia.report.retrieve = function () {
    kademlia.ajaxmanager.retrieve('/api/report', {
        success: function (json) {
            kademlia.reports(json ? $.map(json, function (x) { return new kademlia.report(x); }) : []);
        },
        error: function (err) {

        }
    });
};
kademlia.report.setup = function () {
    ko.applyBindings(kademlia, document.getElementById('kademlia'));
    kademlia.report.retrieve();
    setInterval(kademlia.report.retrieve, 60 * 1000);
};
kademlia.report.setup();
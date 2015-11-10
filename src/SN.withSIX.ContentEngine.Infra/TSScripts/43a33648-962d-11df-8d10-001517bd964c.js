//Acre Guid: 43a33648-962d-11df-8d10-001517bd964c
var SixClient;
(function(SixClient) {
    var Mods;
    (function(Mods) {
        var acre = (function() {
            function acre() {
                this.x64_dll = "plugin\\acre_win64.dll";
                this.x86_dll = "plugin\\acre_win32.dll";
            }

            acre.prototype.processMod = function() {
                this.service = getService("TeamspeakService", this.token);
                if (this.service.IsX86Installed())
                    this.installX86Plugin();
                if (this.service.IsX64Installed())
                    this.installX64Plugin();
            };
            acre.prototype.setToken = function(token) {
                this.token = token;
            };
            acre.prototype.installX64Plugin = function() {
                this.service.InstallX64Plugin(this.x64_dll, true);
            };
            acre.prototype.installX86Plugin = function() {
                this.service.InstallX86Plugin(this.x86_dll, true);
            };
            return acre;
        })();
        registerMod("43a33648-962d-11df-8d10-001517bd964c", new acre());
    })(Mods = SixClient.Mods || (SixClient.Mods = {}));
})(SixClient || (SixClient = {}));
//# sourceMappingURL=43a33648-962d-11df-8d10-001517bd964c.js.map
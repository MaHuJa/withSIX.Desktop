//Acre_A3 Guid: efa97a10-29ef-11e4-864e-001517bd964c
var SixClient;
(function(SixClient) {
    var Mods;
    (function(Mods) {
        var acre_a3 = (function() {
            function acre_a3() {
                this.x64_dll = "plugin\\acre2_win64.dll";
                this.x86_dll = "plugin\\acre2_win32.dll";
            }

            acre_a3.prototype.processMod = function() {
                this.service = getService("TeamspeakService", this.token);
                if (this.service.IsX86Installed())
                    this.installX86Plugin();
                if (this.service.IsX64Installed())
                    this.installX64Plugin();
            };
            acre_a3.prototype.setToken = function(token) {
                this.token = token;
            };
            acre_a3.prototype.installX64Plugin = function() {
                this.service.InstallX64Plugin(this.x64_dll, true);
            };
            acre_a3.prototype.installX86Plugin = function() {
                this.service.InstallX86Plugin(this.x86_dll, true);
            };
            return acre_a3;
        })();
        registerMod("efa97a10-29ef-11e4-864e-001517bd964c", new acre_a3());
    })(Mods = SixClient.Mods || (SixClient.Mods = {}));
})(SixClient || (SixClient = {}));
//# sourceMappingURL=efa97a10-29ef-11e4-864e-001517bd964c.js.map
//Task Force Radio Guid: 436b875a-56e3-11e3-ba1d-001517bd964c
var SixClient;
(function(SixClient) {
    var Mods;
    (function(Mods) {
        var tfr = (function() {
            function tfr() {
                this.pluginFolder = "teamspeak 3 client\\plugins\\";
                this.x64_dll = this.pluginFolder + "task_force_radio_win64.dll";
                this.x86_dll = this.pluginFolder + "task_force_radio_win32.dll";
                this.radioSoundsFolder = this.pluginFolder + "radio-sounds";
            }

            tfr.prototype.processMod = function() {
                this.service = getService("TeamspeakService", this.token);
                if (this.service.IsX86Installed())
                    this.installX86Plugins();
                if (this.service.IsX64Installed())
                    this.installX64Plugins();
            };
            tfr.prototype.setToken = function(token) {
                this.token = token;
            };
            tfr.prototype.installX64Plugins = function() {
                this.service.InstallX64Plugin(this.x64_dll, true);
                this.service.InstallX64PluginFolder(this.radioSoundsFolder, true);
            };
            tfr.prototype.installX86Plugins = function() {
                this.service.InstallX86Plugin(this.x86_dll, true);
                this.service.InstallX86PluginFolder(this.radioSoundsFolder, true);
            };
            return tfr;
        })();
        registerMod("436b875a-56e3-11e3-ba1d-001517bd964c", new tfr());
    })(Mods = SixClient.Mods || (SixClient.Mods = {}));
})(SixClient || (SixClient = {}));
//# sourceMappingURL=436b875a-56e3-11e3-ba1d-001517bd964c.js.map
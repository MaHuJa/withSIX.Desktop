//XCam Guid: dd538a8d-078e-468c-87ea-c351131b495f
var SixClient;
(function(SixClient) {
    var Mods;
    (function(Mods) {
        var xcam = (function() {
            function xcam() {
                this.dll = "make_file.dll";
            }

            xcam.prototype.processMod = function() {
                this.service = getService("GameFolderService", this.token);
                this.service.InstallDllPlugin(this.dll, true);
            };
            xcam.prototype.setToken = function(token) {
                this.token = token;
            };
            return xcam;
        })();
        registerMod("dd538a8d-078e-468c-87ea-c351131b495f", new xcam());
    })(Mods = SixClient.Mods || (SixClient.Mods = {}));
})(SixClient || (SixClient = {}));
//# sourceMappingURL=dd538a8d-078e-468c-87ea-c351131b495f.js.map
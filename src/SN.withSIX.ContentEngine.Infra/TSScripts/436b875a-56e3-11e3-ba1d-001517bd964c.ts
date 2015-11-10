//Task Force Radio Guid: 436b875a-56e3-11e3-ba1d-001517bd964c
module SixClient.Mods {
    class tfr implements IMod {

        service: ITeamspeakService;
        token: string;

        pluginFolder: string = "teamspeak 3 client\\plugins\\";
        x64_dll: string = this.pluginFolder + "task_force_radio_win64.dll";
        x86_dll: string = this.pluginFolder + "task_force_radio_win32.dll";
        radioSoundsFolder: string = this.pluginFolder + "radio-sounds";

        public processMod(): void {
            this.service = getService<ITeamspeakService>("TeamspeakService", this.token);

            if (this.service.IsX86Installed())
                this.installX86Plugins();

            if (this.service.IsX64Installed())
                this.installX64Plugins();
        }

        public setToken(token: string): void {
            this.token = token;
        }

        installX64Plugins() {
            this.service.InstallX64Plugin(this.x64_dll, true);
            this.service.InstallX64PluginFolder(this.radioSoundsFolder, true);
        }

        installX86Plugins() {
            this.service.InstallX86Plugin(this.x86_dll, true);
            this.service.InstallX86PluginFolder(this.radioSoundsFolder, true);
        }
    }

    registerMod("436b875a-56e3-11e3-ba1d-001517bd964c", new tfr());
}
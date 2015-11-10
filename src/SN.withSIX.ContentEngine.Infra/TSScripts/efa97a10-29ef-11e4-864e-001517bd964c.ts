//Acre_A3 Guid: efa97a10-29ef-11e4-864e-001517bd964c
module SixClient.Mods {
    class acre_a3 implements IMod {

        service: ITeamspeakService;
        token: string;

        x64_dll: string = "plugin\\acre2_win64.dll";
        x86_dll: string = "plugin\\acre2_win32.dll";

        public processMod(): void {
            this.service = getService<ITeamspeakService>("TeamspeakService", this.token);

            if (this.service.IsX86Installed())
                this.installX86Plugin();

            if (this.service.IsX64Installed())
                this.installX64Plugin();
        }

        public setToken(token: string): void {
            this.token = token;
        }

        installX64Plugin() {
            this.service.InstallX64Plugin(this.x64_dll, true);
        }

        installX86Plugin() {
            this.service.InstallX86Plugin(this.x86_dll, true);
        }
    }

    registerMod("efa97a10-29ef-11e4-864e-001517bd964c", new acre_a3());
}
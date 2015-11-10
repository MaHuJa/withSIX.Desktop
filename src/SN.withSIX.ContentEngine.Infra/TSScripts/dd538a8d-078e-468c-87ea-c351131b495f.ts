//XCam Guid: dd538a8d-078e-468c-87ea-c351131b495f

module SixClient.Mods {

    class xcam implements IMod {

        service: IGameFolderService;
        token: string;

        dll: string = "make_file.dll";

        public processMod(): void {
            this.service = getService<IGameFolderService>("GameFolderService", this.token);

            this.service.InstallDllPlugin(this.dll, true);
        }

        public setToken(token: string): void {
            this.token = token;
        }

    }

    registerMod("dd538a8d-078e-468c-87ea-c351131b495f", new xcam());
}
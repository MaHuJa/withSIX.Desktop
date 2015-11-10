interface IMod {
    processMod(): void;
    setToken(token: string): void;
}

interface IService {

}

interface ITeamspeakService extends IService {
    IsX86Installed(): boolean;
    IsX64Installed(): boolean;
    InstallX86Plugin(plugin: string, force?: boolean): void;
    InstallX64Plugin(plugin: string, force?: boolean): void;
    InstallX86PluginFolder(plugin: string, force?: boolean): void;
    InstallX64PluginFolder(plugin: string, force?: boolean): void;
}

interface IGameFolderService extends IService {
    InstallDllPlugin(plugin: string, force?: boolean): void;
}

interface Logger {
    DebugException(message: string, e: Error): void;
    WarnException(message: string, e: Error): void;
    ErrorException(message: string, e: Error): void;
    InfoException(message: string, e: Error): void;
    TraceException(message: string, e: Error): void;
    FormattedDebugException(e: Error, message?: string): void;
    FormattedWarnException(e: Error, message?: string): void;
    FormattedErrorException(e: Error, message?: string): void;
    Info(message: string, ...args: any[]): void;
    Debug(message: string, ...args: any[]): void;
    Trace(message: string, ...args: any[]): void;
    Warn(message: string, ...args: any[]): void;
    Error(message: string, ...args: any[]): void;
    Vital(message: string, ...args: any[]): void;
}

declare var Logger: Logger;

declare var getService: <TService extends IService>(name: string, token: string) => TService;

declare var registerMod: (guid: string, mod: IMod) => void;
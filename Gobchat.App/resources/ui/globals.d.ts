declare var _; //imported by html

/*
declare var Gobchat: {
    DefaultProfileConfig: object;
    KeyCodeToKeyEnum: (number) => string;
}; // backend
*/

declare var Gobchat: any;

declare var GobchatAPI: { // backend
    getConfigAsJson: () => Promise<string>;
    synchronizeConfig: (string) => Promise<boolean>;
    setConfigActiveProfile: (string) => Promise<boolean>;
}

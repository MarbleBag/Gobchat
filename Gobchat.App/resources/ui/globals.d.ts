// globals, create in js code
declare var gobConfig: import("./modules/Config").GobchatConfig
declare var gobChatManager: import("./modules/Chat").ChatManager
declare var gobStyles: import("./modules/Style").StyleLoader
declare var gobLocale: import("./modules/Locale").LocaleManager
declare function openGobConfig(): void
declare function saveConfig(): void

declare var _: any
declare var $: JQuery
declare var jQuery: JQuery
declare interface JQuery {
    (a: any): JQuery
    [i: number]: HTMLElement
    remove(): JQuery
    on(action: string, callback: Function): JQuery
    off(action: string, callback: Function): JQuery
    attr(key: string, value?: any): any
    prop(key: string, value?: any): any
    each(callback: (index: number, element: HTMLElement) => boolean): void
    each(callback: (index: number, element: HTMLElement) => void): void
    hide(): JQuery
    show(): JQuery
    html(val: any): JQuery
    html(): string
    text(val: string): JQuery
    text(): string
    val(val: string): JQuery
    val(): string
    find(selector: string): JQuery
    addBack(selector?: string): JQuery
    addClass(c: string): JQuery
    removeClass(c: string): JQuery
    index(a?: any): number
    first(): JQuery
    last(): JQuery
    focus(): JQuery
    animate(a: any): JQuery
    prevAll(selector: string): JQuery
    nextAll(selector: string): JQuery
    extend(a:any, b:any): any
}

// should be included in es2021 ?!
declare interface Object {
    entries: (any) => any
}

// created by backend and some js files
declare namespace Gobchat {
    const DefaultProfileConfig: any
    const KeyCodeToKeyEnum: (number) => string
    const MessageSegmentEnum: {
        UNDEFINED: import("./modules/Chat").MessageSegmentEnum
        SAY: import("./modules/Chat").MessageSegmentEnum
        EMOTE: import("./modules/Chat").MessageSegmentEnum
        OOC: import("./modules/Chat").MessageSegmentEnum
        MENTION: import("./modules/Chat").MessageSegmentEnum
        WEBLINK: import("./modules/Chat").MessageSegmentEnum
    }
    const Channels: { [s: string]: import("./modules/Chat").Channel }
    const ChannelEnum: { [s: string]: import("./modules/Chat").ChatChannelEnum }
}

// created by backend
declare namespace GobchatAPI { 
    function getConfigAsJson(): Promise<string>
    function synchronizeConfig(data: string): Promise<boolean>
    function setConfigActiveProfile(id: string): Promise<boolean>
    function getLocalizedStrings(language: string, keys: string[]): Promise<{ [s: string]: string }>
    function sendInfoChatMessage(msg: string): void
    function sendErrorChatMessage(msg: string): void
    function closeGobchat(): void
    function getPlayerCount(): Promise<number>
    function getPlayersAndDistance(): Promise<string[]>
    function getPlayerDistance(playerName: string): Promise<number>
    function setUIReady(isReady: boolean)
    function readTextFromFile(filePath: string): Promise<string>
}

// created by backend
declare interface ChatMessagesEvent extends CustomEvent<{ messages: import("./modules/Chat").ChatMessage[] }>  {
    
}

declare interface OverlayStateUpdateEvent extends CustomEvent<{ isLocked: boolean }> {

}

declare interface SynchronizeConfigEvent extends CustomEvent {

}

/*
declare interface CustomEventMap {
    "ChatMessagesEvent": ChatMessagesEvent
    "OverlayStateUpdate": CustomEvent
}

declare interface Document { //adds custom event definitions to Document
    addEventListener<K extends keyof CustomEventMap>(type: K, listener: (this: Document, ev: CustomEventMap[K]) => void): void;
    dispatchEvent<K extends keyof CustomEventMap>(ev: CustomEventMap[K]): void;
}
*/






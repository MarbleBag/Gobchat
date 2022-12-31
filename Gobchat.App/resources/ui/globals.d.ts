// globals, create in js code
declare var gobConfig: import("./modules/Config").GobchatConfig
declare var gobChatManager: import("./modules/Chat").ChatControl
declare var gobStyles: import("./modules/Style").StyleLoader
declare var gobLocale: import("./modules/Locale").LocaleManager
declare function openGobConfig(): void
declare function saveConfig(): void

declare var _: any
declare var $: JQuery
declare var jQuery: JQuery
declare interface JQuery<T = HTMLElement> extends Iterable<T> {   
    (a?: any): JQuery<T>
    [i: number]: T
    remove(): JQuery<T>
    on(action: string, callback: Function): JQuery<T>
    off(action: string, callback?: Function): JQuery<T>
    one(action: string, callback: Function): JQuery<T>
    filter(selector: string): JQuery<T>
    remove(): JQuery<T>
    length: number
    attr(key: string, value: any): JQuery<T>
    attr(key: string): string | number | boolean
    prop(key: string, value: any): JQuery<T>
    prop(key: string): string | number | boolean
    each(callback: (index: number, element: T) => boolean): void
    each(callback: (index: number, element: T) => void): void
    hide(): JQuery<T>
    show(): JQuery<T>
    html(val: any): JQuery<T>
    html(): string
    text(val: string): JQuery<T>
    text(): string
    val(val: string): JQuery<T>
    val(): string
    width(): number
    height(): number
    find(selector: string): JQuery<T>
    scrollLeft(): number
    scrollLeft(position:number): JQuery
    scrollRight(): number
    scrollRight(position:number): JQuery
    scrollTop(): number
    scrollTop(position:number): JQuery
    scrollBottom(): number
    scrollBottom(position:number): JQuery
    innerHeight(): number
    innerWidth(): number
    addBack(selector?: string): JQuery<T>
    addClass(c: string): JQuery<T>
    removeClass(c: string): JQuery<T>
    toggleClass(c: string, on?: boolean)
    index(a?: any): number
    first(): JQuery<T>
    last(): JQuery<T>
    focus(): JQuery<T>
    animate(a: any, time?: number): JQuery<T>
    prevAll(selector: string): JQuery<T>
    nextAll(selector: string): JQuery<T>
    extend(a: any, b: any): any
    append(e: any): JQuery<T>
    appendTo(e: any): JQuery<T>
    after(e: any): JQuery<T>
    before(e: any): JQuery<T>
    insertAfter(e: any): JQuery<T>
    insertBefore(e: any): JQuery<T>
    dialog(e: any): JQuery<T>
    parent(selector?: string): JQuery<T>
    hasClass(selector: string): boolean
    is(selector: string): boolean
    children(selector?: string): JQuery<T>
    parent(): JQuery<T>
    eq(e: any): JQuery<T>
    detach(): JQuery<T>
    map<B>(fn: (element: T, index: number) => B): JQuery<B>    
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
declare interface ChatMessagesEvent extends CustomEvent<{ messages: import("./modules/Chat").ChatMessage[] }> {

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






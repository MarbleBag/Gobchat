declare type Optional<T, K extends keyof T> = Pick<Partial<T>, K> & Omit<T, K>;

// lodash
declare var _: Lodash
type Lodash_ListIteratee<T> = string

declare interface Lodash {
    remove<T>(iterable: T[], predicate: (element: T) => boolean): T[]
    includes<T, V extends T>(collection: readonly T[], values: readonly V, fromIndex?: number): boolean
    difference<T, V extends T>(array: readonly T[], values: readonly V[]): T[];
    difference<T, V extends T>(array: readonly T[], ...values: readonly V): T[];
    without<T>(collection: readonly T[], values: readonly T[]): T[]
    without<T>(collection: readonly T[], ...values: readonly T): T[]
    indexOf<T, V extends T>(collection: readonly T[], value: readonly T, fromIndex?: number): number
    sortBy<T>(collection: readonly T[], sorters?: ((element: T) => unknown)[]): T[]
    isEqual(a: unknown, b: unknown): boolean
    every<T>(collection: readonly T[], predicate: (element: T) => boolean): boolean
    mapKeys<T extends object>(collection: readonly T, mapping: (value: T[keyof T], key: string, collection: readonly T) => string): { [index: string]: T[keyof T] }

}

// jquery
declare var $: JQuery
declare var jQuery: JQuery
declare interface JQuery<T = HTMLElement> extends Iterable<T> {
    fn: { [name: string]: Function }
    empty(): JQuery<T>;
    (a?: any, options?: any): JQuery<T>
    [i: number]: T
    load(url: string, params: object | null | undefined, arg2: (response: any, status: any, jqXHR: any) => void): unknown
    remove(): JQuery<T>
    on(action: string, callback: (this: T, event: any) => void): JQuery<T>
    off(action: string, callback?: (this: T, event: any) => void): JQuery<T>
    one(action: string, callback: (this: T, event: any) => void): JQuery<T>
    filter(selector: string): JQuery<T>
    remove(): JQuery<T>
    length: number
    attr(key: string, value: any): JQuery<T>
    attr(key: string): string | number | boolean | undefined | null
    removeAttr(attribute: string): JQuery<T>
    prop(key: string, value: any): JQuery<T>
    prop(key: string): string | number | boolean | null
    each(callback: (this: T, index: number, element: T) => boolean): void
    each(callback: (this: T, index: number, element: T) => void): void
    hide(): JQuery<T>
    show(): JQuery<T>
    toggle(val?: boolean): JQuery<T>
    html(val: any): JQuery<T>
    html(): string
    text(val: string | number | boolean): JQuery<T>
    text(): string
    css(property: string): string
    css(property: string[]): { [property: string]: string }
    css(property: string, value: string | number | ((index: number, value: string) => string | number)): JQuery<T>
    css(properties: { [property: string]: string | number }): JQuery<T>
    val(val: string | number | null): JQuery<T>
    val(): string
    width(): number
    height(): number
    find(selector: string): JQuery<T>
    scrollLeft(): number
    scrollLeft(position: number): JQuery
    scrollRight(): number
    scrollRight(position: number): JQuery
    scrollTop(): number
    scrollTop(position: number): JQuery
    scrollBottom(): number
    scrollBottom(position: number): JQuery
    innerHeight(): number
    innerWidth(): number
    addBack(selector?: string): JQuery<T>
    addClass(c: string | undefined | null): JQuery<T>
    removeClass(c: string | undefined | null): JQuery<T>
    toggleClass(c: string, on?: boolean)
    index(a?: any): number
    first(): JQuery<T>
    last(): JQuery<T>
    focus(): JQuery<T>
    animate(a: any, time?: number): JQuery<T>
    not(selector: string): JQuery<T>
    prevAll(selector: string): JQuery<T>
    nextAll(selector: string): JQuery<T>
    extend<A, B>(a: A, b: B): A & B
    extend<A, B, C>(a: A, b: B, c: C): A & B & C
    append(e: any): JQuery<T>
    appendTo(e: any): JQuery<T>
    add(e: any): JQuery<T>
    after(e: any): JQuery<T>
    before(e: any): JQuery<T>
    insertAfter(e: any): JQuery<T>
    insertBefore(e: any): JQuery<T>
    parent(selector?: string): JQuery<T>
    hasClass(selector: string): boolean
    is(selector: string): boolean
    children(selector?: string): JQuery<T>
    parent(): JQuery<T>
    eq(e: any): JQuery<T>
    detach(): JQuery<T>
    map<B>(fn: (index: number, element: T) => B): JQuery<B>
    get() : T[]
    change(): JQuery
    data(key: string, value: any): JQuery
    data<T>(key: string): T
}

declare interface JQuery<T = HTMLElement> extends Iterable<T> {
    spectrum(options: Partial<{
        color: string,
        flat: boolean,
        showInput: boolean,
        showInitial: boolean,
        allowEmpty: boolean,
        showAlpha: boolean,
        disabled: boolean,
        localStorageKey: string,
        showPalette: boolean,
        showPaletteOnly: boolean,
        togglePaletteOnly: boolean,
        showSelectionPalette: boolean,
        clickoutFiresChange: boolean,
        cancelText: string,
        chooseText: string,
        togglePaletteMoreText: string,
        togglePaletteLessText: string,
        containerClassName: string,
        replacerClassName: string,
        preferredFormat: string,
        maxSelectionSize: number,
        palette: string[string[]],
        selectionPalette: string[],
        hide: (color) => void,
        beforeShow: (color) => boolean
    }>): void
    spectrum(action: "show" | "hide" | "toggle" | "container" | "reflow" | "destroy" | "enable" | "disable"): void
    spectrum(action: "get"): string
    spectrum(action: "set", color: string): void
    spectrum(action: "option", optionName: string): any
    spectrum(action: "option", optionName: string, newOptionValue: any): void
    on(action: "hide.spectrum", callback: (this: HTMLElement, e: T, color: any) => void): JQuery<T>
}

declare interface JQuery<T = HTMLElement> extends Iterable<T> {
    accordion(action: "refresh"): JQuery<T>
    accordion(options: Partial<{
        heightStyle: "content",
        header: string,
    }>): JQuery<T>

    sortable(action: "refresh"): JQuery<T>
    sortable(options: Partial<{
        axis: "y" | "x",
        handle: string,
        stop: (event, ui) => void
    }>): JQuery<T>
}

declare interface JQuery<T = HTMLElement> extends Iterable<T> {
    dialog(action: "widget"): JQuery<T>
    dialog(options: Partial<{

    }>): JQuery<T>
}

// browser
// should be included in es2021 ?!
declare interface Object {
    entries: (any) => any
}

declare interface NodeListOf<TNode extends Node> extends NodeList, Iterable<TNode> {

}

declare interface DOMTokenList extends Iterable<string> {

}

// gobchat
// created by backend and some js files
declare var gobConfig: import("/module/Config").GobchatConfig
declare var gobChatManager: import("/module/Chat").ChatControl
declare var gobStyles: import("/module/Style").StyleLoader
declare var gobLocale: import("/module/Locale").LocaleManager
declare function openGobConfig(): void
declare function saveConfig(): void

declare interface Window {
    saveConfig(): void
}

declare namespace Gobchat {
    const DefaultProfileConfig: any
    const KeyCodeToKeyEnum: (number) => string
    // deprecated
    const MessageSegmentEnum: {
        UNDEFINED: import("/module/Chat").MessageSegmentEnum
        SAY: import("/module/Chat").MessageSegmentEnum
        EMOTE: import("/module/Chat").MessageSegmentEnum
        OOC: import("/module/Chat").MessageSegmentEnum
        MENTION: import("/module/Chat").MessageSegmentEnum
        WEBLINK: import("/module/Chat").MessageSegmentEnum
    }
    const Channels: { [s: string]: import("/module/Chat").Channel }
    const ChannelEnum: { [s: string]: import("/module/Chat").ChatChannelEnum }
}

// created by backend
declare namespace GobchatAPI {
    // chat
    function sendChatMessage(channel: number, source: string, msg: string): void
    function sendInfoChatMessage(msg: string): void
    function sendErrorChatMessage(msg: string): void

    // player data
    function isFeaturePlayerLocationAvailable(): Promise<boolean>
    function getPlayerCount(): Promise<number>
    function getPlayersNearby(): Promise<string[]>
    function getPlayerDistance(playerName: string): Promise<number>
    function getPlayersAndDistance(): Promise<string[]>

    // process
    function getAttachableFFXIVProcesses(): Promise<number[]>
    function getAttachedFFXIVProcess(): Promise<{ Item1: 0 | 1 | 2 | 3, Item2: number }>
    function attachToFFXIVProcess(processId: number): void

    // files
    function openDirectoryDialog(path: string): Promise<string>
    function openFileDialog(filter: string): Promise<string>
    function saveFileDialog(filter: string, fileName: string): Promise<string>
    function writeTextToFile(file: string, text: string): Promise<void>
    function readTextFromFile(file: string): Promise<string>

    // config
    function getConfigAsJson(): Promise<string>
    function setConfigActiveProfile(profileId: string): Promise<void>
    function synchronizeConfig(profileId: string): Promise<void>
    function importProfile(): Promise<string | null>

    // some
    function setUIReady(isReady: boolean): void
    function getScreenDimensions(): Promise<{ Item1: number, Item2: number }>
    function getAppVersion(): Promise<string>
    function getLocalizedStrings(language: string, keys: string[]): Promise<{ [s: string]: string }>
    function closeGobchat(): void
    function getAbsoluteChatLogPath(path: string): Promise<string>
    function getRelativeChatLogPath(path: string): Promise<string>
}

// created by backend
declare interface ChatMessagesEvent extends CustomEvent<{ messages: import("./modules/Chat").ChatMessage[] }> {

}

declare interface OverlayStateUpdateEvent extends CustomEvent<{ isLocked: boolean }> {

}

declare interface SynchronizeConfigEvent extends CustomEvent {

}

declare interface WindowEventMap {
    "custom-event": CustomEvent<{ data: string }>
    "ChatMessagesEvent": ChatMessagesEvent
    "OverlayStateUpdateEvent": OverlayStateUpdateEvent
    "SynchronizeConfigEvent": SynchronizeConfigEvent
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






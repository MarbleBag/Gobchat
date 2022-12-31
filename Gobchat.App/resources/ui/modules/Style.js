/*******************************************************************************
 * Copyright (C) 2019-2022 MarbleBag
 *
 * This program is free software: you can redistribute it and/or modify it under
 * the terms of the GNU Affero General Public License as published by the Free
 * Software Foundation, version 3.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>
 *
 * SPDX-License-Identifier: AGPL-3.0-only
 *******************************************************************************/
'use strict';
var __classPrivateFieldSet = (this && this.__classPrivateFieldSet) || function (receiver, state, value, kind, f) {
    if (kind === "m") throw new TypeError("Private method is not writable");
    if (kind === "a" && !f) throw new TypeError("Private accessor was defined without a setter");
    if (typeof state === "function" ? receiver !== state || !f : !state.has(receiver)) throw new TypeError("Cannot write private member to an object whose class did not declare it");
    return (kind === "a" ? f.call(receiver, value) : f ? f.value = value : state.set(receiver, value)), value;
};
var __classPrivateFieldGet = (this && this.__classPrivateFieldGet) || function (receiver, state, kind, f) {
    if (kind === "a" && !f) throw new TypeError("Private accessor was defined without a getter");
    if (typeof state === "function" ? receiver !== state || !f : !state.has(receiver)) throw new TypeError("Cannot read private member from an object whose class did not declare it");
    return kind === "m" ? f : kind === "a" ? f.call(receiver) : f ? f.value : state.get(receiver);
};
var _StyleLoader_styles, _StyleLoader_activeStyles, _StyleLoader_activeStyleSheetIds, _StyleLoader_filePrefix;
import * as Utility from './CommonUtility.js';
import * as Constants from './Constants.js';
import * as Chat from './Chat.js';
export class StyleLoader {
    constructor(filePrefix) {
        _StyleLoader_styles.set(this, {});
        _StyleLoader_activeStyles.set(this, []);
        _StyleLoader_activeStyleSheetIds.set(this, []);
        _StyleLoader_filePrefix.set(this, void 0);
        __classPrivateFieldSet(this, _StyleLoader_filePrefix, filePrefix ? filePrefix : null, "f");
    }
    async initialize() {
        __classPrivateFieldSet(this, _StyleLoader_styles, {}, "f");
        const json = await GobchatAPI.readTextFromFile("ui/styles/styles.json");
        const styles = JSON.parse(json);
        for (let style of styles) {
            const styleKey = style.label.toLowerCase().trim();
            if (styleKey.length == 0)
                continue;
            __classPrivateFieldGet(this, _StyleLoader_styles, "f")[styleKey] = {
                label: style.label,
                files: [].concat(style.files || []).filter(e => Utility.isString(e)).map(e => e.trim()).filter(e => e.length > 0)
            };
        }
    }
    get styles() {
        return Object.keys(__classPrivateFieldGet(this, _StyleLoader_styles, "f"))
            .map(key => {
            return {
                id: key,
                label: __classPrivateFieldGet(this, _StyleLoader_styles, "f")[key].label
            };
        });
    }
    get activeStyles() {
        return [].concat(__classPrivateFieldGet(this, _StyleLoader_activeStyles, "f") || []);
    }
    async activateStyles(styleIds, target, insertMod = "in") {
        styleIds = [].concat(styleIds || []).filter(e => Utility.isString(e)).map(e => e.toLowerCase());
        for (let styleId of styleIds)
            if (!__classPrivateFieldGet(this, _StyleLoader_styles, "f")[styleId])
                throw new Error(`Style with id '${styleId}' not available`);
        const $target = !target || $(target).length === 0 ? $("head") : $(target).first();
        const $body = $("body");
        // use hide / show to trigger a reflow, so the new loaded style gets applied everywhere.
        // Sometimes, without this, styles aren't applied to scrollbars. Still no idea why.
        $body.hide();
        for (let id of __classPrivateFieldGet(this, _StyleLoader_activeStyleSheetIds, "f"))
            $(`#${id}`).remove();
        __classPrivateFieldSet(this, _StyleLoader_activeStyleSheetIds, [], "f");
        __classPrivateFieldSet(this, _StyleLoader_activeStyles, [], "f");
        const awaitPromises = [];
        for (let styleId of styleIds) {
            __classPrivateFieldGet(this, _StyleLoader_activeStyles, "f").push(styleId);
            const style = __classPrivateFieldGet(this, _StyleLoader_styles, "f")[styleId];
            const randomIdPrefix = Utility.generateId(8);
            for (let file of style.files) {
                const id = `gobstyle-${randomIdPrefix}-${file.replace(/[\s\\/]+/g, '_').replace(/[^-\.\w]+/g, '')}`;
                const $link = $(`<link rel="stylesheet" href="">`).attr('id', id);
                __classPrivateFieldGet(this, _StyleLoader_activeStyleSheetIds, "f").push(id);
                awaitPromises.push(new Promise(function (resolve, reject) {
                    $link.one("load", () => resolve());
                    $link.one("error", () => reject());
                }));
                const path = __classPrivateFieldGet(this, _StyleLoader_filePrefix, "f") ? `${__classPrivateFieldGet(this, _StyleLoader_filePrefix, "f")}/${file}` : file;
                $link.attr("href", path);
                switch (insertMod) {
                    case 'in':
                        $link.appendTo($target);
                        break;
                    case 'after':
                        $link.insertAfter($target);
                        break;
                    case 'before':
                        $link.insertBefore($target);
                }
            }
        }
        awaitPromises.push(new Promise((resolve, reject) => {
            window.requestAnimationFrame(() => resolve());
        }));
        const results = await Promise.allSettled(awaitPromises);
        $body.show();
        let errorMsg = "";
        for (const result of results) {
            if (result.status === "rejected")
                errorMsg += result.reason + '\n';
        }
        if (errorMsg.length > 0)
            throw new Error(errorMsg);
    }
}
_StyleLoader_styles = new WeakMap(), _StyleLoader_activeStyles = new WeakMap(), _StyleLoader_activeStyleSheetIds = new WeakMap(), _StyleLoader_filePrefix = new WeakMap();
export class StyleBuilder {
    static generateAndSetCssRules(htmlStyleSheetId) {
        const rules = StyleBuilder.generateCssRules();
        StyleBuilder.setStyleOnCurrentDocument(htmlStyleSheetId, rules);
    }
    static copy(object) {
        return JSON.parse(JSON.stringify(object));
    }
    static toCss(selectors, ...properties) {
        selectors = [].concat(selectors);
        if (selectors.length === 0 || !properties || properties.length === 0)
            return "";
        let baseProperties = properties[0];
        if (properties.length > 1) {
            baseProperties = StyleBuilder.copy(baseProperties);
            for (let i = 1; i < properties.length; ++i) {
                Object.keys(baseProperties).forEach(key => {
                    const currentValue = baseProperties[key];
                    if (currentValue === null || currentValue === undefined) {
                        if (key in properties[i])
                            baseProperties[key] = properties[i][key];
                    }
                });
            }
        }
        const allSelectors = selectors.map(selector => `${selector}`).join(",");
        return `${allSelectors}${StyleBuilder.objectToCss(baseProperties)}\n`;
    }
    static objectToCss(object) {
        const content = Object.entries(object)
            .filter(e => e[1] !== null && !e[0].startsWith("$"))
            .map(e => `${e[0]}:${e[1]};`)
            .join("\n");
        return `{\n${content}}`;
    }
    static filterTabs(filter) {
        const configTabs = gobConfig.get("behaviour.chattabs.data");
        const filteredTabs = filter ? Object.keys(configTabs).filter(key => filter(configTabs[key])) : Object.keys(configTabs);
        const tabClasses = filteredTabs.map(key => configTabs[key].id)
            .map(id => Utility.formatString(Chat.CssClass.Chat_History_Tab_Partial, id));
        return tabClasses;
    }
    static generateCssRules() {
        const results = [];
        for (let ruleGenerator of StyleBuilder.RuleGenerators)
            results.push(ruleGenerator());
        return results.join("");
    }
    static setStyleOnCurrentDocument(htmlStyleSheetId, cssRules) {
        let styleElement = document.getElementById(htmlStyleSheetId);
        if (!styleElement) {
            styleElement = document.createElement("style");
            styleElement.id = htmlStyleSheetId;
            document.head.appendChild(styleElement);
        }
        styleElement.innerHTML = cssRules;
    }
}
StyleBuilder.RuleGenerators = [];
(() => {
    StyleBuilder.RuleGenerators.push(() => {
        return "";
    });
})();
(() => {
    StyleBuilder.RuleGenerators.push(() => {
        const baseFontSize = gobConfig.get("style.base-font-size");
        return StyleBuilder.toCss(":root", {
            "--gob-font-size": gobConfig.get("style.config.font-size", baseFontSize),
            "--gob-chat-history-font-size": gobConfig.get("style.config.font-size", baseFontSize),
        });
    });
})();
(() => {
    StyleBuilder.RuleGenerators.push(() => {
        const configStyle = gobConfig.get("style");
        const results = [];
        results.push(StyleBuilder.toCss(`.${Chat.CssClass.Chat_History}`, configStyle.chatbox));
        results.push(StyleBuilder.toCss(`.${Chat.CssClass.ChatEntry}`, configStyle.channel.base));
        for (let channel of Object.values(Gobchat.Channels)) {
            if (channel.internalName in configStyle.channel) {
                const channelClass = Utility.formatString(Chat.CssClass.ChatEntry_Channel_Partial, channel.internalName);
                const selector = `.${channelClass} .${Chat.CssClass.ChatEntry_Text}`;
                results.push(StyleBuilder.toCss(selector, configStyle.channel[channel.internalName]));
            }
        }
        return results.join("");
    });
})();
(() => {
    StyleBuilder.RuleGenerators.push(() => {
        const configStyle = gobConfig.get("style");
        const tabClassesWithMentions = StyleBuilder.filterTabs(tab => tab.formatting.mentions);
        const tabClassesWithRoleplay = StyleBuilder.filterTabs(tab => tab.formatting.roleplay);
        const results = [];
        results.push(StyleBuilder.toCss(tabClassesWithMentions.map(tabClass => `.${tabClass} .${Chat.CssClass.ChatEntry_Segment_Mention}`), configStyle.segment.mention));
        results.push(StyleBuilder.toCss(tabClassesWithRoleplay.map(tabClass => `.${tabClass} .${Chat.CssClass.ChatEntry_Segment_Say}`), configStyle.segment.say, configStyle.channel.say));
        results.push(StyleBuilder.toCss(tabClassesWithRoleplay.map(tabClass => `.${tabClass} .${Chat.CssClass.ChatEntry_Segment_Emote}`), configStyle.segment.emote, configStyle.channel.emote));
        results.push(StyleBuilder.toCss(tabClassesWithRoleplay.map(tabClass => `.${tabClass} .${Chat.CssClass.ChatEntry_Segment_Ooc}`), configStyle.segment.ooc));
        results.push(StyleBuilder.toCss(`.${Chat.CssClass.ChatEntry_Segment_Link}`, configStyle.segment.link));
        return results.join("");
    });
})();
(() => {
    StyleBuilder.RuleGenerators.push(() => {
        const tabClasses = StyleBuilder.filterTabs(tab => !tab.formatting.timestamps);
        const selectors = tabClasses.map(tabClass => `.${tabClass} .${Chat.CssClass.ChatEntry_Time}`);
        return StyleBuilder.toCss(selectors, { "display": "none" });
    });
})();
(() => {
    StyleBuilder.RuleGenerators.push(() => {
        const tabClasses = StyleBuilder.filterTabs(tab => tab.formatting.rangefilter);
        if (tabClasses.length === 0)
            return "";
        const configRangeFilter = gobConfig.get("behaviour.rangefilter");
        const startopacity = configRangeFilter.startopacity / (configRangeFilter.maxopacity + 0.0);
        const endopacity = configRangeFilter.endopacity / (configRangeFilter.maxopacity + 0.0);
        const opacityByLevel = (startopacity - endopacity) / (configRangeFilter.opacitysteps - 1);
        const results = [];
        for (let i = 0; i <= configRangeFilter.opacitysteps; ++i) {
            const rangeFilterClass = Utility.formatString(Chat.CssClass.ChatEntry_FadeOut_Partial, i);
            const selectors = tabClasses.map(tabClass => `.${tabClass} .${rangeFilterClass}`);
            const properties = i === 0 ? { "display": "none" } : { "opacity": `${(i - 1) * opacityByLevel + endopacity}` };
            results.push(StyleBuilder.toCss(selectors, properties));
        }
        return results.join("");
    });
})();
(() => {
    StyleBuilder.RuleGenerators.push(() => {
        const configTabs = gobConfig.get("behaviour.chattabs.data");
        const tabs = Object.values(configTabs);
        const results = [];
        for (let tab of tabs) {
            const tabClass = Utility.formatString(Chat.CssClass.Chat_History_Tab_Partial, tab.id);
            const invisibleChannels = _.difference(Constants.ChannelEnumValues, tab.channel.visible)
                .map(id => Constants.ChannelEnumToKey[id])
                .map(key => Gobchat.Channels[key])
                .map(channel => channel.internalName);
            const selectors = invisibleChannels.map(channelName => `.${tabClass} .${Utility.formatString(Chat.CssClass.ChatEntry_Channel_Partial, channelName)}`);
            results.push(StyleBuilder.toCss(selectors, { "display": "none" }));
        }
        return results.join("");
    });
})();
(() => {
    StyleBuilder.RuleGenerators.push(() => {
        const configTriggerGroups = gobConfig.get("behaviour.groups.data");
        const results = [];
        for (let key of Object.keys(configTriggerGroups)) {
            const triggerGroup = configTriggerGroups[key];
            const cssClass = Utility.formatString(Chat.CssClass.ChatEntry_TriggerGroup_Partial, triggerGroup.id);
            results.push(StyleBuilder.toCss(`.${cssClass}`, triggerGroup.style.body));
            results.push(StyleBuilder.toCss(`.${cssClass} .${Chat.CssClass.ChatEntry_Sender}`, triggerGroup.style.header));
        }
        return results.join("");
    });
})();
(() => {
    StyleBuilder.RuleGenerators.push(() => {
        const configStyle = gobConfig.get("style");
        const results = [];
        results.push(StyleBuilder.toCss(`.${Chat.CssClass.ChatEntry_MarkedbySearch}:not(.${Chat.CssClass.ChatEntry_SelectedBySearch})`, configStyle.chatsearch.marked));
        results.push(StyleBuilder.toCss(`.${Chat.CssClass.ChatEntry_SelectedBySearch}`, configStyle.chatsearch.selected));
        return results.join("");
    });
})();

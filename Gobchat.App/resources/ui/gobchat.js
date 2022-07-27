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

'use strict'

/* Resize event handler display */
document.addEventListener("OverlayStateUpdate", function (e) {
    if (!e.detail.isLocked)
        document.documentElement.classList.add("gob-resize-active");
    else
        document.documentElement.classList.remove("gob-resize-active");
});

jQuery(async function ($) {
    window.gobconfig = new Gobchat.GobchatConfig(true)
    await gobconfig.loadConfig()

    window.gobLocale = new Gobchat.LocaleManager()
    gobLocale.setLocale(gobconfig.get("behaviour.language"))
    gobconfig.addProfileEventListener(event => {
        if (event.type === "active")
            gobLocale.setLocale(gobconfig.get("behaviour.language"))
    })
    gobconfig.addPropertyEventListener("behaviour.language", event => {
        if (event.isActive)
            gobLocale.setLocale(gobconfig.get("behaviour.language"))
    })

    window.chatManager = new Gobchat.ChatboxManager()
    await chatManager.control($("#chatbox"))

    GobchatAPI.setUIReady(true)
});

/*
(function () {
    jQuery(function ($) {
        (async () => {
            window.gobconfig = new Gobchat.GobchatConfig(true)
            await gobconfig.loadConfig()

            window.gobLocale = new Gobchat.LocaleManager()
            gobLocale.setLocale(gobconfig.get("behaviour.language"))
            gobconfig.addProfileEventListener(event => {
                if (event.type === "active")
                    gobLocale.setLocale(gobconfig.get("behaviour.language"))
            })
            gobconfig.addPropertyEventListener("behaviour.language", event => {
                if (event.isActive)
                    gobLocale.setLocale(gobconfig.get("behaviour.language"))
            })

            window.chatManager = new Gobchat.ChatboxManager()
            await chatManager.control($("#chatbox"))

            GobchatAPI.setUIReady(true)
        })();
    });
}());
*/

// feature - open config
jQuery(function ($) {
    const navEntries = performance.getEntriesByType("navigation");
    for (let i = 0; i < navEntries.length; ++i) {
        if (navEntries[i].type === "reload") {
            localStorage.removeItem("gobchat-config-open")
            break
        }
    }

    function openGobchatConfig() {
        const isConfigOpen = window.localStorage.getItem("gobchat-config-open") || "false"

        if (isConfigOpen === "true")
            return

        window.localStorage.setItem("gobchat-config-open", "true")

        window.gobconfig.saveToLocalStore()

        const configWidth = gobconfig.get("behaviour.frame.config.size.width")
        const configHeight = gobconfig.get("behaviour.frame.config.size.height")

        const handle = window.open("config/config.html", 'Settings', `width=${configWidth},height=${configHeight}`)
        handle.saveConfig = function () {
            window.gobconfig.loadFromLocalStore()
            window.chatManager.updateStyle()
        }

        handle.focus()

        const timer = setInterval(function () {
            if (handle.closed) {
                clearInterval(timer);
                window.localStorage.removeItem("gobchat-config-open")
            }
        }, 1000);
    }

    $("#gobchat_showconfig").on("click", openGobchatConfig)
    window.openGobconfig = openGobchatConfig
})

// feature - chat search
jQuery(function ($) {
    const CssClass_Selected = "search-msg-selected"
    const Selector_AllSelected = `.${CssClass_Selected}`
    const Selector_VisibleSelected = `.${CssClass_Selected}:visible`

    const CssClass_Marked = "search-msg-marked"
    const Selector_AllMarked = `.${CssClass_Marked}`
    const Selector_VisibleMarked = `.${CssClass_Marked}:visible`

    function GetSearchPanel() {
        return $("#chatbox_search")
    }

    function GetSearchInput() {
        return $("#chatbox_search_txt")
    }

    function GetSearchCounter() {
        return $("#chatbox_search_hits")
    }

    function GetChatHistory() {
        return $("#chatbox .chat-panel > *").first()
    }

    function toggleChatSearch() {
        const $panel = GetSearchPanel()
        $panel.toggle()
        if ($panel.is(":visible"))
            GetSearchInput().focus()
        else
            clearChatSearch()

        window.chatManager.scrollToBottomIfNeeded()

        //Trigger reflow, needed because flex does not recalculate column height after content changes
        //What a nasty bug
        const chatFrame = GetChatHistory()[0]
        const oldDisplay = chatFrame.style.display
        chatFrame.style.display = "none"
        chatFrame.offsetHeight
        chatFrame.style.display = oldDisplay
    }

    function chatSearchProcessCounter(fun) {
        const $counter = GetSearchCounter()
        const txt = $counter.text()
        const split = txt.split("/")
        let total = 0
        let current = 0
        if (split.length == 2) {
            current = parseInt(split[0])
            total = parseInt(split[1])
        }
        const [nCurrent, nTotal] = fun(current, total)
        $counter.text(`${nCurrent} / ${nTotal}`)
    }

    function startChatSearch() {
        clearChatSearch()

        let searchText = GetSearchInput().val()
        if (searchText === null || searchText === undefined)
            return

        searchText = searchText.trim().toUpperCase()
        if (searchText.length === 0)
            return

        GetChatHistory().find(".chat-entry:visible").each(function () {
            if ($(this).text().toUpperCase().indexOf(searchText) >= 0)
                $(this).addClass(CssClass_Marked)
        })

        const $allVisibleHits = GetChatHistory().find(Selector_VisibleMarked)
        $allVisibleHits.last().addClass(CssClass_Selected)
        chatSearchProcessCounter((c, t) => [Math.min(1, $allVisibleHits.length), $allVisibleHits.length])

        scrollChatSearch()
    }

    function clearChatSearch() {
        GetChatHistory().find(Selector_AllSelected).removeClass(CssClass_Selected)
        GetChatHistory().find(Selector_AllMarked).removeClass(CssClass_Marked)
        chatSearchProcessCounter((c, t) => [0, 0])
    }

    function scrollChatSearch() {
        const $target = GetChatHistory().find(Selector_VisibleSelected)
        if ($target.length === 0)
            return

        const target = $target[0]

        const frame = GetChatHistory()[0]

        const containerTop = frame.scrollTop
        const containerBot = frame.clientHeight + containerTop
        const elementTop = target.offsetTop - frame.offsetTop
        const elementBot = target.clientHeight + elementTop
        const isVisible = containerTop <= elementTop && elementBot <= containerBot

        if (isVisible)
            return

        const position = elementTop;

        $(frame).animate({
            scrollTop: position /// $(".chatbox-search-msg-active").first().offset().top // $("chatboxcontent")[0].scrollHeight - $("chatboxcontent")[0].clientHeight
        }, 100)
    }

    $("#chatbox_search").hide()
    $("#gobchat_togglechatsearch").on("click", (e) => { toggleChatSearch() })

    $("#chatbox_search").on("keyup", function (e) {
        if (event.keyCode !== 13) // enter
            return
        startChatSearch()
    })

    $("#chatbox_search_delete").on("click", function (e) {
        clearChatSearch()
    })

    $("#chatbox_search_down").on("click", function (e) {
        const $currentSearch = GetChatHistory().find(Selector_AllSelected)
        $currentSearch.removeClass(CssClass_Selected)

        const $prevSearch = $currentSearch
            .nextUntil(Selector_VisibleMarked)
            .addBack()
            .last()
            .next(Selector_VisibleMarked)

        if ($prevSearch.length !== 0) {
            $prevSearch.addClass(CssClass_Selected)
            chatSearchProcessCounter((c, t) => [c - 1, t])
        } else {
            GetChatHistory().find(Selector_VisibleMarked)
                .first()
                .addClass(CssClass_Selected)
            chatSearchProcessCounter((c, t) => [t, t])
        }
        scrollChatSearch()
    })

    $("#chatbox_search_up").on("click", function (e) {
        const $currentSearch = GetChatHistory().find(Selector_AllSelected)
        $currentSearch.removeClass(CssClass_Selected)

        const $prevSearch = $currentSearch
            .prevUntil(Selector_VisibleMarked)
            .addBack()
            .first()
            .prev(Selector_VisibleMarked)

        if ($prevSearch.length !== 0) {
            $prevSearch.addClass(CssClass_Selected)
            chatSearchProcessCounter((c, t) => [c + 1, t])
        } else {
            GetChatHistory().find(Selector_VisibleMarked)
                .last()
                .addClass(CssClass_Selected)
            chatSearchProcessCounter((c, t) => [1, t])
        }
        scrollChatSearch()
    })

    $("#chatbox_search_hits").on("click", function (e) {
        scrollChatSearch()
    })
})
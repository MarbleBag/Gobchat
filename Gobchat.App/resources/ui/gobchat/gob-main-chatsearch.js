/*******************************************************************************
 * Copyright (C) 2019-2020 MarbleBag
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

jQuery(function ($) {
    function toggleChatSearch() {
        $("#chatbox_search").toggle()
        if ($("#chatbox_search").is(":visible"))
            $("#chatbox_search_txt").focus()
        else
            clearChatSearch()

        window.chatManager.scrollToBottomIfNeeded()

        //Trigger reflow, needed because flex does not recalculate column height after content changes
        //What a nasty bug
        const chatFrame = $("#chatbox_content")[0]
        chatFrame.style.display = "none"
        chatFrame.offsetHeight
        chatFrame.style.display = "flex"
    }

    function chatSearchProcessCounter(fun) {
        const $counter = $("#chatbox_search_hits")
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

        let searchText = $("#chatbox_search_txt").val()
        if (searchText === null || searchText === undefined) return
        searchText = searchText.trim().toUpperCase()
        if (searchText.length === 0) return

        $("#chatbox_content").find(".message-body-base").each(function (i, e) {
            if ($(this).text().toUpperCase().indexOf(searchText) >= 0)
                $(this).addClass("chatbox-search-msg-marked")
        })

        const allHits = $(".chatbox-search-msg-marked")
        allHits.last().addClass("chatbox-search-msg-selected")
        chatSearchProcessCounter((c, t) => [Math.min(1, allHits.length), allHits.length])

        scrollChatSearch()
    }

    function clearChatSearch() {
        $("#chatbox_content").find(".chatbox-search-msg-selected").removeClass("chatbox-search-msg-selected")
        $("#chatbox_content").find(".chatbox-search-msg-marked").removeClass("chatbox-search-msg-marked")
        chatSearchProcessCounter((c, t) => [0, 0])
    }

    function scrollChatSearch() {
        const target = $("#chatbox_content").find(".chatbox-search-msg-selected")[0]
        if (!target)
            return

        const frame = $("#chatbox_content")[0]

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
        const currentSearch = $(".chatbox-search-msg-selected")
        currentSearch.removeClass("chatbox-search-msg-selected")
        const prevSearch = currentSearch.nextUntil(".chatbox-search-msg-marked").addBack().last().next(".chatbox-search-msg-marked")
        if (prevSearch.length !== 0) {
            prevSearch.addClass("chatbox-search-msg-selected")
            chatSearchProcessCounter((c, t) => [c - 1, t])
        } else {
            $(".chatbox-search-msg-marked").first().addClass("chatbox-search-msg-selected")
            chatSearchProcessCounter((c, t) => [t, t])
        }
        scrollChatSearch()
    })

    $("#chatbox_search_up").on("click", function (e) {
        const currentSearch = $(".chatbox-search-msg-selected")
        currentSearch.removeClass("chatbox-search-msg-selected")
        const prevSearch = currentSearch.prevUntil(".chatbox-search-msg-marked").addBack().first().prev(".chatbox-search-msg-marked")
        if (prevSearch.length !== 0) {
            prevSearch.addClass("chatbox-search-msg-selected")
            chatSearchProcessCounter((c, t) => [c + 1, t])
        } else {
            $(".chatbox-search-msg-marked").last().addClass("chatbox-search-msg-selected")
            chatSearchProcessCounter((c, t) => [1, t])
        }
        scrollChatSearch()
    })

    $("#chatbox_search_hits").on("click", function (e) {
        scrollChatSearch()
    })
})
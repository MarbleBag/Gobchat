'use strict'

var Gobchat = (function (Gobchat) {
    const ChatMessageEntrySelector = ".message-body-base"

    const SearchMarkedMessage = "chatbox-search-msg-selected"
    const SearchMarkedMessageSelector = `.${SearchMarkedMessage}`

    const SearchFoundMessage = "chatbox-search-msg-found"
    const SearchFoundMessageSelector = `.${SearchFoundMessage}`

    function scrollChatToActive($chat) {
        const target = $chat.find(SearchMarkedMessageSelector)[0]
        if (!target)
            return

        const frame = $chat[0]

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

    function buildSearchFunction(txt, advanced) {
        if (!advanced) {
            const jQuerySearch = function (i, e) {
                const found = $(this).text().toUpperCase().indexOf(txt) >= 0;
                if (found) {
                    $(this).addClass(SearchFoundMessage)
                }
            }
            return jQuerySearch
        } else {
        }
    }

    function startChatSearch($chat, advanced) {
        clearChatSearch($chat)

        searchParam = getSearchTerm($("#chatbox_search").val())
        if (searchParam === null) return

        $chat.find(ChatMessageEntrySelector).each(function (i, e) {
            const found = $(this).text().toUpperCase().indexOf(searchParam) >= 0;
            if (found) {
                $(this).addClass(SearchFoundMessage)
            }
        })

        $(SearchFoundMessageSelector).last().addClass(SearchMarkedMessage)

        scrollChatToActive($chat)
    }

    function getSearchTerm(text) {
        if (text === null || text === undefined) return null
        text = text.trim().toUpperCase()
        if (text.length === 0) return null
        return text
    }

    function clearChatSearch($chat) {
        $chat.find(SearchMarkedMessageSelector).removeClass(SearchMarkedMessage)
        $chat.find(SearchFoundMessageSelector).removeClass(SearchFoundMessage)
    }

    class GobchatSearch {
    }

    /*
    const shadowTemplate = document.createElement('template')
    const lightTemplate = document.createElement('template')

    /*<style>
      :host {
        <display: inline-block;
        background: url('../images/unchecked-checkbox.svg') no-repeat;
        background-size: contain;
        width: 24px;
        height: 24px;
      }
      :host([hidden]) {
        display: none;
      }
      :host([checked]) {
        background: url('../images/checked-checkbox.svg') no-repeat;
        background-size: contain;
      }
      :host([disabled]) {
        background:
          url('../images/unchecked-checkbox-disabled.svg') no-repeat;
        background-size: contain;
      }
      :host([checked][disabled]) {
        background:
          url('../images/checked-checkbox-disabled.svg') no-repeat;
        background-size: contain;
      }
    </style>

    shadowTemplate.innerHTML = `
        <div>
            <input  id="search_txt" />
            <button id="search_go">      <slot name="startSearch">Go</slot></button>
            <button id="search_delete">  <slot name="deleteSearch"></slot></button>
            <button id="search_backward"><slot name="nextSearch"></slot></button>
            <button id="search_forward"> <slot name="previousSearch"></slot></button>
            <label  id="search_results"></label>
        </div>
    `

    lightTemplate.innerHTML = `
        <i class="fas fa-check" slot="startSearch"></i>
        <i class="fas fa-undo-alt" slot="deleteSearch"></i>
        <i class="fas fa-caret-left" slot="nextSearch"></i>
        <i class="fas fa-caret-right" slot="previousSearch"></i>
    `

    class GobchatChatSearch extends HTMLElement {
        static get observedAttributes() {
            return ["disabled", "open"];
        }

        constructor() {
            super();
            this.attachShadow({ mode: 'open' });
            this.shadowRoot.appendChild(shadowTemplate.content.cloneNode(true));
        }

        connectedCallback() {
            this.appendChild(lightTemplate.content.cloneNode(true));

            const $shadowRoot = $(this.shadowRoot)
            console.log($shadowRoot.find("#search_go").length)
            $shadowRoot.find("#search_go").on("keyup", function (e) {
                if (event.keyCode !== 13) return // enter
                startChatSearch()
            })
        }

        disconnectedCallback() {
            $(this).empty()
        }

        attributeChangedCallback(attrName, oldVal, newVal) {
        }
    }

    function upgradeProperty(obj, prop) {
        if (obj.hasOwnProperty(prop)) {
            let value = obj[prop];
            delete obj[prop];
            obj[prop] = value;
        }
    }

    customElements.define('gobchat-chatsearch', GobchatChatSearch);

    */

    return Gobchat
}(Gobchat || {}));
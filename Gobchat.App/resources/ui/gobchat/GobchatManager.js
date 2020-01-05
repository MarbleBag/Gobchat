'use strict'

//requieres Gobchat.MessageParser
//requieres Gobchat.MessageHtmlBuilder

var Gobchat = (function (Gobchat) {
    function getHourAndMinutes() {
        function twoDigits(t) {
            return t < 10 ? '0' + t : t;
        }

        const d = new Date()
        const hours = twoDigits(d.getHours())
        const minutes = twoDigits(d.getMinutes())
        return `${hours}:${minutes}`
    }

    class GobchatManager {
        constructor(chatHtmlId) {
            this._chatHtmlId = chatHtmlId
        }

        async init() {
            const self = this

            this._chatConfig = new Gobchat.GobchatConfig(true)
            await this.config.loadConfig()

            this._chatConfig.addProfileEventListener(event => {
                if (event.type === "active") this.updateStyle()
            })
            this._chatConfig.addPropertyEventListener("style", event => {
                if (event.isActive) this.updateStyle()
            })
            this._chatConfig.addPropertyEventListener("behaviour", event => {
                if (event.isActive) this.updateStyle()
            })

            this.updateStyle()

            this._messageParser = new Gobchat.MessageParser(self._chatConfig)
            this._messageHtmlBuilder = new Gobchat.MessageHtmlBuilder(self._chatConfig)

            this._cmdManager = new Gobchat.CommandManager(this, self._chatConfig)

            this._scrollbar = new ScrollbarControl(this._chatHtmlId)
            this._scrollbar.init()

            document.addEventListener("ChatMessageEvent", (e) => { self.onNewMessageEvent(e) })
            Gobchat.sendMessageToPlugin({ event: "GobchatReady" })
        }

        get config() {
            return this._chatConfig
        }

        saveConfigToLocalStore() {
            this._chatConfig.saveToLocalStore()
        }

        loadConfigFromLocalStore() {
            this._chatConfig.loadFromLocalStore()
            this._chatConfig.saveConfig()
        }

        //TODO test
        updateStyle() {
            Gobchat.StyleBuilder.updateStyle(this.config, "custome_style_id")
        }

        sendErrorMessage(msg) {
            this.onNewMessage(
                new Gobchat.Message(
                    getHourAndMinutes(),
                    new Gobchat.MessageSource("Gobchat"),
                    Gobchat.ChannelEnum.GOBCHAT_ERROR,
                    [new Gobchat.MessageSegment(Gobchat.MessageSegmentEnum.UNDEFINED, msg)]
                )
            )
        }

        sendInfoMessage(msg) {
            this.onNewMessage(
                new Gobchat.Message(
                    getHourAndMinutes(),
                    new Gobchat.MessageSource("Gobchat"),
                    Gobchat.ChannelEnum.GOBCHAT_INFO,
                    [new Gobchat.MessageSegment(Gobchat.MessageSegmentEnum.UNDEFINED, "&lt;Gobchat&gt; " + msg)]
                )
            )
        }

        onNewMessageEvent(messageEvent) {
            const message = this._messageParser.parseMessageEvent(messageEvent)
            if (!message) return
            this.onNewMessage(message)
            if (message.channel === Gobchat.ChannelEnum.ECHO) {
                this._cmdManager.processCommand(messageEvent.detail.message)
            }
        }

        onNewMessage(message) {
            const messageHtmlElement = this._messageHtmlBuilder.buildHtmlElement(message)
            $("#" + this._chatHtmlId).append(messageHtmlElement)
            this._scrollbar.scrollToBottomIfNeeded()
        }
    }
    Gobchat.GobchatManager = GobchatManager

    class ScrollbarControl {
        constructor(scrollTargetId) {
            this._scrollTargetId = "#" + scrollTargetId
            this._bScrollToBottom = true
        }

        init() {
            const control = this
            const scrollTarget = this._scrollTargetId
            $(scrollTarget).on('scroll', function () {
                let closeToBottom = ($(this).scrollTop() + $(this).innerHeight() + 5 >= $(this)[0].scrollHeight) // +5px for 'being very close'
                control._bScrollToBottom = closeToBottom
            })

            /*$(scrollTarget).on("wheel",function(event){
                const scrollData = event.originalEvent.deltaY
                const scrollElement = $(scrollTarget)[0]

                const viewPortSize = scrollElement.clientHeight
                const scrollableSpace = scrollElement.scrollHeight

                if( scrollableSpace <= viewPortSize )
                    return

                const scrollDistance = 0 //px

                if(scrollData < 0){
                    scrollElement.scrollTop = Math.max(0,scrollElement.scrollTop - scrollDistance)
                }else{
                    scrollElement.scrollTop = Math.min(scrollableSpace-viewPortSize, scrollElement.scrollTop + scrollDistance)
                }

                //console.log($(scrollTarget)[0].clientHeight)
                //console.log($(scrollTarget)[0].scrollHeight)

                //console.log($(scrollTarget)[0].scrollTop)
            })*/

            /*$(scrollTarget).on("mouseover",function(event){
                $(scrollTarget)[0].focus()
            })*/
        }

        get isScrollingNeeded() {
            return this._bScrollToBottom
        }

        scrollToBottomIfNeeded() {
            if (this._bScrollToBottom) {
                const scrollTarget = this._scrollTargetId
                $(scrollTarget).animate({
                    scrollTop: $(scrollTarget)[0].scrollHeight - $(scrollTarget)[0].clientHeight
                }, 10);
            }
        }
    }

    return Gobchat
}(Gobchat || {}));
'use strict'

//requieres Gobchat.MessageParser
//requieres Gobchat.MessageHtmlBuilder

var Gobchat = (function(Gobchat){
	
		//TODO cleanup
		class ParserConfigTest{ //TODO make this configurateable
			constructor(manager){
				this._manager = manager
				this.mentions = []
			}
			
			isMessageRelevant(channelEnum){
				return _.indexOf(Gobchat.PlayerChannel, channelEnum) != -1
			}
			
			isRoleplayChannel(channelEnum){				
				return _.indexOf(Gobchat.RoleplayChannel, channelEnum) != -1
			}
			
			isMentionChannel(channelEnum){
				return true
			}			
		}
		
		class GobchatManager {
			constructor(chatHtmlId){
				this._chatHtmlId = chatHtmlId
			}
			
			init(){
				const manager = this
				const config = new ParserConfigTest(manager)
				this.messageParser = new Gobchat.MessageParser(config,(message) => { manager.onNewMessage(message) })
				this.messageHtmlBuilder = new Gobchat.MessageHtmlBuilder()
				this.scrollbar = new ScrollbarControl(this._chatHtmlId)
				this.scrollbar.init()
				
				//TODO cleanup				
				function onMentionEvent(mentionEvent){
					let mentions = mentionEvent.detail.mentions
					if( mentions ){						
						config.mentions = mentions.map((e)=>{return e.toLowerCase().trim()}).filter((e)=>{return e.length>0})
					}else{
						config.mentions = []
					}
				}
				
				document.addEventListener("ChatMessageEvent", (e) => { manager.messageParser.parseMessageEvent(e) })
				document.addEventListener("MentionsEvent", onMentionEvent)
				
				Gobchat.sendMessageToPlugin({event:"RequestMentions"})	
			}
			
			onNewMessage(message){
				const messageHtmlElement = this.messageHtmlBuilder.buildHtmlElement(message)
				$("#"+this._chatHtmlId).append(messageHtmlElement)
				this.scrollbar.scrollToBottomIfNeeded()
			}
		}
		Gobchat.GobchatManager = GobchatManager
		
		class ScrollbarControl {
			constructor(scrollTargetId){
				this._scrollTargetId = "#" + scrollTargetId
				this._bScrollToBottom = true
			}
			
			init(){
				const control = this
				const scrollTarget = this._scrollTargetId
				$(scrollTarget).on('scroll', function () {
					let closeToBottom = ($(this).scrollTop() + $(this).innerHeight() + 5 >= $(this)[0].scrollHeight) // +5px for 'being very close'
					control._bScrollToBottom = closeToBottom
				})
			}
			
			get isScrollingNeeded(){
				return this._bScrollToBottom
			}
			
			scrollToBottomIfNeeded(){
				if (this._bScrollToBottom) {
					const scrollTarget = this._scrollTargetId 
					$(scrollTarget).animate({
						scrollTop: $(scrollTarget)[0].scrollHeight - $(scrollTarget)[0].clientHeight
					}, 500);
				}
			}
		}
		
	
		return Gobchat	
}(Gobchat || {}));
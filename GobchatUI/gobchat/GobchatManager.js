'use strict'

//requieres Gobchat.MessageParser
//requieres Gobchat.MessageHtmlBuilder

var Gobchat = (function(Gobchat){
	
		//TODO cleanup
		class ParserConfigTest{ //TODO make this configurateable
			constructor(manager){
				this.getConfig = function(key,defaultValue){
					return manager._chatConfig.get(key,defaultValue)
				}
			}
			
			isMessageRelevant(channelEnum){
				const channels = this.getConfig("behaviour.channel.visible")
				return _.includes(channels,channelEnum)
				//return _.indexOf(channels, channelEnum) != -1
			}
			
			isRoleplayChannel(channelEnum){		
				const channels = this.getConfig("behaviour.channel.roleplay")
				return _.includes(channels,channelEnum)			
				//return _.indexOf(Gobchat.RoleplayChannel, channelEnum) != -1
			}
			
			isMentionChannel(channelEnum){
				const channels = this.getConfig("behaviour.channel.mention")
				return _.includes(channels,channelEnum)		
			}

			get isAutodetectEmoteInSay(){
				const val = this._manager._chatConfig.get("behaviour.autodetectEmoteInSay")
				return val
			}
			
			get mentions(){
				return this.getConfig("behaviour.mentions")
			}
		}
		
		class GobchatManager {
			constructor(chatHtmlId){
				this._chatHtmlId = chatHtmlId
			}
			
			init(){
				const manager = this
				
				this._chatConfig = new Gobchat.GobchatConfig()
				this._chatConfig.loadFromPlugin()
				this.updateStyle()
				
				const parserConfig = new ParserConfigTest(manager)
				this._messageParser = new Gobchat.MessageParser(parserConfig,(message) => { manager.onNewMessage(message) })
				this._messageHtmlBuilder = new Gobchat.MessageHtmlBuilder()
				
				this._scrollbar = new ScrollbarControl(this._chatHtmlId)
				this._scrollbar.init()
				
				//TODO cleanup				
				function onMentionEvent(mentionEvent){
					let mentions = mentionEvent.detail.mentions
					if( mentions ){						
						parserConfig.mentions = mentions.map((e)=>{return e.toLowerCase().trim()}).filter((e)=>{return e.length>0})
					}else{
						parserConfig.mentions = []
					}
				}
				
				document.addEventListener("ChatMessageEvent", (e) => { manager._messageParser.parseMessageEvent(e) })
				document.addEventListener("MentionsEvent", onMentionEvent)
				
				Gobchat.sendMessageToPlugin({event:"RequestMentions"})	
			}
			
			saveConfigToLocalStore(){
				this._chatConfig.saveToLocalStore()
			}
			
			loadConfigFromLocalStore(){
				this._chatConfig.loadFromLocalStore()
				this._chatConfig.saveToPlugin()
			}
			
			//TODO test
			updateStyle(){
				Gobchat.StyleBuilder.updateStyle(this._chatConfig.configStyle,"custome_style_id")
			}
			
			onNewMessage(message){
				const messageHtmlElement = this._messageHtmlBuilder.buildHtmlElement(message)
				$("#"+this._chatHtmlId).append(messageHtmlElement)
				this._scrollbar.scrollToBottomIfNeeded()
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
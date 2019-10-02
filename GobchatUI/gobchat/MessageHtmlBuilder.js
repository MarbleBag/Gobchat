'use strict'

var Gobchat = (function(Gobchat){
	
	const ChannelEnum = Gobchat.ChannelEnum
	const MessageSegmentEnum = Gobchat.MessageSegmentEnum
	
	function getCssClassForMessageChannel(channelEnum){		
		 switch (channelEnum) {
			case ChannelEnum.SAY:         		return "message-body-say"
			case ChannelEnum.EMOTE:           	return "message-body-emote"
			case ChannelEnum.TELL_SEND:       	return "message-body-tellsend"
			case ChannelEnum.TELL_RECIEVE:    	return "message-body-tellrecieve"
			case ChannelEnum.GUILD:           	return "message-body-guild"
			case ChannelEnum.YELL:            	return "message-body-yell"
			case ChannelEnum.SHOUT:     		return "message-body-shout"
			case ChannelEnum.PARTY:           	return "message-body-party"
			case ChannelEnum.ALLIANCE:        	return "message-body-alliance"
			case ChannelEnum.LINKSHELL_1:
			case ChannelEnum.LINKSHELL_2:
			case ChannelEnum.LINKSHELL_3:
			case ChannelEnum.LINKSHELL_4:
			case ChannelEnum.LINKSHELL_5:
			case ChannelEnum.LINKSHELL_6:
			case ChannelEnum.LINKSHELL_7:
			case ChannelEnum.LINKSHELL_8:     	return "message-body-linkshell"
			case ChannelEnum.WORLD_LINKSHELL_1:
			case ChannelEnum.WORLD_LINKSHELL_2:
			case ChannelEnum.WORLD_LINKSHELL_3:
			case ChannelEnum.WORLD_LINKSHELL_4:
			case ChannelEnum.WORLD_LINKSHELL_5:
			case ChannelEnum.WORLD_LINKSHELL_6:
			case ChannelEnum.WORLD_LINKSHELL_7:
			case ChannelEnum.WORLD_LINKSHELL_8:	return "message-body-worldlinkshell"
			case ChannelEnum.ERROR:				return "message-body-error"
			default:                        	return null
		}
	}
	
		
	function getCssClassForMessageSegmentType(messageSegmentEnum){
		switch(messageSegmentEnum){
			case MessageSegmentEnum.SAY: 		return "message-segment-say"
			case MessageSegmentEnum.EMOTE: 		return "message-segment-emote"
			case MessageSegmentEnum.OOC: 		return "message-segment-ooc"
			case MessageSegmentEnum.MENTION: 	return "message-segment-mention"
			default:							return null
		}
	}	
	
	function getMessageSenderInnerHtml(messageSource,channelEnum){
		const sourceName = messageSource != null ? messageSource.sourceId : null
		
		switch(channelEnum){
			case ChannelEnum.ECHO:				return "Echo: "
			case ChannelEnum.EMOTE:           	return sourceName + " "
			case ChannelEnum.TELL_SEND:       	return "&gt;&gt; " + sourceName + ": "
			case ChannelEnum.TELL_RECIEVE:    	return sourceName + " &gt;&gt; "
			case ChannelEnum.ANIMATED_EMOTE:	return null //source is set, but the animation message already contains the source name
			case ChannelEnum.GUILD:           	return "[FC]&lt;" + sourceName + "&gt; "	
			case ChannelEnum.PARTY:           	return "&lt;" + sourceName + "&gt; "
			case ChannelEnum.ALLIANCE:        	return "&lt;" + sourceName + "&gt; "
			case ChannelEnum.LINKSHELL_1:		return "[LS1]&lt;" + sourceName + "&gt; "
			case ChannelEnum.LINKSHELL_2:		return "[LS2]&lt;" + sourceName + "&gt; "
			case ChannelEnum.LINKSHELL_3:		return "[LS3]&lt;" + sourceName + "&gt; "
			case ChannelEnum.LINKSHELL_4:		return "[LS4]&lt;" + sourceName + "&gt; "
			case ChannelEnum.LINKSHELL_5:		return "[LS5]&lt;" + sourceName + "&gt; "
			case ChannelEnum.LINKSHELL_6:		return "[LS6]&lt;" + sourceName + "&gt; "
			case ChannelEnum.LINKSHELL_7:		return "[LS7]&lt;" + sourceName + "&gt; "
			case ChannelEnum.LINKSHELL_8:     	return "[LS8]&lt;" + sourceName + "&gt; "
			case ChannelEnum.WORLD_LINKSHELL_1: return "[CWLS1]&lt;" + sourceName + "&gt; "
			case ChannelEnum.WORLD_LINKSHELL_2: return "[CWLS2]&lt;" + sourceName + "&gt; "
			case ChannelEnum.WORLD_LINKSHELL_3: return "[CWLS3]&lt;" + sourceName + "&gt; "
			case ChannelEnum.WORLD_LINKSHELL_4: return "[CWLS4]&lt;" + sourceName + "&gt; "
			case ChannelEnum.WORLD_LINKSHELL_5: return "[CWLS5]&lt;" + sourceName + "&gt; "
			case ChannelEnum.WORLD_LINKSHELL_6: return "[CWLS6]&lt;" + sourceName + "&gt; "
			case ChannelEnum.WORLD_LINKSHELL_7: return "[CWLS7]&lt;" + sourceName + "&gt; "
			case ChannelEnum.WORLD_LINKSHELL_8:	return "[CWLS8]&lt;" + sourceName + "&gt; "
			default:
				if(sourceName != null){
					return sourceName+ ": "
				}
		}
		return null
	}
	
	class MessageHtmlBuilder{
		constructor(){
			
		}
		
		buildHtmlElement(message){			
			const chatEntry = document.createElement("div")
			chatEntry.classList.add("message-body-base")
			
			const timeElement = document.createElement("span")
			timeElement.innerHTML = "[" + message.timestamp + "] "
			chatEntry.appendChild(timeElement)
			
			const messageContainer = document.createElement("span")
			const channelClass = getCssClassForMessageChannel(message.channel)
			if (channelClass) messageContainer.classList.add(channelClass)
			chatEntry.appendChild(messageContainer)
		
			const senderInnerHtml = getMessageSenderInnerHtml(message.source, message.channel)
			if(senderInnerHtml != null){
				const senderElement = document.createElement("span")
				senderElement.innerHTML = senderInnerHtml
				messageContainer.appendChild(senderElement)   
			}
			
			message.segments.forEach((segment) => {
				const segmentType = segment.segmentType
				const segmentText = segment.messageText
				
				const segmentElement = document.createElement("span")
				segmentElement.innerHTML = segmentText
				
				const segmentClass = getCssClassForMessageSegmentType(segmentType)
				if(segmentClass) segmentElement.classList.add(segmentClass)
				
				messageContainer.appendChild(segmentElement)	
			})
			
			return chatEntry
		}
	}
	Gobchat.MessageHtmlBuilder = MessageHtmlBuilder
	
	return Gobchat	
}(Gobchat || {}));
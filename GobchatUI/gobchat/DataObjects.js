'use strict'

var Gobchat = (function(Gobchat){
	
	class MessageSegment {
		constructor(messageSegmentEnum,messageText){
			this.segmentType = messageSegmentEnum
			this.messageText = messageText
		}
	}
	Gobchat.MessageSegment = MessageSegment
		
	class MessageSource {
		constructor(sourceId){
			this.sourceId = sourceId
		}
	}
	Gobchat.MessageSource = MessageSource
		
	class Message {
		constructor(timestamp, messageSource, channelEnum, messageSegments){
			this.timestamp = timestamp
			this.source = messageSource
			this.channel = channelEnum
			this.segments = messageSegments
		}
	}
	Gobchat.Message = Message
		
	class Player{
		constructor(playerId){
			this.playerId = playerId
		}
	}
	
	return Gobchat	
}(Gobchat || {}));
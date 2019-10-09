'use strict'

var Gobchat = (function(Gobchat){
	
	const ChannelEnum = Gobchat.ChannelEnum
	const MessageSegmentEnum = Gobchat.MessageSegmentEnum
	const MessageSegment = Gobchat.MessageSegment
	const MessageSource = Gobchat.MessageSource
	const Message = Gobchat.Message
	
	class MessageParser {
		constructor(parserConfig,onMessageCallback){
			this.onMessageCallback = onMessageCallback
			this.parserConfig = parserConfig
		}
			
		parseMessageEvent(messageEvent){
			const messageEventDetail = messageEvent.detail
			const channel = messageEventDetail.type
			
			if( !this.isMessageRelevant(channel) ){
				return
			}
				
			const timestamp = messageEventDetail.timestamp
			const source = this.createMessageSource(channel, messageEventDetail.source)			
			const messageSegments = [new MessageSegment(MessageSegmentEnum.UNDEFINED, messageEventDetail.message)]
			const message = new Message(timestamp, source, channel, messageSegments)
				
			if( this.isRoleplayChannel(channel) ){
				processMessageSegmentOOC(message)
				processMessageSegmentSayAndEmote(message)
				if(this.isAutodetectEmoteInSay(channel)){
					searchMessageSegmentForEmoteInSay(message)
				}
			}
				
			if( this.isMentionChannel(channel) ){
				const mentions = this.parserConfig.mentions
				if( mentions.length === 0 ) return
				processMessageSegmentMention(message,mentions)
			}
			
			applyMessageSegmentDefaults(message)
				
			this.onMessageCallback(message)
		}
			
		createMessageSource(channelEnum,originalSource){			
			return new MessageSource(originalSource) //TODO for now
		}
		
		isAutodetectEmoteInSay(channelEnum){
			return (channelEnum === ChannelEnum.SAY) && this.parserConfig.isAutodetectEmoteInSay
		}
			
		isMessageRelevant(channelEnum){
			return this.parserConfig.isMessageRelevant(channelEnum)		
		}
			
		isRoleplayChannel(channelEnum){
			return this.parserConfig.isRoleplayChannel(channelEnum)
		}
			
		isMentionChannel(channelEnum){
			return this.parserConfig.isMentionChannel(channelEnum)
		}			
	}		
	Gobchat.MessageParser = MessageParser
	
	class Marker{
		constructor(){
			this._activeMark = null
			this._marks = []
		}
		get mark(){
			return this._activeMark
		}
		get marks(){
			return this._marks
		}
		get count(){
			return this._marks.length + (this._activeMark ? 1 : 0)
		}
		clear(){
			this._activeMark = null
			this._marks = []
		}
		newMark(type,start,end){
			if(this._activeMark) this._marks.push(this._activeMark)			
			this._activeMark = {type: type, start: start, end: end}
		}
		finish(){
			if(this._activeMark) this._marks.push(this._activeMark)
			this._activeMark = null
		}
	}
	
	function applyMessageSegmentDefaults(message){
		//we have only defaults for SAY and EMOTE, skip rest		
		if(message.channel === ChannelEnum.SAY){
			message.segments.forEach((segment)=>{					
					if(segment.segmentType === MessageSegmentEnum.UNDEFINED){
						segment.segmentType = MessageSegmentEnum.SAY
					}
				})
		}else if(message.channel === ChannelEnum.EMOTE){
			message.segments.forEach((segment)=>{					
					if(segment.segmentType === MessageSegmentEnum.UNDEFINED){
						segment.segmentType = MessageSegmentEnum.EMOTE
					}
				})
		}
	}
	
	function searchMessageSegmentForEmoteInSay(message){
		const foundSay = message.segments.some((segment)=>{return segment.segmentType === MessageSegmentEnum.SAY})		
		if(foundSay){
			message.segments.forEach((segment)=>{					
					if(segment.segmentType === MessageSegmentEnum.UNDEFINED){
						segment.segmentType = MessageSegmentEnum.EMOTE
					}
				})
		}
	}
	
	function processMessageSegmentOOC(message){
		const ignoreFunction = (_) => {return false}
		const parseFunction = (marker,type,txt) => {
			const txtLength = txt.length
			
			marker.newMark(type,0,0)
			for(let i = 1; i<txtLength;++i){
				const c1 = txt.charAt(i-1)
				const c2 = txt.charAt(i)
				
				if( c1 === "(" && c2 === "(" ){
					if(marker.mark.type === MessageSegmentEnum.OOC) continue
					marker.mark.end = i-1
					marker.newMark(MessageSegmentEnum.OOC,i-1,i)										
				}else if( c1 === ")" && c2 === ")" ){
					if(marker.mark.type !== MessageSegmentEnum.OOC) continue
					marker.mark.end = i+1	
					marker.newMark(type,i+1,i+1)
				}
			}
			marker.mark.end = txtLength
			
			if(marker.count === 1) marker.clear()
		}
		
		processMessage(message,ignoreFunction,parseFunction)
	}
	
	function processMessageSegmentSayAndEmote(message){
		const ignoreFunction = (type) => {return type === MessageSegmentEnum.OOC}
		const parseFunction = (marker,type,txt) => {
			const txtLength = txt.length
			marker.newMark(type,0,0)
			for(let i = 0; i<txtLength;++i){
				const c = txt.charAt(i)
				if( c === '"' ){
					if( marker.mark.type === MessageSegmentEnum.SAY ){
						marker.mark.end = i+1
						marker.newMark(type,i+1,i+1)
					}else{
						marker.mark.end = i
						marker.newMark(MessageSegmentEnum.SAY,i,i)
					}
				}else if( c === '*' ){
					if( marker.mark.type === MessageSegmentEnum.EMOTE ){
						marker.mark.end = i+1
						marker.newMark(type,i+1,i+1)
					}else{
						marker.mark.end = i
						marker.newMark(MessageSegmentEnum.EMOTE,i,i)
					}
				}
			}
			marker.mark.end = txtLength
			if(marker.count === 1) marker.clear()
		}
		processMessage(message,ignoreFunction,parseFunction)
	}
	
	function processMessageSegmentMention(message,mentions){
		const ignoreFunction = (_) => {return false}
		const parseFunction = (marker,type,txt) => {
			const matches = findAllWordMatches(mentions, txt.toLowerCase())
			if( matches.length === 0) return			
			marker.newMark(type,0,0)			
			for(let match of matches){
				marker.mark.end = match[0]
				marker.newMark(MessageSegmentEnum.MENTION,match[0],match[1])
				marker.newMark(type,match[1],match[1])
			}			
			marker.mark.end = txt.length
		}
		processMessage(message,ignoreFunction,parseFunction)
	}
	
	// ignoreFunction(segmentType)
	// parseFunction(marker,segmentType,segmentText)
	function processMessage(message,ignoreFunction,parseFunction){ //a general solution for message parsing			
		const messageSegments = message.segments
		for(let i=0;i<messageSegments.length;++i){
			const messageSegment = messageSegments[i]
			const segmentType = messageSegment.segmentType
			const segmentText = messageSegment.messageText
			
			if(ignoreFunction(segmentType)) continue
			
			const marker = new Marker()				
			parseFunction(marker,segmentType,segmentText)
			
			marker.finish()
			const newSegments = marker.marks
									.filter((marker)=>{return marker.end-marker.start>0})
									.map((marker)=>{return new MessageSegment(marker.type, segmentText.substring(marker.start, marker.end))})
			
			if(newSegments.length > 0){
				messageSegments.splice.apply(messageSegments,[i,1].concat(newSegments))
				i += newSegments.length - 1 //so an already newly added segment doesn't get processed in the next step
			}
		}
	}
	
	function findAllWordMatches(words,textLine){
		function isBoundary(index){ 
			if(index < 0 || textLine.length <= index) return true //start & end		
			const c = textLine.charAt(index)
			const isLetter = c.toLowerCase() != c.toUpperCase() //works for a lot of character, but not for letters who don't have a diffrent lower and upper case :(
			return !isLetter //as long it's not a letter, it's okay!
		}
			
		function searchByIndexOf(result){
			let startIndex,endIndex, index
			for(let word of words){
				const length = word.length
				startIndex = 0
				while((index = textLine.indexOf(word,startIndex)) > -1){
					endIndex = index+length
					if(isBoundary(index-1) && isBoundary(endIndex)){
						result.push([index,endIndex])
					}
					startIndex = endIndex
				}			
			}
		}
			
		const result = []
		searchByIndexOf(result)
		result.sort((a,b)=>{return a[0]-b[0]})
		
		const merged = []
		if(result.length>0)
			merged.push(result[0])
			
		for(let i=1;i<result.length;++i){
			const last = merged[merged.length-1]
			const next = result[i]			
			if( next[0] <= last[1] ){
				last[1] = next[1]
			}else{
				merged.push(next)
			}		
		}

		return merged	
	}
	
	return Gobchat	
}(Gobchat || {}));
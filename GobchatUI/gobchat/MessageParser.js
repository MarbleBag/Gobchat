'use strict'

var Gobchat = (function(Gobchat){
	
	const ChannelEnum = Gobchat.ChannelEnum
	const MessageSegmentEnum = Gobchat.MessageSegmentEnum
	const MessageSegment = Gobchat.MessageSegment
	const MessageSource = Gobchat.MessageSource
	const Message = Gobchat.Message
	
	const PartyUnicodes = Object.freeze([
		Gobchat.FFUnicode.PARTY_1, Gobchat.FFUnicode.PARTY_2, Gobchat.FFUnicode.PARTY_3, Gobchat.FFUnicode.PARTY_4,
		Gobchat.FFUnicode.PARTY_5, Gobchat.FFUnicode.PARTY_6, Gobchat.FFUnicode.PARTY_7,  Gobchat.FFUnicode.PARTY_8
	])
	
	const RaidUnicodes = Object.freeze([
		Gobchat.FFUnicode.RAID_A, Gobchat.FFUnicode.RAID_B, Gobchat.FFUnicode.RAID_C
	])
	
	const FFGroupUnicodes = Object.freeze([
		Gobchat.FFUnicode.GROUP_1, Gobchat.FFUnicode.GROUP_2, Gobchat.FFUnicode.GROUP_3, Gobchat.FFUnicode.GROUP_4,
		Gobchat.FFUnicode.GROUP_5, Gobchat.FFUnicode.GROUP_6, Gobchat.FFUnicode.GROUP_7
	])
		
	class MessageParser {		
		constructor(config){
			this._config = config
			this._datacenter = null
		}
		
		isMessageRelevant(channelEnum){
			const channels = this._config.get("behaviour.channel.visible")
			return _.includes(channels,channelEnum)
		}
		
		isRoleplayChannel(channelEnum){		
			const channels = this._config.get("behaviour.channel.roleplay")
			return _.includes(channels,channelEnum)
		}
		
		isMentionChannel(channelEnum){
			const channels = this._config.get("behaviour.channel.mention")
			return _.includes(channels,channelEnum)		
		}

		isAutodetectEmoteInSay(channelEnum){
			return (channelEnum === ChannelEnum.SAY) && this._config.get("behaviour.autodetectEmoteInSay")
		}
		
		getMentions(){
			return this._config.get("behaviour.mentions")
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
					searchMessageSegmentsForEmoteInSay(message)
				}
			}
				
			if( this.isMentionChannel(channel) ){
				const mentions = this.getMentions()
				if( mentions && mentions.length !== 0 ){
					processMessageSegmentMention(message,mentions)
				}
			}
			
			applyMessageSegmentDefaults(message)
				
			return message
		}
			
		createMessageSource(channelEnum,originalSource){			
			const source = new MessageSource(originalSource)		
			
			if(originalSource != null && _.includes(Gobchat.PlayerChannel, channelEnum) ){ //message from a player (or at least it should!)
				let readIndex = 0
			
				function getIndexForUnicode(unicodeList){
					const unicodeValue = originalSource.codePointAt(readIndex)
					const unicodeIndex = _.findIndex(unicodeList, (e) => {return e.value === unicodeValue})
					return unicodeIndex 
				}
			
				if(ChannelEnum.PARTY === channelEnum){ //first character is the position within the party
					const index = getIndexForUnicode(PartyUnicodes)
					if(index>=0){
						source.prefix = (source.prefix || "") + `[${index + 1}]` //for now
						readIndex += 1 //party unicodes should be of size 1
					}				
				}else if(ChannelEnum.RAID === channelEnum){ //first character is the raid group
					let index = getIndexForUnicode(RaidUnicodes)
					if(index>=0){
						index += 'A'
						source.prefix = (source.prefix || "") + `[${String.fromCharCode(index)}]` //for now
						readIndex += 1 //raid unicodes should be of size 1
					}	
				}
				
				{
					const ffGroup = getIndexForUnicode(FFGroupUnicodes)
					if(ffGroup>=0){
						source.ffGroupId = ffGroup+1
						source.prefix = (source.prefix || "") + FFGroupUnicodes[ffGroup].char
						readIndex += 1
					}
				}
				
				source.playerName = source.sourceId.substring(readIndex)
									
				this.checkDatacenter()
				this._datacenter = addServerToSource(source, this._datacenter)
			}			
			
			return source
		}
		
		checkDatacenter(){
			const datacenterName = this._config.get("behaviour.datacenter",null)
			if(datacenterName !== null){
				if( this._datacenter && this._datacenter.label === datacenterName ){
					return
				}
				this._datacenter = Gobchat.DatacenterHelper.tryAndGetDatacenterByName(datacenterName)
			}
		}
				
	}		
	Gobchat.MessageParser = MessageParser
	
	//public function
	//playerName - a string. 	Contains first- and lastname, and sometimes also server name (which is joined to the lastname): Firstname Lastname or Firstname LastnameServername
	//datacenter - an object. 	Will be used to find the name of the server. If null, tries to find the Datacenter. See Gobchat.Datacenter.
	//returns - an object. 		Containing 'datacenter' and in case playerName is a combination of player- and server-name, the object also contains 'playerName' and 'serverName', otherwise not.
	//							'found' is a boolean and indicates if the function was successfull
	function tryAndSeparatePlayerFromServer(playerName, datacenter){ //TODO rewrite description
		const result = {datacenter: datacenter, playerName:null, serverName:null, found:false}
		
		if(playerName === null)
			return result
		
		const names = playerName.split(' ')
		
		if(names.length !== 2) //player have always first and last name, unfortunately, the server is joined to the last name
			return result
			
		//returns the last part of a given string, which starts with an uppercase letter and is shorter than the given string
		function getPossibleServerName(str){
			for(let i = str.length-1;1<=i;--i){
				const c = str.charAt(i)
				if(c === c.toUpperCase()){
					return str.substring(i,str.length)
				}
			}
			return null
		}
		
		const lastName = names[names.length-1]
		const serverName = getPossibleServerName(lastName)
		
		if(serverName === null) //doesn't have anything that may be a server
			return result
			
		if( !result.datacenter ){
			result.datacenter = Gobchat.DatacenterHelper.tryAndGetDatacenterByServerName(serverName)
		}
		
		if( result.datacenter === null ) // wasn't a server or we just failed to find it
			return result
			
		if( _.includes(result.datacenter.servers, serverName) ){
			result.playerName = playerName.substring(0, playerName.length - serverName.length)
			result.serverName = serverName
			result.found = true
		}
		
		return result 
	}
	
	function addServerToSource(messageSource, datacenter){
		if( messageSource.playerName === null )
			return
		
		const result = tryAndSeparatePlayerFromServer(messageSource.playerName, datacenter)
		
		if(result.found){
			messageSource.playerName = result.playerName
			messageSource.serverName = result.serverName			
		}
		
		return result.datacenter
	}
	
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
	
	function searchMessageSegmentsForEmoteInSay(message){
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
	
	function processMessageSegmentMention(message, mentions){
		const ignoreFunction = (_) => {return false}
		const parseFunction = (marker,type,txt) => {
			const matches = findAllWordMatches(mentions, txt.toLowerCase())
			if( matches.length === 0) return			
			marker.newMark(type,0,0)			
			for(let match of matches){
				marker.mark.end = match[0]
				marker.newMark(MessageSegmentEnum.MENTION, match[0], match[1])
				marker.newMark(type, match[1], match[1])
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
			parseFunction(marker, segmentType, segmentText)
			
			marker.finish()
			const newSegments = marker.marks
									.filter((marker)=>{return marker.end-marker.start>0})
									.map((marker)=>{return new MessageSegment(marker.type, segmentText.substring(marker.start, marker.end))})
			
			if(newSegments.length > 0){
				messageSegments.splice.apply(messageSegments, [i,1].concat(newSegments)) //deletes the old segment and adds the new segments at it's position
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
	
	Gobchat.MessageParserHelper = Object.freeze({
		tryAndSeparatePlayerFromServer: tryAndSeparatePlayerFromServer
	})
	
	return Gobchat	
}(Gobchat || {}));
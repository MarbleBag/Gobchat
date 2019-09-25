'use strict'

const CHAT_TYPE_SAY 			= 0x000a
const CHAT_TYPE_EMOTE 			= 0x001c
const CHAT_TYPE_YELL 			= 0x001e
const CHAT_TYPE_TELL_SEND 		= 0x000c
const CHAT_TYPE_TELL_RECIEVE 	= 0x000d
const CHAT_TYPE_PARTY 			= 0x000e
const CHAT_TYPE_GUILD 			= 0x0018
const CHAT_TYPE_ALLIANCE 		= 0x000f

const CHAT_TYPE_NPC_TALK 		= 0x0044
const CHAT_TYPE_ANIMATED_EMOTE 	= 0x001d
const CHAT_TYPE_PARTYFINDER 	= 0x0048
const CHAT_TYPE_ECHO			= 0x0038

const CHAT_TYPE_TELEPORT 		= 0x001f
const CHAT_TYPE_LOCATION 		= 0x0039

const CHAT_TYPE_WORLD_LINKSHELL_1 = 0x0025
const CHAT_TYPE_WORLD_LINKSHELL_2 = 0x0065
const CHAT_TYPE_WORLD_LINKSHELL_3 = 0x0066
const CHAT_TYPE_WORLD_LINKSHELL_4 = 0x0067
const CHAT_TYPE_WORLD_LINKSHELL_5 = 0x0068
const CHAT_TYPE_WORLD_LINKSHELL_6 = 0x0069
const CHAT_TYPE_WORLD_LINKSHELL_7 = 0x006A
const CHAT_TYPE_WORLD_LINKSHELL_8 = 0x006B

const CHAT_TYPE_LINKSHELL_1 = 0x0010
const CHAT_TYPE_LINKSHELL_2 = 0x0011
const CHAT_TYPE_LINKSHELL_3 = 0x0012
const CHAT_TYPE_LINKSHELL_4 = 0x0013
const CHAT_TYPE_LINKSHELL_5 = 0x0014
const CHAT_TYPE_LINKSHELL_6 = 0x0015
const CHAT_TYPE_LINKSHELL_7 = 0x0016
const CHAT_TYPE_LINKSHELL_8 = 0x0017

const CHAT_SEGMENT_TYPE_NONE 	= 0
const CHAT_SEGMENT_TYPE_SAY 	= 1
const CHAT_SEGMENT_TYPE_EMOTE 	= 2
const CHAT_SEGMENT_TYPE_OOC 	= 3
const CHAT_SEGMENT_TYPE_MENTION	= 4

let CHAT_SCROLL_TO_BOTTOM = true

const ChatBoxConfig = {

}

class MessageObject {
    constructor(timestamp, source, channel, segments) {
        this.timestamp = timestamp
        this.source = source
        this.channel = channel
        this.segments = segments
    }
}

class MessageSegment {
    constructor(type, message) {
        this.type = type
        this.message = message
    }
}

class ChatManager {
    constructor(sChatId) {
        this.bScrollToBottom = true
		this.sChatId = '#' + sChatId //jquery

        this.mentions = []
		this.playerNameMentions = []
		
        this.channels = {
            roleplay: [
                CHAT_TYPE_SAY, CHAT_TYPE_EMOTE, CHAT_TYPE_YELL, CHAT_TYPE_PARTY, CHAT_TYPE_GUILD, CHAT_TYPE_ALLIANCE
            ],
            ignore: [
                CHAT_TYPE_PARTYFINDER, CHAT_TYPE_TELEPORT, CHAT_TYPE_LOCATION
            ],
			mention: [
				CHAT_TYPE_SAY, CHAT_TYPE_EMOTE, CHAT_TYPE_YELL, CHAT_TYPE_PARTY, CHAT_TYPE_GUILD, CHAT_TYPE_ALLIANCE,
				CHAT_TYPE_LINKSHELL_1, CHAT_TYPE_LINKSHELL_2, CHAT_TYPE_LINKSHELL_3, CHAT_TYPE_LINKSHELL_4, CHAT_TYPE_LINKSHELL_5, CHAT_TYPE_LINKSHELL_6, CHAT_TYPE_LINKSHELL_7, CHAT_TYPE_LINKSHELL_8,
				CHAT_TYPE_WORLD_LINKSHELL_1, CHAT_TYPE_WORLD_LINKSHELL_2, CHAT_TYPE_WORLD_LINKSHELL_3, CHAT_TYPE_WORLD_LINKSHELL_4, CHAT_TYPE_WORLD_LINKSHELL_5, CHAT_TYPE_WORLD_LINKSHELL_6, CHAT_TYPE_WORLD_LINKSHELL_7, CHAT_TYPE_WORLD_LINKSHELL_8
			]
        }
    }

    init() {
        const chatbox = this
        $(chatbox.sChatId).on('scroll', function () {
            let closeToBottom = ($(this).scrollTop() + $(this).innerHeight() + 20 >= $(this)[0].scrollHeight) // +20px for 'being very close'
            chatbox.bScrollToBottom = closeToBottom
        })
		
		document.addEventListener("ChatMessageEvent", (e) => { this.onChatMessageEvent(e) })
		//document.addEventListener("PlayerNameEvent",(e) => { this.onPlayerNameEvent(e) })
		document.addEventListener("MentionsEvent", (e) => { this.onMentionEvent(e) })
		
		sendMessageToPlugin({event:"RequestMentions"})		
    }
	
	isMentionChannel(nChannel){
		return _.indexOf(this.channels.mention,nChannel) != -1
	}

    isRoleplayChannel(nChannel) {
		return _.indexOf(this.channels.roleplay,nChannel) != -1
    }

    isIgnoredChannel(nChannel) {
		//let val = _.indexOf(this.channels.ignore,nChannel)
		//console.log("Ignore channel [" + nChannel + "]: " + val)
		return _.indexOf(this.channels.ignore,nChannel) != -1
    }

    addChatLine(messageObj) {
        function getMessageBlockCssClass(msgObj) {
            switch (msgObj.channel) {
                case CHAT_TYPE_SAY:             return "message-body-say"
                case CHAT_TYPE_EMOTE:           return "message-body-emote"
                case CHAT_TYPE_TELL_SEND:       return "message-body-tells"
                case CHAT_TYPE_TELL_RECIEVE:    return "message-body-tellr"
                case CHAT_TYPE_GUILD:           return "message-body-guild"
                case CHAT_TYPE_YELL:            return "message-body-yell"
                case CHAT_TYPE_PARTY:           return "message-body-party"
                case CHAT_TYPE_ALLIANCE:        return "message-body-alliance"
                default:                        return null
            }
        }

        function buildSender(blockSpan,msgObj) {
            let senderSpan
            switch (msgObj.channel) {
                case CHAT_TYPE_TELL_RECIEVE:
                    senderSpan = document.createElement("span")
                    senderSpan.innerHTML = msgObj.source + " >> "
                    break;
                case CHAT_TYPE_TELL_SEND:
                    senderSpan = document.createElement("span")
                    senderSpan.innerHTML = ">> " + msgObj.source + ": "
                    break;
                case CHAT_TYPE_EMOTE:
                    senderSpan = document.createElement("span")
                    senderSpan.innerHTML = msgObj.source + " "
                    break;
                case CHAT_TYPE_ECHO:
                    senderSpan = document.createElement("span")
                    senderSpan.innerHTML = "Echo: "
                    break;
                case CHAT_TYPE_ANIMATED_EMOTE:
                    //source is set, but the animation message already contains the source name
                    break;
                default:
                    if (msgObj.source) {
                        senderSpan = document.createElement("span")
                        senderSpan.innerHTML = msgObj.source + ": "
                    }
            }

            if (senderSpan) blockSpan.appendChild(senderSpan)   
        }

        function buildMessage(blockSpan,msgObj) {
			function getSegmentClass(type){
				switch(type){
					case CHAT_SEGMENT_TYPE_SAY: 	return "message-segment-say"
					case CHAT_SEGMENT_TYPE_EMOTE: 	return "message-segment-emote"
					case CHAT_SEGMENT_TYPE_OOC: 	return "message-segment-ooc"
					case CHAT_SEGMENT_TYPE_MENTION: return "message-segment-mention"
					default:						return null
				}
			}
			
            msgObj.segments.forEach((segment) => {
				const segmentSpan = document.createElement("span")
				segmentSpan.innerHTML = segment.message
				const segmentClass = getSegmentClass(segment.type)
				if(segmentClass) segmentSpan.classList.add(segmentClass)
				blockSpan.appendChild(segmentSpan)	
			})
        }

        const messageLine = document.createElement("div")
        messageLine.classList.add("message-body-default")

        const timeSpan = document.createElement("span")
        timeSpan.innerHTML = "[" + messageObj.timestamp + "] "
        messageLine.appendChild(timeSpan)

        const blockSpan = document.createElement("span")
        const blockClass = getMessageBlockCssClass(messageObj)
        if (blockClass) blockSpan.classList.add(blockClass)
        messageLine.appendChild(blockSpan)

        buildSender(blockSpan,messageObj)
        buildMessage(blockSpan,messageObj)

		$(this.sChatId).append(messageLine)
        this.scrollToBottomIfNeeded()
    }
	
	onMentionEvent(mentionEvent){
		let mentions = mentionEvent.detail.mentions
		if( mentions ){						
			this.mentions = mentions.map((e)=>{return e.toLowerCase().trim()}).filter((e)=>{return e.length>0})
		}else{
			this.mentions = []
		}
	}
	
	onPlayerNameEvent(playerEvent){
		let playerName = playerEvent.detail.playername
		if( playerName == undefined || playerName == null ) return;
		playerName = playerName.toLowerCase()
		
		const result = new Set([playerName])
		
		const playerNames = playerName.split(" ")
		playerNames.forEach((e)=>{result.add(e)})
		playerNames.forEach((e)=>{e.split("'").forEach((s)=>{result.add(s)})})
		playerNames.forEach((e)=>{result.add(e.replace(/'/g,""))})
						
		//TODO for now
		this.playerNameMentions = Array.from(result)
	}

    onChatMessageEvent(messageEvent) {
        if (this.isIgnoredChannel(messageEvent.detail.type))
            return

        const messageObject = this.buildMessageObject(messageEvent.detail)

        if (this.isRoleplayChannel(messageObject.channel)) {
            this.parseRoleplayFormat(messageObject)
        }

		if( this.isMentionChannel(messageObject.channel) ){
			 this.parseMentions(messageObject)
		}

        this.addChatLine(messageObject)
    }

    buildMessageObject(messageEvent) {
        const msgObj = new MessageObject(
            messageEvent.timestamp,
            messageEvent.source,
            messageEvent.type,
            [new MessageSegment(CHAT_SEGMENT_TYPE_NONE, messageEvent.message)]
        )
        return msgObj
    }
	
    parseMentions(msgObj) {
		const mentions = this.mentions
		
		if( mentions.length === 0 ) return
		
		const segments = msgObj.segments
		for(let i=0;i<segments.length;++i){
			const segment = segments[i]
			const message = segment.message
			const matches = this.findMentionMatches(mentions,message.toLowerCase())
			if( matches.length === 0) continue
			
			const newSegments = []
			
			function makeSegment(type,start,end){
				let newMessage = message.substring(start,end)
				let newSegment = new MessageSegment(type,newMessage)
				newSegments.push(newSegment)
			}
						
			let lastMatch = matches[0]
			if(lastMatch[0] > 0){
				makeSegment(segment.type,0,lastMatch[0])				
			}
			makeSegment(CHAT_SEGMENT_TYPE_MENTION,lastMatch[0],lastMatch[1])
			
			for(let n=1;n<matches.length;++n){
				const nextMatch = matches[n]
				makeSegment(segment.type,lastMatch[1],nextMatch[0])
				makeSegment(CHAT_SEGMENT_TYPE_MENTION,nextMatch[0],nextMatch[1])
				lastMatch = nextMatch			
			}
			
			if(lastMatch[1] < message.length){
				makeSegment(segment.type,lastMatch[1],message.length)
			}
			
			//segments.splice(i,1,...newSegments) //not available	
			segments.splice.apply(segments,[i,1].concat(newSegments))
		}		
    }
	
	findMentionMatches(mentions,message){		
		/*function isBoundary(index){
			if(index < 0 || message.length <= index) return true //start & end		
			const c = message.charAt(index)
			switch(c){
				case ' ':
				case "'":
				case '"':
				case '*':
				case '.':
				case ',':
				case '?':
				case '!':
				case ':':
				case '-':
				case '_':
				case '+':
					return true;
				default:
					return false;
			}
		}*/
		
		function isBoundary(index){ 
			if(index < 0 || message.length <= index) return true //start & end		
			const c = message.charAt(index)
			const isLetter = c.toLowerCase() != c.toUpperCase() //works for a lot of character, but probably not for letters who don't have a diffrent lower and upper case :(
			return !isLetter //as long it's not a letter, it's okay!
		}
		
		function searchByIndexOf(result){
			let startIndex,endIndex, index
			for(let mention of mentions){
				const length = mention.length
				startIndex = 0
				while((index = message.indexOf(mention,startIndex)) > -1){
					endIndex = index+length
					if(isBoundary2(index-1) && isBoundary2(endIndex)){
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

    parseRoleplayFormat(messageObject) {
        function buildMarker(type, start, end) {
            return {
                type: type,
                start: start,
                end: end
            }
        }

		const newSegments = []
		
        for (let segment of messageObject.segments) {	
			const markers = []		
			let activeMark = buildMarker(CHAT_SEGMENT_TYPE_NONE, 0, 0)	
			
            const message = segment.message
            const messageLength = message.length
			
            for (let i = 0; i < messageLength; ++i) {
                let c = message.charAt(i)
				if( c == '"' ){						
					if( activeMark.type == CHAT_SEGMENT_TYPE_SAY ){
						activeMark.end = i+1
						markers.push(activeMark)
						activeMark = buildMarker(CHAT_SEGMENT_TYPE_NONE, i+1, i+1)
					}else{
						activeMark.end = i
						markers.push(activeMark)
						activeMark = buildMarker(CHAT_SEGMENT_TYPE_SAY, i, i)
					}
				}else if(c == '*'){
					if( activeMark.type == CHAT_SEGMENT_TYPE_EMOTE ){
						activeMark.end = i+1
						markers.push(activeMark)
						activeMark = buildMarker(CHAT_SEGMENT_TYPE_NONE, i+1, i+1)
					}else{
						activeMark.end = i
						markers.push(activeMark)
						activeMark = buildMarker(CHAT_SEGMENT_TYPE_EMOTE, i, i)
					}
				}
            }
			
			activeMark.end = messageLength
			markers.push(activeMark)
			
			markers	.filter((marker)=>{return marker.end - marker.start > 0})
					.map((marker)=>{return new MessageSegment(marker.type,message.substring(marker.start,marker.end))})
					.forEach((segment)=>{newSegments.push(segment)})
        }
		
		messageObject.segments = newSegments
    }

    scrollToBottomIfNeeded() {
        if (this.bScrollToBottom) {
			 const chatbox = this
            $(chatbox.sChatId).animate({
                scrollTop: $(chatbox.sChatId)[0].scrollHeight - $(chatbox.sChatId)[0].clientHeight
            }, 500);
        }
    }
}


/*
$().scrollTop()//how much has been scrolled
$().innerHeight()// inner height of the element
DOMElement.scrollHeight//height of the content of the element
*/

//jQuery(function($){})
//same as $(document).ready(function(){}) with $ as local variable (protects for overwrite)

function parseChatMessageOnEvent(messageEvent) {
    const msgObj = processChatMessageEvent(messageEvent.detail)
    let html = []

    let messageLine = document.createElement("div")
    messageLine.classList.add("message-body-default")

    const timeSpan = document.createElement("span")
    timeSpan.innerHTML = "[" + msgObj.timestamp + "] "
    messageLine.appendChild(timeSpan)

    const blockSpan = document.createElement("span")
    const blockClass = getMessageBlockCssClass(msgObj)
    if (blockClass) blockSpan.classList.add(blockClass)
    messageLine.appendChild(blockSpan)

    let senderSpan
    switch (msgObj.type) {
        case CHAT_TYPE_TELL_RECIEVE:
            senderSpan = document.createElement("span")
            senderSpan.innerHTML = msgObj.source + " >> "
            break;
        case CHAT_TYPE_TELL_SEND:
            senderSpan = document.createElement("span")
            senderSpan.innerHTML = ">> " + msgObj.source + ": "
            break;
        case CHAT_TYPE_EMOTE:
            senderSpan = document.createElement("span")
            senderSpan.innerHTML = msgObj.source + " "
            break;
        case CHAT_TYPE_ECHO:
            senderSpan = document.createElement("span")
            senderSpan.innerHTML = "Echo: "
            break;
        case CHAT_TYPE_ANIMATED_EMOTE:
            //source is set, but the animation message already contains the source name
            break;
        default:
            if (msgObj.source) {
                senderSpan = document.createElement("span")
                senderSpan.innerHTML = msgObj.source + ": "
            }
    }

    if (senderSpan) blockSpan.appendChild(senderSpan)
    buildMessageBody(blockSpan, msgObj)
    document.getElementById("chatcontent").appendChild(messageLine)

    if (CHAT_SCROLL_TO_BOTTOM) {
        $("#chatcontent").animate({
            scrollTop: $('#chatcontent')[0].scrollHeight - $('#chatcontent')[0].clientHeight
        }, 500);
    }
}



function buildMessageBody(blockSpan, msgObj) {
    let whiteSpaceAvailable = true
    let whitespaced = false
    msgObj.message.forEach(function (segment) {
        if (segment.type == 'original') return

        whitespaced = whiteSpaceAvailable || /\s/.test(segment.msg.charAt(0)) //current segment		
        whiteSpaceAvailable = /\s/.test(segment.msg.charAt(segment.msg.length - 1)) //for next segment

        let segmentSpan = document.createElement("span")

        //if(!whitespaced){
        //let whitespaceSpan = document.createElement("span")
        //whitespaceSpan.innerHTML = "&#160;"
        //segmentSpan.appendChild(whitespaceSpan)
        //console.log("Add whitespace!")
        //}

        segmentSpan.innerHTML = whitespaced ? "" : " "

        switch (segment.type) {
            case CHAT_SEGMENT_TYPE_SAY:
                segmentSpan.classList.add("message-segment-say")
                segmentSpan.innerHTML += '"' + segment.msg.trim() + '"'
                whiteSpaceAvailable = false
                break
            case CHAT_SEGMENT_TYPE_EMOTE:
                segmentSpan.classList.add("message-segment-emote")
                segmentSpan.innerHTML += segment.msg
                break
            case CHAT_SEGMENT_TYPE_OOC:
                segmentSpan.classList.add("message-segment-ooc")
                segmentSpan.innerHTML += '(( ' + segment.msg.trim() + ' ))'
                whiteSpaceAvailable = false
                break
            default:
                segmentSpan.classList.add("message-segment-default")
                segmentSpan.innerHTML += segment.msg
        }

        blockSpan.appendChild(segmentSpan)
    })
}

function getMessageBlockCssClass(messageObj) {
    if (messageObj.type == CHAT_TYPE_SAY) {
        return "message-body-say"
    } else if (messageObj.type == CHAT_TYPE_EMOTE) {
        return "message-body-emote"
    } else if (messageObj.type == CHAT_TYPE_TELL_SEND) {
        return "message-body-tells"
    } else if (messageObj.type == CHAT_TYPE_TELL_RECIEVE) {
        return "message-body-tellr"
    } else if (messageObj.type == CHAT_TYPE_GUILD) {
        return "message-body-guild"
    } else if (messageObj.type == CHAT_TYPE_YELL) {
        return "message-body-yell"
    } else if (messageObj.type == CHAT_TYPE_PARTY) {
        return "message-body-party"
    } else if (messageObj.type == CHAT_TYPE_ALLIANCE) {
        return "message-body-alliance"
    } else {
        return null
    }
}

function blabla(messageEvent) {
    const messageObj = {
        message: { format: CHAT_SEGMENT_TYPE_NONE, msg: messageEvent.message },
        type: messageEvent.type,
        timestamp: messageEvent.timestamp,
        source: messageEvent.source
    }
    return messageObj
}

function processChatMessageEvent(messageEvent) {

    function getDefaultSegmentType(msgType) {
        switch (msgType) {
            case CHAT_TYPE_SAY: return CHAT_SEGMENT_TYPE_SAY
            case CHAT_TYPE_EMOTE: return CHAT_SEGMENT_TYPE_EMOTE
            default: return CHAT_SEGMENT_TYPE_NONE
        }
    }

    function buildSegment(type, start, end) {
        return {
            type: type,
            msgMark: { start: start, end: end }
        }
    }

    const message = messageEvent.message
    const defaultSegmentType = getDefaultSegmentType(messageEvent.type)

    let mark = 0
    let expectedEncounter = null

    let segment = buildSegment(defaultSegmentType, 0, 0)

    let messageSegments = []
    for (let i = 0; i < message.length; ++i) {
        let c = message.charAt(i)
        if (c == '"') {
            segment.msgMark.end = i
            messageSegments.push(segment)

            mark = i + 1

            if (expectedEncounter == null) {
                expectedEncounter = '"'
                segment = buildSegment(CHAT_SEGMENT_TYPE_SAY, mark, mark)
            } else if (expectedEncounter != null) {
                expectedEncounter = null
                segment = buildSegment(defaultSegmentType, mark, mark)
            }
        } else if (c == '*') {
            segment.msgMark.end = i
            messageSegments.push(segment)

            mark = i + 1

            if (expectedEncounter == null) {
                expectedEncounter = '"'
                segment = buildSegment(CHAT_SEGMENT_TYPE_EMOTE, mark, mark)
            } else if (expectedEncounter != null) {
                expectedEncounter = null
                segment = buildSegment(defaultSegmentType, mark, mark)
            }
        } else if (c == '(') {
            if (!(i + 1 < message.length && message.charAt(i + 1) == '(')) {
                continue // '((' is the only valid marker	
            }
            continue //not working

            segment.msgMark.end = i
            messageSegments.push(segment)

            mark = i + 2

            segment = buildSegment(CHAT_SEGMENT_TYPE_OOC, mark, mark)
        } else if (c == ')') {
            if (!(i + 1 < message.length && message.charAt(i + 1) == ')')) {
                continue // '))' is the only valid marker	
            }
            continue //not working

            segment.msgMark.end = i
            messageSegments.push(segment)

            mark = i + 2

            expectedEncounter = null
            segment = buildSegment(defaultSegmentType, mark, mark)
        }
    }

    //remaining segment
    segment.msgMark.end = message.length
    messageSegments.push(segment)

    messageSegments = messageSegments
        .filter((segment) => { return segment.msgMark.start < segment.msgMark.end })
        .map((segment) => {
            segment.msg = message.substring(segment.msgMark.start, segment.msgMark.end)
            delete segment.msgMark
            return segment
        })
        .filter((segment) => { return segment.msg.trim().length > 0 })

    messageSegments.push({ type: 'original', msg: message })

    const messageObj = {
        message: messageSegments,
        type: messageEvent.type,
        timestamp: messageEvent.timestamp,
        source: messageEvent.source
    }

    return messageObj
}


'use strict'

var Gobchat = (function(Gobchat){

	function getAttributes(source,fallback){
		function getAttributeValue(key,source,fallback){
			if( !fallback ) return key in source ? source[key] : null
			if(key in source){
				var val = source[key]
				if( val == null && key in fallback ){
					val = fallback[key]
				}
				return val
			}else if(key in fallback){
				return fallback[key]
			}
			return null
		}
		
		function buildAttributeList(keys,source,fallback){
			const result = []
			for(let key of keys){
				const data = getAttributeValue(key,source,fallback)		
				if( data != null ){
					result.push([key,data])
				}
			}
			return result				
		}
		
		return buildAttributeList(Object.keys(source),source,fallback)
	}
	
	function buildStyle(styleConfig){	
	
		function buildCssClass(className, classAttributes, classAttributesFallback){			
			const style = `
					.${className}{
						${getAttributes(classAttributes,classAttributesFallback).map((attribute)=>{return `${attribute[0]}: ${attribute[1]};`}).join("")}
					}
				`
			return style
		}
		
		const gobchatStyle = []
		
		{
			const style = buildCssClass(`chatbox-gen`,styleConfig.chatbox)
			gobchatStyle.push(style)
		}
			
		const channelStyle = styleConfig.channel
		for(let channelName in channelStyle){
			const channelData = channelStyle[channelName]
			const style = buildCssClass(`message-body-${channelName}`,channelData)
			gobchatStyle.push(style)
		}
		
		const segmentStyle = styleConfig.segment
		for(let segmentName in segmentStyle){			
			const segmentData = segmentStyle[segmentName]
			const fallback = segmentName in channelStyle ? channelStyle[segmentName] : undefined
			const style = buildCssClass(`message-segment-${segmentName}`,segmentData,fallback)
			gobchatStyle.push(style)
		}
		
		//TODO
		
		return gobchatStyle.join("\n")
	}
	
	function setStyle(styleId,styleCssText){		
		let styleElement = document.getElementById(styleId)
		if(!styleElement){
			styleElement = document.createElement("style")
			styleElement.id = styleId
			document.head.appendChild(styleElement)
			//document.getElementsByTagName('head')[0].appendChild(styleElement)
		}
		
		styleElement.type = 'text/css';
		styleElement.innerHTML = styleCssText	
	}
	
	Gobchat.StyleBuilder = {
		updateStyle: function(styleConfig,styleId){
			const css = buildStyle(styleConfig)
			setStyle(styleId,css)
		}
	}
		
	return Gobchat	
}(Gobchat || {}));
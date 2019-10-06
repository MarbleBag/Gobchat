'use strict';

var Gobchat = (function(Gobchat){
	
		Gobchat.sendMessageToPlugin = function(obj){
			if(!obj) return
			let sJson = JSON.stringify(obj)
			let overlayName = OverlayPluginApi.overlayName
			OverlayPluginApi.overlayMessage(overlayName, sJson)
		}
		
		Gobchat.isString = function(value) {
			return typeof value === 'string' || value instanceof String
		}
		
		Gobchat.isNumber = function(value) {
			return typeof value === 'number' && isFinite(value)
		}
		
		Gobchat.isFunction = function(value) {
			return typeof value === 'function';
		}
		
		Gobchat.isArray = function(value) {
			Array.isArray(value)
			//return value && typeof value === 'object' && value.constructor === Array
		}
		
		Gobchat.isObject = function(value) {
			return value && typeof value === 'object' && value.constructor === Object;
		}
	
		return Gobchat	
}(Gobchat || {}));


// Sends the given obj as json back to the overlay plugin which created this instance


/*
function findAllMatches(str,searchStr){
	if(!str) return null
	if(!searchStr) return null
	
	const searchStrLength = searchStr.length
	let startIndex = 0, index
	let result = []
	
    while ((index = str.indexOf(searchStr, startIndex)) > -1) {
        result.push(index);
        startIndex = index + searchStrLength;
    }
	
	return result
}
*/
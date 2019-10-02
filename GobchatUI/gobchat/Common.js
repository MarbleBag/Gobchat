'use strict';

var Gobchat = (function(Gobchat){
	
		Gobchat.sendMessageToPlugin = function(obj){
			if(!obj) return
			let sJson = JSON.stringify(obj)
			let overlayName = OverlayPluginApi.overlayName
			OverlayPluginApi.overlayMessage(overlayName, sJson)
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
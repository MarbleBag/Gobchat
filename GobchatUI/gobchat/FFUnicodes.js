'use strict'

var Gobchat = (function(Gobchat){

	Gobchat.FFGroupEnum2 = Object.freeze({
		GROUP_1: "★", //U+2605 //CC: 9733 //&#9733;
		GROUP_2: "●", //U+25CF	//CC: 9679
		GROUP_3: "▲", //U+25B2	//CC: 9650
		GROUP_4: "♦", //U+2666 	//CC: 9830
		GROUP_5: "♥", //U+2665 	//CC: 9829
		GROUP_6: "♠", //U+2660 	//CC: 9824
		GROUP_7: "♣", //U+2663 	//CC: 9827
	})
	
	Gobchat.FFGroupEnum = Object.freeze({
		GROUP_1: {char:"★",unicode:"U+2605",value:0x2605},
		GROUP_2: {char:"●",unicode:"U+25CF",value:0x25CF},
		GROUP_3: {char:"▲",unicode:"U+25B2",value:0x25B2},
		GROUP_4: {char:"♦",unicode:"U+2666",value:0x2666},
		GROUP_5: {char:"♥",unicode:"U+2665",value:0x2665},
		GROUP_6: {char:"♠",unicode:"U+2660",value:0x2660},
		GROUP_7: {char:"♣",unicode:"U+2663",value:0x2663},
	})
	
	Gobchat.FFUnicode = Object.freeze({
		PARTY_2: {char:"",unicode:"U+E091",value:0xE091},
	})
		
	
	Gobchat.FFPlayerGroup = Object.freeze([
		Gobchat.FFGroupEnum.GROUP_1,Gobchat.FFGroupEnum.GROUP_2,Gobchat.FFGroupEnum.GROUP_3,Gobchat.FFGroupEnum.GROUP_4,Gobchat.FFGroupEnum.GROUP_5,Gobchat.FFGroupEnum.GROUP_6,Gobchat.FFGroupEnum.GROUP_7,Gobchat.FFGroupEnum.GROUP_8
	])
	
	return Gobchat
	
}(Gobchat || {}));
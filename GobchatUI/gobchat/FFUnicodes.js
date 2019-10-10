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
		
	Gobchat.FFUnicode = Object.freeze({	
		GROUP_1: 	{char: '\u2605', 	value: 0x2605},
		GROUP_2: 	{char: '\u25CF', 	value: 0x25CF},
		GROUP_3: 	{char: '\u25B2',	value: 0x25B2},
		GROUP_4: 	{char: '\u2666', 	value: 0x2666},
		GROUP_5: 	{char: '\u2665', 	value: 0x2665},
		GROUP_6: 	{char: '\u2660', 	value: 0x2660},
		GROUP_7: 	{char: '\u2663',	value: 0x2663},
		PARTY_1: 	{char: '\uE090', 	value: 0xE090},
		PARTY_2: 	{char: '\uE091', 	value: 0xE091},
		PARTY_3: 	{char: '\uE092', 	value: 0xE092},
		PARTY_4: 	{char: '\uE093', 	value: 0xE093},
		PARTY_5: 	{char: '\uE094', 	value: 0xE094},
		PARTY_6: 	{char: '\uE095', 	value: 0xE095},
		PARTY_7: 	{char: '\uE096', 	value: 0xE096},
		PARTY_8: 	{char: '\uE097', 	value: 0xE097},
		RAID_A: 	{char: '\uE071', 	value: 0xE073},
		RAID_B: 	{char: '\uE072', 	value: 0xE072},
		RAID_C: 	{char: '\uE073', 	value: 0xE073},
	})
	
	//lazy
	Object.keys(Gobchat.FFUnicode).forEach( (e) => {
			const tuple = Gobchat.FFUnicode[e]
			tuple.value = tuple.char.codePointAt(0)			
		})
		
	return Gobchat
	
}(Gobchat || {}));
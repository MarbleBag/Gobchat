'use strict';

(function(Gobchat){
	
	jQuery(function ($) { //initialize navigation bar
		window.gobconfig = new Gobchat.GobchatConfig()
		window.gobconfig.loadFromLocalStore()
	
		initializeNavigationBar()
		initializeGeneralConfig()
		initializeChannelConfig()
		initializeRoleplayConfig()
		initializeGroupsConfig()
	})
	
	function initializeNavigationBar(){
		const navAttribute = "data-nav-target"
		const navigationBarId = "mainNavigationBar"
	
		function hideNavigationContent(navigationId){
			$(`#${navigationId} > a`).each(function(){
				const target = $(this).attr(navAttribute)
				if(target){
					$(`#${target}`).hide()
				}
			})
		}
		
		function showActiveNavigationContent(navigationId){
			$(`#${navigationId} > .active`).each(function(){
				const target = $(this).attr(navAttribute)
				if(target)
					$(`#${target}`).show()				
			})
		}
		
		function hasNavigationAttribute(element){
			const target = $(element).attr(navAttribute)
			return target !== null && target !== undefined
		}
		
		hideNavigationContent(navigationBarId)
		showActiveNavigationContent(navigationBarId)
		
		$(`#${navigationBarId}`).on("click",function(event){
			if(!hasNavigationAttribute(event.target)) return				
			hideNavigationContent(navigationBarId)				
			$(`#${navigationBarId} > .active`).removeClass("active")
			$(event.target).addClass("active")
			showActiveNavigationContent(navigationBarId)		
		})
	}
	
	function initializeGeneralConfig(){
		$("#dropdown_language").val(gobconfig.get("behaviour.language"))
		$("#dropdown_language").on("change",function(event){
			const newLanguage = event.target.value || "en"
			gobconfig.set("behaviour.language",newLanguage)
		})
		
		
		$("#txt_mentions").val(gobconfig.get("behaviour.mentions").join(", "))
		$("#txt_mentions").on("change",function(event){
			let words = (event.target.value || "").split(",")
			words = words.filter(w=>w!==null&&w!==undefined).map(w=>w.toLowerCase().trim()).filter(w=>w.length>0)
			gobconfig.set("behaviour.mentions",words)
			event.target.value = words.join(", ")
		})			
		
		$("#input_autoemote").prop('checked',gobconfig.get("behaviour.autodetectEmoteInSay"))
		$("#input_autoemote").on("change",function(event){					
			gobconfig.set("behaviour.autodetectEmoteInSay",event.target.checked)
		})
		
		$("#general_font").val(gobconfig.get("style.channel.base.font-family"))
		$("#general_font").on("change",function(event){
			let newFont = event.target.value || "sans-serif"
			gobconfig.set("style.channel.base.font-family",newFont)
		})
		
		$("#general_font_reset").on("click",function(event){
			gobconfig.reset("style.channel.base.font-family")
			$("#general_font").val(gobconfig.get("style.channel.base.font-family"))
		})
	
		$("#general_font_size").val(gobconfig.get("style.channel.base.font-size"))
		$("#general_font_size").on("change",function(event){
			let newSize = event.target.value || "medium"
			if(Gobchat.isNumber(newSize)){
				const number = Math.round(newSize)
				newSize = number + "px"
			}
			gobconfig.set("style.channel.base.font-size",newSize)
		})		
	}
	
	function initializeChannelConfig(){
		const tblConfig = [
			{label:"General", 	channelEnum:null, 		styleId:"style.channel.base"},
			
			{label:"Say", 			channelEnum:"SAY", 			styleId:"style.channel.say"},
			{label:"Emote", 		channelEnum:"EMOTE", 		styleId:"style.channel.emote"},
			{label:"Yell", 			channelEnum:"YELL", 		styleId:"style.channel.yell"},
			{label:"Shout", 		channelEnum:"SHOUT", 		styleId:"style.channel.shout"},
			{label:"Party", 		channelEnum:"PARTY", 		styleId:"style.channel.party"},
			{label:"Guild", 		channelEnum:"GUILD", 		styleId:"style.channel.guild"},				
			{label:"Tell send",		channelEnum:"TELL_SEND", 	styleId:"style.channel.tellsend"},
			{label:"Tell recieve", 	channelEnum:"TELL_RECIEVE",	styleId:"style.channel.tellrecieve"},				
			{label:"Alliance", 		channelEnum:"ALLIANCE", 	styleId:"style.channel.alliance"},
			
			{label:"NPC speech",			channelEnum:"NPC_TALK", 		styleId:null},
			{label:"Animated emote", 		channelEnum:"ANIMATED_EMOTE", 	styleId:null},
			{label:"Echo", 					channelEnum:"ECHO", 			styleId:null},
			{label:"Random (Other Player)",	channelEnum:"RANDOM_OTHER", 	styleId:null},
			{label:"Random (You)", 			channelEnum:"RANDOM_SELF", 		styleId:null},
			{label:"Error Messages",		channelEnum:"ERROR", 			styleId:null},
			
			{label:"Linkshell 1", channelEnum:"LINKSHELL_1", 	styleId:"style.channel.linkshell-1"},
			{label:"Linkshell 2", channelEnum:"LINKSHELL_2", 	styleId:"style.channel.linkshell-2"},
			{label:"Linkshell 3", channelEnum:"LINKSHELL_3", 	styleId:"style.channel.linkshell-3"},
			{label:"Linkshell 4", channelEnum:"LINKSHELL_4", 	styleId:"style.channel.linkshell-4"},
			{label:"Linkshell 5", channelEnum:"LINKSHELL_5", 	styleId:"style.channel.linkshell-5"},
			{label:"Linkshell 6", channelEnum:"LINKSHELL_6", 	styleId:"style.channel.linkshell-6"},
			{label:"Linkshell 7", channelEnum:"LINKSHELL_7", 	styleId:"style.channel.linkshell-7"},
			{label:"Linkshell 8", channelEnum:"LINKSHELL_8", 	styleId:"style.channel.linkshell-8"},
			
				
			{label:"Cross-world Linkshell 1", channelEnum:"WORLD_LINKSHELL_1", 	styleId:"style.channel.worldlinkshell-1"},			
			{label:"Cross-world Linkshell 2", channelEnum:"WORLD_LINKSHELL_2", 	styleId:"style.channel.worldlinkshell-2"},	
			{label:"Cross-world Linkshell 3", channelEnum:"WORLD_LINKSHELL_3", 	styleId:"style.channel.worldlinkshell-3"},	
			{label:"Cross-world Linkshell 4", channelEnum:"WORLD_LINKSHELL_4", 	styleId:"style.channel.worldlinkshell-4"},
			{label:"Cross-world Linkshell 5", channelEnum:"WORLD_LINKSHELL_5", 	styleId:"style.channel.worldlinkshell-5"},
			{label:"Cross-world Linkshell 6", channelEnum:"WORLD_LINKSHELL_6", 	styleId:"style.channel.worldlinkshell-6"},	
			{label:"Cross-world Linkshell 7", channelEnum:"WORLD_LINKSHELL_7", 	styleId:"style.channel.worldlinkshell-7"},	
			{label:"Cross-world Linkshell 8", channelEnum:"WORLD_LINKSHELL_8", 	styleId:"style.channel.worldlinkshell-8"},	
		]
			
		function makeBehaviourCheck(entry,behaviour){
			if(entry.channelEnum===null || entry.channelEnum===undefined){
				return null
			}
			const channelEnum = Gobchat.ChannelEnum[entry.channelEnum]
			const configKey = `behaviour.channel.${behaviour}` 
			return buildCheckboxForArray(configKey, channelEnum)
		}
		
		function makeColorSelector(entry,type){
			if(entry.styleId===null || entry.styleId===undefined){
				return null
			}
			
			function setColorPalette(input){
				let colorPalette = tblConfig.map(e=>e.styleId).filter(e=>e!==null&&e!==undefined)
					.map(e=>e + ".color").map(e=>window.gobconfig.get(e)).filter(e=>e!==null&&e!==undefined)
				colorPalette = _.union(colorPalette)
				$(input).spectrum("option","palette",colorPalette)
			}
			
			const configKey = entry.styleId + "." + type
			return buildColorSelector(configKey,true,null)
		}
			
		const tbl = $("#tbl_channel_config > tbody")
		tblConfig.forEach((entry)=>{
				const tr = document.createElement("tr")
				tbl.append(tr)
				
				addToTableRow(tr,makeNameLabel(entry.label))
				addToTableRow(tr,makeBehaviourCheck(entry,"visible"))
				addToTableRow(tr,makeBehaviourCheck(entry,"mention"))
				addToTableRow(tr,makeBehaviourCheck(entry,"roleplay"))
				
				addToTableRow(tr,makeColorSelector(entry,"color"))
				addToTableRow(tr,makeColorSelector(entry,"background-color"))
			})
	}
	
	function initializeRoleplayConfig(){
		const tblConfig = [
			{label:"Say", 		styleId:"style.segment.say"},
			{label:"Emote", 	styleId:"style.segment.emote"},
			{label:"OoC", 		styleId:"style.segment.ooc"},
			{label:"Mention", 	styleId:"style.segment.mention"},
		]
		
		
		function makeBehaviourCheck(entry,behaviour){
			if(entry.channelEnum===null || entry.channelEnum===undefined){
				return null
			}
			const channelEnum = Gobchat.ChannelEnum[entry.channelEnum]
			const configKey = `behaviour.channel.${behaviour}` 
			return buildCheckboxForArray(configKey, channelEnum)
		}
		
		function makeColorSelector(entry,type){
			if(entry.styleId===null || entry.styleId===undefined){
				return null
			}
			const configKey = entry.styleId + "." + type
			return buildColorSelector(configKey,true,null)
		}
		
		const tbl = $("#tbl_segment_config > tbody")
		tblConfig.forEach((entry)=>{
				const tr = document.createElement("tr")
				tbl.append(tr)				
				addToTableRow(tr,makeNameLabel(entry.label))				
				addToTableRow(tr,makeColorSelector(entry,"color"))
			})
	}
	
	function initializeGroupsConfig(){
		const tblConfig = [
			{label:"Group 1 ('\u2605')", senderStyle:"style.sender.ffgroup-1",messageStyle:"style.channel.ffgroup-1"},
			{label:"Group 2 ('\u25CF')", senderStyle:"style.sender.ffgroup-2",messageStyle:"style.channel.ffgroup-2"},
			{label:"Group 3 ('\u25B2')", senderStyle:"style.sender.ffgroup-3",messageStyle:"style.channel.ffgroup-3"},
			{label:"Group 4 ('\u2666')", senderStyle:"style.sender.ffgroup-4",messageStyle:"style.channel.ffgroup-4"},
			{label:"Group 5 ('\u2665')", senderStyle:"style.sender.ffgroup-5",messageStyle:"style.channel.ffgroup-5"},
			{label:"Group 6 ('\u2660')", senderStyle:"style.sender.ffgroup-6",messageStyle:"style.channel.ffgroup-6"},
			{label:"Group 7 ('\u2663')", senderStyle:"style.sender.ffgroup-7",messageStyle:"style.channel.ffgroup-7"},
		]
				
		const tbl = $("#ffGroups")
		tblConfig.forEach((entry)=>{
				const groupElement = document.createElement("div") 
				tbl.append(groupElement)
			
				const headerElement = document.createElement("h4")
				headerElement.innerHTML = entry.label
				groupElement.appendChild(headerElement)
				
				const dataElement = document.createElement("div")
				groupElement.appendChild(dataElement)
				
				const dataTbl1 = document.createElement("table")
				const dataTbl1R1 = document.createElement("tr")
				const dataTbl1R2 = document.createElement("tr")
				dataElement.appendChild(dataTbl1)
				dataTbl1.appendChild(dataTbl1R1)
				dataTbl1.appendChild(dataTbl1R2)
				addToTableRow(dataTbl1R1,makeNameLabel("Sender Text"))
				addToTableRow(dataTbl1R1,buildColorSelector(entry.senderStyle + ".color",false,null))
				addToTableRow(dataTbl1R1,makeNameLabel("Sender Background"))
				addToTableRow(dataTbl1R1,buildColorSelector(entry.senderStyle + ".background-color",true,null))
				addToTableRow(dataTbl1R2,makeNameLabel("Message Background"))
				addToTableRow(dataTbl1R2,buildColorSelector(entry.messageStyle + ".background-color",true,null))
			})
		$( "#ffGroups" ).accordion({
				 heightStyle: "content",
				 header: "> div > h4",
			});	
		
	}
	
	//Helper functions
	
	function addToTableRow(rowElement, element){
		const td = document.createElement("td")
		if(element){					
			td.appendChild(element)					
		}
		rowElement.appendChild(td)
	}
	
	function makeNameLabel(name){
		if(name===null || name===undefined){
			return null
		}
		const lbl = document.createElement("label")
		lbl.innerHTML = name
		return lbl
	}
	
	//UI elements
	
	function buildCheckboxForArray(configKey,checkboxValue){
		const input = document.createElement("input")
		input.type = "checkbox"
		
		const data = window.gobconfig.get(configKey)
		if(data === null || data === undefined)
			input.disabled = true
		
		input.checked  = _.includes(data,checkboxValue)
		$(input).on("change",function(event){
				if(event.target.checked && !_.includes(data,checkboxValue)){
					data.push(checkboxValue)
				}else{
					_.remove(data,(i)=>{return i===checkboxValue})
				}
			})		
		
		return input
	}
	
	function buildColorSelector(configKey,hasAlpha,onBeforeShow){
		const div = document.createElement("div")
		const input = document.createElement("input")		
		div.appendChild(input)
		
		const data = window.gobconfig.get(configKey)
		
		input.type = "text"	
		$(input).spectrum({
			preferredFormat: "hex3",
			color: data,
			allowEmpty:true,
			showInput: true,
			showInitial: true,
			showAlpha: hasAlpha,
			showPalette: true,
			//palette: spectrumColorPalette,
			showSelectionPalette: true,
			selectionPalette: [],
			maxSelectionSize: 6,
			clickoutFiresChange: false,
			hide: function(color) {
					if(color !== null){
						gobconfig.set(configKey,color.toString())
					}else{
						gobconfig.set(configKey,null)
					}
				},
			beforeShow: function(color){
				if(onBeforeShow)onBeforeShow(input)
				return true
			}
		})
		
		const btn = document.createElement("button")
		btn.innerHTML = "&#x274C;"
		btn.title = "Reset to default"
		
		$(btn).on("click",function(event){
				window.gobconfig.reset(configKey)
				$(input).spectrum("set", window.gobconfig.get(configKey));
			})	
		
		div.appendChild(btn)
		
		return div
	}
	
}(Gobchat || {}));
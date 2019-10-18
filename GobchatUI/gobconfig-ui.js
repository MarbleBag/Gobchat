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
		
		{
			const dropdown = $("#dropdown_datacenter")
			dropdown.append(new Option("Automatic","auto"))
			$.each(Gobchat.Datacenters,function(idx,region){
				const separator = new Option(`\u2500\u2500\u2500 ${region.label} \u2500\u2500\u2500`)
				separator.disabled = true
				dropdown.append(separator)
				$.each(region.centers,function(idx,datacenter){
					dropdown.append(new Option(datacenter.label,datacenter.label))
				})
			})

			dropdown.val(gobconfig.get("behaviour.datacenter",null) || "auto")				
			dropdown.on("change",function(event){	
				const datacenter = event.target.value
				if(datacenter === "auto"){
					gobconfig.reset("behaviour.datacenter")
				}else{
					gobconfig.set("behaviour.datacenter",event.target.value)
				}
			})
		}
		
		$("#dropdown_showserver").on("change",function(event){					
			//TODO
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
			return buildColorSelector({configKey: configKey})
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
			return buildColorSelector({configKey: configKey})
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
		const configKeyGroupSorting = "userdata.group.sorting"
		const configKeyGroupData 	= "userdata.group.data"
		
		function updateGroupRenderer(){
			$( "#chatgroups" ).sortable( "refresh" )
			$( "#chatgroups" ).accordion( "refresh" )
			updateGroupNumbers()
		}
		
		function updateGroupNumbers(){
			$( "#chatgroups #groupcounter" ).each(function(idx){
				$(this).text(idx+1)
			})
		}
		
		function actionRemoveGroup(event){
			event.stopPropagation()			
			const groupId = $(this).data("groupId")			
			_.remove(window.gobconfig.get(configKeyGroupSorting), e => e === groupId)
			window.gobconfig.remove(configKeyGroupData + "." + groupId)	
			$(`#chatgroups #group-${groupId}`).remove()	
			updateGroupRenderer()
		}

		function buildGroupElement(groupId){
			const groupKey = configKeyGroupData + "." + groupId
			const groupData = window.gobconfig.get(groupKey)
			const isFFGroup = "ffgroup" in groupData
			
			const template = document.querySelector("#t_group_entry")
			const templateNode = document.importNode(template.content, true)
			
			const groupElement = $(templateNode).children("#groupelement")			
			groupElement.attr("id",`group-${groupId}`)
			groupElement.data("groupId",groupId)
			groupElement.find("#groupcounter").text($("#chatgroups > div").length + 1)
			groupElement.find("#groupname").text(groupData.name)
			groupElement.find("#groupdelete").data("groupId",groupId)
			
			if(isFFGroup){
				groupElement.find("#groupdelete").hide()
			}else{
				groupElement.find("#groupdelete").on("click", actionRemoveGroup)
			}
						
			let groupUi = groupElement.find("#groupui")
			groupUi = $("<tbody/>")
						.appendTo(
								$("<table/>")
									.css("width","100%")
									.appendTo(groupUi)
							)
			
			function buildRow(){
				const row = $("<tr/>").appendTo(groupUi)
				if(arguments.length){
					for(let element of arguments){
						$("<td/>").append(element).appendTo(row)
					}
				}
				
				return row
			}
			
			buildRow(
					buildLabel("Group name"),
					$(buildTextbox({configKey: groupKey + ".name", hasReset: isFFGroup}))
						.on("change",function(event){ groupElement.find("#groupname").text(window.gobconfig.get(groupKey + ".name")) })
				)
			
			buildRow(
					buildLabel("Active"),
					buildCheckboxForElement(groupKey + ".active")
				)
				
			buildRow(
					buildLabel("Sender Text"),
					buildColorSelector({configKey: groupKey + ".style.header.color", hasReset:isFFGroup}),
					buildLabel("Sender Background"),
					buildColorSelector({configKey: groupKey + ".style.header.background-color", hasReset:isFFGroup})
				)
				
			buildRow(
					buildLabel("Message Background"),
					buildColorSelector({configKey: groupKey + ".style.body.background-color", hasReset:isFFGroup})
				)

			if("trigger" in groupData){
				const row = buildRow(
					buildLabel("Player names"),
					$("<textarea/>")
						.css("width","100%")
						.attr("rows",5)
				)
				
				row.children("td:has(> textarea)").last().attr("colspan",5)
				
				const txtArea = row.find("textarea")
				
				txtArea.val(gobconfig.get(groupKey + ".trigger").join(", "))
				txtArea.on("change",function(event){
					let words = (event.target.value || "").split(",")
					words = words.filter(w=>w!==null&&w!==undefined).map(w=>w.toLowerCase().trim()).filter(w=>w.length>0)
					gobconfig.set(groupKey + ".trigger",words)
					event.target.value = words.join(", ")
				})
			}		
						
			$("#chatgroups").append(groupElement)
		}
						
		window.gobconfig.get(configKeyGroupSorting).forEach((groupId)=>{
				buildGroupElement(groupId)				
			})
						
		$("#addchatgroup").on("click",function(){
			function makeGroupId(){
				const ids = window.gobconfig.get(configKeyGroupSorting)
				let id = null
				do{
					id = Gobchat.generateId(6)					
				}while(_.includes(ids,id))
				return id
			}
			
			const groupId = makeGroupId()
			
			const group = {
				name: "Unnamed",
				id: groupId,
				active: true,
				trigger: [],
				style: {
					body:{ "background-color": null, },
					header: { "background-color": null, "color": null, },
				},
			}
			
			window.gobconfig.get(configKeyGroupSorting).push(groupId)
			window.gobconfig.get(configKeyGroupData)[groupId] = group		
			
			buildGroupElement(groupId)
			
			updateGroupRenderer()
		})
			
		$( "#chatgroups" ).accordion({
				heightStyle: "content",
				header: "> div > h4",
			})
			.sortable({
				axis: "y",
				handle: "h4",
				stop: function( event, ui ) {	
					//update sort order
					const order = []					
					$( "#chatgroups > div" ).each(function(){
						order.push($(this).data("groupId"))
					})
					
					window.gobconfig.set(configKeyGroupSorting, order.filter(e => e !== null && e !== undefined && e.length > 0))
					
					// Refresh accordion to handle new order
					updateGroupRenderer()
				}
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
	
	// helper functions
	
	function toggleValueInArray(array, value){
		if( _.includes(array, value) ){
			_.remove(array,(i)=>{return i===value})
		}else{
			array.push(value)
		}
	}
	
	function setValueInArray(array, value, available){		
		if( available ){
			if( ! _.includes(array, value) ){
				array.push(value)
			}
		}else{
			_.remove(array,(e)=>{return e === value})
		}
	}
	
	//UI elements
	
	function buildLabel(name){
		if(name===null || name===undefined){
			return null
		}
		const lbl = document.createElement("label")
		lbl.innerHTML = name
		return lbl
	}
	
	function buildCheckboxForArray(configKey,checkboxValue){
		const input = document.createElement("input")
		input.type = "checkbox"
		
		const data = window.gobconfig.get(configKey)
		if(data === null || data === undefined)
			input.disabled = true
		
		input.checked  = _.includes(data,checkboxValue)
		$(input).on("change",function(event){
				setValueInArray(data, checkboxValue, event.target.checked)
			})		
		
		return input
	}
	
	function buildCheckboxForElement(configKey){
		const input = document.createElement("input")
		input.type = "checkbox"
		
		const data = window.gobconfig.get(configKey)
		if(data === null || data === undefined)
			input.disabled = true
		
		input.checked  = data
		$(input).on("change",function(event){
				window.gobconfig.set(configKey, event.target.checked)
			})	
			
		return input
	}
	
	function buildColorSelector(options){
		const defaultOptions = {
			configKey: null,
			hasAlpha: true,
			hasReset: true,
			onBeforeShow: null
		}
		
		options = $.extend(defaultOptions,options)
		if(options.configKey === null){
			throw new Error("ConfigKey can't be null")
		}
		
		const div = document.createElement("div")
		const input = document.createElement("input")		
		div.appendChild(input)
		
		const data = window.gobconfig.get(options.configKey)
		
		input.type = "text"	
		$(input).spectrum({
			preferredFormat: "hex3",
			color: data,
			allowEmpty:true,
			showInput: true,
			showInitial: true,
			showAlpha: options.hasAlpha,
			showPalette: true,
			//palette: spectrumColorPalette,
			showSelectionPalette: true,
			selectionPalette: [],
			maxSelectionSize: 6,
			clickoutFiresChange: false,
			hide: function(color) {
					if(color !== null){
						window.gobconfig.set(options.configKey, color.toString())
					}else{
						window.gobconfig.set(options.configKey, null)
					}
				},
			beforeShow:	function(color){
							if(options.onBeforeShow) options.onBeforeShow(input)
							return true
						}
		})
		
		if(options.hasReset){
			const btn = document.createElement("button")
			btn.innerHTML = "&#x274C;"
			btn.title = "Reset to default"
			
			$(btn).on("click",function(event){
					window.gobconfig.reset(options.configKey)
					$(input).spectrum("set", window.gobconfig.get(options.configKey));
				})
			div.appendChild(btn)	
		}
		
		return div
	}
	
	function buildTextbox(options){
		const defaultOptions = {
			configKey: null,
			hasReset: true,
		}
		
		options = $.extend(defaultOptions,options)
		if(options.configKey === null){
			throw new Error("ConfigKey can't be null")
		}
		
		const div = $("<div/>")
		
		const input = $("<input/>").appendTo(div)
		input.attr("type","text")
		input.val(window.gobconfig.get(options.configKey))
		input.on("change",function(event){
				window.gobconfig.set(options.configKey, input.val())
			})
		
		if(options.hasReset){
			$("<button>&#x274C;</button>")
				.on("click",function(event){
					window.gobconfig.reset(options.configKey)
					input.val(window.gobconfig.get(options.configKey))
				})
				.appendTo(div)
		}
		
		return div
	}
	
}(Gobchat || {}));
'use strict'

var Gobchat = (function(Gobchat){

	//Needs to be updated on server changes
	//TODO Check for locale specific names
	Gobchat.Datacenters = [
		{
			label: "North American",
			centers: [
				{
					label: "Aether",
					servers: [
						"Adamantoise",
						"Cactuar",
						"Faerie",
						"Gilgamesh",
						"Jenova",
						"Midgardsormr",
						"Sargatanas",
						"Siren"
					]
				},
				{
					label: "Primal",
					servers: [
						"Behemoth",
						"Excalibur",
						"Exodus",
						"Famfrit",
						"Hyperion",
						"Lamia",
						"Leviathan",
						"Ultros"
					]
				},
				{
					label: "Crystal",
					servers: [
						"Balmung",
						"Brynhildr",
						"Coeurl",
						"Diabolos",
						"Goblin",
						"Malboro",
						"Mateus",
						"Zalera"
					]
				}
			]
		},
		{
			label: "European",
			centers: [
				{
					label: "Chaos",
					servers: [
						"Cerberus",
						"Louisoix",
						"Moogle",
						"Omega",
						"Ragnarok",
						"Spriggan",
					]
				},
				{
					label: "Light",
					servers: [
						"Shiva",
						"Twintania",
						"Phoenix",
						"Odin",
						"Lich",
						"Zodiark"
					]
				}
			]
		},
		{
			label: "Japanese",
			centers: [
				{
					label: "Elemental",
					servers: [
						"Aegis",
						"Atomos",
						"Carbuncle",
						"Garuda",
						"Gungnir",
						"Kujata",
						"Ramuh",
						"Tonberry",
						"Typhon",
						"Unicorn"
					]
				},
				{
					label: "Gaia",
					servers: [
						"Alexander",
						"Bahamut",
						"Durandal",
						"Fenrir",
						"Ifrit",
						"Ridill",
						"Tiamat",
						"Ultima",
						"Valefor",
						"Yojimbo",
						"Zeromus"
					]
				},
				{
					label: "Mana",
					servers: [
						"Anima",
						"Asura",
						"Belias",
						"Chocobo",
						"Hades",
						"Ixion",
						"Mandragora",
						"Masamune",
						"Pandaemonium",
						"Shinryu",
						"Titan"
					]
				},
			]
		},
	]
	
	Gobchat.DatacenterHelper = Object.freeze({
		
		tryAndGetDatacenterByName: function(datacenterName){
			if(datacenterName === null)
				return null			
			for(let region of Gobchat.Datacenters){
				for(let center of region.centers){
					if(center.label === datacenterName){
						return center
					}
				}
			}			
			return null
		},
		
		tryAndGetDatacenterByServerName: function(serverName){
			if(serverName === null)
				return null			
			for(let region of Gobchat.Datacenters){
				for(let center of region.centers){
					for(let server of center.servers){
						if( server === serverName ){
							return center	
						}
					}
				}
			}			
			return null
		},
		
	})

	return Gobchat
	
}(Gobchat || {}));
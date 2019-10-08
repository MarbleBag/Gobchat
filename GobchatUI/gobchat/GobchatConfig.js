'use strict'

//requieres Gobchat.DefaultChatConfig

var Gobchat = (function(Gobchat,undefined){
		
	function removeInvalidKeys(map,allowedKeys){	
		const availableKeys = Object.keys(map)
		const invalidKeys = availableKeys.filter((k) => { return _.indexOf(allowedKeys,k) === -1 })
		invalidKeys.forEach((k)=>{delete map[k]}) //remove keys which are not allowed
	}
		
	//removes every value from data that is the same as in 	extendedData
	function retainChangesIterator(data, extendedData){
		const callbackHelper = {
			onArray: function(data,extendedData){				
				return _.isEqual(_.sortBy(data), _.sortBy(extendedData)) //same, can be removed
			},
			onCompare: function(data,extendedData){
				return data == extendedData //same, can be removed
			},
			onObject: function(data,extendedData,callbackHelper){
				removeInvalidKeys(data,Object.keys(extendedData)) 
				for(let key of Object.keys(data)){
					if(dataIteratorHelper(data[key], extendedData[key], callbackHelper)) //delete on true
						delete data[key]
				}
				return Object.keys(data).length == 0 //if data is empty, it can be deleted
			}
		}
		
		dataIteratorHelper(data, extendedData, callbackHelper)
	}
	
	//Will merge every value from extendedData into data 
	function mergeIterator(data, extendedData){
		const callbackHelper= {
			onArray: function(data,extendedData){
				return true //lazy, just merge
			},
			onCompare: function(data,extendedData){
				return true //lazy, just merge
			},
			onObject: function(data, extendedData, callbackHelper){
				for(let key of Object.keys(extendedData)){
					if(!(key in data)){
						data[key] = extendedData[key]
					}else{
						if(dataIteratorHelper(data[key], extendedData[key], callbackHelper)){ //merge on true
							data[key] = extendedData[key]
						}				
					}
				}
				return false
			}
		}
		
		callbackHelper.onObject(data, extendedData, callbackHelper)
	}
		
	function dataIteratorHelper(data, extendedData, callbackHelper){
		if(Gobchat.isArray(data)){
			if(Gobchat.isArray(extendedData)){
				return callbackHelper.onArray(data,extendedData)
			}else{
				return false //invalid
			}
		}else if(Gobchat.isArray(extendedData)){
			return false //invalid
		}else if(Gobchat.isObject(data)){
			if(Gobchat.isObject(extendedData)){
				return callbackHelper.onObject(data, extendedData, callbackHelper)
			}else{
				return false //invalid
			}
		}else if(Gobchat.isObject(extendedData)){
			return false //invalid
		}else{
			return callbackHelper.onCompare(data,extendedData)
		}		
	}
	
	function breakKeyDown(key){
		if( key == undefined || key == null ) return []
		const parts = key.split(".")
		return parts
	}
	
	function resolvePath(key,config,value){
		let _config = config
		const keySteps = breakKeyDown(key)
		
		for(let i=0;i<keySteps.length-1;++i){
			const keyStep = keySteps[i]
			if(keyStep in _config){
				_config = _config[keyStep]
			}else{
				throw new InvalidKeyError(`Config error. Key invalid at ${keyStep} - ${key}`);
			}
		}
		
		if(keySteps.length === 0){
			return _config
		}
		
		const targetKey = keySteps[keySteps.length-1]
		if(value !== undefined){
			_config[targetKey] = value
		}
		return _config[targetKey]
	}
		
	//maybe not fast, but free of hassle :^)
	function copyByJson(obj) {
		return JSON.parse(JSON.stringify(obj))
	}
	
	class InvalidKeyError extends Error {
	  constructor(message) {
		super(message)
		this.name = "InvalidKeyError"
	  }
	}
			
	class GobchatConfig{
		constructor(){
			this._propertyListener = []
			this._config = copyByJson(Gobchat.DefaultChatConfig)
		}
		
		restoreDefaultConfig(){
			this._config = copyByJson(Gobchat.DefaultChatConfig)
		}
		
		overwriteConfig(config){
			//TODO return a set of update flags?
			mergeIterator(this._config,config)
		}
		
		getConfigChanges(){
			const config = copyByJson(this._config)
			retainChangesIterator(config,Gobchat.DefaultChatConfig)
			return config
		}
		
		saveToLocalStore(){
			const json = JSON.stringify(this.getConfigChanges())
			window.localStorage.setItem("gobchat-config",json)
		}
		
		loadFromLocalStore(){			
			const json = window.localStorage.getItem("gobchat-config")
			window.localStorage.removeItem("gobchat-config")
			if(json===undefined || json===null)
				return
			const config = JSON.parse(json)
			this.restoreDefaultConfig()				
			this.overwriteConfig(config)
		}
		
		saveToPlugin(){
			const json = JSON.stringify(this.getConfigChanges())
			console.log("Send: " + json)
			Gobchat.sendMessageToPlugin({event:"SaveGobchatConfig",detail:json})
		}
		
		loadFromPlugin(){
			const self = this
			const onLoad = function(e){
				document.removeEventListener("LoadGobchatConfig",onLoad)
				const json = e.detail.data
				if(json===undefined || json===null){
					console.log("No config data from plugin available")
					return
				}
				
				const config = JSON.parse(json)
				self.restoreDefaultConfig()				
				self.overwriteConfig(config)
			}			
			document.addEventListener("LoadGobchatConfig",onLoad,false)
			Gobchat.sendMessageToPlugin({event:"LoadGobchatConfig"})
		}
		
		get configStyle(){
			return this._config.style
		}
		
		get(key,defaultValue){
			if(key===null || key.length===0){
				return this._config
			}
			try{
				const value = resolvePath(key,this._config)
				return value === undefined ? defaultValue : value
			}catch(error){
				if( defaultValue !== undefined ){
					if(error instanceof InvalidKeyError){
						return defaultValue
					}
				}
				throw error								
			}
		}
		
		set(key, value){
			if(key===null || key.length===0){
				this._config = value
			}
			resolvePath(key,this._config,value)
		}
		
		reset(key){
			if(key===null || key.length===0){
				this._config = copyByJson(Gobchat.DefaultChatConfig)
			}
			const original = resolvePath(key,Gobchat.DefaultChatConfig)
			resolvePath(key,this._config,original)
		}
		
		addPropertyListener(topic,callback){
			
		}
		
		firePropertyChange(topic){
			
		}
	}
	Gobchat.GobchatConfig = GobchatConfig
		
	return Gobchat	
}(Gobchat || {}));
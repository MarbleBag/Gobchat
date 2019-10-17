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
				//return _.isEqual(data,extendedData) //same objects can be removed
				return _.isEqual(_.sortBy(data), _.sortBy(extendedData)) //same objects can be removed
			},
			onCompare: function(data,extendedData){
				return data == extendedData //same objects can be removed
			},
			onObject: function(data,extendedData,callbackHelper){
				removeInvalidKeys(data, Object.keys(extendedData)) 
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
		
	function dataIteratorHelper(objA, objB, callbacks){
		if(Gobchat.isArray(objA)){
			if(Gobchat.isArray(objB)){
				return callbacks.onArray(objA, objB, callbacks)
			}else{
				return false //invalid
			}
		}else if(Gobchat.isArray(objB)){
			return false //invalid
		}else if(Gobchat.isObject(objA)){
			if(Gobchat.isObject(objB)){
				return callbacks.onObject(objA, objB, callbacks)
			}else{
				return false //invalid
			}
		}else if(Gobchat.isObject(objB)){
			return false //invalid
		}else{
			return callbacks.onCompare(objA, objB, callbacks)
		}		
	}
	
	function breakKeyDown(key){
		if( key == undefined || key == null ) return []
		const parts = key.split(".")
		return parts
	}
	
	function resolvePath(key, config, value, remove){
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
		if(remove !== undefined && remove){
			delete _config[targetKey]
		}
		return _config[targetKey]
	}
	
	function copyValueForKey(source, key, destination, doJsonCopy){
		let val = resolvePath(key, source)
		if(doJsonCopy)
			val = copyByJson(val)		
		resolvePath(key, destination, val)
	}
	
	function tryAndRemapConfig(config){
		if(config.version = "0.1.4"){
			//TODO			
		}
		
		return config
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
			this._propertyListener = new EventDispatcher()
			this._config = copyByJson(Gobchat.DefaultChatConfig)
		}
		
		restoreDefaultConfig(){
			this._config = copyByJson(Gobchat.DefaultChatConfig)
		}
		
		overwriteConfig(config){
			//TODO return a set of update flags?
			mergeIterator(this._config,config)			
			this.firePropertyChange("ALL") //TODO
		}
		
		getConfigChanges(){
			const config = copyByJson(this._config)
			retainChangesIterator(config, Gobchat.DefaultChatConfig)
			copyValueForKey(this._config, "version", config)
			copyValueForKey(this._config, "userdata", config)
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
			const config = this.getConfigChanges()			
			const json = JSON.stringify(config)
			Gobchat.sendMessageToPlugin({event:"SaveGobchatConfig",detail:json})
		}
		
		loadFromPlugin(callback){ //TODO make this function async 
			const self = this
			const onLoad = function(e){
				document.removeEventListener("LoadGobchatConfig",onLoad)
				const json = e.detail.data
				if(json===undefined || json===null){
					if(callback)callback()
					return
				}
				
				let config = JSON.parse(json)
				const d = config.version
				if( config.version && config.version !== Gobchat.DefaultChatConfig.version){
					config = tryAndRemapConfig(config)
				}
				
				
				if( config.version && config.version !== Gobchat.DefaultChatConfig.version){
					alert(`Error: Config version mismatch.\nSome settings are lost. Check options and resave.\nExpected ${Gobchat.DefaultChatConfig.version} but was ${config.version}`)
				}
				
				self.restoreDefaultConfig()				
				self.overwriteConfig(config)		
				if(callback)callback()
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
			this.firePropertyChange(key)
		}
		
		reset(key){
			if(key===null || key.length===0)
				return
			
			const original = resolvePath(key, Gobchat.DefaultChatConfig)
			resolvePath(key, this._config, original)
			this.firePropertyChange(key)
		}
		
		remove(key){
			if(key===null || key.length===0)
				return
			resolvePath(key, this._config, undefined, true)
			this.firePropertyChange(key)
		}
		
		addPropertyListener(topic, callback){
			this._propertyListener.on(topic, callback)
		}
		
		firePropertyChange(topic){
			this._propertyListener.dispatch(topic, {"topic":topic, manager:this})
		}
	}
	Gobchat.GobchatConfig = GobchatConfig
	
	class EventDispatcher{
		constructor(){
			this.listenersByTopic = new Map([])
		}
		dispatch(topic,data){
			const listeners = this.listenersByTopic.get(topic)
			if(listeners){
				const callbacks = listeners.slice(0)
				callbacks.forEach((callback)=>{callback(data)})
			}
		}
		on(topic,callback){
			if( !callback) return
			let listeners = this.listenersByTopic.get(topic)
			if( !listeners ){
				listeners = []
				this.listenersByTopic.set(topic,listeners)
			}
			listeners.push(callback)
		}
		off(topic,callback){
			let listeners = this.listenersByTopic.get(topic)
			if( listeners ){
				const idx = listeners.indexOf(callback)
				if( idx > -1 ) listeners.splice(idx,1)
				if( listeners.length === 0 ) this.listenersByTopic.delete(topic)
			}
		}
	}
	
	class GobchatConfigLoader{
		constructor(targetVersion){
			this._targetVersion = targetVersion
		}
		
		load(config){
			return config
		}
	}
		
	return Gobchat	
}(Gobchat || {}));
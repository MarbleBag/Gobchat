'use strict'

//requieres Gobchat.DefaultChatConfig

var Gobchat = (function(Gobchat,undefined){
		
	function removeInvalidKeys(map,allowedKeys){	
		const availableKeys = Object.keys(map)
		const invalidKeys = availableKeys.filter((k) => { return _.indexOf(allowedKeys,k) === -1 })
		invalidKeys.forEach((k)=>{delete map[k]}) //remove keys which are not allowed
	}
		
	function removeIteratorHelper1(data, defaultData, checkFunction){
		if(Gobchat.isArray(defaultData)){
			if(Gobchat.isArray(data)){
				return false //do not delete arrays
			}else{
				return true //invalid property type
			}			
		}else if(Gobchat.isArray(data)){
			return true //invalid property type
		}else if(Gobchat.isObject(defaultData)){
			if(Gobchat.isObject(data)){
				return removeIteratorHelper2(data, defaultData, checkFunction)
			}else{
				return true //invalid property type
			}
		}else if(Gobchat.isObject(data)){
			return true //invalid property type
		}else{
			return checkFunction(data,defaultData)
		}
	}
	
	function removeIteratorHelper2(data, defaultData, checkFunction){ //both are objects
		removeInvalidKeys(data,Object.keys(defaultData)) 
		for(let key of Object.keys(data)){
			//if(!(key in comparedToMap))
			//	continue //trivial
			if(removeIteratorHelper1(data[key], defaultData[key], checkFunction))
				delete data[key]
		}
		return Object.keys(data).length == 0 //if data is empty, it can be deleted
	}
	
	function retainChanges(config, defaultConfig){		
		removeIteratorHelper2(config,defaultConfig,(a,b)=>{return a==b})
	}
	
	function retainChangesIterator(data, extendedData){
		function onObjectCompare(data,extendedData,compareFunction,nFunction){
			removeInvalidKeys(data,Object.keys(extendedData)) 
			for(let key of Object.keys(data)){
				if(!dataIteratorHelper(data[key], extendedData[key], compareFunction, nFunction))
					delete data[key]
			}
			return Object.keys(data).length != 0 //if data is empty, it can be deleted
		}		
		dataIteratorHelper(data, extendedData, (a,b)=>{return a!=b}, onObjectCompare)
	}
	
	function mergeIterator(data, extendedData){
		for(let key of Object.keys(extendedData)){
			if(!(key in data)){
				data[key] = extendedData[key]
			}else{
				if(dataIteratorHelper(data[key], extendedData[key], (a,b)=>{return true}, mergeIterator)){
					data[key] = extendedData[key]
				}				
			}
		}
	}
	
	function dataIteratorHelper(data, extendedData,compareFunction,onObjectFunction){
		if(Gobchat.isArray(data)){
			if(Gobchat.isArray(extendedData)){
				return true //valid
			}else{
				return false //invalid
			}
		}else if(Gobchat.isArray(extendedData)){
			return false //invalid
		}else if(Gobchat.isObject(data)){
			if(Gobchat.isObject(extendedData)){
				return onObjectFunction(data, extendedData, compareFunction, onObjectFunction)
			}else{
				return false //invalid
			}
		}else if(Gobchat.isObject(extendedData)){
			return false //invalid
		}else{
			return compareFunction(data,extendedData)
		}		
	}
	
	function breakKeyDown(key){
		if( key == undefined || key == null ) return []
		const parts = key.split(".")
		return parts
	}
		
	//maybe not fast, but free of hassle :^)
	function copyByJson(obj) {
		return JSON.parse(JSON.stringify(obj))
	}
			
	class GobchatConfig{
		constructor(){
			this._propertyListener = []
			this._config = copyByJson(Gobchat.DefaultChatConfig)
		}
		
		loadConfig(config){
			mergeIterator(this._config,config)
		}
		
		saveConfig(){
			const config = copyByJson(this._config)
			retainChangesIterator(config,Gobchat.DefaultChatConfig)
			return config
		}
		
		get configStyle(){
			return this._config.style
		}
		
		get(key){
			let _config = this._config
			const keySteps = breakKeyDown(key)
			
			for(let i=0;i<keySteps.length-1;++i){
				const keyStep = keySteps[i]
				if(keyStep in _config){
					_config = _config[keyStep]
				}else{
					throw new Error(`Config error. Key invalid at ${keyStep} - ${key}`);
				}
			}
			
			if(keySteps.length === 0){
				return _config
			}
			
			const targetKey = keySteps[keySteps.length-1]
			return _config[targetKey]
		}
		
		set(key, value){
			let _config = this._config
			const keySteps = breakKeyDown(key)
			for(let i=0;i<keySteps.length-1;++i){
				const keyStep = keySteps[i]
				if(keyStep in _config){
					_config = _config[keyStep]
				}else{
					throw new Error(`Config error. Key invalid at ${keyStep} - ${key}`);
				}
			}
			
			if(keySteps.length === 0){
				this._config = value
			}else{
				const targetKey = keySteps[keySteps.length-1]
				_config[targetKey] = value
			}
		}
		
		addPropertyListener(topic,callback){
			
		}
		
		firePropertyChange(topic){
			
		}
	}
	Gobchat.GobchatConfig = GobchatConfig
		
	return Gobchat	
}(Gobchat || {}));